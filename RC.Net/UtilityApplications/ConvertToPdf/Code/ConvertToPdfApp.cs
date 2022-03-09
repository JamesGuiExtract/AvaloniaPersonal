using CommandLine;
using System;
using Extract.FileConverter.ConvertToPdf;
using System.IO;
using System.Reactive;
using Extract.FileConverter;
using Extract.Licensing;

namespace Extract.Utilities.FileConverter
{
    /// <summary>
    /// CLI parameters
    /// </summary>
    public class Options
    {
        [Option('i', "input-file", Required = true, HelpText = "Source file to be converted")]
        public string InputFile { get; set; }

        [Option('o', "output-file", Required = true, HelpText = "Destination file")]
        public string OutputFile { get; set; }
    }


    /// <summary>
    /// CLI for <see cref="FileToPdfConverter.Convert"/>
    /// </summary>
    public static class ConvertToPdfApp
    {
        // Entry point
        static int Main(string[] args)
        {
            // Parse the parameters and run the program
            var result = CommandLineHandler.HandleCommandLine<Options, Unit>(args, Run);

            // This won't display in a cmd console because this is a windows app but
            // it can be read by the application that started this process
            if (result.ResultType == ResultType.Failure)
            {
                Console.Error.WriteLine(result.ToString());
                return 1;
            }
            else
            {
                Console.WriteLine(result.ToString());
                return 0;
            }
        }

        // Use the parameters parsed by HandleCommandLine
        static Result<Unit> Run(Options opts)
        {
            try
            {
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                var inputFile = FilePathHolder.Create(opts.InputFile);

                using TemporaryFile tempOutputFile = new(".pdf", false);
                var outputFile = new PdfFile(tempOutputFile.FileName);

                if (MimeKitEmailToPdfConverter.CreateDefault().Convert(inputFile, outputFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(opts.OutputFile));
                    File.Copy(tempOutputFile.FileName, opts.OutputFile, true);

                    return Result.CreateSuccess(Unit.Default);
                }
                else
                {
                    ExtractException ex = new("ELI53239", "Could not convert file");
                    ex.AddDebugData("Input file", opts.InputFile);
                    ex.AddDebugData("Output file", opts.OutputFile);

                    return Result.CreateFailure<Unit>(ex);
                }
            }
            catch (Exception ex)
            {
                return Result.CreateFailure<Unit>(ex.AsExtract("ELI53213"));
            }
        }
    }
}
