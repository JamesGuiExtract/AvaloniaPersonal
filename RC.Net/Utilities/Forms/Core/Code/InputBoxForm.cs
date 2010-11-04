// InputBox - Based on implementation by: Andrew Ma
// URL: http://www.devhood.com/Tools/tool_details.aspx?tool_id=295

using Extract.Licensing;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Summary description for InputBoxForm.
    /// </summary>
    internal class InputBoxForm : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME =
           typeof(InputBoxForm).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Okay <see cref="Button"/>.
        /// </summary>
        Button ok;

        /// <summary>
        /// Cancel <see cref="Button"/>.
        /// </summary>
        Button cancel;

        /// <summary>
        /// <see cref="Label"/> for the input box.
        /// </summary>
        Label label;

        /// <summary>
        /// <see cref="TextBox"/> containing the return value.
        /// </summary>
        TextBox result;

        /// <summary>
        /// Field holding the return result of the input box.
        /// </summary>
        string returnValue;

        /// <summary>
        /// <see cref="Point"/> representing the top left coordinate of where the
        /// input box should be displayed.
        /// </summary>
        Point startLocation;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        System.ComponentModel.Container components = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="InputBoxForm"/> class.
        /// </summary>
        public InputBoxForm()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23145",
                    _OBJECT_NAME);

                // Required for Windows Form Designer support
                InitializeComponent();
                returnValue = "";
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23146", ex);
            }
        }

        #endregion Constructors

        #region IDisposable

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of method with the code editor.
        /// </summary>
        void InitializeComponent()
        {
            ok = new System.Windows.Forms.Button();
            result = new System.Windows.Forms.TextBox();
            label = new System.Windows.Forms.Label();
            cancel = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // ok
            // 
            ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            ok.Location = new System.Drawing.Point(216, 56);
            ok.Name = "ok";
            ok.Size = new System.Drawing.Size(64, 24);
            ok.TabIndex = 1;
            ok.Text = "OK";
            ok.Click += new System.EventHandler(ok_Click);
            // 
            // result
            // 
            result.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            result.Location = new System.Drawing.Point(12, 30);
            result.Name = "result";
            result.Size = new System.Drawing.Size(338, 20);
            result.TabIndex = 0;
            result.WordWrap = false;
            // 
            // label
            // 
            label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            label.Location = new System.Drawing.Point(12, 8);
            label.Name = "label";
            label.Size = new System.Drawing.Size(344, 19);
            label.TabIndex = 3;
            label.Text = "InputBox";
            // 
            // cancel
            // 
            cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancel.Location = new System.Drawing.Point(286, 56);
            cancel.Name = "cancel";
            cancel.Size = new System.Drawing.Size(64, 24);
            cancel.TabIndex = 2;
            cancel.Text = "Cancel";
            // 
            // InputBoxForm
            // 
            AcceptButton = ok;
            AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            CancelButton = cancel;
            ClientSize = new System.Drawing.Size(362, 88);
            Controls.Add(cancel);
            Controls.Add(label);
            Controls.Add(result);
            Controls.Add(ok);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InputBoxForm";
            Text = "InputBox";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        #region event handlers

        /// <summary>
        /// Called during <see cref="Form.Load"/> to set the startup position
        /// (if it has been specified)
        /// </summary>
        /// <param name="e">Data associated with event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // If startup location has been specified, set the start location
            if (!startLocation.IsEmpty)
            {
                Top = startLocation.Y;
                Left = startLocation.X;
            }
        }

        void ok_Click(object sender, EventArgs e)
        {
            // Set the return value
            returnValue = result.Text;

            // Set the dialog result to OK
            DialogResult = DialogResult.OK;
        }

        #endregion event handlers

        /// <summary>
        /// Sets the title for the input box.
        /// </summary>
        public string Title
        {
            set
            {
                Text = value;
            }
        }

        /// <summary>
        /// Sets the prompt label for the input box.
        /// </summary>
        public string Prompt
        {
            set
            {
                label.Text = value;
            }
        }

        /// <summary>
        /// Gets the return result for the input box.
        /// </summary>
        public string ReturnValue
        {
            get
            {
                return returnValue;
            }
        }

        /// <summary>
        /// Sets the default response value for the input box.
        /// </summary>
        public string DefaultResponse
        {
            set
            {
                result.Text = value;
                result.SelectAll();
            }
        }

        /// <summary>
        /// Sets a startup location for the input box.
        /// </summary>
        public Point StartLocation
        {
            set
            {
                startLocation = value;
            }
        }
    }
}
