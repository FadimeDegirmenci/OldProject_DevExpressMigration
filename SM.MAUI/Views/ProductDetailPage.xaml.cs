using SM.MAUI.ViewModels;

namespace SM.MAUI.Views;

public partial class ProductDetailPage : ContentPage
{
    private readonly ProductDetailViewModel _viewModel;

    public ProductDetailPage(ProductDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}