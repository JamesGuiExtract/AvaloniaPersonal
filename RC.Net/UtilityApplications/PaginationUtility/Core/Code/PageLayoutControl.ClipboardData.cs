using System;
using System.Collections.Generic;
using System.Linq;
using Extract.Utilities;

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
            /// a page is from, the second item indicates the page from that document and the third
            /// whether the page was copied in a deleted state. Any <see langword="null"/> entries
            /// in this list indicate an output document boundary which should be represented by a
            /// <see cref="PaginationSeparator"/> when pasted into a <see cref="PageLayoutControl"/>.
            /// </summary>
            List<Tuple<string, int, bool>> _pageData;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ClipboardData"/> class.
            /// </summary>
            /// <param name="pages">The <see cref="Page"/> instances and corresponding deleted
            /// states of those instances to be copied to the clipboard where any
            /// <see langword="null"/> page references indicate a document boundary.</param>
            public ClipboardData(IEnumerable<KeyValuePair<Page, bool>> pages)
            {
                _pageData = new List<Tuple<string, int, bool>>(
                    pages.Select(page => (page.Key == null)
                        ? null
                        : new Tuple<string, int, bool>(
                            page.Key.OriginalDocumentName, page.Key.OriginalPageNumber, page.Value)));
            }

            #endregion Constructors

            #region Methods

            /// <summary>
            /// Gets the <see cref="IEnumerable{Page}"/> represented by this clipboard data.
            /// </summary>
            /// <param name="paginationUtility">The <see cref="IPaginationUtility"/> the data is
            /// needed for.</param>
            /// <returns>The pages represented by this clipboard data where each <see cref="Page"/>
            /// is paired with a <see langword="bool"/> indicating whether the page was copied in a
            /// deleted state.
            /// </returns>
            public IEnumerable<KeyValuePair<Page, bool>> GetPages(IPaginationUtility paginationUtility)
            {
                // Convert each entry in _pageData into either null (for a document boundary) or a
                // Page instance.
                foreach(Tuple<string, int, bool> pageData in _pageData)
                {
                    if (pageData == null)
                    {
                        yield return new KeyValuePair<Page, bool>(null, false);
                        continue;
                    }

                    string fileName = pageData.Item1;
                    int pageNumber = pageData.Item2;
                    bool deleted = pageData.Item3;

                    var page = paginationUtility.GetDocumentPages(fileName, pageNumber);

                    // If unable to get page data, don't throw an exception, just act as though the
                    // data was not on the clipboard in the first place.
                    if (page == null || !page.Any())
                    {
                        break;
                    }

                    yield return new KeyValuePair<Page, bool>(page.Single(), deleted);
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

                            if (!pageData[i].Item3.Equals(_pageData[i].Item3))
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
                var hash = HashCode.Start;

                try
                {
                    hash = hash.Hash(_pageData.Count);

                    for (int i = 0; i < _pageData.Count; i++)
                    {
                        if (_pageData[i] != null)
                        {
                            hash = hash
                                .Hash(_pageData[i].Item1)
                                .Hash(_pageData[i].Item2)
                                .Hash(_pageData[i].Item3);
                        }
                    }

                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI35508");
                }

                return hash;
            }
        }

        #endregion Overrides
    }
}
