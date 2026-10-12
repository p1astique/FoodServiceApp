using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Dtos;
using FoodServiceApp.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Controllers;

public class ChienDichQuangCaoController(AppDbContext db) : GianHangBaseController
{
    public async Task<IActionResult> Index() => View(await db.ChienDichQuangCaos
        .Where(x => x.MaGianHang == CurrentGianHangId).OrderByDescending(x => x.NgayTao).ToListAsync());

    public IActionResult Create() => View("Form", new AdvertisingCampaignRequest
    {
        NgayBatDau = DateTime.Today, NgayKetThuc = DateTime.Today.AddDays(7)
    });

    public async Task<IActionResult> Edit(int id)
    {
        var x = await Owned(id); if (x == null) return NotFound();
        ViewBag.Id = id;
        return View("Form", new AdvertisingCampaignRequest
        {
            TenChienDich = x.TenChienDich, LoaiViTri = x.LoaiViTri, TieuDe = x.TieuDe,
            NoiDung = x.NoiDung, HinhAnh = x.HinhAnh, NgayBatDau = x.NgayBatDau,
            NgayKetThuc = x.NgayKetThuc, NganSach = x.NganSach, KichHoat = x.TrangThai == "DangChay"
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Create(AdvertisingCampaignRequest request) => Save(null, request);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Edit(int id, AdvertisingCampaignRequest request) => Save(id, request);

    private async Task<IActionResult> Save(int? id, AdvertisingCampaignRequest request)
    {
        if (!ModelState.IsValid) { ViewBag.Id = id; return View("Form", request); }
        var x = id == null ? new ChienDichQuangCao { MaGianHang = CurrentGianHangId } : await Owned(id.Value);
        if (x == null) return NotFound();
        x.TenChienDich = request.TenChienDich.Trim(); x.LoaiViTri = request.LoaiViTri;
        x.TieuDe = request.TieuDe.Trim(); x.NoiDung = request.NoiDung?.Trim(); x.HinhAnh = request.HinhAnh?.Trim();
        x.NgayBatDau = request.NgayBatDau; x.NgayKetThuc = request.NgayKetThuc; x.NganSach = request.NganSach;
        x.TrangThai = request.KichHoat ? "DangChay" : "BanNhap";
        if (id == null) db.ChienDichQuangCaos.Add(x);
        await db.SaveChangesAsync();
        TempData["ThongBao"] = "Đã lưu chiến dịch quảng cáo.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var x = await Owned(id); if (x == null) return NotFound();
        if (x.NgayKetThuc <= DateTime.Now) TempData["LoiThongBao"] = "Chiến dịch đã hết hạn, hãy sửa ngày kết thúc trước khi bật.";
        else { x.TrangThai = x.TrangThai == "DangChay" ? "TamDung" : "DangChay"; await db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var x = await Owned(id); if (x == null) return NotFound();
        db.ChienDichQuangCaos.Remove(x); await db.SaveChangesAsync();
        TempData["ThongBao"] = "Đã xóa chiến dịch.";
        return RedirectToAction(nameof(Index));
    }

    private Task<ChienDichQuangCao?> Owned(int id) => db.ChienDichQuangCaos
        .SingleOrDefaultAsync(x => x.MaChienDich == id && x.MaGianHang == CurrentGianHangId);
}
