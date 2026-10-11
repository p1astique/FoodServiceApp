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
        if (!_dangOChedoDangNhap && string.IsNullOrWhiteSpace(EntryHoTen.Text))
        {
            HienLoi("Vui lòng nhập họ tên");
            return;
        }

        // Bug cũ: chế độ Đăng ký không kiểm tra trùng email và không thực sự lưu
        // tài khoản mới vào MockData, nên bấm "Tạo tài khoản" không đăng ký được gì cả.
        if (!_dangOChedoDangNhap && MockData.DanhSachKhachHang.Any(k =>
                string.Equals(k.Email, EntryEmail.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            HienLoi("Email này đã được đăng ký, vui lòng đăng nhập");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        BtnXacNhan.IsEnabled = false;

        try
        {
            // TODO: gọi ASP.NET Core Web API thật:
            //   POST /api/khach-hang/dang-nhap  { email, matKhau }
            //   POST /api/khach-hang/dang-ky    { hoTen, sdt, email, matKhau }
            // Tạm thời giả lập độ trễ mạng để hoàn thiện luồng giao diện.
            await Task.Delay(600);

            if (_dangOChedoDangNhap)
            {
                var taiKhoan = MockData.DanhSachKhachHang.FirstOrDefault(k =>
                    string.Equals(k.Email, EntryEmail.Text.Trim(), StringComparison.OrdinalIgnoreCase));

                if (taiKhoan == null || taiKhoan.MatKhau != EntryMatKhau.Text)
                {
                    HienLoi("Email hoặc mật khẩu không đúng");
                    return;
                }
                if (taiKhoan.TaiKhoanBiKhoa)
                {
                    HienLoi("Tài khoản của bạn đã bị khóa");
                    return;
                }
            }
            else
            {
                var khachHangMoi = new KhachHang
                {
                    Id = "kh" + (MockData.DanhSachKhachHang.Count + 1),
                    HoTen = EntryHoTen.Text.Trim(),
                    Email = EntryEmail.Text.Trim(),
                    MatKhau = EntryMatKhau.Text,
                    SoDienThoai = EntrySdt.Text?.Trim() ?? "",
                    NgayDangKy = DateTime.Now,
                    TongSoDon = 0,
                };
                MockData.DanhSachKhachHang.Add(khachHangMoi);
            }

            await AppShell.DangNhapThanhCongAsync();
        }
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
