using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.DataEntryPrompt
{
    /// <summary>
    /// The <see cref="Form"/> to accept user input.
    /// <para><b>Note:</b></para>
    /// This application is intentionally built without any dependencies on other extract assemblies.
    /// This means it is not licensed and does not use ExtractExceptions.
    /// </summary>
    public partial class DataEntryPromptForm : Form
    {
        #region Constants

        /// <summary>
        /// The full path to the extract systems common application data folder
        /// </summary>
        static readonly string _COMMON_APPLICATION_DATA_PATH = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Extract Systems");

        #endregion Constants

        #region Fields

        /// <summary>
        /// 
        /// </summary>
        ConfigSettings<Extract.DataEntryPrompt.Properties.Settings> _config =
            new ConfigSettings<Extract.DataEntryPrompt.Properties.Settings>(null, false, true);

        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, TextBox> _dataFields = new Dictionary<string, TextBox>();

        /// <summary>
        /// 
        /// </summary>
        Dictionary<TextBox, Regex> _validationRegexes = new Dictionary<TextBox, Regex>();

        /// <summary>
        /// The error provider used to indicate that password fields do not match.
        /// </summary>
        ErrorProvider _errorProvider = new ErrorProvider();

        /// <summary>
        /// 
        /// </summary>
        int _recordId = 1;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryPromptForm"/> class.
        /// </summary>
        public DataEntryPromptForm()
        {
            InitializeComponent();

            _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Text = _config.Settings.Title;
            _okButton.Text = _config.Settings.OKButtonLabel;
            _cancelButton.Text = _config.Settings.CancelButtonLabel;

            int controlIndex = 0;
            for (int i = 1; i <= 10; i++)
            {
                string propertyName = "Field" + i.ToString(CultureInfo.InvariantCulture);
                string validationPropertyName = propertyName + "Validation";

                string fieldName = (string)_config.Settings[propertyName];
                    
                if (!string.IsNullOrEmpty(fieldName))
                {
                    Label label = new Label();
                    label.AutoSize = true;
                    label.Anchor = AnchorStyles.Left;
                    label.Text = fieldName;
                    _tableLayoutPanel.Controls.Add(label, 0, controlIndex++);
                    _tableLayoutPanel.SetColumnSpan(label, 2);

                    TextBox textBox = new TextBox();
                    textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                    textBox.Margin = new Padding(textBox.Margin.Left,
                        textBox.Margin.Top,
                        textBox.Margin.Right + 16,
                        textBox.Margin.Bottom);
                    _tableLayoutPanel.Controls.Add(textBox, 0, controlIndex++);
                    _tableLayoutPanel.SetColumnSpan(textBox, 2);

                    _dataFields[fieldName] = textBox;

                    string validationString = (string)_config.Settings[validationPropertyName];
                    if (!string.IsNullOrEmpty(validationString))
                    {
                        _validationRegexes[textBox] = new Regex(validationString);
                        textBox.TextChanged += HandleTextBox_TextChanged;
                    }
                }
            }

            Trace.Assert(_dataFields.Count > 0, "No fields have been defined in config file.");

            ClearData(false);

            string dataFileName = GetDataFileName();

            var dataFileLines = File.ReadLines(dataFileName)
                .Where(line => !string.IsNullOrEmpty(line))
                .Skip(1)
                .ToArray();

            if (dataFileLines.Length > 0)
            {
                _recordId = dataFileLines
                    .Select(line => line.Split(',')[0])
                    .Select(id => Int32.Parse(id, CultureInfo.InvariantCulture))
                    .Max();
                _recordId++;
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_okButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (TextBox textBox in _dataFields.Values)
                {
                    if (!string.IsNullOrEmpty(_errorProvider.GetError(textBox)))
                    {
                        MessageBox.Show("Please make sure all data is entered correctly.",
                            "Invalid data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                Trace.Assert(!string.IsNullOrEmpty(_config.Settings.DataFileName), 
                    "Data file name not specified.");

                string dataFileName = GetDataFileName();

                string data = string.Join(",",
                    new[] { RecordId }.Concat(_dataFields.Values
                        .Select(box => Quote(box.Text))));
                File.AppendAllLines(dataFileName, new[] { data });
                
                if (!string.IsNullOrEmpty(_config.Settings.PostEntryCommandLine))
                {
                    using (var process = new Process())
                    {
                        process.StartInfo = new ProcessStartInfo(_config.Settings.PostEntryCommandLine,
                            GetEvaluatedArguments());
                        process.Start();
                    }
                }

                if (_config.Settings.KeepOpen)
                {
                    ClearData(true);
                }
                else
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_cancelButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCancelButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_config.Settings.KeepOpen)
                {
                    ClearData(false);
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event of a <see cref="TextBox"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                if (_validationRegexes[textBox].IsMatch(textBox.Text))
                {
                    _errorProvider.SetError(textBox, "");
                }
                else
                {
                    _errorProvider.SetError(textBox, "Invalid entry.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets the current record id as a <see langword="string"/>.
        /// </summary>
        string RecordId
        {
            get
            {
                return _recordId.ToString("00000", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Clears the data.
        /// </summary>
        /// <param name="IncrementId"></param>
        void ClearData(bool IncrementId)
        {
            foreach (TextBox textBox in _dataFields.Values)
            {
                // Ensure the text changes so that it is evaluated.
                textBox.Text = " ";
                textBox.Text = "";
            }

            _dataFields.First().Value.Focus();

            if (IncrementId)
            {
                _recordId++;
            }
        }

        /// <summary>
        /// Gets the name of the data file.
        /// </summary>
        /// <returns></returns>
        string GetDataFileName()
        {
            string dataFileName = _config.Settings.DataFileName;

            // Ensure that fileName is not null or empty
            Trace.Assert(!string.IsNullOrEmpty(dataFileName),
                "File name must not be null or empty string.");

            // Check if the filename is a relative path
            if (!Path.IsPathRooted(dataFileName))
            {
                dataFileName = Path.Combine(_COMMON_APPLICATION_DATA_PATH, dataFileName);
                dataFileName = Path.GetFullPath(dataFileName);
            }

            string directoryName = Path.GetDirectoryName(dataFileName);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (!File.Exists(dataFileName))
            {
                string columnHeaders = string.Join(",",
                    new[] { "ID" }.Concat(_dataFields.Keys)
                        .Select(name => Quote(name)));
                File.WriteAllLines(dataFileName, new[] { columnHeaders });
            }

            return dataFileName;
        }

        /// <summary>
        /// Gets the evaluated arguments.
        /// </summary>
        /// <returns></returns>
        string GetEvaluatedArguments()
        {
            string arguments = _config.Settings.PostEntryCommandLineArguments.Replace(
                "%RecordID%", RecordId);
            foreach (string fieldName in _dataFields.Keys)
            {
                string wrappedName = "%" + fieldName + "%";
                if (arguments.IndexOf(wrappedName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    arguments = arguments.Replace(wrappedName, _dataFields[fieldName].Text);
                }
            }

            return arguments;
        }

        /// <summary>
        /// Returns a quoted version of the supplied string.
        /// <example>If the input value is 'Hello World' then the result
        /// will be '"Hello World"'.</example>
        /// </summary>
        /// <param name="value">The <see cref="String"/> to quote.</param>
        /// <returns>A quoted version of the input string.</returns>
        public static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty) + "\"";
        }

        #endregion Private Members
    }
}
