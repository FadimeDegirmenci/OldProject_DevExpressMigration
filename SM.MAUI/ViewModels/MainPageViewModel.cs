using SM.MAUI.Views;
using SM.MAUI.Services;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public ICommand NavigateToProductsCommand { get; }
        public ICommand NavigateToStockOperationsCommand { get; }
        public ICommand NavigateToWarehousesCommand { get; }
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand TestConnectionCommand { get; }

        private string _connectionStatus = "Bağlantı durumu: Test edilmedi";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        public MainPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Ana Sayfa";

            NavigateToProductsCommand = new Command(async () => await NavigateToProducts());
            NavigateToStockOperationsCommand = new Command(async () => await NavigateToStockOperations());
            NavigateToWarehousesCommand = new Command(async () => await NavigateToWarehouses());
            NavigateToDashboardCommand = new Command(async () => await NavigateToDashboard());
            TestConnectionCommand = new Command(async () => await TestConnection());
        }

        private async Task TestConnection()
        {
            try
            {
                SetBusy(true);
                ConnectionStatus = "Bağlantı test ediliyor...";

                var baseUrl = _apiService.GetBaseUrl();
                var isConnected = await _apiService.TestConnectionAsync();

                if (isConnected)
                {
                    ConnectionStatus = $"✅ API bağlantısı başarılı: {baseUrl}";
                    await ShowSuccess($"API bağlantısı başarılı!\nURL: {baseUrl}");
                }
                else
                {
                    ConnectionStatus = $"❌ API bağlantısı başarısız: {baseUrl}";
                    await ShowError($"API'ye bağlanılamadı!\nURL: {baseUrl}\n\nAPI çalışıyor mu kontrol edin.");
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"❌ Hata: {ex.Message}";
                await ShowError($"Bağlantı testi sırasında hata: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task NavigateToProducts()
        {
            await Shell.Current.GoToAsync(nameof(ProductListPage));
        }

        private async Task NavigateToStockOperations()
        {
            await Shell.Current.GoToAsync(nameof(StockOperationPage));
        }

        private async Task NavigateToWarehouses()
        {
            await Shell.Current.GoToAsync(nameof(WarehouseListPage));
        }

        private async Task NavigateToDashboard()
        {
            await Shell.Current.GoToAsync(nameof(DashboardPage));
        }
    }
}