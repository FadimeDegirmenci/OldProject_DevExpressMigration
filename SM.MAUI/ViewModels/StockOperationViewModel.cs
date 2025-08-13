using SM.Core.Models;
using SM.MAUI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    public class StockOperationViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        #region Properties

        // SKU Girişi
        private string _skuInput = string.Empty;
        public string SkuInput
        {
            get => _skuInput;
            set
            {
                SetProperty(ref _skuInput, value);
                ValidateInput();
            }
        }

        // Seçilen Ürün
        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        // Miktar
        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                SetProperty(ref _quantity, value);
                ValidateInput();
            }
        }

        // Seçilen Depo
        private Warehouse? _selectedWarehouse;
        public Warehouse? SelectedWarehouse
        {
            get => _selectedWarehouse;
            set
            {
                SetProperty(ref _selectedWarehouse, value);
                ValidateInput();
            }
        }

        // Depolar
        private ObservableCollection<Warehouse> _warehouses = new();
        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set => SetProperty(ref _warehouses, value);
        }

        // Validation
        private bool _canPerformOperation;
        public bool CanPerformOperation
        {
            get => _canPerformOperation;
            set => SetProperty(ref _canPerformOperation, value);
        }

        private string _validationMessage = string.Empty;
        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

        // Ürün Detayları Görünürlüğü
        private bool _isProductVisible;
        public bool IsProductVisible
        {
            get => _isProductVisible;
            set => SetProperty(ref _isProductVisible, value);
        }

        // Son İşlem Sonucu
        private string _lastOperationResult = string.Empty;
        public string LastOperationResult
        {
            get => _lastOperationResult;
            set => SetProperty(ref _lastOperationResult, value);
        }

        private bool _hasLastOperationResult;
        public bool HasLastOperationResult
        {
            get => _hasLastOperationResult;
            set => SetProperty(ref _hasLastOperationResult, value);
        }

        #endregion

        #region Commands

        public ICommand SearchProductCommand { get; }
        public ICommand ScanBarcodeCommand { get; }
        public ICommand StockInCommand { get; }
        public ICommand StockOutCommand { get; }
        public ICommand ClearFormCommand { get; }

        #endregion

        public StockOperationViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Stok İşlemleri";

            // Commands
            SearchProductCommand = new Command(async () => await SearchProduct());
            ScanBarcodeCommand = new Command(async () => await ScanBarcode());
            StockInCommand = new Command(async () => await PerformStockIn(), () => CanPerformOperation);
            StockOutCommand = new Command(async () => await PerformStockOut(), () => CanPerformOperation);
            ClearFormCommand = new Command(ClearForm);

            // Property Changed Event
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CanPerformOperation))
                {
                    ((Command)StockInCommand).ChangeCanExecute();
                    ((Command)StockOutCommand).ChangeCanExecute();
                }
            };
        }

        #region Methods

        public async Task LoadWarehouses()
        {
            try
            {
                SetBusy(true);
                ClearError();

                var warehouseList = await _apiService.GetWarehousesAsync();

                Warehouses.Clear();
                foreach (var warehouse in warehouseList)
                {
                    Warehouses.Add(warehouse);
                }

                // İlk depoyu seç
                if (Warehouses.Any())
                {
                    SelectedWarehouse = Warehouses.First();
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Depolar yüklenirken hata: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task SearchProduct()
        {
            if (string.IsNullOrWhiteSpace(SkuInput))
            {
                ValidationMessage = "Lütfen SKU girin";
                return;
            }

            try
            {
                SetBusy(true);
                ClearError();

                var product = await _apiService.GetProductBySkuAsync(SkuInput.Trim());

                if (product != null)
                {
                    SelectedProduct = product;
                    IsProductVisible = true;
                    ValidationMessage = "✅ Ürün bulundu!";

                    // Ürün stok bilgilerini de yükle
                    await LoadProductStock();
                }
                else
                {
                    SelectedProduct = null;
                    IsProductVisible = false;
                    ValidationMessage = "❌ Bu SKU'ya sahip ürün bulunamadı";
                }

                ValidateInput();
            }
            catch (Exception ex)
            {
                SelectedProduct = null;
                IsProductVisible = false;
                ValidationMessage = $"❌ Hata: {ex.Message}";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ScanBarcode()
        {
            try
            {
                // Barkod okuma sayfasına git
                await Shell.Current.GoToAsync("BarcodeScanPage");
            }
            catch (Exception ex)
            {
                await ShowError($"Barkod okuma hatası: {ex.Message}");
            }
        }

        private async Task LoadProductStock()
        {
            if (SelectedProduct == null || SelectedWarehouse == null) return;

            try
            {
                var inventory = await _apiService.GetProductInventoryAsync(SelectedProduct.Id);
                var warehouseStock = inventory?.FirstOrDefault(i => i.WarehouseId == SelectedWarehouse.Id);

                if (warehouseStock != null)
                {
                    ValidationMessage = $"✅ Ürün bulundu! Mevcut stok: {warehouseStock.Quantity} adet";
                }
                else
                {
                    ValidationMessage = "✅ Ürün bulundu! Bu depoda stok yok (0 adet)";
                }
            }
            catch (Exception)
            {
                // Stok bilgisi alınamazsa sessiz geç
                ValidationMessage = "✅ Ürün bulundu!";
            }
        }

        private async Task PerformStockIn()
        {
            if (!CanPerformOperation || SelectedProduct == null || SelectedWarehouse == null)
                return;

            try
            {
                SetBusy(true);
                ClearError();

                var stockDto = new StockOperationDto
                {
                    ProductId = SelectedProduct.Id,
                    WarehouseId = SelectedWarehouse.Id,
                    Quantity = Quantity
                };

                var result = await _apiService.StockInAsync(stockDto);

                if (result.Success)
                {
                    // Başarı mesajı göster
                    await ShowSuccess($"✅ STOK GİRİŞİ BAŞARILI!\n\n{SelectedProduct.Name}\n+{Quantity} adet\n{SelectedWarehouse.Name} deposuna eklendi");

                    // Ürün detay sayfasına git
                    var parameters = new Dictionary<string, object>
                    {
                        ["ProductId"] = SelectedProduct.Id,
                        ["ProductName"] = SelectedProduct.Name
                    };

                    await Shell.Current.GoToAsync("ProductDetailPage", parameters);
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Stok girişi hatası: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task PerformStockOut()
        {
            if (!CanPerformOperation || SelectedProduct == null || SelectedWarehouse == null)
                return;

            try
            {
                SetBusy(true);
                ClearError();

                // Önce mevcut stoku kontrol et
                var inventory = await _apiService.GetProductInventoryAsync(SelectedProduct.Id);
                var warehouseStock = inventory?.FirstOrDefault(i => i.WarehouseId == SelectedWarehouse.Id);

                int currentStock = warehouseStock?.Quantity ?? 0;

                // Stok yetersizliği kontrolü
                if (currentStock < Quantity)
                {
                    string message;
                    if (currentStock == 0)
                    {
                        message = $"❌ STOK YETERSİZ!\n\n'{SelectedProduct.Name}' ürünü\n'{SelectedWarehouse.Name}' deposunda stokta bulunmuyor.\n\nMevcut stok: 0 adet\nİstenen miktar: {Quantity} adet";
                    }
                    else
                    {
                        message = $"❌ STOK YETERSİZ!\n\n'{SelectedProduct.Name}' ürünü için\nyetersiz stok.\n\nMevcut stok: {currentStock} adet\nİstenen miktar: {Quantity} adet\n\nEn fazla {currentStock} adet çıkış yapabilirsiniz.";
                    }

                    await ShowError(message);
                    ValidationMessage = $"❌ Yetersiz stok! Mevcut: {currentStock} adet";
                    return;
                }

                // Stok yeterliyse işlemi gerçekleştir
                var stockDto = new StockOperationDto
                {
                    ProductId = SelectedProduct.Id,
                    WarehouseId = SelectedWarehouse.Id,
                    Quantity = Quantity
                };

                var result = await _apiService.StockOutAsync(stockDto);

                if (result.Success)
                {
                    // Başarı mesajı göster
                    await ShowSuccess($"✅ STOK ÇIKIŞI BAŞARILI!\n\n{SelectedProduct.Name}\n-{Quantity} adet\n{SelectedWarehouse.Name} deposundan çıkarıldı");

                    // Ürün detay sayfasına git
                    var parameters = new Dictionary<string, object>
                    {
                        ["ProductId"] = SelectedProduct.Id,
                        ["ProductName"] = SelectedProduct.Name
                    };

                    await Shell.Current.GoToAsync("ProductDetailPage", parameters);
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Stok çıkışı hatası: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        public async Task HandleBarcodeResult(string scannedSku)
        {
            if (!string.IsNullOrWhiteSpace(scannedSku))
            {
                SkuInput = scannedSku;
                ValidationMessage = "🔍 Barkod okundu, ürün aranıyor...";
                await SearchProduct();
            }
        }

        private void ValidateInput()
        {
            CanPerformOperation = SelectedProduct != null &&
                                 SelectedWarehouse != null &&
                                 Quantity > 0;

            if (string.IsNullOrWhiteSpace(SkuInput))
            {
                ValidationMessage = "SKU girin veya barkod okutun";
            }
            else if (SelectedProduct == null && !string.IsNullOrWhiteSpace(SkuInput))
            {
                ValidationMessage = "Bu SKU için ürün arayın";
            }
            else if (SelectedProduct != null && SelectedWarehouse == null)
            {
                ValidationMessage = "Depo seçin";
            }
            else if (SelectedProduct != null && SelectedWarehouse != null && Quantity <= 0)
            {
                ValidationMessage = "Geçerli miktar girin";
            }
            else if (CanPerformOperation)
            {
                ValidationMessage = "✅ İşlem yapılabilir";
                // Depo değiştiğinde stok bilgisini güncelle
                _ = LoadProductStock();
            }
        }

        private void ClearForm()
        {
            SkuInput = string.Empty;
            SelectedProduct = null;
            IsProductVisible = false;
            Quantity = 1;
            ValidationMessage = string.Empty;
            HasLastOperationResult = false;
            LastOperationResult = string.Empty;
        }

        #endregion

        public async Task OnAppearing()
        {
            await LoadWarehouses();
        }
    }
}