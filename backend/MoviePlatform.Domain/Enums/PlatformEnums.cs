namespace MoviePlatform.Domain.Enums;

public enum PublicationStatus
{
    Draft = 0,
    Processing = 1,
    Ready = 2,
    Published = 3,
    Unpublished = 4,
    Archived = 5
}

public enum PurchaseType
{
    Lifetime = 0,
    Rental = 1
}

public enum PaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Captured = 2,
    Failed = 3,
    Refunded = 4,
    Disputed = 5
}

public enum OrderStatus
{
    Pending = 0,
    AwaitingPayment = 1,
    Paid = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5
}

public enum EntitlementSource
{
    Purchase = 0,
    Subscription = 1,
    Promotion = 2,
    AdminGrant = 3
}

public enum EntitlementStatus
{
    Active = 0,
    Expired = 1,
    Revoked = 2
}

public enum SubscriptionStatus
{
    Pending = 0,
    Active = 1,
    PastDue = 2,
    Cancelled = 3,
    Expired = 4
}

public enum MediaAssetType
{
    SourceVideo = 0,
    HlsManifest = 1,
    HlsSegment = 2,
    Poster = 3,
    Backdrop = 4,
    Trailer = 5,
    Thumbnail = 6
}

public enum EncodingJobStatus
{
    Queued = 0,
    InProgress = 1,
    Succeeded = 2,
    Failed = 3,
    Cancelled = 4
}

public enum UploadSessionStatus
{
    Pending = 0,
    Uploading = 1,
    Completed = 2,
    Expired = 3,
    Failed = 4
}

public enum NotificationChannel
{
    Email = 0,
    Sms = 1,
    InApp = 2
}

public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}

public enum RefundStatus
{
    Requested = 0,
    Approved = 1,
    Rejected = 2,
    Processed = 3
}

public enum BillingInterval
{
    Monthly = 0,
    Annual = 1,
    ThreeMonths = 2,
    SixMonths = 3,
}
