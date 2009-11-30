using Extract.Imaging.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

using EOrientation = UCLID_RASTERANDOCRMGMTLib.EOrientation;
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
        /// The amount to multiply the <see cref="AutoZoomScale"/> when calculating the padding 
        /// around a view.
        /// </summary>
        const int _PADDING_MULTIPLIER = 25;

        /// <summary>
        /// The default value for the <see cref="AutoZoomScale"/>.
        /// </summary>
        const int _DEFAULT_AUTO_ZOOM_SCALE = 5;

        #endregion Constants

        #region Fields

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
        /// <see langword="true"/> if changes have been made to the grid since it was loaded;
        /// <see langword="false"/> if no changes have been made to the grid since it was loaded.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Font for rows that have been visited.
        /// </summary>
        Font _visitedFont;

        /// <summary>
        /// The cell style for rows that have been visited.
        /// </summary>
        DataGridViewCellStyle _visitedCellStyle;

        /// <summary>
        /// The cell style for the page cell of visited rows.
        /// </summary>
        DataGridViewCellStyle _visitedPageCellStyle;

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
        /// Gets the <see cref="DataGridViewCellStyle"/> associated with visited redactions.
        /// </summary>
        /// <value>The <see cref="DataGridViewCellStyle"/> associated with visited redactions.</value>
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
        
        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds the specified row to the <see cref="RedactionGridView"/>.
        /// </summary>
        /// <param name="layerObject">The layer object to add.</param>
        /// <param name="text">The text associated with the <paramref name="layerObject"/>.</param>
        /// <param name="category">The category associated with the <paramref name="layerObject"/>.</param>
        /// <param name="type">The type associated with the <paramref name="layerObject"/>.</param>
        public void Add(LayerObject layerObject, string text, string category, string type)
        {
            try
            {
                Add( new RedactionGridViewRow(layerObject, text, category, type) );
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
        void Add(RedactionGridViewRow row)
        {
            string type = row.RedactionType;
            if (!string.IsNullOrEmpty(type) && !_typeColumn.Items.Contains(type))
            {
                _typeColumn.Items.Add(type);
            }
            _redactions.Add(row);

            _dirty = true;
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
                foreach (DataGridViewRow row in _dataGridView.SelectedRows)
                {
                    RedactionGridViewRow redaction = _redactions[row.Index];
                    redaction.Redacted = !redaction.Redacted;
                    _dataGridView.UpdateCellValue(_redactedColumn.Index, row.Index);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28631", ex);
            }
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
            _imageViewer.LayerObjects.Selection.LayerObjectAdded -= HandleSelectionLayerObjectAdded;
            _imageViewer.LayerObjects.Selection.LayerObjectDeleted -= HandleSelectionLayerObjectDeleted;
            try
            {
                // Mark all selected rows as visited
                foreach (DataGridViewRow row in _dataGridView.SelectedRows)
                {
                    MarkAsVisited(row);
                }

                // Select only those layer objects that correspond to selected rows
                UpdateLayerObjectSelection();

                // Is at least one row selected?
                if (_dataGridView.SelectedRows.Count > 0)
                {
                    // If no redaction is on the currently visible page, 
                    // go to the page of the first selected redaction.
                    if (!IsRedactionVisible())
                    {
                        _imageViewer.PageNumber = GetFirstSelectedPage();
                    }

                    // If auto-zoom is on, zoom around all layer objects on the current page.
                    if (_autoZoom)
                    {
                        PerformAutoZoom();
                    }
                }

                _imageViewer.Invalidate();
            }
            finally
            {
                _imageViewer.LayerObjects.Selection.LayerObjectAdded += HandleSelectionLayerObjectAdded;
                _imageViewer.LayerObjects.Selection.LayerObjectDeleted += HandleSelectionLayerObjectDeleted;
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
                bool shouldBeSelected = selectedIds.Contains(layerObject.Id);
                if (layerObject.Selected != shouldBeSelected)
                {
                    layerObject.Selected = shouldBeSelected;
                    SelectLayerObject(layerObject, shouldBeSelected);
                }
            }
        }

        /// <summary>
        /// Determines whether any redaction is on the currently visible page.
        /// </summary>
        /// <returns><see langword="true"/> if there is a redaction on the currently visible page;
        /// <see langword="false"/> if there are no redactions or all redactions are on 
        /// non-visible pages.</returns>
        bool IsRedactionVisible()
        {
            // Get the current page number
            int currentPage = _imageViewer.PageNumber;
            foreach (RedactionGridViewRow row in SelectedRows)
            {
                foreach (LayerObject layerObject in row.LayerObjects)
                {
                    if (layerObject.PageNumber == currentPage)
                    {
                        return true;
                    }
                }
            }

            return false;
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
        /// Zooms around selected layer objects on the current page.
        /// </summary>
        void PerformAutoZoom()
        {
            // Get combined area of all the selected layer objects on the current page
            Rectangle area = GetSelectedBoundsOnPage(_imageViewer.PageNumber);

            // Adjust the area by the auto zoom scale
            int padding = _autoZoomScale * _PADDING_MULTIPLIER;
            area = _imageViewer.PadViewingRectangle(area, padding, padding, false);
            area = _imageViewer.GetTransformedRectangle(area, false);

            // Zoom to appropriate area
            _imageViewer.ZoomToRectangle(area);
        }

        /// <summary>
        /// Determines the smallest bounding rectangle around all selected layer objects on the 
        /// specified page.
        /// </summary>
        /// <param name="pageNumber">The page on which the selected layer objects appear.</param>
        /// <returns>The smallest bounding rectangle around all selected layer objects on 
        /// <paramref name="pageNumber"/>; or <see cref="Rectangle.Empty"/> if no layer objects 
        /// are selected on <paramref name="pageNumber"/>.</returns>
        Rectangle GetSelectedBoundsOnPage(int pageNumber)
        {
            // Iterate over each layer object on the page
            Rectangle? area = null;
            foreach (RedactionGridViewRow row in SelectedRows)
            {
                foreach (LayerObject layerObject in row.LayerObjects)
                {
                    if (layerObject.PageNumber == pageNumber)
                    {
                        // Append the bounds of this layer object
                        if (area == null)
                        {
                            area = layerObject.GetBounds();
                        }
                        else
                        {
                            area = Rectangle.Union(area.Value, layerObject.GetBounds());
                        }
                    }
                }
            }

            return area ?? Rectangle.Empty;
        }

        /// <summary>
        /// Visually marks the specified row as viewed.
        /// </summary>
        /// <param name="row">The row to mark as visited.</param>
        void MarkAsVisited(DataGridViewRow row)
        {
            row.DefaultCellStyle = VisitedCellStyle;
            row.Cells[_pageColumn.Index].Style = VisitedPageCellStyle;
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

                foreach (DataGridViewRow row in _dataGridView.SelectedRows)
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
                // As layer objects are added to the image viewer, don't handle the event.
                // Otherwise two rows will be added for each attribute.
                _imageViewer.LayerObjects.LayerObjectAdded -= HandleLayerObjectAdded;
                _imageViewer.LayerObjects.LayerObjectDeleted -= HandleLayerObjectDeleted;
                _imageViewer.LayerObjects.LayerObjectChanged -= HandleLayerObjectChanged;
                _imageViewer.LayerObjects.Selection.LayerObjectAdded -= HandleSelectionLayerObjectAdded;
                _imageViewer.LayerObjects.Selection.LayerObjectDeleted -= HandleSelectionLayerObjectDeleted;
                _dataGridView.SelectionChanged -= HandleDataGridViewSelectionChanged;

                try
                {
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

                    _dirty = false;
                }
                finally
                {
                    _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;
                    _imageViewer.LayerObjects.LayerObjectDeleted += HandleLayerObjectDeleted;
                    _imageViewer.LayerObjects.LayerObjectChanged += HandleLayerObjectChanged;
                    _imageViewer.LayerObjects.Selection.LayerObjectAdded += HandleSelectionLayerObjectAdded;
                    _imageViewer.LayerObjects.Selection.LayerObjectDeleted += HandleSelectionLayerObjectDeleted;
                    _dataGridView.SelectionChanged += HandleDataGridViewSelectionChanged;
                }
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
                Add(row);

                foreach (LayerObject layerObject in row.LayerObjects)
                {
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
            // TODO: SetPageNumber is inefficient and should be removed from ImageViewer
            int page = _imageViewer.PageNumber;
            try
            {
                // Iterate over each page of the image.
                LongToObjectMap pageInfoMap = new LongToObjectMap();
                for (int i = 1; i <= _imageViewer.PageCount; i++)
                {
                    _imageViewer.SetPageNumber(i, false, false);

                    // Create the spatial page info for this page
                    SpatialPageInfo pageInfo = new SpatialPageInfo();
                    int width = _imageViewer.ImageWidth;
                    int height = _imageViewer.ImageHeight;
                    pageInfo.SetPageInfo(width, height, EOrientation.kRotNone, 0);

                    // Add it to the map
                    pageInfoMap.Set(i, pageInfo);
                }

                return pageInfoMap;
            }
            finally
            {
                // Restore the original page number
                _imageViewer.SetPageNumber(page, false, false);
            }
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
                        counts[index]++;

                        if (index != (int)CategoryIndex.Clues)
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
                SelectLayerObject(layerObject, select);

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

                case AutoTool.SelectLayerObject:
                    return CursorTool.SelectLayerObject;

                default:
                    throw new ExtractException("ELI27436", "Unexpected auto tool.");
            }
        }

        /// <summary>
        /// Gets the index of the first unviewed row that occurs at or before the specified row.
        /// </summary>
        /// <param name="startIndex">The first index to check for being unviewed.</param>
        /// <returns>The index of the first unviewed row that occurs at or before 
        /// <paramref name="startIndex"/>; or -1 if no such row exists.</returns>
        public int GetPreviousUnviewedRowIndex(int startIndex)
        {
            try
            {
                // Iterate backwards starting at the specified row
                for (int i = startIndex; i >= 0; i--)
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
                throw new ExtractException("ELI27670",
                    "Unable to determine previous unviewed row.", ex);
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
                for (int i = 0; i < _dataGridView.Rows.Count; i++)
                {
                    if (_dataGridView.Rows[i].Selected)
                    {
                        return i;
                    }
                }

                return -1;
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
                for (int i = _dataGridView.Rows.Count - 1; i >= 0; i--)
                {
                    if (_dataGridView.Rows[i].Selected)
                    {
                        return i;
                    }
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27644",
                    "Unable to determine last selected row.", ex);
            }
        }

        /// <summary>
        /// Selects the specified row by index and deselects all other rows.
        /// </summary>
        /// <param name="index">The index of the row to select.</param>
        public void SelectOnly(int index)
        {
            try
            {
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
            if (rows.Count == 1)
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
        /// Marks the specified layer object as selected or not selected.
        /// </summary>
        /// <param name="layerObject">The layer object to mark as selected or not selected.</param>
        /// <param name="select"><see langword="true"/> to mark the layer object as selected;
        /// <see langword="false"/> to mark the layer object as not selected.</param>
        static void SelectLayerObject(LayerObject layerObject, bool select)
        {
            // TODO: Indicate layer object selection in a more prominent way [FIDSC #3771]
            Console.WriteLine(layerObject);
            Console.WriteLine(select);
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
                ExtractException ee = ExtractException.AsExtractException("ELI26673", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                Add(e.LayerObject, "[No text]", "Manual", _lastType);

                if (_autoTool != AutoTool.None)
                {
                    _imageViewer.CursorTool = GetAutoCursorTool();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26677", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                Remove(e.LayerObject);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26678", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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

                // The layer object wasn't found. Complain.
                ExtractException ee = new ExtractException("ELI26952", "Layer object not found.");
                ee.AddDebugData("Id", e.LayerObject.Id, false);
                throw ee;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26951", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27061", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                ExtractException ee = ExtractException.AsExtractException("ELI27065", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                ExtractException ee = ExtractException.AsExtractException("ELI27064", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                // Check if an exemption codes cell was clicked
                if (IsExemptionColumn(e.ColumnIndex) && e.RowIndex >= 0)
                {
                    PromptForExemptions();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26709", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                ExtractException ee = ExtractException.AsExtractException("ELI27725", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                        _imageViewer.ImageFileChanged -= HandleImageFileChanged;
                        _imageViewer.LayerObjects.LayerObjectAdded -= HandleLayerObjectAdded;
                        _imageViewer.LayerObjects.LayerObjectDeleted -= HandleLayerObjectDeleted;
                        _imageViewer.LayerObjects.LayerObjectChanged -= HandleLayerObjectChanged;
                        _imageViewer.LayerObjects.Selection.LayerObjectAdded -= HandleSelectionLayerObjectAdded;
                        _imageViewer.LayerObjects.Selection.LayerObjectDeleted -= HandleSelectionLayerObjectDeleted;
                        _imageViewer.Shortcuts[Keys.T] = null;
                    }

                    // Store the new image viewer
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;
                        _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;
                        _imageViewer.LayerObjects.LayerObjectDeleted += HandleLayerObjectDeleted;
                        _imageViewer.LayerObjects.LayerObjectChanged += HandleLayerObjectChanged;
                        _imageViewer.LayerObjects.Selection.LayerObjectAdded += HandleSelectionLayerObjectAdded;
                        _imageViewer.LayerObjects.Selection.LayerObjectDeleted += HandleSelectionLayerObjectDeleted;
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
