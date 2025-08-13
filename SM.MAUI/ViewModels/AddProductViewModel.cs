using SM.MAUI.Services;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    public class AddProductViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddProductViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Urun Ekle";

            SaveCommand = new Command(async () => await SaveProduct());
            CancelCommand = new Command(async () => await Cancel());
        }

        private async Task SaveProduct()
        {
            if (IsLoading) return;

            try
            {
                SetBusy(true);
                ClearError();

                // Validation
                if (string.IsNullOrWhiteSpace(ProductName))
                {
                    SetError("Urun adi zorunludur!");
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
                    SetError("Gecerli bir fiyat girin!");
                    return;
                }

                // Create product DTO
                var productDto = new CreateProductDto
                {
                    Name = ProductName.Trim(),
                    SKU = SKU.Trim().ToUpper(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    Price = priceValue
                };

                // Save to API
                var createdProduct = await _apiService.CreateProductAsync(productDto);

                await ShowSuccess($"Urun basariyla eklendi!\nAdi: {createdProduct.Name}\nSKU: {createdProduct.SKU}");

                // Go back to product list
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await ShowError($"Urun eklenirken hata olustu: {ex.Message}");
                SetError($"Urun eklenirken hata olustu: {ex.Message}");
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
    }
}