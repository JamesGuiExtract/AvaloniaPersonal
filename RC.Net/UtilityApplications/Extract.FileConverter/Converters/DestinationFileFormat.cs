using System;

namespace Extract.FileConverter
{
    public enum DestinationFileFormat
    {
        Pdf = 0,
        Tif = 1,
    }

    public static class ExtensionMethods
    {
        public static string EnumValue(this DestinationFileFormat destinationFileFormat)
        {
            try
            {
                switch (destinationFileFormat)
                {
                    case DestinationFileFormat.Pdf:
                        return "pdf";
                    case DestinationFileFormat.Tif:
                        return "tif";
                }
                throw new ExtractException("ELI51714", $"The destination format:{destinationFileFormat.AsString()} does not have an enum value conversion.");
            }
            catch (Exception ee)
            {
                throw ee.AsExtract("ELI51719");
            }
        }
    }

}
