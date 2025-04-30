using SmartThings.Views;

namespace SmartThings
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SensorPage), typeof(SensorPage));
        }
    }
}
