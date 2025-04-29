using MQTTnet;
using System.Text;
using Domain.Interfaces;

namespace Infrastructure.Services
{
    public class MqttClientService : IMqttClientService, IDisposable
    {
        private readonly INetworkDiscoveryService _networkService;
        private  IMqttClient _mqttClient;
        private MqttClientOptions _mqttClientOptions;

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
                .WithTcpServer(localIp, 1883)
                .WithClientId($"SmartThingsApp{Guid.NewGuid().ToString()[..5]}")
                .Build();

            _mqttClient.ConnectedAsync += OnConnected;
            _mqttClient.DisconnectedAsync += OnDisconnected;
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageRecieved;


        }

        public event EventHandler<string> MessageReceived;

        public async Task ConnectAsync()
        {
            await _mqttClient.ConnectAsync(_mqttClientOptions);
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
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

            return Task.CompletedTask;
        }


    }
}
