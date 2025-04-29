using MQTTnet;
using System.Text;

namespace Infrastructure
{
    public class MqttClient
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttClientOptions;
        private readonly int _port = 1883;
        
        public MqttClient(string serverIp)
        {
            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(serverIp, _port)
                .WithClientId($"SmartThingsApp{Guid.NewGuid().ToString()[..5]}")
                .Build();

            _mqttClient.ConnectedAsync += OnConnected;
            _mqttClient.DisconnectedAsync += OnDisconnected;
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageRecieved;

            
        }

        public async Task Connect()
        {
            await _mqttClient.ConnectAsync(_mqttClientOptions);
        }

        public async Task Disconnect()
        {
            await _mqttClient.DisconnectAsync();
        }

        public async Task Publish(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            await _mqttClient.PublishAsync(message);
        }

        public async Task Subscribe(string topic)
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
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

            return Task.CompletedTask;
        }
    }
}
