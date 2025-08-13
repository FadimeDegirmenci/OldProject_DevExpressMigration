using SM.MAUI.Services;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    public class AddWarehouseViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        #region Properties

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

        public AddWarehouseViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Yeni Depo Ekle";

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

                // API'ye depo ekleme isteği gönder
                var createDto = new CreateWarehouseDto
                {
                    Name = WarehouseName.Trim(),
                    Location = Location.Trim()
                };

                var createdWarehouse = await _apiService.CreateWarehouseAsync(createDto);

                await Shell.Current.DisplayAlert("Başarılı",
                    $"'{createdWarehouse.Name}' deposu başarıyla eklendi!\n\nLokasyon: {createdWarehouse.Location}",
                    "Tamam");

                // Geri dön ve depo listesini yenile
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await ShowError($"Depo eklenirken hata oluştu: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task Cancel()
        {
            if (!string.IsNullOrWhiteSpace(WarehouseName) || !string.IsNullOrWhiteSpace(Location))
            {
                bool shouldCancel = await Shell.Current.DisplayAlert(
                    "İptal Et",
                    "Girilen bilgiler kaydedilmeyecek. Emin misiniz?",
                    "Evet",
                    "Hayır");

                if (!shouldCancel) return;
            }

            await Shell.Current.GoToAsync("..");
        }

        #endregion
    }
}