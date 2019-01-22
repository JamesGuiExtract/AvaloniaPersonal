using Extract.Interop;
using System;
using Extract.FileActionManager.Forms;

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
        /// <see langword="true"/> if the back drop image should be used if available; 
        /// <see langword="false"/> if the back drop image should be ignored.
        /// </summary>
        readonly bool _useBackdropImage;

        /// <summary>
        /// The path of the back drop image to use if <see cref="_useBackdropImage"/> is 
        /// <see langword="true"/>. May contain path tags.
        /// </summary>
        readonly string _backdropImage;

        /// <summary>
        /// Settings associated with changing the action status of a file once it is committed.
        /// </summary>
        readonly SetFileActionStatusSettings _actionStatusSettings;

        /// <summary>
        /// Indicates whether to launch the verification UI in full screen mode.
        /// </summary>
        readonly bool _launchInFullScreenMode;

        /// <summary>
        /// The slideshow related settings.
        /// </summary>
        readonly SlideshowSettings _slideshowSettings;

        /// <summary>
        /// Specifies whether the user should be able to apply tags.
        /// </summary>
        readonly bool _allowTags = true;

        /// <summary>
        /// Specifies which tags should be available to the users.
        /// </summary>
        FileTagSelectionSettings _tagSettings;

        VerificationModeSetting _verificationModeSetting;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class.
        /// </summary>
        public VerificationSettings() 
            : this(null, null, null, false, null, null, false, null, true, null, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class with the 
        /// specified settings.
        /// </summary>
        public VerificationSettings(GeneralVerificationSettings general, FeedbackSettings feedback, 
            string inputFile, bool useBackdropImage, string backdropImage, 
            SetFileActionStatusSettings actionStatus, bool launchInFullScreenMode, 
            SlideshowSettings slideshowSettings, bool allowTags,
            FileTagSelectionSettings tagSettings, VerificationModeSetting verificationModeSetting)
        {
            _generalSettings = general ?? new GeneralVerificationSettings();
            _feedbackSettings = feedback ?? new FeedbackSettings();
            _inputFile = inputFile ?? @"<SourceDocName>.voa";
            _useBackdropImage = useBackdropImage;
            _backdropImage = backdropImage;
            _actionStatusSettings = actionStatus ?? new SetFileActionStatusSettings();
            _launchInFullScreenMode = launchInFullScreenMode;
            _slideshowSettings = slideshowSettings ?? new SlideshowSettings();
            _allowTags = allowTags;
            _tagSettings = tagSettings ?? new FileTagSelectionSettings();
            _verificationModeSetting = 
                verificationModeSetting ?? new VerificationModeSetting(VerificationMode.Verify);
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
        /// Gets whether to use the <see cref="BackdropImage"/> if it is available.
        /// </summary>
        /// <value><see langword="true"/> if to use the <see cref="BackdropImage"/> if it is 
        /// available; <see langword="false"/> if to ignore the <see cref="BackdropImage"/>.</value>
        public bool UseBackdropImage
        {
            get
            {
                return _useBackdropImage;
            }
        }

        /// <summary>
        /// Gets the path of the back drop image to use if <see cref="UseBackdropImage"/> is 
        /// <see langword="true"/>.
        /// </summary>
        /// <value>The path of the back drop image to use if <see cref="UseBackdropImage"/> is 
        /// <see langword="true"/>.</value>
        public string BackdropImage
        {
            get
            {
                return _backdropImage;
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
        /// Gets a value indicating whether to lauch the verification UI in full screen mode.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to lauch the verification UI in full screen mode; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool LaunchInFullScreenMode
        {
            get
            {
                return _launchInFullScreenMode;
            }
        }

        /// <summary>
        /// Gets the slideshow settings.
        /// </summary>
        /// <value>An <see cref="SlideshowSettings"/> instance containing the settings for the
        /// slideshow.</value>
        public SlideshowSettings SlideshowSettings
        {
            get
            {
                return _slideshowSettings;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user can apply tags.
        /// </summary>
        /// <value><see langword="true"/> if user can apply tags; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool AllowTags
        {
            get
            {
                return _allowTags;
            }
        }

        /// <summary>
        /// Gets the tag settings.
        /// </summary>
        public FileTagSelectionSettings TagSettings
        {
            get
            {
                return _tagSettings;
            }
        }

        /// <summary>
        /// Gets the verification mode setting.
        /// </summary>
        /// <value>
        /// The redaction verification mode.
        /// </value>
        public VerificationModeSetting RedactionVerificationMode
        {
            get
            {
                return _verificationModeSetting;
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
                bool useBackdropImage = false;
                string backdropImage = null;
                SetFileActionStatusSettings actionStatusSettings = null;
                bool launchInFullScreenMode = false;
                SlideshowSettings slideshowSettings = null;
                bool allowTags = true;
                FileTagSelectionSettings tagSettings = null;
                int verifyMode = 0;
                VerificationModeSetting verificationModeSetting = null;

                if (reader.Version < 2)
                {
                    // Ignore obsolete metadata settings
                    reader.ReadBoolean();
                    reader.ReadString();
                }
                if (reader.Version >= 3 && reader.Version <= 10)
                {
                    // Ignore enableInputTracking
                    reader.ReadBoolean();
                }
                if (reader.Version >= 4)
                {
                    actionStatusSettings = SetFileActionStatusSettings.ReadFrom(reader);
                }
                if (reader.Version >= 5)
                {
                    useBackdropImage = reader.ReadBoolean();
                    backdropImage = reader.ReadString();
                }
                if (reader.Version >= 6)
                {
                    launchInFullScreenMode = reader.ReadBoolean();
                }
                if (reader.Version >= 7)
                {
                    slideshowSettings = SlideshowSettings.ReadFrom(reader);
                }
                if (reader.Version >= 9)
                {
                    allowTags = reader.ReadBoolean();
                    tagSettings = FileTagSelectionSettings.ReadFrom(reader);
                }
                if (reader.Version >= 10)
                {
                    verifyMode = reader.ReadInt32();
                    verificationModeSetting = new VerificationModeSetting((VerificationMode)verifyMode);
                }
                else
                {
                    verificationModeSetting = new VerificationModeSetting(VerificationMode.Verify);
                }

                return new VerificationSettings(general, feedback, inputFile, useBackdropImage,
                    backdropImage, actionStatusSettings, launchInFullScreenMode, 
                    slideshowSettings, allowTags, tagSettings, verificationModeSetting);
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
                _actionStatusSettings.WriteTo(writer);
                writer.Write(_useBackdropImage);
                writer.Write(_backdropImage);
                writer.Write(_launchInFullScreenMode);
                _slideshowSettings.WriteTo(writer);
                writer.Write(_allowTags);
                _tagSettings.WriteTo(writer);
                writer.Write((int)RedactionVerificationMode.VerificationMode);
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
