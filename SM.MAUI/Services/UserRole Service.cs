namespace SM.MAUI.Services
{
    public enum UserRole
    {
        Admin,
        WarehouseEmployee
    }

    public interface IUserRoleService
    {
        UserRole CurrentUserRole { get; }
        string CurrentUserName { get; }
        void SetUserRole(UserRole role, string userName);
        bool IsAdmin { get; }
        bool IsWarehouseEmployee { get; }
        void Logout();
    }

    public class UserRoleService : IUserRoleService
    {
        private UserRole _currentUserRole = UserRole.Admin; // Varsayılan admin
        private string _currentUserName = "Kullanıcı";

        public UserRole CurrentUserRole => _currentUserRole;
        public string CurrentUserName => _currentUserName;
        public bool IsAdmin => _currentUserRole == UserRole.Admin;
        public bool IsWarehouseEmployee => _currentUserRole == UserRole.WarehouseEmployee;

        public void SetUserRole(UserRole role, string userName)
        {
            _currentUserRole = role;
            _currentUserName = userName;
        }

        public void Logout()
        {
            _currentUserRole = UserRole.Admin;
            _currentUserName = "Kullanıcı";
        }

        public string GetRoleDisplayName()
        {
            return _currentUserRole switch
            {
                UserRole.Admin => "👨‍💼 Yönetici",
                UserRole.WarehouseEmployee => "👷‍♂️ Depo Çalışanı",
                _ => "Kullanıcı"
            };
        }

        public List<string> GetAllowedFeatures()
        {
            return _currentUserRole switch
            {
                UserRole.Admin => new List<string>
                {
                    "Dashboard",
                    "Ürün Yönetimi",
                    "Depo Yönetimi",
                    "Stok İşlemleri",
                    "Raporlama",
                    "Sistem Ayarları"
                },
                UserRole.WarehouseEmployee => new List<string>
                {
                    "Ürün Görüntüleme",
                    "Stok İşlemleri",
                    "Barkod Okuma"
                },
                _ => new List<string>()
            };
        }
    }
}