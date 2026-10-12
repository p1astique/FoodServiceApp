using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;
namespace FoodServiceApp.Maui.Views;
public partial class VendorProfilePage : ContentPage
{
    public VendorProfilePage() { InitializeComponent(); }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ApiUi.Run(this, async () => {
            await VendorWorkspace.LoadAsync(); var g = VendorWorkspace.Store;
            EntryHinhAnh.Text = g.HinhAnh; LblAnhHienTai.Source = g.ImageUrl;
            EntryTenGianHang.Text = g.TenGianHang; EntryDiaChi.Text = g.DiaChi;
            EntrySdt.Text = g.SoDienThoai; EntryEmail.Text = g.Email;
            EditorMoTa.Text = g.MoTa; SwitchDangMoCua.IsToggled = g.DangMoCua;
            LblThanhCong.IsVisible = false; LblLoi.IsVisible = false;
        });
    }
    private async void OnLuuClicked(object sender, EventArgs e)
    {
        LblThanhCong.IsVisible = false;
        await ApiUi.Run(this, async () => {
            if (string.IsNullOrWhiteSpace(EntryTenGianHang.Text) || string.IsNullOrWhiteSpace(EntryDiaChi.Text) || string.IsNullOrWhiteSpace(EntrySdt.Text))
                throw new InvalidOperationException("Nhập tên, địa chỉ và số điện thoại.");
            await VendorWorkspace.SaveProfile(new GianHang { TenGianHang = EntryTenGianHang.Text.Trim(),
                DiaChi = EntryDiaChi.Text.Trim(), SoDienThoai = EntrySdt.Text.Trim(), MoTa = EditorMoTa.Text,
                HinhAnh = EntryHinhAnh.Text, DangMoCua = SwitchDangMoCua.IsToggled });
            LblThanhCong.IsVisible = true;
        });
    }
}
