using Extract.GoogleCloud.Dto;
using GdPicture14;
using System;

namespace Extract.GdPicture
{
    public class OcrPageProcessor : IPageProcessor
    {
        /// <inheritdoc/>
        public Dto.TextAnnotation? ProcessPage(int imageID, IDocumentContext documentContext)
        {
            _ = documentContext ?? throw new ArgumentNullException(nameof(documentContext));

            GdPictureOCR ocrApi = documentContext.GdPictureUtility.OcrAPI;

            Dto.TextAnnotation? result = null;

            try
            {
                documentContext.ThrowIfStatusNotOK(ocrApi.SetImage(imageID), "ELI54204", "Unable to set image for OCR");

                ocrApi.EnableOrientationDetection = true;
                ocrApi.EnableSkewDetection = true;
                ocrApi.OCRMode = OCRMode.FavorAccuracy;
                ocrApi.MaxThreadCount = 1;
                string resultID = ocrApi.RunOCR();
                documentContext.ThrowIfStatusNotOK(ocrApi.GetStat(), "ELI54207", "Unable to recognize text on page");

                result = OcrToGcvConverter.GetDto(documentContext, imageID, resultID);
            }
            catch (ExtractException uex) when (ocrApi.GetStat() == GdPictureStatus.GenericError)
            {
                // Skip the page so that the rest of the document can be processed
                new ExtractException("ELI54208", "Unable to OCR page with GdPicture. This page has been skipped.", uex).Log();
            }
            finally
            {
                // Clear OCR data after each page to improve memory usage
                ocrApi.ReleaseOCRResults();
            }

            return result;
        }
    }
}