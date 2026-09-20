namespace Application.Common;

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string ManagerOrAbove = "ManagerOrAbove";
    public const string AuthenticatedUser = "AuthenticatedUser";
}
