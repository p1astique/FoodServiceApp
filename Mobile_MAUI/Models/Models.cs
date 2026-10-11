using System.Collections.ObjectModel;

namespace FoodServiceApp.Maui.Models;

public class GianHang
{
    public string Id { get; set; } = "";
    public string TenGianHang { get; set; } = "";
    public string DiaChi { get; set; } = "";
    public string HinhAnh { get; set; } = "🍽️";
    public double DanhGia { get; set; }
    public int ThoiGianGiaoPhut { get; set; }

    // --- bổ sung cho luồng quản lý gian hàng (đối tác) ---
    public string SoDienThoai { get; set; } = "";
    public string Email { get; set; } = "";
    public string MatKhau { get; set; } = "";           // demo only — sau này KHÔNG lưu plaintext, xác thực qua API
    public string MoTa { get; set; } = "";
    public bool DangMoCua { get; set; } = true;          // tình trạng kinh doanh: đang mở bán / tạm đóng (do gian hàng tự bật/tắt)
    public bool TaiKhoanBiKhoa { get; set; } = false;     // do quản trị viên khóa khi có vi phạm/khiếu nại
    public DateTime NgayDangKy { get; set; } = DateTime.Now;
    public TrangThaiDangKyGianHang TrangThaiDangKy { get; set; } = TrangThaiDangKyGianHang.DaDuyet;
}

/// <summary>Trạng thái duyệt hồ sơ khi gian hàng đăng ký tham gia nền tảng.</summary>
public enum TrangThaiDangKyGianHang
{
    ChoDuyet,
    DaDuyet,
    TuChoi
}

public static class TrangThaiDangKyGianHangExtensions
{
    public static string Nhan(this TrangThaiDangKyGianHang t) => t switch
    {
        TrangThaiDangKyGianHang.ChoDuyet => "Chờ duyệt",
        TrangThaiDangKyGianHang.DaDuyet => "Đã duyệt",
        TrangThaiDangKyGianHang.TuChoi => "Bị từ chối",
        _ => ""
    };
}

public class MonAn
{
    public string Id { get; set; } = "";
    public string TenMon { get; set; } = "";
    public decimal Gia { get; set; }
    public string HinhAnh { get; set; } = "🍲";
    public string DanhMuc { get; set; } = "";
    public string GianHangId { get; set; } = "";
    public bool ConHang { get; set; } = true;            // tình trạng kinh doanh của món: còn bán / tạm hết
}

