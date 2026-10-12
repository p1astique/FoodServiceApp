using FoodServiceApp.Maui.Services;
using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

public partial class OrderTrackingPage : ContentPage
{
    public OrderTrackingPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ApiUi.Run(this, async () => {
            DsLichSu.ItemsSource = null; KhungDangGiao.IsVisible = false;
            var orders = await ApiClient.GetAsync<List<VendorWorkspace.OrderDto>>("api/customer/orders", "KhachHang");
            _orders = orders.Select(o => new DonHang { Id = o.MaDonHang.ToString(), TenGianHang = o.TenGianHang,
                TongTienServer = o.TongTien, NgayDat = o.NgayDat, DiaChiGiao = o.DiaChiGiaoHang,
                TrangThai = o.TrangThai switch { "DaXacNhan" or "DangChuanBi" => TrangThaiDonHang.DangLam, "SanSangGiao" or "TaiXeDaNhan" or "DangGiao" => TrangThaiDonHang.DangGiao,
                    "DaGiao" => TrangThaiDonHang.HoanThanh, "DaHuy" => TrangThaiDonHang.DaHuy, _ => TrangThaiDonHang.ChoXacNhan },
                ChiTiet = o.ChiTiet.Select(c => new MucGioHang { MonAn = new MonAn { TenMon = c.TenMon, Gia = c.DonGia }, SoLuong = c.SoLuong }).ToList()
            }).ToList(); TaiDuLieu();
        });
    }

    private List<DonHang> _orders = new();
    private void TaiDuLieu()
    {
        var donDangXuLy = _orders.FirstOrDefault(d =>
            d.TrangThai is TrangThaiDonHang.ChoXacNhan or TrangThaiDonHang.DangLam or TrangThaiDonHang.DangGiao);

        if (donDangXuLy is not null)
        {
            KhungDangGiao.IsVisible = true;
            LblMaDonDangGiao.Text = $"Đơn #{donDangXuLy.Id}";
            LblTrangThaiDangGiao.Text = donDangXuLy.TrangThai.Nhan();
            LblGianHangDangGiao.Text = donDangXuLy.TenGianHang;
            VeThanhTienTrinh(donDangXuLy.TrangThai.Buoc());
        }
        else
        {
            KhungDangGiao.IsVisible = false;
        }

        DsLichSu.ItemsSource = _orders
            .Where(d => d != donDangXuLy)
            .ToList();
    }

    private void VeThanhTienTrinh(int buocHienTai)
    {
        var boxes = new[] { Buoc0, Buoc1, Buoc2, Buoc3 };
        var mauXong = (Color)Application.Current!.Resources["Accent"];
        var mauChua = Color.FromArgb("#40FFFFFF");
        for (int i = 0; i < boxes.Length; i++)
            boxes[i].BackgroundColor = i <= buocHienTai ? mauXong : mauChua;
    }

    private async void OnDonHoanThanhTapped(object sender, EventArgs e)
    {
        if (sender is not Frame { GestureRecognizers: [TapGestureRecognizer { CommandParameter: DonHang don }] })
            return;

        if (don.TrangThai != TrangThaiDonHang.HoanThanh) return;

        await Shell.Current.GoToAsync(nameof(ReviewPage), new Dictionary<string, object>
        {
            { "DonHang", don }
        });
    }
}
