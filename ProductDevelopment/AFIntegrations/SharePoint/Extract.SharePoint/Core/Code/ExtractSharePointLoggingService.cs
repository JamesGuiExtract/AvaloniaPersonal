using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;

namespace Extract.SharePoint
{
    /// <summary>
    /// Error category enum for indicating where the error message is logged from.
    /// </summary>
    public enum ErrorCategoyId
    {
        None = 0,
        Feature = 1,
        CustomAction = 2,
        WebPart = 3,
        IdShieldFileReceiver = 4
    }

    /// <summary>
    /// Helper class for logging exceptions to the SharePoint log.
    /// </summary>
    [System.Runtime.InteropServices.GuidAttribute("0E620A5B-B7F6-49CC-94C2-C2AFE8317A44")]
    public class ExtractSharePointLoggingService : SPDiagnosticsServiceBase
    {
        /// <summary>
        /// The extract logging area (should help sort logs for Extract Systems elements).
        /// </summary>
        static readonly string _EXTRACT_DIAGNOSTIC_AREA = "Extract Systems";

        static ExtractSharePointLoggingService _current;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractSharePointLoggingService"/>
        /// class.
        /// </summary>
        private ExtractSharePointLoggingService()
            : base("Extract Systems Logging Service", SPFarm.Local)
        {
        }

        #region Methods

        /// <summary>
        /// Returns the list of <see cref="SPDiagnosticsArea"/> objects for logging.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<SPDiagnosticsArea> ProvideAreas()
        {
            List<SPDiagnosticsCategory> categories = new List<SPDiagnosticsCategory>();
            foreach(string catName in Enum.GetNames(typeof(ErrorCategoyId)))
            {
                uint catId = (uint)(int)Enum.Parse(typeof(ErrorCategoyId), catName);
                categories.Add(new SPDiagnosticsCategory(catName, TraceSeverity.Unexpected,
                    EventSeverity.Error, 0, catId));
            }

            yield return new SPDiagnosticsArea(_EXTRACT_DIAGNOSTIC_AREA, categories);
        }

        /// <summary>
        /// Helper method to easily log an error.
        /// </summary>
        /// <param name="categoryId">The category for the error.</param>
        /// <param name="errorMessage">The text for the message to log.</param>
        public static void LogError(ErrorCategoyId categoryId, string errorMessage)
        {
            SPDiagnosticsCategory category = ExtractSharePointLoggingService.Current[categoryId];
            ExtractSharePointLoggingService.Current.WriteTrace(0, category, TraceSeverity.Unexpected,
                errorMessage);
        }

        /// <summary>
        /// Helper method to easily log an exception.
        /// </summary>
        /// <param name="categoryId">The category for the exception to log.</param>
        /// <param name="ex">The exception to log.</param>
        public static void LogError(ErrorCategoyId categoryId, Exception ex)
        {
            SPDiagnosticsCategory category = ExtractSharePointLoggingService.Current[categoryId];
            ExtractSharePointLoggingService.Current.WriteTrace(0,
                category, TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Helper property to return an instance of the logging service.
        /// </summary>
        public static ExtractSharePointLoggingService Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new ExtractSharePointLoggingService();
                }

                return _current;
            }
        }

        /// <summary>
        /// Gets a specified category for the specified error category.
        /// </summary>
        /// <param name="id">The category to get the diagnostic category for.</param>
        /// <returns>The specified diagnostic category.</returns>
        public SPDiagnosticsCategory this[ErrorCategoyId id]
        {
            get
            {
                return Areas[_EXTRACT_DIAGNOSTIC_AREA].Categories[id.ToString()];
            }
        }

        #endregion Properties
    }
}
