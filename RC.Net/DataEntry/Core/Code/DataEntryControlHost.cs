using Extract;
using Extract.Drawing;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFOUTPUTHANDLERSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    #region Enums

    /// <summary>
    /// Indicates whether or not unviewed data can be saved.
    /// </summary>
    public enum UnviewedDataSaveMode
    {
        /// <summary>
        /// Allow data to be saved without prompting when unviewed data is present.
        /// </summary>
        Allow,

        /// <summary>
        /// Allow data to be saved when unviewed data is present, but prompt first.
        /// (one prompt to cover all unviewed).
        /// </summary>
        PromptOnceForAll,

        /// <summary>
        /// Require all data to be viewed before saving.
        /// </summary>
        Disallow
    }

    /// <summary>
    /// Indicates whether or not data that does not conform to a validation requirement can be
    /// saved.
    /// </summary>
    public enum InvalidDataSaveMode
    {
        /// <summary>
        /// Allow data to be saved without prompting when data that does not conform to a
        /// validation requirement is present.
        /// </summary>
        Allow,

        /// <summary>
        /// Allow data to be saved when  when data that does not conform to a
        /// validation requirement is present, but prompt for each invalid field first.
        /// </summary>
        PromptForEach,

        /// <summary>
        /// Require all data to meet validation requirements before saving.
        /// </summary>
        Disallow
    }

    /// <summary>
    /// Describes a <see cref="Color"/> to use for indicating active status in a 
    /// <see cref="IDataEntryControl"/> or displaying is associated data in the image viewer for a
    /// range of OCR confidence levels.
    /// </summary>
    public class HighlightColor
    {
        #region Fields

        /// <summary>
        /// The upper limit of OCR confidence associated with a character to be highlighted in the
        /// image viewer that should be highlighted with this color.
        /// </summary>
        private int _maxOcrConfidence;

        /// <summary>
        /// The color the highlight should be.
        /// </summary>
        private Color _color;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialized a new <see cref="HighlightColor"/> instance with the default settings.
        /// </summary>
        public HighlightColor()
            : this(100, Color.LightGreen)
        {
        }

        /// <summary>
        /// Initialized a new <see cref="HighlightColor"/> instance with the specified color and 
        /// maximum associated OCR confidence level.
        /// </summary>
        /// <param name="maxOcrConfidence">The maximum OCR confidence level of a character to be
        /// highlighted in this color.</param>
        /// <param name="color">Specifies the <see cref="Color"/> for a highlight corresponding
        /// to this OCR tier.</param>
        public HighlightColor(int maxOcrConfidence, Color color)
        {
            _maxOcrConfidence = maxOcrConfidence;
            _color = color;
        }

        #endregion Constructors

        /// <summary>
        /// Gets or sets the maximum OCR confidence level of a character to be highlighted in this
        /// color.
        /// </summary>
        /// <value>The maximum OCR confidence level of a character to be highlighted in this color.
        /// </value>
        /// <returns>The maximum OCR confidence level of a character to be highlighted in this color.
        /// </returns>
        public int MaxOcrConfidence
        {
            get
            {
                return _maxOcrConfidence;
            }

            set
            {
                _maxOcrConfidence = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Color"/> for a highlight corresponding to this OCR tier.
        /// </summary>
        /// <value>The <see cref="Color"/> for a highlight corresponding to this OCR tier.</value>
        /// <returns>The <see cref="Color"/> for a highlight corresponding to this OCR tier.</returns>
        public Color Color
        {
            get
            {
                return _color;
            }

            set
            {
                _color = value;
            }
        }
    }

    #endregion Enums

    /// <summary>
    /// A control whose contents define a Data Entry Pane (DEP).  All data entry controls must be
    /// contained in an DataEntryControlHost instance.
    /// <para><b>Note:</b></para>
    /// Only one control host per image viewer is supported .
    /// </summary>
    public partial class DataEntryControlHost : UserControl, IImageViewerControl, IMessageFilter
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DataEntryControlHost).ToString();

        /// <summary>
        /// The value associated with a window's key down message.
        /// </summary>
        private const int _WM_KEYDOWN = 0x100;

        /// <summary>
        /// The value associated with a window's key up message.
        /// </summary>
        private const int _WM_KEYUP = 0x101;

        /// <summary>
        /// The value associated with a window's left mouse button down message.
        /// </summary>
        private const int _WM_LBUTTONDOWN = 0x0201;

        /// <summary>
        /// The value associated with a window's left mouse button up message.
        /// </summary>
        private const int _WM_LBUTTONUP = 0x0202;

        /// <summary>
        /// The value associated with a window's right mouse button down message.
        /// </summary>
        private const int _WM_RBUTTONDOWN = 0x0204;

        /// <summary>
        /// The value associated with a window's right mouse button up message.
        /// </summary>
        private const int _WM_RBUTTONUP = 0x0205;

        /// <summary>
        /// The value associated with a window's mouse wheel message.
        /// </summary>
        private const int _WM_MOUSEWHEEL = 0x020A;

        /// <summary>
        /// The default font size to be used for tooltips.
        /// </summary>
        private const float _TOOLTIP_FONT_SIZE = 13F;

        /// <summary>
        /// The font family used to display data.
        /// </summary>
        private static readonly string _DATA_FONT_FAMILY = "Verdana";

        /// <summary>
        /// The width/height in inches of the icon to be shown in the image viewer to indicate
        /// invalid data.
        /// </summary>
        private const double _ERROR_ICON_SIZE = 0.15;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The image viewer with which to display documents.
        /// </summary>
        private ImageViewer _imageViewer;

        /// <summary>
        /// The vector of attributes associated with any currently open document.
        /// </summary>
        private IUnknownVector _attributes = (IUnknownVector)new IUnknownVectorClass();

        /// <summary>
        /// A dictionary to keep track of the highlights associated with each attribute.
        /// </summary>
        private Dictionary<IAttribute, List<CompositeHighlightLayerObject>> _attributeHighlights =
            new Dictionary<IAttribute, List<CompositeHighlightLayerObject>>();

        /// <summary>
        /// A dictionary to keep track of each attribute's tooltips
        /// </summary>
        private Dictionary<IAttribute, List<TextLayerObject>> _attributeToolTips =
            new Dictionary<IAttribute, List<TextLayerObject>>();

        /// <summary>
        /// A dictionary to keep track of each attribute's error icons
        /// </summary>
        private Dictionary<IAttribute, List<ImageLayerObject>> _attributeErrorIcons =
            new Dictionary<IAttribute, List<ImageLayerObject>>();

        /// <summary>
        /// The size the error icons should be on each page (determined via a combination of
        /// _ERROR_ICON_SIZE and the DPI of the page.
        /// </summary>
        private Dictionary<int, Size> _errorIconSizes = new Dictionary<int, Size>();

        /// <summary>
        /// A dictionary to keep track of the attribute each highlight is related to.
        /// </summary>
        Dictionary<CompositeHighlightLayerObject, IAttribute> _highlightAttributes =
            new Dictionary<CompositeHighlightLayerObject, IAttribute>();

        /// <summary>
        /// A dictionary to keep track of the currently displayed highlights. (NOTE: this collection
        /// represents the highlights that would be displayed if _showingAllHighlights is false. If
        /// _showingAllHighlights is true, though all attribute highlights will be displayed this
        /// collection will only contain the "active" highlights.
        /// </summary>
        Dictionary<IAttribute, bool> _displayedAttributeHighlights =
            new Dictionary<IAttribute, bool>();

        /// <summary>
        /// A dictionary to keep track of each control's active attributes.
        /// </summary>
        private Dictionary<IDataEntryControl, List<IAttribute>> _controlAttributes =
            new Dictionary<IDataEntryControl, List<IAttribute>>();

        /// <summary>
        /// A dictionary to keep track of each control's attributes that have tooltips.
        /// </summary>
        private Dictionary<IDataEntryControl, List<IAttribute>> _controlToolTipAttributes =
            new Dictionary<IDataEntryControl, List<IAttribute>>();

        /// <summary>
        /// Indicates if the user has temporarily hid all tooltips.
        /// </summary>
        private bool _temporarilyHidingTooltips;

        /// <summary>
        /// The attribute that corresponds to a highlight that the selection tool is currently
        /// hovering over.
        /// </summary>
        IAttribute _hoverAttribute;

        /// <summary>
        /// A list of all data controls contained in this control host.
        /// </summary>
        private List<IDataEntryControl> _dataControls = new List<IDataEntryControl>();

        /// <summary>
        /// A list of the controls mapped to root-level attributes. (to which the control host needs
        /// to provide attributes)
        /// </summary>
        private List<IDataEntryControl> _rootLevelControls = new List<IDataEntryControl>();

        /// <summary>
        /// A flag used to indicate that the current document image is changing so that highlight
        /// drawing can be suspended while all the controls refresh their spatial information.
        /// </summary>
        private bool _changingImage;

        /// <summary>
        /// The current "active" data entry.  This is the last data entry control to have received
        /// input focus (but doesn't necessarily mean the control currently has input focus).
        /// </summary>
        private IDataEntryControl _activeDataControl;

        /// <summary>
        /// Keeps track of the last active cursor tool so that the highlight cursor tools can be
        /// automatically re-enabled after focus passes through control that doesn't support
        /// swiping.
        /// </summary>
        private CursorTool _lastCursorTool = CursorTool.None;

        /// <summary>
        /// The OCR manager to be used to recognize text from image swipes.
        /// </summary>
        private SynchronousOcrManager _ocrManager;

        /// <summary>
        /// The font to use to draw toolTip text.
        /// </summary>
        private Font _toolTipFont;

        /// <summary>
        /// To manage tab orders keep track of when focus had belonged to a control outside of the 
        /// control host, but is now returning to a control within the control host
        /// </summary>
        private bool _regainingFocus;

        /// <summary>
        /// To manage tab order when the control host is regaining focus, keep track of whether the 
        /// shift key is down.
        /// </summary>
        private bool _shiftKeyDown;

        /// <summary>
        /// Inidicates when a manual focus change is taking place (tab key was pressed or a
        /// highlight was selected in the image viewer).
        /// </summary>
        private bool _manualFocusEvent;

        /// <summary>
        /// To manage tab order when the control host is regaining focus, keep track of any data
        /// entry control which should receive focus as the result of a mouse click.
        /// </summary>
        private IDataEntryControl _clickedDataEntryControl;

        /// <summary>
        /// The ErrorProvider data entry controls should used to display data validation errors
        /// (unless the control needs a specialized error provider).
        /// </summary>
        private ErrorProvider _errorProvider = new ErrorProvider();

        /// <summary>
        /// The number of unviewed attributes known to exist.
        /// </summary>
        private int _unviewedAttributeCount;

        /// <summary>
        /// The number of attributes with invalid data known to exist.
        /// </summary>
        private int _invalidAttributeCount;

        /// <summary>
        /// Indicates whether data had been modified since the last load or save.
        /// </summary>
        private bool _dirty;

        /// <summary>
        /// Indicates whether all highlights are currently being displayed (true) or if only the
        /// highlights that relate to the selection in the DEP are being displayed (false).
        /// </summary>
        private bool _showingAllHighlights;


        /// <summary>
        /// Specifies whether the data entry controls and image viewer are currently being
        /// prevented from updating.
        /// </summary>
        private bool _controlUpdatesLocked;

        /// <summary>
        /// Indicates whether all data must be viewed before saving and, if not, whether a prompt
        /// will be displayed before allowing unviewed data to be saved.
        /// </summary>
        private UnviewedDataSaveMode _unviewedDataSaveMode = UnviewedDataSaveMode.Allow;

        /// <summary>
        /// Indicates whether all data must conform to validation rules before saving and, if not,
        /// whether a prompt will be displayed before allowing invalid data to be saved.
        /// </summary>
        private InvalidDataSaveMode _invalidDataSaveMode = InvalidDataSaveMode.Disallow;

        /// <summary>
        /// Keeps track of whether the highlights associated with the active control need to be
        /// refreshed.
        /// </summary>
        private bool _refreshActiveControlHighlights;

        /// <summary>
        /// One or more colors to use to highlight data in the image viewer or indicate the active
        /// status of data in a control.
        /// </summary>
        private HighlightColor[] _highlightColors;

        /// <summary>
        /// The boundaries between tiers of OCR confidence in _highlightColors.
        /// </summary>
        private VariantVector _confidenceBoundaries;

        /// <summary>
        /// The default color to use for highlighting data in the image viewer or to indicate the
        /// active status of data in a control. This will be the same color as the top tier color
        /// in _highlightColors.
        /// </summary>
        private Color _defaultHighlightColor = Color.LightGreen;

        /// <summary>
        /// The title of the current DataEntry application.
        /// </summary>
        private string _applicationTitle;

        /// <summary>
        /// A list of names of DataEntry controls that should remain disabled at all times.
        /// </summary>
        private List<string> _disabledControls = new List<string>();

        /// <summary>
        /// The number of selected attributes with highlights that have been accepted by the user.
        /// </summary>
        private int _selectedAttributesWithAcceptedHighlights;

        /// <summary>
        /// The number of selected attributes with unaccepted highlights.
        /// </summary>
        private int _selectedAttributesWithUnacceptedHighlights;

        /// <summary>
        /// The number of selected attributes without spatial information but that have a direct
        /// hint indicating where the data may be (if present).
        /// </summary>
        private int _selectedAttributesWithDirectHints;

        /// <summary>
        /// The number of selected attributes without spatial information but that have an indirect
        /// hint indicating data related field.
        /// </summary>
        private int _selectedAttributesWithIndirectHints;

        /// <summary>
        /// The number of selected attributes without spatial information or hints.
        /// </summary>
        private int _selectedAttributesWithoutHighlights;

        /// <summary>
        /// Indicates whether a swipe is currently being processed.
        /// </summary>
        private bool _processingSwipe;

        /// <summary>
        /// Indicates whether the results of the active swipe should be discarded.
        /// </summary>
        private bool _cancelingSwipe;

        /// <summary>
        /// A database available for use in validation or auto-update queries.
        /// </summary>
        private DbConnection _dbConnection;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        private bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryControlHost"/> class.
        /// </summary>
        public DataEntryControlHost()
        {
            try
            {
                // Load licenses in design mode
                _inDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                // TODO: New license ID?
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI23666",
                    _OBJECT_NAME);

                InitializeComponent();

                // Initializing these members (particularily OcrManager) during design-time
                // crashes Visual Studio.  These members aren't needed during design-time, so
                // we can ignore them.
                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    _ocrManager = new SynchronousOcrManager();
                }

                // Initialize the font to use for tooltips
                _toolTipFont = new Font(_DATA_FONT_FAMILY, _TOOLTIP_FONT_SIZE);

                // Specify the default highlight colors.
                HighlightColor[] highlightColors = {new HighlightColor(89, Color.LightSalmon),
                                                    new HighlightColor(100, Color.LightGreen)};
                this.HighlightColors = highlightColors;

                // Blinking error icons are annoying and unnecessary.
                _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23667", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether the data has been modified since the last load or save operation.
        /// </summary>
        /// <value><see langword="true"/> to consider the data a modified since the last load or
        /// save operation; <see langword="false"/> to consider the data unchanged.</value>
        /// <returns><see langword="true"/> if the data has been modified since the last load or
        /// save operation; <see langword="false"/> if the data is unchanged.</returns>
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
        /// Gets or sets whether highlights for all data mapped to an <see cref="IDataEntryControl"/>
        /// should be displayed in the <see cref="ImageViewer"/> or whether only highlights relating
        /// to the currently selected fields should be displayed.
        /// </summary>
        /// <value><see langword="true"/> if highlights for all data mapped to an
        /// <see cref="IDataEntryControl"/> is to be displayed or <see langword="false"/> if only
        /// the highlights relating to the currently selected fields should be displayed.</value>
        /// <returns><see langword="true"/> if highlights for all data mapped to an
        /// <see cref="IDataEntryControl"/> are being displayed or <see langword="false"/> if only
        /// the highlights relating to the currently selected fields are being displayed.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowAllHighlights
        {
            get
            {
                return _showingAllHighlights;
            }

            set
            {
                try
                {
                    if (value != _showingAllHighlights)
                    {
                        _showingAllHighlights = value;

                        // Show or hide all highlights as appropriate.
                        foreach (IAttribute attribute in _attributeHighlights.Keys)
                        {
                            // If _showAllHighlights is being set to true, show the highlight
                            // (unless it is an indirect hint).
                            if (value)
                            {
                                if (AttributeStatusInfo.GetHintType(attribute) != HintType.Indirect)
                                {
                                    ShowAttributeHighlights(attribute, true);
                                }
                            }
                            // Hide all other attributes as long as they are not part of the active
                            // selection.
                            else if (!_displayedAttributeHighlights.ContainsKey(attribute))
                            {
                                ShowAttributeHighlights(attribute, false);
                            }
                        }

                        DrawHighlights(false);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25120", ex);
                }
                finally
                {
                    // Ensure window updates and image viewer zooming are unlocked.
                    LockControlUpdates(false);
                }
            }
        }

        /// <summary>
        /// Indicates whether all data must be viewed before saving and, if not, whether a prompt
        /// will be displayed before allowing unviewed data to be saved.
        /// </summary>
        /// <value><see langword="UnviewedDataSaveMode.Allow"/> to allow unviewed data to be 
        /// saved without prompting, <see langword="UnviewedDataSaveMode.Prompt"/> to allow unviewed
        /// data to be saved, but only after prompting (once for all unviewed fields) or 
        /// <see langword="UnviewedDataSaveMode.Disallow"/> to require that all data be viewed
        /// before saving.</value>
        /// <returns><see langword="UnviewedDataSaveMode.Allow"/> if unviewed data can be
        /// saved without prompting, <see langword="UnviewedDataSaveMode.Prompt"/> if unviewed data
        /// can be saved, but only after prompting (once for all unviewed fields) or 
        /// <see langword="UnviewedDataSaveMode.Disallow"/> if all data must be viewed before
        /// saving.</returns>
        [Category("Data Entry Control Host")]
        public UnviewedDataSaveMode UnviewedDataSaveMode
        {
            get
            {
                return _unviewedDataSaveMode;
            }

            set
            {
                _unviewedDataSaveMode = value;
            }
        }

        /// <summary>
        /// Indicates whether all data must conform to validation rules before saving and, if not,
        /// whether a prompt will be displayed before allowing invalid data to be saved.
        /// </summary>
        /// <value><see langword="InvalidDataSaveMode.Allow"/> to allow invalid data to be saved
        /// without prompting, <see langword="InvalidDataSaveMode.Prompt"/> to allow invalid data
        /// to be saved, but only after prompting (once for each invalid field) or 
        /// <see langword="InvalidDataSaveMode.Disallow"/> to require that all data meet validation
        /// requirements before saving.</value>
        /// <returns><see langword="InvalidDataSaveMode.Allow"/> if invalid data can be saved
        /// without prompting, <see langword="InvalidDataSaveMode.Prompt"/> if invalid data can
        /// be saved, but only after prompting (once for all unviewed fields) or 
        /// <see langword="InvalidDataSaveMode.Disallow"/> if all data must meet validation
        /// requirements before saving.
        /// </returns>
        [Category("Data Entry Control Host")]
        public InvalidDataSaveMode InvalidDataSaveMode
        {
            get
            {
                return _invalidDataSaveMode;
            }

            set
            {
                _invalidDataSaveMode = value;
            }
        }

        /// <summary>
        /// Gets or sets one or more colors to use to highlight data in the image viewer or indicate
        /// the active status of data in a control.
        /// </summary>
        /// <value>One or more colors to use to highlight data in the image viewer or indicate
        /// the active status of data in a control.
        /// <para><b>Requirements:</b></para>
        /// <list type="bullet">
        /// <bullet>Must not be <see langword="null"/></bullet>
        /// <bullet>Must contain at least one <see cref="HighlightColor"/>.</bullet>
        /// <bullet><see cref="HighlightColor"/>s must be specified in ascending order of OCR
        /// confidence.</bullet>
        /// <bullet>The OCR confidence maximum for each <see cref="HighlightColor"/> must be > 0.
        /// </bullet>
        /// <bullet>The last <see cref="HighlightColor"/> must have an OCR confidence maximum of 100.
        /// </bullet>
        /// </list></value>
        /// <returns>One or more colors to use to highlight data in the image viewer or indicate
        /// the active status of data in a control.</returns>
        // If I make this property a collection as .NET suggests, for some reason the field shows
        // up as read-only in the property viewer (even though DataEntryTwoColumnTable.Rows uses
        // the same idea).
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [Category("Data Entry Control Host")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public HighlightColor[] HighlightColors
        {
            get
            {
                return _highlightColors;
            }

            set
            {
                try
                {
                    int lastConfidenceLevel = 0;

                    ExtractException.Assert("ELI25380",
                        "At least one highlight color must be specified!",
                        value != null && value.Length > 0);

                    foreach (HighlightColor confidenceTier in value)
                    {
                        ExtractException.Assert("ELI25382",
                            "Highlights must be specified in ascending OCR confidence order!",
                            lastConfidenceLevel < confidenceTier.MaxOcrConfidence);

                        lastConfidenceLevel = confidenceTier.MaxOcrConfidence; 
                    }

                    ExtractException.Assert("ELI25383",
                        "The last highlight color must be associated with a max OCR confidence of 100!",
                        value[value.Length - 1].MaxOcrConfidence == 100);

                    // Apply the supplied colors and initialize the default color.
                    _highlightColors = value;
                    _defaultHighlightColor = _highlightColors[_highlightColors.Length - 1].Color;

                    // Initialize _confidenceBoundaries
                    _confidenceBoundaries = (VariantVector)new VariantVectorClass();
                    foreach (HighlightColor confidenceTier in _highlightColors)
                    {
                        _confidenceBoundaries.PushBack(confidenceTier.MaxOcrConfidence);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25384", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the title of the current DataEntry application.
        /// </summary>
        /// <value>The title of the current DataEntry application.</value>
        /// <returns>The title of the current DataEntry application.</returns>
        public string ApplicationTitle
        {
            get
            {
                return _applicationTitle;
            }

            set
            {
                _applicationTitle = value;
            }
        }

        /// <summary>
        /// Gets or sets a comma separated list of names of <see cref="IDataEntryControl"/>s that
        /// should remain disabled at all times.
        /// </summary>
        /// <value>A comma separated list of names of <see cref="IDataEntryControl"/>s.</value>
        /// <returns>A comma separated list of names of <see cref="IDataEntryControl"/>s.</returns>
        public string DisabledControls
        {
            get
            {
                try
                {
                    string disabledControls = "";

                    foreach (string disabledControl in _disabledControls)
                    {
                        if (!string.IsNullOrEmpty(disabledControls))
                        {
                            disabledControls += ",";
                        }

                        disabledControls += disabledControl;
                    }

                    return disabledControls;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26655", ex);
                }
            }

            set
            {
                try
                {
                    _disabledControls.Clear();
                    _disabledControls.AddRange(
                        value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26656", ex);
                }
            }
        }

        /// <summary>
        /// Specifies the database connection to be used in data validation or auto-update queries.
        /// </summary>
        /// <value>The <see cref="DbConnection"/> to be used. (Can be <see langword="null"/> if one
        /// is not required by the DEP).</value>
        /// <returns>The <see cref="DbConnection"/> in use or <see langword="null"/> if none
        /// has been specified.</returns>
        public DbConnection DatabaseConnection
        {
            get
            {
                return _dbConnection;
            }

            set
            {
                _dbConnection = value;
            }
        }

        #endregion Properties

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the <see cref="ImageViewer"/> with which to display the document corresponding to data
        /// contained in the <see cref="DataEntryControlHost"/>'s data controls.
        /// </summary>
        /// <value>Sets the <see cref="ImageViewer"/> used to display the open document. <see langword="null"/>
        /// to disconnect the <see cref="DataEntryControlHost"/> from the image viewer.</value>
        /// <returns>The <see cref="ImageViewer"/> used to display the open document. <see langword="null"/> 
        /// if no connections are established.</returns>
        /// <seealso cref="IImageViewerControl"/>
        [Browsable(false)]
        public virtual ImageViewer ImageViewer
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
                        _imageViewer.CursorToolChanged -= HandleCursorToolChanged;
                        _imageViewer.LayerObjects.LayerObjectAdded -= HandleLayerObjectAdded;
                        _imageViewer.PreviewKeyDown -= HandleImageViewerPreviewKeyDown;
                        _imageViewer.SelectionToolEnteredLayerObject -= HandleSelectionToolEnteredLayerObject;
                        _imageViewer.SelectionToolLeftLayerObject -= HandleSelectionToolLeftLayerObject;
                        _imageViewer.MouseDown -= HandleImageViewerMouseDown;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;

                    // Check if an image viewer was specified
                    if (_imageViewer != null)
                    {
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;
                        _imageViewer.CursorToolChanged += HandleCursorToolChanged;
                        _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;
                        _imageViewer.PreviewKeyDown += HandleImageViewerPreviewKeyDown;
                        _imageViewer.SelectionToolEnteredLayerObject += HandleSelectionToolEnteredLayerObject;
                        _imageViewer.SelectionToolLeftLayerObject += HandleSelectionToolLeftLayerObject;
                        _imageViewer.MouseDown += HandleImageViewerMouseDown;

                        _imageViewer.DefaultHighlightColor = _defaultHighlightColor;
                    }
                }
                catch (Exception e)
                {

                    ExtractException ee = new ExtractException("ELI23665",
                        "Unable to establish connection to image viewer.", e);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members

        #region IMessageFilter Members

        /// <summary>
        /// PreFilterMessage is used to keep track of the states of the shift and tab keys.
        /// </summary>
        /// <param name="m">The <see cref="Message"/> that contains data about the message
        /// to be filtered.</param>
        /// <returns><see langword="true"/> if the message has been handled and should not be
        /// dispatched, <see langword="false"/> if the message should be dispatched.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool PreFilterMessage(ref Message m)
        {
            try
            {
                // If in design mode, just return false
                if (_inDesignMode)
                {
                    return false;
                }

                if (m.Msg == _WM_KEYDOWN || m.Msg == _WM_KEYUP)
                {
                    // Check for shift or tab key press events
                    if (m.WParam == (IntPtr)Keys.ShiftKey)
                    {
                        // Set or clear _shiftKeyDown depending upon whether this is a keydown
                        // or keyup event.
                        _shiftKeyDown = (m.Msg == _WM_KEYDOWN);
                    }
                    else if (m.WParam == (IntPtr)Keys.Tab)
                    {
                        // [DataEntry:346]
                        // If a shift tab is being sent via KeyMethods.SendKeyToControl the tab key
                        // will be received before the shift key. If _shiftKeyDown if false, test
                        // using Control.ModifierKeys to help ensure that shift is not missed.
                        if (!_shiftKeyDown)
                        {
                            _shiftKeyDown = (Control.ModifierKeys == Keys.Shift);
                        }

                        // If the tab key was pressed, indicate a manual focus event and
                        // propagate selection to the next attribute in the tab order.
                        if (m.Msg == _WM_KEYDOWN)
                        {
                            // Indicate a manual focus event so that HandleControlGotFocus allows the
                            // new attribute selection rather than overriding it.
                            _manualFocusEvent = true;

                            Stack<IAttribute> nextTabStopAttribute =
                                AttributeStatusInfo.GetNextTabStopAttribute(_attributes,
                                    ActiveAttributeGenealogy(), !_shiftKeyDown);

                            if (nextTabStopAttribute != null)
                            {
                                PropagateAttributes(nextTabStopAttribute, true);
                            }

                            return true;
                        }
                    }
                }
                else if (!base.ContainsFocus &&
                         (m.Msg == _WM_LBUTTONDOWN || m.Msg == _WM_RBUTTONDOWN))
                {
                    // Attempt to find a data entry control that should receive active status and 
                    // focus as the result of the mouse click.
                    _clickedDataEntryControl = FindClickedDataEntryControl(m);
                }
                else if (m.Msg == _WM_LBUTTONUP || m.Msg == _WM_RBUTTONUP)
                {
                    // Make sure to clear the _clickedDataEntryControl on mouse up. 
                    _clickedDataEntryControl = null;
                }
                else if (m.Msg == _WM_MOUSEWHEEL)
                {
                    // [DataEntry:302]
                    // If the mouse is over the image viewer, select the image viewer and allow it to 
                    // handle the mouse wheel event instead.
                    if (_imageViewer != null && _imageViewer.IsImageAvailable && !_imageViewer.Focused &&
                            _imageViewer.ClientRectangle.Contains(
                                _imageViewer.PointToClient(Control.MousePosition)))
                    {
                        _imageViewer.Select();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI24055", ex);
            }

            return false;
        }

        #endregion IMessageFilter Members

        #region Methods

        /// <summary>
        /// Selects and activates the next unviewed <see cref="IAttribute"/>.
        /// </summary>
        public void GoToNextUnviewed()
        {
            try
            {
                // The shortcut keys will remain enabled for this option; ignore the command if
                // there is no document loaded.
                if (!_imageViewer.IsImageAvailable)
                {
                    return;
                }

                using (new TemporaryWaitCursor())
                {
                    // Attempt to find and propagate the next unviewed attribute
                    if (GetNextUnviewedAttribute() == null)
                    {
                        MessageBox.Show(this, "There are no unviewed items.", _applicationTitle,
                            MessageBoxButtons.OK, MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1, 0);

                        // If we failed to find any unviewed attributes, make sure 
                        // _unviewedAttributeCount is zero and raise the UnviewedItemsFound event to 
                        // indicate no unviewed items are available.
                        _unviewedAttributeCount = 0;
                        OnUnviewedItemsFound(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24645", ex);
            }
        }

        /// <summary>
        /// Selects and activates the next <see cref="IAttribute"/> whose data currently fails
        /// validation.
        /// </summary>
        public void GoToNextInvalid()
        {
            try
            {
                // The shortcut keys will remain enabled for this option; ignore the command if
                // there is no document loaded.
                if (!_imageViewer.IsImageAvailable)
                {
                    return;
                }

                using (new TemporaryWaitCursor())
                {
                    if (GetNextInvalidAttribute() == null)
                    {
                        MessageBox.Show(this, "There are no invalid items.", _applicationTitle,
                            MessageBoxButtons.OK, MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1, 0);

                        // If we failed to find any attributes with invalid data, make sure 
                        // _invalidAttributeCount is zero and raise the InvalidItemsFound event to 
                        // indicate no invalid items remain.
                        _invalidAttributeCount = 0;
                        OnInvalidItemsFound(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24646", ex);
            }
        }

        /// <summary>
        /// Commands the <see cref="DataEntryControlHost"/> to finalize the attribute vector for 
        /// output. This primarily consists of asking each <see cref="IDataEntryControl"/> to 
        /// validate that the data it contains conforms to any validation rules that have been 
        /// applied to it. If so, the vector of attributes as it currently stands is output.
        /// </summary>
        /// <returns><see langword="true"/> if the document's data was successfully saved.
        /// <see langword="false"/> if the data was not saved (such as when data fails validation).
        /// </returns>
        public bool SaveData()
        {
            try
            {
                if (_imageViewer.IsImageAvailable)
                {
                    using (new TemporaryWaitCursor())
                    {
                        // Notify AttributeStatusInfo that the current edit is over so that a
                        // non-incremental value modified event can be raised.
                        AttributeStatusInfo.EndEdit();

                        if (DataCanBeSaved())
                        {
                            // Create a copy of the data to be saved so that attributes that should
                            // not be persisted can be removed.
                            ICopyableObject copyThis = (ICopyableObject)_attributes;
                            IUnknownVector dataCopy = (IUnknownVector)copyThis.Clone();

                            PruneNonPersistingAttributes(dataCopy);

                            // If all attributes passed validation, save the data.
                            dataCopy.SaveTo(_imageViewer.ImageFile + ".voa", true);

                            _dirty = false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI23909", "Unable to save data!", ex);
                ee.AddDebugData("Filename", _imageViewer.ImageFile, false);
                throw ee;
            }

            return true;
        }

        /// <summary>
        /// Toggles whether or not tooltip(s) for the active <see cref="IAttribute"/> are currently
        /// visible.
        /// </summary>
        public void ToggleHideTooltips()
        {
            try
            {
                if (!_temporarilyHidingTooltips)
                {
                    // Remove all tooltips
                    List<IAttribute> tooltipAttributes = new List<IAttribute>(_attributeToolTips.Keys);
                    foreach (IAttribute attribute in tooltipAttributes)
                    {
                        RemoveAttributeToolTip(attribute);
                    }

                    // [DataEntry:307]
                    // Keep tooltips from re-appearing too readily after pressing esc.
                    _temporarilyHidingTooltips = true;

                    _imageViewer.Invalidate();
                }
                else
                {
                    _temporarilyHidingTooltips = false;

                    DrawHighlights(false);
                }
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI24987", ex).Display();
            }
        }

        /// <summary>
        /// Confirms the spatial info for all currently selected highlights. Highlights indicating
        /// low confidence OCR results will turn all green. Direct hints will become true spatial
        /// strings with a solid color highlight.
        /// </summary>
        public void AcceptSpatialInfo()
        {
            try
            {
                // Keep track if any attributes were updated.
                bool spatialInfoConfirmed = false;

                // Loop through every attribute in the active control.
                List<IAttribute> activeAttributes;
                if (_activeDataControl != null &&
                    _controlAttributes.TryGetValue(_activeDataControl, out activeAttributes))
                {
                    foreach (IAttribute attribute in activeAttributes)
                    {
                        // If the attribute has a spatial attribute, but the attribute's value has
                        // not yet been accepted, interpret the edit as implicit acceptance of the
                        // attribute's value.
                        if (AttributeStatusInfo.GetHintType(attribute) == HintType.None &&
                            !AttributeStatusInfo.IsAccepted(attribute))
                        {
                            AttributeStatusInfo.AcceptValue(attribute, true);

                            // Re-create the highlight
                            RemoveAttributeHighlight(attribute);
                            SetAttributeHighlight(attribute, true);

                            spatialInfoConfirmed = true;
                        }
                    }
                }

                // Re-display the highlights if changes were made.
                if (spatialInfoConfirmed)
                {
                    DrawHighlights(false);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25977", ex);
            }
        }

        /// <summary>
        /// Removes all spatial information associated with an <see cref="IAttribute"/>.
        /// </summary>
        public void RemoveSpatialInfo()
        {
            try
            {
                // Keep track if any attributes were updated.
                bool spatialInfoRemoved = false;

                // Loop through every attribute in the active control.
                List<IAttribute> activeAttributes;
                if (_activeDataControl != null &&
                    _controlAttributes.TryGetValue(_activeDataControl, out activeAttributes))
                {
                    foreach (IAttribute attribute in activeAttributes)
                    {
                        // If the attribute has spatial information (a highlight), remove it and
                        // flag the attribute so that hints are not created in its place.
                        if (attribute.Value.HasSpatialInfo())
                        {
                            RemoveAttributeHighlight(attribute);
                            AttributeStatusInfo.EnableHint(attribute, false);
                            attribute.Value.DowngradeToNonSpatialMode();
                            spatialInfoRemoved = true;
                        }
                        // If the attribute has an associated hint, remove the hint and flag the
                        // attribute so that hints are not re-created. 
                        else if (AttributeStatusInfo.GetHintType(attribute) != HintType.None)
                        {
                            RemoveAttributeHighlight(attribute);
                            AttributeStatusInfo.EnableHint(attribute, false);
                            spatialInfoRemoved = true;
                        }
                    }
                }

                // Re-display the highlights if changes were made.
                if (spatialInfoRemoved)
                {
                    DrawHighlights(false);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25978", ex);
            }
        }

        /// <summary>
        /// Resets any existing highlight data and clears the attributes from all controls.
        /// </summary>
        public void ClearData()
        {
            try
            {
                // Dispose of all tooltips
                List<IAttribute> tooltipAttributes = new List<IAttribute>(_attributeToolTips.Keys);
                foreach (IAttribute attribute in tooltipAttributes)
                {
                    RemoveAttributeToolTip(attribute);
                }

                // Dispose of all error icons
                List<IAttribute> errorIconAttributes = new List<IAttribute>(_attributeErrorIcons.Keys);
                foreach (IAttribute attribute in errorIconAttributes)
                {
                    RemoveAttributeErrorIcon(attribute);
                }

                // Dispose of all icons
                foreach (CompositeHighlightLayerObject highlight in _highlightAttributes.Keys)
                {
                    if (_imageViewer.LayerObjects.Contains(highlight))
                    {
                        _imageViewer.LayerObjects.Remove(highlight);
                    }

                    highlight.Dispose();
                }
                _highlightAttributes.Clear();

                // Reset the other attribute mapping fields.
                _controlAttributes.Clear();
                _controlToolTipAttributes.Clear();
                _hoverAttribute = null;
                _displayedAttributeHighlights.Clear();
                _attributeHighlights.Clear();

                if (_attributes != null && _attributes.Size() != 0)
                {
                    // Clear any existing attributes.
                    _attributes = (IUnknownVector)new IUnknownVectorClass();

                    // Clear any existing data in the controls.
                    foreach (IDataEntryControl dataControl in _rootLevelControls)
                    {
                        dataControl.SetAttributes(null);
                    }
                }

                // Clear any data the controls have cached.
                foreach (IDataEntryControl dataControl in _dataControls)
                {
                    dataControl.ClearCachedData();
                }

                _selectedAttributesWithAcceptedHighlights = 0;
                _selectedAttributesWithUnacceptedHighlights = 0;
                _selectedAttributesWithDirectHints = 0;
                _selectedAttributesWithIndirectHints = 0;
                _selectedAttributesWithoutHighlights = 0;

                // Raise the ItemSelectionChanged event to notify listeners that there are no
                // longer attributes selected.
                OnItemSelectionChanged();

                // Reset the unviewed attribute count.
                _unviewedAttributeCount = 0;
                OnUnviewedItemsFound(false);

                // Reset the invalid attribute count.
                _invalidAttributeCount = 0;
                OnInvalidItemsFound(false);

                // Clear the AFUtility instance: the form is running in a single threaded apartment so
                // AFUtility will not be able to be used when another form is created in another
                // apartment.
                DataEntryMethods.AFUtility = null;

                // Reset the map of error icon sizes to use.
                _errorIconSizes.Clear();

                // AttributeStatusInfo cannot persist any data from one document to the next as it can
                // cause COM threading exceptions in FAM mode. Unload its data now.
                AttributeStatusInfo.ResetData(null, null);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25976", ex);
            }
        }

        #endregion Methods

        #region Events

        /// <summary>
        /// Fired when a <see cref="IDataEntryControl"/> has been activated or manipulated in such 
        /// a way that swiping should be either enabled or disabled.
        /// </summary>
        public event EventHandler<SwipingStateChangedEventArgs> SwipingStateChanged;

        /// <summary>
        /// Fired to indicate that unviewed <see cref="IAttribute"/>s are either known to exist or
        /// not to exist.
        /// </summary>
        public event EventHandler<UnviewedItemsFoundEventArgs> UnviewedItemsFound;

        /// <summary>
        /// Fired to indicate that <see cref="IAttribute"/>s with invalid data are either known to
        /// exist or not to exist.
        /// </summary>
        public event EventHandler<InvalidItemsFoundEventArgs> InvalidItemsFound;

        /// <summary>
        /// Fired to notify listeners that a new set of items has been selected.
        /// </summary>
        public event EventHandler<ItemSelectionChangedEventArgs> ItemSelectionChanged;

        #endregion Events

        #region Overrides

        /// <summary>
        /// This call will initialize the data entry control relationships and register all
        /// needed events to be passed to and from the data entry controls.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // Call the base OnLoad method
                base.OnLoad(e);

                // So that PreMessageFilter is called
                Application.AddMessageFilter(this);

                ExtractException.Assert("ELI25377", "Highlight colors not initialized!",
                    _highlightColors != null && _highlightColors.Length > 0);

                AttributeStatusInfo.AttributeInitialized += HandleAttributeInitialized;
                AttributeStatusInfo.ViewedStateChanged += HandleViewedStateChanged;
                AttributeStatusInfo.ValidationStateChanged += HandleValidationStateChanged;

                // Loop through all contained controls looking for controls that implement the 
                // IDataEntryControl interface.  Registers events necessary to facilitate
                // the flow of information between the controls.
                RegisterDataEntryControls(this);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23679", ex);
                ee.AddDebugData("Event Arguments", e, false);
                throw ee;
            }
        }

        /// <summary>
        /// Override <see cref="Control.OnEnter"/> to keep track of when focus had belonged to 
        /// a control outside of the control host, but is now returning to a control within the 
        /// control host.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            _regainingFocus = true;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed; 
        /// otherwise, <see langword="false"/>.</param>
        protected override void Dispose(bool disposing)
        {
            // Unregister events from data entry control before proceeding with the dispose process
            // (which would otherwise trigger during the dispose process)
            UnregisterDataEntryControls();

            // Call base class dispose first since it may trigger events that have dependencies on
            // this classes' disposable fields (specifically _toolTipFont)
            base.Dispose(disposing);

            // Dispose of managed resources
            if (disposing)
            {
                AttributeStatusInfo.AttributeInitialized -= HandleAttributeInitialized;
                AttributeStatusInfo.ViewedStateChanged -= HandleViewedStateChanged;
                AttributeStatusInfo.ValidationStateChanged -= HandleValidationStateChanged;

                // Dispose of managed objects
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_ocrManager != null)
                {
                    _ocrManager.Dispose();
                    _ocrManager = null;
                }

                if (_errorProvider != null)
                {
                    _errorProvider.Dispose();
                    _errorProvider = null;
                }

                if (_toolTipFont != null)
                {
                    _toolTipFont.Dispose();
                    _toolTipFont = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the case that a new document has been opened or that a document has been closed
        /// by updating the <see cref="IDataEntryControl"/>s with any new document's associated 
        /// data. (in the form of an <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s)
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="ImageFileChangedEventArgs"/> that contains the event data.
        /// </param>
        private void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            // Set flag to indicate that a document change is in progress so that highlights
            // are not redrawn as the spatial info of the controls are updated.
            _changingImage = true;

            try
            {
                using (new TemporaryWaitCursor())
                {
                    // Prevent updates to the controls during the attribute propagation
                    // that will occur as data is loaded.
                    LockControlUpdates(true);

                    // Ensure the data in the contols is cleared prior to loading any new data.
                    ClearData();

                    bool imageIsAvailable = _imageViewer.IsImageAvailable;

                    if (imageIsAvailable)
                    {
                        // Calculate the size the error icon for invalid data should be on each
                        // page and create a SpatialPageInfo entry for each page.
                        for (int page = 1; page <= _imageViewer.PageCount; page++)
                        {
                            _imageViewer.SetPageNumber(page, false, false);
                            _errorIconSizes[page] = new Size(
                                (int)(_ERROR_ICON_SIZE * _imageViewer.ImageDpiX),
                                (int)(_ERROR_ICON_SIZE * _imageViewer.ImageDpiY));
                        }
                        _imageViewer.SetPageNumber(1, false, false);

                        // If an image was loaded, look for and attempt to load corresponding data.
                        string dataFilename = e.FileName + ".voa";

                        if (File.Exists(dataFilename))
                        {
                            _attributes.LoadFrom(e.FileName + ".voa", false);
                        }

                        // Notify AttributeStatusInfo of the new attribute hierarchy
                        AttributeStatusInfo.ResetData(_attributes, _dbConnection);

                        // Enable or disable swiping as appropriate.
                        bool swipingEnabled = _activeDataControl != null &&
                                              _activeDataControl.SupportsSwiping;

                        OnSwipingStateChanged(new SwipingStateChangedEventArgs(swipingEnabled));

                        // Populate all root level data with the retrieved data.
                        foreach (IDataEntryControl dataControl in _rootLevelControls)
                        {
                            dataControl.SetAttributes(_attributes);
                        }
                    }

                    // Mark any root-level attributes as propagated if they were not mapped to a 
                    // control.
                    int attributeCount = _attributes.Size();
                    for (int i = 0; i < attributeCount; i++)
                    {
                        IAttribute attribute = (IAttribute)_attributes.At(i);
                        if (AttributeStatusInfo.GetStatusInfo(attribute).OwningControl == null)
                        {
                            AttributeStatusInfo.MarkAsPropagated(attribute, true, true);
                        }
                    }

                    // Enable/Disable all data controls per imageIsAvailable
                    foreach (IDataEntryControl dataControl in _dataControls)
                    {
                        // Remove activate status from the active control.
                        if (!imageIsAvailable && dataControl == _activeDataControl)
                        {
                            dataControl.IndicateActive(false, _imageViewer.DefaultHighlightColor);
                        }

                        // Set the enabled status of every data control depending on the 
                        // availability of data.
                        Control control = (Control)dataControl;

                        control.Enabled = imageIsAvailable && !dataControl.Disabled;
                    }

                    // For as long as unpropagated attributes are found, propagate them and their 
                    // subattributes so that all attributes that can be are mapped into controls.
                    // This enables the entire attribute tree to be navigated forward and backward 
                    // for all types of AttributeStatusInfo scans).
                    Stack<IAttribute> unpropagatedAttributeGenealogy = new Stack<IAttribute>();
                    while (!AttributeStatusInfo.HasBeenPropagated(_attributes, null,
                        unpropagatedAttributeGenealogy))
                    {
                        PropagateAttributes(unpropagatedAttributeGenealogy, false);
                        unpropagatedAttributeGenealogy.Clear();
                    }

                    // [DataEntry:166]
                    // Re-propagate the attributes that were originally propagated.
                    foreach (IDataEntryControl dataEntryControl in _rootLevelControls)
                    {
                        dataEntryControl.PropagateAttribute(null, false);
                    }

                    // Count the number of unviewed attributes in the newly loaded data.
                    _unviewedAttributeCount = CountUnviewedItems();
                    OnUnviewedItemsFound(_unviewedAttributeCount != 0);

                    // Count the number of invalid attributes in the newly loaded data.
                    _invalidAttributeCount = CountInvalidItems();
                    OnInvalidItemsFound(_invalidAttributeCount != 0);

                    // Create highlights for all attributes as long as a document is loaded.
                    if (imageIsAvailable)
                    {
                        CreateAllAttributeHighlights(_attributes);
                    }

                    _refreshActiveControlHighlights = false;
                    _dirty = false;

                    _changingImage = false;

                    DrawHighlights(true);

                    // [DataEntry:432]
                    // Some tasks (such as selecting the first control), must take place after the
                    // ImageFileChanged event is complete. Use BeginInvoke to schedule
                    // FinalizeDocumentLoad at the end of the current message queue.
                    base.BeginInvoke(new ParameterlessDelegate(FinalizeDocumentLoad));
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // If any problem was encountered loading the data, clear the data again
                    // to ensure the controls are in a useable state.
                    ClearData();
                }
                catch (Exception ex2)
                {
                    ExtractException.Log("ELI24013", ex2);
                }

                // Ensure that the _changingImage flag does not remain set.
                _changingImage = false;

                ExtractException ee = new ExtractException("ELI23919", "Failed to load data!", ex);
                ee.AddDebugData("FileName", e.FileName, false);
                ee.Display();
            }
            finally
            {
                LockControlUpdates(false);
            }
        }

        /// <summary>
        /// Handles a new cursor tool being selected so that we can keep track of the most recently
        /// used cursor tool.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="CursorToolChangedEventArgs"/> that contains the event data.
        /// </param>
        private void HandleCursorToolChanged(object sender, CursorToolChangedEventArgs e)
        {
            try
            {
                if (e.CursorTool != CursorTool.None)
                {
                    _lastCursorTool = e.CursorTool;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24092", ex);
            }
        }

        /// <summary>
        /// Handles the case that a data entry control has received focus
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleControlGotFocus(object sender, EventArgs e)
        {
            try
            {
                Control lastActiveDataControl = (Control) _activeDataControl;

                IDataEntryControl newActiveDataControl = sender as IDataEntryControl;

                // If a manual focus event is in progress, don't treat this as a regaining focus
                // event. The sender is the control programatically given focus.
                if (_manualFocusEvent)
                {
                    _regainingFocus = false;
                    _manualFocusEvent = false;
                }
                // If the control host is getting focus back from an outside control, focus needs 
                // to be manually directred to the appropriate data entry control to override the 
                // default tab behavior which will otherwise assign focus to the first control 
                // within the control host.
                else if (_regainingFocus)
                {
                    _regainingFocus = false;

                    // [DataEntry:182]
                    // If focus returned via a mouse press and we have already determined which
                    // data entry control should receive focus as a result, set focus back to that
                    // control.
                    if (_clickedDataEntryControl != null)
                    {
                        newActiveDataControl = _clickedDataEntryControl;
                        ((Control)_clickedDataEntryControl).Focus();
                    }
                    else if (lastActiveDataControl != null)
                    {
                        // Return focus to the control that previously had it.
                        newActiveDataControl = (IDataEntryControl)lastActiveDataControl;
                        lastActiveDataControl.Focus();
                    }
                }

                if (newActiveDataControl == null || _activeDataControl == newActiveDataControl)
                {
                    // This control is already active or the control that gained focus is not a data
                    // entry control there is nothing to do.
                    return;
                }

                // De-activate any existing control that is active
                if (_activeDataControl != null)
                {
                    _activeDataControl.IndicateActive(false, _imageViewer.DefaultHighlightColor);
                }

                // Notify AttributeStatusInfo that the current edit is over so that a
                // non-incremental value modified event can be raised.
                AttributeStatusInfo.EndEdit();

                // Activate the new control
                _activeDataControl = newActiveDataControl;
                _activeDataControl.IndicateActive(true, _imageViewer.DefaultHighlightColor);

                // Once a new control gains focus, show tooltips again if they were hidden.
                _temporarilyHidingTooltips = false;

                DrawHighlights(true);

                // Enable or disable swiping as appropriate.
                bool swipingEnabled = _imageViewer.IsImageAvailable &&
                                      _activeDataControl != null &&
                                      _activeDataControl.SupportsSwiping;

                OnSwipingStateChanged(new SwipingStateChangedEventArgs(swipingEnabled));

                OnItemSelectionChanged();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24093", ex);
            }
            finally
            {
                _regainingFocus = false;
            }
        }

        /// <summary>
        /// Handles the case that a new <see cref="IAttribute"/> is selected within a control. This
        /// can occur as part of the <see cref="IDataEntryControl.PropagateAttributes"/> event, or
        /// it can also occur when a new piece of the <see cref="IDataEntryControl"/> becomes
        /// active.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="AttributesSelectedEventArgs"/> that contains the event
        /// data.</param>
        /// <seealso cref="IDataEntryControl"/>
        private void HandleAttributesSelected(object sender, AttributesSelectedEventArgs e)
        {
            try
            {
                IDataEntryControl dataControl = (IDataEntryControl)sender;

                // Notify AttributeStatusInfo that the current edit is over so that a
                // non-incremental value modified event can be raised.
                AttributeStatusInfo.EndEdit();

                // Create lists to store the new active attributes and attributes that need tooltips.
                // (must be created before the UpdateControlAttributes which is recursive)
                _controlAttributes[dataControl] = new List<IAttribute>();
                _controlToolTipAttributes[dataControl] = new List<IAttribute>();

                // Once a new attribute is selected within a control, show tooltips again if they
                // were hidden.
                _temporarilyHidingTooltips = false;

                UpdateControlAttributes(dataControl, e.Attributes, e.IncludeSubAttributes,
                    e.DisplayToolTips);

                // If this is the active control and the image page is not in the process of being
                // changed, redraw all highlights.
                if (!_changingImage)
                {
                    DrawHighlights(true);
                }

                OnItemSelectionChanged();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24094", ex);
            }
        }

        /// <summary>
        /// Handles the case that data was modified in order to update the dirty flag and any
        /// associated tooltip.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="AttributeValueModifiedEventArgs"/> that contains the
        /// event data.
        /// </param>
        private void HandleAttributeValueModified(object sender, AttributeValueModifiedEventArgs e)
        {
            try
            {
                _dirty = true;

                // Update the attribute's highlights if the modification is not happening during 
                // image loading, and the update is coming from the active data control.
                // [DataEntry:329]
                // Don't accept the text value if the value modification is happening as a
                // result of a swipe.
                if (!_changingImage && !_processingSwipe && e.AcceptSpatialInfo)
                {
                    List<IAttribute> activeAttributes;
                    _controlAttributes.TryGetValue(_activeDataControl, out activeAttributes);

                    // For any attributes that have hints or that had not previously been accepted,
                    // reset their highlights to reflect 100 percent confidence. (The color of a
                    // hint will depend upon whether text has been entered)
                    if (activeAttributes != null && activeAttributes.Contains(e.Attribute) &&
                        (AttributeStatusInfo.GetHintType(e.Attribute) != HintType.None ||
                         !AttributeStatusInfo.IsAccepted(e.Attribute)))
                    {
                        AttributeStatusInfo.AcceptValue(e.Attribute, true);

                        RemoveAttributeHighlight(e.Attribute);

                        // [DataEntry:261] Highlights should be made visible except indirect
                        // hints when multiple attributes are active.
                        bool makeVisible = ((activeAttributes.Count == 1) || 
                             AttributeStatusInfo.GetHintType(e.Attribute) != HintType.Indirect);

                        SetAttributeHighlight(e.Attribute, makeVisible);
                    }

                    // Always redraw the highlights in order to create/update the tooltip of the
                    // attribute that changed (even if it is not the only one selected)
                    DrawHighlights(false);

                    _imageViewer.Invalidate();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24980", ex);
            }
        }

        /// <summary>
        /// Handles the case that one or more <see cref="IAttribute"/>s were deleted from a
        /// <see cref="IDataEntryControl"/> so that the unviewed and invalid attribute counts can
        /// be updated to reflect the deletions.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="AttributeDeletedEventArgs"/> that contains the event data.
        /// </param>
        private void HandleAttributeDeleted(object sender, AttributeDeletedEventArgs e)
        {
            try
            {
                _dirty = true;

                // A refresh of highlights is needed now that attributes have been deleted.
                _refreshActiveControlHighlights = true;

                ProcessDeletedAttributes(DataEntryMethods.AttributeAsVector(e.DeletedAttribute));
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24918", ex);
            }
        }

        /// <summary>
        /// Handles the case that an image region was highlighted.  The contents of the image region
        /// are to be OCR'd so that the recognized text can be assigned to the active control's
        /// <see cref="IAttribute"/> value.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="LayerObjectAddedEventArgs"/> that contains the event data.
        /// </param>
        private void HandleLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                // [DataEntry:185]
                // If a swipe was cancelled by pressing the right mouse button, discard the layer
                // object, clear the cancel flag, and return.
                if (_cancelingSwipe)
                {
                    _cancelingSwipe = false;

                    _imageViewer.LayerObjects.Remove(e.LayerObject);
                    e.LayerObject.Dispose();
                    return;
                }

                if (_activeDataControl != null && _activeDataControl.SupportsSwiping)
                {
                    Highlight highlight = e.LayerObject as Highlight;

                    if (highlight != null)
                    {
                        _processingSwipe = true;

                        // [DataEntry:269] Swipes should trigger document to be marked as dirty.
                        _dirty = true;

                        // TODO: Filter out swipes that are too small to limit unnecessary exceptions

                        // Recognize the text in the highlight's raster zone and send it to the active
                        // data control for processing.
                        using (new TemporaryWaitCursor())
                        {
                            SpatialString ocrText;

                            // TODO: [DataEntry:194] Temporarily suppress exceptions from the OCR manager
                            // to prevent possibility of exceptions during demo.
                            try
                            {
                                // [DataEntry:294] Keep the angle threshold small so long swipes on slightly
                                // skewed docs don't include more text than intended.
                                ocrText = _ocrManager.GetOcrText(
                                    _imageViewer.ImageFile, highlight.ToRasterZone(), 0.2);
                            }
                            catch (ExtractException ee)
                            {
                                ee.Log();
                                ocrText = new SpatialString();
                            }

                            _activeDataControl.ProcessSwipedText(ocrText);

                            _imageViewer.LayerObjects.Remove(e.LayerObject);

                            e.LayerObject.Dispose();

                            // Notify AttributeStatusInfo that the current edit is over so that a
                            // non-incremental value modified event can be raised.
                            AttributeStatusInfo.EndEdit();

                            // It is likely that the spatial information of the selected attributes
                            // changed as a result of the swipe; update the active attribute(s)'
                            // associated highlights.
                            _refreshActiveControlHighlights = true;

                            DrawHighlights(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (_processingSwipe)
                    {
                        _imageViewer.LayerObjects.Remove(e.LayerObject);
                        e.LayerObject.Dispose();
                    }
                }
                catch (Exception ex2)
                {
                    ExtractException.Log("ELI24089", ex2);
                }

                ExtractException.Display("ELI24090", ex);
            }
            finally
            {
                _processingSwipe = false;
            }
        }

        /// <summary>
        /// Propagates <see cref="SwipingStateChanged"/> events from 
        /// <see cref="IDataEntryControl"/>s so that registered listeners know whether the active 
        /// data entry control is an data entry control able to accept swiped input.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="SwipingStateChangedEventArgs"/> that indicates whether 
        /// swiping is being enabled or disabled.</param>
        private void HandleSwipingStateChanged(object sender, SwipingStateChangedEventArgs e)
        {
            try
            {
                if (sender == _activeDataControl)
                {
                    // Propagate the swiping state changed event.
                    OnSwipingStateChanged(e);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24091", ex);
            }
        }

        /// <summary>
        /// Handles the case that an <see cref="IAttribute"/> was initialized within the data entry
        /// framework in order to update unviewed/invalid counts and register for events.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="AttributeInitializedEventArgs"/> that contains the event
        /// data.</param>
        private void HandleAttributeInitialized(object sender, AttributeInitializedEventArgs e)
        {
            try
            {
                _dirty = true;

                if (!AttributeStatusInfo.HasBeenViewed(e.Attribute, false))
                {
                    UpdateUnviewedCount(true);
                }

                if (!AttributeStatusInfo.IsDataValid(e.Attribute))
                {
                    UpdateInvalidCount(true);
                }

                AttributeStatusInfo.GetStatusInfo(e.Attribute).AttributeValueModified +=
                    HandleAttributeValueModified;

                AttributeStatusInfo.GetStatusInfo(e.Attribute).AttributeDeleted +=
                    HandleAttributeDeleted;

                // As long as the attribute being added has spatial info, highlights for the
                // active control need to be refreshed.
                if (e.Attribute.Value.HasSpatialInfo())
                {
                    _refreshActiveControlHighlights = true;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24780", ex);
            }
        }

        /// <summary>
        /// Handles the case that an <see cref="IAttribute"/> that was previously marked as unviewed
        /// has now been viewed (or vice-versa).
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="ViewedStateChangedEventArgs"/> that contains the event data.
        /// </param>
        private void HandleViewedStateChanged(object sender,
            ViewedStateChangedEventArgs e)
        {
            try
            {
                // If the attribute is now marked as viewed.
                if (e.IsDataViewed)
                {
                    UpdateUnviewedCount(false);
                }
                // If the attribute is now marked as unviewed.
                else
                {
                    UpdateUnviewedCount(true);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24934", ex);
            }
        }

        /// <summary>
        /// Handles the case that an <see cref="IAttribute"/> that was previously marked as 
        /// containing invalid data now contains valid data (or vice-versa).
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="ValidationStateChangedEventArgs"/> that contains the event data.
        /// </param>
        private void HandleValidationStateChanged(object sender,
            ValidationStateChangedEventArgs e)
        {
            try
            {
                // Update the invalid count to reflect the new state of the attribute.
                UpdateInvalidCount(!e.IsDataValid);

                // Remove the image viewer error icon if the data is now valid.
                if (e.IsDataValid)
                {
                    RemoveAttributeErrorIcon(e.Attribute);
                }
                // Add an image viewer error icon if the data is now invalid.
                else
                {
                    CreateAttributeErrorIcon(e.Attribute, _showingAllHighlights);
                }

                DrawHighlights(false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24915", ex);
            }
        }

        /// <summary>
        /// Handles an <see cref="ImageViewer"/> PreviewKeyDown event in order to redirect special
        /// key input to the <see cref="DataEntryControlHost"/>'s active control.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="PreviewKeyDownEventArgs"/> that contains the event data.
        /// </param>
        private void HandleImageViewerPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            try
            {
                Control activeControl = _activeDataControl as Control;

                // Give focus back to the active control so it can receive input.
                // Allowing F10 to be passed to the DEP causes the file menu to be selected for some
                // reason... exempt F10 from being handled here.
                // [DataEntry:335] Don't handle tab key either since that already has special
                // handling in PreFilterMessage.
                if (!_imageViewer.Capture && activeControl != null && !activeControl.Focused &&
                    e.KeyCode != Keys.F10 && e.KeyCode != Keys.Tab)
                {
                    if (KeyMethods.SendKeyToControl(e.KeyValue, e.Shift, e.Control, e.Alt, activeControl))
                    {
                        // Allowing special key handling (when IsInputKey == false) seems to cause arrow
                        // keys as well as some shortcuts to do unexpected things.  Set IsInputKey to true 
                        // to disable special handling.
                        e.IsInputKey = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25050", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer"/> SelectionToolEnteredLayerObject event in order
        /// to display a tooltip for data the selection tool is currently hovering over.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="LayerObjectEventArgs"/> that contains the event data.
        /// </param>
        private void HandleSelectionToolEnteredLayerObject(object sender, LayerObjectEventArgs e)
        {
            try
            {
                CompositeHighlightLayerObject highlight = e.LayerObject as CompositeHighlightLayerObject;

                // Ensure the layer object entered was a CompositeHighlightLayerObject (as all attribute
                // highlights should be).
                if (highlight != null)
                {
                    // Ensure an associated attribute can be found and that the highlight does not
                    // represent an indirect hint.
                    IAttribute attribute;
                    if (_highlightAttributes.TryGetValue(highlight, out attribute) &&
                        AttributeStatusInfo.GetHintType(attribute) != HintType.Indirect)
                    {
                        // If there is not currently a hover attribute or the current hover attribute
                        // is a hint while the new candidate is not, use the new attribute as the hover
                        // attribute.
                        if (_hoverAttribute == null ||
                            AttributeStatusInfo.GetHintType(attribute) == HintType.None &&
                            AttributeStatusInfo.GetHintType(_hoverAttribute) != HintType.None)
                        {
                            _hoverAttribute = attribute;
                            DrawHighlights(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25421", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer"/> SelectionToolLeftLayerObject event in order
        /// to remove a tooltip for data the selection tool was previously hovering over.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="LayerObjectEventArgs"/> that contains the event data.
        /// </param>
        private void HandleSelectionToolLeftLayerObject(object sender, LayerObjectEventArgs e)
        {
            try
            {
                // If there is an existing hover attribute, we need to check to see if the layer
                // object the selection tool just left was the hover attribute's highlight.
                if (_hoverAttribute != null)
                {
                    CompositeHighlightLayerObject highlight =
                        e.LayerObject as CompositeHighlightLayerObject;

                    if (highlight != null)
                    {
                        // If the layer object the selection tool just left was a highight for the
                        // current hover attribute, clear the hover attribute.
                        IAttribute attribute;
                        if (_highlightAttributes.TryGetValue(highlight, out attribute) &&
                            attribute == _hoverAttribute)
                        {
                            _hoverAttribute = null;
                            DrawHighlights(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25422", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer"/>'s <see cref="Control.MouseDown"/> event in order
        /// to select an active hover attribute.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        private void HandleImageViewerMouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // Check to see that it was the left button that was pressed and that there is an
                // active hover attribute.
                if (e.Button == MouseButtons.Left && _hoverAttribute != null)
                {
                    using (new TemporaryWaitCursor())
                    {
                        // Obtain the complete genealogy needed to propagate the hover attribute.
                        Stack<IAttribute> attributesToPropagate =
                            GetAttributeGenealogy(_hoverAttribute);

                        // Selection is changing to the hover attribute, so it is no longer the
                        // hover attribute.
                        _hoverAttribute = null;

                        // Indicate a manual focus event so that HandleControlGotFocus allows the
                        // new attribute selection rather than overriding it.
                        _manualFocusEvent = true;

                        // Propagate and select the former hover attribute.
                        PropagateAttributes(attributesToPropagate, true);
                    }
                }
                else if (e.Button == MouseButtons.Right && _imageViewer.Capture && 
                    (Control.MouseButtons & MouseButtons.Left) != 0)
                {
                    // [DataEntry:185], [DataEntry:310]
                    // Allow the right mouse button to cancel the current swipe rather than allowing
                    // the swipe to end as if the left mouse button had been released.
                    _cancelingSwipe = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25415", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Loops through all controls contained in the specified control looking for controls 
        /// that implement the <see cref="IDataEntryControl"/> interface.  Registers events 
        /// necessary to facilitate the flow of information between the controls.
        /// <para><b>Requirements</b></para>
        /// Must be called before the _imageViewer loads a document.
        /// </summary>
        /// <param name="parentControl">The control for which <see cref="IDataEntryControl"/>s
        /// should be registered.</param>
        private void RegisterDataEntryControls(Control parentControl)
        {
            // Loop recursively through all contained controls looking for controls that implement
            // the IDataEntryControl interface
            foreach (Control control in parentControl.Controls)
            {
                // Register each child control.
                RegisterDataEntryControls(control);

                // Check to see if this control is an IDataEntryControl itself.
                IDataEntryControl dataControl = control as IDataEntryControl;
                if (dataControl != null)
                {
                    // Set the font of data controls to the _DATA_FONT_FAMILY.
                    control.Font = new Font(_DATA_FONT_FAMILY, base.Font.Size);

                    // Register for needed events from data entry controls
                    control.GotFocus += HandleControlGotFocus;
                    dataControl.SwipingStateChanged += HandleSwipingStateChanged;
                    dataControl.AttributesSelected += HandleAttributesSelected;

                    // Assign the error provider for the control (if required).
                    IRequiresErrorProvider errorControl = control as IRequiresErrorProvider;
                    if (errorControl != null)
                    {
                        errorControl.SetErrorProvider(_errorProvider);
                    }

                    if (dataControl.ParentDataEntryControl != null)
                    {
                        // Notify this control of new attributes that its parent wants Propagated.
                        dataControl.ParentDataEntryControl.PropagateAttributes +=
                            dataControl.HandlePropagateAttributes;
                    }
                    else
                    {
                        // Add this to the list of controls that need to be supplied data
                        // directly from the control host.
                        _rootLevelControls.Add(dataControl);
                    }

                    // Add this control to the master list of all data entry controls.
                    _dataControls.Add(dataControl);

                    // Disable all data controls until an image is available.
                    control.Enabled = false;

                    if (_disabledControls.Contains(control.Name))
                    {
                        dataControl.Disabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Unregisters the <see cref="DataEntryControlHost"/> from <see cref="IDataEntryControl"/>
        /// events.
        /// </summary>
        private void UnregisterDataEntryControls()
        {
            try
            {
                foreach (IDataEntryControl dataControl in _dataControls)
                {
                    dataControl.SwipingStateChanged -= HandleSwipingStateChanged;
                    dataControl.AttributesSelected -= HandleAttributesSelected;
                }
            }
            catch (Exception ex)
            {
                // This is called from Dispose, so don't throw an exception.
                ExtractException.AsExtractException("ELI25201", ex).Log();
            }
        }

        /// <summary>
        /// Notify registered listeners that a control has been activated or manipulated in such a 
        /// way that swiping should be either enabled or disabled.
        /// </summary>
        /// <param name="e">A <see cref="SwipingStateChangedEventArgs"/> describing whether or not
        /// swiping is to be enabled.</param>
        private void OnSwipingStateChanged(SwipingStateChangedEventArgs e)
        {
            if (e.SwipingEnabled && _imageViewer.CursorTool == CursorTool.None && 
                _imageViewer.IsImageAvailable)
            {
                // If swiping is being re-enabled and the previous active cursor tool was one of
                // the highlight tools, re-enable it so a user does not need to manually
                // re-active the highlight tool after tabbing past controls which don't support
                // swiping.

                if (_lastCursorTool == CursorTool.AngularHighlight)
                {
                    _imageViewer.CursorTool = CursorTool.AngularHighlight;
                }
                else if (_lastCursorTool == CursorTool.RectangularHighlight)
                {
                    _imageViewer.CursorTool = CursorTool.RectangularHighlight;
                }
            }

            // Raise the event for registered listeners
            if (this.SwipingStateChanged != null)
            {
                SwipingStateChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ItemSelectionChanged"/> event.
        /// </summary>
        private void OnItemSelectionChanged()
        {
            if (this.ItemSelectionChanged != null)
            {
                ItemSelectionChanged(this, new ItemSelectionChangedEventArgs(
                    _selectedAttributesWithAcceptedHighlights,
                    _selectedAttributesWithUnacceptedHighlights, 
                    _selectedAttributesWithDirectHints,_selectedAttributesWithIndirectHints,
                    _selectedAttributesWithoutHighlights));
            }
        }

        /// <summary>
        /// Raises the <see cref="UnviewedItemsFound"/> event.
        /// </summary>
        /// <param name="unviewedItemsFound"><see langword="true"/> if unviewed 
        /// <see cref="IAttribute"/>s are now known to exist, <see langword="false"/> if it is
        /// now known that all <see cref="IAttribute"/>s have been viewed.</param>
        private void OnUnviewedItemsFound(bool unviewedItemsFound)
        {
            if (this.UnviewedItemsFound != null)
            {
                UnviewedItemsFound(this, new UnviewedItemsFoundEventArgs(unviewedItemsFound));
            }
        }

        /// <summary>
        /// Raises the <see cref="InvalidItemsFound"/> event.
        /// </summary>
        /// <param name="invalidItemsFound"><see langword="true"/> if <see cref="IAttribute"/>s
        /// with invalid data are now known to exist, <see langword="false"/> if it is
        /// now known that all <see cref="IAttribute"/>s contain valid data.</param>
        private void OnInvalidItemsFound(bool invalidItemsFound)
        {
            if (this.InvalidItemsFound != null)
            {
                InvalidItemsFound(this, new InvalidItemsFoundEventArgs(invalidItemsFound));
            }
        }

        /// <summary>
        /// Renders the <see cref="CompositeHighlightLayerObject"/>s associated with the 
        /// <see cref="IDataEntryControl"/>s. 
        /// </summary>
        /// <param name="ensureActiveAttributeVisible">If <see langword="true"/>, the portion of
        /// the document currently in view will be adjusted to ensure all active attribute(s) and
        /// their associated tooltip is visible.  If <see langword="false"/> the view will be
        /// unchanged even if the attribute and/or tooltip is not currently in the view.</param>
        private void DrawHighlights(bool ensureActiveAttributeVisible)
        {
            // To avoid unnecessary drawing, wait until we are done loading a document before
            // attempting to display any layer objects.
            if (_changingImage)
            {
                return;
            }

            // Refresh the active control highlights if necessary.
            if (_refreshActiveControlHighlights)
            {
                RefreshActiveControlHighlights();
            }

            // Will be populated with the set of attributes to be highlighted by the end of this call.
            Dictionary<IAttribute, bool> newDisplayedAttributeHighlights =
                new Dictionary<IAttribute, bool>();

            // Keep track of the unified bounds of the highlights and any associated tooltip on
            // each page.
            Dictionary<int, Rectangle> unifiedBounds = new Dictionary<int, Rectangle>();

            // Reset the selected attribute counts before iterating the attributes
            _selectedAttributesWithAcceptedHighlights = 0;
            _selectedAttributesWithUnacceptedHighlights = 0;
            _selectedAttributesWithDirectHints = 0;
            _selectedAttributesWithIndirectHints = 0;
            _selectedAttributesWithoutHighlights = 0;

            int firstPageOfHighlights = -1;
            int pageToShow = -1;

            // Obtain the list of active attributes.
            List<IAttribute> attributes;
            if (_activeDataControl != null && 
                _controlAttributes.TryGetValue(_activeDataControl, out attributes))
            {
                // Obtain the list of highlights that need tooltips.
                List<IAttribute> activeToolTipAttributes = null;
                _controlToolTipAttributes.TryGetValue(_activeDataControl, out activeToolTipAttributes);

                // Loop through all active attributes to retrieve their highlights.
                foreach (IAttribute attribute in attributes)
                {
                    // Find any highlight CompositeHighlightLayerObject that has been created for
                    // this data entry control.
                    List<CompositeHighlightLayerObject> highlightList = null;

                    _attributeHighlights.TryGetValue(attribute, out highlightList);

                    // If the attribute has no highlights to display, move on
                    if (highlightList == null || highlightList.Count == 0)
                    {
                        _selectedAttributesWithoutHighlights++;
                        continue;
                    }
                    else
                    {
                        // Update the selected attribute counts appropriately.
                        switch(AttributeStatusInfo.GetHintType(attribute))
                        {
                            case HintType.None:
                                {
                                    if (AttributeStatusInfo.IsAccepted(attribute))
                                    {
                                        _selectedAttributesWithAcceptedHighlights++;
                                    }
                                    else
                                    {
                                        _selectedAttributesWithUnacceptedHighlights++;
                                    }
                                }
                                break;

                            case HintType.Direct:
                                {
                                    _selectedAttributesWithDirectHints++; 
                                }
                                break;

                            case HintType.Indirect:
                                {
                                    _selectedAttributesWithIndirectHints++;
                                }
                                break;
                        }
                    }

                    // If the highlight is an in-direct hint, do not display it unless this is the
                    // only active attribute.
                    if (attributes.Count > 1 && 
                        AttributeStatusInfo.GetHintType(attribute) == HintType.Indirect)
                    {
                        continue;
                    }

                    // Flag each active attribute to be highlighted
                    newDisplayedAttributeHighlights[attribute] = true;

                    // If this attribute was previously highlighted, remove it from the
                    // _displayedAttributeHighlights collection whose contents will be hidden at the
                    // end of this call.
                    if (_displayedAttributeHighlights.ContainsKey(attribute))
                    {
                        _displayedAttributeHighlights.Remove(attribute);
                    }

                    // Display the attribute's error icon (if it has one).
                    ShowErrorIcon(attribute, true);

                    // Display a tooltip if directed to by the control and if possible.
                    if (!_temporarilyHidingTooltips &&
                        AttributeStatusInfo.GetHintType(attribute) != HintType.Indirect &&
                        activeToolTipAttributes.Contains(attribute))
                    {
                        CreateAttributeToolTip(attribute);
                    }
                    // Otherwise, ensure any previous tooltip for the attribute is removed.
                    else
                    {
                        RemoveAttributeToolTip(attribute);
                    }

                    // Make each highlight for an active attribute visible.
                    // Also, display an error icon and tooltip if appropriate, as well as adjust
                    // the view so the entire attribute is visible. (at least the portion on the
                    // current page)
                    foreach (CompositeHighlightLayerObject highlight in highlightList)
                    {
                        highlight.Visible = true;

                        // Update firstPageOfHighlights if appropriate
                        if (firstPageOfHighlights == -1 ||
                            highlight.PageNumber < firstPageOfHighlights)
                        {
                            firstPageOfHighlights = highlight.PageNumber;
                        }

                        // Update pageToShow if appropriate
                        if (highlight.PageNumber == _imageViewer.PageNumber)
                        {
                            pageToShow = highlight.PageNumber;
                        }

                        // If there is not yet an entry for this page in unifiedBounds, create a
                        // new one.
                        if (!unifiedBounds.ContainsKey(highlight.PageNumber))
                        {
                            unifiedBounds[highlight.PageNumber] = highlight.GetBounds();
                        }
                        // Otherwise add to the existing entry for this page
                        else
                        {
                            unifiedBounds[highlight.PageNumber] = Rectangle.Union(
                                unifiedBounds[highlight.PageNumber], highlight.GetBounds());
                        }

                        // Combine the highlight bounds with the error icon bounds (if present).
                        ImageLayerObject errorIcon =
                            GetErrorIconOnPage(attribute, highlight.PageNumber);
                        if (errorIcon != null)
                        {
                            unifiedBounds[highlight.PageNumber] = Rectangle.Union(
                                unifiedBounds[highlight.PageNumber], errorIcon.GetBounds());
                        }

                        // Combine the highlight bounds with the tooltip bounds (if present).
                        TextLayerObject toolTip = GetToolTipOnPage(attribute, highlight.PageNumber);
                        if (toolTip != null)
                        {
                            unifiedBounds[highlight.PageNumber] = Rectangle.Union(
                                unifiedBounds[highlight.PageNumber], toolTip.GetBounds());
                        }
                    }
                }

                // If there is a hover attribute that is different from the active attribute with a
                // tooltip displayed, display a tooltip for the hover attribute.
                if (_hoverAttribute != null && !activeToolTipAttributes.Contains(_hoverAttribute))
                {
                    newDisplayedAttributeHighlights[_hoverAttribute] = true;

                    // If this highlight was previously displayed, remove it from the
                    // _displayedHighlights collection whose contents will be hidden at the end
                    // of this call.
                    if (_displayedAttributeHighlights.ContainsKey(_hoverAttribute))
                    {
                        _displayedAttributeHighlights.Remove(_hoverAttribute);
                    }

                    // Show the hover attribute's highlight and error icon (if one exists).
                    ShowAttributeHighlights(_hoverAttribute, true);

                    if (!_temporarilyHidingTooltips)
                    {
                        // The tooltip should also be displayed for the hover attribute.
                        CreateAttributeToolTip(_hoverAttribute);
                    }
                }

                // Make sure the highlight is in view if ensureActiveAttributeVisible is specified.
                if (ensureActiveAttributeVisible &&
                    (pageToShow != -1 || firstPageOfHighlights != -1))
                {
                    if (pageToShow == -1)
                    {
                        pageToShow = firstPageOfHighlights;
                    }

                    _imageViewer.SetPageNumber(pageToShow, false, true);

                    Rectangle viewRectangle = _imageViewer.GetTransformedRectangle(
                            _imageViewer.GetVisibleImageArea(), true);

                    // Ensure the area of both the highlight and any associated tooltip is visible.
                    if (!viewRectangle.Contains(unifiedBounds[pageToShow]))
                    {
                        // Create a temporary highlight object to use for the CenterOnLayerObject
                        // call.
                        Extract.Imaging.RasterZone rasterZone = new Extract.Imaging.RasterZone(
                            unifiedBounds[pageToShow], pageToShow);
                        Highlight temporaryHighlight = new Highlight(_imageViewer, "", rasterZone);

                        _imageViewer.CenterOnLayerObject(temporaryHighlight, true);

                        temporaryHighlight.Dispose();
                    }
                }
            }

            // Hide all attribute highlights that were previously visible, but should not visible
            // anymore.
            foreach (IAttribute attribute in _displayedAttributeHighlights.Keys)
            {
                if (_showingAllHighlights)
                {
                    // If _showingAllHighlights, only indirect hints need to be hidden.
                    if (AttributeStatusInfo.GetHintType(attribute) == HintType.Indirect)
                    {
                        ShowAttributeHighlights(attribute, false);
                    }

                    // But remove tooltips for all attributes not currently active
                    RemoveAttributeToolTip(attribute);
                }
                else
                {
                    // Hide the highlights and remove any tooltip.
                    ShowAttributeHighlights(attribute, false);
                    RemoveAttributeToolTip(attribute);
                }
            }

            // Move the visible error icons to the top of the z-order so that nothing is drawn on
            // top of them.
            foreach (List<ImageLayerObject> errorIcons in _attributeErrorIcons.Values)
            {
                foreach (ImageLayerObject errorIcon in errorIcons)
                {
                    if (errorIcon.Visible)
                    {
                        _imageViewer.LayerObjects.MoveToTop(errorIcon);
                    }
                }
            }

            // Move the tooltips to the top of the z-order so that nothing is drawn on top of them.
            foreach (List<TextLayerObject> toolTips in _attributeToolTips.Values)
            {
                foreach (TextLayerObject toolTip in toolTips)
                {
                    _imageViewer.LayerObjects.MoveToTop(toolTip);
                }
            }

            // Update _displayedHighlights with the new set of highlights.
            _displayedAttributeHighlights = newDisplayedAttributeHighlights;

            _imageViewer.Invalidate();
        }

        /// <summary>
        /// Propagates the provided <see cref="IAttribute"/>s to their mapped 
        /// <see cref="IDataEntryControl"/>s.
        /// </summary>
        /// <param name="attributes">A stack of <see cref="IAttribute"/>s
        /// where the first attribute in the stack represents the root-level attribute
        /// the target attribute is descended from, and each successive attribute represents
        /// a sub-attribute to the previous until the final attribute is the target attribute.
        /// </param>
        /// <param name="select">If <see langword="true"/>, following propagation, all attributes
        /// should be selected in their respective controls and the target attribute should be
        /// active. If <see langword="false"/>, the data is only propagated behind the scenes 
        /// (which causes the attributes' data to be validated).</param>
        private static IAttribute PropagateAttributes(Stack<IAttribute> attributes, bool select)
        {
            if (attributes.Count == 0)
            {
                // Nothing to do.
                return null;
            }
            
            // Get the first attribute off the stack and obtain its owning control.
            IAttribute attribute = attributes.Pop();
            IDataEntryControl dataEntryControl = 
                AttributeStatusInfo.GetStatusInfo(attribute).OwningControl;
            
            // Initialize the "last" attribute and control.
            IAttribute lastAttribute = attribute;
            IDataEntryControl lastDataEntryControl = dataEntryControl;

            // Loop through each attribute in the chain, but only call PropagateAttribute for the
            // last (deepest) attribute mapped to a particular control.  This is to prevent an 
            // ealier (ancestor) attribute from being marked as viewed.
            while (attributes.Count > 0)
            {
                // Get the next attribute off the stack and obtain its owning control.
                attribute = attributes.Pop();
                dataEntryControl = AttributeStatusInfo.GetStatusInfo(attribute).OwningControl;

                // If the next attribute belongs to a different control, propagate the "last"
                // attribute in the last control.
                if (dataEntryControl != lastDataEntryControl)
                {
                    lastDataEntryControl.PropagateAttribute(lastAttribute, select);
                }

                // Update the "last" attribute and control.
                lastDataEntryControl = dataEntryControl;
                lastAttribute = attribute;
            }

            lastDataEntryControl.PropagateAttribute(lastAttribute, select);

            // Give focus to the target attribute control if selection is requested.
            if (select)
            {
                ((Control)lastDataEntryControl).Focus();
            }

            return lastAttribute;
        }

        /// <summary>
        /// Retrieves an <see cref="IAttribute"/> genealogy describing the currently active
        /// attribute. If more that one attribute is selected in the active control, the first 
        /// attribute (in display order) will be used.
        /// </summary>
        /// <returns>A Stack of <see cref="IAttribute"/>s describing the currently active
        /// <see cref="IAttribute"/> or <see langword="null"/> if there is no active attribute.
        /// </returns>
        private Stack<IAttribute> ActiveAttributeGenealogy()
        {
            if (_activeDataControl != null)
            {
                List<IAttribute> controlAttributes;
                if (_controlAttributes.TryGetValue(_activeDataControl, out controlAttributes))
                {
                    IAttribute firstAttribute = GetFirstAttribute(controlAttributes);
                    return GetAttributeGenealogy(firstAttribute);
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the first <see cref="IAttribute"/> in display order from the provided
        /// vector of <see cref="IAttribute"/>s.
        /// </summary>
        /// <param name="attributes">The list of <see cref="IAttribute"/>s
        /// from which the first attribute in display order should be found.</param>
        /// <returns>The <see cref="IAttribute"/> containing the lowest 
        /// <see cref="AttributeStatusInfo.DisplayOrder"/>.</returns>
        private static IAttribute GetFirstAttribute(List<IAttribute> attributes)
        {
            string firstDisplayOrder = "";
            IAttribute firstAttribute = null;

            // Iterate through all provided attributes to find the one with the lowest display
            // order value.
            foreach (IAttribute attribute in attributes)
            {
                string displayOrder = AttributeStatusInfo.GetStatusInfo(attribute).DisplayOrder;

                // If the current attribute's display order is less than the lowest value found to
                // this point, use this attribute as the first.
                if (string.IsNullOrEmpty(firstDisplayOrder) ||
                    string.Compare(displayOrder, firstDisplayOrder, 
                        StringComparison.CurrentCultureIgnoreCase) < 0)
                {
                    firstDisplayOrder = displayOrder;
                    firstAttribute = attribute;
                }
            }

            return firstAttribute;
        }

        /// <summary>
        /// Retrieves the geneaolgy of the supplied <see cref="IAttribute"/>.
        /// <para><b>Requirements:</b></para>
        /// The supplied <see cref="IAttribute"/> must have been added as a key to the 
        /// _attributeToParentMap dictionary.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose genealogy is requested.
        /// </param>
        /// <returns>A genealogy of <see cref="IAttribute"/>s with each attribute further down the
        /// the stack being a descendent to the previous <see cref="IAttribute"/> in the stack; the
        /// last entry being the specified <see cref="IAttribute"/>.</returns>
        private static Stack<IAttribute> GetAttributeGenealogy(IAttribute attribute)
        {
            Stack<IAttribute> attributeGenealogy = new Stack<IAttribute>();

            while (attribute != null)
            {
                // Add the current attribute to the stack.
                attributeGenealogy.Push(attribute);

                attribute = AttributeStatusInfo.GetParentAttribute(attribute);

                // If a parent for the current attribute cannot be found, 
                if (attribute == null)
                {
                    break;
                }
            }

            return attributeGenealogy;
        }

        /// <summary>
        /// Attempts to find the next <see cref="IAttribute"/> (in display order) whose data
        /// has not been viewed.
        /// </summary>
        /// <returns>The first <see cref="IAttribute"/> that has not been viewed, or 
        /// <see langword="null"/> if no unviewed <see cref="IAttribute"/>s were found.</returns>
        private IAttribute GetNextUnviewedAttribute()
        {
            // Look for any attributes whose data failed validation.
            Stack<IAttribute> unviewedAttributeGenealogy =
                AttributeStatusInfo.FindNextUnviewedAttribute(_attributes,
                ActiveAttributeGenealogy(), true, true);

            if (unviewedAttributeGenealogy != null)
            {
                return PropagateAttributes(unviewedAttributeGenealogy, true);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to find the next <see cref="IAttribute"/> (in display order) whose data
        /// has failed validation.
        /// </summary>
        /// <returns>The first <see cref="IAttribute"/> that failed validation, or 
        /// <see langword="null"/> if no invalid <see cref="IAttribute"/>s were found.</returns>
        private IAttribute GetNextInvalidAttribute()
        {
            // Toggle the enabled status of the active control to force editing to end and 
            // validation to occur for any field that is currently being edited.
            // TODO: [DataEntry:169] Consider a better way to do this...
            if (_activeDataControl != null)
            {
                // So that focus does not jump to the next control, assign focus to the image viewer
                // during the toggle of enabled.
                _imageViewer.Focus();

                Control control = (Control)_activeDataControl;
                control.Enabled = false;
                control.Enabled = true;
                control.Select();
            }

            // Look for any attributes whose data failed validation.
            Stack<IAttribute> invalidAttributeGenealogy = 
                AttributeStatusInfo.FindNextInvalidAttribute(_attributes, 
                ActiveAttributeGenealogy(), true, true);
            
            if (invalidAttributeGenealogy != null)
            {
                return PropagateAttributes(invalidAttributeGenealogy, true);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to find an <see cref="IDataEntryControl"/> that should receive active status 
        /// and focus as the result of the mouse click. This can be either a 
        /// <see cref="IDataEntryControl"/> that was directly clicked or the existing active 
        /// control in the case that the mouse click occurred within the 
        /// <see cref="DataEntryControlHost"/> (in order to prevent .NET from shifting focus away
        /// based on tab order).
        /// </summary>
        /// <param name="m">The <see cref="Message"/> containing data about the mouse press event.
        /// <para><b>Requirements</b></para>
        /// The <see cref="Message"/> must not be <see langword="null"/> and must describe a
        /// WM_LBUTTONDOWN or WM_RBUTTONDOWN event.</param>
        /// <returns>The <see cref="IDataEntryControl"/> that should receive active status.
        /// <see langword="null"/> if no such <see cref="IDataEntryControl"/> was found.
        /// </returns>
        private IDataEntryControl FindClickedDataEntryControl(Message m)
        {
            ExtractException.Assert("ELI24760", "Unexpected message!",
                m != null && (m.Msg == _WM_LBUTTONDOWN || m.Msg == _WM_RBUTTONDOWN));

            // Initialize the return value to null.
            IDataEntryControl clickedDataEntryControl = null;

            // Obtain the control that was clicked (may be a container rather than the specific
            // control.
            Control clickedControl = Control.FromHandle(m.HWnd);

            // [DataEntry:354]
            // Sometimes the window handle may be a child of a .Net control (such as the edit box
            // of a combo box). In this case, a Control will not be created from the handle.
            // Use the Win32 API to find the first ancestor that is a .Net control.
            while (clickedControl == null && m.HWnd != IntPtr.Zero)
            {
                m.HWnd = NativeMethods.GetParentWindowHandle(m.HWnd);

                clickedControl = Control.FromHandle(m.HWnd);
            }

            if (clickedControl != null)
            {
                // Get the position of the mouse in screen coordinates.
                Point mousePosition = new Point((int)((uint)m.LParam & 0x0000FFFF),
                                                (int)((uint)m.LParam & 0xFFFF0000) >> 16);
                mousePosition = clickedControl.PointToScreen(mousePosition);

                // If the click occured within the control host, if it didn't occur on a data entry
                // control, make sure that focus will remain on the currently active data entry 
                // control.
                if (clickedControl == this || base.Contains(clickedControl))
                {
                    clickedDataEntryControl = _activeDataControl;
                }

                // Loop down through all the control's descendents at the mouse position to try to
                // find a data entry control at the click location. 
                while (clickedControl != null)
                {
                    IDataEntryControl testControl = clickedControl as IDataEntryControl;
                    if (testControl != null)
                    {
                        // A data entry control was clicked; this control should get focus.
                        clickedDataEntryControl = testControl;
                        break;
                    }

                    clickedControl = clickedControl.GetChildAtPoint(
                        clickedControl.PointToClient(mousePosition));
                }
            }

            return clickedDataEntryControl;
        }

        /// <summary>
        /// Increments or decrements _unviewedAttributeCount and raises the
        /// <see cref="UnviewedItemsFound"/> event as necessary.
        /// </summary>
        /// <param name="increment"><see langword="true"/> to increment
        /// _unviewedAttributeCount <see langword="false"/> to decrement it.</param>
        private void UpdateUnviewedCount(bool increment)
        {
            if (_changingImage)
            {
                return;
            }

            if (increment)
            {
                // If there were previously not any unviewed attributes, raise UnviewedItemsFound
                // to notify listeners that unviewed attributes are now available.
                if (_unviewedAttributeCount == 0)
                {
                    OnUnviewedItemsFound(true);
                }

                _unviewedAttributeCount++;
            }
            else
            {
                _unviewedAttributeCount--;

                // If the _unviewedAttributeCount now stands at zero, perform a full search for
                // unviewed attributes to confirm the count is in sync, and if so raise the
                // UnviewedItemsFound event to notify listeners there are no more unviewed
                // attributes.
                if (_unviewedAttributeCount <= 0)
                {
                    int unviewedRecount = CountUnviewedItems();
                    if (_unviewedAttributeCount!= 0 || unviewedRecount != 0)
                    {
                        ExtractException ee = new ExtractException("ELI24939",
                            "Unviewed attribute count out of sync!");
                        ee.AddDebugData("Count", _unviewedAttributeCount, false);
                        ee.AddDebugData("Recount", unviewedRecount, false);
#if DEBUG
                        ee.Display();
#else
                        ee.Log();
#endif
                        _unviewedAttributeCount = unviewedRecount;
                    }

                    OnUnviewedItemsFound(_unviewedAttributeCount != 0);
                }
            }
        }

        /// <summary>
        /// Increments or decrements _invalidAttributeCount and raises the
        /// <see cref="InvalidItemsFound"/> event as necessary.
        /// </summary>
        /// <param name="increment"><see langword="true"/> to increment
        /// _invalidAttributeCount <see langword="false"/> to decrement it.</param>
        private void UpdateInvalidCount(bool increment)
        {
            if (_changingImage)
            {
                return;
            }

            if (increment)
            {
                // If there were previously not any invalid attributes, raise InvalidItemsFound
                // to notify listeners that invalid attributes are now available.
                if (_invalidAttributeCount == 0)
                {
                    OnInvalidItemsFound(true);
                }

                _invalidAttributeCount++;
            }
            else
            {
                _invalidAttributeCount--;

                // If the _invalidAttributeCount now stands at zero, perform a full search for
                // invalid attributes to confirm the count is in sync, and if so raise the
                // InvalidItemsFound event to notify listeners there are no more invalid
                // attributes.
                if (_invalidAttributeCount <= 0)
                {
                    int invalidRecount = CountInvalidItems();
                    if (_invalidAttributeCount != 0 || invalidRecount != 0)
                    {
                        ExtractException ee = new ExtractException("ELI24940",
                            "Invalid attribute count out of sync!");
                        ee.AddDebugData("Count", _invalidAttributeCount, false);
                        ee.AddDebugData("Recount", invalidRecount, false);
#if DEBUG
                        ee.Display();
#else
                        ee.Log();
#endif
                        _invalidAttributeCount = invalidRecount;
                    }

                    OnInvalidItemsFound(_invalidAttributeCount != 0);
                }
            }
        }

        /// <summary>
        /// Counts the number of unviewed items in the current <see cref="IAttribute"/> heirarchy.
        /// </summary>
        /// <returns>The number of unviewed items in the current <see cref="IAttribute"/> heirarchy.
        /// </returns>
        private int CountUnviewedItems()
        {
            Stack<IAttribute> startingPoint = null;
            Stack<IAttribute> nextUnviewedAttributeGenealogy = null;
            int count = 0;

            // Loop to find the next unviewed attribute until no more can be found without looping.
            do
            {
                nextUnviewedAttributeGenealogy = AttributeStatusInfo.FindNextUnviewedAttribute(
                    _attributes, startingPoint, true, false);

                if (nextUnviewedAttributeGenealogy != null)
                {
                    count++;

                    // Use the found attribute as the starting point for the search in the next
                    // iteration.
                    startingPoint =
                        CollectionMethods.CopyStack<IAttribute>(nextUnviewedAttributeGenealogy);
                }
            }
            while (nextUnviewedAttributeGenealogy != null);

            return count;
        }

        /// <summary>
        /// Counts the number of invalid items in the current <see cref="IAttribute"/> heirarchy.
        /// </summary>
        /// <returns>The number of invalid items in the current <see cref="IAttribute"/> heirarchy.
        /// </returns>
        private int CountInvalidItems()
        {
            Stack<IAttribute> startingPoint = null;
            Stack<IAttribute> nextInvalidAttributeGenealogy = null;
            int count = 0;

            // Loop to find the next invalid attribute until no more can be found without looping.
            do
            {
                nextInvalidAttributeGenealogy = AttributeStatusInfo.FindNextInvalidAttribute(
                    _attributes, startingPoint, true, false);

                if (nextInvalidAttributeGenealogy != null)
                {
                    count++;

                    // Use the found attribute as the starting point for the search in the next
                    // iteration.
                    startingPoint =
                        CollectionMethods.CopyStack<IAttribute>(nextInvalidAttributeGenealogy);
                }
            }
            while (nextInvalidAttributeGenealogy != null);

            return count;
        }

        /// <summary>
        /// Updates the unviewed and invalid attribute counts to account for the
        /// <see cref="IAttribute"/>s that have been deleted.
        /// </summary>
        /// <param name="deletedAttributes">The <see cref="IAttribute"/>s that have been
        /// deleted. Must not be <see langword="null"/>.</param>
        private void ProcessDeletedAttributes(IUnknownVector deletedAttributes)
        {
            ExtractException.Assert("ELI25178", "Null argument exception!", 
                deletedAttributes != null);

            // Cycle through each deleted attribute
            int count = deletedAttributes.Size();
            for (int i = 0; i < count; i++)
            {
                IAttribute attribute = (IAttribute)deletedAttributes.At(i);

                // Remove the highlight for this attribute
                RemoveAttributeHighlight(attribute);

                // If the current attribute is unviewed, decrement the _unviewedAttributeCount
                // and raise UnviewedItemsFound as appropriate.
                if (!AttributeStatusInfo.HasBeenViewed(attribute, false))
                {
                    UpdateUnviewedCount(false);
                }

                // If the current attribute contains invalid data, decrement the 
                // _invalidAttributeCount and raise InvalidItemsFound as appropriate.
                if (!AttributeStatusInfo.IsDataValid(attribute))
                {
                    UpdateInvalidCount(false);
                }

                AttributeStatusInfo.GetStatusInfo(attribute).AttributeValueModified -=
                    HandleAttributeValueModified;
                AttributeStatusInfo.GetStatusInfo(attribute).AttributeDeleted -=
                    HandleAttributeDeleted;

                // Recursively search attribute's descendents for invalid and unviewed items.
                ProcessDeletedAttributes(attribute.SubAttributes);
            }
        }

        /// <summary>
        /// Returns a list of all <see cref="Extract.Imaging.RasterZone"/>s associated with the 
        /// provided set of <see cref="IAttribute"/>s (including all descendents of the supplied 
        /// <see cref="IAttribute"/>s if needed).
        /// </summary>
        /// <param name="attributes">The set of <see cref="IAttribute"/>s whose raster zones are 
        /// to be returned. Must not be <see langword="null"/>.
        /// </param>
        /// <returns>A list of raster zones from the supplied <see cref="IAttribute"/>s.</returns>
        private static List<Extract.Imaging.RasterZone> GetRasterZones(IUnknownVector attributes)
        {
            ExtractException.Assert("ELI25177", "Null argument exception!", attributes != null);

            // Create a list in which to compile the results.
            List<Extract.Imaging.RasterZone> rasterZones = new List<Extract.Imaging.RasterZone>();

            // Loop through each attribute and compile the raster zones from each.
            int attributeCount = attributes.Size();
            for (int i = 0; i < attributeCount; i++)
            {
                // Can be null, so don't use explicit cast.
                IAttribute attribute = attributes.At(i) as IAttribute;

                // If the attribute doesn't contain any spatial information, there is nothing
                // more to do for this attribute.
                if (attribute == null || attribute.Value == null ||
                    attribute.Value.GetMode() == ESpatialStringMode.kNonSpatialMode)
                {
                    continue;
                }

                // Get the AttributeFinder's list of raster zones (COM)
                IUnknownVector comRasterZones = attribute.Value.GetOriginalImageRasterZones();
                int rasterZoneCount = comRasterZones.Size();

                // Convert each raster zone to an Extract.Imaging.RasterZone and add it to the
                // result list.
                for (int j = 0; j < rasterZoneCount; j++)
                {
                    UCLID_RASTERANDOCRMGMTLib.RasterZone comRasterZone =
                        comRasterZones.At(j) as UCLID_RASTERANDOCRMGMTLib.RasterZone;

                    if (comRasterZone != null)
                    {
                        rasterZones.Add(new Extract.Imaging.RasterZone(comRasterZone));
                    }
                }
            }

            return rasterZones;
        }

        /// <summary>
        /// Ensures the existance of one or more CompositeHighlightLayerObjects (one per page) for
        /// the specified attribute (creating one if necessary). Hints will be updated while actual
        /// highlights based on the data itself will be re-used as is.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which a highlight should be
        /// created. Must not be <see langword="null"/>.</param>
        /// <param name="makeVisible"><see langword="true"/> if the highlight should be initialized
        /// as visible, <see langword="false"/> to create the highlight as not visible. (unless a
        /// highlight it is replacing is already visible)</param>
        private void SetAttributeHighlight(IAttribute attribute, bool makeVisible)
        {
            ExtractException.Assert("ELI25173", "Null argument exception!", attribute != null);

            List<CompositeHighlightLayerObject> highlightList;
            if (_attributeHighlights.TryGetValue(attribute, out highlightList))
            {
                if (makeVisible)
                {
                    foreach (CompositeHighlightLayerObject highlight in highlightList)
                    {
                        highlight.Visible = true;
                    }
                }

                return;
            }

            // Before creating any new highlights, remove all existing ones.
            RemoveAttributeToolTip(attribute);
            RemoveAttributeErrorIcon(attribute);

            bool isHint = AttributeStatusInfo.GetHintType(attribute) != HintType.None;

            // If the attribute does not have a hint and does not have any spatial information, no
            // highlight can be generated.
            if (!attribute.Value.HasSpatialInfo() && 
                (!isHint || !AttributeStatusInfo.HintEnabled(attribute)))
            {
                return;
            }

            VariantVector zoneConfidenceTiers = null;
            IUnknownVector comRasterZones = null;
            List<Extract.Imaging.RasterZone> rasterZones = null;

            // For spatial attributes whose text has not been manually edited, use confidence tiers
            // to color code the highlights using OCR confidence.
            if (attribute.Value.GetMode() == ESpatialStringMode.kSpatialMode &&
                !AttributeStatusInfo.IsAccepted(attribute))
            {
                zoneConfidenceTiers = (VariantVector)new VariantVectorClass();

                comRasterZones = attribute.Value.GetOriginalImageRasterZonesGroupedByConfidence(
                    _confidenceBoundaries, out zoneConfidenceTiers);
            }
            // Otherwise the default highlight color will be used-- no need for confidence tiers.
            else if (!isHint)
            {
                comRasterZones = attribute.Value.GetOriginalImageRasterZones();
            }
            // For hints, get the raster zones from the AttributeStatusInfo.
            else
            {
                rasterZones = new List<Extract.Imaging.RasterZone>(
                    AttributeStatusInfo.GetHintRasterZones(attribute));
            }

            // Convert the COM raster zones to Extract.Imaging.RasterZones
            if (comRasterZones != null)
            {
                rasterZones = new List<Extract.Imaging.RasterZone>();

                int rasterZoneCount = comRasterZones.Size();
                for (int i = 0; i < rasterZoneCount; i++)
                {
                    UCLID_RASTERANDOCRMGMTLib.RasterZone comRasterZone =
                        comRasterZones.At(i) as UCLID_RASTERANDOCRMGMTLib.RasterZone;

                    ExtractException.Assert("ELI25682", "Failed to retrieve raster zone!", 
                        comRasterZone != null);

                    rasterZones.Add(new Extract.Imaging.RasterZone(comRasterZone));
                }
            }
            
            Dictionary<int, List<Extract.Imaging.RasterZone>> highlightZones =
                new Dictionary<int, List<Extract.Imaging.RasterZone>>();

            // Loop through the raster zones and group them by page & confidence tier.
            for (int i = 0;  i < rasterZones.Count; i ++)
            {
                // Determine the page and OCR confidence tier of the raster zone.
                int page = rasterZones[i].PageNumber;
                int confidenceTier = _highlightColors.Length - 1;

                if (zoneConfidenceTiers != null)
                {
                    confidenceTier = (int)zoneConfidenceTiers[i];
                }
                else if (isHint && string.IsNullOrEmpty(attribute.Value.String))
                {
                    // Use the lowest confidence tier for hints without associated text.
                    confidenceTier = 0;
                }

                // Generate a key to use for highlightZones representing each unique pair of
                // OCR confidence tier and page.
                int key = page * 100 + confidenceTier;

                if (!highlightZones.ContainsKey(key))
                {
                    highlightZones[key] = new List<Extract.Imaging.RasterZone>();
                }

                // Add the current raster zone to the dictionary.
                highlightZones[key].Add(rasterZones[i]);
            }

            // Create a list of CompositeHighlightLayerObjects that will be associated with the 
            // control.
            _attributeHighlights[attribute] = new List<CompositeHighlightLayerObject>();

            // Loop through each page that contained raster zones and create a separate
            // CompositeLayerHighlight object for that page and OCR confidence tier.
            foreach (int key in highlightZones.Keys)
            {
                // Parse the page number from the key.
                int page = key / 100;

                // Parse the confidence tier from the key.
                int confidenceTier = key % 100;
                if (confidenceTier < 0 || confidenceTier >= _highlightColors.Length)
                {
                    ExtractException ee = new ExtractException("ELI25379", 
                        "Internal Error: Invalid OCR confidence tier!");
                    ee.AddDebugData("Confidence tier", confidenceTier, false);
                    ee.AddDebugData("Tier count", _highlightColors.Length, false);
                    throw ee;
                }

                // Determine the highlight color using the confidence tier.
                Color highlightColor = _highlightColors[confidenceTier].Color;

                // Create the highlight
                CompositeHighlightLayerObject highlight =
                    new CompositeHighlightLayerObject(_imageViewer, page, "", highlightZones[key],
                        isHint ? Color.White : highlightColor);
                highlight.Selectable = false;

                // If the highlight is a hint, specify the outline color.
                if (isHint)
                {
                    highlight.OutlineColor = highlightColor;
                }

                // [DataEntry:189] Highlights should not be visible unless explicitly directed.
                highlight.Visible = makeVisible;

                _attributeHighlights[attribute].Add(highlight);
                _highlightAttributes[highlight] = attribute;
                _imageViewer.LayerObjects.Add(highlight);
            }

            // Create an error icon for the attribute if the value is currently invalid
            CreateAttributeErrorIcon(attribute, makeVisible);
        }

        /// <summary>
        /// If possible, creates a <see cref="TextLayerObject"/> which acts as a tooltip to display
        /// the text of the supplied <see cref="IAttribute"/>.  Space permitting, the tooltip will
        /// be placed above the supplied <see cref="IAttribute"/> (aligned left).  However, the
        /// tooltip may be shifted left or moved below the image region associated with the supplied
        /// <see cref="IAttribute"/> if it will not otherwise fit on the page.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which a tooltip should be
        /// created. Must not be <see langword="null"/>.</param>
        private void CreateAttributeToolTip(IAttribute attribute)
        {
            ExtractException.Assert("ELI25171", "Null argument exception!", attribute != null);

            // Remove any existing tooltip.
            RemoveAttributeToolTip(attribute);

            // If the attribute doesn't have any text value, a tooltip cannot be created.
            if (string.IsNullOrEmpty(attribute.Value.String))
            {
                return;
            }

            // Group the attribute's highlight's raster zones by page.
            Dictionary<int, List<Extract.Imaging.RasterZone>> rasterZonesByPage =
                new Dictionary<int, List<Extract.Imaging.RasterZone>>();

            List<CompositeHighlightLayerObject> highlights;
            if (_attributeHighlights.TryGetValue(attribute, out highlights))
            {
                foreach (CompositeHighlightLayerObject highlight in highlights)
                {
                    if (!rasterZonesByPage.ContainsKey(highlight.PageNumber))
                    {
                        rasterZonesByPage[highlight.PageNumber] =
                            new List<Extract.Imaging.RasterZone>();
                    }

                    rasterZonesByPage[highlight.PageNumber].AddRange(highlight.GetRasterZones());
                }
            }
            
            // Create a tooltip for each page on which the attribute is present.
            foreach (int page in rasterZonesByPage.Keys)
            {
                List<Extract.Imaging.RasterZone> rasterZones = rasterZonesByPage[page];

                // Use the full text of the attribute, not just the text on this page.
                string toolTipText = attribute.Value.String;

                // Calculate the initial anchorpoint for the tooltip above the highlight
                double tooltipRotation;
                Point tooltipAnchorPoint = GetAnchorPoint(rasterZones, AnchorAlignment.LeftTop, 0, 
                    out tooltipRotation);

                // Create the tooltip
                TextLayerObject toolTip = new TextLayerObject(_imageViewer, page,
                    "ToolTip", toolTipText, _toolTipFont, tooltipAnchorPoint, AnchorAlignment.LeftBottom,
                    Color.Yellow, Color.Black, (float)tooltipRotation);

                // Normalize the coordinates of the tooltip bounds and image viewer to vertical in order
                // to determine if the tooltip position needs to be altered to keep it onpage.
                PointF rotationPoint = new PointF(0, 0);
                Rectangle toolTipBounds = GeometryMethods.RotateRectangle(
                    toolTip.GetBounds(), _imageViewer.Orientation, rotationPoint);
                Rectangle imageBounds = GeometryMethods.RotateRectangle(
                    new Rectangle(0, 0, _imageViewer.ImageWidth, _imageViewer.ImageHeight),
                    _imageViewer.Orientation, rotationPoint);

                // If the tooltip extends off the top of the page, try creating one beneath the
                // attribute instead.
                if (toolTipBounds.Top < imageBounds.Top)
                {
                    // Dispose of the original tooltip before re-creating
                    toolTip.Dispose();
                    toolTip = null;

                    // Calculate an anchor point below the highlight
                    tooltipAnchorPoint = GetAnchorPoint(rasterZones, AnchorAlignment.LeftBottom,
                        180, out tooltipRotation);

                    // Create the new tooltip
                    toolTip = new TextLayerObject(_imageViewer, page, "ToolTip", toolTipText,
                        _toolTipFont, tooltipAnchorPoint, AnchorAlignment.LeftTop,
                        Color.Yellow, Color.Black, (float)tooltipRotation);

                    // Re-obtain normalized bounds for the new tooltip.
                    toolTipBounds = GeometryMethods.RotateRectangle(
                        toolTip.GetBounds(), _imageViewer.Orientation, rotationPoint);
                }

                // If the tooltip extends off the right side of the page, shift it left to keep it
                // onpage.
                if (toolTipBounds.Right > imageBounds.Right)
                {
                    Point[] offset = new Point[1];

                    // If the tooltip is wider than the page, start it at the left edge of the page.
                    if (toolTipBounds.Width > imageBounds.Width)
                    {
                        offset[0] = new Point(-toolTipBounds.Left, 0);
                    }
                    // Otherwise, align the right side with the right edge of the page.
                    else
                    {
                        offset[0] =
                            new Point(imageBounds.Right - toolTipBounds.Right, 0);
                    }

                    // The offset needs to be rotated back to be in relation to the tooltip rotation.
                    using (Matrix transform = new Matrix())
                    {
                        transform.Rotate((float)tooltipRotation);
                        transform.TransformVectors(offset);
                        toolTip.Offset(offset[0], false);
                    }
                }

                toolTip.Selectable = false;
                toolTip.Visible = true;
                _imageViewer.LayerObjects.Add(toolTip);

                if (!_attributeToolTips.ContainsKey(attribute))
                {
                    _attributeToolTips[attribute] = new List<TextLayerObject>();
                }
                _attributeToolTips[attribute].Add(toolTip);
            }
        }

        /// <summary>
        /// If needed, creates a <see cref="ImageLayerObject"/> to display an error icon indicating
        /// that the field's value is invalid. (If the data is valid, no such icon will be created).
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which an the error icon is to
        /// potentially be associated with. Must not be <see langword="null"/>.</param>
        /// <param name="makeVisible"><see langword="true"/> to make the icon visible right away or
        /// <see langword="false"/> if the icon should be invisible initially.</param>
        private void CreateAttributeErrorIcon(IAttribute attribute, bool makeVisible)
        {
            ExtractException.Assert("ELI25699", "Null argument exception!", attribute != null);

            // Remove any existing error icon in case the attribute's spatial area has changed.
            RemoveAttributeErrorIcon(attribute);

            // If the attribute's data is valid, or it is represented by an indirect hint there is
            // nothing else to do.
            if (AttributeStatusInfo.IsDataValid(attribute) ||
                AttributeStatusInfo.GetHintType(attribute) == HintType.Indirect)
            {
                return;
            }

            // Groups the attribute's raster zones by page.
            Dictionary<int, List<Extract.Imaging.RasterZone>> rasterZonesByPage =
                new Dictionary<int, List<Extract.Imaging.RasterZone>>();

            List<CompositeHighlightLayerObject> highlights;
            if (_attributeHighlights.TryGetValue(attribute, out highlights))
            {
                foreach (CompositeHighlightLayerObject highlight in highlights)
                {
                    if (!rasterZonesByPage.ContainsKey(highlight.PageNumber))
                    {
                        rasterZonesByPage[highlight.PageNumber] =
                            new List<Extract.Imaging.RasterZone>();
                    }

                    rasterZonesByPage[highlight.PageNumber].AddRange(highlight.GetRasterZones());
                }
            }

            // Create an error icon for each page on which the attribute is present.
            foreach (int page in rasterZonesByPage.Keys)
            {
                List<Extract.Imaging.RasterZone> rasterZones = rasterZonesByPage[page];

                // The anchor point for the error icon should be to the right of attribute.
                double errorIconRotation;
                Point errorIconAnchorPoint = GetAnchorPoint(rasterZones, AnchorAlignment.Right, 90,
                    out errorIconRotation);

                // Create the error icon
                ImageLayerObject errorIcon = new ImageLayerObject(_imageViewer, page,
                    "", errorIconAnchorPoint, AnchorAlignment.Left,
                    global::Extract.DataEntry.Properties.Resources.LargeErrorIcon,
                    _errorIconSizes[page], (float)errorIconRotation);
                errorIcon.Selectable = false;
                errorIcon.Visible = makeVisible;

                _imageViewer.LayerObjects.Add(errorIcon);

                // NOTE: For now I think the cases where the error icon would extend off-page are so
                // rare that it's not worth handling. But this would be where such a check should
                // be made (see CreateAttributeToolTip).

                if (!_attributeErrorIcons.ContainsKey(attribute))
                {
                    _attributeErrorIcons[attribute] = new List<ImageLayerObject>();
                }

                _attributeErrorIcons[attribute].Add(errorIcon);
            }
        }

        /// <summary>
        /// Finds an anchor point to use to attach an <see cref="AnchoredObject"/> to the specified
        /// list of <see cref="Extract.Imaging.RasterZone"/>s.
        /// </summary>
        /// <param name="rasterZones">The list of <see cref="Extract.Imaging.RasterZone"/>s for
        /// which and anchor point is needed.</param>
        /// <param name="anchorAlignment">The point location along a bounding box of all supplied
        /// zones on which the anchor point should be based.  The bounding box will be relative to
        /// the orientation of the suppled raster zones. (ie, if the raster zones are upside-down,
        /// anchorAlignment.Top would result in an anchor point at the bottom of the zones as they
        /// appear on the page.</param>
        /// <param name="anchorOffsetAngle">The anchor point will be offset from the anchorAlignment
        /// position at this angle. (relative to the raster zones' orientation, not the page)</param>
        /// <param name="anchoredObjectRotation">Specifies the rotation (in degress) that an
        /// associated <see cref="AnchoredObject"/> should be drawn at to match up with the
        /// orientation of the raster zones.  This will be the average rotation of the zones unless
        /// it is sufficiently close to level, in which case the angle will be rounded off to
        /// improve the appearance of the associated <see cref="AnchoredObject"/>.</param>
        /// <returns>A <see cref="Point"/> to use as the anchor for an <see cref="AnchoredObject"/>.
        /// </returns>
        private static Point GetAnchorPoint(
            List<Extract.Imaging.RasterZone> rasterZones, AnchorAlignment anchorAlignment,
            double anchorOffsetAngle, out double anchoredObjectRotation)
        {
            // Keep track of the average height of all raster zones as well as all start and end
            // points.
            int height = 0;
            double firstRasterZoneRotation = 0;
            double averageRotation = 0;
            Point[] rasterZonePoints = new Point[rasterZones.Count * 2];
           
            // Compile the raster zone points as well as the height and rotation from all zones.
            for (int i = 0; i < rasterZones.Count; i++)
            {
                int startIndex = i * 2;
                rasterZonePoints[startIndex] = new Point(rasterZones[i].StartX, rasterZones[i].StartY);
                rasterZonePoints[startIndex + 1] = new Point(rasterZones[i].EndX, rasterZones[i].EndY);
                height += rasterZones[i].Height;
                double rasterZoneRotation = GeometryMethods.GetAngle(
                    rasterZonePoints[startIndex], rasterZonePoints[(startIndex) + 1]);

                // [DataEntry:352]
                // Ensure the angle of each zone is relative to the initial raster zone rotation so
                // there aren't overflow issues when computing the average (eg. -179 vs 180).
                if (i == 0)
                {
                    firstRasterZoneRotation = rasterZoneRotation;
                }
                else
                {
                    // [DataEntry:361]
                    // There seem to be cases where some raster zones are backwards. Round the
                    // angle delta to the nearest PI radians.
                    rasterZoneRotation = firstRasterZoneRotation + GeometryMethods.GetAngleDelta(
                        rasterZoneRotation, firstRasterZoneRotation, Math.PI);
                }
                averageRotation += rasterZoneRotation;
            }

            // Convert to degrees
            averageRotation *= (180.0 / Math.PI);

            // Obtain half the average height and the average rotation of the zones.
            int halfHeight = (height / rasterZones.Count) / 2;
            averageRotation /= rasterZones.Count;
            
            // Rotate the raster zone points into a coordinate system relative to the raster zones'
            // rotation.
            using (Matrix transform = new Matrix())
            {
                transform.Rotate((float)-averageRotation);
                transform.TransformPoints(rasterZonePoints);
            }

            // Obtain a bounding rectangle for the raster zone points.
            Rectangle bounds = GeometryMethods.GetBoundingRectangle(rasterZonePoints);
            bounds.Inflate(0, halfHeight);

            // Based on the raster zones' dimensions, calculate how far from level the raster zones
            // can be and still have a level tooltip before the tooltip would overlap with one of the
            // raster zones. This calculation assumes the tooltip will be placed half the height of
            // the raster zone above the raster zone.
            double roundingCuttoffAngle = (180.0 / Math.PI) * GeometryMethods.GetAngle(
                new Point(0, 0), new Point(bounds.Width, halfHeight));

            // Allow a maximum of 5 degress of departure from level even if a greater angle was
            // calculated based on the raster zone dimensions.
            roundingCuttoffAngle = Math.Min(roundingCuttoffAngle, 5.0);

            // Use the rounded rotation for the angle of the tooltip unless the rotation of the
            // highlight is greater than roundingCuttoffAngle.
            anchoredObjectRotation = Math.Round(averageRotation / 90) * 90;

            double rotationDelta =
                GeometryMethods.GetAngleDelta(anchoredObjectRotation, averageRotation, true);
            if (Math.Abs(rotationDelta) > roundingCuttoffAngle)
            {
                anchoredObjectRotation = averageRotation;
            }

            // Find the reference location for the anchor point using the specified AnchorAlignment.
            Point[] anchorPoint = { bounds.Location };
            
            switch (anchorAlignment)
            {
                case AnchorAlignment.LeftTop:
                    // Nothing to do
                    break;
                case AnchorAlignment.Top:
                    anchorPoint[0].Offset(bounds.Width / 2, 0);
                    break;
                case AnchorAlignment.RightTop:
                    anchorPoint[0].Offset(bounds.Width, 0);
                    break;
                case AnchorAlignment.Right:
                    anchorPoint[0].Offset(bounds.Width, bounds.Height / 2);
                    break;
                case AnchorAlignment.RightBottom:
                    anchorPoint[0].Offset(bounds.Width, bounds.Height);
                    break;
                case AnchorAlignment.Bottom:
                    anchorPoint[0].Offset(bounds.Width / 2, bounds.Height);
                    break;
                case AnchorAlignment.LeftBottom:
                    anchorPoint[0].Offset(0, bounds.Height);
                    break;
                case AnchorAlignment.Left:
                    anchorPoint[0].Offset(0, bounds.Height / 2);
                    break;
                case AnchorAlignment.Center:
                    anchorPoint[0].Offset(bounds.Width / 2, bounds.Height / 2);
                    break;
            }

            using (Matrix transform = new Matrix())
            {
                // Offset the anchor point half the average raster zone height in the direction specified.
                transform.Rotate((float)anchorOffsetAngle);
                Point[] offset = { new Point(0, -halfHeight) };
                transform.TransformVectors(offset);
                anchorPoint[0].Offset(offset[0]);

                // Rotate the anchor point back into the image coordinate system.
                transform.Reset();
                transform.Rotate((float)averageRotation);
                transform.TransformPoints(anchorPoint);
            }

            return anchorPoint[0];
        }

        /// <summary>
        /// Updates the set of active (or selected) <see cref="IAttribute"/>s associated with the
        /// specified <see cref="IDataEntryControl"/>.
        /// </summary>
        /// <param name="dataControl">The <see cref="IDataEntryControl"/> for which the set of
        /// active <see cref="IAttribute"/>s need to be updated. Must not be <see langword="null"/>.
        /// </param>
        /// <param name="attributes">The <see cref="IAttribute"/>s to be considered active in the
        /// specified <see cref="IDataEntryControl"/>. Must not be <see langword="null"/>.</param>
        /// <param name="includeSubAttributes"><see langword="true"/> if all descendents of the
        /// specified <see cref="IAttribute"/>s should be considered active as well;
        /// <see langword="false"/> if they should not.</param>
        /// <param name="displayToolTips"><see langword="true"/> to display tooltips for all
        /// specified <see cref="IAttribute"/>s that are owned by the specfied
        /// <see paramref="dataControl"/>.</param>
        private void UpdateControlAttributes(IDataEntryControl dataControl,
            IUnknownVector attributes, bool includeSubAttributes, bool displayToolTips)
        {
            ExtractException.Assert("ELI25169", "Null argument exception!", dataControl != null);
            ExtractException.Assert("ELI25170", "Null argument exception!", attributes != null);

            // Loop through each attribute and compile the raster zones from each.
            int attributeCount = attributes.Size();
            for (int i = 0; i < attributeCount; i++)
            {
                // Can be null, so don't use explicit cast.
                IAttribute attribute = attributes.At(i) as IAttribute;

                if (attribute == null)
                {
                    continue;
                }

                // If the supplied attribute is not null and we are including sub-attributes, 
                // collect the raster zones from this attribute's children.
                if (includeSubAttributes)
                {
                    UpdateControlAttributes(
                        dataControl, attribute.SubAttributes, true, displayToolTips);
                }

                // If this attribute's value isn't viewable in the DEP, don't create a highlight for
                // it.
                if (!AttributeStatusInfo.IsViewable(attribute))
                {
                    continue;
                }

                _controlAttributes[dataControl].Add(attribute);

                // Only allow tooltips for attributes owned by dataControl.
                if (displayToolTips && AttributeStatusInfo.GetOwningControl(attribute) == dataControl)
                {
                    _controlToolTipAttributes[dataControl].Add(attribute);
                }

                // Don't allow new highlights to be created if there is no longer a document loaded.
                if (_imageViewer.IsImageAvailable)
                {
                    SetAttributeHighlight(attribute, false);
                }
            }
        }

        /// <summary>
        /// Creates highlights for all <see cref="IAttribute"/>s in the provided 
        /// <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/>s for which highlights should be
        /// generated. Must not be <see langword="null"/>.</param>
        void CreateAllAttributeHighlights(IUnknownVector attributes)
        {
            ExtractException.Assert("ELI25174", "Null argument exception!", attributes != null);

            // Loop through each attribute and compile the raster zones from each.
            int attributeCount = attributes.Size();
            for (int i = 0; i < attributeCount; i++)
            {
                // Can be null, so don't use explicit cast.
                IAttribute attribute = attributes.At(i) as IAttribute;

                if (attribute == null)
                {
                    continue;
                }

                // Recursively show highlights for this attribute's descendents.
                CreateAllAttributeHighlights(attribute.SubAttributes);

                // If this attribute is not visible in the DEP, don't create a highlight.
                if (!AttributeStatusInfo.IsViewable(attribute))
                {
                    continue;
                }

                IDataEntryControl owningControl =
                    AttributeStatusInfo.GetStatusInfo(attribute).OwningControl;

                ExtractException.Assert("ELI25139",
                    "Unable to show highlight for unmapped attribute!", owningControl != null);

                // Determine whether the highlight should be visible be default based on
                // _showingAllHighlights and whether the highlight is associated with an indirect
                // hint.
                bool makeVisible = _showingAllHighlights;
                if (makeVisible)
                {
                    if (AttributeStatusInfo.GetHintType(attribute) == HintType.Indirect)
                    {
                        makeVisible = false;
                    }
                }

                // Create/verify the highlight for this attribute.
                SetAttributeHighlight(attribute, makeVisible);
            }
        }

        /// <summary>
        /// Updates the highlights for the currently active <see cref="IAttribute"/>(s).
        /// </summary>
        private void RefreshActiveControlHighlights()
        {
            if (_activeDataControl != null)
            {
                // Retrieve all attributes that are viewable in the active control.
                IEnumerable<IAttribute> attributeList = 
                    GetViewableAttributesInControl(_activeDataControl, _attributes);

                // Remove any existing highlights associated with this attribute.
                foreach (IAttribute attribute in attributeList)
                {
                    RemoveAttributeHighlight(attribute);
                }

                // Attempt to create a new highlight for this attribute.
                foreach (IAttribute attribute in attributeList)
                {
                    bool makeVisible = false;
                    
                    // Determines whether the highlights should default as visible based on
                    // _showingAllHighlights, how many attributes are selected, and whether the
                    // spatial info represents an indirect hint.
                    if (_showingAllHighlights)
                    {
                        if (_controlAttributes[_activeDataControl].Count == 1 &&
                            _controlAttributes[_activeDataControl][0] == attribute)
                        {
                            makeVisible = true;
                        }
                        else if (AttributeStatusInfo.GetHintType(attribute) != HintType.Indirect)
                        {
                            makeVisible = true;
                        }
                    }

                    // Apply the attribute's highlight.
                    SetAttributeHighlight(attribute, makeVisible);
                }

                _refreshActiveControlHighlights = false;
            }
        }

        /// <summary>
        /// Removes any existing highlight associated with the specified <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> that should have its highlight
        /// removed. Must not be <see langword="null"/>.</param>
        private void RemoveAttributeHighlight(IAttribute attribute)
        {
            ExtractException.Assert("ELI25175", "Null argument exception!", attribute != null);

            // The tooltip and error icon (if present) need to be removed along with the highlight.
            RemoveAttributeToolTip(attribute);
            RemoveAttributeErrorIcon(attribute);

            List<CompositeHighlightLayerObject> highlightList;
            if (_attributeHighlights.TryGetValue(attribute, out highlightList))
            {
                foreach (CompositeHighlightLayerObject highlight in highlightList)
                {
                    _highlightAttributes.Remove(highlight);

                    if (_imageViewer.LayerObjects.Contains(highlight))
                    {
                        _imageViewer.LayerObjects.Remove(highlight);
                    }

                    highlight.Dispose();
                }

                _attributeHighlights.Remove(attribute);
            }

            if (_displayedAttributeHighlights.ContainsKey(attribute))
            {
                _displayedAttributeHighlights.Remove(attribute);
            }
        }

        /// <summary>
        /// Removes any existing tooltip associated with the specified 
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> that should have
        /// its associated tooltip removed. Must not be <see langword="null"/>.</param>
        private void RemoveAttributeToolTip(IAttribute attribute)
        {
            ExtractException.Assert("ELI25176", "Null argument exception!", attribute != null);

            List<TextLayerObject> toolTips;
            if (_attributeToolTips.TryGetValue(attribute, out toolTips))
            {
                foreach (TextLayerObject toolTip in toolTips)
                {
                    if (_imageViewer.LayerObjects.Contains(toolTip))
                    {
                        _imageViewer.LayerObjects.Remove(toolTip);
                    }

                    toolTip.Dispose();
                }

                _attributeToolTips.Remove(attribute); 
            }
        }
        
        /// <summary>
        /// Removes any existing error icon associated with the specified <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> that should have its associated 
        /// error icon removed. Must not be <see langword="null"/>.</param>
        private void RemoveAttributeErrorIcon(IAttribute attribute)
        {
            ExtractException.Assert("ELI25702", "Null argument exception!", attribute != null);

            List<ImageLayerObject> errorIcons;
            if (_attributeErrorIcons.TryGetValue(attribute, out errorIcons))
            {
                foreach (ImageLayerObject errorIcon in errorIcons)
                {
                    if (_imageViewer.LayerObjects.Contains(errorIcon))
                    {
                        _imageViewer.LayerObjects.Remove(errorIcon);
                    }

                    errorIcon.Dispose();
                }

                _attributeErrorIcons.Remove(attribute);
            }
        }

        /// <summary>
        /// Displays or hides an <see cref="IAttribute"/>'s error icon (if it has one).
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose error icon should be displayed
        /// or hidden</param>
        /// <param name="show"><see langword="true"/> to display the error icon,
        /// <see langword="false"/> to hide it.</param>
        private void ShowErrorIcon(IAttribute attribute, bool show)
        {
            List<ImageLayerObject> errorIcons;
            if (_attributeErrorIcons.TryGetValue(attribute, out errorIcons))
            {
                foreach (ImageLayerObject errorIcon in errorIcons)
                {
                    errorIcon.Visible = show;
                }
            }
        }

        /// <summary>
        /// Returns the error icon for the specified <see cref="IAttribute"/> on the specified
        /// page. (if such an error icon exists)
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose error icon is requested.</param>
        /// <param name="page">The page number of the desired error icon.</param>
        /// <returns>The <see cref="ImageLayerObject"/> displaying the requested error icon or
        /// <see langword="null"/> if no such error icon exists.</returns>
        private ImageLayerObject GetErrorIconOnPage(IAttribute attribute, int page)
        {
            List<ImageLayerObject> errorIcons;
            if (_attributeErrorIcons.TryGetValue(attribute, out errorIcons))
            {
                foreach (ImageLayerObject errorIcon in errorIcons)
                {
                    if (page == errorIcon.PageNumber)
                    {
                        return errorIcon;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the tooltip for the specified <see cref="IAttribute"/> on the specified
        /// page. (if such a tooltip exists)
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose tooltip is requested.</param>
        /// <param name="page">The page number of the desired tooltip.</param>
        /// <returns>The <see cref="TextLayerObject"/> displaying the requested tooltip or
        /// <see langword="null"/> if no such tooltip exists.</returns>
        private TextLayerObject GetToolTipOnPage(IAttribute attribute, int page)
        {
            List<TextLayerObject> toolTips;
            if (_attributeToolTips.TryGetValue(attribute, out toolTips))
            {
                foreach (TextLayerObject toolTip in toolTips)
                {
                    if (page == toolTip.PageNumber)
                    {
                        return toolTip;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Shows or hides the highlight for the specified <see cref="IAttribute"/>. If the
        /// attribute has an associated error icon, it is also shown or hidden.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to show or hide.</param>
        /// <param name="show"><see langword="true"/> to show the highlight; <see langword="false"/>
        /// to hide it.</param>
        private void ShowAttributeHighlights(IAttribute attribute, bool show)
        {
            ShowErrorIcon(attribute, show);

            List<CompositeHighlightLayerObject> highlightList;
            if (_attributeHighlights.TryGetValue(attribute, out highlightList))
            {
                foreach (CompositeHighlightLayerObject highlight in highlightList)
                {
                    highlight.Visible = show;
                }
            }
        }

        /// <summary>
        /// Using the values of InvalidDataSaveMode and UnviewedDataSaveMode, determines if data
        /// can be saved (prompting as necessary).  If data cannot be saved, an appropriate message
        /// or exception will be displayed and selection will be changed to the first field that
        /// prevented data from being saved.
        /// </summary>
        /// <returns><see langword="true"/> if data can be saved, <see langword="false"/> if the
        /// data cannot be saved at this time.</returns>
        private bool DataCanBeSaved()
        {
            // Keep track of the currently selected attribute and whether selection is changed by
            // this method so that the selection can be restored at the end of this method.
            Stack<IAttribute> currentlySelectedAttribute = ActiveAttributeGenealogy();
            bool changedSelection = false;

            // If saving should be or can be prevented by invalid data, check for invalid data.
            if (_invalidDataSaveMode != InvalidDataSaveMode.Allow)
            {
                // Attempt to find any attributes that haven't passed validation.
                IAttribute firstInvalidAttribute = GetNextInvalidAttribute();
                IAttribute invalidAttribute = firstInvalidAttribute;

                // Loop as long as more invalid attributes are found (for the case that we need to
                // prompt for each invalid field).
                while (invalidAttribute != null)
                {
                    // If GetNextInvalidAttribute found something, the selection has been changed.
                    changedSelection = true;

                    // Obtain a validator that can be used to generate a 
                    // DataEntryValidationException for the attribute.
                    DataEntryValidator validator =
                            AttributeStatusInfo.GetStatusInfo(invalidAttribute).Validator;
                    
                    try
                    {
                        // Generate an exception which can be displayed to the user.
                        if (validator != null)
                        {
                            validator.Validate(invalidAttribute);
                        }
                    }
                    catch (DataEntryValidationException validationException)
                    {
                        // If saving is allowed after prompting, prompt for each attribute that
                        // currently does not meet validation requirements.
                        if (_invalidDataSaveMode == InvalidDataSaveMode.PromptForEach)
                        {
                            string message = validationException.Message + Environment.NewLine +
                                Environment.NewLine + "Do you wish to save the data anyway?";

                            // If the user chooses not to continue the save, return false and leave
                            // selection on the first invalid field.
                            if (MessageBox.Show(this, message, "Invalid data",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button2, 0) == DialogResult.No)
                            {
                                return false;
                            }
                            // If the user choses to continue with the save, look for any further
                            // invalid attributes before returning true.
                            else
                            {
                                invalidAttribute = GetNextInvalidAttribute();

                                // If the next invalid attribute is the first one that was found,
                                // the user has responded to prompts for each-- exit the prompting
                                // loop.
                                if (invalidAttribute == firstInvalidAttribute)
                                {
                                    break;
                                }

                                continue;
                            }
                        }
                        // If saving is disallowed with invalid data, display the validation
                        // exception and return false.
                        else
                        {
                            validationException.Display();
                            return false;
                        }
                    }

                    ExtractException.ThrowLogicException("ELI24640");
                }
            }

            // If saving should be or can be prevented by unviewed data, check for unviewed data.
            if (_unviewedDataSaveMode != UnviewedDataSaveMode.Allow)
            {
                if (GetNextUnviewedAttribute() != null)
                {
                    // If GetNextInvalidAttribute found something, the selection has been changed.
                    changedSelection = true;

                    // If saving should be allowed after a prompting, prompt.
                    if (_unviewedDataSaveMode == UnviewedDataSaveMode.PromptOnceForAll)
                    {
                        if (MessageBox.Show(this, "Not all fields have been viewed, do you wish " +
                            "to save the data anyway?", "Unviewed data", MessageBoxButtons.YesNo, 
                            MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, 0) ==
                                DialogResult.No)
                        {
                            // User chose not to save; leave selection on the first unviewed
                            // attribute and return false.
                            return false;
                        }
                    }
                    // If saving is disallowed, display an exception and return false.
                    else
                    {
                        new ExtractException("ELI25188", "Data cannot be saved until all fields " +
                            "have been viewed!").Display();
                        return false;
                    }
                }
            }

            // Re-propagate the attribute that was selected at the beginning of this method
            if (changedSelection)
            {
                // PropagateAttributes cannot be called in place of this loop since
                // inactive controls may then be left with improper data.
                foreach (IDataEntryControl dataControl in _rootLevelControls)
                {
                    dataControl.PropagateAttribute(null, false);
                }

                // TODO: If multiple cells in a table were selected, only one will be selected at the
                // end of this call.
                PropagateAttributes(currentlySelectedAttribute, true);
            }

            return true;
        }

        /// <summary>
        /// Prevents all <see cref="IDataEntryControl"/>s and the <see cref="ImageViewer"/> from
        /// updating (redrawing) until the lock is released.
        /// </summary>
        /// <param name="lockUpdates"><see langword="true"/> to lock all controls from updating
        /// or <see langword="false"/> to release the lock and allow updates again.</param>
        private void LockControlUpdates(bool lockUpdates)
        {
            if (lockUpdates == _controlUpdatesLocked)
            {
                // If the requested state is the same as the current state, there is nothing to do.
                return;
            }
            else
            {
                _controlUpdatesLocked = lockUpdates;
            }

            // Lock or unlock the _imageViewer using Begin/EndUpdate
            if (lockUpdates)
            {
                _imageViewer.BeginUpdate();
            }
            else
            {
                _imageViewer.EndUpdate();
            }

            // Lock the DEP's parent (a Panel) to prevent scrolling
            FormsMethods.LockControlUpdate(base.Parent, lockUpdates);

            // Lock each visible control.
            foreach (IDataEntryControl dataControl in _dataControls)
            {
                Control control = (Control)dataControl;
                if (control.Visible)
                {
                    FormsMethods.LockControlUpdate(control, lockUpdates);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/>s that are currently viewable in the specified
        /// <see cref="IDataEntryControl"/>.
        /// </summary>
        /// <param name="dataEntryControl">The <see cref="IDataEntryControl"/> for which the
        /// viewable <see cref="IAttribute"/>s are needed.</param>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
        /// in which the returned <see cref="IAttribute"/>(s) must exist.</param>
        /// <returns>The viewable <see cref="IAttribute"/>s in the specified
        /// <see cref="IDataEntryControl"/>.</returns>
        private IEnumerable<IAttribute> GetViewableAttributesInControl(IDataEntryControl dataEntryControl,
            IUnknownVector attributes)
        {
            ExtractException.Assert("ELI25356", "Null argument exception!", dataEntryControl != null);
            ExtractException.Assert("ELI25357", "Null argument exception!", attributes != null);

            // Initialize a list of viewable attributes.
            List<IAttribute> viewableAttributes = new List<IAttribute>();

            // Traverse the tree of attributes looking for viewable ones owned by dataEntryControl.
            int attributeCount = attributes.Size();
            for (int i = 0; i < attributeCount; i++)
            {
                IAttribute attribute = (IAttribute)attributes.At(i);

                if (AttributeStatusInfo.GetOwningControl(attribute) == dataEntryControl &&
                    AttributeStatusInfo.IsViewable(attribute))
                {
                    viewableAttributes.Add(attribute);
                }

                // Recurse through the descendent attributes.
                viewableAttributes.AddRange(
                    GetViewableAttributesInControl(dataEntryControl, attribute.SubAttributes));
            }

            return viewableAttributes;
        }

        /// <summary>
        /// Removes all <see cref="IAttribute"/>s not marked as persistable from the provided
        /// attribute hierarchy.
        /// </summary>
        /// <param name="attributes">The hierarchy of <see cref="IAttribute"/>s from which
        /// non-persistable attributes should be removed.</param>
        private void PruneNonPersistingAttributes(IUnknownVector attributes)
        {
            int count = attributes.Size();
            for (int i = 0; i < count; i++)
            {
                IAttribute attribute = (IAttribute)attributes.At(i);
                if (AttributeStatusInfo.IsAttributePersistable(attribute))
                {
                    PruneNonPersistingAttributes(attribute.SubAttributes);
                }
                else
                {
                    attributes.Remove(i);
                    count--;
                    i--;
                }
            }
        }

        /// <summary>
        /// Delegate for a function that does not take any parameters.
        /// </summary>
        delegate void ParameterlessDelegate();

        /// <summary>
        /// Preforms any operations that need to occur after ImageFileChanged has been called for
        /// a newly loaded document.
        /// </summary>
        void FinalizeDocumentLoad()
        {
            if (_imageViewer.IsImageAvailable)
            {
                // Select the first control on the form.
                base.Focus();
                base.SelectNextControl(null, true, true, true, true);
            }
            else
            {
                // Clear any existing validation errors
                _errorProvider.Clear();
            }

            OnItemSelectionChanged();
        }

        #endregion Private Members
    }
}