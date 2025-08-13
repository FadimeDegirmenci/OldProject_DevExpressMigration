using ZXing.Net.Maui;
namespace SM.MAUI.Views;

public partial class BarcodeScanPage : ContentPage
{
    private bool _isProcessing = false;

    public BarcodeScanPage()
    {
        InitializeComponent();
    }

    private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing) return;

        var firstBarcode = e.Results?.FirstOrDefault();
        if (firstBarcode != null)
        {
            _isProcessing = true;

            // Ana UI thread'de çalýþtýr
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Kamera durdur
                CameraView.IsDetecting = false;

                // Vibrasyon
                try
                {
                    Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
                }
                catch { }

                // DÜZELTME: Parameter anahtarýný QueryProperty ile eþleþtir
                var parameters = new Dictionary<string, object>
                {
                    { "ScannedBarcode", firstBarcode.Value } // "scannedcode" yerine "ScannedBarcode"
                };

                await Shell.Current.GoToAsync("..", parameters);
            });
        }
    }

    private void OnFlashToggleClicked(object sender, EventArgs e)
    {
        CameraView.IsTorchOn = !CameraView.IsTorchOn;
    }

    private void OnRetryClicked(object sender, EventArgs e)
    {
        _isProcessing = false;
        CameraView.IsDetecting = true;
    }

    private async void OnAcceptClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CameraView.IsDetecting = true;
        _isProcessing = false;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CameraView.IsDetecting = false;
    }
}