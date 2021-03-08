using Extract.GoogleCloud.Dto;
using GdPicture14;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Drawing.Drawing2D;

namespace Extract.GdPicture
{
    public static class MicrToGcvConverter
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
        /// Get a google-cloud-vision-compatible object graph from the result of running <see cref="GdPictureImaging.MICRDoMICR"/>
        /// </summary>
        /// <param name="api">The <see cref="GdPictureImaging"/> instance that holds the recognition data</param>
        /// <param name="imageID">The ID of the image page to be processed</param>
        /// <param name="pageNumber">The page number to record in the DTO</param>
        /// <param name="orientation">The detected orientation value that was used to correct the image before recognition was performed</param>
        /// <param name="micrResult">The string result of the recognition operation</param>
        /// <returns>A <see cref="Dto.TextAnnotation"/> representing the recognized spatial characters</returns>
        public static Dto.TextAnnotation GetDto(GdPictureImaging api, int imageID, int pageNumber, int orientation, string micrResult)
        {
            _ = api ?? throw new ArgumentNullException(nameof(api));

            if (String.IsNullOrEmpty(micrResult))
            {
                return new Dto.TextAnnotation
                (
                    pages: Array.Empty<Dto.Page>(),
                    text: string.Empty
                );
            }
            else
            {
                var iter = new MicrIterator(api);
                var width = api.GetWidth(imageID);
                var height = api.GetHeight(imageID);
                var origWidth = width;
                var origHeight = height;
                if (orientation % 180 != 0)
                {
                    Utilities.UtilityMethods.Swap(ref origWidth, ref origHeight);
                }

                using var transform = new Matrix();
                transform.Translate(-width / 2, -height / 2, MatrixOrder.Append);
                transform.Rotate(orientation, MatrixOrder.Append);
                transform.Translate(origWidth / 2, origHeight / 2, MatrixOrder.Append);

                var page = GcvCompatibleConverter.GetPage(iter, pageNumber, origWidth, origHeight, transform);

                return new Dto.TextAnnotation
                (
                    pages: new[] { page },
                    text: micrResult
                );
            }
        }
    }
}
