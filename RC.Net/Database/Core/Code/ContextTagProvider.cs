using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;

namespace Extract.Database
{
    /// <summary>
    /// Provides context-specific path tags in addition to built-in tags that can have different
    /// values depending on the current context. (i.e., "Test" vs. "Prod")
    /// </summary>
    [ComVisible(true)]
    [Guid("C30D753F-2B48-4101-AAB5-F84A5FC404CF")]
    [CLSCompliant(false)]
    [ProgId("Extract.Database.ContextTagProvider")]
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

        #endregion Constants

        #region Fields

        /// <summary>
        /// Keeps track of the value for all tags in the current context.
        /// </summary>
        Dictionary<string, string> _tagValues =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
                return _activeContext;
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
                    return _tagValues.Keys.ToVariantVector();
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
        /// <param name="contextPath">The context path for which tags are being edited.</param>
        /// <param name="hParentWindow">If not <see langword="null"/>, the tag editing UI will be
        /// displayed modally this window; otherwise the editor window will be modeless.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public void EditTags(string contextPath, IntPtr hParentWindow)
        {
            try
            {
                lock (_lock)
                {
                    // If no path is specified, don't load any tags.
                    if (string.IsNullOrWhiteSpace(contextPath))
                    {
                        return;
                    }

                    // Create the database if it doesn't already exist.
                    string settingFileName = Path.Combine(contextPath, _SETTING_FILENAME);
                    if (!File.Exists(settingFileName))
                    {
                        var manager = new ContextTagDatabaseManager(settingFileName);
                        manager.CreateDatabase(true);
                    }

                    // TODO: Open the SDF database.
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38047", "Error editing context-specific tags.");
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
        void LoadTagsForPath(string contextPath)
        {
            lock (_lock)
            {
                _tagValues.Clear();

                // If no path is specified, don't load any tags.
                if (string.IsNullOrWhiteSpace(contextPath))
                {
                    return;
                }

                // If _SETTING_FILENAME doesn't exist, there is nothing more to do.
                string settingFileName = Path.Combine(contextPath, _SETTING_FILENAME);
                if (!File.Exists(settingFileName))
                {
                    return;
                }
            
                // Query the database file to get the active context and associated tag values.
                using (var dbConnectionInfo = new DatabaseConnectionInfo(
                    typeof(SqlCeConnection).AssemblyQualifiedName,
                    SqlCompactMethods.BuildDBConnectionString(settingFileName)))
                {
                    dbConnectionInfo.UseLocalSqlCeCopy = true;

                    using (ContextTagDatabase database = new ContextTagDatabase(
                        (SqlCeConnection)dbConnectionInfo.ManagedDbConnection))
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

                        _activeContext = database.Context
                            .Where(context => context.FPSFileDir == UNCPath)
                            .Select(context => context.Name)
                            .FirstOrDefault();

                        // We were able to find a proper context; load all tag values for this
                        // context.
                        if (_activeContext != null)
                        {
                            _tagValues = database.TagValue
                                .Where(tagValue => tagValue.Context.Name.Equals(_activeContext))
                                .ToDictionary(tagValue => 
                                    tagValue.CustomTag.Name, tagValue => tagValue.Value);
                        }
                    }
                }
            }
        }

        #endregion Private Members
    }
}
