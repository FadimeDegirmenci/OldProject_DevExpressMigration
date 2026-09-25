using SM.MAUI.Views;

namespace SM.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("UserSelectionPage", typeof(UserSelectionPage));

        // Route'ları kaydet - Views namespace'indeki sayfalar
        Routing.RegisterRoute("ProductListPage", typeof(ProductListPage));
        Routing.RegisterRoute("AddProductPage", typeof(AddProductPage));
       
        Routing.RegisterRoute("BarcodeScanPage", typeof(BarcodeScanPage));
        Routing.RegisterRoute("ProductDetailPage", typeof(ProductDetailPage));
        Routing.RegisterRoute("EditProductPage", typeof(EditProductPage));


        Routing.RegisterRoute("StockOperationPage", typeof(StockOperationPage));


        Routing.RegisterRoute("WarehouseListPage", typeof(WarehouseListPage));    
        Routing.RegisterRoute("AddWarehousePage", typeof(AddWarehousePage));
        Routing.RegisterRoute("EditWarehousePage", typeof(EditWarehousePage));
        Routing.RegisterRoute("WarehouseDetailPage", typeof(WarehouseDetailPage));

        Routing.RegisterRoute("DashboardPage", typeof(DashboardPage));
        Routing.RegisterRoute("DevExpressPage", typeof(DevExpressPage));


    }
}