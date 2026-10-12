using System.Data;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Dtos;
using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Controllers.Api;

[ApiController, Route("api/vendor/register")]
public class VendorRegistrationController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(VendorRegisterRequest r)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var login = r.TenDangNhap.Trim();
        if (await db.TaiKhoans.AnyAsync(x => x.TenDangNhap == login))
            return Conflict(new { message = "Tên đăng nhập đã tồn tại." });
        var account = new TaiKhoan { TenDangNhap = login, MatKhau = MatKhauHelper.Hash(r.MatKhau), VaiTro = "GianHang" };
        var store = new GianHang { TaiKhoan = account, TenCuaHang = r.TenCuaHang.Trim(), DiaChi = r.DiaChi.Trim(),
            SDT = r.SDT.Trim(), MoTa = r.MoTa, TrangThaiDuyet = "ChoDuyet", TrangThaiKinhDoanh = "TamNgung" };
        db.GianHangs.Add(store);
        await db.SaveChangesAsync(); await tx.CommitAsync();
        return Ok(new { store.MaGianHang, store.TrangThaiDuyet });
    }
}
