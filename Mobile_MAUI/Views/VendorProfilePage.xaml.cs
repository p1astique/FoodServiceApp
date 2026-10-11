using FoodServiceApp.Maui.Services;

namespace FoodServiceApp.Maui.Views;

public partial class VendorProfilePage : ContentPage
{
    public VendorProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NapHoSo();
    }

    private void NapHoSo()
    {
        var gh = Models.MockData.GianHangHienTai;

        LblAnhHienTai.Text = gh.HinhAnh;
        EntryHinhAnh.Text = gh.HinhAnh;
        EntryTenGianHang.Text = gh.TenGianHang;
        EntryDiaChi.Text = gh.DiaChi;
        EntrySdt.Text = gh.SoDienThoai;
        EntryEmail.Text = gh.Email;
        EntryThoiGianGiao.Text = gh.ThoiGianGiaoPhut.ToString();
        EditorMoTa.Text = gh.MoTa;
        SwitchDangMoCua.IsToggled = gh.DangMoCua;

        LblThanhCong.IsVisible = false;
        LblLoi.IsVisible = false;
    }

    private void OnLuuClicked(object sender, EventArgs e)
    {
        LblLoi.IsVisible = false;
        LblThanhCong.IsVisible = false;

        if (string.IsNullOrWhiteSpace(EntryTenGianHang.Text))
        {
            HienLoi("Vui lòng nhập tên gian hàng");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntryDiaChi.Text))
        {
            HienLoi("Vui lòng nhập địa chỉ cửa hàng");
            return;
        }
        if (string.IsNullOrWhiteSpace(EntrySdt.Text))
        {
            HienLoi("Vui lòng nhập số điện thoại");
            return;
        }
        if (!int.TryParse(EntryThoiGianGiao.Text, out var thoiGianGiao) || thoiGianGiao <= 0)
        {
            HienLoi("Thời gian giao hàng không hợp lệ");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        BtnLuu.IsEnabled = false;

        try
        {
            var gh = Models.MockData.GianHangHienTai;
            gh.HinhAnh = string.IsNullOrWhiteSpace(EntryHinhAnh.Text) ? gh.HinhAnh : EntryHinhAnh.Text.Trim();
            gh.TenGianHang = EntryTenGianHang.Text.Trim();
            gh.DiaChi = EntryDiaChi.Text.Trim();
            gh.SoDienThoai = EntrySdt.Text.Trim();
            gh.ThoiGianGiaoPhut = thoiGianGiao;
            gh.MoTa = EditorMoTa.Text?.Trim() ?? "";
            gh.DangMoCua = SwitchDangMoCua.IsToggled;
            VendorLocalStore.Luu();

            LblAnhHienTai.Text = gh.HinhAnh;
            LblThanhCong.IsVisible = true;
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
