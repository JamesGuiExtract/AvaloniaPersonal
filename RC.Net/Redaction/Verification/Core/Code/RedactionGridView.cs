using Extract.Imaging;
using Extract.Imaging.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

using EOrientation = UCLID_RASTERANDOCRMGMTLib.EOrientation;
using RedactionLayerObject = Extract.Imaging.Forms.Redaction;
using SpatialPageInfo = UCLID_RASTERANDOCRMGMTLib.SpatialPageInfo;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a <see cref="DataGridView"/> that displays information about redactions.
    /// </summary>
    public sealed partial class RedactionGridView : UserControl, IImageViewerControl
    {
        #region Enums

        /// <summary>
        /// Specifies the set of indexes in <see cref="_CATEGORIES"/>.
        /// </summary>
        enum CategoryIndex
        {
            /// <summary>
            /// Clues
            /// </summary>
            Clues,

            /// <summary>
            /// High confidence data
            /// </summary>
            HCData,

            /// <summary>
            /// Low confidence data
            /// </summary>
            LCData,

            /// <summary>
            /// Manual redactions
            /// </summary>
            Manual,

            /// <summary>
            /// Medium confidence data
            /// </summary>
            MCData

            // Note: New values must be inserted in alphabetical order,
            // because the names of the enum are binary searched.
        }

        #endregion Enums

        #region Constants

        /// <summary>
        /// Gets the redaction categories that are recorded in the ID Shield database.
        /// </summary>
        static readonly string[] _CATEGORIES = Enum.GetNames(typeof(CategoryIndex));

        /// <summary>
        /// The minimum amount of padding to apply when calculating the padding around an 
        /// auto-zoom.
        /// </summary>
        const int _MIN_PADDING = 25;

        /// <summary>
        /// The amount to multiply the <see cref="AutoZoomScale"/> when calculating the padding 
        /// around a view.
        /// </summary>
        const int _PADDING_MULTIPLIER = 60;

        /// <summary>
        /// The default value for the <see cref="AutoZoomScale"/>.
        /// </summary>
        const int _DEFAULT_AUTO_ZOOM_SCALE = 5;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The color that toggled on redactions are drawn on the image viewer.
        /// </summary>
        public static readonly Color ToggledRedactionColor = Color.CornflowerBlue;

        /// <summary>
        /// The <see cref="ImageViewer"/> with which the <see cref="RedactionGridView"/> is 
        /// associated.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// Each row of the <see cref="RedactionGridView"/> which represents a redaction.
        /// </summary>
        readonly BindingList<RedactionGridViewRow> _redactions = new BindingList<RedactionGridViewRow>();

        /// <summary>
        /// A dialog that allows the user to select exemption codes.
        /// </summary>
        ExemptionCodeListDialog _exemptionsDialog;

        /// <summary>
        /// The last applied exemption codes or <see langword="null"/> if no exemption code has 
        /// been applied.
        /// </summary>
        ExemptionCodeList _lastCodes;

        /// <summary>
        /// The master list of valid exemption categories and codes.
        /// </summary>
        MasterExemptionCodeList _masterCodes;

        /// <summary>
        /// The last applied redaction type or the empty string if no redaction type has been 
        /// applied.
        /// </summary>
        string _lastType = "";

        /// <summary>
        /// Keeps track of the count of categories of deleted redactions.
        /// </summary>
        readonly int[] _deletedCategoryCounts = new int[_CATEGORIES.Length];

        /// <summary>
        /// The redaction items corresponding to rows deleted since the last save.
        /// </summary>
        readonly List<RedactionItem> _deletedItems = new List<RedactionItem>();

        /// <summary>
        /// The tool to automatically select after a redaction has been created.
        /// </summary>
        AutoTool _autoTool;

        /// <summary>
        /// <see langword="true"/> if the zoom setting should change when redactions are selected;
        /// <see langword="false"/> if the zoom setting should remain the same.
        /// </summary>
        bool _autoZoom;

        /// <summary>
        /// The zoom setting when <see cref="_autoZoom"/> is <see langword="true"/>.
        /// </summary>
        int _autoZoomScale = _DEFAULT_AUTO_ZOOM_SCALE;

        /// <summary>
        /// The confidence level associated with redactions and clues.
        /// </summary>
        ConfidenceLevelsCollection _confidenceLevels;

        /// <summary>
        /// <see langword="true"/> if changes have been made to the grid since it was loaded;
        /// <see langword="false"/> if no changes have been made to the grid since it was loaded.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Font for rows that have been visited.
        /// </summary>
        Font _visitedFont;

        /// <summary>
        /// The cell style for rows that have been not been visited and are read-only.
        /// </summary>
        DataGridViewCellStyle _readOnlyCellStyle;

        /// <summary>
        /// The cell style for the page cell of rows that have been not been visited and are read-only.
        /// </summary>
        DataGridViewCellStyle _readOnlyPageCellStyle;

        /// <summary>
        /// The cell style for rows that have been visited and are editable.
        /// </summary>
        DataGridViewCellStyle _visitedCellStyle;

        /// <summary>
        /// The cell style for the page cell of visited rows that are editable.
        /// </summary>
        DataGridViewCellStyle _visitedPageCellStyle;

        /// <summary>
        /// The cell style for rows that have been visited and are read-only.
        /// </summary>
        DataGridViewCellStyle _visitedReadOnlyCellStyle;

        /// <summary>
        /// The cell style for the page cell of rows that have been visited and are read-only.
        /// </summary>
        DataGridViewCellStyle _visitedReadOnlyPageCellStyle;

        /// <summary>
        /// Indicates whether an auto-zoom operation is in progress so that the grid can keep track
        /// of whether the image viewer is currently zoomed to the selection.
        /// </summary>
        bool _performingAutoZoom;

        /// <summary>
        /// Indicates whether the grid is actively tracking data.
        /// </summary>
        bool _active;

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when an exemption code is applied to a redaction.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when an exemption code is applied to a redaction.")]
        public event EventHandler<ExemptionsAppliedEventArgs> ExemptionsApplied;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="RedactionGridView"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public RedactionGridView()
        {
            InitializeComponent();

            _dataGridView.AutoGenerateColumns = false;
            _dataGridView.DataSource = _redactions;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the rows of the <see cref="RedactionGridView"/>.
        /// </summary>
        /// <returns>The rows of the <see cref="RedactionGridView"/>.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ReadOnlyCollection<RedactionGridViewRow> Rows
        {
            get
            {
                return new ReadOnlyCollection<RedactionGridViewRow>(_redactions);
            }
        }

        /// <summary>
        /// Gets the selected rows of the <see cref="RedactionGridView"/>.
        /// </summary>
        /// <returns>The selected rows of the <see cref="RedactionGridView"/>.</returns>
        IEnumerable<RedactionGridViewRow> SelectedRows
        {
            get
            {
                foreach (DataGridViewRow row in _dataGridView.SelectedRows)
                {
                    yield return _redactions[row.Index];
                }
            }
        }

        /// <summary>
        /// Gets the exemption codes dialog that allows the user to select exemption codes.
        /// </summary>
        /// <returns>The exemption codes dialog that allows the user to select exemption codes.</returns>
        ExemptionCodeListDialog ExemptionsDialog
        {
            get
            {
                // Create the exemption codes if necessary
                if (_exemptionsDialog == null)
                {
                    _exemptionsDialog = new ExemptionCodeListDialog(MasterCodes);
                }

                // Set the last applied exemption code if necessary
                if (_lastCodes != null)
                {
                    _exemptionsDialog.EnableApplyLast = true;
                    _exemptionsDialog.LastExemptionCodeList = _lastCodes;
                }

                return _exemptionsDialog;
            }
        }

        /// <summary>
        /// Gets the master list of valid exemption categories and codes.
        /// </summary>
        /// <returns>The master list of valid exemption categories and codes.</returns>
        MasterExemptionCodeList MasterCodes
        {
            get
            {
                // Lazy instantiation
                if (_masterCodes == null)
                {
                    _masterCodes = new MasterExemptionCodeList();
                }

                return _masterCodes;
            }
        }

        /// <summary>
        /// Gets whether any exemption codes have been applied.
        /// </summary>
        /// <returns><see langword="true"/> if any exemption codes have been applied;
        /// <see langword="false"/> if no exemption codes have been applied.</returns>
        public bool HasAppliedExemptions
        {
            get
            {
                return _lastCodes != null;
            }
        }

        /// <summary>
        /// Gets or sets the possible confidence levels for redactions and clues.
        /// </summary>
        /// <value>The possible confidence levels for redactions and clues.</value>
        // Using a setter makes working with the confidence levels simpler
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ConfidenceLevelsCollection ConfidenceLevels
        {
            get
            {
                return _confidenceLevels;
            }
            set
            {
                _confidenceLevels = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the redactions have been modified since they were last loaded.
        /// </summary>
        /// <value><see langword="true"/> if the redactions have been modified since they were 
        /// last loaded; <see langword="false"/> if the redactions have not been modified since 
        /// they were last loaded.</value>
        /// <returns><see langword="true"/> if the redactions have been modified since they were 
        /// last loaded; <see langword="false"/> if the redactions have not been modified since 
        /// they were last loaded.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Dirty
        {
            get
            {
                return _dirty;
            }
            set
            {
                _dirty = value;
            }
        }

        /// <summary>
        /// Gets whether the redaction grid contains any redactions.
        /// </summary>
        /// <value><see langword="true"/> if the grid contains redactions;
        /// <see langword="false"/> if the grid does not contain redactions.</value>
        public bool HasRedactions
        {
            get
            {
                return _redactions.Count > 0;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CursorTool"/> that is automatically selected when a layer 
        /// object is added.
        /// </summary>
        /// <value>The <see cref="CursorTool"/> that is automatically selected when a layer object 
        /// is added.</value>
        [DefaultValue(AutoTool.None)]
        [Description("The tool that is automatically selected when a redaction is added.")]
        public AutoTool AutoTool
        {
            get
            {
                return _autoTool;
            }
            set
            {
                _autoTool = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to automatically zoom to selected redactions.
        /// </summary>
        /// <value><see langword="true"/> if the view should zoom automatically to selected redactions;
        /// <see langword="false"/> if the view should not change when redactions are selected.</value>
        /// <returns><see langword="true"/> if the view should zoom automatically to selected redactions;
        /// <see langword="false"/> if the view should not change when redactions are selected.</returns>
        [DefaultValue(false)]
        [Description("Whether to automatically zoom to selected redactions.")]
        public bool AutoZoom
        {
            get
            {
                return _autoZoom;
            }
            set
            {
                _autoZoom = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount to zoom out when <see cref="AutoZoom"/> is 
        /// <see langword="true"/>.
        /// </summary>
        /// <value>The amount to zoom out when <see cref="AutoZoom"/> is <see langword="true"/>.
        /// </value>
        [Description("The amount to zoom out when AutoZoom is true.")]
        public int AutoZoomScale
        {
            get
            {
                return _autoZoomScale;
            }
            set
            {
                _autoZoomScale = value;
            }
        }

        /// <summary>
        /// Determines whether the <see cref="AutoZoomScale"/> property should be serialized.
        /// </summary>
        /// <returns><see langword="true"/> if the property should be serialized; 
        /// <see langword="false"/> if the property should not be serialized.</returns>
        bool ShouldSerializeAutoZoomScale()
        {
            return _autoZoom && _autoZoomScale != _DEFAULT_AUTO_ZOOM_SCALE;
        }

        /// <summary>
        /// Resets the <see cref="AutoZoomScale"/> property to its default value.
        /// </summary>
        void ResetAutoZoomScale()
        {
            _autoZoomScale = _DEFAULT_AUTO_ZOOM_SCALE;
        }

        /// <summary>
        /// Gets whether the currently active cell is in edit mode.
        /// </summary>
        /// <value><see langword="true"/> if the currently active cell is in edit mode;
        /// <see langword="false"/> if the currently active cell is not in edit mode.</value>
        public bool IsInEditMode
        {
            get
            {
                return _dataGridView.IsCurrentCellInEditMode && !IsRedactedColumnActive;
            }
        }

        /// <summary>
        /// Gets whether the redaction type combo box is dropped down.
        /// </summary>
        /// <value><see langword="true"/> if the redaction type combo box is dropped down;
        /// <see langword="false"/> if the redaction type combo box is not dropped down.</value>
        bool IsComboBoxDroppedDown
        {
            get
            {
                DataGridViewComboBoxEditingControl control =
                    _dataGridView.EditingControl as DataGridViewComboBoxEditingControl;
                return control != null && control.DroppedDown;
            }
        }

        /// <summary>
        /// Gets or sets whether the redacted column is active.
        /// </summary>
        /// <value><see langword="true"/> if the redacted column is active;
        /// <see langword="false"/> if the redacted column is not active.</value>
        bool IsRedactedColumnActive
        {
            get
            {
                return _dataGridView.CurrentCell != null &&
                    IsRedactedColumn(_dataGridView.CurrentCell.ColumnIndex);
            }
        }

        /// <summary>
        /// Gets the font associated with visited redactions.
        /// </summary>
        /// <value>The font associated with visited redactions.</value>
        Font VisitedFont
        {
            get
            {
                if (_visitedFont == null)
                {
                    // Un-bold the default font
                    Font font = _dataGridView.DefaultCellStyle.Font;
                    _visitedFont = new Font(font, font.Style & ~FontStyle.Bold);
                }

                return _visitedFont;
            }
        }

        /// <summary>
        /// Gets the <see cref="DataGridViewCellStyle"/> associated with unvisited, editable redactions.
        /// </summary>
        /// <value>The <see cref="DataGridViewCellStyle"/> associated with unvisited redactions.</value>
        DataGridViewCellStyle ReadOnlyCellStyle
        {
            get
            {
                if (_readOnlyCellStyle == null)
                {
                    // Use InactiveCaptionText color for this row.
                    DataGridViewCellStyle style = _dataGridView.DefaultCellStyle.Clone();
                    style.SelectionForeColor = Color.LightGray;
                    style.BackColor = Color.LightGray;
                    _readOnlyCellStyle = style;
                }

                return _readOnlyCellStyle;
            }
        }

        /// <summary>
        /// Gets the <see cref="DataGridViewCellStyle"/> for the cell in the page column of unvisited,
        /// editable redactions.
        /// </summary>
        /// <value>The <see cref="DataGridViewCellStyle"/> for the cell in the page column of unvisited
        /// redactions.</value>
        DataGridViewCellStyle ReadOnlyPageCellStyle
        {
            get
            {
                if (_readOnlyPageCellStyle == null)
                {
                    // Use the read-only cell style, but change the text alignment
                    DataGridViewCellStyle style = ReadOnlyCellStyle.Clone();
                    style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    _readOnlyPageCellStyle = style;
                }

                return _readOnlyPageCellStyle;
            }
        }

        /// <summary>
        /// Gets the <see cref="DataGridViewCellStyle"/> associated with visited, editable redactions.
        /// </summary>
        /// <value>The <see cref="DataGridViewCellStyle"/> associated with visited, editable redactions.
        /// </value>
        DataGridViewCellStyle VisitedCellStyle
        {
            get
            {
                if (_visitedCellStyle == null)
                {
                    // Use the visited font for this row
                    DataGridViewCellStyle style = _dataGridView.DefaultCellStyle.Clone();
                    style.Font = VisitedFont;
                    _visitedCellStyle = style;
                }

                return _visitedCellStyle;
            }
        }

        /// <summary>
        /// Gets the <see cref="DataGridViewCellStyle"/> for the cell in the page column of 
        /// visited redactions.
        /// </summary>
        /// <value>The <see cref="DataGridViewCellStyle"/> for the cell in the page column of 
        /// visited redactions.</value>
        DataGridViewCellStyle VisitedPageCellStyle
        {
            get
            {
                if (_visitedPageCellStyle == null)
                {
                    // Use the visited row but change the text alignment
                    DataGridViewCellStyle style = VisitedCellStyle.Clone();
                    style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    _visitedPageCellStyle = style;
                }

                return _visitedPageCellStyle;
            }
        }

        /// <summary>
        /// Gets the <see cref="DataGridViewCellStyle"/> associated with visited, read-only redactions.
        /// </summary>
        /// <value>The <see cref="DataGridViewCellStyle"/> associated with visited, read-only
        /// redactions.</value>
        DataGridViewCellStyle VisitedReadOnlyCellStyle
        {
            get
            {
                if (_visitedReadOnlyCellStyle == null)
                {
                    // Use the read-only cell style, but with the visited font for this row
                    DataGridViewCellStyle style = ReadOnlyCellStyle.Clone();
                    style.Font = VisitedFont;
                    _visitedReadOnlyCellStyle = style;
                }

                return _visitedReadOnlyCellStyle;
            }
        }

        /// <summary>
        /// Gets the <see cref="DataGridViewCellStyle"/> for the cell in the page column of visited, read-only redactions.
        /// </summary>
        /// <value>The <see cref="DataGridViewCellStyle"/> for the cell in the page column of visited, read-only
        /// redactions.</value>
        DataGridViewCellStyle VisitedReadOnlyPageCellStyle
        {
            get
            {
                if (_visitedReadOnlyPageCellStyle == null)
                {
                    // Use the visited, read-only style, but with center alignment for this row.
                    DataGridViewCellStyle style = VisitedReadOnlyCellStyle.Clone();
                    style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    _visitedReadOnlyPageCellStyle = style;
                }

                return _visitedReadOnlyPageCellStyle;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the next KeyUp event should be suppressed.
        /// </summary>
        /// <value><see langword="true"/> to suppress the next next key up event; otherwise,
        /// <see langword="false"/>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal bool SuppressNextKeyUp
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds the specified row to the <see cref="RedactionGridView"/>.
        /// </summary>
        /// <param name="layerObject">The layer object to add.</param>
        /// <param name="text">The text associated with the <paramref name="layerObject"/>.</param>
        /// <param name="category">The category associated with the <paramref name="layerObject"/>.</param>
        /// <param name="type">The type associated with the <paramref name="layerObject"/>.</param>
        /// <param name="confidenceLevel">The confidence level associated with the 
        /// <paramref name="layerObject"/>.</param>
        /// <returns>The index of the row that was added.</returns>
        public int Add(LayerObject layerObject, string text, string category, string type, 
            ConfidenceLevel confidenceLevel)
        {
            try
            {
                return Add( new RedactionGridViewRow(layerObject, text, category, type, confidenceLevel) );
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26716",
                    "Unable to add layer object.", ex);
            }
        }

        /// <summary>
        /// Adds the specified row to the <see cref="RedactionGridView"/>.
        /// </summary>
        /// <param name="row">The row to add.</param>
        /// <returns>The index of the row that was added.</returns>
        int Add(RedactionGridViewRow row)
        {
            string type = row.RedactionType;
            if (!string.IsNullOrEmpty(type) && !_typeColumn.Items.Contains(type))
            {
                _typeColumn.Items.Add(type);
            }

            if (row.RedactionItem == null)
            {
                // [FlexIDSCore:4933]
                // Save a temporary RedactionItem for the row right away so that it can be sorted
                // into the correct position amongst any other existing redactions in the grid.
                LongToObjectMap pageInfoMap = GetPageInfoMap();
                row.SaveRedactionItem("", pageInfoMap);
            }

            // Find the index at which to add the new item to the grid based on its spatial location
            // in the document relative to the existing items.
            int index = Array.BinarySearch(_redactions.ToArray(), row,
                new RedactionGridViewRowComparer());

            if (index < 0)
            {
                // The index where the new row should be added should be the bitwise complement of
                // a negative BinarySearch result.
                index = ~index;
            }

            // [FlexIDSCore:4979]
            // If these was no previous selection, the insert call may end up selecting (and thus
            // marking viewed) another item in the set before the new one is selected.
            // De-activate before adding the new item to prevent this from happening.
            bool wasActive = Active;
            Active = false;
            try
            {
                _redactions.Insert(index, row);
            }
            finally
            {
                Active = wasActive;
            }

            _dirty = true;
            return index;
        }

        /// <summary>
        /// Removes the specified layer object from the redaction grid view.
        /// </summary>
        /// <param name="layerObject">The layer object to remove.</param>
        public void Remove(LayerObject layerObject)
        {
            try
            {
                // Find the layer object and remove it.
                for (int i = 0; i < _redactions.Count; i++)
                {
                    RedactionGridViewRow row = _redactions[i];
                    if (row.TryRemoveLayerObject(layerObject))
                    {
                        if (row.LayerObjects.Count == 0)
                        {
                            AddToDeletedCount(row);

                            _redactions.RemoveAt(i);
                        }

                        _dirty = true;

                        return;
                    }
                }

                // The layer object wasn't found. Complain.
                ExtractException ee = new ExtractException("ELI26681", "Layer object not found.");
                ee.AddDebugData("Id", layerObject.Id, false);
                throw ee;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26693", ex);
            }
        }

        /// <summary>
        /// Adds the specified row to the counts of deleted redaction categories.
        /// </summary>
        /// <param name="row">The row that is being deleted.</param>
        void AddToDeletedCount(RedactionGridViewRow row)
        {
            int index = GetCategoryIndex(row);
            if (index >= 0)
            {
                // Store this deleted attribute if it was created in a previous session
                if (!row.IsNew)
                {
                    _deletedItems.Add(row.RedactionItem);
                }

                // Check if this category of redaction should be counted
                // Note: Deleted manual redactions are not counted
                if (index != (int) CategoryIndex.Manual)
                {
                    _deletedCategoryCounts[index]++;
                }
            }
        }

        /// <summary>
        /// Gets index of the category of the specified row.
        /// </summary>
        /// <param name="row">The row to evaluate.</param>
        /// <returns>The index of the category of <paramref name="row"/>; or -1 if the category of 
        /// <paramref name="row"/> does not correspond to a <see cref="CategoryIndex"/>.</returns>
        static int GetCategoryIndex(RedactionGridViewRow row)
        {
            return Array.BinarySearch(_CATEGORIES, row.Category, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the specified index corresponds to the redacted column.
        /// </summary>
        /// <param name="index">The index to test.</param>
        /// <returns><see langword="true"/> if <paramref name="index"/> corresponds to the 
        /// redacted column; <see langword="false"/> if it does not.</returns>
        bool IsRedactedColumn(int index)
        {
            return _redactedColumn.Index == index;
        }

        /// <summary>
        /// Determines whether the specified index corresponds to the redaction type column.
        /// </summary>
        /// <param name="index">The index to test.</param>
        /// <returns><see langword="true"/> if <paramref name="index"/> corresponds to the 
        /// redaction type column; <see langword="false"/> if it does not.</returns>
        bool IsTypeColumn(int index)
        {
            return _typeColumn.Index == index;
        }

        /// <summary>
        /// Determines whether the specified index corresponds to the exemption code column.
        /// </summary>
        /// <param name="index">The index to test.</param>
        /// <returns><see langword="true"/> if <paramref name="index"/> corresponds to the 
        /// exemption code column; <see langword="false"/> if it does not.</returns>
        bool IsExemptionColumn(int index)
        {
            return _exemptionsColumn.Index == index;
        }

        /// <summary>
        /// Toggles the redacted state of the selected rows.
        /// </summary>
        public void ToggleRedactedState()
        {
            try
            {
                // Determine the resultant redacted state [FIDSC #3897]
                bool redacted = false;
                foreach (RedactionGridViewRow row in SelectedRows.Where(row => !row.ReadOnly))
                {
                    if (!row.Redacted)
                    {
                        redacted = true;
                        break;
                    }
                }

                // Display a warning if necessary and allow the user to cancel
                bool displayWarning = ShouldWarnAboutRedactedState(redacted);
                if (displayWarning && ShowRedactedStateWarning(redacted))
                {
                    return;
                }

                // Set the state
                foreach (DataGridViewRow row in _dataGridView.SelectedRows
                    .Cast<DataGridViewRow>()
                    .Where(row => !row.ReadOnly))
                {
                    RedactionGridViewRow redaction = _redactions[row.Index];
                    redaction.Redacted = redacted;
                    _dirty = true;
                    _dataGridView.UpdateCellValue(_redactedColumn.Index, row.Index);
                }

                // Handle special cases when the check box has input focus [FIDSC #3998]
                if (_dataGridView.IsCurrentCellInEditMode && IsRedactedColumnActive)
                {
                    if (displayWarning)
                    {
                        // If a warning was displayed, the currently edited cell 
                        // will not be in the correct state and needs be restored.
                        DataGridViewCheckBoxCell cell = 
                            (DataGridViewCheckBoxCell) _dataGridView.CurrentCell;
                        cell.EditingCellFormattedValue = redacted;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28631", ex);
            }
        }

        /// <summary>
        /// Displays a warning to the user if the user is setting the redacted state of a row that 
        /// is marked to display a warning.
        /// </summary>
        /// <param name="redacted"><see langword="true"/> if the row(s) are being set to redacted; 
        /// <see langword="false"/> if the row(s) are being set to unredacted.</param>
        /// <returns><see langword="true"/> if a warning was displayed and the user chose to 
        /// cancel; <see langword="false"/> if no warning needed to be displayed or if a warning 
        /// was displayed and the user chose to continue.</returns>
        bool WarnIfSettingRedactedState(bool redacted)
        {
            // Determine whether a warning should be displayed
            if (!ShouldWarnAboutRedactedState(redacted))
            {
                return false;
            }

            return ShowRedactedStateWarning(redacted);
        }

        /// <summary>
        /// Displays a warning to the user if the user is deleting a row that is marked to display 
        /// a warning.
        /// </summary>
        /// <returns><see langword="true"/> if a warning was displayed and the user chose to 
        /// cancel; <see langword="false"/> if no warning needed to be displayed or if a warning 
        /// was displayed and the user chose to continue.</returns>
        bool WarnIfDeletingRedaction()
        {
            // Determine whether a warning should be displayed
            if (!ShouldWarnAboutRedactedState(false))
            {
                return false;
            }

            // Display the warning
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete the selected item(s)?", "Delete redaction?",
                MessageBoxButtons.OKCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);

            return result == DialogResult.Cancel;
        }

        /// <summary>
        /// Determines whether a warning should be displayed when the redacted state of selected 
        /// rows are being changed.
        /// </summary>
        /// <param name="redacted"><see langword="true"/> if the row(s) are being set to redacted; 
        /// <see langword="false"/> if the row(s) are being set to unredacted.</param>
        /// <returns><see langword="true"/> if a warning should be shown; <see langword="false"/>
        /// if a warning should not be displayed.</returns>
        bool ShouldWarnAboutRedactedState(bool redacted)
        {
            bool warn = false;
            foreach (RedactionGridViewRow row in SelectedRows)
            {
                // Warn only if the redacted state has changed [FIDSC #3987]
                if (redacted)
                {
                    if (!row.Redacted && row.WarnIfRedacted)
                    {
                        warn = true;
                        break;
                    }
                }
                else
                {
                    if (row.Redacted && row.WarnIfNotRedacted)
                    {
                        warn = true;
                        break;
                    }
                }
            }

            return warn;
        }

        /// <summary>
        /// Displays a warning message that the redacted state of selected redactions are changing.
        /// </summary>
        /// <param name="redacted"><see langword="true"/> if the row(s) are being set to redacted; 
        /// <see langword="false"/> if the row(s) are being set to unredacted.</param>
        /// <returns><see langword="true"/> if the user chose to cancel; <see langword="false"/> 
        /// if the user chose to continue.</returns>
        static bool ShowRedactedStateWarning(bool redacted)
        {
            string state = redacted ? "redact" : "unredact";
            string message = "Are you sure you want to " + state + " the selected item(s)?";

            // Display the warning
            DialogResult result = MessageBox.Show(message, "Change redacted state?",
                MessageBoxButtons.OKCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);

            return result == DialogResult.Cancel;
        }

        /// <summary>
        /// Prompts the user to select exemption codes for the specified row.
        /// </summary>
        public void PromptForExemptions()
        {
            try
            {
                // Allow the user to select new exemption codes
                ExemptionsDialog.Exemptions = GetCommonSelectedExemptions();
                if (ExemptionsDialog.ShowDialog() == DialogResult.OK)
                {
                    // Apply the result to each selected redaction
                    ExemptionCodeList result = ExemptionsDialog.Exemptions;
                    ApplyExemptionsToSelected(result);

                    // Store the last applied exemption
                    _lastCodes = result;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26714", ex);
            }
        }

        /// <summary>
        /// Selects the layer objects on the image viewer that correspond to selected rows.
        /// </summary>
        void UpdateSelection()
        {
            // Short circuit if no redactions exist to update 
            if (_redactions.Count <= 0)
            {
                return;
            }

            // Prevent this method from calling itself
            // It may be better to de-activate here if active, but it's late in the 9.0 release
            // cycle, so I'm opting to keep code as close a possible to its previous state.
            bool wasActive = Active;
            if (wasActive)
            {
                _imageViewer.LayerObjects.Selection.LayerObjectAdded -= HandleSelectionLayerObjectAdded;
                _imageViewer.LayerObjects.Selection.LayerObjectDeleted -= HandleSelectionLayerObjectDeleted;
            }
            try
            {
                // Mark all selected rows as visited
                foreach (DataGridViewRow row in _dataGridView.SelectedRows)
                {
                    MarkAsVisited(row);
                }

                // Select only those layer objects that correspond to selected rows
                UpdateLayerObjectSelection();

                BringSelectionIntoView(false, false);

                _imageViewer.Invalidate();
            }
            finally
            {
                if (wasActive && Active)
                {
                    _imageViewer.LayerObjects.Selection.LayerObjectAdded += HandleSelectionLayerObjectAdded;
                    _imageViewer.LayerObjects.Selection.LayerObjectDeleted += HandleSelectionLayerObjectDeleted;
                }
            }
        }

        /// <summary>
        /// Centers the view on redactions selected on the current page; if there are no
        /// redactions selected on the current page, uses the first page with a selected redaction.
        /// </summary>
        /// <param name="forceAutoZoom"><see langword="true"/> to perform auto-zoom even if
        /// <see cref="_autoZoom"/> is <see langword="false"/>.</param>
        /// <param name="alwaysShiftZoom"><see langword="true"/> if the zoom should be adjusted
        /// regardless of whether redactions are already in view; <see langword="false"/> if the
        /// zoom should only be adjusted if in auto-zoom mode is on or if at least one redaction
        /// on the page is not fully visible.</param>
        public void BringSelectionIntoView(bool forceAutoZoom, bool alwaysShiftZoom)
        {
            try
            {
                // Is at least one row selected?
                if (_dataGridView.SelectedRows.Count > 0)
                {
                    // If no redaction is on the currently visible page, 
                    // go to the page of the first selected redaction.
                    LayerObject[] selected = GetSelectedLayerObjectsOnVisiblePage();
                    if (selected.Length == 0)
                    {
                        _imageViewer.PageNumber = GetFirstSelectedPage();

                        selected = GetSelectedLayerObjectsOnVisiblePage();
                    }

                    if (_autoZoom || forceAutoZoom)
                    {
                        // Auto-zoom is on, zoom around all layer objects on the current page.
                        PerformAutoZoom(selected);
                    }
                    else if (alwaysShiftZoom || ContainsNonVisibleLayerObject(selected))
                    {
                        _imageViewer.CenterOnLayerObjects(selected);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28857", ex);
            }
        }

        /// <summary>
        /// Determines if the specified layer objects contains at least one layer object that is 
        /// not within the visible view of the user.
        /// </summary>
        /// <param name="layerObjects">The layer objects to test for visibility.</param>
        /// <returns><see langword="true"/> if at least one of the <paramref name="layerObjects"/> 
        /// is outside the user's view; <see langword="false"/> if all the 
        /// <paramref name="layerObjects"/> are in view.</returns>
        bool ContainsNonVisibleLayerObject(IEnumerable<LayerObject> layerObjects)
        {
            Rectangle visibleArea = _imageViewer.GetVisibleImageArea();
            visibleArea = _imageViewer.GetTransformedRectangle(visibleArea, true);

            foreach (LayerObject layerObject in layerObjects)
            {
                if (!layerObject.IsVisible(visibleArea))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Selects layer objects corresponding to the currently selected rows; deselects all 
        /// other layer objects.
        /// </summary>
        void UpdateLayerObjectSelection()
        {
            // Get a collection of the ids of all the layer objects that should be selected
            List<long> selectedIds = new List<long>();
            foreach (RedactionGridViewRow row in SelectedRows)
            {
                foreach (LayerObject layerObject in row.LayerObjects)
                {
                    selectedIds.Add(layerObject.Id);
                }
            }

            // Select/deselect the layer objects corresponding to each selected row
            foreach (LayerObject layerObject in _imageViewer.LayerObjects)
            {
                bool shouldBeSelected =
                    layerObject.Selectable && selectedIds.Contains(layerObject.Id);
                if (layerObject.Selected != shouldBeSelected)
                {
                    layerObject.Selected = shouldBeSelected;
                }
            }
        }

        /// <summary>
        /// Determines whether any redaction is on the currently visible page.
        /// </summary>
        /// <returns><see langword="true"/> if there is a redaction on the currently visible page;
        /// <see langword="false"/> if there are no redactions or all redactions are on 
        /// non-visible pages.</returns>
        LayerObject[] GetSelectedLayerObjectsOnVisiblePage()
        {
            List<LayerObject> result = new List<LayerObject>();

            // Get the current page number
            int currentPage = _imageViewer.PageNumber;
            foreach (RedactionGridViewRow row in SelectedRows)
            {
                foreach (LayerObject layerObject in row.LayerObjects)
                {
                    if (layerObject.PageNumber == currentPage)
                    {
                        result.Add(layerObject);
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// The one-based first page on which selected redactions appear.
        /// </summary>
        /// <returns>The one-based first page on which selected redactions appear; or -1 if there 
        /// are no redactions.</returns>
        int GetFirstSelectedPage()
        {
            int firstPage = -1;
            foreach (RedactionGridViewRow row in SelectedRows)
            {
                if (firstPage > row.PageNumber || firstPage == -1)
                {
                    firstPage = row.PageNumber;
                }
            }

            return firstPage;
        }

        /// <summary>
        /// Zooms around the specified layer objects.
        /// </summary>
        void PerformAutoZoom(IEnumerable<LayerObject> layerObjects)
        {
            try
            {
                _performingAutoZoom = true;

                // Get combined area of all the selected layer objects on the current page
                Rectangle area = LayerObject.GetCombinedBounds(layerObjects);

                // Adjust the area by the auto zoom scale
                int padding = (_autoZoomScale - 1) * _PADDING_MULTIPLIER + _MIN_PADDING;
                area = _imageViewer.PadViewingRectangle(area, padding, padding, false);
                area = _imageViewer.GetTransformedRectangle(area, false);

                // Zoom to appropriate area
                _imageViewer.ZoomToRectangle(area);

                ZoomedToSelection = true;
            }
            finally
            {
                _performingAutoZoom = false;
            }
        }

        /// <summary>
        /// Visually marks the specified row as viewed.
        /// </summary>
        /// <param name="row">The row to mark as visited.</param>
        void MarkAsVisited(DataGridViewRow row)
        {
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.OwningColumn == _pageColumn)
                {
                    cell.Style = row.ReadOnly
                        ? VisitedReadOnlyPageCellStyle
                        : VisitedPageCellStyle;
                }
                else
                {
                    cell.Style = row.ReadOnly
                        ? VisitedReadOnlyCellStyle
                        : VisitedCellStyle;
                }
            }

            _redactions[row.Index].Visited = true;
        }

        /// <summary>
        /// Gets the exemptions codes that are common to all the selected redactions.
        /// </summary>
        /// <returns>The exemptions codes that are common to all the selected redactions.</returns>
        ExemptionCodeList GetCommonSelectedExemptions()
        {
            string category = null;
            List<string> codes = null;
            string otherText = null;
            foreach (RedactionGridViewRow row in SelectedRows)
            {
                // Skip empty exemptions
                if (!row.Exemptions.IsEmpty)
                {
                    // Store the common category
                    category = GetCommonText(category, row.Exemptions.Category);

                    // Store the common code
                    if (codes == null)
                    {
                        codes = new List<string>(row.Exemptions.Codes);
                    }
                    else
                    {
                        // Remove any codes that are not common to all
                        for (int i = 0; i < codes.Count; i++)
                        {
                            if (!row.Exemptions.HasCode(codes[i]))
                            {
                                codes.RemoveAt(i);
                                i--;
                            }
                        }
                    }

                    // Store the common other text
                    otherText = GetCommonText(otherText, row.Exemptions.OtherText);
                }
            }

            return new ExemptionCodeList(category, codes, otherText);
        }

        /// <summary>
        /// If the two strings are equal or <paramref name="common"/> is <see langword="null"/>, 
        /// then returns <paramref name="current"/>; otherwise returns the empty string.
        /// </summary>
        /// <param name="common">The text that all redactions have in common.</param>
        /// <param name="current">The text of a particular redaction.</param>
        /// <returns><paramref name="current"/> if <paramref name="common"/> is 
        /// <see langword="null"/> or equal to <paramref name="current"/>; returns the empty 
        /// string if <paramref name="common"/> is not <see langword="null"/> and 
        /// <paramref name="common"/> is not equal to <paramref name="current"/>.</returns>
        static string GetCommonText(string common, string current)
        {
            return (common == null || current == common) ? current : "";
        }

        /// <summary>
        /// Applies the specified exemption codes to all selected redactions.
        /// </summary>
        /// <param name="exemptions">The exemption codes to apply.</param>
        void ApplyExemptionsToSelected(ExemptionCodeList exemptions)
        {
            if (_dataGridView.SelectedRows.Count > 0)
            {
                _dirty = true;

                foreach (DataGridViewRow row in _dataGridView.SelectedRows
                    .Cast<DataGridViewRow>()
                    .Where(row => !row.ReadOnly))
                {
                    RedactionGridViewRow redaction = _redactions[row.Index];
                    redaction.Exemptions = exemptions;
                    _dataGridView.UpdateCellValue(_exemptionsColumn.Index, row.Index);

                    // Raise the ExemptionsApplied event
                    OnExemptionsApplied(new ExemptionsAppliedEventArgs(exemptions, redaction));
                }
            }
        }

        /// <summary>
        /// Applies the most recently applied exemption codes to all selected redactions.
        /// </summary>
        public void ApplyLastExemptions()
        {
            ApplyExemptionsToSelected(_lastCodes);
        }

        /// <summary>
        /// Loads the rows of the <see cref="RedactionGridView"/> based on the specified vector of 
        /// attributes file.
        /// </summary>
        /// <param name="file">A file containing a vector of attributes.</param>
        /// <param name="visitedRows">The rows to mark as visited; or <see langword="null"/> if 
        /// all rows should be marked as visited.</param>
        public void LoadFrom(RedactionFileLoader file, VisitedItemsCollection visitedRows)
        {
            try
            {
                // Disable handling of all events relating to changing data until the file has
                // finished loading.
                Active = false;

                // Reset the attributes
                _redactions.Clear();
                _imageViewer.LayerObjects.Clear();
                _deletedItems.Clear();
                for (int i = 0; i < _deletedCategoryCounts.Length; i++)
                {
                    _deletedCategoryCounts[i] = 0;
                }

                // Add attributes at each confidence level
                AddAttributesFromFile(file);

                // Sort the redactions spatially
                ArrayList adapter = ArrayList.Adapter(_redactions);
                adapter.Sort(new RedactionGridViewRowComparer());

                // If the row is read-only, apply the appropriate cell-style.
                for (int i = 0; i < _redactions.Count; i++)
                {
                    DataGridViewRow gridRow = _dataGridView.Rows[i];
                    gridRow.ReadOnly = _redactions[i].ReadOnly;
                    if (gridRow.ReadOnly)
                    {
                        foreach (DataGridViewCell cell in _dataGridView.Rows[i].Cells)
                        {
                            cell.Style = cell.OwningColumn == _pageColumn
                                ? ReadOnlyPageCellStyle
                                : ReadOnlyCellStyle;
                        }
                    }
                }

                // Set viewed rows
                if (visitedRows != null)
                {
                    DataGridViewRowCollection rows = _dataGridView.Rows;
                    foreach (int i in visitedRows)
                    {
                        if (i >= rows.Count)
                        {
                            break;
                        }

                        MarkAsVisited(rows[i]);
                    }
                }

                // Clear the selection
                _dataGridView.ClearSelection();

                // Invalidate the image viewer
                _imageViewer.Invalidate();

                // Re-enable handling of all events relating to changing data now that the file is
                // loaded.
                Active = true;

                _dirty = false;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26761",
                    "Unable to load VOA file.", ex);
                ee.AddDebugData("Voa file", file == null ? "null object" : file.FileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Adds attributes from the specified file to the <see cref="RedactionGridView"/>.
        /// </summary>
        /// <param name="file">The file containing the attributes to add.</param>
        void AddAttributesFromFile(RedactionFileLoader file)
        {
            // Iterate over the attributes
            foreach (SensitiveItem item in file.Items)
            {
                // Add each attribute
                RedactionGridViewRow row = 
                    RedactionGridViewRow.FromSensitiveItem(item, _imageViewer, MasterCodes);
                row.IsNew = false;

                Add(row);

                foreach (LayerObject layerObject in row.LayerObjects)
                {
                    layerObject.Movable = !row.ReadOnly;
                    _imageViewer.LayerObjects.Add(layerObject);
                }
            }
        }

        /// <summary>
        /// Saves the current uncommitted changes to the verification file.
        /// </summary>
        /// <param name="sourceDocument">The source document to use for changed attributes.</param>
        /// <returns>The changes saved to the verification file.</returns>
        public RedactionFileChanges SaveChanges(string sourceDocument)
        {
            try
            {
                List<RedactionItem> added = new List<RedactionItem>();
                List<RedactionItem> modified = new List<RedactionItem>();
                LongToObjectMap pageInfoMap = GetPageInfoMap();

                foreach (RedactionGridViewRow row in _redactions)
                {
                    if (row.IsNew)
                    {
                        added.Add(row.SaveRedactionItem(sourceDocument, pageInfoMap));
                    }
                    else if (row.IsModified)
                    {
                        modified.Add(row.SaveRedactionItem(sourceDocument, pageInfoMap));
                    }
                }

                return new RedactionFileChanges(added, _deletedItems, modified);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI28187",
                    "Unable to calculate file changes.", ex);
            }
        }

        /// <summary>
        /// Adds the specified type to the list of valid redaction types.
        /// </summary>
        /// <param name="type">The type to add.</param>
        public void AddRedactionType(string type)
        {
            try
            {
                _typeColumn.Items.Add(type);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27731",
                    "Unable to add redaction type.", ex);
            }
        }

        /// <summary>
        /// Adds the specified types to the list of valid redaction types.
        /// </summary>
        /// <param name="types">The list of types to add.</param>
        public void AddRedactionTypes(string[] types)
        {
            try
            {
                _typeColumn.Items.AddRange(types);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27119",
                    "Unable to add redaction types.", ex);
            }
        }

        /// <summary>
        /// Gets the page info map for the currently open image.
        /// </summary>
        /// <returns>The page info map for the currently open image.</returns>
        LongToObjectMap GetPageInfoMap()
        {
            // Iterate over each page of the image.
            LongToObjectMap pageInfoMap = new LongToObjectMap();
            for (int i = 1; i <= _imageViewer.PageCount; i++)
            {
                ImagePageProperties pageProperties = _imageViewer.GetPageProperties(i);

                // Create the spatial page info for this page
                SpatialPageInfo pageInfo = new SpatialPageInfo();
                int width = pageProperties.Width;
                int height = pageProperties.Height;
                pageInfo.SetPageInfo(width, height, EOrientation.kRotNone, 0);

                // Add it to the map
                pageInfoMap.Set(i, pageInfo);
            }

            return pageInfoMap;
        }

        /// <summary>
        /// Gets the counts of redactions for the current document.
        /// </summary>
        /// <returns>The counts of redaction for the current document.</returns>
        // This method is too computationally expensive to be a property.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public RedactionCounts GetRedactionCounts()
        {
            try
            {
                // Start with the deleted counts
                int[] counts = (int[])_deletedCategoryCounts.Clone();
                int total = 0;

                // Add counts for categories that still exist
                foreach (RedactionGridViewRow row in _redactions)
                {
                    int index = GetCategoryIndex(row);
                    if (index >= 0)
                    {
                        // Only count manual redactions if they are enabled
                        // [FlexIDSCore #4198]
                        if (index != (int)CategoryIndex.Manual || row.Redacted)
                        {
                            counts[index]++;
                        }

                        // Only add to total redaction count if the object is set
                        // to be redacted
                        if (row.Redacted)
                        {
                            total++;
                        }
                    }
                }

                // Get the counts by index
                int high = counts[(int)CategoryIndex.HCData];
                int medium = counts[(int)CategoryIndex.MCData];
                int low = counts[(int)CategoryIndex.LCData];
                int clues = counts[(int)CategoryIndex.Clues];
                int manual = counts[(int)CategoryIndex.Manual];

                return new RedactionCounts(high, medium, low, clues, manual, total);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27317",
                    "Unable to get counts for redaction statistics.", ex);
            }         
        }

        /// <summary>
        /// Selects or deselects the row corresponding the specified layer object.
        /// </summary>
        /// <param name="layerObject">The layer object contained by the row to select or deselect.
        /// </param>
        /// <param name="select"><see langword="true"/> if the row should be selected; 
        /// <see langword="false"/> if the row should be deselected.</param>
        void SelectRowContainingLayerObject(LayerObject layerObject, bool select)
        {
            // Prevent this method from calling itself
            _dataGridView.SelectionChanged -= HandleDataGridViewSelectionChanged;
            try
            {
                foreach (DataGridViewRow row in _dataGridView.Rows)
                {
                    if (_redactions[row.Index].ContainsLayerObject(layerObject))
                    {
                        // Change the selection if necessary
                        if (row.Selected != select)
                        {
                            MarkAsVisited(row);

                            row.Selected = select;
                        }

                        return;
                    }
                }

                // The layer object wasn't found. Complain.
                ExtractException ee = new ExtractException("ELI27062", "Layer object not found.");
                ee.AddDebugData("Id", layerObject.Id, false);
                throw ee;
            }
            finally
            {
                _dataGridView.SelectionChanged += HandleDataGridViewSelectionChanged;
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the character was processed by the control; otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                // Attempt to handle this as a shortcut key first 
                // unless the combo box is dropped down [FIDSC #4186]
                if (!IsComboBoxDroppedDown)
                {
                    // If the key press is the space bar then end editing (if it is active)
                    // in the check box column [FlexIDSCore #3998 & #4267]
                    if (keyData == Keys.Space &&
                        _dataGridView.IsCurrentCellInEditMode && IsRedactedColumnActive)
                    {
                        _dataGridView.EndEdit();
                    }

                    bool keyProcessed = _imageViewer.Shortcuts.ProcessKey(keyData);
                    if (keyProcessed)
                    {
                        // [FlexIDSCore:4917]
                        // They key up event will be fired in the grid event if this key is deemed
                        // processed. Suppress the next key up event in the grid.
                        SuppressNextKeyUp = true;

                        return true;
                    }
                }

                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29915", ex);
                return true;
            }
        }

        /// <summary>
        /// Gets the <see cref="CursorTool"/> that corresponds to the current 
        /// <see cref="AutoTool"/>.
        /// </summary>
        /// <returns>The <see cref="CursorTool"/> corresponding to the current 
        /// <see cref="AutoTool"/>.</returns>
        CursorTool GetAutoCursorTool()
        {
            switch (_autoTool)
            {
                case AutoTool.None:
                    return CursorTool.None;

                case AutoTool.Pan:
                    return CursorTool.Pan;

                case AutoTool.Selection:
                    return CursorTool.SelectLayerObject;

                case AutoTool.Zoom:
                    return CursorTool.ZoomWindow;

                default:
                    throw new ExtractException("ELI27436", "Unexpected auto tool.");
            }
        }

        /// <summary>
        /// Gets the index of the first unviewed row that occurs at or after the specified row.
        /// </summary>
        /// <param name="startIndex">The first index to check for being unviewed.</param>
        /// <returns>The index of the first unviewed row that occurs at or after 
        /// <paramref name="startIndex"/>; or -1 if no such row exists.</returns>
        public int GetNextUnviewedRowIndex(int startIndex)
        {
            try
            {
                // Iterate starting at the specified row
                for (int i = startIndex; i < _dataGridView.Rows.Count; i++)
                {
                    // Return to the first row that is unvisited
                    if (!_redactions[i].Visited)
                    {
                        return i;
                    }
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27645",
                    "Unable to determine next unviewed row.", ex);
            }
        }

        /// <summary>
        /// Determines the index of the row before the currently selected row.
        /// </summary>
        /// <returns>The index of the row before the currently selected row.</returns>
        // This is performing a calculation, so is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetPreviousRowIndex()
        {
            try
            {
                // Check if a row is selected
                int selectedRowIndex = GetFirstSelectedRowIndex();
                if (selectedRowIndex < 0)
                {
                    // No row is selected, return the index of
                    // the first redaction on a previous page
                    int currentPage = _imageViewer.PageNumber;
                    for (int i = _dataGridView.Rows.Count - 1; i >= 0; i--)
                    {
                        if (_redactions[i].PageNumber <= currentPage)
                        {
                            return i;
                        }
                    }
                }
                else if (selectedRowIndex > 0)
                {
                    // Return the previous row index, unless this is the first row of the grid
                    return selectedRowIndex - 1;
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27669",
                    "Unable to determine previous selected row.", ex);
            }
        }

        /// <summary>
        /// Determines the index of the row after the currently selected row.
        /// </summary>
        /// <returns>The index of the row after the currently selected row.</returns>
        // This is performing a calculation, so is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetNextRowIndex()
        {
            try
            {
                // Check if a row is selected
                int selectedRowIndex = GetLastSelectedRowIndex();
                if (selectedRowIndex < 0)
                {
                    // No row is selected, return the index of
                    // the first redaction on a subsequent page
                    int currentPage = _imageViewer.PageNumber;
                    for (int i = 0; i < _dataGridView.Rows.Count; i++)
                    {
                        if (_redactions[i].PageNumber >= currentPage)
                        {
                            return i;
                        }
                    }
                }
                else if (selectedRowIndex + 1 < _dataGridView.Rows.Count)
                {
                    // Return the next row index, unless this is the last row of the grid
                    return selectedRowIndex + 1;
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27642",
                    "Unable to determine next selected row.", ex);
            }
        }

        /// <summary>
        /// Gets the index of the first selected row.
        /// </summary>
        /// <returns>The index of the first selected row.</returns>
        // This is performing a calculation, so is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetFirstSelectedRowIndex()
        {
            try
            {
                if (_dataGridView.SelectedRows.Count > 0)
                {
                    return SelectedRowIndexes.Min();
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27643",
                    "Unable to determine first selected row.", ex);
            }
        }

        /// <summary>
        /// Gets the index of the last selected row.
        /// </summary>
        /// <returns>The index of the last selected row; or -1 if no row is selected.</returns>
        // This is performing a calculation, so is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetLastSelectedRowIndex()
        {
            try
            {
                if (_dataGridView.SelectedRows.Count > 0)
                {
                    return SelectedRowIndexes.Max();
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27644",
                    "Unable to determine last selected row.", ex);
            }
        }

        /// <summary>
        /// Gets the currently selected row indicies.
        /// </summary>
        public IEnumerable<int> SelectedRowIndexes
        {
            get
            {
                try
                {
                    return _dataGridView.SelectedRows
                                .Cast<DataGridViewRow>()
                                .Select(row => row.Index);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI32383");
                }
            }
        }

        /// <overloads>
        /// Selects the specified row and deselects all other rows.
        /// </overloads>
        /// <summary>
        /// Selects the specified row by index and deselects all other rows.
        /// </summary>
        /// <param name="index">The index of the row to select.</param>
        public void SelectOnly(int index)
        {
            try
            {
                _dataGridView.ClearSelection();
                _dataGridView.CurrentCell = _dataGridView.Rows[index].Cells[0];

                if (!_dataGridView.CurrentCell.Selected)
                {
                    _dataGridView.CurrentCell.Selected = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27659",
                    "Unable to select row.", ex);
                ee.AddDebugData("Row index", index, false);
                throw ee;
            }
        }

        /// <summary>
        /// Selects the specified indicies.
        /// </summary>
        /// <param name="indexes">The indicies.</param>
        public void Select(IEnumerable<int> indexes)
        {
            try
            {
                _dataGridView.ClearSelection();
                foreach (int index in indexes)
                {
                    _dataGridView.Rows[index].Selected = true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32384");
            }
        }

        /// <summary>
        /// Selects the specified row by <see cref="RedactionGridViewRow"/> and deselects all other
        /// rows.
        /// </summary>
        /// <param name="row">The <see cref="RedactionGridViewRow"/> in the row to select.</param>
        public void SelectOnly(RedactionGridViewRow row)
        {
            try
            {
                SelectOnly(_redactions.IndexOf(row));
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29907", ex);
            }
        }

        /// <summary>
        /// Cancels the selection of the currently selected rows.
        /// </summary>
        public void ClearSelection()
        {
            try
            {
                _dataGridView.ClearSelection();
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27658",
                    "Unable to clear selection.", ex);
            }
        }

        /// <summary>
        /// Drops down the redaction type list for the currently selected row.
        /// </summary>
        void SelectDropDownTypeList()
        {
            // Check if only one row has input focus
            DataGridViewSelectedRowCollection rows = _dataGridView.SelectedRows;
            if (rows.Count == 1 && !rows[0].ReadOnly)
            {
                // Select the type column
                _dataGridView.CurrentCell = rows[0].Cells[_typeColumn.Index];

                // Drop down the combo box menu
                _dataGridView.BeginEdit(true);
                DataGridViewComboBoxEditingControl control =
                    (DataGridViewComboBoxEditingControl)_dataGridView.EditingControl;
                control.DroppedDown = true;
            }
        }

        /// <summary>
        /// Creates a collection representing the rows that have been visited.
        /// </summary>
        /// <returns>A collection representing the rows that have been visited.</returns>
        // Complex operations are better suited as methods.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public VisitedItemsCollection GetVisitedRows()
        {
            try
            {
                // Sort a copy of the grid rows
                RedactionGridViewRow[] rows = new RedactionGridViewRow[_redactions.Count];
                _redactions.CopyTo(rows, 0);
                Array.Sort(rows, new RedactionGridViewRowComparer());

                // Construct a visited rows collection from the sorted rows
                VisitedItemsCollection items = new VisitedItemsCollection(rows.Length);
                for (int i = 0; i < rows.Length; i++)
                {
                    if (rows[i].Visited)
                    {
                        items[i] = true;
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27736",
                    "Unable to determine visited rows.", ex);
            }
        }

        /// <summary>
        /// Commits any pending user changes.
        /// </summary>
        public void CommitChanges()
        {
            try
            {
                _dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27741",
                    "Unable to commit user changes.", ex);
            }
        }

        /// <summary>
        /// Clears all data from the grid
        /// </summary>
        public void Clear()
        {
            try
            {
                Active = false;

                // Remove all rows
                _dataGridView.Rows.Clear();

                // Clear the data
                _redactions.Clear();
                _deletedItems.Clear();
                for (int i = 0; i < _deletedCategoryCounts.Length; i++)
                {
                    _deletedCategoryCounts[i] = 0;
                }

                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32609");
            }
            finally
            {
                Active = true;
            }
        }

        #endregion Methods

        #region OnEvents

        /// <summary>
        /// Raises the <see cref="ExemptionsApplied"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="ExemptionsApplied"/> 
        /// event.</param>
        void OnExemptionsApplied(ExemptionsAppliedEventArgs e)
        {
            if (ExemptionsApplied != null)
            {
                ExemptionsApplied(this, e);
            }
        }

        #endregion OnEvents

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                _dataGridView.Enabled = _imageViewer.IsImageAvailable;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26673", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Imaging.Forms.ImageViewer.ImageFileClosing"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Imaging.Forms.ImageViewer.ImageFileClosing"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Imaging.Forms.ImageViewer.ImageFileClosing"/> event.</param>
        void HandleImageFileClosing(object sender, ImageFileClosingEventArgs e)
        {
            try
            {
                // Until the next file is opened disable handling of all events relating to
                // changing data.
                Active = false;

                // Deselect layer objects before image closes [FIDSC #4002]
                _imageViewer.LayerObjects.Selection.Clear();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29532", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.DeletingLayerObjects"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.DeletingLayerObjects"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.DeletingLayerObjects"/> event.</param>
        void HandleDeletingLayerObjects(object sender, DeletingLayerObjectsEventArgs e)
        {
            try
            {
                // Only warn if deleting redactions [FIDSC #4002]
                bool deletingRedactions = false;
                foreach (LayerObject layerObject in e.LayerObjects)
                {
                    if (layerObject is RedactionLayerObject)
                    {
                        deletingRedactions = true;
                        break;
                    }
                }

                if (deletingRedactions && WarnIfDeletingRedaction())
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29443", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        void HandleLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                RedactionLayerObject redaction = e.LayerObject as RedactionLayerObject;
                if (redaction != null)
                {
                    string strText;
                    if (e.LayerObject.Tags.Contains(VerificationRuleFormHelper.RedactedMatchTag))
                    {
                        strText = e.LayerObject.Comment;
                    }
                    else
                    {
                        strText = "[No text]";
                    }

                    int addedRowIndex =
                        Add(e.LayerObject, strText, "Manual", _lastType, _confidenceLevels.Manual);

                    redaction.BorderColor = _confidenceLevels.Manual.Color;
                    redaction.Color = ToggledRedactionColor;

                    if (_autoTool != AutoTool.None)
                    {
                        _imageViewer.CursorTool = GetAutoCursorTool();
                    }

                    // Auto select the new layer object [FlexIDSCore #4206]
                    SelectOnly(addedRowIndex);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26677", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        void HandleLayerObjectDeleted(object sender, LayerObjectDeletedEventArgs e)
        {
            try
            {
                if (e.LayerObject is RedactionLayerObject)
                {
                    Remove(e.LayerObject);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26678", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.</param>
        void HandleLayerObjectChanged(object sender, LayerObjectChangedEventArgs e)
        {
            try
            {
                // Find the row that contains the layer object and set it to dirty.
                foreach (RedactionGridViewRow row in _redactions)
                {
                    if (row.ContainsLayerObject(e.LayerObject))
                    {
                        row.LayerObjectsDirty = true;

                        _dirty = true;

                        return;
                    }
                }

                // Intentionlly removed assertion that the layer object was found. A sub-layer
                // object of a CompositeLayerObject may have been modified. In that case the ID will
                // not be a direct member of any of the rows. However, the parent
                // CompositeLayerObject will still get flagged as dirty allowing
                // row.LayerObjectsDirty to be set.
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26951", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        void HandleSelectionLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                // Select the row containing the layer object
                SelectRowContainingLayerObject(e.LayerObject, true);

                // Ensure the top row in the selection is visible
                if (_dataGridView.SelectedRows.Count > 0)
                {
                    _dataGridView.FirstDisplayedScrollingRowIndex = GetFirstSelectedRowIndex();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27061", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        void HandleSelectionLayerObjectDeleted(object sender, LayerObjectDeletedEventArgs e)
        {
            try
            {
                // Deselect the row containing the layer object
                SelectRowContainingLayerObject(e.LayerObject, false);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27065", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.SelectionChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="DataGridView.SelectionChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="DataGridView.SelectionChanged"/> event.</param>
        void HandleDataGridViewSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateSelection();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27064", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellDoubleClick"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="DataGridView.CellDoubleClick"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="DataGridView.CellDoubleClick"/> event.</param>
        void HandleDataGridViewCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Check if an exemption codes cell was clicked and it is not read-only
                if (IsExemptionColumn(e.ColumnIndex) && e.RowIndex >= 0 &&
                    !_redactions[e.RowIndex].ReadOnly)
                {
                    PromptForExemptions();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26709", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellValueChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="DataGridView.CellValueChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="DataGridView.CellValueChanged"/> event.</param>
        void HandleDataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Check if the type column changed
                if (e.RowIndex >= 0)
                {
                    if (IsTypeColumn(e.ColumnIndex))
                    {
                        _dirty = true;

                        _lastType = (string)_dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    }
                    else if (IsRedactedColumn(e.ColumnIndex))
                    {
                        _dirty = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27725", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellContentClick"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="DataGridView.CellContentClick"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="DataGridView.CellContentClick"/> event.</param>
        void HandleDataGridViewCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && IsRedactedColumn(e.ColumnIndex) &&
                    !_redactions[e.RowIndex].ReadOnly)
                {
                    // Warn if necessary about the redacted state
                    bool redacted = Rows[e.RowIndex].Redacted;
                    if (WarnIfSettingRedactedState(!redacted))
                    {
                        // Restore the checkbox to its initial state
                        DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell) 
                                                        _dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                        cell.EditingCellFormattedValue = redacted;
                    }
                    else
                    {
                        // Commit the changes
                        _dirty = true;

                        CommitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28800", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer"/> ZoomChanged event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.ZoomChangedEventArgs"/> instance
        /// containing the event data.</param>
        void HandleZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            try
            {
                // If not in an auto-zoom operation, set the new zoom level as the last
                // non-selection zoom.
                if (!_performingAutoZoom && _imageViewer.ZoomHistoryCount > 0)
                {
                    ZoomedToSelection = false;
                    LastNonSelectionZoomInfo = _imageViewer.ZoomInfo;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33671");
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer"/> PageChanged event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.PageChangedEventArgs"/> instance
        /// containing the event data.</param>
        void HandlePageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                // Upon loading a new page, there is no LastNonSelectionZoomInfo for the current
                // page.
                LastNonSelectionZoomInfo = null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33680");
            }
        }

        /// <summary>
        /// Handles the KeyUp event from the _dataGridView in order to allow it to be suppressed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/>
        /// instance containing the event data.</param>
        void HandleGridKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (SuppressNextKeyUp)
                {
                    e.SuppressKeyPress = true;
                }

                SuppressNextKeyUp = false;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34073");
            }
        }

        #endregion Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="RedactionGridView"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="RedactionGridView"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="RedactionGridView"/> is 
        /// connected. <see langword="null"/> if no connections are established.</returns>
        [Browsable(false)]
        public ImageViewer ImageViewer
        {
            get
            {
                return _imageViewer;
            }
            set
            {
                try
                {
                    // Unregister from previously subscribed-to events
                    if (_imageViewer != null)
                    {
                        Active = false;
                        _imageViewer.ImageFileChanged -= HandleImageFileChanged;
                        _imageViewer.ImageFileClosing -= HandleImageFileClosing;
                        _imageViewer.PageChanged -= HandlePageChanged;
                        _imageViewer.ZoomChanged -= HandleZoomChanged;
                        _imageViewer.Shortcuts[Keys.T] = null;
                    }

                    // Store the new image viewer
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;
                        _imageViewer.ImageFileClosing += HandleImageFileClosing;
                        _imageViewer.PageChanged += HandlePageChanged;
                        _imageViewer.ZoomChanged += HandleZoomChanged;
                        _imageViewer.Shortcuts[Keys.T] = SelectDropDownTypeList;
                    }
                }
                catch (Exception ex)
                {
                    throw new ExtractException("ELI26672", 
                        "Unable to establish connection to image viewer.", ex);
                }
            }
        }

        #endregion IImageViewerControl Members

        #region Internal Members

        /// <summary>
        /// Gets whether the <see cref="ImageViewer"/> is zoomed to the current selection in
        /// grid.
        /// </summary>
        internal bool ZoomedToSelection
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the last zoom info for the current page that is not a zoom to an item that had been
        /// selected in the grid.
        /// </summary>
        internal ZoomInfo? LastNonSelectionZoomInfo
        {
            get;
            private set;
        }

        #endregion Internal Members

        #region Private Members

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RedactionGridView"/> is actively
        /// tracking data.
        /// </summary>
        /// <value><see langword="true"/> if active; otherwise, <see langword="false"/>.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        bool Active
        {
            get
            {
                return _active;
            }

            set
            {
                if (value != _active)
                {
                    _active = value;

                    if (_active)
                    {
                        ExtractException.Assert("ELI32394",
                            "Redaction grid cannot be activated without an ImageViewer.",
                            _imageViewer != null);

                        _imageViewer.LayerObjects.DeletingLayerObjects += HandleDeletingLayerObjects;
                        _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;
                        _imageViewer.LayerObjects.LayerObjectDeleted += HandleLayerObjectDeleted;
                        _imageViewer.LayerObjects.LayerObjectChanged += HandleLayerObjectChanged;
                        _imageViewer.LayerObjects.Selection.LayerObjectAdded += HandleSelectionLayerObjectAdded;
                        _imageViewer.LayerObjects.Selection.LayerObjectDeleted += HandleSelectionLayerObjectDeleted;
                        _dataGridView.SelectionChanged += HandleDataGridViewSelectionChanged;
                    }
                    else if (_imageViewer != null)
                    {
                        _imageViewer.LayerObjects.DeletingLayerObjects -= HandleDeletingLayerObjects;
                        _imageViewer.LayerObjects.LayerObjectAdded -= HandleLayerObjectAdded;
                        _imageViewer.LayerObjects.LayerObjectDeleted -= HandleLayerObjectDeleted;
                        _imageViewer.LayerObjects.LayerObjectChanged -= HandleLayerObjectChanged;
                        _imageViewer.LayerObjects.Selection.LayerObjectAdded -= HandleSelectionLayerObjectAdded;
                        _imageViewer.LayerObjects.Selection.LayerObjectDeleted -= HandleSelectionLayerObjectDeleted;
                        _dataGridView.SelectionChanged -= HandleDataGridViewSelectionChanged;
                    }
                }
            }
        }

        #endregion Private Members
    }

    /// <summary>
    /// Provides data for the <see cref="RedactionGridView.ExemptionsApplied"/> event.
    /// </summary>
    public class ExemptionsAppliedEventArgs : EventArgs
    {
        /// <summary>
        /// The exemption codes that were applied.
        /// </summary>
        readonly ExemptionCodeList _exemptions;

        /// <summary>
        /// The row to which the exemptions were applied.
        /// </summary>
        readonly RedactionGridViewRow _row;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExemptionsAppliedEventArgs"/> class.
        /// </summary>
        /// <param name="exemptions">The exemption codes that were applied.</param>
        /// <param name="row">The row to which the exemptions were applied.</param>
        public ExemptionsAppliedEventArgs(ExemptionCodeList exemptions, RedactionGridViewRow row)
        {
            _exemptions = exemptions;
            _row = row;
        }

        /// <summary>
        /// Gets the exemption codes that were applied.
        /// </summary>
        /// <returns>The exemption codes that were applied.</returns>
        public ExemptionCodeList Exemptions
        {
            get
            {
                return _exemptions;
            }
        }

        /// <summary>
        /// Gets the row to which the exemptions were applied.
        /// </summary>
        /// <returns>The row to which the exemptions were applied.</returns>
        public RedactionGridViewRow Row
        {
            get
            {
                return _row;
            }
        }
    }
}
