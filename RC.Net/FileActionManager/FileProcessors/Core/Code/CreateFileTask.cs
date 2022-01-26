using Extract.AttributeFinder;
using Extract.DataEntry;
using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.SqlDatabase;
using Extract.Utilities;
using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
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
    /// Specifies a the resolution <see cref="CreateFileTask"/> should take when the target file
    /// already exists.
    /// </summary>
    [ComVisible(true)]
    [Guid("CDE405F7-803B-45C1-BD36-4D53C589DC56")]
    public enum CreateFileConflictResolution
    {
        /// <summary>
        /// Don't write the file; throw an exception.
        /// </summary>
        GenerateError = 0,

        /// <summary>
        /// Don't write the file; continue without exception.
        /// </summary>
        SkipWithoutError = 1,

        /// <summary>
        /// Overwrite the existing file.
        /// </summary>
        Overwrite = 2,

        /// <summary>
        /// Append to the existing file.
        /// </summary>
        Append = 3
    }

    /// <summary>
    /// Interface definition for the Create File Task
    /// </summary>
    [ComVisible(true)]
    [Guid("A569A02E-8498-44BA-B007-0961ED223B98")]
    [CLSCompliant(false)]
    public interface ICreateFileTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask,
        ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets the name of the target file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file contents. This may be <see cref="String.Empty"/>
        /// or <see langword="null"/> to produce an empty file.
        /// </summary>
        /// <value>
        /// The file contents.
        /// </value>
        string FileContents { get; set; }

        /// <summary>
        /// Gets or sets the behavior of the task when the target file exists.
        /// </summary>
        /// <value>
        /// The task behavior when the target file exists.
        /// </value>
        CreateFileConflictResolution FileExistsBehavior { get; set; }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> which generates a file.
    /// </summary>
    [ComVisible(true)]
    [Guid("4D7F59D3-ECD2-46F0-8750-71194A131777")]
    [ProgId("Extract.FileActionManager.FileProcessors.CreateFileTask")]
    public class CreateFileTask : ICreateFileTask, IDisposable
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Create file";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

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

        /// <summary>
        /// The name of the file to be generated.
        /// </summary>
        string _fileName;

        /// <summary>
        /// The contents of the file to be generated.
        /// </summary>
        string _fileContents;

        /// <summary>
        /// The <see cref="CreateFileConflictResolution"/> that should be employed when the target
        /// file already exists.
        /// </summary>
        CreateFileConflictResolution _conflictResolution;

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// The name of the VOA file that should be used to expand any attribute queries.
        /// </summary>
        string _dataFileName = "<SourceDocName>.voa";

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
        /// The <see cref="DbConnection"/> to use to resolve data queries.
        /// </summary>
        DbConnection _dbConnection;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFileTask"/> class.
        /// </summary>
        public CreateFileTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFileTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="CreateFileTask"/> from which settings should
        /// be copied.</param>
        public CreateFileTask(CreateFileTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31849");
            }
        }

        #endregion Constructors

        #region ICreateFileTask Members

        /// <summary>
        /// Gets or sets the name of the target file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentNullException("value");
                    }
                    else if (value.Equals("<SourceDocName>", StringComparison.Ordinal))
                    {
                        throw new ExtractException("ELI32393",
                            "Cannot overwrite source document.");
                    }

                    _dirty |= !string.Equals(_fileName, value, StringComparison.OrdinalIgnoreCase);
                    _fileName = value;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32391", "Unable to set file name.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the file contents. This may be <see cref="String.Empty"/>
        /// or <see langword="null"/> to produce an empty file.
        /// </summary>
        /// <value>
        /// The file contents.
        /// </value>
        public string FileContents
        {
            get
            {
                return _fileContents;
            }
            set
            {
                _dirty |= !string.Equals(_fileContents, value, StringComparison.Ordinal);
                _fileContents = value;
            }
        }

        /// <summary>
        /// Gets or sets the behavior of the task when the target file exists.
        /// </summary>
        /// <value>
        /// The task behavior when the target file exists.
        /// </value>
        public CreateFileConflictResolution FileExistsBehavior
        {
            get
            {
                return _conflictResolution;
            }
            set
            {
                try
                {
                    if (!Enum.IsDefined(typeof(CreateFileConflictResolution), value))
                    {
                        throw new ArgumentOutOfRangeException("value", value,
                            "Invalid value specified.");
                    }

                    _dirty |= _conflictResolution != value;
                    _conflictResolution = value;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32392",
                        "Unable to set behavior when target file exists.");
                }
            }
        }

        #endregion

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
        /// Performs configuration needed to create a valid <see cref="CreateFileTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                using (var dialog = new CreateFileTaskSettingsDialog())
                {
                    dialog.FileName = _fileName;
                    dialog.FileContents = _fileContents;
                    dialog.CreateFileConflictResolution = _conflictResolution;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _fileName = dialog.FileName;
                        _fileContents = dialog.FileContents;
                        _conflictResolution = dialog.CreateFileConflictResolution;

                        _dirty = true;
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31833",
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
            return !string.IsNullOrWhiteSpace(_fileName);
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="CreateFileTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="CreateFileTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new CreateFileTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31834", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="CreateFileTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as CreateFileTask;
                if (task == null)
                {
                    throw new InvalidCastException("Invalid cast to CreateFileTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31835", "Unable to copy object.", ex);
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
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
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
                    "ELI31850", _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31851", "Unable to initialize \"Create file\" task.");
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
            try
            {
                _queryDataInitialized = false;
                _dataFileLoaded = false;
                _dbConnection = null;

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI31836", _COMPONENT_DESCRIPTION);

                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(pFAMTM, pFileRecord.Name);
                string fileName = ExpandText(_fileName, pFileRecord, pathTags, pDB);
                string fileContents = ExpandText(_fileContents, pFileRecord, pathTags, pDB);

                ExtractException.Assert("ELI31854",
                    "\"Create file\" task cannot write to the source document",
                    !fileName.Equals(pFileRecord.Name, StringComparison.OrdinalIgnoreCase));

                // Create the directory the file is to be written to if it does not already exist.
                string directory = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(fileName))
                {
                    switch (_conflictResolution)
                    {
                        case CreateFileConflictResolution.GenerateError:
                            {
                                ExtractException ee = new ExtractException("ELI31852",
                                    "Create file task failed to create the file because it already existed.");
                                ee.AddDebugData("Filename", fileName, false);
                                throw ee;
                            }

                        case CreateFileConflictResolution.SkipWithoutError:
                            {
                                return EFileProcessingResult.kProcessingSuccessful;
                            }
                    }
                }

                if (_conflictResolution == CreateFileConflictResolution.Append)
                {
                    // Perform the append operation in a retry block
                    FileSystemMethods.PerformFileOperationWithRetry(
                        () => File.AppendAllText(fileName, fileContents),
                        true);
                }
                else
                {
                    File.WriteAllText(fileName, fileContents);
                }

                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31837", "Unable to process the file.");
            }
            finally
            {
                if (_dbConnection != null)
                {
                    _dbConnection.Dispose();
                    _dbConnection = null;
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
                return LicenseUtilities.IsLicensed(LicenseIdName.FileActionManagerObjects);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31838",
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
                    _fileName = reader.ReadString();
                    _fileContents = reader.ReadString();
                    _conflictResolution = (CreateFileConflictResolution)reader.ReadInt32();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31839",
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
                    writer.Write(_fileName);
                    writer.Write(_fileContents);
                    writer.Write((int)_conflictResolution);

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
                throw ExtractException.CreateComVisible("ELI31840",
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

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="CreateFileTask"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="CreateFileTask"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="CreateFileTask"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
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
                            string dataFileName = pathTags.Expand(_dataFileName);
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
                            throw new ExtractException("ELI38203", "The data file necessary to expand " +
                                "text could not be found; some text may be missing/invalid.");
                        }

                        // If the database connection does not exist and query appears to contain an
                        // SQL query, note the issue for later logging.
                        if (_dbConnection == null && match.Value.IndexOf(
                                "<SQL", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            throw new ExtractException("ELI38204", "No database connection was available " +
                                "to expand text; some text may be missing/invalid.");
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
                            var ee = new ExtractException("ELI38205",
                                "Unable to expand data query for file.", ex);
                            ee.AddDebugData("Query", match.Value, false);
                            ee.AddDebugData("SourceDocName", fileRecord.Name, false);
                            ee.AddDebugData("FPS",
                                pathTags.Expand(FileActionManagerPathTags.FpsFileNameTag), false);
                            throw ee;
                        }
                    }
                    else
                    {
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
                throw ex.AsExtract("ELI38206");
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
        /// Copies the specified <see cref="CreateFileTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="CreateFileTask"/> from which to copy.</param>
        void CopyFrom(CreateFileTask task)
        {
            _fileName = task._fileName;
            _fileContents = task._fileContents;
            _conflictResolution = task._conflictResolution;

            _dirty = true;
        }

        #endregion Private Members
    }
}
