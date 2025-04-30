namespace SmartThings
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;  // Явная установка ViewModel
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            
        }
    }

}
