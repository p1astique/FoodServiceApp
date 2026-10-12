using System.Net.Http.Json;
using System.Net;

namespace FoodServiceApp.Maui.Services;

public class AdminVendorDto
{
    public int MaGianHang { get; set; }
    public int MaTK { get; set; }
    public string TenCuaHang { get; set; } = "";
    public string DiaChi { get; set; } = "";
    public string SDT { get; set; } = "";
    public string Email { get; set; } = "";
    public string? MoTa { get; set; }
    public string? HinhAnh { get; set; }
    public DateTime NgayDangKy { get; set; }
    public string TrangThaiDuyet { get; set; } = "";
    public string TrangThaiKinhDoanh { get; set; } = "";
    public bool TaiKhoanHoatDong { get; set; }
}

public class VendorRegistrationApiClient
{
    public Task<List<AdminVendorDto>> GetPendingAsync() => ApiClient.GetAsync<List<AdminVendorDto>>("api/admin/vendors/pending", "QuanTri");
    public Task<List<AdminVendorDto>> GetApprovedAsync() => ApiClient.GetAsync<List<AdminVendorDto>>("api/admin/vendors/by-status?status=DaDuyet", "QuanTri");
    private static async Task<bool> Post(string path)
    {
        await ApiClient.SendAsync(HttpMethod.Post, path, null, "QuanTri");
        return true;
    }
    public Task<bool> ApproveAsync(int id) => Post($"api/admin/vendors/{id}/approve");
    public Task<bool> RejectAsync(int id) => Post($"api/admin/vendors/{id}/reject");
    public Task<bool> ToggleLockAsync(int id) => Post($"api/admin/vendors/{id}/toggle-lock");
}
