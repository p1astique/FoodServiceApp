using FoodServiceApp.Maui.Services;

namespace FoodServiceApp.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        VendorLocalStore.Nap();
        MainPage = new AppShell();
    }
}
