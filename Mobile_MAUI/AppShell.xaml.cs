using FoodServiceApp.Maui.Views;

namespace FoodServiceApp.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Các trang được điều hướng bằng Shell.Current.GoToAsync (push lên trên,
        // không nằm trong TabBar dưới) — tương đương Navigator.push() bên Flutter.
        Routing.RegisterRoute(nameof(MenuCartPage), typeof(MenuCartPage));
        Routing.RegisterRoute(nameof(PaymentPage), typeof(PaymentPage));
        Routing.RegisterRoute(nameof(OrderTrackingPage) + "Detail", typeof(OrderTrackingPage));
        Routing.RegisterRoute(nameof(ReviewPage), typeof(ReviewPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));

        // Luồng tài xế giao hàng (điều hướng dạng stack, nằm ngoài TabBar khách hàng).
        Routing.RegisterRoute(nameof(DriverLoginPage), typeof(DriverLoginPage));
        Routing.RegisterRoute(nameof(DriverHomePage), typeof(DriverHomePage));
        Routing.RegisterRoute(nameof(DriverDeliveryDetailPage), typeof(DriverDeliveryDetailPage));

        // Luồng quản lý gian hàng (đối tác) — điều hướng dạng stack, nằm ngoài TabBar khách hàng.
        Routing.RegisterRoute(nameof(VendorLoginPage), typeof(VendorLoginPage));
        Routing.RegisterRoute(nameof(VendorRegisterPage), typeof(VendorRegisterPage));
        Routing.RegisterRoute(nameof(VendorHomePage), typeof(VendorHomePage));
        Routing.RegisterRoute(nameof(VendorOrdersPage), typeof(VendorOrdersPage));
        Routing.RegisterRoute(nameof(VendorMenuPage), typeof(VendorMenuPage));
        Routing.RegisterRoute(nameof(VendorMenuItemEditPage), typeof(VendorMenuItemEditPage));
        Routing.RegisterRoute(nameof(VendorRevenuePage), typeof(VendorRevenuePage));
        Routing.RegisterRoute(nameof(VendorProfilePage), typeof(VendorProfilePage));
        Routing.RegisterRoute(nameof(VendorPromotionsPage), typeof(VendorPromotionsPage));
        Routing.RegisterRoute(nameof(VendorPromotionEditPage), typeof(VendorPromotionEditPage));

        // Luồng quản lý quản trị — điều hướng dạng stack, nằm ngoài TabBar khách hàng.
        Routing.RegisterRoute(nameof(AdminLoginPage), typeof(AdminLoginPage));
        Routing.RegisterRoute(nameof(AdminHomePage), typeof(AdminHomePage));
        Routing.RegisterRoute(nameof(AdminVendorApprovalPage), typeof(AdminVendorApprovalPage));
        Routing.RegisterRoute(nameof(AdminVendorListPage), typeof(AdminVendorListPage));
        Routing.RegisterRoute(nameof(AdminCustomerListPage), typeof(AdminCustomerListPage));
        Routing.RegisterRoute(nameof(AdminIssuesPage), typeof(AdminIssuesPage));
        Routing.RegisterRoute(nameof(AdminPromotionsPage), typeof(AdminPromotionsPage));
        Routing.RegisterRoute(nameof(AdminPromotionEditPage), typeof(AdminPromotionEditPage));
        Routing.RegisterRoute(nameof(AdminStatisticsPage), typeof(AdminStatisticsPage));
    }

    /// <summary>
    /// Gọi sau khi đăng nhập thành công: ẩn trang đăng nhập, hiện TabBar chính.
    /// Tương đương Navigator.pushReplacement(HomeScreen) bên Flutter.
    /// </summary>
    public static async Task DangNhapThanhCongAsync()
    {
        await Current.GoToAsync("//chinh/trang-chu");
    }
}
