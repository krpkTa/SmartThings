using MQTTnet;
using System.Text;
using Domain.Interfaces;
using Domain.Models;
using System.Diagnostics;
using System.Globalization;

namespace Infrastructure.Services
{
    public class MqttClientService : IMqttClientService, IDisposable
    {
        private readonly INetworkDiscoveryService _networkService;
        private  IMqttClient _mqttClient;
        private MqttClientOptions _mqttClientOptions;

        public event EventHandler<string> MessageReceived;
        public event EventHandler<SensorData> SensorDataReceived;
        SensorData data = new SensorData();

        public MqttClientService(INetworkDiscoveryService networkService)
        {
            _networkService = networkService;
        }

        public async Task InitializeAsync()
        {
            var localIp = await _networkService.GetLocalNetworkIpAsync();

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("192.168.242.119", 1883)
                .WithClientId($"SmartThingsApp{Guid.NewGuid().ToString()[..5]}")
                .Build();

            _mqttClient.ConnectedAsync += OnConnected;
            _mqttClient.DisconnectedAsync += OnDisconnected;
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageRecieved;


        }


        public async Task ConnectAsync()
        {
            if (_mqttClient.IsConnected) return;

            await _mqttClient.ConnectAsync(_mqttClientOptions);
            await SubscribeToSensorTopics();
            
        }

        private async Task SubscribeToSensorTopics()
        {
            await _mqttClient.SubscribeAsync("Hrodno/ESP-D6-357E/T");
            Debug.WriteLine("Subscride Hrodno/T");
            await _mqttClient.SubscribeAsync("Hrodno/ESP-D6-357E/H");
            Debug.WriteLine("Subscride Hrodno/H");
            await _mqttClient.SubscribeAsync("Hrodno/ESP-D6-357E/P");
            Debug.WriteLine("Subscride Hrodno/P");
        }

        public async Task DisconnectAsync()
        {
            await _mqttClient.DisconnectAsync();
        }

        public void Dispose() =>  _mqttClient.Dispose();

        public async Task PublishAsync(string topic, string payload)
        {
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
                

                switch (topic)
                {
                    case "Hrodno/ESP-D6-357E/T":
                        if (float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out float temp))
                            data.Temperature = temp;
                        else
                            Debug.WriteLine($"Неверный формат температуры: {payload}");
                        break;

                    case "Hrodno/ESP-D6-357E/H":
                        if (float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out float humidity))
                            data.Humidity = humidity;
                        break;

                    case "Hrodno/ESP-D6-357E/P":
                        if (float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out float pressure))
                            data.Pressure = pressure;
                        break;
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
