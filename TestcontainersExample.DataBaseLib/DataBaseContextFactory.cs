using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TestcontainersExample.DataBaseLib;

public class DataBaseContextFactory : IDesignTimeDbContextFactory<DataBaseContext>
{
    public DataBaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataBaseContext>();

        var connectionString = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build()
            .GetConnectionString("DefaultConnection");

        optionsBuilder
            .UseLazyLoadingProxies()
            .UseNpgsql(connectionString);
        return new DataBaseContext(optionsBuilder.Options);
    }

    public static DataBaseContext CreateDbContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataBaseContext>();
        optionsBuilder
            .UseLazyLoadingProxies()
            .UseNpgsql(connectionString);
        return new DataBaseContext(optionsBuilder.Options);
    }
}