using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Identity;
using MoviePlatform.Domain.Identity;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Identity;

public sealed class SessionService(
    MoviePlatformDbContext db,
    SignInManager<ApplicationUser> signInManager,
    IOptions<AuthOptions> authOptions) : ISessionService
{
    private const string SessionIdCookie = "mp.sid";

    public async Task IssueSessionAsync(ApplicationUserRef user, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var entity = await db.Users.FirstAsync(u => u.Id == user.Id, cancellationToken);
        await signInManager.SignInAsync(entity, isPersistent: true);

        var refreshToken = TokenHashing.GenerateSecureToken();
        var session = new UserSession
        {
            UserId = user.Id,
            RefreshTokenHash = TokenHashing.Hash(refreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(authOptions.Value.RefreshTokenDays),
            CreatedAt = DateTimeOffset.UtcNow,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        };

        db.UserSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        AppendRefreshCookie(httpContext, refreshToken, session.ExpiresAt);
        httpContext.Response.Cookies.Append(
            SessionIdCookie,
            session.Id.ToString(),
            BuildCookieOptions(httpContext, session.ExpiresAt));
    }

    public async Task<bool> TryRefreshAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Cookies.TryGetValue(authOptions.Value.RefreshCookieName, out var refreshToken)
            || !httpContext.Request.Cookies.TryGetValue(SessionIdCookie, out var sessionIdRaw)
            || !Guid.TryParse(sessionIdRaw, out var sessionId))
        {
            return false;
        }

        var hash = TokenHashing.Hash(refreshToken);
        var session = await db.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.RefreshTokenHash == hash, cancellationToken);

        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        var user = session.User;
        if (user.DeletedAt is not null)
        {
            return false;
        }

        session.RevokedAt = DateTimeOffset.UtcNow;

        var newRefresh = TokenHashing.GenerateSecureToken();
        var newSession = new UserSession
        {
            UserId = user.Id,
            RefreshTokenHash = TokenHashing.Hash(newRefresh),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(authOptions.Value.RefreshTokenDays),
            CreatedAt = DateTimeOffset.UtcNow,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        };

        db.UserSessions.Add(newSession);
        await db.SaveChangesAsync(cancellationToken);

        await signInManager.SignInAsync(user, isPersistent: true);
        AppendRefreshCookie(httpContext, newRefresh, newSession.ExpiresAt);
        httpContext.Response.Cookies.Append(
            SessionIdCookie,
            newSession.Id.ToString(),
            BuildCookieOptions(httpContext, newSession.ExpiresAt));
        return true;
    }

    public async Task RevokeCurrentSessionAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (httpContext.Request.Cookies.TryGetValue(SessionIdCookie, out var sessionIdRaw)
            && Guid.TryParse(sessionIdRaw, out var sessionId))
        {
            var session = await db.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
            if (session is not null)
            {
                session.RevokedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        httpContext.Response.Cookies.Delete(authOptions.Value.RefreshCookieName, new CookieOptions { Path = "/api/v1/auth" });
        httpContext.Response.Cookies.Delete(SessionIdCookie, new CookieOptions { Path = "/" });
        await signInManager.SignOutAsync();
    }

    private void AppendRefreshCookie(HttpContext httpContext, string token, DateTimeOffset expires)
    {
        httpContext.Response.Cookies.Append(
            authOptions.Value.RefreshCookieName,
            token,
            BuildCookieOptions(httpContext, expires));
    }

    private static CookieOptions BuildCookieOptions(HttpContext httpContext, DateTimeOffset expires) => new()
    {
        HttpOnly = true,
        Secure = httpContext.Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Expires = expires,
        Path = "/api/v1/auth"
    };
}
