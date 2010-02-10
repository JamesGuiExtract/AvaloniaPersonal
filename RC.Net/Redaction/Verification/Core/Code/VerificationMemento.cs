using Extract.Imaging.Forms;
using Extract.Utilities;

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
        /// <see langword="true"/> if non-output redactions should be toggled off when loaded; 
        /// <see langword="false"/> if non-output redactions should not be toggled off.
        /// </summary>
        bool _toggleOffRedactions = true;

        /// <summary>
        /// A collection of the visited 0-based page numbers.
        /// </summary>
        VisitedItemsCollection _visitedPages;

        /// <summary>
        /// A collection of the visited 0-based redaction indices.
        /// </summary>
        VisitedItemsCollection _visitedRedactions;

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
                string basePath = FileSystemMethods.GetFullPathWithoutExtension(feedbackImage);
                _foundAttributesFileName = basePath + ".found.voa";
                _expectedAttributesFileName = basePath + ".expected.voa";
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
        /// Gets or sets whether non-output redactions should be toggled off.
        /// </summary>
        /// <value><see langword="true"/> if non-output redactions should be toggled off;
        /// <see langword="false"/> if non-output redactions should not be toggled off.</value>
        public bool ToggleOffRedactions
        {
            get 
            {
                return _toggleOffRedactions;
            }
            set
            {
                _toggleOffRedactions = value;
            }
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

        #endregion Properties
    }
}
