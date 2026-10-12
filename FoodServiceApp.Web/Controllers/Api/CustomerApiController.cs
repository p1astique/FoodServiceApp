using System.ComponentModel.DataAnnotations;
using System.Data;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Dtos;
using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Models.ViewModels;
using FoodServiceApp.Web.Services;
using FoodServiceApp.Web.Services.Maps;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Controllers.Api;

[ApiController, Route("api/customer"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "KhachHang")]
public class CustomerApiController(AppDbContext db, IGoongMapsService maps) : ApiBaseController
{
    [HttpGet("orders")]
    public async Task<IActionResult> Orders()
    {
        var customer = await db.KhachHangs.SingleAsync(x => x.MaTK == CurrentMaTK);
        var orders = await db.DonHangs.Include(x => x.GianHang).Include(x => x.ChiTiets).ThenInclude(x => x.MonAn)
            .Where(x => x.MaKH == customer.MaKH).OrderByDescending(x => x.NgayDat).ToListAsync();
        return Ok(orders.Select(x => new DonHangDto(x.MaDonHang, x.TrangThai, x.TongTien, x.NgayDat,
            x.DiaChiGiaoHang, x.GhiChu, x.GianHang!.TenCuaHang, x.GianHang.DiaChi,
            x.ChiTiets.Select(c => new ChiTietDonHangDto(c.TenMon, c.SoLuong, c.DonGia, c.ThanhTien, c.TuyChonMon)).ToList(), x.PhiGiaoHang)));
    }

    [HttpPost("quote")]
    public Task<IActionResult> Quote(CustomerOrderRequest r) => Checkout(r, false);
    [HttpPost("orders")]
    public Task<IActionResult> Order(CustomerOrderRequest r) => Checkout(r, true);

