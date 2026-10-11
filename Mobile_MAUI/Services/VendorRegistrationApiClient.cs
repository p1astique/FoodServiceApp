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
    private readonly HttpClient _httpClient;

    public VendorRegistrationApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:5001/"),
            DefaultRequestVersion = HttpVersion.Version11,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
        };
    }

    public async Task<List<AdminVendorDto>> GetPendingAsync() =>
        await _httpClient.GetFromJsonAsync<List<AdminVendorDto>>("api/admin/vendors/pending") ?? [];

    public async Task<List<AdminVendorDto>> GetApprovedAsync() =>
        await _httpClient.GetFromJsonAsync<List<AdminVendorDto>>("api/admin/vendors/by-status?status=DaDuyet") ?? [];

    public async Task<bool> ApproveAsync(int id) =>
        (await _httpClient.PostAsync($"api/admin/vendors/{id}/approve", null)).IsSuccessStatusCode;

    public async Task<bool> RejectAsync(int id) =>
        (await _httpClient.PostAsync($"api/admin/vendors/{id}/reject", null)).IsSuccessStatusCode;

    public async Task<bool> ToggleLockAsync(int id) =>
        (await _httpClient.PostAsync($"api/admin/vendors/{id}/toggle-lock", null)).IsSuccessStatusCode;
}
