using System;
using System.Collections.Generic;
using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System.Linq;
using System.IO;

namespace CreateHtmlFromImage
{
    class CreateHtmlFromImageApp
    {
        static int Main(string[] argv)
        {
            bool saveErrors = false;
            string uexName = null;
            int usage(bool error = false)
            {
                var message = "Usage:\r\n" +
                "  CreateHtmlFromImage\r\n" +
                "      <imagePathOrDir>\r\n" +
                "      [/html4 | /html3 ]\r\n" +
                "      [/params <pathToOCRParametersRuleset>]\r\n" +
                "      [/ef <exceptionFile>]\r\n\r\n";

                if (saveErrors)
                {
                    var ue = new ExtractException("ELI46461", error ? "Bad arguments" : "CreateHtmlFromImage usage");
                    ue.AddDebugData("Usage", message, true);
                    throw ue;
                }
                UtilityMethods.ShowMessageBox(message, "CreateHtmlFromImage Usage", error);

                return error ? -1 : 0;
            }

            try
            {
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                var args = new List<string>(argv.Length);
                bool html40 = true;
                string parametersFile = null;
                for (int argNum = 0; argNum < argv.Length; argNum++)
                {
                    var arg = argv[argNum];
                    if (arg.StartsWith("-", StringComparison.Ordinal)
                        || arg.StartsWith("/", StringComparison.Ordinal))
                    {
                        var val = arg.Substring(1);
                        if (val.Equals("ef", StringComparison.OrdinalIgnoreCase))
                        {
                            saveErrors = true;

                            if (++argNum < argv.Length)
                            {
                                uexName = argv[argNum];
                                continue;
                            }
                            else
                            {
                                return usage(error: true);
                            }
                        }
                        else if (val.StartsWith("html4", StringComparison.OrdinalIgnoreCase))
                        {
                            html40 = true;
                        }
                        else if (val.StartsWith("html3", StringComparison.OrdinalIgnoreCase))
                        {
                            html40 = false;
                        }
                        else if (val.StartsWith("param", StringComparison.OrdinalIgnoreCase))
                        {
                            if (++argNum < argv.Length)
                            {
                                parametersFile = argv[argNum];
                                continue;
                            }
                            else
                            {
                                return usage(error: true);
                            }
                        }
                    }

                    args.Add(arg);
                }

                // Process standard args
                if (args.Count < 1)
                {
                    Console.Error.WriteLine("Not enough args");
                    return usage(error: true);
                }
                var imagePathOrDir = Path.GetFullPath(args[0]);
                if (Directory.Exists(imagePathOrDir))
                {
                    var files = Directory.GetFiles(imagePathOrDir, "*.*", SearchOption.AllDirectories)
                        .Where(p =>
                        {
                            var ext = Path.GetExtension(p).ToLowerInvariant();
                            return ext.EndsWith(".pdf") || ext.EndsWith(".tif");
                        });
                    foreach (var path in files)
                    {
                        CreateHtmlFromImage.Process(path, html40, parametersFile);
                    }
                }
                else if (File.Exists(imagePathOrDir))
                {
                    CreateHtmlFromImage.Process(imagePathOrDir, html40, parametersFile);
                }
                else
                {
                    throw new ArgumentException(UtilityMethods.FormatCurrent($"Dir or file not found: {imagePathOrDir}"));
                }
            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI46462");

                if (saveErrors)
                {
                    ue.Log(uexName);
                }
                else
                {
                    ue.Display();
                }

                return -1;
            }

            return 0;
        }
    }
}
