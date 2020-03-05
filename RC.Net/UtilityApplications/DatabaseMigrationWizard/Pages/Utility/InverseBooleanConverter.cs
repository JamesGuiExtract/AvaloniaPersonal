using System;
using System.Windows.Data;

namespace DatabaseMigrationWizard.Pages.Utility
{
    /// <summary>
    /// Used in the UI to negate booleans (for some reason you cant use ! in the UI).
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !bool.Parse(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !bool.Parse(value.ToString());
        }
    }
}
