using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class AdminHomePage : ContentPage
{
    public AdminHomePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NapTongQuan();
    }

    private void NapTongQuan()
    {
        LblChaoAdmin.Text = $"Xin chào, {MockData.QuanTriVienHienTai.HoTen}";

        var gianHangDaDuyet = MockData.DanhSachGianHang.Where(g => g.TrangThaiDangKy == TrangThaiDangKyGianHang.DaDuyet).ToList();
        LblTongGianHang.Text = gianHangDaDuyet.Count.ToString();
        LblTongKhachHang.Text = MockData.DanhSachKhachHang.Count.ToString();

        var tongDoanhThu = gianHangDaDuyet.Sum(g => MockData.DoanhThuUocTinh(g.Id));
        LblTongDoanhThu.Text = $"{tongDoanhThu:N0}đ";

        var soChoDuyet = MockData.DanhSachGianHang.Count(g => g.TrangThaiDangKy == TrangThaiDangKyGianHang.ChoDuyet);
        LblSoChoDuyet.Text = soChoDuyet > 0 ? $"{soChoDuyet} hồ sơ mới" : "Không có hồ sơ mới";
    }

    private async void OnDuyetGianHangTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminVendorApprovalPage));
    private async void OnDanhSachGianHangTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminVendorListPage));
    private async void OnKhachHangTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminCustomerListPage));
    private async void OnVanDeTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminIssuesPage));
    private async void OnKhuyenMaiTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminPromotionsPage));
    private async void OnThongKeTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminStatisticsPage));

    private void OnRefreshing(object? sender, EventArgs e)
    {
        NapTongQuan();
        RefreshTrangChu.IsRefreshing = false;
    }

    private async void OnDangXuatClicked(object sender, EventArgs e)
    {
        bool xacNhan = await DisplayAlert("Đăng xuất", "Bạn có chắc muốn đăng xuất khỏi cổng quản trị?", "Đăng xuất", "Hủy");
        if (!xacNhan) return;

        await Shell.Current.GoToAsync("//dang-nhap");
    }
}
