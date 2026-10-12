namespace FoodServiceApp.Maui.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        ToolbarItems.Add(new ToolbarItem("Đăng xuất", null, async () => {
            Services.ApiClient.Logout("KhachHang");
            await Shell.Current.GoToAsync("//dang-nhap");
        }));
    }
}
