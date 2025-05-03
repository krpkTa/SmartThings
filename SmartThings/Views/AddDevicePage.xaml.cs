using SmartThings.ViewModels;

namespace SmartThings.Views;

public partial class AddDevicePage : ContentPage
{
	public AddDevicePage(AddDeviceViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}