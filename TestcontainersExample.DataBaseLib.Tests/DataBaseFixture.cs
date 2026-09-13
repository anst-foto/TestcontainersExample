using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace TestcontainersExample.DataBaseLib.Tests;

public class DataBaseFixture : IAsyncLifetime
{
    private readonly IContainer _container;

    public DataBaseFixture()
    {
        _container = new PostgreSqlBuilder("postgres:18")
            .WithDatabase("books_db")
            .WithUsername("test_user")
            .WithPassword("1234")
            .WithExposedPort(PostgreSqlBuilder.PostgreSqlPort)
            .Build();
    }
    
    public string ConnectionString => _container.GetConnectionString();
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        await using var context = DataBaseContextFactory.CreateDbContext(ConnectionString);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}