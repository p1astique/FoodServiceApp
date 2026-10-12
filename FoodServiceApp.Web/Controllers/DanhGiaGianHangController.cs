using FoodServiceApp.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Controllers;
public class DanhGiaGianHangController(AppDbContext db) : GianHangBaseController
{
    public async Task<IActionResult> Index()
    {
        var reviews = await (from d in db.DanhGias join k in db.KhachHangs on d.MaKH equals k.MaKH
            where d.MaGianHang == CurrentGianHangId orderby d.NgayDanhGia descending
            select new { d.MaDanhGia, Loai = "Cửa hàng", Ten = k.HoTen, d.SoSao, d.NoiDung, Ngay = d.NgayDanhGia, d.MaDonHang, d.PhanHoiGianHang, d.NgayPhanHoi }).ToListAsync();
        var itemReviews = await (from d in db.DanhGiaMons join k in db.KhachHangs on d.MaKH equals k.MaKH
            join m in db.MonAns on d.MaMonAn equals m.MaMonAn join c in db.DanhMucs on m.MaDanhMuc equals c.MaDanhMuc
            where c.MaGianHang == CurrentGianHangId orderby d.NgayDanhGia descending
            select new { Loai = m.TenMon, Ten = k.HoTen, d.SoSao, d.NoiDung, Ngay = d.NgayDanhGia, d.MaDonHang }).ToListAsync();
        ViewBag.DiemTrungBinh = reviews.Count == 0 ? 0 : reviews.Average(x => x.SoSao);
        return View(reviews.Cast<object>().Concat(itemReviews.Cast<object>()).ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PhanHoi(int id, string noiDung)
    {
        var review = await db.DanhGias.FirstOrDefaultAsync(x => x.MaDanhGia == id && x.MaGianHang == CurrentGianHangId);
        if (review == null) return NotFound();
        if (string.IsNullOrWhiteSpace(noiDung) || noiDung.Trim().Length > 500)
        { TempData["LoiThongBao"] = "Phản hồi phải từ 1 đến 500 ký tự."; return RedirectToAction("Index"); }
        review.PhanHoiGianHang = noiDung.Trim(); review.NgayPhanHoi = DateTime.Now;
        await db.SaveChangesAsync();
        TempData["ThongBao"] = "Đã lưu phản hồi đánh giá.";
        return RedirectToAction("Index");
    }
}
