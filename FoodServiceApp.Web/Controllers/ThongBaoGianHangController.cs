using FoodServiceApp.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Controllers;
public class ThongBaoGianHangController(AppDbContext db) : GianHangBaseController
{
    public async Task<IActionResult> Index()
    {
        var since = DateTime.Now.AddDays(-30);
        var orders = await db.DonHangs.Where(x => x.MaGianHang == CurrentGianHangId && x.NgayDat >= since)
            .OrderByDescending(x => x.NgayDat).Select(x => new { x.MaDonHang, x.TrangThai, Ngay = x.NgayDat }).Take(50).ToListAsync();
        var reviews = await db.DanhGias.Where(x => x.MaGianHang == CurrentGianHangId && x.NgayDanhGia >= since)
            .OrderByDescending(x => x.NgayDanhGia).Select(x => new { x.MaDonHang, x.SoSao, Ngay = x.NgayDanhGia }).Take(20).ToListAsync();
        ViewBag.Orders = orders; ViewBag.Reviews = reviews;
        return View();
    }
}
