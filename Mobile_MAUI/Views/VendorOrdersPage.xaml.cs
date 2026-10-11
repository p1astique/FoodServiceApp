using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;

namespace FoodServiceApp.Maui.Views;

public partial class VendorOrdersPage : ContentPage
{
    // null = "Tất cả"
    private readonly (string Nhan, TrangThaiDonHang? TrangThai)[] _boLoc =
    {
        ("Tất cả", null),
        ("Chờ xác nhận", TrangThaiDonHang.ChoXacNhan),
        ("Đang chuẩn bị", TrangThaiDonHang.DangLam),
        ("Đang giao", TrangThaiDonHang.DangGiao),
        ("Hoàn thành", TrangThaiDonHang.HoanThanh),
        ("Đã hủy", TrangThaiDonHang.DaHuy),
    };

    private TrangThaiDonHang? _dangLoc;
    private readonly List<Button> _nutBoLoc = new();

    public VendorOrdersPage()
    {
        InitializeComponent();
        DungBoLoc();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NapDanhSach();
    }

    private void DungBoLoc()
    {
        foreach (var (nhan, trangThai) in _boLoc)
        {
            var nut = new Button
            {
                Text = nhan,
                CornerRadius = 16,
                HeightRequest = 34,
                Padding = new Thickness(14, 0),
                FontSize = 12,
                BackgroundColor = trangThai is null ? (Color)Application.Current!.Resources["Primary"] : Colors.White,
                TextColor = trangThai is null ? Colors.White : (Color)Application.Current!.Resources["TextGrey"],
                BorderColor = (Color)Application.Current!.Resources["BorderColor"],
                BorderWidth = 1,
            };
            nut.Clicked += (_, _) => LocTheoTrangThai(trangThai, nut);
            _nutBoLoc.Add(nut);
            KhungBoLoc.Children.Add(nut);
        }
    }

    private void LocTheoTrangThai(TrangThaiDonHang? trangThai, Button nutDuocChon)
    {
        _dangLoc = trangThai;

        foreach (var nut in _nutBoLoc)
        {
            bool laDuocChon = nut == nutDuocChon;
            nut.BackgroundColor = laDuocChon ? (Color)Application.Current!.Resources["Primary"] : Colors.White;
            nut.TextColor = laDuocChon ? Colors.White : (Color)Application.Current!.Resources["TextGrey"];
        }

        NapDanhSach();
    }

    private void NapDanhSach()
    {
        // TODO: thay bằng GET /api/gian-hang/don-hang?trangThai=... khi có API thật.
        var danhSach = MockData.DonHangCuaGianHang
            .Where(d => d.GianHangId == MockData.GianHangHienTai.Id)
            .Where(d => _dangLoc is null || d.TrangThai == _dangLoc)
            .OrderByDescending(d => d.NgayDat)
            .ToList();

        DsDonHang.ItemsSource = danhSach;
        LblRong.IsVisible = danhSach.Count == 0;
    }

    private async void OnXacNhanClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: DonHang don }) return;

        don.TrangThai = TrangThaiDonHang.DangLam;
        VendorLocalStore.Luu();
        // TODO: gọi PUT /api/gian-hang/don-hang/{id}/xac-nhan khi có API thật.

        NapDanhSach();
        await DisplayAlert("Đã xác nhận", $"Đơn #{don.Id} đã được xác nhận, hãy bắt đầu chuẩn bị món.", "OK");
    }

    private async void OnTuChoiClicked(object sender, EventArgs e)
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

        NapDanhSach();
    }

    private async void OnSanSangGiaoClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: DonHang don }) return;

        don.TrangThai = TrangThaiDonHang.DangGiao;
        VendorLocalStore.Luu();
        // TODO: gọi PUT /api/gian-hang/don-hang/{id}/san-sang-giao khi có API thật,
        // backend sẽ đẩy đơn này vào hàng đợi cho tài xế nhận (xem DriverHomePage).

        NapDanhSach();
        await DisplayAlert("Đã bàn giao", $"Đơn #{don.Id} đã sẵn sàng, đang chờ tài xế đến lấy.", "OK");
    }

    private void OnRefreshing(object? sender, EventArgs e)
    {
        NapDanhSach();
        RefreshDsDon.IsRefreshing = false;
    }
}
