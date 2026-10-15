using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Products.Infrastructure.Persistence;

namespace Products.Tests.Integration;

/// <summary>
/// Boots the real API against a dedicated SQL Server test database.
/// Override the target server with the TEST_CONNECTION_STRING environment variable.
/// </summary>
public sealed class ProductsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string DefaultTestConnectionString =
        "Server=.\\SQLEXPRESS;Database=ProductsDb_IntegrationTests;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    private static readonly string TestConnectionString =
        Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING") ?? DefaultTestConnectionString;

    public ProductsApiFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", TestConnectionString);
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task ResetAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Products.ExecuteDeleteAsync();
        await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Products', RESEED, 0)");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionString);
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }
}
