using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace IDShieldOffice
{
    partial class AboutIDShieldOfficeForm : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(AboutIDShieldOfficeForm).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The currently executing assembly.
        /// </summary>
        private Assembly thisAssembly = Assembly.GetExecutingAssembly();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AboutIDShieldOfficeForm"/> class.
        /// </summary>
        public AboutIDShieldOfficeForm()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23180",
                    _OBJECT_NAME);

                InitializeComponent();

                //  Initialize the AboutBox to display the product information from the assembly information.
                //  Change assembly information settings for your application through either:
                //  - Project->Properties->Application->Assembly Information
                //  - AssemblyInfo.cs
                this.Text = String.Format(CultureInfo.CurrentCulture, "About {0}", AssemblyProduct);
                this._labelProductName.Text = AssemblyProduct;
                this._labelVersion.Text = String.Format(CultureInfo.CurrentCulture, "Version {0}",
                    AssemblyVersion);
                this._labelCopyright.Text = AssemblyCopyright;
                this._labelCompanyName.Text = AssemblyCompany;
                this._textBoxDescription.Text = AssemblyDescription;
                this._linkLabelWebsite.Text = AssemblyCompanyUrl;

                if (LicenseUtilities.IsTemporaryLicense(LicenseIdName.IdShieldOfficeObject))
                {
                    // Get the expiration date
                    DateTime date =
                        LicenseUtilities.GetExpirationDate(LicenseIdName.IdShieldOfficeObject);

                    // Build the expiration date text for the label
                    StringBuilder sb = new StringBuilder("License expires on ");
                    sb.Append(date.ToLongDateString());
                    sb.Append(" (");
                    sb.Append(((TimeSpan)(date - DateTime.Now)).Days.ToString(CultureInfo.CurrentCulture));
                    sb.Append(" days remaining).");
                    this._labelLicenseInformation.Text = sb.ToString();
                }
                else
                {
                    // Product is permanently licensed
                    this._labelLicenseInformation.Text = "Permanently licensed";
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23074", ex);
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
                ExtractException.Display("ELI23077", ex);
            }
        }

        #endregion Event Handlers

        #region Assembly Attribute Accessors

        /// <summary>
        /// Gets the title of the currently executing assembly.
        /// </summary>
        /// <returns>The title of the currently executing assembly.</returns>
        public string AssemblyTitle
        {
            get
            {
                // Get all Title attributes on this assembly
                object[] attributes = thisAssembly.GetCustomAttributes(
                    typeof(AssemblyTitleAttribute), false);
                // If there is at least one Title attribute
                if (attributes.Length > 0)
                {
                    // Select the first one
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    // If it is not an empty string, return it
                    if (!string.IsNullOrEmpty(titleAttribute.Title))
                        return titleAttribute.Title;
                }
                // If there was no Title attribute, or if the Title attribute was the empty string,
                // return the .exe name
                return System.IO.Path.GetFileNameWithoutExtension(thisAssembly.CodeBase);
            }
        }

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
        /// Gets the description of the currently excuting assembly.
        /// </summary>
        /// <returns>The description of the currently excuting assembly.</returns>
        public string AssemblyDescription
        {
            get
            {
                // Get all Description attributes on this assembly
                object[] attributes = thisAssembly.GetCustomAttributes(
                    typeof(AssemblyDescriptionAttribute), false);
                // If there aren't any Description attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Description attribute, return its value
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        /// <summary>
        /// Gets the product of the currently excuting assembly.
        /// </summary>
        /// <returns>The product of the currently excuting assembly.</returns>
        public string AssemblyProduct
        {
            get
            {
                // Get all Product attributes on this assembly
                object[] attributes = thisAssembly.GetCustomAttributes(
                    typeof(AssemblyProductAttribute), false);
                // If there aren't any Product attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Product attribute, return its value
                return ((AssemblyProductAttribute)attributes[0]).Product;
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
