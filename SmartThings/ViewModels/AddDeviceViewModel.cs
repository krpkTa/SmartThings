using ApplicationLayer.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
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
        private string _topic = string.Empty;

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

                await _deviceService.AddDeviceAsync(Name, Uid, $"home/{Uid}/data");
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
