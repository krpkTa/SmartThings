using ApplicationLayer.Service;
using Domain.Interfaces;
using Domain.Models;
using System.Diagnostics;

namespace SmartThings.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMqttClientService _mqttClientService;

    public DeviceService(
        IDeviceRepository deviceRepository,
        IMqttClientService mqttClientService)
    {
        _deviceRepository = deviceRepository;
        _mqttClientService = mqttClientService;
    }
    public async Task InitializeAsync()
    {
        try
        {
            // Явно загружаем устройства через репозиторий
            var devices = await _deviceRepository.GetAllAsync();
            Debug.WriteLine($"Initializing service with {devices.Count()} devices");

            // Подписываемся на топики всех устройств
            foreach (var device in devices)
            {
                await SubscribeToAllDevicesAsync(device);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing service: {ex}");
        }
    }
    

    private async Task SubscribeToAllDevicesAsync(SmartDevice device)
    {
        foreach (var topic in device.Topics)
        {
            var fullTopic = $"{device.UID}/{topic}";
            await _mqttClientService.SubscribeAsync(fullTopic);
        }
    }

    public async Task<SmartDevice> AddDeviceAsync(string name, string uid, List<string> deviceTopics)
    {
        var device = new SmartDevice
        {
            Id = Guid.NewGuid(),
            Name = name,
            UID = uid,
            Topics = deviceTopics
        };

        foreach (var topic in deviceTopics)
        {
            var fullTopic = $"{uid}/{topic}";
            await _mqttClientService.SubscribeAsync(fullTopic);
        }

        return await _deviceRepository.AddAsync(device);
    }

    public async Task<IEnumerable<SmartDevice>> GetAllDevicesAsync()
    {
        return await _deviceRepository.GetAllAsync();
    }

    public async Task<SmartDevice?> GetDeviceByIdAsync(Guid id)
    {
        return await _deviceRepository.GetByIdAsync(id);
    }

    public async Task<bool> DeleteDeviceAsync(Guid deviceId)
    {
        try
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null) return false;

            await _deviceRepository.DeleteAsync(device);
            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<List<SmartDevice>> LoadDevicesAsync()
    {
        return await _deviceRepository.LoadDevicesAsync();
    }
}