using System;
using System.Windows.Data;

namespace Extract.Utilities.WPF
{
    public class BooleanToWaitCursorConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is bool b && b
                ? System.Windows.Input.Cursors.Wait
                : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
