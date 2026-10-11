using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Dtos;
using FoodServiceApp.Web.Models.Entities;

namespace FoodServiceApp.Web.Controllers.Api
{
    [ApiController]
    [Route("api/driver")]
    public class DriverController : DriverApiBaseController
    {
        private readonly AppDbContext _db;

        public DriverController(AppDbContext db)
        {
            _db = db;
        }

        private async Task<int> GetMaTaiXeAsync() =>
            (await _db.TaiXes.FirstAsync(t => t.MaTK == CurrentMaTK)).MaTaiXe;

        private static DonHangDto MapDonHang(DonHang d) => new(
            d.MaDonHang, d.TrangThai, d.TongTien, d.NgayDat, d.DiaChiGiaoHang, d.GhiChu,
            d.GianHang!.TenCuaHang, d.GianHang.DiaChi,
            d.ChiTiets.Select(c => new ChiTietDonHangDto(c.MonAn!.TenMon, c.SoLuong, c.DonGia, c.ThanhTien)).ToList(),
            d.PhiGiaoHang);

        [HttpGet("home")]
        public async Task<IActionResult> Home()
        {
            var maTaiXe = await GetMaTaiXeAsync();
            var taiXe = await _db.TaiXes.FirstAsync(t => t.MaTaiXe == maTaiXe);
            var homNay = DateTime.Today;

            var donDangGiao = await _db.DonHangs
                .Include(d => d.GianHang)
                .Include(d => d.ChiTiets).ThenInclude(c => c.MonAn)
                .FirstOrDefaultAsync(d => d.MaTaiXe == maTaiXe && d.TrangThai == "DangGiao");

            var thuNhapHomNay = await _db.GiaoHangs
                .Include(g => g.DonHang)
                .Where(g => g.MaTaiXe == maTaiXe && g.TrangThai == "GiaoThanhCong" && g.ThoiGianGiao >= homNay)
                .SumAsync(g => g.DonHang!.PhiGiaoHang);

            return Ok(new DriverHomeDto(
                taiXe.HoTen, taiXe.TrangThai,
                await _db.GiaoHangs.CountAsync(g => g.MaTaiXe == maTaiXe && g.TrangThai == "GiaoThanhCong" && g.ThoiGianGiao >= homNay),
                thuNhapHomNay,
                donDangGiao == null ? null : MapDonHang(donDangGiao),
                await _db.DonHangs.CountAsync(d => d.TrangThai == "DaXacNhan" && d.MaTaiXe == null)
            ));
        }

        [HttpPost("toggle-san-sang")]
        public async Task<IActionResult> ToggleSanSang()
        {
            var maTaiXe = await GetMaTaiXeAsync();
            var taiXe = await _db.TaiXes.FirstAsync(t => t.MaTaiXe == maTaiXe);
            if (taiXe.TrangThai == "DangGiao")
                return BadRequest(new { message = "Đang trong chuyến giao, không thể đổi trạng thái." });

            taiXe.TrangThai = taiXe.TrangThai == "SanSang" ? "NgoaiTuyen" : "SanSang";
            await _db.SaveChangesAsync();
            return Ok(new { taiXe.TrangThai });
        }

        [HttpGet("available-orders")]
        public async Task<IActionResult> AvailableOrders()
        {
            var donHangs = await _db.DonHangs
                .Include(d => d.GianHang)
                .Include(d => d.ChiTiets).ThenInclude(c => c.MonAn)
                .Where(d => d.TrangThai == "DaXacNhan" && d.MaTaiXe == null)
                .OrderBy(d => d.NgayDat)
                .ToListAsync();

            return Ok(donHangs.Select(MapDonHang));
        }

        [HttpPost("orders/{id:int}/accept")]
        public async Task<IActionResult> AcceptOrder(int id)
        {
            var maTaiXe = await GetMaTaiXeAsync();

            var dangGiao = await _db.DonHangs.AnyAsync(d => d.MaTaiXe == maTaiXe && d.TrangThai == "DangGiao");
            if (dangGiao) return BadRequest(new { message = "Bạn đang có chuyến giao chưa hoàn thành." });

            var don = await _db.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == id && d.TrangThai == "DaXacNhan" && d.MaTaiXe == null);
            if (don == null) return Conflict(new { message = "Đơn đã có tài xế khác nhận." });

            don.MaTaiXe = maTaiXe;
            don.TrangThai = "DangGiao";
            _db.GiaoHangs.Add(new GiaoHang { MaDonHang = id, MaTaiXe = maTaiXe, TrangThai = "DaNhan", ThoiGianNhan = DateTime.Now });

            var taiXe = await _db.TaiXes.FirstAsync(t => t.MaTaiXe == maTaiXe);
            taiXe.TrangThai = "DangGiao";

            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã nhận đơn." });
        }

        [HttpPost("orders/{id:int}/start")]
        public async Task<IActionResult> StartDelivery(int id)
        {
            var maTaiXe = await GetMaTaiXeAsync();
            var giaoHang = await _db.GiaoHangs.FirstOrDefaultAsync(g => g.MaDonHang == id && g.MaTaiXe == maTaiXe);
            if (giaoHang == null) return NotFound();

            giaoHang.TrangThai = "DangGiao";
            await _db.SaveChangesAsync();
            return Ok(new { giaoHang.TrangThai });
        }

        [HttpPost("orders/{id:int}/complete")]
        public async Task<IActionResult> CompleteDelivery(int id)
        {
            var maTaiXe = await GetMaTaiXeAsync();
            var don = await _db.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaTaiXe == maTaiXe);
            var giaoHang = await _db.GiaoHangs.FirstOrDefaultAsync(g => g.MaDonHang == id && g.MaTaiXe == maTaiXe);
            if (don == null || giaoHang == null) return NotFound();

            don.TrangThai = "DaGiao";
            giaoHang.TrangThai = "GiaoThanhCong";
            giaoHang.ThoiGianGiao = DateTime.Now;

            var taiXe = await _db.TaiXes.FirstAsync(t => t.MaTaiXe == maTaiXe);
            taiXe.TrangThai = "SanSang";

            await _db.SaveChangesAsync();
            return Ok(new { message = "Hoàn thành giao hàng." });
        }

        [HttpPost("orders/{id:int}/fail")]
        public async Task<IActionResult> FailDelivery(int id, [FromBody] string lyDo)
        {
            var maTaiXe = await GetMaTaiXeAsync();
            var don = await _db.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaTaiXe == maTaiXe);
            var giaoHang = await _db.GiaoHangs.FirstOrDefaultAsync(g => g.MaDonHang == id && g.MaTaiXe == maTaiXe);
            if (don == null || giaoHang == null) return NotFound();

            giaoHang.TrangThai = "GiaoThatBai";
            don.GhiChu = string.IsNullOrEmpty(don.GhiChu) ? $"Giao thất bại: {lyDo}" : $"{don.GhiChu} | Giao thất bại: {lyDo}";

            var taiXe = await _db.TaiXes.FirstAsync(t => t.MaTaiXe == maTaiXe);
            taiXe.TrangThai = "SanSang";

            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã ghi nhận giao thất bại." });
        }
    }
}
