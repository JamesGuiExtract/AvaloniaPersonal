using Extract.DataEntry.Utilities.DataEntryApplication.Properties;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    partial class AboutForm : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(AboutForm).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The currently executing assembly.
        /// </summary>
        private Assembly thisAssembly = Assembly.GetExecutingAssembly();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AboutForm"/> class.
        /// </summary>
        /// <param name="applicationConfig">The product information.</param>
        /// <param name="dataEntryControlHost">The <see cref="DataEntryControlHost"/> to be
        /// used as the source of the product version.</param>
        public AboutForm(ConfigSettings<Settings> applicationConfig, 
            DataEntryControlHost dataEntryControlHost)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI26960", _OBJECT_NAME);

                ExtractException.Assert("ELI30538", "Null argument exception!",
                    applicationConfig != null);
                ExtractException.Assert("ELI27009", "Null argument exception!",
                    dataEntryControlHost != null);

                InitializeComponent();

                // Initialize product info from the applicationConfig passed in.
                if (!string.IsNullOrEmpty(applicationConfig.Settings.AboutLogo))
                {
                    string logoFilename =
                        DataEntryMethods.ResolvePath(applicationConfig.Settings.AboutLogo);
                    _logoImage.Image = Bitmap.FromFile(logoFilename);
                }
                _labelProductName.Text = applicationConfig.Settings.ApplicationTitle;
                _textBoxDescription.Text = applicationConfig.Settings.ApplicationDescription;
                Text = String.Format(CultureInfo.CurrentCulture, "About {0}",
                    applicationConfig.Settings.ApplicationTitle);

                // Extract the product version from the dataEntryControlHost's assembly.
                Assembly controlHostAssembly = Assembly.GetAssembly(dataEntryControlHost.GetType());
                _labelProductVersion.Text = String.Format(CultureInfo.CurrentCulture,
                    "Version {0}", controlHostAssembly.GetName().Version.ToString());

                //  Initialize framework version and company info from the assembly information.
                //  Change assembly information settings for your application through either:
                //  - Project->Properties->Application->Assembly Information
                //  - AssemblyInfo.cs
                _labelFrameworkVersion.Text = String.Format(CultureInfo.CurrentCulture,
                    "Framework version {0}", AssemblyVersion);
                _labelCopyright.Text = AssemblyCopyright;
                _labelCompanyName.Text = AssemblyCompany;
                _linkLabelWebsite.Text = AssemblyCompanyUrl;

                if (LicenseUtilities.IsTemporaryLicense(LicenseIdName.DataEntryCoreComponents))
                {
                    // Get the expiration date
                    DateTime date =
                        LicenseUtilities.GetExpirationDate(LicenseIdName.DataEntryCoreComponents);

                    // Build the expiration date text for the label
                    StringBuilder sb = new StringBuilder("License expires on ");
                    sb.Append(date.ToLongDateString());
                    sb.Append(" (");
                    sb.Append(((TimeSpan)(date - DateTime.Now)).Days.ToString(CultureInfo.CurrentCulture));
                    sb.Append(" days remaining).");
                    _labelLicenseInformation.Text = sb.ToString();
                }
                else
                {
                    // Product is permanently licensed
                    _labelLicenseInformation.Text = "Permanently licensed";
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26961", ex);
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="LinkLabel.LinkClicked"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The <see cref="LinkLabelLinkClickedEventArgs"/>
        /// data associated with the event.</param>
        private void HandleLinkLabelWebsiteClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(_linkLabelWebsite.Text);

            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26962", ex);
            }
        }

        #endregion Event Handlers

        #region Assembly Attribute Accessors

        /// <summary>
        /// Gets the version of the currently executing assembly.
        /// </summary>
        /// <returns>The version of the currently executing assembly.</returns>
        public string AssemblyVersion
        {
            get
            {
                return thisAssembly.GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Gets the copyright of the currently excuting assembly.
        /// </summary>
        /// <returns>The copyright of the currently excuting assembly.</returns>
        public static string AssemblyCopyright
        {
            get
            {
                // Get all Copyright attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(
                    typeof(AssemblyCopyrightAttribute), false);
                // If there aren't any Copyright attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Copyright attribute, return its value
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        /// <summary>
        /// Gets the company of the currently excuting assembly.
        /// </summary>
        /// <returns>The company of the currently excuting assembly.</returns>
        public string AssemblyCompany
        {
            get
            {
                // Get all Company attributes on this assembly
                object[] attributes = thisAssembly.GetCustomAttributes(
                    typeof(AssemblyCompanyAttribute), false);
                // If there aren't any Company attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Company attribute, return its value
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        /// <summary>
        /// Gets the company url of the currently executing assembly.
        /// </summary>
        /// <returns>The company url of the currently executing assembly.</returns>
        public string AssemblyCompanyUrl
        {
            get
            {
                object[] attributes = thisAssembly.GetCustomAttributes(
                    typeof(Extract.AssemblyCompanyUrlAttribute), false);
                // If there aren't any CompanyUrl attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a CompanyUrl attribute, return its value
                return ((Extract.AssemblyCompanyUrlAttribute)attributes[0]).CompanyUrl;
            }
        }

        #endregion Assembly Attribute Accessors
    }
}