/// <summary>Tài khoản khách hàng — dùng cho phía quản trị để theo dõi/khóa tài khoản.</summary>
public class KhachHang
{
    public string Id { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
    public string MatKhau { get; set; } = "";
    public string SoDienThoai { get; set; } = "";
    public DateTime NgayDangKy { get; set; } = DateTime.Now;
    public int TongSoDon { get; set; }
    public bool TaiKhoanBiKhoa { get; set; } = false;
}

/// <summary>Tài khoản quản trị viên hệ thống.</summary>
public class QuanTriVien
{
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
    public string MatKhau { get; set; } = "";
}

public enum TrangThaiVanDe { MoiPhatSinh, DangXuLy, DaGiaiQuyet }

public static class TrangThaiVanDeExtensions
{
    public static string Nhan(this TrangThaiVanDe t) => t switch
    {
        TrangThaiVanDe.MoiPhatSinh => "Mới phát sinh",
        TrangThaiVanDe.DangXuLy => "Đang xử lý",
        TrangThaiVanDe.DaGiaiQuyet => "Đã giải quyết",
        _ => ""
    };
}

/// <summary>Vấn đề/khiếu nại phát sinh trong quá trình vận hành, do quản trị viên tiếp nhận và xử lý.</summary>
public class VanDeHeThong
{
    public string Id { get; set; } = "";
    public string TieuDe { get; set; } = "";
    public string MoTa { get; set; } = "";
    public string NguonGoc { get; set; } = "";           // VD: tên gian hàng hoặc khách hàng liên quan
    public DateTime NgayBaoCao { get; set; } = DateTime.Now;
    public TrangThaiVanDe TrangThai { get; set; } = TrangThaiVanDe.MoiPhatSinh;
}

public enum LoaiGiamGia { PhanTram, SoTien }

public static class LoaiGiamGiaExtensions
{
    public static string Nhan(this LoaiGiamGia t) => t switch
    {
        LoaiGiamGia.PhanTram => "Giảm theo %",
        LoaiGiamGia.SoTien => "Giảm số tiền cố định",
        _ => ""
    };
}

/// <summary>
/// Chương trình khuyến mãi / mã giảm giá / voucher. Có thể do một gian hàng tự tạo
/// (<see cref="GianHangId"/> khác rỗng) hoặc do quản trị viên tạo áp dụng toàn hệ thống
/// (<see cref="GianHangId"/> rỗng/null — xem <see cref="ApDungToanHeThong"/>).
/// </summary>
public class KhuyenMai
{
    public string Id { get; set; } = "";
    public string MaCode { get; set; } = "";
    public string TenChuongTrinh { get; set; } = "";
    public string MoTa { get; set; } = "";
    public LoaiGiamGia LoaiGiam { get; set; } = LoaiGiamGia.PhanTram;
    public decimal GiaTriGiam { get; set; }              // % (0-100) nếu LoaiGiam=PhanTram, hoặc số tiền cố định nếu SoTien
    public decimal GiaTriDonToiThieu { get; set; }        // đơn hàng tối thiểu để áp dụng được mã
    public decimal? GiamToiDa { get; set; }               // trần số tiền giảm — chỉ áp dụng khi LoaiGiam=PhanTram
    public DateTime NgayBatDau { get; set; } = DateTime.Now;
    public DateTime NgayKetThuc { get; set; } = DateTime.Now.AddMonths(1);
    public string? GianHangId { get; set; }                // null/"" = áp dụng cho TOÀN HỆ THỐNG
    public int SoLuongToiDa { get; set; } = 100;           // giới hạn tổng lượt sử dụng
    public int SoLuongDaDung { get; set; } = 0;
    public bool DangHoatDong { get; set; } = true;         // do người tạo bật/tắt thủ công

    public bool ApDungToanHeThong => string.IsNullOrEmpty(GianHangId);
    public bool ConHan => DateTime.Now <= NgayKetThuc;
    public bool ChuaBatDau => DateTime.Now < NgayBatDau;
    public bool ConLuot => SoLuongDaDung < SoLuongToiDa;

    /// <summary>Trạng thái hiển thị tổng hợp cho danh sách quản lý.</summary>
    public string TrangThaiHienThi
    {
        get
        {
            if (!DangHoatDong) return "Đã tắt";
            if (ChuaBatDau) return "Chưa bắt đầu";
            if (!ConHan) return "Đã hết hạn";
            if (!ConLuot) return "Hết lượt dùng";
            return "Đang hoạt động";
        }
    }

    /// <summary>Mô tả ngắn gọn mức giảm, VD "Giảm 20% (tối đa 30.000đ)" hoặc "Giảm 15.000đ".</summary>
    public string GiaTriGiamHienThi => LoaiGiam == LoaiGiamGia.PhanTram
        ? $"Giảm {GiaTriGiam:0.#}%" + (GiamToiDa is decimal tran ? $" (tối đa {tran:N0}đ)" : "")
        : $"Giảm {GiaTriGiam:N0}đ";

    /// <summary>Phạm vi áp dụng để hiển thị, VD "Toàn hệ thống" hoặc tên gian hàng cụ thể.</summary>
    public string PhamViHienThi => ApDungToanHeThong
        ? "Toàn hệ thống"
        : MockData.DanhSachGianHang.FirstOrDefault(g => g.Id == GianHangId)?.TenGianHang ?? "Gian hàng";
}

public class MucGioHang
{
    public MonAn MonAn { get; set; } = null!;
    public int SoLuong { get; set; } = 1;

