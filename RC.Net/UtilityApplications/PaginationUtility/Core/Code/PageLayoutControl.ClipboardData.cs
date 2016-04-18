using System;
using System.Collections.Generic;
using System.Linq;

namespace Extract.UtilityApplications.PaginationUtility
{
    partial class PageLayoutControl
    {
        /// <summary>
        /// Represents <see cref="PaginationControl"/>s copied to the clipboard by a
        /// <see cref="PageLayoutControl"/>.
        /// </summary>
        [Serializable]
        class ClipboardData
        {
            #region Fields

            /// <summary>
            /// A list of <see cref="Tuple"/>s where the first item indicates the input filename the
            /// a page is from and the second item indicates the page from that document. Any
            /// <see langword="null"/> entries in this list indicate an output document boundary
            /// which should be represented by a <see cref="PaginationSeparator"/> when pasted into
            /// a <see cref="PageLayoutControl"/>.
            /// </summary>
            List<Tuple<string, int>> _pageData;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ClipboardData"/> class.
            /// </summary>
            /// <param name="pages">The <see cref="Page"/> instances to be copied to the clipboard
            /// where any <see langword="null"/> entries indicate a document boundary.</param>
            public ClipboardData(IEnumerable<Page> pages)
            {
                _pageData = new List<Tuple<string, int>>(
                    pages.Select(page => (page == null)
                        ? null
                        : new Tuple<string, int>(page.OriginalDocumentName, page.OriginalPageNumber)));
            }

            #endregion Constructors

            #region Methods

            /// <summary>
            /// Gets the <see cref="IEnumerable{Page}"/> represented by this clipboard data.
            /// </summary>
/// <param name="paginationUtility">The <see cref="IPaginationUtility"/> the data is
/// needed for.</param>
            /// <returns>The <see cref="IEnumerable{Page}"/> represented by this clipboard data.
            /// </returns>
            public IEnumerable<Page> GetPages(IPaginationUtility paginationUtility)
            {
                // Convert each entry in _pageData into either null (for a document boundary) or a
                // Page instance.
                foreach(Tuple<string, int> pageData in _pageData)
                {
                    if (pageData == null)
                    {
                        yield return null;
                        continue;
                    }

                    string fileName = pageData.Item1;
                    int pageNumber = pageData.Item2;

                    var page = paginationUtility.GetDocumentPages(fileName, pageNumber);

                    // If unable to get page data, don't throw an exception, just act as though the
                    // data was not on the clipboard in the first place.
                    if (page == null || !page.Any())
                    {
                        break;
                    }

                    yield return page.Single();
                }
            }

            #endregion Methods

            #region Overrides

            /// <summary>
            /// Determines whether the specified <see cref="System.Object"/> is equal to this
            /// instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.
            /// </param>
            /// <returns><see langword="true"/> if the specified <see cref="System.Object"/> is
            /// equal to this instance; otherwise, <see langword="false"/>.
            /// </returns>
            public override bool Equals(object obj)
            {
                try
                {
                    // An equivalent object must be a ClipboardData instance.
                    var clipboardData = obj as ClipboardData;
                    if (clipboardData == null)
                    {
                        return false;
                    }

                    // An equivalent object must have the same number of entries in _pageData.
                    var pageData = clipboardData._pageData;
                    if (pageData.Count != _pageData.Count)
                    {
                        return false;
                    }

                    // An equivalent object must have equivalent entries in _pageData.
                    for (int i = 0; i < _pageData.Count; i++)
                    {
                        if ((pageData[i] == null) != (_pageData[i] == null))
                        {
                            return false;
                        }

                        if (pageData[i] != null)
                        {
                            if (!pageData[i].Item1.Equals(_pageData[i].Item1))
                            {
                                return false;
                            }

                            if (!pageData[i].Item2.Equals(_pageData[i].Item2))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35507");
                }
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data
            /// structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                int hashCode = base.GetHashCode();

                try
                {
                    hashCode = hashCode ^ _pageData.GetHashCode();

                    for (int i = 0; i < _pageData.Count; i++)
                    {
                        if (_pageData[i] != null)
                        {
                            hashCode ^= _pageData[i].Item1.GetHashCode();
                            hashCode ^= _pageData[i].Item2.GetHashCode();
                        }
                    }

                    return hashCode;
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI35508");
                }

                return hashCode;
            }
        }

        #endregion Overrides
    }
}
