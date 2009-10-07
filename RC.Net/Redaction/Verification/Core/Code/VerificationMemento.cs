using Extract.Imaging.Forms;
using System.Diagnostics;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents the previous state of a verified document.
    /// </summary>
    internal class VerificationMemento
    {
        #region VerificationMemento Fields

        /// <summary>
        /// The image file associated with the verified document.
        /// </summary>
        readonly string _imageFile;

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
        /// The amount of time the verify has spent on the document
        /// </summary>
        readonly Stopwatch _screenTime = new Stopwatch();

        /// <summary>
        /// A collection of the visited 0-based page numbers.
        /// </summary>
        VisitedItemsCollection _visitedPages;

        /// <summary>
        /// A collection of the visited 0-based redaction indices.
        /// </summary>
        VisitedItemsCollection _visitedRedactions;

        #endregion VerificationMemento Fields

        #region VerificationMemento Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationMemento"/> class.
        /// </summary>
        public VerificationMemento(string imageFile, int fileId, int actionId, 
            string attributesFile, string documentType, string feedbackImage)
        {
            _imageFile = imageFile;
            _fileId = fileId;
            _actionId = actionId;
            _voaFile = attributesFile;
            _documentType = documentType;
            _feedbackImage = feedbackImage;
        }

        #endregion VerificationMemento Constructors

        #region VerificationMemento Properties

        /// <summary>
        /// Gets the image file associated with the verified document.
        /// </summary>
        /// <returns>The image file associated with the verified document.</returns>
        public string ImageFile
        {
            get
            {
                return _imageFile;
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

        #endregion VerificationMemento Properties

        #region VerificationMemento Methods

        /// <summary>
        /// Starts the screen verification time clock.
        /// </summary>
        public void StartScreenTime()
        {
            _screenTime.Start();
        }

        /// <summary>
        /// Stops the screen verification time clock.
        /// </summary>
        /// <returns>The total elapsed seconds of screen verification time.</returns>
        public double StopScreenTime()
        {
            _screenTime.Stop();
            return _screenTime.ElapsedMilliseconds / 1000.0;
        }

        #endregion VerificationMemento Methods
    }
}
