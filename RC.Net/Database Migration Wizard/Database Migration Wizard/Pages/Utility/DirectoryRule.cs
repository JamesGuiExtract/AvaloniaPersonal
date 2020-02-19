using Extract;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace DatabaseMigrationWizard.Pages
{
    public class DirectoryRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                string directory = value.ToString();

                if (!Directory.Exists(directory))
                {
                    return new ValidationResult(false, "The directory does not exist, or you do not have permissions to it!");
                }

                return ValidationResult.ValidResult;
            }
            catch(Exception e)
            {
                throw new ExtractException("ELI49678", "Pass a valid directory.", e);
            }
        }
    }
}
