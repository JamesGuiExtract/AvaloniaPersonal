using Extract.GoogleCloud.Dto;
using GdPicture14;
using System;

namespace Extract.GdPicture
{
    public class MicrPageProcessor : IPageProcessor
    {
        const string _MICR_CHARS = "0123456789ABCD";

        /// <inheritdoc/>
        public Dto.TextAnnotation? ProcessPage(int imageID, IDocumentContext documentContext)
        {
            _ = documentContext ?? throw new ArgumentNullException(nameof(documentContext));

            GdPictureOCR ocrApi = documentContext.GdPictureUtility.OcrAPI;
            GdPictureImaging imagingApi = documentContext.GdPictureUtility.ImagingAPI;

            Dto.TextAnnotation? result = null;
            string stringResult;
            int orientation;

            try
            {
                documentContext.ThrowIfStatusNotOK(ocrApi.SetImage(imageID), "ELI51548", "Unable to set image for orientation detection");

                try
                {
                    orientation = ocrApi.GetOrientation();
                    documentContext.ThrowIfStatusNotOK(ocrApi.GetStat(), "ELI51549", "Unable to get orientation");
                }
                catch (ExtractException uex)
                {
                    orientation = 0;
                    uex.Log();
                }
                finally
                {
                    // Clear OCR data after each page to improve memory usage
                    ocrApi.ReleaseOCRResults();
                }

                if (orientation != 0)
                {
                    documentContext.ThrowIfStatusNotOK(imagingApi.RotateAngle(imageID, 360 - orientation), "ELI51550", "Failed to rotate page");
                }

                stringResult = imagingApi.MICRDoMICR(imageID, MICRFont.MICRFontE13B, MICRContext.MICRContextDocument, _MICR_CHARS, ExpectedSymbols: 0);
                documentContext.ThrowIfStatusNotOK(imagingApi.GetStat(), "ELI51541", "Unable to find MICR on page");

                if (!String.IsNullOrWhiteSpace(stringResult))
                {
                    result = MicrToGcvConverter.GetDto(imagingApi, imageID, documentContext.CurrentPageNumber, orientation, stringResult);
                }
            }
            catch (ExtractException uex) when (imagingApi.GetStat() == GdPictureStatus.GenericError)
            {
                // Skip the page so that the rest of the document can be searched
                // https://extract.atlassian.net/browse/ISSUE-18413
                new ExtractException("ELI53528", "Unable to process page with GdPicture. This page has been skipped.", uex).Log();
            }
            finally
            {
                // Clear MICR data after each page to improve memory usage
                imagingApi.MICRClear();
            }

            return result;
        }
    }
}
