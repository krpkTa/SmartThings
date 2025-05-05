using ApplicationLayer.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Services;
using SmartThings.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SmartThings.Presentation.ViewModels
{
    public partial class AllDevicesViewModel : ObservableObject
    {
        private readonly IDeviceService _deviceService;
        private readonly IMqttClientService _mqttService;

        [ObservableProperty]
        private ObservableCollection<SmartDevice> _devices = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRefreshing;

        public AllDevicesViewModel(IDeviceService deviceService, IMqttClientService mqttService)
        {
            _deviceService = deviceService;
            _mqttService = mqttService;
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
                        var tempList = Devices.ToList();
                        tempList.Remove(device);
                        Devices = new ObservableCollection<SmartDevice>(tempList);
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

        [RelayCommand]
        private async Task TurnOn(string deviceUid)
        {
            try
            {
                var device = Devices.FirstOrDefault(d => d.UID == deviceUid);
                if (device != null)
                {
                    var topic = $"{device.UID}/LED"; // или другой топик из device.Topics
                    await _mqttService.PublishAsync(topic, "1");
                    Debug.WriteLine($"Устройство {device.Name} включено");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка включения: {ex}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось включить устройство", "OK");
            }
        }

        [RelayCommand]
        private async Task TurnOff(string deviceUid)
        {
            try
            {
                var device = Devices.FirstOrDefault(d => d.UID == deviceUid);
                if (device != null)
                {
                    var topic = $"{device.UID}/LED"; // или другой топик из device.Topics
                    await _mqttService.PublishAsync(topic, "0");
                    Debug.WriteLine($"Устройство {device.Name} выключено");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка выключения: {ex}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось выключить устройство", "OK");
            }
        }
    }
}