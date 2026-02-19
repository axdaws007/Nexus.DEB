using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Infrastructure.Authentication;
using Nexus.DEB.Infrastructure.Events;
using Nexus.DEB.Infrastructure.Http;
using Nexus.DEB.Infrastructure.Persistence;
using Nexus.DEB.Infrastructure.Services;
using Nexus.DEB.Infrastructure.Services.Registries;
using Nexus.DEB.Infrastructure.Validators;
using System.Reflection;

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
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

            services.AddHttpClient("CbacApi", client =>
            {
                var baseUrl = configuration["LegacyApis:CBAC:BaseUrl"]
                    ?? throw new InvalidOperationException("LegacyApis:CBAC:BaseUrl is not configured");
                var cbacTimeout = int.Parse(configuration["LegacyApis:CBAC:Timeout"] ?? "30");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(cbacTimeout);
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();


            services.AddHttpClient("WorkflowApi", client =>
            {
                var baseUrl = configuration["LegacyApis:PAWS:BaseUrl"]
                    ?? throw new InvalidOperationException("LegacyApis:PAWS:BaseUrl is not configured");
                var timeout = int.Parse(configuration["LegacyApis:PAWS:Timeout"] ?? "30");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeout);
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

            services.AddHttpClient("DmsApi", client =>
            {
                var baseUrl = configuration["LegacyApis:DMS:BaseUrl"]
                    ?? throw new InvalidOperationException("LegacyApis:DMS:BaseUrl is not configured");
                var timeout = int.Parse(configuration["LegacyApis:DMS:Timeout"] ?? "30");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeout);
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

            services.AddHttpClient("AuditApi", client =>
            {
                var baseUrl = configuration["LegacyApis:Audit:BaseUrl"]
                    ?? throw new InvalidOperationException("LegacyApis:Audit:BaseUrl is not configured");
                var timeout = int.Parse(configuration["LegacyApis:Audit:Timeout"] ?? "30");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeout);
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

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
			services.AddScoped<IDataLoaderService, DataLoaderService>();

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
            services.AddTransient<IAuditService, AuditService>();
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
            services.AddTransient<IApplicationSettingsService, ApplicationSettingsService>(); 


			services.AddScoped<IWorkflowValidationService, WorkflowValidationService>();
            services.AddScoped<IWorkflowSideEffectService, WorkflowSideEffectService>();

            // Register domain services
            services.AddScoped<IRequirementDomainService, RequirementDomainService>();
            services.AddScoped<IScopeDomainService, ScopeDomainService>();
			services.AddScoped<IStandardVersionDomainService, StandardVersionDomainService>();
			services.AddScoped<IStatementDomainService, StatementDomainService>();
            services.AddScoped<ICommentDomainService, CommentDomainService>();
            services.AddScoped<ISavedSearchDomainService, SavedSearchDomainService>();
            services.AddScoped<ITaskDomainService, TaskDomainService>();
            services.AddScoped<ISectionDomainService, SectionDomainService>();

            // Register validator registry
            services.AddScoped<ITransitionValidatorRegistry, TransitionValidatorRegistry>();
            services.AddScoped<ITransitionSideEffectRegistry, TransitionSideEffectRegistry>();

            // Register all validators (auto-discovered)
            services.AddScoped<ITransitionValidator, ValidateReviewDateTransitionValidator>();

            // HTTP request correlationId service
            services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

            // Register the delegating handler
            services.AddTransient<CorrelationIdDelegatingHandler>();

            return services;
        }

        /// <summary>
        /// Adds domain event publishing infrastructure and auto-discovers all subscribers.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configure">Optional configuration action</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddDomainEvents(
            this IServiceCollection services,
            Action<DomainEventOptions>? configure = null)
        {
            var options = new DomainEventOptions();
            configure?.Invoke(options);

            // Register the publisher
            services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

            // Register the dashboard provider registry
            services.AddScoped<IDashboardInfoProviderRegistry, DashboardInfoProviderRegistry>();

            // Auto-discover and register subscribers
            RegisterSubscribers(services, options);

            // Auto-discover and register dashboard info providers
            RegisterDashboardInfoProviders(services, options);

            return services;
        }

        private static void RegisterSubscribers(IServiceCollection services, DomainEventOptions options)
        {
            var logger = options.Logger;
            var subscriberInterfaceType = typeof(IDomainEventSubscriber<>);

            // Get assemblies to scan
            var assemblies = GetAssembliesToScan(options);

            logger?.LogDebug(
                "Scanning {Count} assemblies for IDomainEventSubscriber implementations",
                assemblies.Count);

            var registeredCount = 0;
            var eventTypes = new HashSet<Type>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t is { IsClass: true, IsAbstract: false })
                        .Where(t => t.GetInterfaces().Any(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == subscriberInterfaceType))
                        .ToList();

                    if (types.Count > 0)
                    {
                        logger?.LogDebug(
                            "Found {Count} subscriber(s) in {Assembly}",
                            types.Count,
                            assembly.GetName().Name);
                    }

                    foreach (var type in types)
                    {
                        var interfaces = type.GetInterfaces()
                            .Where(i => i.IsGenericType &&
                                        i.GetGenericTypeDefinition() == subscriberInterfaceType);

                        foreach (var @interface in interfaces)
                        {
                            // Register the subscriber as the interface type
                            services.AddScoped(@interface, type);

                            var eventType = @interface.GetGenericArguments()[0];
                            eventTypes.Add(eventType);
                            registeredCount++;

                            logger?.LogDebug(
                                "Registered subscriber: {Type} for event {EventType}",
                                type.Name,
                                eventType.Name);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    logger?.LogWarning(
                        ex,
                        "Could not load types from assembly {Assembly}",
                        assembly.GetName().Name);
                }
            }

            logger?.LogInformation(
                "Domain events configured: {Count} subscriber(s) for {EventCount} event type(s)",
                registeredCount,
                eventTypes.Count);
        }

        private static void RegisterDashboardInfoProviders(IServiceCollection services, DomainEventOptions options)
        {
            var logger = options.Logger;
            var providerInterfaceType = typeof(IDashboardInfoProvider);

            var assemblies = GetAssembliesToScan(options);

            logger?.LogDebug(
                "Scanning {Count} assemblies for IDashboardInfoProvider implementations",
                assemblies.Count);

            var registeredCount = 0;

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t is { IsClass: true, IsAbstract: false })
                        .Where(t => providerInterfaceType.IsAssignableFrom(t))
                        .ToList();

                    if (types.Count > 0)
                    {
                        logger?.LogDebug(
                            "Found {Count} dashboard provider(s) in {Assembly}",
                            types.Count,
                            assembly.GetName().Name);
                    }

                    foreach (var type in types)
                    {
                        // Register as both the interface and concrete type
                        services.AddScoped(providerInterfaceType, type);
                        registeredCount++;

                        logger?.LogDebug(
                            "Registered dashboard provider: {Type}",
                            type.Name);
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    logger?.LogWarning(
                        ex,
                        "Could not load types from assembly {Assembly}",
                        assembly.GetName().Name);
                }
            }

            logger?.LogInformation(
                "Dashboard providers configured: {Count} provider(s)",
                registeredCount);
        }

        private static List<Assembly> GetAssembliesToScan(DomainEventOptions options)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic &&
                            options.AssemblyPrefixes.Any(p =>
                                a.GetName().Name?.StartsWith(p, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();
        }
    }
}