using MQTTnet;
using System.Text;
using Domain.Interfaces;
using Domain.Models;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Concurrent;

namespace Infrastructure.Services
{
    public class MqttClientService : IMqttClientService, IDisposable
    {
        private  IMqttClient _mqttClient;
        private MqttClientOptions _mqttClientOptions;
        private readonly ConcurrentDictionary<string, SmartDevice> _devices = new ConcurrentDictionary<string, SmartDevice>();
        private bool _isInitialized;

        public event EventHandler<string> MessageReceived;
        public event EventHandler<SensorData> SensorDataReceived;
        SensorData data = new SensorData();

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("192.168.242.119", 1883)
                .WithClientId($"SmartThingsApp{Guid.NewGuid().ToString()[..5]}")
                .Build();

            _mqttClient.ConnectedAsync += OnConnected;
            _mqttClient.DisconnectedAsync += OnDisconnected;
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageRecieved;

            //try
            //{
            //    await _mqttClient.ConnectAsync(_mqttClientOptions);
            //    _isInitialized = true;

            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine($"Initialization failed: {ex.Message}");

            //}

        }


        public async Task ConnectAsync()
        {
            if (_mqttClient.IsConnected) return;

            await _mqttClient.ConnectAsync(_mqttClientOptions);
            
            
        }

        public async Task SubscribeToDeviceAsync(SmartDevice device)
        {
            if (!_isInitialized) await InitializeAsync();
            
            if (_devices.TryAdd(device.UID, device))
            {
                var topic = $"{device.UID}/#";
                await _mqttClient.SubscribeAsync(topic);
            }
        }
        public async Task UnsubscribeFromDeviceAsync(string deviceUid)
        {
            if (_devices.TryRemove(deviceUid, out var device))
            {
                var topic = $"{device.Topic}/#";
                await _mqttClient.UnsubscribeAsync(topic);
                Debug.WriteLine($"Unsubscribed from device {deviceUid}");
            }
        }
        public async Task DisconnectAsync()
        {
            await _mqttClient.DisconnectAsync();
        }

        public void Dispose() =>  _mqttClient.Dispose();

        public async Task PublishAsync(string topic, string payload)
        {
            if (!_isInitialized) await InitializeAsync();

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            await _mqttClient.PublishAsync(message);
        }

        public async Task SubscribeAsync(string topic)
        {
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build());
        }

        private Task OnConnected(MqttClientConnectedEventArgs args)
        {
            return Task.CompletedTask;
        }

        private Task OnDisconnected(MqttClientDisconnectedEventArgs args)
        {
            return Task.CompletedTask;
        }
        private Task OnMessageRecieved(MqttApplicationMessageReceivedEventArgs args)
        {
            try
            {
                var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
                Debug.WriteLine($"📨 [MQTT] Топик: {args.ApplicationMessage.Topic}, Данные: {payload}");

                var sensorData = ParseSensorData(args.ApplicationMessage.Topic, payload);
                if (sensorData != null)
                {
                    Debug.WriteLine($"🔄 Данные получены: T={sensorData.Temperature}, H={sensorData.Humidity}, P={sensorData.Pressure}");
                    SensorDataReceived?.Invoke(this, sensorData);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❗ Ошибка: {ex}");
            }
            return Task.CompletedTask;
        }
        private SensorData? ParseSensorData(string topic, string payload)
        {

            try
            {
                payload = payload.Trim(); // Удаляем лишние символы

                if (topic.EndsWith("/T") && float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out float temp))
                {
                    data.Temperature = temp;
                }
                if (topic.EndsWith("/H") && float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out float humidity))
                {
                    data.Humidity = humidity;
                }
                if (topic.EndsWith("/P") && float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out float pressure))
                {
                    data.Pressure = pressure;
                }

                    return data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в ParseSensorData: {ex.Message}");
                return null;
            }
        }

    }
}
