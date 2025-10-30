using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Nexus.DEB.Application;
using Nexus.DEB.Infrastructure;
using Nexus.DEB.Infrastructure.Authentication;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;
var configuration = builder.Configuration;
var allowedOrigins = configuration["CORS:AllowedOrigins"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("GraphQLPolicy", corsBuilder =>
    {
        if (environment.IsDevelopment())
        {
            // Development: Allow localhost origins for testing
            // Note: Can't use AllowAnyOrigin() with AllowCredentials()
            corsBuilder
                .WithOrigins(
                    "http://localhost:3000",  // React dev server
                    "http://localhost:5173",  // Vite dev server
                    "https://localhost:5001", // GraphQL IDE (Banana Cake Pop)
                    "https://localhost:7001"  // Alternative HTTPS port
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // Required for Forms Authentication cookies
        }
        else
        {
            // Production: Must have configured origins
            if (string.IsNullOrWhiteSpace(allowedOrigins))
            {
                throw new InvalidOperationException(
                    "CORS:AllowedOrigins must be configured for production environments. " +
                    "Add allowed origins to appsettings.json separated by semicolons.");
            }

            var domains = allowedOrigins
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .ToArray();

            if (domains.Length == 0)
            {
                throw new InvalidOperationException(
                    "CORS:AllowedOrigins is configured but contains no valid domains.");
            }

            corsBuilder
                .WithOrigins(domains)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // Required for Forms Authentication cookies
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);

builder.Services.AddHttpContextAccessor();

var decryptionKey = configuration["Authentication:DecryptionKey"] ?? throw new InvalidOperationException("Authentication:DecryptionKey is required");
var validationKey = configuration["Authentication:ValidationKey"] ?? throw new InvalidOperationException("Authentication:ValidationKey is required");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    var cookieName = configuration["Authentication:CookieName"] ?? ".ASPXAUTH";
    var cookieDomain = configuration["Authentication:CookieDomain"];

    // Parse RequireHttps (default: true)
    var requireHttps = true;
    if (bool.TryParse(configuration["Authentication:RequireHttps"], out var configHttps))
    {
        requireHttps = configHttps;
    }

    // Parse SlidingExpiration (default: true)
    var slidingExpiration = true;
    if (bool.TryParse(configuration["Authentication:SlidingExpiration"], out var configSliding))
    {
        slidingExpiration = configSliding;
    }

    // Parse CookieExpirationMinutes (default: 480 = 8 hours)
    var cookieExpirationMinutes = 480;
    if (int.TryParse(configuration["Authentication:CookieExpirationMinutes"], out var configMinutes))
    {
        cookieExpirationMinutes = configMinutes;
    }

    options.Cookie.Name = cookieName;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = requireHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = slidingExpiration;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieExpirationMinutes);

    if (!string.IsNullOrWhiteSpace(cookieDomain))
    {
        options.Cookie.Domain = cookieDomain;
    }

    // Use the custom AspNetTicketDataFormat for .NET Framework 4.8 compatibility
    options.TicketDataFormat = new AspNetTicketDataFormat(decryptionKey, validationKey);

    // Configure authentication challenge behavior
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            // For API calls, return 401 instead of redirecting
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            // For API calls, return 403 instead of redirecting
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder
    .AddGraphQL()
    .AddAuthorization()
    .AddTypes();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseCors("GraphQLPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);
