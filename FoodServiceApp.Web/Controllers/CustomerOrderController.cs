using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Models.ViewModels;

namespace FoodServiceApp.Web.Controllers
{
    [Route("MuaHang/DonHang")]
    public class CustomerOrderController : CustomerBaseController
    {
        private readonly AppDbContext _db;

        public CustomerOrderController(AppDbContext db)
        {
            _db = db;
        }

        // Đơn đang xử lý (chưa hoàn tất/huỷ) — "theo dõi trạng thái đơn hàng và giao hàng"
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var danhSach = await _db.DonHangs
                .Include(d => d.GianHang)
                .Include(d => d.ChiTiets).ThenInclude(c => c.MonAn)
                .Where(d => d.MaKH == CurrentMaKH && d.TrangThai != "DaGiao" && d.TrangThai != "DaHuy")
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(danhSach);
        }

        // Lịch sử mua hàng — đơn đã hoàn tất hoặc đã huỷ
        [HttpGet("LichSu")]
        public async Task<IActionResult> LichSu()
        {
            var danhSach = await _db.DonHangs
                .Include(d => d.GianHang)
                .Include(d => d.ChiTiets).ThenInclude(c => c.MonAn)
                .Where(d => d.MaKH == CurrentMaKH && (d.TrangThai == "DaGiao" || d.TrangThai == "DaHuy"))
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            var maDonDaDanhGia = await _db.DanhGias
                .Where(dg => dg.MaKH == CurrentMaKH)
                .Select(dg => dg.MaDonHang)
                .ToListAsync();

            ViewBag.MaDonDaDanhGia = maDonDaDanhGia;
            return View(danhSach);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var don = await _db.DonHangs
                .Include(d => d.GianHang)
                .Include(d => d.ChiTiets).ThenInclude(c => c.MonAn)
                .Include(d => d.ThanhToan)
                .Include(d => d.GiaoHang).ThenInclude(g => g!.TaiXe)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaKH == CurrentMaKH);

            if (don == null) return NotFound();
            return View(don);
        }

        [HttpPost("{id:int}/huy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDon(int id)
        {
            var don = await _db.DonHangs.Include(d => d.ThanhToan)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaKH == CurrentMaKH);
            if (don == null) return NotFound();

            if (don.TrangThai != "ChoXacNhan")
            {
                TempData["LoiThongBao"] = "Đơn đã được gian hàng xác nhận, không thể tự huỷ. Vui lòng liên hệ gian hàng.";
                return RedirectToAction("ChiTiet", new { id });
            }

            don.TrangThai = "DaHuy";

            // Đơn đã thanh toán online mà bị huỷ -> đánh dấu hoàn tiền (xử lý hoàn tiền thật
            // qua cổng thanh toán cần gọi thêm API của VNPay/Momo, ở đây cập nhật trạng thái
            // trong CSDL để Quản trị/Gian hàng theo dõi và đối soát).
            if (don.ThanhToan != null && don.ThanhToan.TrangThai == "DaThanhToan")
                don.ThanhToan.TrangThai = "DaHoanTien";

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã huỷ đơn #{don.MaDonHang}.";
            return RedirectToAction("Index");
        }

        [HttpGet("{id:int}/danh-gia")]
        public async Task<IActionResult> DanhGia(int id)
        {
            var don = await _db.DonHangs.Include(d => d.GianHang)
                .Include(d => d.ChiTiets).ThenInclude(c => c.MonAn)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaKH == CurrentMaKH && d.TrangThai == "DaGiao");
            if (don == null) return NotFound();

            if (await _db.DanhGias.AnyAsync(dg => dg.MaDonHang == id))
            {
                TempData["LoiThongBao"] = "Đơn này đã được đánh giá rồi.";
                return RedirectToAction("LichSu");
            }

            return View(new DanhGiaFormViewModel
            {
                MaDonHang = don.MaDonHang,
                MaGianHang = don.MaGianHang,
                TenGianHang = don.GianHang!.TenCuaHang,
                Mons = don.ChiTiets
                    .GroupBy(c => c.MaMonAn)
                    .Select(g => new MonDanhGiaItem { MaMonAn = g.Key, TenMon = g.First().MonAn?.TenMon ?? "Món ăn" })
                    .ToList(),
            });
        }

        [HttpPost("{id:int}/danh-gia")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhGia(int id, DanhGiaFormViewModel model)
        {
            var don = await _db.DonHangs.Include(d => d.ChiTiets)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaKH == CurrentMaKH && d.TrangThai == "DaGiao");
            if (don == null) return NotFound();

            if (await _db.DanhGias.AnyAsync(dg => dg.MaDonHang == id))
                return RedirectToAction("LichSu");

            if (!ModelState.IsValid) return View(model);

            // Chỉ nhận đánh giá cho các món thật sự nằm trong đơn này (chống sửa form)
            var monTrongDon = don.ChiTiets.Select(c => c.MaMonAn).ToHashSet();
            foreach (var m in model.Mons.Where(m => m.SoSao >= 1 && m.SoSao <= 5 && monTrongDon.Contains(m.MaMonAn)))
            {
                _db.DanhGiaMons.Add(new DanhGiaMon
                {
                    MaKH = CurrentMaKH,
                    MaMonAn = m.MaMonAn,
                    MaDonHang = id,
                    SoSao = m.SoSao,
                    NoiDung = m.NoiDung,
                });
            }

            _db.DanhGias.Add(new DanhGia
            {
                MaKH = CurrentMaKH,
                MaGianHang = don.MaGianHang,
                MaDonHang = id,
                SoSao = model.SoSao,
                NoiDung = model.NoiDung,
            });
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Cảm ơn bạn đã đánh giá!";
            return RedirectToAction("LichSu");
        }
    }
}
