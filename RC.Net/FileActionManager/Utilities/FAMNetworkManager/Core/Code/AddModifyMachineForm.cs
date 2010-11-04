using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Forms;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Form used to add a new machine to the data grid.
    /// </summary>
    partial class AddModifyMachineForm : Form
    {
        #region Fields

        /// <summary>
        /// Rows that will need to be updated based on changes made
        /// </summary>
        List<FAMNetworkDashboardRow> _rows;

        /// <summary>
        /// Whether or not data has been changed.
        /// </summary>
        bool _dataChanged;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AddModifyMachineForm"/> class.
        /// </summary>
        public AddModifyMachineForm() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddModifyMachineForm"/> class.
        /// </summary>
        /// <param name="groupNames">The group names.</param>
        public AddModifyMachineForm(IEnumerable<string> groupNames)
            : this(null, groupNames)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddModifyMachineForm"/> class.
        /// </summary>
        /// <param name="rows">The rows.</param>
        /// <param name="groupNames">The group names.</param>
        public AddModifyMachineForm(IEnumerable<FAMNetworkDashboardRow> rows, IEnumerable<string> groupNames)
        {
            InitializeComponent();

            if (rows != null)
            {
                _rows = new List<FAMNetworkDashboardRow>(rows);
            }
            else
            {
                _rows = new List<FAMNetworkDashboardRow>();
            }

            if (groupNames != null)
            {
                foreach (string name in groupNames)
                {
                    _groupNameCombo.Items.Add(name);
                }
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                string defaultGroupName = null;
                int count = _rows.Count;
                if (count == 0)
                {
                    Text = "Add Machine";
                }
                else if (count == 1)
                {
                    Text = "Modify Machine Name Or Group";
                    _textMachineName.Text = _rows[0].Cells[(int)GridColumns.MachineName].Value.ToString();
                    defaultGroupName = _rows[0].Cells[(int)GridColumns.GroupName].Value.ToString();
                }
                else if (count > 1)
                {
                    Text = "Modify Group";
                    StringBuilder sb = new StringBuilder(
                        _rows[0].Cells[(int)GridColumns.MachineName].Value.ToString());
                    defaultGroupName = _rows[0].Cells[(int)GridColumns.GroupName].Value.ToString();
                    for (int i = 1; i < _rows.Count; i++)
                    {
                        if (defaultGroupName != null)
                        {
                            string temp = _rows[i].Cells[(int)GridColumns.GroupName].Value.ToString();
                            if (!defaultGroupName.Equals(temp, StringComparison.Ordinal))
                            {
                                defaultGroupName = null;
                            }
                        }
                        sb.Append(',');
                        sb.Append(_rows[i].Cells[(int)GridColumns.MachineName].Value.ToString());
                    }
                    _textMachineName.Text = sb.ToString();
                    _textMachineName.ReadOnly = true;
                }

                if (defaultGroupName != null)
                {
                    int index = _groupNameCombo.FindStringExact(defaultGroupName);
                    if (index != -1)
                    {
                        _groupNameCombo.SelectedIndex = index;
                    }
                }

                _dataChanged = false;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30838", ex);
            }
        }

        /// <summary>
        /// Handles the browse machine button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleBrowseMachineButtonClick(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the machine name text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleMachineNameTextChanged(object sender, EventArgs e)
        {
            _dataChanged = true;
        }

        /// <summary>
        /// Handles the group name text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleGroupNameTextChanged(object sender, EventArgs e)
        {
            _dataChanged = true;
        }

        /// <summary>
        /// Handles the ok button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Ensure that the machine name text box is not empty
                if (string.IsNullOrWhiteSpace(_textMachineName.Text))
                {
                    MessageBox.Show("You must specify a machine name.",
                        "No Machine Name", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1, 0);
                    return;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30981", ex);
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the data was changed or not.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if data changed; otherwise, <see langword="false"/>.
        /// </value>
        internal bool DataChanged
        {
            get
            {
                return _dataChanged;
            }
        }

        /// <summary>
        /// Gets the name of the machine.
        /// </summary>
        /// <value>The name of the machine.</value>
        internal string MachineName
        {
            get
            {
                return _textMachineName.Text.Trim();
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
                return _groupNameCombo.Text.Trim();
            }
        }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        /// <value>The rows.</value>
        internal ReadOnlyCollection<FAMNetworkDashboardRow> Rows
        {
            get
            {
                return _rows.AsReadOnly();
            }
        }

        #endregion Properties
    }
}
