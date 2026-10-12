using FoodServiceApp.Maui.Services;
using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class TrangChuPage : ContentPage
{
    public TrangChuPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ApiUi.Run(this, async () => { DsGianHang.ItemsSource = null; DsGianHang.ItemsSource = await ApiClient.GetAsync<List<GianHang>>("api/catalog/stores", null); });
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
