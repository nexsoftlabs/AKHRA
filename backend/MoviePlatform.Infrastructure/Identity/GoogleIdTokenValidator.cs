using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Identity;

namespace MoviePlatform.Infrastructure.Identity;

public sealed class GoogleIdTokenValidator(IOptions<AuthOptions> options) : IGoogleIdTokenValidator
{
    public async Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientId = options.Value.GoogleClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return null;
        }

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [clientId]
        };

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return new GoogleTokenPayload(
                payload.Subject,
                payload.Email,
                payload.Name,
                payload.EmailVerified);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
