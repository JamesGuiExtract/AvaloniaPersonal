using Extract.AttributeFinder;
using System;
using System.Collections.Generic;
using UCLID_COMUTILSLib;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="PaginationDocumentData"/> derivation that tracks data associated with a
    /// <see cref="DataEntryDocumentDataPanel"/>.
    /// </summary>
    public class DataEntryPaginationDocumentData : PaginationDocumentData
    {
        #region Fields

        /// <summary>
        /// A copy of the original data loaded (to implement revert functionality)
        /// </summary>
        IUnknownVector _originalData;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DataEntryPaginationDocumentData"/>.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
        /// this data is based on.
        /// </param>
        /// <param name="sourceDocName">The source document related to <see cref="_documentData"/>
        /// if there is a singular source document; otherwise <see langword="null"/>.</param>
        public DataEntryPaginationDocumentData(IUnknownVector attributes, string sourceDocName)
            : base(attributes)
        {
            try
            {
                SourceDocName = sourceDocName;

                _originalData = (IUnknownVector)((ICopyableObject)attributes).Clone();
                _originalData.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41357");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The source document related to this instance if there is a singular source document;
        /// otherwise, <see langword="null"/>.
        /// </summary>
        public string SourceDocName
        {
            get;
            private set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// A description of the document.
        /// </summary>
        public override string Summary
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Gets a value indicating whether editing of this data is allowed.
        /// </summary>
        /// <value><see langword="true"/> if data editing is allowed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public override bool AllowDataEdit
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Unused base class implementation.
        /// </summary>
        protected override Dictionary<string, PaginationDataField> Fields
        {
            get
            {
                return new Dictionary<string, PaginationDataField>();
            }
        }

        /// <summary>
        /// Reverts the data back to its original values.
        /// </summary>
        public override void Revert()
        {
            try
            {
                base.Attributes = _originalData;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41358");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance wants to override whether the document
        /// is returned to the server for reprocessing.
        /// </summary>
        /// <value><see langword="null"/> to if the decision should not be overridden, otherwise
        /// a boolean value indicating what the override should be.</value>
        public override bool? SendForReprocessing

        {
            get
            {
                return base.SendForReprocessing;
            }
        }

        #endregion Overrides
    }
}
