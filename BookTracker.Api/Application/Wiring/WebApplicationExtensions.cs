using BookTracker.Api.Endpoints;
using BookTracker.Api.Seeding;
using BookTracker.Api.Storage;

namespace BookTracker.Api.Wiring;

// Database setup, data seeding, and endpoint integration
public static class WebApplicationExtensions
{
    public static WebApplication UseBookTracker(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.Database.EnsureCreated(); // Create DB

            if (app.Configuration.GetValue<bool>("SeedDatabase"))
            {
                DatabaseSeeder.SeedBooks(dbContext, 500); // RUN.. (seeds)
            }
        }

        app.MapBookEndpoints();

        return app;
    }
}