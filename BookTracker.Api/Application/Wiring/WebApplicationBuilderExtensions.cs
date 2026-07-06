using BookTracker.Api.Application;
using BookTracker.Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Wiring;

// Groups all BookTracker startup configuration
// to keep Program.cs clean and readable.
public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddBookTracker(this WebApplicationBuilder builder)
    {
        RegisterStorage(builder);
        RegisterHandlers(builder.Services); // Automatically discover and register all handlers.


        return builder;
    }

    private static void RegisterStorage(WebApplicationBuilder builder) // Registers the database and repository services.
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("BookTracker")));

        builder.Services.AddScoped<IBookRepository, EfBookRepository>();
    }

    private static void RegisterHandlers(IServiceCollection services)
    {

        // Reflection inspects the compiled assembly at runtime.
        // It retrieves every type (classes, interfaces, records, etc.),
        // then keeps only the concrete classes that implement IHandler.
        var handlerTypes = HandlerMarker.Assembly // (ASSEMBLY) >>> BookTracker.Api.dll (IN RUNTIME)
            .GetTypes()
            .Where(IsHandler);

        // Register each discovered handler with the dependency injection container.
        foreach (var type in handlerTypes)
        {
            services.AddScoped(type);
        }
    }

    // Determines whether a type should be registered as a Handler.
    private static bool IsHandler(Type type)
    {
        return type is { IsClass: true, IsAbstract: false }
            && type.IsAssignableTo(HandlerMarker); // Keep only classes that implement the IHandler marker interface.

    }

    // Store Type information about IHandler.
    // Reflection uses this marker to identify Handler classes.
    private static readonly Type HandlerMarker = typeof(IHandler);
}