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
        void OnSearchTextChanged(object sender, EventArgs e)
        {
            var textEdit = (DevExpress.Maui.Editors.TextEdit)sender;
            var text = textEdit.Text?.Trim();

            if (string.IsNullOrEmpty(text))
            {
                productGrid.FilterString = string.Empty;
                return;
            }

            var escaped = text.Replace("'", "''");

            productGrid.FilterString =
                $"Contains([Name], '{escaped}') Or Contains([SKU], '{escaped}')";
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