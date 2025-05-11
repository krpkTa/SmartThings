using SmartThings.Views;

namespace SmartThings
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AddDevicePage), typeof(AddDevicePage));
            Routing.RegisterRoute(nameof(AllDevices), typeof(AllDevices));
            Routing.RegisterRoute(nameof(Temperature), typeof(Temperature));
            Routing.RegisterRoute(nameof(Pressure), typeof(Pressure));
            Routing.RegisterRoute(nameof(Humidity), typeof(Humidity));
        }
    }
}
