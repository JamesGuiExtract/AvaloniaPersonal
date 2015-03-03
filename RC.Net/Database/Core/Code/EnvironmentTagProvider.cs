using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;

namespace Extract.Database
{
    /// <summary>
    /// Provides custom path tags in addition to built-in tags that can have different values
    /// depending on the current environment or "face". (i.e., "Test" vs. "Prod")
    /// </summary>
    [ComVisible(true)]
    [Guid("C30D753F-2B48-4101-AAB5-F84A5FC404CF")]
    [CLSCompliant(false)]
    [ProgId("Extract.Database.EnvironmentTagProvider")]
    public class EnvironmentTagProvider : IEnvironmentTagProvider
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(EnvironmentTagProvider).ToString();

        /// <summary>
        /// The name of the SQL CE database file that defines environment tags.
        /// </summary>
        static readonly string _SETTING_FILENAME = "FAMSettings.sdf";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Keeps track of the name to value for all currently defined tags.
        /// </summary>
        Dictionary<string, string> _environmentTagValues =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The path for which the environment tags apply (the directory where the FPS files that
        /// will use these settings are).
        /// </summary>
        string _contextPath;

        /// <summary>
        /// The face currently defining the tag values that will be returned by
        /// <see cref="GetTagValue"/>.
        /// </summary>
        string _activeFace;

        /// <summary>
        /// Controls access to _environmentTagValues from multiple threads.
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentTagProvider"/> class.
        /// </summary>
        public EnvironmentTagProvider()
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

        #region IEnvironmentTagProvider

        /// <summary>
        /// Gets or sets the path for which the environment tags apply (the directory where the FPS
        /// files that will use these settings are).
        /// </summary>
        /// <value>
        /// The path for which the environment tags apply
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
// Loading of environment/context specific tags disabled until some open questions with the
// implementation are resolved.
//LoadTagsForPath(value);
                        _contextPath = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI37900",
                        "Failed to load environment tags for specified path.");
                }
            }
        }

        /// <summary>
        /// Gets the face currently defining the tag values that will be returned by
        ///	<see cref="GetTagValue"/>
        /// </summary>
        public string ActiveFace
        {
            get
            {
                return _activeFace;
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
                    return _environmentTagValues.Keys.ToVariantVector();
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI37909");
                ee.AddDebugData("ActiveFace", _activeFace, false);

                throw ee.CreateComVisible("ELI37901", "Failed to load environment-specific tags.");
            }
        }

        /// <summary>
        /// Gets the value for the specified tag in the specified environment.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public string GetTagValue(string tagName)
        {
            try 
	        {
                lock (_lock)
                {
                    return _environmentTagValues[tagName];
                }
	        }
	        catch (Exception ex)
	        {
                var ee = ex.AsExtract("ELI37909");
                ee.AddDebugData("ActiveFace", _activeFace, false);
                ee.AddDebugData("TagName", tagName, false);

                throw ee.CreateComVisible("ELI37902", "Failed to retrieve environment tag value.");
	        }
        }

        #endregion IEnvironmentTagProvider

        #region Private Members

        /// <summary>
        /// Initializes <see cref="_environmentTagValues"/> with the tags for the specified
        /// <see paramref="contextPath"/>.
        /// </summary>
        /// <param name="contextPath">The path for which the environment tags are to be loaded.
        /// </param>
        void LoadTagsForPath(string contextPath)
        {
            lock (_lock)
            {
                _environmentTagValues.Clear();

                // Check to see if a settings file exists for the specified path.
                if (string.IsNullOrWhiteSpace(contextPath))
                {
                    return;
                }

                string settingFileName = Path.Combine(contextPath, _SETTING_FILENAME);
                if (!File.Exists(settingFileName))
                {
                    return;
                }
            
                // Query the database file to get the active face and associated tag values.
                using (var dbConnectionInfo = new DatabaseConnectionInfo(
                    typeof(SqlCeConnection).AssemblyQualifiedName,
                    SqlCompactMethods.BuildDBConnectionString(settingFileName)))
                {
                    dbConnectionInfo.UseLocalSqlCeCopy = true;

                    // In case some users are using a mapped drive, convert to a UNC path to try to
                    // ensure as much as possible that all users accessing the same folder will be
                    // correctly associated with the proper face.
                    string UNCPath = contextPath;
                    FileSystemMethods.ConvertToNetworkPath(ref UNCPath, false);

                    var parameters = new Dictionary<string, string>();
                    parameters["Path"] = UNCPath + "%";

                    _activeFace = DBMethods.GetQueryResultsAsStringArray(
                        dbConnectionInfo.ManagedDbConnection,
                        "SELECT [Name] FROM [Face] WHERE [RootPath] LIKE @Path", parameters, "\t")
                        .FirstOrDefault();

                    // We were able to find a proper face; load all tag values for this face.
                    if (_activeFace != null)
                    {
                        parameters.Clear();
                        parameters["Face"] = _activeFace;

                        using (DataTable results = DBMethods.ExecuteDBQuery(
                            dbConnectionInfo.ManagedDbConnection,
                            "SELECT [Tag].[Name], [FaceTag].[Value] FROM [FaceTag] " +
                            "	INNER JOIN [Face] ON [FaceTag].[FaceID] = [Face].[ID] " +
                            "	INNER JOIN [Tag] ON [FaceTag].[TagID] = [Tag].[ID] " +
                            "	WHERE [Face].[Name] = @Face", parameters))
                        {
                            // Enclose the tag name in brackets as it will appear in path tag
                            // expressions.
                            _environmentTagValues = results.Rows.OfType<DataRow>()
                                .ToDictionary(
                                    row => "<" + (string)row[0] + ">", row => (string)row[1]);
                        }
                    }
                }
            }
        }

        #endregion Private Members
    }
}
