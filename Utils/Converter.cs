
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Symbolic_Algebra_Solver.Converter
{
    public class BooleanVisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool) value;
            boolValue = (parameter != null) ? !boolValue : boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
