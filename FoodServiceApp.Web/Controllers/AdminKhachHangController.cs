using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;

namespace FoodServiceApp.Web.Controllers
{
    [Route("QuanTri/KhachHang")]
    public class AdminKhachHangController : QuanTriBaseController
    {
        private readonly AppDbContext _db;

        public AdminKhachHangController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? timKiem)
        {
            var query = _db.KhachHangs
                .Join(_db.TaiKhoans, kh => kh.MaTK, tk => tk.MaTK, (kh, tk) => new { KhachHang = kh, TaiKhoan = tk });

            if (!string.IsNullOrWhiteSpace(timKiem))
                query = query.Where(x => x.KhachHang.HoTen.Contains(timKiem) || x.KhachHang.SDT.Contains(timKiem));

            var danhSach = await query.OrderByDescending(x => x.KhachHang.MaKH).ToListAsync();

            ViewBag.TimKiem = timKiem ?? "";
            return View(danhSach.Select(x => (x.KhachHang, x.TaiKhoan)).ToList());
        }

        [HttpPost("ToggleKhoa/{maTK:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleKhoa(int maTK)
        {
            var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.MaTK == maTK && t.VaiTro == "KhachHang");
            if (taiKhoan == null) return NotFound();

            taiKhoan.TrangThai = !taiKhoan.TrangThai;
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = taiKhoan.TrangThai ? "Đã mở khoá tài khoản." : "Đã khoá tài khoản.";
            return RedirectToAction("Index");
        }
    }
}
