using SM.MAUI.ViewModels;

namespace SM.MAUI.Views
{
    public partial class EditProductPage : ContentPage
    {
        public EditProductPage(EditProductViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Sayfa açýldýðýnda ilk input'a focus ver
            ProductNameEntry.Focus();
        }
    }
}