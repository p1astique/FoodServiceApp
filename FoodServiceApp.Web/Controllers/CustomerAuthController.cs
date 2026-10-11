using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Models.ViewModels;
using FoodServiceApp.Web.Services;

namespace FoodServiceApp.Web.Controllers
{
    [Route("MuaHang/Auth")]
    public class CustomerAuthController : Controller
    {
        private readonly AppDbContext _db;

        public CustomerAuthController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("KhachHang"))
                return RedirectToAction("Index", "CustomerHome");

            return View("Login", new LoginViewModel());
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (!ModelState.IsValid)
                return View("Login", model);

            // DEMO ONLY: so sánh mật khẩu plaintext — xem ghi chú bảo mật trong README.
            var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(t =>
                t.TenDangNhap == model.TenDangNhap && t.VaiTro == "KhachHang");

            if (taiKhoan == null || !MatKhauHelper.Khop(model.MatKhau, taiKhoan.MatKhau))
            {
                model.LoiThongBao = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View("Login", model);
            }

            if (!taiKhoan.TrangThai)
            {
                model.LoiThongBao = "Tài khoản của bạn đã bị khoá.";
                return View("Login", model);
            }

            var khachHang = await _db.KhachHangs.FirstAsync(k => k.MaTK == taiKhoan.MaTK);
            await DangNhapAsync(taiKhoan, khachHang);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "CustomerHome");
        }

        [HttpGet("DangKy")]
        public IActionResult DangKy() => View(new CustomerRegisterViewModel());

        [HttpPost("DangKy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(CustomerRegisterViewModel model)
        {
            if (model.MatKhau != model.XacNhanMatKhau)
                ModelState.AddModelError(nameof(model.XacNhanMatKhau), "Xác nhận mật khẩu không khớp.");

            if (await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");

            if (!ModelState.IsValid) return View(model);

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap,
                MatKhau = MatKhauHelper.Hash(model.MatKhau),
                VaiTro = "KhachHang",
            };
            _db.TaiKhoans.Add(taiKhoan);
            await _db.SaveChangesAsync();

            var khachHang = new KhachHang
            {
                MaTK = taiKhoan.MaTK,
                HoTen = model.HoTen,
                SDT = model.SDT,
                Email = model.Email,
            };
            _db.KhachHangs.Add(khachHang);
            await _db.SaveChangesAsync();

            await DangNhapAsync(taiKhoan, khachHang);
            return RedirectToAction("Index", "CustomerHome");
        }

        private async Task DangNhapAsync(TaiKhoan taiKhoan, KhachHang khachHang)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, taiKhoan.MaTK.ToString()),
                new Claim(ClaimTypes.Name, taiKhoan.TenDangNhap),
                new Claim(ClaimTypes.Role, "KhachHang"),
                new Claim("MaKH", khachHang.MaKH.ToString()),
                new Claim("HoTenKhachHang", khachHang.HoTen),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "CustomerHome");
        }
    }
}
