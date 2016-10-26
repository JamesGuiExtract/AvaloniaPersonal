using Extract.Licensing;
using System;
using System.ComponentModel;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Defines a column that can be added to a <see cref="DataEntryTable"/> in order to allow
    /// selection of the proper CSN from the encounters stored in the FAMDB. The column will
    /// provide a button that opens a UI to allow the user to view and select from the possible
    /// matching encounters.
    /// </summary>
    public class EncounterPickerTableColumn : RecordPickerTableColumn
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(EncounterPickerTableColumn).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        /// <summary>
        /// The <see cref="EncounterDataConfiguration"/> defining data to retrieve data from a FAM
        /// database.
        /// </summary>
        EncounterDataConfiguration _dataConfiguration;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EncounterPickerTableColumn"/> class.
        /// </summary>
        public EncounterPickerTableColumn()
            : base()
        {
            try
            {
                // Because LicenseUsageMode.UsageMode isn't always accurate, this will be re-checked
                // in OnDataGridViewChanged.
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                // Load licenses in design mode
                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI41562", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41563");
            }
        }

        #endregion Constructors

        #region Properties
        
        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the encounter date. The
        /// path should either be rooted or be relative to the LabDE encounter attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the encounter date.
        /// </value>
        [DefaultValue(EncounterDataConfiguration._DEFAULT_DATE_ATTRIBUTE_PATH)]
        [Category(DesignerGridCategory)]
        public string EncounterDateAttribute
        {
            get
            {
                try
                {
                    return EncounterDataConfiguration.EncounterDateAttributePath;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41564");
                }
            }

            set
            {
                try
                {
                    EncounterDataConfiguration.EncounterDateAttributePath = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41565");
                }
            }
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the encounter time. The
        /// path should either be rooted or be relative to the LabDE encounter attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the encounter time.
        /// </value>
        [DefaultValue(EncounterDataConfiguration._DEFAULT_TIME_ATTRIBUTE_PATH)]
        [Category(DesignerGridCategory)]
        public string EncounterTimeAttribute
        {
            get
            {
                try
                {
                    return EncounterDataConfiguration.EncounterTimeAttributePath;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41566");
                }
            }

            set
            {
                try
                {
                    EncounterDataConfiguration.EncounterTimeAttributePath = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41567");
                }
            }
        }

        #endregion Properties  

        #region Overrides

        /// <summary>
        /// Gets the data configuration.
        /// </summary>
        /// <value>
        /// The data configuration.
        /// </value>
        internal override IFAMDataConfiguration DataConfiguration
        {
            get
            {
                return EncounterDataConfiguration;
            }
        }

        /// <summary>
        /// Creates an exact copy of the <see cref="DataEntryTableColumn"/> instance.
        /// </summary>
        /// <returns>An exact copy of the <see cref="DataEntryTableColumn"/> instance.</returns>
        public override object Clone()
        {
            try
            {
                EncounterPickerTableColumn column = (EncounterPickerTableColumn)base.Clone();

                column.AutoPopulate = this.AutoPopulate;
                column.AutoSelectionFilter = this.AutoSelectionFilter;
                column.AutoSelectionRecord = this.AutoSelectionRecord;
                column.EncounterDateAttribute = this.EncounterDateAttribute;
                column.EncounterTimeAttribute = this.EncounterTimeAttribute;
                column.ColorQueryConditions = this.ColorQueryConditions;
                column.RecordIdAttribute = this.RecordIdAttribute;
                column.RecordIdColumn = this.RecordIdColumn;
                column.RecordIdColumn = this.RecordIdColumn;
                column.RecordMatchCriteria = this.RecordMatchCriteria;
                column.RecordQueryColumns = this.RecordQueryColumns;

                return column;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI41568", ex);
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Gets the <see cref="EncounterDataConfiguration"/> defining data to retrieve data from a
        /// FAM database.
        /// </summary>
        /// <value>
        /// The <see cref="EncounterDataConfiguration"/> defining data to retrieve data from a FAM
        /// database.
        /// </value>
        EncounterDataConfiguration EncounterDataConfiguration
        {
            get
            {
                if (_dataConfiguration == null)
                {
                    _dataConfiguration = new EncounterDataConfiguration();
                }

                return _dataConfiguration;
            }
        }

        #endregion Private Members
    }
}
