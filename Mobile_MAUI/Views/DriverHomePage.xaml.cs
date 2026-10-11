using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class DriverHomePage : ContentPage
{
    public DriverHomePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NapHoSo();
        CapNhatDonDangGiao();
        NapDanhSachDonChoNhan();
    }

    private void NapHoSo()
    {
        var taiXe = MockData.TaiXeHienTai;
        LblAnhTaiXe.Text = taiXe.Anh;
        LblTenTaiXe.Text = taiXe.HoTen;
        LblBienSo.Text = taiXe.BienSoXe;
        LblThuNhapHomNay.Text = $"{taiXe.TongThuNhapHomNay:N0}đ";
        LblSoDonHomNay.Text = taiXe.SoDonHomNay.ToString();

        SwitchHoatDong.Toggled -= OnSwitchHoatDongToggled;
        SwitchHoatDong.IsToggled = taiXe.DangHoatDong;
        SwitchHoatDong.Toggled += OnSwitchHoatDongToggled;
        LblTrangThaiHoatDong.Text = taiXe.DangHoatDong ? "Đang bật" : "Đang tắt";
    }

    private void OnSwitchHoatDongToggled(object? sender, ToggledEventArgs e)
    {
        MockData.TaiXeHienTai.DangHoatDong = e.Value;
        LblTrangThaiHoatDong.Text = e.Value ? "Đang bật" : "Đang tắt";
        NapDanhSachDonChoNhan();
    }

    private void CapNhatDonDangGiao()
    {
        var don = MockData.DonHangDangGiaoCuaTaiXe;
        if (don is null)
        {
            KhungDonDangGiao.IsVisible = false;
            return;
        }

        KhungDonDangGiao.IsVisible = true;
        LblMaDonDangGiao.Text = $"#{don.Id}";
        LblGianHangDangGiao.Text = don.TenGianHang;
        LblDiaChiGiaoDangGiao.Text = $"🏁 {don.DiaChiGiao}";
    }

    private void NapDanhSachDonChoNhan()
    {
        // TODO: thay bằng GET /api/tai-xe/don-cho-nhan khi có API thật.
        var dangHoatDong = MockData.TaiXeHienTai.DangHoatDong;
        var daNhanDon = MockData.DonHangDangGiaoCuaTaiXe is not null;

        DsDonChoNhan.ItemsSource = (dangHoatDong && !daNhanDon)
            ? MockData.DonHangChoTaiXe.Where(d => d.TaiXeId is null).ToList()
            : new List<DonHang>();

        LblTatCa.IsVisible = !dangHoatDong;
        LblTatCa.Text = !dangHoatDong
            ? "Bật trạng thái hoạt động để xem đơn hàng gần bạn."
            : daNhanDon
                ? "Hoàn thành đơn đang giao để nhận đơn tiếp theo."
                : "";
        if (dangHoatDong && daNhanDon)
            LblTatCa.IsVisible = true;
    }

    private async void OnNhanDonClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: DonHang don }) return;

        if (MockData.DonHangDangGiaoCuaTaiXe is not null)
        {
            await DisplayAlert("Không thể nhận đơn", "Bạn cần hoàn thành đơn đang giao trước.", "Đóng");
            return;
        }

        bool xacNhan = await DisplayAlert(
            "Nhận đơn giao hàng",
            $"Nhận đơn #{don.Id} từ {don.TenGianHang}?\nPhí giao: {don.PhiGiaoHang:N0}đ",
            "Nhận đơn", "Để sau");
        if (!xacNhan) return;

        don.TaiXeId = MockData.TaiXeHienTai.Id;
        MockData.DonHangDangGiaoCuaTaiXe = don;

        CapNhatDonDangGiao();
        NapDanhSachDonChoNhan();

        await Shell.Current.GoToAsync(nameof(DriverDeliveryDetailPage));
    }

    private async void OnXemDonDangGiaoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(DriverDeliveryDetailPage));
    }

    private void OnRefreshing(object? sender, EventArgs e)
    {
        NapHoSo();
        CapNhatDonDangGiao();
        NapDanhSachDonChoNhan();
        RefreshDanhSach.IsRefreshing = false;
    }

    private async void OnDangXuatClicked(object sender, EventArgs e)
    {
        bool xacNhan = await DisplayAlert("Đăng xuất", "Bạn có chắc muốn đăng xuất?", "Đăng xuất", "Hủy");
        if (!xacNhan) return;

        await Shell.Current.GoToAsync("//dang-nhap");
    }
}
