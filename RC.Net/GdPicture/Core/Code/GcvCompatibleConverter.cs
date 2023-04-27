using Extract.GoogleCloud.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Extract.GdPicture
{
    /// <summary>
    /// TODO: This class doesn't depend on GdPicture and could be put into an OCR-engine-agnostic project
    /// </summary>
    public static class GcvCompatibleConverter
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
        /// Build a <see cref="Dto.Page"/> graph from OCR results using a <see cref="IRecognizedCharactersIterator"/>
        /// </summary>
        /// <param name="iter">An iterator to be used to enumerate the OCR results</param>
        /// <param name="pageNumber">The page number that the results are from</param>
        /// <param name="originalWidth">The width of the original image page (before orientation correction)</param>
        /// <param name="originalHeight">The height of the original image page (before orientation correction)</param>
        /// <param name="transform">A geometric transformation to be applied to the spatial data to convert it to original image coordinates</param>
        /// <param name="orientation">The detected orientation of the OCR result. This will be used to change the order of the points
        /// so that the orientation can be deduced</param>
        /// <param name="ocrSkew">The detected skew of the OCR result. This will be used to rotate the rectangular bounds of, e.g., the
        /// recognized characters about their centers</param>
        public static Dto.Page GetPage(
            IRecognizedCharactersIterator iter,
            int pageNumber,
            int originalWidth,
            int originalHeight,
            Matrix transform,
            int orientation,
            float ocrSkew)
        {
            _ = iter ?? throw new ArgumentNullException(nameof(iter));

            // Normalize orientation to be a positive number
            // https://extract.atlassian.net/browse/ISSUE-19245
            orientation = (orientation + 360) % 360;

            // Confirm that orientation is a multiple of 90
            ExtractException.Assert("ELI54292", "Unexpected orientation value", orientation % 90 == 0);

            return new Dto.Page
            (
                property: null,
                pageNumber: pageNumber,
                width: originalWidth,
                height: originalHeight,
                blocks: GetBlocks(iter, transform, orientation, ocrSkew).ToArray()
            );
        }

        static IEnumerable<Dto.Block> GetBlocks(IRecognizedCharactersIterator iter, Matrix transform, int orientation, float ocrSkew)
        {
            var level = PageIteratorLevel.Block;
            do
            {
                if (iter.IsAtBeginningOf(level) && iter.TryGetBoundingBox(level, out var bounds))
                {
                    Dto.Paragraph[] paragraphs = GetParagraphs(iter, transform, orientation, ocrSkew).ToArray();
                    float confidence = paragraphs.Length == 0 ? 0 : paragraphs.Average(p => p.confidence) / 100;
                    yield return new Dto.Block
                    (
                        property: null,
                        text: null,
                        blockType: Dto.BlockType.FlowingText,
                        boundingBox: GetBounds(bounds, transform, orientation, ocrSkew),
                        confidence: confidence,
                        paragraphs: paragraphs
                    );
                }
            }
            while (iter.Next(level));
        }

        static Dto.BoundingPoly GetBounds(Rect bounds, Matrix transform, int orientation, float ocrSkew)
        {
            var vertices = new Point[]
            {
                new Point(x: bounds.Left, y: bounds.Top),
                new Point(x: bounds.Right, y: bounds.Top),
                new Point(x: bounds.Right, y: bounds.Bottom),
                new Point(x: bounds.Left, y: bounds.Bottom)
            };

            // Rotate the order of the points, a la Google Cloud Vision
            if (orientation != 0)
            {
                int rotateSteps = orientation / 90;

                Point[] rotatedVertices = new Point[4];
                for (int i = 0; i < 4; i++)
                    rotatedVertices[i] = vertices[(i + rotateSteps) % 4];

                vertices = rotatedVertices;
            }

            // Do page-level rotation
            transform.TransformPoints(vertices);

            // Do OCR skew correction
            if (ocrSkew != 0)
            {
                Matrix ocrSkewTransform = new();
                Point centerOfRect = new(
                    x: bounds.Left + (bounds.Width / 2),
                    y: bounds.Top + (bounds.Height / 2));
                ocrSkewTransform.Translate(-centerOfRect.X, -centerOfRect.Y, MatrixOrder.Append);
                ocrSkewTransform.Rotate(ocrSkew, MatrixOrder.Append);
                ocrSkewTransform.Translate(centerOfRect.X, centerOfRect.Y, MatrixOrder.Append);
                ocrSkewTransform.TransformPoints(vertices);
            }

            return new Dto.BoundingPoly
            (
                vertices: vertices.Select(point => new Dto.Vertex(x: point.X, y: point.Y)).ToArray(),
                normalizedVertices: null
            );
        }

        static IEnumerable<Dto.Paragraph> GetParagraphs(IRecognizedCharactersIterator iter, Matrix transform, int orientation, float ocrSkew)
        {
            var prevLevel = PageIteratorLevel.Block;
            var level = PageIteratorLevel.Para;
            do
            {
                if (iter.IsAtBeginningOf(level))
                {
                    Dto.Word[] words = GetWords(iter, transform, orientation, ocrSkew).ToArray();
                    float confidence = words.Length == 0 ? 0 : words.Average(p => p.confidence) / 100;
                    yield return new Dto.Paragraph
                    (
                        property: null,
                        boundingBox: null,
                        confidence: confidence,
                        text: iter.GetText(level),
                        words: words
                    );
                }
            }
            while (iter.Next(prevLevel, level));
        }

        static IEnumerable<Dto.Word> GetWords(IRecognizedCharactersIterator iter, Matrix transform, int orientation, float ocrSkew)
        {
            var prevLevel = PageIteratorLevel.Para;
            var level = PageIteratorLevel.Word;
            do
            {
                if (iter.IsAtBeginningOf(level) && iter.TryGetBoundingBox(level, out var bounds))
                {
                    // Add bounding box if it's valid
                    Dto.BoundingPoly? boundingBox = null;
                    if (Math.Abs(bounds.Width) >= 1 && Math.Abs(bounds.Height) >= 1)
                    {
                        boundingBox = GetBounds(bounds, transform, orientation, ocrSkew);
                    }

                    Dto.Symbol[] symbols = GetSymbols(iter, transform, orientation, ocrSkew).ToArray();
                    float confidence = symbols.Length == 0 ? 0 : symbols.Average(p => p.confidence) / 100;
                    var word = new Dto.Word
                    (
                        property: null,
                        text: null,
                        boundingBox: boundingBox,
                        confidence: confidence,
                        symbols: symbols
                    );

                    yield return word;
                }
            }
            while (iter.Next(prevLevel, level));
        }

        static IEnumerable<Dto.Symbol> GetSymbols(IRecognizedCharactersIterator iter, Matrix transform, int orientation, float ocrSkew)
        {
            var prevLevel = PageIteratorLevel.Word;
            var level = PageIteratorLevel.Symbol;
            do
            {
                if (iter.IsAtBeginningOf(level)
                    && iter.TryGetBoundingBox(level, out var bounds)
                    && !String.IsNullOrWhiteSpace(iter.GetText(level)))
                {
                    // Add bounding box if it's valid
                    Dto.BoundingPoly? boundingBox = null;
                    if (Math.Abs(bounds.Width) >= 1 && Math.Abs(bounds.Height) >= 1)
                    {
                        boundingBox = GetBounds(bounds, transform, orientation, ocrSkew);
                    }

                    Dto.TextProperty? property = null;
                    if (iter.IsAtFinalOf(PageIteratorLevel.Para, level))
                    {
                        property = GetBreak(Dto.BreakType.EOL_SURE_SPACE);
                    }
                    else if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, level))
                    {
                        property = GetBreak(Dto.BreakType.LINE_BREAK);
                    }
                    else if (iter.IsAtFinalOf(PageIteratorLevel.Word, level))
                    {
                        property = GetBreak(Dto.BreakType.SPACE); // TODO: What about SURE_SPACE?
                    }

                    var sym = new Dto.Symbol
                    (
                        property: property,
                        boundingBox: boundingBox,
                        confidence: iter.GetConfidence(level) / 100,
                        text: iter.GetText(level)
                    );

                    yield return sym;
                }
            }
            while (iter.Next(prevLevel, level));
        }

        static Dto.TextProperty? GetBreak(Dto.BreakType breakType)
        {
            return new Dto.TextProperty
            (
                detectedBreak: new Dto.DetectedBreak(type: breakType, isPrefix: null),
                detectedLanguages: null
            );
        }
    }
}
