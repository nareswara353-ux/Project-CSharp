namespace Application.Common;

public interface IEmailTemplateRenderer
{
    Task<string> RenderAsync<TModel>(
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default);
}

public static class EmailTemplateNames
{
    public const string WelcomeCustomer = "WelcomeCustomer";
    public const string CustomerDeactivated = "CustomerDeactivated";
    public const string OrderCreated = "OrderCreated";
    public const string OrderShipped = "OrderShipped";
    public const string OrderCancelled = "OrderCancelled";
    public const string UserWelcome = "UserWelcome";
    public const string PasswordChanged = "PasswordChanged";
    public const string StockLowAlert = "StockLowAlert";
}
