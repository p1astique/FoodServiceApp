using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Controllers.Api;

[ApiController]
[Route("api/admin/vendors")]
public class AdminVendorController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminVendorController(AppDbContext db) => _db = db;

    [HttpGet("by-status")]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        var query = _db.GianHangs.AsNoTracking()
            .Join(_db.TaiKhoans.AsNoTracking(), g => g.MaTK, t => t.MaTK,
                (g, t) => new { GianHang = g, TaiKhoan = t });

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.GianHang.TrangThaiDuyet == status);

        var result = await query
            .OrderByDescending(x => x.GianHang.NgayDangKy)
            .Select(x => new AdminVendorDto(
                x.GianHang.MaGianHang, x.GianHang.MaTK, x.GianHang.TenCuaHang,
                x.GianHang.DiaChi, x.GianHang.SDT, x.TaiKhoan.TenDangNhap,
                x.GianHang.MoTa, x.GianHang.HinhAnh, x.GianHang.NgayDangKy,
                x.GianHang.TrangThaiDuyet, x.GianHang.TrangThaiKinhDoanh,
                x.TaiKhoan.TrangThai))
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _db.GianHangs
            .AsNoTracking()
            .Where(g => g.TrangThaiDuyet == "ChoDuyet")
            .Join(
                _db.TaiKhoans.AsNoTracking(),
                g => g.MaTK,
                t => t.MaTK,
                (g, t) => new { GianHang = g, TaiKhoan = t }
            )
            .OrderBy(x => x.GianHang.NgayDangKy)
            .Select(x => new AdminVendorDto(
                x.GianHang.MaGianHang,
                x.GianHang.MaTK,
                x.GianHang.TenCuaHang,
                x.GianHang.DiaChi,
                x.GianHang.SDT,
                x.TaiKhoan.TenDangNhap,
                x.GianHang.MoTa,
                x.GianHang.HinhAnh,
                x.GianHang.NgayDangKy,
                x.GianHang.TrangThaiDuyet,
                x.GianHang.TrangThaiKinhDoanh,
                x.TaiKhoan.TrangThai
            ))
            .ToListAsync();

        return Ok(result);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var g = await _db.GianHangs.FirstOrDefaultAsync(x => x.MaGianHang == id);
        if (g is null) return NotFound(new { message = "Không tìm thấy hồ sơ gian hàng." });
        g.TrangThaiDuyet = "DaDuyet";
        g.TrangThaiKinhDoanh = "DangMo";
        await _db.SaveChangesAsync();
        return Ok(new { message = "Đã duyệt gian hàng.", id });
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var g = await _db.GianHangs.FirstOrDefaultAsync(x => x.MaGianHang == id);
        if (g is null) return NotFound(new { message = "Không tìm thấy hồ sơ gian hàng." });
        g.TrangThaiDuyet = "TuChoi";
        await _db.SaveChangesAsync();
        return Ok(new { message = "Đã từ chối hồ sơ gian hàng.", id });
    }

    [HttpPost("{id:int}/toggle-lock")]
    public async Task<IActionResult> ToggleLock(int id)
    {
        var account = await _db.GianHangs
            .Where(g => g.MaGianHang == id)
            .Select(g => g.TaiKhoan)
            .FirstOrDefaultAsync();
        if (account is null) return NotFound(new { message = "Không tìm thấy tài khoản gian hàng." });
        account.TrangThai = !account.TrangThai;
        await _db.SaveChangesAsync();
        return Ok(new { active = account.TrangThai });
    }
}
