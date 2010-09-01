using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace Extract.UEXExtractor
{
    class UEXExtractorMain
    {
        /// <summary>
        /// The settings that dictate how to extract the exception data
        /// </summary>
        class Settings
        {
            /// <summary>
            /// The set of UEX files specified by the input filename or directory name.
            /// </summary>
            public List<string> UexFiles = new List<string>();

            /// <summary>
            /// Specifies whether to process sub-directories if a directory is specified.
            /// </summary>
            public bool Recursive;

            /// <summary>
            /// Specifies whether results should be appended to an existing output file rather
            /// than replacing an existing file.
            /// </summary>
            public bool Append;

            /// <summary>
            /// Specifies the file containing the query specification.
            /// </summary>
            public string SpecificationFile;

            /// <summary>
            /// The file the results should be written to.
            /// </summary>
            public string OutputFile;

            /// <summary>
            /// Initializes a new <see cref="Settings"/> instance.
            /// </summary>
            /// <param name="args">The command-line arguments the application was launched with.
            /// </param>
            public Settings(string[] args)
            {
                ExtractException.Assert("ELI30581", "Missing required argument.", args.Length >= 3);

                string uexPath = null;

                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];

                    if (arg.Equals("/r", StringComparison.OrdinalIgnoreCase))
                    {
                        Recursive = true;
                    }
                    else if (arg.Equals("/a", StringComparison.OrdinalIgnoreCase))
                    {
                        Append = true;
                    }
                    else if (string.IsNullOrEmpty(uexPath))
                    {
                        uexPath = arg;
                    }
                    else if (string.IsNullOrEmpty(SpecificationFile))
                    {
                        SpecificationFile = arg;
                    }
                    else if (string.IsNullOrEmpty(OutputFile))
                    {
                        OutputFile = arg;
                    }
                    else
                    {
                        throw new ExtractException("ELI30590", "Invalid parameter.");
                    }
                }

                ExtractException.Assert("ELI30585", "Invalid specifications file.",
                    File.Exists(SpecificationFile));
                ExtractException.Assert("ELI30585", "Output filename must be specified.",
                    !string.IsNullOrEmpty(OutputFile));

                FindUexFiles(uexPath);
                ExtractException.Assert("ELI30582", 
                    "No UEX files were found at the specified path.", UexFiles.Count > 0);
            }

            /// <summary>
            /// Compiles a list of UEX files to process based on the provided path.
            /// </summary>
            void FindUexFiles(string path)
            {
                if (File.Exists(path))
                {
                    ExtractException.Assert("ELI30583",
                        "The recursive parameter is valid only for directories.", !Recursive);

                    UexFiles.Add(path);
                }
                else if (Directory.Exists(path))
                {
                    UexFiles.AddRange(Directory.GetFiles(path, "*.uex",
                        Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                }
                else
                {
                    ExtractException ee = new ExtractException("ELI30584", "Invalid UEX file path.");
                    ee.AddDebugData("Path", path, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Extracts data from one or more UEX files.
        /// </summary>
        static void Main(string[] args)
        {
            Settings settings = null;
            ExtractExceptionQuery query = null;

            try
            {
                Console.WriteLine();

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30589", "UEXExtractor");

                // Attempt to load the settings from the command-line argument.  If unsuccessful, 
                // log the problem, print usage, then return.
                settings = new Settings(args);

                // Check to see if the user is looking for usage information.
                if (args.Length >= 1 && (args[0].Equals("/?") || args[0].Equals("-?")))
                {
                    PrintUsage();
                    return;
                }

                // Initialize the query that will be used to process the exceptions.
                string[] specifications = File.ReadAllLines(settings.SpecificationFile);
                query = new ExtractExceptionQuery(specifications);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI30586", ex);
                Console.WriteLine(ex.Message);
                PrintUsage();
                return;
            }

            try
            {
                // If appending to the results of an existing file, first load the existing results.
                List<string> results = new List<string>();
                if (settings.Append && File.Exists(settings.OutputFile))
                {
                    results.AddRange(File.ReadAllLines(settings.OutputFile));
                }

                // Process each UEX file.
                int fileIndex = 1;
                string totalCount = settings.UexFiles.Count.ToString(CultureInfo.CurrentCulture);
                foreach (string uexFilePath in settings.UexFiles)
                {
                    // Display a status message.
                    StringBuilder statusMessage = new StringBuilder("Processing file ");
                    statusMessage.Append((fileIndex++).ToString(CultureInfo.CurrentCulture));
                    statusMessage.Append(" of ");
                    statusMessage.Append(totalCount);
                    statusMessage.Append(": \"");
                    statusMessage.Append(uexFilePath);
                    statusMessage.Append("\"");
                    Console.WriteLine(statusMessage);

                    // Iterate through all exceptions in the file, collecting the query results
                    // along the way.
                    int resultCount = 0;
                    IEnumerable<ExtractException> fileExceptions =
                        ExtractException.LoadAllFromFile("ELI30580", uexFilePath);
                    Console.Write("\rFound 0 items...");
                    foreach (ExtractException ee in fileExceptions)
                    {
                        ReadOnlyCollection<string> tempResults = query.GetResults(ee);
                        if (tempResults.Count > 0)
                        {
                            results.AddRange(tempResults);
                            resultCount += tempResults.Count;
                            Console.Write("\rFound " + resultCount.ToString(CultureInfo.CurrentCulture)
                                + " items...");
                        }
                    }

                    Console.WriteLine();
                }

                File.WriteAllLines(settings.OutputFile, results.ToArray());
                Console.WriteLine("Complete.");
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI30588", ex);
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Prints usage information for UEXExctractor.
        /// </summary>
        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("------------");
            Console.WriteLine("ExtractDebugData.exe {UEXFile|FolderName} [/r] ExtractSpecsFileName");
            Console.WriteLine("OutputFileName [/a]");
            Console.WriteLine();
            Console.WriteLine("UEXFile: The UEX file from which to extract data");
            Console.WriteLine();
            Console.WriteLine("FolderName: A folder containing the UEX files from which to extract data.");
            Console.WriteLine();
            Console.WriteLine("/r: Process folder recursively. Only valid when FolderName is specified");
            Console.WriteLine();
            Console.WriteLine("ExtractSpecsFileName: The file containing the query specification.");
            Console.WriteLine("Each comma delimited line has 5 parameters:");
            Console.WriteLine("Parameter 1: \"I\" for include, or \"E\" for exclude.");
            Console.WriteLine("Parameter 2: \"T\" for top level exception, \"I\" for inner exception,");
            Console.WriteLine("blank for either type of exception.");
            Console.WriteLine("Parameter 3: Regex that should match the ELI code, or blank for any ELI code.");
            Console.WriteLine("Parameter 4: Regex that should match the exception's text, or blank to match ");
            Console.WriteLine("any exception's text.");
            Console.WriteLine("Parameter 5: Name of debug data item whose value should be extracted, or blank");
            Console.WriteLine("to match any debug data item for an inclusion, or nothing for an exclusion.");
            Console.WriteLine();
            Console.WriteLine("OutputFileName: The name of the file to which the results should be written.");
            Console.WriteLine();
            Console.WriteLine("/a: Specifies that the results should be appended to the output file if it");
            Console.WriteLine("already exists rather than overwriting the existing file.");
        }
    }
}
