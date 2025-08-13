using SM.Core.Models;
using SM.MAUI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    [QueryProperty(nameof(WarehouseId), "WarehouseId")]
    [QueryProperty(nameof(WarehouseName), "WarehouseName")]
    public class WarehouseDetailViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        #region Properties

        private int _warehouseId;
        public int WarehouseId
        {
            get => _warehouseId;
            set
            {
                SetProperty(ref _warehouseId, value);
                _ = LoadWarehouseDetail();
            }
        }

        private string _warehouseName = string.Empty;
        public string WarehouseName
        {
            get => _warehouseName;
            set => SetProperty(ref _warehouseName, value);
        }

        private string _location = string.Empty;
        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        private DateTime _createdDate;
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        private int _totalProducts;
        public int TotalProducts
        {
            get => _totalProducts;
            set => SetProperty(ref _totalProducts, value);
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

        private ObservableCollection<InventoryItem> _inventoryItems = new();
        public ObservableCollection<InventoryItem> InventoryItems
        {
            get => _inventoryItems;
            set => SetProperty(ref _inventoryItems, value);
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand EditWarehouseCommand { get; }
        public ICommand DeleteWarehouseCommand { get; }
        public ICommand ViewStockOperationsCommand { get; }

        #endregion

        public WarehouseDetailViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Depo Detayı";

            RefreshCommand = new Command(async () => await LoadWarehouseDetail());
            EditWarehouseCommand = new Command(async () => await EditWarehouse());
            DeleteWarehouseCommand = new Command(async () => await DeleteWarehouse());
            ViewStockOperationsCommand = new Command(async () => await ViewStockOperations());
        }

        #region Methods

        // PUBLIC metod - Page'den çağrılabilir
        public async Task LoadWarehouseDetail()
        {
            if (WarehouseId <= 0) return;

            try
            {
                SetBusy(true);
                ClearError();

                // Depo bilgilerini al
                var warehouse = await _apiService.GetWarehouseByIdAsync(WarehouseId);
                if (warehouse != null)
                {
                    WarehouseName = warehouse.Name;
                    Location = warehouse.Location;
                    CreatedDate = warehouse.CreatedDate;
                }

                // Depo envanterini al
                var inventory = await _apiService.GetWarehouseInventoryAsync(WarehouseId);

                InventoryItems.Clear();
                TotalProducts = 0;
                TotalStockQuantity = 0;
                TotalStockValue = 0;

                if (inventory.Any())
                {
                    foreach (var item in inventory)
                    {
                        InventoryItems.Add(item);
                        TotalStockQuantity += item.Quantity;
                        TotalStockValue += item.Quantity * (item.Product?.Price ?? 0);
                    }
                    TotalProducts = inventory.Count;
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Depo detayları yüklenirken hata: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task EditWarehouse()
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    ["WarehouseId"] = WarehouseId,
                    ["WarehouseName"] = WarehouseName,
                    ["Location"] = Location
                };

                await Shell.Current.GoToAsync("EditWarehousePage", parameters);
            }
            catch (Exception ex)
            {
                await ShowError($"Düzenleme sayfasına giderken hata: {ex.Message}");
            }
        }

        private async Task DeleteWarehouse()
        {
            try
            {
                // Stok kontrolü
                if (TotalStockQuantity > 0)
                {
                    await ShowError($"Bu depoda {TotalStockQuantity} adet ürün bulunuyor!\n\nÖnce tüm stokları başka depoya taşıyın veya stok çıkışı yapın.");
                    return;
                }

                // Onay al
                bool shouldDelete = await Shell.Current.DisplayAlert(
                    "Depo Sil",
                    $"'{WarehouseName}' deposunu silmek istediğinizden emin misiniz?\n\nBu işlem geri alınamaz!",
                    "Evet, Sil",
                    "İptal");

                if (!shouldDelete) return;

                // İkinci onay
                bool finalConfirm = await Shell.Current.DisplayAlert(
                    "Son Onay",
                    $"'{WarehouseName}' deposu kalıcı olarak silinecek.\n\nDevam etmek istiyor musunuz?",
                    "Evet, Kesinlikle Sil",
                    "İptal");

                if (!finalConfirm) return;

                SetBusy(true);
                ClearError();

                bool deleted = await _apiService.DeleteWarehouseAsync(WarehouseId);

                if (deleted)
                {
                    await Shell.Current.DisplayAlert("Başarılı", $"'{WarehouseName}' deposu başarıyla silindi!", "Tamam");

                    // Depo listesine geri dön
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Depo silinirken hata oluştu: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ViewStockOperations()
        {
            await Shell.Current.GoToAsync("StockOperationPage");
        }

        #endregion
    }
}