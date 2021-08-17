using Extract.AttributeFinder;
using Extract.DataEntry;
using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.SqlDatabase;
using System;
using System.Data.Common;
using System.Data.OleDb;
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
    /// Interface definition for the Set Metadata Task
    /// </summary>
    [ComVisible(true)]
    [Guid("85673F27-B59F-45F5-8749-D03168C42AC6")]
    [CLSCompliant(false)]
    public interface ISetMetadataTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask,
        ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets the metadata field name that is being set.
        /// </summary>
        /// <value>
        /// The metadata field name that is being set.
        /// </value>
        string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the new metadata field value
        /// </summary>
        /// <value>
        /// The new metadata field value
        /// </value>
        string Value { get; set; }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> which sets a metadata value for a file.
    /// </summary>
    [ComVisible(true)]
    [Guid("28E27A76-53E1-4125-9AB4-B4F798B91853")]
    [ProgId("Extract.FileActionManager.FileProcessors.SetMetadataTask")]
    public class SetMetadataTask : ISetMetadataTask, IDisposable
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Set Metadata";

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
        /// The metadata field name that is being set.
        /// </summary>
        string _fieldName;

        /// <summary>
        /// The new metadata field value
        /// </value>
        /// </summary>
        string _value;

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
        /// Initializes a new instance of the <see cref="SetMetadataTask"/> class.
        /// </summary>
        public SetMetadataTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetMetadataTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="SetMetadataTask"/> from which settings should
        /// be copied.</param>
        public SetMetadataTask(SetMetadataTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43498");
            }
        }

        #endregion Constructors

        #region ISetMetadataTask Members

        /// <summary>
        /// Gets or sets the metadata field name that is being set.
        /// </summary>
        /// <value>
        /// The metadata field name that is being set.
        /// </value>
        public string FieldName
        {
            get
            {
                return _fieldName;
            }
            set
            {
                try
                {
					if (!string.Equals(_fieldName, value, StringComparison.OrdinalIgnoreCase))
					{
						_fieldName = value;
						_dirty = true;
					}
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI43499", "Unable to set metadata field name.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the new metadata field value
        /// </summary>
        /// <value>
        /// The new metadata field value
        /// </value>
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
				try
				{
					if (!string.Equals(_value, value, StringComparison.OrdinalIgnoreCase))
					{
						_value = value;
						_dirty = true;
					}
				}
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI43500", "Unable to set metadata value.");
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
        /// Performs configuration needed to create a valid <see cref="SetMetadataTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                using (var dialog = new SetMetadataTaskSettingsDialog())
                {
                    dialog.FieldName = FieldName;
                    dialog.Value = Value;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        FieldName = dialog.FieldName;
                        Value = dialog.Value;

                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI43501",
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
            return !string.IsNullOrWhiteSpace(_fieldName);
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="SetMetadataTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="SetMetadataTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new SetMetadataTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI43502", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="SetMetadataTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as SetMetadataTask;
                if (task == null)
                {
                    throw new InvalidCastException("Invalid cast to SetMetadataTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI43503", "Unable to copy object.", ex);
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
                    "ELI43504", _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI43505", "Unable to initialize \"Set Metadata\" task.");
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
                    "ELI43506", _COMPONENT_DESCRIPTION);

                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(pFAMTM, pFileRecord.Name);
                string fieldName = ExpandText(FieldName, pFileRecord, pathTags, pDB);
                string value = ExpandText(Value, pFileRecord, pathTags, pDB);

				pDB.SetMetadataFieldValue(pFileRecord.FileID, fieldName, value);

                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI43507", "Unable to process the file.");
            }
            finally
            {
                _dbConnection?.Dispose();
                _dbConnection = null;
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
                throw ExtractException.CreateComVisible("ELI43508",
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
                    _fieldName = reader.ReadString();
                    _value = reader.ReadString();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI43509",
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
                    writer.Write(_fieldName);
                    writer.Write(_value);

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
                throw ExtractException.CreateComVisible("ELI43510",
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
        /// Releases all resources used by the <see cref="SetMetadataTask"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="SetMetadataTask"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="SetMetadataTask"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                _dbConnection?.Dispose();
                _dbConnection = null;
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
                            throw new ExtractException("ELI43511", "The data file necessary to expand " +
                                "text could not be found; some text may be missing/invalid.");
                        }

                        // If the database connection does not exist and query appears to contain an
                        // SQL query, note the issue for later logging.
                        if (_dbConnection == null && match.Value.IndexOf(
                                "<SQL", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            throw new ExtractException("ELI43512", "No database connection was available " +
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
                            var ee = new ExtractException("ELI43513",
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
                throw ex.AsExtract("ELI43514");
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
        /// Copies the specified <see cref="SetMetadataTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="SetMetadataTask"/> from which to copy.</param>
        void CopyFrom(SetMetadataTask task)
        {
            _fieldName = task.FieldName;
            _value = task.Value;

            _dirty = true;
        }

        #endregion Private Members
    }
}
