namespace MoviePlatform.Application.Authorization;

public static class AppRoles
{
    public const string RegisteredUser = "RegisteredUser";
    public const string ContentManager = "ContentManager";
    public const string SupportAgent = "SupportAgent";
    public const string FinanceAdmin = "FinanceAdmin";
    public const string PlatformAdmin = "PlatformAdmin";
    public const string SuperAdmin = "SuperAdmin";

    public static readonly string[] All =
    [
        RegisteredUser,
        ContentManager,
        SupportAgent,
        FinanceAdmin,
        PlatformAdmin,
        SuperAdmin
    ];
}

public static class AppPolicies
{
    public const string AdminAccess = "AdminAccess";
    public const string AdminPanel = "AdminPanel";
    public const string ContentManagement = "ContentManagement";
    public const string FinanceAccess = "FinanceAccess";
    public const string SupportAccess = "SupportAccess";
}
