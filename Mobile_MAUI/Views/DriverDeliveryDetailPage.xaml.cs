using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class DriverDeliveryDetailPage : ContentPage
{
    private DonHang? _don;

    public DriverDeliveryDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _don = MockData.DonHangDangGiaoCuaTaiXe;

        if (_don is null)
        {
            // Không còn đơn đang giao (đã hoàn tất) -> quay lại trang chủ tài xế.
            Shell.Current.GoToAsync("..");
            return;
        }

        LblMaDon.Text = $"Đơn #{_don.Id}";
        LblPhiGiao.Text = $"+{_don.PhiGiaoHang:N0}đ";
        LblGianHang.Text = _don.TenGianHang;
        LblDiaChiLay.Text = _don.DiaChiLayHang;
        LblDiaChiGiao.Text = _don.DiaChiGiao;
        DsMon.ItemsSource = _don.ChiTiet;

        CapNhatTheoTrangThai();
    }

    private void CapNhatTheoTrangThai()
    {
        if (_don is null) return;

        if (_don.TrangThai == TrangThaiDonHang.DangLam)
        {
            LblTrangThaiHienTai.Text = "🍳 Quán đang chuẩn bị món — đến lấy hàng khi sẵn sàng";
            BtnHanhDongChinh.Text = "Đã lấy hàng, bắt đầu giao";
        }
        else
        {
            LblTrangThaiHienTai.Text = "🛵 Đang trên đường giao đến khách hàng";
            BtnHanhDongChinh.Text = "Đã giao thành công";
        }
    }

    private async void OnHanhDongChinhClicked(object sender, EventArgs e)
    {
        if (_don is null) return;

        if (_don.TrangThai == TrangThaiDonHang.DangLam)
        {
            _don.TrangThai = TrangThaiDonHang.DangGiao;
            CapNhatTheoTrangThai();
            return;
        }

        // Hoàn tất giao hàng.
        _don.TrangThai = TrangThaiDonHang.HoanThanh;

        var taiXe = MockData.TaiXeHienTai;
        taiXe.TongThuNhapHomNay += _don.PhiGiaoHang;
        taiXe.SoDonHomNay += 1;

        MockData.DonHangChoTaiXe.Remove(_don);
        MockData.DonHangDaGiaoCuaTaiXe.Add(_don);
        MockData.DonHangDangGiaoCuaTaiXe = null;

        await DisplayAlert("Hoàn tất", $"Đã giao đơn #{_don.Id} thành công. Bạn nhận được {_don.PhiGiaoHang:N0}đ.", "OK");

        // TODO: gọi PUT /api/tai-xe/don-hang/{id}/hoan-thanh khi có API thật.
        await Shell.Current.GoToAsync("..");
    }

    private async void OnGoiKhachClicked(object sender, EventArgs e)
    {
        // TODO: nối vào tính năng gọi điện thật (PhoneDialer) khi có số điện thoại khách hàng từ API.
        await DisplayAlert("Gọi khách hàng", "Chức năng gọi điện sẽ được bổ sung khi nối API thật.", "Đóng");
    }
}
