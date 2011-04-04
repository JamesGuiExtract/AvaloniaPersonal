using Extract.Encryption;
using Extract.Licensing;
using System;
using System.IO;
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
        /// Saves the settings to the current level as specified by <see cref="UserSettings"/>.
        /// </summary>
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
        /// Gets or sets whether the current settings are user level settings or global settings.
        /// If <see langword="true"/> then the settings are user level settings.
        /// </summary>
        bool UserSettings { get; set; }

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

        #endregion Constants

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
                var settings = new ConfigSettings<ExtractSmtp>(
                    userSpecific ? _USER_SETTINGS : _GLOBAL_SETTINGS, false, true).Settings;

                // Load settings
                Server = settings.Server ?? string.Empty;
                Port = settings.Port > 0 ? settings.Port : _DEFAULT_PORT;
                SenderName = settings.SenderName ?? string.Empty;
                SenderAddress = settings.SenderAddress ?? string.Empty;
                EmailSignature = settings.EmailSignature ?? string.Empty;
                Timeout = settings.Timeout;
                UseSsl = settings.UseSsl;
                UserSettings = userSpecific;

                // Load encrypted settings
                var label = new MapLabel();
                if (!string.IsNullOrWhiteSpace(settings.UserName))
                {
                    UserName = settings.UserName.ExtractDecrypt(label);
                }
                if (!string.IsNullOrWhiteSpace(settings.Password))
                {
                    Password = settings.Password.ExtractDecrypt(label);
                }
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
                var config = new ConfigSettings<ExtractSmtp>(
                    UserSettings ? _USER_SETTINGS : _GLOBAL_SETTINGS, false, true);
                var settings = config.Settings;

                // Save settings
                settings.Server = Server;
                settings.Port = Port;
                settings.SenderName = SenderName;
                settings.SenderAddress = SenderAddress;
                settings.EmailSignature = EmailSignature;
                settings.Timeout = Timeout;
                settings.UseSsl = UseSsl;

                // Save encrypted settings
                var label = new MapLabel();
                settings.UserName = UserName.ExtractEncrypt(label);
                settings.Password = Password.ExtractEncrypt(label);

                // Save the settings to disk
                config.Save();
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
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the port for the SMTP server.
        /// </summary>
        /// <value>
        /// The port for the SMTP server.
        /// </value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the login user name.
        /// </summary>
        /// <value>
        /// The login user name.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the login password.
        /// </summary>
        /// <value>
        /// The login password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the sender name.
        /// </summary>
        /// <value>
        /// The name of the sender.
        /// </value>
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets the sender address.
        /// </summary>
        /// <value>
        /// The sender address.
        /// </value>
        public string SenderAddress { get; set; }

        /// <summary>
        /// Gets or sets the email signature.
        /// </summary>
        /// <value>
        /// The email signature.
        /// </value>
        public string EmailSignature { get; set; }

        /// <summary>
        /// Gets or sets the timeout in milliseconds when connecting to the server.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email should be sent using SSL.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> then the email will be sent using SSL.
        /// <para><b>Note:</b></para>
        /// This requires that the SMTP server supports SSL communication.
        /// </value>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Gets or sets whether the current settings are user level settings or global settings.
        /// If <see langword="true"/> then the settings are user level settings.
        /// </summary>
        public bool UserSettings { get; set; }

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
    }
}
