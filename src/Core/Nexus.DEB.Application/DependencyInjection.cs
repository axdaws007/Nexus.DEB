using Microsoft.Extensions.DependencyInjection;

namespace Nexus.DEB.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register application services here
            // Example: services.AddScoped<IYourService, YourService>();

            return services;
        }
    }
}
