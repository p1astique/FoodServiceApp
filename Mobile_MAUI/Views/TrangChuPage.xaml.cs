using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class TrangChuPage : ContentPage
{
    public TrangChuPage()
    {
        InitializeComponent();
        // TODO: thay bằng GET /api/gian-hang?q=&danh_muc= khi có API thật
        DsGianHang.ItemsSource = MockData.DanhSachGianHang;
    }

    private async void OnGianHangTapped(object sender, EventArgs e)
    {
        if (sender is Frame { GestureRecognizers: [TapGestureRecognizer { CommandParameter: GianHang gh }] })
        {
            await Shell.Current.GoToAsync(nameof(MenuCartPage), new Dictionary<string, object>
            {
                { "GianHang", gh }
            });
        }
    }

    private async void OnTaiKhoanTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage));
    }
}
