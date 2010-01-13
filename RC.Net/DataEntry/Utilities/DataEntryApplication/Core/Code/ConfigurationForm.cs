using Extract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// A <see cref="Form"/> used to configure settings for <see cref="ComClass"/>.
    /// </summary>
    public partial class ConfigurationForm : Form
    {
        /// <summary>
        /// The <see cref="ComClass"/> instance for which configuration is being performed.
        /// </summary>
        ComClass _comClass;

        /// <summary>
        /// Initializes a new <see cref="ConfigurationForm"/> instance for the specified
        /// <see cref="ComClass"/> instance.
        /// </summary>
        /// <param name="comClass">The <see cref="ComClass"/> instance which is to be configured.
        /// </param>
        public ConfigurationForm(ComClass comClass)
        {
            try
            {
                ExtractException.Assert("ELI25475", "Null argument exception!", comClass != null);

                InitializeComponent();

                _comClass = comClass;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25492", ex);
            }
        }

        /// <summary>
        /// Displays the form to allow the user to specify settings.
        /// </summary>
        /// <returns><see langword="true"/> if the user applied the settings or
        /// <see langworld="false"/> if configuration was cancelled.</returns>
        public bool Configure()
        {
            try
            {
                // Initialize the form's controls
                _configFileNameTextBox.Text = _comClass.ConfigFileName;
                _enableInputTrackingCheckBox.Checked = _comClass.InputEventTrackingEnabled;
                _enableCountersCheckBox.Checked = _comClass.CountersEnabled;

                // Display the form modally and wait for the result
                if (ShowDialog() == DialogResult.OK)
                {
                    // The user is applying settings; apply the current form values to the _comClass.
                    _comClass.ConfigFileName = _configFileNameTextBox.Text;
                    _comClass.InputEventTrackingEnabled = _enableInputTrackingCheckBox.Checked;
                    _comClass.CountersEnabled = _enableCountersCheckBox.Checked;

                    return true;
                }
                else
                {
                    // Configuration was cancelled.  Return without applying the changes.
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25468", ex);
            }
        }

        /// <summary>
        /// Handles the case that the user clicked the browse button for the configuration file.
        /// An open file dialog is created and displayed to allow the user to browse to the
        /// configuration file of their choosing.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleFileBrowseButtonClick(object sender, EventArgs e)
        {
            try
            {
                // The OpenFileDialog cless will not work in a multithreaded apartment. Initialize a
                // new thread in single thread apartment mode for the open file dialog.
                Thread browseForFileThread = new Thread(new ParameterizedThreadStart(BrowseForFile));
                browseForFileThread.SetApartmentState(ApartmentState.STA);

                // Run the thread to allow selection of the parameter for _configFileNameTextBox.
                browseForFileThread.Start(_configFileNameTextBox);

                // Allow the parent's UI to be updated while the open file dialog is open.
                while (browseForFileThread.IsAlive)
                {
                    Application.DoEvents();

                    // Prevent the loop from using to much CPU.
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25477", ex);
            }
        }

        /// <summary>
        /// Displays an <see cref="OpenFileDialog"/> to allow the user to select a filename to
        /// populate the provided <see cref="TextBox"/>
        /// </summary>
        /// <param name="fileNameTextBoxObject">A <see cref="TextBox"/> whose value should be
        /// updated via the <see cref="OpenFileDialog"/>.</param>
        void BrowseForFile(Object fileNameTextBoxObject)
        {
            try
            {
                TextBox fileNameTextBox = fileNameTextBoxObject as TextBox;
                ExtractException.Assert("ELI25484", "Null argument exception!", 
                    fileNameTextBox != null);

                // Create and initialized the OpenFileDialog
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = fileNameTextBox.Text;
                openFileDialog.Filter = "Configuration files (*.config)|*.config|All files (*.*)|*.*";

                // Display the dialog and check the result
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // If the user pressed OK on the file open dialog, apply the result.
                    UpdateTextBoxValue(fileNameTextBox, openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25483", ex);
            }
        }

        /// <summary>
        /// Updates a <see cref="TextBox"/> value from any thread.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to updated.</param>
        /// <param name="value">The value to apply to the <see cref="TextBox"/>.</param>
        void UpdateTextBoxValue(TextBox textBox, string value)
        {
            // If not running in the UI thread than an invoke is required
            if (base.InvokeRequired)
            {
                // Invoke this call in the UI thread.
                base.Invoke(new UpdateTextBoxParameterDelegate(UpdateTextBoxValue), 
                    new object[] { textBox, value });

                // Just return as the Invoke will take care of updating the UI
                return;
            }

            // If in the UI thread, apply the specified value.
            textBox.Text = value;
        }

        #region Delegates

        /// <summary>
        /// Delegate for <see cref="UpdateTextBoxValue"/>.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to updated.</param>
        /// <param name="value">The value to apply to the <see cref="TextBox"/>.</param>
        delegate void UpdateTextBoxParameterDelegate(TextBox textBox, string value);

        #endregion Delegates
    }
}
