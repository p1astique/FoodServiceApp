using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Models.ViewModels;

namespace FoodServiceApp.Web.Controllers
{
    [Route("QuanTri/KhuyenMai")]
    public class AdminKhuyenMaiController : QuanTriBaseController
    {
        private readonly AppDbContext _db;

        public AdminKhuyenMaiController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var danhSach = await _db.KhuyenMais
                .Where(k => k.MaGianHang == null)
                .OrderByDescending(k => k.NgayBatDau)
                .ToListAsync();
            return View(danhSach);
        }

        [HttpGet("Create")]
        public IActionResult Create() => View(new KhuyenMaiFormViewModel());

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhuyenMaiFormViewModel vm)
        {
            if (vm.NgayKetThuc <= vm.NgayBatDau)
                ModelState.AddModelError(nameof(vm.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");

            if (await _db.KhuyenMais.AnyAsync(k => k.MaCode == vm.MaCode))
                ModelState.AddModelError(nameof(vm.MaCode), "Mã khuyến mãi này đã tồn tại.");

            if (!ModelState.IsValid) return View(vm);

            var km = new KhuyenMai
            {
                MaGianHang = null, // toàn hệ thống
                MaCode = vm.MaCode.Trim().ToUpper(),
                MoTa = vm.MoTa,
                DieuKienApDung = vm.DieuKienApDung,
                NgayBatDau = vm.NgayBatDau,
                NgayKetThuc = vm.NgayKetThuc,
                TrangThai = vm.TrangThai,
                PhanTramGiam = vm.LoaiGiam == "PhanTram" ? vm.GiaTriGiam : null,
                SoTienGiam = vm.LoaiGiam == "SoTien" ? vm.GiaTriGiam : null,
            };

            _db.KhuyenMais.Add(km);
            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã tạo mã khuyến mãi toàn hệ thống {km.MaCode}.";
            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var km = await _db.KhuyenMais.FirstOrDefaultAsync(k => k.MaKhuyenMai == id && k.MaGianHang == null);
            if (km == null) return NotFound();

            var vm = new KhuyenMaiFormViewModel
            {
                MaKhuyenMai = km.MaKhuyenMai,
                MaCode = km.MaCode,
                MoTa = km.MoTa,
                DieuKienApDung = km.DieuKienApDung,
                LoaiGiam = km.PhanTramGiam != null ? "PhanTram" : "SoTien",
                GiaTriGiam = km.PhanTramGiam ?? km.SoTienGiam ?? 0,
                NgayBatDau = km.NgayBatDau,
                NgayKetThuc = km.NgayKetThuc,
                TrangThai = km.TrangThai,
            };
            return View(vm);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhuyenMaiFormViewModel vm)
        {
            var km = await _db.KhuyenMais.FirstOrDefaultAsync(k => k.MaKhuyenMai == id && k.MaGianHang == null);
            if (km == null) return NotFound();

            if (vm.NgayKetThuc <= vm.NgayBatDau)
                ModelState.AddModelError(nameof(vm.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");

            if (!ModelState.IsValid) return View(vm);

            km.MoTa = vm.MoTa;
            km.DieuKienApDung = vm.DieuKienApDung;
            km.NgayBatDau = vm.NgayBatDau;
            km.NgayKetThuc = vm.NgayKetThuc;
            km.TrangThai = vm.TrangThai;
            km.PhanTramGiam = vm.LoaiGiam == "PhanTram" ? vm.GiaTriGiam : null;
            km.SoTienGiam = vm.LoaiGiam == "SoTien" ? vm.GiaTriGiam : null;

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã cập nhật mã {km.MaCode}.";
            return RedirectToAction("Index");
        }

        [HttpPost("ToggleTrangThai/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var km = await _db.KhuyenMais.FirstOrDefaultAsync(k => k.MaKhuyenMai == id && k.MaGianHang == null);
            if (km == null) return NotFound();

            km.TrangThai = !km.TrangThai;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
