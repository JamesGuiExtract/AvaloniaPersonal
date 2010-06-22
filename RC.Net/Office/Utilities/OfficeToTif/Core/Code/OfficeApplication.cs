using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Office.Utilities.OfficeToTif
{
    /// <summary>
    /// Enumeration to list what MS Office application to target.
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
        PowerPoint = 3
    }
}
