using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;

namespace FoodServiceApp.Web.Controllers
{
    [Route("QuanTri")]
    public class AdminDashboardController : QuanTriBaseController
    {
        private readonly AppDbContext _db;

        public AdminDashboardController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        [HttpGet("Dashboard")]
        public async Task<IActionResult> Index()
        {
            var homNay = DateTime.Today;

            ViewBag.TongKhachHang = await _db.KhachHangs.CountAsync();
            ViewBag.TongGianHang = await _db.GianHangs.CountAsync();
            ViewBag.GianHangDangMo = await _db.GianHangs.CountAsync(g => g.TrangThaiKinhDoanh == "DangMo");
            ViewBag.TongDonHomNay = await _db.DonHangs.CountAsync(d => d.NgayDat >= homNay);
            ViewBag.DoanhThuHomNay = await _db.DonHangs
                .Where(d => d.NgayDat >= homNay && d.TrangThai == "DaGiao")
                .SumAsync(d => (decimal?)d.TongTien) ?? 0;

            ViewBag.SoChoDuyet = await _db.GianHangs.CountAsync(g => g.TrangThaiDuyet == "ChoDuyet");
            ViewBag.SoSuCo = await _db.DonHangs.CountAsync(d =>
                d.TrangThai == "DangGiao" && d.GiaoHang != null && d.GiaoHang.TrangThai == "GiaoThatBai");

            ViewBag.TopGianHang = await _db.DonHangs
                .Where(d => d.TrangThai == "DaGiao")
                .Include(d => d.GianHang)
                .GroupBy(d => d.GianHang!.TenCuaHang)
                .Select(g => new { TenGianHang = g.Key, DoanhThu = g.Sum(x => x.TongTien) })
                .OrderByDescending(x => x.DoanhThu)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
