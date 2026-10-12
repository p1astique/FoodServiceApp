namespace FoodServiceApp.Maui.Services;
public static class ApiUi
{
    public static async Task Run(Page page, Func<Task> action)
    {
        if (page.IsBusy) return;
        page.IsBusy = true; page.IsEnabled = false;
        try { await action(); }
        catch (Exception ex) { await page.DisplayAlert("Không hoàn tất thao tác", ex.Message, "Đóng"); }
        finally { page.IsEnabled = true; page.IsBusy = false; }
    }
}
