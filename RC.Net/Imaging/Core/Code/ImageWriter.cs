using System;
using Extract.Imaging.Utilities;
using Extract.Licensing;
using Extract.Utilities;
using Leadtools;
using Leadtools.Codecs;
using System.IO;

namespace Extract.Imaging
{
    /// <summary>
    /// Represents a writer that can write image files.
    /// </summary>
    public sealed class ImageWriter : IDisposable
    {
        #region Constants

        static readonly string _OBJECT_NAME = typeof(ImageWriter).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the file to write.
        /// </summary>
        readonly string _fileName;

        /// <summary>
        /// Used to encode the image file.
        /// </summary>
        RasterCodecs _codecs;

        /// <summary>
        /// A temporary file used to store the image file until it is ready to <see cref="Commit"/>.
        /// </summary>
        TemporaryFile _tempFile;

        /// <summary>
        /// The number of pages written so far.
        /// </summary>
        int _pageCount;

        /// <summary>
        /// The file format of the output image.
        /// </summary>
        readonly RasterImageFormat _format;

        /// <summary>
        /// <see langword="true"/> if the output image is a portable document format (PDF) file;
        /// <see langword="false"/> if the output image is not a PDF file.
        /// </summary>
        readonly bool _isPdf;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageWriter"/> class.
        /// </summary>
        /// <param name="fileName">The name of the file to output.</param>
        /// <param name="codecs">The codecs used to encode the image.</param>
        /// <param name="format">The output file format.</param>
        /// <param name="append"><see langword="true"/> if <see paramref="fileName"/> should be
        /// appended to if it already exists; <see langword="false"/> otherwise.</param>
        internal ImageWriter(string fileName, RasterCodecs codecs, RasterImageFormat format,
            bool append)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28487",
                    _OBJECT_NAME);

                _fileName = fileName;
                _codecs = codecs;
                _tempFile = new TemporaryFile(true);
                _format = format;
                _isPdf = ImageMethods.IsPdf(format);

                if (append && File.Exists(fileName))
                {
                    File.Copy(fileName, _tempFile.FileName, true);
                }
            }
            catch (Exception ex)
            {
                if (_tempFile != null)
                {
                    _tempFile.Dispose();
                    _tempFile = null;
                }

                ExtractException ee = new ExtractException("ELI28624",
                    "Unable to create image reader.", ex);
                ee.AddDebugData("File name", fileName, false);
                throw ee;
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Appends the specified image to the output image.
        /// </summary>
        /// <param name="image">The image to append to the output image.</param>
        public void AppendImage(RasterImage image)
        {
            try
            {
                int count = image.PageCount;

                _codecs.Save(image, _tempFile.FileName, _format, image.BitsPerPixel, 1, count,
                    _pageCount + 1, CodecsSavePageMode.Append); 

                _pageCount += count;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28466",
                    "Unable to append page.", ex);
                ee.AddDebugData("Destination file", _fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Writes the specified tag to the specified page of the output image.
        /// </summary>
        /// <param name="tag">The tag to write.</param>
        /// <param name="pageNumber">The page number on which the tag should appear.</param>
        public void WriteTagOnPage(RasterTagMetadata tag, int pageNumber)
        {
            try
            {
                if (_isPdf)
                {
                    throw new ExtractException("ELI28621", "Cannot write PDF tags.");
                }

                _codecs.WriteTag(_tempFile.FileName, pageNumber, tag);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28483",
                    "Unable to write tag.", ex);
                ee.AddDebugData("Destination file", _fileName, false);
                ee.AddDebugData("Page number", pageNumber, false);
                throw ee;
            }
        }

        /// <summary>
        /// Commits the image to disk.
        /// </summary>
        /// <param name="overwrite"><see langword="true"/> if the output file should be 
        /// overwritten; <see langword="false"/> if an exception should be thrown if the 
        /// destination file exists.</param>
        public void Commit(bool overwrite)
        {
            try
            {
                // https://extract.atlassian.net/browse/ISSUE-16595
                // The files being produced by this class when running under the FAM service
                // (rather than via an application being run via the UI) do not seem to have
                // correct permissions applied. This can be worked around by creating the file
                // via .Net and copying in the bytes rather tha moving the file itself.
                using (var fileStream = new FileStream(_fileName,
                    overwrite ? FileMode.Create : FileMode.CreateNew,
                    FileAccess.ReadWrite))
                {
                    var fileData = File.ReadAllBytes(_tempFile.FileName);
                    fileStream.Write(fileData, 0, fileData.Length);
                }

                FileSystemMethods.DeleteFile(_tempFile.FileName);

                _tempFile.Dispose();
                _tempFile = null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28484", ex);
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ImageWriter"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ImageWriter"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ImageWriter"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }
                if (_tempFile != null)
                {
                    _tempFile.Dispose();
                    _tempFile = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}