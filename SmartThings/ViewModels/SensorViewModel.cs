using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Interfaces;
using Domain.Models;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

public partial class SensorViewModel : ObservableObject
{
    [ObservableProperty]
    private SensorData _currentData = new();

    [ObservableProperty]
    private ObservableCollection<ChartEntry> _humidityEntries = new();

    [ObservableProperty]
    private bool _isLoading;

    private const int MaxDataPoints = 100;
    private readonly IMqttClientService _mqttService;
    private string _currentDeviceId = "ESP-D6-357E";

    public LineChart HumidityChart { get; private set; }

    public SensorViewModel(IMqttClientService mqttService)
    {
        _mqttService = mqttService;
        InitializeChart();
        _mqttService.SensorDataReceived += OnSensorDataReceived;
    }


    private void InitializeChart()
    {
        HumidityChart = new LineChart
        {
            Entries = HumidityEntries,
            LineMode = LineMode.Straight,
            PointMode = PointMode.Circle,
            LabelTextSize = 24,
            LineSize = 4,
            PointSize = 8,
            BackgroundColor = SKColors.Transparent,
            AnimationDuration = TimeSpan.FromMilliseconds(500),
            IsAnimated = true
        };
    }

    private void OnSensorDataReceived(object sender, SensorData newData)
    {
        if (newData.DeviceUID != _currentDeviceId) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentData = new SensorData
            {
                DeviceUID = newData.DeviceUID,
                Temperature = newData.Temperature,
                Humidity = newData.Humidity,
                Pressure = newData.Pressure,
                DateTime = DateTime.Now
            };

            if (newData.Humidity.HasValue)
            {
                AddDataPoint(newData.Humidity.Value, CurrentData.DateTime);
            }
        });
    }

    private void AddDataPoint(float value, DateTime timestamp)
    {
        HumidityEntries.Add(new ChartEntry(value)
        {
            Color = SKColor.Parse("#2980b9"),
            Label = timestamp.ToString("HH:mm"),
            ValueLabel = value.ToString("F1")
        });

        if (HumidityEntries.Count > MaxDataPoints)
            HumidityEntries.RemoveAt(0);

        UpdateChart();
    }

    private void UpdateChart()
    {
        HumidityChart.Entries = new List<ChartEntry>(HumidityEntries);
        OnPropertyChanged(nameof(HumidityChart));
    }

    [RelayCommand]
    public async Task LoadHistory(string deviceId)
    {
        _currentDeviceId = deviceId;
        IsLoading = true;

        try
        {
            Debug.WriteLine($"Starting history load for {deviceId}");

            var historyData = await _mqttService.GetDeviceHistoryAsync(
                deviceId,
                TimeSpan.FromSeconds(10)); // Увеличили таймаут до 10 секунд

            if (historyData?.History == null)
            {
                Debug.WriteLine("Received empty history data");
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                HumidityEntries.Clear();

                foreach (var entry in historyData.History.OrderBy(h => h.Timestamp))
                {
                    if (entry.Humidity.HasValue)
                    {
                        HumidityEntries.Add(new ChartEntry((float)entry.Humidity.Value)
                        {
                            Color = SKColor.Parse("#2980b9"),
                            Label = entry.Timestamp.ToString("HH:mm"),
                            ValueLabel = entry.Humidity.Value.ToString("F1")
                        });
                    }
                }

                UpdateChart();

                Debug.WriteLine($"Loaded {HumidityEntries.Count} history points");
            });
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("History loading timed out");
            await Application.Current.MainPage.DisplayAlert("Timeout",
                "Server didn't respond in time", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"History load error: {ex.GetType().Name}: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error",
                $"Failed to load history: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}