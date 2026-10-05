using SM.MAUI.ViewModels;

namespace SM.MAUI.Views
{
    public partial class ProductLoadMorePage : ContentPage
    {
        private readonly ProductLoadMoreViewModel _viewModel;

        public ProductLoadMorePage(ProductLoadMoreViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadFirstPageAsync();
        }
    }
}