    public decimal ThanhTien => MonAn.Gia * SoLuong;
}

/// <summary>Tài xế giao hàng.</summary>
public class TaiXe
{
    public string Id { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string Anh { get; set; } = "🛵";
    public string SoDienThoai { get; set; } = "";
    public string BienSoXe { get; set; } = "";
    public double DanhGia { get; set; }
    public bool DangHoatDong { get; set; } = true; // đang bật nhận đơn hay không
    public decimal TongThuNhapHomNay { get; set; }
    public int SoDonHomNay { get; set; }
}

public enum TrangThaiDonHang
{
    ChoXacNhan,
    DangLam,
    DangGiao,
    HoanThanh,
    DaHuy
}

public static class TrangThaiDonHangExtensions
{
    public static string Nhan(this TrangThaiDonHang t) => t switch
    {
        TrangThaiDonHang.ChoXacNhan => "Chờ xác nhận",
        TrangThaiDonHang.DangLam => "Đang chuẩn bị",
        TrangThaiDonHang.DangGiao => "Đang giao",
        TrangThaiDonHang.HoanThanh => "Hoàn thành",
        TrangThaiDonHang.DaHuy => "Đã hủy",
        _ => ""
    };

    // Bước tương ứng trên thanh tiến trình 4 bước (0-3)
    public static int Buoc(this TrangThaiDonHang t) => t switch
    {
        TrangThaiDonHang.ChoXacNhan => 0,
        TrangThaiDonHang.DangLam => 1,
        TrangThaiDonHang.DangGiao => 2,
        TrangThaiDonHang.HoanThanh => 3,
        TrangThaiDonHang.DaHuy => 0,
        _ => 0
    };
}

public class DonHang
{
    public string Id { get; set; } = "";
    public string GianHangId { get; set; } = "";         // dùng để lọc đơn hàng theo từng gian hàng (đối tác)
    public string TenGianHang { get; set; } = "";
    public List<MucGioHang> ChiTiet { get; set; } = new();
    public TrangThaiDonHang TrangThai { get; set; }
    public string DiaChiGiao { get; set; } = "";
    public DateTime NgayDat { get; set; }

    // --- bổ sung cho luồng tài xế giao hàng ---
    public string DiaChiLayHang { get; set; } = ""; // địa chỉ gian hàng, nơi tài xế đến lấy món
    public string? TaiXeId { get; set; }             // null = chưa có tài xế nhận
    public decimal PhiGiaoHang { get; set; } = 15000; // tiền tài xế nhận cho đơn này

    public decimal TongTien => ChiTiet.Sum(x => x.ThanhTien);
}

/// <summary>
/// Dữ liệu mẫu (mock data) để dựng giao diện — sau này thay bằng gọi
/// ASP.NET Core Web API thật (xem Services/ApiClient.cs).
/// </summary>
public static class MockData
{
    // Danh mục được lưu riêng theo từng gian hàng, ban đầu suy ra từ các món mẫu hiện có.
    public static Dictionary<string, List<string>> DanhMucTheoGianHang { get; } = new()
    {
        ["gh1"] = new() { "Món chính", "Món phụ", "Nước uống", "Tráng miệng" }
    };

    public static List<string> DanhMucCuaGianHang(string gianHangId)
    {
        if (!DanhMucTheoGianHang.TryGetValue(gianHangId, out var danhMuc))
        {
            danhMuc = DanhSachMonAn.Where(m => m.GianHangId == gianHangId)
                .Select(m => m.DanhMuc).Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            DanhMucTheoGianHang[gianHangId] = danhMuc;
        }
        return danhMuc;
    }

    public static List<GianHang> DanhSachGianHang { get; } = new()
    {
        new GianHang
        {
            Id = "gh1", TenGianHang = "Cơm Tấm Sài Gòn", DiaChi = "12 Nguyễn Trãi, Q.5", HinhAnh = "🍚",
            DanhGia = 4.7, ThoiGianGiaoPhut = 25,
            SoDienThoai = "0908 111 222", Email = "comtam.saigon@gianhang.vn", MatKhau = "123456",
            MoTa = "Cơm tấm sườn bì chả chuẩn vị Sài Gòn, bán từ 6h-21h mỗi ngày.",
            DangMoCua = true, TrangThaiDangKy = TrangThaiDangKyGianHang.DaDuyet,
        },
        new GianHang { Id = "gh2", TenGianHang = "Trà Sữa Mộng Mơ", DiaChi = "88 Lê Văn Việt, Q.9", HinhAnh = "🧋", DanhGia = 4.5, ThoiGianGiaoPhut = 15 },
        new GianHang { Id = "gh3", TenGianHang = "Bún Bò Huế Cô Ba", DiaChi = "45 Hoàng Diệu, Q.4", HinhAnh = "🍜", DanhGia = 4.8, ThoiGianGiaoPhut = 30 },
        new GianHang { Id = "gh4", TenGianHang = "Pizza Góc Phố", DiaChi = "9 Cách Mạng Tháng 8", HinhAnh = "🍕", DanhGia = 4.3, ThoiGianGiaoPhut = 35 },
    };

