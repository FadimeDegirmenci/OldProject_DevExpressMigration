using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // YENİ EKLEME - HasError property
        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        // YENİ EKLEME - ClearErrorCommand
        public ICommand ClearErrorCommand { get; }

        // YENİ EKLEME - Constructor
        public BaseViewModel()
        {
            ClearErrorCommand = new Command(() => ClearError());
        }

        protected void SetBusy(bool value)
        {
            IsBusy = value;
            IsLoading = value; // Compatibility için her ikisini de set et
        }

        protected void SetError(string message)
        {
            ErrorMessage = message;
            HasError = !string.IsNullOrEmpty(message); // YENİ EKLEME
        }

        protected void ClearError()
        {
            ErrorMessage = string.Empty;
            HasError = false; // YENİ EKLEME
        }

        protected async Task ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true; // YENİ EKLEME

            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", message, "Tamam");
            }
        }

        protected async Task ShowSuccess(string message)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Başarılı", message, "Tamam");
            }
        }

        protected async Task<bool> ShowConfirmation(string message)
        {
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert("Onay", message, "Evet", "Hayır");
            }
            return false;
        }

        // Toast mesajı için (opsiyonel)
        protected void ShowToast(string message)
        {
            // MAUI'de toast için CommunityToolkit.Maui kullanılabilir
            // Şimdilik basit bir implementasyon
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Bilgi", message, "Tamam");
                }
            });
        }
    }
}