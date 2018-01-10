using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Extract.UtilityApplications.NERTrainer
{
    /// <summary>
    /// Application to be used to test the NERTrainer, which is to be a ServiceProcess
    /// </summary>
    public class NERTrainerApp
    {
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
                        "\r\n  To train/test a machine:\r\n    NERTrainer <settingsFile> /databaseServer <server> /databaseName <name> [/s] [/ef <exceptionFile>]" +
                        "\r\n    /s = silent = no progress bar or exceptions displayed" +
                        "\r\n    /ef <exceptionFile> log exceptions to file" +
                        "\r\n       (supports propagate errors to FAM option)" +
                        "\r\n       (/ef also implies /s)" +
                        "\r\n  To edit a settings file:\r\n    NERTrainer /c <settingsFile> [/databaseServer <server>] [/databaseName <name>]", "NER Trainer", error);
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
                    Type trainerType = typeof(NERTrainer);
                    for (int argNum = 0; argNum < args.Length; argNum++)
                    {
                        var arg = args[argNum];

                        if (arg.StartsWith("--", StringComparison.Ordinal))
                        {
                            var val = arg.Substring(2);
                            if (trainerType.TryGetProperty(val, true, out var prop))
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
                                        var ue = new ExtractException("ELI45101", "Unable to set property", ex);
                                        ue.AddDebugData("Property", val, false);
                                        ue.AddDebugData("Value", args[argNum], false);
                                        ue.Log();
                                    }
                                }
                                else
                                {
                                    var ue = new ExtractException("ELI45102", "No value given for property");
                                    ue.AddDebugData("Property", val, false);
                                    ue.Log();
                                    return usage(error: true);
                                }
                            }
                            else
                            {
                                var ue = new ExtractException("ELI45103", "Unrecognized property");
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
                                // /ef implies /s // (else exceptions would be displayed)
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
                                var ue = new ExtractException("ELI45104", "Unrecognized option!");
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
                            var ue = new ExtractException("ELI45105", "Unrecognized argument!");
                            ue.AddDebugData("Argument", arg, false);
                            ue.Log();
                            return usage(error: true);
                        }
                    }

                    var trainer = File.Exists(settingsFile)
                        ? NERTrainer.FromJson(File.ReadAllText(settingsFile))
                        : new NERTrainer();

                    foreach(var (property, value) in propertiesToSet)
                    {
                        property.SetValue(trainer, value);
                    }

                    if (configure)
                    {
                        using (var form = new NERTrainerConfigurationDialog(trainer, databaseServer, databaseName))
                        {
                            Application.Run(form);
                            var result = form.DialogResult;
                            if (result == DialogResult.OK)
                            {
                                File.WriteAllText(settingsFile, trainer.ToJson());
                            }
                        }

                        // Prevent COM Exception Caught being logged
                        // https://extract.atlassian.net/browse/ISSUE-14980
                        GC.Collect();
                    }
                    else
                    {
                        trainer.Process(databaseServer, databaseName);
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
                var ue = ex.AsExtract("ELI45106");

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