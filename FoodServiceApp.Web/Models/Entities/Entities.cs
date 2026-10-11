using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodServiceApp.Web.Models.Entities
{
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key] public int MaTK { get; set; }
        public string TenDangNhap { get; set; } = "";
        public string MatKhau { get; set; } = "";
        public string VaiTro { get; set; } = ""; // KhachHang / GianHang / QuanTri / TaiXe
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public bool TrangThai { get; set; } = true;
    }

    [Table("GianHang")]
    public class GianHang
    {
        [Key] public int MaGianHang { get; set; }
        public int MaTK { get; set; }
        public TaiKhoan? TaiKhoan { get; set; }
        public string TenCuaHang { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string SDT { get; set; } = "";
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public string TrangThaiKinhDoanh { get; set; } = "DangMo"; // DangMo / DongCua / TamNgung
        public string TrangThaiDuyet { get; set; } = "ChoDuyet"; // ChoDuyet / DaDuyet / TuChoi
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        public ICollection<DanhMuc> DanhMucs { get; set; } = new List<DanhMuc>();
        public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
        public ICollection<KhuyenMai> KhuyenMais { get; set; } = new List<KhuyenMai>();
    }

    [Table("QuanTri")]
    public class QuanTri
    {
        [Key] public int MaQuanTri { get; set; }
        public int MaTK { get; set; }
        public string HoTen { get; set; } = "";
        public string SDT { get; set; } = "";
        public string? Email { get; set; }
        public string CapQuyenHan { get; set; } = "NhanVien";
        public string? PhongBanPhuTrach { get; set; }
    }

    [Table("KhachHang")]
    public class KhachHang
    {
        [Key] public int MaKH { get; set; }
        public int MaTK { get; set; }
        public string HoTen { get; set; } = "";
        public string SDT { get; set; } = "";
        public string? Email { get; set; }
        public string? DiaChiGiaoHang { get; set; }
    }

    [Table("TaiXe")]
    public class TaiXe
    {
        [Key] public int MaTaiXe { get; set; }
        public int MaTK { get; set; }
        public string HoTen { get; set; } = "";
        public string SDT { get; set; } = "";
        public string? PhuongTien { get; set; }
        public string? BienSo { get; set; }
        public string? KhuVucHoatDong { get; set; }
        public string TrangThai { get; set; } = "SanSang";
        public string TrangThaiDuyet { get; set; } = "DaDuyet"; // ChoDuyet / DaDuyet / TuChoi
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        public ICollection<GiaoHang> GiaoHangs { get; set; } = new List<GiaoHang>();
    }

    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key] public int MaDanhMuc { get; set; }
        public int MaGianHang { get; set; }
        public GianHang? GianHang { get; set; }
        public string TenDanhMuc { get; set; } = "";

        public ICollection<MonAn> MonAns { get; set; } = new List<MonAn>();
    }

    [Table("MonAn")]
    public class MonAn
    {
        [Key] public int MaMonAn { get; set; }
        public int MaDanhMuc { get; set; }
        public DanhMuc? DanhMuc { get; set; }
        public string TenMon { get; set; } = "";
        public decimal Gia { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public string TinhTrang { get; set; } = "ConHang"; // ConHang / HetHang
    }

    [Table("DonHang")]
    public class DonHang
    {
        [Key] public int MaDonHang { get; set; }
        public int MaKH { get; set; }
        public int MaGianHang { get; set; }
        public GianHang? GianHang { get; set; }
        public int? MaTaiXe { get; set; }
        public DateTime NgayDat { get; set; } = DateTime.Now;
        // ChoXacNhan / DaXacNhan / DangGiao / DaGiao / DaHuy
        public string TrangThai { get; set; } = "ChoXacNhan";
        public decimal TongTien { get; set; }
        /// <summary>Phí giao hàng (tính theo khoảng cách qua Goong Maps, xem CustomerCartController) — toàn bộ khoản này là thu nhập của tài xế khi giao thành công.</summary>
        public decimal PhiGiaoHang { get; set; } = 0;
        public string DiaChiGiaoHang { get; set; } = "";
        public string? GhiChu { get; set; }
        public int? MaKhuyenMaiApDung { get; set; }
        public KhuyenMai? KhuyenMaiApDung { get; set; }

        public ICollection<ChiTietDonHang> ChiTiets { get; set; } = new List<ChiTietDonHang>();
        public ThanhToan? ThanhToan { get; set; }
        public GiaoHang? GiaoHang { get; set; }
    }

    [Table("ChiTietDonHang")]
    public class ChiTietDonHang
    {
        [Key] public int MaCTDH { get; set; }
        public int MaDonHang { get; set; }
        public int MaMonAn { get; set; }
        public MonAn? MonAn { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        [NotMapped] public decimal ThanhTien => SoLuong * DonGia;
    }

    [Table("ThanhToan")]
    public class ThanhToan
    {
        [Key] public int MaThanhToan { get; set; }
        public int MaDonHang { get; set; }
        public string HinhThuc { get; set; } = "TienMat"; // TienMat / VNPay / Momo
        public string TrangThai { get; set; } = "ChoThanhToan";
        public string? MaGiaoDich { get; set; }
        public DateTime? NgayThanhToan { get; set; }
    }

    [Table("KhuyenMai")]
    public class KhuyenMai
    {
        [Key] public int MaKhuyenMai { get; set; }
        public int? MaGianHang { get; set; } // null = áp dụng toàn hệ thống
        public int? MaMonAn { get; set; }    // null = áp dụng cho cả đơn; có giá trị = chỉ giảm trên món này
        public string MaCode { get; set; } = "";
        public string? MoTa { get; set; }
        public decimal? PhanTramGiam { get; set; }
        public decimal? SoTienGiam { get; set; }
        public string? DieuKienApDung { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public bool TrangThai { get; set; } = true;

        [NotMapped] public bool ApDungToanHeThong => MaGianHang == null;
        [NotMapped] public bool ConHan => DateTime.Now <= NgayKetThuc;
        [NotMapped] public bool ChuaBatDau => DateTime.Now < NgayBatDau;
    }

    [Table("TaiXeTuChoiDon")]
    public class TaiXeTuChoiDon
    {
        [Key] public int MaTuChoi { get; set; }
        public int MaTaiXe { get; set; }
        public int MaDonHang { get; set; }
        public DateTime NgayTuChoi { get; set; } = DateTime.Now;
    }

    [Table("DanhGia")]
    public class DanhGia
    {
        [Key] public int MaDanhGia { get; set; }
        public int MaKH { get; set; }
        public int MaGianHang { get; set; }
        public int MaDonHang { get; set; }
        public int SoSao { get; set; }
        public string? NoiDung { get; set; }
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;
    }

    [Table("GiaoHang")]
    public class GiaoHang
    {
        [Key] public int MaGiaoHang { get; set; }
        public int MaDonHang { get; set; }
        public DonHang? DonHang { get; set; }
        public int MaTaiXe { get; set; }
        public TaiXe? TaiXe { get; set; }
        // DaNhan / DangGiao / GiaoThanhCong / GiaoThatBai
        public string TrangThai { get; set; } = "DaNhan";
        public DateTime? ThoiGianNhan { get; set; }
        public DateTime? ThoiGianGiao { get; set; }
    }

    [Table("DanhGiaMon")]
    public class DanhGiaMon
    {
        [Key] public int MaDanhGiaMon { get; set; }
        public int MaKH { get; set; }
        public int MaMonAn { get; set; }
        public MonAn? MonAn { get; set; }
        public int MaDonHang { get; set; }
        public int SoSao { get; set; }
        public string? NoiDung { get; set; }
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;
    }

    [Table("DiaChiKhachHang")]
    public class DiaChiKhachHang
    {
        [Key] public int MaDiaChi { get; set; }
        public int MaKH { get; set; }
        public string Nhan { get; set; } = "Nhà"; // Nhà / Cơ quan / ...
        public string DiaChi { get; set; } = "";
        public bool MacDinh { get; set; }
    }

    [Table("PhanHoi")]
    public class PhanHoi
    {
        [Key] public int MaPhanHoi { get; set; }
        public int MaKH { get; set; }
        public KhachHang? KhachHang { get; set; }
        public int? MaDonHang { get; set; }
        public int? MaGianHang { get; set; }
        public string Loai { get; set; } = "GopY"; // GopY / KhieuNai / Khac
        public string TieuDe { get; set; } = "";
        public string NoiDung { get; set; } = "";
        public string TrangThai { get; set; } = "ChuaXuLy"; // ChuaXuLy / DaXuLy
        public string? TraLoi { get; set; }
        public DateTime NgayGui { get; set; } = DateTime.Now;
        public DateTime? NgayXuLy { get; set; }
    }

}
