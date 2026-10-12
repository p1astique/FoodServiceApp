using FoodServiceApp.Maui.Services;
using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class AdminHomePage : ContentPage
{
    public AdminHomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await NapTongQuan();
    }

    private Task NapTongQuan() => ApiUi.Run(this, async () => {
        var home = await ApiClient.GetAsync<AdminSummary>("api/admin/home", "QuanTri");
        var pending = await new VendorRegistrationApiClient().GetPendingAsync();
        LblChaoAdmin.Text = "Cổng quản trị";
        LblTongGianHang.Text = home.TongGianHang.ToString(); LblTongKhachHang.Text = home.TongKhachHang.ToString();
        LblTongDoanhThu.Text = $"{home.DoanhThuHomNay:N0}đ hôm nay"; LblSoChoDuyet.Text = $"{pending.Count} hồ sơ chờ duyệt";
    });
    private record AdminSummary(int TongGianHang, int TongKhachHang, decimal DoanhThuHomNay);
    private async void OnDuyetGianHangTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminVendorApprovalPage));
    private async void OnDanhSachGianHangTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminVendorListPage));
    private async void OnKhachHangTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminCustomerListPage));
    private async void OnVanDeTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminIssuesPage));
    private async void OnKhuyenMaiTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminPromotionsPage));
    private async void OnThongKeTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminStatisticsPage));

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await NapTongQuan();
        RefreshTrangChu.IsRefreshing = false;
    }

    private async void OnDangXuatClicked(object sender, EventArgs e)
    {
        bool xacNhan = await DisplayAlert("Đăng xuất", "Bạn có chắc muốn đăng xuất khỏi cổng quản trị?", "Đăng xuất", "Hủy");
        if (!xacNhan) return;

        ApiClient.Logout("QuanTri");
        await Shell.Current.GoToAsync("//dang-nhap");
    }
}
