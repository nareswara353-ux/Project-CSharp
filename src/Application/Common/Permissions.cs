namespace Application.Common;

public static class Permissions
{
    public static class Customers
    {
        public const string Read = "customers.read";
        public const string Create = "customers.create";
        public const string Update = "customers.update";
        public const string Delete = "customers.delete";
    }

    public static class Products
    {
        public const string Read = "products.read";
        public const string Create = "products.create";
        public const string Update = "products.update";
        public const string Delete = "products.delete";
    }

    public static class Orders
    {
        public const string Read = "orders.read";
        public const string Create = "orders.create";
        public const string Confirm = "orders.confirm";
        public const string Ship = "orders.ship";
        public const string Cancel = "orders.cancel";
    }

    public static class Reports
    {
        public const string Sales = "reports.sales";
        public const string Inventory = "reports.inventory";
    }

    public static class Admin
    {
        public const string ManageUsers = "admin.users.manage";
        public const string ManageFeatureFlags = "admin.features.manage";
        public const string ViewAuditLog = "admin.audit.view";
    }

    public static IReadOnlyList<string> All { get; } = new[]
    {
        Customers.Read, Customers.Create, Customers.Update, Customers.Delete,
        Products.Read, Products.Create, Products.Update, Products.Delete,
        Orders.Read, Orders.Create, Orders.Confirm, Orders.Ship, Orders.Cancel,
        Reports.Sales, Reports.Inventory,
        Admin.ManageUsers, Admin.ManageFeatureFlags, Admin.ViewAuditLog
    };
}
