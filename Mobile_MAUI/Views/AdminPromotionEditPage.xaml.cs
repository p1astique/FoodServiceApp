using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class AdminPromotionEditPage : ContentPage
{
    public AdminPromotionEditPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        PickerLoaiGiam.SelectedIndex = 0;
        DateBatDau.Date = DateTime.Today;
        DateKetThuc.Date = DateTime.Today.AddMonths(1);
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
        if (MockData.DanhSachKhuyenMai.Any(k => string.Equals(k.MaCode, maCode, StringComparison.OrdinalIgnoreCase)))
        {
            HienLoi("Mã này đã tồn tại, vui lòng chọn mã khác");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntryTenChuongTrinh.Text))
        {
            HienLoi("Vui lòng nhập tên chương trình");
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
            // TODO: gọi POST /api/quan-tri/khuyen-mai khi có API thật.
            await Task.Delay(400);

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
                DangHoatDong = true,
                GianHangId = null, // quản trị viên tạo => áp dụng toàn hệ thống
            };
            MockData.DanhSachKhuyenMai.Add(kmMoi);

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            BtnLuu.IsEnabled = true;
        }
    }

    private void HienLoi(string thongDiep)
    {
        LblLoi.Text = thongDiep;
        LblLoi.IsVisible = true;
    }
}
