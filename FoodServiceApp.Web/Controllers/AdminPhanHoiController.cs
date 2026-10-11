using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;

namespace FoodServiceApp.Web.Controllers
{
    // Quản trị viên xem và xử lý phản hồi của khách hàng.
    [Route("QuanTri/PhanHoi")]
    public class AdminPhanHoiController : QuanTriBaseController
    {
        private readonly AppDbContext _db;

        public AdminPhanHoiController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? trangThai)
        {
            var query = _db.PhanHois.Include(p => p.KhachHang).AsQueryable();
            if (trangThai == "ChuaXuLy" || trangThai == "DaXuLy")
                query = query.Where(p => p.TrangThai == trangThai);

            ViewBag.TrangThaiDangLoc = trangThai ?? "";
            ViewBag.SoChuaXuLy = await _db.PhanHois.CountAsync(p => p.TrangThai == "ChuaXuLy");
            return View(await query.OrderBy(p => p.TrangThai == "DaXuLy").ThenByDescending(p => p.NgayGui).ToListAsync());
        }

        [HttpPost("{id:int}/xu-ly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuLy(int id, string? traLoi)
        {
            var ph = await _db.PhanHois.FirstOrDefaultAsync(p => p.MaPhanHoi == id);
            if (ph == null) return NotFound();

            if (string.IsNullOrWhiteSpace(traLoi))
            {
                TempData["LoiThongBao"] = "Vui lòng nhập nội dung trả lời trước khi xử lý.";
                return RedirectToAction("Index");
            }

            ph.TraLoi = traLoi.Trim();
            ph.TrangThai = "DaXuLy";
            ph.NgayXuLy = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã trả lời phản hồi #{ph.MaPhanHoi}.";
            return RedirectToAction("Index");
        }
    }
}