    public static List<MonAn> DanhSachMonAn { get; } = new()
    {
        new MonAn { Id = "ma1", TenMon = "Cơm tấm sườn bì chả", Gia = 45000, HinhAnh = "🍛", DanhMuc = "Món chính", GianHangId = "gh1" },
        new MonAn { Id = "ma2", TenMon = "Cơm tấm sườn nướng", Gia = 40000, HinhAnh = "🍖", DanhMuc = "Món chính", GianHangId = "gh1" },
        new MonAn { Id = "ma3", TenMon = "Canh khổ qua", Gia = 15000, HinhAnh = "🥣", DanhMuc = "Món phụ", GianHangId = "gh1" },
        new MonAn { Id = "ma4", TenMon = "Trứng ốp la", Gia = 10000, HinhAnh = "🍳", DanhMuc = "Món phụ", GianHangId = "gh1" },
        new MonAn { Id = "ma5", TenMon = "Trà tắc", Gia = 12000, HinhAnh = "🥤", DanhMuc = "Nước uống", GianHangId = "gh1" },
    };

    public static ObservableCollection<DonHang> LichSuDonHang { get; } = new()
    {
        new DonHang
        {
            Id = "DH1029",
            GianHangId = "gh1",
            TenGianHang = "Cơm Tấm Sài Gòn",
            ChiTiet = new() { new MucGioHang { MonAn = DanhSachMonAn[0], SoLuong = 2 }, new MucGioHang { MonAn = DanhSachMonAn[4], SoLuong = 1 } },
            TrangThai = TrangThaiDonHang.DangGiao,
            DiaChiGiao = "123 Lý Thường Kiệt, Q.10",
            NgayDat = DateTime.Now.AddMinutes(-12),
        },
        new DonHang
        {
            Id = "DH1017",
            GianHangId = "gh2",
            TenGianHang = "Trà Sữa Mộng Mơ",
            ChiTiet = new() { new MucGioHang { MonAn = DanhSachMonAn[4], SoLuong = 3 } },
            TrangThai = TrangThaiDonHang.HoanThanh,
            DiaChiGiao = "123 Lý Thường Kiệt, Q.10",
            NgayDat = DateTime.Now.AddDays(-1).AddHours(-3),
        },
    };

    /// <summary>Tài khoản tài xế mẫu (đăng nhập demo cho luồng giao hàng).</summary>
    public static TaiXe TaiXeHienTai { get; } = new()
    {
        Id = "tx1",
        HoTen = "Nguyễn Văn Hải",
        Anh = "🛵",
        SoDienThoai = "0909 888 777",
        BienSoXe = "59-P1 123.45",
        DanhGia = 4.9,
        DangHoatDong = true,
        TongThuNhapHomNay = 0,
        SoDonHomNay = 0,
    };

