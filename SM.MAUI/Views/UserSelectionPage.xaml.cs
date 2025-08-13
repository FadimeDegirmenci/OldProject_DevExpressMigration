using SM.MAUI.Services;

namespace SM.MAUI.Views
{
    public partial class UserSelectionPage : ContentPage
    {
        private readonly IUserRoleService _userRoleService;

        public UserSelectionPage(IUserRoleService userRoleService)
        {
            InitializeComponent();
            _userRoleService = userRoleService;
        }

        private async void OnAdminSelected(object sender, TappedEventArgs e)
        {
            try
            {
                // Admin rolünü set et
                _userRoleService.SetUserRole(UserRole.Admin, "Yönetici");

                // Hoş geldin mesajı
                await DisplayAlert("Hoş Geldiniz! 👨‍💼",
                    "Yönetici olarak giriş yaptınız.\n\nTüm sistem özelliklerine erişiminiz bulunmaktadır.",
                    "Devam Et");

                // Ana sayfaya yönlendir
                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Giriş yapılırken hata oluştu: {ex.Message}", "Tamam");
            }
        }

        private async void OnWarehouseEmployeeSelected(object sender, TappedEventArgs e)
        {
            try
            {
                // Depo çalışanı rolünü set et
                _userRoleService.SetUserRole(UserRole.WarehouseEmployee, "Depo Çalışanı");

                // Hoş geldin mesajı
                await DisplayAlert("Hoş Geldiniz! 👷‍♂️",
                    "Depo Çalışanı olarak giriş yaptınız.\n\nStok işlemleri ve ürün görüntüleme yetkileriniz bulunmaktadır.",
                    "Devam Et");

                // Ana sayfaya yönlendir  
                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Giriş yapılırken hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }
}