using DevExpress.Maui.DataGrid;
using SM.Core.Models;
using SM.MAUI.ViewModels;

namespace SM.MAUI.Views
{
    public partial class ProductListPage : ContentPage
    {
        private readonly ProductListViewModel _viewModel;

        public ProductListPage(ProductListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearing();   // ← eksik olan satır, ürünleri burada yüklüyor
        }

        void OnProductTapConfirmed(object sender, DataGridGestureEventArgs e)
        {
            if (e.Item is Product product)
                _viewModel.ProductTappedCommand.Execute(product);
        }

        async void OnBackToMainClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}