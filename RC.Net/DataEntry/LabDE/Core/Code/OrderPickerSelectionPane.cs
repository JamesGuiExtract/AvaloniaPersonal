using Extract.FileActionManager.Utilities;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Defines the custom <see cref="IFFIFileSelectionPane"/> to be used in the FFI instance opened
    /// by an <see cref="OrderPickerTableColumn"/>. This pane will display the orders that may be
    /// associated with the currently selected order row in a LabDE DEP.
    /// </summary>
    public partial class OrderPickerSelectionPane : UserControl, IFFIFileSelectionPane
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(OrderPickerSelectionPane).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickerSelectionPane"/> class.
        /// </summary>
        public OrderPickerSelectionPane()
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                // Load licenses in design mode
                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI38143", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38144");
            }
        }

        #endregion Constructors

        #region Public Members

        /// <summary>
        /// Gets the order number that was selected in the UI.
        /// </summary>
        public string SelectedOrderNumber
        {
            get;
            set;
        }

        #endregion Public Members

        #region IFFIFileSelectionPane

        /// <summary>
        /// Raised when the file list indicated by the selection pane has changed.
        /// </summary>
        public event EventHandler<EventArgs> RefreshRequired;

        /// <summary>
        /// Gets the title of the pane.
        /// </summary>
        public string Title
        {
            get
            {
                return "Available orders";
            }
        }

        /// <summary>
        /// The IDs of the files to be populated in the FFI file list.
        /// </summary>
        public IEnumerable<int> SelectedFileIds
        {
            get
            {
                try
                {
                    string orderNumber = GetSelectedOrderNumber();

                    if (string.IsNullOrEmpty(orderNumber))
                    {
                        return new int[0];
                    }
                    else
                    {
                        return RowData.GetCorrespondingFileIds(orderNumber);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38145");
                }
            }
        }

        /// <summary>
        /// The position of the pane in the FFI.
        /// </summary>
        public SelectionPanePosition PanePosition
        {
            get
            {
                return SelectionPanePosition.Top;
            }
        }

        /// <summary>
        /// Gets the <see cref="Control"/> that is to be added into the FFI.
        /// </summary>
        public Control Control
        {
            get
            {
                return this;
            }
        }

        #endregion IFFIFileSelectionPane

        #region Internal Members

        /// <summary>
        /// The <see cref="FAMOrderRow"/> that is used to retrieve and cache order information for
        /// the currently selected <see cref="DataEntryTableRow"/>.
        /// </summary>
        internal FAMOrderRow RowData
        {
            get;
            set;
        }

        #endregion Internal Members

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Control.VisibleChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            try
            {
                base.OnVisibleChanged(e);

                // When initially being made visible, set the initial selection bases on
                // SelectedOrderNumber.
                if (Visible)
                {
                    _ordersDataGridView.ClearSelection();
                    _ordersDataGridView.CurrentCell =
                        _ordersDataGridView.Rows
                            .OfType<DataGridViewRow>()
                            .Select(row => row.Cells[0])
                            .Where(cell => !string.IsNullOrEmpty(SelectedOrderNumber) &&
                                SelectedOrderNumber.Equals(cell.Value.ToString(), StringComparison.Ordinal))
                            .SingleOrDefault();
                    if (_ordersDataGridView.CurrentCell != null)
                    {
                        _ordersDataGridView.CurrentCell.OwningRow.Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38170");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="DataGridView.SelectionChanged"/>event of the
        /// <see cref="_ordersDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOrdersDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                // Refresh to display the that have been filed against the newly selected order.
                OnRefreshRequired();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38146");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_okButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                // On OK, set the SelectedOrderNumber property for the caller.
                SelectedOrderNumber = GetSelectedOrderNumber();

                var ffi = TopLevelControl as FAMFileInspectorForm;
                if (ffi != null)
                {
                    ffi.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38147");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_cancelButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCancelButton_Click(object sender, EventArgs e)
        {
            try
            {
                var ffi = TopLevelControl as FAMFileInspectorForm;
                if (ffi != null)
                {
                    ffi.DialogResult = DialogResult.Cancel;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38148");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Updates the data in <see cref="_ordersDataGridView"/> to display the current possible
        /// matching orders in the FAM database.
        /// </summary>
        public void UpdateOrderSelectionGrid()
        {
            try
            {
                var disposableSource = _ordersDataGridView.DataSource as IDisposable;

                _ordersDataGridView.DataSource = RowData.MatchingOrders.Copy();

                if (disposableSource != null)
                {
                    disposableSource.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38149");
            }
        }

        /// <summary>
        /// Gets the order number of the currently selected row in
        /// <see cref="_ordersDataGridView"/>.
        /// </summary>
        /// <returns>The order number of the currently selected row.</returns>
        string GetSelectedOrderNumber()
        {
            if (_ordersDataGridView.SelectedRows.Count != 1)
            {
                return null;
            }

            return _ordersDataGridView.SelectedRows
                .OfType<DataGridViewRow>()
                .Single()
                .Cells[0].Value.ToString();
        }

        /// <summary>
        /// Raises the <see cref="RefreshRequired"/> event.
        /// </summary>
        void OnRefreshRequired()
        {
            if (RefreshRequired != null)
            {
                RefreshRequired(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
