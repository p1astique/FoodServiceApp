using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Services;

public record KetQuaVoucher(bool HopLe, decimal GiamTienMon, decimal GiamPhiGiaoHang, string? Loi)
{
    public decimal TongGiam => GiamTienMon + GiamPhiGiaoHang;
}

public static class KhuyenMaiHelper
{
    public static async Task<KetQuaVoucher> KiemTraAsync(AppDbContext db, KhuyenMai km, Cart gio,
        int maGianHang, int maKH, decimal phiGiaoHang)
    {
        var now = DateTime.Now;
        if (!km.TrangThai) return Loi("Voucher đang tạm tắt.");
        if (now < km.NgayBatDau) return Loi("Voucher chưa đến thời gian sử dụng.");
        if (now > km.NgayKetThuc) return Loi("Voucher đã hết hạn.");
        var storeScope = km.MaGianHang ?? km.MaGianHangApDung;
        if (storeScope != null && storeScope != maGianHang) return Loi("Voucher không áp dụng cho gian hàng này.");
        if (gio.TongTien < km.GiaTriDonToiThieu)
            return Loi($"Đơn hàng tối thiểu {km.GiaTriDonToiThieu:N0}đ để dùng voucher.");
        if (km.SoLuongToiDa != null && km.SoLuongDaDung >= km.SoLuongToiDa)
            return Loi("Voucher đã hết lượt sử dụng.");
        if (km.MoiKhachMotLan && await db.SuDungKhuyenMais.AnyAsync(x => x.MaKhuyenMai == km.MaKhuyenMai && x.MaKH == maKH))
            return Loi("Bạn đã sử dụng voucher này rồi.");

        var eligible = gio.Items.AsEnumerable();
        if (km.MaMonAn != null) eligible = eligible.Where(x => x.MaMonAn == km.MaMonAn);
        if (km.MaDanhMuc != null)
        {
            var ids = await db.MonAns.Where(x => x.MaDanhMuc == km.MaDanhMuc).Select(x => x.MaMonAn).ToListAsync();
            eligible = eligible.Where(x => ids.Contains(x.MaMonAn));
        }
        var baseAmount = eligible.Sum(x => x.Gia * x.SoLuong);
        if ((km.MaMonAn != null || km.MaDanhMuc != null) && baseAmount <= 0)
            return Loi("Giỏ hàng không có món thuộc phạm vi áp dụng của voucher.");

        var foodDiscount = km.GiamPhiVanChuyen ? 0 : km.PhanTramGiam != null
            ? Math.Round(baseAmount * km.PhanTramGiam.Value / 100)
            : km.SoTienGiam ?? 0;
        foodDiscount = Math.Clamp(foodDiscount, 0, baseAmount);
        var shippingDiscount = km.GiamPhiVanChuyen
            ? Math.Min(phiGiaoHang, km.GiamPhiVanChuyenToiDa ?? phiGiaoHang) : 0;
        return new(true, foodDiscount, shippingDiscount, null);
    }

    private static KetQuaVoucher Loi(string message) => new(false, 0, 0, message);

    public static decimal TinhGiam(KhuyenMai km, Cart gio, out string? loi)
    {
        loi = null;
        var eligible = km.MaMonAn == null ? gio.Items : gio.Items.Where(i => i.MaMonAn == km.MaMonAn);
        var amount = eligible.Sum(i => i.Gia * i.SoLuong);
        if (amount <= 0) { loi = "Giỏ hàng không có món thuộc phạm vi voucher."; return 0; }
        return Math.Clamp(km.PhanTramGiam != null ? Math.Round(amount * km.PhanTramGiam.Value / 100) : km.SoTienGiam ?? 0, 0, amount);
    }
}
