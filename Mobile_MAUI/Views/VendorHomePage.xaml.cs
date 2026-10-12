using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;
namespace FoodServiceApp.Maui.Views;
public partial class VendorHomePage : ContentPage
{
    private bool _binding;
    public VendorHomePage() { InitializeComponent(); }
    protected override async void OnAppearing() { base.OnAppearing(); await Reload(); }
    private Task Reload() => ApiUi.Run(this, async () => { DsDonMoi.ItemsSource = null; await VendorWorkspace.LoadAsync(); Render(); });
    private void Render()
    {
        var g = VendorWorkspace.Store;
        LblAnhGianHang.Source = g.ImageUrl; LblTenGianHang.Text = g.TenGianHang; LblDiaChiGianHang.Text = g.DiaChi;
        _binding = true; SwitchMoCua.IsToggled = g.DangMoCua; _binding = false;
        LblTrangThaiMoCua.Text = g.DangMoCua ? "Đang mở bán" : "Đang tạm đóng";
        var done = VendorWorkspace.Orders.Where(x => x.TrangThai == TrangThaiDonHang.HoanThanh && x.NgayDat.Date == DateTime.Today).ToList();
        LblDoanhThuHomNay.Text = $"{done.Sum(x => x.TongTien):N0}đ"; LblSoDonHomNay.Text = done.Count.ToString();
        var waiting = VendorWorkspace.Orders.Where(x => x.TrangThai == TrangThaiDonHang.ChoXacNhan).ToList();
        LblSoDonChoXacNhan.Text = $"{waiting.Count} đơn chờ xác nhận";
        LblSoMonAn.Text = $"{VendorWorkspace.Items.Count(x => x.ConHang)} món đang bán";
        DsDonMoi.ItemsSource = waiting; LblKhongCoDonMoi.IsVisible = waiting.Count == 0;
    }
    private async void OnSwitchMoCuaToggled(object? sender, ToggledEventArgs e)
    {
        if (_binding || IsBusy) return;
        await ApiUi.Run(this, async () => {
            try {
                await ApiClient.SendAsync(HttpMethod.Put, "api/vendor/business-status", new { DangMoCua = e.Value });
                await VendorWorkspace.LoadAsync(); Render();
            } catch { _binding = true; SwitchMoCua.IsToggled = !e.Value; _binding = false; throw; }
        });
    }
    private async Task Update(object sender, string status)
    {
        if (sender is not Button { CommandParameter: DonHang order }) return;
        if (status == "DaHuy" && !await DisplayAlert("Hủy đơn", "Hủy đơn này? Khoản đã thanh toán cần quản trị đối soát hoàn tiền.", "Hủy đơn", "Quay lại")) return;
        await ApiUi.Run(this, async () => { await VendorWorkspace.UpdateOrder(order.Id, status); await VendorWorkspace.LoadAsync(); Render(); });
    }
    private async void OnXacNhanNhanhClicked(object sender, EventArgs e) => await Update(sender, "DaXacNhan");
    private async void OnTuChoiNhanhClicked(object sender, EventArgs e) => await Update(sender, "DaHuy");
    private async void OnXemDonHangTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorOrdersPage));
    private async void OnXemThucDonTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorMenuPage));
    private async void OnXemDoanhThuTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorRevenuePage));
    private async void OnXemHoSoTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorProfilePage));
    private async void OnXemKhuyenMaiTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorPromotionsPage));
    private async void OnXemChienDichTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorCampaignsPage));
    private async void OnXemCauHinhTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorSettingsPage));
    private async void OnRefreshing(object? sender, EventArgs e) { try { await Reload(); } finally { RefreshTrangChu.IsRefreshing = false; } }
    private async void OnDangXuatClicked(object sender, EventArgs e)
    {
        ApiClient.Logout("GianHang"); VendorWorkspace.Clear(); await Shell.Current.GoToAsync("//dang-nhap");
    }
}
