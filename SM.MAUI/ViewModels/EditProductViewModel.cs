using SM.MAUI.Services;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    [QueryProperty(nameof(ProductId), "ProductId")]
    [QueryProperty(nameof(ProductName), "ProductName")]
    [QueryProperty(nameof(ProductSKU), "ProductSKU")]
    [QueryProperty(nameof(ProductPrice), "ProductPrice")]
    [QueryProperty(nameof(ProductDescription), "ProductDescription")]
    public class EditProductViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        #region Properties

        private int _productId;
        public int ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        private string _productName = string.Empty;
        public string ProductName
        {
            get => _productName;
            set
            {
                SetProperty(ref _productName, value);
                ValidateInput();
            }
        }

        private string _productSKU = string.Empty;
        public string ProductSKU
        {
            get => _productSKU;
            set
            {
                SetProperty(ref _productSKU, value);
                ValidateInput();
            }
        }

        private decimal _productPrice;
        public decimal ProductPrice
        {
            get => _productPrice;
            set
            {
                SetProperty(ref _productPrice, value);
                ValidateInput();
            }
        }

        private string _productDescription = string.Empty;
        public string ProductDescription
        {
            get => _productDescription;
            set => SetProperty(ref _productDescription, value);
        }

        private bool _isFormValid;
        public bool IsFormValid
        {
            get => _isFormValid;
            set => SetProperty(ref _isFormValid, value);
        }

        private string _nameError = string.Empty;
        public string NameError
        {
            get => _nameError;
            set => SetProperty(ref _nameError, value);
        }

        private string _skuError = string.Empty;
        public string SKUError
        {
            get => _skuError;
            set => SetProperty(ref _skuError, value);
        }

        private string _priceError = string.Empty;
        public string PriceError
        {
            get => _priceError;
            set => SetProperty(ref _priceError, value);
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        public EditProductViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Urun Duzenle";

            SaveCommand = new Command(async () => await SaveProduct(), () => IsFormValid && !IsBusy);
            CancelCommand = new Command(async () => await Cancel());

            // Command'ların durumunu güncelle
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsFormValid) || e.PropertyName == nameof(IsBusy))
                {
                    ((Command)SaveCommand).ChangeCanExecute();
                }
            };
        }

        private void ValidateInput()
        {
            // İsim validasyonu
            if (string.IsNullOrWhiteSpace(ProductName))
            {
                NameError = "Ürün adı gereklidir";
            }
            else if (ProductName.Length > 100)
            {
                NameError = "Ürün adı en fazla 100 karakter olabilir";
            }
            else
            {
                NameError = string.Empty;
            }

            // SKU validasyonu
            if (string.IsNullOrWhiteSpace(ProductSKU))
            {
                SKUError = "SKU gereklidir";
            }
            else if (ProductSKU.Length > 50)
            {
                SKUError = "SKU en fazla 50 karakter olabilir";
            }
            else
            {
                SKUError = string.Empty;
            }

            // Fiyat validasyonu
            if (ProductPrice <= 0)
            {
                PriceError = "Fiyat 0'dan büyük olmalıdır";
            }
            else
            {
                PriceError = string.Empty;
            }

            // Form geçerlilik durumu
            IsFormValid = string.IsNullOrEmpty(NameError) &&
                          string.IsNullOrEmpty(SKUError) &&
                          string.IsNullOrEmpty(PriceError) &&
                          !string.IsNullOrWhiteSpace(ProductName) &&
                          !string.IsNullOrWhiteSpace(ProductSKU) &&
                          ProductPrice > 0;
        }

        private async Task SaveProduct()
        {
            if (!IsFormValid || IsBusy) return;

            try
            {
                SetBusy(true);
                ClearError();

                var updateDto = new UpdateProductDto
                {
                    Name = ProductName.Trim(),
                    SKU = ProductSKU.Trim(),
                    Description = string.IsNullOrWhiteSpace(ProductDescription) ? null : ProductDescription.Trim(),
                    Price = ProductPrice
                };

                var updatedProduct = await _apiService.UpdateProductAsync(ProductId, updateDto);

                await Shell.Current.DisplayAlert("Başarılı", "Ürün başarıyla güncellendi!", "Tamam");

                // Detay sayfasına geri dön ve güncellenmiş bilgileri gönder
                var parameters = new Dictionary<string, object>
                {
                    ["ProductId"] = ProductId,
                    ["ProductName"] = updatedProduct.Name
                };

                await Shell.Current.GoToAsync("..", parameters);
            }
            catch (Exception ex)
            {
                await ShowError($"Ürün güncellenirken hata oluştu: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task Cancel()
        {
            bool shouldCancel = await Shell.Current.DisplayAlert(
                "İptal Et",
                "Değişiklikler kaydedilmeyecek. Emin misiniz?",
                "Evet",
                "Hayır");

            if (shouldCancel)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}