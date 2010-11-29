using System;
using System.Globalization;

namespace Extract.Database
{
    /// <summary>
    /// Helper struct for managing FPSFile table data.
    /// </summary>
    public struct FpsFileTableData
    {
        #region Fields

        /// <summary>
        /// The name of the FPS file to run.
        /// </summary>
        readonly string _fileName;

        /// <summary>
        /// The number of times to use the FPS file.
        /// </summary>
        readonly int _numberOfTimesToUse;

        /// <summary>
        /// The number of files to process before respawning the FPS file.
        /// </summary>
        readonly int _numberOfFilesToProcess;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FpsFileTableData"/> struct.
        /// </summary>
        /// <param name="fileName">Name of the FPS file.</param>
        /// <param name="numberOfTimesToUse">The number of times to use the FPS file.</param>
        /// <param name="numberOfFilesToProcess">The number of files to process.</param>
        public FpsFileTableData(string fileName, int numberOfTimesToUse, string numberOfFilesToProcess)
            : this(fileName, numberOfTimesToUse,
            int.Parse(numberOfFilesToProcess, CultureInfo.InvariantCulture))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FpsFileTableData"/> struct.
        /// </summary>
        /// <param name="fileName">Name of the FPS file.</param>
        /// <param name="numberOfTimesToUse">The number of times to use the FPS file.</param>
        /// <param name="numberOfFilesToProcess">The number of files to process.</param>
        public FpsFileTableData(string fileName, int numberOfTimesToUse, int numberOfFilesToProcess)
        {
            _fileName = fileName;
            _numberOfTimesToUse = numberOfTimesToUse;
            _numberOfFilesToProcess = numberOfFilesToProcess;
        }

        #endregion Constructor

        #region Equals Implementation

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this == (FpsFileTableData)obj;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _fileName.GetHashCode() ^ _numberOfFilesToProcess.GetHashCode()
                ^ _numberOfTimesToUse.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="firstItem">The first object to compare.</param>
        /// <param name="secondItem">The second object to compare.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(FpsFileTableData firstItem, FpsFileTableData secondItem)
        {
            return (firstItem._fileName.Equals(secondItem._fileName, StringComparison.OrdinalIgnoreCase)
                && firstItem._numberOfFilesToProcess == secondItem._numberOfFilesToProcess
                && firstItem._numberOfTimesToUse == secondItem._numberOfTimesToUse);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="firstItem">The first object to compare.</param>
        /// <param name="secondItem">The second object to compare.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(FpsFileTableData firstItem, FpsFileTableData secondItem)
        {
            return !(firstItem == secondItem);
        }

        #endregion Equals Implementation

        #region Properties

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }

        /// <summary>
        /// Gets the number of times to use.
        /// </summary>
        /// <value>The number of times to use.</value>
        public int NumberOfTimesToUse
        {
            get
            {
                return _numberOfTimesToUse;
            }
        }

        /// <summary>
        /// Gets the number of files to process.
        /// </summary>
        /// <value>The number of files to process.</value>
        public int NumberOfFilesToProcess
        {
            get
            {
                return _numberOfFilesToProcess;
            }
        }

        #endregion Properties
    }
}