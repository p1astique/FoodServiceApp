using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class VendorRevenuePage : ContentPage
{
    public VendorRevenuePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NapBaoCao();
    }

    private void NapBaoCao()
    {
        // TODO: thay bằng GET /api/gian-hang/doanh-thu?tu=...&den=... khi có API thật.
        var donHoanThanh = MockData.DonHangCuaGianHang
            .Where(d => d.GianHangId == MockData.GianHangHienTai.Id && d.TrangThai == TrangThaiDonHang.HoanThanh)
            .OrderByDescending(d => d.NgayDat)
            .ToList();

        var homNay = donHoanThanh.Where(d => d.NgayDat.Date == DateTime.Now.Date).ToList();
        var bay_ngay = donHoanThanh.Where(d => d.NgayDat >= DateTime.Now.AddDays(-7)).ToList();

        LblDoanhThuHomNay.Text = $"{homNay.Sum(d => d.TongTien):N0}đ";
        LblDoanhThu7Ngay.Text = $"{bay_ngay.Sum(d => d.TongTien):N0}đ";
        LblTongDonHoanThanh.Text = donHoanThanh.Count.ToString();
        LblGiaTriTrungBinh.Text = donHoanThanh.Count > 0
            ? $"{donHoanThanh.Average(d => d.TongTien):N0}đ"
            : "0đ";

        NapMonBanChay(donHoanThanh);

        DsDonHoanThanh.ItemsSource = donHoanThanh;
        LblRong.IsVisible = donHoanThanh.Count == 0;
    }

    private void NapMonBanChay(List<DonHang> donHoanThanh)
    {
        KhungMonBanChay.Children.Clear();

        var monBanChay = donHoanThanh
            .SelectMany(d => d.ChiTiet)
            .GroupBy(ct => ct.MonAn.TenMon)
            .Select(g => new { TenMon = g.Key, SoLuong = g.Sum(ct => ct.SoLuong) })
            .OrderByDescending(x => x.SoLuong)
            .Take(5)
            .ToList();

        if (monBanChay.Count == 0)
        {
            KhungMonBanChay.Children.Add(new Label
            {
                Text = "Chưa có dữ liệu bán hàng.",
                FontSize = 13,
                TextColor = (Color)Application.Current!.Resources["TextGrey"],
            });
            return;
        }

        foreach (var mon in monBanChay)
        {
            var dong = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) } };
            var lblTen = new Label { Text = mon.TenMon, FontSize = 13 };
            var lblSoLuong = new Label
            {
                Text = $"{mon.SoLuong} phần",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)Application.Current!.Resources["Primary"],
            };
            Grid.SetColumn(lblTen, 0);
            Grid.SetColumn(lblSoLuong, 1);
            dong.Children.Add(lblTen);
            dong.Children.Add(lblSoLuong);
            KhungMonBanChay.Children.Add(dong);
        }
    }
}
