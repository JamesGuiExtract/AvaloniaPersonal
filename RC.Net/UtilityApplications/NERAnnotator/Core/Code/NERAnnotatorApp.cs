using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Extract.Licensing;
using Extract.Utilities;

namespace Extract.UtilityApplications.NERAnnotator
{
    public static class NERAnnotatorApp
    {
        [STAThread]
        static int Main(string[] args)
        {
            bool silent = false;
            bool saveErrors = false;
            string uexName = null;
            string cancelTokenName = null;
            try
            {
                int usage(bool error = false)
                {
                    UtilityMethods.ShowMessageBox("Usage:" +
                        "\r\n  To open the editor:\r\n    NERAnnotator" +
                        "\r\n  To create a labeled tokens file:\r\n    NERAnnotator <settingsFile> /p [/s] [/ef <exceptionFile>] [--<propertyName> <propertyValue> ...]" +
                        "\r\n    /s = silent = no progress bar or exceptions displayed" +
                        "\r\n    /ef <exceptionFile> log exceptions to file" +
                        "\r\n       (supports propagate errors to FAM option)" +
                        "\r\n       (/ef also implies /s)" +
                        "\r\n    /CancelTokenName <CancelTokenName> used when called from another Extract application to" +
                        "\r\n       allow calling application to cancel using CancellationToken." +
                        "\r\n       (/CancelTokenName implies /s)" +
                        "\r\n    /<propertyName> <propertyValue> override settings file properties" +
                        "\r\n  To edit a settings file:\r\n    NERAnnotator <settingsFile>", "NER Annotator", error);
                    return error ? -1 : 0;
                }

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                Application.EnableVisualStyles();

                if (args.Length > 0)
                {
                    string action = null;
                    string settingsFile = null;
                    List<(PropertyInfo property, object value)> propertiesToSet = new List<(PropertyInfo, object)>();
                    Type settingsType = typeof(Settings);
                    for (int argNum = 0; argNum < args.Length; argNum++)
                    {
                        var arg = args[argNum];

                        if (arg.StartsWith("--", StringComparison.Ordinal))
                        {
                            var val = arg.Substring(2);
                            if (settingsType.TryGetProperty(val, true, out var prop))
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
                                var ue = new ExtractException("ELI45054", "Unrecognized property");
                                ue.AddDebugData("Property", val, false);
                                ue.Log();
                                return usage(error: true);
                            }
                        }
                        else if (arg.StartsWith("-", StringComparison.Ordinal)
                            || arg.StartsWith("/", StringComparison.Ordinal))
                        {
                            var val = arg.Substring(1);
                            if (val.Equals("s", StringComparison.OrdinalIgnoreCase))
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
                            else if (val.Equals("CancelTokenName", StringComparison.OrdinalIgnoreCase))
                            {
                                silent = true;

                                if (++argNum < args.Length)
                                {
                                    cancelTokenName = args[argNum];
                                    continue;
                                }
                                else
                                {
                                    return usage(error: true);
                                }
                            }
                            else
                            {
                                action = val;
                            }
                        }
                        else if (settingsFile == null)
                        {
                            settingsFile = arg;
                        }
                        else
                        {
                            return usage(error: true);
                        }
                    }

                    if (string.Equals(action, "h", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(action, "?", StringComparison.OrdinalIgnoreCase))
                    {
                        return usage(error: false);
                    }
                    else if (string.Equals(action, "p", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(settingsFile))
                    {
                        var settings = Settings.LoadFrom(settingsFile);
                        foreach(var (property, value) in propertiesToSet)
                        {
                            property.SetValue(settings, value);
                        }
                        if (silent)
                        {
                            NamedTokenSource namedTokenSource = null;
                            CancellationToken cancelToken = CancellationToken.None;
                            if (!string.IsNullOrWhiteSpace(cancelTokenName))
                            {
                                namedTokenSource = new NamedTokenSource(cancelTokenName);
                                cancelToken = namedTokenSource.Token;
                            }
                            NERAnnotator.Process(settings, _ => { }, cancelToken);
                        }
                        else
                        {
                            using (var win = new AnnotationStatus(settings))
                                win.ShowDialog();
                        }
                    }
                    else if (action == null && !string.IsNullOrEmpty(settingsFile))
                    {
                        Application.Run(new AnnotationConfigurationDialog(settingsFile));
                    }
                    else
                    {
                        return usage(error: true);
                    }
                }
                else
                {
                    Application.Run(new AnnotationConfigurationDialog());
                }

                return 0;
            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI44900");

                if (saveErrors && uexName != null)
                {
                    try
                    {
                        ue.Log(uexName);
                    }
                    catch { }
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