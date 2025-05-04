using ApplicationLayer.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Models;
using SmartThings.Views;
using System.Collections.ObjectModel;

namespace SmartThings.Presentation.ViewModels;

public partial class AllDevicesViewModel : ObservableObject
{
    private readonly IDeviceService _deviceService;

    [ObservableProperty]
    private ObservableCollection<SmartDevice> _devices = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    public AllDevicesViewModel(IDeviceService deviceService)
    {
        _deviceService = deviceService;
        LoadDevicesCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDevices()
    {
        try
        {
            IsRefreshing = true;
            var devices = await _deviceService.LoadDevicesAsync();
            Devices = new ObservableCollection<SmartDevice>(devices);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteDevice(SmartDevice device)
    {
        if (device == null) return;

        try
        {
            IsBusy = true;

            bool confirm = await Shell.Current.DisplayAlert(
                "Удаление устройства",
                $"Удалить {device.Name}?",
                "Да", "Нет");

            if (confirm)
            {
                var success = await _deviceService.DeleteDeviceAsync(device.Id);

                if (success)
                {
                    // Вариант 1: Удаление с анимацией
                    var tempList = Devices.ToList();
                    tempList.Remove(device);
                    Devices = new ObservableCollection<SmartDevice>(tempList);

                    // Вариант 2: Полное обновление
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToAddDevice()
    {
        await Shell.Current.GoToAsync(nameof(AddDevicePage));
    }
}