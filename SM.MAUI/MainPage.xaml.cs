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
    private async void OnDevExpressDesignTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("DevExpressPage");
    }
}