using SM.Core.Models;
using SM.MAUI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    [QueryProperty(nameof(ProductId), "ProductId")]
    [QueryProperty(nameof(ProductName), "ProductName")]
    public class ProductDetailViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        private int _productId;
        public int ProductId
        {
            get => _productId;
            set
            {
                SetProperty(ref _productId, value);
                _ = LoadProductDetail();
            }
        }

        private string _productName = string.Empty;
        public string ProductName
        {
            get => _productName;
            set => SetProperty(ref _productName, value);
        }

        private string _productSKU = string.Empty;
        public string ProductSKU
        {
            get => _productSKU;
            set => SetProperty(ref _productSKU, value);
        }

        private decimal _productPrice;
        public decimal ProductPrice
        {
            get => _productPrice;
            set => SetProperty(ref _productPrice, value);
        }

        private string _productDescription = string.Empty;
        public string ProductDescription
        {
            get => _productDescription;
            set => SetProperty(ref _productDescription, value);
        }

        private int _totalStock;
        public int TotalStock
        {
            get => _totalStock;
            set => SetProperty(ref _totalStock, value);
        }

        private ObservableCollection<WarehouseStockItem> _warehouseStocks = new();
        public ObservableCollection<WarehouseStockItem> WarehouseStocks
        {
            get => _warehouseStocks;
            set => SetProperty(ref _warehouseStocks, value);
        }

        // Commands
        public ICommand GoToStockOperationsCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public ProductDetailViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Urun Detayi";

            GoToStockOperationsCommand = new Command(async () => await GoToStockOperations());
            EditProductCommand = new Command(async () => await EditProduct());
            RefreshCommand = new Command(async () => await LoadProductDetail());
            DeleteProductCommand = new Command(async () => await DeleteProduct());
        }

        private async Task LoadProductDetail()
        {
            if (ProductId <= 0) return;

            try
            {
                SetBusy(true);
                ClearError();

                // Urun bilgilerini al
                var product = await _apiService.GetProductByIdAsync(ProductId);
                if (product != null)
                {
                    ProductName = product.Name;
                    ProductSKU = product.SKU;
                    ProductPrice = product.Price;
                    ProductDescription = product.Description ?? "Aciklama yok";
                }

                // Urun envanterini al
                var inventory = await _apiService.GetProductInventoryAsync(ProductId);

                WarehouseStocks.Clear();
                TotalStock = 0;

                if (inventory.Any())
                {
                    foreach (var item in inventory)
                    {
                        WarehouseStocks.Add(new WarehouseStockItem
                        {
                            WarehouseName = item.Warehouse?.Name ?? "Bilinmeyen Depo",
                            Location = item.Warehouse?.Location ?? "Bilinmeyen Lokasyon",
                            Quantity = item.Quantity
                        });

                        TotalStock += item.Quantity;
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Urun detaylari yuklenirken hata: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task GoToStockOperations()
        {
            await Shell.Current.GoToAsync("StockOperationPage");
        }

        private async Task EditProduct()
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    ["ProductId"] = ProductId,
                    ["ProductName"] = ProductName,
                    ["ProductSKU"] = ProductSKU,
                    ["ProductPrice"] = ProductPrice,
                    ["ProductDescription"] = ProductDescription
                };

                await Shell.Current.GoToAsync("EditProductPage", parameters);
            }
            catch (Exception ex)
            {
                await ShowError($"Düzenleme sayfasina giderken hata: {ex.Message}");
            }
        }

        private async Task DeleteProduct()
        {
            try
            {
                // Onay al
                bool shouldDelete = await Shell.Current.DisplayAlert(
                    "Ürün Sil",
                    $"'{ProductName}' ürününü silmek istediğinizden emin misiniz?\n\nBu işlem geri alınamaz!",
                    "Evet, Sil",
                    "İptal");

                if (!shouldDelete) return;

                // İkinci onay (önemli işlem olduğu için)
                bool finalConfirm = await Shell.Current.DisplayAlert(
                    "Son Onay",
                    $"'{ProductName}' ürünü kalıcı olarak silinecek.\n\nDevam etmek istiyor musunuz?",
                    "Evet, Kesinlikle Sil",
                    "İptal");

                if (!finalConfirm) return;

                SetBusy(true);
                ClearError();

                bool deleted = await _apiService.DeleteProductAsync(ProductId);

                if (deleted)
                {
                    await Shell.Current.DisplayAlert("Başarılı", $"'{ProductName}' ürünü başarıyla silindi!", "Tamam");

                    // Ürün listesine geri dön
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Ürün silinirken hata oluştu: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }
    }

    // Depo stok item modeli
    public class WarehouseStockItem
    {
        public string WarehouseName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}