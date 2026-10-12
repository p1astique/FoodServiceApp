using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;
namespace FoodServiceApp.Maui.Views;
[QueryProperty(nameof(KhuyenMaiId), "KhuyenMaiId")]
public partial class VendorPromotionEditPage : ContentPage
{
    public string? KhuyenMaiId { get; set; }
    private KhuyenMai? _item;
    public VendorPromotionEditPage() { InitializeComponent(); }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ApiUi.Run(this, async () => {
            await VendorWorkspace.LoadAsync(); _item = VendorWorkspace.Promotions.FirstOrDefault(x => x.Id == KhuyenMaiId);
            if (KhuyenMaiId != null && _item == null) throw new InvalidOperationException("Khuyến mãi không còn tồn tại.");
            EntryMaCode.Text = _item?.MaCode ?? ""; EntryMaCode.IsEnabled = _item == null;
            EditorMoTa.Text = _item?.MoTa ?? "";
            PickerLoaiGiam.SelectedIndex = _item?.GiamPhiVanChuyen == true ? 2 : _item?.LoaiGiam == LoaiGiamGia.SoTien ? 1 : 0;
            EntryGiaTriGiam.Text = _item?.GiaTriGiam.ToString("0.##") ?? "";
            EntryDonToiThieu.Text = _item?.GiaTriDonToiThieu.ToString("0.##") ?? "0";
            EntrySoLuongToiDa.Text = _item?.SoLuongToiDa?.ToString() ?? "";
            EntryGiamPhiToiDa.Text = _item?.GiamPhiVanChuyenToiDa?.ToString("0.##") ?? "";
            SwitchMoiKhachMotLan.IsToggled = _item?.MoiKhachMotLan ?? false;
            DateBatDau.Date = _item?.NgayBatDau.Date ?? DateTime.Today;
            DateKetThuc.Date = _item?.NgayKetThuc.Date ?? DateTime.Today.AddMonths(1);
            SwitchDangHoatDong.IsToggled = _item?.DangHoatDong ?? true; BtnXoa.IsVisible = _item != null;
            PickerMon.ItemsSource = new[] { "Toàn bộ đơn hàng" }.Concat(VendorWorkspace.Items.Select(x => x.TenMon + " (#" + x.Id + ")")).ToList();
            PickerMon.SelectedIndex = _item?.MaMonAn == null ? 0 : VendorWorkspace.Items.FindIndex(x => x.Id == _item.MaMonAn.ToString()) + 1;
            PickerDanhMuc.ItemsSource = new[] { "Tất cả danh mục" }.Concat(VendorWorkspace.Categories.Select(x => x.TenDanhMuc)).ToList();
            PickerDanhMuc.SelectedIndex = _item?.MaDanhMuc == null ? 0 : VendorWorkspace.Categories.FindIndex(x => x.MaDanhMuc == _item.MaDanhMuc) + 1;
        });
    }
    private void OnLoaiGiamChanged(object? sender, EventArgs e) { if (LblNhanGiaTri != null) LblNhanGiaTri.Text = PickerLoaiGiam.SelectedIndex == 0 ? "Giá trị giảm (%)" : PickerLoaiGiam.SelectedIndex == 1 ? "Giá trị giảm (đ)" : "Giá trị giảm (không dùng cho phí vận chuyển)"; }
    private async void OnLuuClicked(object sender, EventArgs e)
    {
        await ApiUi.Run(this, async () => {
            var shipping = PickerLoaiGiam.SelectedIndex == 2;
            decimal.TryParse(EntryGiaTriGiam.Text, out var amount);
            if (string.IsNullOrWhiteSpace(EntryMaCode.Text) || (!shipping && amount <= 0))
                throw new InvalidOperationException("Nhập mã và giá trị giảm hợp lệ.");
            var itemId = PickerMon.SelectedIndex > 0 ? int.Parse(VendorWorkspace.Items[PickerMon.SelectedIndex - 1].Id) : (int?)null;
            var categoryId = PickerDanhMuc.SelectedIndex > 0 ? VendorWorkspace.Categories[PickerDanhMuc.SelectedIndex - 1].MaDanhMuc : (int?)null;
            if (itemId != null && categoryId != null) throw new InvalidOperationException("Chỉ chọn món hoặc danh mục áp dụng.");
            decimal.TryParse(EntryDonToiThieu.Text, out var minimum);
            int? limit = int.TryParse(EntrySoLuongToiDa.Text, out var parsedLimit) ? parsedLimit : null;
            decimal? shippingCap = decimal.TryParse(EntryGiamPhiToiDa.Text, out var parsedCap) ? parsedCap : null;
            await ApiClient.SendAsync(_item == null ? HttpMethod.Post : HttpMethod.Put,
                "api/vendor/promotions" + (_item == null ? "" : $"/{_item.Id}"), new {
                    MaCode = EntryMaCode.Text.Trim(), MoTa = EditorMoTa.Text, MaMonAn = itemId, MaDanhMuc = categoryId,
                    LaPhanTram = PickerLoaiGiam.SelectedIndex == 0, GiaTriGiam = amount,
                    GiaTriDonToiThieu = minimum, SoLuongToiDa = limit, MoiKhachMotLan = SwitchMoiKhachMotLan.IsToggled,
                    GiamPhiVanChuyen = shipping, GiamPhiVanChuyenToiDa = shippingCap,
                    NgayBatDau = DateBatDau.Date, NgayKetThuc = DateKetThuc.Date.AddDays(1).AddTicks(-1),
                    TrangThai = SwitchDangHoatDong.IsToggled
                });
            await Shell.Current.GoToAsync("..");
        });
    }
    private async void OnXoaClicked(object sender, EventArgs e)
    {
        if (_item == null || !await DisplayAlert("Xóa khuyến mãi", $"Xóa {_item.MaCode}?", "Xóa", "Hủy")) return;
        await ApiUi.Run(this, async () => { await ApiClient.SendAsync(HttpMethod.Delete, $"api/vendor/promotions/{_item.Id}"); await Shell.Current.GoToAsync(".."); });
    }
}
