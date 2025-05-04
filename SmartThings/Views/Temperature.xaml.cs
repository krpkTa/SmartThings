using SmartThings.ViewModels;

namespace SmartThings.Views;

public partial class Temperature : ContentPage
{
	public Temperature(SensorViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}