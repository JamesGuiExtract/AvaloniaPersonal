using Extract;
using System;
using System.Windows.Data;

namespace DatabaseMigrationWizard.Pages.Utility
{
    public class BooleanAndConverter : IMultiValueConverter
    {
        /// <summary>
        /// Does an "AND" comparision between two booleans in a WPF UI
        /// </summary>
        /// <param name="values">The values to compare.</param>
        /// <param name="targetType">Used in the WPF definition for some reason. Unused here</param>
        /// <param name="parameter">Used in the WPF definition for some reason. Unused here</param>
        /// <param name="culture">Used in the WPF definition for some reason. Unused here</param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                foreach (object value in values)
                {
                    if ((value is bool) && (bool)value == false)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI49781");
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
        }
    }
}
