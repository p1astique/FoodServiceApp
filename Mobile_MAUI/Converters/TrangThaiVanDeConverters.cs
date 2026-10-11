using System.Globalization;
using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Converters;

public class TrangThaiVanDeToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TrangThaiVanDe t ? t.Nhan() : "";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>True khi vấn đề CHƯA được đánh dấu "Đã giải quyết" — dùng để hiện nút hành động.</summary>
public class VanDeChuaGiaiQuyetConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TrangThaiVanDe t && t != TrangThaiVanDe.DaGiaiQuyet;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
