using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Extract.Utilities;

namespace Extract.LabDE.StandardLabDE.Test
{
    /// <summary>
    /// Manages different test images.
    /// </summary>
    public class TestImageManager : IDisposable
    {
        #region TestImageManager Constants

        static readonly string[] _IMAGE_RESOURCE_NAMES = new string[]
        {
            // Basic image with Hematology Test
            "Resources.Image221.tif", 
        };

        const int _BASIC_HEMATOLOGY_INDEX = 0;
        const int _TOTAL_IMAGES = 1;

        #endregion TestImageManager Constants

        #region TestImageManager Fields

        /// <summary>
        /// An array of the temporary image file names. If any entry is <see langword="null"/> 
        /// the image file has not yet been created.
        /// </summary>
        string[] _imageFileNames = new string[_TOTAL_IMAGES];

        /// <summary>
        /// The StandardLabDE testing assembly.
        /// </summary>
        Assembly _assembly = Assembly.GetAssembly(typeof(TestStandardLabDE));

        #endregion TestImageManager Fields

        #region TestImageManager Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestImageManager"/> class.
        /// </summary>
        public TestImageManager()
        {
        }

        #endregion TestImageManager Constructors

        #region TestImageManager Finalizer

        /// <summary>
        /// Frees resources and performs other cleanup operations before the 
        /// <see cref="TestImageManager"/> is reclaimed by garbage collection.
        /// </summary>
        ~TestImageManager()
        {
            Dispose(false);
        }
	
        #endregion TestImageManager Finalizer

        #region TestImageManager Properties

        /// <summary>
        /// Gets the Basic Hematology test image file name.
        /// </summary>
        /// <returns>The Basic Hematology test image file name.</returns>
        public string BasicHematology
        {
            get
            {
                return GetImageByIndex(_BASIC_HEMATOLOGY_INDEX);
            }
        }

        #endregion TestImageManager Properties

        #region TestImageManager Methods

        /// <summary>
        /// Get the temporary image file name corresponding to the specified index.
        /// </summary>
        /// <param name="index">The index of the </param>
        /// <returns></returns>
        string GetImageByIndex(int index)
        {

            return _imageFileNames[index] ?? CreateTemporaryImage(index);
        }

        /// <summary>
        /// Creates a temorary image file from an embedded resource.
        /// </summary>
        /// <param name="imageFileIndex">The index of the image file in 
        /// <see cref="_IMAGE_RESOURCE_NAMES"/>.</param>
        /// <returns>The file name of the image generated from the resource at 
        /// <paramref name="imageFileIndex"/> in <see cref="_IMAGE_RESOURCE_NAMES"/>.</returns>
        string CreateTemporaryImage(int imageFileIndex)
        {
            try
            {
                // Generate a temp file name
                string fileName = Path.GetTempFileName();

                // Get the name of the resource that contains the image data
                string resource = _IMAGE_RESOURCE_NAMES[imageFileIndex];

                // Write the embedded testing image to a temporary file
                using (Stream stream =
                    _assembly.GetManifestResourceStream(typeof(TestStandardLabDE), resource))
                {
                    File.WriteAllBytes(fileName, StreamMethods.ConvertStreamToByteArray(stream));
                }

                // Store the image file name
                _imageFileNames[imageFileIndex] = fileName;

                // Return the file
                return fileName;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25498", ex);
                ee.Log();
                throw ee;
            }
        }

        #endregion TestImageManager Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="TestImageManager"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="TestImageManager"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="TestImageManager"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
            }

            // Dispose of unmanaged resources
            if (_imageFileNames != null)
            {
                for (int i = 0; i < _imageFileNames.Length; i++)
                {
                    string fileName = _imageFileNames[i];
                    if (fileName != null && File.Exists(fileName))
                    {
                        File.Delete(fileName);
                        _imageFileNames[i] = null;
                    }
                }
                _imageFileNames = null;
            }
        }

        #endregion IDisposable Members
    }
}
