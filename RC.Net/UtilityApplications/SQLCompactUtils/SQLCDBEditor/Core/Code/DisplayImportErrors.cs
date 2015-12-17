using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// Display the state of data insert rows, including errors, so user can review and
    /// decide whether to save valid data (despite errors preventing saving some of the data)
    /// or to cancel and not save any data.
    /// </summary>
    public partial class DisplayImportErrors : Form
    {
        /// <summary>
        /// Display data insert rows, at least one will have an error. 
        /// Note that the continue button returns DialogResult.OK, the Cancel button
        /// return DialogResult.Cancel.
        /// </summary>
        /// <param name="messages"></param>
        public DisplayImportErrors(string[] messages)
        {
            InitializeComponent();

            List<string> ls = new List<string>();
            int rowsSucceeded = 0;
            foreach (var message in messages)
            {
                if (message.StartsWith("*", StringComparison.CurrentCultureIgnoreCase))
                {
                    ls.Add(message);
                }
                else
                {
                    ++rowsSucceeded;
                }
            }

            ls.Add("\r\n");

            int rowsFailed = messages.Length - rowsSucceeded;
            var result = String.Format(CultureInfo.CurrentCulture,
                                       "On Continue, {0} rows will succeed, and {1} rows will fail.",
                                       rowsSucceeded,
                                       rowsFailed);

            resultTextBox.Text = result;

            ImportErrorsTextBox.Lines = ls.ToArray();
            ImportErrorsTextBox.SelectionLength = 0;
        }
    }
}
