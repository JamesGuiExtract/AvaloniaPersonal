using Extract.Database;
using Extract.Imaging.Forms;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Demo_Pagination
{
    /// <summary>
    /// The <see cref="PaginationDocumentData"/> instance to use for <see cref="PaginationDocumentDataPanel"/>
    /// </summary>
    internal class Demo_PaginationDocumentData : PaginationDocumentData
    {
        #region Fields

        /// <summary>
        /// The data's <see cref="PaginationDataField"/>s
        /// </summary>
        Dictionary<string, PaginationDataField> _fields;

        /// <summary>
        /// The <see cref="PaginationDataField"/> currently identified as invalid.
        /// </summary>
        HashSet<string> _invalidFields = new HashSet<string>();

        /// <summary>
        /// The fields used from the connected FAM database.
        /// </summary>
        Dictionary<string, string> _dbFields;

        /// <summary>
        /// The FAM DB being used.
        /// </summary>
        IFileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The DB connection used for queries to compare entered data against <see cref="_fileProcessingDB"/>.
        /// </summary>
        [ThreadStatic]
        OleDbConnection _oleDbConnection;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Demo_PaginationDocumentData"/> class.
        /// </summary>
        /// <param name="attributes">The attributes representing the data.</param>
        /// <param name="fileProcessingDB">The file processing database being used.</param>
        /// <param name="imageViewer">The image viewer being used.</param>
        public Demo_PaginationDocumentData(IUnknownVector attributes, FileProcessingDB fileProcessingDB,
            ImageViewer imageViewer)
            : base(attributes)
        {
            try
            {
                _fileProcessingDB = fileProcessingDB;

                _fields = new Dictionary<string, PaginationDataField>()
            {
                {
                    "DocumentType", new PaginationDataField("DocumentType")
                },

                {
                    "DocumentDate", new PaginationDataField("DocumentDate")
                },

                {
                    "DocumentComment", new PaginationDataField("DocumentComment")
                },

                {
                    "PatientFirst", new PaginationDataField("PatientInfo", "Name", "First")
                },

                {
                    "PatientMiddle", new PaginationDataField("PatientInfo", "Name", "Middle")
                },

                {
                    "PatientLast", new PaginationDataField("PatientInfo", "Name", "Last")
                },

                {
                    "PatientDOB", new PaginationDataField("PatientInfo", "DOB")
                },

                {
                    "PatientSex", new PaginationDataField("PatientInfo", "Gender")
                },

                {
                    "PatientMRN", new PaginationDataField("PatientInfo", "MR_Number")
                },

                {
                    "ReferralFirst", new PaginationDataField("Referral", "Name", "First")
                },

                {
                    "ReferralMiddle", new PaginationDataField("Referral", "Name", "Middle")
                },

                {
                    "ReferralLast", new PaginationDataField("Referral", "Name", "Last")
                },

                {
                    "ReferralAddress", new PaginationDataField("Referral", "Address")
                },

                {
                    "BloodType", new PaginationDataField("BloodType")
                },

                {
                    "BloodRh", new PaginationDataField("Rh")
                },

                {
                    "HistoryHighlight1", new PaginationDataField("PatientHighlights", "PH1")
                },

                {
                    "HistoryHighlight2", new PaginationDataField("PatientHighlights", "PH2")
                },

                {
                    "HistoryHighlight3", new PaginationDataField("PatientHighlights", "PH3")
                },

                {
                    "HistoryHighlight4", new PaginationDataField("PatientHighlights", "PH4")
                },

                {
                    "HistoryHighlight5", new PaginationDataField("PatientHighlights", "PH5")
                },

                {
                    "LabCollectionDate", new PaginationDataField("CollectionDate")
                },

                {
                    "LabCollectionTime", new PaginationDataField("CollectionTime")
                },

                {
                    "RadiologyProcedure", new PaginationDataField("RadiologyProcedure")
                },

                {
                    "RadiologyImpression", new PaginationDataField("RadiologyImpression")
                },

                {
                    "InsuranceProvider", new PaginationDataField("InsuranceProvider")
                }
            };

                _dbFields = new Dictionary<string, string>()
            {
                {
                    "PatientFirst", "FirstName"
                },

                {
                    "PatientLast", "LastName"
                },

                {
                    "PatientDOB", "DOB"
                },

                {
                    "PatientMRN", "MRN"
                },
            };

                SetOriginalForm();

                if (string.IsNullOrWhiteSpace(PatientMRN))
                {
                    PatientMRN = LookupMRN(PatientFirst, PatientLast, PatientDOB);
                }

                IsValueValid("PatientFirst");
                IsValueValid("PatientLast");
                IsDateValid("PatientDOB");
                IsMRNValid();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41376");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when <see cref="Revert"/> has been called.
        /// </summary>
        public event EventHandler<EventArgs> DataReverted;

        #endregion Events

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
                OnDocumentDataStateChanged();
            }
        }

        /// <summary>
        ///  Gets or sets the value of the DocumentDate field.
        /// </summary>
        public string DocumentDate
        {
            get
            {
                return GetAttributeValue(Fields["DocumentDate"]);
            }

            set
            {
                SetAttributeValue(Fields["DocumentDate"], value);
                OnDocumentDataStateChanged();
            }
        }

        /// <summary>
        ///  Gets or sets the value of the DocumentComment field.
        /// </summary>
        public string DocumentComment
        {
            get
            {
                return GetAttributeValue(Fields["DocumentComment"]);
            }

            set
            {
                SetAttributeValue(Fields["DocumentComment"], value);
                OnDocumentDataStateChanged();
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
                OnDocumentDataStateChanged();
            }
        }

        /// <summary>
        ///  Gets or sets the value of the PatientMiddle field.
        /// </summary>
        public string PatientMiddle
        {
            get
            {
                return GetAttributeValue(Fields["PatientMiddle"]);
            }

            set
            {
                SetAttributeValue(Fields["PatientMiddle"], value);
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
                OnDocumentDataStateChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the PatientDOB field.
        /// </summary>
        public string PatientDOB
        {
            get
            {
                return GetAttributeValue(Fields["PatientDOB"]);
            }

            set
            {
                SetAttributeValue(Fields["PatientDOB"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the PatientSex field.
        /// </summary>
        public string PatientSex
        {
            get
            {
                return GetAttributeValue(Fields["PatientSex"]);
            }

            set
            {
                SetAttributeValue(Fields["PatientSex"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the PatientMRN field.
        /// </summary>
        public string PatientMRN
        {
            get
            {
                return GetAttributeValue(Fields["PatientMRN"]);
            }

            set
            {
                SetAttributeValue(Fields["PatientMRN"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the ReferralFirst field.
        /// </summary>
        public string ReferralFirst
        {
            get
            {
                return GetAttributeValue(Fields["ReferralFirst"]);
            }

            set
            {
                SetAttributeValue(Fields["ReferralFirst"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the ReferralMiddle field.
        /// </summary>
        public string ReferralMiddle
        {
            get
            {
                return GetAttributeValue(Fields["ReferralMiddle"]);
            }

            set
            {
                SetAttributeValue(Fields["ReferralMiddle"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the ReferralLast field.
        /// </summary>
        public string ReferralLast
        {
            get
            {
                return GetAttributeValue(Fields["ReferralLast"]);
            }

            set
            {
                SetAttributeValue(Fields["ReferralLast"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the ReferralAddress field.
        /// </summary>
        public string ReferralAddress
        {
            get
            {
                return GetAttributeValue(Fields["ReferralAddress"]);
            }

            set
            {
                SetAttributeValue(Fields["ReferralAddress"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the BloodType field.
        /// </summary>
        public string BloodType
        {
            get
            {
                return GetAttributeValue(Fields["BloodType"]);
            }

            set
            {
                SetAttributeValue(Fields["BloodType"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the BloodRh field.
        /// </summary>
        public string BloodRh
        {
            get
            {
                return GetAttributeValue(Fields["BloodRh"]);
            }

            set
            {
                SetAttributeValue(Fields["BloodRh"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the HistoryHighlight1 field.
        /// </summary>
        public string HistoryHighlight1
        {
            get
            {
                return GetAttributeValue(Fields["HistoryHighlight1"]);
            }

            set
            {
                SetAttributeValue(Fields["HistoryHighlight1"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the HistoryHighlight2 field.
        /// </summary>
        public string HistoryHighlight2
        {
            get
            {
                return GetAttributeValue(Fields["HistoryHighlight2"]);
            }

            set
            {
                SetAttributeValue(Fields["HistoryHighlight2"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the HistoryHighlight3 field.
        /// </summary>
        public string HistoryHighlight3
        {
            get
            {
                return GetAttributeValue(Fields["HistoryHighlight3"]);
            }

            set
            {
                SetAttributeValue(Fields["HistoryHighlight3"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the HistoryHighlight4 field.
        /// </summary>
        public string HistoryHighlight4
        {
            get
            {
                return GetAttributeValue(Fields["HistoryHighlight4"]);
            }

            set
            {
                SetAttributeValue(Fields["HistoryHighlight4"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the HistoryHighlight5 field.
        /// </summary>
        public string HistoryHighlight5
        {
            get
            {
                return GetAttributeValue(Fields["HistoryHighlight5"]);
            }

            set
            {
                SetAttributeValue(Fields["HistoryHighlight5"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the LabCollectionDate field.
        /// </summary>
        public string LabCollectionDate
        {
            get
            {
                return GetAttributeValue(Fields["LabCollectionDate"]);
            }

            set
            {
                SetAttributeValue(Fields["LabCollectionDate"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the LabCollectionTime field.
        /// </summary>
        public string LabCollectionTime
        {
            get
            {
                return GetAttributeValue(Fields["LabCollectionTime"]);
            }

            set
            {
                SetAttributeValue(Fields["LabCollectionTime"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the RadiologyProcedure field.
        /// </summary>
        public string RadiologyProcedure
        {
            get
            {
                return GetAttributeValue(Fields["RadiologyProcedure"]);
            }

            set
            {
                SetAttributeValue(Fields["RadiologyProcedure"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the RadiologyImpression field.
        /// </summary>
        public string RadiologyImpression
        {
            get
            {
                return GetAttributeValue(Fields["RadiologyImpression"]);
            }

            set
            {
                SetAttributeValue(Fields["RadiologyImpression"], value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the InsuranceProvider field.
        /// </summary>
        public string InsuranceProvider
        {
            get
            {
                return GetAttributeValue(Fields["InsuranceProvider"]);
            }

            set
            {
                SetAttributeValue(Fields["InsuranceProvider"], value);
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Attempts an MRN look-up given the provided demographic data.
        /// </summary>
        /// <param name="firstName">The patient first name.</param>
        /// <param name="lastName">The patient last name.</param>
        /// <param name="DOB">The patient date of birth.</param>
        /// <returns>The MRN from a single matching record in the database; otherwise, empty.</returns>
        public string LookupMRN(string firstName, string lastName, string DOB)
        {
            try
            {
                string query =
                        "SELECT [MRN] FROM [LabDEPatient]" +
                        "   WHERE [FirstName] = ?" +
                        "   AND [LastName] = ?" +
                        "   AND [DOB] = CASE WHEN ISDATE(?) = 1 THEN CONVERT(DATETIME, ?) ELSE GETDATE() END";

                var parameters = new Dictionary<string, string>
            {
                {"?0", firstName},
                {"?1", lastName},
                {"?2", DOB},
                {"?3", DOB}
            };

                var results = DBMethods.GetQueryResultsAsStringArray(
                    OleDbConnection, query, parameters, ",");

                return (results.Length == 1)
                    ? results.First()
                    : "";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41378");
            }
        }

        /// <summary>
        /// Gets potential matching MRNs for the currently entered demographic data based on matching two of the
        /// following 3 fields: first name, last name or DOB.
        /// </summary>
        /// <returns>An array of potentially matching MRNs.</returns>
        public string[] GetPossibleMRNs()
        {
            try
            {
                string query =
                        "SELECT '[' + [MRN] + '] ' + [FirstName] + ' ' + [LastName] + ', DOB: ' + CONVERT(NVARCHAR(10), [DOB], 110) " +
                        "    FROM [LabDEPatient] " +
                        "WHERE " +
                        // Offer auto-complete for any patients that match 2 of 3 fields
                        // If this query is changed, be sure to test against a large database
                        "([LastName] LIKE (CASE WHEN LEN(?) > 1 THEN SUBSTRING(?,1,50) + '%' ELSE '' END) " +
                        "AND [FirstName] LIKE (CASE WHEN LEN(?) > 0 THEN SUBSTRING(?,1,50) + '%' ELSE '' END)) " +
                        "OR " +
                        "([LastName] LIKE (CASE WHEN LEN(?) > 1 THEN SUBSTRING(?,1,50) + '%' ELSE '' END) " +
                        "AND CONVERT(NVARCHAR(10), [DOB], 101) = SUBSTRING(?,1,10)) " +
                        "OR " +
                        "([FirstName] LIKE (CASE WHEN LEN(?) > 0 THEN SUBSTRING(?,1,50) + '%' ELSE '' END) " +
                        "AND CONVERT(NVARCHAR(10), [DOB], 101) = SUBSTRING(?,1,10)) ";

                var parameters = new Dictionary<string, string>
            {
                {"?0", PatientLast},
                {"?1", PatientLast},
                {"?2", PatientFirst},
                {"?3", PatientFirst},
                {"?4", PatientLast},
                {"?5", PatientLast},
                {"?6", PatientDOB},
                {"?7", PatientFirst},
                {"?8", PatientFirst},
                {"?9", PatientDOB},
            };

                return DBMethods.GetQueryResultsAsStringArray(
                    OleDbConnection, query, parameters, ",");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41379");
            }
        }

        /// <summary>
        /// Determines whether the currently entered MRN is valid.
        /// </summary>
        /// <returns><c>true</c> if the currently entered MRN is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMRNValid()
        {
            try
            {
                bool valid = false;

                if (PatientMRN.Equals("unknown", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(PatientMRN) &&
                    !string.IsNullOrEmpty(PatientFirst))
                {
                    valid = false;
                }
                else
                {
                    string query = string.Format(
                        "SELECT TOP(1) 1 FROM [LabDEPatient]" +
                        "    WHERE LEN(?) = 0 OR ? = [MRN]");

                    string mrn = GetAttributeValue(Fields["PatientMRN"]);

                    var parameters = new Dictionary<string, string>
                {
                    {"?0", mrn},
                    {"?1", mrn}
                };

                    valid = (DBMethods.GetQueryResultsAsStringArray(
                        OleDbConnection, query, parameters, ",").Length > 0);
                }

                SetValidity("PatientMRN", valid);

                return valid;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41380");
            }
        }

        /// <summary>
        /// Determines whether the current value of the specified <paramref name="fieldName"/> is valid
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns><c>true</c> if field's value is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValueValid(string fieldName)
        {
            try
            {
                string mrn = PatientMRN;
                if (mrn.Equals("unknown", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }

                string value = GetAttributeValue(Fields[fieldName]);

                string query = string.Format(
                    "SELECT TOP(1) 1 FROM [LabDEPatient]" +
                    "    WHERE LEN(?) = 0" +
                    "    OR ([MRN] = ? AND ? = [{0}])", _dbFields[fieldName]);

                var parameters = new Dictionary<string, string>
            {
                {"?0", mrn},
                {"?1", mrn},
                {"?2", value}
            };

                bool valid = (DBMethods.GetQueryResultsAsStringArray(
                    OleDbConnection, query, parameters, ",").Length > 0);

                SetValidity(fieldName, valid);

                return valid;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41381");
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="DateTime"/> value of the specified 
        /// <paramref name="fieldName"/> is valid
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns><c>true</c> if field's value is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDateValid(string fieldName)
        {
            try
            {
                string mrn = PatientMRN;
                if (mrn.Equals("unknown", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }

                string value = GetAttributeValue(Fields[fieldName]);

                string query = string.Format(
                    "SELECT TOP(1) 1 FROM [LabDEPatient]" +
                    "    WHERE LEN(?) = 0" +
                    "    OR ([MRN] = ?" +
                    "       AND CASE WHEN ISDATE(?) = 1 THEN CONVERT(DATETIME, ?) ELSE GETDATE() END = [DOB])",
                        fieldName);

                var parameters = new Dictionary<string, string>
            {
                {"?0", mrn},
                {"?1", mrn},
                {"?2", value},
                {"?3", value}
            };

                bool valid = (DBMethods.GetQueryResultsAsStringArray(
                    OleDbConnection, query, parameters, ",").Length > 0);

                SetValidity(fieldName, valid);

                return valid;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41382");
            }
        }

        /// <summary>
        /// Sets the validity of the specified <see cref="fieldName"/>.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="valid"><c>true</c> to mark the field as valid; <c>false</c> to mark it as invalid.</param>
        public void SetValidity(string fieldName, bool valid)
        {
            try
            {
                bool previousState = _invalidFields.Any();

                if (valid)
                {
                    _invalidFields.Remove(fieldName);
                }
                else
                {
                    _invalidFields.Add(fieldName);
                }

                bool newState = _invalidFields.Any();

                if (previousState != newState)
                {
                    OnDocumentDataStateChanged();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41383");
            }
        }

        #endregion Methods

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
        /// Gets whether this instances data has been modified.
        /// </summary>
        public override bool DataError
        {
            get
            {
                return _invalidFields.Any();
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
        /// A description of the document.
        /// </summary>
        public override string Summary
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture,
                    "{0}     {1} {2}     {3}", DocumentType, PatientFirst, PatientLast, DocumentDate);
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
                if (!string.IsNullOrWhiteSpace(DocumentType) &&
                    !DocumentType.Equals("Lab Results", StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return base.SendForReprocessing;
                }
            }
        }

        /// <summary>
        /// Reverts the data back to its original values.
        /// </summary>
        public override void Revert()
        {
            base.Revert();

            OnDataReverted();
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Gets the DB connection used for queries to compare entered data against <see cref="_fileProcessingDB"/>.
        /// </summary>
        OleDbConnection OleDbConnection
        {
            get
            {
                if (_oleDbConnection == null)
                {
                    ExtractException.Assert("ELI41377", "Missing database connection.",
                        _fileProcessingDB != null);

                    _oleDbConnection = new OleDbConnection(_fileProcessingDB.ConnectionString);
                    _oleDbConnection.Open();
                }

                return _oleDbConnection;
            }
        }

        /// <summary>
        /// Raises the <see cref="DataReverted"/> event.
        /// </summary>
        void OnDataReverted()
        {
            var eventHandler = DataReverted;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