    /// <summary>
    /// Đơn hàng đã nấu xong, đang chờ tài xế nhận (TaiXeId == null).
    /// Sau này thay bằng GET /api/tai-xe/don-cho-nhan khi có API thật.
    /// </summary>
    public static ObservableCollection<DonHang> DonHangChoTaiXe { get; } = new()
    {
        new DonHang
        {
            Id = "DH2001",
            GianHangId = "gh1",
            TenGianHang = "Cơm Tấm Sài Gòn",
            ChiTiet = new() { new MucGioHang { MonAn = DanhSachMonAn[0], SoLuong = 1 }, new MucGioHang { MonAn = DanhSachMonAn[2], SoLuong = 1 } },
            TrangThai = TrangThaiDonHang.DangLam,
            DiaChiLayHang = "12 Nguyễn Trãi, Q.5",
            DiaChiGiao = "56 Trần Hưng Đạo, Q.1",
            NgayDat = DateTime.Now.AddMinutes(-9),
            PhiGiaoHang = 18000,
        },
        new DonHang
        {
            Id = "DH2002",
            GianHangId = "gh2",
            TenGianHang = "Trà Sữa Mộng Mơ",
            ChiTiet = new() { new MucGioHang { MonAn = DanhSachMonAn[4], SoLuong = 2 } },
            TrangThai = TrangThaiDonHang.DangLam,
            DiaChiLayHang = "88 Lê Văn Việt, Q.9",
            DiaChiGiao = "20 Nguyễn Huệ, Q.1",
            NgayDat = DateTime.Now.AddMinutes(-4),
            PhiGiaoHang = 14000,
        },
        new DonHang
        {
            Id = "DH2003",
            GianHangId = "gh3",
            TenGianHang = "Bún Bò Huế Cô Ba",
            ChiTiet = new() { new MucGioHang { MonAn = DanhSachMonAn[1], SoLuong = 1 } },
            TrangThai = TrangThaiDonHang.DangLam,
            DiaChiLayHang = "45 Hoàng Diệu, Q.4",
            DiaChiGiao = "10 Cách Mạng Tháng 8, Q.3",
            NgayDat = DateTime.Now.AddMinutes(-2),
            PhiGiaoHang = 20000,
        },
    };

    /// <summary>Đơn tài xế hiện đang giao (chỉ 1 đơn tại 1 thời điểm), null nếu chưa nhận đơn nào.</summary>
    public static DonHang? DonHangDangGiaoCuaTaiXe { get; set; }

    /// <summary>Đơn tài xế đã giao xong trong ca hôm nay.</summary>
    public static ObservableCollection<DonHang> DonHangDaGiaoCuaTaiXe { get; } = new();

    // ====================== LUỒNG QUẢN LÝ GIAN HÀNG (ĐỐI TÁC) ======================

    /// <summary>Gian hàng đang đăng nhập (tài khoản đối tác demo — mặc định "Cơm Tấm Sài Gòn").</summary>
    public static GianHang GianHangHienTai { get; set; } = DanhSachGianHang.First(g => g.Id == "gh1");

    /// <summary>Lấy danh sách món ăn thuộc về một gian hàng.</summary>
    public static List<MonAn> MonAnCuaGianHang(string gianHangId) =>
        DanhSachMonAn.Where(m => m.GianHangId == gianHangId).ToList();

