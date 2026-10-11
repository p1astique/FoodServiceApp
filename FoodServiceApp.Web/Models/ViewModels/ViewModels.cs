using System.ComponentModel.DataAnnotations;
using FoodServiceApp.Web.Models.Entities;

namespace FoodServiceApp.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = "";

        public string? LoiThongBao { get; set; }
    }

    public class VendorRegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên cửa hàng")]
        public string TenCuaHang { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string DiaChi { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SDT { get; set; } = "";

        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        public string XacNhanMatKhau { get; set; } = "";
    }

    public class DriverRegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SDT { get; set; } = "";

        public string? PhuongTien { get; set; }
        public string? BienSo { get; set; }
        public string? KhuVucHoatDong { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        public string XacNhanMatKhau { get; set; } = "";
    }

    public class AdminCreateDriverViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SDT { get; set; } = "";

        public string? PhuongTien { get; set; }
        public string? BienSo { get; set; }
        public string? KhuVucHoatDong { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        public string XacNhanMatKhau { get; set; } = "";
    }

    public class DashboardViewModel
    {
        public GianHang GianHang { get; set; } = null!;
        public decimal DoanhThuHomNay { get; set; }
        public int SoDonHomNay { get; set; }
        public int SoDonChoXacNhan { get; set; }
        public int SoMonAn { get; set; }
        public List<DonHang> DonMoiNhat { get; set; } = new();
    }

    public class MonAnFormViewModel
    {
        public int MaMonAn { get; set; }
        public int MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên món")]
        public string TenMon { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(0, 100000000, ErrorMessage = "Giá không hợp lệ")]
        public decimal Gia { get; set; }

        public string? MoTa { get; set; }
        public string? HinhAnhHienTai { get; set; }
        public IFormFile? AnhMoi { get; set; }
        public bool ConHang { get; set; } = true;

        public string TenDanhMucMoi { get; set; } = "";
        public List<DanhMuc> DanhSachDanhMuc { get; set; } = new();
    }

    public class KhuyenMaiFormViewModel
    {
        public int MaKhuyenMai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã khuyến mãi")]
        public string MaCode { get; set; } = "";

        public string? MoTa { get; set; }

        [Required] public string LoaiGiam { get; set; } = "PhanTram"; // PhanTram / SoTien
        public decimal GiaTriGiam { get; set; }
        public string? DieuKienApDung { get; set; }
        public int? MaMonAn { get; set; } // chọn món để chỉ giảm giá món đó; để trống = cả đơn
        public List<MonAn> DanhSachMon { get; set; } = new();

        [Required] public DateTime NgayBatDau { get; set; } = DateTime.Today;
        [Required] public DateTime NgayKetThuc { get; set; } = DateTime.Today.AddMonths(1);
        public bool TrangThai { get; set; } = true;
    }

    public class HoSoViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên cửa hàng")]
        public string TenCuaHang { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string DiaChi { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SDT { get; set; } = "";

        public string? MoTa { get; set; }
        public string? HinhAnhHienTai { get; set; }
        public IFormFile? AnhMoi { get; set; }

        public string? MatKhauMoi { get; set; }
        public string? XacNhanMatKhauMoi { get; set; }
    }

    public class DoanhThuViewModel
    {
        public decimal TongDoanhThu30Ngay { get; set; }
        public int TongDonHoanThanh30Ngay { get; set; }
        public List<(DateTime Ngay, decimal DoanhThu)> DoanhThuTheoNgay { get; set; } = new();
        public List<(string TenMon, int SoLuongBan)> MonBanChay { get; set; } = new();
    }

    public class DriverThuNhapViewModel
    {
        public decimal TongThuNhap30Ngay { get; set; }
        public int TongDonGiaoThanhCong30Ngay { get; set; }
        public List<(DateTime Ngay, decimal ThuNhap)> ThuNhapTheoNgay { get; set; } = new();
        public List<GiaoHang> LichSuGanDay { get; set; } = new();
    }
}
