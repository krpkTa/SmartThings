using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Domain.Models
{
    public partial class SensorData : ObservableObject // или INotifyPropertyChanged
{
    private float _temperature;
    private float _humidity;
    private float _pressure;
    private DateTime _dateTime;

    public float Temperature
    {
        get => _temperature;
        set => SetProperty(ref _temperature, value);
    }

    public float Humidity
    {
        get => _humidity;
        set => SetProperty(ref _humidity, value);
    }

    public float Pressure
    {
        get => _pressure;
        set => SetProperty(ref _pressure, value);
    }

    public DateTime DateTime
    {
        get => _dateTime;
        set => SetProperty(ref _dateTime, value);
    }
}
}