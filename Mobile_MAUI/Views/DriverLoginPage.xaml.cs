namespace FoodServiceApp.Maui.Views;

public partial class DriverLoginPage : ContentPage
{
    public DriverLoginPage()
    {
        InitializeComponent();
    }

    private async void OnDangNhapClicked(object sender, EventArgs e)
    {
        LblLoi.IsVisible = false;

        if (string.IsNullOrWhiteSpace(EntrySdt.Text))
        {
            HienLoi("Nhập số điện thoại");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntryMatKhau.Text) || EntryMatKhau.Text.Length < 6)
        {
            HienLoi("Mật khẩu tối thiểu 6 ký tự");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        BtnDangNhap.IsEnabled = false;

        try
        {
            // TODO: gọi ASP.NET Core Web API thật: POST /api/tai-xe/dang-nhap { sdt, matKhau }
            await Task.Delay(500);
            await Shell.Current.GoToAsync(nameof(DriverHomePage));
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
