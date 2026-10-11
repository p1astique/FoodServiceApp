using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;

namespace FoodServiceApp.Maui.Views;

[QueryProperty(nameof(MonAnId), "MonAnId")]
public partial class VendorMenuItemEditPage : ContentPage
{
    public string? MonAnId { get; set; }

    private MonAn? _monDangSua;

    public VendorMenuItemEditPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _monDangSua = string.IsNullOrEmpty(MonAnId)
            ? null
            : MockData.DanhSachMonAn.FirstOrDefault(m => m.Id == MonAnId);

        if (_monDangSua is not null)
        {
            Title = "Sửa món ăn";
            BtnXoa.IsVisible = true;
            BtnLuu.Text = "Lưu thay đổi";

            EntryHinhAnh.Text = _monDangSua.HinhAnh;
            EntryTenMon.Text = _monDangSua.TenMon;
            PickerDanhMuc.SelectedItem = _monDangSua.DanhMuc;
            EntryGia.Text = ((long)_monDangSua.Gia).ToString();
            SwitchConHang.IsToggled = _monDangSua.ConHang;
            PickerDanhMuc.ItemsSource = MockData.DanhMucCuaGianHang(MockData.GianHangHienTai.Id).ToList();
        }
        else
        {
            Title = "Thêm món ăn";
            BtnXoa.IsVisible = false;
            BtnLuu.Text = "Lưu món ăn";

            EntryHinhAnh.Text = "🍲";
            EntryTenMon.Text = "";
            PickerDanhMuc.SelectedItem = null;
            EntryGia.Text = "";
            SwitchConHang.IsToggled = true;
            PickerDanhMuc.ItemsSource = MockData.DanhMucCuaGianHang(MockData.GianHangHienTai.Id).ToList();
        }
    }

    private async void OnLuuClicked(object sender, EventArgs e)
    {
        LblLoi.IsVisible = false;

        if (string.IsNullOrWhiteSpace(EntryTenMon.Text))
        {
            HienLoi("Vui lòng nhập tên món");
            return;
        }
        if (PickerDanhMuc.SelectedItem is null)
        {
            HienLoi("Hãy tạo danh mục trước khi thêm món ăn");
            return;
        }
        if (!decimal.TryParse(EntryGia.Text, out var gia) || gia <= 0)
        {
            HienLoi("Giá bán không hợp lệ");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        BtnLuu.IsEnabled = false;

        try
        {
            var hinhAnh = string.IsNullOrWhiteSpace(EntryHinhAnh.Text) ? "🍲" : EntryHinhAnh.Text.Trim();
            var danhMuc = (string)PickerDanhMuc.SelectedItem;

            if (_monDangSua is not null)
            {
                _monDangSua.TenMon = EntryTenMon.Text.Trim();
                _monDangSua.DanhMuc = danhMuc;
                _monDangSua.Gia = gia;
                _monDangSua.HinhAnh = hinhAnh;
                _monDangSua.ConHang = SwitchConHang.IsToggled;
            }
            else
            {
                var monMoi = new MonAn
                {
                    Id = "ma" + (MockData.DanhSachMonAn.Count + 1),
                    TenMon = EntryTenMon.Text.Trim(),
                    DanhMuc = danhMuc,
                    Gia = gia,
                    HinhAnh = hinhAnh,
                    ConHang = SwitchConHang.IsToggled,
                    GianHangId = MockData.GianHangHienTai.Id,
                };
                MockData.DanhSachMonAn.Add(monMoi);
            }

            VendorLocalStore.Luu();

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            BtnLuu.IsEnabled = true;
        }
    }

    private async void OnXoaClicked(object sender, EventArgs e)
    {
        if (_monDangSua is null) return;

        bool xacNhan = await DisplayAlert("Xóa món ăn", $"Xóa \"{_monDangSua.TenMon}\" khỏi thực đơn?", "Xóa", "Hủy");
        if (!xacNhan) return;

        MockData.DanhSachMonAn.Remove(_monDangSua);
        VendorLocalStore.Luu();

        await Shell.Current.GoToAsync("..");
    }

    private void HienLoi(string thongDiep)
    {
        LblLoi.Text = thongDiep;
        LblLoi.IsVisible = true;
    }
}
