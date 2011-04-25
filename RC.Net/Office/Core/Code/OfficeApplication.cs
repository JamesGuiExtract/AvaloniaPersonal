using System;
using System.Globalization;

namespace Extract.Office
{
    /// <summary>
    /// Enumeration of MS Office applications
    /// </summary>
    public enum OfficeApplication
    {
        /// <summary>
        /// Unknown office application.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// MS Word.
        /// </summary>
        Word = 1,

        /// <summary>
        /// MS Excel.
        /// </summary>
        Excel = 2,

        /// <summary>
        /// MS PowerPoint.
        /// </summary>
        PowerPoint = 3,

        /// <summary>
        /// MS Access
        /// </summary>
        Access = 4,

        /// <summary>
        /// MS Outlook
        /// </summary>
        Outlook = 5,

        /// <summary>
        /// MS Publisher
        /// </summary>
        Publisher = 6,

        /// <summary>
        /// MS OneNote
        /// </summary>
        OneNote = 7
    }

    /// <summary>
    /// Extension methods for working with the OfficeApplication enum.
    /// </summary>
    public static class OfficeApplicationExtensionMethods
    {
        /// <summary>
        /// Converts the enum value into a string representation.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The string form of the enumbertion value.</returns>
        public static string AsString(this OfficeApplication value)
        {
            try
            {
                if (Enum.IsDefined(typeof(OfficeApplication), value))
                {
                    return Enum.GetName(typeof(OfficeApplication), value);
                }
                else
                {
                    return "Unknown: " + ((int)value).ToString(CultureInfo.CurrentCulture);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32439");
            }
        }
    }
}
