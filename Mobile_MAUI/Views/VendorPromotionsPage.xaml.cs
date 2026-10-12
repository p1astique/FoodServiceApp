using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;
namespace FoodServiceApp.Maui.Views;
public partial class VendorPromotionsPage : ContentPage
{
    private bool _binding;
    public VendorPromotionsPage() { InitializeComponent(); }
    protected override async void OnAppearing() { base.OnAppearing(); await Reload(); }
    private Task Reload() => ApiUi.Run(this, async () => { DsKhuyenMai.ItemsSource = null; await VendorWorkspace.LoadAsync(); Render(); });
    private void Render() { _binding = true; DsKhuyenMai.ItemsSource = VendorWorkspace.Promotions; LblRong.IsVisible = VendorWorkspace.Promotions.Count == 0; _binding = false; }
    private async void OnBatTatToggled(object? sender, ToggledEventArgs e) {
        if (_binding || IsBusy || sender is not Switch { BindingContext: KhuyenMai km } toggle) return;
        await ApiUi.Run(this, async () => {
            try { await ApiClient.SendAsync(HttpMethod.Put, $"api/vendor/promotions/{km.Id}/status", new { TrangThai = e.Value }); }
            catch { _binding = true; toggle.IsToggled = !e.Value; _binding = false; throw; }
            await VendorWorkspace.LoadAsync(); Render();
        });
    }
    private async void OnThemClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorPromotionEditPage));
    private async void OnSuaClicked(object sender, EventArgs e) {
        if (sender is Button { CommandParameter: KhuyenMai km })
            await Shell.Current.GoToAsync(nameof(VendorPromotionEditPage), new Dictionary<string, object> { ["KhuyenMaiId"] = km.Id });
    }
    private async void OnXoaClicked(object sender, EventArgs e) {
        if (sender is not Button { CommandParameter: KhuyenMai km } || !await DisplayAlert("Xóa khuyến mãi", $"Xóa {km.MaCode}?", "Xóa", "Hủy")) return;
        await ApiUi.Run(this, async () => { await ApiClient.SendAsync(HttpMethod.Delete, $"api/vendor/promotions/{km.Id}"); await VendorWorkspace.LoadAsync(); Render(); });
    }
    private async void OnRefreshing(object? sender, EventArgs e) { try { await Reload(); } finally { RefreshDsKm.IsRefreshing = false; } }
}
