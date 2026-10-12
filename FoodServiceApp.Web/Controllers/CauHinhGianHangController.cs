using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Dtos;
using FoodServiceApp.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Controllers;

public class CauHinhGianHangController(AppDbContext db) : GianHangBaseController
{
    public async Task<IActionResult> Index()
    {
        var item = await db.CauHinhGianHangs.FindAsync(CurrentGianHangId);
        return View(item == null ? new StoreSettingsRequest() : Map(item));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(StoreSettingsRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        var item = await db.CauHinhGianHangs.FindAsync(CurrentGianHangId);
        if (item == null)
        {
            item = new CauHinhGianHang { MaGianHang = CurrentGianHangId };
            db.CauHinhGianHangs.Add(item);
        }
        Apply(item, request);
        await db.SaveChangesAsync();
        TempData["ThongBao"] = "Đã lưu cấu hình vận hành cửa hàng.";
        return RedirectToAction(nameof(Index));
    }

    private static StoreSettingsRequest Map(CauHinhGianHang x) => new()
    {
        GioMoCua = x.GioMoCua, GioDongCua = x.GioDongCua, TuDongDongMo = x.TuDongDongMo,
        TamDungNhanDon = x.TamDungNhanDon, LyDoTamDung = x.LyDoTamDung,
        ThongBaoCuaHang = x.ThongBaoCuaHang, ThoiGianChuanBiPhut = x.ThoiGianChuanBiPhut
    };

    internal static void Apply(CauHinhGianHang x, StoreSettingsRequest r)
    {
        x.GioMoCua = r.GioMoCua; x.GioDongCua = r.GioDongCua; x.TuDongDongMo = r.TuDongDongMo;
        x.TamDungNhanDon = r.TamDungNhanDon; x.LyDoTamDung = r.LyDoTamDung?.Trim();
        x.ThongBaoCuaHang = r.ThongBaoCuaHang?.Trim(); x.ThoiGianChuanBiPhut = r.ThoiGianChuanBiPhut;
    }
}
