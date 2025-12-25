using System.Globalization;
using System.Windows.Data;
using BillPaymentManager.Models;

namespace BillPaymentManager.Converters;

/// <summary>
/// Converts PaymentProvider enum to display string
/// </summary>
public class ProviderToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return "All Providers";
        
        if (value is PaymentProvider provider)
        {
            return provider.ToString();
        }
        
        return value.ToString() ?? "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
