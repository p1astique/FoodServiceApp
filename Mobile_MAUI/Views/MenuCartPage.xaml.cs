using FoodServiceApp.Maui.Models;
using System.Collections.ObjectModel;

namespace FoodServiceApp.Maui.Views;

[QueryProperty(nameof(GianHang), "GianHang")]
public partial class MenuCartPage : ContentPage
{
    private readonly List<MucGioHang> _gioHang = new();

    public GianHang GianHang { get; set; } = null!;

    public MenuCartPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (GianHang is null) return;

        LblIconGianHang.Text = GianHang.HinhAnh;
        LblTenGianHang.Text = GianHang.TenGianHang;
        LblThongTinGianHang.Text = $"⭐ {GianHang.DanhGia}   {GianHang.ThoiGianGiaoPhut} phút giao";

        // TODO: thay bằng GET /api/gian-hang/{id}/mon-an khi có API thật
        DsMonAn.ItemsSource = MockData.DanhSachMonAn
            .Where(m => m.GianHangId == GianHang.Id)
            .ToList();
    }

    private void OnThemMonClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: MonAn mon }) return;

        var dong = _gioHang.FirstOrDefault(x => x.MonAn.Id == mon.Id);
        if (dong is null)
            _gioHang.Add(new MucGioHang { MonAn = mon, SoLuong = 1 });
        else
            dong.SoLuong++;

        CapNhatThanhGioHang();
    }

    private void CapNhatThanhGioHang()
    {
        if (_gioHang.Count == 0)
        {
            BtnXemGioHang.IsVisible = false;
            return;
        }

        var tongSoLuong = _gioHang.Sum(x => x.SoLuong);
        var tongTien = _gioHang.Sum(x => x.ThanhTien);
        BtnXemGioHang.Text = $"{tongSoLuong} món   •   Xem giỏ hàng   •   {tongTien:N0}đ";
        BtnXemGioHang.IsVisible = true;
    }

    private async void OnXemGioHangClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PaymentPage), new Dictionary<string, object>
        {
            { "GioHang", new ObservableCollection<MucGioHang>(_gioHang) },
            { "TenGianHang", GianHang.TenGianHang },
            { "GianHangId", GianHang.Id },
        });
    }
}
