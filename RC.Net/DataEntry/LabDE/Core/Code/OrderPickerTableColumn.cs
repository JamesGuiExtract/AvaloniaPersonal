﻿using Extract.Licensing;
using System;
using System.ComponentModel;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Defines a column that can be added to a <see cref="DataEntryTable"/> in order to allow
    /// selection of the proper order number from the orders stored in the FAMDB. The column will
    /// provide a button that opens a UI to allow the user to view and select from the possible
    /// matching orders.
    /// </summary>
    public class OrderPickerTableColumn : RecordPickerTableColumn
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(OrderPickerTableColumn).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        /// <summary>
        /// The <see cref="OrderDataConfiguration"/> defining data to retrieve data from a FAM
        /// database.
        /// </summary>
        OrderDataConfiguration _dataConfiguration;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickerTableColumn"/> class.
        /// </summary>
        public OrderPickerTableColumn()
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
                    LicenseIdName.LabDECoreObjects, "ELI41555", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41556");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the collection date. The
        /// path should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the collection date.
        /// </value>
        [DefaultValue(OrderDataConfiguration._DEFAULT_COLLECTION_DATE_ATTRIBUTE_PATH)]
        [Category(DesignerGridCategory)]
        public string CollectionDateAttribute
        {
            get
            {
                try
                {
                    return OrderDataConfiguration.CollectionDateAttributePath;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41557");
                }
            }

            set
            {
                try
                {
                    OrderDataConfiguration.CollectionDateAttributePath = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41558");
                }
            }
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the collection time. The
        /// path should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the collection time.
        /// </value>
        [DefaultValue(OrderDataConfiguration._DEFAULT_COLLECTION_TIME_ATTRIBUTE_PATH)]
        [Category(DesignerGridCategory)]
        public string CollectionTimeAttribute
        {
            get
            {
                try
                {
                    return OrderDataConfiguration.CollectionTimeAttributePath;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41559");
                }
            }

            set
            {
                try
                {
                    OrderDataConfiguration.CollectionTimeAttributePath = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41560");
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
                return OrderDataConfiguration;
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
                OrderPickerTableColumn column = (OrderPickerTableColumn)base.Clone();

                column.AutoPopulate = this.AutoPopulate;
                column.AutoSelectionFilter = this.AutoSelectionFilter;
                column.AutoSelectionRecord = this.AutoSelectionRecord;
                column.CollectionDateAttribute = this.CollectionDateAttribute;
                column.CollectionTimeAttribute = this.CollectionTimeAttribute;
                column.ColorQueryConditions = this.ColorQueryConditions;
                column.RecordIdAttribute = this.RecordIdAttribute;
                column.RecordIdColumn = this.RecordIdColumn;
                column.RecordMatchCriteria = this.RecordMatchCriteria;
                column.RecordQueryColumns = this.RecordQueryColumns;

                return column;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI41561", ex);
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Gets the <see cref="OrderDataConfiguration"/> defining data to retrieve data from a FAM
        /// database.
        /// </summary>
        /// <value>
        /// The <see cref="OrderDataConfiguration"/> defining data to retrieve data from a FAM
        /// database.
        /// </value>
        OrderDataConfiguration OrderDataConfiguration
        {
            get
            {
                if (_dataConfiguration == null)
                {
                    _dataConfiguration = new OrderDataConfiguration();
                }

                return _dataConfiguration;
            }
        }

        #endregion Private Members
    }
}
