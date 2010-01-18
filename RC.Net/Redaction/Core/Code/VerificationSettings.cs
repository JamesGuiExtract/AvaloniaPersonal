using Extract.Interop;
using System;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents the settings used for verification.
    /// </summary>
    public class VerificationSettings
    {
        #region Fields

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
        /// Settings associated with changing the action status of a file once it is committed.
        /// </summary>
        readonly SetFileActionStatusSettings _actionStatusSettings;

        /// <summary>
        /// Whether input event tracking should be enabled.
        /// </summary>
        readonly bool _enableInputTracking;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class.
        /// </summary>
        public VerificationSettings() 
            : this(null, null, null, null, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class with the 
        /// specified settings.
        /// </summary>
        public VerificationSettings(GeneralVerificationSettings general, FeedbackSettings feedback, 
            string inputFile, SetFileActionStatusSettings actionStatus, bool enableInputTracking)
        {
            _generalSettings = general ?? new GeneralVerificationSettings();
            _feedbackSettings = feedback ?? new FeedbackSettings();
            _inputFile = inputFile ?? @"<SourceDocName>.voa";
            _actionStatusSettings = actionStatus ?? new SetFileActionStatusSettings();
            _enableInputTracking = enableInputTracking;
        }

        #endregion Constructors

        #region Properties

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
        /// Gets the settings associated with setting the action status for file when it is 
        /// committed.
        /// </summary>
        /// <value>The settings associated with setting the action status for file when it is 
        /// committed.</value>
        public SetFileActionStatusSettings ActionStatusSettings
        {
            get
            {
                return _actionStatusSettings;
            }
        }

        /// <summary>
        /// Gets whether input event tracking should be enabled.
        /// </summary>
        /// <returns><see langword="true"/> if input event tracking should be enabled and
        /// <see langword="false"/> otherwise.</returns>
        public bool EnableInputTracking
        {
            get
            {
                return _enableInputTracking;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a <see cref="VerificationSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="VerificationSettings"/>.</param>
        /// <returns>A <see cref="VerificationSettings"/> created from the specified 
        /// <see cref="IStreamReader"/>.</returns>
        public static VerificationSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                GeneralVerificationSettings general = GeneralVerificationSettings.ReadFrom(reader);
                FeedbackSettings feedback = FeedbackSettings.ReadFrom(reader);
                string inputFile = reader.ReadString();
                SetFileActionStatusSettings actionStatusSettings = null;
                bool enableInputTracking = false;

                if (reader.Version < 2)
                {
                    // Ignore obsolete metadata settings
                    reader.ReadBoolean();
                    reader.ReadString();
                }
                if (reader.Version >= 3)
                {
                    enableInputTracking = reader.ReadBoolean();
                }
                if (reader.Version >= 4)
                {
                	actionStatusSettings = SetFileActionStatusSettings.ReadFrom(reader);
                }

                return new VerificationSettings(general, feedback, inputFile, actionStatusSettings, enableInputTracking);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26520",
                    "Unable to read verification settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="VerificationSettings"/> to the specified 
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="VerificationSettings"/> will be written.</param>
        public void WriteTo(IStreamWriter writer)
        {
            try
            {
                _generalSettings.WriteTo(writer);
                _feedbackSettings.WriteTo(writer);
                writer.Write(_inputFile);
                writer.Write(_enableInputTracking);
                _actionStatusSettings.WriteTo(writer);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26521",
                    "Unable to write verification settings.", ex);
            }
        }

        #endregion Methods
    }
}
