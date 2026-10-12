using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;
namespace FoodServiceApp.Maui.Views;
public partial class VendorOrdersPage : ContentPage
{
    private TrangThaiDonHang? _filter;
    public VendorOrdersPage()
    {
        InitializeComponent();
        var all = new Button { Text = "Tất cả" }; all.Clicked += (_, _) => { _filter = null; Render(); }; KhungBoLoc.Children.Add(all);
        foreach (var status in Enum.GetValues<TrangThaiDonHang>()) {
            var button = new Button { Text = status.Nhan() };
            button.Clicked += (_, _) => { _filter = status; Render(); }; KhungBoLoc.Children.Add(button);
        }
    }
    protected override async void OnAppearing() { base.OnAppearing(); await Reload(); }
    private Task Reload() => ApiUi.Run(this, async () => { DsDonHang.ItemsSource = null; await VendorWorkspace.LoadAsync(); Render(); });
    private void Render() {
        var items = VendorWorkspace.Orders.Where(x => _filter == null || x.TrangThai == _filter).OrderByDescending(x => x.NgayDat).ToList();
        DsDonHang.ItemsSource = items; LblRong.IsVisible = items.Count == 0;
    }
    private async Task Update(object sender, string status) {
        if (sender is not Button { CommandParameter: DonHang order }) return;
        if (status == "DaHuy" && !await DisplayAlert("Hủy đơn", "Hủy đơn này? Khoản đã thanh toán cần quản trị đối soát hoàn tiền.", "Hủy đơn", "Quay lại")) return;
        await ApiUi.Run(this, async () => { await VendorWorkspace.UpdateOrder(order.Id, status); await VendorWorkspace.LoadAsync(); Render(); });
    }
    private async void OnXacNhanClicked(object sender, EventArgs e) => await Update(sender, "DaXacNhan");
    private async void OnTuChoiClicked(object sender, EventArgs e) => await Update(sender, "DaHuy");
    private async void OnSanSangGiaoClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: DonHang order }) return;
        await Update(sender, order.TrangThaiMayChu == "DaXacNhan" ? "DangChuanBi" : "SanSangGiao");
    }
    private async void OnRefreshing(object? sender, EventArgs e) { try { await Reload(); } finally { RefreshDsDon.IsRefreshing = false; } }
}
