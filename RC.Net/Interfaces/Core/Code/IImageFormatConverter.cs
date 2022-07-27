using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Extract.Interfaces
{
    /// <summary>
    /// Supported output file types of <see cref="IImageFormatConverter"/>
    /// </summary>
    [ComVisible(true)]
    [Guid("54C86AE5-A269-48E4-8DF4-426A97CC97C9")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public enum ImageFormatConverterFileType
    {
        kFileType_None,
        kFileType_Tif,
        kFileType_Pdf,
        kFileType_Jpg
    }

    /// <summary>
    /// Copy of IMF_FORMAT in KernelAPI.h
    /// </summary>
    [ComVisible(true)]
    [Guid("AFAB8280-1494-4D97-B1E8-0509AF095819")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    [SuppressMessage("Design", "CA1027:Mark enums with FlagsAttribute")]
    public enum ImageFormatConverterNuanceFormat
    {
        FF_TIFNO =  0,      /*!< Uncompressed TIFF image format.*/
        FF_TIFPB,           /*!< Packbits TIFF image format.*/
        FF_TIFHU,           /*!< Group 3 Modified TIFF image format.*/
        FF_TIFG31,          /*!< Standard G3 1D TIFF image format.*/
        FF_TIFG32,          /*!< Standard G3 2D TIFF image format.*/
        FF_TIFG4,           /*!< Standard G4 TIFF image format.*/
        FF_TIFLZW,          /*!< TIFF-LZW image format incorporating Unisys compression.*/
        FF_PCX,             /*!< PCX format.*/
        FF_DCX,             /*!< DCX format.*/
        FF_SIM,             /*!< Simple file format. \SupponwWLlm */
        FF_BMP_NO,          /*!< Windows bitmap format without compression.*/
        FF_BMP_RLE8,        /*!< Windows bitmap RLE8 format.*/
        FF_BMP_RLE4,        /*!< Windows bitmap RLE4 format. Only for reading! */
        FF_AWD,             /*!< Not supported!*/
        FF_JPG_SUPERB,      /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). JPEG format with negligible information loss.*/
        FF_JPG_LOSSLESS = FF_JPG_SUPERB,    /*!< Deprecated, same as \ref FF_JPG_SUPERB.*/
        FF_JPG_GOOD,        /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). JPEG format with average information loss.
                            (Results in medium-size image files when saving.)*/
        FF_JPG_MIN,         /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). JPEG format optimized for minimum image file size. Worst image quality.*/
        FF_PDA,             /*!< Caere's proprietary compound document format.
                            Only for reading! Only images with a single zone are supported!*/
        FF_PNG,             /*!< Portable Image format for Network Graphics.*/
        FF_XIFF,            /*!< TIFF-FX format. Only for reading! \Supponw */
        FF_GIF,             /*!< GIF image format incorporating Unisys compression.*/
        FF_MAX_LOSSLESS,    /*!< PaperPort MAX file format without information loss. Only for reading! \Supponw */
        FF_MAX_GOOD,        /*!< PaperPort MAX file format with average information loss. Only for reading! \Supponw */
        FF_MAX_MIN,         /*!< PaperPort MAX file format optimized for minimum image file size. Only for reading! \Supponw */
        FF_PGX,             /*!< Only for internal use! */
        FF_PDF_MIN,         /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). Adobe PDF format. Minimum image file size. \SupponwWLl*/
        FF_PDF_GOOD,        /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). Adobe PDF format. Results in medium-size image files when saving. \SupponwWLl*/
        FF_PDF_SUPERB,      /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). Adobe PDF format with negligible information loss. \SupponwWLl*/
        FF_PDF_LOSSLESS = FF_PDF_SUPERB,    /*!< Deprecated, use \ref FF_PDF_SUPERB instead. */
        FF_PDF_MRC_MIN,     /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). Adobe PDF format with MRC technology. Optimized for minimum image file size. \SupponwWLl */
        FF_PDF_MRC_GOOD,    /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). Adobe PDF format with MRC technology. (Results in medium-size image files when saving.) \SupponwWLl */
        FF_PDF_MRC_SUPERB,  /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). Adobe PDF format with MRC technology. PDF with small information loss. \SupponwWLl */
        FF_PDF_MRC_LOSSLESS = FF_PDF_MRC_SUPERB,    /*!< Deprecated, use \ref FF_PDF_MRC_SUPERB instead. */
        FF_TIFJPGNEW,       /*!< New JPG Compressed TIFF image format.*/
        FF_JPG2K_SUPERB,    /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). JPEG2000 file format. Negligible information loss.*/
        FF_JPG2K_LOSSLESS = FF_JPG2K_SUPERB,    /*!< Deprecated, same as \ref FF_JPG2K_SUPERB.*/
        FF_JPG2K_GOOD,      /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). JPEG2000 file format. Results in medium-size image files when saving. */
        FF_JPG2K_MIN,       /*!< Deprecated (see usage of \ref MODIMF_NEWFORMATS "new formats"). JPEG2000 file format. Optimized for minimum image file size. */
        FF_JBIG2_LOSSLESS,  /*!< JBIG2 file format. Lossless image saving. */
        FF_JBIG2_LOSSY,     /*!< JBIG2 file format. Optimized for minimum image file size. */
        FF_XPS,             /*!< XPS file format. \SupponwW */
        FF_WMP,             /*!< Microsoft HD Photo (Windows Media Photo). \SupponwW */
        FF_JBIG,            /*!< JBIG format. Only for reading! \SupponwW */
        FF_OPG,             /*!< "OmniPageCSDK" \ref HPAGE serialization format. One-page format, not appendable.
                            OPG file contains all members of the \c HPAGE including images, zones, letters and other internal information stored by \c HPAGE.
                            Binary images, not image-only PDF images and palette-color images are saved using lossless compression,
                            true-color and gray-scale images are saved using lossy compression.
                            When OPG is being saved \ref kRecSaveImg, \ref kRecSaveImgF and \ref kRecSaveImgForceF do not use their \c iiImg parameter,
                            \ref kRecSaveImgArea and \ref kRecSaveImgAreaF do not use their \c iiImg and \c pRect parameters.
                            Loading an OPG file restores the saved \c HPAGE. \ref IMG_CONVERSION "Primary image conversion" is not applied when OPG file is loaded. */
        FF_PDF,             /*!< Adobe PDF format with changeable compression level (see usage of \ref MODIMF_NEWFORMATS "new formats"). \SupponwWLl */
        FF_PDF_MRC,         /*!< Adobe PDF format using MRC technology with changeable compression level (see usage of \ref MODIMF_NEWFORMATS "new formats"). \SupponwWLl */
        FF_JPG,             /*!< JPEG format with changeable compression level (see usage of \ref MODIMF_NEWFORMATS "new formats"). \SupponwWLl */
        FF_JPG2K,           /*!< JPEG2000 file format with changeable compression level (see usage of \ref MODIMF_NEWFORMATS "new formats"). \SupponwWLl */
        FF_TIF,             /*!< TIF file format with changeable compression and compression level (see usage of \ref MODIMF_NEWFORMATS "new formats"). \SupponwWLl */
        FF_SIZE             /*!< Number of supported image file formats.*/
    }

    /// <summary>
    /// Interface for Nuance image conversion methods
    /// </summary>
    [ComVisible(true)]
    [Guid("DC0D16F5-F974-4B81-8FDE-2DAE10361AD9")]
    public interface IImageFormatConverter
    {
        /// <summary>
        /// Convert an image/PDF file with options to exclude specific pages
        /// </summary>
        /// <param name="inputFileName">Input file path</param>
        /// <param name="outputFileName">Output file path for the converted pages</param>
        /// <param name="outputType">Type for the output file</param>
        /// <param name="preserveColor">Whether to preserve color or not</param>
        /// <param name="pagesToRemove">Comma separated list of page numbers or page ranges to exclude</param>
        /// <param name="explicitFormat">The format used for the output (see Nuance docs for more info)</param>
        /// <param name="compressionLevel">Level of compression to use (see Nuance docs for more info)</param>
        void ConvertImage(
            string inputFileName,
            string outputFileName,
            ImageFormatConverterFileType outputType,
            bool preserveColor,
            string pagesToRemove,
            ImageFormatConverterNuanceFormat explicitFormat,
            int compressionLevel);

        /// <summary>
        /// Convert a single page of an image/PDF file
        /// </summary>
        /// <param name="inputFileName">Input file path</param>
        /// <param name="outputFileName">Output file path for the converted page</param>
        /// <param name="outputType">Type for the output file</param>
        /// <param name="preserveColor">Whether to preserve color or not</param>
        /// <param name="page">The page number to convert</param>
        /// <param name="explicitFormat">The format used for the output (see Nuance docs for more info)</param>
        /// <param name="compressionLevel">Level of compression to use (see Nuance docs for more info)</param>
        void ConvertImagePage(
            string inputFileName,
            string outputFileName,
            ImageFormatConverterFileType outputType,
            bool preserveColor,
            int page,
            ImageFormatConverterNuanceFormat explicitFormat,
            int compressionLevel);
    }
}
