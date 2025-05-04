using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Domain.Models
{
    public partial class SensorData : ObservableObject // или INotifyPropertyChanged
{
    private float? _temperature;
    private float? _humidity;
    private float? _pressure;
    private DateTime _dateTime;
    private string _deviceUID;

    public string DeviceUID
            {
                get { return _deviceUID; }
                set { _deviceUID = value; }
            }
        public float? Temperature { get; set; }  // Добавлен знак вопроса для nullable
        public float? Humidity { get; set; }     // Добавлен знак вопроса для nullable
        public float? Pressure { get; set; }

        public DateTime DateTime
    {
        get => _dateTime;
        set => SetProperty(ref _dateTime, value);
    }
        public SensorData Clone()
        {
            return new SensorData
            {
                DeviceUID = this.DeviceUID,
                Temperature = this.Temperature,
                Humidity = this.Humidity,
                Pressure = this.Pressure,
                DateTime = this.DateTime
            };
        }
        public bool IsValid()
        {
            return Temperature.HasValue || Humidity.HasValue || Pressure.HasValue;
        }
    }
}