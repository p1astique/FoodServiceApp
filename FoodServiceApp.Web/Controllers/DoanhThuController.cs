using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.ViewModels;

namespace FoodServiceApp.Web.Controllers
{
    public class DoanhThuController : GianHangBaseController
    {
        private readonly AppDbContext _db;

        public DoanhThuController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string ky = "30ngay")
        {
            var denNgay = DateTime.Today.AddDays(1);
            var tuNgay = ky switch { "ngay" => DateTime.Today, "thang" => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                "nam" => new DateTime(DateTime.Today.Year, 1, 1), _ => DateTime.Today.AddDays(-29) };

            var donHoanThanh = await _db.DonHangs
                .Include(d => d.ChiTiets).ThenInclude(c => c.MonAn)
                .Where(d => d.MaGianHang == CurrentGianHangId
                            && d.TrangThai == "DaGiao"
                            && d.NgayDat >= tuNgay && d.NgayDat < denNgay)
                .ToListAsync();

            var doanhThuTheoNgay = donHoanThanh
                .GroupBy(d => d.NgayDat.Date)
                .OrderBy(g => g.Key)
                .Select(g => (g.Key, g.Sum(d => d.TongTien - d.PhiGiaoHang)))
                .ToList();

            var monBanChay = donHoanThanh
                .SelectMany(d => d.ChiTiets)
                .GroupBy(c => string.IsNullOrEmpty(c.TenMon) ? c.MonAn?.TenMon ?? "Không rõ" : c.TenMon)
                .Select(g => (g.Key, g.Sum(c => c.SoLuong)))
                .OrderByDescending(x => x.Item2)
                .Take(5)
                .ToList();

            var gross = donHoanThanh.Sum(d => d.TongTien - d.PhiGiaoHang);
            var fee = Math.Round(gross * 0.10m, 0);
            var vm = new DoanhThuViewModel
            {
                Ky = ky, TuNgay = tuNgay, DenNgay = denNgay.AddDays(-1),
                TongDoanhThu30Ngay = gross, PhiNenTang = fee, ThucNhan = gross - fee,
                TongDonHoanThanh30Ngay = donHoanThanh.Count,
                DoanhThuTheoNgay = doanhThuTheoNgay,
                MonBanChay = monBanChay,
            };

            return View(vm);
        }
    }
}
