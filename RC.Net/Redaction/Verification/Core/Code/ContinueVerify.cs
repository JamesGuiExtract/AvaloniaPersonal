using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// The dialog that is used to continue verification - prompts to continue session or restart verify.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class ContinueVerify : Form
    {
        /// <summary>
        /// Gets a value indicating whether to continue the verification session.
        /// </summary>
        /// <value>
        /// <c>true</c> if [continue verify session]; otherwise, <c>false</c>.
        /// </value>
        public bool ContinueVerifySession
        {
            get
            {
                return _priorSessionRadioButton.Checked;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinueVerify"/> class.
        /// </summary>
        public ContinueVerify()
        {
            InitializeComponent();
        }
    }
}
