using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.ViewModels;
using FoodServiceApp.Web.Services;

namespace FoodServiceApp.Web.Controllers
{
    [Route("MuaHang")]
    public class CustomerHomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly CartService _cart;
        private const int PageSize = 12;

        public CustomerHomeController(AppDbContext db, CartService cart)
        {
            _db = db;
            _cart = cart;
        }

        [HttpGet("")]
        [HttpGet("/")]
        public async Task<IActionResult> Index(string? tuKhoa, string? khuVuc, double? minSao, string? sapXep, int trang = 1)
        {
            trang = Math.Max(1, trang);
            minSao = minSao is >= 1 and <= 5 ? minSao : null;
            sapXep = string.IsNullOrWhiteSpace(sapXep) ? "phu-hop" : sapXep.Trim().ToLowerInvariant();

            var query =
                from g in _db.GianHangs
                where g.TrangThaiKinhDoanh == "DangMo"
                join dg in _db.DanhGias on g.MaGianHang equals dg.MaGianHang into danhGias
                select new
                {
                    GianHang = g,
                    DiemTrungBinh = danhGias.Select(x => (double?)x.SoSao).Average() ?? 0,
                    SoDanhGia = danhGias.Count()
                };

            // Tìm theo tên gian hàng, địa chỉ/khu vực hoặc tên món thuộc gian hàng.
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var kw = tuKhoa.Trim();
                query = query.Where(x => x.GianHang.TenCuaHang.Contains(kw)
                    || x.GianHang.DiaChi.Contains(kw)
                    || _db.DanhMucs.Any(d => d.MaGianHang == x.GianHang.MaGianHang
                        && d.MonAns.Any(m => m.TenMon.Contains(kw))));
            }

            if (!string.IsNullOrWhiteSpace(khuVuc))
            {
                var kv = khuVuc.Trim();
                query = query.Where(x => x.GianHang.DiaChi.Contains(kv));
            }

            if (minSao.HasValue)
                query = query.Where(x => x.DiemTrungBinh >= minSao.Value && x.SoDanhGia > 0);

            query = sapXep switch
            {
                "danh-gia" => query.OrderByDescending(x => x.DiemTrungBinh).ThenByDescending(x => x.SoDanhGia).ThenBy(x => x.GianHang.TenCuaHang),
                "ten-a-z" => query.OrderBy(x => x.GianHang.TenCuaHang),
                "ten-z-a" => query.OrderByDescending(x => x.GianHang.TenCuaHang),
                _ => query.OrderByDescending(x => x.SoDanhGia > 0).ThenByDescending(x => x.DiemTrungBinh).ThenBy(x => x.GianHang.TenCuaHang)
            };

            var tongKetQua = await query.CountAsync();
            var tongTrang = Math.Max(1, (int)Math.Ceiling(tongKetQua / (double)PageSize));
            if (trang > tongTrang) trang = tongTrang;

