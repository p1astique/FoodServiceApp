using FoodServiceApp.Maui.Services;
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (GianHang is null) return;

        LblIconGianHang.Source = GianHang.ImageUrl;
        LblTenGianHang.Text = GianHang.TenGianHang;
        LblThongTinGianHang.Text = $"⭐ {GianHang.DanhGia:0.0}";

        await ApiUi.Run(this, async () => {
            DsMonAn.ItemsSource = null;
            var items = await ApiClient.GetAsync<List<MonAn>>($"api/catalog/stores/{GianHang.Id}/items", null);
            DsMonAn.ItemsSource = items;
            _gioHang.RemoveAll(x => items.All(m => m.Id != x.MonAn.Id));
            foreach (var line in _gioHang) line.MonAn = items.Single(x => x.Id == line.MonAn.Id);
            CapNhatThanhGioHang();
        });
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
