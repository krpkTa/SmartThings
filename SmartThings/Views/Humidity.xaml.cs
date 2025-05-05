using SmartThings.ViewModels;

namespace SmartThings.Views;

public partial class Humidity : ContentPage
{
	public Humidity(SensorViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;

	}
}