using Extract.Interop;
using System;
using Extract.FileActionManager.Forms;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// Represents the settings used for verification via <see cref="DataEntryApplicationForm"/>.
    /// </summary>
    public class VerificationSettings
    {
        #region Fields

        /// <summary>
        /// The name of the DataEntry configuration file to use for the
        /// <see cref="DataEntryApplicationForm"/>.
        /// </summary>
        string _configFileName;

        /// <summary>
        /// Specifies whether input event tracking should be logged in the database.
        /// </summary>
        bool _inputEventTrackingEnabled;

        /// <summary>
        /// Specifies whether counts will be recorded for the defined data entry counters.
        /// </summary>
        bool _countersEnabled;

        /// <summary>
        /// Specifies whether the users are able to apply tags.
        /// </summary>
        bool _allowTags = true;

        /// <summary>
        /// Specifies which tags should be available to the users.
        /// </summary>
        FileTagSelectionSettings _tagSettings = new FileTagSelectionSettings();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class.
        /// </summary>
        public VerificationSettings()
            : this(null, false, false, true, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class.
        /// </summary>
        /// <param name="configFileName">The name of the DataEntry configuration file to use for the
        /// <see cref="DataEntryApplicationForm"/>.</param>
        public VerificationSettings(string configFileName)
            : this(configFileName, false, false, true, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class.
        /// </summary>
        /// <param name="source">A <see cref="VerificationSettings"/> instance to copy settings
        /// from.</param>
        public VerificationSettings(VerificationSettings source)
        {
            _configFileName = source.ConfigFileName;
            _inputEventTrackingEnabled = source.InputEventTrackingEnabled;
            _countersEnabled = source.CountersEnabled;
            _allowTags = source.AllowTags;
            _tagSettings = (source.TagSettings == null)
                ? new FileTagSelectionSettings()
                : new FileTagSelectionSettings(source.TagSettings);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class with the 
        /// specified settings.
        /// </summary>
        /// <param name="configFileName">The name of the DataEntry configuration file to use for the
        /// <see cref="DataEntryApplicationForm"/>.
        /// </param>
        /// <param name="inputEventTrackingEnabled">Specifies whether input event tracking should be
        /// logged in the database.</param>
        /// <param name="countersEnabled">Specifies whether counts will be recorded for the defined
        /// data entry counters.</param>
        /// <param name="allowTags">Specifies whether the users are able to apply tags.</param>
        /// <param name="tagSettings">Specifies which tags should be available to the users.</param>
        public VerificationSettings(string configFileName, bool inputEventTrackingEnabled,
            bool countersEnabled, bool allowTags, FileTagSelectionSettings tagSettings)
        {
            _configFileName = configFileName;
            _inputEventTrackingEnabled = inputEventTrackingEnabled;
            _countersEnabled = countersEnabled;
            _allowTags = allowTags;
            _tagSettings = tagSettings ?? new FileTagSelectionSettings();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the name of the DataEntry configuration file to use for the
        /// <see cref="DataEntryApplicationForm"/>.
        /// </summary>
        /// <value>The name of the DataEntry configuration file to use.</value>
        public string ConfigFileName
        {
            get
            {
                return _configFileName;
            }
        }

        /// <summary>
        /// Gets whether input event tracking should be logged in the database.
        /// <para><b>Note</b></para>
        /// Input tracking will only be recorded if this option is <see langword="true"/> and
        /// the "EnableInputEventTracking" option is set in the database.
        /// </summary>
        /// <value><see langword="true"/> to record data from user input, <see langword="false"/>
        /// otherwise.</value>
        public bool InputEventTrackingEnabled
        {
            get
            {
                return _inputEventTrackingEnabled;
            }
        }

        /// <summary>
        /// Gets whether counts will be recorded for the defined data entry counters.
        /// <para><b>Note</b></para>
        /// Counter values will only be recorded if this option is <see langword="true"/> and
        /// the "EnableDataEntryCounters" option is set in the database.
        /// </summary>
        /// <value><see langword="true"/> to record counts for the defined counters,
        /// <see langword="false"/> otherwise.</value>
        public bool CountersEnabled
        {
            get
            {
                return _countersEnabled;
            }
        }
        
        /// <summary>
        /// Gets or sets whether users can apply tags to documents.
        /// </summary>
        /// <value><see langword="true"/> if users can apply tags to documents,
        /// <see langword="false"/> otherwise.</value>
        public bool AllowTags
        {
            get
            {
                return _allowTags;
            }
        }

        /// <summary>
        /// Gets the <see cref="FileTagSelectionSettings"/> specifying which tags are to be
        /// available.
        /// </summary>
        /// <value>
        /// The <see cref="FileTagSelectionSettings"/> .
        /// </value>
        public FileTagSelectionSettings TagSettings
        {
            get
            {
                return _tagSettings;
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
                string configFileName = null;
                bool inputEventTrackingEnabled = false;
                bool countersEnabled = false;
                bool allowTags = false;
                var tagSettings = new FileTagSelectionSettings();

                if (reader.Version >= 2)
                {
                    configFileName = reader.ReadString();
                }

                if (reader.Version >= 3)
                {
                    inputEventTrackingEnabled = reader.ReadBoolean();
                }

                if (reader.Version >= 4)
                {
                    countersEnabled = reader.ReadBoolean();
                }

                if (reader.Version >= 5)
                {
                    allowTags = reader.ReadBoolean();
                    tagSettings = FileTagSelectionSettings.ReadFrom(reader);
                }

                return new VerificationSettings(configFileName, inputEventTrackingEnabled,
                    countersEnabled, allowTags, tagSettings);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI37239",
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
                writer.Write(_configFileName);
                writer.Write(_inputEventTrackingEnabled);
                writer.Write(_countersEnabled);
                writer.Write(_allowTags);
                _tagSettings.WriteTo(writer);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI37240",
                    "Unable to write verification settings.", ex);
            }
        }

        #endregion Methods
    }
}
