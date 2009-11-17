using System;

namespace Extract.Redaction
{
    /// <summary>
    /// Provides data for the <see cref="DataFileControl.DataFileChanged"/> event.
    /// </summary>
    public class DataFileChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The ID Shield data file that changed.
        /// </summary>
        readonly string _dataFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataFileChangedEventArgs"/> class.
        /// </summary>
        /// <param name="dataFile">The ID Shield data file that changed.</param>
        public DataFileChangedEventArgs(string dataFile)
        {
            _dataFile = dataFile;
        }

        /// <summary>
        /// Gets the ID Shield data file that changed.
        /// </summary>
        /// <returns>The ID Shield data file that changed.</returns>
        public string DataFile
        {
            get
            {
                return _dataFile;
            }
        }
    }
}
