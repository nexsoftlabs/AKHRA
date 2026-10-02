using MoviePlatform.Domain.Common;

namespace MoviePlatform.Domain.Identity;

public class PhoneVerificationChallenge : Entity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public int AttemptCount { get; set; }
    public bool IsConsumed { get; set; }
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
