using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace Extract.Imaging
{
    /// <summary>
    /// Generates Bates numbers for a single document.
    /// </summary>
    internal class BatesNumberGenerator : IBatesNumberGenerator
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(BatesNumberGenerator).ToString();

        #endregion Constants

        #region BatesNumberGenerator Fields

        /// <summary>
        /// The next Bates number or <see langword="null"/> if the next number has not yet been
        /// retrieved.
        /// </summary>
        long? _nextNumber;

        /// <summary>
        /// The format settings for Bates numbers.
        /// </summary>
        BatesNumberFormat _format;

        /// <summary>
        /// A stream to the next number file.
        /// </summary>
        Stream _stream;

        /// <summary>
        /// Reads Bates numbers from a stream.
        /// </summary>
        StreamReader _reader;

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion BatesNumberGenerator Fields

        #region BatesNumberGenerator Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BatesNumberGenerator"/> class.
        /// </summary>
        public BatesNumberGenerator()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatesNumberGenerator"/> class.
        /// </summary>
        /// <param name="format">The underlying <see cref="BatesNumberFormat"/> object.
        /// Caller is responsible for disposing of the format object.</param>
        public BatesNumberGenerator(BatesNumberFormat format)
        {
            // Validate the license
            _licenseCache.Validate("ELI23182");

            _format = format;
        }

        #endregion BatesNumberGenerator Constructors

        #region BatesNumberGenerator Methods

        /// <summary>
        /// Commits changes to the next Bates number.
        /// </summary>
        public void Commit()
        {
            // Check if there are any changes to commit
            if (_nextNumber == null)
            {
                return;
            }

            // Check whether only one Bates number was used for the entire document
            if (_format.AppendPageNumber)
            {
                if (_nextNumber == long.MaxValue)
                {
                    _nextNumber = 0;
                }

                _nextNumber++;
            }

            // Check whether to use the next number file
            if (_format.UseNextNumberFile)
            {
                if (_stream == null)
                {
                    // This is a logic error
                    throw new ExtractException("ELI22488", 
                        "Cannot commit changes to unopened next number file.");
                }

                // Reset the stream to the start
                // Note: If the reader were null, it would be a non-serious logic error
                if (_reader != null)
                {
                    // Note: It would be a bad idea to reader.Close() now, 
                    // because this would close the underlying stream.
                    _reader.DiscardBufferedData();
                }
                _stream.Seek(0, SeekOrigin.Begin);

                // Store the next value in the file
                using (StreamWriter writer = new StreamWriter(_stream))
                {
                    writer.WriteLine(_nextNumber);
                }

                // It is now safe to dispose of the reader and stream
                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }
                _stream.Dispose();
                _stream = null;
            }
            else
            {
                // Commit the changes to the registry
                RegistryManager.SetAndReleaseNextBatesNumber(_nextNumber.Value);

                // Commit the changes to format settings
                _format.NextNumber = _nextNumber.Value;
            }

            // Reset the next number
            _nextNumber = null;
        }

        /// <summary>
        /// Retrieves and increments as appropriate the next Bates number using the current 
        /// settings.
        /// </summary>
        /// <param name="increment"><see langword="true"/> if the next Bates number should be 
        /// incremented; <see langword="false"/> if the next Bates number should not be 
        /// incremented.</param>
        /// <returns>The next Bates number using the current settings.</returns>
        long GetNextNumber(bool increment)
        {
            // Check if we have already read the next number [IDSO #68]
            if (_nextNumber == null)
            {
                // Check whether the Bates number should come from a Bates number file
                if (_format.UseNextNumberFile)
                {
                    _nextNumber = GetNextNumberFromFile(false);
                }
                else
                {
                    // Store the registry value
                    _nextNumber = RegistryManager.GetAndHoldNextBatesNumber();
                }
            }

            // Increment the next number if it should be incremented per page
            if (increment && !_format.AppendPageNumber)
            {
                if (_nextNumber == long.MaxValue)
                {
                    _nextNumber = 0;
                }

                _nextNumber++;
                return _nextNumber.Value - 1;
            }

            return _nextNumber.Value;
        }

        /// <summary>
        /// Retrieves the next number without incrementing it.
        /// </summary>
        /// <returns>The next number without incrementing it.</returns>
        public long PeekNextNumber()
        {
            // Return the next number from the file or the next specified number
            if (_format.UseNextNumberFile)
            {
                return GetNextNumberFromFile(true);
            }
            else if (string.IsNullOrEmpty(_format.DatabaseCounter))
            {
                return _format.NextNumber;
            }
            else
            {
                throw new ExtractException("ELI27841",
                    "This implementation of the BatesNumberGenerator does not support using a database counter.");
            }
        }

        /// <summary>
        /// Retrieves the next Bates number from the next number file.
        /// </summary>
        /// <param name="peek"><see langword="true"/> if an invalid next number should return 
        /// -1; <see langword="false"/> if an invalid next number should throw an exception.
        /// </param>
        /// <returns>The next Bates number from the specified file, or -1 if the next Bates number 
        /// is not valid and <paramref name="peek"/> is <see langword="false"/>.</returns>
        long GetNextNumberFromFile(bool peek)
        {
            try
            {
                // Ensure the Bates number file exists
                if (!File.Exists(_format.NextNumberFile))
                {
                    if (peek)
                    {
                        return -1;
                    }

                    throw new ExtractException("ELI22452", "Invalid file.");
                }

                // Get exclusive access to the Bates number file
                _stream = new FileStream(_format.NextNumberFile, FileMode.Open, 
                    FileAccess.ReadWrite, FileShare.None);
                _reader = new StreamReader(_stream);

                // Get the first line of the stream
                String input = _reader.ReadLine();
                if (input == null)
                {
                    if (peek)
                    {
                        return -1;
                    }

                    throw new ExtractException("ELI22453", "File is empty.");
                }

                // Get the next number or -1 if the number is invalid
                long nextNumber;
                if (!long.TryParse(input, out nextNumber) || nextNumber < 0)
                {
                    if (peek)
                    {
                        return -1;
                    }

                    throw new ExtractException("ELI22455", "Invalid file format.");
                }

                return nextNumber;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI22451",
                    "Unable to get next Bates number.", ex);
                if (_format != null)
                {
                    ee.AddDebugData("File name", _format.NextNumberFile, false);
                }
                else
                {
                    ee.AddDebugData("Format", "null", false);
                }
                
                throw ee;
            }
        }

        /// <summary>
        /// Retrieves the next Bates number as text using the page number.
        /// </summary>
        /// <param name="pageNumber">The page number for the Bates number.</param>
        /// <returns>The next Bates number as text.</returns>
        public string GetNextNumberString(int pageNumber)
        {
            // Get the next Bates number. 
            // Increment if there is a separate Bates number for each page
            long nextNumber = GetNextNumber(!_format.AppendPageNumber);
            if (nextNumber < 0)
            {
                ExtractException ee = new ExtractException("ELI22457", "Invalid Bates number.");
                ee.AddDebugData("Bates number", nextNumber, false);
                throw ee;
            }

            return BatesNumberHelper.GetStringFromNumber(_format, nextNumber, pageNumber);
        }

        /// <summary>
        /// Retrieves the next Bates numbers as text using the total page count.
        /// </summary>
        /// <param name="totalPages">The total number of pages for the Bates number.</param>
        /// <returns>The next Bates numbers as text.</returns>
        public ReadOnlyCollection<string> GetNextNumberStrings(int totalPages)
        {
            List<string> nextNumbers = new List<string>(totalPages);
            for (int i=1; i <= totalPages; i++)
            {
                nextNumbers.Add(GetNextNumberString(i));
            }
            
            return nextNumbers.AsReadOnly();
        }

        /// <summary>
        /// Retrieves the next Bates number as text without incrementing the Bates number.
        /// </summary>
        /// <param name="pageNumber">The page number on which the Bates number appears.</param>
        /// <returns>The next Bates number as text or the empty string if the Bates number was 
        /// invalid.</returns>
        public string PeekNextNumberString(int pageNumber)
        {
            // Return the empty string if the next Bates number is invalid
            long nextNumber = PeekNextNumber();
            return nextNumber >= 0 ?
                BatesNumberHelper.GetStringFromNumber(_format, nextNumber, pageNumber) : "";
        }

        /// <summary>
        /// Retrieves the next Bates number without incrementing the Bates number.
        /// </summary>
        /// <param name="format">The format settings to use for the Bates number.</param>
        /// <returns>The next Bates number or -1 if the Bates number was invalid.</returns>
        public static long PeekNextNumberFromFile(BatesNumberFormat format)
        {
            using (BatesNumberGenerator generator = new BatesNumberGenerator(format))
            {
                return generator.GetNextNumberFromFile(true);
            }
        }

        #endregion BatesNumberGenerator Methods

        #region Properties

        /// <summary>
        /// Gets/sets the underlying Bates number format object.
        /// </summary>
        /// <returns>The <see cref="BatesNumberFormat"/> object.</returns>
        /// <value>The <see cref="BatesNumberFormat"/> object.</value>
        public BatesNumberFormat Format
        {
            get
            {
                return _format;
            }
            set
            {
                try
                {
                    ExtractException.Assert("ELI27844", "Bates number format must not be null.",
                        value != null);

                    _format = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27845", ex);
                }
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="BatesNumberGenerator"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="BatesNumberGenerator"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="BatesNumberGenerator"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }
                if (_nextNumber != null && !_format.UseNextNumberFile)
                {
                    RegistryManager.ReleaseNextBatesNumber();
                    _nextNumber = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
