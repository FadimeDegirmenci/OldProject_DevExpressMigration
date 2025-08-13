using SM.MAUI.ViewModels;

namespace SM.MAUI.Views
{
    public partial class AddWarehousePage : ContentPage
    {
        public AddWarehousePage(AddWarehouseViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Sayfa açýldýðýnda ilk input'a focus ver
            WarehouseNameEntry.Focus();
        }
    }
}