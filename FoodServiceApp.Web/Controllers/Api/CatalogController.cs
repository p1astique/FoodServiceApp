using FoodServiceApp.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Controllers.Api;
[ApiController, Route("api/catalog")]
public class CatalogController(AppDbContext db) : ControllerBase
{
    [HttpGet("stores")]
    public async Task<IActionResult> Stores() => Ok(await db.GianHangs
        .Where(g => g.TrangThaiDuyet == "DaDuyet" && g.TrangThaiKinhDoanh == "DangMo" && g.TaiKhoan!.TrangThai)
        .Select(g => new { Id = g.MaGianHang.ToString(), TenGianHang = g.TenCuaHang, g.DiaChi, g.HinhAnh,
            DangMoCua = true, DanhGia = db.DanhGias.Where(d => d.MaGianHang == g.MaGianHang).Average(d => (double?)d.SoSao) ?? 0 })
        .ToListAsync());
    [HttpGet("stores/{id:int}/items")]
    public async Task<IActionResult> Items(int id)
    {
        if (!await db.GianHangs.AnyAsync(g => g.MaGianHang == id && g.TrangThaiDuyet == "DaDuyet" && g.TrangThaiKinhDoanh == "DangMo" && g.TaiKhoan!.TrangThai)) return NotFound();
        return Ok(await db.MonAns.Where(m => m.DanhMuc!.MaGianHang == id && m.DanhMuc.TrangThai && m.TinhTrang == "ConHang")
            .Select(m => new { Id = m.MaMonAn.ToString(), m.TenMon, m.Gia, m.HinhAnh, m.MoTa,
                m.KichCo, m.Topping, m.MucCay,
                GianHangId = id.ToString(), DanhMuc = m.DanhMuc!.TenDanhMuc, ConHang = true }).ToListAsync());
    }

    [HttpGet("stores/{id:int}/settings")]
    public async Task<IActionResult> StoreSettings(int id)
    {
        var exists = await db.GianHangs.AnyAsync(g => g.MaGianHang == id && g.TrangThaiDuyet == "DaDuyet" && g.TaiKhoan!.TrangThai);
        if (!exists) return NotFound();
        var x = await db.CauHinhGianHangs.FindAsync(id);
        return Ok(new { GioMoCua = x?.GioMoCua, GioDongCua = x?.GioDongCua, ThongBao = x?.ThongBaoCuaHang,
            ThoiGianChuanBiPhut = x?.ThoiGianChuanBiPhut ?? 20, DangNhanDon = x?.TamDungNhanDon != true,
            LyDoTamDung = x?.LyDoTamDung });
    }

    [HttpGet("campaigns")]
    public async Task<IActionResult> Campaigns([FromQuery] string? position)
    {
        var now = DateTime.Now;
        var query = db.ChienDichQuangCaos.Where(x => x.TrangThai == "DangChay" && x.NgayBatDau <= now && x.NgayKetThuc > now &&
            x.GianHang!.TrangThaiDuyet == "DaDuyet" && x.GianHang.TrangThaiKinhDoanh == "DangMo" && x.GianHang.TaiKhoan!.TrangThai);
        if (!string.IsNullOrWhiteSpace(position)) query = query.Where(x => x.LoaiViTri == position);
        return Ok(await query.OrderByDescending(x => x.NganSach).Select(x => new { x.MaChienDich, x.MaGianHang,
            x.TieuDe, x.NoiDung, x.HinhAnh, x.LoaiViTri, TenGianHang = x.GianHang!.TenCuaHang }).ToListAsync());
    }

    [HttpPost("campaigns/{id:int}/interaction")]
    public async Task<IActionResult> CampaignInteraction(int id)
    {
        var now = DateTime.Now;
        var x = await db.ChienDichQuangCaos.SingleOrDefaultAsync(x => x.MaChienDich == id && x.TrangThai == "DangChay" && x.NgayBatDau <= now && x.NgayKetThuc > now);
        if (x == null) return NotFound();
        x.LuotTuongTac++; await db.SaveChangesAsync(); return NoContent();
    }
}
