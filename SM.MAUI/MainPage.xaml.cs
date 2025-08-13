using SM.MAUI.Services;
using SM.MAUI.Views;

namespace SM.MAUI;

public partial class MainPage : ContentPage
{
    private int clickCount = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnTestButtonClicked(object sender, EventArgs e)
    {
        clickCount++;
        await DisplayAlert("Test", $"MAUI Android'de çalışıyor!\nTık sayısı: {clickCount}", "Harika!");
    }

    // ESKİ API TEST METODUNU KALDIR/COMMENT OUT ET:
    /*
    private async void OnApiTestTapped(object sender, EventArgs e)
    {
        try
        {
            var apiService = new ApiService();
            await DisplayAlert("Test Başlıyor", "API bağlantısı test ediliyor...", "Tamam");
            
            var isConnected = await apiService.TestConnectionAsync();
            var baseUrl = apiService.GetBaseUrl();
            
            if (isConnected)
            {
                await DisplayAlert("✅ Başarılı!", $"API bağlantısı çalışıyor!\n\nURL: {baseUrl}", "Harika!");
            }
            else
            {
                await DisplayAlert("❌ Hata", $"API'ye bağlanılamadı!\n\nURL: {baseUrl}\n\nAPI çalışıyor mu kontrol edin.", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Hata", $"Bağlantı hatası:\n{ex.Message}", "Tamam");
        }
    }
    */

    private async void OnProductManagementTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("ProductListPage");
    }

    private async void OnStockOperationTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("StockOperationPage");
    }

    private async void OnDashboardTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("DashboardPage");
    }

    // YENİ EKLEME - Depo Yönetimi metodu (eski API Test yerine)
    private async void OnWarehouseManagementTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("WarehouseListPage");
    }
}