using Extract.AttributeFinder;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Utilities.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;
using EOrientation = UCLID_RASTERANDOCRMGMTLib.EOrientation;
using RedactionLayerObject = Extract.Imaging.Forms.Redaction;
using SpatialPageInfo = UCLID_RASTERANDOCRMGMTLib.SpatialPageInfo;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a <see cref="DataGridView"/> that displays information about redactions.
    /// </summary>
    [CLSCompliant(false)]
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
        /// RedactionType that represents a full page clue
        /// </summary>
        static readonly string _FULL_PAGE_CLUE_TYPE = "FullPageClue";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The color that toggled on redactions are drawn on the image viewer.
        /// </summary>
        public static readonly Color DefaultRedactionFillColor = Color.CornflowerBlue;

        /// <summary>
        /// The color that toggled on highlights are drawn on the image viewer.
        /// </summary>
        public static readonly Color DefaultHighlightFillColor = Color.Yellow;

        /// <summary>
        /// The <see cref="ImageViewer"/> with which the <see cref="RedactionGridView"/> is 
        /// associated.
        /// </summary>
        IDocumentViewer _imageViewer;

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
        int _autoZoomScale = Imaging.Forms.ImageViewer.DefaultAutoZoomScale;

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
        /// Indicates whether the grid is actively tracking data.
        /// </summary>
        bool _active;
        
        /// <summary>
        /// If non-zero, indicates that changes in selection in <see cref="_redactions"/> should not
        /// be acted upon.
        /// </summary>
        int _preventSelectionRefCount;

        /// <summary>
        /// If non-zero, indicates that the current view should not be changed.
        /// </summary>
        int _preventViewChangeRefCount;

        /// <summary>
        /// Keeps track of the last rows selected before _preventSelectionRefCount went into affect.
        /// </summary>
        List<DataGridViewRow> _lastSelectedRows;

        /// <summary>
        /// Caches the page info map from initial call to <see cref="GetPageInfoMap"/> for the
        /// current image.
        /// </summary>
        LongToObjectMap _pageInfoMap;

        /// <summary>
        /// Task to represent the background collection of page info. Used to prefetch this data
        /// so that creating the first manual redaction on very large images will not cause the UI to freeze
        /// </summary>
        Task<List<(int page, int width, int height)>> _getPageInfoTask;

        /// <summary>
        /// Func that will collect the page info for each page of the image
        /// </summary>
        Func<List<(int page, int width, int height)>> _getPageInfo;

        /// <summary>
        /// The source document for the redactions
        /// </summary>
        string _sourceDocument;

        /// <summary>
        /// Indicates when redaction type column has been clicked by indicating the row in which it
        /// has been clicked; -1 if there is no active click of the redaction type column.
        /// </summary>
        int _typeComboClickedRow = -1;

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

            _getPageInfo = () =>
                // Iterate over each page of the image.
                Enumerable.Range(1, _imageViewer.PageCount)
                .Select(i =>
                {
                    if (_imageViewer.SpatialPageInfos?.Contains(i) ?? false)
                    {
                        var pageinfo = (SpatialPageInfo)_imageViewer.SpatialPageInfos.GetValue(i);
                        return (page: i, width: pageinfo.Width, height: pageinfo.Height);
                    }
                    else
                    {
                        ImagePageProperties pageProperties = _imageViewer.GetPageProperties(i);
                        return (page: i, width: pageProperties.Width, height: pageProperties.Height);
                    }
                })
                .ToList();
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
                try
                {
                    if (value != _dirty)
                    {
                        if (value == false)
                        {
                            // [FlexIDSCore:4747]
                            // If marking the grid as not dirty, also clear the running list of deleted
                            // items so they don't get reported on with subsequent saves.
                            _deletedItems.Clear();
                        }

                        _dirty = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34317");
                }
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
                try
                {
                    _autoZoomScale = value;
                    if (_imageViewer != null)
                    {
                        _imageViewer.AutoZoomScale = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34980");
                }
            }
        }

        /// <summary>
        /// Determines whether the <see cref="AutoZoomScale"/> property should be serialized.
        /// </summary>
        /// <returns><see langword="true"/> if the property should be serialized; 
        /// <see langword="false"/> if the property should not be serialized.</returns>
        bool ShouldSerializeAutoZoomScale()
        {
            return _autoZoom && _autoZoomScale != Imaging.Forms.ImageViewer.DefaultAutoZoomScale;
        }

        /// <summary>
        /// Resets the <see cref="AutoZoomScale"/> property to its default value.
        /// </summary>
        void ResetAutoZoomScale()
        {
            _autoZoomScale = Imaging.Forms.ImageViewer.DefaultAutoZoomScale;
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
        /// <value><see langword="true"/> to suppress the next key up event; otherwise,
        /// <see langword="false"/>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal bool SuppressNextKeyUp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the last viewed item page number
        /// </summary>
        /// <param name="index">index of last visited sensitive item</param> 
        /// <returns>Page numberof item</returns>
        public int GetLastViewedItemPageNumber(int index)
        {
            try
            {
                if (0 == index)
                {
                    // Nothing was viewed, avoid throwing an exception.
                    return 0;
                }

                ExtractException.Assert("ELI39873",
                                        String.Format(CultureInfo.InvariantCulture,
                                                      "Index value: {0}, is out-of-range, maximum: {1}",
                                                      index,
                                                      _dataGridView.Rows.Count),
                                        index < _dataGridView.Rows.Count);

                return _redactions[index].PageNumber;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39871");
            }
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
                var row = new RedactionGridViewRow(layerObject, text, category, type, confidenceLevel);
                row.TypeChanged += HandleRowTypeChanged;

                return Add(row);
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
                row.SaveRedactionItem(_sourceDocument, pageInfoMap);
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
                        row.TypeChanged -= HandleRowTypeChanged;

                        // Store the currently selected rows so selection of any non-deleted rows
                        // can be restored after the deletion.
                        List<DataGridViewRow> originallySelectedRows =
                            new List<DataGridViewRow>(_dataGridView.SelectedRows.Cast<DataGridViewRow>());

                        if (row.LayerObjects.Count == 0)
                        {
                            AddToDeletedCount(row);

                            try
                            {
                                // [FlexIDSCore:4989]
                                // A new item will be automatically selected in _redactions after
                                // deleting the specified one. Prevent handling of this selection
                                // change to prevent causing the document page to change.
                                _preventSelectionRefCount++;

                                _redactions.RemoveAt(i);
                            }
                            finally
                            {
                                _preventSelectionRefCount--;
                            }
                        }

                        // Set selection to any rows from the previous selection that still exist.
                        // (It is probably not possible for any rows to be in this set, but just in
                        // case...)
                        Select(originallySelectedRows
                            .Where(selectedRow => _dataGridView.Rows.Contains(selectedRow))
                            .Select(selectedRow => selectedRow.Index)
                            , false);

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
            // It may be better to deactivate here if active, but it's late in the 9.0 release
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

                BringSelectionIntoView();

                // https://extract.atlassian.net/browse/ISSUE-14931
                // If this new selection is a single row that was selected by clicking on the type
                // column, open the type dropdown.
                if (_dataGridView.SelectedRows.Count == 1 &&
                    _dataGridView.SelectedRows[0].Index == _typeComboClickedRow)
                {
                    _typeComboClickedRow = -1;
                    SelectDropDownTypeList();
                }

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
        public void BringSelectionIntoView()
        {
            try
            {
                // Is at least one row selected?
                if (_dataGridView.SelectedRows.Count > 0 && _preventViewChangeRefCount == 0)
                {
                    _imageViewer.BringSelectionIntoView(_autoZoom);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28857", ex);
            }
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
        /// Updates the visited rows in the data grid.
        /// </summary>
        /// <param name="rowIndexes">The list of row indexes.</param>
        public void UpdateVisitedRows(IEnumerable<int> rowIndexes)
        {
            try
            {
                foreach (int index in rowIndexes)
                {
                    DataGridViewRow row = _dataGridView.Rows[index];
                    MarkAsVisited(row);
                }

                UpdateSelection();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39844");
            }
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
                _sourceDocument = file.SourceDocument;

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
                if (item.Level.Highlight)
                {
                    var highlights = _imageViewer.CreateHighlights(item.Attribute.SpatialString,
                        item.Level.FillColor.HasValue
                            ? item.Level.FillColor.Value
                            : DefaultHighlightFillColor);

                    foreach (var highlight in highlights)
                    {
                        highlight.Selectable = false;
                        _imageViewer.LayerObjects.Add(highlight);
                    }
                }
                else
                {
                    // Add each attribute
                    RedactionGridViewRow row =
                        RedactionGridViewRow.FromSensitiveItem(item, _imageViewer, MasterCodes);
                    row.IsNew = false;
                    row.TypeChanged += HandleRowTypeChanged;

                    Add(row);

                    foreach (LayerObject layerObject in row.LayerObjects)
                    {
                        layerObject.Movable = !row.ReadOnly;
                        _imageViewer.LayerObjects.Add(layerObject);
                    }
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
            // [FlexIDSCore:4996]
            // Some situations end up calling into this method many times and this is an expensive
            // operation. Cache the map for future use.
            if (_pageInfoMap == null)
            {
                using (new TemporaryWaitCursor())
                {
                    List<(int page, int width, int height)> result = null;

                    // If the task was not started for some reason then just run the action directly
                    if (_getPageInfoTask == null || _getPageInfoTask.Status == TaskStatus.WaitingToRun)
                    {
                        result = _getPageInfo();
                    }
                    else
                    {
                        _getPageInfoTask.Wait();

                        if (_getPageInfoTask.IsFaulted)
                        {
                            var ag = _getPageInfoTask.Exception as AggregateException;
                            var ue = (ag?.InnerException ?? _getPageInfoTask.Exception).AsExtract("ELI43421");
                            throw ue;
                        }

                        result = _getPageInfoTask.Result;
                    }

                    var pageInfoMap = new LongToObjectMap();
                    foreach ((int page, int width, int height) in result)
                    {
                        // Create the spatial page info for this page
                        SpatialPageInfo pageInfo = new SpatialPageInfo();
                        pageInfo.Initialize(width, height, EOrientation.kRotNone, 0);

                        // Add it to the map
                        pageInfoMap.Set(page, pageInfo);
                    }
                    _pageInfoMap = pageInfoMap;
                }
            }

            return _pageInfoMap;
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
        /// Gets the index of the first unviewed row with a full page clue that occurs at or after the specified row.
        /// </summary>
        /// <param name="startIndex">The first index to check for being unviewed.</param>
        /// <returns>The index of the first unviewed row with a full page clue that occurs at or after 
        /// <paramref name="startIndex"/>; or -1 if no such row exists.</returns>
        public int GetNextUnviewedFullPageClueRowIndex(int startIndex)
        {
            try
            {
                for (int i = startIndex; i < _dataGridView.Rows.Count; i++)
                {
                    if (!_redactions[i].Visited && _redactions[i].RedactionType == _FULL_PAGE_CLUE_TYPE)
                    {
                        return i;
                    }
                }
                return -1;

            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI46170",
                    "Unable to determine next unviewed full page clue", ex);
            }
        }

        /// <summary>
        /// Determines the index of the row before the currently selected row.
        /// </summary>
        /// <returns>The index of the row before the currently selected row.</returns>
        // This is performing a calculation, so is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetPreviousRowIndex(bool fullPageClue = false)
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
                        if (_redactions[i].PageNumber <= currentPage
                            && (!fullPageClue || _redactions[i].RedactionType == _FULL_PAGE_CLUE_TYPE))
                        {
                            return i;
                        }
                    }
                }
                else if (selectedRowIndex > 0)
                {
                    // Return the previous row index, unless this is the first row of the grid
                    if (fullPageClue)
                    {
                        for (int i = selectedRowIndex - 1; i >= 0; i--)
                        {
                            if (_redactions[i].RedactionType == _FULL_PAGE_CLUE_TYPE)
                            {
                                return i;
                            }
                        }
                    }
                    else
                    {
                        return selectedRowIndex - 1;
                    }
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
        public int GetNextRowIndex(bool fullPageClue = false)
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
                        if (_redactions[i].PageNumber >= currentPage
                            && (!fullPageClue || _redactions[i].RedactionType == _FULL_PAGE_CLUE_TYPE))
                        {
                            return i;
                        }
                    }
                }
                else if (selectedRowIndex + 1 < _dataGridView.Rows.Count)
                {
                    if(fullPageClue)
                    {
                        for (int i = selectedRowIndex + 1; i <_dataGridView.Rows.Count; i++)
                        {
                            if (_redactions[i].RedactionType == _FULL_PAGE_CLUE_TYPE)
                            {
                                return i;
                            }
                        }
                    }
                    else
                    {
                        // Return the next row index, unless this is the last row of the grid
                        return selectedRowIndex + 1;
                    }
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
        /// Gets the currently selected row indices.
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
        /// Selects the specified indices.
        /// </summary>
        /// <param name="indexes">The indices.</param>
        /// <param name="updateZoom"><see langword="true"/> to zoom to the new selection per
        /// auto-zoom settings; <see langword="false"/> to not disturb the current view in the
        /// image viewer.</param>
        public void Select(IEnumerable<int> indexes, bool updateZoom)
        {
            try
            {
                if (!updateZoom)
                {
                    _preventViewChangeRefCount++;
                }

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
            finally
            {
                if (!updateZoom)
                {
                    _preventViewChangeRefCount--;
                }
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

        /// <summary>
        /// Prevents changes in the grids data from changing selection until
        /// <see cref="AllowSelection"/> is called.
        /// </summary>
        internal void PreventSelection()
        {
            try
            {
                // Keep track of the selection to restore after selection is re-allowed.
                if (_preventSelectionRefCount == 0)
                {
                    _lastSelectedRows = new List<DataGridViewRow>(
                        _dataGridView.SelectedRows.Cast<DataGridViewRow>());
                }

                _preventSelectionRefCount++;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34353");
            }
        }

        /// <summary>
        /// Re-allows automatic selection based on changes in the grid after a previous call to
        /// <see cref="PreventSelection"/>.
        /// <para><b>Note</b></para>
        /// <see cref="PreventSelection"/> and <see cref="AllowSelection"/> should be always be
        /// called in pairs. If one pair of this calls is nested inside another pair, selection
        /// would still be prevented until the outer-scopes call to <see cref="AllowSelection"/>.
        /// </summary>
        internal void AllowSelection()
        {
            try
            {
                _preventSelectionRefCount--;

                // Restore selection to the same rows that had it before selection was prevented.
                if (_preventSelectionRefCount == 0 && _lastSelectedRows != null)
                {
                    Select(_lastSelectedRows
                        .Where(row => _dataGridView.Rows.Contains(row))
                        .Select(row => row.Index),
                        false);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34352");
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
        /// Handles the <see cref="Extract.Imaging.Forms.IDocumentViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.IDocumentViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.IDocumentViewer.ImageFileChanged"/> event.</param>
        void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                _pageInfoMap = null;

                if (_imageViewer.IsImageAvailable)
                {
                    _getPageInfoTask = Task.Run(_getPageInfo);
                    _dataGridView.Enabled = true;
                }
                else
                {
                    _dataGridView.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26673", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Imaging.Forms.IDocumentViewer.ImageFileClosing"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Imaging.Forms.IDocumentViewer.ImageFileClosing"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Imaging.Forms.IDocumentViewer.ImageFileClosing"/> event.</param>
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

                    // https://extract.atlassian.net/browse/ISSUE-13365
                    // Find the appropriate confidence level to use for the new attribute.
                    var confidenceLevel = GetConfidenceLevel("Manual", _lastType, _confidenceLevels.Manual);
                    int addedRowIndex = Add(e.LayerObject, strText, "Manual", _lastType, confidenceLevel);

                    // Note: The new row will not honor ReadOnly or Highlight properties in this session;
                    // Only after re-loading from disk would either of those properties be honored.
                    redaction.BorderColor = confidenceLevel.Color;
                    redaction.Color = confidenceLevel.FillColor.HasValue
                        ? confidenceLevel.FillColor.Value
                        : RedactionGridView.DefaultRedactionFillColor;

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

                // Intentionally removed assertion that the layer object was found. A sub-layer
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
                if (e.LayerObject is RedactionLayerObject)
                {
                    // Select the row containing the layer object
                    SelectRowContainingLayerObject(e.LayerObject, true);

                    // Ensure the top row in the selection is visible
                    if (_dataGridView.SelectedRows.Count > 0)
                    {
                        _dataGridView.FirstDisplayedScrollingRowIndex = GetFirstSelectedRowIndex();
                    }
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
                if (e.LayerObject is RedactionLayerObject)
                {
                    // Deselect the row containing the layer object
                    SelectRowContainingLayerObject(e.LayerObject, false);
                }
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
                if (_preventSelectionRefCount == 0)
                {
                    UpdateSelection();
                }
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
        /// Manually raises the CellValueChanged event so that _lastType is set, even if focus doesn't change
        /// before a new attribute is added.
        /// https://extract.atlassian.net/browse/ISSUE-7363
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HandleDataGridViewCellDirtyStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (_dataGridView.IsCurrentCellDirty && IsTypeColumn(_dataGridView.CurrentCell.ColumnIndex))
                {
                    // This fires the cell value changed event below
                    _dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI38408", ex);
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

        /// <summary>
        /// Handles the <see cref="RedactionGridViewRow.TypeChanged"/> event for any row in the grid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRowTypeChanged(object sender, EventArgs e)
        {
            try
            {
                var row = (RedactionGridViewRow)sender;

                // https://extract.atlassian.net/browse/ISSUE-13365
                // Check to see if the new type means the row should be associated with a new confidence
                // level.
                var newConfidenceLevel = GetConfidenceLevel(
                    row.Category, row.RedactionType, _confidenceLevels.Manual);
                if (newConfidenceLevel.ShortName != row.ConfidenceLevel.ShortName)
                {
                    row.ConfidenceLevel = newConfidenceLevel;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39109");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellMouseDown"/> event of <see cref="_dataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellMouseEventArgs"/> instance containing the
        /// event data.</param>
        void HandleDataGridViewCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                // https://extract.atlassian.net/browse/ISSUE-14931
                // The redaction type combo should open with a single click; check if the redaction
                // type column is the one that was clicked.
                if (e.ColumnIndex == _typeColumn.Index)
                {
                    // If the clicked row is already the singly selected row, open the combo
                    if (_dataGridView.SelectedRows.Count == 1 &&
                        _dataGridView.SelectedRows[0].Index == e.RowIndex)
                    {
                        SelectDropDownTypeList();
                    }
                    // Otherwise indicate that the type combo was clicked so that if this is the
                    // singly selected row upon UpdateSelection the type combo can be opened then.
                    else
                    {
                        _typeComboClickedRow = e.RowIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44813");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellMouseDown"/> event of <see cref="_dataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellMouseEventArgs"/> instance containing the
        /// event data.</param>
        void HandleDataGridViewCellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Ensure _typeComboClickedRow if the click didn't result in the redaction type combo opening.
            _typeComboClickedRow = -1;
        }

        /// <summary>
        /// Handles the <see cref="Control.Leave"/> event of <see cref="_dataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDataGridViewLeave(object sender, EventArgs e)
        {
            // Ensure _typeComboClickedRow if the click didn't result in the redaction type combo opening.
            _typeComboClickedRow = -1;
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
        public IDocumentViewer ImageViewer
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
                        _imageViewer.Shortcuts[Keys.T] = null;
                    }

                    // Store the new image viewer
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.AutoZoomScale = _autoZoomScale;
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;
                        _imageViewer.ImageFileClosing += HandleImageFileClosing;
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
                        _dataGridView.CellMouseDown += HandleDataGridViewCellMouseDown;
                        _dataGridView.CellMouseUp += HandleDataGridViewCellMouseUp;
                        _dataGridView.Leave += HandleDataGridViewLeave;
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
                        _dataGridView.CellMouseDown -= HandleDataGridViewCellMouseDown;
                        _dataGridView.CellMouseUp -= HandleDataGridViewCellMouseUp;
                        _dataGridView.Leave -= HandleDataGridViewLeave;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ConfidenceLevel"/> that should be associated with the specified
        /// <see paramref="category"/> and <see paramref="type"/>. If there are multiple matching
        /// confidence levels, the first defined level will be used.
        /// </summary>
        /// <param name="category">The category (or attribute name)</param>
        /// <param name="type">The redaction type (attribute type)</param>
        /// <param name="defaultLevel">The default <see cref="ConfidenceLevel"/> to use if no
        /// matching confidence level is found.</param>
        /// <returns>The <see cref="ConfidenceLevel"/> that should be associated with the specified
        /// <see paramref="category"/> and <see paramref="type"/>.</returns>
        ConfidenceLevel GetConfidenceLevel(string category, string type, ConfidenceLevel defaultLevel)
        {
            // Create a dummy attribute based on the specified category and type for testing against
            // the available confidence levels.
            ComAttribute attribute = new ComAttribute();
            attribute.Value.ReplaceAndDowngradeToNonSpatial(category);
            attribute.Type = type;
            attribute.ReportMemoryUsage();

            return _confidenceLevels.GetConfidenceLevel(attribute, defaultLevel);
        }

        #endregion Private Members
    }

    /// <summary>
    /// Provides data for the <see cref="RedactionGridView.ExemptionsApplied"/> event.
    /// </summary>
    [CLSCompliant(false)]
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
