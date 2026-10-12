using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Services;

// In-memory snapshot of server data. No demo fallback and no local persistence of vendor records.
public static class VendorWorkspace
{
    public static GianHang Store { get; private set; } = new();
    public static List<MonAn> Items { get; private set; } = new();
    public static List<Category> Categories { get; private set; } = new();
    public static List<DonHang> Orders { get; private set; } = new();
    public static List<KhuyenMai> Promotions { get; private set; } = new();
    public static void Clear() { Store = new(); Items = new(); Categories = new(); Orders = new(); Promotions = new(); }
    public static async Task LoadAsync()
    {
        try
        {
            var store = await ApiClient.GetAsync<GianHang>("api/vendor/profile");
            var categories = await ApiClient.GetAsync<List<Category>>("api/vendor/categories");
            var items = await ApiClient.GetAsync<List<MonAn>>("api/vendor/items");
            var orders = await ApiClient.GetAsync<List<OrderDto>>("api/vendor/orders");
            var promotions = await ApiClient.GetAsync<List<KhuyenMai>>("api/vendor/promotions");
            Store = store; Categories = categories; Items = items; Promotions = promotions;
            Orders = orders.Select(o => new DonHang {
                Id = o.MaDonHang.ToString(), GianHangId = store.Id, TenGianHang = o.TenGianHang,
                DiaChiGiao = o.DiaChiGiaoHang, DiaChiLayHang = o.DiaChiGianHang, NgayDat = o.NgayDat,
                TongTienServer = o.TongTien, PhiGiaoHang = o.PhiGiaoHang,
                TrangThaiMayChu = o.TrangThai,
                TrangThai = o.TrangThai switch { "DaXacNhan" or "DangChuanBi" => TrangThaiDonHang.DangLam,
                    "SanSangGiao" or "TaiXeDaNhan" or "DangGiao" => TrangThaiDonHang.DangGiao, "DaGiao" => TrangThaiDonHang.HoanThanh,
                    "DaHuy" => TrangThaiDonHang.DaHuy, _ => TrangThaiDonHang.ChoXacNhan },
                ChiTiet = o.ChiTiet.Select(c => new MucGioHang { SoLuong = c.SoLuong,
                    MonAn = new MonAn { TenMon = c.TenMon, Gia = c.DonGia } }).ToList()
            }).ToList();
        }
        catch { Clear(); throw; }
    }
    public static Task UpdateOrder(string id, string status) => ApiClient.SendAsync(HttpMethod.Post,
        $"api/vendor/orders/{id}/status", new { TrangThaiMoi = status, LyDo = status == "DaHuy" ? "Gian hàng từ chối đơn" : null });
    public static Task SaveProfile(GianHang g) => ApiClient.SendAsync(HttpMethod.Put, "api/vendor/profile",
        new { TenCuaHang = g.TenGianHang, g.DiaChi, SDT = g.SoDienThoai, g.MoTa, g.HinhAnh, g.DangMoCua });
    public record Category(int MaDanhMuc, string TenDanhMuc);
    public record DetailDto(string TenMon, int SoLuong, decimal DonGia);
    public record OrderDto(int MaDonHang, string TrangThai, decimal TongTien, DateTime NgayDat,
        string DiaChiGiaoHang, string TenGianHang, string DiaChiGianHang, List<DetailDto> ChiTiet, decimal PhiGiaoHang);
}
