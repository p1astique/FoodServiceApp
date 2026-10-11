namespace FoodServiceApp.Web.Models.Dtos
{
    public record LoginRequest(string TenDangNhap, string MatKhau);
    public record LoginResponse(string Token, string VaiTro, int MaTK, string HoTenHienThi);

    public record MonAnDto(int MaMonAn, string TenMon, decimal Gia, string? MoTa, string? HinhAnh, bool ConHang);

    public record ChiTietDonHangDto(string TenMon, int SoLuong, decimal DonGia, decimal ThanhTien);

    public record DonHangDto(
        int MaDonHang, string TrangThai, decimal TongTien, DateTime NgayDat,
        string DiaChiGiaoHang, string? GhiChu,
        string TenGianHang, string DiaChiGianHang,
        List<ChiTietDonHangDto> ChiTiet,
        decimal PhiGiaoHang = 0);

    public record VendorHomeDto(
        string TenCuaHang, string TrangThaiKinhDoanh,
        decimal DoanhThuHomNay, int SoDonHomNay, int SoDonChoXacNhan, int SoMonAn);

    public record AdminHomeDto(
        int TongKhachHang, int TongGianHang, int GianHangDangMo,
        int TongDonHomNay, decimal DoanhThuHomNay);

    public record VendorListItemDto(int MaGianHang, string TenCuaHang, string DiaChi, DateTime NgayDangKy, string TrangThaiKinhDoanh, bool TaiKhoanHoatDong);

    public record AdminVendorDto(
        int MaGianHang, int MaTK, string TenCuaHang, string DiaChi, string SDT,
        string Email, string? MoTa, string? HinhAnh, DateTime NgayDangKy,
        string TrangThaiDuyet, string TrangThaiKinhDoanh, bool TaiKhoanHoatDong);

    public record CustomerListItemDto(int MaKH, string HoTen, string SDT, string? Email, bool TaiKhoanHoatDong);

    public record DriverHomeDto(
        string HoTen, string TrangThai, int SoDonHomNay, decimal ThuNhapHomNay, DonHangDto? DonDangGiao, int SoDonChoNhan);

    public record UpdateOrderStatusRequest(string TrangThaiMoi);
}
