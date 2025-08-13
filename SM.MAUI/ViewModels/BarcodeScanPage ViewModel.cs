namespace SM.MAUI.ViewModels
{
    public class BarcodeScanViewModel : BaseViewModel
    {
        private string _scannedBarcode = string.Empty;
        public string ScannedBarcode
        {
            get => _scannedBarcode;
            set => SetProperty(ref _scannedBarcode, value);
        }

        private bool _hasResult;
        public bool HasResult
        {
            get => _hasResult;
            set => SetProperty(ref _hasResult, value);
        }

        public BarcodeScanViewModel()
        {
            Title = "Barkod Okuyucu";
        }

        public void SetBarcodeResult(string barcode)
        {
            ScannedBarcode = barcode;
            HasResult = !string.IsNullOrEmpty(barcode);
        }

        public void ClearResult()
        {
            ScannedBarcode = string.Empty;
            HasResult = false;
        }
    }
}