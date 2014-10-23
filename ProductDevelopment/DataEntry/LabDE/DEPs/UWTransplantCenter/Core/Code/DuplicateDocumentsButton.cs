using ADODB;
using Extract.FileActionManager.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.DEP.UWTransplantCenter
{
    /// <summary>
    /// A <see cref="DataEntryButton"/> extension that allows for the display of documents that
    /// appear to be duplicates of the current document in a <see cref="FAMFileInspectorForm"/>
    /// such that appropriate action may be taken to resolve the duplicates.
    /// See: https://extract.atlassian.net/browse/CUST-409
    /// </summary>
    internal class DuplicateDocumentsButton : DataEntryButton
    {
        #region Constants

        /// <summary>
        /// Template for the button text.
        /// </summary>
        const string _BUTTON_LABEL_TEXT = "{0} duplicate document(s)...";

        /// <summary>
        /// Default tag to apply to documents that have been ignored.
        /// </summary>
        const string _DEFAULT_TAG_FOR_IGNORE = "User_Ignore document";

        /// <summary>
        /// Default output path for stapled document output.
        /// </summary>
        const string _DEFAULT_STAPLED_DOCUMENT_OUTPUT =
            @"$DirOf($DirOf(<SourceDocName>))\Lab\Stapled_<FirstName>_<LastName>_$Now().tif";
        
        /// <summary>
        /// Default tag to apply to documents that have been stapled.
        /// </summary>
        const string _DEFAULT_TAG_FOR_STAPLE = "";

        /// <summary>
        /// Default metadata field name that should be updated whenever a document is stapled.
        /// </summary>
        const string _DEFAULT_STAPLED_INTO_METADATA_FIELD_NAME = "StapledInto";

        /// <summary>
        /// Default metadata value to be applied.
        /// </summary>
        const string _DEFAULT_STAPLED_INTO_METADATA_FIELD_VALUE = "<StapledDocumentOutput>";

        /// <summary>
        /// Default name of the database metadata field that stores the patient first name.
        /// </summary>
        const string _DEFAULT_PATIENT_FIRST_NAME_METADATA_FIELD = "PatientFirstName";

        /// <summary>
        /// Default name of the database metadata field that stores the patient last name.
        /// </summary>
        const string _DEFAULT_PATIENT_LAST_NAME_METADATA_FIELD = "PatientLastName";

        /// <summary>
        /// Default name of the database metadata field that stores the patient date of birth.
        /// </summary>
        const string _DEFAULT_PATIENT_DOB_METADATA_FIELD = "PatientDOB";

        /// <summary>
        /// Default  name of the database metadata field that stores the document's collection date(s).
        /// </summary>
        const string _DEFAULT_COLLECTION_DATE_METADATA_FIELD = "CollectionDate";
        
        #endregion Constants

        #region Fields

        /// <summary>
        /// Used to launch a <see cref="FAMFileInspectorForm"/> to display the duplicate documents.
        /// </summary>
        FAMFileInspectorComLibrary _famFileInspector = new FAMFileInspectorComLibrary();

        /// <summary>
        /// A custom column for the FFI that allows for various actions to be performed on the
        /// displayed documents.
        /// </summary>
        DuplicateDocumentsFFIColumn _ffiActionColumn;

        /// <summary>
        /// A custom column that displays the status of the files prior to being loaded into the
        /// FFI.
        /// </summary>
        PreviousStatusFFIColumn _ffiStatusColumn;

        /// <summary>
        /// A vector of all custom columns to be used in the FFI.
        /// </summary>
        IUnknownVector _ffiCustomColumns = new IUnknownVector();

        /// <summary>
        /// The patient first name for the current document.
        /// </summary>
        string _firstName;

        /// <summary>
        /// The patient last name for the current document.
        /// </summary>
        string _lastName;

        /// <summary>
        /// The patient date of birth for the current document.
        /// </summary>
        string _dob;

        /// <summary>
        /// A comma-delimited list of collection dates for the current document.
        /// </summary>
        string _collectionDate;

        /// <summary>
        /// The original <see cref="P:Control.BackColor"/> for the button. Used to restore the color
        /// after having set it to the <see cref="DataEntryButton.FlashColor"/>.
        /// </summary>
        Color _originalBackColor;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateDocumentsButton"/> class.
        /// </summary>
        public DuplicateDocumentsButton()
        {
            try
            {
                _originalBackColor = BackColor;

                _ffiActionColumn = new DuplicateDocumentsFFIColumn();
                _ffiStatusColumn = new PreviousStatusFFIColumn(_ffiActionColumn);

                _ffiCustomColumns.PushBack(_ffiActionColumn);
                _ffiCustomColumns.PushBack(_ffiStatusColumn);

                TagForIgnore = _DEFAULT_TAG_FOR_IGNORE;
                StapledDocumentOutput = _DEFAULT_STAPLED_DOCUMENT_OUTPUT;
                TagForStaple = _DEFAULT_TAG_FOR_STAPLE;
                StapledIntoMetadataFieldName = _DEFAULT_STAPLED_INTO_METADATA_FIELD_NAME;
                StapledIntoMetadataFieldValue = _DEFAULT_STAPLED_INTO_METADATA_FIELD_VALUE;
                PatientFirstNameMetadataField = _DEFAULT_PATIENT_FIRST_NAME_METADATA_FIELD;
                PatientLastNameMetadataField = _DEFAULT_PATIENT_LAST_NAME_METADATA_FIELD;
                PatientDOBMetadataField = _DEFAULT_PATIENT_DOB_METADATA_FIELD;
                CollectionDateMetadataField = _DEFAULT_COLLECTION_DATE_METADATA_FIELD;

                // DataReset handler ensures property values aren't carried over from one document
                // to the next.
                AttributeStatusInfo.DataReset += HandleAttributeStatusInfo_DataReset;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37442");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a tag to apply to documents that have been ignored.
        /// </summary>
        /// <value>
        /// A tag to apply to documents that have been ignored or <see langword="null"/> if no tag
        /// is to be applied to ignored documents.
        /// </value>
        [Category("UW Custom Setting")]
        [DefaultValue(_DEFAULT_TAG_FOR_IGNORE)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string TagForIgnore
        {
            get
            {
                return _ffiActionColumn.TagForIgnore;
            }

            set
            {
                _ffiActionColumn.TagForIgnore = value;
            }
        }

        /// <summary>
        /// Gets or sets the output path for stapled document output. Path tags/functions are
        /// supported including the tags &lt;SourceDocName&gt;, &lt;FirstName&gt; &lt;LastName&gt;,
        /// &lt;DOB&gt; and &lt;CollectionDate&gt;
        /// </summary>
        /// <value>
        /// The output path for stapled document output.
        /// </value>
        [Category("UW Custom Setting")]
        [DefaultValue(_DEFAULT_STAPLED_DOCUMENT_OUTPUT)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string StapledDocumentOutput
        {
            get
            {
                return _ffiActionColumn.StapledDocumentOutput;
            }

            set
            {
                _ffiActionColumn.StapledDocumentOutput = value;
            }
        }

        /// <summary>
        /// Gets or sets a tag to apply to documents that have been stapled.
        /// </summary>
        /// <value>
        /// A tag to apply to documents that have been stapled or <see langword="null"/> if no tag
        /// is to be applied to stapled documents.
        /// </value>
        [Category("UW Custom Setting")]
        [DefaultValue(_DEFAULT_TAG_FOR_STAPLE)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string TagForStaple
        {
            get
            {
                return _ffiActionColumn.TagForStaple;
            }

            set
            {
                _ffiActionColumn.TagForStaple = value;
            }
        }

        /// <summary>
        /// Gets or sets a metadata field name that can be updated whenever a document is stapled.
        /// </summary>
        /// <value>
        /// A metadata field name that should be updated whenever a document is stapled or
        /// <see langword="null"/> if no metadata field is to be updated.
        /// </value>
        [Category("UW Custom Setting")]
        [DefaultValue(_DEFAULT_STAPLED_INTO_METADATA_FIELD_NAME)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string StapledIntoMetadataFieldName
        {
            get
            {
                return _ffiActionColumn.StapledIntoMetadataFieldName;
            }

            set
            {
                _ffiActionColumn.StapledIntoMetadataFieldName = value;
            }
        }

        /// <summary>
        /// Gets or sets the value to be applied to <see cref="StapledIntoMetadataFieldName"/>
        /// whenever a document is stapled. Path tags/functions are supported including the tags
        /// &lt;StapledDocumentOutput&gt;, &lt;SourceDocName&gt;, &lt;FirstName&gt;,
        /// &lt;LastName&gt;, &lt;DOB&gt; and &lt;CollectionDate&gt;
        /// </summary>
        /// <value>
        /// The metadata value to be applied.
        /// </value>
        [Category("UW Custom Setting")]
        [DefaultValue(_DEFAULT_STAPLED_INTO_METADATA_FIELD_VALUE)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string StapledIntoMetadataFieldValue
        {
            get
            {
                return _ffiActionColumn.StapledIntoMetadataFieldValue;
            }

            set
            {
                _ffiActionColumn.StapledIntoMetadataFieldValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the database metadata field that stores the patient first name.
        /// </summary>
        /// <value>
        /// The name of the database metadata field that stores the patient first name.
        /// </value>
        [Category("UW Custom Setting")]
        [DefaultValue(_DEFAULT_PATIENT_FIRST_NAME_METADATA_FIELD)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string PatientFirstNameMetadataField
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the database metadata field that stores the patient last name.
        /// </summary>
        /// <value>
        /// The name of the database metadata field that stores the patient last name.
        /// </value>
        [Category("UW Custom Setting")]
        [DefaultValue(_DEFAULT_PATIENT_LAST_NAME_METADATA_FIELD)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string PatientLastNameMetadataField
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the database metadata field that stores the patient date of
        /// birth.
        /// </summary>
        /// <value>
        /// The name of the database metadata field that stores the patient date of birth.
        /// </value>
        [Category("UW Custom Setting")]
        [DefaultValue(_DEFAULT_PATIENT_DOB_METADATA_FIELD)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string PatientDOBMetadataField
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the database metadata field that stores the document's
        /// collection date(s).
        /// </summary>
        /// <value>
        /// The name of the database metadata field that stores the document's collection date(s).
        /// </value>
        [Category("UW Custom Setting")]
        [DefaultValue(_DEFAULT_COLLECTION_DATE_METADATA_FIELD)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string CollectionDateMetadataField
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the patient first name from the current document.
        /// </summary>
        /// <value>
        /// The patient first name from the current document.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FirstName
        {
            get
            {
                return _firstName;
            }

            set
            {
                if (value != _firstName)
                {
                    _firstName = value;

                    UpdateButtonState();
                }
            }
        }

        /// <summary>
        /// Gets or sets the patient last name from the current document.
        /// </summary>
        /// <value>
        /// The patient last name from the current document.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LastName
        {
            get
            {
                return _lastName;
            }

            set
            {
                if (value != _lastName)
                {
                    _lastName = value;

                    UpdateButtonState();
                }
            }
        }

        /// <summary>
        /// Gets or sets the patient date of birth from the current document.
        /// </summary>
        /// <value>
        /// The patient date of birth from the current document.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DOB
        {
            get
            {
                return _dob;
            }

            set
            {
                if (value != _dob)
                {
                    _dob = value;

                    UpdateButtonState();
                }
            }
        }

        /// <summary>
        /// Gets or sets a comma delimited list of collection dates from the current document.
        /// </summary>
        /// <value>
        /// A comma delimited list of collection dates from the current document.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CollectionDate
        {
            get
            {
                return _collectionDate;
            }

            set
            {
                if (value != _collectionDate)
                {
                    _collectionDate = value;

                    UpdateButtonState();
                }
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:Control.Click"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                base.OnClick(e);

                string currentFileName = DataEntryControlHost.ImageViewer.ImageFile;

                // Provide the _ffiActionColumn with the necessary information
                _ffiActionColumn.OriginalFileName = currentFileName;
                _ffiActionColumn.CurrentFileID = FileProcessingDB.GetFileID(currentFileName);
                _ffiActionColumn.DataEntryApplication = DataEntryControlHost.DataEntryApplication;
                _ffiActionColumn.FileProcessingDB = FileProcessingDB;
                _ffiActionColumn.FirstName = FirstName;
                _ffiActionColumn.LastName = LastName;
                _ffiActionColumn.DOB = DOB;
                _ffiActionColumn.CollectionDate = CollectionDate;

                // Initialize the FFI with a query that will list the current document as well as
                // any documents that appear to be duplicates of this document based on patient info
                // and collection date.
                FAMFileSelector selector = new FAMFileSelector();
                selector.AddQueryCondition(DuplicateDocumentsQuery);

                // Collection dates are comma delimited, but for ease of use in code to compare
                // dates no spaces are include. Add spaces to the summary message for better
                // readability.
                string collectionDates = CollectionDate.Replace(",", ", ");

                string selectorSummary = string.Format(CultureInfo.CurrentCulture,
                    "Potential duplicate documents for {0}, {1}\r\n" +
                    "DOB: {2}\r\n" +
                    "Collection date(s): {3}",
                    LastName, FirstName, DOB, collectionDates);

                _famFileInspector.OpenFAMFileInspector(
                    FileProcessingDB, selector, true, selectorSummary, _ffiCustomColumns,
                    TopLevelControl.Handle);

                // Once the FFI has been opened, stop the button's flashing regardless of whether
                // they took action; the point was to make them aware of possible duplicates and
                // they have acknowledged awarness by using the button.
                if (Flash)
                {
                    Flash = false;
                    BackColor = FlashColor;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37443");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:Control.EnabledChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            try
            {
                base.OnEnabledChanged(e);

                if (Enabled && !KeyFieldsArePopulated)
                {
                    Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37585");
            }
        }

        #endregion Overrides

        #region Event handlers

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.DataReset"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAttributeStatusInfo_DataReset(object sender, EventArgs e)
        {
            try
            {
                // Ensure these property values aren't carried over from one document to the next.
                FirstName = "";
                LastName = "";
                DOB = "";
                CollectionDate = "";
                Flash = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37556");
            }
        }

        #endregion Event handlers

        #region Private members

        /// <summary>
        /// Gets the <see cref="FileProcessingDB"/> currently being used.
        /// </summary>
        /// <value>
        /// The <see cref="FileProcessingDB"/> currently being used.
        /// </value>
        FileProcessingDB FileProcessingDB
        {
            get
            {
                return (DataEntryControlHost == null)
                    ? null
                    : DataEntryControlHost.DataEntryApplication.FileProcessingDB;
            }
        }

        /// <summary>
        /// Gets a value indicating whether all key fields are populated in order to be able to
        /// query for potential duplicate documents.
        /// </summary>
        /// <value><see langword="true"/> if all key fields are populated; otherwise,
        /// <see langword="false"/>.</value>
        bool KeyFieldsArePopulated
        {
            get
            {
                return FileProcessingDB != null &&
                        !string.IsNullOrWhiteSpace(FirstName) &&
                        !string.IsNullOrWhiteSpace(LastName) &&
                        !string.IsNullOrWhiteSpace(DOB) &&
                        !string.IsNullOrWhiteSpace(CollectionDate);
            }
        }

        /// <summary>
        /// Gets a query that will select the file ID of the current document as well as any other
        /// document that appears to be a duplicate of this one based on the patient info or
        /// collection dates.
        /// </summary>
        string DuplicateDocumentsQuery
        {
            get
            {
                string query = string.Format(CultureInfo.InvariantCulture,
                    "SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "   INNER JOIN [FileMetadataFieldValue] ON [FAMFile].[ID] = [FileMetadataFieldValue].[FileID] " +
                    "   INNER JOIN [MetadataField] ON [MetadataFieldID] = [MetadataField].[ID] " +
                    "   WHERE [Name] = '{0}' AND LEN('{1}') > 0 AND [Value] = '{2}' " +
                    "INTERSECT " +
                    "SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "   INNER JOIN [FileMetadataFieldValue] ON [FAMFile].[ID] = [FileMetadataFieldValue].[FileID] " +
                    "   INNER JOIN [MetadataField] ON [MetadataFieldID] = [MetadataField].[ID] " +
                    "   WHERE [Name] = '{3}' AND LEN('{4}') > 0 AND [Value] = '{5}' " +
                    "INTERSECT " +
                    "SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "   INNER JOIN [FileMetadataFieldValue] ON [FAMFile].[ID] = [FileMetadataFieldValue].[FileID] " +
                    "   INNER JOIN [MetadataField] ON [MetadataFieldID] = [MetadataField].[ID] " +
                    "   WHERE [Name] = '{6}' AND LEN('{7}') > 0 AND [Value] = '{8}' ",
                    PatientFirstNameMetadataField, FirstName, FirstName,
                    PatientLastNameMetadataField, LastName, LastName,
                    PatientDOBMetadataField, DOB, DOB);

                query += " INTERSECT " +
                    "SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "   INNER JOIN [FileMetadataFieldValue] ON [FAMFile].[ID] = [FileMetadataFieldValue].[FileID] " +
                    "   INNER JOIN [MetadataField] ON [MetadataFieldID] = [MetadataField].[ID] " +
                    "   WHERE [Name] = 'CollectionDate' AND (";
                
                string[] collectionDates = string.IsNullOrWhiteSpace(CollectionDate)
                    ? new[] { "[NO_COLLECTION_DATE]"}
                    : CollectionDate.Split(',');

                // Match on any of the collection dates in the document.
                query += string.Join(" OR ",
                    collectionDates.Select(date => " [Value] LIKE '%" + date + "%' ")) + ")";

                // Always display the current document regardless of whether the metadata was properly located.
                query += " UNION " +
                    "SELECT [FAMFile].[ID] FROM [FAMFile] WHERE [FileName] = '" + 
                    AttributeStatusInfo.SourceDocName + "'";

                query = "SELECT [FAMFile].[ID] FROM [FAMFile] WHERE [ID] IN (" + query + ")";

                return query;
            }
        }

        /// <summary>
        /// Updates the state of the button based on whether key fields have been filled in and
        /// whether any other documents appear to be potential duplicates of this one.
        /// </summary>
        void UpdateButtonState()
        {
            BackColor = _originalBackColor;

            int duplicateCount = 0;

            if (KeyFieldsArePopulated)
            {
                Enabled = true;
                duplicateCount = GetMatchingDocumentCount() - 1;
                Flash = (duplicateCount > 0);
            }
            else
            {
                Enabled = false;
                Flash = false;
            }

            Text = string.Format(CultureInfo.CurrentCulture, _BUTTON_LABEL_TEXT, duplicateCount);
        }

        /// <summary>
        /// Gets the number of apparant duplicate documents in the database.
        /// </summary>
        /// <returns>The number of apparant duplicate documents in the database.</returns>
        int GetMatchingDocumentCount()
        {
            Recordset adoRecordset = null;

            try
            {
                // Don't bother running query to check for dups if any of these fields are blank--
                // we already know there will not be any results.
                if (KeyFieldsArePopulated)
                {
                    adoRecordset = FileProcessingDB.GetResultsForQuery(DuplicateDocumentsQuery);
                    return adoRecordset.RecordCount;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37572");
            }
            finally
            {
                if (adoRecordset != null)
                {
                    adoRecordset.Close();
                }
            }
        }

        #endregion Private members
    }
}
