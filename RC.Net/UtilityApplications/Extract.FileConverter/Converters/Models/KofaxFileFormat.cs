using System;
using System.ComponentModel;

namespace Extract.FileConverter
{
    public enum KofaxFileFormat
    {
        [Description("none")]
        None = 0,
        [Description("tifno")]
        TifNo = 1,
        [Description("tifpb")]
        TifPB = 2,
        [Description("tifhu")]
        TifHU = 3,
        [Description("tifg31")]
        TifG31 = 4,
        [Description("tifg32")]
        TifG32 = 5,
        [Description("tifg4")]
        TifG4 = 6,
        [Description("tiflzw")]
        TifLempelZWelch = 7,
        [Description("pdf")]
        Pdf = 8,
        [Description("pdf mrc")]
        PdfMixedRasterContent = 9,
    }

    public static class MapKofaxFileFormats
    {
        public static string ToImageFormatConverterFormat(KofaxFileFormat kofaxFileFormat)
        {
            try
            {
                return kofaxFileFormat switch
                {
                    KofaxFileFormat.None => "none",
                    KofaxFileFormat.TifNo => "tifno",
                    KofaxFileFormat.TifPB => "tifpb",
                    KofaxFileFormat.TifHU => "tifhu",
                    KofaxFileFormat.TifG31 => "tifg31",
                    KofaxFileFormat.TifG32 => "tifg32",
                    KofaxFileFormat.TifG4 => "tifg4",
                    KofaxFileFormat.TifLempelZWelch => "tiflzw",
                    KofaxFileFormat.Pdf => "pdf",
                    KofaxFileFormat.PdfMixedRasterContent => "pdf_mrc",
                    _ => throw new NotImplementedException()
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51721");
            }
        }
    }
}
