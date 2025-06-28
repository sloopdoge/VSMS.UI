namespace VSMS.Domain.Constants;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string CompanyAdmin = "CompanyAdmin";
    public const string CompanyManager = "CompanyManager";
    public const string User = "User";

    public static readonly string[] All = [Admin, CompanyAdmin, CompanyManager, User];
}