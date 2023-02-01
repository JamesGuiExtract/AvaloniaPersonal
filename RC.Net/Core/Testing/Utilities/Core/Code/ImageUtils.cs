using Leadtools.Codecs;
using Leadtools.Drawing;
using Leadtools.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace Extract.Testing.Utilities
{
    public class ImageUtils
    {
        /// <summary>
        /// Compute the normalized sum of square differences summed over the pages
        /// </summary>
        public static double ComparePagesAsImages(IMG originalDoc, IMG newDoc)
        {
            try
            {
                var origPages = originalDoc switch
                {
                    TIF tif => GetPagesAsImages(tif),
                    PDF pdf => GetPagesAsImages(pdf),
                    _ => throw new ExtractException("ELI53966", "Unexpected image type")
                };

                var newPages = newDoc switch
                {
                    TIF tif => GetPagesAsImages(tif),
                    PDF pdf => GetPagesAsImages(pdf),
                    _ => throw new ExtractException("ELI53967", "Unexpected image type")
                };

                return ComparePagesAsImages(origPages, newPages);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53969");
            }
        }

        [CLSCompliant(false)]
        public static string GetText(PDF pdf)
        {
            var document = pdf.PdfBoxDocument;
            var totalPages = document.getNumberOfPages();

            return GetTextStripper(1, totalPages)
                .getText(document);
        }

        [CLSCompliant(false)]
        public static string GetText(PDF pdf, int pageNumber)
        {
            return GetTextStripper(pageNumber, pageNumber)
                .getText(pdf.PdfBoxDocument);
        }

        static org.apache.pdfbox.text.PDFTextStripper GetTextStripper(int startPage, int endPage)
        {
            var stripper = new org.apache.pdfbox.text.PDFTextStripper();
            stripper.setSortByPosition(true);
            stripper.setStartPage(startPage);
            stripper.setEndPage(endPage);

            return stripper;
        }

        // Normalized sum of square differences summed over the pages
        // NOTE: this method disposes of the input bitmaps
        static double ComparePagesAsImages(IEnumerable<Bitmap> originalPages, IEnumerable<Bitmap> newPages)
        {
            try
            {
                double errors = 0;

                var pageNum = 0;
                foreach (var pair in originalPages.Zip(newPages, (o, n) => (o, n)))
                {
                    pageNum++;

                    byte[] origBytes;
                    byte[] newBytes;

                    // If images aren't the same size, resize to the orig size
                    if (pair.o.Width != pair.n.Width || pair.o.Height != pair.n.Height)
                    {
                        using var origPage = pair.o;
                        using var newPage = pair.n;

                        // Count any differences in size as errors
                        errors += Math.Pow(origPage.Width - newPage.Width, 2);
                        errors += Math.Pow(origPage.Height - newPage.Height, 2);

                        using var resized = ResizeImage(newPage, origPage.Width, origPage.Height);

                        origBytes = GetBytesFromBitmap(origPage);
                        newBytes = GetBytesFromBitmap(resized);
                    }
                    else
                    {
                        using var origPage = pair.o;
                        using var newPage = pair.n;

                        origBytes = GetBytesFromBitmap(origPage);
                        newBytes = GetBytesFromBitmap(newPage);
                    }

                    double pageErrors = 0, origNorm = 0, newNorm = 0;
                    for (int y = 0; y < origBytes.Length; y++)
                    {
                        pageErrors += Math.Pow(origBytes[y] - newBytes[y], 2);
                        origNorm += Math.Pow(origBytes[y], 2);
                        newNorm += Math.Pow(newBytes[y], 2);
                    }
                    var normalizedErrors = pageErrors / (Math.Sqrt(origNorm * newNorm) + 1);
                    errors += normalizedErrors;
                }
                return errors;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53970");
            }
        }

        static byte[] GetBytesFromBitmap(Bitmap bmp)
        {
            BitmapData bmpData = null;
            try
            {
                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                int numbytes = bmpData.Stride * bmp.Height;
                byte[] byteData = new byte[numbytes];
                IntPtr ptr = bmpData.Scan0;

                Marshal.Copy(ptr, byteData, 0, numbytes);

                return byteData;
            }
            finally
            {
                if (bmpData != null)
                {
                    bmp.UnlockBits(bmpData);
                }
            }
        }

        [CLSCompliant(false)]
        public static Bitmap[] GetPagesAsImages(PDF pdf, IEnumerable<int> pageNumbers = null) 
        {
            try
            {
                var document = pdf.LeadDocument;
                document.Resolution = 300;
                using RasterCodecs codecs = new RasterCodecs();
                pageNumbers = pageNumbers ?? Enumerable.Range(1, document.Pages.Count);
                return pageNumbers.Select(pageNum =>
                    {
                        using var image = document.GetPageImage(codecs, pageNum);
                        using var img = RasterImageConverter.ConvertToImage(
                            image, ConvertToImageOptions.None);
                        return new Bitmap(img);
                    }).ToArray();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53972");
            }
        }

        public static Bitmap[] GetPagesAsImages(TIF tif, IEnumerable<int> pageNumbers = null)
        {
            try
            {
                var imageStreamSource = tif.ImageStreamSource;
                var decoder = new TiffBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                pageNumbers = pageNumbers ?? Enumerable.Range(1, decoder.Frames.Count);
                return pageNumbers
                    .Select(pageNum => GetBitmapFromFrame(decoder.Frames[pageNum - 1]))
                    .ToArray();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53973");
            }
        }

        static Bitmap GetBitmapFromFrame(BitmapFrame bmpSource)
        {
            using MemoryStream outStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(bmpSource);
            enc.Save(outStream);
            return new Bitmap(outStream);
        }

        static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using var graphics = Graphics.FromImage(destImage);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            return destImage;
        } 
    }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "IMG")]
    public abstract class IMG : IDisposable
    {
        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TIF")]
    public class TIF : IMG
    {
        bool disposedValue;

        public TIF(string path)
        {
            Path = path;
            ImageStreamSource = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public FileStream ImageStreamSource { get; }

        public string Path { get; }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ImageStreamSource.Dispose();
                }
                disposedValue = true;
            }
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "PDF")]
    [CLSCompliant(false)]
    public class PDF : IMG
    {
        bool disposedValue;
        PDFDocument leadDocument;
        org.apache.pdfbox.pdmodel.PDDocument pdDocument;

        public PDF(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public org.apache.pdfbox.pdmodel.PDDocument PdfBoxDocument
        {
            get
            {
                if (pdDocument == null)
                {
                    pdDocument = org.apache.pdfbox.pdmodel.PDDocument
                        .load(new java.io.File(Path));
                }
                return pdDocument;
            }
        }

        public PDFDocument LeadDocument
        {
            get
            {
                if (leadDocument == null)
                {
                    leadDocument = new PDFDocument(Path);
                }
                return leadDocument;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (leadDocument != null)
                    {
                        leadDocument.Dispose();
                    }
                    if (pdDocument != null)
                    {
                        pdDocument.close();
                    }
                }
                disposedValue = true;
            }
        }
    }
}
