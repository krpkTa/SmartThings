using SmartThings.Presentation.ViewModels;
using SmartThings.ViewModels;

namespace SmartThings.Views;

public partial class AllDevices : ContentPage
{
	public AllDevices(AllDevicesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}