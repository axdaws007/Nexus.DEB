using Nexus.DEB.Application;
using Nexus.DEB.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;
var allowedOrigins = builder.Configuration["CORS:AllowedOrigins"];

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
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder
    .AddGraphQL()
    .AddTypes();

var app = builder.Build();

app.UseCors("GraphQLPolicy");

app.MapGraphQL();

app.RunWithGraphQLCommands(args);
