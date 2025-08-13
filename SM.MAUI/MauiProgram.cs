using Microsoft.Extensions.Logging;
using SM.MAUI.Services;
using SM.MAUI.ViewModels;
using SM.MAUI.Views;
using Syncfusion.Maui.Core.Hosting;
using ZXing.Net.Maui.Controls;

namespace SM.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureSyncfusionCore() // ✅ BU SATIRI EKLEDİM
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ✅ SYNCFUSION LİSANS AYARI (İSTEĞE BAĞLI)
        // Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY_HERE");

        // Services
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<IUserRoleService, UserRoleService>();

        // Views
        builder.Services.AddTransient<ProductListPage>();
        builder.Services.AddTransient<AddProductPage>();
        builder.Services.AddTransient<StockOperationPage>();
        builder.Services.AddTransient<ProductDetailPage>();
        builder.Services.AddTransient<EditProductPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<WarehouseListPage>();
        builder.Services.AddTransient<AddWarehousePage>();
        builder.Services.AddTransient<EditWarehousePage>();
        builder.Services.AddTransient<WarehouseDetailPage>();
        builder.Services.AddTransient<UserSelectionPage>();

        // ViewModels
        builder.Services.AddTransient<ProductListViewModel>();
        builder.Services.AddTransient<AddProductViewModel>();
        builder.Services.AddTransient<StockOperationViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddTransient<EditProductViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<WarehouseListViewModel>();
        builder.Services.AddTransient<AddWarehouseViewModel>();
        builder.Services.AddTransient<EditWarehouseViewModel>();
        builder.Services.AddTransient<WarehouseDetailViewModel>();

#if DEBUG
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif

        return builder.Build();
    }
}