using System.Text;
using BookTracker.Api.Application;
using BookTracker.Api.Domain.Members;
using BookTracker.Api.Security;
using BookTracker.Api.Storage;
using BookTracker.Api.Storage.Books;
using BookTracker.Api.Storage.Members;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BookTracker.Api.Wiring;

// Groups all BookTracker startup configuration
// to keep Program.cs clean and readable.
public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddBookTracker(this WebApplicationBuilder builder)
    {
        RegisterStorage(builder);
        RegisterHandlers(builder.Services);
        RegisterAuthentication(builder);

        return builder;
    }

    private static void RegisterStorage(WebApplicationBuilder builder) // Registers the database and repository services.
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("BookTracker")));

        builder.Services.AddScoped<IBookRepository, EfBookRepository>();
        builder.Services.AddScoped<IMemberRepository, EfMemberRepository>();
        builder.Services.AddScoped<IPasswordHasher<Member>, PasswordHasher<Member>>();
    }

    private static void RegisterAuthentication(WebApplicationBuilder builder)
    // This fucntion method tells ASP.NET Core: From today, the project uses JWT for login, and this is how to generate and verify tokens.
    {
        var settings = builder.Configuration // lees van appsettings.json.. appsettings.Development.json... User Secrets
            .GetRequiredSection(JwtSettings.SectionName)
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are missing.");

        if (string.IsNullOrWhiteSpace(settings.SigningKey))
        {
            throw new InvalidOperationException("JWT signing key is missing.");
        }

        builder.Services.AddSingleton(settings);
        builder.Services.AddScoped<JwtTokenGenerator>();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = settings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = settings.Audience,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(settings.SigningKey)),
                        ClockSkew = TimeSpan.Zero
                    };
            });

        builder.Services.AddAuthorization();
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