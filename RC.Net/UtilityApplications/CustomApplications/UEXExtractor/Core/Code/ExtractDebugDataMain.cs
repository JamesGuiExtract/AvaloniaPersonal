using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace Extract.ExtractDebugData
{
    class ExtractDebugDataMain
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
            /// Specifies whether unselected exceptions should be output.
            /// </summary>
            public bool OutputUnselectedExceptions;

            /// <summary>
            /// The uex file that should be written to if selected or unselected exceptions are to
            /// be written to disk.
            /// </summary>
            public string ExceptionOutputFile;

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
                    else if (arg.Equals("/u", StringComparison.OrdinalIgnoreCase))
                    {
                        OutputUnselectedExceptions = true;
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

                if (OutputUnselectedExceptions)
                {
                    // GetFullPathWithoutExtension will get a path relative to the application, not
                    // the working directory-- ensure we get a path relative to the current working
                    // directory.
                    string outputFile = FileSystemMethods.GetAbsolutePath(OutputFile,
                        Environment.CurrentDirectory);
                    ExceptionOutputFile = FileSystemMethods.GetFullPathWithoutExtension(outputFile);
                    ExceptionOutputFile += ".Unselected.uex";
                }
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
            TemporaryFile tempUexFileCopy = null;

            try
            {
                Console.WriteLine();

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30589", "ExtractDebugData");

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
                List<string> unselectedExceptions = new List<string>();

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

                    // Don't open directly any exception file named "ExtractException.uex" to
                    // prevent the possibility of access issues with the active exception log.
                    string workingFilePath;
                    if (Path.GetFileName(uexFilePath).Equals(
                        "ExtractException.uex", StringComparison.OrdinalIgnoreCase))
                    {
                        tempUexFileCopy = new TemporaryFile(".uex");
                        File.Copy(uexFilePath, tempUexFileCopy.FileName, true);
                        workingFilePath = tempUexFileCopy.FileName;
                    }
                    else
                    {
                        workingFilePath = uexFilePath;
                    }

                    // Iterate through all exceptions in the file, collecting the query results
                    // along the way.
                    int resultCount = 0;
                    int lineNumber = 0;
                    string[] exceptionFileLines = null;
                    if (settings.OutputUnselectedExceptions)
                    {
                        // For efficiency, if we are going to be writing exceptions out to file,
                        // rather than use the "Log" function on the exception COM object, just
                        // write the corresponding line from the source uex file.
                        exceptionFileLines = File.ReadAllLines(workingFilePath);
                    }

                    IEnumerable<ExtractException> fileExceptions =
                        ExtractException.LoadAllFromFile("ELI30580", workingFilePath, true);
                    Console.Write("\rFound 0 items...");
                    
                    foreach (ExtractException ee in fileExceptions)
                    {
                        if (ee.EliCode.Equals("ELI30603", StringComparison.Ordinal))
                        {
                            Console.Write("\r");
                            Console.WriteLine(ee.Message);
                            Console.Write("\rFound " + resultCount.ToString(CultureInfo.CurrentCulture)
                                + " items...");
                            lineNumber++;
                            continue;
                        }

                        ReadOnlyCollection<string> tempResults = query.GetDebugData(ee);
                        int dataItemCount = tempResults.Count;
                        if (dataItemCount > 0)
                        {
                            results.AddRange(tempResults);
                            resultCount += dataItemCount;
                            Console.Write("\rFound " + resultCount.ToString(CultureInfo.CurrentCulture)
                                + " items...");
                        }

                        // If writing "unselected" exceptions, write out any exception where both
                        // the following conditions are true:
                        // (1)  No debug data item contained within the original UEX line (which
                        //      could represent a hierarchy of exceptions when displayed) was
                        //      exported to the output file.
                        // (2)  Either the top level exception associated with the original UEX line
                        //      or one of the inner exceptions associated with the original UEX line
                        //      was not excluded according to the specs.
                        if (settings.OutputUnselectedExceptions && 
                            dataItemCount == 0 && !query.GetIsEntirelyExcluded(ee))
                        {
                            unselectedExceptions.Add(exceptionFileLines[lineNumber]);
                        }

                        lineNumber++;
                    }

                    if (tempUexFileCopy != null)
                    {
                        tempUexFileCopy.Dispose();
                        tempUexFileCopy = null;
                    }

                    Console.WriteLine();
                }

                if (settings.Append)
                {
                    File.AppendAllLines(settings.OutputFile, results.ToArray());
                    if (settings.OutputUnselectedExceptions)
                    {
                        File.AppendAllLines(settings.ExceptionOutputFile,
                            unselectedExceptions.ToArray());
                    }
                }
                else
                {
                    File.WriteAllLines(settings.OutputFile, results.ToArray());
                    if (settings.OutputUnselectedExceptions)
                    {
                        File.WriteAllLines(settings.ExceptionOutputFile,
                            unselectedExceptions.ToArray());
                    }
                }

                Console.WriteLine("Complete.");
            }
            catch (Exception ex)
            {
                if (tempUexFileCopy != null)
                {
                    tempUexFileCopy.Dispose();
                }

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
            Console.WriteLine("OutputFileName [/a] [/u]");
            Console.WriteLine();
            Console.WriteLine("UEXFile: The UEX file from which to extract data");
            Console.WriteLine();
            Console.WriteLine("FolderName: A folder containing the UEX files from which to extract data.");
            Console.WriteLine();
            Console.WriteLine("/r: Process folder recursively. Only valid when FolderName is specified");
            Console.WriteLine();
            Console.WriteLine("/u: Outputs to [OutputFileName].Unselected.uex exceptions in which ");
            Console.WriteLine("no debug data was selected and either the top-level exception or");
            Console.WriteLine("one of its inner exceptions were not excluded.");
            Console.WriteLine();
            Console.WriteLine("ExtractSpecsFileName: The file containing the query specification.");
            Console.WriteLine("Each comma delimited line has 6 parameters:");
            Console.WriteLine("Parameter 1: \"I\" for include, or \"E\" for exclude.");
            Console.WriteLine("Parameter 2: \"T\" for top level exception, \"I\" for inner exception,");
            Console.WriteLine("blank for either type of exception.");
            Console.WriteLine("Parameter 3: Regex that should match the ELI code, or blank for any ELI code.");
            Console.WriteLine("Parameter 4: Regex that should match the exception's text, or blank to match ");
            Console.WriteLine("any exception's text.");
            Console.WriteLine("Parameter 5: Regex matching the name of any debug data item whose value ");
            Console.WriteLine("should be extracted, or blank to match any debug data item.");
            Console.WriteLine("Parameter 6: Regex matching the value of any debug data item whose value ");
            Console.WriteLine("should be extracted, or blank to match any debug data item.");
            Console.WriteLine();
            Console.WriteLine("OutputFileName: The name of the file to which the results should be written.");
            Console.WriteLine();
            Console.WriteLine("/a: Specifies that the results should be appended to the output file if it");
            Console.WriteLine("already exists rather than overwriting the existing file.");
        }
    }
}
