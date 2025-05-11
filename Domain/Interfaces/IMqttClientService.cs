using Domain.Models;

namespace Domain.Interfaces
{
    public interface IMqttClientService
    {
        bool IsConnected { get; }
        Task ConnectAsync();
        Task DisconnectAsync();
        Task PublishAsync(string topic, string payload);
        Task SubscribeAsync(string topic);
        Task InitializeAsync();
        Task SubscribeToDeviceAsync(SmartDevice device);
        Task UnsubscribeFromDeviceAsync(string deviceUid);
        Task<DeviceHistoryData> GetDeviceHistoryAsync(string uid, TimeSpan timeout);

        event EventHandler Connected; // Добавляем событие
        event EventHandler Disconnected;
        event EventHandler<string> MessageReceived;
        event EventHandler<SensorData> SensorDataReceived;
    }
}
