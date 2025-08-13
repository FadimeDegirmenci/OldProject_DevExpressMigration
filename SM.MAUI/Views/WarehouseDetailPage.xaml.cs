using SM.MAUI.ViewModels;

namespace SM.MAUI.Views
{
    public partial class WarehouseDetailPage : ContentPage
    {
        private WarehouseDetailViewModel _viewModel;

        public WarehouseDetailPage(WarehouseDetailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // ViewModel'in LoadWarehouseDetail metodunu direkt çaðýr
            if (_viewModel != null)
            {
                await _viewModel.LoadWarehouseDetail();
            }
        }
    }
}