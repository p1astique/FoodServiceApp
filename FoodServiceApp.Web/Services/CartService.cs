using System.Text.Json;
using FoodServiceApp.Web.Models.ViewModels;

namespace FoodServiceApp.Web.Services
{
    /// <summary>
    /// Giỏ hàng lưu trong Session (không cần bảng CSDL riêng) — mỗi giỏ hàng chỉ
    /// chứa món của MỘT gian hàng tại một thời điểm, giống các app đặt đồ ăn thật.
    /// </summary>
    public class CartService
    {
        private const string SessionKey = "GioHang";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public Cart LayGioHang()
        {
            var json = Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return new Cart();
            return JsonSerializer.Deserialize<Cart>(json) ?? new Cart();
        }

        private void Luu(Cart cart)
        {
            Session.SetString(SessionKey, JsonSerializer.Serialize(cart));
        }

        public void XoaGioHang() => Session.Remove(SessionKey);

        /// <returns>null nếu thêm thành công; ngược lại trả về thông báo lỗi (khác gian hàng)</returns>
        public string? ThemMon(int maGianHang, string tenGianHang, CartItem monMoi)
        {
            var cart = LayGioHang();

            if (cart.Items.Any() && cart.MaGianHang != maGianHang)
                return "Giỏ hàng đang có món của gian hàng khác. Vui lòng đặt xong hoặc xoá giỏ hàng hiện tại trước khi thêm món từ gian hàng mới.";

            cart.MaGianHang = maGianHang;
            cart.TenGianHang = tenGianHang;

            var monCu = cart.Items.FirstOrDefault(i => i.MaMonAn == monMoi.MaMonAn);
            if (monCu != null)
                monCu.SoLuong += monMoi.SoLuong;
            else
                cart.Items.Add(monMoi);

            Luu(cart);
            return null;
        }

        public void CapNhatSoLuong(int maMonAn, int soLuong)
        {
            var cart = LayGioHang();
            var mon = cart.Items.FirstOrDefault(i => i.MaMonAn == maMonAn);
            if (mon == null) return;

            if (soLuong <= 0) cart.Items.Remove(mon);
            else mon.SoLuong = Math.Clamp(soLuong, 1, 50);

            if (!cart.Items.Any()) { XoaGioHang(); return; }
            Luu(cart);
        }

        public void CapNhatThongTinMon(int maMonAn, string tenMon, decimal gia, string? hinhAnh)
        {
            var cart = LayGioHang();
            var mon = cart.Items.FirstOrDefault(i => i.MaMonAn == maMonAn);
            if (mon == null) return;

            mon.TenMon = tenMon;
            mon.Gia = gia;
            mon.HinhAnh = hinhAnh;
            Luu(cart);
        }
    }
}
