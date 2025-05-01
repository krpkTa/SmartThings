using Domain.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Общие сервисы (для всех платформ)
            services.AddSingleton<IMqttClientService>(provider =>
            {
                var networkService = provider.GetRequiredService<INetworkDiscoveryService>();
                return new MqttClientService(networkService);
            });

            // Платформенно-специфичные сервисы
#if ANDROID
            services.AddSingleton<INetworkDiscoveryService, AndroidNetworkInfo>();
#else
            services.AddSingleton<INetworkDiscoveryService, NetworkDiscoveryService>();
#endif

            return services;
        }
    }
}