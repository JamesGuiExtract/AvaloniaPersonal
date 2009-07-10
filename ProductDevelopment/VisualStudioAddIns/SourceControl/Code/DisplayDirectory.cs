using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControl
{
    /// <summary>
    /// Represents a local path and its abbreviated display form.
    /// </summary>
    public class DisplayDirectory
    {
        #region DisplayDirectory Constants

        /// <summary>
        /// Display directories are created from the capital letters, numbers, and punctuation 
        /// marks of their corresponding physical directory.
        /// </summary>
        static readonly char[] _SIGNIFICANT_CHARACTERS = 
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890._-".ToCharArray();

        #endregion DisplayDirectory Constants

        #region DisplayDirectory Fields

        /// <summary>
        /// The physical (local) directory to which this display directory corresponds
        /// </summary>
        string _physicalDirectory;

        /// <summary>
        /// The abbreviated (display) representation of the directory.
        /// </summary>
        string _displayDirectory;

        #endregion DisplayDirectory Fields

        #region DisplayDirectory Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayDirectory"/> class.
        /// </summary>
        public DisplayDirectory(string physicalDirectory)
        {
            _physicalDirectory = physicalDirectory;
            _displayDirectory = "/";

            // Construct the display directory from the first character and each subsequent 
            // capital letter, number, or punctuation mark in the physical directory.
            int letterIndex = 0;
            while (letterIndex >= 0 && letterIndex < _physicalDirectory.Length)
            {
                // Add this significant letter
                _displayDirectory += _physicalDirectory[letterIndex];

                // Find the next significant letter
                letterIndex = _physicalDirectory.IndexOfAny(_SIGNIFICANT_CHARACTERS, letterIndex + 1);
            }

            // Replace UCLID with U
            _displayDirectory = _displayDirectory.Replace("UCLID", "U");

            // If the result length is two or less, don't use the abbreviated form
            if (_displayDirectory.Length <= 2)
            {
                _displayDirectory = "/" + _physicalDirectory;
            }
        }

        #endregion DisplayDirectory Constructors

        #region DisplayDirectory Properties

        /// <summary>
        /// The physical (local) directory.
        /// </summary>
        public string Physical
        {
            get
            {
                return _physicalDirectory;
            }
            set
            {
                _physicalDirectory = value;
            }
        }

        /// <summary>
        /// The abbreviated display name for the directory.
        /// </summary>
        public string Display
        {
            get
            {
                return _displayDirectory;
            }
            set
            {
                _displayDirectory = value;
            }
        }

        #endregion DisplayDirectory Properties
    }
}
