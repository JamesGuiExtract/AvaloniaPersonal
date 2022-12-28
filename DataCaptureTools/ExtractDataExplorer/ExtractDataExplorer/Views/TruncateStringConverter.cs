using System;
using System.Globalization;
using System.Windows.Data;

namespace ExtractDataExplorer.Views
{
    public sealed class TruncateStringConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string singleLine = (value?.ToString() ?? "").Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
            if (singleLine.Length > 100)
            {
                return singleLine.Substring(0, 97) + "...";
            }

            return singleLine;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
