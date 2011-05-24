using Extract.ExceptionService;
using Extract.Utilities;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.ExceptionHelper
{
    static class ExceptionHelperProgram
    {
        /// <summary>
        /// Enumeration that lists the possible operations for the exception
        /// </summary>
        [Flags]
        enum ExceptionOperation
        {
            /// <summary>
            /// No operation
            /// </summary>
            None = 0x0,

            /// <summary>
            /// Log the exception locally
            /// </summary>
            Log = 0x1,

            /// <summary>
            /// Display the exception
            /// </summary>
            Display = 0x2,

            /// <summary>
            /// Log the exception remotely
            /// </summary>
            RemoteLog = 0x4
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [SuppressMessage("ExtractRules", "ES0002:MethodsShouldContainValidEliCodes")]
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 1 || args.Length > 11)
                {
                    ShowUsage("Incorrect number of arguments.");
                    return;
                }
                else if (args.Contains("/?"))
                {
                    ShowUsage();
                    return;
                }

                // Get the file name
                string fileName = Path.GetFullPath(args[0]);

                // Parse the remaining arguments
                var operation = ExceptionOperation.None;
                string remoteMachine = null;
                int processId = -1;
                string productName = null;
                string eliCode = null;
                bool deleteExceptionFile = false;
                for (int i = 1; i < args.Length; i++)
                {
                    var argument = args[i];
                    if (argument.StartsWith("ELI", StringComparison.Ordinal))
                    {
                        if (!string.IsNullOrEmpty(eliCode))
                        {
                            ShowUsage("Only one ELI code can be specified. First Code: "
                                + eliCode + " Second: " + argument);
                            return;
                        }
                        else if (!Regex.IsMatch(argument, @"^ELI\d{5,}$"))
                        {
                            ShowUsage("ELI code did not match required format.");
                            return;
                        }
                        eliCode = argument;
                    }
                    else if (argument.Equals("/display", StringComparison.OrdinalIgnoreCase))
                    {
                        operation |= ExceptionOperation.Display;
                    }
                    else if (argument.Equals("/log", StringComparison.OrdinalIgnoreCase))
                    {
                        operation |= ExceptionOperation.Log;
                    }
                    else if (argument.Equals("/remote", StringComparison.OrdinalIgnoreCase))
                    {
                        // Check for ip address/machine name
                        if (i + 1 < args.Length)
                        {
                            remoteMachine = args[++i];
                        }
                        else
                        {
                            ShowUsage("Missing machine/ip address following the remote switch.");
                            return;
                        }
                        operation |= ExceptionOperation.RemoteLog;
                    }
                    else if (argument.Equals("/pid", StringComparison.OrdinalIgnoreCase))
                    {
                        if (i + 1 < args.Length)
                        {
                            var pid = args[++i];
                            if (!int.TryParse(pid, out processId))
                            {
                                ShowUsage("Invalid Process ID specified: " + pid);
                                return;
                            }
                        }
                        else
                        {
                            ShowUsage("Missing Process ID.");
                            return;
                        }
                    }
                    else if (argument.Equals("/product", StringComparison.OrdinalIgnoreCase))
                    {
                        if (i + 1 < args.Length)
                        {
                            productName = args[++i];
                        }
                        else
                        {
                            ShowUsage("Missing product name.");
                            return;
                        }
                    }
                    else if (argument.Equals("/delete", StringComparison.OrdinalIgnoreCase))
                    {
                        deleteExceptionFile = true;
                    }
                    else
                    {
                        ShowUsage("Unrecognized argument: " + argument);
                        return;
                    }
                }

                // If no operation specified yet, set the operation to log
                if (operation == ExceptionOperation.None)
                {
                    operation = ExceptionOperation.Log;
                }

                if (string.IsNullOrEmpty(eliCode))
                {
                    eliCode = "ELI30288";
                }

                // Load the exception from disk
                var exception = ExtractException.LoadFromFile(eliCode, fileName);

                if (deleteExceptionFile)
                {
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI32597", true);
                    }
                }

                // Get the current time (needed for both the local and remote logging)
                var dateTimeUtc = DateTime.UtcNow;
                if (operation.HasFlag(ExceptionOperation.Log))
                {
                    // Compute the number of seconds since 01/01/1970 00:00:00
                    var baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var dateTime = (int)(dateTimeUtc - baseTime).TotalSeconds;

                    var ee = exception.AsExtract(eliCode);
                    ee.Log(Environment.MachineName, Environment.UserName, dateTime,
                        processId, productName, true);
                }
                if (operation.HasFlag(ExceptionOperation.RemoteLog))
                {
                    LogRemotely(remoteMachine, exception, eliCode, dateTimeUtc.ToFileTimeUtc(),
                        processId, productName);
                }
                if (operation.HasFlag(ExceptionOperation.Display))
                {
                    exception.ExtractDisplay(eliCode);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI30289", true);
            }
        }

        /// <summary>
        /// Logs the exception to the specified remote exception service.
        /// </summary>
        /// <param name="remoteAddress">The remote address.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="eliCode">The eli code.</param>
        /// <param name="dateTimeUtc">The utc file time value.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="productVersion">The product version.</param>
        static void LogRemotely(string remoteAddress, Exception ex, string eliCode,
            long dateTimeUtc, int processId, string productVersion)
        {
            ChannelFactory<IExtractExceptionLogger> factory = null;
            try
            {
                // Build the address and open the channel to the WCF service
                var address = string.Concat("net.tcp://",
                    remoteAddress, "/", ExceptionLoggerData.WcfTcpEndPoint);
                factory = new ChannelFactory<IExtractExceptionLogger>(new NetTcpBinding(),
                    new EndpointAddress(address));
                var logger = factory.CreateChannel();

                logger.LogException(new ExceptionLoggerData(ex, eliCode,
                    Environment.MachineName, Environment.UserName,
                    dateTimeUtc, processId, productVersion));

                factory.Close();
                factory = null;
            }
            catch (Exception ex2)
            {
                if (factory != null)
                {
                    factory.Abort();
                    factory = null;
                }

                // Failed to use logging service so log exception locally
                ex2.ExtractLog("ELI32595", true);
                ex.ExtractLog(eliCode, true);
            }
        }

        /// <overloads>Displays the usage message.</overloads>
        /// <summary>
        /// Displays the usage message.
        /// </summary>
        static void ShowUsage()
        {
            ShowUsage(null);
        }

        /// <summary>
        /// Displays the usage message prepended with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message to display before the usage message.</param>
        static void ShowUsage(string errorMessage)
        {
            // Check if there is an error message
            bool isError = !string.IsNullOrEmpty(errorMessage);

            // Initialize the string builder with the error message if specified
            StringBuilder usage = new StringBuilder(isError ? errorMessage : "");
            if (isError)
            {
                usage.AppendLine();
                usage.AppendLine();
            }

            // Add the command line syntax
            usage.Append(Environment.GetCommandLineArgs()[0]);
            usage.Append(" </?>|<exceptionfile> [EliCode] [/display] [/log] [/remote <machine/ipaddress>]");
            usage.Append(" [/pid <ProcessId>] [/product <ProductName>] /delete");
            usage.AppendLine();
            usage.Append("The default behavior is for the exception to be logged locally.  ");
            usage.Append("If you would like to log locally and remotely you must specify both ");
            usage.Append("the /remote and /local flags.");
            usage.AppendLine();
            usage.AppendLine("Note - the process id, and product name are only added when exceptions are logged.");
            usage.AppendLine();
            usage.AppendLine("Options:");
            usage.AppendLine("    /? - Display help");
            usage.AppendLine("    exceptionfile - The name of the file containing");
            usage.AppendLine("        the serialized exception");
            usage.AppendLine("    EliCode - Optional eli code to associate with the exception.");
            usage.AppendLine("        Must start with ELI and contain at least 5 digits");
            usage.AppendLine("    /display - Display the exception.");
            usage.AppendLine("    /log - Log the exception locally (this is the default behavior).");
            usage.AppendLine("    /remote - Log the exception to a remote machine.");
            usage.AppendLine("    <machine/ipaddress> - The name or ip address of the machine");
            usage.AppendLine("        that is running the exception service.");
            usage.AppendLine("    /delete - Deletes the specified <exceptionfile> after logging the exception.");

            // Display the usage as an error or as an information box
            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }
    }
}