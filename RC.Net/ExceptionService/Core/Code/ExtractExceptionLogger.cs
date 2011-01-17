using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;

namespace Extract.ExceptionService
{
    class ExtractExceptionLogger : IExtractExceptionLogger
    {
        #region Fields

#if DEBUG
        /// <summary>
        /// Path to the exception helper application.
        /// </summary>
        static readonly string _EXCEPTION_HELPER_APP = @"D:\Engineering\Binaries\Debug\ExceptionHelper.exe";
#else
        /// <summary>
        /// Path to the Extract systems folder in the program files directory.
        /// </summary>
        static readonly string _EXTRACT_FOLDER = Path.Combine(
            Environment.GetEnvironmentVariable("ProgramFiles(x86)") ??
            Environment.GetEnvironmentVariable("ProgramFiles"), "Extract Systems");

        /// <summary>
        /// Path to the Extract common components folder.
        /// </summary>
        static readonly string _COMMON_COMPONENTS =
            Path.Combine(_EXTRACT_FOLDER, "CommonComponents");

        /// <summary>
        /// Path to the exception helper application.
        /// </summary>
        static readonly string _EXCEPTION_HELPER_APP = Path.Combine(_COMMON_COMPONENTS,
            "ExceptionHelper.exe");
#endif

        #endregion

        #region Methods

        /// <summary>
        /// Adds the specified ELI code as debug data to the exception.
        /// </summary>
        /// <param name="ex">The exception to add the ELI code to.</param>
        /// <param name="eliCode">The ELI code to add.</param>
        // FxCop is catching the "ELICode" key value as an invalid eli code, safe to suppress
        // this warning.
        [SuppressMessage("ExtractRules", "ES0002:MethodsShouldContainValidEliCodes")]
        static void AddEliDebugData(Exception ex, string eliCode)
        {
            try
            {
                string rootKey = "ELICode";
                string key = rootKey;
                int i = 1;
                while (ex.Data.Contains(key))
                {
                    key = rootKey + i.ToString(CultureInfo.InvariantCulture);
                    i++;
                }

                ex.Data.Add(key, eliCode);
            }
            catch
            {
                // Just eat exceptions in this method
            }
        }

        #endregion Methods

        #region IExtractExceptionLogger Members

        /// <summary>
        /// Logs the exception data to the local Extract exception log file.
        /// </summary>
        /// <param name="exceptionData">The data to be logged.</param>
        public void LogException(ExceptionLoggerData exceptionData)
        {
            string tempFile = null;
            try
            {
                string eliCode = exceptionData.EliCode;
                Exception ee = exceptionData.ExceptionData;

                // Check if there is an eli code, if so add it as debug data
                if (!string.IsNullOrEmpty(eliCode))
                {
                    AddEliDebugData(ee, eliCode);
                }

                // Serialize the exception as a hex string and write to temp file
                string hexException = ee.ToSerializedHexString();
                tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, hexException);

                // Build the arguments for the exception helper app
                StringBuilder arguments = new StringBuilder();
                arguments.Append('"');
                arguments.Append(tempFile);
                arguments.Append('"');

                // Launch the helper app
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = _EXCEPTION_HELPER_APP;
                    process.StartInfo.Arguments = arguments.ToString();
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch (Exception exception)
            {
                throw new FaultException("Unable to log exception. " + exception.Message);
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFile) && File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        #endregion
    }
}
