using System;
using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    public class ConnectionButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Conectado" : "Conectar";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}