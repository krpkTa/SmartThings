using SmartThings.Views;

namespace SmartThings
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SensorPage), typeof(SensorPage));
            Routing.RegisterRoute(nameof(AddDevicePage), typeof(AddDevicePage));
            Routing.RegisterRoute(nameof(AllDevices), typeof(AllDevices));
        }
    }
}
