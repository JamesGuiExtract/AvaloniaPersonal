using Extract.Utilities;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Forms
{
    /// <summary>
    /// Form used to view attribute history
    /// </summary>
    [CLSCompliant(false)]
    public partial class RuleTesterAttributeHistoryForm : Form
    {
        #region Fields

        ComAttribute _attribute;

        #endregion Fields

        #region Constructors 

        /// <summary>
        /// Constructor for form
        /// </summary>
        public RuleTesterAttributeHistoryForm(ComAttribute attribute)
        {
            try
            {
                InitializeComponent();

                _attribute = attribute;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41771");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                ShowAttributeHistory();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41772");
            }
        }

        #endregion Overrides

        #region Private Methods

        /// <summary>
        /// Method used to show attribute history
        /// </summary>
        void ShowAttributeHistory()
        {
            var trace = _attribute.DataObject as IUnknownVector;
            if (trace != null)
            {
                var items = trace.ToIEnumerable<object>()
                    .OfType<IStrToStrMap>()
                    .Reverse()
                    .ToList();

                var keys = (new[] { "Ruleset", "Attribute", "Rule number", "Rule description" })
                    // Just in case there are new keys introduced without this getting updated
                    .Union(items.SelectMany(map => map.GetKeys().ToIEnumerable<string>()).OrderBy(k => k))
                    .ToList();

                // Initialize the DataGridView
                _historyDataGridView.AutoGenerateColumns = false;

                var columns = keys.Select((key, i) =>
                {
                    var column = new DataGridViewTextBoxColumn();
                    column.Name = key;
                    column.ReadOnly = true;
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    if (i == 0)
                    {
                        column.MinimumWidth = 500;
                    }
                    else if (i == 3)
                    {
                        column.MinimumWidth = 250;
                    }
                    return column;
                }).ToArray();
                _historyDataGridView.Columns.AddRange(columns);

                // Add rows
                foreach (var item in items.Select((map, i) =>
                        keys.Select(k => map.Contains(k) ? map.GetValue(k) : "")
                            .ToArray()))
                {
                    var rowIdx = _historyDataGridView.Rows.Add(item);

                    // Create row number
                    _historyDataGridView.Rows[rowIdx].HeaderCell.Value = (rowIdx + 1)
                        .ToString(CultureInfo.CurrentCulture);
                }
            }
        }

        #endregion Private Methods
    }
}