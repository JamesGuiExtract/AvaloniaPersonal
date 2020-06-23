using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace Extract.ReportingDevExpress
{
    /// <summary>
    /// The about box for the Report Viewer application.
    /// </summary>
    partial class AboutReportViewer : Form
    {
        /// <summary>
        /// Initializes a new <see cref="AboutReportViewer"/> class.
        /// </summary>
        /// <param name="icon">The icon to display in the about box.</param>
        public AboutReportViewer(Icon icon)
        {
            try
            {
                InitializeComponent();

                //  Initialize the AboutBox to display the product information from the assembly information.
                //  Change assembly information settings for your application through either:
                //  - Project->Properties->Application->Assembly Information
                //  - AssemblyInfo.cs
                this.Text = String.Format(CultureInfo.CurrentCulture, "About {0}", AssemblyTitle);
                this.labelProductName.Text = AssemblyProduct;
                this.labelVersion.Text = String.Format(CultureInfo.CurrentCulture, "Version {0}",
                    AssemblyVersion);
                this.labelCopyright.Text = AssemblyCopyright;
                this.labelCompanyName.Text = AssemblyCompany;

                // Set the icon in the image box
                _pictureIcon.Image = icon.ToBitmap();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23775", ex);
            }
        }

        #region Assembly Attribute Accessors

        /// <summary>
        /// Gets the title of the assembly.
        /// </summary>
        /// <returns>The title of the assembly.</returns>
        public static string AssemblyTitle
        {
            get
            {
                try
                {
                    // Get all Title attributes on this assembly
                    object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                    // If there is at least one Title attribute
                    if (attributes.Length > 0)
                    {
                        // Select the first one
                        AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                        // If it is not an empty string, return it
                        if (!string.IsNullOrEmpty(titleAttribute.Title))
                            return titleAttribute.Title;
                    }
                    // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
                    return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23776", ex);
                }
            }
        }

        /// <summary>
        /// Gets the version of the assembly.
        /// </summary>
        /// <returns>The version of the assembly.</returns>
        public static string AssemblyVersion
        {
            get
            {
                try
                {
                    return Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23777", ex);
                }
            }
        }

        /// <summary>
        /// Gets the product from the assembly.
        /// </summary>
        /// <returns>The product from the assembly.</returns>
        public static string AssemblyProduct
        {
            get
            {
                try
                {
                    // Get all Product attributes on this assembly
                    object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                    // If there aren't any Product attributes, return an empty string
                    if (attributes.Length == 0)
                        return "";
                    // If there is a Product attribute, return its value
                    return ((AssemblyProductAttribute)attributes[0]).Product;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23779", ex);
                }
            }
        }

        /// <summary>
        /// Gets the copyright information from the assembly.
        /// </summary>
        /// <returns>The copyright information from the assembly</returns>
        public static string AssemblyCopyright
        {
            get
            {
                try
                {
                    // Get all Copyright attributes on this assembly
                    object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                    // If there aren't any Copyright attributes, return an empty string
                    if (attributes.Length == 0)
                        return "";
                    // If there is a Copyright attribute, return its value
                    return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23780", ex);
                }
            }
        }

        /// <summary>
        /// Gets the company name from the assembly.
        /// </summary>
        /// <returns>The company name from the assembly.</returns>
        public static string AssemblyCompany
        {
            get
            {
                try
                {
                    // Get all Company attributes on this assembly
                    object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                    // If there aren't any Company attributes, return an empty string
                    if (attributes.Length == 0)
                        return "";
                    // If there is a Company attribute, return its value
                    return ((AssemblyCompanyAttribute)attributes[0]).Company;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23781", ex);
                }
            }
        }

        #endregion
    }
}
