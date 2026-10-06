using System;
using System.Collections.Generic;
using System.Text;

namespace SM.MAUI.Services
{
    public class SessionService
    {
        // Giriş yapan kullanıcının bilgileri
        public string Username { get; private set; } = string.Empty;
        public string Role { get; private set; } = string.Empty;

        // Kolaylık için hesaplanan özellikler
        public bool IsLoggedIn => !string.IsNullOrEmpty(Username);
        public bool IsAdmin => Role == "Yonetici";

        // Giriş başarılı olunca çağrılır
        public void Start(LoginResponseDto login)
        {
            Username = login.Username;
            Role = login.Role;
        }
    }
}
