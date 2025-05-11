using System.Diagnostics;

namespace SmartThings.Views
{
    public partial class Humidity : ContentPage
    {
        private readonly SensorViewModel _viewModel;

        public Humidity(SensorViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // ���������, ��� ��������� ���������� deviceId
            await _viewModel.LoadHistory("ESP-D6-357E"); // �������� �� �������� ID
        }
    }
}