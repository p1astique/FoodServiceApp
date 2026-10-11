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
        if (MockData.DanhSachGianHang.Any(g => string.Equals(g.Email, EntryEmail.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            HienLoi("Email này đã được đăng ký cho một gian hàng khác");
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
            // TODO: gọi ASP.NET Core Web API thật:
            //   POST /api/gian-hang/dang-ky { tenGianHang, diaChi, sdt, email, matKhau, moTa }
            // Hồ sơ mặc định ở trạng thái ChoDuyet, quản trị viên duyệt trong module Quản lý quản trị.
            await Task.Delay(600);

            var gianHangMoi = new GianHang
            {
                Id = "gh" + (MockData.DanhSachGianHang.Count + 1),
                TenGianHang = EntryTenGianHang.Text.Trim(),
                DiaChi = EntryDiaChi.Text.Trim(),
                SoDienThoai = EntrySdt.Text.Trim(),
                Email = EntryEmail.Text.Trim(),
                MatKhau = EntryMatKhau.Text,
                MoTa = EditorMoTa.Text?.Trim() ?? "",
                HinhAnh = "🍽️",
                DanhGia = 0,
                ThoiGianGiaoPhut = 30,
                DangMoCua = false,
                TrangThaiDangKy = TrangThaiDangKyGianHang.ChoDuyet,
                NgayDangKy = DateTime.Now,
            };
            MockData.DanhSachGianHang.Add(gianHangMoi);

            await DisplayAlert(
                "Đăng ký thành công",
                "Hồ sơ gian hàng đã được gửi. Quản trị viên sẽ duyệt trong thời gian sớm nhất, bạn sẽ nhận thông báo qua email khi được kích hoạt.",
                "Đã hiểu");

            await Shell.Current.GoToAsync("..");
        }
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
