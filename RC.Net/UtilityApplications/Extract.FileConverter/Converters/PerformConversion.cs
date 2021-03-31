using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.FileConverter.Converters
{
    public static class PerformConversion
    {
        /// <summary>
        /// Pass this an array of converters. It will try executing them in order for the given input file.
        /// </summary>
        /// <param name="converters"></param>
        public static void Convert(IConverter[] converters, string inputFile, FileFormat destinationFileFormat)
        {
            bool conversionSuccessful = false;
            try
            {
                foreach(var converter in converters)
                {
                    try
                    {
                        converter.Convert(inputFile, destinationFileFormat);
                        conversionSuccessful = true;
                    }
                    catch(Exception ex)
                    {
                        // Simply log the error we want to try each converter.
                        ex.ExtractLog("ELI51668");
                    }
                    if(conversionSuccessful)
                    {
                        break;
                    }
                }

                if(!conversionSuccessful)
                {
                    var extractException = new ExtractException("ELI51669", "Failed to convert the given file to the destination format. Please try adding more converters.");
                    extractException.AddDebugData("File Name", inputFile);
                    extractException.AddDebugData("Destination Format", destinationFileFormat.AsString());
                    throw extractException;
                }
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI51667");
            }
        }
    }
}
