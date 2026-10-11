using FoodServiceApp.Maui.Services;

namespace FoodServiceApp.Maui.Views;

public partial class AdminVendorListPage : ContentPage
{
    private readonly VendorRegistrationApiClient _api = new();

    public AdminVendorListPage() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await NapDanhSachAsync();
    }

    private async Task NapDanhSachAsync()
    {
        try
        {
            DsGianHang.ItemsSource = await _api.GetApprovedAsync();
        }
        catch (Exception ex)
        {
            DsGianHang.ItemsSource = null;
            await DisplayAlert("Không tải được dữ liệu", $"Không thể kết nối Web API.\n\n{ex.Message}", "OK");
        }
    }

    private async void OnKhoaMoClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: AdminVendorDto vendor }) return;

        var action = vendor.TaiKhoanHoatDong ? "khóa" : "mở khóa";
        if (!await DisplayAlert("Xác nhận", $"Bạn muốn {action} tài khoản \"{vendor.TenCuaHang}\"?", "Đồng ý", "Hủy")) return;

        if (!await _api.ToggleLockAsync(vendor.MaGianHang))
        {
            await DisplayAlert("Không thành công", "Server không thể cập nhật tài khoản.", "OK");
            return;
        }

        await NapDanhSachAsync();
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        try { await NapDanhSachAsync(); }
        finally { RefreshDsGianHang.IsRefreshing = false; }
    }
}