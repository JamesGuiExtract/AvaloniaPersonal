using Extract.FileActionManager.Forms;
using Extract.Interop;
using System;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// Represents the settings used for verification via <see cref="DataEntryApplicationForm"/>.
    /// </summary>
    public class VerificationSettings
    {
        #region Fields

        /// <summary>
        /// Specifies which tags should be available to the users.
        /// </summary>
        FileTagSelectionSettings _tagSettings = new FileTagSelectionSettings();

        /// <summary>
        /// The settings for pagination.
        /// </summary>
        PaginationSettings _paginationSettings = new PaginationSettings();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class.
        /// </summary>
        public VerificationSettings()
            : this(null, false, true, null, false, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class.
        /// </summary>
        /// <param name="configFileName">The name of the DataEntry configuration file to use for the
        /// <see cref="DataEntryApplicationForm"/>.</param>
        public VerificationSettings(string configFileName)
            : this(configFileName, false, true, null, false, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class.
        /// </summary>
        /// <param name="source">A <see cref="VerificationSettings"/> instance to copy settings
        /// from.</param>
        public VerificationSettings(VerificationSettings source)
        {
            ConfigFileName = source.ConfigFileName;
            CountersEnabled = source.CountersEnabled;
            AllowTags = source.AllowTags;
            _tagSettings = (source.TagSettings == null)
                ? new FileTagSelectionSettings()
                : new FileTagSelectionSettings(source.TagSettings);
            PaginationEnabled = source.PaginationEnabled;
            _paginationSettings = (source.PaginationSettings == null)
                ? new PaginationSettings()
                : new PaginationSettings(source.PaginationSettings);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationSettings"/> class with the
        /// specified settings.
        /// </summary>
        /// <param name="configFileName">The name of the DataEntry configuration file to use for the
        /// <see cref="DataEntryApplicationForm"/>.</param>
        /// <param name="countersEnabled">Specifies whether counts will be recorded for the defined
        /// data entry counters.</param>
        /// <param name="allowTags">Specifies whether the users are able to apply tags.</param>
        /// <param name="tagSettings">Specifies which tags should be available to the users.</param>
        /// <param name="paginationEnabled"><see langword="true"/> if pagination should be enabled;
        /// otherwise, <see langword="false"/></param>
        /// <param name="paginationSettings">A <see cref="PaginationSettings"/> specifying the
        /// settings for pagination.</param>
        public VerificationSettings(string configFileName, bool countersEnabled, 
            bool allowTags, FileTagSelectionSettings tagSettings,
            bool paginationEnabled, PaginationSettings paginationSettings)
        {
            ConfigFileName = configFileName;
            CountersEnabled = countersEnabled;
            AllowTags = allowTags;
            _tagSettings = tagSettings ?? new FileTagSelectionSettings();
            PaginationEnabled = paginationEnabled;
            _paginationSettings = paginationSettings ?? new PaginationSettings();
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
            get;
            private set;
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
            get;
            private set;
        }
        
        /// <summary>
        /// Gets or sets whether users can apply tags to documents.
        /// </summary>
        /// <value><see langword="true"/> if users can apply tags to documents,
        /// <see langword="false"/> otherwise.</value>
        public bool AllowTags
        {
            get;
            private set;
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

        /// <summary>
        /// Gets whether pagination is to be enabled.
        /// </summary>
        public bool PaginationEnabled
        {
            get;
            private set;
        }

        /// <summary>
        /// A <see cref="PaginationSettings"/> specifying the settings for pagination.
        /// </summary>
        public PaginationSettings PaginationSettings
        {
            get
            {
                return _paginationSettings;
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
                bool countersEnabled = false;
                bool allowTags = true;
                var tagSettings = new FileTagSelectionSettings();
                bool paginationEnabled = false;
                var paginationSettings = new PaginationSettings();

                if (reader.Version >= 2)
                {
                    configFileName = reader.ReadString();
                }
                if (reader.Version >= 3 && reader.Version <= 6)
                {
                    reader.ReadBoolean();
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

                if (reader.Version >= 6)
                {
                    paginationEnabled = reader.ReadBoolean();
                    paginationSettings = PaginationSettings.ReadFrom(reader);
                }

                return new VerificationSettings(configFileName, countersEnabled, 
                    allowTags, tagSettings, paginationEnabled, paginationSettings);
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
                writer.Write(ConfigFileName);
                writer.Write(CountersEnabled);
                writer.Write(AllowTags);
                _tagSettings.WriteTo(writer);
                writer.Write(PaginationEnabled);
                _paginationSettings.WriteTo(writer);
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
