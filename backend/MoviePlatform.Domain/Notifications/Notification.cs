using MoviePlatform.Domain.Common;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Identity;

namespace MoviePlatform.Domain.Notifications;

public class Notification : Entity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public NotificationChannel Channel { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
}
