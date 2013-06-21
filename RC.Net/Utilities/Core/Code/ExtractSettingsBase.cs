using Extract.Licensing;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Extract.Utilities
{
    /// <summary>
    /// Wrapper for <see cref="ApplicationSettingsBase"/> objects that contain assembly settings.
    /// Allows the settings to be optionally, dynamically updated and persisted as property values
    /// are changed.
    /// </summary>
    /// <typeparam name="T">The <see cref="ApplicationSettingsBase"/> type wrapped.</typeparam>
    abstract public class ExtractSettingsBase<T> where T : ApplicationSettingsBase, new()
    {
        #region Fields

        /// <summary>
        /// The <see cref="ApplicationSettingsBase"/> instance containing the assembly settings.
        /// </summary>
        T _settings;

        /// <summary>
        /// If <see langword="true"/>, properties will be saved to disk as soon as they are
        /// modified and re-freshed from disk as necessary every time the <see cref="Settings"/>
        /// property is accessed. If <see langword="false"/> the properties will only be saved and
        /// loaded on-demand.
        /// </summary>
        bool _dynamic;

        /// <summary>
        /// Indicates whether wrapped class has application scoped properties.
        /// </summary>
        bool _hasApplicationProperties;

        /// <summary>
        /// Indicates whether wrapped class has user scoped properties.
        /// </summary>
        bool _hasUserProperties;

        /// <summary>
        /// The last known modification time of the persisted settings location. Used to determine
        /// if settings need to be updated from the persisted location.
        /// </summary>
        DateTime _lastModified;

        /// <summary>
        /// Keeps track of properties modified since the last save.
        /// </summary>
        readonly ConcurrentBag<string> _modifiedProperties = new ConcurrentBag<string>();

        /// <summary>
        /// Indicates if the settings are currently being refreshed from disk. Used to prevent
        /// recursion due to the RefreshSettings call in the Settings getter.
        /// </summary>
        bool _refreshing;

        /// <summary>
        /// Indicates whether this instance has been completely constructed (including any subclass
        /// constructors).
        /// </summary>
        bool _constructed;

        #endregion Fields
        
        #region Constructors

        /// <summary>
        /// Initializes a new ExtractSettingsBase instance.
        /// </summary>
        /// <param name="dynamic">If <see langword="true"/>, properties will be saved to their
        /// persisted location soon as they are modified and re-freshed from their persisted
        /// location as necessary every time the Settings property is accessed. If
        /// <see langword="false"/> the properties will only be loaded and saved to their persisted
        /// location when explicitly requested.</param>
        // FXCop warns because the constructor calls CheckPropertyTypes which accesses Settings
        // which calls virtual method RefreshSettings. But RefreshSettings checks the Constructed
        // property before executing thereby preventing any unintended consequences.
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected ExtractSettingsBase(bool dynamic)
        {
            try
            {
                // Validate that the calling assembly is an extract assembly
                if (!LicenseUtilities.VerifyAssemblyData(Assembly.GetCallingAssembly()))
                {
                    var ee = new ExtractException("ELI30041",
                        "Object is not usable in current configuration.");
                    ee.AddDebugData("Object Name", this.GetType().ToString(), false);
                    throw ee;
                }

                _dynamic = dynamic;

                // Create a new instance (will have the default settings)
                _settings = new T();

                // Determine whether there are application and/or user scoped properties available.
                CheckPropertyTypes();

                _settings.SettingChanging += HandleSettingChanging;
                _settings.PropertyChanged += HandlePropertyChanged;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32877");
            }
        }

        #endregion Constructors

        #region Public Members

        /// <summary>
        /// Gets the <see cref="ApplicationSettingsBase"/> instance containing the settings
        /// <para><b>Note</b></para>
        /// In dynamic mode, it is important to access settings through this property with every
        /// call to ensure settings are in sync with their persisted location.
        /// </summary>
        /// <returns>The <see cref="ApplicationSettingsBase"/> instance.</returns>
        public T Settings
        {
            get
            {
                // It is important not to call RefreshSettings if Constructed isn't set; otherwise
                // the overloaded class likely doesn't exist to be called.
                if (_dynamic && Constructed)
                {
                    // If the persisted location has been modified since the settings were last
                    // loaded, update the settings to reflect the persisted location.
                    RefreshSettings();
                }

                return _settings;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has unsaved changes.
        /// </summary>
        /// <value><see langword="true"/> if this instance has unsaved changes; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool HasUnsavedChanges
        {
            get
            {
                // If dynamic, changes will always be saved immediately.
                return (!_dynamic && !_modifiedProperties.IsEmpty);
            }
        }

        /// <summary>
        /// Specifies that some or all properties should be created in the persisted location even
        /// if the value has not changed from its default. This allows for easier manipulation of
        /// the settings by the user.
        /// </summary>
        /// <param name="propertyNames">If specified, the names of the properties to generate. If
        /// not specified, values for all members of the <see cref="Settings"/> type will be
        /// generated.</param>
        public virtual void GeneratePropertyValues(params string[] propertyNames)
        {
            try
            {
                // If no property names were not specified, generate a list of all names in Settings.
                if (propertyNames.Length == 0)
                {
                    propertyNames = Settings.Properties
                        .OfType<SettingsProperty>()
                        .Select(property => property.Name)
                        .ToArray();
                }

                // Explicity call SavePropertyValue for all specified settings to force fields to be
                // created in the persisted location.
                foreach (string settingName in propertyNames)
                {
                    SavePropertyValue(settingName);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32878");
            }
        }

        /// <summary>
        /// Loads the data from the persisted location into the <see cref="Settings"/> instance.
        /// <para><b>Note to inheritors</b></para>
        /// Call the base.Load() at the beginning of the override, before any code to load data.
        /// </summary>
        public virtual void Load()
        {
            try
            {
                Loaded = false;

                _lastModified = GetLastModifiedTime();
            }
            catch (Exception ex)
            {
                // If there was an error loading, it is possible this instance could still be used.
                // Set loaded back to true so that any property changes are still tracked.
                Loaded = true;

                throw ex.AsExtract("ELI32901");
            }
        }

        /// <summary>
        /// Saves modified <see cref="ApplicationSettingsBase"/> user properties to their persisted
        /// location.
        /// </summary>
        public virtual void Save()
        {
            try
            {
                string[] modifiedProperties = _modifiedProperties.ToArray();

                // Clear _modifiedProperties before saving the properties in the collection to this
                // point.
                string property;
                while (_modifiedProperties.TryTake(out property));

                // If there are no modified properties, there is nothing to do.
                if (modifiedProperties.Length == 0)
                {
                    return;
                }

                // Loop through each modified setting and apply them.
                foreach (string modifiedProperty in modifiedProperties)
                {
                    SavePropertyValue(modifiedProperty);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32879");
            }
        }

        #endregion Public Members

        #region Protected Members

        /// <summary>
        /// Commits the current value of the specified property to the persisted location.
        /// </summary>
        /// <param name="settingName">The name of the property to be applied.</param>
        protected virtual void SavePropertyValue(string settingName)
        {
            try
            {
                SettingsProperty property = Settings.Properties[settingName];
                ExtractException.Assert("ELI32880", "Invalid property.", property != null);

                string value = GetPropertyAsString(settingName);
                SavePropertyValue(settingName, value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32881");
            }
        }

        /// <summary>
        /// Gets the value of the specified property as a string.
        /// </summary>
        /// <param name="propertyName">Name of the property to retrieve.</param>
        /// <returns>The value of the specified property as a string.</returns>
        protected virtual string GetPropertyAsString(string propertyName)
        {
            try
            {
                object propertyObject = Settings[propertyName];
                ExtractException.Assert("ELI32883", "Invalid property.", propertyObject != null);

                Type settingType = Settings.Properties[propertyName].PropertyType;
                if (settingType == typeof(string))
                {
                    return (string)propertyObject;
                }
                else
                {
                    return TypeDescriptor.GetConverter(settingType)
                        .ConvertToString(propertyObject);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI32884");
                ee.AddDebugData("Setting name", propertyName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Updates the specified property from a string value.
        /// </summary>
        /// <param name="propertyName">Name of the setting to update</param>
        /// <param name="value">The <see langword="string"/> value to convert and apply.</param>
        protected void UpdatePropertyFromString(string propertyName, string value)
        {
            try 
	        {
                SettingsProperty property = Settings.Properties[propertyName];
                ExtractException.Assert("ELI32885", "Invalid property.", property != null);

                if (property.PropertyType == typeof(string))
                {
                    Settings[propertyName] = value;
                }
                else
                {
                    Settings[propertyName] = TypeDescriptor.GetConverter(property.PropertyType)
                        .ConvertFromString(value);
                }
	        }
	        catch (Exception ex)
	        {
                ExtractException ee = ex.AsExtract("ELI32886");
                ee.AddDebugData("Property name", propertyName, false);
                throw ee;
	        }
        }

        /// <overloads>Determines whether the specified property is a user scoped property.
        /// </overloads>
        /// <summary>Determines whether the specified property is a user scoped property.
        /// </summary>
        /// <param name="propertyName">Name of the property to check.</param>
        /// <returns><see langword="true"/> if the specified property is a user scoped property;
        /// <see langword="false"/> if it is an application scoped property.</returns>
        protected bool IsUserProperty(string propertyName)
        {
            try
            {
                SettingsProperty property = Settings.Properties[propertyName];
                ExtractException.Assert("ELI32887", "Invalid property.", property != null);

                return IsUserProperty(property);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32888");
            }
        }

        /// <summary>
        /// Determines whether the specified property is a user scoped property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns>
        /// <see langword="true"/> if the specified property is a user scoped property;
        /// <see langword="false"/> if it is an application scoped property.</returns>
        protected bool IsUserProperty(SettingsProperty property)
        {
            try
            {
                bool isUserProperty =
                    property.Attributes.ContainsKey(typeof(UserScopedSettingAttribute));
                return isUserProperty;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32889");
            }
        }

        /// <summary>
        /// Gets a value indicating whether wrapped class has application scoped properties.
        /// </summary>
        /// <value><see langword="true"/> if the wrapped class has application scoped properties;
        /// otherwise, <see langword="false"/>.</value>
        protected bool HasApplicationProperties
        {
            get
            {
                return _hasApplicationProperties;
            }
        }

        /// <summary>
        /// Gets a value indicating whether wrapped class has user scoped properties.
        /// </summary>
        /// <value><see langword="true"/> if the wrapped class has user scoped properties;
        /// otherwise, <see langword="false"/>.</value>
        protected bool HasUserProperties
        {
            get
            {
                return _hasUserProperties;
            }
        }

        /// <summary>
        /// Gets or sets whether this instance has been completely constructed (including any subclass
        /// constructors).
        /// <para><b>Note to implementers</b></para>
        /// It is required that any derived class set this property to <see langword="true"/> at
        /// the end of the the class's constructor.
        /// </summary>
        protected bool Constructed
        {
            get
            {
                return _constructed;
            }

            set
            {
                try
                {
                    if (value != _constructed)
                    {
                        ExtractException.Assert("ELI32890", "Invalid logic error.", value);
                        _constructed = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI32891");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the settings have been loaded. Changes to
        /// properties will not be tracked when this property is <see langword="false"/>.
        /// </summary>
        /// <value><see langword="true"/> if the settings have been loaded; otherwise,
        /// <see langword="false"/>.
        /// </value>
        protected bool Loaded
        {
            get;
            set;
        }

        #endregion Protected Members

        #region Abstract Methods

        /// <summary>
        /// Gets the last modified time of the persisted location.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> the persisted location was last modified .</returns>
        // The date time will generally need to be calculated, thus by Extract standards, it should
        // be a method, not a property.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected abstract DateTime GetLastModifiedTime();

        /// <summary>
        /// Commits the specified <see paramref="value"/> of the specified property to the persisted
        /// location.
        /// <para><b>Note to inheritors</b></para>
        /// Even if the specified value is the default value, this call should result in the
        /// creation of the field in the persisted location if it does not exist.
        /// </summary>
        /// <param name="propertyName">The name of the property to be applied.</param>
        /// <param name="value">The value to apply.</param>
        protected abstract void SavePropertyValue(string propertyName, string value);

        #endregion Abstract Methods

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="ApplicationSettingsBase.SettingChanging"/> event of
        /// <see cref="_settings"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Configuration.SettingChangingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleSettingChanging(object sender, SettingChangingEventArgs e)
        {
            try
            {
                // If the value being applied isn't different that the existing value, ignore the
                // change so it doesn't needlessly trigger a save or set HasUnsavedChanges.
                if (Loaded && e.NewValue.Equals(Settings[e.SettingName]))
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35941");
            }
        }

        /// <summary>
        /// Handles the case that a _settings property was changed.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        protected void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                // If the settings have not yet finished loading, don't track changes to the values.
                if (Loaded)
                {
                    if (!_modifiedProperties.Contains(e.PropertyName))
                    {
                        _modifiedProperties.Add(e.PropertyName);
                    }

                    // If in dynamic mode, save right away.
                    if (_dynamic)
                    {
                        Save();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI29691",
                    "Error saving settings change!", ex);
                ee.AddDebugData("Property Name", e.PropertyName, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Updates <see cref="HasUserProperties"/> and <see cref="HasApplicationProperties"/> to
        /// reflect whether the wrapped <see cref="ApplicationSettingsBase"/> class contains
        /// application and/or user scoped properties.
        /// </summary>
        void CheckPropertyTypes()
        {
            foreach (SettingsProperty property in Settings.Properties)
            {
                if (IsUserProperty(property))
                {
                    _hasUserProperties = true;

                    if (_hasApplicationProperties)
                    {
                        break;
                    }
                }
                else
                {
                    _hasApplicationProperties = true;

                    if (_hasUserProperties)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the persisted location has been modified since the settings were last
        /// loaded and updates the settings to reflect the persisted location if necessary.
        /// </summary>
        void RefreshSettings()
        {
            if (_refreshing || !Constructed)
            {
                return;
            }

            try
            {
                _refreshing = true;

                // Load the settings from their persisted location if they have been modifed since
                // the last time they were loaded.
                DateTime lastModified = GetLastModifiedTime();
                if (lastModified > _lastModified)
                {
                    Load();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32882");
            }
            finally
            {
                _refreshing = false;
            }
        }

        #endregion Private Members
    }
}
