using System;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// A simple form to display the results of the export operation.
    /// </summary>
    public partial class ExportResultsForm : Form
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="results"></param>
        public ExportResultsForm(string[] results)
        {
            InitializeComponent();

            int countOfErrors = 0;
            int textStartIndex = 0;
            foreach (var result in results)
            {
                bool inError = result.StartsWith("*", StringComparison.OrdinalIgnoreCase);
                if (inError)
                {
                    ++countOfErrors;
                }

                ResultsRichTextBox.Select(textStartIndex, result.Length);
                ResultsRichTextBox.SelectionColor = inError ? Color.Red : Color.Black;
                textStartIndex += result.Length;
                
                ResultsRichTextBox.AppendText(result);
            }

            if (countOfErrors > 0)
            {
                ResultsRichTextBox.AppendText("\r\n");

                string msg = "* The relevant table is empty";

                ResultsRichTextBox.Select(textStartIndex, msg.Length);
                ResultsRichTextBox.SelectionColor = Color.Red;

                ResultsRichTextBox.AppendText(msg);
            }
        }
    }
}
