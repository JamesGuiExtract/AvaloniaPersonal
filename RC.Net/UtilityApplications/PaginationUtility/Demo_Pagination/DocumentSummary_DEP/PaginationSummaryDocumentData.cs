using Extract.Utilities;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DocumentSummary
{
    /// <summary>
    /// The <see cref="PaginationDocumentData"/> instance to use for <see cref="PaginationDocumentDataPanel"/>
    /// </summary>
    internal class PaginationSummaryDocumentData : PaginationDocumentData
    {
        #region Fields

        /// <summary>
        /// The data's <see cref="PaginationDataField"/>s
        /// </summary>
        Dictionary<string, PaginationDataField> _fields;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSummaryDocumentData"/> class.
        /// </summary>
        /// <param name="attributes">The attributes representing the data.</param>
        public PaginationSummaryDocumentData(IUnknownVector attributes, string sourceDocName)
            : base(attributes, sourceDocName)
        {
            try
            {
                // Prevent adding attributes for a top-level document with suggested paginations.
                var rootAttributeNames = attributes.ToIEnumerable<IAttribute>()
                    .Select(attribute => attribute.Name)
                    .Distinct()
                    .ToArray();
                if (rootAttributeNames.Length == 1 && rootAttributeNames.Single() == "Document")
                {
                    return;
                }

                // Inherit data from source document if none exists for this one.
                if (attributes.Size() == 0 && File.Exists(sourceDocName + ".voa"))
                {
                    attributes.LoadFrom(sourceDocName + ".voa", false);
                }

                _fields = new Dictionary<string, PaginationDataField>()
                    {
                        {
                            "DocumentType", new PaginationDataField("DocumentType")
                        },

                        {
                            "PatientFirst", new PaginationDataField("PatientInfo", "Name", "First")
                        },

                        {
                            "PatientLast", new PaginationDataField("PatientInfo", "Name", "Last")
                        },
                    };

                // In order to avoid an issue in DataEntryApplicationForm where this field, as a part of
                // _paginagionAttributesToRefresh (sic), is expected to return an attribute, make sure an
                // attribute for each fields exists.
                foreach (var field in _fields.Values)
                {
                    field.TreatAsUnmodified = true;
                    if (field.GetAttribute(attributes) == null)
                    {
                        SetAttributeValue(field, " ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49991");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the value of the DocumentType field.
        /// </summary>
        public string DocumentType
        {
            get
            {
                return GetAttributeValue(Fields["DocumentType"]);
            }

            set
            {
                SetAttributeValue(Fields["DocumentType"], value);
            }
        }

        /// <summary>
        ///  Gets or sets the value of the PatientFirst field.
        /// </summary>
        public string PatientFirst
        {
            get
            {
                return GetAttributeValue(Fields["PatientFirst"]);
            }

            set
            {
                SetAttributeValue(Fields["PatientFirst"], value);
            }
        }

        /// <summary>
        ///  Gets or sets the value of the PatientLast field.
        /// </summary>
        public string PatientLast
        {
            get
            {
                return GetAttributeValue(Fields["PatientLast"]);
            }

            set
            {
                SetAttributeValue(Fields["PatientLast"], value);
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Maps all field names for the extending class with the <see cref="PaginationDataField"/>
        /// that defines the field.
        /// </summary>
        protected override Dictionary<string, PaginationDataField> Fields
        {
            get
            {
                return _fields;
            }
        }

        /// <summary>
        /// A description of the document.
        /// </summary>
        public override string Summary
        {
            get
            {
                string name = string.Format(CultureInfo.CurrentCulture, "{0} {1}", PatientFirst, PatientLast);
                name = string.IsNullOrWhiteSpace(name)
                    ? "<UNKNOWN PATIENT>"
                    : name.ToUpper();

                string documentType = string.IsNullOrWhiteSpace(DocumentType)
                    ? "<Unknown document type>"
                    : DocumentType;

                return string.Format(CultureInfo.CurrentCulture,
                    "{0} - {1}", name, documentType);
            }
        }

        #endregion Overrides
    }
}
