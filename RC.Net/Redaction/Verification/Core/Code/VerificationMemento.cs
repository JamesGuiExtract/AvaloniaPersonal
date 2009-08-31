using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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

        #endregion VerificationMemento Fields

        #region VerificationMemento Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationMemento"/> class.
        /// </summary>
        public VerificationMemento(string imageFile, string attributesFile, string documentType,
            string feedbackImage)
        {
            _imageFile = imageFile;
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
