using SM.MAUI.ViewModels;
using SM.Core.Models;

namespace SM.MAUI.Views;

public partial class ProductListPage : ContentPage
{
    private readonly ProductListViewModel _viewModel;

    public ProductListPage(ProductListViewModel viewModel)
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

    private async void OnProductTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is Product product)
        {
            var parameters = new Dictionary<string, object>
            {
                { "ProductId", product.Id },
                { "ProductName", product.Name }
            };

            await Shell.Current.GoToAsync("ProductDetailPage", parameters);
        }
    }
}