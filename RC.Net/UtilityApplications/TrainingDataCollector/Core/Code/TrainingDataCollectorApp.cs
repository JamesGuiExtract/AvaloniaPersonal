using Extract.ETL;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Extract.UtilityApplications.MachineLearning
{
    /// <summary>
    /// Application to be used to test the TrainingDataCollector, which is to be a ServiceProcess
    /// </summary>
    public class TrainingDataCollectorApp : ITrainingCoordinator
    {
        public string ProjectName => "";

        public string RootDir { get; set; }

        public int NumberOfBackupModelsToKeep { get => 0; set => throw new NotImplementedException(); }

        private class Args
        {
            public bool configure = false;
            public string settingsFile = null;
            public string databaseServer = null;
            public string databaseName = null;
            public string cancelTokenName = null;
            public string rootDir = null;
            public long lowestIDToProcess = 0;
            public long highestIDToProcess = 0;
            public string outputCsvPath = "";
            public List<(PropertyInfo property, object value)> propertiesToSet = new List<(PropertyInfo, object)>();
        }

        [STAThread]
        static int Main(string[] args)
        {
            bool silent = false;
            bool saveErrors = false;
            string uexName = null;
            try
            {
                int usage(bool error = false)
                {
                    UtilityMethods.ShowMessageBox("Usage:" +
                        "\r\n  To create/store data:\r\n    TrainingDataCollector <settingsFile> /databaseServer <server>" +
                        "\r\n    /databaseName <name> [/s] [/ef <exceptionFile>] [/cancelTokenName <name>] [/rootDir <path>]" +
                        "\r\n    [/processSingleBatch <lowestIDToProcess> <highestIDToProcess> <outputCSV>]" +
                        "\r\n    /s = silent = no progress bar or exceptions displayed" +
                        "\r\n    /ef <exceptionFile> log exceptions to file" +
                        "\r\n       (supports propagate errors to FAM option)" +
                        "\r\n       (/ef also implies /s)" +
                        "\r\n    /cancelTokenName <name> used when called from another Extract application to" +
                        "\r\n       allow calling application to cancel using a CancellationToken." +
                        "\r\n       (/cancelTokenName implies /s)" +
                        "\r\n    /rootDir <path> sets the root dir used to resolve relative paths (default to dir of settingsFile)" +
                        "\r\n    /processSingleBatch <lowestIDToProcess> <highestIDToProcess> <outputCSV> used to process an explicitly-bounded batch of files" +
                        "\r\n       with output to a CSV instead of the DB. This mode is used internally by TrainingDataCollector.Process to process batches of the input." +
                        "\r\n  To edit a settings file:\r\n    TrainingDataCollector /c <settingsFile> [/databaseServer <server>] [/databaseName <name>]", "Training Data Collector", error);
                    return error ? -1 : 0;
                }

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                Application.EnableVisualStyles();

                var parsedArgs = new Args();
                Type collectorType = typeof(TrainingDataCollector);
                if (args.Length > 0)
                {
                    for (int argNum = 0; argNum < args.Length; argNum++)
                    {
                        var arg = args[argNum];

                        if (arg.StartsWith("--", StringComparison.Ordinal))
                        {
                            var val = arg.Substring(2);
                            if (collectorType.TryGetProperty(val, true, out var prop))
                            {
                                if (++argNum < args.Length)
                                {
                                    try
                                    {
                                        var type = prop.PropertyType;
                                        var value = Convert.ChangeType(args[argNum], type, CultureInfo.InvariantCulture);
                                        parsedArgs.propertiesToSet.Add((prop, value));
                                        continue;
                                    }
                                    catch (Exception ex)
                                    {
                                        var ue = new ExtractException("ELI45038", "Unable to set property", ex);
                                        ue.AddDebugData("Property", val, false);
                                        ue.AddDebugData("Value", args[argNum], false);
                                        ue.Log();
                                    }
                                }
                                else
                                {
                                    var ue = new ExtractException("ELI45039", "No value given for property");
                                    ue.AddDebugData("Property", val, false);
                                    ue.Log();
                                    return usage(error: true);
                                }
                            }
                            else
                            {
                                var ue = new ExtractException("ELI45055", "Unrecognized property");
                                ue.AddDebugData("Property", val, false);
                                ue.Log();
                                return usage(error: true);
                            }
                        }
                        else if (arg.StartsWith("-", StringComparison.Ordinal) || arg.StartsWith("/", StringComparison.Ordinal))
                        {
                            var val = arg.Substring(1);
                            if (val.Equals("h", StringComparison.OrdinalIgnoreCase)
                                || val.Equals("?", StringComparison.OrdinalIgnoreCase))
                            {
                                return usage(error: false);
                            }
                            else if (val.Equals("c", StringComparison.OrdinalIgnoreCase))
                            {
                                parsedArgs.configure = true;
                            }
                            else if (val.Equals("s", StringComparison.OrdinalIgnoreCase))
                            {
                                silent = true;
                            }
                            else if (val.Equals("ef", StringComparison.OrdinalIgnoreCase))
                            {
                                saveErrors = true;
                                // /ef implies /s
                                // (else exceptions would be displayed)
                                silent = true;

                                if (++argNum < args.Length)
                                {
                                    uexName = args[argNum];
                                    continue;
                                }
                                else
                                {
                                    return usage(error: true);
                                }
                            }
                            else if (val.Equals("databaseServer", StringComparison.OrdinalIgnoreCase))
                            {
                                if (++argNum < args.Length)
                                {
                                    parsedArgs.databaseServer = args[argNum];
                                    continue;
                                }
                                else
                                {
                                    return usage(error: true);
                                }
                            }
                            else if (val.Equals("databaseName", StringComparison.OrdinalIgnoreCase))
                            {
                                if (++argNum < args.Length)
                                {
                                    parsedArgs.databaseName = args[argNum];
                                    continue;
                                }
                                else
                                {
                                    return usage(error: true);
                                }
                            }
                            else if (val.Equals("cancelTokenName", StringComparison.OrdinalIgnoreCase))
                            {
                                if (++argNum < args.Length)
                                {
                                    parsedArgs.cancelTokenName = args[argNum];
                                    continue;
                                }
                                else
                                {
                                    return usage(error: true);
                                }
                            }
                            else if (val.Equals("rootDir", StringComparison.OrdinalIgnoreCase))
                            {
                                if (++argNum < args.Length)
                                {
                                    parsedArgs.rootDir = args[argNum];
                                    continue;
                                }
                                else
                                {
                                    return usage(error: true);
                                }
                            }
                            else if (val.Equals("processSingleBatch", StringComparison.OrdinalIgnoreCase))
                            {
                                if (++argNum < args.Length
                                    && long.TryParse(args[argNum], out parsedArgs.lowestIDToProcess)
                                    && parsedArgs.lowestIDToProcess > 0
                                    && ++argNum < args.Length
                                    && long.TryParse(args[argNum], out parsedArgs.highestIDToProcess)
                                    && parsedArgs.highestIDToProcess >= parsedArgs.lowestIDToProcess
                                    && ++argNum < args.Length
                                    )
                                {
                                    parsedArgs.outputCsvPath = args[argNum];
                                    continue;
                                }
                                else
                                {
                                    return usage(error: true);
                                }
                            }
                            else
                            {
                                var ue = new ExtractException("ELI45052", "Unrecognized option!");
                                ue.AddDebugData("Option", val, false);
                                ue.Log();
                                return usage(error: true);
                            }
                        }
                        else if (parsedArgs.settingsFile == null)
                        {
                            parsedArgs.settingsFile = arg;
                        }
                        else
                        {
                            var ue = new ExtractException("ELI45053", "Unrecognized argument!");
                            ue.AddDebugData("Argument", arg, false);
                            ue.Log();
                            return usage(error: true);
                        }
                    }

                    RunApp(parsedArgs);
                }
                else
                {
                    return usage(error: true);
                }

                return 0;
            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI45051");

                if (saveErrors && uexName != null)
                {
                    try
                    {
                        ue.Log(uexName);
                    }
                    catch {}
                }
                else if (silent)
                {
                    ue.Log();
                }
                else
                {
                    ue.Display();
                }
                return -1;
            }
        }

        private static void RunApp(Args args)
        {
            NamedTokenSource namedTokenSource = null;
            CancellationToken cancelToken = CancellationToken.None;
            if (!string.IsNullOrWhiteSpace(args.cancelTokenName))
            {
                namedTokenSource = new NamedTokenSource(args.cancelTokenName);
                cancelToken = namedTokenSource.Token;
            }

            var collector = File.Exists(args.settingsFile)
                ? TrainingDataCollector.FromJson(File.ReadAllText(args.settingsFile))
                : new TrainingDataCollector();
            var coordinator = new TrainingDataCollectorApp { RootDir = args.rootDir ?? Path.GetDirectoryName(Path.GetFullPath(args.settingsFile))};
            collector.Container = coordinator;
            collector.DatabaseServer = args.databaseServer;
            collector.DatabaseName = args.databaseName;

            foreach(var (property, value) in args.propertiesToSet)
            {
                property.SetValue(collector, value);
            }

            if (args.configure)
            {
                using (var form = new TrainingDataCollectorConfigurationDialog(collector, args.databaseServer, args.databaseName))
                {
                    Application.Run(form);
                    var result = form.DialogResult;
                    if (result == DialogResult.OK)
                    {
                        File.WriteAllText(args.settingsFile, collector.ToJson());
                    }
                }
                // Prevent COM Exception Caught being logged
                // https://extract.atlassian.net/browse/ISSUE-14980
                GC.Collect();
            }
            else
            {
                collector.Log = str => Console.Write(str);
                if (args.lowestIDToProcess > 0)
                {
                    collector.ProcessSingleBatch(args.lowestIDToProcess, args.highestIDToProcess, args.outputCsvPath, cancelToken);
                }
                else
                {
                    collector.Process(cancelToken);
                }
                File.WriteAllText(args.settingsFile, collector.ToJson());
            }
        }
    }
}