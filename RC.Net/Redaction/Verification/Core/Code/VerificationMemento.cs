using Extract.Imaging;
using Extract.Imaging.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents the previous state of a verified document.
    /// </summary>
    internal class VerificationMemento
    {
        #region Fields

        /// <summary>
        /// The image file associated with the verified document.
        /// </summary>
        readonly string _sourceDocument;

        /// <summary>
        /// The image file to display in the image viewer.
        /// </summary>
        readonly string _displayImage;

        /// <summary>
        /// The id of the file.
        /// </summary>
        readonly int _fileId;

        /// <summary>
        /// The id of the action associated with the file.
        /// </summary>
        readonly int _actionId;

        /// <summary>
        /// The ID of the currently active FileTaskSession row. <see langword="null"/> if there is
        /// no such row.
        /// </summary>
        int? _fileTaskSessionId;

        /// <summary>
        /// The voa file with the current (perhaps uncommitted) redactions.
        /// </summary>
        readonly string _voaFile;

        /// <summary>
        /// The type of the document.
        /// </summary>
        readonly string _documentType;

        /// <summary>
        /// The fully expanded path to the destination feedback image.
        /// </summary>
        readonly string _feedbackImage;

        /// <summary>
        /// The fully expanded path to the destination found attributes voa file.
        /// </summary>
        readonly string _foundAttributesFileName;

        /// <summary>
        /// The fully expanded path to the destination expected attributes voa file.
        /// </summary>
        readonly string _expectedAttributesFileName;

        /// <summary>
        /// Indicates whether redactions have been loaded or saved as part of this memento whether
        /// or not the memento presently contains redactions.
        /// </summary>
        bool _hasContainedRedactions;

        /// <summary>
        /// The number of pages in the document.
        /// </summary>
        int _pageCount;

        /// <summary>
        /// A collection of the visited 0-based page numbers.
        /// </summary>
        VisitedItemsCollection _visitedPages;

        /// <summary>
        /// A collection of the visited 0-based redaction indices.
        /// </summary>
        VisitedItemsCollection _visitedRedactions;

        /// <summary>
        /// The number of seconds the document has been displayed for verification this session.
        /// </summary>
        double _screenTimeThisSession;

        /// <summary>
        /// The cumulative number of seconds elapsed between the time the previous document was
        /// saved and this document was fully loaded.
        /// </summary>
        double _overheadTimeThisSession;

        /// <summary>
        /// The selected redaction grid rows for the document.
        /// </summary>
        HashSet<int> _selection = new HashSet<int>();

        /// <summary>
        /// The redaction verification continue dialog has been displayed once, or not
        /// </summary>
        bool _continueDialogDisplayed;

        /// <summary>
        /// Continuing redaction - so don't perform normal selection of first grid row.
        /// </summary>
        bool _continuingRedactionSuspendNormalSelection;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationMemento"/> class.
        /// </summary>
        public VerificationMemento(string imageFile, string displayImage, int fileId, int actionId, 
            string attributesFile, string documentType, string feedbackImage)
        {
            _sourceDocument = imageFile;
            _displayImage = displayImage;
            _fileId = fileId;
            _actionId = actionId;
            _voaFile = attributesFile;
            _documentType = documentType;

            if (feedbackImage != null)
            {
                _feedbackImage = feedbackImage;

                // Initialize the destination found and expected voa files using the destination
                // feedbackImage filename as a base.
                _foundAttributesFileName = feedbackImage + ".found.voa";
                _expectedAttributesFileName = feedbackImage + ".expected.voa";
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the image file associated with the verified document.
        /// </summary>
        /// <returns>The image file associated with the verified document.</returns>
        public string SourceDocument
        {
            get
            {
                return _sourceDocument;
            }
        }

        /// <summary>
        /// Gets the image that should be displayed in the image viewer.
        /// </summary>
        /// <value>The image that should be displayed in the image viewer.</value>
        public string DisplayImage
        {
            get
            {
                return _displayImage;
            }
        }

        /// <summary>
        /// Gets the file id associated with the document.
        /// </summary>
        /// <value>The file id associated with the document.</value>
        public int FileId
        {
            get
            {
                return _fileId;
            }
        }

        /// <summary>
        /// Gets the action id associated with the document.
        /// </summary>
        /// <value>The action id associated with the document.</value>
        public int ActionId
        {
            get
            {
                return _actionId;
            }
        }

        /// <summary>
        /// The ID of the currently active FileTaskSession row. <see langword="null"/> if there is
        /// no such row.
        /// </summary>
        public int? FileTaskSessionID
        {
            get
            {
                return _fileTaskSessionId;
            }

            set
            {
                _fileTaskSessionId = value;
            }
        }

        /// <summary>
        /// Gets the vector of attributes (VOA) file associated with the verified document.
        /// </summary>
        /// <returns>The vector of attributes (VOA) file associated with the verified document.</returns>
        public string AttributesFile
        {
            get
            {
                return _voaFile;
            }
        }

        /// <summary>
        /// Gets the type of the document.
        /// </summary>
        /// <value>The type of the document.</value>
        public string DocumentType
        {
            get
            {
                return _documentType;
            }
        }

        /// <summary>
        /// Gets the fully expanded path of the destination for the feedback image.
        /// </summary>
        /// <value>The fully expanded path of the destination for the feedback image.</value>
        public string FeedbackImage
        {
            get
            {
                return _feedbackImage;
            }
        }

        /// <summary>
        /// Gets the filename that should be used to save found attributes for feedback.
        /// </summary>
        /// <returns>The filename that should be used to save found attributes for feedback.
        /// </returns>
        public string FoundAttributesFileName
        {
            get
            {
                return _foundAttributesFileName;
            }
        }

        /// <summary>
        /// Gets the filename that should be used to save expected attributes for feedback.
        /// </summary>
        /// <returns>The filename that should be used to save expected attributes for feedback.
        /// </returns>
        public string ExpectedAttributesFileName
        {
            get
            {
                return _expectedAttributesFileName;
            }
        }

        /// <summary>
        /// Gets or sets whether redactions have been loaded or saved as part of this memento
        /// whether or not the memento presently contains redactions.
        /// </summary>
        /// <value><see langword="true"/> if the memento is known to have been loaded or saved with
        /// redactions present; <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if the memento is known to have been loaded or saved with
        /// redactions present; <see langword="false"/> otherwise.</returns>
        public bool HasContainedRedactions
        {
            get
            {
                return _hasContainedRedactions;
            }

            set
            {
                _hasContainedRedactions = value;
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
                return _pageCount;
            }

            set
            {
                _pageCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="ImagePageData"/> instances for the document.
        /// </summary>
        /// <value>The <see cref="ReadOnlyCollection{T}"/> of <see cref="ImagePageData"/> instances
        /// for each page of the document.</value>
        public ReadOnlyCollection<ImagePageData> ImagePageData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the 0-based visited page numbers.
        /// </summary>
        /// <value>The 0-based visited page numbers.</value>
        public VisitedItemsCollection VisitedPages
        {
            get
            {
                return _visitedPages;
            }
            set
            {
                _visitedPages = value;
            }
        }

        /// <summary>
        /// Gets or sets the 0-based visited redaction indexes.
        /// </summary>
        /// <value>The 0-based visited redaction indexes.</value>
        public VisitedItemsCollection VisitedRedactions
        {
            get
            {
                return _visitedRedactions;
            }
            set
            {
                _visitedRedactions = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of seconds the document has been displayed for verification this
        /// session.
        /// </summary>
        /// <value>The number of seconds the document has been displayed for verification this
        /// session.</value>
        public double ScreenTimeThisSession
        {
            get
            {
                return _screenTimeThisSession;
            }

            set
            {
                _screenTimeThisSession = value;
            }
        }

        /// <summary>
        /// Gets or sets the cumulative number of seconds elapsed between the time the previous
        /// document was saved and this document was fully loaded.
        /// </summary>
        /// <value>The cumulative number of seconds elapsed between the time the previous document
        /// was saved and this document was fully loaded.</value>
        public double OverheadTimeThisSession
        {
            get
            {
                return _overheadTimeThisSession;
            }

            set
            {
                _overheadTimeThisSession = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected redaction grid rows for the document.
        /// </summary>
        /// <value>
        /// The selected redaction grid rows for the document.
        /// </value>
        public IEnumerable<int> Selection
        {
            get
            {
                return _selection;
            }

            set
            {
                _selection = new HashSet<int>(value);
            }
        }

        /// <summary>
        /// Gets or sets OCR data to cache for the document.
        /// </summary>
        /// <value>A <see cref="ThreadSafeSpatialString"/> instance representing the OCR data associated
        /// with the document.
        /// </value>
        public ThreadSafeSpatialString OcrData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the continue dialog has been 
        /// displayed once already in this session.
        /// </summary>
        /// <value>
        /// <c>true</c> if [continue dialog displayed]; otherwise, <c>false</c>.
        /// </value>
        public bool ContinueDialogDisplayed
        {
            get
            {
                return _continueDialogDisplayed;
            }
            set
            {
                _continueDialogDisplayed = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to suspend navigation due to being in 
        /// "Continue verification" mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if [continuing redaction suspend navigation]; otherwise, <c>false</c>.
        /// </value>
        public bool ContinuingRedactionSuspendNavigation
        {
            get
            {
                return _continuingRedactionSuspendNormalSelection;
            }
            set
            {
                _continuingRedactionSuspendNormalSelection = value;
            }
        }

        #endregion Properties
    }
}
