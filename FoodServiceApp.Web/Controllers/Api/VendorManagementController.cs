using System.Data;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Dtos;
using FoodServiceApp.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Controllers.Api;

[ApiController, Route("api/vendor")]
public class VendorManagementController(AppDbContext db) : VendorApiBaseController
{
    private Task<GianHang> Store() => db.GianHangs.Include(x => x.TaiKhoan).SingleAsync(x => x.MaTK == CurrentMaTK);
    private async Task<int> StoreId() => (await Store()).MaGianHang;

    [HttpPut("business-status")]
    public async Task<IActionResult> BusinessStatus(BusinessStatusRequest r)
    {
        var g = await Store(); g.TrangThaiKinhDoanh = r.DangMoCua ? "DangMo" : "DongCua";
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var g = await Store();
        return Ok(new { Id = g.MaGianHang.ToString(), TenGianHang = g.TenCuaHang, g.DiaChi,
            SoDienThoai = g.SDT, Email = g.TaiKhoan!.TenDangNhap, g.MoTa, g.HinhAnh,
            DangMoCua = g.TrangThaiKinhDoanh == "DangMo", g.TrangThaiDuyet });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> Profile(VendorProfileRequest r)
    {
        var g = await Store();
        g.TenCuaHang = r.TenCuaHang.Trim(); g.DiaChi = r.DiaChi.Trim(); g.SDT = r.SDT.Trim();
        g.MoTa = r.MoTa; g.HinhAnh = r.HinhAnh;
        g.TrangThaiKinhDoanh = r.DangMoCua ? "DangMo" : "DongCua";
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var id = await StoreId();
        return Ok(await db.DanhMucs.Where(x => x.MaGianHang == id).OrderBy(x => x.TenDanhMuc)
            .OrderBy(x => x.ThuTuHienThi).ThenBy(x => x.TenDanhMuc)
            .Select(x => new { x.MaDanhMuc, x.TenDanhMuc, x.MoTa, x.ThuTuHienThi, x.TrangThai }).ToListAsync());
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(CategoryRequest r)
    {
        var id = await StoreId();
        var name = r.TenDanhMuc.Trim();
        if (await db.DanhMucs.AnyAsync(x => x.MaGianHang == id && x.TenDanhMuc == name))
            return Conflict(new { message = "Danh mục đã tồn tại." });
        var item = new DanhMuc { MaGianHang = id, TenDanhMuc = name, MoTa = r.MoTa?.Trim(), ThuTuHienThi = r.ThuTuHienThi, TrangThai = r.TrangThai };
        db.DanhMucs.Add(item); await db.SaveChangesAsync();
        return Ok(new { item.MaDanhMuc, item.TenDanhMuc });
    }

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequest r)
    {
        var store = await StoreId();
        var item = await db.DanhMucs.SingleOrDefaultAsync(x => x.MaDanhMuc == id && x.MaGianHang == store);
        if (item == null) return NotFound();
        var name = r.TenDanhMuc.Trim();
        if (await db.DanhMucs.AnyAsync(x => x.MaGianHang == store && x.MaDanhMuc != id && x.TenDanhMuc == name))
            return Conflict(new { message = "Danh mục đã tồn tại." });
        item.TenDanhMuc = name; item.MoTa = r.MoTa?.Trim(); item.ThuTuHienThi = r.ThuTuHienThi; item.TrangThai = r.TrangThai;
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var store = await StoreId();
        var item = await db.DanhMucs.SingleOrDefaultAsync(x => x.MaDanhMuc == id && x.MaGianHang == store);
        if (item == null) return NotFound();
        if (await db.MonAns.AnyAsync(x => x.MaDanhMuc == id))
            return Conflict(new { message = "Hãy chuyển hoặc xóa các món trước khi xóa danh mục." });
        db.DanhMucs.Remove(item); await db.SaveChangesAsync(); return NoContent();
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings()
    {
        var store = await StoreId();
        var x = await db.CauHinhGianHangs.FindAsync(store);
        x ??= new CauHinhGianHang { MaGianHang = store };
        return Ok(new { x.GioMoCua, x.GioDongCua, x.TuDongDongMo, x.TamDungNhanDon,
            x.LyDoTamDung, x.ThongBaoCuaHang, x.ThoiGianChuanBiPhut });
    }

    [HttpPut("settings")]
    public async Task<IActionResult> Settings(StoreSettingsRequest r)
    {
        var store = await StoreId();
        var x = await db.CauHinhGianHangs.FindAsync(store);
        if (x == null) { x = new CauHinhGianHang { MaGianHang = store }; db.CauHinhGianHangs.Add(x); }
        x.GioMoCua = r.GioMoCua; x.GioDongCua = r.GioDongCua; x.TuDongDongMo = r.TuDongDongMo;
        x.TamDungNhanDon = r.TamDungNhanDon; x.LyDoTamDung = r.LyDoTamDung?.Trim();
        x.ThongBaoCuaHang = r.ThongBaoCuaHang?.Trim(); x.ThoiGianChuanBiPhut = r.ThoiGianChuanBiPhut;
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpGet("campaigns")]
    public async Task<IActionResult> Campaigns()
    {
        var store = await StoreId();
        return Ok(await db.ChienDichQuangCaos.Where(x => x.MaGianHang == store)
            .OrderByDescending(x => x.NgayTao).Select(x => new { x.MaChienDich, x.TenChienDich,
                x.LoaiViTri, x.TieuDe, x.NoiDung, x.HinhAnh, x.NgayBatDau, x.NgayKetThuc,
                x.NganSach, x.TrangThai, x.LuotHienThi, x.LuotTuongTac }).ToListAsync());
    }

    [HttpPost("campaigns")]
    public Task<IActionResult> CreateCampaign(AdvertisingCampaignRequest r) => SaveCampaign(null, r);

    [HttpPut("campaigns/{id:int}")]
    public Task<IActionResult> UpdateCampaign(int id, AdvertisingCampaignRequest r) => SaveCampaign(id, r);

    private async Task<IActionResult> SaveCampaign(int? id, AdvertisingCampaignRequest r)
    {
        var store = await StoreId();
        var x = id == null ? new ChienDichQuangCao { MaGianHang = store } :
            await db.ChienDichQuangCaos.SingleOrDefaultAsync(x => x.MaChienDich == id && x.MaGianHang == store);
        if (x == null) return NotFound();
        x.TenChienDich = r.TenChienDich.Trim(); x.LoaiViTri = r.LoaiViTri; x.TieuDe = r.TieuDe.Trim();
        x.NoiDung = r.NoiDung?.Trim(); x.HinhAnh = r.HinhAnh?.Trim(); x.NgayBatDau = r.NgayBatDau;
        x.NgayKetThuc = r.NgayKetThuc; x.NganSach = r.NganSach; x.TrangThai = r.KichHoat ? "DangChay" : "BanNhap";
        if (id == null) db.ChienDichQuangCaos.Add(x);
        await db.SaveChangesAsync(); return Ok(new { x.MaChienDich });
    }

    [HttpDelete("campaigns/{id:int}")]
    public async Task<IActionResult> DeleteCampaign(int id)
    {
        var store = await StoreId();
        var x = await db.ChienDichQuangCaos.SingleOrDefaultAsync(x => x.MaChienDich == id && x.MaGianHang == store);
        if (x == null) return NotFound();
        db.ChienDichQuangCaos.Remove(x); await db.SaveChangesAsync(); return NoContent();
    }

    [HttpGet("items")]
    public async Task<IActionResult> Items()
    {
        var store = await StoreId();
        return Ok(await db.MonAns.Where(x => x.DanhMuc!.MaGianHang == store).Select(x => new {
            Id = x.MaMonAn.ToString(), x.MaDanhMuc, x.TenMon, x.Gia, x.MoTa, x.HinhAnh,
            x.KichCo, x.Topping, x.MucCay,
            DanhMuc = x.DanhMuc!.TenDanhMuc, GianHangId = store.ToString(), ConHang = x.TinhTrang == "ConHang"
        }).ToListAsync());
    }

    [HttpPost("items")]
    public Task<IActionResult> CreateItem(MenuItemRequest r) => SaveItem(null, r);
    [HttpPut("items/{id:int}")]
    public Task<IActionResult> UpdateItem(int id, MenuItemRequest r) => SaveItem(id, r);
    private async Task<IActionResult> SaveItem(int? id, MenuItemRequest r)
    {
        var store = await StoreId();
        if (!await db.DanhMucs.AnyAsync(x => x.MaDanhMuc == r.MaDanhMuc && x.MaGianHang == store))
            return BadRequest(new { message = "Danh mục không thuộc gian hàng của bạn." });
        var item = id == null ? new MonAn() : await db.MonAns.SingleOrDefaultAsync(x => x.MaMonAn == id && x.DanhMuc!.MaGianHang == store);
        if (item == null) return NotFound();
        item.MaDanhMuc = r.MaDanhMuc; item.TenMon = r.TenMon.Trim(); item.Gia = r.Gia;
        item.MoTa = r.MoTa; item.HinhAnh = r.HinhAnh; item.TinhTrang = r.ConHang ? "ConHang" : "HetHang";
        item.KichCo = r.KichCo; item.Topping = r.Topping; item.MucCay = r.MucCay;
        if (id == null) db.MonAns.Add(item);
        await db.SaveChangesAsync(); return Ok(new { item.MaMonAn });
    }

    [HttpPut("items/{id:int}/availability")]
    public async Task<IActionResult> Availability(int id, AvailabilityRequest r)
    {
        var store = await StoreId();
        var item = await db.MonAns.SingleOrDefaultAsync(x => x.MaMonAn == id && x.DanhMuc!.MaGianHang == store);
        if (item == null) return NotFound();
        item.TinhTrang = r.ConHang ? "ConHang" : "HetHang";
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("items/{id:int}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var store = await StoreId();
        var item = await db.MonAns.SingleOrDefaultAsync(x => x.MaMonAn == id && x.DanhMuc!.MaGianHang == store);
        if (item == null) return NotFound();
        if (await db.ChiTietDonHangs.AnyAsync(x => x.MaMonAn == id) || await db.KhuyenMais.AnyAsync(x => x.MaMonAn == id))
            return Conflict(new { message = "Món đã có trong đơn hoặc khuyến mãi; hãy chuyển sang hết hàng." });
        db.MonAns.Remove(item); await db.SaveChangesAsync(); return NoContent();
    }

    [HttpGet("promotions")]
    public async Task<IActionResult> Promotions()
    {
        var store = await StoreId();
        return Ok(await db.KhuyenMais.Where(x => x.MaGianHang == store).Select(x => new {
            Id = x.MaKhuyenMai.ToString(), x.MaCode, x.MoTa, x.MaMonAn, x.MaDanhMuc,
            TenChuongTrinh = x.MoTa ?? x.MaCode, LoaiGiam = x.GiamPhiVanChuyen ? 2 : x.PhanTramGiam != null ? 0 : 1,
            GiaTriGiam = x.PhanTramGiam ?? x.SoTienGiam ?? 0, x.NgayBatDau, x.NgayKetThuc,
            DangHoatDong = x.TrangThai, GianHangId = store.ToString(), x.GiaTriDonToiThieu,
            x.SoLuongToiDa, x.SoLuongDaDung, x.MoiKhachMotLan, x.GiamPhiVanChuyen, x.GiamPhiVanChuyenToiDa
        }).ToListAsync());
    }

    [HttpGet("reports/revenue")]
    public async Task<IActionResult> RevenueReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var store = await StoreId(); var start = (from ?? DateTime.Today.AddDays(-29)).Date;
        var end = (to ?? DateTime.Today).Date.AddDays(1);
        var orders = await db.DonHangs.Where(x => x.MaGianHang == store && x.NgayDat >= start && x.NgayDat < end).ToListAsync();
        var completed = orders.Where(x => x.TrangThai == "DaGiao").ToList();
        var gross = completed.Sum(x => x.TongTien - x.PhiGiaoHang); var platformFee = Math.Round(gross * .10m, 0);
        return Ok(new { TuNgay = start, DenNgay = end.AddDays(-1), TongDon = orders.Count,
            DonHoanThanh = completed.Count, DonHuy = orders.Count(x => x.TrangThai == "DaHuy"),
            DoanhThu = gross, PhiNenTang = platformFee, ThucNhan = gross - platformFee });
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> Reviews()
    {
        var store = await StoreId();
        return Ok(await db.DanhGias.Where(x => x.MaGianHang == store).OrderByDescending(x => x.NgayDanhGia)
            .Select(x => new { x.MaDanhGia, x.MaDonHang, x.SoSao, x.NoiDung, x.NgayDanhGia, x.PhanHoiGianHang, x.NgayPhanHoi }).ToListAsync());
    }

    [HttpPost("reviews/{id:int}/reply")]
    public async Task<IActionResult> ReplyReview(int id, ReviewReplyRequest request)
    {
        var store = await StoreId();
        var review = await db.DanhGias.FirstOrDefaultAsync(x => x.MaDanhGia == id && x.MaGianHang == store);
        if (review == null) return NotFound();
        review.PhanHoiGianHang = request.NoiDung.Trim(); review.NgayPhanHoi = DateTime.Now;
        await db.SaveChangesAsync(); return NoContent();
    }
    [HttpPost("promotions")]
    public Task<IActionResult> CreatePromotion(PromotionRequest r) => SavePromotion(null, r);
    [HttpPut("promotions/{id:int}")]
    public Task<IActionResult> UpdatePromotion(int id, PromotionRequest r) => SavePromotion(id, r);
    private async Task<IActionResult> SavePromotion(int? id, PromotionRequest r)
    {
        var store = await StoreId();
        if (r.MaMonAn != null && !await db.MonAns.AnyAsync(x => x.MaMonAn == r.MaMonAn && x.DanhMuc!.MaGianHang == store))
            return BadRequest(new { message = "Món không thuộc gian hàng của bạn." });
        if (r.MaDanhMuc != null && !await db.DanhMucs.AnyAsync(x => x.MaDanhMuc == r.MaDanhMuc && x.MaGianHang == store))
            return BadRequest(new { message = "Danh mục không thuộc gian hàng của bạn." });
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var code = r.MaCode.Trim().ToUpperInvariant();
        if (await db.KhuyenMais.AnyAsync(x => x.MaCode == code && (id == null || x.MaKhuyenMai != id)))
            return Conflict(new { message = "Mã khuyến mãi đã tồn tại." });
        var item = id == null ? new KhuyenMai { MaGianHang = store, MaCode = code } :
            await db.KhuyenMais.SingleOrDefaultAsync(x => x.MaKhuyenMai == id && x.MaGianHang == store);
        if (item == null) return NotFound();
        if (item.MaCode != code) return BadRequest(new { message = "Không được đổi mã đã tạo." });
        item.MoTa = r.MoTa; item.MaMonAn = r.MaMonAn; item.MaDanhMuc = r.MaDanhMuc;
        item.GiamPhiVanChuyen = r.GiamPhiVanChuyen; item.GiamPhiVanChuyenToiDa = r.GiamPhiVanChuyenToiDa;
        item.PhanTramGiam = !r.GiamPhiVanChuyen && r.LaPhanTram ? r.GiaTriGiam : null;
        item.SoTienGiam = !r.GiamPhiVanChuyen && !r.LaPhanTram ? r.GiaTriGiam : null;
        item.GiaTriDonToiThieu = r.GiaTriDonToiThieu; item.SoLuongToiDa = r.SoLuongToiDa;
        item.MoiKhachMotLan = r.MoiKhachMotLan;
        item.NgayBatDau = r.NgayBatDau; item.NgayKetThuc = r.NgayKetThuc; item.TrangThai = r.TrangThai;
        if (id == null) db.KhuyenMais.Add(item);
        await db.SaveChangesAsync(); await tx.CommitAsync(); return Ok(new { item.MaKhuyenMai });
    }
    [HttpPut("promotions/{id:int}/status")]
    public async Task<IActionResult> PromotionStatus(int id, PromotionStatusRequest r)
    {
        var store = await StoreId();
        var item = await db.KhuyenMais.SingleOrDefaultAsync(x => x.MaKhuyenMai == id && x.MaGianHang == store);
        if (item == null) return NotFound();
        item.TrangThai = r.TrangThai; await db.SaveChangesAsync(); return NoContent();
    }
    [HttpDelete("promotions/{id:int}")]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        var store = await StoreId();
        var item = await db.KhuyenMais.SingleOrDefaultAsync(x => x.MaKhuyenMai == id && x.MaGianHang == store);
        if (item == null) return NotFound();
        if (await db.DonHangs.AnyAsync(x => x.MaKhuyenMaiApDung == id))
            return Conflict(new { message = "Mã đã được sử dụng; hãy tắt thay vì xóa." });
        db.KhuyenMais.Remove(item); await db.SaveChangesAsync(); return NoContent();
    }
}
public record BusinessStatusRequest(bool DangMoCua);
public record ReviewReplyRequest([property: System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(500)] string NoiDung);
