using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.IO;

namespace RedactionPredictor
{
    public partial class Templates
    {
        static int Main(string[] args)
        {
            int usage(bool error = false)
            {
                var message = "Usage:\r\n" +
                "  To create a template:\r\n" +
                "    RedactionPredictor <--create-template|-c|/c> <imagePath> <pageNum> <voaPath> <outputDir>\r\n" +
                "  To apply a template/templates:\r\n" +
                "    RedactionPredictor <--apply-template|-a|/a> <templateDir> <imagePath> <outputVoaPath>";
                UtilityMethods.ShowMessageBox(message, "RedactionPredictor Usage", error);

                return error ? -1 : 0;
            }

            try
            {
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                if (args.Length < 4)
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
                        if (args.Length == 5 && int.TryParse(args[2], out int pageNum))
                        {
                            var imagePath = Path.GetFullPath(args[1]);
                            var voaPath = Path.GetFullPath(args[3]);
                            var outputDir = Path.GetFullPath(args[4]);
                            CreateTemplate(imagePath, pageNum, voaPath, outputDir);
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
                            var templateDir = Path.GetFullPath(args[1]);
                            var imagePath = Path.GetFullPath(args[2]);
                            var voaPath = Path.GetFullPath(args[3]);
                            ApplyTemplate(templateDir, imagePath, voaPath);
                            break;
                        }
                    default:
                        Console.Error.WriteLine("Unrecognized task");
                        return usage(error: true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI44686");
                Console.Error.WriteLine("Error occurred");
                return -1;
            }

            return 0;
        }
    }
}
