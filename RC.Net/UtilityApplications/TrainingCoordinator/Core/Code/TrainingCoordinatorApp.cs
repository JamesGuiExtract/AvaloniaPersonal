using Extract.Licensing;
using Extract.Utilities;
using System;
using System.IO;
using System.Windows.Forms;

namespace Extract.UtilityApplications.MachineLearning
{
    /// <summary>
    /// Application to be used to test the TrainingCoordinator, which is to be a ServiceProcess
    /// </summary>
    public static class TrainingCoordinatorApp
    {
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                int usage(bool error = false)
                {
                    UtilityMethods.ShowMessageBox("Usage:" +
                        "\r\n  To edit a settings file:\r\n    TrainingCoordinator <settingsFile> [/databaseServer <server>] [/databaseName <name>]", "Training Coordinator", error);
                    return error ? -1 : 0;
                }

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                Application.EnableVisualStyles();

                if (args.Length > 0)
                {
                    string settingsFile = null;
                    string databaseServer = null;
                    string databaseName = null;

                    for (int argNum = 0; argNum < args.Length; argNum++)
                    {
                        var arg = args[argNum];

                        if (arg.StartsWith("-", StringComparison.Ordinal)
                            || arg.StartsWith("/", StringComparison.Ordinal))
                        {
                            var val = arg.Substring(1);
                            if (val.Equals("h", StringComparison.OrdinalIgnoreCase)
                                    || val.Equals("?", StringComparison.OrdinalIgnoreCase))
                            {
                                return usage(error: false);
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
                                var ue = new ExtractException("ELI45795", "Unrecognized option!");
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
                            var ue = new ExtractException("ELI45796", "Unrecognized argument!");
                            ue.AddDebugData("Argument", arg, false);
                            ue.Log();
                            return usage(error: true);
                        }
                    }

                    var coordinator = File.Exists(settingsFile)
                        ? TrainingCoordinator.FromJson(File.ReadAllText(settingsFile))
                        : new TrainingCoordinator();

                    coordinator.RootDir = Path.GetDirectoryName(Path.GetFullPath(settingsFile));
                    using (var form = new TrainingCoordinatorConfigurationDialog(coordinator, databaseServer, databaseName))
                    {
                        Application.Run(form);
                        var result = form.DialogResult;
                        if (result == DialogResult.OK)
                        {
                            File.WriteAllText(settingsFile, coordinator.ToJson());
                        }
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
                var ue = ex.AsExtract("ELI45797");

                ue.Display();
                return -1;
            }
        }
    }
}