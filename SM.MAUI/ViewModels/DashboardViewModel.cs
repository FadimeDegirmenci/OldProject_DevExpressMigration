using SM.MAUI.Services;
using SM.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        #region Properties

        private int _totalProducts;
        public int TotalProducts
        {
            get => _totalProducts;
            set => SetProperty(ref _totalProducts, value);
        }

        private int _totalWarehouses;
        public int TotalWarehouses
        {
            get => _totalWarehouses;
            set => SetProperty(ref _totalWarehouses, value);
        }

        private int _totalStockQuantity;
        public int TotalStockQuantity
        {
            get => _totalStockQuantity;
            set => SetProperty(ref _totalStockQuantity, value);
        }

        private decimal _totalStockValue;
        public decimal TotalStockValue
        {
            get => _totalStockValue;
            set => SetProperty(ref _totalStockValue, value);
        }

        private ObservableCollection<TopProductDto> _topProducts = new();
        public ObservableCollection<TopProductDto> TopProducts
        {
            get => _topProducts;
            set => SetProperty(ref _topProducts, value);
        }

        private ObservableCollection<WarehouseStockDto> _warehouseStocks = new();
        public ObservableCollection<WarehouseStockDto> WarehouseStocks
        {
            get => _warehouseStocks;
            set => SetProperty(ref _warehouseStocks, value);
        }

        // ✅ Gerçek az stoklu ürünler için
        private ObservableCollection<LowStockProductDto> _lowStockProducts = new();
        public ObservableCollection<LowStockProductDto> LowStockProducts
        {
            get => _lowStockProducts;
            set => SetProperty(ref _lowStockProducts, value);
        }

        private DateTime _lastUpdated;
        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        private string _lastUpdatedText = string.Empty;
        public string LastUpdatedText
        {
            get => _lastUpdatedText;
            set => SetProperty(ref _lastUpdatedText, value);
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand GoToProductsCommand { get; }
        public ICommand GoToWarehousesCommand { get; }

        #endregion

        public DashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Dashboard - Genel Bakış";

            RefreshCommand = new Command(async () => await LoadDashboardData());
            GoToProductsCommand = new Command(async () => await GoToProducts());
            GoToWarehousesCommand = new Command(async () => await GoToWarehouses());
        }

        public async Task LoadDashboardData()
        {
            try
            {
                SetBusy(true);
                ClearError();

                var dashboardData = await _apiService.GetDashboardSummaryAsync();

                if (dashboardData != null)
                {
                    TotalProducts = dashboardData.TotalProducts;
                    TotalWarehouses = dashboardData.TotalWarehouses;
                    TotalStockQuantity = dashboardData.TotalStockQuantity;
                    TotalStockValue = dashboardData.TotalStockValue;

                    // Top Products
                    TopProducts.Clear();
                    foreach (var product in dashboardData.TopProducts)
                    {
                        TopProducts.Add(product);
                    }

                    // Warehouse Stocks
                    WarehouseStocks.Clear();
                    foreach (var warehouse in dashboardData.WarehouseStocks)
                    {
                        WarehouseStocks.Add(warehouse);
                    }

                    // ✅ Gerçek az stoklu ürünleri yükle
                    await LoadLowStockProducts();

                    LastUpdated = DateTime.Now;
                    UpdateLastUpdatedText();
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Dashboard verileri yüklenirken hata: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        // ✅ Gerçek az stoklu ürünleri API'den yükle
        private async Task LoadLowStockProducts()
        {
            try
            {
                LowStockProducts.Clear();

                // Tüm envanter verilerini çek
                var allInventory = await _apiService.GetAllInventoryAsync();

                // Ürün bazında toplam stokları hesapla
                var productStocks = allInventory
                    .GroupBy(inv => new { inv.ProductId, inv.Product?.Name, inv.Product?.SKU })
                    .Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name ?? "Bilinmeyen Ürün",
                        SKU = g.Key.SKU ?? "",
                        TotalQuantity = g.Sum(inv => inv.Quantity),
                        Products = g.ToList()
                    })
                    .Where(p => p.TotalQuantity > 0) // Sadece stokta olan ürünler
                    .OrderBy(p => p.TotalQuantity) // En az stoklu önce gelsin
                    .Take(5) // İlk 5 tanesini al
                    .ToList();

                foreach (var productStock in productStocks)
                {
                    // Kritik stok seviyesi belirleme (örnek: 20'nin altındakiler kritik)
                    var isInCriticalStock = productStock.TotalQuantity <= 20;
                    var warningLevel = productStock.TotalQuantity switch
                    {
                        <= 5 => "🔴 Çok Kritik",
                        <= 10 => "🟡 Kritik",
                        <= 20 => "🟠 Düşük",
                        _ => "🟢 Normal"
                    };

                    LowStockProducts.Add(new LowStockProductDto
                    {
                        ProductName = productStock.ProductName,
                        SKU = productStock.SKU,
                        CurrentStock = productStock.TotalQuantity,
                        WarningLevel = warningLevel,
                        IsInCriticalStock = isInCriticalStock
                    });
                }

                // Eğer hiç ürün yoksa demo veri göster
                if (!LowStockProducts.Any())
                {
                    LowStockProducts.Add(new LowStockProductDto
                    {
                        ProductName = "Demo Ürün 1",
                        SKU = "DEMO001",
                        CurrentStock = 3,
                        WarningLevel = "🔴 Çok Kritik",
                        IsInCriticalStock = true
                    });
                    LowStockProducts.Add(new LowStockProductDto
                    {
                        ProductName = "Demo Ürün 2",
                        SKU = "DEMO002",
                        CurrentStock = 8,
                        WarningLevel = "🟡 Kritik",
                        IsInCriticalStock = true
                    });
                    LowStockProducts.Add(new LowStockProductDto
                    {
                        ProductName = "Demo Ürün 3",
                        SKU = "DEMO003",
                        CurrentStock = 15,
                        WarningLevel = "🟠 Düşük",
                        IsInCriticalStock = false
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LowStockProducts yüklenirken hata: {ex.Message}");

                // Hata durumunda da demo veri göster
                if (!LowStockProducts.Any())
                {
                    LowStockProducts.Add(new LowStockProductDto
                    {
                        ProductName = "Veri yüklenemedi",
                        SKU = "ERROR",
                        CurrentStock = 0,
                        WarningLevel = "❌ Hata",
                        IsInCriticalStock = true
                    });
                }
            }
        }

        private void UpdateLastUpdatedText()
        {
            var timeSpan = DateTime.Now - LastUpdated;

            if (timeSpan.TotalMinutes < 1)
            {
                LastUpdatedText = "Az önce güncellendi";
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                LastUpdatedText = $"{(int)timeSpan.TotalMinutes} dakika önce";
            }
            else if (timeSpan.TotalHours < 24)
            {
                LastUpdatedText = $"{(int)timeSpan.TotalHours} saat önce";
            }
            else
            {
                LastUpdatedText = LastUpdated.ToString("dd.MM.yyyy HH:mm");
            }
        }

        private async Task GoToProducts()
        {
            await Shell.Current.GoToAsync("ProductListPage");
        }

        private async Task GoToWarehouses()
        {
            await Shell.Current.GoToAsync("WarehouseListPage");
        }

        public async Task OnAppearing()
        {
            await LoadDashboardData();

            // Timer başlat - her dakika güncelle
            Application.Current?.Dispatcher.StartTimer(TimeSpan.FromMinutes(1), () =>
            {
                UpdateLastUpdatedText();
                return true;
            });
        }
    }

    // ✅ Az stoklu ürünler için özel DTO
    public class LowStockProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public string WarningLevel { get; set; } = string.Empty;
        public bool IsInCriticalStock { get; set; }
    }
}