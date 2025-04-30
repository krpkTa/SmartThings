using SmartThings.ViewModels;

namespace SmartThings.Views;

public partial class SensorPage : ContentPage
{
	public SensorPage(SensorViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}