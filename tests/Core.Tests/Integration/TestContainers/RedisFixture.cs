using Testcontainers.Redis;

namespace Core.Tests.Integration.TestContainers;

public class RedisFixture : IAsyncLifetime
{
    private RedisContainer? _container;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7.4-alpine")
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

[CollectionDefinition("RedisCollection")]
public class RedisCollection : ICollectionFixture<RedisFixture>
{
}
