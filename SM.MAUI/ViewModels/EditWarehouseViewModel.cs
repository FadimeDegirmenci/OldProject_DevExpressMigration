using SM.MAUI.Services;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    [QueryProperty(nameof(WarehouseId), "WarehouseId")]
    [QueryProperty(nameof(WarehouseName), "WarehouseName")]
    [QueryProperty(nameof(Location), "Location")]
    public class EditWarehouseViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        #region Properties

        private int _warehouseId;
        public int WarehouseId
        {
            get => _warehouseId;
            set => SetProperty(ref _warehouseId, value);
        }

        private string _warehouseName = string.Empty;
        public string WarehouseName
        {
            get => _warehouseName;
            set
            {
                SetProperty(ref _warehouseName, value);
                ValidateInput();
            }
        }

        private string _location = string.Empty;
        public string Location
        {
            get => _location;
            set
            {
                SetProperty(ref _location, value);
                ValidateInput();
            }
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

        private string _locationError = string.Empty;
        public string LocationError
        {
            get => _locationError;
            set => SetProperty(ref _locationError, value);
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        public EditWarehouseViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Depo Düzenle";

            SaveCommand = new Command(async () => await SaveWarehouse(), () => IsFormValid && !IsBusy);
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

        #region Methods

        private void ValidateInput()
        {
            // İsim validasyonu
            if (string.IsNullOrWhiteSpace(WarehouseName))
            {
                NameError = "Depo adı gereklidir";
            }
            else if (WarehouseName.Length > 100)
            {
                NameError = "Depo adı en fazla 100 karakter olabilir";
            }
            else
            {
                NameError = string.Empty;
            }

            // Lokasyon validasyonu
            if (string.IsNullOrWhiteSpace(Location))
            {
                LocationError = "Lokasyon gereklidir";
            }
            else if (Location.Length > 200)
            {
                LocationError = "Lokasyon en fazla 200 karakter olabilir";
            }
            else
            {
                LocationError = string.Empty;
            }

            // Form geçerlilik durumu
            IsFormValid = string.IsNullOrEmpty(NameError) &&
                          string.IsNullOrEmpty(LocationError) &&
                          !string.IsNullOrWhiteSpace(WarehouseName) &&
                          !string.IsNullOrWhiteSpace(Location);
        }

        private async Task SaveWarehouse()
        {
            if (!IsFormValid || IsBusy) return;

            try
            {
                SetBusy(true);
                ClearError();

                var updateDto = new UpdateWarehouseDto
                {
                    Name = WarehouseName.Trim(),
                    Location = Location.Trim()
                };

                var updatedWarehouse = await _apiService.UpdateWarehouseAsync(WarehouseId, updateDto);

                await Shell.Current.DisplayAlert("Başarılı", "Depo başarıyla güncellendi!", "Tamam");

                // Detay sayfasına geri dön ve güncellenmiş bilgileri gönder
                var parameters = new Dictionary<string, object>
                {
                    ["WarehouseId"] = WarehouseId,
                    ["WarehouseName"] = updatedWarehouse.Name
                };

                await Shell.Current.GoToAsync("..", parameters);
            }
            catch (Exception ex)
            {
                await ShowError($"Depo güncellenirken hata oluştu: {ex.Message}");
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

        #endregion
    }
}