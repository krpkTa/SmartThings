using Domain.Interfaces;
using Domain.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Infrastructure.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly string _filePath;
        private List<SmartDevice> _devices;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public DeviceRepository()
        {
            // Use Environment.SpecialFolder as a cross-platform solution
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _filePath = Path.Combine(appDataPath, "SmartThings", "devices.json");

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            _devices = LoadDevices();
        }

        public async Task<SmartDevice> AddAsync(SmartDevice device)
        {
            _devices.Add(device);
            await SaveDevicesAsync();
            return device;
        }

        public Task<IEnumerable<SmartDevice>> GetAllAsync() => Task.FromResult(_devices.AsEnumerable());

        public Task<SmartDevice?> GetByIdAsync(Guid id) => Task.FromResult(_devices.FirstOrDefault(d => d.Id == id));

        public async Task DeleteAsync(SmartDevice device)
        {
            _devices.Remove(device);
            await SaveDevicesAsync();
        }

        private List<SmartDevice> LoadDevices()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<SmartDevice>();

                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<SmartDevice>>(json, _jsonOptions)
                       ?? new List<SmartDevice>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading devices: {ex.Message}");
                return new List<SmartDevice>();
            }
        }

        private async Task SaveDevicesAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_devices, _jsonOptions);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving devices: {ex.Message}");
            }
        }
    }
}