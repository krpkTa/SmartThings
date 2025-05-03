using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Interfaces;
using Microsoft.Maui.Controls;
using MQTTnet.Exceptions;
using SmartThings.Views;
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

    [ObservableProperty]
    private bool _deviceState;

    public MainViewModel(IMqttClientService mqttService, INetworkDiscoveryService networkService)
    {
        _mqttService = mqttService;
        _networkService = networkService;
        _ = InitializeAsync(); // Запуск без ожидания
    }
    [RelayCommand]
    private async Task SwitchOn()
    {
        try
        {
            await _mqttService.PublishAsync(
                topic: "Hrodno/ESP-D6-357E/LED",
                payload: "1");
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Ошибка отправки: {ex}");
            DeviceState = !DeviceState;
        }
    }
    [RelayCommand]
    private async Task SwitchOff()
    {
        try
        {
            await _mqttService.PublishAsync(
                topic: "Hrodno/ESP-D6-357E/LED",
                payload: "0");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка отправки: {ex}");
            DeviceState = !DeviceState;
        }
    }
    [RelayCommand]
    private async Task NavigateToSensors(Frame frame)
    {
        if (frame != null)
        {
            await frame.ScaleTo(0.95, 100, Easing.CubicOut);
            await frame.ScaleTo(1, 100, Easing.CubicIn);
        }

        await Shell.Current.GoToAsync(nameof(SensorPage));
    }
    [RelayCommand]
    private async Task NavigateToAddDevice(Frame frame)
    {
        if (frame != null)
        {
            await frame.ScaleTo(0.95, 100, Easing.CubicOut);
            await frame.ScaleTo(1, 100, Easing.CubicIn);
        }

        await Shell.Current.GoToAsync(nameof(AddDevicePage));
    }
    [RelayCommand]
    private async Task NavigateToAllDevice(Frame frame)
    {
        if (frame != null)
        {
            await frame.ScaleTo(0.95, 100, Easing.CubicOut);
            await frame.ScaleTo(1, 100, Easing.CubicIn);
        }

        await Shell.Current.GoToAsync(nameof(AllDevices));
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