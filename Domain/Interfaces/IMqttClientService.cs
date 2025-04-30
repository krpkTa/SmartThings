using Domain.Models;

namespace Domain.Interfaces
{
    public interface IMqttClientService
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task PublishAsync(string topic, string payload);
        Task SubscribeAsync(string topic);
        Task InitializeAsync();

        event EventHandler<string> MessageReceived;
        event EventHandler<SensorData> SensorDataReceived;
    }
}
