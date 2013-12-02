using Microsoft.Win32;
using System.Globalization;

namespace Extract.UtilityApplications.Services
{
    /// <summary>
    /// Manages registry settings for the ESAppMonitorService project. 
    /// </summary>
    internal static class RegistryManager
    {
        #region Constants

        /// <summary>
        /// The registry key for AppMonitor registry values.
        /// </summary>
        const string _EXTRACT_APPMONITOR_SUBKEY = @"Software\Extract Systems\ESAppMonitorService";

        /// <summary>
        /// The registry value specifying the polling frequency.
        /// </summary>
        const string _POLLING_FREQUENCY = "PollingFrequency";

        /// <summary>
        /// The default polling frequency in seconds.
        /// </summary>
        const double _DEFAULT_POLLING_FREQUENCY = 5;

        /// <summary>
        /// The registry value specifying the number of seconds an OCR process can remain idle
        /// before it should be force-killed.
        /// </summary>
        const string _OCR_PROCESS_TIMEOUT = "OCRProcessTimeout";

        /// <summary>
        /// The default number of seconds an OCR process can remain idle before it should be
        /// force-killed.
        /// </summary>
        const double _DEFAULT_OCR_PROCESS_TIMEOUT = 300;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The registry key to use for the app monitor service settings.
        /// </summary>     
        static readonly RegistryKey _appMonitorRegistryKey =
            Registry.LocalMachine.OpenSubKey(_EXTRACT_APPMONITOR_SUBKEY);

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the frequency at which applications should be polled.
        /// </summary>
        /// <returns>The interval, in seconds, at which polling should take place.</returns>
        public static double PollingFrequency
        {
            get 
            {
                double pollingFrequency = _DEFAULT_POLLING_FREQUENCY;

                if (_appMonitorRegistryKey != null)
                {
                    string registryValue =
                        _appMonitorRegistryKey.GetValue(_POLLING_FREQUENCY) as string;
                    if (!string.IsNullOrWhiteSpace(registryValue))
                    {
                        if (!double.TryParse(registryValue, NumberStyles.Number,
                            CultureInfo.CurrentCulture, out pollingFrequency))
                        {
                            pollingFrequency = _DEFAULT_POLLING_FREQUENCY;
                        }
                    }
                }

                return pollingFrequency;
            }
        }

        /// <summary>
        /// Gets the number of seconds an SSOCR2 process should be allowed to remain idle before
        /// being force-killed.
        /// </summary>
        /// <returns>The number of seconds an SSOCR2 process should be allowed to remain idle before
        /// being force-killed.</returns>
        public static double OcrProcessTimeout
        {
            get
            {
                double ocrProcessTimeout = _DEFAULT_OCR_PROCESS_TIMEOUT;

                if (_appMonitorRegistryKey != null)
                {
                    string registryValue =
                        _appMonitorRegistryKey.GetValue(_OCR_PROCESS_TIMEOUT) as string;
                    if (!string.IsNullOrWhiteSpace(registryValue))
                    {
                        if (!double.TryParse(registryValue, NumberStyles.Number,
                            CultureInfo.CurrentCulture, out ocrProcessTimeout))
                        {
                            ocrProcessTimeout = _DEFAULT_OCR_PROCESS_TIMEOUT;
                        }
                    }
                }

                return ocrProcessTimeout;
            }
        }

        #endregion Properties
    }
}
