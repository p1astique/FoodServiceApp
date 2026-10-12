using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FoodServiceApp.Maui.Services;

public static class ApiClient
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(20) };
    public static string BaseUrl
    {
        get => Preferences.Default.Get("api-base-url", DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5000/" : "https://localhost:5001/");
        set
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http"))
                throw new InvalidOperationException("Địa chỉ API phải bắt đầu bằng https:// hoặc http://.");
            if (!string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
                throw new InvalidOperationException("Nhập địa chỉ máy chủ không chứa tài khoản, query hoặc fragment.");
            Logout("GianHang"); Logout("QuanTri"); Logout("KhachHang"); VendorWorkspace.Clear();
            Preferences.Default.Set("api-base-url", value.TrimEnd('/') + "/");
        }
    }
    public static async Task LoginAsync(string role, string username, string password)
    {
        Logout(role);
        var result = await SendAsync<LoginResult>(HttpMethod.Post, $"api/auth/login/{role}",
            new { TenDangNhap = username.Trim(), MatKhau = password });
        await SecureStorage.Default.SetAsync($"api-token-{role}", result.Token);
    }
    public static void Logout(string role) => SecureStorage.Default.Remove($"api-token-{role}");
    public static string? ImageUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (path.StartsWith("/uploads/")) return new Uri(new Uri(BaseUrl), path).ToString();
        return Uri.TryCreate(path, UriKind.Absolute, out var url) && url.Scheme is "https" or "http" ? path : null;
    }
    public static Task<T> GetAsync<T>(string path, string? role = "GianHang") => SendAsync<T>(HttpMethod.Get, path, null, role);
    public static async Task SendAsync(HttpMethod method, string path, object? body = null, string? role = "GianHang")
    {
        using var response = await RequestAsync(method, path, body, role);
    }
    public static async Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null, string? role = null)
    {
        using var response = await RequestAsync(method, path, body, role);
        return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException("Server trả dữ liệu rỗng.");
    }
    private static async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string path, object? body, string? role)
    {
        using var request = new HttpRequestMessage(method, new Uri(new Uri(BaseUrl), path));
        if (role != null)
        {
            var token = await SecureStorage.Default.GetAsync($"api-token-{role}");
            if (string.IsNullOrEmpty(token)) throw new InvalidOperationException("Vui lòng đăng nhập lại.");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        if (body != null) request.Content = JsonContent.Create(body);
        HttpResponseMessage response;
        try { response = await Http.SendAsync(request); }
        catch (HttpRequestException) { throw new InvalidOperationException("Không kết nối được API. Kiểm tra địa chỉ server, mạng và chứng chỉ HTTPS."); }
        catch (TaskCanceledException) { throw new InvalidOperationException("Server phản hồi quá lâu. Hãy thử lại."); }
        if (response.IsSuccessStatusCode) return response;
        var message = $"Yêu cầu thất bại ({(int)response.StatusCode}).";
        if (response.StatusCode == HttpStatusCode.Unauthorized && role != null)
        {
            Logout(role); message = "Phiên đăng nhập hết hạn hoặc tài khoản đã khóa. Vui lòng đăng nhập lại.";
        }
        try
        {
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (json.RootElement.TryGetProperty("message", out var text)) message = text.GetString() ?? message;
            else if (json.RootElement.TryGetProperty("errors", out var errors))
                message = string.Join("\n", errors.EnumerateObject().SelectMany(x => x.Value.EnumerateArray()).Select(x => x.GetString()));
        }
        catch (JsonException) { }
        response.Dispose(); throw new InvalidOperationException(message);
    }
    private record LoginResult(string Token);
}
