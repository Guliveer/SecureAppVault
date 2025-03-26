using System;
using System.Globalization;
using System.Windows.Data;

namespace SecureAppVault.Converters
{
    public class BoolToLockContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Lock" : "Unlock";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}