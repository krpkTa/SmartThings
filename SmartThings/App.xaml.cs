using Domain.Interfaces;
using MQTTnet;
using System.Diagnostics;

namespace SmartThings
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        private readonly IMqttClientService _mqttService;

        public App(IMqttClientService mqttService)
        {
            _mqttService = mqttService;
            InitializeComponent();

            MainPage = new AppShell();

            Task.Run(async () =>
            {
                try
                {
                    await _mqttService.InitializeAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MQTT Init Error: {ex.Message}");
                }
            });
            // Assuming you're using Shell navigation

            // Handle theme changes
            Current!.RequestedThemeChanged += (_, _) => { /* ... */ };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            // Handle window closing (MAUI's equivalent of "Quitting")
            window.Destroying += async (_, _) =>
            {
                await _mqttService.DisconnectAsync();
            };

            return window;
        }
    }
}