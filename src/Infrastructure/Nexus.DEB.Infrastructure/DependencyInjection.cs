using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Infrastructure.Authentication;
using Nexus.DEB.Infrastructure.Persistence;
using Nexus.DEB.Infrastructure.Services;
using Nexus.DEB.Infrastructure.Validators;

namespace Nexus.DEB.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1024; // Limit number of entries (optional)
            });

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


            services.AddHttpClient("WorkflowApi", client =>
            {
                var baseUrl = configuration["LegacyApis:PAWS:BaseUrl"]
                    ?? throw new InvalidOperationException("LegacyApis:PAWS:BaseUrl is not configured");
                var timeout = int.Parse(configuration["LegacyApis:PAWS:Timeout"] ?? "30");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeout);
            });

            services.AddHttpClient("DmsApi", client =>
            {
                var baseUrl = configuration["LegacyApis:DMS:BaseUrl"]
                    ?? throw new InvalidOperationException("LegacyApis:DMS:BaseUrl is not configured");
                var timeout = int.Parse(configuration["LegacyApis:DMS:Timeout"] ?? "30");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeout);
            });

            // Database - Using Pooled DbContext Factory for better performance
            services.AddPooledDbContextFactory<DebContext>((sp, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DEB"),
                        b => b.MigrationsAssembly(typeof(DebContext).Assembly.FullName));
                options.AddInterceptors(sp.GetRequiredService<ChangeEventInterceptor>());
            });

            // Register IDebContext using a factory-created instance
            services.AddScoped<IDebContext>(provider =>
            {
                var factory = provider.GetRequiredService<IDbContextFactory<DebContext>>();
                return factory.CreateDbContext();
            });

			services.AddSingleton<ChangeEventInterceptor>();

			// Other infrastructure services will be registered here
			services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IDebService, DebService>(); 

            services.AddScoped<CbacService>();
            services.AddScoped<ICbacService>(provider =>
            {
                var innerService = provider.GetRequiredService<CbacService>();
                var cache = provider.GetRequiredService<IMemoryCache>();
                var currentUserService = provider.GetRequiredService<ICurrentUserService>();
                var configuration = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<ILogger<CachedCbacService>>();

                return new CachedCbacService(innerService, cache, currentUserService, configuration, logger);
            });

            services.AddTransient<CisService>();
            services.AddTransient<ICisService>(provider =>
            {
                var innerService = provider.GetRequiredService<CisService>();
                var cache = provider.GetRequiredService<IMemoryCache>();
                var configuration = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<ILogger<CachedCisService>>();

                return new CachedCisService(innerService, cache, configuration, logger);
            });

            // Note: PawsService is used by field resolvers and needs to be transient
            services.AddTransient<ICurrentUserService, CurrentUserService>();
            services.AddTransient<IPawsService, PawsService>();
            services.AddTransient<IDmsService, DmsService>();
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
            services.AddTransient<IApplicationSettingsService, ApplicationSettingsService>();


            services.AddScoped<IWorkflowValidationService, WorkflowValidationService>();
            services.AddScoped<IWorkflowSideEffectService, WorkflowSideEffectService>();

            // Register domain services
            services.AddScoped<IStatementDomainService, StatementDomainService>();
            services.AddScoped<ICommentDomainService, CommentDomainService>();
			services.AddScoped<ISavedSearchDomainService, SavedSearchDomainService>();
            services.AddScoped<ITaskDomainService, TaskDomainService>();

            // Register validator registry
            services.AddScoped<ITransitionValidatorRegistry, TransitionValidatorRegistry>();
            services.AddScoped<ITransitionSideEffectRegistry, TransitionSideEffectRegistry>();

            // Register all validators (auto-discovered)
            services.AddScoped<ITransitionValidator, ValidateReviewDateTransitionValidator>();

            return services;
        }
    }
}
