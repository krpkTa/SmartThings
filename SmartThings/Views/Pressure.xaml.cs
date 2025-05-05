using SmartThings.ViewModels;

namespace SmartThings.Views;

public partial class Pressure : ContentPage
{
	public Pressure(SensorViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}