using SM.MAUI.ViewModels;

namespace SM.MAUI.Views
{
    public partial class DashboardPage : ContentPage
    {
        private DashboardViewModel _viewModel;

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearing();
        }
    }
}