using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class OrderTrackingPage : ContentPage
{
    public OrderTrackingPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        TaiDuLieu();
    }

    private void TaiDuLieu()
    {
        // TODO: thay bằng GET /api/don-hang?trang_thai= khi có API thật
        var donDangXuLy = MockData.LichSuDonHang.FirstOrDefault(d =>
            d.TrangThai is TrangThaiDonHang.ChoXacNhan or TrangThaiDonHang.DangLam or TrangThaiDonHang.DangGiao);

        if (donDangXuLy is not null)
        {
            KhungDangGiao.IsVisible = true;
            LblMaDonDangGiao.Text = $"Đơn #{donDangXuLy.Id}";
            LblTrangThaiDangGiao.Text = donDangXuLy.TrangThai.Nhan();
            LblGianHangDangGiao.Text = donDangXuLy.TenGianHang;
            VeThanhTienTrinh(donDangXuLy.TrangThai.Buoc());
        }
        else
        {
            KhungDangGiao.IsVisible = false;
        }

        DsLichSu.ItemsSource = MockData.LichSuDonHang
            .Where(d => d != donDangXuLy)
            .ToList();
    }

    private void VeThanhTienTrinh(int buocHienTai)
    {
        var boxes = new[] { Buoc0, Buoc1, Buoc2, Buoc3 };
        var mauXong = (Color)Application.Current!.Resources["Accent"];
        var mauChua = Color.FromArgb("#40FFFFFF");
        for (int i = 0; i < boxes.Length; i++)
            boxes[i].BackgroundColor = i <= buocHienTai ? mauXong : mauChua;
    }

    private async void OnDonHoanThanhTapped(object sender, EventArgs e)
    {
        if (sender is not Frame { GestureRecognizers: [TapGestureRecognizer { CommandParameter: DonHang don }] })
            return;

        if (don.TrangThai != TrangThaiDonHang.HoanThanh) return;

        await Shell.Current.GoToAsync(nameof(ReviewPage), new Dictionary<string, object>
        {
            { "DonHang", don }
        });
    }
}
