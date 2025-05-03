using ApplicationLayer.Service;
using Domain.Interfaces;
using Domain.Models;

namespace SmartThings.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;

    public DeviceService(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<SmartDevice> AddDeviceAsync(string name, string uid, string topic)
    {
        var device = new SmartDevice
        {
            Id = Guid.NewGuid(),
            Name = name,
            UID = uid,
            Topic = topic,
        };

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
}