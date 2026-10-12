using System.Data;
using System.ComponentModel.DataAnnotations;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Controllers.Api;
[ApiController, Route("api/customer/register")]
public class CustomerRegistrationController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(CustomerRegisterRequest r)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var login = r.TenDangNhap.Trim();
        if (await db.TaiKhoans.AnyAsync(x => x.TenDangNhap == login)) return Conflict(new { message = "Tên đăng nhập đã tồn tại." });
        var account = new TaiKhoan { TenDangNhap = login, MatKhau = MatKhauHelper.Hash(r.MatKhau), VaiTro = "KhachHang" };
        db.TaiKhoans.Add(account); await db.SaveChangesAsync();
        db.KhachHangs.Add(new KhachHang { MaTK = account.MaTK, HoTen = r.HoTen.Trim(), SDT = r.SDT.Trim() });
        await db.SaveChangesAsync(); await tx.CommitAsync(); return Ok(new { message = "Đã tạo tài khoản." });
    }
}
public class CustomerRegisterRequest
{
    [Required, StringLength(50)] public string TenDangNhap { get; set; } = "";
    [Required, StringLength(72, MinimumLength = 6)] public string MatKhau { get; set; } = "";
    [Required, StringLength(100)] public string HoTen { get; set; } = "";
    [Required, StringLength(15)] public string SDT { get; set; } = "";
}
