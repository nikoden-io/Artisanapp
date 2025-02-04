using Artisaback.Data.DbContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MongoDb;

namespace Artisaback.Tests.Api;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:8.0")
        .WithPortBinding(27017, true)
        .Build();

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testSettings = new Dictionary<string, string>
            {
                { "MongoDb:ConnectionString", _mongoContainer.GetConnectionString() },
                { "MongoDb:Database", "TestDb" },
                { "Jwt:Secret", "YourLongerSuperSecretKey1234567890" }
            };
            config.AddInMemoryCollection(testSettings);
        });

        builder.ConfigureServices((context, services) =>
        {
            // Remove existing MongoDbContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MongoDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Add MongoDbContext with Testcontainers connection
            services.AddDbContext<MongoDbContext>(options =>
                options.UseMongoDB(context.Configuration["MongoDb:ConnectionString"],
                    context.Configuration["MongoDb:Database"])
            );
        });
    }
}