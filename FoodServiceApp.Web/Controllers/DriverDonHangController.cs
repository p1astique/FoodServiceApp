using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Entities;

namespace FoodServiceApp.Web.Controllers
{
    [Route("TaiXe/DonHang")]
    public class DriverDonHangController : TaiXeBaseController
    {
        private readonly AppDbContext _db;

        public DriverDonHangController(AppDbContext db)
        {
            _db = db;
        }

        // Danh sách đơn đã được gian hàng xác nhận, đang chờ tài xế nhận
        [HttpGet("ChoNhan")]
        public async Task<IActionResult> ChoNhan()
        {
            var dangGiao = await _db.DonHangs.AnyAsync(d => d.MaTaiXe == CurrentTaiXeId && d.TrangThai == "DangGiao");
            if (dangGiao)
            {
                TempData["LoiThongBao"] = "Bạn đang có một chuyến giao chưa hoàn thành, hãy xử lý xong trước khi nhận đơn mới.";
                return RedirectToAction("DonHienTai");
            }

            // Đơn có địa chỉ giao hoặc địa chỉ gian hàng trùng khu vực hoạt động của tài xế được
            // đánh dấu "Gần bạn" và xếp lên đầu; không lọc hẳn để tài xế vẫn thấy mọi đơn khác.
            var taiXe = await _db.TaiXes.FirstAsync(t => t.MaTaiXe == CurrentTaiXeId);
            var khuVucs = (taiXe.KhuVucHoatDong ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var danhSach = await _db.DonHangs
                .Include(d => d.GianHang)
                .Include(d => d.ChiTiets)
                .Where(d => d.TrangThai == "DaXacNhan" && d.MaTaiXe == null
                    && !_db.TaiXeTuChoiDons.Any(t => t.MaTaiXe == CurrentTaiXeId && t.MaDonHang == d.MaDonHang))
                .OrderBy(d => d.NgayDat)
                .ToListAsync();

            bool GanKhuVuc(DonHang d) => khuVucs.Length > 0 && khuVucs.Any(kv =>
                (d.DiaChiGiaoHang?.Contains(kv, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (d.GianHang?.DiaChi?.Contains(kv, StringComparison.OrdinalIgnoreCase) ?? false));

            var sapXep = danhSach.OrderByDescending(GanKhuVuc).ThenBy(d => d.NgayDat).ToList();
            ViewBag.DonGanKhuVuc = sapXep.Where(GanKhuVuc).Select(d => d.MaDonHang).ToHashSet();
            ViewBag.KhuVucHoatDong = taiXe.KhuVucHoatDong;
            return View(sapXep);
        }

        // Tài xế từ chối đơn: đơn được ẩn khỏi danh sách của riêng tài xế này, các tài xế khác vẫn thấy
        [HttpPost("TuChoi/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoi(int id)
        {
            var conCho = await _db.DonHangs.AnyAsync(d => d.MaDonHang == id && d.TrangThai == "DaXacNhan" && d.MaTaiXe == null);
            var daTuChoi = await _db.TaiXeTuChoiDons.AnyAsync(t => t.MaTaiXe == CurrentTaiXeId && t.MaDonHang == id);
            if (conCho && !daTuChoi)
            {
                _db.TaiXeTuChoiDons.Add(new TaiXeTuChoiDon { MaTaiXe = CurrentTaiXeId, MaDonHang = id });
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = $"Đã từ chối đơn #{id}.";
            }
            return RedirectToAction("ChoNhan");
        }

        [HttpPost("Nhan/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nhan(int id)
        {
            var dangGiao = await _db.DonHangs.AnyAsync(d => d.MaTaiXe == CurrentTaiXeId && d.TrangThai == "DangGiao");
            if (dangGiao)
            {
                TempData["LoiThongBao"] = "Bạn đang có một chuyến giao chưa hoàn thành.";
                return RedirectToAction("DonHienTai");
            }

            var don = await _db.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == id && d.TrangThai == "DaXacNhan" && d.MaTaiXe == null);
            if (don == null)
            {
                TempData["LoiThongBao"] = "Đơn này đã có tài xế khác nhận hoặc không còn tồn tại.";
                return RedirectToAction("ChoNhan");
            }

            don.MaTaiXe = CurrentTaiXeId;
            don.TrangThai = "DangGiao";

            _db.GiaoHangs.Add(new GiaoHang
            {
                MaDonHang = don.MaDonHang,
                MaTaiXe = CurrentTaiXeId,
                TrangThai = "DaNhan",
                ThoiGianNhan = DateTime.Now,
            });

            var taiXe = await _db.TaiXes.FirstAsync(t => t.MaTaiXe == CurrentTaiXeId);
            taiXe.TrangThai = "DangGiao";

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã nhận đơn #{don.MaDonHang}.";
            return RedirectToAction("DonHienTai");
        }

        // Chuyến giao đang thực hiện của tài xế hiện tại
        [HttpGet("HienTai")]
        public async Task<IActionResult> DonHienTai()
        {
            var don = await _db.DonHangs
                .Include(d => d.GianHang)
                .Include(d => d.ChiTiets).ThenInclude(c => c.MonAn)
                .Include(d => d.GiaoHang)
                .FirstOrDefaultAsync(d => d.MaTaiXe == CurrentTaiXeId && d.TrangThai == "DangGiao");

            return View(don);
        }

        [HttpPost("BatDauGiao/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatDauGiao(int id)
        {
            var giaoHang = await _db.GiaoHangs.FirstOrDefaultAsync(g => g.MaDonHang == id && g.MaTaiXe == CurrentTaiXeId);
            if (giaoHang == null) return NotFound();

            giaoHang.TrangThai = "DangGiao";
            await _db.SaveChangesAsync();
            return RedirectToAction("DonHienTai");
        }

        [HttpPost("HoanThanh/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanThanh(int id)
        {
            var don = await _db.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaTaiXe == CurrentTaiXeId);
            var giaoHang = await _db.GiaoHangs.FirstOrDefaultAsync(g => g.MaDonHang == id && g.MaTaiXe == CurrentTaiXeId);
            if (don == null || giaoHang == null) return NotFound();

            don.TrangThai = "DaGiao";
            giaoHang.TrangThai = "GiaoThanhCong";
            giaoHang.ThoiGianGiao = DateTime.Now;

            var taiXe = await _db.TaiXes.FirstAsync(t => t.MaTaiXe == CurrentTaiXeId);
            taiXe.TrangThai = "SanSang";

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã hoàn thành giao đơn #{don.MaDonHang}.";
            return RedirectToAction("Index", "DriverDashboard");
        }

        [HttpPost("GiaoThatBai/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiaoThatBai(int id, string lyDo)
        {
            var don = await _db.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaTaiXe == CurrentTaiXeId);
            var giaoHang = await _db.GiaoHangs.FirstOrDefaultAsync(g => g.MaDonHang == id && g.MaTaiXe == CurrentTaiXeId);
            if (don == null || giaoHang == null) return NotFound();

            giaoHang.TrangThai = "GiaoThatBai";
            don.GhiChu = string.IsNullOrEmpty(don.GhiChu) ? $"Giao thất bại: {lyDo}" : $"{don.GhiChu} | Giao thất bại: {lyDo}";
            // Đơn giữ trạng thái DangGiao — cần Quản trị/Gian hàng xử lý tiếp (giao lại hoặc hủy đơn thủ công).

            var taiXe = await _db.TaiXes.FirstAsync(t => t.MaTaiXe == CurrentTaiXeId);
            taiXe.TrangThai = "SanSang";

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã ghi nhận giao thất bại đơn #{don.MaDonHang}. Gian hàng/Quản trị sẽ xử lý tiếp.";
            return RedirectToAction("Index", "DriverDashboard");
        }

        [HttpGet("LichSu")]
        public async Task<IActionResult> LichSu()
        {
            var danhSach = await _db.GiaoHangs
                .Include(g => g.DonHang).ThenInclude(d => d!.GianHang)
                .Where(g => g.MaTaiXe == CurrentTaiXeId && (g.TrangThai == "GiaoThanhCong" || g.TrangThai == "GiaoThatBai"))
                .OrderByDescending(g => g.ThoiGianGiao)
                .Take(50)
                .ToListAsync();

            return View(danhSach);
        }
    }
}
