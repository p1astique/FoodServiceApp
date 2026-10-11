using Microsoft.AspNetCore.Mvc;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.ViewModels;
using FoodServiceApp.Web.Services;

namespace FoodServiceApp.Web.Controllers
{
    [Route("MuaHang/HoSo")]
    public class CustomerHoSoController : CustomerBaseController
    {
        private readonly AppDbContext _db;

        public CustomerHoSoController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var kh = await _db.KhachHangs.FindAsync(CurrentMaKH);
            var vm = new CustomerHoSoViewModel
            {
                HoTen = kh!.HoTen,
                SDT = kh.SDT,
                Email = kh.Email,
                DiaChiGiaoHang = kh.DiaChiGiaoHang,
            };
            return View(vm);
        }

        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CustomerHoSoViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.MatKhauMoi) && vm.MatKhauMoi != vm.XacNhanMatKhauMoi)
                ModelState.AddModelError(nameof(vm.XacNhanMatKhauMoi), "Xác nhận mật khẩu không khớp.");

            if (!ModelState.IsValid) return View(vm);

            var kh = await _db.KhachHangs.FindAsync(CurrentMaKH);
            kh!.HoTen = vm.HoTen;
            kh.SDT = vm.SDT;
            kh.Email = vm.Email;
            kh.DiaChiGiaoHang = vm.DiaChiGiaoHang;

            if (!string.IsNullOrEmpty(vm.MatKhauMoi))
            {
                var taiKhoan = await _db.TaiKhoans.FindAsync(kh.MaTK);
                taiKhoan!.MatKhau = MatKhauHelper.Hash(vm.MatKhauMoi);
            }

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = "Đã cập nhật hồ sơ.";
            return RedirectToAction("Index");
        }
    }
}
