using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;
namespace FoodServiceApp.Maui.Views;
[QueryProperty(nameof(MonAnId), "MonAnId")]
public partial class VendorMenuItemEditPage : ContentPage
{
    public string? MonAnId { get; set; }
    private MonAn? _item;
    public VendorMenuItemEditPage() { InitializeComponent(); }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ApiUi.Run(this, async () => {
            await VendorWorkspace.LoadAsync();
            _item = VendorWorkspace.Items.FirstOrDefault(x => x.Id == MonAnId);
            if (MonAnId != null && _item == null) throw new InvalidOperationException("Món không còn tồn tại.");
            PickerDanhMuc.ItemsSource = VendorWorkspace.Categories.Select(x => x.TenDanhMuc).ToList();
            PickerDanhMuc.SelectedItem = _item?.DanhMuc;
            EntryTenMon.Text = _item?.TenMon ?? ""; EntryGia.Text = _item?.Gia.ToString("0") ?? "";
            EntryHinhAnh.Text = _item?.HinhAnh ?? ""; SwitchConHang.IsToggled = _item?.ConHang ?? true;
            EntryMoTa.Text = _item?.MoTa ?? ""; EntryKichCo.Text = _item?.KichCo ?? "";
            EntryTopping.Text = _item?.Topping ?? ""; EntryMucCay.Text = _item?.MucCay ?? "";
            BtnXoa.IsVisible = _item != null; Title = _item == null ? "Thêm món ăn" : "Sửa món ăn";
        });
    }
    private async void OnLuuClicked(object sender, EventArgs e)
    {
        await ApiUi.Run(this, async () => {
            var category = VendorWorkspace.Categories.FirstOrDefault(x => x.TenDanhMuc == PickerDanhMuc.SelectedItem as string);
            if (category == null || string.IsNullOrWhiteSpace(EntryTenMon.Text) || !decimal.TryParse(EntryGia.Text, out var price) || price <= 0)
                throw new InvalidOperationException("Nhập tên, chọn danh mục và giá lớn hơn 0.");
            await ApiClient.SendAsync(_item == null ? HttpMethod.Post : HttpMethod.Put,
                "api/vendor/items" + (_item == null ? "" : $"/{_item.Id}"), new {
                    category.MaDanhMuc, TenMon = EntryTenMon.Text.Trim(), Gia = price,
                    HinhAnh = EntryHinhAnh.Text, MoTa = EntryMoTa.Text, KichCo = EntryKichCo.Text,
                    Topping = EntryTopping.Text, MucCay = EntryMucCay.Text, ConHang = SwitchConHang.IsToggled
                });
            await Shell.Current.GoToAsync("..");
        });
    }
    private async void OnXoaClicked(object sender, EventArgs e)
    {
        if (_item == null || !await DisplayAlert("Xóa món", $"Xóa {_item.TenMon}?", "Xóa", "Hủy")) return;
        await ApiUi.Run(this, async () => { await ApiClient.SendAsync(HttpMethod.Delete, $"api/vendor/items/{_item.Id}"); await Shell.Current.GoToAsync(".."); });
    }
}
