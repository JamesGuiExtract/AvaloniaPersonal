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
            /// Specifies whether selected exceptions should be output.
            /// </summary>
            public bool OutputSelectedExceptions;

            /// <summary>
            /// Specifies whether unreferenced exceptions should be output.
            /// </summary>
            public bool OutputUnreferencedExceptions;

            /// <summary>
            /// The uex file that should be written to if selected exceptions are to be written to
            /// disk.
            /// </summary>
            public string SelectedExceptionOutputFile;

            /// <summary>
            /// The uex file that should be written to if unreferenced exceptions are to be written
            /// to disk.
            /// </summary>
            public string UnreferencedExceptionOutputFile;

            /// <summary>
            /// Specifies whether only unique values should be output.
            /// </summary>
            public bool OutputOnlyUniqueValues;

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
                    else if (arg.Equals("/q", StringComparison.OrdinalIgnoreCase))
                    {
                        OutputOnlyUniqueValues = true;
                    }
                    else if (arg.Equals("/s", StringComparison.OrdinalIgnoreCase))
                    {
                        OutputSelectedExceptions = true;
                    }
                    else if (arg.Equals("/u", StringComparison.OrdinalIgnoreCase))
                    {
                        OutputUnreferencedExceptions = true;
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

                if (OutputSelectedExceptions || OutputSelectedExceptions)
                {
                    // GetFullPathWithoutExtension will get a path relative to the application, not
                    // the working directory-- ensure we get a path relative to the current working
                    // directory.
                    string outputFile = FileSystemMethods.GetAbsolutePath(OutputFile,
                        Environment.CurrentDirectory);
                    outputFile = FileSystemMethods.GetFullPathWithoutExtension(outputFile);

                    if (OutputSelectedExceptions)
                    {
                        SelectedExceptionOutputFile = outputFile + ".Selected.uex";
                    }
                    if (OutputUnreferencedExceptions)
                    {
                        UnreferencedExceptionOutputFile = outputFile + ".Unreferenced.uex";
                    }
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

            // Temporary copy of the source uex file.
            TemporaryFile tempUexFileCopy = null;

            // File to write exceptions that were selected by the query.
            StreamWriter selectedExceptionOutputFile = null;

            // File to write exceptions that were not referenced by the query.
            StreamWriter unreferencedExceptionOutputFile = null;

            try
            {
                // For non-unique results
                List<string> results = null;

                // For unique results. The key is upper case for case-insensitive comparison, the 
                // value is what will actually be written to file.
                Dictionary<string, string> uniqueResults = null;

                if (settings.OutputOnlyUniqueValues)
                {
                    uniqueResults = new Dictionary<string, string>();

                    if (settings.Append && File.Exists(settings.OutputFile))
                    {
                        string[] existingValues = File.ReadAllLines(settings.OutputFile);
                        foreach (string value in existingValues)
                        {
                            // Set value to null to indicate it doesn't need to be re-written to
                            // the output file.
                            uniqueResults[value.ToUpper(CultureInfo.CurrentCulture)] = null;
                        }
                    }
                }
                else
                {
                    results = new List<string>();
                }

                if (settings.OutputSelectedExceptions)
                {
                    selectedExceptionOutputFile = settings.Append
                        ? File.AppendText(settings.SelectedExceptionOutputFile)
                        : File.CreateText(settings.SelectedExceptionOutputFile);
                }

                if (settings.OutputUnreferencedExceptions)
                {
                    unreferencedExceptionOutputFile = settings.Append
                        ? File.AppendText(settings.UnreferencedExceptionOutputFile)
                        : File.CreateText(settings.UnreferencedExceptionOutputFile);
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
                    if (settings.OutputUnreferencedExceptions)
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
                            if (settings.OutputOnlyUniqueValues)
                            {
                                foreach (string value in tempResults)
                                {
                                    // Check case-insensitively for value alreay in results.
                                    string upperValue = value.ToUpper(CultureInfo.CurrentCulture);
                                    if (!uniqueResults.ContainsKey(upperValue))
                                    {
                                        uniqueResults[upperValue] = value;
                                        resultCount++;
                                    }
                                }
                            }
                            else
                            {
                                results.AddRange(tempResults);
                                resultCount += dataItemCount;
                            }


                            Console.Write("\rFound " + resultCount.ToString(CultureInfo.CurrentCulture)
                                + " items...");
                        }

                        // Write the exception out as a selected exception if applicable.
                        if (settings.OutputSelectedExceptions && dataItemCount > 0)
                        {
                            selectedExceptionOutputFile.WriteLine(exceptionFileLines[lineNumber]);
                        }

                        // If writing "unreferenced" exceptions, write out any exception where both
                        // the following conditions are true:
                        // (1)  No debug data item contained within the original UEX line (which
                        //      could represent a hierarchy of exceptions when displayed) was
                        //      exported to the output file.
                        // (2)  Either the top level exception associated with the original UEX line
                        //      or one of the inner exceptions associated with the original UEX line
                        //      was not excluded according to the specs.
                        if (settings.OutputUnreferencedExceptions &&
                            dataItemCount == 0 && !query.GetIsEntirelyExcluded(ee))
                        {
                            unreferencedExceptionOutputFile.WriteLine(exceptionFileLines[lineNumber]);
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

                using (StreamWriter outputFile = settings.Append
                    ? File.AppendText(settings.OutputFile)
                    : File.CreateText(settings.OutputFile))
                {
                    if (settings.OutputOnlyUniqueValues)
                    {
                        foreach (string value in uniqueResults.Values)
                        {
                            // Only output new values found, not any values that previously existed
                            // in the output file.
                            if (!string.IsNullOrEmpty(value))
                            {
                                outputFile.WriteLine(value);
                            }
                        }
                    }
                    else
                    {
                        foreach (string value in results)
                        {
                            outputFile.WriteLine(value);
                        }
                    }
                }

                Console.WriteLine("Complete.");
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI30588", ex);
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (tempUexFileCopy != null)
                {
                    tempUexFileCopy.Dispose();
                }

                if (selectedExceptionOutputFile != null)
                {
                    selectedExceptionOutputFile.Dispose();

                    // If the file is zero size, delete it since empty uex files are not valid.
                    try
                    {
                        FileInfo fileInfo = new FileInfo(settings.SelectedExceptionOutputFile);
                        if (fileInfo.Length == 0)
                        {
                            fileInfo.Delete();
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtractException.Log("ELI30611", ex);
                        Console.WriteLine(ex.Message);
                    }
                }

                if (unreferencedExceptionOutputFile != null)
                {
                    unreferencedExceptionOutputFile.Dispose();

                    // If the file is zero size, delete it since empty uex files are not valid.
                    try
                    {
                        FileInfo fileInfo = new FileInfo(settings.UnreferencedExceptionOutputFile);
                        if (fileInfo.Length == 0)
                        {
                            fileInfo.Delete();
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtractException.Log("ELI30612", ex);
                        Console.WriteLine(ex.Message);
                    }
                }
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
            Console.WriteLine("OutputFileName [/a] [/q] [/s] [/u]");
            Console.WriteLine();
            Console.WriteLine("UEXFile: The UEX file from which to extract data");
            Console.WriteLine();
            Console.WriteLine("FolderName: A folder containing the UEX files from which to extract data.");
            Console.WriteLine();
            Console.WriteLine("/r: Process folder recursively. Only valid when FolderName is specified");
            Console.WriteLine();
            Console.WriteLine("/q: Only output unique values. If used in conjunction with /a, values ");
            Console.WriteLine("that already exist in the specified output file will not be written.");
            Console.WriteLine();
            Console.WriteLine("/s: Outputs to [OutputFileName].Selected.uex exceptions which either");
            Console.WriteLine("matches the specification or which contains an inner exception that");
            Console.WriteLine("matches the specification.");
            Console.WriteLine();
            Console.WriteLine("/u: Outputs to [OutputFileName].Unreferenced.uex exceptions in which ");
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
