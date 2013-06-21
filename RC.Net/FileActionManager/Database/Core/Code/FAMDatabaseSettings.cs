using Extract.Utilities;
using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// An <see cref="ExtractSettingsBase{T}"/> that allows persistance to and from a FAM database.
    /// </summary>
    /// <typeparam name="T">The <see cref="ApplicationSettingsBase"/> derivative to be persisted.
    /// </typeparam>
    [CLSCompliant(false)]
    public class FAMDatabaseSettings<T> : ExtractSettingsBase<T> where T : ApplicationSettingsBase, new()
    {
        #region Constants

        /// <summary>
        /// Constant for the DB setting containing last change to the DBInfo settings. 
        /// </summary>
        const string _LAST_DB_INFO_CHANGE = "LastDBInfoChange";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="IFileProcessingDB"/> to which the settings should be persisted.
        /// </summary>
        IFileProcessingDB _FAMDatabase;

        /// <summary>
        /// The prefix that should be added to all property names in <see typeparam="T"/> to obtain
        /// the name of setting in the DBInfo table.
        /// </summary>
        string _settingPrefix;

        #endregion Fields

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMDatabaseSettings{T}"/> class.
        /// </summary>
        /// <param name="FAMDatabase">The <see cref="IFileProcessingDB"/> to which the settings
        /// should be persisted.</param>
        /// <param name="settingPrefix">The prefix that should be added to all property names in
        /// <see typeparam="T"/> to obtain the name of setting in the DBInfo table.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FAM")]
        public FAMDatabaseSettings(IFileProcessingDB FAMDatabase, string settingPrefix)
            : this(FAMDatabase, true, settingPrefix)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMDatabaseSettings&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="FAMDatabase">The <see cref="IFileProcessingDB"/> to which the settings
        /// should be persisted.</param>
        /// <param name="dynamic">If <see langword="true"/>, properties will be saved to disk as 
        /// soon as they are modified and re-freshed from disk as necessary every time the Settings 
        /// property is accessed. If <see langword="false"/> the properties will only be loaded and
        /// saved to disk when explicitly requested.</param>
        /// <param name="settingPrefix">The prefix that should be added to all property names in
        /// <see typeparam="T"/> to obtain the name of setting in the DBInfo table.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FAM")]
        public FAMDatabaseSettings(IFileProcessingDB FAMDatabase, bool dynamic, string settingPrefix)
            : base(dynamic)
        {
            try 
	        {	        
		        _FAMDatabase = FAMDatabase;
                _settingPrefix = settingPrefix;

                Load();

                Constructed = true;
	        }
	        catch (Exception ex)
	        {
		        throw ex.AsExtract("ELI35923");
	        }
        }

        #endregion Contructors

        #region Overrides

        /// <summary>
        /// Loads the data from the the DBInfo table into the Settings instance.
        /// </summary>
        public override void Load()
        {
            try
            {
                base.Load();

                foreach (SettingsProperty property in Settings.Properties)
                {
                    string value =
                        _FAMDatabase.GetDBInfoSetting(_settingPrefix + property.Name, false);

                    if (value != null)
                    {
                        // Boolean values are represented by 0 or 1 in the DBInfo table
                        if (property.PropertyType == typeof(bool))
                        {
                            if (value == "1")
                            {
                                value = true.ToString(CultureInfo.InvariantCulture);
                            }
                            else if (value == "0")
                            {
                                value = false.ToString(CultureInfo.InvariantCulture);
                            }
                        }

                        UpdatePropertyFromString(property.Name, value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35922");
            }
            finally
            {
                Loaded = true;
            }
        }

        /// <summary>
        /// Gets the value of the specified property as a string.
        /// </summary>
        /// <param name="propertyName">Name of the property to retrieve.</param>
        /// <returns>The value of the specified property as a string.</returns>
        protected override string GetPropertyAsString(string propertyName)
        {
            try
            {
                if (Settings.Properties[propertyName].PropertyType == typeof(bool))
                {
                    // Boolean values are represented by 0 or 1 in the DBInfo table
                    return ((bool)Settings[propertyName]) ? "1" : "0";
                }
                else
                {
                    return base.GetPropertyAsString(propertyName);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI35943");
                ee.AddDebugData("Setting name", propertyName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Commits the specified <see paramref="value"/> of the specified property to the DBInfo
        /// table.
        /// </summary>
        /// <param name="propertyName">The name of the property to be applied.</param>
        /// <param name="value">The value to apply.</param>
        protected override void SavePropertyValue(string propertyName, string value)
        {
            try
            {
                _FAMDatabase.SetDBInfoSetting(_settingPrefix + propertyName, value, true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35921");
            }
        }

        /// <summary>
        /// Gets the most recent last modified time of a DBInfo setting.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> the DBInfo setting was last modified .</returns>
        protected override DateTime GetLastModifiedTime()
        {
            try
            {
                string lastModifiedValue = _FAMDatabase.GetDBInfoSetting(_LAST_DB_INFO_CHANGE, true);
                DateTime lastModified =
                    DateTime.Parse(lastModifiedValue, CultureInfo.InvariantCulture);

                return lastModified;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35942");
            }
        }

        #endregion Overrides
    }
}