    /// <summary>
    /// Hàng đợi đơn hàng của gian hàng hiện tại — độc lập với luồng tài xế/khách hàng
    /// vì mỗi vai trò có góc nhìn khác nhau trên cùng một đơn (sau này thay bằng
    /// GET /api/gian-hang/don-hang khi có API thật, backend sẽ đồng bộ trạng thái chung).
    /// </summary>
    public static ObservableCollection<DonHang> DonHangCuaGianHang { get; } = new()
    {
        new DonHang
        {
            Id = "DH3001",
            GianHangId = "gh1",
            TenGianHang = "Cơm Tấm Sài Gòn",
            ChiTiet = new() { new MucGioHang { MonAn = DanhSachMonAn[0], SoLuong = 2 } },
            TrangThai = TrangThaiDonHang.ChoXacNhan,
            DiaChiLayHang = "12 Nguyễn Trãi, Q.5",
            DiaChiGiao = "77 Nguyễn Đình Chiểu, Q.3",
            NgayDat = DateTime.Now.AddMinutes(-3),
            PhiGiaoHang = 16000,
        },
        new DonHang
        {
            Id = "DH3002",
            GianHangId = "gh1",
            TenGianHang = "Cơm Tấm Sài Gòn",
            ChiTiet = new() { new MucGioHang { MonAn = DanhSachMonAn[1], SoLuong = 1 }, new MucGioHang { MonAn = DanhSachMonAn[3], SoLuong = 1 } },
            TrangThai = TrangThaiDonHang.ChoXacNhan,
            DiaChiLayHang = "12 Nguyễn Trãi, Q.5",
            DiaChiGiao = "205 Võ Văn Tần, Q.3",
            NgayDat = DateTime.Now.AddMinutes(-1),
            PhiGiaoHang = 15000,
        },
        new DonHang
        {
            Id = "DH2999",
            GianHangId = "gh1",
            TenGianHang = "Cơm Tấm Sài Gòn",
            ChiTiet = new() { new MucGioHang { MonAn = DanhSachMonAn[0], SoLuong = 1 }, new MucGioHang { MonAn = DanhSachMonAn[4], SoLuong = 1 } },
            TrangThai = TrangThaiDonHang.DangLam,
            DiaChiLayHang = "12 Nguyễn Trãi, Q.5",
            DiaChiGiao = "9 Cống Quỳnh, Q.1",
            NgayDat = DateTime.Now.AddMinutes(-15),
            PhiGiaoHang = 17000,
        },
        new DonHang
        {
            Id = "DH2950",
            GianHangId = "gh1",
            TenGianHang = "Cơm Tấm Sài Gòn",
            ChiTiet = new() { new MucGioHang { MonAn = DanhSachMonAn[0], SoLuong = 3 } },
            TrangThai = TrangThaiDonHang.HoanThanh,
            DiaChiLayHang = "12 Nguyễn Trãi, Q.5",
            DiaChiGiao = "31 Trần Phú, Q.5",
            NgayDat = DateTime.Now.AddHours(-2),
            PhiGiaoHang = 16000,
        },
        new DonHang
        {
            Id = "DH2941",
            GianHangId = "gh1",
            TenGianHang = "Cơm Tấm Sài Gòn",
            ChiTiet = new() { new MucGioHang { MonAn = DanhSachMonAn[1], SoLuong = 2 }, new MucGioHang { MonAn = DanhSachMonAn[2], SoLuong = 2 } },
            TrangThai = TrangThaiDonHang.HoanThanh,
            DiaChiLayHang = "12 Nguyễn Trãi, Q.5",
            DiaChiGiao = "150 An Dương Vương, Q.5",
            NgayDat = DateTime.Now.AddHours(-5),
            PhiGiaoHang = 18000,
        },
        new DonHang
        {
            Id = "DH2888",
            GianHangId = "gh1",
            TenGianHang = "Cơm Tấm Sài Gòn",
            ChiTiet = new() { new MucGioHang { MonAn = DanhSachMonAn[0], SoLuong = 1 } },
            TrangThai = TrangThaiDonHang.DaHuy,
            DiaChiLayHang = "12 Nguyễn Trãi, Q.5",
            DiaChiGiao = "60 Nguyễn Chí Thanh, Q.5",
            NgayDat = DateTime.Now.AddDays(-1).AddHours(-1),
            PhiGiaoHang = 15000,
        },
    };

    // ====================== LUỒNG QUẢN LÝ QUẢN TRỊ ======================

    /// <summary>Tài khoản quản trị viên demo (đăng nhập cổng quản trị).</summary>
    public static QuanTriVien QuanTriVienHienTai { get; } = new()
    {
        HoTen = "Quản trị hệ thống",
        Email = "admin@fooddelivery.vn",
        MatKhau = "admin123",
    };

    /// <summary>Danh sách tài khoản khách hàng — sau này thay bằng GET /api/quan-tri/khach-hang.</summary>
    public static ObservableCollection<KhachHang> DanhSachKhachHang { get; } = new()
    {
        new KhachHang { Id = "kh1", HoTen = "Trần Thị Bích", Email = "bich.tran@gmail.com", MatKhau = "123456", SoDienThoai = "0912 345 678", NgayDangKy = DateTime.Now.AddMonths(-6), TongSoDon = 24 },
        new KhachHang { Id = "kh2", HoTen = "Lê Văn Phong", Email = "phong.le@gmail.com", MatKhau = "123456", SoDienThoai = "0938 222 111", NgayDangKy = DateTime.Now.AddMonths(-3), TongSoDon = 9 },
        new KhachHang { Id = "kh3", HoTen = "Nguyễn Thị Hạnh", Email = "hanh.nguyen@gmail.com", MatKhau = "123456", SoDienThoai = "0977 456 890", NgayDangKy = DateTime.Now.AddDays(-20), TongSoDon = 3 },
        new KhachHang { Id = "kh4", HoTen = "Phạm Minh Đức", Email = "duc.pham@gmail.com", MatKhau = "123456", SoDienThoai = "0909 123 456", NgayDangKy = DateTime.Now.AddDays(-2), TongSoDon = 1 },
    };

