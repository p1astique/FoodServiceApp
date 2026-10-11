using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

[QueryProperty(nameof(KhuyenMaiId), "KhuyenMaiId")]
public partial class VendorPromotionEditPage : ContentPage
{
    public string? KhuyenMaiId { get; set; }

    private KhuyenMai? _kmDangSua;

    public VendorPromotionEditPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _kmDangSua = string.IsNullOrEmpty(KhuyenMaiId)
            ? null
            : MockData.DanhSachKhuyenMai.FirstOrDefault(k => k.Id == KhuyenMaiId);

        if (_kmDangSua is not null)
        {
            Title = "Sửa khuyến mãi";
            BtnXoa.IsVisible = true;
            BtnLuu.Text = "Lưu thay đổi";

            EntryMaCode.Text = _kmDangSua.MaCode;
            EntryMaCode.IsEnabled = false; // không cho đổi mã sau khi đã tạo, tránh khách hàng nhầm lẫn
            EntryTenChuongTrinh.Text = _kmDangSua.TenChuongTrinh;
            EditorMoTa.Text = _kmDangSua.MoTa;
            PickerLoaiGiam.SelectedIndex = _kmDangSua.LoaiGiam == LoaiGiamGia.PhanTram ? 0 : 1;
            EntryGiaTriGiam.Text = _kmDangSua.GiaTriGiam.ToString("0.#");
            EntryGiamToiDa.Text = _kmDangSua.GiamToiDa?.ToString("0") ?? "";
            EntryDonToiThieu.Text = ((long)_kmDangSua.GiaTriDonToiThieu).ToString();
            EntrySoLuongToiDa.Text = _kmDangSua.SoLuongToiDa.ToString();
            DateBatDau.Date = _kmDangSua.NgayBatDau.Date;
            DateKetThuc.Date = _kmDangSua.NgayKetThuc.Date;
            SwitchDangHoatDong.IsToggled = _kmDangSua.DangHoatDong;
        }
        else
        {
            Title = "Thêm khuyến mãi";
            BtnXoa.IsVisible = false;
            BtnLuu.Text = "Lưu khuyến mãi";

            EntryMaCode.Text = "";
            EntryMaCode.IsEnabled = true;
            EntryTenChuongTrinh.Text = "";
            EditorMoTa.Text = "";
            PickerLoaiGiam.SelectedIndex = 0;
            EntryGiaTriGiam.Text = "";
            EntryGiamToiDa.Text = "";
            EntryDonToiThieu.Text = "";
            EntrySoLuongToiDa.Text = "";
            DateBatDau.Date = DateTime.Today;
            DateKetThuc.Date = DateTime.Today.AddMonths(1);
            SwitchDangHoatDong.IsToggled = true;
        }

