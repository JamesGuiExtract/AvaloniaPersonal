using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents the settings for the <see cref="VerificationTask"/>.
    /// </summary>
    public class VerificationSettings
    {
        #region VerificationSettings Fields

        /// <summary>
        /// General settings associated with verification.
        /// </summary>
        readonly GeneralVerificationSettings _generalSettings;

        /// <summary>
        /// Settings associated with collecting verification feedback.
        /// </summary>
        readonly FeedbackSettings _feedbackSettings;

        /// <summary>
        /// The path to the Vector of Attributes (VOA) file being verified. May contain tags.
        /// </summary>
        readonly string _inputFile;

        /// <summary>
        /// Settings associated with verification metadata xml.
        /// </summary>
        readonly MetadataSettings _metadataSettings;

        #endregion VerificationSettings Fields

        #region VerificationSettings Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class.
        /// </summary>
        public VerificationSettings() : this(null, null, null, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class with 
        /// default settings.
        /// </summary>
        public VerificationSettings(GeneralVerificationSettings general, FeedbackSettings feedback,
            string inputFile, MetadataSettings metadata)
        {
            _generalSettings = general ?? new GeneralVerificationSettings();
            _feedbackSettings = feedback ?? new FeedbackSettings();
            _inputFile = inputFile ?? @"<SourceDocName>.voa";
            _metadataSettings = metadata ?? new MetadataSettings();
        }

        #endregion VerificationSettings Constructors

        #region VerificationSettings Properties

        /// <summary>
        /// Gets the general settings associated with verification.
        /// </summary>
        /// <returns>The general settings associated with verification.</returns>
        public GeneralVerificationSettings General
        {
            get
            {
                return _generalSettings;
            }
        }

        /// <summary>
        /// Gets the settings associated with collecting verification feedback.
        /// </summary>
        /// <returns>The settings associated with collecting verification feedback.</returns>
        public FeedbackSettings Feedback
        {
            get
            {
                return _feedbackSettings;
            }
        }

        /// <summary>
        /// Gets the path to the vector of attribute (VOA) file being verified. May contain tags.
        /// </summary>
        /// <returns>The path to the vector of attribute (VOA) file being verified. May contain 
        /// tags.</returns>
        public string InputFile
        {
            get
            {
                return _inputFile;
            }
        }

        /// <summary>
        /// Gets the settings associated with verification metadata xml.
        /// </summary>
        /// <returns>The settings associated with verification metadata xml.</returns>
        public MetadataSettings Metadata
        {
            get
            {
                return _metadataSettings;
            }
        }

        #endregion VerificationSettings Properties
    }
}
