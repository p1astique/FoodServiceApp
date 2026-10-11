using FoodServiceApp.Maui.Services;

namespace FoodServiceApp.Maui.Views;

public partial class AdminVendorApprovalPage : ContentPage
{
    private readonly VendorRegistrationApiClient _api = new();

    public AdminVendorApprovalPage() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await NapDanhSachAsync();
    }

    private async Task NapDanhSachAsync()
    {
        try
        {
            var data = await _api.GetPendingAsync();
            DsChoDuyet.ItemsSource = data;
            LblRong.IsVisible = data.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Không tải được dữ liệu",
                $"Không thể kết nối Web API.\n\n{ex}",
                "OK");
        }
    }

    private async void OnDuyetClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: AdminVendorDto vendor }) return;
        if (!await DisplayAlert("Duyệt hồ sơ", $"Duyệt gian hàng \"{vendor.TenCuaHang}\"?", "Duyệt", "Để sau")) return;
        if (!await _api.ApproveAsync(vendor.MaGianHang))
        {
            await DisplayAlert("Không thành công", "Server không thể duyệt hồ sơ này.", "OK");
            return;
        }
        await NapDanhSachAsync();
        await DisplayAlert("Đã duyệt", $"Gian hàng \"{vendor.TenCuaHang}\" đã được duyệt.", "OK");
    }

    private async void OnTuChoiClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: AdminVendorDto vendor }) return;
        if (!await DisplayAlert("Từ chối hồ sơ", $"Từ chối \"{vendor.TenCuaHang}\"?", "Từ chối", "Để sau")) return;
        if (!await _api.RejectAsync(vendor.MaGianHang))
        {
            await DisplayAlert("Không thành công", "Server không thể từ chối hồ sơ này.", "OK");
            return;
        }
        await NapDanhSachAsync();
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        try { await NapDanhSachAsync(); }
        finally { RefreshDsChoDuyet.IsRefreshing = false; }
    }
}