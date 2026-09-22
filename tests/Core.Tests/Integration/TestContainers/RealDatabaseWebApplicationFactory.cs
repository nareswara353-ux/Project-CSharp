using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Core.Tests.Integration.TestContainers;

public class RealDatabaseWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public RealDatabaseWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["Jwt:Secret"] = "INTEGRATION_TEST_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG",
                ["Jwt:Issuer"] = "PortfolioEnterpriseAPI-Test",
                ["Jwt:Audience"] = "PortfolioEnterpriseClient-Test",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Caching:Provider"] = "None"
            });
        });

        builder.ConfigureServices(services =>
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }
}
