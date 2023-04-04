using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;
using UCLID_IMAGEUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.GdPicture
{
    [ComVisible(true)]
    [Guid("676C925F-9104-4D9D-A153-1A12D2159CC3")]
    [ProgId("Extract.GdPicture.OCREngine")]
    public class OCREngineCOMClass : IOCREngine
    {
        /// <summary>
        /// Recognizes text on the specified image page. Returns result as a spatial string.
        /// </summary>
        /// <param name="strImageFileName">Path to the image file on which to recognize text</param>
        /// <param name="lStartPage">The first 1-based page number to OCR</param>
        /// <param name="lEndPage">the last 1-based page number to OCR, or -1 to use the last page.
		///	All pages from lStartPage to lEndPage inclusive will be OCRed</param>
        /// <param name="eFilter">unsupported (ignored)</param>
        /// <param name="bstrCustomFilterCharacters">unsupported (ignored)</param>
        /// <param name="eTradeOff">unsupported (ignored)</param>
        /// <param name="bReturnSpatialInfo">unsupported (ignored)</param>
        /// <param name="pProgressStatus">unsupported (ignored)</param>
        /// <param name="pOCRParameters">unsupported (ignored)</param>
        public SpatialString RecognizeTextInImage(
            string strImageFileName,
            int lStartPage,
            int lEndPage,
            EFilterCharacters eFilter, // TODO: add support
            string bstrCustomFilterCharacters, // TODO: add support
            EOcrTradeOff eTradeOff, // TODO: add support
            bool bReturnSpatialInfo, // TODO: add support
            ProgressStatus pProgressStatus, // TODO: add support
            IOCRParameters pOCRParameters) // TODO: add support
        {
            try
            {
                // Handle OCRFileProcessor page range: 1,-1
                // TODO: Support more variations such as 3,-1
                IList<int>? pagesToSearch = null;
                if (lEndPage >= lStartPage)
                {
                    pagesToSearch = Enumerable.Range(lStartPage, lEndPage - lStartPage + 1).ToList();
                }

                return RecognizeText(strImageFileName, pagesToSearch);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI54202");
            }
        }

        /// <summary>
        /// Recognize specified pages of the image and store the text in a SpatialString
        /// </summary>
        /// <param name="strImageFileName">Path to the image file on which to recognize text</param>
        /// <param name="strPageNumbers">a string containing specified page numbers. Valid 
		/// format: single page (eg. 2, 5), a range of pages (eg. 3-9), last X number 
		/// of pages (eg. -3). They are separated by comma(,).</param>
        /// <param name="bReturnSpatialInfo">unsupported (ignored)</param>
        /// <param name="pProgressStatus">unsupported (ignored)</param>
        /// <param name="pOCRParameters">unsupported (ignored)</param>
        /// <returns></returns>
        public SpatialString RecognizeTextInImage2(
            string strImageFileName,
            string strPageNumbers,
            bool bReturnSpatialInfo, // TODO: Add support
            ProgressStatus pProgressStatus, // TODO: Add support
            IOCRParameters pOCRParameters) // TODO: Add support
        {
            try
            {
                IList<int> pagesToSearch = new ImageUtils()
                    .GetImagePageNumbers(strImageFileName, strPageNumbers)
                    .ToIEnumerable<int>()
                    .ToList();

                return RecognizeText(strImageFileName, pagesToSearch);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI54203");
            }
        }

        static SpatialString RecognizeText(string imageFileName, IList<int>? pagesToSearch)
        {
            using TemporaryFile tmpFile = new(".uss", false);
            string ussFile = tmpFile.FileName;
            using DocumentProcessor ocrEngine = new(new OcrPageProcessor());
            ocrEngine.CreateUssFileFromSpecifiedPages(imageFileName, ussFile, pagesToSearch);

            SpatialStringClass spatialString = new();
            spatialString.LoadFrom(ussFile, false);
            spatialString.ReportMemoryUsage();

            return spatialString;
        }

        #region Unsupported Methods

        public bool SupportsTrainingFiles()
        {
            throw new NotImplementedException();
        }

        public void LoadTrainingFile(string strTrainingFileName)
        {
            throw new NotImplementedException();
        }

        public SpatialString RecognizeTextInImageZone(string strImageFileName, int lStartPage, int lEndPage, LongRectangle pZone, int nRotationInDegrees, EFilterCharacters eFilter, string bstrCustomFilterCharacters, bool bDetectHandwriting, bool bReturnUnrecognized, bool bReturnSpatialInfo, ProgressStatus pProgressStatus, IOCRParameters pOCRParameters)
        {
            throw new NotImplementedException();
        }

        public void WhackOCREngine()
        {
            throw new NotImplementedException();
        }

        public void CreateOutputImage(string bstrImageFileName, string bstrFormat, string bstrOutputFileName, IOCRParameters pOCRParameters)
        {
            throw new NotImplementedException();
        }

        public object GetPDFImage(string bstrFileName, int nPage)
        {
            throw new NotImplementedException();
        }

        #endregion Unsupported Methods
    }
}
