using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;

namespace FoodServiceApp.Web.Controllers
{
    public class DonHangController : GianHangBaseController
    {
        private readonly AppDbContext _db;

        public DonHangController(AppDbContext db)
        {
            _db = db;
        }

        // GET /DonHang?trangThai=ChoXacNhan
        public async Task<IActionResult> Index(string? trangThai)
        {
            var query = _db.DonHangs
                .Include(d => d.ChiTiets).ThenInclude(c => c.MonAn)
                .Where(d => d.MaGianHang == CurrentGianHangId);

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            var danhSach = await query.OrderByDescending(d => d.NgayDat).ToListAsync();

            ViewBag.TrangThaiDangLoc = trangThai ?? "";
            return View(danhSach);
        }

        public async Task<IActionResult> ChiTiet(int id)
        {
            var don = await _db.DonHangs
                .Include(d => d.ChiTiets).ThenInclude(c => c.MonAn)
                .Include(d => d.ThanhToan)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaGianHang == CurrentGianHangId);

            if (don == null) return NotFound();
            ViewBag.NhatKy = await _db.NhatKyTrangThaiDons.Where(x => x.MaDonHang == id).OrderBy(x => x.ThoiGian).ToListAsync();
            return View(don);
        }

        // Luồng trạng thái hợp lệ: ChoXacNhan -> DaXacNhan -> DangGiao -> DaGiao
        //                          ChoXacNhan/DaXacNhan -> DaHuy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, string trangThaiMoi, string? lyDo)
        {
            var don = await _db.DonHangs.Include(d => d.ThanhToan)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaGianHang == CurrentGianHangId);
            if (don == null) return NotFound();

            if (await FoodServiceApp.Web.Services.VendorOrderService.UpdateAsync(_db, CurrentGianHangId, id, trangThaiMoi, lyDo))
                TempData["ThongBao"] = trangThaiMoi == "DaHuy" && don.ThanhToan?.TrangThai == "DaThanhToan"
                    ? "Đã hủy đơn. Khoản đã thanh toán cần quản trị đối soát hoàn tiền."
                    : "Đã cập nhật trạng thái đơn.";
            else
                TempData["LoiThongBao"] = "Đơn đã đổi trạng thái hoặc thao tác không hợp lệ. Hãy tải lại.";
            return RedirectToAction("Index");
        }
    }
}
