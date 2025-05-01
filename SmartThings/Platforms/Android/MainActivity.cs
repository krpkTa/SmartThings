using Android.App;
using Android.Content.PM;
using Android.OS;
using Domain.Interfaces;

namespace SmartThings
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnDestroy()
        {
            var mqttService = MauiApplication.Current.Services.GetService<IMqttClientService>();
            mqttService?.DisconnectAsync().Wait();
            base.OnDestroy();
        }
    }

}
