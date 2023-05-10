using System;
using System.Globalization;
using System.Windows.Data;

namespace ExtractDataExplorer.Views
{
    public sealed class TruncateSecondStringConverter : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
            {
                return "";
            }

            string primary = values[0]?.ToString() ?? "";
            string extension = values[1]?.ToString() ?? "";

            int availableExtensionLength = Math.Max(0, 85 - primary.Length);
            if (availableExtensionLength <= 3)
            {
                return "...";
            }

            string singleLine = extension.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
            if (singleLine.Length > availableExtensionLength)
            {
                return singleLine.Substring(0, availableExtensionLength - 3) + "...";
            }

            return singleLine;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
