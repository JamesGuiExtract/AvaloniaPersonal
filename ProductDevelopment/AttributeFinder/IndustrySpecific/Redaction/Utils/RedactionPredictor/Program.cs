using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace RedactionPredictor
{
    public partial class Templates
    {
        static int Main(string[] argv)
        {
            bool saveErrors = false;
            string uexName = null;
            int usage(bool error = false)
            {
                var message = "Usage:\r\n" +
                "  To create a template:\r\n" +
                "    RedactionPredictor\r\n" +
                "        <--create-template|-c|/c>\r\n" +
                "        <imagePath>\r\n" +
                "        <pageNum>\r\n" +
                "        <voaPath>\r\n" +
                "        <templateLibrary>\r\n" +
                "        [/ef <exceptionFile>]\r\n\r\n" +
                "  To apply a template/templates:\r\n" +
                "    RedactionPredictor\r\n" +
                "        <--apply-template|-a|/a>\r\n" +
                "        <templateLibrary>\r\n" +
                "        <imagePath>\r\n" +
                "        <outputVoaPath>\r\n" +
                "        [--pages <pageRange>]\r\n" +
                "        [--classifyAttributes]\r\n" +
                "        [--outputAllFormFields]\r\n" +
                "        [/ef <exceptionFile>]";

                if (saveErrors)
                {
                    var ue = new ExtractException("ELI45278", error ? "Bad arguments" : "RedactionPredictor usage");
                    ue.AddDebugData("Usage", message, true);
                    throw ue;
                }
                UtilityMethods.ShowMessageBox(message, "RedactionPredictor Usage", error);

                return error ? -1 : 0;
            }

            try
            {
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                // Check for optional args
                var args = new List<string>(argv.Length);
                string pageRange = null;
                bool classifyAttributes = false;
                bool outputAllFormFields = false;
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
                        else if (val.Equals("-pages"))
                        {
                            if (++argNum < argv.Length)
                            {
                                pageRange = argv[argNum];
                                continue;
                            }
                            else
                            {
                                return usage(error: true);
                            }
                        }
                        else if (val.Equals("-classifyAttributes"))
                        {
                            classifyAttributes = true;
                            continue;
                        }
                        else if (val.Equals("-outputAllFormFields"))
                        {
                            outputAllFormFields = true;
                            continue;
                        }
                    }

                    args.Add(arg);
                }

                // Process standard args
                if (args.Count < 4)
                {
                    Console.Error.WriteLine("Not enough args");
                    return usage(error: true);
                }
                var task = args[0].ToLowerInvariant();

                switch (task)
                {
                    case "--create-template":
                    case "-c":
                    case "/c":
                        if (args.Count == 5 && int.TryParse(args[2], out int pageNum))
                        {
                            var imagePath = Path.GetFullPath(args[1]);
                            var voaPath = Path.GetFullPath(args[3]);
                            var templateLibrary = Path.GetFullPath(args[4]);
                            CreateTemplate(imagePath, pageNum, voaPath, templateLibrary);
                            break;
                        }
                        else
                        {
                            Console.Error.WriteLine("Incorrect args");
                            return usage(error: true);
                        }
                    case "--apply-template":
                    case "-a":
                    case "/a":
                        {
                            if (args.Count == 4)
                            {
                                var templateLibrary = Path.GetFullPath(args[1]);
                                var imagePath = Path.GetFullPath(args[2]);
                                var voaPath = Path.GetFullPath(args[3]);
                                ApplyTemplate(templateLibrary, imagePath, pageRange, voaPath, classifyAttributes, outputAllFormFields);
                                break;
                            }
                            else
                            {
                                Console.Error.WriteLine("Incorrect args");
                                return usage(error: true);
                            }
                        }
                    default:
                        Console.Error.WriteLine("Unrecognized task");
                        return usage(error: true);
                }
            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI50045");

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
