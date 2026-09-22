using System.Reflection;
using System.Text.RegularExpressions;
using Application.Common;

namespace Infrastructure.Email;

public partial class SimpleEmailTemplateRenderer : IEmailTemplateRenderer
{
    public Task<string> RenderAsync<TModel>(
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            throw new ArgumentException("Template name cannot be empty", nameof(templateName));

        if (model is null)
            throw new ArgumentNullException(nameof(model));

        var template = GetTemplate(templateName);
        var result = ReplacePlaceholders(template, model);

        return Task.FromResult(result);
    }

    private static string ReplacePlaceholders<TModel>(string template, TModel model)
    {
        var properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(model)?.ToString() ?? string.Empty;
            template = template.Replace($"{{{{{prop.Name}}}}}", value, StringComparison.OrdinalIgnoreCase);
        }

        return template;
    }

    private static string GetTemplate(string name) => name switch
    {
        EmailTemplateNames.WelcomeCustomer =>
            "<h1>Welcome {{FullName}}!</h1><p>Your account with email {{Email}} has been created.</p>",
        EmailTemplateNames.CustomerDeactivated =>
            "<h1>Account Deactivated</h1><p>Hello {{FullName}}, your account ({{Email}}) has been deactivated.</p>",
        EmailTemplateNames.OrderCreated =>
            "<h1>Order Confirmation</h1><p>Order {{OrderId}} for {{TotalAmount}} {{Currency}} has been received.</p>",
        EmailTemplateNames.OrderShipped =>
            "<h1>Order Shipped</h1><p>Order {{OrderId}} is on its way!</p>",
        EmailTemplateNames.OrderCancelled =>
            "<h1>Order Cancelled</h1><p>Order {{OrderId}} has been cancelled.</p>",
        EmailTemplateNames.UserWelcome =>
            "<h1>Welcome {{Username}}!</h1><p>Please verify your email: {{Email}}.</p>",
        EmailTemplateNames.PasswordChanged =>
            "<h1>Password Changed</h1><p>Hello {{Username}}, your password was changed successfully.</p>",
        EmailTemplateNames.StockLowAlert =>
            "<h1>Low Stock Alert</h1><p>Product {{ProductName}} (SKU: {{Sku}}) has only {{Stock}} units left.</p>",
        _ => throw new InvalidOperationException($"Unknown template: {name}")
    };
}
