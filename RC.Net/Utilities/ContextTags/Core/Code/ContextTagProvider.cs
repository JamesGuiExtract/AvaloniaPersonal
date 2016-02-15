﻿using Extract.Database;
using Extract.Licensing;
using Extract.SQLCDBEditor;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.ContextTags
{
    /// <summary>
    /// Provides context-specific path tags in addition to built-in tags that can have different
    /// values depending on the current context. (i.e., "Test" vs. "Prod")
    /// </summary>
    [ComVisible(true)]
    [Guid("C30D753F-2B48-4101-AAB5-F84A5FC404CF")]
    [CLSCompliant(false)]
    [ProgId("Extract.Utilities.ContextTags.ContextTagProvider")]
    public class ContextTagProvider : IContextTagProvider
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ContextTagProvider).ToString();

        /// <summary>
        /// The name of the SQL CE database file that defines the context-specific tags.
        /// </summary>
        static readonly string _SETTING_FILENAME = "CustomTags.sdf";

        /// <summary>
        /// The label of the option in the tags list to edit the available custom tags.
        /// </summary>
        static readonly string _EDIT_CUSTOM_TAGS_LABEL = "Edit custom tags...";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Keeps track of the value for all tags in the current context.
        /// </summary>
        Dictionary<string, string> _tagValues =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// A cached <see cref="VariantVector"/> for each thread that calls into GetTagNames.
        /// </summary>
        Dictionary<int, VariantVector> _tagNamesVectors = new Dictionary<int, VariantVector>();

        /// <summary>
        /// The path for which the tags apply (the directory where the FPS files that will use
        /// these settings are).
        /// </summary>
        string _contextPath;

        /// <summary>
        /// The context currently defining the tag values that will be returned by
        /// <see cref="GetTagValue"/>.
        /// </summary>
        string _activeContext;

        /// <summary>
        /// Controls access to _tagValues from multiple threads.
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagProvider"/> class.
        /// </summary>
        public ContextTagProvider()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI37898",
                    _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37899");
            }
        }

        #endregion Constructors

        #region IContextTagProvider

        /// <summary>
        /// Gets or sets the path for which the environment tags apply (the directory where the FPS
        /// files that will use these settings are).
        /// </summary>
        /// <value>
        /// The path for which the context-specific tags apply
        /// </value>
        public string ContextPath
        {
            get
            {
                return _contextPath;
            }

            set
            {
                try
                {
                    if (value != _contextPath)
                    {
                        LoadTagsForPath(value);
                        _contextPath = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI37900",
                        "Failed to load context-specific tags for specified path.");
                }
            }
        }

        /// <summary>
        /// Gets the context currently defining the tag values that will be returned by
        ///	<see cref="GetTagValue"/>
        /// </summary>
        public string ActiveContext
        {
            get
            {
                return (_activeContext == null) ? "": _activeContext;
            }
        }

        /// <summary>
        /// Gets a <see cref="VariantVector"/> of all environment-specific tags available for use.
        /// </summary>
        /// <returns>
        /// A <see cref="VariantVector"/> of all environment-specific tags available for use.
        /// </returns>
        public VariantVector GetTagNames()
        {
            try
            {
                lock (_lock)
                {
                    // https://extract.atlassian.net/browse/ISSUE-13001
                    // Besides the processing overhead of re-generating the VariantVector for each
                    // call, it was leading to memory leaks (the ReportMemoryUsage framework doesn't
                    // currently support use on VariantVectors). Generate and re-use a single
                    // VariantVector instance for each thread that calls in.
                    VariantVector tagNamesVector;
                    if (!_tagNamesVectors.TryGetValue(Thread.CurrentThread.ManagedThreadId, out tagNamesVector))
                    {
                        tagNamesVector = _tagValues.Keys.ToVariantVector();
                        _tagNamesVectors[Thread.CurrentThread.ManagedThreadId] = tagNamesVector;
                    }

                    return tagNamesVector;
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI37909");
                ee.AddDebugData("ActiveContext", _activeContext, false);

                throw ee.CreateComVisible("ELI37901", "Failed to load context-specific tags.");
            }
        }

        /// <summary>
        /// Gets the value for the specified tag in the <see cref="ActiveContext"/>.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>The value for the specified tag in the <see cref="ActiveContext"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public string GetTagValue(string tagName)
        {
            try 
	        {
                lock (_lock)
                {
                    return _tagValues[tagName];
                }
	        }
	        catch (Exception ex)
	        {
                var ee = ex.AsExtract("ELI37962");
                ee.AddDebugData("ActiveContext", _activeContext, false);
                ee.AddDebugData("TagName", tagName, false);

                throw ee.CreateComVisible("ELI37902", "Failed to retrieve environment tag value.");
	        }
        }

        /// <summary>
        /// Displays a UI to edit the available tags for the specified bstrContextPath.
        /// </summary>
        /// <param name="hParentWindow">If not <see langword="null"/>, the tag editing UI will be
        /// displayed modally this window; otherwise the editor window will be modeless.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public void EditTags(int hParentWindow)
        {
            try
            {
                ExtractException.Assert("ELI38057", "Null argument exception", hParentWindow != 0);

                lock (_lock)
                {
                    ExtractException.Assert("ELI38056",
                        "Cannot edit tags when context has not been set",
                        !string.IsNullOrWhiteSpace(ContextPath));

                    bool createdDatabase = false;

                    // Create the database if it doesn't already exist.
                    string settingFileName = Path.Combine(ContextPath, _SETTING_FILENAME);
                    if (!File.Exists(settingFileName))
                    {
                        var manager = new ContextTagDatabaseManager(settingFileName);
                        manager.CreateDatabase(true);
                        createdDatabase = true;
                    }

                    EditDatabase(settingFileName, (IntPtr)hParentWindow);

                    // Re-load so that the available tags reflect the edits.
                    if (!LoadTagsForPath(ContextPath) && createdDatabase)
                    {
                        // If the database didn't previously exist and no tags were added, don't
                        // keep the database file.
                        FileSystemMethods.DeleteFile(settingFileName);

                        // Clear the "No context defined!" message if the database isn't to be
                        // persisted.
                        _activeContext = "";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38047", "Error editing context-specific tags.");
            }
        }

        /// <summary>
        /// Gets the tags that have not been defined values in the current context.
        /// </summary>
        /// <returns>The tags that have not been defined values in the current context.</returns>
        public VariantVector GetUndefinedTags()
        {
            try 
	        {
                if (string.IsNullOrWhiteSpace(_activeContext))
                {
                    return new VariantVector();
                }

                return _tagValues
                    .Where(tag => string.IsNullOrWhiteSpace(tag.Value) && 
                        !tag.Key.Equals(_EDIT_CUSTOM_TAGS_LABEL, StringComparison.OrdinalIgnoreCase))
                    .Select(tag => tag.Key)
                    .ToVariantVector();
	        }
	        catch (Exception ex)
	        {
		        throw ex.CreateComVisible("ELI38094", "Failed to check tag values.");
	        }
        }

        /// <summary>
        /// Clears the loaded tags and reloads the tags from the database 
        /// </summary>
        public void RefreshTags()
        {
            try
            {
                LoadTagsForPath(_contextPath);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38306", "Failed to load tags.");
            }
        }

        #endregion IContextTagProvider

        #region Private Members

        /// <summary>
        /// Initializes <see cref="_tagValues"/> with the tags for the specified
        /// <see paramref="contextPath"/>.
        /// </summary>
        /// <param name="contextPath">The path for which the context-specific tags are to be loaded.
        /// </param>
        /// <returns><see langword="true"/> if a non-empty database was loaded;
        /// <see langword="false"/> if the database did not exist or was empty.</returns>
        bool LoadTagsForPath(string contextPath)
        {
            lock (_lock)
            {
                _tagValues.Clear();
                _tagNamesVectors.Clear();

                // Clear the active context
                _activeContext = "";

                // If no path is specified, don't load any tags.
                if (string.IsNullOrWhiteSpace(contextPath))
                {
                    return false;
                }

                // If _SETTING_FILENAME doesn't exist, there is nothing more to do.
                string settingFileName = Path.Combine(contextPath, _SETTING_FILENAME);
                if (!File.Exists(settingFileName))
                {
                    // Even if there are no custom tags available, provide the option to edit.
                    _tagValues.Add(_EDIT_CUSTOM_TAGS_LABEL, "");
                    return false;
                }

                bool databasePopulated = false;
            
                // Query the database file to get the active context and associated tag values.
                using (var dbConnectionInfo = new DatabaseConnectionInfo(
                    typeof(SqlCeConnection).AssemblyQualifiedName,
                    SqlCompactMethods.BuildDBConnectionString(settingFileName)))
                {
                    dbConnectionInfo.UseLocalSqlCeCopy = true;

                    using (ContextTagDatabase database = new ContextTagDatabase(
                        dbConnectionInfo.ManagedDbConnection))
                    {
                        // In case some users are using a mapped drive, convert to a UNC path to try
                        // to ensure as much as possible that all users accessing the same folder
                        // will be correctly associated with the proper context.
                        string UNCPath = contextPath;
                        FileSystemMethods.ConvertToNetworkPath(ref UNCPath, false);
                        if (UNCPath.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
                        {
                            UNCPath = UNCPath.Substring(0, UNCPath.Length - 1);
                        }

                        _activeContext = database.GetContextNameForDirectory(UNCPath);

                        // We were able to find a proper context; load all tag values for this
                        // context.
                        if (_activeContext != null)
                        {
                            // The row that is mapped to the new row in a DataGridView can end up being
                            // persisted. Ignore any unnamed custom tags.
                            _tagValues = database.TagValue
                                .Where(tagValue => tagValue.Context.Name.Equals(_activeContext) &&
                                    tagValue.CustomTag.Name != "")
                                .OrderBy(tagValue => tagValue.CustomTag.Name)
                                .ToDictionary(tagValue => 
                                    tagValue.CustomTag.Name, tagValue => tagValue.Value);
                        }
                        else
                        {
                            _activeContext = "No context defined!";
                        }
 
 						databasePopulated = database.Context.Any() || database.CustomTag.Any();
                   }
                }

                _tagValues.Add(_EDIT_CUSTOM_TAGS_LABEL, "");

                return databasePopulated;
            }
        }

        /// <summary>
        /// Opens the specified <see paramref="databaseFile"/> for editing in the
        /// <see cref="SQLCDBEditor"/>.
        /// </summary>
        /// <param name="databaseFile">The database file to edit.</param>
        /// <param name="parentWindow">If not <see langword="null"/>, the tag editing UI will be
        /// displayed modally this window; otherwise the editor window will be modeless.</param>
        void EditDatabase(string databaseFile, IntPtr parentWindow)
        {
            // The form will be launched in a different thread; use finishedEvent to keep track of
            // when the editor UI has been closed.
            using (ManualResetEvent finishedEvent = new ManualResetEvent(false))
            {
                lock (_lock)
                {

                    SQLCDBEditorForm sqlDbEditorForm = null;

                    // The form needs to be launched into it's own STA thread. Otherwise there are
                    // message handling issue that can interfere with some WinForms code.
                    Thread uiThread = new Thread(() =>
                    {
                        try
                        {
                            // A new form is needed.
                            sqlDbEditorForm = new SQLCDBEditorForm(databaseFile, false);

                            if (parentWindow == IntPtr.Zero)
                            {
                                // Run non-modal. Processing of a subsequent call can be
                                // processed as soon as the form has been activated.
                                sqlDbEditorForm.Activated += (sender, e) => finishedEvent.Set();
                                Application.Run(sqlDbEditorForm);
                            }
                            else
                            {
                                // Run modally to owner.
                                sqlDbEditorForm.ShowDialog(
                                    FormsMethods.WindowFromHandle(parentWindow));
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI38054");
                        }
                        finally
                        {
                            if (sqlDbEditorForm != null)
                            {
                                sqlDbEditorForm.Dispose();
                                sqlDbEditorForm = null;
                            }

                            finishedEvent.Set();
                        }
                    });
                    uiThread.SetApartmentState(ApartmentState.STA);
                    uiThread.Start();

                    // Don't release the lock this call has until either the form has finished
                    // initializing and been activated or a modal instance has been closed.
                    WaitForHandle(finishedEvent, parentWindow != IntPtr.Zero);
                }
            }
        }

        /// <summary>
        /// Idles the current thread until while running the message loop until 
        /// <see paramref="waitHandle"/> is signaled.
        /// </summary>
        /// <param name="waitHandle">The <see cref="WaitHandle"/> to wait upon.</param>
        /// <param name="doNonInputEvents">If <see langword="true"/>, non-user input events
        /// necessary to initialize the editor UI as a modal form will be processed during this
        /// wait; User input events will be ignored.</param>
        static void WaitForHandle(WaitHandle waitHandle, bool doNonInputEvents)
        {
            while (!waitHandle.WaitOne(0))
            {
                if (doNonInputEvents)
                {
                    WindowsMessage.DoEventsExcept(WindowsMessage.UserInputMessages);
                }
            }
        }

        #endregion Private Members
    }
}
