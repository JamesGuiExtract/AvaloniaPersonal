using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Extract.Licensing;
using Microsoft.Win32.SafeHandles;

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
        public static int OperationTimeoutExitCode { get; private set; } = 1460;

        /// <summary>
        /// This operation returned because it was canceled
        /// </summary>
        public static int OperationCanceledExitCode { get; private set; } = 1461;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Caches a determination as to whether the this process is running internally at Extract.
        /// </summary>
        static bool? _isExtractInternal;

        #endregion Fields


        #region Internal Methods

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
        /// <see cref="ManagementObject"/>s  can be referenced with a '.' following the child
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

        #endregion Internal Methods


        #region Public Methods

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
        /// <param name="createNoWindow">Whether to eschew shell execution to avoid showing a cmd window</param>
        /// <param name="cancelToken">Used to cancel, which will kill the process</param>
        /// <returns>The exit code of the process or  
        /// If canceled by cancelToken returns <see cref="OperationCanceledExitCode"/></returns>
        public static int RunExecutable(string exeFile, IEnumerable<string> arguments, bool createNoWindow = false,
            CancellationToken cancelToken = default(CancellationToken))
        {
            return RunExecutable(exeFile, arguments, int.MaxValue, createNoWindow, cancelToken);
        }

        /// <summary>
        /// Runs the executable. Will block until <paramref name="timeToWait"/> has exceeded
        /// or executable completes. (if <paramref name="timeToWait"/> is
        /// <see cref="Int32.MaxValue"/> then will block until executable completes).
        /// </summary>
        /// <param name="exeFile">The exe file.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="timeToWait">The time to wait.</param>
        /// <param name="createNoWindow">Whether to eschew shell execution to avoid showing a cmd window</param>
        /// <returns>The exit code of the process or <see cref="OperationTimeoutExitCode"/> ("This operation returned because the
        /// timeout period expired.") if timeToWait has expired. 
        /// If canceled by cancelToken returns <see cref="OperationCanceledExitCode"/></returns>
        public static int RunExecutable(string exeFile, IEnumerable<string> arguments,
            int timeToWait, bool createNoWindow = false, CancellationToken cancelToken = default(CancellationToken))
        {
            try
            {
                string argumentString = string.Join(" ", arguments
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => (s.Contains(' ') && s[0] != '"') ? s.Quote() : s)
                    .ToArray());

                return RunExecutable(exeFile, argumentString, timeToWait, createNoWindow, cancelToken);
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
        /// <param name="createNoWindow">Whether to eschew shell execution to avoid showing a cmd window</param>
        /// <param name="cancelToken"><see cref="CancellationToken"/> that can be used to cancel the execution</param>
        /// <param name="startAndReturnImmediately">Runs the exeFile and return without waiting for app to complete"/></param>
        /// <returns>The exit code of the process or <see cref="OperationTimeoutExitCode"/> ("This operation returned because the
        /// timeout period expired.") if timeToWait has expired. 
        /// If canceled by cancelToken returns <see cref="OperationCanceledExitCode"/></returns>
        public static int RunExecutable(string exeFile, string arguments, int timeToWait, bool createNoWindow = false,
            CancellationToken cancelToken = default(CancellationToken), bool startAndReturnImmediately = false)
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
                    process.StartInfo = new ProcessStartInfo(exeFile, arguments)
                    {
                        UseShellExecute = !createNoWindow,
                        CreateNoWindow = createNoWindow
                    };
                    process.Start();
                    if (startAndReturnImmediately)
                    {
                        return 0;
                    }
                    ManualResetEvent exitEvent = new ManualResetEvent(true);
                    exitEvent.SafeWaitHandle = new SafeWaitHandle(process.Handle, false);
                    int waitResult = (WaitHandle.WaitAny(
                        new WaitHandle[]
                        {
                            exitEvent,
                            cancelToken.WaitHandle
                        },
                        timeToWait));
                    switch (waitResult)
                    {
                        case WaitHandle.WaitTimeout:
                            process.CloseMainWindow();

                            // CloseMainWindow() only works if there is a UI and even then may not actually result in the process
                            // ending so kill it if it doesn't exit by itself
                            // https://extract.atlassian.net/browse/ISSUE-15386
                            if (!process.WaitForExit(2000))
                            {
                                process.Kill();
                            }
                            return OperationTimeoutExitCode;
                        case 1:
                            process.CloseMainWindow();

                            // CloseMainWindow() only works if there is a UI and even then may not actually result in the process
                            // ending so kill it if it doesn't exit by itself
                            // https://extract.atlassian.net/browse/ISSUE-15386
                            if (!process.WaitForExit(2000))
                            {
                                process.Kill();
                            }
                            return OperationCanceledExitCode;
                    }
                    return process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32159");
            }
        }

        /// <summary>
        /// Runs an executable
        /// </summary>
        /// <param name="command">The exe file and arguments.</param>
        /// <param name="standardOutput">The output of the process</param>
        /// <param name="standardError">The error message of the process</param>
        /// <returns>The exit code of the process or 
        /// If canceled by cancelToken returns <see cref="OperationCanceledExitCode"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static int RunExecutable(string command, out string standardOutput,
            out string standardError, CancellationToken cancelToken = default(CancellationToken))
        {
            try
            {
                var args = CommandLineToArgs(command);
                var exeName = args.FirstOrDefault();
                ExtractException.Assert("ELI45098", "No executable given",
                    exeName != null,
                    "Command", command);

                return RunExecutable(exeName, args.Skip(1),
                    out standardOutput, out standardError, cancelToken);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45099");
            }
        }

        /// <summary>
        /// Runs an executable
        /// </summary>
        /// <param name="exeFile">The exe file.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="standardOutput">The output of the process</param>
        /// <param name="standardError">The error message of the process</param>
        /// <returns>The exit code of the process or
        /// If canceled by cancelToken returns <see cref="OperationCanceledExitCode"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static int RunExecutable(string exeFile, IEnumerable<string> arguments,
            out string standardOutput, out string standardError, CancellationToken cancelToken = default(CancellationToken))
        {
            try
            {
                string argumentString = string.Join(" ", arguments
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => (s.Contains(' ') && s[0] != '"') ? s.Quote() : s)
                    .ToArray());

                return RunExecutable(exeFile, argumentString,
                    out standardOutput, out standardError, cancelToken);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45113");
            }
        }

        /// <summary>
        /// Runs an executable
        /// </summary>
        /// <param name="exeFile">The exe file.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="standardOutput">The output of the process</param>
        /// <param name="standardError">The error message of the process</param>
        /// <returns>The exit code of the process or 
        /// If canceled by cancelToken returns <see cref="OperationCanceledExitCode"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        static int RunExecutable(string exeFile, string argumentString,
            out string standardOutput, out string standardError, CancellationToken cancelToken = default(CancellationToken))
        {
            try
            {
                // Verify this object is either licensed OR
                // is called from Extract code
                if (!LicenseUtilities.IsLicensed(LicenseIdName.ExtractCoreObjects)
                    && !LicenseUtilities.VerifyAssemblyData(Assembly.GetCallingAssembly()))
                {
                    var ee = new ExtractException("ELI45114", "Object is not licensed.");
                    ee.AddDebugData("Object Name", _OBJECT_NAME, false);
                    throw ee;
                }

                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo(exeFile, argumentString)
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    process.Start();

                    Task<string> standardOutputTask = ConsumeOutput(process.StandardOutput);
                    Task<string> standardErrorTask = ConsumeOutput(process.StandardError);

                    ManualResetEvent exitEvent = new ManualResetEvent(true);
                    exitEvent.SafeWaitHandle = new SafeWaitHandle(process.Handle, false);
                    int waitResult = (WaitHandle.WaitAny(
                        new WaitHandle[]
                        {
                            exitEvent,
                            cancelToken.WaitHandle
                        }));

                    if (waitResult == 1)
                    {
                        process.CloseMainWindow();

                        // CloseMainWindow() only works if there is a UI and even then may not actually result in the process
                        // ending so kill it if it doesn't exit by itself
                        // https://extract.atlassian.net/browse/ISSUE-15386
                        if (!process.WaitForExit(2000))
                        {
                            process.Kill();
                        }
                        standardOutput = standardOutputTask.GetAwaiter().GetResult();
                        standardError = standardErrorTask.GetAwaiter().GetResult();

						return OperationCanceledExitCode;
                    }

                    standardOutput = standardOutputTask.GetAwaiter().GetResult();
                    standardError = standardErrorTask.GetAwaiter().GetResult();

                    return process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI45115");
                ue.AddDebugData("Executable", exeFile, false);
                ue.AddDebugData("Arguments", argumentString, false);
                throw ue;
            }
        }

        /// <summary>
        /// Runs the specified executable with the specified arguments, appends a /ef [TempFile]
        /// to the argument list. If an exception is logged to the temp file, it will be
        /// loaded and thrown.
        /// </summary>
        /// <param name="exeFile">The executable to run.</param>
        /// <param name="arguments">The command line arguments for the executable.</param>
        /// <param name="cancelToken"><see cref="CancellationToken"/> that can be used to cancel the execution</param>
        /// <param name="addCancellableNameAsArgument">If <c>true</c> another argument will be added to the argument list 
        /// /CancelTokenName "eventName"
        /// if <c>false</c> there will be no changes</param>
        /// <returns>The exit code of the process or  
        /// If canceled by cancelToken returns <see cref="OperationCanceledExitCode"/></returns>
        public static int RunExtractExecutable(string exeFile, IEnumerable<string> arguments,
            CancellationToken cancelToken = default(CancellationToken), bool addCancellableNameAsArgument = false)
        {
            try
            {
                string argumentString = string.Join(" ", arguments
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => (s.Contains(' ') && s[0] != '"') ? s.Quote() : s)
                    .ToArray());

                return RunExtractExecutable(exeFile, argumentString, cancelToken, addCancellableNameAsArgument);
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
        /// <param name="cancelToken"><see cref="CancellationToken"/> that can be used to cancel the execution</param>
        /// <param name="addCancellableNameAsArgument">If <c>true</c> another argument will be added to the argument list 
        /// /CancelTokenName "eventName"
        /// if <c>false</c> there will be no changes</param>
        /// <returns>The exit code of the process or 
        /// If canceled by cancelToken returns <see cref="OperationCanceledExitCode"/></returns>
        public static int RunExtractExecutable(string exeFile, string arguments,
            CancellationToken cancelToken = default(CancellationToken), bool addCancellableNameAsArgument = false)
        {
            try
            {
                // use a guid as an event name
                string cancelEventName = @"Global\" + Guid.NewGuid().ToString(); ;

                // Register the Cancel method for the named token with the passed in cancel token
                // this will cause the 
                using (var namedTokenSource = new NamedTokenSource(cancelEventName))
                using (var tokenRegistration = cancelToken.Register(() => namedTokenSource.Cancel()))
                {
                    using (var tempFile = new TemporaryFile(".uex", false))
                    {
                        arguments += (addCancellableNameAsArgument) ? " /CancelTokenName " + cancelEventName.Quote() : string.Empty;
                        arguments += " /ef " + tempFile.FileName.Quote();

                        // If passing the Cancellable name as an argument it is expected that the called program will
                        // exit when the Cancel source associated with the Name  is canceled so pass the default that 
                        // will not cancel, otherwise pass the cancelToken argument so the executable will attempt 
                        // to be closed if cancelToken is canceled
                        CancellationToken cancelTokenToPass = (addCancellableNameAsArgument) ? default(CancellationToken) : cancelToken;

                        // Run the executable and wait for it to exit
                        int exitCode = RunExecutable(exeFile, arguments, int.MaxValue, cancelToken: cancelTokenToPass);

                        var info = new FileInfo(tempFile.FileName);
                        if (info.Length > 0)
                        {
                            throw ExtractException.LoadFromFile("ELI31875", tempFile.FileName);
                        }

                        return exitCode;
                    }
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
        /// Runs the specified executable with the specified arguments, appends a /ef [TempFile]
        /// to the argument list. If an exception is logged to the temp file, it will be
        /// loaded and thrown or logged.
        /// </summary>
        /// <param name="exeFile">The executable to run.</param>
        /// <param name="arguments">The command line arguments for the executable.</param>
        /// <param name="standardOutput">The output of the process</param>
        /// <param name="standardError">The error message of the process</param>
        /// <param name="cancelToken"><see cref="CancellationToken"/> that can be used to cancel the execution</param>
        /// <param name="addCancellableNameAsArgument">If <c>true</c> another argument will be added to the argument list 
        /// /CancelTokenName "eventName"
        /// if <c>false</c> there will be no changes</param>
        /// <param name="onlyLogExceptionsFromExecutable">Whether to log exceptions written to the temp file rather than throwing them</param>
        /// <returns>The exit code of the process or  
        /// If canceled by cancelToken returns <see cref="OperationCanceledExitCode"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static int RunExtractExecutable(string exeFile, IEnumerable<string> arguments,
            out string standardOutput, out string standardError,
            CancellationToken cancelToken = default(CancellationToken), bool addCancellableNameAsArgument = false,
            bool onlyLogExceptionsFromExecutable = false)
        {
            try
            {
                string argumentString = string.Join(" ", arguments
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => (s.Contains(' ') && s[0] != '"') ? s.Quote() : s)
                    .ToArray());

                return RunExtractExecutable(exeFile, argumentString, out standardOutput, out standardError,
                    cancelToken, addCancellableNameAsArgument, onlyLogExceptionsFromExecutable);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45706");
            }
        }

        /// <summary>
        /// Runs the specified executable with the specified arguments, appends a /ef [TempFile]
        /// to the argument list. If an exception is logged to the temp file, it will be
        /// loaded and thrown or logged.
        /// </summary>
        /// <param name="exeFile">The executable to run.</param>
        /// <param name="arguments">The command line arguments for the executable.</param>
        /// <param name="standardOutput">The output of the process</param>
        /// <param name="standardError">The error message of the process</param>
        /// <param name="cancelToken"><see cref="CancellationToken"/> that can be used to cancel the execution</param>
        /// <param name="addCancellableNameAsArgument">If <c>true</c> another argument will be added to the argument list 
        /// /CancelTokenName "eventName"
        /// if <c>false</c> there will be no changes</param>
        /// <param name="onlyLogExceptionsFromExecutable">Whether to log exceptions written to the temp file rather than throwing them</param>
        /// <returns>The exit code of the process or 
        /// If canceled by cancelToken returns <see cref="OperationCanceledExitCode"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static int RunExtractExecutable(string exeFile, string arguments,
            out string standardOutput, out string standardError,
            CancellationToken cancelToken = default(CancellationToken), bool addCancellableNameAsArgument = false,
            bool onlyLogExceptionsFromExecutable = false)
        {
            try
            {
                // use a guid as an event name
                string cancelEventName = @"Global\" + Guid.NewGuid().ToString(); ;

                // Register the Cancel method for the named token with the passed in cancel token
                // this will cause the 
                using (var namedTokenSource = new NamedTokenSource(cancelEventName))
                using (var tokenRegistration = cancelToken.Register(() => namedTokenSource.Cancel()))
                {
                    using (var tempFile = new TemporaryFile(".uex", false))
                    {
                        arguments += (addCancellableNameAsArgument) ? " /CancelTokenName " + cancelEventName.Quote() : string.Empty;
                        arguments += " /ef " + tempFile.FileName.Quote();

                        // If passing the Cancellable name as an argument it is expected that the called program will
                        // exit when the Cancel source associated with the Name  is canceled so pass the default that 
                        // will not cancel, otherwise pass the cancelToken argument so the executable will attempt 
                        // to be closed if cancelToken is canceled
                        CancellationToken cancelTokenToPass = (addCancellableNameAsArgument) ? default(CancellationToken) : cancelToken;

                        // Run the executable and wait for it to exit
                        int exitCode = RunExecutable(exeFile, arguments, out standardOutput, out standardError, cancelTokenToPass);

                        var info = new FileInfo(tempFile.FileName);
                        if (info.Length > 0)
                        {
                            var ue = ExtractException.LoadFromFile("ELI45707", tempFile.FileName);
                            if (onlyLogExceptionsFromExecutable)
                            {
                                ue.Log();
                            }
                            else
                            {
                                throw ue;
                            }
                        }

                        return exitCode;
                    }
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI45708");
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

        /// <summary>
        /// Displays a prompt for windows credentials and validates the credentials entered against
        /// the domain or machine. If invalid credentials are entered, the prompt will be
        /// re-displayed to allow the user to try again.
        /// </summary>
        /// <param name="parent">The prompting <see cref="IntPtr"/> that, if specified, will cause
        /// the prompt to run modally; if <see langword="null"/> the prompt will be displayed
        /// non-modally.</param>
        /// <param name="caption">The caption to display in the prompt.</param>
        /// <param name="message">The message to display in the prompt.</param>
        /// <param name="defaultToCurrentUser"><see langword="true"/> to initialize the prompt with
        /// the current user's account; <see langword="false"/> to not initialize the prompt with
        /// any account.</param>
        /// <returns>A <see cref="WindowsIdentity"/> representing the authenticated credentials if
        /// successful, or <see langword="null"/> if the user cancelled without having entered valid
        /// credentials.</returns>
        public static WindowsIdentity PromptForAndValidateWindowsCredentials(IntPtr parent,
            string caption, string message, bool defaultToCurrentUser)
        {
            try
            {
                return NativeMethods.PromptForAndValidateWindowsCredentials(
                    parent, caption, message, defaultToCurrentUser);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37644");
            }
        }

        /// <summary>
        /// Determines whether this process is running internally at Extract by checking for drive
        /// mappings to either fnp2 or es-it-dc-01.
        /// </summary>
        /// <returns> <see langword="true"/> if this process is running internally at Extract;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsExtractInternal()
        {
            try
            {
                if (!_isExtractInternal.HasValue)
                {
                    _isExtractInternal = NativeMethods.isInternalToolsLicensed();
                }

                return _isExtractInternal.Value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39370");
            }
        }

        /// <summary>
        /// Converts a command-line string into an array of args, including the program name
        /// </summary>
        /// <remarks>
        /// Taken from https://stackoverflow.com/a/749653
        /// </remarks>
        /// <param name="commandLine">The command-line to split</param>
        /// <returns>Array of arguments</returns>
        public static string[] CommandLineToArgs(string commandLine)
        {
            IntPtr argv = IntPtr.Zero;
            try
            {
                argv = NativeMethods.CommandLineToArgvW(commandLine, out int argc);
                ExtractException.Assert("ELI45097", "Unable to parse input", argv != IntPtr.Zero);

                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }
                return args;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45096");
            }
            finally
            {
                if (argv != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(argv);
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Handle output stream of a process so that a '\r' not followed by a '\n' causes the previous text on the line to be overwritten (as with the console)
        /// </summary>
        /// <param name="reader">The text reader to handle, e.g., Process.StandardOutput</param>
        static async Task<string> ConsumeOutput(TextReader reader)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.AutoFlush = true;

            long outputDataStartOfLine = 0;
            char lastChar = '\n';
            char[] buffer = new char[256];
            int charsRead;
            while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                for (int i = 0; i < charsRead; i++)
                {
                    char thisChar = buffer[i];

                    // Handle carriage return
                    if (lastChar == '\r' && thisChar != '\n')
                    {
                        stream.Position = outputDataStartOfLine;
                    }

                    writer.Write(thisChar);

                    if (thisChar == '\n')
                    {
                        outputDataStartOfLine = stream.Position;
                    }

                    lastChar = thisChar;
                }
            }
            stream.Position = 0;

            return new StreamReader(stream).ReadToEnd();
        }

        #endregion Private Methods
    }
}
