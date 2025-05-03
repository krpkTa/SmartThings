using ApplicationLayer.Service;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using MQTTnet;
using SmartThings.Application.Services;
using SmartThings.Presentation.ViewModels;
using SmartThings.ViewModels;
using SmartThings.Views;
using System.Diagnostics;

namespace SmartThings
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Правильная регистрация MQTT сервисов
            builder.Services.AddSingleton<IMqttClient>(serviceProvider =>
            {
                var factory = new MqttClientFactory();
                return factory.CreateMqttClient();
            });

            builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
            builder.Services.AddInfrastructure();

            builder.Services.AddScoped<IDeviceRepository, DeviceRepository>()
                .AddScoped<IDeviceService, DeviceService>();

            // Регистрация ViewModels и Pages
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<SensorViewModel>();
            builder.Services.AddTransient<AddDeviceViewModel>();
            builder.Services.AddTransient<AllDevicesViewModel>();
            builder.Services.AddTransient<AllDevices>();
            builder.Services.AddTransient<SensorPage>();
            builder.Services.AddTransient<AddDevicePage>();

            return builder.Build();
        }
    }
}