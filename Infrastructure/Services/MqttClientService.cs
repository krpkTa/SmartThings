using MQTTnet;
using Domain.Interfaces;
using Domain.Models;
using System.Diagnostics;
using System.Text;
using System.Globalization;
using System.Text.Json;
using MQTTnet.Protocol;
using System.Buffers;

namespace Infrastructure.Services
{
    public class MqttClientService : IMqttClientService
    {
        private readonly INetworkDiscoveryService _networkService;
        private IMqttClient _mqttClient;
        private MqttClientOptions _mqttClientOptions;

        public bool IsConnected => _mqttClient?.IsConnected ?? false;
        private readonly Dictionary<string, SensorData> _lastValues = new();

        public event EventHandler<SensorData> SensorDataUpdated;
        public event EventHandler<string> MessageReceived;
        public event EventHandler<SensorData> SensorDataReceived;
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public MqttClientService(INetworkDiscoveryService networkService)
        {
            _networkService = networkService;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var localIp = await _networkService.GetLocalNetworkIpAsync();
                Debug.WriteLine($"Initializing MQTT with local IP: {localIp}");

                var factory = new MqttClientFactory();
                _mqttClient = factory.CreateMqttClient();

                _mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("192.168.1.106", 1883)
                    .WithClientId($"SmartThingsApp_{Guid.NewGuid().ToString()[..5]}")
                    .Build();

                SetupEventHandlers();
                await ConnectWithRetryAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MQTT Initialization failed: {ex}");
                throw;
            }
        }

        private void SetupEventHandlers()
        {
            _mqttClient.ConnectedAsync += async e =>
            {
                Connected?.Invoke(this, EventArgs.Empty);
                await Task.CompletedTask;
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                Disconnected?.Invoke(this, EventArgs.Empty);
                await Task.CompletedTask;
            };

            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        private async Task TryReconnectAsync()
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await ConnectWithRetryAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Reconnect failed: {ex}");
            }
        }

        private async Task ConnectWithRetryAsync(int maxAttempts = 3)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await _mqttClient.ConnectAsync(_mqttClientOptions);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Connection attempt {attempt} failed: {ex}");
                    if (attempt == maxAttempts) throw;
                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
                }
            }
        }

        public async Task ConnectAsync()
        {
            if (IsConnected) return;
            await ConnectWithRetryAsync();
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected) return;
            await _mqttClient.DisconnectAsync();
        }

        public async Task PublishAsync(string topic, string payload)
        {
            if (!IsConnected)
                throw new InvalidOperationException("MQTT client is not connected");

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            await _mqttClient.PublishAsync(message);
        }

        public async Task SubscribeAsync(string topic)
        {
            if (!IsConnected)
                throw new InvalidOperationException("MQTT client is not connected");

            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build());
        }

        private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            try
            {
                var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
                var topic = args.ApplicationMessage.Topic;

                Debug.WriteLine($"Received MQTT message - Topic: {topic}, Payload: {payload}");

                // 1. Получаем идентификатор устройства
                var deviceId = GetDeviceIdFromTopic(topic);

                // 2. Создаём или получаем объект данных для этого устройства
                if (!_lastValues.TryGetValue(deviceId, out var sensorData))
                {
                    sensorData = new SensorData
                    {
                        DeviceUID = deviceId,
                        DateTime = DateTime.UtcNow
                    };
                    _lastValues[deviceId] = sensorData;
                }

                // 3. Парсим данные из payload
                var parsedData = ParseSensorData(topic, payload);
                if (parsedData == null) return Task.CompletedTask;

                // 4. Обновляем соответствующие поля в sensorData
                if (parsedData.Temperature.HasValue)
                {
                    sensorData.Temperature = parsedData.Temperature;
                    sensorData.DateTime = DateTime.UtcNow;
                }
                if (parsedData.Humidity.HasValue)
                {
                    sensorData.Humidity = parsedData.Humidity;
                    sensorData.DateTime = DateTime.UtcNow;
                }
                if (parsedData.Pressure.HasValue)
                {
                    sensorData.Pressure = parsedData.Pressure;
                    sensorData.DateTime = DateTime.UtcNow;
                }

                // 5. Отправляем обновленные данные
                var dataCopy = new SensorData
                {
                    DeviceUID = sensorData.DeviceUID,
                    Temperature = sensorData.Temperature,
                    Humidity = sensorData.Humidity,
                    Pressure = sensorData.Pressure,
                    DateTime = sensorData.DateTime
                };

                SensorDataReceived?.Invoke(this, dataCopy);
                Debug.WriteLine($"Processed data - Temp: {dataCopy.Temperature}, Hum: {dataCopy.Humidity}, Pres: {dataCopy.Pressure}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing MQTT message: {ex}");
            }

            return Task.CompletedTask;
        }

        public async Task<DeviceHistoryData> GetDeviceHistoryAsync(string uid, TimeSpan timeout)
        {
            var responseTopic = $"devices/{uid}/history/data";

            // Subscribe to response topic first
            await _mqttClient.SubscribeAsync(responseTopic, MqttQualityOfServiceLevel.AtLeastOnce);

            var tcs = new TaskCompletionSource<DeviceHistoryData>();
            using var cts = new CancellationTokenSource(timeout);

            // Proper async handler with correct signature
            async Task Handler(MqttApplicationMessageReceivedEventArgs e)
            {
                try
                {
                    if (e.ApplicationMessage.Topic == responseTopic)
                    {
                        // Get payload as string (works for all MQTTnet versions)
                        string payload;

                        // For MQTTnet 4.x+
                        if (e.ApplicationMessage.Payload.ToString() != default)
                        {
                            payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload.ToArray());
                        }
                        // For older versions
                        else if (e.ApplicationMessage.Payload.ToString() != null)
                        {
                            payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                        }
                        else
                        {
                            throw new InvalidDataException("Empty message payload");
                        }

                        var data = JsonSerializer.Deserialize<DeviceHistoryData>(payload);
                        tcs.TrySetResult(data ?? throw new InvalidDataException("Deserialization returned null"));
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            _mqttClient.ApplicationMessageReceivedAsync += Handler;

            try
            {
                // Send history request
                await _mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"{uid}/history/get")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());

                // Wait for response with timeout
                using (cts.Token.Register(() => tcs.TrySetCanceled()))
                {
                    return await tcs.Task;
                }
            }
            finally
            {
                // Cleanup
                _mqttClient.ApplicationMessageReceivedAsync -= Handler;
                await _mqttClient.UnsubscribeAsync(responseTopic);
            }
        }

        // Вспомогательный метод для извлечения ID устройства из топика
        private string GetDeviceIdFromTopic(string topic)
        {
            var parts = topic.Split('/');
            return parts.Length > 0 ? parts[0] : "unknown";
        }

        private SensorData ParseSensorData(string topic, string payload)
        {
            try
            {
                payload = payload.Trim();
                var data = new SensorData();

                var topicParts = topic.Split('/');
                if (topicParts.Length != 2) return null;

                string topicName = topicParts[1].ToLower();

                switch (topicName)
                {
                    case "t":
                        if (float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out float temp))
                            data.Temperature = temp;
                        break;
                    case "h":
                        if (float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out float humidity))
                            data.Humidity = humidity;
                        break;
                    case "p":
                        if (float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out float pressure))
                            data.Pressure = pressure;
                        break;
                }

                return data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Parse error: {ex}");
                return null;
            }
        }

        public void Dispose()
        {
            _mqttClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}