        CapNhatNhanGiaTri();
    }

    private void OnLoaiGiamChanged(object? sender, EventArgs e) => CapNhatNhanGiaTri();

    private void CapNhatNhanGiaTri()
    {
        bool laPhanTram = PickerLoaiGiam.SelectedIndex == 0;
        LblNhanGiaTri.Text = laPhanTram ? "Giá trị giảm (%) *" : "Giá trị giảm (đ) *";
        LblGiamToiDa.IsVisible = laPhanTram;
        EntryGiamToiDa.IsVisible = laPhanTram;
    }

    private async void OnLuuClicked(object sender, EventArgs e)
    {
        LblLoi.IsVisible = false;

        if (string.IsNullOrWhiteSpace(EntryMaCode.Text))
        {
            HienLoi("Vui lòng nhập mã khuyến mãi");
            return;
        }
        var maCode = EntryMaCode.Text.Trim().ToUpper();
        if (MockData.DanhSachKhuyenMai.Any(k => k.Id != KhuyenMaiId && string.Equals(k.MaCode, maCode, StringComparison.OrdinalIgnoreCase)))
        {
            HienLoi("Mã này đã tồn tại, vui lòng chọn mã khác");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntryTenChuongTrinh.Text))
        {
            HienLoi("Vui lòng nhập tên chương trình");
            return;
        }
        if (PickerLoaiGiam.SelectedIndex < 0)
        {
            HienLoi("Vui lòng chọn hình thức giảm giá");
            return;
        }
        var laPhanTram = PickerLoaiGiam.SelectedIndex == 0;
        if (!decimal.TryParse(EntryGiaTriGiam.Text, out var giaTriGiam) || giaTriGiam <= 0)
        {
            HienLoi("Giá trị giảm không hợp lệ");
            return;
        }
        if (laPhanTram && giaTriGiam > 100)
        {
            HienLoi("Giảm theo % không được vượt quá 100");
            return;
        }
        decimal? giamToiDa = null;
        if (laPhanTram && !string.IsNullOrWhiteSpace(EntryGiamToiDa.Text))
        {
            if (!decimal.TryParse(EntryGiamToiDa.Text, out var gtd) || gtd <= 0)
            {
                HienLoi("Mức giảm tối đa không hợp lệ");
                return;
            }
            giamToiDa = gtd;
        }
        if (!decimal.TryParse(EntryDonToiThieu.Text, out var donToiThieu) || donToiThieu < 0)
        {
            donToiThieu = 0;
        }
        if (!int.TryParse(EntrySoLuongToiDa.Text, out var soLuongToiDa) || soLuongToiDa <= 0)
        {
            HienLoi("Số lượt sử dụng tối đa không hợp lệ");
            return;
        }
        if (DateKetThuc.Date < DateBatDau.Date)
        {
            HienLoi("Ngày kết thúc phải sau ngày bắt đầu");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        BtnLuu.IsEnabled = false;

        try
        {
            // TODO: gọi ASP.NET Core Web API thật:
            //   POST /api/gian-hang/khuyen-mai       (thêm mới)
            //   PUT  /api/gian-hang/khuyen-mai/{id}    (cập nhật)
            await Task.Delay(400);

            if (_kmDangSua is not null)
            {
                _kmDangSua.TenChuongTrinh = EntryTenChuongTrinh.Text.Trim();
                _kmDangSua.MoTa = EditorMoTa.Text?.Trim() ?? "";
                _kmDangSua.LoaiGiam = laPhanTram ? LoaiGiamGia.PhanTram : LoaiGiamGia.SoTien;
                _kmDangSua.GiaTriGiam = giaTriGiam;
                _kmDangSua.GiamToiDa = laPhanTram ? giamToiDa : null;
                _kmDangSua.GiaTriDonToiThieu = donToiThieu;
                _kmDangSua.SoLuongToiDa = soLuongToiDa;
                _kmDangSua.NgayBatDau = DateBatDau.Date;
                _kmDangSua.NgayKetThuc = DateKetThuc.Date;
                _kmDangSua.DangHoatDong = SwitchDangHoatDong.IsToggled;
            }
            else
            {
                var kmMoi = new KhuyenMai
                {
                    Id = "km" + (MockData.DanhSachKhuyenMai.Count + 1),
                    MaCode = maCode,
                    TenChuongTrinh = EntryTenChuongTrinh.Text.Trim(),
                    MoTa = EditorMoTa.Text?.Trim() ?? "",
                    LoaiGiam = laPhanTram ? LoaiGiamGia.PhanTram : LoaiGiamGia.SoTien,
                    GiaTriGiam = giaTriGiam,
                    GiamToiDa = laPhanTram ? giamToiDa : null,
                    GiaTriDonToiThieu = donToiThieu,
                    SoLuongToiDa = soLuongToiDa,
                    SoLuongDaDung = 0,
                    NgayBatDau = DateBatDau.Date,
                    NgayKetThuc = DateKetThuc.Date,
                    DangHoatDong = SwitchDangHoatDong.IsToggled,
                    GianHangId = MockData.GianHangHienTai.Id, // khuyến mãi do gian hàng tự tạo — chỉ áp dụng cho gian hàng này
                };
                MockData.DanhSachKhuyenMai.Add(kmMoi);
            }

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
        if (_kmDangSua is null) return;

        bool xacNhan = await DisplayAlert("Xóa khuyến mãi", $"Xóa mã \"{_kmDangSua.MaCode}\"?", "Xóa", "Hủy");
        if (!xacNhan) return;

        MockData.DanhSachKhuyenMai.Remove(_kmDangSua);
        // TODO: gọi DELETE /api/gian-hang/khuyen-mai/{id} khi có API thật.

        await Shell.Current.GoToAsync("..");
    }

    private void HienLoi(string thongDiep)
    {
        LblLoi.Text = thongDiep;
        LblLoi.IsVisible = true;
    }
}
