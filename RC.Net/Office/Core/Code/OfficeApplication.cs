using System;
using System.Collections.Generic;
using System.Text;

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
}
