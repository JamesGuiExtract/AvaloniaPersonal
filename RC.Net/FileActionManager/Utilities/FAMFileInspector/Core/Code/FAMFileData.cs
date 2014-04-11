using Extract.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Represents data pertaining to a file listed in the <see cref="FAMFileInspectorForm"/>.
    /// </summary>
    internal class FAMFileData : IComparable<FAMFileData>
    {
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        List<PageState> _pageData = new List<PageState>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMFileData"/> class.
        /// </summary>
        /// <param name="fileName">The name of the file to which this instance pertains.</param>
        public FAMFileData(string fileName)
        {
            try
            {
                FileName = fileName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35747");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public bool Dirty
        {
            get
            {
                return PageData.Any(page => page.Dirty);
            }
        }

        /// <summary>
        /// Gets or sets the name of the file to which this instance pertains.
        /// </summary>
        /// <value>
        /// The name of the file to which this instance pertains.
        /// </value>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether this instance should reflect the results of the most recent text
        /// search data search or neither.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to reflect the results most recent text search,
        /// <see langword="false"/> to reflect the results most recent data search, or
        /// <see langword="null"/> for neither.
        /// </value>
        public bool? ShowTextResults
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Flagged
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this file was a match for the most recent
        /// indicated by <see cref="ShowTextResults"/>.
        /// </summary>
        /// <value><see langword="true"/> if this file was a match for the search; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool FileMatchesSearch
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the text matches from the most recent text search.
        /// </summary>
        /// <value>
        /// The <see cref="Match"/>s indicating the matching text or <see langword="null"/> if the
        /// file did not have any OCR data.
        /// </value>
        public IEnumerable<Match> TextMatches
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the data matches from the most recent data search.
        /// </summary>
        /// <value>
        /// The <see cref="ThreadSafeSpatialString"/>s indicating the matching data or
        /// <see langword="null"/> if the file did not have a VOA file.
        /// </value>
        public IEnumerable<ThreadSafeSpatialString> DataMatches
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an exception generated while searching this file.
        /// </summary>
        /// <value>
        /// The exception generated while searching this file or <see langword="null"/> if there was
        /// no error searching the file.
        /// </value>
        public ExtractException Exception
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the OCR text associated with the file.
        /// </summary>
        public SpatialString OcrText
        {
            get
            {
                try
                {
                    string ussFilename = FileName + ".uss";
                    if (File.Exists(ussFilename))
                    {
                        var ocrText = new SpatialString();
                        ocrText.LoadFrom(ussFilename, false);
                        return ocrText;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35748");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/>s from the VOA file associated with this file.
        /// </summary>
        public IUnknownVector Attributes
        {
            get
            {
                try
                {
                    string voaFilename = FileName + ".voa";
                    if (File.Exists(voaFilename))
                    {
                        var data = new IUnknownVector();
                        data.LoadFrom(voaFilename, false);
                        return data;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35749");
                }
            }
        }

        /// <summary>
        /// Gets or sets the match count for the last type of search run.
        /// </summary>
        /// <value>
        /// The match count, -1 if there are no results available or -2 if there was an error
        /// searching this file.
        /// </value>
        public int MatchCount
        {
            get
            {
                try
                {
                    if (Exception != null)
                    {
                        return -2;
                    }
                    if (ShowTextResults == null)
                    {
                        return -1;
                    }

                    int matchCount = ShowTextResults.Value
                        ? (TextMatches == null) ? -1 : TextMatches.Count()
                        : (DataMatches == null) ? -1 : DataMatches.Count();

                    return matchCount;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35833");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<PageState> PageData
        {
            get
            {
                return _pageData;
            }
        }

        /// <summary>
        /// Gets or sets the page count.
        /// </summary>
        /// <value>
        /// The page count.
        /// </value>
        public int PageCount
        {
            get
            {
                return _pageData.Count;
            }

            set
            {
                if (value != _pageData.Count)
                {
                    _pageData = 
                        Enumerable.Range(0, value).Select(i => new PageState()).ToList();
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Clears the most recent search results.
        /// </summary>
        public void ClearSearchResults()
        {
            try
            {
                Exception = null;
                FileMatchesSearch = false;
                ShowTextResults = null;
                TextMatches = null;
                DataMatches = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35750");
            }
        }

        #endregion Methods

        #region IComparable<FAMFileData>

        /// <summary>
        /// Compares the current object with another object type <see cref="T:FAMFileData"/>.
        /// </summary>
        /// <param name="other">An <see cref="FAMFileData"/> instance to compare with this instance.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared.
        /// The return value has the following meanings: Value Meaning Less than zero This object is
        /// less than the other parameter.Zero This object is equal to other. Greater than zero This
        /// object is greater than other.
        /// </returns>
        public int CompareTo(FAMFileData other)
        {
            try
            {
                return MatchCount.CompareTo(other.MatchCount);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35752");
            }
        }

        #endregion IComparable<FAMFileData>

        #region Overrides

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            try
            {
                if (Exception != null)
                {
                    return "(Error)";
                }

                if (ShowTextResults.HasValue)
                {
                    if (ShowTextResults.Value)
                    {
                        if (TextMatches != null)
                        {
                            return TextMatches.Count().ToString(CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            return "(No OCR)";
                        }
                    }
                    else
                    {
                        if (DataMatches != null)
                        {
                            return DataMatches.Count().ToString(CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            return "(No Data)";
                        }
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35751");
            }
        }

        #endregion Overrides
    }

    /// <summary>
    /// Extension methods for the <see cref="FAMFileData"/> class.
    /// </summary>
    internal static class FAMFileDataFAMExtensionMethods
    {
        /// <summary>
        /// Gets the file search results.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        public static FAMFileData GetFileData(this DataGridViewRow row)
        {
            try
            {
                return (FAMFileData)row.Cells[FAMFileInspectorForm._FILE_LIST_MATCH_COLUMN_INDEX].Value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35746");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class PageState
    {
        bool _dirty;
        int _orientation;

        /// <summary>
        /// 
        /// </summary>
        public bool Dirty
        {
            get
            {
                return _dirty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Orientation
        {
            get
            {
                return _orientation;
            }

            set
            {
                if (value != _orientation)
                {
                    _orientation = value;
                    _dirty = true;
                }
            }
        }
    }
}