            var ketQua = await query
                .Skip((trang - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var danhSach = ketQua.Select(x => new GianHangCardViewModel
            {
                MaGianHang = x.GianHang.MaGianHang,
                TenCuaHang = x.GianHang.TenCuaHang,
                DiaChi = x.GianHang.DiaChi,
                HinhAnh = x.GianHang.HinhAnh,
                TrangThaiKinhDoanh = x.GianHang.TrangThaiKinhDoanh,
                DiemTrungBinh = x.DiemTrungBinh,
                SoDanhGia = x.SoDanhGia,
            }).ToList();

            return View(new CustomerHomeViewModel
            {
                TuKhoa = tuKhoa,
                KhuVuc = khuVuc,
                MinSao = minSao,
                SapXep = sapXep,
                Trang = trang,
                TongTrang = tongTrang,
                TongKetQua = tongKetQua,
                DanhSachGianHang = danhSach,
            });
        }

        [HttpGet("GianHang/{id:int}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var gianHang = await _db.GianHangs.FirstOrDefaultAsync(g => g.MaGianHang == id);
            if (gianHang == null) return NotFound();

            var danhMucs = await _db.DanhMucs
                .Include(d => d.MonAns)
                .Where(d => d.MaGianHang == id)
                .ToListAsync();

            var danhGias = await _db.DanhGias
                .Where(d => d.MaGianHang == id)
                .OrderByDescending(d => d.NgayDanhGia)
                .Take(10)
                .ToListAsync();

            ViewBag.GianHang = gianHang;
            ViewBag.DiemTrungBinh = await _db.DanhGias
                .Where(d => d.MaGianHang == id)
                .Select(d => (double?)d.SoSao)
                .AverageAsync() ?? 0;
            ViewBag.SoDanhGia = await _db.DanhGias.CountAsync(d => d.MaGianHang == id);
            ViewBag.DanhGiaGanDay = danhGias;
            ViewBag.GioHang = _cart.LayGioHang();

            return View(danhMucs);
        }

        [HttpGet("Mon/{id:int}")]
        public async Task<IActionResult> ChiTietMon(int id)
        {
            var mon = await _db.MonAns.Include(m => m.DanhMuc).FirstOrDefaultAsync(m => m.MaMonAn == id);
            if (mon == null || mon.DanhMuc == null) return NotFound();

            var gianHang = await _db.GianHangs.FirstAsync(g => g.MaGianHang == mon.DanhMuc.MaGianHang);

            var soSao = await _db.DanhGiaMons.Where(d => d.MaMonAn == id).Select(d => d.SoSao).ToListAsync();
            var danhGias = await (from d in _db.DanhGiaMons
                                  join k in _db.KhachHangs on d.MaKH equals k.MaKH
                                  where d.MaMonAn == id
                                  orderby d.NgayDanhGia descending
                                  select new DanhGiaMonHienThi
                                  {
                                      TenKhach = k.HoTen,
                                      SoSao = d.SoSao,
                                      NoiDung = d.NoiDung,
                                      Ngay = d.NgayDanhGia,
                                  }).Take(10).ToListAsync();

            return View(new MonChiTietViewModel
            {
                Mon = mon,
                GianHang = gianHang,
                DiemTrungBinh = soSao.Any() ? soSao.Average() : 0,
                SoDanhGia = soSao.Count,
                DanhGias = danhGias,
            });
        }

        [HttpPost("GianHang/{maGianHang:int}/them-gio")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemVaoGio(int maGianHang, int maMonAn, int soLuong = 1, bool veTrangMon = false)
        {
            soLuong = Math.Clamp(soLuong, 1, 50);

            var gianHang = await _db.GianHangs.FirstOrDefaultAsync(g => g.MaGianHang == maGianHang);
            var monAn = await _db.MonAns
                .Include(m => m.DanhMuc)
                .FirstOrDefaultAsync(m => m.MaMonAn == maMonAn);

            if (gianHang == null || monAn == null || monAn.DanhMuc?.MaGianHang != maGianHang)
                return NotFound();

            if (gianHang.TrangThaiKinhDoanh != "DangMo")
            {
                TempData["LoiThongBao"] = "Gian hàng hiện không nhận đơn.";
                return RedirectToAction("ChiTiet", new { id = maGianHang });
            }

            if (monAn.TinhTrang != "ConHang")
            {
                TempData["LoiThongBao"] = "Món này hiện đã hết hàng.";
                return RedirectToAction("ChiTiet", new { id = maGianHang });
            }

            var loi = _cart.ThemMon(maGianHang, gianHang.TenCuaHang, new CartItem
            {
                MaMonAn = monAn.MaMonAn,
                TenMon = monAn.TenMon,
                Gia = monAn.Gia,
                SoLuong = soLuong,
                HinhAnh = monAn.HinhAnh,
            });

            if (loi != null) TempData["LoiThongBao"] = loi;
            else TempData["ThongBao"] = $"Đã thêm \"{monAn.TenMon}\" vào giỏ hàng.";

            if (veTrangMon) return RedirectToAction("ChiTietMon", new { id = maMonAn });
            return RedirectToAction("ChiTiet", new { id = maGianHang });
        }
    }
}
