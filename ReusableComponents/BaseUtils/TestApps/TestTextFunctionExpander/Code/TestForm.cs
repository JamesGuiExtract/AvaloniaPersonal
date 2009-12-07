using System;
using Extract.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TestTextFunctionExpander;

namespace TestTextFuntionExpander
{
    public partial class TestForm : Form
    {
        /// <summary>
        /// The default source doc name value
        /// </summary>
        static readonly string _SOURCE_DOC = @"C:\InputFiles\TestImage01.tif";

        public TestForm()
        {
            InitializeComponent();
            _buttonModify.Enabled = false;
            _textSourceDoc.Text = _SOURCE_DOC;
        }

        private void HandleTestButtonClick(object sender, EventArgs e)
        {
            try
            {
                _listExpansion.Items.Clear();

                // Get a tag manager to use for expanding the tags
                FileActionManagerPathTags tags = new FileActionManagerPathTags(
                    _textSourceDoc.Text, @"C:\Demo_IDShield\FPSFiles");

                // Get all the lines from the values list
                List<string> values = new List<string>(_listValues.Items.Count);
                foreach (object value in _listValues.Items)
                {
                    values.Add(value.ToString());
                }

                // Expand each value from the values list
                List<string> expansion = new List<string>(values.Count);
                foreach (string value in values)
                {
                    expansion.Add(tags.Expand(value));
                }

                _listExpansion.Items.AddRange(expansion.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
       }

        private void HandleValueListDoubleClick(object sender, EventArgs e)
        {
            try
            {
                // Get the index of the selected item
                int index = _listValues.SelectedIndex;
                if (index > -1)
                {
                    // Get the selected item
                    string value = _listValues.Items[index].ToString();

                    // Create an add/modify dialog with the value to modify
                    AddModifyValueForm modify = new AddModifyValueForm(value);
                    if (modify.ShowDialog() == DialogResult.OK)
                    {
                        // Modify the value in the list
                        _listValues.Items[index] = modify.Value;
                    }
                }
                else
                {
                    // There is no item to modify, add a new value
                    AddModifyValueForm add = new AddModifyValueForm();
                    if (add.ShowDialog() == DialogResult.OK)
                    {
                        // Add the value
                        _listValues.Items.Add(add.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleButtonAddClick(object sender, EventArgs e)
        {
            try
            {
                // Show the add value form
                AddModifyValueForm add = new AddModifyValueForm();
                if (add.ShowDialog() == DialogResult.OK)
                {
                    // Add the new value
                    _listValues.Items.Add(add.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleModifyButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get the index of the selected item
                int index = _listValues.SelectedIndex;
                if (index > -1)
                {
                    // Get the selected item
                    string value = _listValues.Items[index].ToString();

                    // Create a modify form with the value to modify
                    AddModifyValueForm modify = new AddModifyValueForm(value);
                    if (modify.ShowDialog() == DialogResult.OK)
                    {
                        // Modify the value
                        _listValues.Items[index] = modify.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleSelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable the modify button if an item is selected
            _buttonModify.Enabled = _listValues.SelectedIndex != -1;
        }

        private void HandleButtonClearClick(object sender, EventArgs e)
        {
            try
            {
                // Clear the list
                _listValues.Items.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}