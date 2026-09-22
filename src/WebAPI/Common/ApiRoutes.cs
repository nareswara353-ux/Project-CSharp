namespace WebAPI.Common;

public static class ApiRoutes
{
    public const string Base = "api/v{version:apiVersion}";

    public const string Auth = Base + "/auth";
    public const string Customers = Base + "/customers";
    public const string Products = Base + "/products";
    public const string Orders = Base + "/orders";
    public const string Users = Base + "/users";
    public const string Reports = Base + "/reports";
}
