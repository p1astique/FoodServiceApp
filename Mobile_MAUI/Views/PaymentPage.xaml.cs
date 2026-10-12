using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;
using System.Collections.ObjectModel;
namespace FoodServiceApp.Maui.Views;
[QueryProperty(nameof(GioHang), "GioHang")]
[QueryProperty(nameof(TenGianHang), "TenGianHang")]
[QueryProperty(nameof(GianHangId), "GianHangId")]
public partial class PaymentPage : ContentPage
{
    public ObservableCollection<MucGioHang> GioHang { get; set; } = new();
    public string TenGianHang { get; set; } = "";
    public string GianHangId { get; set; } = "";
    private bool _submitted;
    public PaymentPage() { InitializeComponent(); }
    protected override void OnAppearing()
    {
        base.OnAppearing(); DsChiTiet.ItemsSource = GioHang;
        DanhSachHinhThuc.Children.Clear(); DanhSachHinhThuc.Children.Add(new Label { Text = "Tiền mặt khi nhận hàng", Padding = 12 });
        LblTamTinh.Text = $"{GioHang.Sum(x => x.ThanhTien):N0}đ";
        LblPhiGiao.Text = "Tính khi xác nhận"; LblTongCong.Text = "Chưa tính phí và khuyến mãi";
        BtnXacNhanDatHang.Text = "Tính tiền và xác nhận đặt hàng";
        BtnXacNhanDatHang.IsEnabled = !_submitted;
    }
    private object Request(decimal? total = null) => new {
        MaGianHang = int.Parse(GianHangId), DiaChiGiaoHang = EntryDiaChi.Text?.Trim(), GhiChu = EditorGhiChu.Text,
        MaKhuyenMai = EntryMaKhuyenMai.Text?.Trim(), TongTienXacNhan = total,
        Items = GioHang.Select(x => new { MaMonAn = int.Parse(x.MonAn.Id), x.SoLuong }).ToArray()
    };
    private async Task<Quote> Calculate()
    {
        var quote = await ApiClient.SendAsync<Quote>(HttpMethod.Post, "api/customer/quote", Request(), "KhachHang");
        LblTamTinh.Text = $"{quote.TamTinh:N0}đ"; LblPhiGiao.Text = $"{quote.PhiGiaoHang:N0}đ";
        LblTongCong.Text = $"{quote.TongTien:N0}đ"; RowGiamGia.IsVisible = quote.GiamGia > 0;
        LblGiamGia.Text = $"-{quote.GiamGia:N0}đ";
        return quote;
    }
    private async void OnApDungKhuyenMaiClicked(object sender, EventArgs e) => await ApiUi.Run(this, async () => {
        LblKetQuaKhuyenMai.IsVisible = false; await Calculate();
        LblKetQuaKhuyenMai.Text = "Đã tính lại theo giá và khuyến mãi hiện tại."; LblKetQuaKhuyenMai.IsVisible = true;
    });
    private async void OnXacNhanDatHangClicked(object sender, EventArgs e)
    {
        if (_submitted) return;
        await ApiUi.Run(this, async () => {
            var quote = await Calculate();
            if (!await DisplayAlert("Xác nhận đặt hàng", $"Tổng tiền {quote.TongTien:N0}đ, thanh toán khi nhận hàng. Tiếp tục?", "Đặt hàng", "Quay lại")) return;
            await ApiClient.SendAsync(HttpMethod.Post, "api/customer/orders", Request(quote.TongTien), "KhachHang");
            _submitted = true; GioHang.Clear();
            await Shell.Current.GoToAsync("//chinh/don-hang");
        });
    }
    private record Quote(decimal TamTinh, decimal GiamGia, decimal PhiGiaoHang, decimal TongTien);
}
