using SmartThings.ViewModels;
using System.Diagnostics;

namespace SmartThings.Views;

public partial class SensorPage : ContentPage
{
    
	public SensorPage(SensorViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        
    }
}