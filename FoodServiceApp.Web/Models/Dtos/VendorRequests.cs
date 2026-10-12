using System.ComponentModel.DataAnnotations;

namespace FoodServiceApp.Web.Models.Dtos;

public class VendorProfileRequest
{
    [Required, StringLength(150)] public string TenCuaHang { get; set; } = "";
    [Required, StringLength(255)] public string DiaChi { get; set; } = "";
    [Required, StringLength(15)] public string SDT { get; set; } = "";
    [StringLength(500)] public string? MoTa { get; set; }
    [StringLength(255)] public string? HinhAnh { get; set; }
    public bool DangMoCua { get; set; }
}
public class VendorRegisterRequest : VendorProfileRequest
{
    [Required, StringLength(50)] public string TenDangNhap { get; set; } = "";
    [Required, StringLength(72, MinimumLength = 6)] public string MatKhau { get; set; } = "";
}
public class CategoryRequest
{
    [Required, StringLength(100)] public string TenDanhMuc { get; set; } = "";
    [StringLength(255)] public string? MoTa { get; set; }
    [Range(0, 999)] public int ThuTuHienThi { get; set; }
    public bool TrangThai { get; set; } = true;
}

public class StoreSettingsRequest : IValidatableObject
{
    public TimeSpan GioMoCua { get; set; } = new(7, 0, 0);
    public TimeSpan GioDongCua { get; set; } = new(22, 0, 0);
    public bool TuDongDongMo { get; set; }
    public bool TamDungNhanDon { get; set; }
    [StringLength(255)] public string? LyDoTamDung { get; set; }
    [StringLength(300)] public string? ThongBaoCuaHang { get; set; }
    [Range(5, 180)] public int ThoiGianChuanBiPhut { get; set; } = 20;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (GioDongCua == GioMoCua) yield return new("Giờ đóng cửa phải khác giờ mở cửa.");
        if (TamDungNhanDon && string.IsNullOrWhiteSpace(LyDoTamDung))
            yield return new("Vui lòng nhập lý do tạm dừng nhận đơn.", [nameof(LyDoTamDung)]);
    }
}

public class AdvertisingCampaignRequest : IValidatableObject
{
    [Required, StringLength(150)] public string TenChienDich { get; set; } = "";
    [Required, RegularExpression("Banner|NoiBat|GoiY")] public string LoaiViTri { get; set; } = "NoiBat";
    [Required, StringLength(150)] public string TieuDe { get; set; } = "";
    [StringLength(500)] public string? NoiDung { get; set; }
    [StringLength(500)] public string? HinhAnh { get; set; }
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    [Range(typeof(decimal), "0", "1000000000")] public decimal NganSach { get; set; }
    public bool KichHoat { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (NgayKetThuc <= NgayBatDau) yield return new("Ngày kết thúc phải sau ngày bắt đầu.");
    }
}
public class MenuItemRequest
{
    [Range(1, int.MaxValue)] public int MaDanhMuc { get; set; }
    [Required, StringLength(150)] public string TenMon { get; set; } = "";
    [Range(typeof(decimal), "1", "100000000")] public decimal Gia { get; set; }
    [StringLength(500)] public string? MoTa { get; set; }
    [StringLength(255)] public string? HinhAnh { get; set; }
    [StringLength(255)] public string? KichCo { get; set; }
    [StringLength(500)] public string? Topping { get; set; }
    [StringLength(255)] public string? MucCay { get; set; }
    public bool ConHang { get; set; } = true;
}
public class PromotionRequest : IValidatableObject
{
    [Required, StringLength(30), RegularExpression(@"[A-Za-z0-9_-]+", ErrorMessage = "Mã chỉ gồm chữ, số, gạch ngang và gạch dưới.")] public string MaCode { get; set; } = "";
    [StringLength(255)] public string? MoTa { get; set; }
    public int? MaMonAn { get; set; }
    public int? MaDanhMuc { get; set; }
    public bool LaPhanTram { get; set; }
    [Range(typeof(decimal), "0", "100000000")] public decimal GiaTriGiam { get; set; }
    [Range(typeof(decimal), "0", "100000000")] public decimal GiaTriDonToiThieu { get; set; }
    [Range(1, int.MaxValue)] public int? SoLuongToiDa { get; set; }
    public bool MoiKhachMotLan { get; set; }
    public bool GiamPhiVanChuyen { get; set; }
    [Range(typeof(decimal), "0", "100000000")] public decimal? GiamPhiVanChuyenToiDa { get; set; }
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public bool TrangThai { get; set; } = true;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (LaPhanTram && GiaTriGiam > 100) yield return new("Phần trăm giảm không vượt quá 100.");
        if (!GiamPhiVanChuyen && GiaTriGiam <= 0) yield return new("Giá trị giảm phải lớn hơn 0.");
        if (MaMonAn != null && MaDanhMuc != null) yield return new("Chỉ chọn món hoặc danh mục áp dụng.");
        if (NgayKetThuc <= NgayBatDau) yield return new("Ngày kết thúc phải sau ngày bắt đầu.");
    }
}
public record AvailabilityRequest(bool ConHang);
public record PromotionStatusRequest(bool TrangThai);
