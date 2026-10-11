using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Models.ViewModels;

namespace FoodServiceApp.Web.Controllers
{
    // Sổ địa chỉ giao hàng của khách: thêm, xóa, đặt mặc định. Trang Checkout cho chọn nhanh từ sổ này.
    [Route("MuaHang/DiaChi")]
    public class CustomerDiaChiController : CustomerBaseController
    {
        private const int ToiDaDiaChi = 10;
        private readonly AppDbContext _db;

        public CustomerDiaChiController(AppDbContext db)
        {
            _db = db;
        }

        private async Task<List<DiaChiKhachHang>> LayDanhSachAsync() =>
            await _db.DiaChiKhachHangs.Where(d => d.MaKH == CurrentMaKH)
                .OrderByDescending(d => d.MacDinh).ThenBy(d => d.MaDiaChi).ToListAsync();

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewBag.DanhSach = await LayDanhSachAsync();
            return View(new DiaChiFormViewModel());
        }

        [HttpPost("them")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(DiaChiFormViewModel model)
        {
            var soHienCo = await _db.DiaChiKhachHangs.CountAsync(d => d.MaKH == CurrentMaKH);
            if (soHienCo >= ToiDaDiaChi)
                ModelState.AddModelError("", $"Mỗi tài khoản chỉ lưu tối đa {ToiDaDiaChi} địa chỉ.");

            if (!ModelState.IsValid)
            {
                ViewBag.DanhSach = await LayDanhSachAsync();
                return View("Index", model);
            }

            // Địa chỉ đầu tiên luôn là mặc định
            var laMacDinh = model.MacDinh || soHienCo == 0;
            if (laMacDinh)
            {
                var cu = await _db.DiaChiKhachHangs.Where(d => d.MaKH == CurrentMaKH && d.MacDinh).ToListAsync();
                foreach (var c in cu) c.MacDinh = false;
            }

            _db.DiaChiKhachHangs.Add(new DiaChiKhachHang
            {
                MaKH = CurrentMaKH,
                Nhan = model.Nhan.Trim(),
                DiaChi = model.DiaChi.Trim(),
                MacDinh = laMacDinh,
            });
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Đã thêm địa chỉ giao hàng.";
            return RedirectToAction("Index");
        }

        [HttpPost("{id:int}/mac-dinh")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatMacDinh(int id)
        {
            var ds = await _db.DiaChiKhachHangs.Where(d => d.MaKH == CurrentMaKH).ToListAsync();
            var chon = ds.FirstOrDefault(d => d.MaDiaChi == id);
            if (chon == null) return NotFound();

            foreach (var d in ds) d.MacDinh = d.MaDiaChi == id;
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Đã đặt làm địa chỉ mặc định.";
            return RedirectToAction("Index");
        }

        [HttpPost("{id:int}/xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var dc = await _db.DiaChiKhachHangs.FirstOrDefaultAsync(d => d.MaDiaChi == id && d.MaKH == CurrentMaKH);
            if (dc == null) return NotFound();

            var laMacDinh = dc.MacDinh;
            _db.DiaChiKhachHangs.Remove(dc);
            await _db.SaveChangesAsync();

            // Nếu xóa địa chỉ mặc định thì chuyển mặc định sang địa chỉ còn lại đầu tiên
            if (laMacDinh)
            {
                var conLai = await _db.DiaChiKhachHangs.Where(d => d.MaKH == CurrentMaKH).OrderBy(d => d.MaDiaChi).FirstOrDefaultAsync();
                if (conLai != null) { conLai.MacDinh = true; await _db.SaveChangesAsync(); }
            }

            TempData["ThongBao"] = "Đã xóa địa chỉ.";
            return RedirectToAction("Index");
        }
    }
}
