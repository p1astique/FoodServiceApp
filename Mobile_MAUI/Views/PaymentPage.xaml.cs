using FoodServiceApp.Maui.Models;
using System.Collections.ObjectModel;

namespace FoodServiceApp.Maui.Views;

public enum HinhThucThanhToan { TienMat, ChuyenKhoan, ViDienTu }

[QueryProperty(nameof(GioHang), "GioHang")]
[QueryProperty(nameof(TenGianHang), "TenGianHang")]
[QueryProperty(nameof(GianHangId), "GianHangId")]
public partial class PaymentPage : ContentPage
{
    public ObservableCollection<MucGioHang> GioHang { get; set; } = new();
    public string TenGianHang { get; set; } = "";
    public string GianHangId { get; set; } = "";

    private HinhThucThanhToan _hinhThucChon = HinhThucThanhToan.TienMat;
    private decimal _giamGia = 0;
    private const decimal PhiGiaoHang = 15000;

    private readonly (HinhThucThanhToan Loai, string Nhan, string Icon)[] _dsHinhThuc =
    {
        (HinhThucThanhToan.TienMat, "Tiền mặt khi nhận hàng", "💵"),
        (HinhThucThanhToan.ChuyenKhoan, "Chuyển khoản ngân hàng", "🏦"),
        (HinhThucThanhToan.ViDienTu, "Ví điện tử (Momo/ZaloPay)", "👛"),
    };

    public PaymentPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DsChiTiet.ItemsSource = GioHang;
        VeDanhSachHinhThuc();
        CapNhatTongTien();
    }

    private void VeDanhSachHinhThuc()
    {
        DanhSachHinhThuc.Children.Clear();
        foreach (var ht in _dsHinhThuc)
        {
            var chon = ht.Loai == _hinhThucChon;
            var row = new Grid { Padding = new Thickness(12, 10) };
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            row.Add(new Label { Text = ht.Icon, FontSize = 18, VerticalOptions = LayoutOptions.Center }, 0, 0);
            row.Add(new Label
            {
                Text = ht.Nhan,
                FontSize = 13.5,
                FontAttributes = chon ? FontAttributes.Bold : FontAttributes.None,
                VerticalOptions = LayoutOptions.Center
            }, 1, 0);
            row.Add(new Label
            {
                Text = chon ? "●" : "○",
                TextColor = chon ? (Color)Application.Current!.Resources["Primary"] : (Color)Application.Current!.Resources["BorderColor"],
                VerticalOptions = LayoutOptions.Center
            }, 2, 0);

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => { _hinhThucChon = ht.Loai; VeDanhSachHinhThuc(); };
            row.GestureRecognizers.Add(tap);

            DanhSachHinhThuc.Children.Add(row);
        }
    }

    private decimal TamTinh => GioHang.Sum(x => x.ThanhTien);
    private decimal TongCong => TamTinh + PhiGiaoHang - _giamGia;

    private void CapNhatTongTien()
    {
        LblTamTinh.Text = $"{TamTinh:N0}đ";
        LblTongCong.Text = $"{TongCong:N0}đ";
        RowGiamGia.IsVisible = _giamGia > 0;
        if (_giamGia > 0) LblGiamGia.Text = $"-{_giamGia:N0}đ";
        BtnXacNhanDatHang.Text = $"Xác nhận đặt hàng • {TongCong:N0}đ";
    }

    private async void OnApDungKhuyenMaiClicked(object sender, EventArgs e)
    {
        var maCode = EntryMaKhuyenMai.Text?.Trim();
        if (string.IsNullOrEmpty(maCode)) return;

        // TODO: gọi POST /api/khuyen-mai/ap-dung { ma_code, tam_tinh, gian_hang_id }
        await Task.Delay(300);

        var (thanhCong, soTienGiam, thongDiep) = MockData.ApDungMaKhuyenMai(maCode, GianHangId, TamTinh);
        _giamGia = thanhCong ? soTienGiam : 0;
        LblKetQuaKhuyenMai.Text = (thanhCong ? "✔ " : "✘ ") + thongDiep;
        LblKetQuaKhuyenMai.TextColor = (Color)Application.Current!.Resources[thanhCong ? "Success" : "Danger"];
        LblKetQuaKhuyenMai.IsVisible = true;
        CapNhatTongTien();
    }

    private async void OnXacNhanDatHangClicked(object sender, EventArgs e)
    {
        LoadingXacNhan.IsVisible = true;
        LoadingXacNhan.IsRunning = true;
        BtnXacNhanDatHang.IsEnabled = false;

        try
        {
            // TODO: gọi POST /api/don-hang { gian_hang_id, dia_chi_giao, ghi_chu,
            //   hinh_thuc_thanh_toan, ma_khuyen_mai, chi_tiet: [...] }
            // rồi POST /api/thanh-toan để xử lý thanh toán theo hình thức đã chọn.
            await Task.Delay(900);

            await Shell.Current.GoToAsync("//chinh/don-hang");
        }
        finally
        {
            LoadingXacNhan.IsVisible = false;
            LoadingXacNhan.IsRunning = false;
            BtnXacNhanDatHang.IsEnabled = true;
        }
    }
}
