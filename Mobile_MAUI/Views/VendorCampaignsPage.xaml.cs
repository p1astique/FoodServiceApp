using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;
namespace FoodServiceApp.Maui.Views;
public partial class VendorCampaignsPage : ContentPage
{
    public VendorCampaignsPage() { InitializeComponent(); ViTri.SelectedIndex = 1; BatDau.Date = DateTime.Today; KetThuc.Date = DateTime.Today.AddDays(7); }
    protected override async void OnAppearing() { base.OnAppearing(); await Reload(); }
    private async Task Reload() => await ApiUi.Run(this, async () => { var data = await ApiClient.GetAsync<List<ChienDichQuangCao>>("api/vendor/campaigns"); DanhSach.ItemsSource=data; Rong.IsVisible=data.Count==0; });
    private async void OnCreate(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Ten.Text) || string.IsNullOrWhiteSpace(TieuDe.Text) || !decimal.TryParse(NganSach.Text, out var budget) || budget < 0 || KetThuc.Date <= BatDau.Date) { await DisplayAlert("Dữ liệu chưa đúng", "Nhập tên, tiêu đề, ngân sách hợp lệ và ngày kết thúc sau ngày bắt đầu.", "OK"); return; }
        await ApiUi.Run(this, async () => { await ApiClient.SendAsync(HttpMethod.Post, "api/vendor/campaigns", new { TenChienDich=Ten.Text, LoaiViTri=ViTri.SelectedItem?.ToString() ?? "NoiBat", TieuDe=TieuDe.Text, NoiDung=NoiDung.Text, HinhAnh=HinhAnh.Text, NgayBatDau=BatDau.Date, NgayKetThuc=KetThuc.Date.AddDays(1).AddSeconds(-1), NganSach=budget, KichHoat=KichHoat.IsToggled }); Ten.Text=TieuDe.Text=NoiDung.Text=HinhAnh.Text=NganSach.Text=""; await Reload(); });
    }
    private async void OnDelete(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: ChienDichQuangCao x } || !await DisplayAlert("Xóa chiến dịch", $"Xóa {x.TenChienDich}?", "Xóa", "Hủy")) return;
        await ApiUi.Run(this, async () => { await ApiClient.SendAsync(HttpMethod.Delete, $"api/vendor/campaigns/{x.MaChienDich}"); await Reload(); });
    }
}
