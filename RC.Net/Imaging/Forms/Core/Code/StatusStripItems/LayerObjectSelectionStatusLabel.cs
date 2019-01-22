using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Imaging.Forms.StatusStripItems
{
    /// <summary>
    /// A status label that indicates which layer object (by number) is currently selected and
    /// visible.
    /// </summary>
    public partial class LayerObjectSelectionStatusLabel : ToolStripStatusLabel, IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(LayerObjectSelectionStatusLabel).ToString();

        /// <summary>
        /// The text that displays on the label in the Visual Studio designer.
        /// </summary>
        static readonly string _DEFAULT_DESIGNTIME_TEXT = "(Layer object selection)";

        /// <summary>
        /// The default value for <see cref="LayerObjectName"/>.
        /// </summary>
        static readonly string _DEFAULT_LAYER_OBJECT_NAME = "Layer object";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Image viewer with which this status label connects.
        /// </summary>
        IDocumentViewer _imageViewer;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerObjectSelectionStatusLabel"/> class.
        /// </summary>
        public LayerObjectSelectionStatusLabel()
            : base(" ")
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI35776",
                    _OBJECT_NAME);

                InitializeComponent();

                // If this is design-time, set the text of this label so it is visible.
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    base.Text = _DEFAULT_DESIGNTIME_TEXT;
                }
                
                LayerObjectName = _DEFAULT_LAYER_OBJECT_NAME;

                base.AutoSize = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI35777", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the term used to refer to <see cref="LayerObject"/>s in the current
        /// context.
        /// </summary>
        /// <value>
        /// The term used to refer to <see cref="LayerObject"/>s in the current context.
        /// </value>
        [DefaultValue("Layer object")]
        public string LayerObjectName
        {
            get;
            set;
        }

        #endregion Properties

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="LayerObjectSelectionStatusLabel"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="LayerObjectSelectionStatusLabel"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="LayerObjectSelectionStatusLabel"/> is 
        /// connected. <see langword="null"/> if no connections are established.</returns>
        [Browsable(false)]
        [CLSCompliant(false)]
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
                        _imageViewer.ImageFileChanged -= Handle_ImageFileChanged;
                        _imageViewer.PageChanged -= Handle_PageChanged;
                        _imageViewer.ZoomChanged -= Handle_ZoomChanged;
                        _imageViewer.ScrollPositionChanged -= Handle_ScrollPositionChanged;
                        _imageViewer.LayerObjects.Selection.LayerObjectVisibilityChanged -= Handle_LayerObjectVisibilityChanged;
                        _imageViewer.LayerObjects.Selection.LayerObjectChanged -= Handle_LayerObjectChanged;
                        _imageViewer.LayerObjects.Selection.LayerObjectAdded -= Handle_LayerObjectAdded;
                        _imageViewer.LayerObjects.Selection.LayerObjectDeleted -=  HandleSelection_LayerObjectDeleted;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.ImageFileChanged += Handle_ImageFileChanged;
                        _imageViewer.PageChanged += Handle_PageChanged;
                        _imageViewer.ZoomChanged += Handle_ZoomChanged;
                        _imageViewer.ScrollPositionChanged += Handle_ScrollPositionChanged;
                        _imageViewer.LayerObjects.Selection.LayerObjectVisibilityChanged += Handle_LayerObjectVisibilityChanged;
                        _imageViewer.LayerObjects.Selection.LayerObjectChanged += Handle_LayerObjectChanged;
                        _imageViewer.LayerObjects.Selection.LayerObjectAdded += Handle_LayerObjectAdded;
                        _imageViewer.LayerObjects.Selection.LayerObjectDeleted += HandleSelection_LayerObjectDeleted;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI35778",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        void Handle_ImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                UpdateLabel();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35781", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.PageChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.PageChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.PageChanged"/> event.</param>
        void Handle_PageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                UpdateLabel();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35782", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.ZoomChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ZoomChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ZoomChanged"/> event.</param>
        void Handle_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            try
            {
                UpdateLabel();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35783", ex);
            }
        }

        /// <summary>
        /// Handles the ScrollPositionChanged event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// ScrollPositionChanged event.</param>
        /// <param name="e">The event data associated with the
        /// ScrollPositionChanged event.</param>
        void Handle_ScrollPositionChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateLabel();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35784", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event of the
        /// <see cref="T:ImageViewer.Selection"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.LayerObjectAddedEventArgs"/>
        /// instance containing the event data.</param>
        void Handle_LayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                UpdateLabel();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35775");
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event of the
        /// <see cref="T:ImageViewer.Selection"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.LayerObjectDeletedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleSelection_LayerObjectDeleted(object sender, LayerObjectDeletedEventArgs e)
        {
            try
            {
                UpdateLabel();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35774");
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.</param>
        void Handle_LayerObjectChanged(object sender, LayerObjectChangedEventArgs e)
        {
            try
            {
                UpdateLabel();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35779", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectVisibilityChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectVisibilityChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectVisibilityChanged"/> event.</param>
        void Handle_LayerObjectVisibilityChanged(object sender,
            LayerObjectVisibilityChangedEventArgs e)
        {
            try
            {
                UpdateLabel();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35780", ex);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays the index of any singly selected layer object that is currently visible.
        /// </summary>
        void UpdateLabel()
        {
            // Are we attached to an image viewer?
            if (_imageViewer != null)
            {
                // Is there a singly selected object?
                LayerObject selectedObject = (_imageViewer.LayerObjects.Selection.Count <= 1)
                    ? _imageViewer.LayerObjects.Selection.SingleOrDefault()
                    : null;

                if (selectedObject != null)
                {
                    // Can we find the index in all objects of the selected item?
                    var sortedLayerObjects = _imageViewer.LayerObjects.GetSortedCollection();
                    int index = sortedLayerObjects.IndexOf(selectedObject);

                    if (index >= 0)
                    {
                        // Is the selected item visible right now?
                        Rectangle viewRectangle = _imageViewer.GetTransformedRectangle(
                            _imageViewer.GetVisibleImageArea(), true);

                        if (selectedObject.IsContained(viewRectangle, _imageViewer.PageNumber))
                        {
                            // If so, display the index.
                            Text = string.Format(CultureInfo.CurrentCulture, "{0} {1:D} of {2:D}",
                                LayerObjectName, index + 1, sortedLayerObjects.Count);
                            
                            return;
                        }
                    }
                }
            }

            Text = "";
        }

        #endregion Private Members
    }
}
