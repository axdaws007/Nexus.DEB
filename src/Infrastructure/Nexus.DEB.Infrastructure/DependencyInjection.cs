using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Infrastructure.Authentication;
using Nexus.DEB.Infrastructure.Persistence;
using Nexus.DEB.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<AspNetTicketDataFormat>(provider =>
            {
                var decryptionKey = configuration["Authentication:DecryptionKey"]
                    ?? throw new InvalidOperationException("Authentication:DecryptionKey is not configured");
                var validationKey = configuration["Authentication:ValidationKey"]
                    ?? throw new InvalidOperationException("Authentication:ValidationKey is not configured");

                return new AspNetTicketDataFormat(decryptionKey, validationKey);
            });

            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUserValidationService, SampleUserValidationService>();


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
