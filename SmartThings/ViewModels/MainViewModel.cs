using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Interfaces;
using MQTTnet.Exceptions;
using System.Diagnostics;
using System.Net.Sockets;

public partial class MainViewModel : ObservableObject
{
    private readonly IMqttClientService _mqttService;
    private readonly INetworkDiscoveryService _networkService;
    private bool _isInitialized = false;

    [ObservableProperty]
    private string _localIpAddress = "Идет определение...";

    [ObservableProperty]
    private string _statusMessage = "Инициализация...";

    public MainViewModel(IMqttClientService mqttService, INetworkDiscoveryService networkService)
    {
        _mqttService = mqttService;
        _networkService = networkService;
        _ = InitializeAsync(); // Запуск без ожидания
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            Debug.WriteLine("Этап 1: Получение IP");
            var ip = await _networkService.GetLocalNetworkIpAsync().ConfigureAwait(false);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                LocalIpAddress = ip;
                StatusMessage = "IP получен, подключение...";
            });

            Debug.WriteLine($"Этап 2: Подключение MQTT с IP: {ip}");
            await _mqttService.ConnectAsync().ConfigureAwait(false);
            await _mqttService.SubscribeAsync("home/status").ConfigureAwait(false);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusMessage = "Успешное подключение";
                _isInitialized = true;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка инициализации: {ex}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusMessage = $"Ошибка: {GetUserFriendlyError(ex)}";
                LocalIpAddress = "Ошибка подключения";
            });
        }
    }

    private string GetUserFriendlyError(Exception ex)
    {
        return ex switch
        {
            SocketException => "Проблемы с сетью",
            MqttCommunicationException => "Ошибка связи с MQTT",
            _ => ex.Message
        };
    }
}