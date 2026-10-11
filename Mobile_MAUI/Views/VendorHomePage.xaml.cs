using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;

namespace FoodServiceApp.Maui.Views;

public partial class VendorHomePage : ContentPage
{
    public VendorHomePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NapHoSo();
        NapThongKeNhanh();
        NapDonMoi();
    }

    private void NapHoSo()
    {
        var gh = MockData.GianHangHienTai;
        LblAnhGianHang.Text = gh.HinhAnh;
        LblTenGianHang.Text = gh.TenGianHang;
        LblDiaChiGianHang.Text = gh.DiaChi;

        SwitchMoCua.Toggled -= OnSwitchMoCuaToggled;
        SwitchMoCua.IsToggled = gh.DangMoCua;
        SwitchMoCua.Toggled += OnSwitchMoCuaToggled;
        LblTrangThaiMoCua.Text = gh.DangMoCua ? "Đang mở bán" : "Đang tạm đóng";
    }

    private void NapThongKeNhanh()
    {
        var donCuaToi = MockData.DonHangCuaGianHang.Where(d => d.GianHangId == MockData.GianHangHienTai.Id);

        var hoanThanhHomNay = donCuaToi.Where(d => d.TrangThai == TrangThaiDonHang.HoanThanh && d.NgayDat.Date == DateTime.Now.Date).ToList();
        LblDoanhThuHomNay.Text = $"{hoanThanhHomNay.Sum(d => d.TongTien):N0}đ";
        LblSoDonHomNay.Text = hoanThanhHomNay.Count.ToString();

        var soChoXacNhan = donCuaToi.Count(d => d.TrangThai == TrangThaiDonHang.ChoXacNhan);
        LblSoDonChoXacNhan.Text = soChoXacNhan > 0 ? $"{soChoXacNhan} đơn chờ xác nhận" : "Không có đơn chờ";

        LblSoMonAn.Text = $"{MockData.MonAnCuaGianHang(MockData.GianHangHienTai.Id).Count} món đang bán";
    }

    private void NapDonMoi()
    {
        var donMoi = MockData.DonHangCuaGianHang
            .Where(d => d.GianHangId == MockData.GianHangHienTai.Id && d.TrangThai == TrangThaiDonHang.ChoXacNhan)
            .OrderBy(d => d.NgayDat)
            .ToList();

        DsDonMoi.ItemsSource = donMoi;
        LblKhongCoDonMoi.IsVisible = donMoi.Count == 0;
    }

    private void OnSwitchMoCuaToggled(object? sender, ToggledEventArgs e)
    {
        MockData.GianHangHienTai.DangMoCua = e.Value;
        VendorLocalStore.Luu();
        LblTrangThaiMoCua.Text = e.Value ? "Đang mở bán" : "Đang tạm đóng";
        // TODO: gọi PUT /api/gian-hang/{id}/trang-thai-kinh-doanh { dangMoCua } khi có API thật.
    }

    private async void OnXacNhanNhanhClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: DonHang don }) return;

        don.TrangThai = TrangThaiDonHang.DangLam;
        VendorLocalStore.Luu();
        // TODO: gọi PUT /api/gian-hang/don-hang/{id}/xac-nhan khi có API thật.

        NapThongKeNhanh();
        NapDonMoi();
        await DisplayAlert("Đã xác nhận", $"Đơn #{don.Id} đã được xác nhận và chuyển sang chuẩn bị món.", "OK");
    }

    private async void OnTuChoiNhanhClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: DonHang don }) return;

        bool xacNhan = await DisplayAlert(
            "Từ chối đơn hàng",
            $"Từ chối đơn #{don.Id}? Khách hàng sẽ được hoàn tiền tự động theo quy định.",
            "Từ chối", "Để sau");
        if (!xacNhan) return;

        don.TrangThai = TrangThaiDonHang.DaHuy;
        VendorLocalStore.Luu();
        // TODO: gọi PUT /api/gian-hang/don-hang/{id}/tu-choi khi có API thật (kèm xử lý hoàn tiền).

        NapThongKeNhanh();
        NapDonMoi();
    }

    private async void OnXemDonHangTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorOrdersPage));
    private async void OnXemThucDonTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorMenuPage));
    private async void OnXemDoanhThuTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorRevenuePage));
    private async void OnXemHoSoTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorProfilePage));
    private async void OnXemKhuyenMaiTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorPromotionsPage));

    private void OnRefreshing(object? sender, EventArgs e)
    {
        NapHoSo();
        NapThongKeNhanh();
        NapDonMoi();
        RefreshTrangChu.IsRefreshing = false;
    }

    private async void OnDangXuatClicked(object sender, EventArgs e)
    {
        bool xacNhan = await DisplayAlert("Đăng xuất", "Bạn có chắc muốn đăng xuất khỏi gian hàng?", "Đăng xuất", "Hủy");
        if (!xacNhan) return;

        MockData.GianHangHienTai = MockData.DanhSachGianHang.First(g => g.Id == "gh1");
        await Shell.Current.GoToAsync("//dang-nhap");
    }
}