    /// <summary>Danh sách vấn đề/khiếu nại phát sinh — sau này thay bằng GET /api/quan-tri/van-de.</summary>
    public static ObservableCollection<VanDeHeThong> DanhSachVanDe { get; } = new()
    {
        new VanDeHeThong
        {
            Id = "vd1", TieuDe = "Khách hàng khiếu nại đơn giao trễ 40 phút",
            MoTa = "Đơn #DH1029 dự kiến giao lúc 12:00 nhưng đến 12:40 vẫn chưa nhận được.",
            NguonGoc = "Cơm Tấm Sài Gòn", NgayBaoCao = DateTime.Now.AddHours(-3), TrangThai = TrangThaiVanDe.DangXuLy,
        },
        new VanDeHeThong
        {
            Id = "vd2", TieuDe = "Gian hàng báo lỗi không nhận được thông báo đơn mới",
            MoTa = "Đối tác phản ánh ứng dụng không hiện đơn mới trong 15 phút dù khách đã đặt thành công.",
            NguonGoc = "Trà Sữa Mộng Mơ", NgayBaoCao = DateTime.Now.AddHours(-8), TrangThai = TrangThaiVanDe.MoiPhatSinh,
        },
        new VanDeHeThong
        {
            Id = "vd3", TieuDe = "Yêu cầu hoàn tiền do món giao sai",
            MoTa = "Khách đặt bún bò nhưng nhận được cơm tấm. Đã liên hệ gian hàng xác nhận và hoàn tiền cho khách.",
            NguonGoc = "Bún Bò Huế Cô Ba", NgayBaoCao = DateTime.Now.AddDays(-1), TrangThai = TrangThaiVanDe.DaGiaiQuyet,
        },
    };

    /// <summary>
    /// Danh sách chương trình khuyến mãi — gồm cả mã do từng gian hàng tự tạo và mã
    /// do quản trị viên tạo áp dụng toàn hệ thống. Sau này thay bằng GET /api/khuyen-mai.
    /// </summary>
    public static ObservableCollection<KhuyenMai> DanhSachKhuyenMai { get; } = new()
    {
        new KhuyenMai
        {
            Id = "km1", MaCode = "GIAM20", TenChuongTrinh = "Chào hè giảm 20%",
            MoTa = "Giảm 20% cho mọi đơn hàng, tối đa 30.000đ.",
            LoaiGiam = LoaiGiamGia.PhanTram, GiaTriGiam = 20, GiamToiDa = 30000, GiaTriDonToiThieu = 50000,
            GianHangId = null, SoLuongToiDa = 500, SoLuongDaDung = 128,
            NgayBatDau = DateTime.Now.AddDays(-10), NgayKetThuc = DateTime.Now.AddDays(20), DangHoatDong = true,
        },
        new KhuyenMai
        {
            Id = "km2", MaCode = "FREESHIP15K", TenChuongTrinh = "Miễn phí giao hàng",
            MoTa = "Giảm thẳng 15.000đ áp dụng cho mọi đơn hàng trên toàn hệ thống.",
            LoaiGiam = LoaiGiamGia.SoTien, GiaTriGiam = 15000, GiaTriDonToiThieu = 0,
            GianHangId = null, SoLuongToiDa = 1000, SoLuongDaDung = 340,
            NgayBatDau = DateTime.Now.AddDays(-30), NgayKetThuc = DateTime.Now.AddDays(60), DangHoatDong = true,
        },
        new KhuyenMai
        {
            Id = "km3", MaCode = "COMTAM10K", TenChuongTrinh = "Ưu đãi khách quen Cơm Tấm",
            MoTa = "Giảm 10.000đ cho đơn từ 30.000đ tại Cơm Tấm Sài Gòn.",
            LoaiGiam = LoaiGiamGia.SoTien, GiaTriGiam = 10000, GiaTriDonToiThieu = 30000,
            GianHangId = "gh1", SoLuongToiDa = 200, SoLuongDaDung = 47,
            NgayBatDau = DateTime.Now.AddDays(-15), NgayKetThuc = DateTime.Now.AddDays(15), DangHoatDong = true,
        },
        new KhuyenMai
        {
            Id = "km4", MaCode = "TRASUA5K", TenChuongTrinh = "Giảm giá trà sữa",
            MoTa = "Giảm 5.000đ cho đơn từ 20.000đ tại Trà Sữa Mộng Mơ.",
            LoaiGiam = LoaiGiamGia.SoTien, GiaTriGiam = 5000, GiaTriDonToiThieu = 20000,
            GianHangId = "gh2", SoLuongToiDa = 150, SoLuongDaDung = 150,
            NgayBatDau = DateTime.Now.AddDays(-20), NgayKetThuc = DateTime.Now.AddDays(10), DangHoatDong = true,
        },
        new KhuyenMai
        {
            Id = "km5", MaCode = "TET2026", TenChuongTrinh = "Khuyến mãi Tết (đã kết thúc)",
            MoTa = "Chương trình giảm giá Tết Nguyên Đán đã kết thúc.",
            LoaiGiam = LoaiGiamGia.PhanTram, GiaTriGiam = 15, GiamToiDa = 25000, GiaTriDonToiThieu = 40000,
            GianHangId = null, SoLuongToiDa = 300, SoLuongDaDung = 300,
            NgayBatDau = DateTime.Now.AddDays(-60), NgayKetThuc = DateTime.Now.AddDays(-30), DangHoatDong = true,
        },
    };

