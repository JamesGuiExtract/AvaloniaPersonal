using Extract.FileActionManager.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
        #region Properties

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

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateDocumentsButton"/> class.
        /// </summary>
        public DuplicateDocumentsButton()
        {
            try
            {
                _ffiActionColumn = new DuplicateDocumentsFFIColumn();
                _ffiStatusColumn = new PreviousStatusFFIColumn(_ffiActionColumn);

                _ffiCustomColumns.PushBack(_ffiActionColumn);
                _ffiCustomColumns.PushBack(_ffiStatusColumn);

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
        [DefaultValue("User_Ignore document")]
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
        [DefaultValue(@"$DirOf(<SourceDocName>)\Stapled_<FirstName>_<LastName>_$RandomAlphaNumeric(5).tif")]
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
        [DefaultValue("")]
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
        /// A metadata field name that can be updated whenever a document is stapled or
        /// <see langword="null"/> if no metadata field is to be updated.
        /// </value>
        [Category("UW Custom Setting")]
        [DefaultValue("StapledInto")]
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
        [DefaultValue("<StapledDocumentOutput>")]
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
        [DefaultValue("PatientFirstName")]
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
        [DefaultValue("PatientLastName")]
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
        [DefaultValue("PatientDOB")]
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
        [DefaultValue("CollectionDate")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string CollectionDateMetadataField
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="FileProcessingDB"/> currently being used.
        /// </summary>
        /// <value>
        /// The <see cref="FileProcessingDB"/> currently being used.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FileProcessingDB FileProcessingDB
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
            get;
            set;
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
            get;
            set;
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
            get;
            set;
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
            get;
            set;
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

                FileProcessingDB = DataEntryControlHost.DataEntryApplication.FileProcessingDB;
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

                string selectorSummary = string.Format(CultureInfo.CurrentCulture,
                    "Potential duplicate documents for {0}, {1}\r\n" +
                    "DOB: {2}\r\n" +
                    "Collection date(s): {3}",
                    LastName, FirstName, DOB, CollectionDate);

                _famFileInspector.OpenFAMFileInspector(
                    FileProcessingDB, selector, true, selectorSummary, _ffiCustomColumns);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37443");
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
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37556");
            }
        }

        #endregion Event handlers

        #region Private members

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
                    "   WHERE [Name] = '{0}' AND [Value] = '{1}' " +
                    "INTERSECT " +
                    "SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "   INNER JOIN [FileMetadataFieldValue] ON [FAMFile].[ID] = [FileMetadataFieldValue].[FileID] " +
                    "   INNER JOIN [MetadataField] ON [MetadataFieldID] = [MetadataField].[ID] " +
                    "   WHERE [Name] = '{2}' AND [Value] = '{3}' " +
                    "INTERSECT " +
                    "SELECT [FAMFile].[ID] FROM [FAMFile] " +
                    "   INNER JOIN [FileMetadataFieldValue] ON [FAMFile].[ID] = [FileMetadataFieldValue].[FileID] " +
                    "   INNER JOIN [MetadataField] ON [MetadataFieldID] = [MetadataField].[ID] " +
                    "   WHERE [Name] = '{4}' AND [Value] = '{5}' ",
                    PatientFirstNameMetadataField, FirstName,
                    PatientLastNameMetadataField, LastName,
                    PatientDOBMetadataField, DOB);

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

        #endregion Private members
    }
}
