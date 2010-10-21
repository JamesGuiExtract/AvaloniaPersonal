using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Form used to add a new machine to the data grid.
    /// </summary>
    partial class AddMachineForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddMachineForm"/> class.
        /// </summary>
        public AddMachineForm() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddMachineForm"/> class.
        /// </summary>
        public AddMachineForm(IEnumerable<string> groupNames)
        {
            InitializeComponent();

            if (groupNames != null)
            {
                foreach (string name in groupNames)
                {
                    _groupNameCombo.Items.Add(name);
                }
            }
        }

        #region Methods

        /// <summary>
        /// Handles the browse machine button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleBrowseMachineButtonClick(object sender, EventArgs e)
        {
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the name of the machine.
        /// </summary>
        /// <value>The name of the machine.</value>
        internal string MachineName
        {
            get
            {
                return _textMachineName.Text;
            }
        }

        /// <summary>
        /// Gets the name of the group.
        /// </summary>
        /// <value>The name of the group.</value>
        internal string GroupName
        {
            get
            {
                return _groupNameCombo.Text;
            }
        }

        #endregion Properties
    }
}
