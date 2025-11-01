using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Infrastructure.Authentication;
using Nexus.DEB.Infrastructure.Persistence;
using Nexus.DEB.Infrastructure.Services;

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

            services.AddHttpClient("CisApi", client =>
            {
                var baseUrl = configuration["LegacyApis:CIS:BaseUrl"]
                    ?? throw new InvalidOperationException("LegacyApis:CIS:BaseUrl is not configured");
                var cisTimeout = int.Parse(configuration["LegacyApis:CIS:Timeout"] ?? "30");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(cisTimeout);
            });

            services.AddHttpClient("CbacApi", client =>
            {
                var baseUrl = configuration["LegacyApis:CBAC:BaseUrl"]
                    ?? throw new InvalidOperationException("LegacyApis:CBAC:BaseUrl is not configured");
                var cbacTimeout = int.Parse(configuration["LegacyApis:CBAC:Timeout"] ?? "30");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(cbacTimeout);
            });


            services.AddHttpClient("PawsApi", client =>
            {
                var baseUrl = configuration["LegacyApis:Paws:BaseUrl"]
                    ?? throw new InvalidOperationException("LegacyApis:Paws:BaseUrl is not configured");
                var timeout = int.Parse(configuration["LegacyApis:Paws:Timeout"] ?? "30");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeout);
            });

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
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<ICbacService, CbacService>();
            services.AddScoped<IDebService, DebService>();

            // Note: PawsService is used by field resolvers and needs to be transient
            services.AddTransient<ICurrentUserService, CurrentUserService>();
            services.AddTransient<IPawsService, PawsService>();
            services.AddTransient<ICisService, CisService>();

            return services;
        }
    }
}
