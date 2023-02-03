using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Extract.ErrorHandling
{
    partial class ErrorDisplay : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(ErrorDisplay).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The currently executing assembly.
        /// </summary>
        private Assembly thisAssembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// The current Exception represented as an HTML string
        /// </summary>
        private string HTMLException = "";

        /// <summary>
        /// Stores the temporary file name where the Error Details view lives
        /// </summary>
        private string TempDetailsFile = "";

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ErrorDisplay"/> class.
        /// </summary>
        /// <param name="brandingResources">The product information.</param>
        public ErrorDisplay(ExtractException extractException)
        {
            try
            {
                InitializeComponent();
                _labelErrorMessage.Text = extractException.Message;
                HTMLException = extractException.ToHtml();
            }
            catch (Exception ex)
            {
                ex.AsExtractException("ELI53636").Display();
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
        private void HandleDetailsClick(object sender, System.EventArgs e)
        {
            try
            {
                string tmpDetailsFilePath = Path.GetTempFileName();
                TempDetailsFile = tmpDetailsFilePath.Replace(".tmp", ".html");
                File.Move(tmpDetailsFilePath, TempDetailsFile);
                File.WriteAllText(TempDetailsFile, HTMLException);
                var proc = new Process();
                proc.StartInfo = new ProcessStartInfo(TempDetailsFile)
                {
                    UseShellExecute = true
                };
                proc.Start();
            }
            catch (Exception ex)
            {
                ex.AsExtractException("ELI53637").Display();
            }
        }


        /// <summary>
        /// Dispose of temporary file on form close
        /// </summary>
        /// <param name="sender">Sending Object</param>
        /// <param name="e">Form clode parameters</param>
        private void ErrorDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TempDetailsFile != "" && File.Exists(TempDetailsFile))
            {
                File.Delete(TempDetailsFile);
            }
        }

        #endregion Event Handlers

        #region Assembly Attribute Accessors

        #endregion Assembly Attribute Accessors
    }
}
