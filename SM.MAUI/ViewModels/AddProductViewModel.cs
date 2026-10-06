using SM.MAUI.Services;
using SM.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    public class AddProductViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        #region Properties

        // DataForm'un doldurduğu nesne (6 alanın hepsi burada)
        public AddProductFormDto ProductForm { get; } = new();

        private ObservableCollection<WarehouseDisplayModel> _warehouses = new();
        public ObservableCollection<WarehouseDisplayModel> Warehouses
        {
            get => _warehouses;
            set => SetProperty(ref _warehouses, value);
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

        public ICommand CancelCommand { get; }

        #endregion

        public AddProductViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Ürün Ekle";

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

        // Code-behind, form geçerliyse bu metodu çağırır
        public async Task SaveProductAsync()
        {
            if (IsLoading) return;

            try
            {
                SetBusy(true);
                ClearError();
                SuccessMessage = string.Empty;

                // Form doğrulandığı için WarehouseId burada dolu
                int warehouseId = ProductForm.WarehouseId ?? 0;
                int initialStock = ProductForm.InitialStock ?? 0;

                // 1) Formdaki bilgilerden API'nin beklediği ürün DTO'sunu hazırla
                var productDto = new CreateProductDto
                {
                    Name = ProductForm.Name.Trim(),
                    SKU = ProductForm.SKU.Trim().ToUpper(),
                    Description = string.IsNullOrWhiteSpace(ProductForm.Description)
                        ? null
                        : ProductForm.Description.Trim(),
                    Price = ProductForm.Price
                };

                var createdProduct = await _apiService.CreateProductAsync(productDto);

                // 2) Başlangıç stoğu girildiyse stok DTO'sunu hazırla ve gönder
                if (initialStock > 0)
                {
                    var stockDto = new StockOperationDto
                    {
                        ProductId = createdProduct.Id,
                        WarehouseId = warehouseId,
                        Quantity = initialStock
                    };

                    await _apiService.StockInAsync(stockDto);
                }

                // 3) Başarı mesajı
                var warehouseName = Warehouses.FirstOrDefault(w => w.Id == warehouseId)?.Name ?? "-";

                var successMsg = $"Ürün başarıyla eklendi!\n" +
                                 $"• Adı: {createdProduct.Name}\n" +
                                 $"• SKU: {createdProduct.SKU}\n" +
                                 $"• Depo: {warehouseName}";

                if (initialStock > 0)
                {
                    successMsg += $"\n• Başlangıç Stok: {initialStock} adet";
                }

                await ShowSuccess(successMsg);

                // Go back to product list
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await ShowError($"Ürün eklenirken hata oluştu: {ex.Message}");
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

    public class AddProductFormDto
    {
        [Required(ErrorMessage = "Ürün adı zorunludur!")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU zorunludur!")]
        [StringLength(50, ErrorMessage = "SKU en fazla 50 karakter olabilir")]
        public string SKU { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır!")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Depo seçimi zorunludur!")]
        public int? WarehouseId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Başlangıç stoğu negatif olamaz!")]
        public int? InitialStock { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }
    }

    #endregion
}