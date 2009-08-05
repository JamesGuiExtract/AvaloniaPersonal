using Extract.AttributeFinder;
using Extract.Imaging.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;
using EOrientation = UCLID_RASTERANDOCRMGMTLib.EOrientation;
using SpatialPageInfo = UCLID_RASTERANDOCRMGMTLib.SpatialPageInfo;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a <see cref="DataGridView"/> that displays information about redactions.
    /// </summary>
    public partial class RedactionGridView : UserControl, IImageViewerControl
    {
        #region RedactionGridView Constants

        /// <summary>
        /// Directory where exemption code xml files are stored.
        /// </summary>
        static readonly string _EXEMPTION_DIRECTORY =
#if DEBUG
            "..\\..\\ProductDevelopment\\AttributeFinder\\IndustrySpecific\\Redaction\\RedactionCustomComponents\\ExemptionCodes";
#else
	        "..\\IDShield\\ExemptionCodes";
#endif

        #endregion RedactionGridView Constants

        #region RedactionGridView Fields

        /// <summary>
        /// The <see cref="ImageViewer"/> with which the <see cref="RedactionGridView"/> is 
        /// associated.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// Each row of the <see cref="RedactionGridView"/> which represents a redaction.
        /// </summary>
        BindingList<RedactionGridViewRow> _redactions = new BindingList<RedactionGridViewRow>();

        /// <summary>
        /// A dialog that allows the user to select exemption codes.
        /// </summary>
        ExemptionCodeListDialog _exemptionsDialog;

        /// <summary>
        /// The last applied exemption codes or <see langword="null"/> if no exemption code has 
        /// been applied.
        /// </summary>
        ExemptionCodeList _lastApplied;

        /// <summary>
        /// The master list of valid exemption categories and codes.
        /// </summary>
        MasterExemptionCodeList _masterCodes;

        #endregion RedactionGridView Fields

        #region RedactionGridView Events

        /// <summary>
        /// Occurs when an exemption code is applied to a redaction.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when an exemption code is applied to a redaction.")]
        public event EventHandler<ExemptionsAppliedEventArgs> ExemptionsApplied;

        #endregion RedactionGridView Events

        #region RedactionGridView Constructors

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

        #endregion RedactionGridView Constructors

        #region RedactionGridView Properties

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
                if (_lastApplied != null)
                {
                    _exemptionsDialog.EnableApplyLast = true;
                    _exemptionsDialog.LastExemptionCodeList = _lastApplied;
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
                    string directory = FileSystemMethods.GetAbsolutePath(_EXEMPTION_DIRECTORY);
                    _masterCodes = new MasterExemptionCodeList(directory);
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
                return _lastApplied != null;
            }
        }

        #endregion RedactionGridView Properties

        #region RedactionGridView Methods

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
            if (!_typeColumn.Items.Contains(row.RedactionType))
            {
                _typeColumn.Items.Add(row.RedactionType);
            }
            _redactions.Add(row);
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
                        if (row.LayerObjectCount == 0)
                        {
                            _redactions.RemoveAt(i);
                        }
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
                    _lastApplied = result;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26714", ex);
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
            foreach (DataGridViewRow row in _dataGridView.SelectedRows)
            {
                RedactionGridViewRow redaction = _redactions[row.Index];
                redaction.Exemptions = exemptions;
                _dataGridView.UpdateCellValue(_exemptionsColumn.Index, row.Index);

                // Raise the ExemptionsApplied event
                OnExemptionsApplied(new ExemptionsAppliedEventArgs(exemptions, redaction));
            }
        }

        /// <summary>
        /// Applies the most recently applied exemption codes to all selected redactions.
        /// </summary>
        public void ApplyLastExemptions()
        {
            ApplyExemptionsToSelected(_lastApplied);
        }

        /// <summary>
        /// Loads the rows of the <see cref="RedactionGridView"/> based on the specified vector of 
        /// attributes file.
        /// </summary>
        /// <param name="fileName">A file containing a vector of attributes.</param>
        public void LoadFrom(string fileName)
        {
            try
            {
                // As layer objects are added to the image viewer, don't handle the event.
                // Otherwise two rows will be added for each attribute.
                _imageViewer.LayerObjects.LayerObjectAdded -= HandleLayerObjectAdded;

                // Iterate over the attributes
                foreach (ComAttribute attribute in AttributesFile.ReadAll(fileName))
                {
                    // Add a row for each attribute
                    RedactionGridViewRow row = 
                        RedactionGridViewRow.FromComAttribute(attribute, _imageViewer, MasterCodes);
                    if (row != null)
                    {
                        Add(row);

                        foreach (LayerObject layerObject in row.LayerObjects)
                        {
                            _imageViewer.LayerObjects.Add(layerObject);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26761",
                    "Unable to load VOA file.", ex);
                ee.AddDebugData("Voa file", fileName, false);
                throw ee;
            }
            finally
            {
                _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;
            }
        }

        /// <summary>
        /// Saves the rows of the <see cref="RedactionGridView"/> to the specified vector of 
        /// attributes file.
        /// </summary>
        /// <param name="fileName">A file to contain a vector of attributes.</param>
        public void SaveTo(string fileName)
        {
            try
            {
                // Get the image information
                string sourceDocName = _imageViewer.ImageFile;
                LongToObjectMap pageInfoMap = GetPageInfoMap();

                // Create an attribute for each row
                IUnknownVector attributes = new IUnknownVector();
                foreach (RedactionGridViewRow row in _redactions)
                {
                    ComAttribute attribute = row.ToComAttribute(sourceDocName, pageInfoMap);
                    attributes.PushBack(attribute);
                }

                // Save the attributes
                attributes.SaveTo(fileName, false);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26950",
                    "Unable to save VOA file.", ex);
                ee.AddDebugData("Voa file", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the page info map for the currently open image.
        /// </summary>
        /// <returns>The page info map for the currently open image.</returns>
        LongToObjectMap GetPageInfoMap()
        {
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

        #endregion RedactionGridView Methods

        #region RedactionGridView OnEvents

        /// <summary>
        /// Raises the <see cref="ExemptionsApplied"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="ExemptionsApplied"/> 
        /// event.</param>
        protected virtual void OnExemptionsApplied(ExemptionsAppliedEventArgs e)
        {
            if (ExemptionsApplied != null)
            {
                ExemptionsApplied(this, e);
            }
        }

        #endregion RedactionGridView OnEvents

        #region RedactionGridView Event Handlers

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
                Add(e.LayerObject, "[No text]", "Man", "");
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

        #endregion RedactionGridView Event Handlers

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
