using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Domain.Models
{
    public partial class SensorData : ObservableObject
    {
        [ObservableProperty]
        private DateTime _dateTime;

        [ObservableProperty]
        private float _temperature;

        [ObservableProperty]
        private float _pressure;

        [ObservableProperty]
        private float _humidity;
    }
}