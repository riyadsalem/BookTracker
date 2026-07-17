using BookTracker.Api.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookTracker.Api.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? connection;
    public EfReader GetReader() => new(Services);
    public EfWriter GetWriter() => new(Services);

    private static readonly KeyValuePair<string, string?>[]
        TestSettings =
        [
            new("SeedDatabase", "false"),
            new("Jwt:Issuer", "BookTracker.Tests"),
            new("Jwt:Audience", "BookTracker.Tests"),
            new(
                "Jwt:SigningKey",
                "book-tracker-test-signing-key-with-32-characters"),
            new("Jwt:ExpirationMinutes", "10")
        ];

    /*
    private SqliteConnection connection = null!;
    /*
    After the tests finish, we must clean up the SqliteConnection by calling Dispose().
    Dispose(bool disposing) has two cases: disposing = true means Dispose() was called directly,
    so it is safe to clean managed resources like SqliteConnection.
    disposing = false means the Garbage Collector is cleaning the object through the finalizer,
    so managed resources may already be gone. Therefore,
    we make the connection nullable (SqliteConnection?) and use connection?.Dispose() to avoid a NullReferenceException.
    */

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(
            configuration =>
                configuration.AddInMemoryCollection(TestSettings));

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        /*
        Run the application in the Testing environment so the
        Development startup doesn't execute Database.Migrate().
        Tests create the in-memory database using EnsureCreated().
        */

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(service =>
                service.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connection));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated(); ////// gebruik daaaaaaaaaaaaaaaaaaaaaaaaaaat now (niet migrate)
        });
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) connection?.Dispose();

        base.Dispose(disposing);
    }

}