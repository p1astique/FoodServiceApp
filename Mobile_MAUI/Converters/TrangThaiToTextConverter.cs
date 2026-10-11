using System.Globalization;
using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Converters;

public class TrangThaiToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TrangThaiDonHang t ? t.Nhan() : "";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
