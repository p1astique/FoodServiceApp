using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FoodServiceApp.Web.Controllers
{
    [Authorize(Roles = "GianHang")]
    public abstract class GianHangBaseController : Controller
    {
        protected int CurrentGianHangId =>
            int.Parse(User.FindFirst("MaGianHang")!.Value);

        // Gian hàng chưa được Quản trị duyệt thì không cho vào các trang nghiệp vụ
        // (trừ chính trang "Chờ duyệt" và Hồ sơ, để họ vẫn xem/sửa được thông tin
        // đã đăng ký trong lúc chờ).
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var db = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var store = await db.GianHangs.Include(x => x.TaiKhoan).FirstOrDefaultAsync(x => x.MaGianHang == CurrentGianHangId);
            if (store?.TaiKhoan?.TrangThai != true || store.TrangThaiDuyet == "TuChoi")
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }
            var trangThaiDuyet = store.TrangThaiDuyet;
            var controllerTen = context.RouteData.Values["controller"]?.ToString();
            var actionTen = context.RouteData.Values["action"]?.ToString();

            var duocPhepKhiChuaDuyet =
                (controllerTen == "Dashboard" && actionTen == "ChoDuyet") ||
                controllerTen == "HoSo";

            if (trangThaiDuyet != "DaDuyet" && !duocPhepKhiChuaDuyet)
            {
                context.Result = new RedirectToActionResult("ChoDuyet", "Dashboard", null);
                return;
            }
            await next();
        }
    }
}
