using System.Text.Json;
using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Services;

/// <summary>Lưu trạng thái demo của khu vực gian hàng trên thiết bị hiện tại.</summary>
public static class VendorLocalStore
{
    private const string Key = "vendor-workspace-v1";

    private sealed class Snapshot
    {
        public List<GianHang> GianHangs { get; set; } = new();
        public List<MonAn> MonAns { get; set; } = new();
        public Dictionary<string, List<string>> DanhMucs { get; set; } = new();
        public List<DonHang> DonHangs { get; set; } = new();
    }

    public static void Nap()
    {
        var json = Preferences.Default.Get(Key, string.Empty);
        if (string.IsNullOrWhiteSpace(json)) return;
        try
        {
            var state = JsonSerializer.Deserialize<Snapshot>(json);
            if (state is null) return;
            Copy(state.GianHangs, MockData.DanhSachGianHang, x => x.Id, (from, to) =>
            {
                to.TenGianHang = from.TenGianHang; to.DiaChi = from.DiaChi; to.HinhAnh = from.HinhAnh;
                to.SoDienThoai = from.SoDienThoai; to.Email = from.Email; to.MoTa = from.MoTa;
                to.ThoiGianGiaoPhut = from.ThoiGianGiaoPhut; to.DangMoCua = from.DangMoCua;
            });
            MockData.DanhSachMonAn.RemoveAll(m => state.MonAns.Any(s => s.GianHangId == m.GianHangId));
            MockData.DanhSachMonAn.AddRange(state.MonAns);
            foreach (var item in state.DanhMucs) MockData.DanhMucTheoGianHang[item.Key] = item.Value;
            foreach (var don in state.DonHangs)
            {
                var current = MockData.DonHangCuaGianHang.FirstOrDefault(d => d.Id == don.Id);
                if (current is not null) current.TrangThai = don.TrangThai;
            }
        }
        catch (JsonException) { Preferences.Default.Remove(Key); }
    }

    public static void Luu()
    {
        var state = new Snapshot
        {
            GianHangs = MockData.DanhSachGianHang,
            MonAns = MockData.DanhSachMonAn,
            DanhMucs = MockData.DanhMucTheoGianHang,
            DonHangs = MockData.DonHangCuaGianHang.ToList()
        };
        Preferences.Default.Set(Key, JsonSerializer.Serialize(state));
    }

    private static void Copy<T>(IEnumerable<T> source, IEnumerable<T> target, Func<T, string> key, Action<T, T> copy)
    {
        var byKey = target.ToDictionary(key);
        foreach (var item in source)
            if (byKey.TryGetValue(key(item), out var existing)) copy(item, existing);
    }
}
