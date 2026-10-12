using FoodServiceApp.Maui.Services;

namespace FoodServiceApp.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