    private async Task<IActionResult> Checkout(CustomerOrderRequest r, bool save)
    {
        if (r.Items.Count is < 1 or > 100 || r.Items.Any(x => x.SoLuong is < 1 or > 99) ||
            r.Items.Select(x => x.MaMonAn).Distinct().Count() != r.Items.Count)
            return BadRequest(new { message = "Giỏ hàng rỗng, trùng món hoặc số lượng không hợp lệ." });
        var store = await db.GianHangs.SingleOrDefaultAsync(x => x.MaGianHang == r.MaGianHang);
        if (store == null) return NotFound();
        var settings = await db.CauHinhGianHangs.FindAsync(r.MaGianHang);
        if (settings?.TamDungNhanDon == true)
            return Conflict(new { message = settings.LyDoTamDung ?? "Gian hàng đang tạm dừng nhận đơn." });
        if (settings?.TuDongDongMo == true)
        {
            var time = DateTime.Now.TimeOfDay;
            var open = settings.GioMoCua < settings.GioDongCua
                ? time >= settings.GioMoCua && time < settings.GioDongCua
                : time >= settings.GioMoCua || time < settings.GioDongCua;
            if (!open) return Conflict(new { message = $"Gian hàng chỉ nhận đơn từ {settings.GioMoCua:hh\\:mm} đến {settings.GioDongCua:hh\\:mm}." });
        }
        decimal fee = 15000;
        try {
            var from = await maps.GeocodeAsync(store.DiaChi); var to = await maps.GeocodeAsync(r.DiaChiGiaoHang);
            if (from != null && to != null) {
                var distance = await maps.GetDistanceAsync(from, to);
                if (distance != null) fee = Math.Ceiling((decimal)distance.DistanceKm) * 5000m;
            }
        } catch { /* Same fallback as the website when maps are unavailable. */ }
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (!await db.GianHangs.AnyAsync(x => x.MaGianHang == r.MaGianHang && x.TrangThaiDuyet == "DaDuyet" && x.TrangThaiKinhDoanh == "DangMo" && x.TaiKhoan!.TrangThai))
            return Conflict(new { message = "Gian hàng hiện không nhận đơn." });
        var ids = r.Items.Select(x => x.MaMonAn).ToList();
        var items = await db.MonAns.Where(x => ids.Contains(x.MaMonAn) && x.DanhMuc!.MaGianHang == r.MaGianHang && x.DanhMuc.TrangThai && x.TinhTrang == "ConHang").ToListAsync();
        if (items.Count != ids.Count) return Conflict(new { message = "Có món đã hết hàng hoặc không thuộc gian hàng." });
        var cart = new Cart { MaGianHang = r.MaGianHang, Items = items.Select(x => new CartItem {
            MaMonAn = x.MaMonAn, Gia = x.Gia, TenMon = x.TenMon,
            SoLuong = r.Items.Single(i => i.MaMonAn == x.MaMonAn).SoLuong,
            TuyChonMon = r.Items.Single(i => i.MaMonAn == x.MaMonAn).TuyChonMon }).ToList() };
        var customer = await db.KhachHangs.SingleAsync(x => x.MaTK == CurrentMaTK);
        decimal discount = 0, shippingDiscount = 0; KhuyenMai? promotion = null;
        if (!string.IsNullOrWhiteSpace(r.MaKhuyenMai)) {
            var code = r.MaKhuyenMai.Trim().ToUpperInvariant();
            promotion = await db.KhuyenMais.SingleOrDefaultAsync(x => x.MaCode == code);
            if (promotion == null) return BadRequest(new { message = "Mã khuyến mãi không tồn tại." });
            var result = await KhuyenMaiHelper.KiemTraAsync(db, promotion, cart, r.MaGianHang, customer.MaKH, fee);
            if (!result.HopLe) return BadRequest(new { message = result.Loi });
            discount = result.GiamTienMon;
            shippingDiscount = result.GiamPhiGiaoHang;
        }
        var total = cart.TongTien - discount + fee - shippingDiscount;
        if (!save) return Ok(new { TamTinh = cart.TongTien, GiamGia = discount, PhiGiaoHang = fee, GiamPhiGiaoHang = shippingDiscount, TongTien = total });
        if (r.TongTienXacNhan != total) return Conflict(new { message = "Giá hoặc phí đã thay đổi. Hãy tính lại và xác nhận tổng tiền mới." });
        var order = new DonHang { MaKH = customer.MaKH, MaGianHang = r.MaGianHang, TongTien = total,
            PhiGiaoHang = fee, DiaChiGiaoHang = r.DiaChiGiaoHang.Trim(), GhiChu = r.GhiChu, MaKhuyenMaiApDung = promotion?.MaKhuyenMai,
            ChiTiets = cart.Items.Select(x => new ChiTietDonHang { MaMonAn = x.MaMonAn, TenMon = x.TenMon, SoLuong = x.SoLuong, DonGia = x.Gia, TuyChonMon = x.TuyChonMon }).ToList(),
            ThanhToan = new ThanhToan { HinhThuc = "TienMat", TrangThai = "ChoThanhToan" } };
        db.DonHangs.Add(order);
        await db.SaveChangesAsync();
        if (promotion != null) {
            promotion.SoLuongDaDung++;
            db.SuDungKhuyenMais.Add(new SuDungKhuyenMai { MaKhuyenMai = promotion.MaKhuyenMai,
                MaKH = customer.MaKH, MaDonHang = order.MaDonHang, NgaySuDung = DateTime.Now });
            await db.SaveChangesAsync();
        }
        await tx.CommitAsync();
        return Ok(new { order.MaDonHang, order.TongTien, TuyChonMon = cart.Items.FirstOrDefault()?.TuyChonMon });
    }
}
public class CustomerOrderRequest
{
    [Range(1,int.MaxValue)] public int MaGianHang { get; set; }
    [Required, StringLength(255)] public string DiaChiGiaoHang { get; set; } = "";
    [StringLength(255)] public string? GhiChu { get; set; }
    [StringLength(30)] public string? MaKhuyenMai { get; set; }
    public decimal? TongTienXacNhan { get; set; }
    [Required] public List<CustomerOrderItem> Items { get; set; } = new();
}
public class CustomerOrderItem
{
    [Range(1, int.MaxValue)] public int MaMonAn { get; set; }
    [Range(1, 99)] public int SoLuong { get; set; }
    [StringLength(500)] public string? TuyChonMon { get; set; }
}
