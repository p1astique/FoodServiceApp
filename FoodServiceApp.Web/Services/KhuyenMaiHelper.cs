using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Models.ViewModels;

namespace FoodServiceApp.Web.Services
{
    /// <summary>Tính tiền giảm của một mã khuyến mãi trên giỏ hàng (áp theo gian hàng hoặc theo từng món).</summary>
    public static class KhuyenMaiHelper
    {
        public static decimal TinhGiam(KhuyenMai km, Cart gio, out string? loi)
        {
            loi = null;
            var coSo = km.MaMonAn == null
                ? gio.TongTien
                : gio.Items.Where(i => i.MaMonAn == km.MaMonAn).Sum(i => i.Gia * i.SoLuong);
            if (coSo <= 0)
            {
                loi = "Mã này chỉ áp dụng cho một món cụ thể mà giỏ hàng của bạn chưa có.";
                return 0;
            }
            var giam = km.PhanTramGiam != null
                ? Math.Round(coSo * km.PhanTramGiam.Value / 100)
                : (km.SoTienGiam ?? 0);
            return Math.Clamp(giam, 0, coSo);
        }
    }
}
