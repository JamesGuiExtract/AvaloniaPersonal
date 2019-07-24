using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    public partial class SelectFromListInputBox : Form
    {

        #region Properties

        /// <summary>
        /// Caption for the title bar of the dialog box
        /// </summary>
        public string Title
        {
            get
            {
                return Text;
            }

            set
            {
                Text = value;
            }
        }

        /// <summary>
        /// Prompt text for the data being selected
        /// </summary>
        public string Prompt
        {
            get
            {
                return label.Text;
            }

            set
            {
                label.Text = value;
            }
        }

        /// <summary>
        /// A list of strings to be used for the ComboBox to be selected
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<string> SelectionStrings
        {
            get
            {
                return comboBox.DataSource as IList<string>;
            }

            set
            {
                comboBox.DataSource = value;
            }
        }

        /// <summary>
        /// The selected value from the ComboBox
        /// </summary>
        public string ReturnValue { get; private set; }

        /// <summary>
        /// The default value to be selected in the ComboBox initially
        /// </summary>
        public string DefaultResponse { get; set; }

        /// <summary>
        /// <see cref="Point"/> representing the top left coordinate of where the
        /// input box should be displayed.
        /// </summary>
        public Point StartLocation { get; set; }

        #endregion

        #region Constructors

        public SelectFromListInputBox()
        {
            try
            {
                InitializeComponent();
                ReturnValue = string.Empty;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI47008", ex);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called during <see cref="Form.Load"/> to set the startup position
        /// (if it has been specified)
        /// </summary>
        /// <param name="e">Data associated with event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                if (!StartLocation.IsEmpty)
                {
                    Top = StartLocation.Y;
                    Left = StartLocation.X;
                }
                comboBox.SelectedItem = DefaultResponse;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47009");
            }
        }

        void HandleOKClick(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.OK;
                ReturnValue = comboBox.SelectedValue as string;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47010");
            }
        }

        #endregion
    }
}
