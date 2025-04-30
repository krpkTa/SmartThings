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

        public SensorViewModel(IMqttClientService mqttClientService)
        {
            mqttClientService.SensorDataReceived += OnSensorDataReceived;
        }
        private void OnSensorDataReceived(object sender, SensorData newData)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Создаём КОПИЮ текущих данных
                var updatedData = new SensorData
                {
                    Temperature = CurrentData.Temperature,
                    Humidity = CurrentData.Humidity,
                    Pressure = CurrentData.Pressure,
                    DateTime = DateTime.Now
                };

                // Обновляем только пришедшие значения
                if (newData.Temperature != 0) updatedData.Temperature = newData.Temperature;
                if (newData.Humidity != 0) updatedData.Humidity = newData.Humidity;
                if (newData.Pressure != 0) updatedData.Pressure = newData.Pressure;

                CurrentData = updatedData; // Полное обновление с сохранением всех значений
            });
        }
    }
}
