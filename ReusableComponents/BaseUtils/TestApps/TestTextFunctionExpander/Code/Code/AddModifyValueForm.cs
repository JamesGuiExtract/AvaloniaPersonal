using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Extract.Utilities.Forms;

namespace TestTextFunctionExpander
{
    public partial class AddModifyValueForm : Form
    {
        string _value;

        public AddModifyValueForm()
            : this("")
        {
        }

        public AddModifyValueForm(string value)
        {
            InitializeComponent();
            _value = value ?? "";

            _textValue.Text = _value;
        }

        private void HandleValueTextChanged(object sender, EventArgs e)
        {
            try
            {
                _value = _textValue.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
        }

        private void HandlePathTagSelected(object sender, TagSelectedEventArgs e)
        {
            try
            {
                _textValue.SelectedText = e.Tag;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}