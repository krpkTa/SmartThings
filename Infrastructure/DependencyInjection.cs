using Domain.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<INetworkDiscoveryService, NetworkDiscoveryService>();

            services.AddSingleton<IMqttClientService>(provider =>
            {
                var networkService = provider.GetRequiredService<INetworkDiscoveryService>();
                return new MqttClientService(networkService);
            });

            return services;
        }
    }
}
