using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using MQTTnet;
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

            // Регистрация ViewModels и Pages
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddSingleton<SensorViewModel>();
            builder.Services.AddTransient<SensorPage>();

            return builder.Build();
        }
    }
}