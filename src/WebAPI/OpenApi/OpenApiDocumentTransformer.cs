using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace WebAPI.OpenApi;

public sealed class OpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Portfolio Enterprise API",
            Version = "v1",
            Description = "Enterprise-grade .NET 8 API with Clean Architecture, DDD, CQRS, and JWT auth.",
            Contact = new OpenApiContact
            {
                Name = "Portfolio Enterprise",
                Email = "dev@portfolio.local",
                Url = new Uri("https://github.com/your-org/Project-CSharp")
            },
            License = new OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };

        document.Servers.Clear();
        document.Servers.Add(new OpenApiServer
        {
            Url = "https://localhost:5001",
            Description = "Local development (HTTPS)"
        });
        document.Servers.Add(new OpenApiServer
        {
            Url = "http://localhost:8080",
            Description = "Docker container"
        });

        document.Tags = new List<OpenApiTag>
        {
            new() { Name = "Authentication", Description = "Register and login endpoints" },
            new() { Name = "Customers", Description = "Customer management" },
            new() { Name = "Products", Description = "Product catalog" },
            new() { Name = "Orders", Description = "Order lifecycle" },
            new() { Name = "Users", Description = "User profile and management" },
            new() { Name = "Reports", Description = "Sales and inventory reports" },
            new() { Name = "Files", Description = "File upload and download" },
            new() { Name = "FeatureFlags", Description = "Runtime feature toggles" },
            new() { Name = "AuditLogs", Description = "Audit trail access (Admin only)" }
        };

        return Task.CompletedTask;
    }
}
