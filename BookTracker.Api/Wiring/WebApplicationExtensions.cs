using BookTracker.Api.Domain.Members;
using BookTracker.Api.Endpoints;
using BookTracker.Api.Endpoints.Auth;
using BookTracker.Api.Endpoints.Members;
using BookTracker.Api.Seeding;
using BookTracker.Api.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookTracker.Api.Middleware;

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
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Member>>();

            /*
            EnsureCreated() → Creates the database once. No schema updates.
            Migrations → Track database schema changes over time.
            Migrate() → Applies only pending migrations and keeps existing data intact.
            */
            // dbContext.Database.EnsureCreated(); // Create DB
            dbContext.Database.Migrate(); // Apply pending migrations.....



            if (app.Configuration.GetValue<bool>("SeedDatabase"))
            {
                DatabaseSeeder.SeedBooks(dbContext, 500); // RUN.. (seeds)
                DatabaseSeeder.SeedAdministrator(dbContext, app.Configuration, passwordHasher);

            }
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseAuthentication(); // Who are YOU?
        app.UseAuthorization(); // Are YOU allowed???
        app.MapAuthEndpoints();
        app.MapBookEndpoints();
        app.MapMemberEndpoints();

        return app;
    }
}