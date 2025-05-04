using ApplicationLayer.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace SmartThings.ViewModels
{
    public partial class AddDeviceViewModel : ObservableObject
    {
        private readonly IDeviceService _deviceService;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _uid = string.Empty;

        [ObservableProperty]
        private string _deviceTopics;



        public AddDeviceViewModel(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [RelayCommand]
        private async Task AddNewDevice()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Uid))
                {
                    await Shell.Current.DisplayAlert("Error", "All fields are required", "OK");
                    return;
                }
                List<string> deviceTopics = string.IsNullOrWhiteSpace(DeviceTopics)
                    ? new List<string>()
                    : DeviceTopics.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();
                await _deviceService.AddDeviceAsync(Name, Uid, deviceTopics);
                await Shell.Current.DisplayAlert("Success", "Device added", "OK");
                await Shell.Current.GoToAsync("//MainPage"); // Возврат на предыдущую страницу
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding device: {ex}");
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        
    }
}
