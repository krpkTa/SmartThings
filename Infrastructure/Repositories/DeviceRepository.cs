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
        private readonly IMqttClientService _mqttClientService;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public DeviceRepository(IMqttClientService mqttClientService)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _filePath = Path.Combine(appDataPath, "SmartThings", "devices.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));

            _mqttClientService = mqttClientService;
            _devices = new List<SmartDevice>(); // Инициализируем пустым списком
        }

        public int GetCount() => _devices.Count;

        public async Task InitializeAsync()
        {
            try
            {
                _devices = await LoadDevicesAsync();
                Debug.WriteLine($"Loaded {_devices.Count} devices");

                if (_mqttClientService.IsConnected)
                {
                    await SubscribeToAllDevicesAsync();
                }
                else
                {
                    Debug.WriteLine("MQTT not connected, delaying subscriptions");
                    _mqttClientService.Connected += OnMqttConnected;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Initialize error: {ex}");
            }
        }

        private async void OnMqttConnected(object sender, EventArgs e)
        {
            try
            {
                await SubscribeToAllDevicesAsync();
                _mqttClientService.Connected -= OnMqttConnected;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Delayed subscribe error: {ex}");
            }
        }

        public async Task<SmartDevice> AddAsync(SmartDevice device)
        {
            // Загружаем текущий список устройств
            var currentDevices = await LoadDevicesAsync();

            // Проверяем, нет ли уже устройства с таким UID
            if (currentDevices.Any(d => d.UID == device.UID))
            {
                Debug.WriteLine($"Device with UID {device.UID} already exists");
                throw new InvalidOperationException($"Device with UID {device.UID} already exists");
            }

            // Добавляем новое устройство
            currentDevices.Add(device);
            _devices = currentDevices; // Обновляем поле класса

            // Сохраняем обновленный список
            await SaveDevicesAsync();

            // Подписываемся на топики
            await SubscribeToDeviceTopics(device);

            return device;
        }


        private async Task SubscribeToAllDevicesAsync()
        {
            foreach (var device in _devices)
            {
                await SubscribeToDeviceTopics(device);
            }
        }

        private async Task SubscribeToDeviceTopics(SmartDevice device)
        {
            foreach (var topic in device.Topics)
            {
                var fullTopic = $"{device.UID}/{topic}";
                try
                {
                    await _mqttClientService.SubscribeAsync(fullTopic);
                    Debug.WriteLine($"Subscribed to: {fullTopic}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to subscribe to {fullTopic}: {ex.Message}");
                }
            }
        }

        public Task<IEnumerable<SmartDevice>> GetAllAsync() => Task.FromResult(_devices.AsEnumerable());

        public Task<SmartDevice?> GetByIdAsync(Guid id) => Task.FromResult(_devices.FirstOrDefault(d => d.Id == id));

        public async Task DeleteAsync(SmartDevice device)
        {
            _devices.Remove(device);
            await SaveDevicesAsync();
        }

        public async Task<List<SmartDevice>> LoadDevicesAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    Debug.WriteLine("Devices file not found, creating new");
                    return new List<SmartDevice>();
                }

                var json = await File.ReadAllTextAsync(_filePath);
                var devices = JsonSerializer.Deserialize<List<SmartDevice>>(json, _jsonOptions);

                return devices ?? new List<SmartDevice>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading devices: {ex}");
                return new List<SmartDevice>();
            }
        }

        private async Task SaveDevicesAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_devices, _jsonOptions);

                // Создаем резервную копию перед записью
                if (File.Exists(_filePath))
                {
                    var backupPath = _filePath + ".bak";
                    File.Copy(_filePath, backupPath, overwrite: true);
                }

                // Атомарная запись файла
                var tempPath = Path.GetTempFileName();
                await File.WriteAllTextAsync(tempPath, json);
                File.Move(tempPath, _filePath, overwrite: true);

                Debug.WriteLine($"Saved {_devices.Count} devices to storage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving devices: {ex.Message}");
                throw; // Пробрасываем исключение дальше
            }
        }
    }
}
