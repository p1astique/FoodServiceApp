using System.ComponentModel.DataAnnotations;
using FoodServiceApp.Web.Models.Entities;

namespace FoodServiceApp.Web.Models.ViewModels
{
    public class CustomerRegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SDT { get; set; } = "";

        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        public string XacNhanMatKhau { get; set; } = "";

        public string? LoiThongBao { get; set; }
    }

    public class CustomerHomeViewModel
    {
        public string? TuKhoa { get; set; }
        public string? KhuVuc { get; set; }
        public double? MinSao { get; set; }
        public string SapXep { get; set; } = "phu-hop";
        public int Trang { get; set; } = 1;
        public int TongTrang { get; set; } = 1;
        public int TongKetQua { get; set; }
        public List<GianHangCardViewModel> DanhSachGianHang { get; set; } = new();
    }

    public class GianHangCardViewModel
    {
        public int MaGianHang { get; set; }
        public string TenCuaHang { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string? HinhAnh { get; set; }
        public string TrangThaiKinhDoanh { get; set; } = "";
        public double DiemTrungBinh { get; set; }
        public int SoDanhGia { get; set; }
    }

    public class CartItem
    {
        public int MaMonAn { get; set; }
        public string TenMon { get; set; } = "";
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public string? HinhAnh { get; set; }
    }

    public class Cart
    {
        public int MaGianHang { get; set; }
        public string TenGianHang { get; set; } = "";
        public List<CartItem> Items { get; set; } = new();

        public decimal TongTien => Items.Sum(i => i.Gia * i.SoLuong);
        public int TongSoMon => Items.Sum(i => i.SoLuong);
    }

    public class CheckoutViewModel
    {
        public Cart GioHang { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string DiaChiGiaoHang { get; set; } = "";

        public string? GhiChu { get; set; }
        public string? MaKhuyenMai { get; set; }

        [Required]
        public string HinhThucThanhToan { get; set; } = "TienMat"; // TienMat / VNPay / Momo

        public decimal SoTienGiam { get; set; }
        /// <summary>Phí giao hàng ước tính hiển thị cho khách trước khi đặt — server tính lại chính xác lúc DatHang.</summary>
        public decimal PhiGiaoHangUocTinh { get; set; }
        public decimal ThanhTien => Math.Max(0, GioHang.TongTien - SoTienGiam) + PhiGiaoHangUocTinh;
        public string? LoiThongBao { get; set; }
    }

    public class CustomerHoSoViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; } = "";
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SDT { get; set; } = "";
        public string? Email { get; set; }
        public string? DiaChiGiaoHang { get; set; }
        public string? MatKhauMoi { get; set; }
        public string? XacNhanMatKhauMoi { get; set; }
    }

    public class DanhGiaFormViewModel
    {
        public int MaDonHang { get; set; }
        public int MaGianHang { get; set; }
        public string TenGianHang { get; set; } = "";

        [Range(1, 5, ErrorMessage = "Vui lòng chọn số sao từ 1 đến 5")]
        public int SoSao { get; set; } = 5;

        public string? NoiDung { get; set; }

        /// <summary>Đánh giá riêng từng món trong đơn (SoSao = 0 nghĩa là bỏ qua món đó).</summary>
        public List<MonDanhGiaItem> Mons { get; set; } = new();
    }

    public class MonDanhGiaItem
    {
        public int MaMonAn { get; set; }
        public string TenMon { get; set; } = "";
        public int SoSao { get; set; } // 0 = không đánh giá
        public string? NoiDung { get; set; }
    }

    public class DanhGiaMonHienThi
    {
        public string TenKhach { get; set; } = "";
        public int SoSao { get; set; }
        public string? NoiDung { get; set; }
        public DateTime Ngay { get; set; }
    }

    public class MonChiTietViewModel
    {
        public MonAn Mon { get; set; } = null!;
        public GianHang GianHang { get; set; } = null!;
        public double DiemTrungBinh { get; set; }
        public int SoDanhGia { get; set; }
        public List<DanhGiaMonHienThi> DanhGias { get; set; } = new();
    }

    public class DiaChiFormViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập nhãn địa chỉ (vd: Nhà, Cơ quan)")]
        public string Nhan { get; set; } = "";
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string DiaChi { get; set; } = "";
        public bool MacDinh { get; set; }
    }

    public class PhanHoiFormViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng chọn loại phản hồi")]
        public string Loai { get; set; } = "GopY";
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [System.ComponentModel.DataAnnotations.StringLength(150)]
        public string TieuDe { get; set; } = "";
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập nội dung")]
        [System.ComponentModel.DataAnnotations.StringLength(1000)]
        public string NoiDung { get; set; } = "";
        public int? MaDonHang { get; set; }
    }
}
