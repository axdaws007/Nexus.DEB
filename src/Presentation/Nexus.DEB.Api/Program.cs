using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Nexus.DEB.Api.Restful;
using Nexus.DEB.Api.Security;
using Nexus.DEB.Application;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain;
using Nexus.DEB.Infrastructure;
using Nexus.DEB.Infrastructure.Authentication;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;
var configuration = builder.Configuration;
var allowedOrigins = configuration["CORS:AllowedOrigins"];
var useLocalNitro = configuration.GetValue<bool>("UseLocalNitro", false);

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
                .WithExposedHeaders("Content-Disposition", "Content-Length")
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
                .WithExposedHeaders("Content-Disposition", "Content-Length")
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(DebHelper.Policies.CanAddComments, policy => policy.RequireClaim(DebHelper.ClaimTypes.Capability, DebHelper.Capabilites.AllCreateCommentCapabilities));
    options.AddPolicy(DebHelper.Policies.CanDeleteComments, policy => policy.RequireClaim(DebHelper.ClaimTypes.Capability, DebHelper.Capabilites.AllDeleteCommentCapabilities));
    options.AddPolicy(DebHelper.Policies.CanAddSoCEvidence, policy => policy.RequireClaim(DebHelper.ClaimTypes.Capability, DebHelper.Capabilites.CanAddSoCEvidence));
    options.AddPolicy(DebHelper.Policies.CanEditSoCEvidence, policy => policy.RequireClaim(DebHelper.ClaimTypes.Capability, DebHelper.Capabilites.CanEditSoCEvidence));
    options.AddPolicy(DebHelper.Policies.CanDeleteSoCEvidence, policy => policy.RequireClaim(DebHelper.ClaimTypes.Capability, DebHelper.Capabilites.CanDeleteSoCEvidence));
    options.AddPolicy(DebHelper.Policies.CanViewSoCEvidence, policy => policy.RequireClaim(DebHelper.ClaimTypes.Capability, DebHelper.Capabilites.CanViewSoCEvidence));

    options.AddPolicy(DebHelper.Policies.CanCreateOrEditSoC, policy => policy.RequireClaim(DebHelper.ClaimTypes.Capability, DebHelper.Capabilites.CanEditSoC));
    options.AddPolicy(DebHelper.Policies.CanCreateSoCTask, policy => policy.RequireClaim(DebHelper.ClaimTypes.Capability, DebHelper.Capabilites.CanCreateSoCTask));
    options.AddPolicy(DebHelper.Policies.CanEditSoCTask, policy => policy.RequireClaim(DebHelper.ClaimTypes.Capability, DebHelper.Capabilites.CanEditSoCTask));
});

builder
    .AddGraphQL()
    .AddAuthorization()
    .AddTypes()
    .AddMutationConventions()
    .AddGlobalObjectIdentification()
    .ModifyCostOptions(options =>
    {
        options.MaxFieldCost = 3000;
    })
    .ModifyPagingOptions(x =>
    {
        x.MaxPageSize = 200;
        x.IncludeTotalCount = true;
    })
    .AddProjections()
    .AddSorting()
    ;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("edev"))
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("GraphQLPolicy");
app.UseAuthentication();
app.UseMiddleware<CapabilitiesHttpRequestInterceptor>();
app.UseAuthorization();

app.Use(async (ctx, next) =>
{
	var db = ctx.RequestServices.GetRequiredService<IDebContext>();
	await db.SetFormattedUser();
	await next();
});

app.MapGraphQL().WithOptions(new GraphQLServerOptions
{
    Tool =
    {
        ServeMode = (useLocalNitro) ? GraphQLToolServeMode.Embedded : GraphQLToolServeMode.Latest
    }
});

app.MapExportEndpoints();
app.MapWorkflowDiagramEndpoints();
app.MapDmsEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapTestDataEndpoints();
}

app.RunWithGraphQLCommands(args);
