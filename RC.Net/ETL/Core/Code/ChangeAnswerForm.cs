using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Windows.Forms;

namespace Extract.ETL
{
    public partial class ChangeAnswerForm : Form
    {
        MachineLearningService _service;

        public ChangeAnswerForm(MachineLearningService service)
        {
            _service = service;

            InitializeComponent();
        }

        private void HandleChangeButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    string oldAnswer = _oldAnswerTextBox.Text;
                    string newAnswer = _newAnswerTextBox.Text;
                    bool changedAnything = _service.ChangeAnswer(oldAnswer, newAnswer, false);

                    if (!changedAnything)
                    {
                        DialogResult = DialogResult.None;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45850");
                DialogResult = DialogResult.None;
            }
        }
    }
}
