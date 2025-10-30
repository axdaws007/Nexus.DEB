using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Infrastructure.Authentication;
using Nexus.DEB.Infrastructure.Persistence;
using Nexus.DEB.Infrastructure.Services;
using System.Net;

namespace Nexus.DEB.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMemoryCache();

            services.AddSingleton<AspNetTicketDataFormat>(provider =>
            {
                var decryptionKey = configuration["Authentication:DecryptionKey"]
                    ?? throw new InvalidOperationException("Authentication:DecryptionKey is not configured");
                var validationKey = configuration["Authentication:ValidationKey"]
                    ?? throw new InvalidOperationException("Authentication:ValidationKey is not configured");

                return new AspNetTicketDataFormat(decryptionKey, validationKey);
            });

            var cisIdentityBaseUrl = configuration["LegacyApis:CIS:BaseUrl"]
                ?? throw new InvalidOperationException("LegacyApis:CIS:BaseUrl is not configured");

            var cisIdentityTimeout = int.Parse(configuration["LegacyApis:CIS:Timeout"]);

            services.AddHttpClient("CisApi")
                    .ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new HttpClientHandler
                        {
                            UseCookies = false,
                            AllowAutoRedirect = false // Important for authentication scenarios
                        };
                    })
                    .ConfigureHttpClient(client =>
                    {
                        client.BaseAddress = new Uri(cisIdentityBaseUrl);
                        client.Timeout = TimeSpan.FromSeconds(cisIdentityTimeout);
                        // Add any headers if needed (API keys, etc.)
                        // client.DefaultRequestHeaders.Add("X-API-Key", "your-api-key");
                    });

            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUserValidationService, CisIdentityApiClient>();


            // Database - Using Pooled DbContext Factory for better performance
            services.AddPooledDbContextFactory<DebContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DEB"),
                    b => b.MigrationsAssembly(typeof(DebContext).Assembly.FullName)));

            // Register IDebContext using a factory-created instance
            services.AddScoped<IDebContext>(provider =>
            {
                var factory = provider.GetRequiredService<IDbContextFactory<DebContext>>();
                return factory.CreateDbContext();
            });

            // Other infrastructure services will be registered here

            return services;
        }
    }
}
