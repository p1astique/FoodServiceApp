using FoodServiceApp.Maui.Services;
using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class LoginPage : ContentPage
{
    private bool _dangOChedoDangNhap = true;

    public LoginPage()
    {
        InitializeComponent();
    }

    private void OnTabDangNhapClicked(object sender, EventArgs e) => ChuyenCheDo(true);
    private void OnTabDangKyClicked(object sender, EventArgs e) => ChuyenCheDo(false);

    private void ChuyenCheDo(bool laDangNhap)
    {
        _dangOChedoDangNhap = laDangNhap;
        KhungDangKy.IsVisible = !laDangNhap;
        BtnXacNhan.Text = laDangNhap ? "Đăng nhập" : "Tạo tài khoản";
        LblPhuDe.Text = laDangNhap ? "Đăng nhập để tiếp tục đặt món" : "Tạo tài khoản mới để bắt đầu";

        TabDangNhap.BackgroundColor = laDangNhap ? Colors.White : Colors.Transparent;
        TabDangNhap.TextColor = laDangNhap ? (Color)Application.Current!.Resources["Primary"] : (Color)Application.Current!.Resources["TextGrey"];
        TabDangKy.BackgroundColor = laDangNhap ? Colors.Transparent : Colors.White;
        TabDangKy.TextColor = laDangNhap ? (Color)Application.Current!.Resources["TextGrey"] : (Color)Application.Current!.Resources["Primary"];
    }

    private async void OnXacNhanClicked(object sender, EventArgs e)
    {
        LblLoi.IsVisible = false;

        if (string.IsNullOrWhiteSpace(EntryEmail.Text))
        {
            HienLoi("Vui lòng nhập tên đăng nhập");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntryMatKhau.Text) || EntryMatKhau.Text.Length < 6)
        {
            HienLoi("Mật khẩu tối thiểu 6 ký tự");
            return;
        }
        if (!_dangOChedoDangNhap && string.IsNullOrWhiteSpace(EntryHoTen.Text))
        {
            HienLoi("Vui lòng nhập họ tên");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        BtnXacNhan.IsEnabled = false;

        try
        {
            if (!_dangOChedoDangNhap)
                await ApiClient.SendAsync(HttpMethod.Post, "api/customer/register", new {
                    TenDangNhap = EntryEmail.Text.Trim(), MatKhau = EntryMatKhau.Text,
                    HoTen = EntryHoTen.Text.Trim(), SDT = EntrySdt.Text?.Trim() ?? ""
                }, role: null);
            await ApiClient.LoginAsync("KhachHang", EntryEmail.Text, EntryMatKhau.Text);
            await AppShell.DangNhapThanhCongAsync();
        }
        catch (Exception ex) { HienLoi(ex.Message); }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            BtnXacNhan.IsEnabled = true;
        }
    }

    private void HienLoi(string thongDiep)
    {
        LblLoi.Text = thongDiep;
        LblLoi.IsVisible = true;
    }

    private async void OnConfigureServerClicked(object sender, EventArgs e)
    {
        var url = await DisplayPromptAsync("Kết nối máy chủ", "Địa chỉ Web API", initialValue: ApiClient.BaseUrl);
        if (string.IsNullOrWhiteSpace(url)) return;
        try { ApiClient.BaseUrl = url; }
        catch (Exception ex) { HienLoi(ex.Message); }
    }

    private async void OnQuenMatKhauTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Quên mật khẩu", "Chức năng đặt lại mật khẩu sẽ được bổ sung khi nối API thật.", "Đóng");
    }

    private async void OnDangNhapTaiXeTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(DriverLoginPage));
    }

    private async void OnDangNhapGianHangTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(VendorLoginPage));
    }

    private async void OnDangNhapQuanTriTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AdminLoginPage));
    }
}
