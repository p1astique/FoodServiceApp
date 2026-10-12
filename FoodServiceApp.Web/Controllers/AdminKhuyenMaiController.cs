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
        private async Task NapDanhSach(KhuyenMaiFormViewModel vm)
        {
            vm.DanhSachGianHang = await _db.GianHangs.Where(x => x.TrangThaiDuyet == "DaDuyet").OrderBy(x => x.TenCuaHang).ToListAsync();
            vm.DanhSachDanhMuc = await _db.DanhMucs.OrderBy(x => x.TenDanhMuc).ToListAsync();
            vm.DanhSachMon = await _db.MonAns.OrderBy(x => x.TenMon).ToListAsync();
        }

        public async Task<IActionResult> Create() { var vm = new KhuyenMaiFormViewModel(); await NapDanhSach(vm); return View(vm); }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhuyenMaiFormViewModel vm)
        {
            if (vm.NgayKetThuc <= vm.NgayBatDau)
                ModelState.AddModelError(nameof(vm.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");

            if (await _db.KhuyenMais.AnyAsync(k => k.MaCode == vm.MaCode))
                ModelState.AddModelError(nameof(vm.MaCode), "Mã khuyến mãi này đã tồn tại.");
            await ValidateScope(vm);
            ValidateVoucher(vm);

            if (!ModelState.IsValid) { await NapDanhSach(vm); return View(vm); }

            var km = new KhuyenMai
            {
                MaGianHang = null, // toàn hệ thống
                MaGianHangApDung = vm.MaGianHangApDung, MaDanhMuc = vm.MaDanhMuc, MaMonAn = vm.MaMonAn,
                MaCode = vm.MaCode.Trim().ToUpper(),
                MoTa = vm.MoTa,
                DieuKienApDung = vm.DieuKienApDung,
                NgayBatDau = vm.NgayBatDau,
                NgayKetThuc = vm.NgayKetThuc,
                TrangThai = vm.TrangThai,
                GiaTriDonToiThieu = vm.GiaTriDonToiThieu, SoLuongToiDa = vm.SoLuongToiDa,
                MoiKhachMotLan = vm.MoiKhachMotLan, GiamPhiVanChuyen = vm.GiamPhiVanChuyen,
                GiamPhiVanChuyenToiDa = vm.GiamPhiVanChuyenToiDa,
                PhanTramGiam = !vm.GiamPhiVanChuyen && vm.LoaiGiam == "PhanTram" ? vm.GiaTriGiam : null,
                SoTienGiam = !vm.GiamPhiVanChuyen && vm.LoaiGiam == "SoTien" ? vm.GiaTriGiam : null,
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
                MaGianHangApDung = km.MaGianHangApDung, MaDanhMuc = km.MaDanhMuc, MaMonAn = km.MaMonAn,
                GiaTriDonToiThieu = km.GiaTriDonToiThieu, SoLuongToiDa = km.SoLuongToiDa,
                MoiKhachMotLan = km.MoiKhachMotLan, GiamPhiVanChuyen = km.GiamPhiVanChuyen,
                GiamPhiVanChuyenToiDa = km.GiamPhiVanChuyenToiDa,
            };
            await NapDanhSach(vm);
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
            await ValidateScope(vm);
            ValidateVoucher(vm);

            if (!ModelState.IsValid) { vm.MaCode = km.MaCode; await NapDanhSach(vm); return View(vm); }

            km.MoTa = vm.MoTa;
            km.DieuKienApDung = vm.DieuKienApDung;
            km.NgayBatDau = vm.NgayBatDau;
            km.NgayKetThuc = vm.NgayKetThuc;
            km.TrangThai = vm.TrangThai;
            km.MaGianHangApDung = vm.MaGianHangApDung; km.MaDanhMuc = vm.MaDanhMuc; km.MaMonAn = vm.MaMonAn;
            km.GiaTriDonToiThieu = vm.GiaTriDonToiThieu; km.SoLuongToiDa = vm.SoLuongToiDa;
            km.MoiKhachMotLan = vm.MoiKhachMotLan; km.GiamPhiVanChuyen = vm.GiamPhiVanChuyen;
            km.GiamPhiVanChuyenToiDa = vm.GiamPhiVanChuyenToiDa;
            km.PhanTramGiam = !vm.GiamPhiVanChuyen && vm.LoaiGiam == "PhanTram" ? vm.GiaTriGiam : null;
            km.SoTienGiam = !vm.GiamPhiVanChuyen && vm.LoaiGiam == "SoTien" ? vm.GiaTriGiam : null;

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã cập nhật mã {km.MaCode}.";
            return RedirectToAction("Index");
        }

        private void ValidateVoucher(KhuyenMaiFormViewModel vm)
        {
            if (!vm.GiamPhiVanChuyen && vm.GiaTriGiam <= 0) ModelState.AddModelError(nameof(vm.GiaTriGiam), "Giá trị giảm phải lớn hơn 0.");
            if (vm.LoaiGiam == "PhanTram" && vm.GiaTriGiam > 100) ModelState.AddModelError(nameof(vm.GiaTriGiam), "Phần trăm không vượt quá 100.");
            if (vm.MaMonAn != null && vm.MaDanhMuc != null) ModelState.AddModelError("", "Chỉ chọn món hoặc danh mục.");
            if (vm.SoLuongToiDa is <= 0) ModelState.AddModelError(nameof(vm.SoLuongToiDa), "Giới hạn lượt phải lớn hơn 0.");
        }

        private async Task ValidateScope(KhuyenMaiFormViewModel vm)
        {
            if (vm.MaGianHangApDung != null && !await _db.GianHangs.AnyAsync(x => x.MaGianHang == vm.MaGianHangApDung))
                ModelState.AddModelError(nameof(vm.MaGianHangApDung), "Gian hàng áp dụng không tồn tại.");
            if (vm.MaDanhMuc != null && !await _db.DanhMucs.AnyAsync(x => x.MaDanhMuc == vm.MaDanhMuc && (vm.MaGianHangApDung == null || x.MaGianHang == vm.MaGianHangApDung)))
                ModelState.AddModelError(nameof(vm.MaDanhMuc), "Danh mục không thuộc gian hàng đã chọn.");
            if (vm.MaMonAn != null && !await _db.MonAns.AnyAsync(x => x.MaMonAn == vm.MaMonAn && (vm.MaGianHangApDung == null || x.DanhMuc!.MaGianHang == vm.MaGianHangApDung)))
                ModelState.AddModelError(nameof(vm.MaMonAn), "Món không thuộc gian hàng đã chọn.");
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
