using ApplicationLayer.Service;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SmartThings
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        private readonly IDeviceService _deviceService;
        private readonly IMqttClientService _mqttClientService;
        private readonly IDeviceRepository _deviceRepository;

        public App(IDeviceService deviceService,
            IMqttClientService mqttClientService,
            IDeviceRepository deviceRepository)
        {
            InitializeComponent();

            _deviceService = deviceService;
            _mqttClientService = mqttClientService;
            _deviceRepository = deviceRepository;

            MainPage = new AppShell();

            Task.Run(InitializeAppAsync);
            // Assuming you're using Shell navigation

            // Handle theme changes
            Current!.RequestedThemeChanged += (_, _) => { /* ... */ };
            
        }
        
        private async Task InitializeAppAsync()
        {
            try
            {
                await _mqttClientService.InitializeAsync();
                Debug.WriteLine("MQTT initialized");

                // 2. Инициализация репозитория (загружает устройства)
                await _deviceRepository.InitializeAsync();

                // 3. Получаем и проверяем устройства
                var devices = await _deviceRepository.GetAllAsync();
                Debug.WriteLine($"Total devices after init: {devices.Count()}");

                // 4. Инициализация сервиса (подписки)
                await _deviceService.InitializeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Destroying += async (_, _) =>
            {
                await _mqttClientService.DisconnectAsync();
            };

            return window;
        }
    }
}