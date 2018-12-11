using Extract.ETL;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Extract.UtilityApplications.TrainingDataCollector
{
    /// <summary>
    /// Application to be used to test the TrainingDataCollector, which is to be a ServiceProcess
    /// </summary>
    public class TrainingDataCollectorApp : ITrainingCoordinator
    {
        public string ProjectName => "";

        public string RootDir { get; set; }

        public int NumberOfBackupModelsToKeep { get => 0; set => throw new NotImplementedException(); }

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
                        "\r\n  To create/store data:\r\n    TrainingDataCollector <settingsFile> /databaseServer <server> /databaseName <name> [/s] [/ef <exceptionFile>]" +
                        "\r\n    /s = silent = no progress bar or exceptions displayed" +
                        "\r\n    /ef <exceptionFile> log exceptions to file" +
                        "\r\n       (supports propagate errors to FAM option)" +
                        "\r\n       (/ef also implies /s)" +
                        "\r\n  To edit a settings file:\r\n    TrainingDataCollector /c <settingsFile> [/databaseServer <server>] [/databaseName <name>]", "NER Data Collector", error);
                    return error ? -1 : 0;
                }

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                Application.EnableVisualStyles();

                if (args.Length > 0)
                {
                    bool configure = false;
                    string settingsFile = null;
                    string databaseServer = null;
                    string databaseName = null;
                    List<(PropertyInfo property, object value)> propertiesToSet = new List<(PropertyInfo, object)>();
                    Type collectorType = typeof(TrainingDataCollector);
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
                                        var value = Convert.ChangeType(args[argNum], type);
                                        propertiesToSet.Add((prop, value));
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
                        else if (arg.StartsWith("-", StringComparison.Ordinal)
                            || arg.StartsWith("/", StringComparison.Ordinal))
                        {
                            var val = arg.Substring(1);
                            if (val.Equals("h", StringComparison.OrdinalIgnoreCase)
                                    || val.Equals("?", StringComparison.OrdinalIgnoreCase))
                            {
                                return usage(error: false);
                            }
                            else if (val.Equals("c", StringComparison.OrdinalIgnoreCase))
                            {
                                configure = true;
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
                                    databaseServer = args[argNum];
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
                                    databaseName = args[argNum];
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
                        else if (settingsFile == null)
                        {
                            settingsFile = arg;
                        }
                        else
                        {
                            var ue = new ExtractException("ELI45053", "Unrecognized argument!");
                            ue.AddDebugData("Argument", arg, false);
                            ue.Log();
                            return usage(error: true);
                        }
                    }

                    var collector = File.Exists(settingsFile)
                        ? TrainingDataCollector.FromJson(File.ReadAllText(settingsFile))
                        : new TrainingDataCollector();
                    var coordinator = new TrainingDataCollectorApp { RootDir = Path.GetDirectoryName(Path.GetFullPath(settingsFile)) };
                    collector.Container = coordinator;
                    collector.DatabaseServer = databaseServer;
                    collector.DatabaseName = databaseName;

                    foreach(var (property, value) in propertiesToSet)
                    {
                        property.SetValue(collector, value);
                    }

                    if (configure)
                    {
                        using (var form = new TrainingDataCollectorConfigurationDialog(collector, databaseServer, databaseName))
                        {
                            Application.Run(form);
                            var result = form.DialogResult;
                            if (result == DialogResult.OK)
                            {
                                File.WriteAllText(settingsFile, collector.ToJson());
                            }
                        }
                        // Prevent COM Exception Caught being logged
                        // https://extract.atlassian.net/browse/ISSUE-14980
                        GC.Collect();
                    }
                    else
                    {
                        collector.Process(System.Threading.CancellationToken.None);
                        File.WriteAllText(settingsFile, collector.ToJson());
                    }
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
    }
}