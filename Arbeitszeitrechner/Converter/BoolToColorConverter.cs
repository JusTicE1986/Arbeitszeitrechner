using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Arbeitszeitrechner.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool istImSoll)
            {
                return istImSoll ? Brushes.LightGreen : Brushes.LightCoral;
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