    /// <summary>
    /// Kiểm tra và áp dụng một mã khuyến mãi cho đơn hàng — dùng chung cho trang thanh toán
    /// của khách hàng. Sau này thay bằng POST /api/khuyen-mai/ap-dung khi có API thật
    /// (backend sẽ tăng SoLuongDaDung khi đơn hàng đặt thành công, không phải ngay khi nhập mã).
    /// </summary>
    public static (bool ThanhCong, decimal SoTienGiam, string ThongDiep) ApDungMaKhuyenMai(
        string maCode, string gianHangId, decimal tamTinh)
    {
        var km = DanhSachKhuyenMai.FirstOrDefault(k => string.Equals(k.MaCode, maCode, StringComparison.OrdinalIgnoreCase));
        if (km is null) return (false, 0, "Mã khuyến mãi không tồn tại");
        if (!km.DangHoatDong) return (false, 0, "Mã khuyến mãi hiện đã ngừng hoạt động");
        if (km.ChuaBatDau) return (false, 0, "Mã khuyến mãi chưa đến ngày áp dụng");
        if (!km.ConHan) return (false, 0, "Mã khuyến mãi đã hết hạn sử dụng");
        if (!km.ConLuot) return (false, 0, "Mã khuyến mãi đã hết lượt sử dụng");
        if (!km.ApDungToanHeThong && km.GianHangId != gianHangId)
            return (false, 0, "Mã khuyến mãi không áp dụng cho gian hàng này");
        if (tamTinh < km.GiaTriDonToiThieu)
            return (false, 0, $"Đơn hàng cần tối thiểu {km.GiaTriDonToiThieu:N0}đ để dùng mã này");

        var giam = km.LoaiGiam == LoaiGiamGia.PhanTram
            ? Math.Round(tamTinh * km.GiaTriGiam / 100m)
            : km.GiaTriGiam;
        if (km.LoaiGiam == LoaiGiamGia.PhanTram && km.GiamToiDa is decimal tran && giam > tran)
            giam = tran;
        if (giam > tamTinh) giam = tamTinh;

        return (true, giam, $"Áp dụng thành công: giảm {giam:N0}đ");
    }

    /// <summary>Gộp toàn bộ đơn hàng từ mọi nguồn mock hiện có — dùng cho thống kê & báo cáo toàn hệ thống.</summary>
    public static IEnumerable<DonHang> TatCaDonHangHeThong =>
        LichSuDonHang.Concat(DonHangChoTaiXe).Concat(DonHangCuaGianHang);

    /// <summary>
    /// Ước tính doanh thu (đơn đã hoàn thành) của một gian hàng, gộp từ mọi nguồn dữ liệu mock hiện có
    /// — sau này thay bằng GET /api/quan-tri/doanh-thu?gianHangId=... khi có API + CSDL thật.
    /// </summary>
    public static decimal DoanhThuUocTinh(string gianHangId)
    {
        return TatCaDonHangHeThong
            .Where(d => d.GianHangId == gianHangId && d.TrangThai == TrangThaiDonHang.HoanThanh)
            .Sum(d => d.TongTien);
    }
}
