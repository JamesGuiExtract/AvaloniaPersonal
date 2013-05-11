using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.ServiceProcess;

namespace Extract.Utilities
{
    /// <summary>
    /// A utility class of system methods.
    /// </summary>
    public static class SystemMethods
    {
        #region Constants

        /// <summary>
        /// Object name used in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(SystemMethods).ToString();

        /// <summary>
        /// "This operation returned because the timeout period expired."
        /// </summary>
        static int _OPERATION_TIMEOUT_EXIT_CODE = 1460;

        #endregion Constants

        /// <summary>
        /// Enumerates all <see cref="ManagementObject"/>s of the specified class.
        /// </summary>
        /// <param name="wmiClass">The class name of all resulting objects.</param>
        /// <returns>An enumeration of all <see cref="ManagementObject"/>s matching the specified
        /// criteria.
        /// <para><b>Note</b></para>
        /// The caller is responsible for disposing of all <see cref="ManagementObject"/>s returned.
        /// </returns>
        internal static IEnumerable<ManagementObject> GetWmiObjects(string wmiClass)
        {
            // Obtain all instances of the specified class.
            using (ManagementClass wmiObject = new ManagementClass(wmiClass))
            {
                using (ManagementObjectCollection instances = wmiObject.GetInstances())
                {
                    foreach (ManagementObject instance in instances)
                    {
                        yield return instance;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the specified property from the specified <see cref="ManagementObject"/>
        /// </summary>
        /// <param name="wmiObject">The <see cref="ManagementObject"/> to retrieve the property from.
        /// </param>
        /// <param name="propertyName">The name of the property to retrieve. Properties on
        /// see cref="ManagementObject"/>s  can be referenced with a '.' following the child
        /// object's property name.</param>
        /// <returns></returns>
        internal static object GetWmiProperty(ManagementBaseObject wmiObject, string propertyName)
        {
            try
            {
                // If a '.' is used to specify a property on a child object, obtain the child object
                // then request the property from it.
                int nextElementPos = propertyName.IndexOf('.');
                if (nextElementPos >= 0)
                {
                    string childObjectName = propertyName.Substring(0, nextElementPos);
                    using (ManagementObject childObject =
                        new ManagementObject(wmiObject[childObjectName].ToString()))
                    {
                        return GetWmiProperty(childObject,
                            propertyName.Substring(nextElementPos + 1));
                    }
                }
                // Otherwise just return the specified property.
                else
                {
                    return wmiObject[propertyName];
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27319", ex);
            }
        }

        /// <summary>
        /// Checks to see if the specified process is running, returns <see langword="true"/> if
        /// the process is running and <see langword="false"/> if it is not.
        /// </summary>
        /// <param name="processId">The process ID to look for.</param>
        /// <returns><see langword="true"/> if the process is running and
        /// <see langword="false"/> if it is not running.</returns>
        public static bool IsProcessRunning(int processId)
        {
            return IsProcessRunning(null, processId);
        }

        /// <summary>
        /// Checks to see if the specified process is running, returns <see langword="true"/> if
        /// the process is running and <see langword="false"/> if it is not.
        /// </summary>
        /// <param name="processId">The process ID to look for.</param>
        /// <param name="processName">The name of the process to look for.</param>
        /// <returns><see langword="true"/> if the process is running and
        /// <see langword="false"/> if it is not running.</returns>
        public static bool IsProcessRunning(string processName, int processId)
        {
            try
            {
                try
                {
                    using (Process process = Process.GetProcessById(processId))
                    {

                        // If a process name is specified, return whether the
                        // process names are equal
                        if (!string.IsNullOrEmpty(processName))
                        {
                            return process.ProcessName.Equals(processName,
                                StringComparison.OrdinalIgnoreCase);
                        }

                        // Found the process so it is running
                        return true;
                    }
                }
                catch
                {
                    // Exception indicates that the specified process ID is not running
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28771", ex);
            }
        }

        /// <summary>
        /// Gets the process name for the specified process ID.
        /// </summary>
        /// <param name="processId">The process ID to search for.</param>
        /// <returns>The name for the specified process ID.</returns>
        /// <exception cref="ExtractException">If the specified process ID is
        /// not currently running.</exception>
        public static string GetProcessName(int processId)
        {
            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    return process.ProcessName;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29807", ex);
            }
        }

        /// <summary>
        /// Gets the current process's ID.
        /// </summary>
        /// <returns>The ID for the current process.</returns>
        // This method is performing a calculation and so is better suited as a method
        // as opposed to a property.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static int GetCurrentProcessId()
        {
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    return process.Id;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30136", ex);
            }
        }

        /// <summary>
        /// Gets whether the current executing process is running under Wow64 or not.
        /// </summary>
        /// <returns><see langword="true"/> if it is running under Wow64 and
        /// <see langword="false"/> otherwise.</returns>
        public static bool IsCurrentProcessWow64()
        {
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    return IsProcessWow64(process);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30189", ex);
            }
        }

        /// <summary>
        /// Gets whether the specified process is running under Wow64 or not.
        /// </summary>
        /// <param name="process">The process to check.</param>
        /// <returns><see langword="true"/> if it is running under Wow64 and
        /// <see langword="false"/> otherwise.</returns>
        public static bool IsProcessWow64(Process process)
        {
            try
            {
                return NativeMethods.IsProcessRunningWow64(process);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30190", ex);
            }
        }

        /// <summary>
        /// Checks whether the service exists or not.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns><see langword="true"/> if the service exists on the local machine and
        /// <see langword="false"/> if it does not.</returns>
        public static bool CheckServiceExists(string serviceName)
        {
            return CheckServiceExists(serviceName, ".");
        }

        /// <summary>
        /// Checks whether the service exists or not.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="machineName">Name of the machine.</param>
        /// <returns><see langword="true"/> if the service exists on the machine and
        /// <see langword="false"/> if it does not.</returns>
        public static bool CheckServiceExists(string serviceName, string machineName)
        {
            ServiceController[] controllers = null;
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30808", _OBJECT_NAME);
                controllers = ServiceController.GetServices(machineName);
                var service = controllers.FirstOrDefault(s =>
                    s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
                return service != null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30807", ex);
            }
            finally
            {
                if (controllers != null)
                {
                    foreach (var controller in controllers)
                    {
                        controller.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Runs the executable. Will block until the executable completes.
        /// </summary>
        /// <param name="exeFile">The exe file.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The exit code of the process or 1460 ("This operation returned because the
        /// timeout period expired.") if timeToWait has expired.</returns>
        public static int RunExecutable(string exeFile, IEnumerable<string> arguments)
        {
            return RunExecutable(exeFile, arguments, int.MaxValue);
        }

        /// <summary>
        /// Runs the executable. Will block until <paramref name="timeToWait"/> has exceeded
        /// or executable completes. (if <paramref name="timeToWait"/> is
        /// <see cref="Int32.MaxValue"/> then will block until executable completes).
        /// </summary>
        /// <param name="exeFile">The exe file.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="timeToWait">The time to wait.</param>
        /// <returns>The exit code of the process or 1460 ("This operation returned because the
        /// timeout period expired.") if timeToWait has expired.</returns>
        public static int RunExecutable(string exeFile, IEnumerable<string> arguments,
            int timeToWait)
        {
            try
            {
                string argumentString = string.Join(" ", arguments
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => (s.Contains(' ') && s[0] != '"') ? s.Quote() : s)
                    .ToArray());

                return RunExecutable(exeFile, argumentString, timeToWait);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35807");
            }
        }

        /// <summary>
        /// Runs the executable. Will block until <paramref name="timeToWait"/> has exceeded
        /// or executable completes. (if <paramref name="timeToWait"/> is
        /// <see cref="Int32.MaxValue"/> then will block until executable completes).
        /// </summary>
        /// <param name="exeFile">The exe file.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="timeToWait">The time to wait.</param>
        /// <returns>The exit code of the process or 1460 ("This operation returned because the
        /// timeout period expired.") if timeToWait has expired.</returns>
        public static int RunExecutable(string exeFile, string arguments, int timeToWait)
        {
            try
            {
                // Verify this object is either licensed OR
                // is called from Extract code
                if (!LicenseUtilities.IsLicensed(LicenseIdName.ExtractCoreObjects)
                    && !LicenseUtilities.VerifyAssemblyData(Assembly.GetCallingAssembly()))
                {
                    var ee = new ExtractException("ELI32158", "Object is not licensed.");
                    ee.AddDebugData("Object Name", _OBJECT_NAME, false);
                    throw ee;
                }

                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo(exeFile, arguments);
                    process.Start();
                    if (process.WaitForExit(timeToWait))
                    {
                        return process.ExitCode;
                    }
                    else
                    {
                        return _OPERATION_TIMEOUT_EXIT_CODE;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32159");
            }
        }

        /// <summary>
        /// Runs the specified executable with the specified arguments, appends a /ef [TempFile]
        /// to the argument list. If an exception is logged to the temp file, it will be
        /// loaded and thrown.
        /// </summary>
        /// <param name="exeFile">The executable to run.</param>
        /// <param name="arguments">The command line arguments for the executable.</param>
        /// <returns>The exit code of the process or 1460 ("This operation returned because the
        /// timeout period expired.") if timeToWait has expired.</returns>
        public static int RunExtractExecutable(string exeFile, IEnumerable<string> arguments)
        {
            try
            {
                string argumentString = string.Join(" ", arguments
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => (s.Contains(' ') && s[0] != '"') ? s.Quote() : s)
                    .ToArray());

                return RunExtractExecutable(exeFile, argumentString);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35810");
            }
        }

        /// <summary>
        /// Runs the specified executable with the specified arguments, appends a /ef [TempFile]
        /// to the argument list. If an exception is logged to the temp file, it will be
        /// loaded and thrown.
        /// </summary>
        /// <param name="exeFile">The executable to run.</param>
        /// <param name="arguments">The command line arguments for the executable.</param>
        /// <returns>The exit code of the process or 1460 ("This operation returned because the
        /// timeout period expired.") if timeToWait has expired.</returns>
        public static int RunExtractExecutable(string exeFile, string arguments)
        {
            try
            {
                using (var tempFile = new TemporaryFile(".uex", false))
                {
                    arguments += " /ef " + tempFile.FileName.Quote();

                    // Run the executable and wait for it to exit
                    int exitCode = RunExecutable(exeFile, arguments, int.MaxValue);

                    var info = new FileInfo(tempFile.FileName);
                    if (info.Length > 0)
                    {
                        throw ExtractException.LoadFromFile("ELI31875", tempFile.FileName);
                    }

                    return exitCode;
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI31876");
                ee.AddDebugData("Executable Name", exeFile, false);

                // Encrypt the arguments as they may contain passwords or other information
                ee.AddDebugData("Arguments", arguments, true);

                throw ee;
            }
        }

        /// <summary>
        /// Gets the time elapsed since the system was last started or rebooted.
        /// </summary>
        /// <returns>The <see cref="TimeSpan"/> elapsed since the system was last started or
        /// rebooted.</returns>
        public static TimeSpan SystemUptime
        {
            get
            {
                try
                {
                    return NativeMethods.SystemUptime;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35294");
                }
            }
        }
    }
}
