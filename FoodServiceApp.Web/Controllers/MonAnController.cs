using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Models.ViewModels;
using FoodServiceApp.Web.Services;

namespace FoodServiceApp.Web.Controllers
{
    public class MonAnController : GianHangBaseController
    {
        private readonly AppDbContext _db;
        private readonly IFileStorageService _storage;

        public MonAnController(AppDbContext db, IFileStorageService storage)
        {
            _db = db;
            _storage = storage;
        }

        public async Task<IActionResult> Index()
        {
            var danhSach = await _db.MonAns
                .Include(m => m.DanhMuc)
                .Where(m => m.DanhMuc!.MaGianHang == CurrentGianHangId)
                .OrderBy(m => m.DanhMuc!.TenDanhMuc).ThenBy(m => m.TenMon)
                .ToListAsync();

            return View(danhSach);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new MonAnFormViewModel
            {
                DanhSachDanhMuc = await _db.DanhMucs.Where(d => d.MaGianHang == CurrentGianHangId).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonAnFormViewModel vm)
        {
            // Cho phép tạo danh mục mới ngay trong form nếu người dùng nhập TenDanhMucMoi
            if (ModelState.IsValid && vm.MaDanhMuc == 0 && !string.IsNullOrWhiteSpace(vm.TenDanhMucMoi))
            {
                var danhMucMoi = new DanhMuc { MaGianHang = CurrentGianHangId, TenDanhMuc = vm.TenDanhMucMoi.Trim() };
                _db.DanhMucs.Add(danhMucMoi);
                await _db.SaveChangesAsync();
                vm.MaDanhMuc = danhMucMoi.MaDanhMuc;
            }

            if (vm.MaDanhMuc == 0)
                ModelState.AddModelError(nameof(vm.MaDanhMuc), "Vui lòng chọn hoặc nhập danh mục.");

            if (!await _db.DanhMucs.AnyAsync(d => d.MaDanhMuc == vm.MaDanhMuc && d.MaGianHang == CurrentGianHangId))
                ModelState.AddModelError(nameof(vm.MaDanhMuc), "Danh mục không thuộc gian hàng của bạn.");
            if (!ModelState.IsValid)
            {
                vm.DanhSachDanhMuc = await _db.DanhMucs.Where(d => d.MaGianHang == CurrentGianHangId).ToListAsync();
                return View(vm);
            }

            var monAn = new MonAn
            {
                MaDanhMuc = vm.MaDanhMuc,
                TenMon = vm.TenMon,
                Gia = vm.Gia,
                MoTa = vm.MoTa,
                KichCo = vm.KichCo,
                Topping = vm.Topping,
                MucCay = vm.MucCay,
                TinhTrang = vm.ConHang ? "ConHang" : "HetHang",
            };

            if (vm.AnhMoi != null)
                monAn.HinhAnh = await _storage.SaveFileAsync(vm.AnhMoi, "monan");

            _db.MonAns.Add(monAn);
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã thêm món \"{monAn.TenMon}\".";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var monAn = await _db.MonAns.Include(m => m.DanhMuc)
                .FirstOrDefaultAsync(m => m.MaMonAn == id && m.DanhMuc!.MaGianHang == CurrentGianHangId);
            if (monAn == null) return NotFound();

            var vm = new MonAnFormViewModel
            {
                MaMonAn = monAn.MaMonAn,
                MaDanhMuc = monAn.MaDanhMuc,
                TenMon = monAn.TenMon,
                Gia = monAn.Gia,
                MoTa = monAn.MoTa,
                KichCo = monAn.KichCo,
                Topping = monAn.Topping,
                MucCay = monAn.MucCay,
                HinhAnhHienTai = monAn.HinhAnh,
                ConHang = monAn.TinhTrang == "ConHang",
                DanhSachDanhMuc = await _db.DanhMucs.Where(d => d.MaGianHang == CurrentGianHangId).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MonAnFormViewModel vm)
        {
            var monAn = await _db.MonAns.Include(m => m.DanhMuc)
                .FirstOrDefaultAsync(m => m.MaMonAn == id && m.DanhMuc!.MaGianHang == CurrentGianHangId);
            if (monAn == null) return NotFound();

            if (ModelState.IsValid && vm.MaDanhMuc == 0 && !string.IsNullOrWhiteSpace(vm.TenDanhMucMoi))
            {
                var name = vm.TenDanhMucMoi.Trim();
                var category = await _db.DanhMucs.FirstOrDefaultAsync(d => d.MaGianHang == CurrentGianHangId && d.TenDanhMuc == name);
                if (category == null)
                {
                    category = new DanhMuc { MaGianHang = CurrentGianHangId, TenDanhMuc = name };
                    _db.DanhMucs.Add(category);
                    await _db.SaveChangesAsync();
                }
                vm.MaDanhMuc = category.MaDanhMuc;
            }
            if (!await _db.DanhMucs.AnyAsync(d => d.MaDanhMuc == vm.MaDanhMuc && d.MaGianHang == CurrentGianHangId))
                ModelState.AddModelError(nameof(vm.MaDanhMuc), "Danh mục không thuộc gian hàng của bạn.");
            if (!ModelState.IsValid)
            {
                vm.DanhSachDanhMuc = await _db.DanhMucs.Where(d => d.MaGianHang == CurrentGianHangId).ToListAsync();
                return View(vm);
            }

            monAn.TenMon = vm.TenMon;
            monAn.Gia = vm.Gia;
            monAn.MoTa = vm.MoTa;
            monAn.KichCo = vm.KichCo;
            monAn.Topping = vm.Topping;
            monAn.MucCay = vm.MucCay;
            monAn.TinhTrang = vm.ConHang ? "ConHang" : "HetHang";
            if (vm.MaDanhMuc != 0) monAn.MaDanhMuc = vm.MaDanhMuc;

            if (vm.AnhMoi != null)
            {
                if (!string.IsNullOrEmpty(monAn.HinhAnh)) _storage.DeleteFile(monAn.HinhAnh);
                monAn.HinhAnh = await _storage.SaveFileAsync(vm.AnhMoi, "monan");
            }

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã cập nhật món \"{monAn.TenMon}\".";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var monAn = await _db.MonAns.Include(m => m.DanhMuc)
                .FirstOrDefaultAsync(m => m.MaMonAn == id && m.DanhMuc!.MaGianHang == CurrentGianHangId);
            if (monAn == null) return NotFound();

            var dangDuocDat = await _db.ChiTietDonHangs.AnyAsync(c => c.MaMonAn == id);
            if (dangDuocDat)
            {
                // Không xoá vĩnh viễn món đã từng có trong đơn hàng — chỉ chuyển sang Hết hàng
                // để không phá vỡ dữ liệu lịch sử đơn hàng đã tồn tại.
                monAn.TinhTrang = "HetHang";
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = $"Món \"{monAn.TenMon}\" đã có trong đơn hàng nên không thể xoá — đã chuyển sang trạng thái Hết hàng.";
            }
            else
            {
                if (!string.IsNullOrEmpty(monAn.HinhAnh)) _storage.DeleteFile(monAn.HinhAnh);
                _db.MonAns.Remove(monAn);
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = $"Đã xoá món \"{monAn.TenMon}\".";
            }

            return RedirectToAction("Index");
        }
    }
}
