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

        if (string.IsNullOrWhiteSpace(EntryEmail.Text) || !EntryEmail.Text.Contains('@'))
        {
            HienLoi("Email không hợp lệ");
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
            // TODO: gọi ASP.NET Core Web API thật: POST /api/quan-tri/dang-nhap { email, matKhau }
            await Task.Delay(500);

            var admin = MockData.QuanTriVienHienTai;
            bool dungThongTin =
                string.Equals(admin.Email, EntryEmail.Text.Trim(), StringComparison.OrdinalIgnoreCase) &&
                admin.MatKhau == EntryMatKhau.Text;

            if (!dungThongTin)
            {
                HienLoi("Email hoặc mật khẩu không đúng");
                return;
            }

            await Shell.Current.GoToAsync(nameof(AdminHomePage));
        }
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
}
