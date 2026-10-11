using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class AdminIssuesPage : ContentPage
{
    public AdminIssuesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NapDanhSach();
    }

    private void NapDanhSach()
    {
        // TODO: thay bằng GET /api/quan-tri/van-de khi có API thật.
        DsVanDe.ItemsSource = MockData.DanhSachVanDe
            .OrderBy(v => v.TrangThai == TrangThaiVanDe.DaGiaiQuyet)
            .ThenByDescending(v => v.NgayBaoCao)
            .ToList();
    }

    private void OnDangXuLyClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: VanDeHeThong vanDe }) return;

        vanDe.TrangThai = TrangThaiVanDe.DangXuLy;
        // TODO: gọi PUT /api/quan-tri/van-de/{id}/dang-xu-ly khi có API thật.

        NapDanhSach();
    }

    private async void OnGiaiQuyetClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: VanDeHeThong vanDe }) return;

        vanDe.TrangThai = TrangThaiVanDe.DaGiaiQuyet;
        // TODO: gọi PUT /api/quan-tri/van-de/{id}/giai-quyet khi có API thật.

        NapDanhSach();
        await DisplayAlert("Đã cập nhật", $"Vấn đề \"{vanDe.TieuDe}\" đã được đánh dấu giải quyết xong.", "OK");
    }
}
