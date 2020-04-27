using ADODB;
using Extract.FileActionManager.Utilities;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using static System.FormattableString;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// A <see cref="DataEntryButton"/> extension that allows for the display of documents that
    /// appear to be duplicates of the current document in a <see cref="FAMFileInspectorForm"/>
    /// such that appropriate action may be taken to resolve the duplicates.
    /// See: https://extract.atlassian.net/browse/CUST-409
    /// </summary>
    public class DuplicateDocumentsButton : DataEntryButton
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DuplicateDocumentsButton).ToString();

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
            @"$DirOf(<SourceDocName>)\Stapled_$Now().tif";
        
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
        /// A list of all custom columns to be used in the FFI.
        /// </summary>
        List<IFAMFileInspectorColumn> _ffiCustomColumns = new List<IFAMFileInspectorColumn>();

        /// <summary>
        /// Keeps track of documents this instance has requested be checked-out in the context of
        /// the current document to avoid repeatedly re-checking whether they need to be.
        /// </summary>
        HashSet<int> _checkedOutFiles = new HashSet<int>();

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
        /// Indicates whether pending files that are apparent duplicates of the displayed document
        /// should be automatically checked out for processing by this instance even before the
        /// button is pressed.
        /// </summary>
        bool _autoCheckoutDuplicateFiles = true;

        /// <summary>
        /// The original <see cref="P:Control.BackColor"/> for the button. Used to restore the color
        /// after having set it to the <see cref="DataEntryButton.FlashColor"/>.
        /// </summary>
        Color _originalBackColor;

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateDocumentsButton"/> class.
        /// </summary>
        public DuplicateDocumentsButton()
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                // Load licenses in design mode
                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI37953", _OBJECT_NAME);

                _originalBackColor = BackColor;

                ExtractException.Assert("ELI37947", "ActionColumn is not defined.",
                    ActionColumn != null);
                _ffiCustomColumns.Add(ActionColumn);
                if (StatusColumn != null)
                {
                    _ffiCustomColumns.Add(StatusColumn);
                }

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
        /// An <see cref="IFAMFileInspectorColumn"/> for the FFI that allows for various actions to
        /// be performed on the displayed documents.
        /// </summary>
        public virtual DuplicateDocumentsFFIColumn ActionColumn
        {
            get
            {
                if (_ffiActionColumn == null)
                {
                    _ffiActionColumn = new DuplicateDocumentsFFIColumn();
                }
                return _ffiActionColumn;
            }
        }

        /// <summary>
        /// An <see cref="IFAMFileInspectorColumn"/> that displays the status of the files prior to
        /// being loaded into the FFI.
        /// </summary>
        public virtual PreviousStatusFFIColumn StatusColumn
        {
            get
            {
                if (_ffiStatusColumn == null)
                {
                    _ffiStatusColumn = new PreviousStatusFFIColumn(ActionColumn);
                }
                return _ffiStatusColumn;
            }
        }

        /// <summary>
        /// Gets or sets a tag to apply to documents that have been ignored.
        /// </summary>
        /// <value>
        /// A tag to apply to documents that have been ignored or <see langword="null"/> if no tag
        /// is to be applied to ignored documents.
        /// </value>
        [Category("LabDE Configuration Setting")]
        [DefaultValue(_DEFAULT_TAG_FOR_IGNORE)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string TagForIgnore
        {
            get
            {
                return ActionColumn.TagForIgnore;
            }

            set
            {
                ActionColumn.TagForIgnore = value;
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
        [Category("LabDE Configuration Setting")]
        [DefaultValue(_DEFAULT_STAPLED_DOCUMENT_OUTPUT)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string StapledDocumentOutput
        {
            get
            {
                return ActionColumn.StapledDocumentOutput;
            }

            set
            {
                ActionColumn.StapledDocumentOutput = value;
            }
        }

        /// <summary>
        /// Gets or sets a tag to apply to documents that have been stapled.
        /// </summary>
        /// <value>
        /// A tag to apply to documents that have been stapled or <see langword="null"/> if no tag
        /// is to be applied to stapled documents.
        /// </value>
        [Category("LabDE Configuration Setting")]
        [DefaultValue(_DEFAULT_TAG_FOR_STAPLE)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string TagForStaple
        {
            get
            {
                return ActionColumn.TagForStaple;
            }

            set
            {
                ActionColumn.TagForStaple = value;
            }
        }

        /// <summary>
        /// Gets or sets a metadata field name that can be updated whenever a document is stapled.
        /// </summary>
        /// <value>
        /// A metadata field name that should be updated whenever a document is stapled or
        /// <see langword="null"/> if no metadata field is to be updated.
        /// </value>
        [Category("LabDE Configuration Setting")]
        [DefaultValue(_DEFAULT_STAPLED_INTO_METADATA_FIELD_NAME)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string StapledIntoMetadataFieldName
        {
            get
            {
                return ActionColumn.StapledIntoMetadataFieldName;
            }

            set
            {
                ActionColumn.StapledIntoMetadataFieldName = value;
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
        [Category("LabDE Configuration Setting")]
        [DefaultValue(_DEFAULT_STAPLED_INTO_METADATA_FIELD_VALUE)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string StapledIntoMetadataFieldValue
        {
            get
            {
                return ActionColumn.StapledIntoMetadataFieldValue;
            }

            set
            {
                ActionColumn.StapledIntoMetadataFieldValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the database metadata field that stores the patient first name.
        /// </summary>
        /// <value>
        /// The name of the database metadata field that stores the patient first name.
        /// </value>
        [Category("LabDE Configuration Setting")]
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
        [Category("LabDE Configuration Setting")]
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
        [Category("LabDE Configuration Setting")]
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
        [Category("LabDE Configuration Setting")]
        [DefaultValue(_DEFAULT_COLLECTION_DATE_METADATA_FIELD)]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string CollectionDateMetadataField
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether pending files that are apparent duplicates
        /// of the displayed document should be automatically checked out for processing by this
        /// instance even before the button is pressed.
        /// </summary>
        /// <value><see langword="true"/> if apparent duplicates should be automatically checked out;
        /// otherwise, <see langword="false"/>.
        /// </value>
        [Category("LabDE Configuration Setting")]
        [DefaultValue(true)]
        public bool AutoCheckoutDuplicateFiles
        {
            get
            {
                return _autoCheckoutDuplicateFiles;
            }

            set
            {
                _autoCheckoutDuplicateFiles = value;
            }
        }

        #region Properties

        /// <summary>
        /// The action name to be set to pending for ignored/skipped documents so they can be cleaned up
        /// </summary>
        [Category("LabDE Configuration Setting")]
        public virtual string CleanupAction
        {
            get
            {
                return ActionColumn.CleanupAction;
            }

            set
            {
                ActionColumn.CleanupAction = value;
            }
        }

        #endregion Properties

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
                try
                {
                    if (value != _firstName)
                    {
                        _firstName = value;

                        UpdateButtonState();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37950");
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
                try
                {
                    if (value != _lastName)
                    {
                        _lastName = value;

                        UpdateButtonState();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37951");
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
                try
                {
                    if (value != _dob)
                    {
                        _dob = value;

                        UpdateButtonState();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37949");
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
                try
                {
                    if (value != _collectionDate)
                    {
                        _collectionDate = value;

                        UpdateButtonState();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37948");
                }
            }
        }

        #endregion Properties

        #region Public Members

        /// <summary>
        /// Gets a FAMFileInspectorForm that has been initialized for use in conjunction with this
        /// button for unit testing. Files will be loaded into the FFI according to the current property
        /// settings and FAM DB metadata info.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public FAMFileInspectorForm GetTestFAMFileInspector()
        {
            try
            {
                var fileInspectorForm = GetFAMFileInspectorForm();
                fileInspectorForm.InitializeUnitTestMode();

                return fileInspectorForm;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49782");
            }
        }

        #endregion Public Members

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

                // Stop the button's flashing; the point was to make them aware of possible duplicates
                // and they have acknowledged awareness by using the button.
                // https://extract.atlassian.net/browse/ISSUE-12657
                // Since OpenFAMFileInspector will block until the FFI is closed and all changes 
                // have been applied, the flashing needs to be turned off before displaying the FFI,
                // otherwise the new flashing status applied for any new document displayed will be
                // overridden.
                if (Flash)
                {
                    Flash = false;
                    BackColor = FlashColor;
                }

                using (var fileInspectorForm = GetFAMFileInspectorForm())
                {
                    fileInspectorForm.ShowDialog(DataEntryControlHost);
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
                else if (!Enabled)
                {
                    // This button will be disabled either between documents or when patient info is
                    // being substantially changed. In either case, it is a good opportunity to clear
                    // the set of files that need not be re-checked for check-out.
                    _checkedOutFiles.Clear();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37585");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> if managed resources should be disposed;
        /// otherwise, <see langword="false" />.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AttributeStatusInfo.DataReset -= HandleAttributeStatusInfo_DataReset;
            }

            base.Dispose(disposing);
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

        #region Protected Members

        /// <summary>
        /// Gets a <see cref="FAMFileInspectorForm"/> instance to be used to display probable
        /// duplicate files for resolution.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual FAMFileInspectorForm GetFAMFileInspectorForm()
        {
            // Provide the ActionColumn with the necessary information
            ActionColumn.Initialize(DataEntryControlHost.DataEntryApplication);
            ActionColumn.FirstName = FirstName;
            ActionColumn.LastName = LastName;
            ActionColumn.DOB = DOB;
            ActionColumn.CollectionDate = CollectionDate;

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

            var fileInspectorForm = new FAMFileInspectorForm();
            fileInspectorForm.UseDatabaseMode = true;
            fileInspectorForm.FileProcessingDB.DuplicateConnection(FileProcessingDB);
            fileInspectorForm.FileSelector = selector;
            fileInspectorForm.LockFileSelector = true;
            fileInspectorForm.LockedFileSelectionSummary = selectorSummary;
            foreach (var column in _ffiCustomColumns)
            {
                fileInspectorForm.AddCustomColumn(column);
            }

            return fileInspectorForm;
        }

        /// <summary>
        /// Gets the <see cref="FileProcessingDB"/> currently being used.
        /// </summary>
        /// <value>
        /// The <see cref="FileProcessingDB"/> currently being used.
        /// </value>
        protected FileProcessingDB FileProcessingDB
        {
            get
            {
                return (DataEntryApplication == null)
                    ? null
                    : DataEntryApplication.FileProcessingDB;
            }
        }

        /// <summary>
        /// Gets a value indicating whether all key fields are populated in order to be able to
        /// query for potential duplicate documents.
        /// </summary>
        /// <value><see langword="true"/> if all key fields are populated; otherwise,
        /// <see langword="false"/>.</value>
        protected virtual bool KeyFieldsArePopulated
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
        protected virtual string DuplicateDocumentsQuery
        {
            get
            {
                string firstName = FirstName.Replace("'", "''");
                string lastName = LastName.Replace("'", "''");
                string dob = DOB.Replace("'", "''");
                string sourceDocName = AttributeStatusInfo.SourceDocName.Replace("'", "''");
                // Build a comma-delimited list of conditions for collection dates to allow match on
                // any of the collection dates in the document using a 'LIKE' comparison
                string collectionDateConditions = string.IsNullOrWhiteSpace(CollectionDate)
                    ? "1 = 0"  // Always false if the CollectionDate value does not exist.
                    : string.Join(" OR ", 
                        CollectionDate.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(date => "  [CollectionDate].[Value] LIKE '%" + date.Replace("'", "''") + "%'"));

                string query = Invariant($@"
                    SELECT [FAMFile].[ID] FROM [FAMFile]
                        INNER JOIN [FileMetadataFieldValue] AS [FirstName]
                            ON [FirstName].[FileID] = [FAMFile].[ID] AND [FirstName].[MetadataFieldID] =
			                    (SELECT [ID] FROM [MetadataField] WHERE [Name] = '{PatientFirstNameMetadataField}')
                            AND LEN('{firstName}') > 0              -- Don't allow a match against a file missing key metadata
                            AND [FirstName].[Value] = '{firstName}'
                        INNER JOIN [FileMetadataFieldValue] AS [LastName]
                            ON [LastName].[FileID] = [FAMFile].[ID] AND [LastName].[MetadataFieldID] =
			                    (SELECT [ID] FROM [MetadataField] WHERE [Name] = '{PatientLastNameMetadataField}')
                            AND LEN('{lastName}') > 0              -- Don't allow a match against a file missing key metadata
                            AND [LastName].[Value] = '{lastName}'
                        INNER JOIN [FileMetadataFieldValue] AS [DOB]
                            ON [DOB].[FileID] = [FAMFile].[ID] AND [DOB].[MetadataFieldID] =
			                    (SELECT [ID] FROM [MetadataField] WHERE [Name] = '{PatientDOBMetadataField}')
                            AND LEN('{dob}') > 0              -- Don't allow a match against a file missing key metadata
                            AND [DOB].[Value] = '{dob}'

                    -- Use LIKE to match one any one of multiple collection dates that may exist in the document.
                        INNER JOIN [FileMetadataFieldValue] AS [CollectionDate]
                            ON [CollectionDate].[FileID] = [FAMFile].[ID] AND [CollectionDate].[MetadataFieldID] =
			                    (SELECT [ID] FROM [MetadataField] WHERE [Name] = '{CollectionDateMetadataField}')
                            AND ({collectionDateConditions})
                    
                    -- Always display the current document regardless of whether the metadata was properly located.
                    UNION SELECT [ID] FROM [FAMFile] WHERE [FileName] = '{sourceDocName}'");

                // To ensure "ORDER BY [FAMFile].[ID]" can be correctly applied to the query result
                query = "SELECT [FAMFile].[ID] FROM [FAMFile] WHERE [ID] IN (" + query + ")";

                return query;
            }
        }

        /// <summary>
        /// Updates the state of the button based on whether key fields have been filled in and
        /// whether any other documents appear to be potential duplicates of this one.
        /// </summary>
        protected virtual void UpdateButtonState()
        {
            // If this is not visible, do not bother updating the button state.
            // https://extract.atlassian.net/browse/ISSUE-14894
            // Ensure no updates occur and no files are attempted to be checked out if this thread
            // is ending. This helps to prevent issues in DataEntryPanelContainer.UpdateDocumentStatusThread
            // as the paginate files task is closed.
            if (Visible && !AttributeStatusInfo.ThreadEnding)
            {
                BackColor = _originalBackColor;

                int duplicateCount = 0;

                if (KeyFieldsArePopulated)
                {
                    Enabled = true;
                    duplicateCount = GetMatchingDocumentCount() - 1;
                    Flash = (duplicateCount > 0);

                    if (AutoCheckoutDuplicateFiles)
                    {
                        CheckOutDuplicateFiles();
                    }
                }
                else
                {
                    Enabled = false;
                    Flash = false;
                }

                Text = string.Format(CultureInfo.CurrentCulture, _BUTTON_LABEL_TEXT, duplicateCount);
            }
        }

        #endregion Protected Members

        #region Private Members

        /// <summary>
        /// Gets the <see cref="IDataEntryApplication"/> in which this button exists.
        /// NOTE: This property is private because some DEP implementations already included a
        /// definition this would conflict with.
        /// </summary>
        IDataEntryApplication DataEntryApplication
        {
            get
            {
                return (DataEntryControlHost == null)
                    ? null
                    : DataEntryControlHost.DataEntryApplication;
            }
        }

        /// <summary>
        /// Gets the number of apparent duplicate documents in the database.
        /// </summary>
        /// <returns>The number of apparent duplicate documents in the database.</returns>
        int GetMatchingDocumentCount()
        {
            Recordset adoRecordset = null;

            try
            {
                // Don't bother running query to check for dups if any of these fields are blank--
                // we already know there will not be any results.
                if (KeyFieldsArePopulated)
                {
                    // https://extract.atlassian.net/browse/ISSUE-14820
                    // The FFI will limit the selected files to the current workflow via usage of a
                    // FAMFileSelector like this. Do so here to keep the count returned here
                    // consistent with the files that will be displayed when the FFI is launched.
                    FAMFileSelector selector = new FAMFileSelector();
                    selector.AddQueryCondition(DuplicateDocumentsQuery);
                    string strQuery = selector.BuildQuery(FileProcessingDB, "[FAMFile].[ID]",
                        " ORDER BY [FAMFile].[ID]", false);

                    adoRecordset = FileProcessingDB.GetResultsForQuery(strQuery);
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

        /// <summary>
        /// Checks out (grabs) for processing all files currently pending on this action that appear
        /// to be duplicates of the file currently displayed.
        /// https://extract.atlassian.net/browse/ISSUE-13568
        /// </summary>
        void CheckOutDuplicateFiles()
        {
            if (!KeyFieldsArePopulated)
            {
                return;
            }

            // Compile a set of FileIDs that appear to be duplicates of this one.
            Recordset duplicateFileIDRecordset =
                FileProcessingDB.GetResultsForQuery(DuplicateDocumentsQuery);

            HashSet<int> duplicateFileIDs = new HashSet<int>();
            while (!duplicateFileIDRecordset.EOF)
            {
                duplicateFileIDs.Add((int)duplicateFileIDRecordset.Fields["ID"].Value);

                duplicateFileIDRecordset.MoveNext();
            }
            duplicateFileIDRecordset.Close();

            // https://extract.atlassian.net/browse/ISSUE-14894
            // Get the currentFileID to ensure under no circumstances will this attempt to check out
            // the current file. This had been occurring in DataEntryPanelContainer.UpdateDocumentStatusThread
            // after processing had been stopped and the files were returned to the queue.
            int currentFileID = FileProcessingDB.GetFileID(AttributeStatusInfo.SourceDocName);

            // Check each file that hasn't already been checked out to see if it should be checked
            // out.
            foreach (int fileID in duplicateFileIDs
                .Where(fileID => fileID != currentFileID && !_checkedOutFiles.Contains(fileID)))
            {
                EActionStatus actionStatus =
                    DataEntryApplication.FileProcessingDB.GetFileStatus(
                        fileID, DataEntryApplication.DatabaseActionName, false);

                // Only attempt to check out files that are currently pending for verification.
                if (actionStatus == EActionStatus.kActionPending)
                {
                    // Ignore exceptions caused by the file status changing between the above check
                    // and the attempt to check out the file
                    // https://extract.atlassian.net/browse/ISSUE-15672
                    try
                    {
                        if (DataEntryApplication.FileRequestHandler.CheckoutForProcessing(
                            fileID, true, out actionStatus))
                        {
                            _checkedOutFiles.Add(fileID);
                        }
                    }
                    catch (Exception)
                    {
                        try
                        {
                            actionStatus =
                                DataEntryApplication.FileProcessingDB.GetFileStatus(
                                    fileID, DataEntryApplication.DatabaseActionName, false);
                        }
                        catch (Exception)
                        { }

                        if (actionStatus == EActionStatus.kActionPending)
                        {
                            throw;
                        }
                    }
                }
            }
        }

        #endregion Private Members
    }
}
