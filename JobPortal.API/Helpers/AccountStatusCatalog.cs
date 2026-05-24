namespace JobPortal.API.Helpers;

public static class AccountStatusCatalog
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";

    public static bool IsActiveStatus(string status) =>
        string.Equals(status, Active, StringComparison.OrdinalIgnoreCase);

    public static string FromActiveFlag(bool active) => active ? Active : Inactive;
}
