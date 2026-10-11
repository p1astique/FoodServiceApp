using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.ViewModels;

namespace FoodServiceApp.Web.Controllers
{
    [Route("TaiXe/ThuNhap")]
    public class DriverThuNhapController : TaiXeBaseController
    {
        private readonly AppDbContext _db;

        public DriverThuNhapController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string ky = "Thang")
        {
            // ky = Tuan (7 ngày gần nhất) hoặc Thang (30 ngày gần nhất, mặc định)
            if (ky != "Tuan") ky = "Thang";
            var tuNgay = ky == "Tuan" ? DateTime.Today.AddDays(-6) : DateTime.Today.AddDays(-29);
            ViewBag.KyBaoCao = ky;
            ViewBag.ThuNhapHomNay = 0m;

            var giaoThanhCong = await _db.GiaoHangs
                .Include(g => g.DonHang)
                .Where(g => g.MaTaiXe == CurrentTaiXeId
                            && g.TrangThai == "GiaoThanhCong"
                            && g.ThoiGianGiao >= tuNgay)
                .OrderByDescending(g => g.ThoiGianGiao)
                .ToListAsync();

            var thuNhapTheoNgay = giaoThanhCong
                .GroupBy(g => g.ThoiGianGiao!.Value.Date)
                .OrderBy(g => g.Key)
                .Select(g => (g.Key, g.Sum(x => x.DonHang!.PhiGiaoHang)))
                .ToList();

            ViewBag.ThuNhapHomNay = giaoThanhCong
                .Where(g => g.ThoiGianGiao >= DateTime.Today)
                .Sum(g => g.DonHang!.PhiGiaoHang);

            var vm = new DriverThuNhapViewModel
            {
                TongThuNhap30Ngay = giaoThanhCong.Sum(g => g.DonHang!.PhiGiaoHang),
                TongDonGiaoThanhCong30Ngay = giaoThanhCong.Count,
                ThuNhapTheoNgay = thuNhapTheoNgay,
                LichSuGanDay = giaoThanhCong.Take(20).ToList(),
            };

            return View(vm);
        }
    }
}
