using Extract;
using System;
using System.Windows.Data;

namespace DatabaseMigrationWizard.Pages.Utility
{
    public class BooleanAndConverter : IMultiValueConverter
    {
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
