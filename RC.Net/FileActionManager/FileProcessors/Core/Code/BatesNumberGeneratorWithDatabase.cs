using Extract;
using Extract.Imaging;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    internal class BatesNumberGeneratorWithDatabase : IBatesNumberGenerator
    {
        #region Fields

        /// <summary>
        /// The format settings for Bates numbers.
        /// </summary>
        BatesNumberFormat _format;

        /// <summary>
        /// The database manager used for getting and setting the Bates number
        /// </summary>
        IFileProcessingDB _databaseManager;

        /// <summary>
        /// The last Bates number retrieved from the database.
        /// </summary>
        long _lastBatesNumber;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BatesNumberGeneratorWithDatabase"/> class.
        /// </summary>
        /// <param name="format">The Bates number format object to use.</param>
        /// <param name="databaseManager">The database manager object to use.</param>
        public BatesNumberGeneratorWithDatabase(BatesNumberFormat format,
            IFileProcessingDB databaseManager)
        {
            try
            {
                // Ensure the db manager is not null
                ExtractException.Assert("ELI27897", "Database manager must not be null.",
                    databaseManager != null);

                // Store the db manager
                _databaseManager = databaseManager;

                _format = format;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27899", ex);
            }
        }

        #endregion Constructors

        #region IBatesNumberGenerator Members

        /// <summary>
        /// Commits the changes to the bates number.
        /// </summary>
        public void Commit()
        {
            // Nothing to do, the commit is performed when GetNextNumberString is called
        }

        /// <summary>
        /// Retrieves the next Bates numbers as text using the total page count.
        /// </summary>
        /// <param name="totalPages">The total number of pages for the Bates number.</param>
        /// <returns>The next Bates numbers as text.</returns>
        public ReadOnlyCollection<string> GetNextNumberStrings(int totalPages)
        {
            try
            {
                List<string> batesNumbers = new List<string>(totalPages);

                // Compute the first page number
                // If same number for every page, just get the next number
                // If different number for each page then first number is (Last - Total) + 1
                long nextNumber;
                if (_format.AppendPageNumber)
                {
                    nextNumber = GetLastNumber(1);
                }
                else
                {
                    nextNumber = (GetLastNumber(totalPages) - totalPages);
                }

                for (int i = 1; i <= totalPages; i++)
                {
                    // If using a new number for each page, then increment the number
                    if (!_format.AppendPageNumber)
                    {
                        nextNumber++;
                    }
                    batesNumbers.Add(BatesNumberHelper.GetStringFromNumber(_format, nextNumber, i));
                }

                // Return the collection of bates numbers
                return batesNumbers.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27900", ex);
            }
        }

        /// <summary>
        /// Retrieves the next Bates number as text using the page number.
        /// </summary>
        /// <param name="pageNumber">The page number for the Bates number.</param>
        /// <returns>The next Bates number as text.</returns>
        public string GetNextNumberString(int pageNumber)
        {
            throw new ExtractException("ELI27901",
                "This bates number generator does not support this method.");
        }

        /// <summary>
        /// Returns the next Bates number but does not perform the increment on the number. The
        /// caller should not assume the number returned by this method will ultimately be the
        /// next Bates number, it is just the next Bates number at the time of the call.
        /// </summary>
        /// <returns>The next Bates number (does not perform the Bates number increment).</returns>
        public long PeekNextNumber()
        {
            try
            {
                long returnValue = 1;
                string temp = _format.DatabaseCounter;
                if (!string.IsNullOrEmpty(temp) &&
                    _databaseManager.IsUserCounterValid(temp))
                {
                    // Return the current value of the counter + 1
                    returnValue = _databaseManager.GetUserCounterValue(temp) + 1;
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27902", ex);
            }
        }

        /// <summary>
        /// Retrieves the next Bates number as text without incrementing the Bates number.
        /// </summary>
        /// <param name="pageNumber">The page number on which the Bates number appears.</param>
        /// <returns>The next Bates number as text or the empty string if the Bates number was 
        /// invalid.</returns>
        public string PeekNextNumberString(int pageNumber)
        {
            try
            {
                // Return the string representation of the next Bates number
                return BatesNumberHelper.GetStringFromNumber(_format, PeekNextNumber(), pageNumber);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27903", ex);
            }
        }

        /// <summary>
        /// Retrieves and increments as appropriate the next Bates number using the current 
        /// settings.
        /// </summary>
        /// <param name="offset">The amount to offset the Bates number counter by</param>
        /// <returns>The next Bates number using the current settings.</returns>
        long GetLastNumber(long offset)
        {
            _lastBatesNumber = _databaseManager.OffsetUserCounter(_format.DatabaseCounter,
                offset);

            return _lastBatesNumber;
        }

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
                    ExtractException.Assert("ELI27904", "Format must not be null.", value != null);

                    // Validate that the format contains a valid database counter
                    if (!value.UseDatabaseCounter)
                    {
                        throw new ExtractException("ELI27905",
                            "This Bates number generator only supports database counters.");
                    }

                    // Store the format
                    _format = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27906", ex);
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="IFileProcessingDB"/> object for the generator.
        /// </summary>
        /// <returns>The <see cref="IFileProcessingDB"/> object.</returns>
        public IFileProcessingDB DatabaseManager
        {
            get
            {
                return _databaseManager;
            }
        }

        /// <summary>
        /// Gets the last Bates number that was retrieved from the database. (This value
        /// is set when a call to GetNextNumberStrings is made).
        /// </summary>
        /// <returns>The last Bates number that was retrieved from the database.</returns>
        public long LastBatesNumber
        {
            get
            {
                return _lastBatesNumber;
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="BatesNumberGeneratorWithDatabase"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="BatesNumberGeneratorWithDatabase"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="BatesNumberGeneratorWithDatabase"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            _databaseManager = null;
        }

        #endregion
    }
}
