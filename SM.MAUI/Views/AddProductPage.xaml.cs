using SM.MAUI.ViewModels;

namespace SM.MAUI.Views;

public partial class AddProductPage : ContentPage
{
    private readonly AddProductViewModel _viewModel;

    public AddProductPage(AddProductViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}