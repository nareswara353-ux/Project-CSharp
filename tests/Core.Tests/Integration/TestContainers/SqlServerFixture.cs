using Testcontainers.MsSql;

namespace Core.Tests.Integration.TestContainers;

public class SqlServerFixture : IAsyncLifetime
{
    private MsSqlContainer? _container;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Strong!Passw0rd_ForTest")
            .Build();

        await _container.StartAsync();

        ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }
}

[CollectionDefinition("SqlServerCollection")]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
}
