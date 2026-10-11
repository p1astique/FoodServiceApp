using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class AdminCustomerListPage : ContentPage
{
    public AdminCustomerListPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NapDanhSach();
    }

    private void NapDanhSach()
    {
        // TODO: thay bằng GET /api/quan-tri/khach-hang khi có API thật.
        DsKhachHang.ItemsSource = MockData.DanhSachKhachHang.OrderByDescending(k => k.NgayDangKy).ToList();
    }

    private async void OnKhoaMoClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: KhachHang kh }) return;

        if (kh.TaiKhoanBiKhoa)
        {
            kh.TaiKhoanBiKhoa = false;
            // TODO: gọi PUT /api/quan-tri/khach-hang/{id}/mo-khoa khi có API thật.
            NapDanhSach();
            await DisplayAlert("Đã mở khóa", $"Tài khoản \"{kh.HoTen}\" đã được kích hoạt lại.", "OK");
            return;
        }

        bool xacNhan = await DisplayAlert(
            "Khóa tài khoản khách hàng",
            $"Khóa tài khoản \"{kh.HoTen}\"? Khách hàng sẽ không thể đăng nhập cho đến khi được mở lại.",
            "Khóa", "Hủy");
        if (!xacNhan) return;

        kh.TaiKhoanBiKhoa = true;
        // TODO: gọi PUT /api/quan-tri/khach-hang/{id}/khoa khi có API thật.

        NapDanhSach();
    }
}
