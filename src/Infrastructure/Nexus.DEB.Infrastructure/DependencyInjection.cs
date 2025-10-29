using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Infrastructure.Persistence;
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
