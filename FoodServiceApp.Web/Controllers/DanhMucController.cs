using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Dtos;
using FoodServiceApp.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Controllers;

public class DanhMucController(AppDbContext db) : GianHangBaseController
{
    public async Task<IActionResult> Index() => View(await db.DanhMucs.Include(x => x.MonAns)
        .Where(x => x.MaGianHang == CurrentGianHangId).OrderBy(x => x.ThuTuHienThi).ThenBy(x => x.TenDanhMuc).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int? id, CategoryRequest request)
    {
        var name = request.TenDanhMuc?.Trim();
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(name))
            TempData["LoiThongBao"] = "Tên danh mục phải có từ 1 đến 100 ký tự.";
        else if (await db.DanhMucs.AnyAsync(x => x.MaGianHang == CurrentGianHangId && x.TenDanhMuc == name && (id == null || x.MaDanhMuc != id)))
            TempData["LoiThongBao"] = "Danh mục đã tồn tại.";
        else
        {
            var item = id == null ? new DanhMuc { MaGianHang = CurrentGianHangId } :
                await db.DanhMucs.SingleOrDefaultAsync(x => x.MaDanhMuc == id && x.MaGianHang == CurrentGianHangId);
            if (item == null) return NotFound();
            item.TenDanhMuc = name;
            item.MoTa = request.MoTa?.Trim();
            item.ThuTuHienThi = request.ThuTuHienThi;
            item.TrangThai = request.TrangThai;
            if (id == null) db.DanhMucs.Add(item);
            await db.SaveChangesAsync(); TempData["ThongBao"] = "Đã lưu danh mục.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var item = await db.DanhMucs.SingleOrDefaultAsync(x => x.MaDanhMuc == id && x.MaGianHang == CurrentGianHangId);
        if (item == null) return NotFound();
        item.TrangThai = !item.TrangThai;
        await db.SaveChangesAsync();
        TempData["ThongBao"] = item.TrangThai ? "Đã hiển thị danh mục." : "Đã ẩn danh mục khỏi khách hàng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.DanhMucs.SingleOrDefaultAsync(x => x.MaDanhMuc == id && x.MaGianHang == CurrentGianHangId);
        if (item == null) return NotFound();
        if (await db.MonAns.AnyAsync(x => x.MaDanhMuc == id))
            TempData["LoiThongBao"] = "Hãy chuyển hoặc xóa các món trước khi xóa danh mục.";
        else { db.DanhMucs.Remove(item); await db.SaveChangesAsync(); TempData["ThongBao"] = "Đã xóa danh mục."; }
        return RedirectToAction(nameof(Index));
    }
}
