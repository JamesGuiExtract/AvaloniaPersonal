using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.UtilityApplications.NERAnnotator
{
    public class NERAnnotatorApp
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
                        "\r\n  To open the editor:\r\n    NERAnnotator" +
                        "\r\n  To create a labeled tokens file:\r\n    NERAnnotator <settingsFile> /p [/s] [/ef <exceptionFile>]" +
                        "\r\n    /s = silent = no progress bar or exceptions displayed" +
                        "\r\n    /ef <exceptionFile> log exceptions to file" +
                        "\r\n       (supports propagate errors to FAM option)" +
                        "\r\n       (/ef also implies /s)" +
                        "\r\n  To edit a settings file:\r\n    NERAnnotator <settingsFile>", "NER Annotator", error);
                    return error ? -1 : 0;
                }

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                Application.EnableVisualStyles();

                if (args.Length > 0)
                {
                    string action = null;
                    string settingsFile = null;
                    foreach (var arg in args)
                    {
                        if (saveErrors && uexName == null)
                        {
                            uexName = arg;
                        }
                        else if (action == null && arg.StartsWith("-", StringComparison.Ordinal)
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
                                // /ef implies /s
                                // (else exceptions would be displayed)
                                silent = true;
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
                            usage(error: true);
                        }
                    }

                    if (string.Equals(action, "h", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(action, "?", StringComparison.OrdinalIgnoreCase))
                    {
                        usage(error: false);
                    }
                    else if (string.Equals(action, "p", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(settingsFile))
                    {
                        var settings = Settings.LoadFrom(settingsFile);
                        if (silent)
                        {
                            NERAnnotator.Process(settings, _ => { }, System.Threading.CancellationToken.None);
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
                        usage(error: true);
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