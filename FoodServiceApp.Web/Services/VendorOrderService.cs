using FoodServiceApp.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Services;

public static class VendorOrderService
{
    public static async Task<bool> UpdateAsync(AppDbContext db, int store, int order, string status, string? reason = null)
    {
        var current = await db.DonHangs.Where(x => x.MaGianHang == store && x.MaDonHang == order)
            .Select(x => x.TrangThai).SingleOrDefaultAsync();
        if (current == null) return false;
        var valid = (current, status) switch {
            ("ChoXacNhan", "DaXacNhan") => true,
            ("DaXacNhan", "DangChuanBi") => true,
            ("DangChuanBi", "SanSangGiao") => true,
            ("ChoXacNhan" or "DaXacNhan" or "DangChuanBi" or "SanSangGiao", "DaHuy") => !string.IsNullOrWhiteSpace(reason),
            _ => false
        };
        if (!valid) return false;
        var changed = await db.DonHangs.Where(x => x.MaGianHang == store && x.MaDonHang == order && x.TrangThai == current)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.TrangThai, status).SetProperty(x => x.LyDoHuy, status == "DaHuy" ? reason : null));
        if (changed != 1) return false;
        db.NhatKyTrangThaiDons.Add(new Models.Entities.NhatKyTrangThaiDon { MaDonHang = order,
            TrangThaiCu = current, TrangThaiMoi = status, LoaiNguoiThucHien = "GianHang", MaNguoiThucHien = store,
            LyDo = reason?.Trim() });
        await db.SaveChangesAsync();
        return true;
    }
}
