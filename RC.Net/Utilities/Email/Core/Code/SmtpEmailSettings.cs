using Extract.Encryption;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Email
{
    /// <summary>
    /// Interface definition for SMTP Email settings.
    /// </summary>
    [Guid("F16154C3-773A-45A0-9D07-05044FCFD934")]
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface ISmtpEmailSettings
    {
        #region Methods

        /// <summary>
        /// Loads the settings.
        /// </summary>
        /// <param name="userSpecific">if set to <see langword="true"/> then
        /// settings will be loaded from the users local data location; otherwise
        /// the settings will be loaded from the common application data location.</param>
        void LoadSettings(bool userSpecific);

        /// <summary>
        /// Saves the settings to the same location from which they were loaded.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if settings were never loaded.</throws>
        void SaveSettings();

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets or sets the name of the SMTP server.
        /// </summary>
        /// <value>
        /// The SMTP server.
        /// </value>
        string Server { get; set; }

        /// <summary>
        /// Gets or sets the port for the SMTP server.
        /// </summary>
        /// <value>
        /// The port for the SMTP server.
        /// </value>
        int Port { get; set; }

        /// <summary>
        /// Gets or sets the login user name.
        /// </summary>
        /// <value>
        /// The login user name. 
        /// </value>
        string UserName { get; set; }

        /// <summary>
        /// Gets or sets the login password.
        /// </summary>
        /// <value>
        /// The login password.
        /// </value>
        string Password { get; set; }

        /// <summary>
        /// Gets or sets the sender name.
        /// </summary>
        /// <value>
        /// The name of the sender.
        /// </value>
        string SenderName { get; set; }

        /// <summary>
        /// Gets or sets the sender address.
        /// </summary>
        /// <value>
        /// The sender address.
        /// </value>
        string SenderAddress { get; set; }

        /// <summary>
        /// Gets or sets the email signature.
        /// </summary>
        /// <value>
        /// The email signature.
        /// </value>
        string EmailSignature { get; set; }

        /// <summary>
        /// Gets or sets the timeout when connecting to the server.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        int Timeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email should be sent using SSL.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> then the email will be sent using SSL.
        /// <para><b>Note:</b></para>
        /// This requires that the SMTP server supports SSL communication.
        /// </value>
        bool UseSsl { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has unsaved changes.
        /// </summary>
        /// <value><see langword="true"/> if this instance has unsaved changes; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool HasUnsavedChanges { get; }

        #endregion Properties
    }

    /// <summary>
    /// Class for setting/getting extract SMTP email settings
    /// </summary>
    [Guid("9EAF8EDB-FEE3-4D46-B0FA-7792BA2205A4")]
    [ProgId("Extract.Utilities.Email.SMTPEmailSettings")]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class SmtpEmailSettings : ISmtpEmailSettings, IConfigurableObject
    {
        #region Constants

        /// <summary>
        /// The name of the folder to store the email settings in.
        /// </summary>
        const string _SETTINGS_FOLDER = "EmailSettings";

        /// <summary>
        /// The name of the file for these settings.
        /// </summary>
        const string _SETTINGS_FILE = "SMTPSettings.config";

        /// <summary>
        /// Location to store the user settings.
        /// </summary>
        static readonly string _USER_SETTINGS = Path.Combine(
            FileSystemMethods.ApplicationDataPath, _SETTINGS_FOLDER, _SETTINGS_FILE);

        /// <summary>
        /// Location to store the global settings.
        /// </summary>
        static readonly string _GLOBAL_SETTINGS = Path.Combine(
            FileSystemMethods.CommonApplicationDataPath, _SETTINGS_FOLDER, _SETTINGS_FILE);

        /// <summary>
        /// The default SMTP port value
        /// </summary>
        const int _DEFAULT_PORT = 25;

        /// <summary>
        /// Maps all property names in <see cref="ExtractSmtp"/> to the name of setting in the
        /// DBInfo table. The type of ILookup is used because it is immutable.
        /// </summary>
        // ILookup is immutable
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly ILookup<string, string> PropertyNameLookup =
            (new Dictionary<string, string>()
            {
                { "Server", "EmailServer" },
                { "Port", "EmailPort" },
                { "UserName", "EmailUserName" },
                { "Password", "EmailPassword" },
                { "SenderName", "EmailSenderName" },
                { "SenderAddress", "EmailSenderAddress" },
                { "EmailSignature", "EmailSignature" },
                { "Timeout", "EmailTimeout" },
                { "UseSsl", "EmailUseSsl" }
            }).ToLookup(pair => pair.Key, pair => pair.Value);

        #endregion Constants

        #region Fields

        /// <summary>
        /// The main settings instance used to store the email settings.
        /// </summary>
        ExtractSettingsBase<ExtractSmtp> _settings;

        /// <summary>
        /// If <see cref="_settings"/> has not been supplied, a temporary instance of
        /// <see cref="_temporarySettings"/> to maintain settings in memory, but that cannot be
        /// persisted.
        /// </summary>
        ExtractSmtp _temporarySettings;

        #endregion Fields

        #region Public Methods

        /// <summary>
        /// Loads the settings.
        /// </summary>
        /// <param name="settings">The <see cref="ExtractSettingsBase{ExtractSmtp}"/> instance from
        /// which settings should be loaded and persisted.</param>
        public void LoadSettings(ExtractSettingsBase<ExtractSmtp> settings)
        {
            try
            {
                _settings = settings;
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI35938");
            }
        }

        #endregion Public Methods

        #region ISmtpEmailSettings Members

        /// <summary>
        /// Loads the settings.
        /// </summary>
        /// <param name="userSpecific">if set to <see langword="true"/> then
        /// settings will be loaded from the users local data location; otherwise
        /// the settings will be loaded from the common application data location.</param>
        public void LoadSettings(bool userSpecific)
        {
            try
            {
                _settings = new ConfigSettings<ExtractSmtp>(
                    userSpecific ? _USER_SETTINGS : _GLOBAL_SETTINGS, false, true);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32253", "Unable to load SMTP settings.");
            }
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                ExtractException.Assert("ELI35936", "Settings have not been loaded.",
                    _settings != null);

                // Save the settings to disk
                _settings.Save();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32254", "Unable to save SMTP settings.");
            }
        }

        /// <summary>
        /// Gets or sets the name of the SMTP server.
        /// </summary>
        /// <value>
        /// The SMTP server.
        /// </value>
        public string Server
        {
            get
            {
                return Settings.Server;
            }

            set
            {
                Settings.Server = value;
            }
        }

        /// <summary>
        /// Gets or sets the port for the SMTP server.
        /// </summary>
        /// <value>
        /// The port for the SMTP server.
        /// </value>
        public int Port
        {
            get
            {
                return Settings.Port;
            }

            set
            {
                Settings.Port = value;
            }
        }

        /// <summary>
        /// Gets or sets the login user name.
        /// </summary>
        /// <value>
        /// The login user name.
        /// </value>
        public string UserName
        {
            get
            {
                try
                {
                    // return decrypted username
                    string userName = Settings.UserName;
                    if (string.IsNullOrWhiteSpace(userName))
                    {
                        return "";
                    }
                    else
                    {
                        return userName.ExtractDecrypt(new MapLabel());
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35932");
                }
            }

            set
            {
                try
                {
                    // Because encryption output may differ for the same input, don't set the
                    // username unless it has changed to prevent needless saving of data.
                    if (!UserName.Equals(value, StringComparison.Ordinal))
                    {
                        Settings.UserName = value.ExtractEncrypt(new MapLabel());
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35933");
                }
            }
        }

        /// <summary>
        /// Gets or sets the login password.
        /// </summary>
        /// <value>
        /// The login password.
        /// </value>
        public string Password
        {
            get
            {
                try
                {
                    // return decrypted password
                    string password = Settings.Password;
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        return "";
                    }
                    else
                    {
                        return password.ExtractDecrypt(new MapLabel());
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35934");
                }
            }

            set
            {
                try
                {
                    // Because encryption output may differ for the same input, don't set the
                    // password unless it has changed to prevent needless saving of data.
                    if (!Password.Equals(value, StringComparison.Ordinal))
                    {
                        Settings.Password = value.ExtractEncrypt(new MapLabel());
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35935");
                }
            }
        }

        /// <summary>
        /// Gets or sets the sender name.
        /// </summary>
        /// <value>
        /// The name of the sender.
        /// </value>
        public string SenderName
        {
            get
            {
                return Settings.SenderName;
            }

            set
            {
                Settings.SenderName = value;
            }
        }

        /// <summary>
        /// Gets or sets the sender address.
        /// </summary>
        /// <value>
        /// The sender address.
        /// </value>
        public string SenderAddress
        {
            get
            {
                return Settings.SenderAddress;
            }

            set
            {
                Settings.SenderAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the email signature.
        /// </summary>
        /// <value>
        /// The email signature.
        /// </value>
        public string EmailSignature
        {
            get
            {
                return Settings.EmailSignature;
            }

            set
            {
                Settings.EmailSignature = value;
            }
        }

        /// <summary>
        /// Gets or sets the timeout in milliseconds when connecting to the server.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public int Timeout
        {
            get
            {
                return Settings.Timeout;
            }

            set
            {
                Settings.Timeout = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the email should be sent using SSL.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> then the email will be sent using SSL.
        /// <para><b>Note:</b></para>
        /// This requires that the SMTP server supports SSL communication.
        /// </value>
        public bool UseSsl
        {
            get
            {
                return Settings.UseSsl;
            }

            set
            {
                Settings.UseSsl = value;
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
                return _settings != null && _settings.HasUnsavedChanges;
            }
        }

        #endregion

        #region IConfigurableObject Members

        /// <summary>
        /// Runs the configuration.
        /// </summary>
        /// <returns>True if the configuration was successfully updated.</returns>
        public bool RunConfiguration()
        {
            try
            {
                using (var dialog = new SmtpEmailSettingsDialog())
                {
                    return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32266", "Configuration failed.");
            }
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Gets the <see cref="ExtractSmtp"/> instance to which settings should be loaded from and
        /// applied.
        /// </summary>
        ExtractSmtp Settings
        {
            get
            {
                // If no permanent settings instance has been provided, use a temporary ExtractSmtp
                // instance.
                if (_settings == null)
                {
                    if (_temporarySettings == null)
                    {
                        _temporarySettings = new ExtractSmtp();
                    }

                    return _temporarySettings;
                }
                // Otherwise, use the setting from the permanent settings instance.
                else
                {
                    return _settings.Settings;
                }
            }
        }


        #endregion Private Members
    }
}

