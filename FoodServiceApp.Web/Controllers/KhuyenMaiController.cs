using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Models.ViewModels;

namespace FoodServiceApp.Web.Controllers
{
    public class KhuyenMaiController : GianHangBaseController
    {
        private readonly AppDbContext _db;

        public KhuyenMaiController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // Chỉ hiển thị khuyến mãi do chính gian hàng này tạo.
            // Khuyến mãi toàn hệ thống (MaGianHang = null) do Quản trị quản lý, không sửa/xoá ở đây.
            var danhSach = await _db.KhuyenMais
                .Where(k => k.MaGianHang == CurrentGianHangId)
                .OrderByDescending(k => k.NgayBatDau)
                .ToListAsync();
            return View(danhSach);
        }

        private async Task<List<MonAn>> LayMonAsync() =>
            await _db.MonAns.Include(m => m.DanhMuc)
                .Where(m => m.DanhMuc!.MaGianHang == CurrentGianHangId).OrderBy(m => m.TenMon).ToListAsync();

        private Task<List<DanhMuc>> LayDanhMucAsync() => _db.DanhMucs
            .Where(x => x.MaGianHang == CurrentGianHangId).OrderBy(x => x.ThuTuHienThi).ToListAsync();

        private async Task NapDanhSach(KhuyenMaiFormViewModel vm)
        { vm.DanhSachMon = await LayMonAsync(); vm.DanhSachDanhMuc = await LayDanhMucAsync(); }

        public async Task<IActionResult> Create() { var vm = new KhuyenMaiFormViewModel(); await NapDanhSach(vm); return View(vm); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhuyenMaiFormViewModel vm)
        {
            if (vm.NgayKetThuc <= vm.NgayBatDau)
                ModelState.AddModelError(nameof(vm.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");

            if (await _db.KhuyenMais.AnyAsync(k => k.MaCode == vm.MaCode))
                ModelState.AddModelError(nameof(vm.MaCode), "Mã khuyến mãi này đã tồn tại.");

            if (vm.MaMonAn != null && !await _db.MonAns.AnyAsync(m => m.MaMonAn == vm.MaMonAn && m.DanhMuc!.MaGianHang == CurrentGianHangId))
                ModelState.AddModelError(nameof(vm.MaMonAn), "Món không thuộc gian hàng của bạn.");
            if (vm.MaDanhMuc != null && !await _db.DanhMucs.AnyAsync(x => x.MaDanhMuc == vm.MaDanhMuc && x.MaGianHang == CurrentGianHangId))
                ModelState.AddModelError(nameof(vm.MaDanhMuc), "Danh mục không thuộc gian hàng của bạn.");
            if (vm.MaDanhMuc != null && !await _db.DanhMucs.AnyAsync(x => x.MaDanhMuc == vm.MaDanhMuc && x.MaGianHang == CurrentGianHangId))
                ModelState.AddModelError(nameof(vm.MaDanhMuc), "Danh mục không thuộc gian hàng của bạn.");
            ValidateVoucher(vm);

            if (!ModelState.IsValid) { await NapDanhSach(vm); return View(vm); }

            var km = new KhuyenMai
            {
                MaGianHang = CurrentGianHangId,
                MaMonAn = vm.MaMonAn,
                MaDanhMuc = vm.MaDanhMuc,
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
            TempData["ThongBao"] = $"Đã tạo mã khuyến mãi {km.MaCode}.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var km = await _db.KhuyenMais.FirstOrDefaultAsync(k => k.MaKhuyenMai == id && k.MaGianHang == CurrentGianHangId);
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
                MaMonAn = km.MaMonAn,
                MaDanhMuc = km.MaDanhMuc, GiaTriDonToiThieu = km.GiaTriDonToiThieu,
                SoLuongToiDa = km.SoLuongToiDa, MoiKhachMotLan = km.MoiKhachMotLan,
                GiamPhiVanChuyen = km.GiamPhiVanChuyen, GiamPhiVanChuyenToiDa = km.GiamPhiVanChuyenToiDa,
            };
            await NapDanhSach(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhuyenMaiFormViewModel vm)
        {
            var km = await _db.KhuyenMais.FirstOrDefaultAsync(k => k.MaKhuyenMai == id && k.MaGianHang == CurrentGianHangId);
            if (km == null) return NotFound();

            if (vm.NgayKetThuc <= vm.NgayBatDau)
                ModelState.AddModelError(nameof(vm.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");

            if (vm.MaMonAn != null && !await _db.MonAns.AnyAsync(m => m.MaMonAn == vm.MaMonAn && m.DanhMuc!.MaGianHang == CurrentGianHangId))
                ModelState.AddModelError(nameof(vm.MaMonAn), "Món không thuộc gian hàng của bạn.");

            ValidateVoucher(vm);
            if (!ModelState.IsValid) { vm.MaCode = km.MaCode; await NapDanhSach(vm); return View(vm); }

            km.MaMonAn = vm.MaMonAn;
            km.MaDanhMuc = vm.MaDanhMuc;
            km.MoTa = vm.MoTa;
            km.DieuKienApDung = vm.DieuKienApDung;
            km.NgayBatDau = vm.NgayBatDau;
            km.NgayKetThuc = vm.NgayKetThuc;
            km.TrangThai = vm.TrangThai;
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
            if (vm.MaMonAn != null && vm.MaDanhMuc != null) ModelState.AddModelError("", "Chỉ chọn món hoặc danh mục, không chọn đồng thời.");
            if (vm.SoLuongToiDa is <= 0) ModelState.AddModelError(nameof(vm.SoLuongToiDa), "Giới hạn lượt phải lớn hơn 0.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var km = await _db.KhuyenMais.FirstOrDefaultAsync(k => k.MaKhuyenMai == id && k.MaGianHang == CurrentGianHangId);
            if (km == null) return NotFound();

            km.TrangThai = !km.TrangThai;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
