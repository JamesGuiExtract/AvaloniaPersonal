using System;
using System.Linq;

namespace Extract.FileConverter
{
    public static class PerformConversion
    {
        /// <summary>
        /// Pass this an array of converters. It will try executing them in order for the given input file.
        /// </summary>
        /// <param name="converters"></param>
        public static void Convert(IConverter[] converters, string inputFile, DestinationFileFormat destinationFileFormat)
        {
            bool conversionSuccessful = false;
            try
            {
                foreach (IConverter converter in converters.Where(m => m.IsEnabled))
                {
                    try
                    {
                        converter.Convert(inputFile, destinationFileFormat);
                        conversionSuccessful = true;
                    }
                    catch (Exception ex)
                    {
                        // Simply log the error we want to try each converter.
                        ex.ExtractLog("ELI51668");
                    }
                    if (conversionSuccessful)
                    {
                        break;
                    }
                }

                if (!conversionSuccessful)
                {
                    ExtractException extractException = new ExtractException("ELI51669", "Failed to convert the given file to the destination format. Please try adding more converters, or check the extract exception log for addtional details.");
                    extractException.AddDebugData("File Name", inputFile);
                    extractException.AddDebugData("Destination Format", destinationFileFormat.ToString());
                    throw extractException;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51667");
            }
        }
    }
}
