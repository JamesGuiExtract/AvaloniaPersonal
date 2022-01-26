using Extract.AttributeFinder;
using Extract.DataEntry;
using Extract.FileActionManager.Database;
using Extract.FileActionManager.Forms;
using Extract.Interfaces;
using Extract.Interop;
using Extract.Licensing;
using Extract.SqlDatabase;
using Extract.Utilities;
using Extract.Utilities.Email;
using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Interface definition for the <see cref="SendEmailTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("50ECE538-B7CD-41FD-900D-EADDB83469BD")]
    [CLSCompliant(false)]
    public interface ISendEmailTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask,
        ILicensedComponent, IPersistStream, IDisposable
    {
        /// <summary>
        /// The primary email recipient(s) (multiple addresses should be delimited by ';' or ',')
        /// </summary>
        string Recipient { get; set; }

        /// <summary>
        /// The email recipient(s) to be copied (multiple addresses should be delimited by ';' or ',')
        /// </summary>
        string CarbonCopyRecipient { get; set; }

        /// <summary>
        /// The email subject.
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// The email body.
        /// </summary>
        string Body { get; set; }

        /// <summary>
        /// The full paths of all files to attach to the email.
        /// </summary>
        // In order to be COM accessible, the property type needs to be an array.
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        string[] Attachments { get; set; }

        /// <summary>
        /// The name of the VOA file that should be used to expand any attribute queries.
        /// </summary>
        string DataFileName { get; set; }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> which sends and email.
    /// </summary>
    [ComVisible(true)]
    [Guid("0A4F188F-CF04-4871-9010-230F73FAC966")]
    [ProgId("Extract.FileActionManager.FileProcessors.SendEmailTask")]
    public class SendEmailTask : ISendEmailTask, IErrorEmailTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Send email";

        /// <summary>
        /// Current task version.
        /// Version 2: 
        /// Updated tag names: ActionName -> DatabaseAction and DatabaseServerName -> DatabaseServer
        /// </summary>
        const int _CURRENT_VERSION = 2;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        /// <summary>
        /// A special tag that provides a path to a temporary copy of an
        /// <see cref="ExtractException"/> for the <see cref="Attachments"/> field when
        /// <see cref="StringizedException"/> has been specified.
        /// </summary>
        public static readonly string ExceptionFileTag = "<ExceptionFile>";

        #endregion Constants

        /// <summary>
        /// Regex that parses text to find "matches" where each match is a section of the source
        /// text that alternates between recognized queries and non-query text. The sum of all
        /// matches = the original source text.
        /// </summary>
        static Regex _queryParserRegex =
            new Regex(@"((?!<Query>[\s\S]+?</Query>)[\S\s])+|<Query>[\s\S]+?</Query>",
                RegexOptions.Compiled);

        /// <summary>
        /// Regex that finds all shorthand attribute queries in text.
        /// </summary>
        static Regex _attributeQueryFinderRegex = new Regex(@"</[\s\S]+?>", RegexOptions.Compiled);

        #region Fields

        /// <summary>
        /// The primary email recipient(s) (multiple addresses should be delimited by ';' or ',')
        /// </summary>
        string _recipient = "";

        /// <summary>
        /// The email recipient(s) to be copied (multiple addresses should be delimited by ';' or ',')
        /// </summary>
        string _carbonCopyRecipient = "";

        /// <summary>
        /// The email subject.
        /// </summary>
        string _subject = "";

        /// <summary>
        /// The email body.
        /// </summary>
        string _body = "";

        /// <summary>
        /// The full paths of all files to attach to the email.
        /// </summary>
        string[] _attachments = new string[0];

        /// <summary>
        /// The name of the VOA file that should be used to expand any attribute queries.
        /// </summary>
        string _dataFileName = "<SourceDocName>.voa";

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Indicates whether data to run data entry queries has been initialized for the current
        /// file.
        /// </summary>
        bool _queryDataInitialized;

        /// <summary>
        /// Indicates whether the VOA file was loaded.
        /// </summary>
        bool _dataFileLoaded;

        /// <summary>
        /// Indicates whether the VOA file was needed for a query, but could not be found.
        /// </summary>
        bool _dataFileMissing;

        /// <summary>
        /// Indicates whether a database connection file was needed for a query or path tag, but
        /// could not be found.
        /// </summary>
        bool _dbConnectionMissing;

        /// <summary>
        /// The <see cref="DbConnection"/> to use to resolve data queries.
        /// </summary>
        DbConnection _dbConnection;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailTask"/> class.
        /// </summary>
        public SendEmailTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="SendEmailTask"/> from which settings should
        /// be copied.</param>
        public SendEmailTask(SendEmailTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35948");
            }
        }

        #endregion Constructors

        #region ISendEmailTask Members

        /// <summary>
        /// Gets or sets the primary recipient(s).
        /// <para><b>Note</b></para>
        /// Multiple addresses should be delimited by ';' or ','.
        /// </summary>
        /// <value>
        /// The primary recipient(s).
        /// </value>
        public string Recipient
        {
            get
            {
                return _recipient;
            }

            set
            {
                _dirty |= !string.Equals(_recipient, value, StringComparison.Ordinal);
                _recipient = value;
            }
        }

        /// <summary>
        /// Gets or sets the recipient(s) to be copied.
        /// <para><b>Note</b></para>
        /// Multiple addresses should be delimited by ';' or ','
        /// </summary>
        /// <value>
        /// The recipient(s) to be copied.
        /// </value>
        public string CarbonCopyRecipient
        {
            get
            {
                return _carbonCopyRecipient;
            }

            set
            {
                _dirty |= !string.Equals(_carbonCopyRecipient, value, StringComparison.Ordinal);
                _carbonCopyRecipient = value;
            }
        }

        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        /// <value>
        /// The subject of the email.
        /// </value>
        public string Subject
        {
            get
            {
                return _subject;
            }

            set
            {
                _dirty |= !string.Equals(_subject, value, StringComparison.Ordinal);
                _subject = value;
            }
        }

        /// <summary>
        /// Gets or sets the body of the email.
        /// </summary>
        /// <value>
        /// The body of the email.
        /// </value>
        public string Body
        {
            get
            {
                return _body;
            }

            set
            {
                _dirty |= !string.Equals(_body, value, StringComparison.Ordinal);
                _body = value;
            }
        }

        /// <summary>
        /// Gets or sets the full paths of all files to attach to the email.
        /// </summary>
        /// <value>
        /// The full paths of all files to attach to the email.
        /// </value>
        // Allow the return value to be an array since this is a COM visible property.
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Attachments
        {
            get
            {
                return _attachments;
            }

            set
            {
                _dirty = value.SequenceEqual(_attachments);
                _attachments = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the VOA file to use for attribute value expansion.
        /// </summary>
        /// <value>
        /// The name of the VOA file to use for attribute value expansion.
        /// </value>
        public string DataFileName
        {
            get
            {
                return _dataFileName;
            }

            set
            {
                _dirty |= !string.Equals(_dataFileName, value, StringComparison.Ordinal);
                _dataFileName = value;
            }
        }

        #endregion ISendEmailTask Members

        #region IErrorEmailTask Members

        /// <summary>
        /// When this task is being used as an error handling option, this property specifies the
        /// <see cref="ExtractException"/> that triggered the error (in stringized form).
        /// </summary>
        public string StringizedException
        {
            get;
            set;
        }

        /// <summary>
        /// Allows configuration of this instance in the context of and error handler.
        /// </summary>
        public bool ConfigureErrorEmail()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI36114", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (SendEmailTask)Clone();

                using (var dialog = new SendEmailTaskSettingsDialog(cloneOfThis, true))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dialog.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI36115",
                    "Error running configuration.", ex);
            }
        }

        /// <summary>
        /// Applies the default settings for an email to be sent as an error handler.
        /// <para><b>Note</b></para>
        /// The <see cref="Recipient"/> and <see cref="CarbonCopyRecipient"/> fields are not
        /// modified by this call.
        /// </summary>
        public void ApplyDefaultErrorEmailSettings()
        {
            try
            {
                Subject = "ERROR: A file failed in action \"<ActionName>\" on $Env(COMPUTERNAME)";
                Body = "Filename: <SourceDocName>\r\nFPS: <FPSFileName>";
                Attachments = new[] { ExceptionFileTag };
                DataFileName = "<SourceDocName>.voa";
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36116", "Error applying default email settings.");
            }
        }

        /// <summary>
        /// Validates that proper settings exist for use as an <see cref="IErrorEmailTask"/>.
        /// </summary>
        public void ValidateErrorEmailConfiguration()
        {
            try
            {
                ExtractException.Assert("ELI36159", "Error email recipient has not been specified.",
                    !string.IsNullOrWhiteSpace(Recipient));

                ExtractException.Assert("ELI36161",
                    "Error email subject has not been specified.",
                    !string.IsNullOrWhiteSpace(Subject));
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36164", ex.Message);
            }
        }

        /// <summary>
        /// Checks that proper outbound email server settings exist to be able to use an
        /// <see cref="IErrorEmailTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if proper email server settings exist; otherwise,
        /// <see langword="false"/>.</returns>
        public bool IsEmailServerConfigured()
        {
            try
            {
                string emailServer = "";
                try
                {
                    FileProcessingDB fileProcessingDB = new FileProcessingDB();
                    fileProcessingDB.ConnectLastUsedDBThisProcess();

                    var emailSettings =
                        new FAMDatabaseSettings<ExtractSmtp>(
                            fileProcessingDB, false, SmtpEmailSettings.PropertyNameLookup);
                    emailServer = emailSettings.Settings.Server;
                }
                catch (Exception ex)
                {
                    var ee = new ExtractException("ELI36163",
                        "Unable to validate database email settings", ex);
                    throw ee;
                }

                return !string.IsNullOrWhiteSpace(emailServer);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36168", ex.Message);
            }
        }

        #endregion IErrorEmailTask Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="SendEmailTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI35949", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (SendEmailTask)Clone();

                using (var dialog = new SendEmailTaskSettingsDialog(cloneOfThis))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dialog.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI35950",
                    "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object has been configured and <see langword="false"/>
        /// otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                return (!string.IsNullOrWhiteSpace(Recipient) &&
                        !string.IsNullOrWhiteSpace(Subject));
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35961", "Unable to check configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="SendEmailTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="SendEmailTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new SendEmailTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI35951", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="SendEmailTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as SendEmailTask;
                if (task == null)
                {
                    throw new InvalidCastException("Invalid cast to SendEmailTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI35952", "Unable to copy object.", ex);
            }
        }

        #endregion ICopyableObject Members

        #region IFileProcessingTask Members

        /// <summary>
        /// Gets the minimum stack size needed for the thread in which this task is to be run.
        /// </summary>
        /// <value>
        /// The minimum stack size needed for the thread in which this task is to be run.
        /// </value>
        [CLSCompliant(false)]
        public uint MinStackSize
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns a value indicating that the task does not display a UI
        /// </summary>
        public bool DisplaysUI
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            // Do nothing, this task is not cancellable
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            // Nothing to do
        }

        /// <summary>
        /// Called to notify the file processor that the pending document queue is empty, but
        ///	the processing tasks have been configured to remain running until the next document
        ///	has been supplied. If the processor will standby until the next file is supplied it
        ///	should return <see langword="true"/>. If the processor wants to cancel processing,
        ///	it should return <see langword="false"/>. If the processor does not immediately know
        ///	whether processing should be cancelled right away, it may block until it does know,
        ///	and return at that time.
        /// <para><b>Note</b></para>
        /// This call will be made on a different thread than the other calls, so the Standby call
        /// must be thread-safe. This allows the file processor to block on the Standby call, but
        /// it also means that call to <see cref="ProcessFile"/> or <see cref="Close"/> may come
        /// while the Standby call is still occurring. If this happens, the return value of Standby
        /// will be ignored; however, Standby should promptly return in this case to avoid
        /// needlessly keeping a thread alive.
        /// </summary>
        /// <returns><see langword="true"/> to standby until the next file is supplied;
        /// <see langword="false"/> to cancel processing.</returns>
        public bool Standby()
        {
            return true;
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>  
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use to expand path tags and
        /// functions.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="pFileRequestHandler">The <see cref="IFileRequestHandler"/> that can be used
        /// by the task to carry out requests for files to be checked out, released or re-ordered
        /// in the queue.</param>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            IFileRequestHandler pFileRequestHandler)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI35953", _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35954", "Unable to initialize \"Send email\" task.");
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
        /// <param name="pFileRecord">The file record that contains the info of the file being 
        /// processed.</param>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">A File Action Manager Tag Manager for expanding tags.</param>
        /// <param name="pDB">The File Action Manager database.</param>
        /// <param name="pProgressStatus">Object to provide progress status updates to caller.
        /// </param>
        /// <param name="bCancelRequested"><see langword="true"/> if cancel was requested; 
        /// <see langword="false"/> otherwise.</param>
        /// <returns>An <see cref="EFileProcessingResult"/> indicating the result of the
        /// processing.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord,
            int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            // If the StringizedException is to be attached to this email, tempExceptionFile will
            // specify it's temporary location on disk.
            TemporaryFile tempExceptionFile = null;

            try
            {
                _queryDataInitialized = false;
                _dataFileLoaded = false;
                _dataFileMissing = false;
                _dbConnectionMissing = false;
                _dbConnection = null;

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI35955", _COMPONENT_DESCRIPTION);

                var emailSettings = new SmtpEmailSettings();
                if (pDB != null)
                {
                    emailSettings.LoadSettings(
                        new FAMDatabaseSettings<ExtractSmtp>(
                            pDB, false, SmtpEmailSettings.PropertyNameLookup));
                }
                else
                {
                    emailSettings.LoadSettings(false);
                }

                // Create the pathTags instance to be used to expand any path tags/functions.
                FileActionManagerPathTags pathTags = new FileActionManagerPathTags(
                    pFAMTM, pFileRecord.Name);
                pathTags.AlwaysShowDatabaseTags = true;

                // Create and fill in the properties of an ExtractEmailMessage.
                ExtractEmailMessage emailMessage = new ExtractEmailMessage();
                emailMessage.EmailSettings = emailSettings;
                string expandedRecipients =
                    ExpandText(Recipient, pFileRecord, pathTags, pDB);
                emailMessage.Recipients =
                    expandedRecipients
                    .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToVariantVector();
                string expandedCcRecipients =
                    ExpandText(CarbonCopyRecipient, pFileRecord, pathTags, pDB);
                emailMessage.CarbonCopyRecipients = expandedCcRecipients
                    .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToVariantVector();
                emailMessage.Subject = ExpandText(Subject, pFileRecord, pathTags, pDB);
                emailMessage.Body = ExpandText(Body, pFileRecord, pathTags, pDB);

                PrepareAttachments(emailMessage, pFileRecord, pathTags, pDB, ref tempExceptionFile);

                // If there is any text that could not be properly expanded because of a missing VOA
                // file or DB connection, add error text to the email as well as logging an error.
                if (_dataFileMissing)
                {
                    emailMessage.Body += "\r\n\r\nERROR: The data file necessary to expand text " +
                        "in this email could not be found; some email text may be missing/invalid.";

                    var ee = new ExtractException("ELI35984", "The data file necessary to expand " +
                        "text in an email could not be found; some email text may be missing/invalid.");
                    ee.AddDebugData("Data file", DataFileName, false);
                    ee.AddDebugData("SourceDocName", pFileRecord.Name, false);
                    ee.AddDebugData("FPS",
                        pathTags.Expand(FileActionManagerPathTags.FpsFileNameTag), false);
                    ee.Log();
                }

                if (_dbConnectionMissing)
                {
                    emailMessage.Body += "\r\n\r\nERROR: No database connection was available to " +
                        "expand text in this email; some email text may be missing/invalid.";

                    var ee = new ExtractException("ELI35993", "No database connection was available " +
                        "to expand text in an email; some email text may be missing/invalid.");
                    ee.AddDebugData("Data file", DataFileName, false);
                    ee.AddDebugData("SourceDocName", pFileRecord.Name, false);
                    ee.AddDebugData("FPS",
                        pathTags.Expand(FileActionManagerPathTags.FpsFileNameTag), false);
                    ee.Log();
                }

                emailMessage.Send();

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35956", "Unable to process the file.");
            }
            finally
            {
                _dbConnection?.Dispose();
                _dbConnection = null;

                if (tempExceptionFile != null)
                {
                    tempExceptionFile.Dispose();
                    tempExceptionFile = null;
                }
            }
        }
      
        #endregion IFileProcessingTask Members

        #region IAccessRequired Members

        /// <summary>
        /// Returns bool value indicating if the task requires admin access.
        /// </summary>
        /// <returns><see langword="true"/> if the task requires admin access
        /// <see langword="false"/> if task does not require admin access.</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IAccessRequired Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            try
            {
                return LicenseUtilities.IsLicensed(_LICENSE_ID);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI35957",
                    "Unable to determine license status.", ex);
            }
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns><see cref="HResult.Ok"/> if changes have been made; 
        /// <see cref="HResult.False"/> if changes have not been made.</returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    _recipient = reader.ReadString();
                    _carbonCopyRecipient = reader.ReadString();
                    _subject = reader.ReadString();
                    _body = reader.ReadString();
                    _attachments = reader.ReadStringArray();
                    _dataFileName = reader.ReadString();

                    if (reader.Version < 2)
                    {
                        // Update tag name of <DatabaseServerName> to <DatabaseServer>
                        _subject = _subject.Replace("<DatabaseServerName>",
                            FileActionManagerPathTags.DatabaseServerTag);
                        _body = _body.Replace("<DatabaseServerName>",
                            FileActionManagerPathTags.DatabaseServerTag);
                    }
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI35958",
                    "Unable to load object from stream.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If 
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(_recipient);
                    writer.Write(_carbonCopyRecipient);
                    writer.Write(_subject);
                    writer.Write(_body);
                    writer.Write(_attachments);
                    writer.Write(_dataFileName);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI35959",
                    "Unable to save object to stream", ex);
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in 
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion IPersistStream Members

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ExtractImageAreaTask"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ExtractImageAreaTask"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="ExtractImageAreaTask"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dbConnection != null)
                {
                    _dbConnection.Dispose();
                    _dbConnection = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Finds and attaches to the <see paramref="emailMessage"/> any configured attachments
        /// and logs appropriate error information if any attachments are missing.
        /// </summary>
        /// <param name="emailMessage">The <see cref="ExtractEmailMessage"/> to which the
        /// attachments should be attached.</param>
        /// <param name="fileRecord">The <see cref="FileRecord"/> relating to the
        /// <see paramref="emailMessage"/></param>
        /// <param name="pathTags">The <see cref="FileActionManagerPathTags"/> instance to use to
        /// expand path tags and functions in the filenames.</param>
        /// <param name="fileProcessingDB">The File Action Manager database being used for
        /// processing.</param>
        /// <param name="tempExceptionFile"></param>
        void PrepareAttachments(ExtractEmailMessage emailMessage, FileRecord fileRecord,
            FileActionManagerPathTags pathTags, IFileProcessingDB fileProcessingDB,
            ref TemporaryFile tempExceptionFile)
        {
            if (!string.IsNullOrEmpty(StringizedException))
            {
                tempExceptionFile = new TemporaryFile(
                    null, Path.GetFileName(fileRecord.Name) + ".uex", null, false);
                string exceptionFileName = tempExceptionFile.FileName;
                var ee = ExtractException.FromStringizedByteStream("ELI36117", StringizedException);
                ee.Log(exceptionFileName);
                pathTags.AddTag(ExceptionFileTag, exceptionFileName);
            }

            var expandedAttachmentPaths = Attachments
                .Select(attachment => ExpandText(
                    attachment, fileRecord, pathTags, fileProcessingDB));

            var missingAttachments = expandedAttachmentPaths
                .Where(filePath => !File.Exists(filePath));

            if (missingAttachments.Any())
            {
                var ee = new ExtractException("ELI35972",
                    "One or more email attachments could not be found or accessed.");

                foreach (string missingAttachment in missingAttachments)
                {
                    emailMessage.Body += string.Format(CultureInfo.CurrentCulture, "\r\n\r\n" +
                        "ERROR: The file \"{0}\" was configured to be attached to this email. " +  
                        "However, that file was not found or was not accessible.",
                        missingAttachment);
                    ee.AddDebugData("Attachment", missingAttachment, false);
                    ee.AddDebugData("FPS",
                        pathTags.Expand(FileActionManagerPathTags.FpsFileNameTag), false);
                }

                ee.Log();
            }

            emailMessage.Attachments = expandedAttachmentPaths
                .Except(missingAttachments)
                .ToVariantVector();
        }

        /// <summary>
        /// Expand all path tags/functions and data queries in the specified <see paramref="text"/>.
        /// <para><b>Note</b></para>
        /// This expansion supports shorthand attribute queries in the form &lt;/AttributeName&gt;
        /// </summary>
        /// <param name="text">The text to be expanded.</param>
        /// <param name="fileRecord">The <see cref="FileRecord"/> relating to the text to be
        /// expanded.</param>
        /// <param name="pathTags">The <see cref="FileActionManagerPathTags"/> instance to use to
        /// expand path tags and functions in the <see paramref="text"/>.</param>
        /// <param name="fileProcessingDB">The File Action Manager database being used for
        /// processing.</param>
        /// <returns><see paramref="text"/> with all path tags/functions as well as data queries
        /// expanded.</returns>
        string ExpandText(string text, FileRecord fileRecord, FileActionManagerPathTags pathTags,
            IFileProcessingDB fileProcessingDB)
        {
            try
            {
                // Don't attempt to expand a blank string.
                if (string.IsNullOrWhiteSpace(text))
                {
                    return "";
                }

                string expandedOutput = "";

                // Parse the source text into alternating "matches" where every other "match" is a
                // query and the "matches" in-between are non-query text.
                var matches = _queryParserRegex.Matches(text)
                    .OfType<Match>()
                    .ToList();

                // Iterate all non-query text to see if it contains any shorthand query syntax that
                // needs to be expanded.
                // (</AttributeName> for <Query><Attribute>AttributeName</Attribute></Query>)
                foreach (Match match in matches
                    .Where(match => !IsQuery(match))
                    .ToArray())
                {
                    // Substitute any attribute query shorthand with the full query syntax.
                    string matchText =
                        _attributeQueryFinderRegex.Replace(match.Value, SubstituteAttributeQuery);

                    // If after substitutions the _queryParserRegex finds more than one partition, or
                    // the one and only partition is a query, one or more shorthand queries were
                    // expanded. Insert the expanded partitions in place of the original one.
                    var subMatches = _queryParserRegex.Matches(matchText);
                    if (subMatches.Count > 1 || IsQuery(subMatches[0]))
                    {
                        int index = matches.IndexOf(match);
                        matches.RemoveAt(index);
                        matches.InsertRange(index, subMatches.OfType<Match>());
                    }
                }

                // Iterate all partitions of the source text, evaluating any queries as we go.
                foreach (Match match in matches)
                {
                    if (IsQuery(match))
                    {
                        // The first time a query in encountered, load the database and data for all
                        // subsequent queries for this files to use.
                        if (!_queryDataInitialized)
                        {
                            if (fileProcessingDB != null)
                            {
                                var connectionString = SqlUtil.CreateConnectionString(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
                                _dbConnection = new ExtractRoleConnection(connectionString);
                                _dbConnection.Open();
                            }

                            IUnknownVector sourceAttributes = new IUnknownVector();
                            string dataFileName = pathTags.Expand(DataFileName);
                            if (File.Exists(dataFileName))
                            {
                                // If data file exists, load it.
                                sourceAttributes.LoadFrom(dataFileName, false);

                                // So that the garbage collector knows of and properly manages the associated
                                // memory.
                                sourceAttributes.ReportMemoryUsage();

                                _dataFileLoaded = true;
                            }

                            AttributeStatusInfo.InitializeForQuery(sourceAttributes,
                                fileRecord.Name, _dbConnection, pathTags);

                            _queryDataInitialized = true;
                        }

                        // If data file does not exist and query appears to contain an attribute
                        // query, note the issue for later logging.
                        if (!_dataFileLoaded && match.Value.IndexOf(
                                "<Attribute", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _dataFileMissing = true;
                        }

                        // If the database connection does not exist and query appears to contain an
                        // SQL query, note the issue for later logging.
                        if (_dbConnection == null && match.Value.IndexOf(
                                "<SQL", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _dbConnectionMissing = true;
                        }

                        try
                        {
                            // Append the query result to the expanded output in place of the query.
                            using (var dataQuery = DataEntryQuery.Create(match.Value, null, _dbConnection))
                            {
                                expandedOutput += string.Join("\r\n", dataQuery.Evaluate().ToStringArray());
                            }
                        }
                        catch (Exception ex)
                        {
                            expandedOutput += "<Unable to evaluate query>";
                            var ee = new ExtractException("ELI35992",
                                "Unable to expand data query in email", ex);
                            ee.AddDebugData("Query", match.Value, false);
                            ee.AddDebugData("SourceDocName", fileRecord.Name, false);
                            ee.AddDebugData("FPS",
                                pathTags.Expand(FileActionManagerPathTags.FpsFileNameTag), false);
                            ee.Log();
                        }
                    }
                    else
                    {
                        // If the database connection does not exist and the text appears to contain
                        // tags than need it, note the issue for later logging.
                        if (fileProcessingDB == null &&
                            (match.Value.IndexOf(FileActionManagerPathTags.DatabaseActionTag, StringComparison.OrdinalIgnoreCase) >= 0 ||
                             match.Value.IndexOf(FileActionManagerPathTags.DatabaseServerTag, StringComparison.OrdinalIgnoreCase) >= 0 ||
                             match.Value.IndexOf(FileActionManagerPathTags.DatabaseServerTag, StringComparison.OrdinalIgnoreCase) >= 0))

                        {
                            _dbConnectionMissing = true;
                        }

                        // Append any non-query text as is.
                        expandedOutput += match.Value;
                    }
                }

                // Once all queries have been expanded, expand any path tags and functions as well.
                expandedOutput = pathTags.Expand(expandedOutput);

                return expandedOutput;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35947");
            }
        }

        /// <summary>
        /// Determines whether the specified <see paramref="match"/> is a data query.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> to check.</param>
        /// <returns><see langword="true"/> if the match is a data query; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        static bool IsQuery(Match match)
        {
            return match.Value.StartsWith("<Query>", StringComparison.OrdinalIgnoreCase) &&
                   match.Value.EndsWith("</Query>", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Substitutes full data query syntax for any shorthand attribute queries within the
        /// specified <see paramref="match"/>.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> for which substitution should be done.
        /// </param>
        /// <returns>The text of the match with full data query syntax substituted for any shorthand
        /// attribute queries </returns>
        static string SubstituteAttributeQuery(Match match)
        {
            string result = "<Query><Attribute>" + 
                match.Value.Substring(1, match.Length - 2) +
                "</Attribute></Query>";

            return result;
        }

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// <see cref="ExtractCategories.FileProcessorsGuid"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// <see cref="ExtractCategories.FileProcessorsGuid"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="SendEmailTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="SendEmailTask"/> from which to copy.</param>
        void CopyFrom(SendEmailTask task)
        {
            _recipient = task._recipient;
            _carbonCopyRecipient = task._carbonCopyRecipient;
            _subject = task._subject;
            _body = task._body;
            _attachments = task._attachments;
            _dataFileName = task._dataFileName;

            _dirty = true;
        }

        #endregion Private Members
    }
}
