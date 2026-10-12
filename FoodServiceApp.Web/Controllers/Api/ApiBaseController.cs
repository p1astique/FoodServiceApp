using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;

namespace FoodServiceApp.Web.Controllers.Api
{
    // Toàn bộ Controllers/Api/* dùng JWT (cho mobile/FoodApp), khác với Controllers/*
    // (MVC, dùng Cookie cho trình duyệt). Chỉ định rõ AuthenticationSchemes ở đây để
    // không bị nhầm sang scheme mặc định (Cookie) đã đặt trong Program.cs.
    public abstract class ApiBaseController : ControllerBase, IAsyncActionFilter
    {
        protected int CurrentMaTK =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [NonAction]
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var db = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ||
                !await db.TaiKhoans.AnyAsync(x => x.MaTK == id && x.TrangThai))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Phiên đăng nhập không còn hợp lệ hoặc tài khoản đã bị khóa." });
                return;
            }
            if (User.IsInRole("GianHang") && !await db.GianHangs.AnyAsync(x => x.MaTK == id && x.TrangThaiDuyet == "DaDuyet"))
            {
                context.Result = new ObjectResult(new { message = "Gian hàng chưa được duyệt hoặc đã bị từ chối." }) { StatusCode = 403 };
                return;
            }
            await next();
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "GianHang")]
    public abstract class VendorApiBaseController : ApiBaseController { }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "QuanTri")]
    public abstract class AdminApiBaseController : ApiBaseController { }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "TaiXe")]
    public abstract class DriverApiBaseController : ApiBaseController { }
}
