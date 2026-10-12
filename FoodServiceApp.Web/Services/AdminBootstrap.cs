using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceApp.Web.Services;

/// <summary>Interactive local setup for a database that has no administrator yet.</summary>
public static class AdminBootstrap
{
    public static async Task RunAsync(AppDbContext db)
    {
        if (Console.IsInputRedirected) throw new InvalidOperationException("Chạy thiết lập từ terminal tương tác.");
        if (await db.TaiKhoans.AnyAsync(x => x.VaiTro == "QuanTri"))
            throw new InvalidOperationException("Đã có tài khoản quản trị. Dùng tài khoản hiện có.");
        Console.Write("Tên đăng nhập quản trị (1–50 ký tự): ");
        var name = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 50 || await db.TaiKhoans.AnyAsync(x => x.TenDangNhap == name))
            throw new InvalidOperationException("Tên đăng nhập không hợp lệ hoặc đã tồn tại.");
        Console.Write("Mật khẩu (8–72 ký tự): "); var password = ReadPassword();
        Console.Write("Nhập lại mật khẩu: "); var confirmation = ReadPassword();
        if (password.Length is < 8 or > 72 || password != confirmation)
            throw new InvalidOperationException("Mật khẩu không hợp lệ hoặc không khớp.");
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        if (await db.TaiKhoans.AnyAsync(x => x.VaiTro == "QuanTri" || x.TenDangNhap == name))
            throw new InvalidOperationException("Tài khoản đã được tạo trong lúc thiết lập.");
        var account = new TaiKhoan { TenDangNhap = name, MatKhau = MatKhauHelper.Hash(password), VaiTro = "QuanTri" };
        db.TaiKhoans.Add(account); await db.SaveChangesAsync();
        db.QuanTris.Add(new QuanTri { MaTK = account.MaTK, HoTen = "Quản trị viên", SDT = "", CapQuyenHan = "SuperAdmin" });
        await db.SaveChangesAsync(); await tx.CommitAsync();
        Console.WriteLine("Đã tạo quản trị viên. Mật khẩu được lưu dạng BCrypt.");
    }
    private static string ReadPassword()
    {
        var value = new System.Text.StringBuilder();
        while (true) {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return value.ToString(); }
            if (key.Key == ConsoleKey.Backspace && value.Length > 0) value.Length--;
            else if (!char.IsControl(key.KeyChar) && value.Length < 73) value.Append(key.KeyChar);
        }
    }
}
