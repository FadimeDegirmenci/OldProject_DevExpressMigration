using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Input;
using SM.MAUI.Services;

namespace SM.MAUI.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly SessionService _sessionService;

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(ApiService apiService, SessionService sessionService)
        {
            _apiService = apiService;
            _sessionService = sessionService;
            Title = "Giriş";

            LoginCommand = new Command(async () => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            if (IsBusy) return;

            ClearError();

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                SetError("Kullanıcı adı ve şifre zorunludur.");
                return;
            }

            try
            {
                SetBusy(true);

                // 1) API'ye giriş isteği: başarılıysa token ApiService'e yerleşir
                var result = await _apiService.LoginAsync(Username.Trim(), Password);

                // 2) Kullanıcı adı ve rolü oturuma yaz
                _sessionService.Start(result);

                // 3) Şifreyi hafızada tutma
                Password = string.Empty;

                // 4) Ana menüye geç
                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }
    }
}