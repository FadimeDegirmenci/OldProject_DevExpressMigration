using SM.MAUI.Services;
using SM.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    public class AddProductViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        #region Properties

        private string _productName = string.Empty;
        public string ProductName
        {
            get => _productName;
            set => SetProperty(ref _productName, value);
        }

        private string _sku = string.Empty;
        public string SKU
        {
            get => _sku;
            set => SetProperty(ref _sku, value);
        }

        private string _price = string.Empty;
        public string Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private string _initialStock = string.Empty;
        public string InitialStock
        {
            get => _initialStock;
            set => SetProperty(ref _initialStock, value);
        }

        private ObservableCollection<WarehouseDisplayModel> _warehouses = new();
        public ObservableCollection<WarehouseDisplayModel> Warehouses
        {
            get => _warehouses;
            set => SetProperty(ref _warehouses, value);
        }

        private WarehouseDisplayModel? _selectedWarehouse;
        public WarehouseDisplayModel? SelectedWarehouse
        {
            get => _selectedWarehouse;
            set => SetProperty(ref _selectedWarehouse, value);
        }

        private bool _showPreview;
        public bool ShowPreview
        {
            get => _showPreview;
            set => SetProperty(ref _showPreview, value);
        }

        private string _successMessage = string.Empty;
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        public AddProductViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Ürün Ekle";

            SaveCommand = new Command(async () => await SaveProduct());
            CancelCommand = new Command(async () => await Cancel());

            // Load warehouses when ViewModel is created
            _ = LoadWarehouses();
        }

        #region Methods

        private async Task LoadWarehouses()
        {
            try
            {
                SetBusy(true);
                var warehouses = await _apiService.GetWarehousesAsync();

                Warehouses.Clear();
                foreach (var warehouse in warehouses)
                {
                    Warehouses.Add(new WarehouseDisplayModel
                    {
                        Id = warehouse.Id,
                        Name = warehouse.Name,
                        Location = warehouse.Location,
                        DisplayName = $"{warehouse.Name} - {warehouse.Location}"
                    });
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Depolar yüklenirken hata oluştu: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task SaveProduct()
        {
            if (IsLoading) return;

            try
            {
                SetBusy(true);
                ClearError();
                SuccessMessage = string.Empty;

                // Validation
                if (string.IsNullOrWhiteSpace(ProductName))
                {
                    SetError("Ürün adı zorunludur!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(SKU))
                {
                    SetError("SKU zorunludur!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Price))
                {
                    SetError("Fiyat zorunludur!");
                    return;
                }

                if (!decimal.TryParse(Price.Replace(',', '.'), out decimal priceValue) || priceValue <= 0)
                {
                    SetError("Geçerli bir fiyat girin!");
                    return;
                }

                if (SelectedWarehouse == null)
                {
                    SetError("Depo seçimi zorunludur!");
                    return;
                }

                // Validate initial stock if provided
                int initialStockValue = 0;
                if (!string.IsNullOrWhiteSpace(InitialStock))
                {
                    if (!int.TryParse(InitialStock, out initialStockValue) || initialStockValue < 0)
                    {
                        SetError("Geçerli bir başlangıç stok miktarı girin!");
                        return;
                    }
                }

                // Create product DTO
                var productDto = new CreateProductDto
                {
                    Name = ProductName.Trim(),
                    SKU = SKU.Trim().ToUpper(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    Price = priceValue
                };

                // Save product to API
                var createdProduct = await _apiService.CreateProductAsync(productDto);

                // If initial stock is provided, add stock to the selected warehouse
                if (initialStockValue > 0)
                {
                    var stockDto = new StockOperationDto
                    {
                        ProductId = createdProduct.Id,
                        WarehouseId = SelectedWarehouse.Id,
                        Quantity = initialStockValue
                    };

                    await _apiService.StockInAsync(stockDto);
                }

                // Show success message
                var successMsg = $"Ürün başarıyla eklendi!\n" +
                               $"• Adı: {createdProduct.Name}\n" +
                               $"• SKU: {createdProduct.SKU}\n" +
                               $"• Depo: {SelectedWarehouse.Name}";

                if (initialStockValue > 0)
                {
                    successMsg += $"\n• Başlangıç Stok: {initialStockValue} adet";
                }

                await ShowSuccess(successMsg);

                // Go back to product list
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await ShowError($"Ürün eklenirken hata oluştu: {ex.Message}");
                SetError($"Ürün eklenirken hata oluştu: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }

        #endregion
    }

    #region Helper Models

    public class WarehouseDisplayModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    #endregion
}