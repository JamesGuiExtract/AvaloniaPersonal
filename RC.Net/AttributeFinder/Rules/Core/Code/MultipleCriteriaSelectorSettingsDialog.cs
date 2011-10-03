using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

using SelectorDataRow =
    Extract.Utilities.Forms.BetterDataGridViewRow<UCLID_COMUTILSLib.ObjectWithDescription>;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="MultipleCriteriaSelector"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class MultipleCriteriaSelectorSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(MultipleCriteriaSelectorSettingsDialog).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// A <see cref="MiscUtils"/> instance to use configuring the
        /// <see cref="IAttributeSelector"/>s.
        /// </summary>
        MiscUtils _miscUtils = new MiscUtils();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleCriteriaSelectorSettingsDialog"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="MultipleCriteriaSelector"/> instance to configure.
        /// </param>
        public MultipleCriteriaSelectorSettingsDialog(MultipleCriteriaSelector settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleWritingCoreObjects, "ELI33870",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33871");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="MultipleCriteriaSelector"/> to configure.
        /// </summary>
        /// <value>The <see cref="MultipleCriteriaSelector"/> to configure.</value>
        public MultipleCriteriaSelector Settings
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the index of the currently selected row.
        /// </summary>
        /// <value>
        /// The index of the currently selected row.
        /// </value>
        int SelectedRowIndex
        {
            get
            {

                int index = (_selectorDataGridView.SelectedRows.Count == 0)
                    ? -1
                    : _selectorDataGridView.SelectedRows
                        .Cast<DataGridViewRow>()
                        .First()
                        .Index;

                return index;
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    int count = Settings.Selectors.Size();
                    ExtractException.Assert("ELI33876", "Corrupted configuration",
                        count == Settings.NegatedSelectors.Length);

                    _selectorDataGridView.Rows.Clear();

                    for (int i = 0; i < count; i++)
                    {
                        ObjectWithDescription owd = (ObjectWithDescription)Settings.Selectors.At(i);

                        _selectorDataGridView.Rows.Add(new SelectorDataRow());
                        SelectorDataRow row = (SelectorDataRow)_selectorDataGridView.Rows[i];
                        row.DataItem = owd;
                        row.Cells[0].Value = owd.Enabled;
                        row.Cells[1].Value = Settings.NegatedSelectors[i];
                        row.Cells[2].Value = owd.Description;
                    }

                    _selectorDataGridView.ClearSelection();

                    _commandButton.Enabled = false;
                    _deleteButton.Enabled = false;

                    _andRadioButton.Checked = Settings.SelectExclusively;
                    _orRadioButton.Checked = !Settings.SelectExclusively;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33872");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// In the case that the OK button is clicked, validates the settings, applies them, and
        /// closes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // If there are invalid settings, prompt and return without closing.
                if (WarnIfInvalid())
                {
                    return;
                }

                Settings.Selectors = new IUnknownVector();
                Settings.NegatedSelectors = new bool[_selectorDataGridView.Rows.Count];

                foreach (SelectorDataRow row in _selectorDataGridView.Rows.Cast<SelectorDataRow>())
                {
                    ObjectWithDescription owd = row.DataItem;
                    owd.Enabled = (bool)row.Cells[0].Value;
                    Settings.Selectors.PushBack(owd);

                    Settings.NegatedSelectors[row.Index] = (bool)row.Cells[1].Value;
                }

                Settings.SelectExclusively = _andRadioButton.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33880");
            }
        }

        /// <summary>
        /// Handles the insert button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleInsertButtonClick(object sender, EventArgs e)
        {
            try
            {
                Point menuLocationPoint = PointToScreen(ClientRectangle.Location);
                menuLocationPoint.Offset(_insertButton.Right, _insertButton.Top);

                int index = (SelectedRowIndex == -1)
                    ? _selectorDataGridView.Rows.Count
                    : SelectedRowIndex;

                ObjectWithDescription owd = new ObjectWithDescription();
                Guid guid = new Guid();
                if (_miscUtils.AllowUserToSelectAndConfigureObject(owd, "Attribute selector",
                        ExtractCategories.AttributeSelectorsName, false, 0, guid))
                {
                    _selectorDataGridView.Rows.Insert(index, new SelectorDataRow());
                    SelectorDataRow row = (SelectorDataRow)_selectorDataGridView.Rows[index];
                    row.DataItem = owd;
                    row.Cells[0].Value = owd.Enabled;
                    row.Cells[1].Value = false;
                    row.Cells[2].Value = owd.Description;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33877");
            }
        }

        /// <summary>
        /// Handles the command button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCommandsButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_selectorDataGridView.SelectedRows.Count == 1)
                {
                    Point menuLocationPoint = PointToScreen(ClientRectangle.Location);
                    menuLocationPoint.Offset(_commandButton.Right, _commandButton.Top);

                    int index = SelectedRowIndex;

                    ObjectWithDescription owd =
                        ((SelectorDataRow)_selectorDataGridView.Rows[index])
                        .DataItem;

                    Guid guid = new Guid();
                    if (_miscUtils.HandlePlugInObjectCommandButtonClick(owd, "Attribute Selector",
                            ExtractCategories.AttributeSelectorsName, true, 0, ref guid,
                            menuLocationPoint.X, menuLocationPoint.Y))
                    {
                        _selectorDataGridView.Rows[index].Cells[2].Value = owd.Description;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33879");
            }
        }

        /// <summary>
        /// Handles the delete button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDeleteButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_selectorDataGridView.SelectedRows.Count == 1)
                {
                    _selectorDataGridView.Rows.RemoveAt(SelectedRowIndex);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33884");
            }
        }

        /// <summary>
        /// Handles a new selection in the grid.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                bool bEnable = (_selectorDataGridView.SelectedRows.Count != 0);

                _commandButton.Enabled = bEnable;
                _deleteButton.Enabled = bEnable;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33882");
            }
        }

        /// <summary>
        /// Handles the case that the 
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellEventArgs"/>
        /// instance containing the event data.</param>
        void HandleCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // If either the enabled or negated column was modified, update the underlying value.
                if (e.ColumnIndex < 2)
                {
                    DataGridViewCell cell = _selectorDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    cell.Value = !(bool)cell.Value;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33885");
            }
        }

        /// <summary>
        /// Handles a double-click in a grid cell
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellEventArgs"/>
        /// instance containing the event data.</param>
        void HandleCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    ObjectWithDescription owd =
                        ((SelectorDataRow)_selectorDataGridView.Rows[e.RowIndex]).DataItem;

                    // Basic double-click = configure
                    if (Control.ModifierKeys == Keys.None)
                    {
                        _miscUtils.AllowUserToConfigureObjectProperties(owd);
                    }
                    // Alt + double-click = edit description
                    else if (Control.ModifierKeys == Keys.Alt)
                    {
                        if (_miscUtils.AllowUserToConfigureObjectDescription(owd))
                        {
                            _selectorDataGridView.Rows[e.RowIndex].Cells[2].Value =
                                owd.Description;
                        }
                    }
                    // Ctrl + double-click = Select and configure
                    else if (Control.ModifierKeys == Keys.Control)
                    {
                        Guid guid = new Guid();
                        if (_miscUtils.AllowUserToSelectAndConfigureObject(owd, "Attribute Selector",
                            ExtractCategories.AttributeSelectorsName, true, 0, ref guid))
                        {
                            _selectorDataGridView.Rows[e.RowIndex].Cells[2].Value =
                                owd.Description;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33886");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (_selectorDataGridView.Rows.Count == 0)
            {
                _insertButton.Focus();
                UtilityMethods.ShowMessageBox("At least one selector must be specified.",
                    "Add a selector", false);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
