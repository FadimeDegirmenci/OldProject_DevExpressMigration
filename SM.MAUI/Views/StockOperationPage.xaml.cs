using SM.MAUI.ViewModels;

namespace SM.MAUI.Views
{
    // DÜZELTME: QueryProperty parametresi ile anahtar eþleþmeli
    [QueryProperty(nameof(ScannedBarcode), "ScannedBarcode")]
    public partial class StockOperationPage : ContentPage
    {
        private StockOperationViewModel _viewModel;

        // DÜZELTME: Property adýný deðiþtir
        public string ScannedBarcode { get; set; } = string.Empty;

        public StockOperationPage(StockOperationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearing();

            // DÜZELTME: Property adýný güncellenmiþ haliyle kullan
            if (!string.IsNullOrEmpty(ScannedBarcode))
            {
                await _viewModel.HandleBarcodeResult(ScannedBarcode);
                ScannedBarcode = string.Empty; // Temizle
            }
            else
            {
                // SKU Entry'ye focus ver
                SkuEntry.Focus();
            }
        }
    }
}