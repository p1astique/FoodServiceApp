using FoodServiceApp.Maui.Services;
using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class AdminLoginPage : ContentPage
{
    public AdminLoginPage()
    {
        InitializeComponent();
    }

    private async void OnDangNhapClicked(object sender, EventArgs e)
    {
        LblLoi.IsVisible = false;

        if (string.IsNullOrWhiteSpace(EntryEmail.Text))
        {
            HienLoi("Vui lòng nhập tên đăng nhập");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntryMatKhau.Text))
        {
            HienLoi("Vui lòng nhập mật khẩu");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        BtnDangNhap.IsEnabled = false;

        try
        {
            await ApiClient.LoginAsync("QuanTri", EntryEmail.Text, EntryMatKhau.Text);
            await Shell.Current.GoToAsync(nameof(AdminHomePage));
        }
        catch (Exception ex) { HienLoi(ex.Message); }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            BtnDangNhap.IsEnabled = true;
        }
    }

    private void HienLoi(string thongDiep)
    {
        LblLoi.Text = thongDiep;
        LblLoi.IsVisible = true;
    }

    private async void OnQuayLaiTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
    private async void OnConfigureServerClicked(object sender, EventArgs e)
    {
        var url = await DisplayPromptAsync("Kết nối máy chủ", "Địa chỉ Web API", initialValue: ApiClient.BaseUrl);
        if (string.IsNullOrWhiteSpace(url)) return;
        try { ApiClient.BaseUrl = url; ApiClient.Logout("GianHang"); ApiClient.Logout("QuanTri"); VendorWorkspace.Clear(); }
        catch (Exception ex) { HienLoi(ex.Message); }
    }
}
