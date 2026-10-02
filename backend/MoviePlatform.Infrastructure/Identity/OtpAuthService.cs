using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Authorization;
using MoviePlatform.Application.Identity;
using MoviePlatform.Domain.Identity;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Identity;

public sealed class OtpAuthService(
    MoviePlatformDbContext db,
    UserManager<ApplicationUser> userManager,
    ISessionService sessionService,
    ISmsSender smsSender,
    IDistributedCache cache,
    IOptions<AuthOptions> authOptions) : IOtpAuthService
{
    public async Task<OtpRequestResult> RequestOtpAsync(OtpRequest request, string? ipAddress, CancellationToken cancellationToken)
    {
        if (!PhoneNumberNormalizer.TryNormalize(request.PhoneNumber, out var phone))
        {
            return OtpRequestResult.Fail("invalid_phone", "Phone number could not be normalized.");
        }

        var options = authOptions.Value;
        var cooldownKey = $"otp:cooldown:{phone}";
        if (await cache.GetStringAsync(cooldownKey, cancellationToken) is not null)
        {
            return OtpRequestResult.Fail("rate_limited", "Please wait before requesting another code.", options.OtpRequestCooldownSeconds);
        }

        var hourKey = $"otp:hour:{phone}:{DateTimeOffset.UtcNow:yyyyMMddHH}";
        var hourCountRaw = await cache.GetStringAsync(hourKey, cancellationToken);
        var hourCount = hourCountRaw is null ? 0 : int.Parse(hourCountRaw);
        if (hourCount >= options.OtpMaxRequestsPerHourPerPhone)
        {
            return OtpRequestResult.Fail("rate_limited", "Too many OTP requests. Try again later.");
        }

        var code = Random.Shared.Next(100000, 999999).ToString();
        var challenge = new PhoneVerificationChallenge
        {
            PhoneNumber = phone,
            CodeHash = TokenHashing.Hash(code),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(options.OtpExpiryMinutes),
            CreatedAt = DateTimeOffset.UtcNow,
            IpAddress = ipAddress
        };

        db.PhoneVerificationChallenges.Add(challenge);
        await db.SaveChangesAsync(cancellationToken);

        await smsSender.SendAsync(phone, $"Your verification code is {code}", cancellationToken);

        await cache.SetStringAsync(
            cooldownKey,
            "1",
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(options.OtpRequestCooldownSeconds) },
            cancellationToken);

        await cache.SetStringAsync(
            hourKey,
            (hourCount + 1).ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) },
            cancellationToken);

        return OtpRequestResult.Ok();
    }

    public async Task<AuthResult> VerifyOtpAsync(OtpVerifyRequest request, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!PhoneNumberNormalizer.TryNormalize(request.PhoneNumber, out var phone))
        {
            return AuthResult.Fail("invalid_phone", "Phone number could not be normalized.");
        }

        var options = authOptions.Value;
        var challenge = await db.PhoneVerificationChallenges
            .Where(c => c.PhoneNumber == phone && !c.IsConsumed)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (challenge is null)
        {
            return AuthResult.Fail("otp_invalid", "No active verification code.");
        }

        if (challenge.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return AuthResult.Fail("otp_expired", "Verification code has expired.");
        }

        if (challenge.AttemptCount >= options.OtpMaxAttempts)
        {
            return AuthResult.Fail("otp_locked", "Too many invalid attempts.");
        }

        challenge.AttemptCount++;
        if (!string.Equals(challenge.CodeHash, TokenHashing.Hash(request.Code), StringComparison.Ordinal))
        {
            await db.SaveChangesAsync(cancellationToken);
            return AuthResult.Fail("otp_invalid", "Invalid verification code.");
        }

        challenge.IsConsumed = true;
        await db.SaveChangesAsync(cancellationToken);

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone, cancellationToken);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = phone,
                PhoneNumber = phone,
                PhoneNumberConfirmed = true,
                IsPhoneVerified = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var create = await userManager.CreateAsync(user);
            if (!create.Succeeded)
            {
                return AuthResult.Fail("registration_failed", string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(user, AppRoles.RegisteredUser);
        }
        else
        {
            if (user.DeletedAt is not null)
            {
                return AuthResult.Fail("account_deleted", "This account has been deleted.");
            }

            user.IsPhoneVerified = true;
            user.PhoneNumberConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        await sessionService.IssueSessionAsync(new ApplicationUserRef(user.Id, user.Email, user.UserName), httpContext, cancellationToken);
        return AuthResult.Ok(await UserProfileMapper.ToProfileAsync(user, userManager));
    }
}
