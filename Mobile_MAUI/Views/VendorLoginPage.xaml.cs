using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class VendorLoginPage : ContentPage
{
    public VendorLoginPage()
    {
        InitializeComponent();
    }

    private async void OnDangNhapClicked(object sender, EventArgs e)
    {
        LblLoi.IsVisible = false;

        if (string.IsNullOrWhiteSpace(EntryEmail.Text) || !EntryEmail.Text.Contains('@'))
        {
            HienLoi("Email không hợp lệ");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntryMatKhau.Text))
        {
            HienLoi("Vui lòng nhập mật khẩu");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        BtnDangNhap.IsEnabled = false;

        try
        {
            // TODO: gọi ASP.NET Core Web API thật: POST /api/gian-hang/dang-nhap { email, matKhau }
            await Task.Delay(500);

            var gianHang = MockData.DanhSachGianHang.FirstOrDefault(g =>
                string.Equals(g.Email, EntryEmail.Text.Trim(), StringComparison.OrdinalIgnoreCase) &&
                g.MatKhau == EntryMatKhau.Text);

            if (gianHang is null)
            {
                HienLoi("Email hoặc mật khẩu không đúng");
                return;
            }

            if (gianHang.TrangThaiDangKy == TrangThaiDangKyGianHang.ChoDuyet)
            {
                HienLoi("Hồ sơ gian hàng của bạn đang chờ quản trị viên duyệt");
                return;
            }
            if (gianHang.TrangThaiDangKy == TrangThaiDangKyGianHang.TuChoi)
            {
                HienLoi("Hồ sơ gian hàng đã bị từ chối. Vui lòng liên hệ quản trị viên");
                return;
            }
            if (gianHang.TaiKhoanBiKhoa)
            {
                HienLoi("Tài khoản gian hàng đang bị khóa. Vui lòng liên hệ quản trị viên");
                return;
            }

            MockData.GianHangHienTai = gianHang;
            await Shell.Current.GoToAsync(nameof(VendorHomePage));
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            BtnDangNhap.IsEnabled = true;
        }
    }

    private void HienLoi(string thongDiep)
    {
        LblLoi.Text = thongDiep;
        LblLoi.IsVisible = true;
    }

    private async void OnDangKyTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(VendorRegisterPage));
    }

    private async void OnQuayLaiTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
