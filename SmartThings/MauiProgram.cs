using Domain.Interfaces;
using Infrastructure;
using Microsoft.Extensions.Logging;
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

            // Register services
            builder.Services.AddInfrastructure();
            builder.Services.AddTransient<MainViewModel>();

            // Build the app
            var app = builder.Build();

            // Initialize MQTT service
            var mqttService = app.Services.GetRequiredService<IMqttClientService>();
            Task.Run(async () =>
            {
                try
                {
                    await mqttService.InitializeAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MQTT initialization failed: {ex.Message}");
                }
            });

            return app;
        }
    }
}