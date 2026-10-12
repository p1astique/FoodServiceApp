using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;
namespace FoodServiceApp.Maui.Views;
public partial class VendorMenuPage : ContentPage
{
    private string? _category;
    private bool _binding;
    public VendorMenuPage() { InitializeComponent(); }
    protected override async void OnAppearing() { base.OnAppearing(); await Reload(); }
    private Task Reload() => ApiUi.Run(this, async () => { DsMonAn.ItemsSource = null; await VendorWorkspace.LoadAsync(); Render(); });
    private void Render()
    {
        _binding = true;
        try {
            KhungDanhMuc.Children.Clear(); AddFilter("Tất cả", null);
            foreach (var c in VendorWorkspace.Categories) AddFilter(c.TenDanhMuc, c.TenDanhMuc);
            var items = VendorWorkspace.Items.Where(x => _category == null || x.DanhMuc == _category).ToList();
            DsMonAn.ItemsSource = items; LblRong.IsVisible = items.Count == 0;
        } finally { _binding = false; }
    }
    private void AddFilter(string text, string? value)
    {
        var button = new Button { Text = text, CornerRadius = 18, HeightRequest = 36,
            BackgroundColor = value == _category ? Colors.LightBlue : Colors.White, TextColor = Colors.Black };
        button.Clicked += (_, _) => { _category = value; Render(); };
        KhungDanhMuc.Children.Add(button);
    }
    private async void OnQuanLyDanhMucClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet("Quản lý danh mục", "Đóng", null, "Thêm", "Đổi tên", "Xóa");
        if (action is not ("Thêm" or "Đổi tên" or "Xóa")) return;
        VendorWorkspace.Category? category = null;
        if (action != "Thêm") {
            var selected = await DisplayActionSheet("Chọn danh mục", "Hủy", null, VendorWorkspace.Categories.Select(x => x.TenDanhMuc).ToArray());
            category = VendorWorkspace.Categories.FirstOrDefault(x => x.TenDanhMuc == selected);
            if (category == null) return;
        }
        string? name = null;
        if (action != "Xóa") {
            name = await DisplayPromptAsync(action + " danh mục", "Tên danh mục", initialValue: category?.TenDanhMuc, maxLength: 100);
            if (string.IsNullOrWhiteSpace(name)) return;
        } else if (!await DisplayAlert("Xóa danh mục", "Danh mục phải không còn món. Tiếp tục?", "Xóa", "Hủy")) return;
        await ApiUi.Run(this, async () => {
            await ApiClient.SendAsync(action == "Xóa" ? HttpMethod.Delete : category == null ? HttpMethod.Post : HttpMethod.Put,
                "api/vendor/categories" + (category == null ? "" : $"/{category.MaDanhMuc}"),
                action == "Xóa" ? null : new { TenDanhMuc = name!.Trim() });
            _category = null; await VendorWorkspace.LoadAsync(); Render();
        });
    }
    private async void OnConHangToggled(object? sender, ToggledEventArgs e)
    {
        if (_binding || IsBusy || sender is not Switch { BindingContext: MonAn item } toggle) return;
        await ApiUi.Run(this, async () => {
            try { await ApiClient.SendAsync(HttpMethod.Put, $"api/vendor/items/{item.Id}/availability", new { ConHang = e.Value }); }
            catch { _binding = true; toggle.IsToggled = !e.Value; _binding = false; throw; }
            await VendorWorkspace.LoadAsync(); Render();
        });
    }
    private async void OnThemMonClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(VendorMenuItemEditPage));
    private async void OnSuaClicked(object sender, EventArgs e) {
        if (sender is Button { CommandParameter: MonAn item })
            await Shell.Current.GoToAsync(nameof(VendorMenuItemEditPage), new Dictionary<string, object> { ["MonAnId"] = item.Id });
    }
    private async void OnXoaClicked(object sender, EventArgs e) {
        if (sender is not Button { CommandParameter: MonAn item } || !await DisplayAlert("Xóa món", $"Xóa {item.TenMon}?", "Xóa", "Hủy")) return;
        await ApiUi.Run(this, async () => { await ApiClient.SendAsync(HttpMethod.Delete, $"api/vendor/items/{item.Id}"); await VendorWorkspace.LoadAsync(); Render(); });
    }
    private async void OnRefreshing(object? sender, EventArgs e) { try { await Reload(); } finally { RefreshThucDon.IsRefreshing = false; } }
}
