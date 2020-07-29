using Extract.Licensing.Internal;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Extract.FAMDBCounterManager
{
    /// <summary>
    /// Class for setting/getting email settings used for the FAM DB Counter Manager utility.
    /// <para><b>Note</b></para>
    /// This class is a modified copy of Extract.Utilities.Email.SmtpEmailSettings. This project is
    /// not linked to Extract.Utilities.Email to avoid COM dependencies.
    /// </summary>
    internal class EmailSettingsManager
    {
        #region Constants

        /// <summary>
        /// The name of the file for these settings.
        /// </summary>
        const string _SETTINGS_FILE = "EmailSettings.config";

        /// <summary>
        /// Location to store the global settings.
        /// </summary>
        static readonly string _GLOBAL_SETTINGS = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _SETTINGS_FILE);

        /// <summary>
        /// The default SMTP port value
        /// </summary>
        const int _DEFAULT_PORT = 25;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The main settings instance used to store the email settings.
        /// </summary>
        ExtractSettingsBase<EmailSettings> _settings;

        /// <summary>
        /// If <see cref="_settings"/> has not been supplied, a temporary instance of
        /// <see cref="_temporarySettings"/> to maintain settings in memory, but that cannot be
        /// persisted.
        /// </summary>
        EmailSettings _temporarySettings;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSettingsManager"/> class.
        /// </summary>
        public EmailSettingsManager()
        {
            LoadSettings();
        }

        #endregion Constructors

        #region Properties

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
                return Settings.UserName;
            }

            set
            {
                Settings.UserName = value;
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
                // return decrypted password
                string password = Settings.Password;
                if (string.IsNullOrWhiteSpace(password))
                {
                    return "";
                }
                else
                {
                    return Decrypt(password);
                }
            }

            set
            {
                // Because encryption output may differ for the same input, don't set the
                // password unless it has changed to prevent needless saving of data.
                if (!Password.Equals(value, StringComparison.Ordinal))
                {
                    Settings.Password = Encrypt(value);
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
        /// Gets or sets the email subject template text.
        /// </summary>
        /// <value>
        /// The email subject template text.
        /// </value>
        public string SubjectTemplate
        {
            get
            {
                return Settings.SubjectTemplate;
            }

            set
            {
                Settings.SubjectTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the email body template text.
        /// </summary>
        /// <value>
        /// The email body template text.
        /// </value>
        public string EditableBodyTemplate
        {
            get
            {
                return Settings.EditableBodyTemplate;
            }

            set
            {
                Settings.EditableBodyTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the email body template text.
        /// </summary>
        /// <value>
        /// The email body template text.
        /// </value>
        public string ReadonlyBodyTemplate
        {
            get
            {
                return Settings.ReadonlyBodyTemplate;
            }

            set
            {
                Settings.ReadonlyBodyTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the database info.
        /// </summary>
        /// <value>
        /// The database info.
        /// </value>
        public DatabaseInfo DatabaseInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the counter operation info.
        /// </summary>
        /// <value>
        /// The counter operation info.
        /// </value>
        public CounterOperationInfo CounterOperationInfo
        {
            get;
            set;
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Loads the settings.
        /// </summary>
        public void LoadSettings()
        {
            _settings = new ConfigSettings<EmailSettings>(_GLOBAL_SETTINGS, false, true);
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        public void SaveSettings()
        {
            UtilityMethods.Assert(_settings != null, "Settings have not been loaded");

            // Save the settings to disk
            _settings.Save();
        }

        /// <summary>
        /// Runs the configuration.
        /// </summary>
        /// <returns>True if the configuration was successfully updated.</returns>
        public bool RunConfiguration()
        {
            using (var dialog = new EmailSettingsDialog(this))
            {
                return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
            }
        }

        /// <summary>
        /// Gets the email subject expanded using <see cref="CounterOperationInfo"/> and
        /// <see cref="DatabaseInfo"/>.
        /// </summary>
        /// <returns>The expanded email subject.</returns>
        public string GetSubject()
        {
            return GetExpandedString(SubjectTemplate);
        }

        /// <summary>
        /// Gets the editable portion of the email body expanded using
        /// <see cref="CounterOperationInfo"/> and <see cref="DatabaseInfo"/>.
        /// </summary>
        /// <returns>The expanded editable portion of the email body.</returns>
        public string GetEditableBody()
        {
            return GetExpandedString(EditableBodyTemplate);
        }

        /// <summary>
        /// Gets the read-only portion of the email body expanded using
        /// <see cref="CounterOperationInfo"/> and <see cref="DatabaseInfo"/>.
        /// </summary>
        /// <returns>The expanded read-only portion of the email body.</returns>
        public string GetReadonlyBody()
        {
            return GetExpandedString(ReadonlyBodyTemplate);
        }
        
        #endregion Public Methods

        #region Private Members

        /// <summary>
        /// Gets the <see cref="EmailSettings"/> instance to which settings should be loaded from
        /// and applied.
        /// </summary>
        EmailSettings Settings
        {
            get
            {
                // If no permanent settings instance has been provided, use a temporary EmailSettings
                // instance.
                if (_settings == null)
                {
                    if (_temporarySettings == null)
                    {
                        _temporarySettings = new EmailSettings();
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

        /// <summary>
        /// Expands the specified <see paramref="value"/> using <see cref="CounterOperationInfo"/>.
        /// </summary>
        /// <param name="value">The template text to be expanded.</param>
        /// <returns>The expanded value.</returns>
        string GetExpandedString(string value)
        {
            value = Replace(value, "<DatabaseID>", DatabaseInfo.DatabaseID.ToString().ToUpperInvariant());
            value = Replace(value, "<DatabaseServer>", DatabaseInfo.DatabaseServer);
            value = Replace(value, "<DatabaseName>", DatabaseInfo.DatabaseName);
            value = Replace(value, "<DatabaseCreationTime>", DatabaseInfo.CreationTime.DateTimeToString());
            value = Replace(value, "<DatabaseRestoreTime>", DatabaseInfo.RestoreTime.DateTimeToString());
            value = Replace(value, "<DatabaseLastCounterUpdateTime>", DatabaseInfo.LastCounterUpdateTime.DateTimeToString());
            value = Replace(value, "<DatabaseDateTimeStamp>", DatabaseInfo.DateTimeStamp.DateTimeToString());
            value = Replace(value, "<Customer>", CounterOperationInfo.Customer);
            value = Replace(value, "<Comment>", CounterOperationInfo.Comment);
            value = Replace(value, "<Description>", CounterOperationInfo.Description);
            value = Replace(value, "<CodeType>", CounterOperationInfo.CodeType);
            value = Replace(value, "<Code>", CounterOperationInfo.Code);

            return value;
        }

        /// <summary>
        /// Replaces the specified <see paramref="tag"/> with the specified
        /// <see cref="replacement"/>.
        /// <see paramref="replacement"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> in which the replacement should take
        /// place.</param>
        /// <param name="tag">The <see langword="string"/> to be replaced in <see paramref="value"/>.
        /// </param>
        /// <param name="replacement">The <see langword="string"/> with which to replace
        /// <see paramref="tag"/></param>
        /// <returns>The <see paramref="value"/> after having performed the replacement.</returns>
        static string Replace(string value, string tag, string replacement)
        {
            if (string.IsNullOrWhiteSpace(replacement))
            {
                string pattern =
                    @"[\r\n]?[\n]?[\x20]*" + Regex.Escape(tag) + @"(?=[\x20]*([\r\n])|($))";

                // If the replacement value is empty and the tag is alone on an otherwise empty line,
                // remove the entire line.
                if (Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                {
                    return Regex.Replace(value, pattern, "",
                        RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }
            }

            return Regex.Replace(value, Regex.Escape(tag), replacement,
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        /// <summary>
        /// Encrypts the specified <see paramref="unencryptedString"/>.
        /// </summary>
        /// <param name="unencryptedString">The string to be encrypted.</param>
        /// <returns>A hex string representing the encrypted bytes.</returns>
        static string Encrypt(string unencryptedString)
        {
            var unencryptedBytes = new ByteArrayManipulator();
            unencryptedBytes.Write(unencryptedString);
            // Data must be passed for encryption in 8 byte blocks.
            var encryptedBytes = NativeMethods.EncryptDecryptBytes(unencryptedBytes.GetBytes(8), true);
            return encryptedBytes.ToHexString();
        }

        /// <summary>
        /// Decrypts the specified <see paramref="encryptedHexString"/>.
        /// </summary>
        /// <param name="encryptedHexString">A hex string representing the encrypted bytes to be
        /// decrypted.</param>
        /// <returns>The decrypted string.</returns>
        static string Decrypt(string encryptedHexString)
        {
            byte[] encyptedBytes = encryptedHexString.HexStringToBytes();
            var decyrptedBytes = new ByteArrayManipulator(
                NativeMethods.EncryptDecryptBytes(encyptedBytes, false));
            return decyrptedBytes.ReadString();
        }

        #endregion Private Members
    }
}

