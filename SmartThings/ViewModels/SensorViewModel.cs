using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Interfaces;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartThings.ViewModels
{
    public partial class SensorViewModel : ObservableObject
    {
        [ObservableProperty]
        private SensorData _currentData = new();

        public SensorViewModel(IMqttClientService mqttClientService) => mqttClientService.SensorDataReceived += OnSensorDataReceived;

        private void OnSensorDataReceived(object sender, SensorData newData)
        {
            Debug.WriteLine($"🔥 [ViewModel] Получены данные: T={newData.Temperature}, H={newData.Humidity}, P={newData.Pressure}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                CurrentData = new SensorData
                {
                    Temperature = newData.Temperature,
                    Humidity = newData.Humidity,
                    Pressure = newData.Pressure,
                    DateTime = DateTime.Now
                };
                // Обновляем CurrentData НАПРЯМУЮ (без создания нового объекта)
                if (newData.Temperature.HasValue) CurrentData.Temperature = newData.Temperature;
                if (newData.Humidity.HasValue) CurrentData.Humidity = newData.Humidity;
                if (newData.Pressure.HasValue) CurrentData.Pressure = newData.Pressure;

                CurrentData.Temperature = newData?.Temperature;
                CurrentData.DateTime = DateTime.Now; // Обновляем время

                Debug.WriteLine($"Данные: T={CurrentData.Temperature}, H={CurrentData.Humidity}, P={CurrentData.Pressure}");

                // Форсируем обновление UI (если нужно)
                OnPropertyChanged(nameof(CurrentData));
            });
        }
    }
}
