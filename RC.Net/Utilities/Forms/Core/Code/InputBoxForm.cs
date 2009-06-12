// InputBox - Based on implementation by: Andrew Ma
// URL: http://www.devhood.com/Tools/tool_details.aspx?tool_id=295

using Extract;
using Extract.Licensing;
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
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
        private static readonly string _OBJECT_NAME =
            typeof(InputBoxForm).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Okay <see cref="Button"/>.
        /// </summary>
		private Button ok;

        /// <summary>
        /// Cancel <see cref="Button"/>.
        /// </summary>
		private Button cancel;

        /// <summary>
        /// <see cref="Label"/> for the input box.
        /// </summary>
		private Label label;

        /// <summary>
        /// <see cref="TextBox"/> containing the return value.
        /// </summary>
		private TextBox result;

        /// <summary>
        /// Field holding the return result of the input box.
        /// </summary>
		private string returnValue;

        /// <summary>
        /// <see cref="Point"/> representing the top left coordinate of where the
        /// input box should be displayed.
        /// </summary>
		private Point startLocation;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

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
                this.returnValue = "";
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
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
        }

        #endregion IDisposable

        #region Windows Form Designer generated code
        /// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.ok = new System.Windows.Forms.Button();
            this.result = new System.Windows.Forms.TextBox();
            this.label = new System.Windows.Forms.Label();
            this.cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ok
            // 
            this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ok.Location = new System.Drawing.Point(216, 56);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(64, 24);
            this.ok.TabIndex = 1;
            this.ok.Text = "OK";
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // result
            // 
            this.result.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.result.Location = new System.Drawing.Point(12, 30);
            this.result.Name = "result";
            this.result.Size = new System.Drawing.Size(338, 20);
            this.result.TabIndex = 0;
            this.result.WordWrap = false;
            // 
            // label
            // 
            this.label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label.Location = new System.Drawing.Point(12, 8);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(344, 19);
            this.label.TabIndex = 3;
            this.label.Text = "InputBox";
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(286, 56);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(64, 24);
            this.cancel.TabIndex = 2;
            this.cancel.Text = "Cancel";
            // 
            // InputBoxForm
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(362, 88);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.label);
            this.Controls.Add(this.result);
            this.Controls.Add(this.ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputBoxForm";
            this.Text = "InputBox";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        #region event handlers

        /// <summary>
        /// Called during <see cref="Form.Load"/> to set the startup position
        /// (if it has been specified)
        /// </summary>
        /// <param name="e">Data associated with this event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // If startup location has been specified, set the start location
			if (!this.startLocation.IsEmpty) 
			{
				this.Top = this.startLocation.Y;
				this.Left = this.startLocation.X;
			}
        }

        private void ok_Click(object sender, EventArgs e)
        {
            // Set the return value
            returnValue = result.Text;

            // Set the dialog result to OK
            this.DialogResult = DialogResult.OK;

            // Close this dialog
            this.Close();
        }

        #endregion event handlers

        /// <summary>
        /// Sets the title for the input box.
        /// </summary>
        public string Title
		{
			set
			{
				this.Text = value;
			}
		}

        /// <summary>
        /// Sets the prompt label for the input box.
        /// </summary>
		public string Prompt
		{
			set
			{
				this.label.Text = value;
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
				this.result.Text = value;
				this.result.SelectAll();
			}
		}

        /// <summary>
        /// Sets a startup location for the input box.
        /// </summary>
		public Point StartLocation
		{
			set
			{
				this.startLocation = value;
			}
		}
    }
}
