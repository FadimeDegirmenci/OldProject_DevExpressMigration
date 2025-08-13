using SM.MAUI.Services;
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

        // ✅ YENİ: En az stoklu ürünler için
        private ObservableCollection<TopProductDto> _lowStockProducts = new();
        public ObservableCollection<TopProductDto> LowStockProducts
        {
            get => _lowStockProducts;
            set => SetProperty(ref _lowStockProducts, value);
        }

        // ✅ YENİ: Haftalık trend için
        private ObservableCollection<TrendDataDto> _weeklyTrend = new();
        public ObservableCollection<TrendDataDto> WeeklyTrend
        {
            get => _weeklyTrend;
            set => SetProperty(ref _weeklyTrend, value);
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
            Title = "Dashboard - Genel Bakis";

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

                    // ✅ YENİ: En az stoklu ürünleri yükle (demo veri)
                    await LoadLowStockProducts();

                    // ✅ YENİ: Haftalık trend verilerini yükle (demo veri)
                    await LoadWeeklyTrend();

                    LastUpdated = DateTime.Now;
                    UpdateLastUpdatedText();
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Dashboard verileri yuklenirken hata: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        // ✅ YENİ: En az stoklu ürünleri yükle
        private async Task LoadLowStockProducts()
        {
            try
            {
                // API'den gerçek veri gelene kadar demo veri
                LowStockProducts.Clear();

                // En az stoklu ürünleri TopProducts'tan türet (tersten sıralayarak)
                var lowStockItems = TopProducts.OrderBy(p => p.TotalQuantity).Take(5);

                foreach (var item in lowStockItems)
                {
                    LowStockProducts.Add(new TopProductDto
                    {
                        ProductName = item.ProductName,
                        TotalQuantity = item.TotalQuantity,
                        TotalValue = item.TotalValue
                    });
                }

                // Eğer hiç veri yoksa demo veri ekle
                if (!LowStockProducts.Any())
                {
                    LowStockProducts.Add(new TopProductDto { ProductName = "Kırtasiye", TotalQuantity = 5, TotalValue = 150 });
                    LowStockProducts.Add(new TopProductDto { ProductName = "Aksesuar", TotalQuantity = 8, TotalValue = 320 });
                    LowStockProducts.Add(new TopProductDto { ProductName = "Elektronik", TotalQuantity = 12, TotalValue = 2400 });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LowStockProducts yüklenirken hata: {ex.Message}");
            }
        }

        // ✅ YENİ: Haftalık trend verilerini yükle
        private async Task LoadWeeklyTrend()
        {
            try
            {
                WeeklyTrend.Clear();

                // Demo haftalık trend verileri
                var today = DateTime.Today;
                for (int i = 6; i >= 0; i--)
                {
                    var date = today.AddDays(-i);
                    var random = new Random(date.Day);

                    WeeklyTrend.Add(new TrendDataDto
                    {
                        Date = date,
                        DayName = GetTurkishDayName(date.DayOfWeek),
                        StockIn = random.Next(20, 100),
                        StockOut = random.Next(15, 80),
                        TotalValue = random.Next(5000, 25000)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WeeklyTrend yüklenirken hata: {ex.Message}");
            }
        }

        // ✅ YENİ: Türkçe gün adları
        private string GetTurkishDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Pzt",
                DayOfWeek.Tuesday => "Sal",
                DayOfWeek.Wednesday => "Çar",
                DayOfWeek.Thursday => "Per",
                DayOfWeek.Friday => "Cum",
                DayOfWeek.Saturday => "Cmt",
                DayOfWeek.Sunday => "Paz",
                _ => "?"
            };
        }

        private void UpdateLastUpdatedText()
        {
            var timeSpan = DateTime.Now - LastUpdated;

            if (timeSpan.TotalMinutes < 1)
            {
                LastUpdatedText = "Az once guncellendi";
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                LastUpdatedText = $"{(int)timeSpan.TotalMinutes} dakika once";
            }
            else if (timeSpan.TotalHours < 24)
            {
                LastUpdatedText = $"{(int)timeSpan.TotalHours} saat once";
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

            // Timer başlat - her dakika güncelle (Application.Current kullan)
            Application.Current?.Dispatcher.StartTimer(TimeSpan.FromMinutes(1), () =>
            {
                UpdateLastUpdatedText();
                return true;
            });
        }
    }

    // ✅ YENİ: Trend verisi için DTO
    public class TrendDataDto
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public int StockIn { get; set; }
        public int StockOut { get; set; }
        public decimal TotalValue { get; set; }
    }
}