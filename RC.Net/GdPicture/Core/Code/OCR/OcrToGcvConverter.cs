using Extract.GoogleCloud.Dto;
using GdPicture14;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Drawing.Drawing2D;

namespace Extract.GdPicture
{
    public static class OcrToGcvConverter
    {
        /// <summary>
        /// Get serializer that will produce GCV-compatible JSON (string enums, no nulls)
        /// </summary>
        public static JsonSerializer GetJsonSerializer()
        {
            var jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            jsonSerializer.Converters.Add(new StringEnumConverter());
            return jsonSerializer;
        }

        /// <summary>
        /// Get a google-cloud-vision-compatible object graph from the result of running <see cref="GdPictureOCR.RunOCR"/>
        /// </summary>
        /// <param name="documentContext">Information about the current document and utility functions</param>
        /// <param name="imageID">The ID of the image page to be processed</param>
        /// <param name="resultID">The ID of the OCR result</param>
        /// <returns>A <see cref="Dto.TextAnnotation"/> representing the recognized spatial characters</returns>
        public static Dto.TextAnnotation GetDto(
            IDocumentContext documentContext,
            int imageID,
            string resultID)
        {
            _ = documentContext ?? throw new ArgumentNullException(nameof(documentContext));

            GdPictureOCR ocrApi = documentContext.GdPictureUtility.OcrAPI;
            GdPictureImaging imagingApi = documentContext.GdPictureUtility.ImagingAPI;

            string ocrResult = ocrApi.GetOCRResultText(resultID);

            if (String.IsNullOrEmpty(ocrResult))
            {
                return new Dto.TextAnnotation
                (
                    pages: Array.Empty<Dto.Page>(),
                    text: string.Empty
                );
            }
            else
            {
                OcrIterator iter = new(ocrApi, resultID);
                int width = imagingApi.GetWidth(imageID);
                int height = imagingApi.GetHeight(imageID);
                int rotation = ocrApi.GetPageRotation(resultID);
                float ocrSkew = ocrApi.GetPageSkewAngle(resultID);

                using var transform = new Matrix();

                var page = GcvCompatibleConverter.GetPage(
                    iter: iter,
                    pageNumber: documentContext.CurrentPageNumber,
                    originalWidth: width,
                    originalHeight: height,
                    transform: transform,
                    orientation: rotation,
                    ocrSkew: ocrSkew);

                return new Dto.TextAnnotation
                (
                    pages: new[] { page },
                    text: ocrResult
                );
            }
        }
    }
}
