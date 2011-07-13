using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Microsoft.Win32;

namespace Extract.Utilities
{
    /// <summary>
    /// Wrapper for <see cref="ApplicationSettingsBase"/> that allows the settings to be
    /// loaded/saved to a specified registry key and, optionally, dynamically updated as property
    /// values are changed.
    /// </summary>
    public sealed class RegistrySettings<T> : ExtractSettingsBase<T> where T : ApplicationSettingsBase, new()
    {
        #region Fields

        /// <summary>
        /// The <see cref="RegistryKey"/> which application scope settings will be saved/loaded to/from.
        /// </summary>
        RegistryKey _localMachineKey;

        /// <summary>
        /// The <see cref="RegistryKey"/> which user scope settings will be saved/loaded to/from.
        /// </summary>
        RegistryKey _currentUserKey;

        /// <summary>
        /// Mutex to protect access to the registry key (which are not guaranteed to be thread-safe).
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Consturctors

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrySettings&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="registryKeyName">Name of the registry key.</param>
        public RegistrySettings(string registryKeyName)
            : this(registryKeyName, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrySettings&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="registryKeyName">Name of the registry key.</param>
        /// <param name="dynamic">if set to <see langword="true"/> [dynamic].</param>
        public RegistrySettings(string registryKeyName, bool dynamic)
            :base(dynamic)
        {
            try
            {
                if (HasApplicationProperties)
                {
                    _localMachineKey = GetRegistryKey(Registry.LocalMachine, registryKeyName);
                }

                if (HasUserProperties)
                {
                    _currentUserKey = GetRegistryKey(Registry.CurrentUser, registryKeyName);
                }

                Load();

                Constructed = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32897");
            }
        }

        #endregion Consturctors

        #region Overrides

        /// <summary>
        /// Loads the data from the registry into the Settings instance.
        /// </summary>
        public override void Load()
        {
            try
            {
                base.Load();

                lock (_lock)
                {
                    foreach (SettingsProperty property in Settings.Properties)
                    {
                        string value = null;
                        if (IsUserProperty(property))
                        {
                            if (_currentUserKey != null)
                            {
                                value = _currentUserKey.GetValue(property.Name) as string;
                            }
                        }
                        else if (_localMachineKey != null)
                        {
                            value = _localMachineKey.GetValue(property.Name) as string;
                        }

                        if (value != null)
                        {
                            UpdatePropertyFromString(property.Name, value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32898");
            }
        }

        /// <summary>
        /// Commits the specified <see paramref="value"/> of the specified property to the registry.
        /// </summary>
        /// <param name="propertyName">The name of the property to be applied.</param>
        /// <param name="value">The value to apply.</param>
        protected override void SavePropertyValue(string propertyName, string value)
        {
            try
            {
                if (IsUserProperty(propertyName))
                {
                    lock (_lock)
                    {
                        if (_currentUserKey != null)
                        {
                            _currentUserKey.SetValue(propertyName, value);
                        }
                    }
                }
                else
                {
                    // We may not have been able to write to the local machine key. Ignore any
                    // problems saving as the only reason an application scoped value should be
                    // saved is for the GeneratePropertyValues call-- it is expected that the keys
                    // will not be generated for user's without appropriate permissions.
                    try
                    {
                        lock (_lock)
                        {
                            if (_localMachineKey != null)
                            {
                                _localMachineKey.SetValue(propertyName, value);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32899");
            }
        }

        /// <summary>
        /// Gets the most recent last modified time of either of the registry keys.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> the registry was last modified .</returns>
        protected override DateTime GetLastModifiedTime()
        {
            try
            {
                DateTime lastModified = new DateTime();
                if (_localMachineKey != null)
                {
                    lastModified = NativeMethods.GetRegistryKeyLastWriteTime(_localMachineKey);
                }

                if (_currentUserKey != null)
                {
                    DateTime userLastModified =
                        NativeMethods.GetRegistryKeyLastWriteTime(_currentUserKey);
                    lastModified = new DateTime(Math.Max(userLastModified.Ticks, lastModified.Ticks));
                }

                return lastModified;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32900");
            }
        }

        #endregion Overrides

        /// <summary>
        /// Attempts to open the specified registry key with as much permission as possible. This
        /// includes creating the key if it is missing and permissions allow for its creation.
        /// </summary>
        /// <param name="hive">The registry hive in which the specified key should be opened; either
        /// <see cref="Registry.CurrentUser"/> or <see cref="Registry.LocalMachine"/>.</param>
        /// <param name="registryKeyName">The name of the registry key to open.</param>
        /// <returns>The <see cref="RegistryKey"/> if it could be opened.</returns>
        static RegistryKey GetRegistryKey(RegistryKey hive, string registryKeyName)
        {
            RegistryKey key = null;

            try
            {
                key = hive.CreateSubKey(registryKeyName);
            }
            catch
            {
                try
                {
                    key = hive.OpenSubKey(registryKeyName, false);
                }
                catch (Exception ex2)
                {
                    // We should at least be able to read user-scoped
                    if (hive == Registry.CurrentUser)
                    {
                        ExtractException ee = new ExtractException("ELI32912",
                            "Failed to open registry key; default values will be used.", ex2);
                        ee.AddDebugData("Registry hive", hive.Name, false);
                        ee.AddDebugData("Registry key", registryKeyName, false);
                        ee.Display();
                    }
                }
            }

            return key;
        }
    }
}
