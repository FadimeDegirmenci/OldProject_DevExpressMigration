using SM.MAUI.ViewModels;

namespace SM.MAUI.Views
{
    public partial class WarehouseListPage : ContentPage
    {
        private WarehouseListViewModel _viewModel;

        public WarehouseListPage(WarehouseListViewModel viewModel)
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

        private async void OnBackToMainClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}