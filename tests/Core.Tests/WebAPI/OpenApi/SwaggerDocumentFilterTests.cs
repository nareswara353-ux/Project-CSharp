using FluentAssertions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebAPI.OpenApi;

namespace Core.Tests.WebAPI.OpenApi;

public class SwaggerDocumentFilterTests
{
    private readonly SwaggerDocumentFilter _filter = new();
    private readonly OpenApiDocument _document = new();
    private readonly DocumentFilterContext _context;

    public SwaggerDocumentFilterTests()
    {
        _context = new DocumentFilterContext(
            new List<Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription>(),
            new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions())),
            new SchemaRepository());
    }

    [Fact]
    public void Apply_ShouldSetDocumentInfo()
    {
        _filter.Apply(_document, _context);

        _document.Info.Should().NotBeNull();
        _document.Info.Title.Should().Be("Portfolio Enterprise API");
        _document.Info.Version.Should().Be("v1");
        _document.Info.Contact.Email.Should().Be("dev@portfolio.local");
        _document.Info.License.Name.Should().Be("MIT");
    }

    [Fact]
    public void Apply_ShouldSetServers()
    {
        _filter.Apply(_document, _context);

        _document.Servers.Should().NotBeNull();
        _document.Servers.Should().HaveCount(2);
        _document.Servers.Should().Contain(s => s.Url == "https://localhost:5001");
        _document.Servers.Should().Contain(s => s.Url == "http://localhost:8080");
    }

    [Fact]
    public void Apply_ShouldSetTags()
    {
        _filter.Apply(_document, _context);

        _document.Tags.Should().NotBeNull();
        _document.Tags.Should().HaveCount(9);
        _document.Tags.Should().Contain(t => t.Name == "Authentication");
        _document.Tags.Should().Contain(t => t.Name == "Customers");
        _document.Tags.Should().Contain(t => t.Name == "Orders");
        _document.Tags.Should().Contain(t => t.Name == "AuditLogs");
    }
}
