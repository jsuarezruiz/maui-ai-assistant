using System.Globalization;
using System.Windows.Data;

namespace MAUI_AI_Assistant
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
