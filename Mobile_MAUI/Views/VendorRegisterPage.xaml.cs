using FoodServiceApp.Maui.Services;
using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class VendorRegisterPage : ContentPage
{
    public VendorRegisterPage()
    {
        InitializeComponent();
    }

    private async void OnGuiDangKyClicked(object sender, EventArgs e)
    {
        LblLoi.IsVisible = false;

        if (string.IsNullOrWhiteSpace(EntryTenGianHang.Text))
        {
            HienLoi("Vui lòng nhập tên gian hàng");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntryDiaChi.Text))
        {
            HienLoi("Vui lòng nhập địa chỉ cửa hàng");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntrySdt.Text))
        {
            HienLoi("Vui lòng nhập số điện thoại");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntryEmail.Text) || !EntryEmail.Text.Contains('@'))
        {
            HienLoi("Email không hợp lệ");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntryMatKhau.Text) || EntryMatKhau.Text.Length < 6)
        {
            HienLoi("Mật khẩu tối thiểu 6 ký tự");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        BtnGuiDangKy.IsEnabled = false;

        try
        {
            await ApiClient.SendAsync(HttpMethod.Post, "api/vendor/register", new {
                TenDangNhap = EntryEmail.Text.Trim(), MatKhau = EntryMatKhau.Text,
                TenCuaHang = EntryTenGianHang.Text.Trim(), DiaChi = EntryDiaChi.Text.Trim(),
                SDT = EntrySdt.Text.Trim(), MoTa = EditorMoTa.Text?.Trim()
            }, role: null);
            await DisplayAlert(
                "Đăng ký thành công",
                "Hồ sơ đã được lưu trên máy chủ và đang chờ quản trị viên duyệt.",
                "Đã hiểu");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { HienLoi(ex.Message); }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            BtnGuiDangKy.IsEnabled = true;
        }
    }

    private void HienLoi(string thongDiep)
    {
        LblLoi.Text = thongDiep;
        LblLoi.IsVisible = true;
    }
}
