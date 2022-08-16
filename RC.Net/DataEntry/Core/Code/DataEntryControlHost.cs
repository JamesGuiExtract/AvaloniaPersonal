using Extract.AttributeFinder;
using Extract.Database;
using Extract.Drawing;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.Utilities.FSharp;
using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using System.Collections.Immutable;

using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;
using ESpatialStringMode = UCLID_RASTERANDOCRMGMTLib.ESpatialStringMode;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;
using SpatialPageInfo = UCLID_RASTERANDOCRMGMTLib.SpatialPageInfo;
using HighlightDictionary =
    System.Collections.Generic.Dictionary<
        UCLID_AFCORELib.IAttribute, System.Collections.Generic.List<
            Extract.Imaging.Forms.CompositeHighlightLayerObject>>;

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
        Allow = 0,

        /// <summary>
        /// Allow data to be saved without prompting only if the data is all valid or only
        /// validation warnings are present (as opposed to completely invalid data).
        /// </summary>
        AllowWithWarnings = 1,

        /// <summary>
        /// Allow data to be saved when data that does not conform to a validation requirement is
        /// present, but prompt for each invalid field first.
        /// </summary>
        PromptForEach = 2,

        /// <summary>
        /// Require all data to meet validation requirements before saving.
        /// </summary>
        Disallow = 3,

        /// <summary>
        /// Allow data to be saved without prompting only if the data is all valid or only
        /// validation warnings are present, but prompt for each field that has a warning first.
        /// </summary>
        PromptForEachWarning = 4
    }

    /// <summary>
    /// Defines a field to be selected in the panel.
    /// </summary>
    public enum FieldSelection
    {
        /// <summary>
        /// Leave any existing selection as-is.
        /// </summary>
        DoNotReset = 0,

        /// <summary>
        /// Select the first tab stop in the panel
        /// </summary>
        First = 1,

        /// <summary>
        /// Select the last tab stop in the panel
        /// </summary>
        Last = 2,

        /// <summary>
        /// Select the first field with a data error in the panel
        /// </summary>
        Error = 3
    }

    #endregion Enums

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
        int _maxOcrConfidence;

        /// <summary>
        /// The color the highlight should be.
        /// </summary>
        Color _color;

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

    /// <summary>
    /// A control whose contents define a Data Entry Pane (DEP).  All data entry controls must be
    /// contained in an DataEntryControlHost instance.
    /// <para><b>Note:</b></para>
    /// Only one control host per image viewer is supported .
    /// </summary>
    public partial class DataEntryControlHost : UserControl, IImageViewerControl,
        IMessageFilter
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataEntryControlHost).ToString();

        /// <summary>
        /// The width/height in inches of the icon to be shown in the image viewer to indicate
        /// invalid data.
        /// </summary>
        const double _ERROR_ICON_SIZE = 0.15;

        /// <summary>
        /// The maximum percentage of the shortest page dimension that will be included on either
        /// side of an object as context in auto-zoom mode.
        /// </summary>
        const double _AUTO_ZOOM_MAX_CONTEXT = 0.75;

        /// <summary>
        /// A string representation of the GUID for <see cref="AttributeStorageManagerClass"/> 
        /// </summary>
        static readonly string _ATTRIBUTE_STORAGE_MANAGER_GUID =
            typeof(AttributeStorageManagerClass).GUID.ToString("B");

        #endregion Constants

        #region Fields

        /// <summary>
        /// The source of application-wide settings and events.
        /// </summary>
        IDataEntryApplication _dataEntryApp;

        /// <summary>
        /// The image viewer with which to display documents.
        /// <para><b>Note</b></para>
        /// 11/20/16 SNK
        /// Recently added usages of the data entry framework (PaginationPanel) mean that the
        /// _imageViewer can be set to null even while a image is still loaded. In this class, use
        /// the ImageViewer property getter to allow an extending class to provided the ImageViewer
        /// on-demand (via overload of the getter) in response to events that are re-activating the
        /// data entry controls.
        /// Use this field in place of the property in cases where an extending class should not be
        /// allowed the opportunity to try to provide the ImageViewer on-demand.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// Indicates whether this DEP should be monitoring events from the
        /// <see cref="_imageViewer"/>.
        /// </summary>
        bool _active;

        /// <summary>
        /// The vector of attributes associated with any currently open document.
        /// </summary>
        IUnknownVector _attributes = new IUnknownVectorClass();

        /// <summary>
        /// The name of the source document to which _attributes correspond.
        /// </summary>
        string _sourceDocName;

        /// <summary>
        /// The <see cref="IAttribute"/>s output from the most recent call to <see cref="SaveData"/>.
        /// <see langword="null"/> if data has not been saved or new data has been loaded since the
        /// most recent save.
        /// </summary>
        IUnknownVector _mostRecentlySaveAttributes;

        /// <summary>
        /// A dictionary to keep track of the highlights associated with each attribute.
        /// </summary>
        readonly HighlightDictionary _attributeHighlights = new HighlightDictionary();

        /// <summary>
        /// A dictionary to keep track of each attribute's tooltips
        /// </summary>
        Dictionary<IAttribute, DataEntryToolTip> _attributeToolTips =
            new Dictionary<IAttribute, DataEntryToolTip>();

        /// <summary>
        /// A dictionary to keep track of each attribute's error icons
        /// </summary>
        readonly Dictionary<IAttribute, List<ImageLayerObject>> _attributeErrorIcons =
            new Dictionary<IAttribute, List<ImageLayerObject>>();

        /// <summary>
        /// The size the error icons should be on each page (determined via a combination of
        /// _ERROR_ICON_SIZE and the DPI of the page.
        /// </summary>
        readonly Dictionary<int, Size> _errorIconSizes = new Dictionary<int, Size>();

        /// <summary>
        /// A dictionary to keep track of the attribute each highlight is related to.
        /// </summary>
        readonly Dictionary<CompositeHighlightLayerObject, IAttribute> _highlightAttributes =
            new Dictionary<CompositeHighlightLayerObject, IAttribute>();

        /// <summary>
        /// A dictionary to keep track of the currently displayed highlights. (NOTE: this collection
        /// represents the highlights that would be displayed if ShowAllHighlights is false. If
        /// ShowAllHighlights is true, though all attribute highlights will be displayed this
        /// collection will only contain the "active" highlights.
        /// </summary>
        Dictionary<IAttribute, bool> _displayedAttributeHighlights =
            new Dictionary<IAttribute, bool>();

        /// <summary>
        /// A dictionary to keep track of each control's selection state and active attributes.
        /// </summary>
        readonly Dictionary<IDataEntryControl, SelectionState> _controlSelectionState =
            new Dictionary<IDataEntryControl, SelectionState>();

        /// <summary>
        /// A dictionary to keep track of each control's attributes that have tooltips.
        /// </summary>
        readonly Dictionary<IDataEntryControl, List<IAttribute>> _controlToolTipAttributes =
            new Dictionary<IDataEntryControl, List<IAttribute>>();

        /// <summary>
        /// Keeps track of the overall bounds of the current selection for each page.
        /// </summary>
        Dictionary<int, Rectangle> _selectionBounds = new Dictionary<int, Rectangle>();

        /// <summary>
        /// Indicates if the user has temporarily hid all tooltips.
        /// </summary>
        bool _temporarilyHidingTooltips;

        /// <summary>
        /// The attribute that corresponds to a highlight that the selection tool is currently
        /// hovering over.
        /// </summary>
        IAttribute _hoverAttribute;

        /// <summary>
        /// The <see cref="DataEntryToolTip"/> associated with the active _hoverAttribute.
        /// </summary>
        DataEntryToolTip _hoverToolTip;

        /// <summary>
        /// A list of all data controls contained in this control host.
        /// </summary>
        readonly List<IDataEntryControl> _dataControls = new List<IDataEntryControl>();

        /// <summary>
        /// A list of all controls on the DEP which do not implement IDataEntryControl.
        /// </summary>
        readonly List<Control> _nonDataControls = new List<Control>();

        /// <summary>
        /// A list of the controls mapped to root-level attributes. (to which the control host needs
        /// to provide attributes)
        /// </summary>
        readonly List<IDataEntryControl> _rootLevelControls = new List<IDataEntryControl>();

        /// <summary>
        /// A flag used to indicate that the current document data is changing so that highlight
        /// drawing can be suspended while all the controls refresh their spatial information.
        /// </summary>
        bool _changingData;

        /// <summary>
        /// Indicates which field (if any) should be selected upon completion of a LoadData call.
        /// </summary>
        FieldSelection _initialSelection = FieldSelection.DoNotReset;

        /// <summary>
        /// The current "active" data entry.  This is the last data entry control to have received
        /// input focus (but doesn't necessarily mean the control currently has input focus).
        /// </summary>
        IDataEntryControl _activeDataControl;

        /// <summary>
        /// In order to prevent logging the same focus change multiple times, keep track of the last
        /// attribute for which focus was logged.
        /// </summary>
        IAttribute _lastLoggedFocusAttribute;

        /// <summary>
        /// In order to prevent logging the same focus change multiple times, keep track of the last
        /// control for which focus was logged.
        /// </summary>
        Control _lastLoggedFocusControl;

        /// <summary>
        /// Keeps track of the last active cursor tool so that the highlight cursor tools can be
        /// automatically re-enabled after focus passes through control that doesn't support
        /// swiping.
        /// </summary>
        CursorTool _lastCursorTool = CursorTool.None;

        /// <summary>
        /// The OCR manager to be used to recognize text from image swipes.
        /// </summary>
        SynchronousOcrManager _ocrManager;

        /// <summary>
        /// The font to use to draw toolTip text.
        /// </summary>
        Font _toolTipFont;

        /// <summary>
        /// To manage tab orders keep track of when focus had belonged to a control outside of the 
        /// control host, but is now returning to a control within the control host
        /// </summary>
        bool _regainingFocus;

        /// <summary>
        /// To manage tab order when the control host is regaining focus, keep track of whether the 
        /// shift key is down.
        /// </summary>
        bool _shiftKeyDown;

        /// <summary>
        /// To determine the appropriate times to switch from tabbing-by-field to tabbing-by-group,
        /// keep track of whether the tab key is down.
        /// </summary>
        bool _tabKeyDown;

        /// <summary>
        /// Keeps track of all keys that are currently depressed.
        /// </summary>
        HashSet<Keys> _depressedKeys = new HashSet<Keys>();

        /// <summary>
        /// Indicates when a manual focus change is taking place (tab key was pressed or a
        /// highlight was selected in the image viewer).
        /// </summary>
        bool _manualFocusEvent;

        /// <summary>
        /// The <see cref="Control"/> that has most recently gained focus and is scheduled for
        /// processing based on the focus change (via DataEntryControlGotFocusInvoked).
        /// </summary>
        IDataEntryControl _focusingControl;

        /// <summary>
        /// A <see cref="Control"/> for which a second attempt at focusing is taking place since
        /// the first attempt failed to switch focus (usually/always due to edit mode in a table
        /// ending and putting focus back in the table that should be losing focus).
        /// </summary>
        IDataEntryControl _refocusingControl;

        /// <summary>
        /// To manage tab order when the control host is regaining focus, keep track of any data
        /// entry control which should receive focus as the result of a mouse click.
        /// </summary>
        IDataEntryControl _clickedDataEntryControl;

        /// <summary>
        /// The ErrorProvider data entry controls should used to display data validation errors
        /// (unless the control needs a specialized error provider).
        /// </summary>
        ErrorProvider _validationErrorProvider = new ErrorProvider();

        /// <summary>
        /// The ErrorProvider data entry controls should used to display data validation errors
        /// (unless the control needs a specialized error provider).
        /// </summary>
        ErrorProvider _validationWarningErrorProvider = new ErrorProvider();

        /// <summary>
        /// The current set of unviewed attributes.
        /// </summary>
        ImmutableHashSet<IAttribute> _unviewedAttributes = ImmutableHashSet.Create<IAttribute>();

        /// <summary>
        /// The attributes that currently have invalid data.
        /// </summary>
        ImmutableHashSet<IAttribute> _invalidAttributes = ImmutableHashSet.Create<IAttribute>();

        /// <summary>
        /// Indicates whether data had been modified since the last load or save.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Specifies whether the data entry controls and image viewer are currently being
        /// prevented from updating.
        /// </summary>
        bool _controlUpdatesLocked;

        /// <summary>
        /// Indicates whether all data must be viewed before saving and, if not, whether a prompt
        /// will be displayed before allowing unviewed data to be saved.
        /// </summary>
        UnviewedDataSaveMode _unviewedDataSaveMode = UnviewedDataSaveMode.Allow;

        /// <summary>
        /// Indicates whether all data must conform to validation rules before saving and, if not,
        /// whether a prompt will be displayed before allowing invalid data to be saved.
        /// </summary>
        InvalidDataSaveMode _invalidDataSaveMode = InvalidDataSaveMode.PromptForEachWarning;

        /// <summary>
        /// Keeps track of whether the highlights associated with the active control need to be
        /// refreshed.
        /// </summary>
        bool _refreshActiveControlHighlights;

        /// <summary>
        /// One or more colors to use to highlight data in the image viewer or indicate the active
        /// status of data in a control.
        /// </summary>
        HighlightColor[] _highlightColors;

        /// <summary>
        /// The boundaries between tiers of OCR confidence in _highlightColors.
        /// </summary>
        VariantVector _confidenceBoundaries;

        /// <summary>
        /// A list of names of DataEntry controls that should remain disabled at all times.
        /// </summary>
        readonly List<string> _disabledControls = new List<string>();

        /// <summary>
        /// A list of names of DataEntry controls in which data validation should be disabled.
        /// </summary>
        readonly List<string> _disabledValidationControls = new List<string>();

        /// <summary>
        /// The number of selected attributes with highlights that have been accepted by the user.
        /// </summary>
        int _selectedAttributesWithAcceptedHighlights;

        /// <summary>
        /// The number of selected attributes with unaccepted highlights.
        /// </summary>
        int _selectedAttributesWithUnacceptedHighlights;

        /// <summary>
        /// The number of selected attributes without spatial information but that have a direct
        /// hint indicating where the data may be (if present).
        /// </summary>
        int _selectedAttributesWithDirectHints;

        /// <summary>
        /// The number of selected attributes without spatial information but that have an indirect
        /// hint indicating data related field.
        /// </summary>
        int _selectedAttributesWithIndirectHints;

        /// <summary>
        /// The number of selected attributes without spatial information or hints.
        /// </summary>
        int _selectedAttributesWithoutHighlights;

        /// <summary>
        /// Indicates whether a swipe is currently being processed.
        /// </summary>
        bool _processingSwipe;

        /// <summary>
        /// Database(s) available for use in validation or auto-update queries; The key is the
        /// connection name (blank for default connection).
        /// </summary>
        Dictionary<string, DbConnection> _dbConnections;

        /// <summary>
        /// The default <see cref="DbConnection"/> to use when a connection is not specified by
        /// name. This is also the connection that will be used to load smart tags.
        /// </summary>
        DbConnection _defaultDbConnection;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        /// <summary>
        /// A control that should be used to display change the file comment for a FAM task.
        /// </summary>
        Control _commentControl;

        /// <summary>
        /// The size of the visible image region (in image coordinates) the last time the user
        /// manually adjusted zoom.
        /// </summary>
        Size _lastManualZoomSize;

        /// <summary>
        /// Indicates whether the image viewer is currently being zoomed automatically per AutoZoom
        /// settings or by programmatically changing pages.
        /// </summary>
        bool _performingProgrammaticZoom;

        /// <summary>
        /// Indicates whether the view is currently zoomed to the current selection either via the
        /// F6 shortcut or the auto-zoom mode.
        /// </summary>
        bool _zoomedToSelection;

        /// <summary>
        /// Indicates whether the view is currently zoomed to the current selection via the F2
        /// shortcut.
        /// </summary>
        bool _manuallyZoomedToSelection;

        /// <summary>
        /// Indicates the page number on which the view was last zoomed to the selection.
        /// </summary>
        int _zoomToSelectionPage;

        /// <summary>
        /// Keeps track of the last region specified to be zoomed to by EnforceAutoZoom or the F2
        /// toggle auto-zoom shortcut.
        /// </summary>
        Rectangle _lastViewArea;

        /// <summary>
        /// The last page area viewed without being zoomed to the current selection.
        /// </summary>
        Rectangle? _lastNonZoomedToSelectionViewArea;

        /// <summary>
        /// The last fit mode in affect prior to being zoomed to the current selection.
        /// </summary>
        FitMode? _lastNonZoomedToSelectionFitMode;

        /// <summary>
        /// A <see cref="ToolTip"/> used to display notifications to the user.
        /// </summary>
        ToolTip _userNotificationTooltip;

        /// <summary>
        /// A list of attributes that have been added since the last time DrawHighlights was run.
        /// </summary>
        readonly List<IAttribute> _newlyAddedAttributes = new List<IAttribute>();

        /// <summary>
        /// Indicates the number of updates controls have indicated are in progress. DrawHighlights
        /// and other general processing that can be delayed should be until there are no more
        /// updates in progress.
        /// </summary>
        uint _controlUpdateReferenceCount;

        /// <summary>
        /// Indicates whether DrawHighlights is currently being processed.
        /// </summary>
        bool _drawingHighlights;

        /// <summary>
        /// Indicates whether the last navigation (selection change) that occurred was done via the
        /// tab key, and if so in which direction. <see langword="null"/> if the last navigation was
        /// not via the tab key, <see langword="true"/> if the last navigation was via tab
        /// (forward), <see langword="false"/> if the last navigation was shift + tab (backward).
        /// </summary>
        bool? _lastNavigationViaTabKey;

        /// <summary>
        /// Indicates any currently selected attribute tab group.
        /// </summary>
        IAttribute _currentlySelectedGroupAttribute;

        /// <summary>
        /// Provided smart tag support to all text controls in the DEP.
        /// </summary>
        SmartTagManager _smartTagManager;

        /// <summary>
        /// Indicates whether the host in the midst of an undo operation.
        /// </summary>
        bool _inUndo;

        /// <summary>
        /// Indicates whether the host in the midst of a redo operation.
        /// </summary>
        bool _inRedo;

        /// <summary>
        /// Indicates whether the host is currently idle (message pump is empty)
        /// </summary>
        bool _isIdle = true;

        /// <summary>
        /// The current <see cref="DataValidity"/> of the data loaded into the control.
        /// </summary>
        DataValidity _dataValidity = DataValidity.Valid;

        /// <summary>
        /// Commands that should be executed the next time the host is idle along with ELI codes
        /// that should be attributed to any exceptions.
        /// </summary>
        Queue<Tuple<Action, string>> _idleCommands = new Queue<Tuple<Action, string>>();

        /// <summary>
        /// Indicates a selection state that should be applied after a currently in-progress update.
        /// (ControlUpdateReferenceCount > 0)
        /// </summary>
        SelectionState _pendingSelection;

        /// <summary>
        /// Indicates whether the form as been loaded.
        /// </summary>
        bool _isLoaded;

        /// <summary>
        /// Indicates whether a document has been completely loaded. (finished
        /// <see cref="FinalizeDocumentLoad"/>)
        /// </summary>
        bool _isDocumentLoaded;

        /// <summary>
        /// Keeps track of the start time when Config.Settings.PerformanceTesting is true.
        /// When enabled, the UI will automatically move to the next document after each is loaded.
        /// When processing stops and the UI is closed, it will log an exception with total run time.
        /// </summary>
        static DateTime? _performanceTestingStartTime;

        /// <summary>
        /// Indicates whether RDT is licensed.
        /// </summary>
        bool _rdtLicense;

        /// <summary>
        /// Specifies a <see cref="Control"/> that is the target of a property dump drag-drop
        /// operation (available only with RDT license).
        /// </summary>
        Control _propertyDumpTarget;

        /// <summary>
        /// Type to enable delegate field for PreFilterMessage method
        /// </summary>
        delegate bool MessageFilterType(ref Message m);

        /// <summary>
        /// Message filter delegates. With some exceptions, e.g., Undo/Redo actions, these filters will be run
        /// before this instance's PreFilterMessage method does anything
        /// </summary>
        /// <remarks>
        /// Using an immutable linked list so that it can be iterated safely without copying.
        /// This list will be used a lot more than it is modified and, I think, added to more than removed from
        /// so this is a fit data structure
        /// </remarks>
        FSharpList<MessageFilterType> _messageFilters = ListModule.Empty<MessageFilterType>();

        /// <summary>
        /// Indicates whether validation errors/warnings will be flagged with an icon in the DEP
        /// </summary>
        bool _showValidationIcons = true;

        /// <summary>
        /// The icon to show for any validation errors
        /// </summary>
        Icon _errorIcon;

        /// <summary>
        /// The icon to show for any validation warnings.
        /// </summary>
        Icon _warningIcon;

        /// <summary>
        /// The icon to show in place of _errorIcon or _warningIcon if _showValidationIcons == false;
        /// </summary>
        Icon _blankIcon;

        /// <summary>
        /// Indicates a deferred <see cref="TabNavigation"/> event that should be executed on the next
        /// tab navigation operation.
        /// </summary>
        TabNavigationEventArgs _pendingTabNavigationEventArgs;

        #endregion Fields

        #region Delegates

        /// <summary>
        /// Delegate for a function that does not take any parameters.
        /// </summary>
        delegate void ParameterlessDelegate();

        /// <summary>
        /// Delegate for a function that takes a single <see cref="IDataEntryControl"/> parameter.
        /// </summary>
        /// <param name="dataEntryControl">The <see cref="IDataEntryControl"/> parameter.</param>
        delegate void DataEntryControlDelegate(IDataEntryControl dataEntryControl);

        #endregion Delegates

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
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI23666", _OBJECT_NAME);

                _rdtLicense = LicenseUtilities.IsLicensed(
                    LicenseIdName.RuleDevelopmentToolkitObjects);

                Config = new ConfigSettings<Properties.Settings>();

                InitializeComponent();

                // Initializing these members (particularly OcrManager) during design-time
                // crashes Visual Studio.  These members aren't needed during design-time, so
                // we can ignore them.
                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    _ocrManager = new SynchronousOcrManager();
                }

                // Specify the default highlight colors.
                HighlightColor[] highlightColors = {new HighlightColor(89, Color.LightSalmon),
                                                    new HighlightColor(100, Color.LightGreen)};
                HighlightColors = highlightColors;

                // Set the selection pen (used only to specify color of word highlighter dashed
                // hover border).
                LayerObject.SelectionPen = ExtractPens.GetThickPen(Color.Gray);

                // Blinking error icons are annoying and unnecessary.

                _validationErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                _validationWarningErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;

                _errorIcon = _validationErrorProvider.Icon;
                // Scale SystemIcons.Warning down to 16x16 for _validationWarningErrorProvider
                using (var resizedWarningImage = SystemIcons.Warning.ToBitmap().ResizeHighQuality(16, 16))
                {
                    // NOTE: This requires the icon to be explicitly destroyed via
                    // NativeMethods.DestroyIcon to prevent GDI object leaks.
                    _warningIcon = Icon.FromHandle(resizedWarningImage.GetHicon());
                    _validationWarningErrorProvider.Icon = _warningIcon;
                }

                // Create transparent 1 pixel icon to use if ShowValidationIcons == false
                // (ErrorProvider does not allow for a null icon)
                using (Bitmap blankImage = new Bitmap(1, 1))
                {
                    blankImage.SetPixel(0, 0, Color.FromArgb(0, 0, 0, 0));

                    // NOTE: This requires the icon to be explicitly destroyed via
                    // NativeMethods.DestroyIcon to prevent GDI object leaks.
                    _blankIcon = Icon.FromHandle(blankImage.GetHicon());
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23667", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDataEntryApplication DataEntryApplication
        {
            get
            {
                return _dataEntryApp;
            }

            set
            {
                _dataEntryApp = value;
            }
        }

        /// <summary>
        /// Gets or sets the configuration settings for the data entry panel.
        /// </summary>
        /// <value>
        /// The configuration settings for the data entry panel.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ConfigSettings<Properties.Settings> Config
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the data has been modified since the last load or save operation.
        /// </summary>
        /// <value><see langword="true"/> if the data has been modified since the last load or
        /// save operation; <see langword="false"/> if the data is unchanged.</value>
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
        /// Gets or sets whether keyboard input should be disabled.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool DisableKeyboardInput
        {
            get;
            set;
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
        [DefaultValue(UnviewedDataSaveMode.Allow)]
        public virtual UnviewedDataSaveMode UnviewedDataSaveMode
        {
            get
            {
                return AttributeStatusInfo.PerformanceTesting
                    ? UnviewedDataSaveMode.Allow
                    : _unviewedDataSaveMode;
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
        [DefaultValue(InvalidDataSaveMode.PromptForEachWarning)]
        public virtual InvalidDataSaveMode InvalidDataSaveMode
        {
            get
            {
                return AttributeStatusInfo.PerformanceTesting
                    ? InvalidDataSaveMode.Allow
                    : _invalidDataSaveMode;
            }

            set
            {
                _invalidDataSaveMode = value;
            }
        }

        /// <summary>
        /// The default color to use for highlighting data in the image viewer or to indicate the
        /// active status of data in a control. This will be the same color as the top tier color
        /// in _highlightColors.
        /// </summary>
        public Color ActiveSelectionColor
        {
            get;
            set;
        } = ExtractColors.LightLightBlue;

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

                    // Initialize _confidenceBoundaries
                    _confidenceBoundaries = new VariantVectorClass();
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
        /// Gets or sets a comma separated list of names of <see cref="IDataEntryControl"/>s that
        /// should remain disabled at all times.
        /// </summary>
        /// <value>A comma separated list of names of <see cref="IDataEntryControl"/>s.</value>
        /// <returns>A comma separated list of names of <see cref="IDataEntryControl"/>s.</returns>
        [Category("Data Entry Control Host")]
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
        /// Indicates whether validation is enabled for the all data in the panel as a whole.
        /// If <c>false</c>, validation queries will continue to provide auto-complete lists
        /// and alter case if ValidationCorrectsCase is set for any field, but it will not
        /// show any data errors or warnings or prevent saving of the document.
        /// </summary>
        bool _validationEnabled = true;
        [Category("Data Entry Control Host")]
        [DefaultValue(true)]
        public bool ValidationEnabled
        {
            get
            {
                return _validationEnabled;
            }

            set
            {
                try
                {
                    if (value != _validationEnabled)
                    {
                        _validationEnabled = value;

                        if (IsDocumentLoaded)
                        {
                            foreach (var control in _dataControls)
                            {
                                AttributeStatusInfo.RefreshValidation(control);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI50338");
                }
            }
        }

        /// <summary>
        /// Gets or sets a comma separated list of names of <see cref="IDataEntryControl"/>s on
        /// which validation should be disabled.
        /// <para><b>Note</b></para>
        /// If data validation is disabled, while a mapped <see cref="IAttribute"/> will never be 
        /// flagged as invalid, it will still take advantage of the ability of validation lists to
        /// generate auto-complete lists, populate combo boxes and to trim and correct-case of
        /// values.
        /// </summary>
        /// <value>A comma separated list of names of <see cref="IDataEntryControl"/>s.</value>
        /// <returns>A comma separated list of names of <see cref="IDataEntryControl"/>s.</returns>
        [Category("Data Entry Control Host")]
        public string DisabledValidationControls
        {
            get
            {
                try
                {
                    string disabledValidationControls = "";

                    foreach (string disabledValidationControl in _disabledValidationControls)
                    {
                        if (!string.IsNullOrEmpty(disabledValidationControl))
                        {
                            disabledValidationControls += ",";
                        }

                        disabledValidationControls += disabledValidationControl;
                    }

                    return disabledValidationControls;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26965", ex);
                }
            }

            set
            {
                try
                {
                    _disabledValidationControls.Clear();
                    _disabledValidationControls.AddRange(
                        value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26966", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Control"/> that should be used to display change the file
        /// comment for a FAM task.
        /// </summary>
        /// <value>A <see cref="Control"/> that should be used to display change the file comment
        /// for a FAM task.</value>
        /// <returns>A <see cref="Control"/> that should be used to display change the file comment
        /// for a FAM task.</returns>
        [Category("Data Entry Control Host")]
        public Control CommentControl
        {
            get
            {
                return _commentControl;
            }

            set
            {
                try
                {
                    if (_commentControl != value)
                    {
                        if (_commentControl != null)
                        {
                            _commentControl.TextChanged -= HandleCommentTextChanged;
                        }

                        _commentControl = value;

                        if (_commentControl != null)
                        {
                            _commentControl.TextChanged += HandleCommentTextChanged;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26969", ex);
                }
            }
        }

        /// <summary>
        /// Gets the database connection to be used in data validation or auto-update queries.
        /// </summary>
        /// <value>The <see cref="DbConnection"/>(s) to be used; The key is the connection name
        /// (blank for default connection). (Can be <see langword="null"/> if one
        /// is not required by the DEP).</value>
        /// <returns>The <see cref="DbConnection"/> in use or <see langword="null"/> if none
        /// has been specified.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<string, DbConnection> DatabaseConnections
        {
            get
            {
                return _dbConnections;
            }
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/>s output from the most recent call to
        /// <see cref="SaveData"/>.
        /// </summary>
        /// <returns>The <see cref="IAttribute"/>s output from the most recent call to
        /// <see cref="SaveData"/> or <see langword="null"/> if data has not been saved or new data
        /// has been loaded since the most recent save.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IUnknownVector MostRecentlySavedAttributes
        {
            get
            {
                return _mostRecentlySaveAttributes;
            }
        }

        /// <summary>
        /// Indicates whether an update of control values is currently in-progress.
        /// <para><b>Note</b></para>
        /// Not all control updates are indicated by this property, but ones that require a
        /// significant amount of processing which may cause certain calls to be needlessly repeated
        /// unless skipped/postponed while this property is <see langword="true"/>.
        /// Some examples of updates indicated by this property are: Loading a document or 
        /// selecting/creating a new table row on a table that has dependent controls.
        /// If <see langword="true"/>, the <see cref="UpdateEnded"/> event can be used to be
        /// notified when the update is complete.
        /// </summary>
        /// <returns><see langword="true"/> if a significant update of control values is in progress,
        /// <see langword="false"/> otherwise.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UpdateInProgress
        {
            get
            {
                return (_changingData || ControlUpdateReferenceCount > 0);
            }
        }

        /// <summary>
        /// Gets the <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s that represents
        /// the currently loaded data.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IUnknownVector Attributes
        {
            get
            {
                return _attributes;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this DEP should be monitoring events from the
        /// <see cref="_imageViewer"/>.
        /// </summary>
        /// <value><see langword="true"/> if this DEP should be monitoring events from the
        /// <see cref="_imageViewer"/>; otherwise, <see langword="false"/>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Active
        {
            get
            {
                return _active;
            }

            set
            {
                try
                {
                    if (value != _active)
                    {
                        _active = value;

                        if (_imageViewer != null)
                        {
                            if (value)
                            {
                                RegisterForEvents();
                            }
                            else
                            {
                                UnregisterForEvents();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34068");
                }
            }
        }

        /// <summary>
        /// Gets a value used to indicate that the current document data is changing (either being
        /// loaded or cleared).
        /// </summary>
        /// <value><see langword="true"/> if the loaded data is changing; otherwise,
        /// <see langword="false"/>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ChangingData
        {
            get
            {
                return _changingData;
            }
        }

        /// <summary>
        /// Gets a value used to indicate whether the UI has a document and it has finished loading.
        /// </summary>
        /// <value><see langword="true"/> if there is a document that has finished loading;
        /// otherwise, <see langword="false"/>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDocumentLoaded
        {
            get
            {
                return _isDocumentLoaded;
            }
        }

        /// <summary>
        /// Gets the overall data validity.
        /// </summary>
        /// <value>
        /// The overall data validity.
        /// </value>
        public DataValidity DataValidity
        {
            get
            {
                try
                {
                    // NOTE: _invalidAttributes may contain attributes have underlying 
                    // _dataValidity errors, but that aren't currenlty reported as invalid
                    // because of control or attribute viewability or validation enabled status.
                    return _invalidAttributes
                        .Select(AttributeStatusInfo.GetDataValidity)
                        .Aggregate(DataValidity.Valid, (acc, v) => acc | v);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41414");
                }
            }
        }

        /// <summary>
        /// Gets whether any data is unviewed.
        /// </summary>
        /// <value>
        /// <c>true</c> if any data is data unviewed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDataUnviewed
        {
            get
            {
                try
                {
                    return _unviewedAttributes.Any();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41415");
                }
            }
        }

        /// <summary>
        /// Gets the current "active" data entry. This is the last data entry control to have
        /// received input focus (but doesn't necessarily mean the control currently has input
        /// focus).
        /// </summary>
        public IDataEntryControl ActiveDataControl
        {
            get
            {
                return _activeDataControl;
            }
        }

        /// <summary>
        /// Gets a value indicating whether swiping is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if swiping is enabled; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SwipingEnabled
        {
            get
            {
                try
                {
                    return ImageViewer != null &&
                        ImageViewer.IsImageAvailable &&
                        _activeDataControl != null &&
                        _activeDataControl.SupportsSwiping;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI44710");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether validation errors/warnings will be flagged with an icon in the DEP.
        /// </summary>
        public virtual bool ShowValidationIcons
        {
            get
            {
                return _showValidationIcons;
            }

            set
            {
                try
                {
                    if (value != _showValidationIcons)
                    {
                        // _blankIcon is a transparent 1 pixel icon (ErrorProvider does not allow for a null icon)
                        _validationErrorProvider.Icon = value ? _errorIcon : _blankIcon;
                        _validationWarningErrorProvider.Icon = value ? _warningIcon : _blankIcon;

                        _showValidationIcons = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47280");
                }
            }
        }

        /// <summary>
        /// <see cref="TabNavigationEventArgs"/> indicating a deferred <see cref="TabNavigation"/>
        /// event that should be executed on the next tab navigation operation.
        /// </summary>
        public TabNavigationEventArgs PendingTabNavigationEventArgs
        {
            get
            {
                return _pendingTabNavigationEventArgs;
            }

            set
            {
                _pendingTabNavigationEventArgs = value;
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
                    if (value != _imageViewer)
                    {
                        // Unregister from previously subscribed-to events
                        if (_imageViewer != null && _active)
                        {
                            UnregisterForEvents();

                            if (_attributes != null)
                            {
                                ClearHighlights();
                            }
                        }

                        // Store the new image viewer internally
                        _imageViewer = value;

                        // Check if an image viewer was specified
                        if (_imageViewer != null)
                        {
                            if (_active)
                            {
                                RegisterForEvents();

                                if (_attributes != null && _imageViewer.IsImageAvailable)
                                {
                                    CreateAllAttributeHighlights(_attributes, null);
                                }
                            }

                            _imageViewer.DefaultHighlightColor = _highlightColors[_highlightColors.Length - 1].Color;
                            _imageViewer.AllowBandedSelection = false;
                            _changingData = false;
                        }
                        else
                        {
                            _errorIconSizes.Clear();

                            // In the case that the image viewer is being set to null, we may not be
                            // switching documents, but it may be activating a pagination tab. In this
                            // case, image viewer events and processing should be prevented just as they
                            // are when switching documents.
                            _changingData = true;
                        }
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
        public bool PreFilterMessage(ref Message m)
        {
            try
            {
                // If in design mode, just return false
                if (_inDesignMode)
                {
                    return false;
                }

                // [DataEntry:1034]
                // Ensure no user input is handled while in the process of undoing an operation.
                if (InUndo || InRedo)
                {
                    if (m.Msg == WindowsMessage.KeyDown || m.Msg == WindowsMessage.KeyUp ||
                        m.Msg == WindowsMessage.LeftButtonDown ||
                        m.Msg == WindowsMessage.LeftButtonUp ||
                        m.Msg == WindowsMessage.MiddleButtonDown ||
                        m.Msg == WindowsMessage.MiddleButtonUp ||
                        m.Msg == WindowsMessage.RightButtonDown ||
                        m.Msg == WindowsMessage.RightButtonUp)
                    {
                        return true;
                    }
                }

                // If a message is being processed, we are no longer idle. (NOTE: The EnterIdle
                // message does not seem to be handled by PreFilterMessage).
                if (_isIdle)
                {
                    _isIdle = false;
                }

                foreach (MessageFilterType filter in _messageFilters)
                {
                    if (filter(ref m))
                    {
                        return true;
                    }
                }

                if (m.Msg == WindowsMessage.KeyDown || m.Msg == WindowsMessage.KeyUp)
                {
                    if (DisableKeyboardInput)
                    {
                        return false;
                    }

                    if (m.Msg == WindowsMessage.KeyDown)
                    {
                        _depressedKeys.Add((Keys)m.WParam);
                    }
                    else
                    {
                        _depressedKeys.Remove((Keys)m.WParam);
                    }

                    // [DataEntry:1230]
                    // If a modifier key is going down or up, notify image viewer to update cursor
                    // to prevent cases where the active tool gets stuck in an inappropriate state.
                    if (m.WParam == (IntPtr)Keys.ShiftKey || m.WParam == (IntPtr)Keys.Control
                        || m.WParam == (IntPtr)Keys.Alt)
                    {
                        if (_imageViewer != null)
                        {
                            _imageViewer.UpdateCursor();
                        }
                    }

                    // Check for shift or tab key press events
                    if (m.WParam == (IntPtr)Keys.ShiftKey)
                    {
                        // Set or clear _shiftKeyDown depending upon whether this is a keydown
                        // or keyup event.
                        _shiftKeyDown = (m.Msg == WindowsMessage.KeyDown);
                    }
                    else if (m.WParam == (IntPtr)Keys.Tab &&
                        (_smartTagManager == null || !_smartTagManager.IsActive))
                    {
                        _tabKeyDown = (m.Msg == WindowsMessage.KeyDown);

                        // [DataEntry:346]
                        // If a shift tab is being sent via KeyMethods.SendKeyToControl the tab key
                        // will be received before the shift key. If _shiftKeyDown if false, test
                        // using Control.ModifierKeys to help ensure that shift is not missed.
                        if (!_shiftKeyDown)
                        {
                            _shiftKeyDown = (ModifierKeys == Keys.Shift);
                        }

                        // If the tab key was pressed, indicate a manual focus event and
                        // propagate selection to the next attribute in the tab order.
                        if (m.Msg == WindowsMessage.KeyDown && IsDocumentLoaded)
                        {
                            AdvanceToNextTabStop(!_shiftKeyDown, viaTabKey: true);

                            // So that the input tracker is able to track this input
                            OnMessageHandled(new MessageHandledEventArgs(m));

                            return true;
                        }
                    }
                }
                else if (!ContainsFocus && (m.Msg == WindowsMessage.LeftButtonDown ||
                    m.Msg == WindowsMessage.RightButtonDown))
                {
                    // Attempt to find a data entry control that should receive active status and 
                    // focus as the result of the mouse click.
                    _clickedDataEntryControl = FindClickedDataEntryControl(m);
                }
                else if (m.Msg == WindowsMessage.LeftButtonUp ||
                    m.Msg == WindowsMessage.RightButtonUp)
                {
                    // Make sure to clear the _clickedDataEntryControl on mouse up. 
                    _clickedDataEntryControl = null;
                }
                else if (m.Msg == WindowsMessage.MouseWheel)
                {
                    // [DataEntry:302]
                    // If the mouse is over the image viewer, select the image viewer and allow it to 
                    // handle the mouse wheel event instead.
                    if (_imageViewer != null && _imageViewer.IsImageAvailable && !_imageViewer.Focused &&
                            _imageViewer.ClientRectangle.Contains(
                                _imageViewer.PointToClient(MousePosition)))
                    {
                        _imageViewer.Focus();

                        // So that the input tracker is able to track this input
                        OnMessageHandled(new MessageHandledEventArgs(m));

                        return true;
                    }
                }

                // https://extract.atlassian.net/browse/ISSUE-13640
                // If RDT is licensed, implement ability to dump all control properties to a text
                // file.
                if (_rdtLicense)
                {
                    ProcessPropertyDumpDragDrop(m);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24055", ex);
            }

            return false;
        }

        #endregion IMessageFilter Members

        #region Methods

        /// <summary>
        /// Set the database connection(s) to be used in data validation or auto-update queries.
        /// </summary>
        /// <param name="dbConnections">The <see cref="DbConnection"/>(s) to be used; The key is the
        /// connection name (blank for default connection). (Can be <see langword="null"/> if one is
        /// not required by the DEP).</param>
        public void SetDatabaseConnections(Dictionary<string, DbConnection> dbConnections)
        {
            try
            {
                if (_dbConnections?.SequenceEqual(dbConnections ?? new Dictionary<string, DbConnection>()) != true)
                {
                    _dbConnections = dbConnections;

                    DbConnection defaultDbConnection = null;
                    if (_dbConnections != null)
                    {
                        _dbConnections.TryGetValue("", out defaultDbConnection);
                    }

                    if (defaultDbConnection != _defaultDbConnection)
                    {
                        _defaultDbConnection = defaultDbConnection;

                        if (_isLoaded)
                        {
                            // If the dbconnection is changed, the SmartTagManager needs to be
                            // updated.
                            InitializeSmartTagManager();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28879", ex);
            }
        }

        /// <summary>
        /// Loads the provided data in the <see cref="IDataEntryControl" />s.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector" /> of <see cref="IAttribute" />s
        /// that represent the document's data or <see langword="null" /> to clear all data from the
        /// DEP, remove data validation warnings and to disable the DEP.</param>
        /// <param name="sourceDocName">The name of the source document to which
        /// <see paramref="attributes"/> correspond.</param>
        /// <param name="forEditing"><c>true</c> if the loaded data is to be displayed for editing;
        /// <c>false</c> if the data is to be displayed read-only, or if it is being used for
        /// background formatting.</param>
        /// <param name="initialSelection">Indicates which field should be selected upon completion
        /// of the load (if any).</param>
        public void LoadData(IUnknownVector attributes, string sourceDocName, bool forEditing,
            FieldSelection initialSelection, Guid attributeSetID = new Guid())
        {
            try
            {
                _sourceDocName = sourceDocName;
                _initialSelection = initialSelection;
                HighlightDictionary preCreatedHighlights = null;

                using (new TemporaryWaitCursor())
                {
                    // De-activate any existing control that is active to prevent problems with
                    // last selected control remaining active when the next document is loaded.
                    ClearSelection();

                    // Disable all controls to prevent focus from being grabbed during load.
                    EnableControls(false);
                    
                    // Prevent updates to the controls during the attribute propagation
                    // that will occur as data is loaded.
                    LockControlUpdates(true);

                    // Ensure the data in the controls is cleared prior to loading any new data.
                    ClearData();

                    // Set flag to indicate that a document change is in progress so that highlights
                    // are not redrawn as the spatial info of the controls are updated. Set this
                    // after the call to ClearData (which will independently set and clear
                    // _changingData)
                    _changingData = true;

                    // While data is loading, disable validation triggers. Unlike auto-update
                    // triggers that need to be processed while data is loading, validation triggers
                    // can wait until the data is loaded to prevent validation triggers from firing
                    // more often than they have to.
                    AttributeStatusInfo.EnableValidationTriggers(false);

                    bool imageIsAvailable = ImageViewer != null && ImageViewer.IsImageAvailable;

                    if (attributes != null)
                    {
                        // [DataEntry:693]
                        // The attributes need to be released (by nulling the DataObjects) to prevent
                        // handle leaks.
                        if (_mostRecentlySaveAttributes != null)
                        {
                            AttributeStatusInfo.ReleaseAttributes(_mostRecentlySaveAttributes);
                            _mostRecentlySaveAttributes = null;
                        }

                        // If an image was loaded, look for and attempt to load corresponding data.
                        _attributes = attributes;

                        // Notify AttributeStatusInfo of the new attribute hierarchy
                        AttributeStatusInfo.ResetData(_sourceDocName, _attributes,
                            _dbConnections, pathTags: null, noUILoad: false, forEditing,
                            attributeSetID);

                        if (attributes.Size() > 0 &&
                            AttributeStatusInfo.IsLoggingEnabled(LogCategories.DataLoad))
                        {
                            AttributeStatusInfo.Logger.LogEvent(LogCategories.DataLoad, null,
                                "Begin: ----------------------------------------------------");
                        }

                        // Enable or disable swiping as appropriate.
                        OnSwipingStateChanged(new SwipingStateChangedEventArgs(SwipingEnabled));

                        // Populate all root level data with the retrieved data.
                        foreach (IDataEntryControl dataControl in _rootLevelControls)
                        {
                            dataControl.SetAttributes(_attributes);
                        }

                        if (_commentControl != null)
                        {
                            _commentControl.Text = _dataEntryApp.DatabaseComment;
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

                    // For as long as unpropagated attributes are found, propagate them and their 
                    // subattributes so that all attributes that can be are mapped into controls.
                    // This enables the entire attribute tree to be navigated forward and backward 
                    // for all types of AttributeStatusInfo scans).
                    Stack<IAttribute> nonPropagatedAttributeGenealogy = new Stack<IAttribute>();
                    while (AttributeStatusInfo.GetNonPropagatedAttributes(_attributes, null,
                        nonPropagatedAttributeGenealogy))
                    {
                        PropagateAttributes(nonPropagatedAttributeGenealogy, false, false);
                        nonPropagatedAttributeGenealogy.Clear();
                    }

                    // [DataEntry:166]
                    // Re-propagate the attributes that were originally propagated.
                    foreach (IDataEntryControl dataEntryControl in _rootLevelControls)
                    {
                        dataEntryControl.PropagateAttribute(null, false, false);
                    }

                    if (attributes != null)
                    {
                        // After all the data is loaded, re-enable validation triggers.
                        AttributeStatusInfo.EnableValidationTriggers(true);
                    }

                    // Create highlights for all attributes as long as a document is loaded.
                    if (imageIsAvailable)
                    {
                        CreateAllAttributeHighlights(_attributes, preCreatedHighlights);
                    }

                    if (ControlUpdateReferenceCount != 0)
                    {
                        ExtractException ee = new ExtractException("ELI30028",
                            "Application trace: Control update reference count non-zero");
                        ee.Log();
                    }
                    // Set the _controlUpdateReferenceCount field rather than the property to avoid the
                    // possibility of an UpdateEnded event until the document has finished loading.
                    _controlUpdateReferenceCount = 0;
                    _refreshActiveControlHighlights = false;
                    _dirty = false;
                    _changingData = false;
                    _pendingTabNavigationEventArgs = null;

                    if (imageIsAvailable)
                    {
                        DrawHighlights(true);
                    }

                    // [DataEntry:432]
                    // Some tasks (such as selecting the first control), must take place after the
                    // ImageFileChanged event is complete. Use BeginInvoke to schedule
                    // FinalizeDocumentLoad at the end of the current message queue.
                    if (forEditing && attributes != null)
                    {
                        _shiftKeyDown = ModifierKeys.HasFlag(Keys.Shift);
                        _tabKeyDown = ModifierKeys.HasFlag(Keys.Tab);

                        this.SafeBeginInvoke("ELI34448", () => FinalizeDocumentLoad());
                    }
                    else
                    {
                        OnUpdateEnded(new EventArgs());

                        if (_attributes != null && _attributes.Size() > 0 &&
                            AttributeStatusInfo.IsLoggingEnabled(LogCategories.DataLoad))
                        {
                            ExecuteOnIdle("ELI41670", () => AttributeStatusInfo.Logger.LogEvent(
                                LogCategories.DataLoad, null,
                                "END (NoFinalize): ----------------------------------------------------"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // If any problem was encountered loading the data, clear the data again
                    // to ensure the controls are in a usable state.
                    ClearData();
                }
                catch (Exception ex2)
                {
                    ExtractException.Log("ELI24013", ex2);
                }

                // Ensure that the _changingData flag does not remain set.
                _changingData = false;

                // https://extract.atlassian.net/browse/ISSUE-17141
                // Ensure any operations that were waiting on OnUpdateEnded are executed.
                OnUpdateEnded(new EventArgs());

                ExtractException ee = new ExtractException("ELI23919", "Failed to load data!", ex);
                if (_imageViewer != null && _imageViewer.IsImageAvailable)
                {
                    ee.AddDebugData("FileName", _sourceDocName, false);
                }
                throw ee;
            }
            finally
            {
                LockControlUpdates(false);
            }
        }

        /// <summary>
        /// Commands the <see cref="DataEntryControlHost"/> to finalize and return the attribute
        /// vector.
        /// </summary>
        /// <param name="validateData"><c>true</c> if the data should be validated; otherwise, 
        /// <c>false</c>.</param>
        /// <param name="pruneUnmappedAttributes"><c>true</c> if unmapped attributes should be
        /// pruned from the returned hierarchy; <c>false</c> to keep them in the hierarchy.</param>
        /// <returns>An <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s representing the
        /// current data.
        /// </returns>
        // Since this method has side-effects, it should not be a property.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IUnknownVector GetData(bool validateData, bool pruneUnmappedAttributes = true)
        {
            try
            {
                // Notify AttributeStatusInfo that the current edit is over so that a
                // non-incremental value modified event can be raised.
                AttributeStatusInfo.EndEdit();

                if (!validateData || DataCanBeSaved())
                {

                    // Clear the status info data objects for the cloned/pruned collection
                    if (_mostRecentlySaveAttributes != null)
                    {
                        AttributeStatusInfo.ReleaseAttributes(_mostRecentlySaveAttributes);
                    }

                    // Create a copy of the data to be saved so that attributes that should
                    // not be persisted can be removed.
                    ICloneIdentifiableObject copyThis = (ICloneIdentifiableObject)_attributes;
                    _mostRecentlySaveAttributes = (IUnknownVector)copyThis.CloneIdentifiableObject();
                    _mostRecentlySaveAttributes.ReportMemoryUsage();

                    DataEntryMethods.PruneNonPersistingAttributes(_mostRecentlySaveAttributes, pruneUnmappedAttributes: pruneUnmappedAttributes);

                    return _mostRecentlySaveAttributes;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI35073", "Unable to get data!", ex);
                if (_imageViewer != null && _imageViewer.IsImageAvailable)
                {
                    ee.AddDebugData("Filename", _sourceDocName, false);
                }
                throw ee;
            }
        }

        /// <summary>
        /// Commands the <see cref="DataEntryControlHost"/> to finalize the attribute vector for 
        /// output. This primarily consists of asking each <see cref="IDataEntryControl"/> to 
        /// validate that the data it contains conforms to any validation rules that have been 
        /// applied to it. If so, the vector of attributes as it currently stands is output.
        /// </summary>
        /// <param name="validateData"><see langword="true"/> if the save should only be performed
        /// if all data in the DEP passes validation, <see langword="false"/> if data should be
        /// saved even if there is invalid data.</param>
        /// <returns><see langword="true"/> if the document's data was successfully saved.
        /// <see langword="false"/> if the data was not saved (such as when data fails validation).
        /// </returns>
        public virtual bool SaveData(bool validateData)
        {
            try
            {
                if (ImageViewer != null && ImageViewer.IsImageAvailable)
                {
                    using (new TemporaryWaitCursor())
                    {
                        // Notify AttributeStatusInfo that the current edit is over so that a
                        // non-incremental value modified event can be raised.
                        AttributeStatusInfo.EndEdit();

                        if (!validateData || DataCanBeSaved())
                        {
                            // Clear the status info data objects for the cloned/pruned collection
                            if (_mostRecentlySaveAttributes != null)
                            {
                                AttributeStatusInfo.ReleaseAttributes(_mostRecentlySaveAttributes);
                            }

                            // Create a copy of the data to be saved so that attributes that should
                            // not be persisted can be removed.
                            ICloneIdentifiableObject copyThis = (ICloneIdentifiableObject)_attributes;
                            _mostRecentlySaveAttributes = (IUnknownVector)copyThis.CloneIdentifiableObject();
                            _mostRecentlySaveAttributes.ReportMemoryUsage();

                            DataEntryMethods.PruneNonPersistingAttributes(_mostRecentlySaveAttributes);

                            OnDataSaving(_mostRecentlySaveAttributes, forCommit: validateData);

                            // If all attributes passed validation, save the data.
                            _mostRecentlySaveAttributes.SaveTo(_sourceDocName + ".voa",
                                true, _ATTRIBUTE_STORAGE_MANAGER_GUID);

                            OnDataSaved();

                            if (AttributeStatusInfo.IsLoggingEnabled(LogCategories.DataSave))
                            {
                                AttributeStatusInfo.Logger.LogEvent(LogCategories.DataSave, null,
                                    "----------------------------------------------------");
                            }

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
                if (_imageViewer != null && _imageViewer.IsImageAvailable)
                {
                    ee.AddDebugData("Filename", _sourceDocName, false);
                }
                throw ee;
            }

            return true;
        }

        /// <summary>
        /// Forces application of modified database data to all validation and auto-update queries
        /// by clearing data that has been cached and re-executing all queries.
        /// </summary>
        public virtual void RefreshDatabaseData()
        {
            try
            {
                AttributeStatusInfo.ClearQueryCache();
                AttributeStatusInfo.RefreshAutoUpdateValues();
                AttributeStatusInfo.RefreshValidationQueries();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38191");
            }
        }

        /// <summary>
        /// Selects and activates the next unviewed <see cref="IAttribute"/>.
        /// </summary>
        public void GoToNextUnviewed()
        {
            try
            {
                // The shortcut keys will remain enabled for this option; ignore the command if
                // there is no document loaded.
                if (_imageViewer == null || !_imageViewer.IsImageAvailable)
                {
                    return;
                }

                using (new TemporaryWaitCursor())
                {
                    // Attempt to find and propagate the next unviewed attribute
                    if (GetNextUnviewedAttribute(tabStopsOnly: false) == null)
                    {
                        MessageBox.Show(this, "There are no unviewed items.",
                            _dataEntryApp.ApplicationTitle, MessageBoxButtons.OK,
                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);

                        // If we failed to find any unviewed attributes, make sure 
                        // _unviewedAttributeCount is zero and raise the UnviewedDataStateChanged event to 
                        // indicate no unviewed items are available.
                        _unviewedAttributes = ImmutableHashSet.Create<IAttribute>();
                        OnUnviewedDataStateChanged();
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
        /// validation. A prompt will be displayed if there are no more invalid items.
        /// </summary>
        public void GoToNextInvalidWithPromptIfNone()
        {
            try
            {
                if (!GoToNextInvalid(includeWarnings: true, loop: true))
                {
                    MessageBox.Show(this, "There are no invalid items.",
                        _dataEntryApp.ApplicationTitle, MessageBoxButtons.OK,
                        MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50172");
            }
        }

        /// <summary>
        /// Selects and activates the next <see cref="IAttribute"/> whose data currently fails
        /// validation.
        /// </summary>
        /// <param name="includeWarnings">Indicates whether to includes fields with warnings as
        /// targets for this navigation.</param>
        /// <param name="loop">Indicates whether navigation should loop back to the beginning
        /// if no further invalid data is found beyond the current field.</param>
        /// <returns><c>true</c> if selection was advanced to the next invalid field; <c>false</c>
        /// if no more invalid fields were found.</returns>
        public bool GoToNextInvalid(bool includeWarnings, bool loop)
        {
            try
            {
                // The shortcut keys will remain enabled for this option; ignore the command if
                // there is no document loaded.
                if (_imageViewer == null || !_imageViewer.IsImageAvailable)
                {
                    return false;
                }

                using (new TemporaryWaitCursor())
                {
                    if (GetNextInvalidAttribute(includeWarnings, loop, enabledOnly: true) == null)
                    {
                        if (loop)
                        {
                            // If we failed to find any attributes with invalid data (even when looping),
                            // make sure _invalidAttributes is zero and raise the DataValidityChanged
                            // event to indicate no invalid items remain.
                            _invalidAttributes = ImmutableHashSet.Create<IAttribute>();
                            OnDataValidityChanged();
                        }

                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24646", ex);
            }

            return false;
        }

        /// <summary>
        /// Toggles whether or not tooltip(s) for the active <see cref="IAttribute"/> are currently
        /// visible.
        /// </summary>
        public void ToggleHideTooltips()
        {
            try
            {
                if (_imageViewer == null || !_imageViewer.IsImageAvailable)
                {
                    return;
                }

                if (!_temporarilyHidingTooltips)
                {
                    // Remove tooltips for all selected attributes
                    List<IAttribute> tooltipAttributes = new List<IAttribute>(_attributeToolTips.Keys);
                    foreach (IAttribute attribute in tooltipAttributes)
                    {
                        RemoveAttributeToolTip(attribute);
                    }

                    // [DataEntry:821]
                    // Show and hide the error icons along with the tooltips.
                    ShowAllErrorIcons(false);

                    // Remove the hoverAttribute's tooltip.
                    if (_hoverAttribute != null && _hoverToolTip != null)
                    {
                        RemoveAttributeToolTip(_hoverAttribute);
                    }

                    // [DataEntry:307]
                    // Keep tooltips from re-appearing too readily after pressing esc.
                    _temporarilyHidingTooltips = true;

                    _imageViewer.Invalidate();
                }
                else
                {
                    _temporarilyHidingTooltips = false;

                    // The error icons for selected attributes will re-display automatically,
                    // but if all highlights are showing, need to re-display all error icons.
                    if (_dataEntryApp.ShowAllHighlights)
                    {
                        ShowAllErrorIcons(true);
                    }

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

                try
                {
                    // Prevent multiple re-draws or undo mementos from being triggered until all
                    // highlights have been accepted.
                    ControlUpdateReferenceCount++;

                    // Loop through every attribute in the active control.
                    foreach (IAttribute attribute in GetActiveAttributes())
                    {
                        // If the attribute has a spatial attribute, but the attribute's value has
                        // not yet been accepted, interpret the edit as implicit acceptance of the
                        // attribute's value.
                        if (AttributeStatusInfo.GetHintType(attribute) == HintType.None &&
                            !AttributeStatusInfo.IsAccepted(attribute))
                        {
                            // AddMemento needs to be called before changing the value so that the
                            // DataEntryModifiedAttributeMemento knows of the attribute's original value.
                            AttributeStatusInfo.UndoManager.AddMemento(
                                new DataEntryModifiedAttributeMemento(attribute));

                            // Accepting spatial info will not trigger an EndEdit call to separate this
                            // as an independent operation but it should considered one.
                            AttributeStatusInfo.UndoManager.StartNewOperation();

                            AttributeStatusInfo.AcceptValue(attribute, true);

                            // Re-create the highlight
                            RemoveAttributeHighlight(attribute);
                            SetAttributeHighlight(attribute, true);

                            spatialInfoConfirmed = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI35364", ex);
                }
                finally
                {
                    ControlUpdateReferenceCount--;
                }

                // Re-display the highlights if changes were made.
                if (spatialInfoConfirmed)
                {
                    DrawHighlights(false);

                    // Raise item selection changed to report on the new status of the selected
                    // attribute(s). (i.e., that they no longer contain unaccepted highlights).
                    OnItemSelectionChanged();
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
                // Keeps track of the attributes from which spatial info was removed.
                List<IAttribute> modifiedAttributes = new List<IAttribute>();

                try
                {
                    // Prevent multiple re-draws from being triggered with each cell processed.
                    ControlUpdateReferenceCount++;

                    // Loop through every attribute in the active control.
                    foreach (IAttribute attribute in GetActiveAttributes())
                    {
                        // [DataEntry:642] Don't allow spatial info to be removed from attributes
                        // mapped to dependent controls.
                        if (AttributeStatusInfo.GetOwningControl(attribute) == _activeDataControl &&
                            AttributeStatusInfo.RemoveSpatialInfo(attribute))
                        {
                            modifiedAttributes.Add(attribute);
                        }
                    }
                }
                finally
                {
                    ControlUpdateReferenceCount--;
                }

                // Re-display the highlights if changes were made.
                if (modifiedAttributes.Count > 0)
                {
                    // [DataEntry:547]
                    // Refresh the attributes so that any hints can be updated.
                    _activeDataControl.RefreshAttributes(true, modifiedAttributes.ToArray());
                    _refreshActiveControlHighlights = true;

                    DrawHighlights(false);

                    // Raise item selection changed to report on the new status of the selected
                    // attribute(s). (i.e., that they no longer contain highlights).
                    OnItemSelectionChanged();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25978", ex);
            }
        }

        /// <summary>
        /// Reverts the changes from the last recorded operation.
        /// </summary>
        public void Undo()
        {
            // Ensure the undo mementos from one undo/redo operation do not get confused with
            // another.
            if (InUndo || InRedo)
            {
                return;
            }

            try
            {
                _inUndo = true;
                UndoOrRedo(true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34435");
            }
            finally
            {
                ExecuteOnIdle("ELI34439", () => _inUndo = false);
            }
        }

        /// <summary>
        /// Re-does the changes undone in the last undo operation.
        /// </summary>
        public void Redo()
        {
            // Ensure the undo mementos from one undo/redo operation do not get confused with
            // another.
            if (InUndo || InRedo)
            {
                return;
            }

            try
            {
                _inRedo = true;
                UndoOrRedo(false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34436");
            }
            finally
            {
                ExecuteOnIdle("ELI34440", () => _inRedo = false);
            }
        }

        /// <summary>
        /// Reverts the changes from the last recorded operation or re-does the changes undone in
        /// the last undo operation.
        /// </summary>
        /// <param name="undo"><see langword="true"/> to perform a undo operation,
        /// <see langword="false"/> to perform a redo operation.</param>
        public void UndoOrRedo(bool undo)
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    try
                    {
                        // https://extract.atlassian.net/browse/ISSUE-12551
                        // There may be pending incremental changes that have not yet been committed
                        // via EndEdit. Call EndEdit here, otherwise any queries that would have
                        // fired as part of the EndEdit that will eventually be triggered below will
                        // be blocked due to AttributeStatusInfo.BlockAutoUpdateQueries. If redo is
                        // subsequently called, the redo will then not contain any of the values
                        // that would have been set by these queries.
                        AttributeStatusInfo.EndEdit();

                        if (AttributeStatusInfo.IsLoggingEnabled(
                            undo ? LogCategories.Undo : LogCategories.Redo))
                        {
                            AttributeStatusInfo.Logger.LogEvent(
                                undo ? LogCategories.Undo : LogCategories.Redo, null, "BEGIN");
                        }

                        // [DataEntry:1186]
                        // Unless blocked, the changes that are undone/redone here may trigger
                        // auto-update queries to fire which then result in data that is not in the
                        // correct state.
                        AttributeStatusInfo.BlockAutoUpdateQueries = true;
                        _controlUpdateReferenceCount++;

                        // If the smart tag manager is active, deactivate it before the undo/redo
                        // so that it doesn't try to apply any value.
                        if (_smartTagManager != null && _smartTagManager.IsActive)
                        {
                            _smartTagManager.SafeDeactivate();
                        }

                        // Before performing the undo/redo, remove the active control status from
                        // the currently active control. Otherwise if undo attempts to revert
                        // the status of currently selected attributes, it will not properly take
                        // effect even if a different control will be active once the operation is
                        // complete.
                        if (_activeDataControl != null)
                        {
                            _activeDataControl.IndicateActive(false, ActiveSelectionColor);
                        }

                        if (undo)
                        {
                            AttributeStatusInfo.UndoManager.Undo(false);
                        }
                        else
                        {
                            AttributeStatusInfo.UndoManager.Redo(false);
                        }
                    }
                    finally
                    {
                        _controlUpdateReferenceCount--;
                        _refreshActiveControlHighlights = true;

                        DrawHighlights(true);

                        // Call IndicateActive on the active data control after the undo operation
                        // to ensure the selected attributes get marked viewed as appropriate.
                        if (_activeDataControl != null)
                        {
                            ExecuteOnIdle("ELI34414", () => _activeDataControl.IndicateActive(
                                true, ActiveSelectionColor));
                        }

                        // Invoke EndUndo/EndRedo on idle the to ensure that nothing that happened
                        // as a result of the undo counts as part of a new operation. Schedule the
                        // idle operation via BeginInvoke to ensure that the
                        // DataEntryActiveControlMemento from DataEntryControlGotFocusInvoked does
                        // not get scheduled before this.
                        this.SafeBeginInvoke("ELI34447", () =>
                        {
                            if (undo)
                            {
                                ExecuteOnIdle("ELI34415", () =>
                                    {
                                        AttributeStatusInfo.UndoManager.EndUndo();
                                        AttributeStatusInfo.BlockAutoUpdateQueries = false;
                                        OnDataChanged();

                                        if (AttributeStatusInfo.IsLoggingEnabled(LogCategories.Undo))
                                        {
                                            AttributeStatusInfo.Logger.LogEvent(
                                                LogCategories.Undo, null, "END");
                                        }
                                    });
                            }
                            else
                            {
                                ExecuteOnIdle("ELI34444", () =>
                                    {
                                        AttributeStatusInfo.UndoManager.EndRedo();
                                        AttributeStatusInfo.BlockAutoUpdateQueries = false;
                                        OnDataChanged();

                                        if (AttributeStatusInfo.IsLoggingEnabled(LogCategories.Redo))
                                        {
                                            AttributeStatusInfo.Logger.LogEvent(
                                                LogCategories.Redo, null, "END");
                                        }
                                    });
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                AttributeStatusInfo.BlockAutoUpdateQueries = false;

                throw ExtractException.AsExtractException("ELI31008", ex);
            }
        }

        /// <summary>
        /// Toggles the view between zoomed to the current selection and set to the last view area
        /// where zoom to selection was not in effect.
        /// </summary>
        public void ToggleZoomToSelection()
        {
            try
            {
                // The shortcut keys will remain enabled for this option; ignore the command if
                // there is no document loaded.
                if (_imageViewer == null || !_imageViewer.IsImageAvailable)
                {
                    return;
                }

                // If already zoomed in on the current selection, zoom back out to the last manual
                // zoom.
                if (_zoomedToSelection)
                {
                    if (_zoomToSelectionPage != _imageViewer.PageNumber)
                    {
                        _zoomedToSelection = false;
                    }
                    else
                    {
                        Rectangle currentViewRegion = _imageViewer.GetTransformedRectangle(
                            _imageViewer.GetVisibleImageArea(), true);
                        Rectangle selectionViewRegion;
                        _selectionBounds.TryGetValue(_imageViewer.PageNumber, out selectionViewRegion);

                        if (_selectionBounds.Count > 0 &&
                            (selectionViewRegion == null || !currentViewRegion.Contains(selectionViewRegion)))
                        {
                            _zoomedToSelection = false;
                        }
                        else
                        {
                            if (_lastNonZoomedToSelectionViewArea.HasValue)
                            {
                                // Ensure an F6 toggle out of auto-zoom will always zoom out at
                                // least 5% compared to the current auto-zoom level.
                                Size lastSize = _lastNonZoomedToSelectionViewArea.Value.Size;
                                if ((((double)lastSize.Width / (double)currentViewRegion.Width) < 1.05) &&
                                    (((double)lastSize.Height / (double)currentViewRegion.Height) < 1.05))
                                {
                                    return;
                                }
                            }
                            else
                            // If _lastNonZoomedToSelectionViewArea has not yet been set, default to
                            // zooming out to the full page.
                            {
                                _lastNonZoomedToSelectionViewArea = new Rectangle(0, 0,
                                    _imageViewer.ImageWidth, _imageViewer.ImageHeight);
                            }

                            // If auto-zoom is currently in effect for the page we are currently on
                            // and there is a _lastNonZoomedToSelectionViewArea, restore the view
                            // area to _lastNonZoomedToSelectionViewArea.
                            _performingProgrammaticZoom = true;

                            // [DataEntry:1187]
                            // If the current field will not visible after restoring
                            // _lastNonZoomedToSelectionViewArea, center
                            // _lastNonZoomedToSelectionViewArea on the selected field first.
                            if (!_lastNonZoomedToSelectionViewArea.Value.Contains(selectionViewRegion))
                            {
                                Size size = _lastNonZoomedToSelectionViewArea.Value.Size;
                                Point location = new Point(
                                    selectionViewRegion.X +
                                        ((selectionViewRegion.Width - size.Width) / 2),
                                    selectionViewRegion.Y +
                                        ((selectionViewRegion.Height - size.Height) / 2));

                                _lastNonZoomedToSelectionViewArea = new Rectangle(location, size);
                            }

                            Rectangle viewRegion =
                                _imageViewer.GetTransformedRectangle(
                                    _lastNonZoomedToSelectionViewArea.Value, false);

                            _imageViewer.ZoomToRectangle(viewRegion);

                            // If there was a fit mode in effect before the zoom to selection,
                            // restore it as well.
                            if (_lastNonZoomedToSelectionFitMode.HasValue)
                            {
                                _imageViewer.FitMode = _lastNonZoomedToSelectionFitMode.Value;
                            }

                            _lastViewArea = _lastNonZoomedToSelectionViewArea.Value;
                            // [DataEntry:1286] Consider this the last manual zoom size.
                            _lastManualZoomSize = _lastViewArea.Size;
                            _zoomedToSelection = false;
                            _manuallyZoomedToSelection = false;
                            return;
                        }
                    }
                }

                // If not zoomed in on the current selection, zoom in on it.
                if (!_zoomedToSelection)
                {
                    // Ensure we are on the same page as the selection.
                    if (!_selectionBounds.Keys.Contains(_imageViewer.PageNumber))
                    {
                        if (!_selectionBounds.Any())
                        {
                            return;
                        }
                        else
                        {
                            SetImageViewerPageNumber(_selectionBounds.Keys.First());
                        }
                    }

                    // Force an auto-zoom to the selection bounds on the current page.
                    EnforceAutoZoomSettings(true);

                    // [DataEntry:1286]
                    // In the case where a user has pressed F6 to zoom in on a particular field,
                    // the new zoom level should be considered "manual" so that subsequent zoom out
                    // if necessary logic is based on this zoom level, not the previous zoom level.
                    Rectangle viewArea = _imageViewer.GetTransformedRectangle(
                        _imageViewer.GetVisibleImageArea(), true);
                    _lastManualZoomSize = viewArea.Size;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34985");
            }
            finally
            {
                _performingProgrammaticZoom = false;
            }
        }

        /// <summary>
        /// Resets any existing highlight data and clears the attributes from all controls.
        /// </summary>
        public void ClearData()
        {
            try
            {
                _isDocumentLoaded = false;

                // Set flag to indicate that a document change is in progress so that highlights
                // are not redrawn as the spatial info of the controls are updated.
                _changingData = true;

                // Unregister for events in the process of unregistering the data entry controls.
                // The UI shouldn't be responding to any control events while clearing data.
                // Control events will be re-registered in finally block.
                UnregisterDataEntryControls();

                // Forget all LastAppliedStringValues that are currently being remembered to ensure
                // that they don't get used later on after the value has been changed to something
                // else.
                AttributeStatusInfo.ForgetLastAppliedStringValues();

                // The UndoManager does not need to track changes until data has been reloaded.
                AttributeStatusInfo.UndoManager.TrackOperations = false;

                // Clear any existing data in the controls. Do this before clearing any of the
                // host's fields to avoid any possibility that clearing the controls triggers
                // attributes to be added to already cleared fields.
                foreach (IDataEntryControl dataControl in _rootLevelControls)
                {
                    dataControl.SetAttributes(null);
                }

                // Clear any data the controls have cached.
                foreach (IDataEntryControl dataControl in _dataControls)
                {
                    dataControl.ClearCachedData();
                }

                _selectionBounds.Clear();

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

                // Dispose of all highlights
                if (_imageViewer != null)
                {
                    foreach (CompositeHighlightLayerObject highlight in _highlightAttributes.Keys)
                    {
                        if (_imageViewer.LayerObjects.Contains(highlight))
                        {
                            _imageViewer.LayerObjects.Remove(highlight, true, false);
                        }
                        else
                        {
                            highlight.Dispose();
                        }
                    }
                }
                _highlightAttributes.Clear();

                if (_hoverToolTip != null)
                {
                    _hoverToolTip.Dispose();
                    _hoverToolTip = null;
                }

                if (_userNotificationTooltip != null)
                {
                    _userNotificationTooltip.Dispose();
                    _userNotificationTooltip = null;
                }

                // Reset the other attribute mapping fields.
                _controlSelectionState.Clear();
                _controlToolTipAttributes.Clear();
                _hoverAttribute = null;
                _displayedAttributeHighlights.Clear();
                _attributeHighlights.Clear();
                _newlyAddedAttributes.Clear();
                _highlightAttributes.Clear();

                // AttributeStatusInfo cannot persist any data from one document to the next as it can
                // cause COM threading exceptions in FAM mode. Unload its data now.
                AttributeStatusInfo.ResetData();

                if (_attributes != null)
                {
                    // Clear any existing attributes.
                    _attributes = new IUnknownVectorClass();
                }

                // Clear the status info data objects for the cloned/pruned collection
                if (_mostRecentlySaveAttributes != null)
                {
                    AttributeStatusInfo.ReleaseAttributes(_mostRecentlySaveAttributes);
                }

                // [DataEntry:576]
                // Since the data associated with the currently selected control has been cleared,
                // set _activeDataControl to null so that the next control focus change is processed
                // re-initializes the current selection even if the same control is still selected.
                ClearSelection();

                _selectedAttributesWithAcceptedHighlights = 0;
                _selectedAttributesWithUnacceptedHighlights = 0;
                _selectedAttributesWithDirectHints = 0;
                _selectedAttributesWithIndirectHints = 0;
                _selectedAttributesWithoutHighlights = 0;

                _zoomedToSelection = false;
                _manuallyZoomedToSelection = false;
                _zoomToSelectionPage = 0;

                // Raise the ItemSelectionChanged event to notify listeners that there are no
                // longer attributes selected.
                OnItemSelectionChanged();

                // Reset the unviewed attribute count.
                _unviewedAttributes = ImmutableHashSet.Create<IAttribute>();
                OnUnviewedDataStateChanged();

                // Reset the invalid attribute count.
                _invalidAttributes = ImmutableHashSet.Create<IAttribute>();
                OnDataValidityChanged();

                // Clear the AFUtility instance: the form is running in a single threaded apartment so
                // AFUtility will not be able to be used when another form is created in another
                // apartment.
                DataEntryMethods.AFUtility = null;

                // Reset the map of error icon sizes to use.
                _errorIconSizes.Clear();

                _lastLoggedFocusAttribute = null;
                _lastLoggedFocusControl = null;
                _dataValidity = DataValidity.Valid;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25976", ex);
            }
            finally
            {
                _changingData = false;
                ReRegisterDataEntryControls();
            }
        }

        /// <summary>
        /// Ensures a field is selected by selecting the first field if necessary.
        /// </summary>
        /// <param name="targetField">Indicates which field should be selected upon completion
        /// of the load (if any).</param>
        /// <returns><c>true</c> if the result of the call is that a field in the DEP has received
        /// focus; <c>false</c> if focus could not be applied or was handled externally via
        /// <see cref="TabNavigation"/> event.</returns>
        public bool EnsureFieldSelection(FieldSelection targetField, bool viaTabKey)
        {
            try
            {
                if (!IsDocumentLoaded)
                {
                    _initialSelection = targetField;
                    return false;
                }

                if (viaTabKey && ApplyPendingTabNavigation(true))
                {
                    return false;
                }

                if (targetField != FieldSelection.DoNotReset)
                {
                    ClearSelection();
                }
                else
                {
                    var selectedAttribute = ActiveAttributeGenealogy(true, null);
                    if (selectedAttribute?.Any() == true)
                    {
                        return true;
                    }
                }

                bool advanced = (targetField == FieldSelection.Error)
                    ? GoToNextInvalid(includeWarnings: true, loop: true)
                    : AdvanceToNextTabStop(targetField != FieldSelection.Last, viaTabKey: false);
                
                OnItemSelectionChanged();

                return advanced;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40236");
            }
        }

        /// <summary>
        /// Clears any active field selection.
        /// </summary>
        public void ClearSelection()
        {
            try
            {
                _activeDataControl?.IndicateActive(false, ActiveSelectionColor);

                // Remove highlights so that when you click on the document type combo the last selected control doesn't show its highlight.
                // The above line, IndicateActive(false, ActiveSelectionColor), will redisplay the highlight for a table cell
                // that is in edit mode so this remove highlight call needs to happen after that.
                // https://extract.atlassian.net/browse/ISSUE-17146
                RemoveActiveAttributeHighlights();

                _activeDataControl = null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50111");
            }
        }

        /// <summary>
        /// Add a message filter that will run, mostly, before this instance handles messages
        /// </summary>
        /// <remarks>
        /// Filters will be run in last-added-first order
        /// </remarks>
        /// <param name="messageFilter">The <see cref="IMessageFilter"/> to add</param>
        public void AddMessageFilter(IMessageFilter messageFilter)
        {
            _messageFilters =
                FSharpList<MessageFilterType>.Cons(messageFilter.PreFilterMessage, _messageFilters);
        }

        /// <summary>
        /// Remove a message filter from the list
        /// </summary>
        /// <param name="messageFilter">The <see cref="IMessageFilter"/> to remove</param>
        public void RemoveMessageFilter(IMessageFilter messageFilter)
        {
            _messageFilters = _messageFilters
                .Where(func => func != messageFilter.PreFilterMessage)
                .ToFSharpList();
        }

        /// <summary>
        /// If <see cref="AttributeStatusInfo.PerformanceTesting"/> is true, logs an exception reporting
        /// peformance data that has been collected in the current verification session.
        /// </summary>
        public static void ReportPerformanceResults()
        {
            try
            {
                if (_performanceTestingStartTime.HasValue)
                {
                    var ee = new ExtractException("ELI36157", "TotalTime: " +
                                (DateTime.Now - _performanceTestingStartTime.Value).ToString(
                                    "g", CultureInfo.CurrentCulture));
                    DataEntryQuery.ReportPerformanceData(ee);
                    ee.Log();
                    _performanceTestingStartTime = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53595");
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
        /// Raised when the <see cref="IsDataUnviewed"/> property changes.
        /// </summary>
        public event EventHandler<EventArgs> UnviewedDataStateChanged;

        /// <summary>
        /// Raised when the <see cref="DataValidity"/> property changes.
        /// </summary>
        public event EventHandler<EventArgs> DataValidityChanged;

        /// <summary>
        /// Fired to notify listeners that a new set of items has been selected.
        /// </summary>
        public event EventHandler<ItemSelectionChangedEventArgs> ItemSelectionChanged;

        /// <summary>
        /// Indicates that the <see cref="IMessageFilter.PreFilterMessage"/> method has
        /// handled the <see cref="Message"/> and will return true.
        /// </summary>
        public event EventHandler<MessageHandledEventArgs> MessageHandled;

        /// <summary>
        /// Indicates that a significant update of control values has ended. Examples include
        /// loading a new document or selecting/creating a new table row on a table with dependent
        /// controls.
        /// </summary>
        public event EventHandler<EventArgs> UpdateEnded;

        /// <summary>
        /// Raised just before the DEP's data is saved to disk.
        /// </summary>
        public event EventHandler<AttributesEventArgs> DataSaving;

        /// <summary>
        /// Raised whenever the DEP's data has been saved.
        /// </summary>
        public event EventHandler<EventArgs> DataSaved;

        /// <summary>
        /// Occurs when a document has finished loading.
        /// </summary>
        public event EventHandler<EventArgs> DocumentLoaded;

        /// <summary>
        /// Raised whenever a <see cref="DataEntryControl"/> is registered in the panel.
        /// </summary>
        public event EventHandler<DataEntryControlEventArgs> ControlRegistered;

        /// <summary>
        /// Raised whenever a <see cref="DataEntryControl"/> is unregistered from the panel.
        /// </summary>
        public event EventHandler<DataEntryControlEventArgs> ControlUnregistered;

        /// <summary>
        /// Raised when tab navigation is activated; if handled, the application hosting the DEP
        /// should implement the UI response to the navigation. If not handled, this class will.
        /// (including loop around to the first/last tab stop of the DEP if necessary)
        /// </summary>
        public event EventHandler<TabNavigationEventArgs> TabNavigation;

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

                if (!_inDesignMode)
                {
                    // If testing performance, record the start time when the form is loaded.
                    if (AttributeStatusInfo.PerformanceTesting && !_performanceTestingStartTime.HasValue)
                    {
                        _performanceTestingStartTime = DateTime.Now;
                    }

                    ExtractException.Assert("ELI30678", "Application data not initialized.",
                        _dataEntryApp != null);

                    ExtractException.Assert("ELI25377", "Highlight colors not initialized!",
                        _highlightColors != null && _highlightColors.Length > 0);

                    // Initialize the font to use for tooltips
                    _toolTipFont = new Font(Config.Settings.FontFamily, Config.Settings.TooltipFontSize);

                    // Loop through all contained controls looking for controls that implement the 
                    // IDataEntryControl interface.  Registers events necessary to facilitate
                    // the flow of information between the controls.
                    RegisterDataEntryControls(this);

                    // Create and initialize smart tag support for all text controls.
                    InitializeSmartTagManager();

                    Application.Idle += HandleApplicationIdle;
                }

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI23679");
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.ParentChanged"/> event.
        /// Overridden to handle the case that this panel was either added to or removed from the
        /// application's DEP pane.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnParentChanged(EventArgs e)
        {
            try
            {
                base.OnParentChanged(e);

                ProcessAncestorChange();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI30625", ex);
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
            try
            {
                base.OnEnter(e);

                _regainingFocus = true;

                // https://extract.atlassian.net/browse/ISSUE-17127
                // The panel may not have been able to track all shift/tab key changes in the case
                // that is was closed in the Pagination UI
                // Re-initialize shift/tab key status every time focus is regained in the DEP.
                _shiftKeyDown = ModifierKeys.HasFlag(Keys.Shift);
                _tabKeyDown = ModifierKeys.HasFlag(Keys.Tab);

                // https://extract.atlassian.net/browse/ISSUE-13981
                // Ensure _regainingFocus only remains set for the duration of the action that
                // triggered this OnEnter. Some events (such as swipes) that trigger OnEnter do not
                // trigger a subsequent GotFocus so _regainingFocus could get left as true and the
                // next focus even could wind up with multiple fields focused.
                ExecuteOnIdle("ELI41638", () => _regainingFocus = false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41639");
            }
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

                // https://extract.atlassian.net/browse/ISSUE-12987
                // If the DEP is being disposed, clear any cached data associated with this UI
                // thread. This ensures cached data is not leaked if verification is stopped and
                // restarted.
                AttributeStatusInfo.ClearQueryCache();

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

                if (_validationErrorProvider != null)
                {
                    _validationErrorProvider.Dispose();
                    _validationErrorProvider = null;
                }

                if (_validationWarningErrorProvider != null)
                {
                    _validationWarningErrorProvider.Dispose();
                    _validationWarningErrorProvider = null;
                }

                if (_warningIcon != null)
                {
                    NativeMethods.DestroyIcon(_warningIcon);
                    _warningIcon = null;
                }

                if (_blankIcon != null)
                {
                    NativeMethods.DestroyIcon(_blankIcon);
                    _blankIcon = null;
                }

                if (_toolTipFont != null)
                {
                    _toolTipFont.Dispose();
                    _toolTipFont = null;
                }

                foreach (DataEntryToolTip toolTip in _attributeToolTips.Values)
                {
                    toolTip.Dispose();
                }
                _attributeToolTips.Clear();

                if (_hoverToolTip != null)
                {
                    _hoverToolTip.Dispose();
                    _hoverToolTip = null;
                }

                if (_smartTagManager != null)
                {
                    _smartTagManager.Dispose();
                    _smartTagManager = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles a new cursor tool being selected so that we can keep track of the most recently
        /// used cursor tool.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="CursorToolChangedEventArgs"/> that contains the event data.
        /// </param>
        void HandleCursorToolChanged(object sender, CursorToolChangedEventArgs e)
        {
            try
            {
                if (e.CursorTool != _lastCursorTool)
                {
                    if (e.CursorTool == CursorTool.SelectLayerObject)
                    {
                        ImageViewer.CursorEnteredLayerObject += HandleCursorEnteredLayerObject;
                        ImageViewer.CursorLeftLayerObject += HandleCursorLeftLayerObject;
                    }
                    else if (_lastCursorTool == CursorTool.SelectLayerObject)
                    {
                        ImageViewer.CursorEnteredLayerObject -= HandleCursorEnteredLayerObject;
                        ImageViewer.CursorLeftLayerObject -= HandleCursorLeftLayerObject;
                    }

                    if (e.CursorTool != CursorTool.None)
                    {
                        _lastCursorTool = e.CursorTool;
                    }
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
        void HandleControlGotFocus(object sender, EventArgs e)
        {
            try
            {
                if (!IsDocumentLoaded)
                {
                    return;
                }

                // Keep track of the control that should be gaining focus.
                _focusingControl = (IDataEntryControl)sender;

                // As part of normal changes, we ensure the active control mementos are sent last,
                // but as part of undo operations, they need to be sent first.
                if (InUndo && _focusingControl != _activeDataControl)
                {
                    var activeControlMemento =
                        new DataEntryActiveControlMemento(_activeDataControl);

                    AttributeStatusInfo.UndoManager.AddMemento(activeControlMemento);
                }

                // Schedule the focus change to be handled via the message queue. This prevents
                // situations where focus can be called from within the GotFocus handler as warned
                // against here:
                // http://msdn.microsoft.com/en-us/library/system.windows.forms.control.enter.aspx
                BeginInvoke(new DataEntryControlDelegate(DataEntryControlGotFocusInvoked),
                    new object[] { _focusingControl });
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24093", ex);
            } 
        }

        /// <summary>
        /// Handles a <see cref="IDataEntryControl"/> gaining focus, invoked via the message queue.
        /// </summary>
        /// <param name="newActiveDataControl">The <see cref="IDataEntryControl"/> that is gaining
        /// focus.</param>
        void DataEntryControlGotFocusInvoked(IDataEntryControl newActiveDataControl)
        {
            try
            {
                // In some cases one control may initially gain focus before focus is redirected.
                // This is usually due to .Net initially trying to direct focus (such as to the
                // first control in the form), before the DataEntryControlHost directs focus to
                // the appropriate data entry control.
                // In this case, ignore handling of the original focus change (handle only the most
                // recent focus change).
                if (newActiveDataControl != _focusingControl)
                {
                    return;
                }
                _focusingControl = null;

                Control lastActiveDataControl = (Control)_activeDataControl;

                // If a manual focus event is in progress, don't treat this as a regaining focus
                // event. The sender is the control programmatically given focus.
                if (_manualFocusEvent)
                {
                    _regainingFocus = false;
                    _manualFocusEvent = false;
                }
                // If the control host is getting focus back from an outside control, focus needs 
                // to be manually directed to the appropriate data entry control to override the 
                // default tab behavior which will otherwise assign focus to the first control 
                // within the control host.
                else if (_regainingFocus)
                {
                    _regainingFocus = false;

                    // [DataEntry:182]
                    // If focus returned via a mouse press and we have already determined which
                    // data entry control should receive focus as a result, set focus back to that
                    // control.
                    if (_clickedDataEntryControl is Control clickedControl)
                    {
                        newActiveDataControl = _clickedDataEntryControl;

                        // Don't call Focus() if the control already has focus because this will cause a dropped-down combo box to close
                        // https://extract.atlassian.net/browse/ISSUE-799
                        if (!clickedControl.Focused)
                        {
                            clickedControl.Focus();
                        }
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

                // Create a DataEntryActiveControlMemento now before _activeDataControl gets
                // re-assigned, but don't send it to the UndoManager until all messages in the
                // current chain are processed to ensure any significant events (like status
                // changes) are triggered by IndicateActive before this "Supporting" memento is
                // added. This ensures that the control that was active prior to the focus event
                // that triggered the change is restored.
                if (_activeDataControl != null)
                {
                    var activeControlMemento =
                        new DataEntryActiveControlMemento(_activeDataControl);

                    ExecuteOnIdle("ELI34417", () =>
                        AttributeStatusInfo.UndoManager.AddMemento(activeControlMemento));
                }

                // If a refresh of the highlights is needed and the control is going out of focus,
                // refresh the highlights now.
                if (_refreshActiveControlHighlights && !_changingData)
                {
                    RefreshActiveControlHighlights();
                }

                // If focus has changed to another control, it is up to that control to indicate via
                // the AttributesSelected event whether an attribute group is selected.
                _currentlySelectedGroupAttribute = null;

                // Notify AttributeStatusInfo that the current edit is over so that a
                // non-incremental value modified event can be raised.
                AttributeStatusInfo.EndEdit();

                // De-activate any existing control that is active
                if (_activeDataControl != null)
                {
                    _activeDataControl.IndicateActive(false, ActiveSelectionColor);
                }

                // If this method was attempting to change focus, there is at least once case where
                // the control that was to gain focus does not yet have it. That case is where a
                // table was in edit mode-- moving focus away from the table ends edit mode which
                // triggers the table to re-focus the table. Make a second attempt at focusing the
                // desired control if it does not yet have focus.
                Control newControl = (Control)newActiveDataControl;
                if (!newControl.Focused)
                {
                    // Keep track of refocus attempts to prevent the possibility of infinite
                    // recursion.
                    if (_refocusingControl == newActiveDataControl)
                    {
                        new ExtractException("ELI30036",
                            "Application trace: Failed to activate control").Log();
                    }
                    else
                    {
                        _refocusingControl = newActiveDataControl;
                        newControl.Focus();
                        return;
                    }
                }

                _refocusingControl = null;

                _activeDataControl = newActiveDataControl;
                _activeDataControl.IndicateActive(true, ActiveSelectionColor);

                // Once a new control gains focus, show tooltips again if they were hidden.
                if (_temporarilyHidingTooltips)
                {
                    _temporarilyHidingTooltips = false;

                    // The error icons for selected attributes will re-display automatically,
                    // but if all highlights are showing, need to re-display all error icons.
                    if (_dataEntryApp.ShowAllHighlights)
                    {
                        ShowAllErrorIcons(true);
                    }
                }

                DrawHighlights(true);

                // Enable or disable swiping as appropriate.
                OnSwipingStateChanged(new SwipingStateChangedEventArgs(SwipingEnabled));

                OnItemSelectionChanged();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30202", ex);
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
        void HandleAttributesSelected(object sender, AttributesSelectedEventArgs e)
        {
            try
            {
                // If an attribute has been selected, _manualFocusEvent and _regainingFocus can be
                // reset here. Otherwise, focus changes between multiple cells in the same control
                // can leave these set causing unexpected attributes selected.
                _manualFocusEvent = false;
                _regainingFocus = false;

                // Note whether the current selection was due to tabbing (used by the tabbing-by-row
                // logic).
                if (_tabKeyDown)
                {
                    _lastNavigationViaTabKey = !_shiftKeyDown;
                }
                else
                {
                    _lastNavigationViaTabKey = null;
                }

                ExtractException endEditException = null;
                try
                {
                    // Notify AttributeStatusInfo that the current edit is over so that a
                    // non-incremental value modified event can be raised.
                    AttributeStatusInfo.EndEdit();
                }
                catch (Exception ex)
                {
                    // If an exception happens while processing EndEdit, we still need to continue
                    // processing the selection change--  while EndEdit should have cleaned up any
                    // bad value, if this method does not run to completion, tab order and
                    // highlighting will be in a bad state.
                    endEditException = ExtractException.AsExtractException("ELI27097", ex);
                }

                // Displays the appropriate highlights, tooltips and error icons.
                // Don't finalize (change pages, draw highlights) if a control focus change to
                // another control is pending.
                bool suppressSelectionFinalization =
                    _focusingControl != null && _focusingControl != e.SelectionState.DataControl;
                ApplySelection(e.SelectionState, suppressSelectionFinalization);

                // If an exception was thrown from EndEdit, throw it here.
                if (endEditException != null)
                {
                    throw endEditException;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24094", ex);
            }
        }

        /// <summary>
        /// Handles the case that a <see cref="IDataEntryControl"/> has begun processing an update
        /// based on user interaction with that control. Calls to <see cref="DrawHighlights"/> will
        /// be suspended until the update is complete.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        void HandleControlUpdateStarted(object sender, EventArgs e)
        {
            try
            {
                ControlUpdateReferenceCount++;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27269", ex);
            }
        }

        /// <summary>
        /// Handles the case that a <see cref="IDataEntryControl"/> has finished processing an
        /// update based on user interaction with that control.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        void HandleControlUpdateEnded(object sender, EventArgs e)
        {
            try
            {
                ControlUpdateReferenceCount--;

                if (ControlUpdateReferenceCount == 0)
                {
                    DrawHighlights(true);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27270", ex);
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
        void HandleAttributeValueModified(object sender, AttributeValueModifiedEventArgs e)
        {
            try
            {
                if (InUndo || InRedo)
                {
                    // None of the below code needs to execute when undoing or re-doing attribute
                    // values.
                    return;
                }

                // [DataEntry:757, 798]
                // Check to see if the clipboard should be cleared based on a control's
                // ClearClipboardOnPaste setting. This code is a shortcut to be able to implement
                // ClearClipboardOnPaste in one place rather than overriding more controls 
                // (such as DataGridViewTextBoxEditingControl) and intercepting WM_PAINT message in
                // WndProc. It assumes that when text is on the clipboard that matches the text
                // that the new attribute's value ends with that it has been pasted.
                // NOTE: Don't call Clipboard.ContainsText separately... that seems to lead to
                // "Clipboard operation did not succeed" exceptions in this case.
                // https://extract.atlassian.net/browse/ISSUE-16596
                // Trying to access the clipboard in a background context can cause a exceptions which
                // spam the log file.
                if (e.AutoUpdatedAttributes.Count == 0
                     && DataEntryApplication?.RunningInBackground == false)
                {
                    IDataEntryControl owningControl =
                        AttributeStatusInfo.GetOwningControl(e.Attribute);
                    if (owningControl != null && owningControl.ClearClipboardOnPaste)
                    {
                        try
                        {
                            string text = Clipboard.GetText();

                            if (e.Attribute.Value.String.EndsWith(text, StringComparison.Ordinal))
                            {
                                DataEntryMethods.ClearClipboardData();
                            }
                        }
                        catch (Exception ex)
                        {
                            // https://extract.atlassian.net/browse/ISSUE-14305
                            // Clipboard operations being finicky has long been an issue. Don't allow a
                            // failure checking clipboard data to cause a larger issue.
                            ex.ExtractLog("ELI41668");
                        }
                    }
                }

                if (AttributeStatusInfo.IsAttributePersistable(e.Attribute))
                {
                    OnDataChanged();
                }

                // Blank attributes will not be counted as unviewed in terms of the value of
                // IsDataUnviewed, but if a query applies a value while still in the unviewed state,
                // then it should count as unviewed.
                if (!AttributeStatusInfo.HasBeenViewedOrIsNotViewable(e.Attribute, false) &&
                    !string.IsNullOrWhiteSpace(e.Attribute.Value.String))
                {
                    var previousSet = _unviewedAttributes;
                    _unviewedAttributes = _unviewedAttributes.Add(e.Attribute);
                    if (previousSet.IsEmpty)
                    {
                        OnUnviewedDataStateChanged();
                    }
                }

                // If the spatial info for a viewable attribute has changed, re-create the highlight 
                // for the attribute with the new spatial information.
                if (e.SpatialInfoChanged && AttributeStatusInfo.IsAttributeViewable(e.Attribute))
                {
                    RemoveAttributeHighlight(e.Attribute);

                    SetAttributeHighlight(e.Attribute, false);

                    _refreshActiveControlHighlights = true;

                    // Update the highlights as long as image is not currently loading or a swipe
                    // is not currently in progress.
                    if (!_changingData && !_processingSwipe)
                    {
                        DrawHighlights(false);
                    }
                }

                // Update the attribute's highlights if the modification is not happening during 
                // image loading, and the update is coming from the active data control.
                // [DataEntry:329]
                // Don't accept the text value if the value modification is happening as a
                // result of a swipe.
                if (!_changingData && _activeDataControl != null && !_processingSwipe
                    && e.AcceptSpatialInfo)
                {
                    SelectionState selectionState;
                    _controlSelectionState.TryGetValue(_activeDataControl, out selectionState);

                    // For any attributes that have hints or that had not previously been accepted,
                    // reset their highlights to reflect 100 percent confidence. (The color of a
                    // hint will depend upon whether text has been entered)
                    if (selectionState != null && selectionState.Attributes.Contains(e.Attribute) &&
                        (AttributeStatusInfo.GetHintType(e.Attribute) != HintType.None ||
                         !AttributeStatusInfo.IsAccepted(e.Attribute)))
                    {
                        AttributeStatusInfo.AcceptValue(e.Attribute, true);

                        RemoveAttributeHighlight(e.Attribute);

                        // [DataEntry:261] Highlights should be made visible except indirect
                        // hints when multiple attributes are active.
                        bool makeVisible = ((selectionState.Attributes.Count == 1) || 
                             AttributeStatusInfo.GetHintType(e.Attribute) != HintType.Indirect);

                        SetAttributeHighlight(e.Attribute, makeVisible);
                    }

                    // Always redraw the highlights in order to create/update the tooltip of the
                    // attribute that changed (even if it is not the only one selected)
                    DrawHighlights(false);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24980", ex);
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
        void HandleAttributeDeleted(object sender, AttributeDeletedEventArgs e)
        {
            try
            {
                if (AttributeStatusInfo.IsAttributePersistable(e.DeletedAttribute))
                {
                    OnDataChanged();
                }

                if (e.DeletedAttribute.Value.HasSpatialInfo())
                {
                    // A refresh of highlights is needed now that attributes have been deleted.
                    _refreshActiveControlHighlights = true;
                }

                ProcessDeletedAttributes(DataEntryMethods.AttributeAsVector(e.DeletedAttribute));
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24918", ex);
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
        void HandleLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            // Keep track of whether this event kicks off processing of a swipe.
            bool startedSwipeProcessing = false;

            try
            {
                // Don't attempt to process the event as a swipe if the user cancelled a swipe after
                // starting it, if a swipe is already being processed, or if the active control does
                // not support swiping.
                if (!_processingSwipe && SwipingEnabled)
                {
                    List<RasterZone> highlightedZones = new List<RasterZone>();

                    // Angular and rectangular swipes will be highlights.
                    Highlight highlight = e.LayerObject as Highlight;
                    if (highlight != null)
                    {
                        highlightedZones.Add(highlight.ToRasterZone());
                    }
                    else
                    {
                        // Word highlighter swipes will be CompositeHighlightLayerObjects.
                        CompositeHighlightLayerObject compositeHighlight =
                            e.LayerObject as CompositeHighlightLayerObject;
                        if (compositeHighlight != null)
                        {
                            highlightedZones.AddRange(compositeHighlight.GetRasterZones());
                        }
                    }

                    if (highlightedZones.Count > 0)
                    {
                        startedSwipeProcessing = true;
                        _processingSwipe = true;

                        ImageViewer.LayerObjects.Remove(e.LayerObject, true, false);

                        // Recognize the text in the highlight's raster zone and send it to the active
                        // data control for processing.
                        using (new TemporaryWaitCursor())
                        {
                            SpatialString ocrText = null;

                            try
                            {

                                foreach (RasterZone zone in highlightedZones)
                                {
                                    // [DataEntry:294] Keep the angle threshold small so long swipes
                                    // on slightly skewed docs don't include more text than intended.
                                    SpatialString zoneOcrText = _ocrManager.GetOcrText(
                                        ImageViewer.ImageFile, zone, 0.2);

                                    if (ocrText == null)
                                    {
                                        ocrText = zoneOcrText;
                                    }
                                    else
                                    {
                                        ocrText.Append(zoneOcrText);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ExtractException.Log("ELI27101", ex);
                                
                                // If the OCR engine errored, display a tooltip and ignore the
                                // swipe.
                                ShowUserNotificationTooltip("An error was encountered reading " +
                                    "the swiped text.");
                                return;
                            }

                            // If a highlight was created using the auto-fit mode of the word
                            // highlight tool, create a hybrid string whose spatial area is the full
                            // area of the "swipe" rather than just what OCR'd. (allows an easy way
                            // to add spatial info for a field.)
                            if (ImageViewer.CursorTool == CursorTool.WordHighlight)
                            {
                                // Create unrotated/skewed spatial page info for the resulting
                                // hybrid string.
                                var spatialPageInfos = new LongToObjectMap();
                                var spatialPageInfo = new SpatialPageInfo();
                                spatialPageInfo.Initialize(ImageViewer.ImageWidth, ImageViewer.ImageHeight, 0, 0);
                                spatialPageInfos.Set(ImageViewer.PageNumber, spatialPageInfo);

                                // Create the hybrid result using the spatial data from the swipe
                                // with the text from the OCR attempt.
                                var hybridOcrText = new SpatialString();
                                hybridOcrText.CreateHybridString(
                                    highlightedZones
                                        .Select(rasterZone => rasterZone.ToComRasterZone())
                                        .ToIUnknownVector(),
                                    (ocrText == null) ? "" : ocrText.String,
                                    ImageViewer.ImageFile, spatialPageInfos);
                                ocrText = hybridOcrText;
                            }
                            else
                            {
                                // If no OCR results were produced, notify the user.
                                if (ocrText == null || string.IsNullOrEmpty(ocrText.String))
                                {
                                    ShowUserNotificationTooltip("No text was recognized.");
                                    return;
                                }
                            }

                            SendSpatialStringToActiveControl(ocrText);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If this event kicked off processing of a swipe, refresh the image viewer so the
                // swipe highlight is removed.
                if (startedSwipeProcessing)
                {
                    try
                    {
                        ImageViewer.Invalidate();
                    }
                    catch (Exception ex2)
                    {
                        ExtractException.Display("ELI24089", ex2);
                    }
                }

                // It should be highly unlikely to catch an exception here; most processes
                // that can result in an exception from swiping should trigger a user notification
                // tooltip.
                ExtractException.Display("ELI24090", ex);
            }
            finally
            {
                if (startedSwipeProcessing)
                {
                    // Only if startedSwipeProcessing is set are we dealing with the original swipe.
                    // Otherwise, the layer is a side effect of a swipe and the swipe event is still
                    // in progress.
                    _processingSwipe = false;
                }
            }
        }

        /// <summary>
        /// Handles the case that OCR text is highlighted in the image viewer. (i.e., "swiped" with
        /// the word highlight tool.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.OcrTextEventArgs"/> instance
        /// containing the event data.</param>
        void HandleOcrTextHighlighted(object sender, OcrTextEventArgs e)
        {
            try
            {
                _processingSwipe = true;

                SendSpatialStringToActiveControl(e.OcrData.SpatialString);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34069");
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
        void HandleSwipingStateChanged(object sender, SwipingStateChangedEventArgs e)
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
        void HandleAttributeInitialized(object sender, AttributeInitializedEventArgs e)
        {
            try
            {
                if (AttributeStatusInfo.IsAttributePersistable(e.Attribute))
                {
                    OnDataChanged();
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

                // Keep track of newly created attributes so that empty ones can be marked as
                // viewed.
                if (!_changingData)
                {
                    _newlyAddedAttributes.Add(e.Attribute);
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
        void HandleViewedStateChanged(object sender,
            ViewedStateChangedEventArgs e)
        {
            try
            {
                bool previousIsDataViewed = IsDataUnviewed;
                if (e.IsDataViewed)
                {
                    _unviewedAttributes = _unviewedAttributes.Remove(e.Attribute);
                }
                else if (!string.IsNullOrWhiteSpace(e.Attribute.Value.String))
                {
                    _unviewedAttributes = _unviewedAttributes.Add(e.Attribute);
                }

                if (IsDataUnviewed != previousIsDataViewed)
                {
                    OnUnviewedDataStateChanged();
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
        void HandleValidationStateChanged(object sender,
            ValidationStateChangedEventArgs e)
        {
            try
            {
                if (e.DataValidity == DataValidity.Valid)
                {
                    // Do not remove from _invalidAttributes any attributes that currently don't
                    // have validation enabled; they should continue to be tracked in the case
                    // validation is re-enabled.
                    if (_invalidAttributes.Contains(e.Attribute)
                        && AttributeStatusInfo.IsValidationEnabled(e.Attribute))
                    {
                        _invalidAttributes = _invalidAttributes.Remove(e.Attribute);
                    }
                }
                else
                {
                    _invalidAttributes = _invalidAttributes.Add(e.Attribute);
                }

                var newDataValidity = DataValidity;
                if (_dataValidity != newDataValidity)
                {
                    OnDataValidityChanged();
                }

                _dataValidity = newDataValidity;

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
        void HandleImageViewerPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            try
            {
                if (!ProcessImageViewerKeyboardInput)
                {
                    return;
                }

                Control activeControl = _activeDataControl as Control;

                // Give focus back to the active control so it can receive input.
                // Allowing F10 to be passed to the DEP causes the file menu to be selected for some
                // reason... exempt F10 from being handled here.
                // [DataEntry:335] Don't handle tab key either since that already has special
                // handling in PreFilterMessage.
                bool sendKeyToActiveControl = (!ImageViewer.Capture && activeControl != null && 
                    !activeControl.Focused && e.KeyCode != Keys.F10 && e.KeyCode != Keys.Tab);

                // If the image viewer is in a separate form which has focus, DataEntryApplication's
                // ProcessCmdKey (which enables shortcuts to be processed) will not receive
                // keystrokes and sendKeyToActiveControl will not be true. Therefore, also redirect
                // keystrokes to the DEP when the image viewer is receiving input while another
                // form is active.
                // TODO: I think there's probably better ways to do this, and I think the code to
                // handle this situation is better suited in DataEntryApplicationForm.
                bool separateImageWindowKeyEvent =
                    (!sendKeyToActiveControl && ParentForm != Form.ActiveForm);

                if (sendKeyToActiveControl || separateImageWindowKeyEvent)
                {
                    Control target = sendKeyToActiveControl ? activeControl : this;

                    if (KeyMethods.SendKeyToControl(e.KeyValue, e.Shift, e.Control, e.Alt, target))
                    {
                        // Allowing special key handling (when IsInputKey == false) seems to cause
                        // arrow keys as well as some shortcuts to do unexpected things.  Set
                        // IsInputKey to true to disable special handling.
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
        /// Handles the <see cref="ImageViewer"/> CursorEnteredLayerObject event in order
        /// to display a tooltip for data the selection tool is currently hovering over.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="LayerObjectEventArgs"/> that contains the event data.
        /// </param>
        void HandleCursorEnteredLayerObject(object sender, LayerObjectEventArgs e)
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
                    // [DataEntry:1194]
                    // Don't allow an attribute to become a hover attribute if the highlight is
                    // somehow still around for an attribute that no longer has spatial info.
                    IAttribute attribute;
                    if (_highlightAttributes.TryGetValue(highlight, out attribute) &&
                        AttributeStatusInfo.HasSpatialInfo(attribute, true))
                    {
                        // If there is not currently a hover attribute or the current hover attribute
                        // is a hint while the new candidate is not, use the new attribute as the
                        // hover attribute.
                        if (_hoverAttribute == null ||
                            (AttributeStatusInfo.GetHintType(attribute) == HintType.None &&
                             AttributeStatusInfo.GetHintType(_hoverAttribute) != HintType.None))
                        {
                            // Remove any tooltip the previous hover attribute may have had.
                            if (_hoverAttribute != null)
                            {
                                RemoveAttributeToolTip(_hoverAttribute);
                            }

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
        /// Handles the <see cref="ImageViewer"/> CursorLeftLayerObject event in order
        /// to remove a tooltip for data the selection tool was previously hovering over.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="LayerObjectEventArgs"/> that contains the event data.
        /// </param>
        void HandleCursorLeftLayerObject(object sender, LayerObjectEventArgs e)
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
                        // If the layer object the selection tool just left was a highlight for the
                        // current hover attribute, clear the hover attribute.
                        IAttribute attribute;
                        if (_highlightAttributes.TryGetValue(highlight, out attribute) &&
                            attribute == _hoverAttribute)
                        {
                            RemoveAttributeToolTip(_hoverAttribute);
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
        void HandleImageViewerMouseDown(object sender, MouseEventArgs e)
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
                        RemoveAttributeToolTip(_hoverAttribute);
                        _hoverAttribute = null;

                        // Indicate a manual focus event so that HandleControlGotFocus allows the
                        // new attribute selection rather than overriding it.
                        _manualFocusEvent = true;

                        // Propagate and select the former hover attribute.
                        PropagateAttributes(attributesToPropagate, true, false);

                        // The tooltip has been removed. Call DrawHighlights to ensure it is
                        // redisplayed as part of the new selection.
                        DrawHighlights(true);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25415", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the comment control's text has changed.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleCommentTextChanged(object sender, EventArgs e)
        {
            try
            {
                _dataEntryApp.DatabaseComment = _commentControl.Text;

                OnDataChanged();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26968", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the  <see cref="ImageViewer"/>'s level of zoom has been changed.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleImageViewerZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            try
            {
                if (!_performingProgrammaticZoom)
                {
                    Rectangle viewArea = ImageViewer.GetTransformedRectangle(
                        ImageViewer.GetVisibleImageArea(), true);
                    Size zoomSize = viewArea.Size;

                    // Before assigning _lastNonAutoZoomViewArea and _lastManualZoomSize, be sure
                    // the size > 0 in both dimensions (will be zero if image viewer window is
                    // minimized).
                    if (zoomSize.Width > 0 && zoomSize.Height > 0)
                    {
                        _lastManualZoomSize = zoomSize;
                        _zoomedToSelection = false;
                        _manuallyZoomedToSelection = false;
                        _lastNonZoomedToSelectionViewArea = viewArea;
                        _lastNonZoomedToSelectionFitMode = ImageViewer.FitMode;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27056", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the  <see cref="ImageViewer"/>'s scroll position has been changed.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleImageViewerScrollPositionsChanged(object sender, EventArgs e)
        {
            try
            {
                // Don't allow scrolling to reset _zoomedToSelection or
                // _lastNonZoomedToSelectionViewArea.
                if (!_performingProgrammaticZoom && !_zoomedToSelection)
                {
                    Rectangle viewArea = ImageViewer.GetTransformedRectangle(
                        ImageViewer.GetVisibleImageArea(), true);
                    _lastManualZoomSize = viewArea.Size;
                    _lastNonZoomedToSelectionViewArea = viewArea;
                    _lastNonZoomedToSelectionFitMode = ImageViewer.FitMode;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27067", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the  <see cref="ImageViewer"/> has changed pages.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="PageChangedEventArgs"/> that contains the event data.
        /// </param>
        void HandleImageViewerPageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                // [DataEntry:661]
                // After changing pages, draw highlights, otherwise the tooltip for the selected
                // control will not be visible.
                DrawHighlights(false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27586", ex);
                ee.AddDebugData("Event Data", e, false);
                throw ee;
            }
        }

        /// <summary>
        /// Handles the <see cref="SmartTagManager.ApplyingValue"/> event so that any smart tags
        /// values that need to be resolved using a <see cref="DataEntryQuery"/> can be.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="SmartTagApplyingValueEventArgs"/> that contains the event data.
        /// </param>
        void HandleSmartTagApplyingValue(object sender, SmartTagApplyingValueEventArgs e)
        {
            try
            {
                // If a smart tag was selected and the tag value being applied is a query, execute a
                // DataEntryQuery on the value and update the value with the result.
                if (e.SmartTagSelected  &&
                    e.Value.StartsWith("<Query", StringComparison.OrdinalIgnoreCase))
                {
                    // Find the attribute that should be used as the root attribute for a
                    // DataEntryQuery.
                    IAttribute activeAttribute = GetActiveAttribute(null);

                    DataEntryQuery dataEntryQuery =
                        DataEntryQuery.Create(e.Value, activeAttribute, _dbConnections);
                    QueryResult queryResult = dataEntryQuery.Evaluate();
                    e.Value = queryResult.ToString();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28878", ex);
                ee.AddDebugData("Event Data", e, false);
                throw ee;
            }
        }

        /// <summary>
        /// Handles the ImageViewer FitModeChanged event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="FitModeChangedEventArgs"/> that contains the event data.
        /// </param>
        void HandleImageViewerFitModeChanged(object sender, FitModeChangedEventArgs e)
        {
            try
            {
                // If the fit mode was changed, enforce auto-zoom on the next selection even if
                // the next selection is the same.
                _lastViewArea = new Rectangle();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI29143", ex);
                ee.AddDebugData("Event Data", e, false);
                throw ee;
            }
        }

        /// <summary>
        /// Handles the case that the value of <see cref="IDataEntryApplication.ShowAllHighlights"/>
        /// has changed.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleShowAllHighlightsChanged(object sender, EventArgs e)
        {
            try
            {
                // This instance may be part of a doc type configuration that is not currently
                // active. If _imageViewer is not set, don't do anything.
                if (_imageViewer != null)
                {
                    // Show or hide all highlights as appropriate.
                    foreach (IAttribute attribute in _attributeHighlights.Keys)
                    {
                        bool selected = _displayedAttributeHighlights.ContainsKey(attribute);

                        // If _showAllHighlights is being set to true, show the highlight
                        // (unless it is an indirect hint).
                        if (_dataEntryApp.ShowAllHighlights)
                        {
                            if (!selected && !_temporarilyHidingTooltips)
                            {
                                // Create a new error icon so that its position will
                                // be based off of the attribute, not a tooltip
                                CreateAttributeErrorIcon(attribute, true);
                            }

                            if (AttributeStatusInfo.HasSpatialInfo(attribute, true))
                            {
                                ShowAttributeHighlights(attribute, true);
                            }
                        }
                        // Hide all other attributes as long as they are not part of the active
                        // selection.
                        else if (!selected)
                        {
                            ShowAttributeHighlights(attribute, false);
                        }
                    }

                    DrawHighlights(false);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30676", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Application.Idle"/> event in order to execute any pending
        /// commands from <see cref="ExecuteOnIdle"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleApplicationIdle(object sender, EventArgs e)
        {
            try
            {
                if (_idleCommands.Count > 0)
                {
                    Tuple<Action, string> command = _idleCommands.Dequeue();
                    this.SafeBeginInvoke(command.Item2, () => command.Item1());
                }
                else
                {
                    _isIdle = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31023", ex);
            }
        }

        #endregion Event Handlers

        #region Internal Members

        /// <overloads>Retrieves the <see cref="RasterZone"/>s of the specified
        /// <see cref="IAttribute"/>(s) highlights grouped by page.</overloads>
        /// <summary>
        /// Retrieves the <see cref="RasterZone"/>s of the specified <see cref="IAttribute"/>
        /// highlights grouped by page.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose highlight 
        /// <see cref="RasterZone"/>s will be returned.</param>
        /// <param name="includeSubAttributes"><see langword="true"/> if raster zones from all
        /// descendants of the specified <see cref="IAttribute"/> should be included;
        /// <see langword="false"/> if they should not.</param>
        /// <returns>The <see cref="RasterZone"/>s of the <see paramref="attribute"/> grouped by
        /// page.</returns>
        Dictionary<int, List<RasterZone>> GetAttributeRasterZonesByPage(IAttribute attribute, 
            bool includeSubAttributes)
        {
            return GetAttributeRasterZonesByPage(new IAttribute[] { attribute },
                includeSubAttributes);
        }

        /// <summary>
        /// Retrieves the <see cref="RasterZone"/>s of the specified <see cref="IAttribute"/>s'
        /// highlights grouped by page.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/>s whose highlight
        /// <see cref="RasterZone"/>s will be returned.</param>
        /// <param name="includeSubAttributes"><see langword="true"/> if raster zones from all
        /// descendants of the specified <see cref="IAttribute"/>s should be included;
        /// <see langword="false"/> if they should not.</param>
        /// <returns>The <see cref="RasterZone"/>s of the <see paramref="attributes"/>' highlights
        /// grouped by page.</returns>
        Dictionary<int, List<RasterZone>> GetAttributeRasterZonesByPage(
            IEnumerable<IAttribute> attributes, bool includeSubAttributes)
        {
            Dictionary<int, List<RasterZone>> rasterZonesByPage =
                new Dictionary<int,List<RasterZone>>();

            // Loop through each attribute
            foreach (IAttribute attribute in
                DataEntryMethods.ToAttributeEnumerable(attributes, includeSubAttributes))
            {
                // Try to find the highlight(s) that have been associated with the attribute.
                List<CompositeHighlightLayerObject> highlights;
                if (_attributeHighlights.TryGetValue(attribute, out highlights))
                {
                    // Add the raster zones of each highlight to the appropriate page in the
                    // dictionary return value.
                    foreach (CompositeHighlightLayerObject highlight in highlights)
                    {
                        List<RasterZone> rasterZones;
                        if (!rasterZonesByPage.TryGetValue(highlight.PageNumber, out rasterZones))
                        {
                            rasterZones = new List<RasterZone>();
                            rasterZonesByPage[highlight.PageNumber] = rasterZones;
                        }

                        rasterZones.AddRange(highlight.GetRasterZones());
                    }
                }
            }

            return rasterZonesByPage;
        }

        /// <summary>
        /// Sorts the specified <see cref="IAttribute"/>s according to their positions in the
        /// document. The sorting will attempt to identify attributes in a row across the page and
        /// and work from left-to-right across the row before dropping down to the next row.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/>s to sort.</param>
        /// <returns>The sorted <see cref="IAttribute"/>s.</returns>
        internal List<IAttribute> SortAttributesSpatially(List<IAttribute> attributes)
        {
            // Compile dictionaries mapping each page to a list of attributes that begin on the
            // specified page and that map each attribute to a rectangle describing its bounds.
            Dictionary<int, List<IAttribute>> attributesByPage =
                new Dictionary<int, List<IAttribute>>();
            Dictionary<IAttribute, Rectangle> attributeBounds =
                new Dictionary<IAttribute, Rectangle>();

            // Loop through the specified attributes to collect the info.
            foreach(IAttribute attribute in attributes)
            {
                Dictionary<int, List<RasterZone>> rasterZonesByPage =
                    GetAttributeRasterZonesByPage(attribute, false);

                if (rasterZonesByPage.Count > 0)
                {
                    // Identify the first page the attribute is found on
                    int firstPage = -1;
                    foreach (int page in rasterZonesByPage.Keys)
                    {
                        if (firstPage == -1 || page < firstPage)
                        {
                            firstPage = page;
                        }
                    }

                    if (!attributesByPage.ContainsKey(firstPage))
                    {
                        attributesByPage[firstPage] = new List<IAttribute>();
                    }
                    attributesByPage[firstPage].Add(attribute);

                    // Calculate the attribute's overall bounds on its first page.
                    Rectangle bounds = new Rectangle();
                    foreach (RasterZone rasterZone in rasterZonesByPage[firstPage])
                    {
                        if (bounds.IsEmpty)
                        {
                            bounds = rasterZone.GetRectangularBounds();
                        }
                        else
                        {
                            bounds = Rectangle.Union(bounds, rasterZone.GetRectangularBounds());
                        }
                    }

                    attributeBounds[attribute] = bounds;
                }
            }

            // Using this data, sort the attributes. 
            List<IAttribute> sortedAttributes =
                SortAttributesSpatially(attributesByPage, attributeBounds);

            // The sorted attributes will not include non-spatial attributes. Append any non-spatial
            // attributes at the end of the list.
            foreach (IAttribute attribute in attributes)
            {
                if (!sortedAttributes.Contains(attribute))
                {
                    sortedAttributes.Add(attribute);
                }
            }

            return sortedAttributes;
        }

        /// <summary>
        /// Applies the selection state represented by <see paramref="selectionState"/> by
        /// displaying the appropriate highlights, tooltips and error icons.
        /// </summary>
        /// <param name="selectionState">The <see cref="SelectionState"/> to apply.</param>
        /// <param name="suppressSelectionFinalization"><see langword="true"/> if the highlight
        /// refresh and raising of <see cref="ItemSelectionChanged"/> should be suppressed;
        /// otherwise, <see langword="false"/>.</param>
        internal void ApplySelection(SelectionState selectionState, bool suppressSelectionFinalization)
        {
            ExtractException.Assert("ELI25169", "Null argument exception!", selectionState != null);
            ExtractException.Assert("ELI31018", "Null argument exception!",
                selectionState.DataControl != null);

            // [DataEntry:1176]
            // If in the middle of an update, the application of the selection needs to be delayed
            // until after the update is complete. ItemSelectionChanged depends on data calculated
            // in DrawHighlights, which is skipped during updates.
            if (ControlUpdateReferenceCount > 0)
            {
                _pendingSelection = selectionState;
            }

            SelectionState lastSelectionState;
            if (_controlSelectionState.TryGetValue(selectionState.DataControl, out lastSelectionState))
            {
                AttributeStatusInfo.UndoManager.AddMemento(
                    new DataEntrySelectionMemento(this, lastSelectionState));
            }

            _controlSelectionState[selectionState.DataControl] = selectionState;
            _controlToolTipAttributes[selectionState.DataControl] = new List<IAttribute>();

            // Once a new attribute is selected within a control, show tooltips again if they
            // were hidden.
            if (_temporarilyHidingTooltips)
            {
                _temporarilyHidingTooltips = false;

                // The error icons for selected attributes will re-display automatically,
                // but if all highlights are showing, need to re-display all error icons.
                if (_dataEntryApp.ShowAllHighlights)
                {
                    ShowAllErrorIcons(true);
                }
            }

            // Loop through each attribute and compile the raster zones from each.
            foreach (IAttribute attribute in selectionState.Attributes)
            {
                // If this attribute's value isn't viewable in the DEP, don't create a highlight for
                // it.
                if (!AttributeStatusInfo.IsAttributeViewable(attribute))
                {
                    continue;
                }

                // Only allow tooltips for attributes owned by dataControl.
                if (selectionState.DisplayToolTips &&
                    AttributeStatusInfo.GetOwningControl(attribute) == selectionState.DataControl)
                {
                    _controlToolTipAttributes[selectionState.DataControl].Add(attribute);
                }

                // Don't allow new highlights to be created if there is no longer a document loaded.
                if (ImageViewer != null && ImageViewer.IsImageAvailable)
                {
                    SetAttributeHighlight(attribute, false);
                }
            }

            // The DrawHighlights call and ItemSelectionChanged event may be delayed if this will be
            // done later.
            if (!suppressSelectionFinalization)
            {
                // If this is the active control and the image page is not in the process of being
                // changed, redraw all highlights.
                if (!_changingData && selectionState.DataControl == _activeDataControl)
                {
                    DrawHighlights(true);
                }

                OnItemSelectionChanged();

                _currentlySelectedGroupAttribute = selectionState.SelectedGroupAttribute;
            }
        }

        /// <summary>
        /// Indicates whether the specified <see paramref="key"/> is currently depressed.
        /// </summary>
        /// <param name="key">The <see cref="Keys"/> value to check if down.</param>
        /// <returns><see langword="true"/> if the specified key is currently down; otherwise,
        /// <see langword="false"/>.</returns>
        internal bool IsKeyDown(Keys key)
        {
            try
            {
                return _depressedKeys.Contains(key);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36172");
            }
        }

        #endregion Internal Members

        #region Protected Members

        /// <summary>
        /// Gets a <see cref="IDataEntryControl"/> that is currently in the process of gaining focus.
        /// </summary>
        /// <value>
        /// A <see cref="IDataEntryControl"/> that is currently in the process of gaining focus or
        /// <see langword="null"/> if no control is presently gaining focus.
        /// </value>
        protected IDataEntryControl FocusingControl
        {
            get
            {
                return _focusingControl;
            }
        }

        /// <summary>
        /// Gets a value indicating whether keyboard input directed at the <see cref="ImageViewer"/>
        /// should be processed by this panel.
        /// </summary>
        /// <value><c>true</c> if keyboard input directed at the <see cref="ImageViewer"/>
        /// should be processed by this panel; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool ProcessImageViewerKeyboardInput
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Navigates to the specified page, settings _performingProgrammaticZoom in the process to
        /// avoid handling scroll and zoom events that occur as a result.
        /// </summary>
        /// <param name="pageNumber">The page to be displayed</param>
        protected virtual void SetImageViewerPageNumber(int pageNumber)
        {
            try
            {
                if (ImageViewer != null && pageNumber != ImageViewer.PageNumber)
                {
                    _performingProgrammaticZoom = true;
                    _lastViewArea = new Rectangle();
                    ImageViewer.PageNumber = pageNumber;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29105", ex);
            }
            finally
            {
                _performingProgrammaticZoom = false;
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
        protected virtual void DrawHighlights(bool ensureActiveAttributeVisible)
        {
            // To avoid unnecessary drawing, wait until we are done loading a document or a
            // control is done with an update before attempting to display any layer objects.
            // Also, ensure against recursive calls.
            if (ImageViewer == null || _changingData || _drawingHighlights || ControlUpdateReferenceCount > 0)
            {
                return;
            }

            try
            {
                _drawingHighlights = true;

                // [DataEntry:1176]
                // If there was a pending selection to apply, do it now before the highlights are
                // drawn.
                if (_pendingSelection != null)
                {
                    // Specify to ApplySelection that is should suppress the draw highlights call
                    // and ItemSelectionChanged event since that event depends on data calculated in
                    // this method; ItemSelectionChanged will be raised at the end of this method.
                    ApplySelection(_pendingSelection, true);

                    // As long as the pending selection involves the active control, ensure active
                    // attributes are visible.
                    ensureActiveAttributeVisible =
                        (_pendingSelection.DataControl == _activeDataControl);
                }

                // Refresh the active control highlights if necessary.
                if (_refreshActiveControlHighlights)
                {
                    RefreshActiveControlHighlights();
                }

                // Update the viewed state of newly added attributes as appropriate.
                MarkEmptyAttributesAsViewed(_newlyAddedAttributes);
                _newlyAddedAttributes.Clear();

                // Will be populated with the set of attributes to be highlighted by the end of this
                // call.
                Dictionary<IAttribute, bool> newDisplayedAttributeHighlights =
                    new Dictionary<IAttribute, bool>();

                // Keep track of the unified bounds of the highlights and any associated tooltip on
                // each page.
                _selectionBounds.Clear();

                // Reset the selected attribute counts before iterating the attributes
                _selectedAttributesWithAcceptedHighlights = 0;
                _selectedAttributesWithUnacceptedHighlights = 0;
                _selectedAttributesWithDirectHints = 0;
                _selectedAttributesWithIndirectHints = 0;
                _selectedAttributesWithoutHighlights = 0;

                int firstPageOfHighlights = -1;
                int pageToShow = -1;

                // Obtain the current selection state as well as the list of highlights that need
                // tooltips.
                SelectionState selectionState = null;
                List<IAttribute> activeToolTipAttributes = null;
                if (_activeDataControl != null)
                {
                    _controlSelectionState.TryGetValue(_activeDataControl, out selectionState);
                    _controlToolTipAttributes.TryGetValue(
                        _activeDataControl, out activeToolTipAttributes);
                }

                // Loop through all active attributes to retrieve their highlights.
                foreach (IAttribute attribute in GetActiveAttributes())
                {
                    // Find any highlight CompositeHighlightLayerObject that has been created for
                    // this data entry control.
                    List<CompositeHighlightLayerObject> highlightList;

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
                        switch (AttributeStatusInfo.GetHintType(attribute))
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

                    // If the highlight is an in-direct hint, do not display it unless this is
                    // the only active attribute.
                    if (selectionState.Attributes.Count > 1 &&
                        AttributeStatusInfo.GetHintType(attribute) == HintType.Indirect)
                    {
                        continue;
                    }

                    // Flag each active attribute to be highlighted
                    newDisplayedAttributeHighlights[attribute] = true;

                    // If this attribute was previously highlighted, remove it from the
                    // _displayedAttributeHighlights collection whose contents will be hidden at
                    // the end of this call.
                    if (_displayedAttributeHighlights.ContainsKey(attribute))
                    {
                        _displayedAttributeHighlights.Remove(attribute);
                    }

                    // Display a tooltip if directed to by the control and if possible.
                    if (!_temporarilyHidingTooltips &&
                        AttributeStatusInfo.GetHintType(attribute) != HintType.Indirect &&
                        activeToolTipAttributes.Contains(attribute))
                    {
                        ShowAttributeToolTip(attribute);
                    }
                    // Otherwise, ensure any previous tooltip for the attribute is removed.
                    else
                    {
                        RemoveAttributeToolTip(attribute);

                        // Recreate the error icon so that it is no longer based on the tooltip
                        CreateAttributeErrorIcon(attribute, !_temporarilyHidingTooltips);
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
                        if (highlight.PageNumber == ImageViewer.PageNumber)
                        {
                            pageToShow = highlight.PageNumber;
                        }

                        // If there is not yet an entry for this page in unifiedBounds, create a
                        // new one.
                        if (!_selectionBounds.ContainsKey(highlight.PageNumber))
                        {
                            _selectionBounds[highlight.PageNumber] = highlight.GetBounds();
                        }
                        // Otherwise add to the existing entry for this page
                        else
                        {
                            _selectionBounds[highlight.PageNumber] = Rectangle.Union(
                                _selectionBounds[highlight.PageNumber], highlight.GetBounds());
                        }

                        // Combine the highlight bounds with the error icon bounds (if present).
                        ImageLayerObject errorIcon =
                            GetErrorIconOnPage(attribute, highlight.PageNumber);
                        if (errorIcon != null)
                        {
                            _selectionBounds[highlight.PageNumber] = Rectangle.Union(
                                _selectionBounds[highlight.PageNumber], errorIcon.GetBounds());
                        }
                    }
                }

                // [DataEntry:1192]
                // It is possible the _hoverAttribute has had its spatial info removed or has been
                // deleted since becoming the _hoverAttribute. If so, don't create a tooltip for it
                // and reset _hoverAttribute to null.
                if (_hoverAttribute != null &&
                    !AttributeStatusInfo.HasSpatialInfo(_hoverAttribute, true))
                {
                    _hoverAttribute = null;
                }

                // If there is a hover attribute that is different from the active attribute with
                // a tooltip displayed, display a tooltip for the hover attribute.
                if (_hoverAttribute != null &&
                        (_temporarilyHidingTooltips ||
                         activeToolTipAttributes == null ||
                         !activeToolTipAttributes.Contains(_hoverAttribute)))
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

                    if (!string.IsNullOrEmpty(_hoverAttribute.Value.String))
                    {
                        // The tooltip should also be displayed for the hover attribute, but
                        // don't position it along with the tooltips for currently selected
                        // attributes.
                        RemoveAttributeToolTip(_hoverAttribute);
                        _hoverToolTip = new DataEntryToolTip(this, _hoverAttribute, true, null);

                        ImageViewer.LayerObjects.Add(_hoverToolTip.TextLayerObject, false);
                        CreateAttributeErrorIcon(_hoverAttribute, true, _hoverToolTip);
                    }
                }

                // Hide all attribute highlights that were previously visible, but should not
                // visible anymore.
                foreach (IAttribute attribute in _displayedAttributeHighlights.Keys)
                {
                    if (_dataEntryApp.ShowAllHighlights)
                    {
                        // If ShowAllHighlights, only indirect hints need to be hidden.
                        if (AttributeStatusInfo.GetHintType(attribute) == HintType.Indirect)
                        {
                            ShowAttributeHighlights(attribute, false);
                        }

                        // But remove tooltips for all attributes not currently active
                        RemoveAttributeToolTip(attribute);

                        // Recreate the error icon so that it is no longer based on the tooltip
                        CreateAttributeErrorIcon(attribute, !_temporarilyHidingTooltips);
                    }
                    else
                    {
                        // Hide the highlights and remove any tooltip.
                        ShowAttributeHighlights(attribute, false);
                        RemoveAttributeToolTip(attribute);
                    }
                }

                // Move the visible error icons to the top of the z-order so that nothing is drawn
                // on top of them.
                foreach (List<ImageLayerObject> errorIcons in _attributeErrorIcons.Values)
                {
                    foreach (ImageLayerObject errorIcon in errorIcons)
                    {
                        if (errorIcon.Visible)
                        {
                            ImageViewer.LayerObjects.MoveToTop(errorIcon);
                        }
                    }
                }

                // Move to the appropriate page of the document.
                if (ensureActiveAttributeVisible &&
                    (pageToShow != -1 || firstPageOfHighlights != -1))
                {
                    if (pageToShow == -1)
                    {
                        pageToShow = firstPageOfHighlights;
                    }

                    SetImageViewerPageNumber(pageToShow);
                }

                // Create & position tooltips for the currently selected attribute(s).
                PositionToolTips();

                // Make sure the highlight is in view if ensureActiveAttributeVisible is specified.
                if (ensureActiveAttributeVisible && pageToShow != -1)
                {
                    EnforceAutoZoomSettings(false);
                }

                // Update _displayedHighlights with the new set of highlights.
                _displayedAttributeHighlights = newDisplayedAttributeHighlights;

                // If we applied a pending selection change, we need to raise the
                // ItemSelectionChanged event now.
                if (_pendingSelection != null)
                {
                    OnItemSelectionChanged();
                    _currentlySelectedGroupAttribute = _pendingSelection.SelectedGroupAttribute;
                    _pendingSelection = null;
                }

                ImageViewer.Invalidate();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27268", ex);
            }
            finally
            {
                _drawingHighlights = false;
            }
        }

        /// <summary>
        /// Removes all highlights that have been added to the <see cref="ImageViewer"/> in
        /// conjunction with the data currently being verified.
        /// </summary>
        protected void ClearHighlights()
        {
            List<IAttribute> tooltipAttributes = new List<IAttribute>(_attributeToolTips.Keys);
            foreach (IAttribute attribute in tooltipAttributes)
            {
                RemoveAttributeToolTip(attribute);
            }

            List<IAttribute> errorIconAttributes = new List<IAttribute>(_attributeErrorIcons.Keys);
            foreach (IAttribute attribute in errorIconAttributes)
            {
                RemoveAttributeErrorIcon(attribute);
            }

            foreach (CompositeHighlightLayerObject highlight in _highlightAttributes.Keys)
            {
                if (ImageViewer.LayerObjects.Contains(highlight))
                {
                    ImageViewer.LayerObjects.Remove(highlight, true, false);
                }
                else
                {
                    highlight.Dispose();
                }
            }

            if (_hoverToolTip != null)
            {
                _hoverToolTip.Dispose();
                _hoverToolTip = null;
            }

            if (_userNotificationTooltip != null)
            {
                _userNotificationTooltip.Dispose();
                _userNotificationTooltip = null;
            }

            _highlightAttributes.Clear();
            _displayedAttributeHighlights.Clear();
            _attributeHighlights.Clear();
            _hoverAttribute = null;
        }

        /// <summary>
        /// Using the values of InvalidDataSaveMode and UnviewedDataSaveMode, determines if data
        /// can be saved (prompting as necessary).  If data cannot be saved, an appropriate message
        /// or exception will be displayed and selection will be changed to the first field that
        /// prevented data from being saved.
        /// </summary>
        /// <returns><see langword="true"/> if data can be saved, <see langword="false"/> if the
        /// data cannot be saved at this time.</returns>
        protected virtual bool DataCanBeSaved()
        {
            // Keep track of the currently selected attribute and whether selection is changed by
            // this method so that the selection can be restored at the end of this method.
            Stack<IAttribute> currentlySelectedAttribute = ActiveAttributeGenealogy(true, null);
            if (currentlySelectedAttribute == null || !currentlySelectedAttribute.Any())
            {
                // https://extract.atlassian.net/browse/ISSUE-12548
                // In the case that nothing is selected, navigate to the first invalid attribute as
                // the starting point for validation.
                currentlySelectedAttribute =
                    AttributeStatusInfo.FindNextAttributeByValidity(_attributes,
                        DataValidity.Invalid | DataValidity.ValidationWarning, null, true, false);

                // If there are no invalid attributes, we can return early from this call.
                if (currentlySelectedAttribute == null || !currentlySelectedAttribute.Any())
                {
                    return true;
                }
            }

            bool changedSelection = false;
            bool hasError = HasInvalidAttribute(false);

            // Don't prompt or display exception about validation warnings if we are allowing them
            // or if prompting for each warning but there are truly invalid attributes.
            bool ignoreWarnings =
                   InvalidDataSaveMode == InvalidDataSaveMode.AllowWithWarnings
                || InvalidDataSaveMode == InvalidDataSaveMode.PromptForEachWarning && hasError;

            // If saving could be prevented by invalid data, iterate through the invalid attributes
            // in order to display the appropriate exception or prompt user for confirmation.
            if (   InvalidDataSaveMode == InvalidDataSaveMode.Disallow
                || InvalidDataSaveMode == InvalidDataSaveMode.AllowWithWarnings && hasError
                || InvalidDataSaveMode == InvalidDataSaveMode.PromptForEach
                || InvalidDataSaveMode == InvalidDataSaveMode.PromptForEachWarning
               )

            {
                // Find the first attribute that doesn't pass validation.
                IAttribute firstInvalidAttribute = null;
                IAttribute currentAttribute = currentlySelectedAttribute.Last();
                DataValidity validity = AttributeStatusInfo.GetDataValidity(currentAttribute);
                if (   validity == DataValidity.Invalid
                    || validity == DataValidity.ValidationWarning && !ignoreWarnings)
                {
                    firstInvalidAttribute = currentAttribute;
                }
                else
                {
                    firstInvalidAttribute = GetNextInvalidAttribute(!ignoreWarnings, loop: true, enabledOnly: false);

                    // If GetNextInvalidAttribute found something, the selection has been changed.
                    changedSelection = firstInvalidAttribute != null;
                }

                IAttribute invalidAttribute = firstInvalidAttribute;

                // Loop as long as more invalid attributes are found (for the case that we need to
                // prompt for each invalid field).
                while (invalidAttribute != null)
                {
                    try
                    {
                        using (new TemporaryWaitCursor())
                        {
                            // Obtain the complete genealogy needed to propagate the invalid attribute.
                            Stack<IAttribute> attributesToPropagate =
                                GetAttributeGenealogy(invalidAttribute);

                            // Indicate a manual focus event so that HandleControlGotFocus allows the
                            // new attribute selection rather than overriding it.
                            _manualFocusEvent = true;

                            // Propagate and select the invalid attribute.
                            PropagateAttributes(attributesToPropagate, true, false);

                            // Since this loop is being run within an event handler, DoEvents needs
                            // to be called to allow the attribute propagation to occur.
                            Application.DoEvents();
                        }

                        // Generate an exception which can be displayed to the user.
                        AttributeStatusInfo.Validate(invalidAttribute, true);
                    }
                    catch (DataEntryValidationException validationException)
                    {
                        // If saving is allowed after prompting, prompt for each attribute that
                        // currently does not meet validation requirements.
                        if (   InvalidDataSaveMode == InvalidDataSaveMode.PromptForEach
                            || InvalidDataSaveMode == InvalidDataSaveMode.PromptForEachWarning && !hasError
                           )
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
                            // If the user chooses to continue with the save, look for any further
                            // invalid attributes before returning true.
                            else
                            {
                                invalidAttribute = GetNextInvalidAttribute(!ignoreWarnings, loop: true, enabledOnly: false);

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
            if (UnviewedDataSaveMode != UnviewedDataSaveMode.Allow)
            {
                if (GetNextUnviewedAttribute(tabStopsOnly: false) != null)
                {
                    // If GetNextUnviewedAttribute found something, the selection has been changed.
                    changedSelection = true;

                    // If saving should be allowed after a prompting, prompt.
                    if (UnviewedDataSaveMode == UnviewedDataSaveMode.PromptOnceForAll)
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
                    dataControl.PropagateAttribute(null, false, false);
                }

                // TODO: If multiple cells in a table were selected, only one will be selected at the
                // end of this call.
                PropagateAttributes(currentlySelectedAttribute, true, false);
            }

            return true;
        }

        /// <summary>
        /// Indicates document data has changed.
        /// </summary>
        protected virtual void OnDataChanged()
        {
            // https://extract.atlassian.net/browse/ISSUE-14269
            // The DataEntryConfigurationManager (and perhaps other classes as well) may modify data
            // independently before the data is loaded into the DEP. Only mark as dirty if a
            // document is currently loaded.
            if (_isDocumentLoaded)
            {
                _dirty = true;
            }
        }

        /// <summary>
        /// Notify registered listeners that a control has been activated or manipulated in such a 
        /// way that swiping should be either enabled or disabled.
        /// </summary>
        /// <param name="e">A <see cref="SwipingStateChangedEventArgs"/> describing whether or not
        /// swiping is to be enabled.</param>
        protected virtual void OnSwipingStateChanged(SwipingStateChangedEventArgs e)
        {
            if (e.SwipingEnabled && ImageViewer != null && ImageViewer.IsImageAvailable &&
                ImageViewer.CursorTool == CursorTool.None)
            {
                // If swiping is being re-enabled and the previous active cursor tool was one of
                // the highlight tools, re-enable it so a user does not need to manually
                // re-active the highlight tool after tabbing past controls which don't support
                // swiping.

                if (_lastCursorTool == CursorTool.AngularHighlight)
                {
                    ImageViewer.CursorTool = CursorTool.AngularHighlight;
                }
                else if (_lastCursorTool == CursorTool.RectangularHighlight)
                {
                    ImageViewer.CursorTool = CursorTool.RectangularHighlight;
                }
                else if (_lastCursorTool == CursorTool.WordHighlight)
                {
                    ImageViewer.CursorTool = CursorTool.WordHighlight;
                }
            }

            // Raise the event for registered listeners
            if (SwipingStateChanged != null)
            {
                SwipingStateChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ItemSelectionChanged"/> event.
        /// </summary>
        protected virtual void OnItemSelectionChanged()
        {
            if (!_changingData && AttributeStatusInfo.IsLoggingEnabled(LogCategories.Focus))
            {
                // While this is a good place to guarantee all focus changes are logged, it often
                // results in multiple calls relating to the same focus change. Prevent logging the
                // same focus change multiple times.
                IAttribute focusedAttribute = GetActiveAttribute(null);
                Control focusedControl = _activeDataControl as Control;

                if (focusedAttribute != _lastLoggedFocusAttribute ||
                    focusedControl != _lastLoggedFocusControl)
                {
                    AttributeStatusInfo.Logger.LogEvent(LogCategories.Focus, focusedAttribute,
                        focusedControl, "");

                    _lastLoggedFocusAttribute = focusedAttribute;
                    _lastLoggedFocusControl = focusedControl;
                }
            }

            if (ItemSelectionChanged != null)
            {
                ItemSelectionChanged(this, new ItemSelectionChangedEventArgs(
                    _selectedAttributesWithAcceptedHighlights,
                    _selectedAttributesWithUnacceptedHighlights,
                    _selectedAttributesWithDirectHints, _selectedAttributesWithIndirectHints,
                    _selectedAttributesWithoutHighlights));
            }
        }

        /// <summary>
        /// Raises the <see cref="UnviewedDataStateChanged"/> event.
        /// </summary>
        protected virtual void OnUnviewedDataStateChanged()
        {
            UnviewedDataStateChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the <see cref="DataValidityChanged"/> event.
        /// </summary>
        protected virtual void OnDataValidityChanged()
        {
            DataValidityChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the <see cref="DataSaving"/> event.
        /// </summary>
        /// <param name="attributes">The attribute vector to be saved to disk.</param>
        /// <param name="forCommit"><c>true</c> if the data is being saved as part of a commit; 
        /// otherwise, <c>false</c>.</param>
        protected virtual void OnDataSaving(IUnknownVector attributes, bool forCommit)
        {
            DataSaving?.Invoke(this, new AttributesEventArgs(attributes));
        }

        /// <summary>
        /// Called when a document has finished loading.
        /// </summary>
        protected virtual void OnDocumentLoaded()
        {
            DocumentLoaded?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the <see cref="MessageHandled"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        void OnMessageHandled(MessageHandledEventArgs e)
        {
            try
            {
                if (MessageHandled != null)
                {
                    MessageHandled(this, e);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI29134", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="UpdateEnded"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnUpdateEnded(EventArgs e)
        {
            try
            {
                if (UpdateEnded != null)
                {
                    UpdateEnded(this, e);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI30114", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="DataSaved"/> event.
        /// </summary>
        protected virtual void OnDataSaved()
        {
            try
            {
                if (DataSaved != null)
                {
                    DataSaved(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI38427", ex);
            }
        }

        #endregion Protected Members

        #region Private Members

        /// <summary>
        /// Gets a value indicating whether the host in the midst of an undo operation.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if in an undo operation; otherwise, <see langword="false"/>.
        /// </value>
        bool InUndo
        {
            get
            {
                return (_inUndo || AttributeStatusInfo.UndoManager.InUndoOperation);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the host in the midst of an redo operation.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if in an redo operation; otherwise, <see langword="false"/>.
        /// </value>
        bool InRedo
        {
            get
            {
                return (_inRedo || AttributeStatusInfo.UndoManager.InRedoOperation);
            }
        }

        /// <summary>
        /// Indicates the number of updates controls have indicated are in progress. DrawHighlights
        /// and other general processing that can be delayed should be until there are no more
        /// updates in progress.
        /// </summary>
        protected uint ControlUpdateReferenceCount
        {
            get
            {
                return _controlUpdateReferenceCount;
            }

            set
            {
                try
                {
                    if (_controlUpdateReferenceCount != value)
                    {
                        // Within a control update, don't allow changes to be grouped into separate
                        // operations. Everything up until _controlUpdateReferenceCount == 0 should
                        // be considered a single operation.
                        if (_controlUpdateReferenceCount == 0 && value > 0)
                        {
                            AttributeStatusInfo.UndoManager.OperationInProgress = true;

                            // https://extract.atlassian.net/browse/ISSUE-12453
                            // While a controls are in the midst of updating and attributes are not
                            // completely initialized/deleted, evaluation of queries can produce
                            // unexpected results. Delay execution of these queries until the
                            // update is complete.
                            AttributeStatusInfo.PauseQueries = true;
                        }

                        _controlUpdateReferenceCount = value;

                        if (_controlUpdateReferenceCount == 0)
                        {
                            // Allow any queries that would have triggered during this update to
                            // execute now.
                            AttributeStatusInfo.PauseQueries = false;

                            OnUpdateEnded(new EventArgs());

                            // [DataEntry:1027]
                            // In order ensure that all message processing that happens a result of
                            // the initial user input, don't end the current operation until the
                            // host is once again idle and therefore occurs after any other message
                            // invokes triggers by the initial operation.
                            ExecuteOnIdle("ELI34418", () =>
                                AttributeStatusInfo.UndoManager.OperationInProgress = false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30113", ex);
                }
            }
        }

        /// <summary>
        /// Loops through all controls contained in the specified control looking for controls 
        /// that implement the <see cref="IDataEntryControl"/> interface.  Registers events 
        /// necessary to facilitate the flow of information between the controls.
        /// <para><b>Requirements</b></para>
        /// Must be called before the ImageViewer loads a document.
        /// </summary>
        /// <param name="parentControl">The control for which <see cref="IDataEntryControl"/>s
        /// should be registered.</param>
        void RegisterDataEntryControls(Control parentControl)
        {
            // Loop recursively through all contained controls looking for controls that implement
            // the IDataEntryControl interface
            foreach (Control control in parentControl.Controls)
            {
                // Register each child control.
                RegisterDataEntryControls(control);

                // [DataEntry:1139]
                // Set the font of each control whether or not it is a data entry control to ensure
                // the fonts are consistent.
                control.Font = new Font(Config.Settings.FontFamily,
                    (Config.Settings.ControlFontSize > 0)
                        ? Config.Settings.ControlFontSize
                        : base.Font.Size);

                // Check to see if this control is an IDataEntryControl itself.
                IDataEntryControl dataControl = control as IDataEntryControl;
                if (dataControl == null)
                {
                    _nonDataControls.Add(control);
                }
                else
                {
                    dataControl.DataEntryControlHost = this;

                    // Register for needed events from data entry controls
                    control.GotFocus += HandleControlGotFocus;
                    dataControl.SwipingStateChanged += HandleSwipingStateChanged;
                    dataControl.AttributesSelected += HandleAttributesSelected;
                    dataControl.UpdateStarted += HandleControlUpdateStarted;
                    dataControl.UpdateEnded += HandleControlUpdateEnded;

                    // Assign the error provider for the control (if required).
                    IRequiresErrorProvider errorControl = control as IRequiresErrorProvider;
                    if (errorControl != null)
                    {
                        errorControl.SetErrorProviders(
                            _validationErrorProvider, _validationWarningErrorProvider);
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

                    if (_disabledValidationControls.Contains(control.Name))
                    {
                        dataControl.ValidationEnabled = false;
                    }
                        
                    ControlRegistered?.Invoke(this, new DataEntryControlEventArgs(dataControl));
                }
            }
        }

        /// <summary>
        /// Unregisters the <see cref="DataEntryControlHost"/> from <see cref="IDataEntryControl"/>
        /// events.
        /// </summary>
        void UnregisterDataEntryControls()
        {
            try
            {
                foreach (IDataEntryControl dataControl in _dataControls)
                {
                    ControlUnregistered?.Invoke(this, new DataEntryControlEventArgs(dataControl));

                    ((Control)dataControl).GotFocus -= HandleControlGotFocus;
                    dataControl.SwipingStateChanged -= HandleSwipingStateChanged;
                    dataControl.AttributesSelected -= HandleAttributesSelected;
                    dataControl.UpdateStarted -= HandleControlUpdateStarted;
                    dataControl.UpdateEnded -= HandleControlUpdateEnded;
                    dataControl.DataEntryControlHost = null;
                }
            }
            catch (Exception ex)
            {
                // This is called from Dispose, so don't throw an exception.
                ex.ExtractLog("ELI41597");
            }
        }

        /// <summary>
        /// Re-register the <see cref="DataEntryControlHost"/> from <see cref="IDataEntryControl"/>
        /// events.
        /// </summary>
        void ReRegisterDataEntryControls()
        {
            foreach (IDataEntryControl dataControl in _dataControls)
            {
                ((Control)dataControl).GotFocus += HandleControlGotFocus;
                dataControl.SwipingStateChanged += HandleSwipingStateChanged;
                dataControl.AttributesSelected += HandleAttributesSelected;
                dataControl.UpdateStarted += HandleControlUpdateStarted;
                dataControl.UpdateEnded += HandleControlUpdateEnded;
                dataControl.DataEntryControlHost = this;

                ControlRegistered?.Invoke(this, new DataEntryControlEventArgs(dataControl));
            }
        }

        /// <summary>
        /// Attempts to create or update the existing <see cref="SmartTagManager"/> using the smart
        /// tags defined in the current database connection.
        /// </summary>
        void InitializeSmartTagManager()
        {
            try
            {
                // DataEntry SmartTags require a database connection.
                if (_defaultDbConnection == null)
                {
                    if (_smartTagManager != null)
                    {
                        _smartTagManager.Dispose();
                        _smartTagManager = null;
                    }

                    return;
                }

                // DataEntry SmartTags require a 'SmartTag' table.
                if (!DBMethods.GetTableNames(_defaultDbConnection).Any(name => name == "SmartTag"))
                {
                    if (_smartTagManager != null)
                    {
                        _smartTagManager.Dispose();
                        _smartTagManager = null;
                    }

                    return;
                }

                // Retrieve the smart tags...
                var queryResults = DBMethods.GetQueryResultsAsStringArray(_defaultDbConnection,
                    "SELECT TagName, TagValue FROM [SmartTag]");

                // And put them into a dictionary that the SmartTagManager can use
                Dictionary<string, string> smartTags = new Dictionary<string, string>();
                foreach (string smartTag in queryResults)
                {
                    string[] columns =
                        smartTag.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (columns.Length == 2)
                    {
                        smartTags[columns[0]] = columns[1];
                    }
                }

                // Create or update the SmartTagManager
                if (_smartTagManager == null)
                {
                    _smartTagManager = new SmartTagManager(this, smartTags);
                    _smartTagManager.ApplyingValue += HandleSmartTagApplyingValue;
                }
                else
                {
                    _smartTagManager.UpdateSmartTags(smartTags);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28902", ex);
            }
        }

        /// <summary>
        /// Advances field selection to the next or previous tab stop. Selection will "wrap around"
        /// appropriately after reaching the last tab stop (or first when navigating backwards).
        /// </summary>
        /// <param name="forward"><see langword="true"/> to go to the next tab stop or
        /// <see langword="false"/> to go to the previous tab stop.</param>
        /// <param name="viaTabKey"><c>true</c> if the navigation was initiated via the tab key.</param>
        /// <returns><c>true</c> if the result of the call is that a field in the DEP has received
        /// focus; <c>false</c> if focus could not be applied or was handled externally via
        /// <see cref="TabNavigation"/> event.</returns>
        bool AdvanceToNextTabStop(bool forward, bool viaTabKey)
        {
            // Notify AttributeStatusInfo that the current edit is over.
            // This will also be called as part of a focus change event, but it needs
            // to be done here first so that any auto-updating that needs to occur
            // occurs prior to finding the next tab stop since the next tab stop may
            // depend on an auto-update.
            AttributeStatusInfo.EndEdit();

            // If there are _pendingNavigateOutEventArgs, once tab navigation is used again, execute
            // the pending TabNavigationEventArgs call or reset it.
            if (viaTabKey && ApplyPendingTabNavigation(forward))
            {
                return false;
            }

            // Tab navigation should loop if there is no handler to process tabbing out of DEP.
            bool tabLoop = TabNavigation == null;
            IAttribute originalActiveAttribute = GetActiveAttribute(!forward);
            IAttribute activeAttribute = originalActiveAttribute;
            Stack<IAttribute> activeGenealogy = ActiveAttributeGenealogy(!forward,
                _dataEntryApp.AllowTabbingByGroup ? _currentlySelectedGroupAttribute : null);
            bool repeat = false;

            // https://extract.atlassian.net/browse/ISSUE-13005
            // Per comment below, this loop to select the next attribute may need to be repeated to
            // find one that is currently visible and enabled.
            do
            {
                repeat = false;

                // Find the next attribute genealogy in the tab order and whether it should be
                // selected individually or as a group (row).
                bool tabByGroup;
                Stack<IAttribute> nextTabStopGenealogy = GetNextTabStopGenealogy(
                    activeAttribute, activeGenealogy, forward, tabLoop, out tabByGroup);

                // Allow registered TabNavigation listener a chance to handle the navigation first.
                if (viaTabKey)
                {
                    bool lastStop = nextTabStopGenealogy == null || (!forward && originalActiveAttribute == null);
                    if (OnTabNavigation(forward, lastStop))
                    {
                        return false;
                    }
                }

                if (nextTabStopGenealogy != null)
                {
                    // Indicate a manual focus event so that HandleControlGotFocus allows the
                    // new attribute selection rather than overriding it.
                    _manualFocusEvent = true;

                    activeGenealogy = new Stack<IAttribute>(nextTabStopGenealogy.Reverse());
                    activeAttribute = PropagateAttributes(nextTabStopGenealogy, true, tabByGroup);

                    // https://extract.atlassian.net/browse/ISSUE-13005
                    // To support programmatic changes of controls, we need to expect that the
                    // attribute in question may be in a control that is not currently visible or
                    // enabled. In this case, we need to continue on until we get to the next
                    // attribute from a visible control.
                    // NOTE: Because visibility of a control could change via propagation, an
                    // attempt needs to be made to propagate an attribute before checking visibility
                    // rather than making the check for visibility part of GetNextTabStopGenealogy.
                    var owningControl =
                        AttributeStatusInfo.GetOwningControl(activeAttribute) as Control;
                    if (owningControl != null && (!owningControl.Visible || !owningControl.Enabled))
                    {
                        ExtractException.Assert("ELI38240", "Failed to advance selection.",
                            activeAttribute != originalActiveAttribute);

                        // Prevent infinite loop in the case that there are no active/enabled controls
                        // https://extract.atlassian.net/browse/ISSUE-14265
                        if (originalActiveAttribute == null)
                        {
                            originalActiveAttribute = activeAttribute;
                        }

                        repeat = true;
                    }
                }
            }
            while (repeat);

            return (activeAttribute != null);
        }

        /// <summary>
        /// If there is any _pendingTabNavigationEventArgs, raise the event.
        /// </summary>
        /// <returns><c>true</c> if the event was handled by a registered event listener;
        /// <c>false</c> if there was not a pending event, a listener, or the event was not
        /// handled.</returns>
        bool ApplyPendingTabNavigation(bool forward)
        {
            if (_pendingTabNavigationEventArgs != null)
            {
                var eventArgs = new TabNavigationEventArgs(_pendingTabNavigationEventArgs);
                _pendingTabNavigationEventArgs = null;
                if (_isDocumentLoaded && (forward == eventArgs?.Forward))
                {
                    TabNavigation?.Invoke(this, eventArgs);
                    if (eventArgs.Handled)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Raises the <see cref="TabNavigation"/> event
        /// </summary>
        /// <returns><c>true</c> if the event was processed by an event listener; otherwise, <c>false</c>.</returns>
        bool OnTabNavigation(bool forward, bool lastStop)
        {
            if (_isDocumentLoaded && TabNavigation != null)
            {
                var eventArgs = new TabNavigationEventArgs(forward, lastStop);
                TabNavigation.Invoke(this, eventArgs);

                if (eventArgs.Handled)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds the next attribute genealogy after <see paramref="activeAttribute"/> and
        /// <see paramref="activeGenealogy"/> in the tab order (<see paramref="forward"/> or
        /// backward) and whether it should be selected individually or as a group (row).
        /// </summary>
        /// <param name="startingPoint">The attribute that best represents the starting point of the
        /// search in the specified search direction.</param>
        /// <param name="activeGenealogy">The attribute genealogy that best represents the starting
        /// point of the search in the specified search direction.</param>
        /// <param name="forward"><see langword="true"/> if the next tab stop should be found,
        /// <see langword="false"/> if the previous should be found.</param>
        /// <param name="tabByGroup"><see langword="true"/> if the resulting genealogy should be
        /// selected as a group, otherwise, <see langword="false"/>.</param>
        /// <returns>A Stack of <see cref="IAttribute"/>s describing the next tab stop in the
        /// specified direction.
        /// </returns>
        Stack<IAttribute> GetNextTabStopGenealogy(IAttribute startingPoint,
            Stack<IAttribute> activeGenealogy, bool forward, bool loop, out bool tabByGroup)
        {
            Stack<IAttribute> nextTabStopGenealogy = null;
            tabByGroup = _dataEntryApp.AllowTabbingByGroup;
            nextTabStopGenealogy = null;

            if (tabByGroup)
            {
                // If _dataEntryApp.AllowTabbingByGroup and a group is currently selected,
                // use GetNextTabGroupAttribute to search for the next attribute
                // group (rather than tab stop attribute) in controls that support
                // attribute groups. (tab stops will be found in controls that
                // don't).
                if (_currentlySelectedGroupAttribute != null)
                {
                    nextTabStopGenealogy =
                        AttributeStatusInfo.GetNextTabGroupAttribute(_attributes,
                            activeGenealogy, forward, loop);
                }
                // If no attribute group is currently selected, use
                // GetNextTabStopOrGroupAttribute to find either the next tab
                // stop attribute or the next attribute group within the same
                // control. If nothing is found within the current control, 
                // the next attribute group will be found in controls that support
                // attribute groups (tab stops will be found in controls that don't).
                else
                {
                    // If the active attribute is already a tab group attribute but the tab group
                    // is not currently selected and the previous navigation was via tab in the same
                    // direction as the current navigation, select the tab group without advancing
                    // the active attribute.
                    if (startingPoint != null && _lastNavigationViaTabKey != null &&
                        _lastNavigationViaTabKey.Value == forward)
                    {
                        List<IAttribute> tabGroup =
                            AttributeStatusInfo.GetAttributeTabGroup(startingPoint);
                        if (tabGroup != null && tabGroup.Count > 0)
                        {
                            nextTabStopGenealogy = GetAttributeGenealogy(startingPoint);
                        }
                    }

                    // Otherwise, advance the current selection to the next tab stop or group.
                    if (nextTabStopGenealogy == null)
                    {
                        nextTabStopGenealogy =
                            AttributeStatusInfo.GetNextTabStopOrGroupAttribute(_attributes,
                                activeGenealogy, forward, loop);

                        // [DataEntry:754]
                        // If the next tab stop attribute represents an attribute group and that
                        // group contains the currently active attribute and is a tab stop on its
                        // own, don't select the group, rather first select the attribute
                        // independently (set selectGroup = false).
                        if (startingPoint != null && nextTabStopGenealogy != null && nextTabStopGenealogy.Count > 0)
                        {
                            IAttribute nextTabStopAttribute = nextTabStopGenealogy.Last();

                            if (AttributeStatusInfo.GetAttributeTabGroup(nextTabStopAttribute) == null)
                            {
                                // [DataEntry:840]
                                // If nextTabStopAttribute is null, the control owning it does not
                                // support tabbing by group. Don't select by group.
                                tabByGroup = false;
                            }
                            else if (AttributeStatusInfo.IsAttributeTabStop(nextTabStopAttribute))
                            {
                                List<IAttribute> tabGroup =
                                    AttributeStatusInfo.GetAttributeTabGroup(nextTabStopAttribute);

                                if (tabGroup != null && tabGroup.Contains(startingPoint))
                                {
                                    tabByGroup = false;
                                }
                            }
                        }
                    }
                }
            }
            // Tabbing by row is not supported-- find the next tab stop attribute.
            else
            {
                nextTabStopGenealogy =
                    AttributeStatusInfo.GetNextTabStopAttribute(_attributes,
                        activeGenealogy, forward, loop);
            }

            return nextTabStopGenealogy;
        }

        /// <summary>
        /// Sends <see paramref="ocrText"/> to active control.
        /// </summary>
        /// <param name="ocrText">The ocr text to send to the active control.</param>
        void SendSpatialStringToActiveControl(SpatialString ocrText)
        {
            if (_activeDataControl == null)
            {
                // If there is no active control, there is nothing to do.
                return;
            }

            try
            {
                // Delay calls to DrawHighlights until processing of the swipe is
                // complete.
                ControlUpdateReferenceCount++;

                if (AttributeStatusInfo.IsLoggingEnabled(LogCategories.SwipedText))
                {
                    AttributeStatusInfo.Logger.LogEvent(LogCategories.SwipedText,
                        GetActiveAttribute(false), ocrText.String);
                }

                using (new TemporaryWaitCursor())
                {
                    // If a swipe did not produce any results usable by the control,
                    // notify the user.
                    if (!_activeDataControl.ProcessSwipedText(ocrText))
                    {
                        ShowUserNotificationTooltip("Unable to format swiped text " +
                            "into the current selection.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI27090", ex);

                // Notify the user of errors the control encountered processing the 
                // swiped text.
                ShowUserNotificationTooltip("An error was encountered while " +
                    "formatting the swiped text into the current selection.");
                return;
            }
            finally
            {
                ControlUpdateReferenceCount--;
            }

            // [DataEntry:269] Swipes should trigger document to be marked as dirty.
            // Ensure the change
            // https://extract.atlassian.net/browse/ISSUE-15472
            // Make sure this is called after the swipe has been processed, otherwise affected
            // entities that use data queries in response to changed data may not have the
            // swipe result available.
            OnDataChanged();

            try
            {
                // Notify AttributeStatusInfo that the current edit is over so that
                // a non-incremental value modified event can be raised.
                AttributeStatusInfo.EndEdit();
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI27091", ex);

                // Notify the user of errors encountered while applying the change
                // (likely involves auto-update queries).
                ShowUserNotificationTooltip("An error was encountered while " +
                    "applying the swiped text to the current selection.");
            }

            // It is likely that the spatial information of the selected attributes
            // changed as a result of the swipe; update the active attribute(s)'
            // associated highlights.
            _refreshActiveControlHighlights = true;

            DrawHighlights(true);
        }

        /// <summary>
        /// Marks any of the specified <see cref="IAttribute"/>s with blank values as viewed.
        /// </summary>
        /// <param name="attributes">The attributes whose viewed status should be set according to
        /// the absence of a value.</param>
        static void MarkEmptyAttributesAsViewed(IEnumerable<IAttribute> attributes)
        {
            foreach (IAttribute attribute in attributes)
            {
                Dictionary<IDataEntryControl, List<IAttribute>> attributesMarkedAsViewed =
                    new Dictionary<IDataEntryControl, List<IAttribute>>();

                if (!AttributeStatusInfo.HasBeenViewedOrIsNotViewable(attribute, false) &&
                    string.IsNullOrEmpty(attribute.Value.String))
                {
                    AttributeStatusInfo.MarkAsViewed(attribute, true);
                    IDataEntryControl owningControl = AttributeStatusInfo.GetOwningControl(attribute);

                    if (!attributesMarkedAsViewed.ContainsKey(owningControl))
                    {
                        attributesMarkedAsViewed[owningControl] = new List<IAttribute>();
                    }

                    attributesMarkedAsViewed[owningControl].Add(attribute);
                }

                // Refresh any attributes whose viewed status has been changed so that their
                // appearance reflects the new viewed status.
                foreach (KeyValuePair<IDataEntryControl, List<IAttribute>> updatedAttributes in 
                    attributesMarkedAsViewed)
                {
                    updatedAttributes.Key.RefreshAttributes(
                        false, updatedAttributes.Value.ToArray());
                }
            }
        }

        /// <summary>
        /// Adjusts the view and zoom to best display the selected region(s) on the current page
        /// according to the specified auto-zoom settings.
        /// </summary>
        /// <param name="forceZoomToSelection"><see langword="true"/> to force a zoom to selection
        /// regardless of the current AutoZoomMode; <see langword="false"/> to zoom according to the
        /// current AutoZoomMode.</param>
        void EnforceAutoZoomSettings(bool forceZoomToSelection)
        {
            try
            {
                if (_selectionBounds.Count == 0 ||
                    !_selectionBounds.Keys.Contains(ImageViewer.PageNumber))
                {
                    // Nothing to do.
                    return;
                }

                // [DataEntry:1275]
                // Ensure the auto-zoom takes into account the selected attribute's tooltip.
                foreach (KeyValuePair<IAttribute, DataEntryToolTip> attributeTooltip in
                        _attributeToolTips)
                {
                    _selectionBounds[ImageViewer.PageNumber] = Rectangle.Union(
                        _selectionBounds[ImageViewer.PageNumber],
                        attributeTooltip.Value.TextLayerObject.GetBounds());
                }

                // Initialize the newViewRegion as the selected object region.
                Rectangle selectedImageRegion = _selectionBounds[ImageViewer.PageNumber];
                Rectangle newViewRegion = selectedImageRegion;

                // [DataEntry:532] Ensure we're not trying to zoom to a region that extends offpage.
                newViewRegion.Intersect(
                    new Rectangle(0, 0, ImageViewer.ImageWidth, ImageViewer.ImageHeight));

                // Determine the current view region.
                Rectangle currentViewRegion = ImageViewer.GetTransformedRectangle(
                        ImageViewer.GetVisibleImageArea(), true);

                // If we are trying to enforce auto-zoom on the same selection as last time and the
                // current selection is in view, there is nothing to do.
                if (!forceZoomToSelection && _lastViewArea == selectedImageRegion &&
                    currentViewRegion.Contains(newViewRegion))
                {
                    return;
                }

                _performingProgrammaticZoom = true;

                // The amount of padding to be added in each direction (always add at least 3 pixels
                // so tooltip borders do not extend offscreen.
                int xPadAmount = 3;
                int yPadAmount = 3;

                AutoZoomMode autoZoomMode = forceZoomToSelection
                    ? AutoZoomMode.AutoZoom
                    : _dataEntryApp.AutoZoomMode;

                if (autoZoomMode == AutoZoomMode.NoZoom)
                {
                    // If the selected object is already completely visible, there is nothing to do.
                    if (currentViewRegion.Contains(newViewRegion))
                    {
                        return;
                    }

                    // Determine x-axis offset to get from the center of the current view to the
                    // center of the new view.
                    int xOffset = (newViewRegion.Left - currentViewRegion.Left) +
                                  (newViewRegion.Right - currentViewRegion.Right);
                    xOffset /= 2;

                    // Determine y-axis offset to get from the center of the current view to the
                    // center of the new view.
                    int yOffset = (newViewRegion.Top - currentViewRegion.Top) +
                                  (newViewRegion.Bottom - currentViewRegion.Bottom);
                    yOffset /= 2;

                    // Shift the current view rectangle by the calculated offsets.  This will be the
                    // new view whether or not the selected object is completely contained or not.
                    newViewRegion = currentViewRegion;
                    newViewRegion.Offset(xOffset, yOffset);
                }
                else if (autoZoomMode == AutoZoomMode.ZoomOutIfNecessary)
                {
                    if (!_lastManualZoomSize.IsEmpty)
                    {
                        // Determine the amount the current view must be resized to get back to the last
                        // manual zoom level.
                        int widthAdjustment = _lastManualZoomSize.Width - currentViewRegion.Width;
                        widthAdjustment = widthAdjustment > 0 ? 0 : widthAdjustment;

                        int heightAdjustment = _lastManualZoomSize.Height - currentViewRegion.Height;
                        heightAdjustment = heightAdjustment > 0 ? 0 : heightAdjustment;

                        // If the current zoom is further out than the last manual zoom, set 
                        // selectedImageRegion as currentViewRegion "zoomed" back in to the last manual
                        // zoom level.
                        if (widthAdjustment < 0 || heightAdjustment < 0)
                        {
                            currentViewRegion.Inflate(widthAdjustment, heightAdjustment);
                        }
                    }

                    // If the selected object is completely visible in currentViewRegion use the
                    // currentViewRegion.
                    if (currentViewRegion.Contains(newViewRegion))
                    {
                        newViewRegion = currentViewRegion;

                        // Don't pad the currentViewRegion.
                        xPadAmount = 0;
                        yPadAmount = 0;
                    }
                    else
                    {
                        // If selectedImageRegion is not completely contained in the current view,
                        // adjust the view to include the extents of the selected object.
                        // If the user has used F2 to zoom in, don't jump back to _lastManualZoomSize
                        // when changing selection.
                        int totalWidth = xPadAmount * 2 + selectedImageRegion.Width;
                        if (!_manuallyZoomedToSelection && (totalWidth < _lastManualZoomSize.Width))
                        {
                            xPadAmount += (_lastManualZoomSize.Width - totalWidth) / 2;
                            totalWidth = xPadAmount * 2 + selectedImageRegion.Width;
                        }
                        // [DataEntry:1276]
                        // Ensure ZoomOutIfNecessary never zooms in.
                        if (totalWidth < currentViewRegion.Width)
                        {
                            xPadAmount += (currentViewRegion.Width - totalWidth) / 2;
                        }

                        int totalHeight = yPadAmount * 2 + selectedImageRegion.Width;
                        if (!_manuallyZoomedToSelection && (totalHeight < _lastManualZoomSize.Width))
                        {
                            yPadAmount += (_lastManualZoomSize.Width - totalHeight) / 2;
                            totalHeight = yPadAmount * 2 + selectedImageRegion.Width;
                        }
                        // [DataEntry:1276]
                        // Ensure ZoomOutIfNecessary never zooms in.
                        if (totalHeight < currentViewRegion.Height)
                        {
                            yPadAmount += (currentViewRegion.Height - totalHeight) / 2;
                        }
                    }
                }
                else // autoZoomMode == AutoZoomMode.AutoZoom
                {
                    // Determine the maximum amount of context space that can be added based on a
                    // percentage of the smaller dimension.
                    int smallerDimension = Math.Min(ImageViewer.ImageWidth,
                        ImageViewer.ImageHeight);
                    int maxPadAmount = (int)(smallerDimension * _AUTO_ZOOM_MAX_CONTEXT) / 2;

                    // If zooming to the current selection, translate the zoomContext percentage
                    // into a value that grows exponentially from 0 to 1 so that the more
                    // _dataEntryApp.AutoZoomContext approaches 1, the text pixels are being padded.
                    double padFactor = Math.Pow(_dataEntryApp.AutoZoomContext, 2);

                    // Calculate the pad amounts as a fraction of the maxPadAmount specified determined
                    // using padFactor.
                    xPadAmount += (int)(maxPadAmount * padFactor);
                    yPadAmount = xPadAmount;
                }

                if (newViewRegion != currentViewRegion || xPadAmount != 0 || yPadAmount != 0)
                {
                    // Update the zoomed-to-selection state variables.
                    _zoomedToSelection = (autoZoomMode == AutoZoomMode.AutoZoom);
                    _manuallyZoomedToSelection = forceZoomToSelection || (_zoomedToSelection && _manuallyZoomedToSelection);
                    if (_zoomedToSelection)
                    {
                        _zoomToSelectionPage = ImageViewer.PageNumber;
                    }

                    FitMode fitMode = ImageViewer.FitMode;

                    for (int i = 0; i < 3; i++)
                    {
                        // Apply the padding.
                        Rectangle transformedViewRegion = ImageViewer.PadViewingRectangle(
                            newViewRegion, xPadAmount, yPadAmount, true);

                        // Translate the image coordinates into client coordinates.
                        transformedViewRegion =
                            ImageViewer.GetTransformedRectangle(transformedViewRegion, false);

                        // Zoom to the specified rectangle.
                        ImageViewer.ZoomToRectangle(transformedViewRegion);

                        currentViewRegion = ImageViewer.GetTransformedRectangle(
                            ImageViewer.GetVisibleImageArea(), true);

                        // [DataEntry:1277]
                        // Test to ensure all of the selected image region is contained. Scrollbars
                        // that appeared as a result of the zoom or rounding issue may result in a
                        // bit of the selectedImageRegion to extend offscreen.
                        if (currentViewRegion.Contains(selectedImageRegion))
                        {
                            break;
                        }
                        else
                        {
                            // It appears that most if not all cases are resolve by simply trying
                            // the zoom again. But just in case, make a final attempt that inflates
                            // the zoom rectangle a bit.
                            newViewRegion.Inflate(i * 2, i * 2);
                        }
                    }

                    // [DataEntry:1096]
                    // Restore the previous fit mode if zoom operation cleared the fit mode
                    // (Unless AutoZoom is being used which should clear the fit mode).
                    if (fitMode != ImageViewer.FitMode && autoZoomMode != AutoZoomMode.AutoZoom)
                    {
                        ImageViewer.FitMode = fitMode;
                    }

                    // If zoom has to change very much to zoom on the specified rectangle, calling
                    // GetTransformedRectangle again after the first call will likely result in a
                    // slightly different rectangle. To prevent multiple calls, keep track of the last
                    // specified selectedImageRegion, and don't re-apply zoomed-to-selection settings
                    // after it is applied the first time.
                    _lastViewArea = selectedImageRegion;  
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27113", ex);
            }
            finally
            {
                _performingProgrammaticZoom = false;
            }
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
        /// <param name="selectTabGroup"><see langword="true"/> if an attribute group should be
        /// selected provided the specified attribute represents an attribute group,
        /// <see langword="false"/> otherwise.</param>
        /// <returns></returns>
        IAttribute PropagateAttributes(Stack<IAttribute> attributes, bool select,
            bool selectTabGroup)
        {
            if (attributes.Count == 0)
            {
                // Nothing to do.
                return null;
            }

            // It is up to the owning control to indicate via the AttributesSelected event whether
            // an attribute group is selected
            _currentlySelectedGroupAttribute = null;
            
            // Get the first attribute off the stack and obtain its owning control.
            IAttribute attribute = attributes.Pop();
            IDataEntryControl dataEntryControl = 
                AttributeStatusInfo.GetStatusInfo(attribute).OwningControl;

            // Initialize the "last" attribute and control.
            IAttribute lastAttribute = attribute;
            IDataEntryControl lastDataEntryControl = dataEntryControl;

            // Loop through each attribute in the chain, but only call PropagateAttribute for the
            // last (deepest) attribute mapped to a particular control.  This is to prevent an 
            // earlier (ancestor) attribute from being marked as viewed.
            while (attributes.Count > 0)
            {
                // Get the next attribute off the stack and obtain its owning control.
                attribute = attributes.Pop();
                dataEntryControl = AttributeStatusInfo.GetStatusInfo(attribute).OwningControl;

                // If the next attribute belongs to a different control, propagate the "last"
                // attribute in the last control.
                if (dataEntryControl != lastDataEntryControl)
                {
                    lastDataEntryControl.PropagateAttribute(lastAttribute, select, selectTabGroup);
                }

                // Update the "last" attribute and control.
                lastDataEntryControl = dataEntryControl;
                lastAttribute = attribute;
            }

            lastDataEntryControl.PropagateAttribute(lastAttribute, select, selectTabGroup);

            // Give focus to the target attribute control if selection is requested.
            if (select)
            {
                ((Control)lastDataEntryControl).Focus();

                // https://extract.atlassian.net/browse/ISSUE-18217
                // Select all text when focusing text box control
                if (lastDataEntryControl is TextBoxBase textBoxBase)
                {
                    textBoxBase.SelectAll();
                }
            }

            return lastAttribute;
        }

        /// <summary>
        /// Retrieves an <see cref="IAttribute"/> genealogy describing the currently active
        /// attribute. If more that one attribute is selected in the active control, the first 
        /// attribute (in display order) will be used.
        /// </summary>
        /// <param name="first"><see langword="true"/> if the first of the selected attributes
        /// should be returned, <see langword="false"/> if the last should.</param>
        /// <param name="includedAttribute">If not <see langword="null"/>, the specified attribute
        /// should be considered part of the active set.</param>
        /// <returns>A Stack of <see cref="IAttribute"/>s describing the currently active
        /// <see cref="IAttribute"/> or <see langword="null"/> if there is no active attribute.
        /// </returns>
        Stack<IAttribute> ActiveAttributeGenealogy(bool first, IAttribute includedAttribute)
        {
            if (_activeDataControl != null)
            {
                SelectionState selectionState;
                if (_controlSelectionState.TryGetValue(_activeDataControl, out selectionState))
                {
                    List<IAttribute> controlAttributes =
                        new List<IAttribute>(selectionState.Attributes);

                    if (includedAttribute != null)
                    {
                        controlAttributes.Add(includedAttribute);
                    }

                    IAttribute attribute =
                        GetFirstOrLastAttribute(controlAttributes, first, _activeDataControl);
                    return GetAttributeGenealogy(attribute);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/>s currently selected in the _activeDataControl.
        /// </summary>
        /// <returns>The <see cref="IAttribute"/>s currently selected in the _activeDataControl.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected IEnumerable<IAttribute> GetActiveAttributes()
        {
            SelectionState selectionState;
            if (_activeDataControl != null &&
                _controlSelectionState.TryGetValue(_activeDataControl, out selectionState))
            {
                // [DataEntry:1177]
                // Only viewable attributes that are within the current control's selection should
                // be considered active.
                foreach (IAttribute attribute in selectionState.Attributes
                    .Where(attribute => AttributeStatusInfo.IsAttributeViewable(attribute) &&
                        (selectionState.DataControl.HighlightSelectionInChildControls ||
                        AttributeStatusInfo.GetOwningControl(attribute) == selectionState.DataControl)))
                {
                    yield return attribute;
                }
            }
        }

        /// <summary>
        /// Retrieves the active attribute or selects from one of several active attributes.
        /// </summary>
        /// <param name="first">If <see langword="true"/> and at least 2 attributes are active, the
        /// first attribute (in tab order) will be returned, if <see langword="false"/> the last
        /// active attribute would be returned or if <see langword="null"/>, an attribute will be
        /// returned only if there is one and only one attribute active.</param>
        /// <returns>The active attribute per <see paramref="first"/>, or <see langword="null"/> if
        /// no qualifying attribute could be found.</returns>
        IAttribute GetActiveAttribute(bool? first)
        {
            if (_activeDataControl != null)
            {
                SelectionState selectionState;
                if (_controlSelectionState.TryGetValue(_activeDataControl, out selectionState))
                {
                    if (selectionState.Attributes.Count == 1)
                    {
                        return selectionState.Attributes[0];
                    }
                    else if (first != null)
                    {
                        return GetFirstOrLastAttribute(selectionState.Attributes, first.Value, 
                            _activeDataControl);
                    }
                }
            }
        
            return null;
        }

        /// <summary>
        /// Retrieves the first or last <see cref="IAttribute"/> in display order from the provided
        /// vector of <see cref="IAttribute"/>s.
        /// </summary>
        /// <param name="attributes">The list of <see cref="IAttribute"/>s
        /// from which the first or last attribute in display order should be found.</param>
        /// <param name="first"><see langword="true"/> if the first attribute from the specified
        /// list should be return <see langword="false"/> if the last attribute should be returned.
        /// </param>
        /// <param name="dataEntryControl">If not <see langword="null"/>, attributes not in the
        /// specified control will be ignored when selecting the active attribute.</param>
        /// <returns>The <see cref="IAttribute"/> containing the lowest (or highest)
        /// <see cref="AttributeStatusInfo.DisplayOrder"/>.</returns>
        static IAttribute GetFirstOrLastAttribute(IList<IAttribute> attributes, bool first,
            IDataEntryControl dataEntryControl)
        {
            if (attributes.Count == 0)
            {
                return null;
            }

            IAttribute targetAttribute = attributes[0];
            string targetDisplayOrder =
                AttributeStatusInfo.GetStatusInfo(targetAttribute).DisplayOrder;

            // Iterate through all provided attributes to find the one with the lowest display
            // order value.
            for (int i = 1; i < attributes.Count; i++)
            {
                IAttribute attribute = attributes[i];
                AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);

                // [DataEntry:4494]
                // If the selection is to be limited to the specified control, ignore attributes
                // from any other control.
                if (dataEntryControl == null || statusInfo.OwningControl == dataEntryControl)
                {
                    string displayOrder = AttributeStatusInfo.GetStatusInfo(attribute).DisplayOrder;
                    int comparisonResult = string.Compare(displayOrder, targetDisplayOrder,
                        StringComparison.CurrentCultureIgnoreCase);

                    // If the current attribute's display order is less than the lowest value found
                    // to this point (or greater than the highest value found when first == false),
                    // use this attribute as the target.
                    if ((first && comparisonResult < 0) || (!first && comparisonResult > 0))
                    {
                        targetDisplayOrder = displayOrder;
                        targetAttribute = attribute;
                    }
                }
            }

            return targetAttribute;
        }

        /// <summary>
        /// Retrieves the genealogy of the supplied <see cref="IAttribute"/>.
        /// <para><b>Requirements:</b></para>
        /// The supplied <see cref="IAttribute"/> must have been added as a key to the 
        /// _attributeToParentMap dictionary.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose genealogy is requested.
        /// </param>
        /// <returns>A genealogy of <see cref="IAttribute"/>s with each attribute further down the
        /// the stack being a descendant to the previous <see cref="IAttribute"/> in the stack; the
        /// last entry being the specified <see cref="IAttribute"/>.</returns>
        static Stack<IAttribute> GetAttributeGenealogy(IAttribute attribute)
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
        /// <param name="tabStopsOnly"><c>true</c> to confirm only that all tab-stop
        /// attributes have been viewed; <c>false</c> to confirm all visible
        /// attributes have been viewed.</param>
        /// <returns>The first <see cref="IAttribute"/> that has not been viewed, or 
        /// <see langword="null"/> if no unviewed <see cref="IAttribute"/>s were found.</returns>
        IAttribute GetNextUnviewedAttribute(bool tabStopsOnly)
        {
            // Look for any attributes whose data failed validation.
            Stack<IAttribute> unviewedAttributeGenealogy =
                AttributeStatusInfo.FindNextUnviewedAttribute(_attributes,
                ActiveAttributeGenealogy(true, null), true, true, tabStopsOnly);

            if (unviewedAttributeGenealogy != null)
            {
                return PropagateAttributes(unviewedAttributeGenealogy, true, false);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Determines whether there is an <see cref="IAttribute"/> whose data has failed
        /// validation.
        /// </summary>
        /// <param name="includeValidationWarnings"><see langword="true"/> if attributes marked
        /// AllowWithWarnings should be considered as invalid.</param>
        /// <returns><see langword="true"/> if there are any invalid <see cref="IAttribute"/>s
        /// else <see langword="false"/></returns>
        bool HasInvalidAttribute(bool includeValidationWarnings)
        {
            var targetValidity = includeValidationWarnings
                ? DataValidity.Invalid | DataValidity.ValidationWarning
                : DataValidity.Invalid;

            // Look for any attributes whose data failed validation.
            Stack<IAttribute> invalidAttributeGenealogy =
                AttributeStatusInfo.FindNextAttributeByValidity(_attributes,
                    targetValidity, ActiveAttributeGenealogy(true, null), true, true);

            return invalidAttributeGenealogy != null;
        }

        /// <summary>
        /// Attempts to find the next <see cref="IAttribute"/> (in display order) whose data
        /// has failed validation.
        /// </summary>
        /// <param name="includeWarnings"><see langword="true"/> if attributes marked
        /// AllowWithWarnings should be included in the search.</param>
        /// <param name="loop">Indicates whether navigation should loop back to the beginning
        /// if no further invalid data is found beyond the current field.</param>
        /// <param name="enabledOnly"><c>true</c> if only fields that are visible and enabled;
        /// <c>false</c> to return any field with invalid data.</param>
        /// <returns>The first <see cref="IAttribute"/> that failed validation, or 
        /// <see langword="null"/> if no invalid <see cref="IAttribute"/>s were found.</returns>
        IAttribute GetNextInvalidAttribute(bool includeWarnings, bool loop, bool enabledOnly)
        {
            // Toggle the enabled status of the active control to force editing to end and 
            // validation to occur for any field that is currently being edited.
            // TODO: [DataEntry:169] Consider a better way to do this...
            if (_activeDataControl != null)
            {
                // So that focus does not jump to the next control, assign focus to the image viewer
                // during the toggle of enabled.
                if (ImageViewer != null)
                {
                    ImageViewer.Focus();
                }

                Control control = (Control)_activeDataControl;
                control.Enabled = false;
                control.Enabled = true;
                control.Select();
            }

            var targetValidity = includeWarnings
                ? DataValidity.Invalid | DataValidity.ValidationWarning
                : DataValidity.Invalid;

            var activeAttributeGenealogy = ActiveAttributeGenealogy(true, null);
            var checkedAttributes = new HashSet<IAttribute>();

            // Loop in case enabledOnly == true and next invalid is in disabled control.
            do
            {
                Stack<IAttribute> invalidAttributeGenealogy =
                AttributeStatusInfo.FindNextAttributeByValidity(_attributes,
                    targetValidity, activeAttributeGenealogy, true, loop);

                if (invalidAttributeGenealogy != null)
                {
                    var activeAttribute = PropagateAttributes(
                        new Stack<IAttribute>(invalidAttributeGenealogy.Reverse()), // preserve genealogy stack for use below
                        true, false);

                    // Prevent infinite loop
                    if (checkedAttributes.Contains(activeAttribute))
                    {
                        return null;
                    }

                    if (!enabledOnly)
                    {
                        return activeAttribute;
                    }

                    var owningControl = AttributeStatusInfo.GetOwningControl(activeAttribute) as Control;
                    if (owningControl?.Visible == true && owningControl?.Enabled == true)
                    {
                        return activeAttribute;
                    }
                    else
                    {
                        checkedAttributes.Add(activeAttribute);
                        activeAttributeGenealogy = invalidAttributeGenealogy;
                    }
                }
                else
                {
                    return null;
                }
            }
            while (true);
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
        /// <see cref="WindowsMessage.LeftButtonDown"/> or 
        /// <see cref="WindowsMessage.RightButtonDown"/>.</param>
        /// <returns>The <see cref="IDataEntryControl"/> that should receive active status.
        /// <see langword="null"/> if no such <see cref="IDataEntryControl"/> was found.
        /// </returns>
        IDataEntryControl FindClickedDataEntryControl(Message m)
        {
            ExtractException.Assert("ELI24760", "Unexpected message!",
                (m.Msg == WindowsMessage.LeftButtonDown || m.Msg == WindowsMessage.RightButtonDown));

            // Initialize the return value to null.
            IDataEntryControl clickedDataEntryControl = null;

            // Obtain the control that was clicked (may be a container rather than the specific
            // control.
            Control clickedControl = FromHandle(m.HWnd);

            // [DataEntry:354]
            // Sometimes the window handle may be a child of a .Net control (such as the edit box
            // of a combo box). In this case, a Control will not be created from the handle.
            // Use the Win32 API to find the first ancestor that is a .Net control.
            while (clickedControl == null && m.HWnd != IntPtr.Zero)
            {
                m.HWnd = NativeMethods.GetParentWindowHandle(m.HWnd);

                clickedControl = FromHandle(m.HWnd);
            }

            if (clickedControl != null)
            {
                // Get the position of the mouse in screen coordinates.
                Point mousePosition = new Point((int)((uint)m.LParam & 0x0000FFFF),
                                                (int)((uint)m.LParam & 0xFFFF0000) >> 16);
                mousePosition = clickedControl.PointToScreen(mousePosition);

                // If the click occurred within the control host, if it didn't occur on a data entry
                // control, make sure that focus will remain on the currently active data entry 
                // control.
                if (clickedControl == this || Contains(clickedControl))
                {
                    clickedDataEntryControl = _activeDataControl;
                }

                // Loop down through all the control's descendants at the mouse position to try to
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
        /// Updates the unviewed and invalid attribute counts to account for the
        /// <see cref="IAttribute"/>s that have been deleted.
        /// </summary>
        /// <param name="deletedAttributes">The <see cref="IAttribute"/>s that have been
        /// deleted. Must not be <see langword="null"/>.</param>
        void ProcessDeletedAttributes(IUnknownVector deletedAttributes)
        {
            ExtractException.Assert("ELI25178", "Null argument exception!", 
                deletedAttributes != null);

            // Cycle through each deleted attribute
            foreach (IAttribute attribute in
                DataEntryMethods.ToAttributeEnumerable(deletedAttributes, true))
            {
                // If the attribute is part of _newlyAddedAttributes, remove it.
                if (_newlyAddedAttributes.Contains(attribute))
                {
                    _newlyAddedAttributes.Remove(attribute);
                }

                // Remove the highlight for this attribute
                RemoveAttributeHighlight(attribute);

                bool previousIsDataUnviewed = IsDataUnviewed;
                DataValidity previousDataValidity = DataValidity;

                _unviewedAttributes = _unviewedAttributes.Remove(attribute);
                _invalidAttributes = _invalidAttributes.Remove(attribute);

                if (IsDataUnviewed != previousIsDataUnviewed)
                {
                    OnUnviewedDataStateChanged();
                }

                if (previousDataValidity != DataValidity)
                {
                    OnDataValidityChanged();
                }

                AttributeStatusInfo.GetStatusInfo(attribute).AttributeValueModified -=
                    HandleAttributeValueModified;
                AttributeStatusInfo.GetStatusInfo(attribute).AttributeDeleted -=
                    HandleAttributeDeleted;
            }
        }

        /// <summary>
        /// Returns a list of all <see cref="RasterZone"/>s associated with the 
        /// provided set of <see cref="IAttribute"/>s (including all descendants of the supplied 
        /// <see cref="IAttribute"/>s if needed).
        /// </summary>
        /// <param name="attributes">The set of <see cref="IAttribute"/>s whose raster zones are 
        /// to be returned. Must not be <see langword="null"/>.
        /// </param>
        /// <returns>A list of raster zones from the supplied <see cref="IAttribute"/>s.</returns>
        static List<RasterZone> GetRasterZones(IUnknownVector attributes)
        {
            ExtractException.Assert("ELI25177", "Null argument exception!", attributes != null);

            // Create a list in which to compile the results.
            List<RasterZone> rasterZones = new List<RasterZone>();

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

                // Convert each raster zone to a RasterZone and add it to the result list.
                for (int j = 0; j < rasterZoneCount; j++)
                {
                    ComRasterZone comRasterZone =
                        comRasterZones.At(j) as ComRasterZone;

                    if (comRasterZone != null)
                    {
                        rasterZones.Add(new RasterZone(comRasterZone));
                    }
                }
            }

            return rasterZones;
        }

        /// <summary>
        /// Ensures the existence of one or more CompositeHighlightLayerObjects (one per page) for
        /// the specified attribute (creating one if necessary). Hints will be updated while actual
        /// highlights based on the data itself will be re-used as is.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which a highlight should be
        /// created. Must not be <see langword="null"/>.</param>
        /// <param name="makeVisible"><see langword="true"/> if the highlight should be initialized
        /// as visible, <see langword="false"/> to create the highlight as not visible. (unless a
        /// highlight it is replacing is already visible)</param>
        void SetAttributeHighlight(IAttribute attribute, bool makeVisible)
        {
            ExtractException.Assert("ELI25173", "Null argument exception!", attribute != null);

            // [DataEntry:1153]
            // There is at least one situation in which Undo can cause a table cell's mapped
            // attribute (which has already been deleted) to be selected. Make sure a highlight is
            // never added for a deleted (or uninitialized) attribute.
            var statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);
            if (!statusInfo.IsInitialized)
            {
                return;
            }

            // [DataEntry:1178]
            // Never set highlights for un-viewable attributes.
            if (!AttributeStatusInfo.IsAttributeViewable(attribute))
            {
                return;
            }

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

            // If the attribute does not have a hint and does not have any spatial information, no
            // highlight can be generated.
            if (!AttributeStatusInfo.HasSpatialInfo(attribute, false))
            {
                return;
            }

            bool isHint = AttributeStatusInfo.GetHintType(attribute) != HintType.None;
            bool isAccepted = AttributeStatusInfo.IsAccepted(attribute);

            foreach (var highlight in
                CreateAttributeHighlight(attribute, makeVisible, isHint, isAccepted))
            {
                _attributeHighlights[attribute].Add(highlight);
                _highlightAttributes[highlight] = attribute;
                ImageViewer.LayerObjects.Add(highlight, false);
            }
        }

        /// <summary>
        /// Creates the <see cref="CompositeHighlightLayerObject"/>s for the highlight of the
        /// <see paremref="attribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the highlight is needed.</param>
        /// <param name="makeVisible"><see langword="true"/> if the highlight should be immediately
        /// visible, otherwise <see langword="false"/>.</param>
        /// <param name="isHint"><see langword="true"/> if the highlight should be a hint,
        /// otherwise <see langword="false"/>.</param>
        /// <param name="isAccepted"><see langword="true"/> if the highlight should be accepted,
        /// otherwise <see langword="false"/>.</param>
        /// <returns>The <see cref="CompositeHighlightLayerObject"/>s for the specified
        /// <see paramref="attribute"/></returns>
        List<CompositeHighlightLayerObject> CreateAttributeHighlight(IAttribute attribute,
            bool makeVisible, bool isHint, bool isAccepted)
        {
            List<CompositeHighlightLayerObject> attributeHighlights =
                new List<CompositeHighlightLayerObject>();

            VariantVector zoneConfidenceTiers = null;
            VariantVector zoneIndices = null;
            IUnknownVector comRasterZones = null;
            List<RasterZone> rasterZones = null;

            // https://extract.atlassian.net/browse/ISSUE-14328
            // In the PaginationPanel, the document open in the image viewer may not be the document
            // to which this highlight pertains.
            // https://extract.atlassian.net/browse/ISSUE-14901
            // The above fix broke smart hints since the hints do not have a SourceDocName set.
            // It appears other changes now prevent ISSUE-14328 from occurring even without this
            // check, though it seems hard to track down exactly what changes along the way would
            // have done so. I'm torn between the risk of re-introducing ISSUE-14328 and other
            // as-yet undiscovered consequences of this early return. As a compromise, I'm adding a
            // check to confirm the attribute has an SDN before allowing the early return.
            if (!string.IsNullOrEmpty(attribute.Value.SourceDocName) &&
                !FileSystemMethods.ArePathsEqual(attribute.Value.SourceDocName, ImageViewer.ImageFile))
            {
                return attributeHighlights;
            }

            // For spatial attributes whose text has not been manually edited, use confidence tiers
            // to color code the highlights using OCR confidence.
            if (attribute.Value.GetMode() == ESpatialStringMode.kSpatialMode && !isAccepted)
            {
                comRasterZones = attribute.Value.GetOriginalImageRasterZonesGroupedByConfidence(
                    _confidenceBoundaries, false, out zoneConfidenceTiers, out zoneIndices);
            }
            // Otherwise the default highlight color will be used-- no need for confidence tiers.
            else if (!isHint)
            {
                comRasterZones = attribute.Value.GetOriginalImageRasterZones();
            }
            // For hints, get the raster zones from the AttributeStatusInfo.
            else
            {
                rasterZones = new List<RasterZone>(
                    AttributeStatusInfo.GetHintRasterZones(attribute));
            }

            // Convert the COM raster zones to Extract.Imaging.RasterZones
            if (comRasterZones != null)
            {
                rasterZones = new List<RasterZone>();

                int rasterZoneCount = comRasterZones.Size();
                for (int i = 0; i < rasterZoneCount; i++)
                {
                    ComRasterZone comRasterZone = comRasterZones.At(i) as ComRasterZone;

                    ExtractException.Assert("ELI25682", "Failed to retrieve raster zone!", 
                        comRasterZone != null);

                    rasterZones.Add(new RasterZone(comRasterZone));
                }
            }
            
            Dictionary<int, List<RasterZone>> highlightZones =
                new Dictionary<int, List<RasterZone>>();

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
                    highlightZones[key] = new List<RasterZone>();
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
                    new CompositeHighlightLayerObject(ImageViewer, page, "", highlightZones[key],
                        isHint ? Color.White : highlightColor);
                highlight.Selectable = false;
                highlight.CanRender = false;

                // If the highlight is a hint, specify the outline color.
                if (isHint)
                {
                    highlight.OutlineColor = highlightColor;
                }

                // [DataEntry:189] Highlights should not be visible unless explicitly directed.
                highlight.Visible = makeVisible;

                attributeHighlights.Add(highlight);
            }
            
            return attributeHighlights;
        }

        /// <summary>
        /// Shows a tooltip for the specified <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which a tooltip should be
        /// created.</param>
        void ShowAttributeToolTip(IAttribute attribute)
        {
            RemoveAttributeToolTip(attribute);

            // [DataEntry:1192]
            // Ensure tooltips don't get created for non-spatial or deleted attributes;
            if (AttributeStatusInfo.HasSpatialInfo(attribute, true) &&
                !string.IsNullOrEmpty(attribute.Value.String))
            {
                _attributeToolTips[attribute] = null;
            }
            else
            {
                // Recreate the error icon so that it is based on the attribute
                // highlight instead of the non-existent tool-tip (this takes care
                // of the case that a value was deleted without deleting the
                // highlight spatial info, e.g., by back-spacing)
                CreateAttributeErrorIcon(attribute, true);
            }
        }

        /// <summary>
        /// Creates and positions tooltips for attributes designated to have tooltips.
        /// </summary>
        void PositionToolTips()
        {
            // Loop through all attributes designated to receive a tooltip. Compile the bounding
            // rectangles of all the attributes on the current page that have spatial info and
            // keep track of the attributes that don't.
            List<IAttribute> attributesNotNeedingTooltips = new List<IAttribute>();
            Dictionary<IAttribute, RasterZone> attributeBoundingZones =
                new Dictionary<IAttribute, RasterZone>();
            foreach (IAttribute attribute in _attributeToolTips.Keys)
            {
                // Prepare true spatial attributes
                if (attribute.Value.HasSpatialInfo())
                {
                    SpatialString valueOnPage = attribute.Value.GetSpecifiedPages(
                        ImageViewer.PageNumber, ImageViewer.PageNumber);

                    // Ensure the attribute has spatial info on the current page
                    if (valueOnPage != null && valueOnPage.HasSpatialInfo())
                    {
                        LongRectangle rectangle = valueOnPage.GetOCRImageBounds();
                        int left, top, right, bottom;
                        rectangle.GetBounds(out left, out top, out right, out bottom);

                        attributeBoundingZones[attribute] =
                            new RasterZone(Rectangle.FromLTRB(left, top, right, bottom),
                                ImageViewer.PageNumber);
                    }
                    // If the attribute does not have spatial info on the current page, skip it.
                    else
                    {
                        attributesNotNeedingTooltips.Add(attribute);
                    }
                }
                // Prepare attributes with direct hints.
                else if (AttributeStatusInfo.GetHintType(attribute) == HintType.Direct)
                {
                    // Keep track of the overall bounds of hints on the current page
                    // (currently, there should only be one).
                    Rectangle? bounds = null;
                    
                    // Get the bounds of any rectangles on the current page
                    IEnumerable<RasterZone> rasterZones = AttributeStatusInfo.GetHintRasterZones(attribute);
                    foreach (RasterZone rasterZone in rasterZones)
                    {
                        if (rasterZone.PageNumber == ImageViewer.PageNumber)
                        {
                            if (bounds == null)
                            {
                                bounds = rasterZone.GetRectangularBounds();
                            }
                            else
                            {
                                bounds = Rectangle.Union(bounds.Value, rasterZone.GetRectangularBounds());
                            }
                        }
                    }

                    // If there was at least one raster zone, use this attribute.
                    if (bounds != null)
                    {
                        attributeBoundingZones[attribute] = new RasterZone(bounds.Value, ImageViewer.PageNumber);
                    }
                    // Otherwise, skip this attribute.
                    else
                    {
                        attributesNotNeedingTooltips.Add(attribute);
                    }
                }
                // If not a spatial attribute and not an attribute with a direct hint, skip this
                // attribute.
                else
                {
                    attributesNotNeedingTooltips.Add(attribute);
                }
            }

            // Clear the tooltip designation for attributes without spatial info on the current page.
            foreach (IAttribute attribute in attributesNotNeedingTooltips)
            {
                RemoveAttributeToolTip(attribute);
            }

            if (attributeBoundingZones.Count > 0)
            {
                // Determine whether the attributes appear to be arranged horizontally or vertically.
                bool horizontal =
                    HasHorizontalOrientation(new List<RasterZone>(attributeBoundingZones.Values));
                Dictionary<IAttribute, DataEntryToolTip> newAttributeToolTips =
                    new Dictionary<IAttribute, DataEntryToolTip>();

                // Create the tooltips
                foreach (IAttribute attribute in _attributeToolTips.Keys)
                {
                    newAttributeToolTips[attribute] = new DataEntryToolTip(this, attribute,
                        horizontal, attributeBoundingZones[attribute].GetRectangularBounds());
                }

                // Position the tooltips.
                DataEntryToolTip.PositionToolTips(
                    new List<DataEntryToolTip>(newAttributeToolTips.Values));

                // Update _attributeToolTips with a version containing the created tooltips.
                _attributeToolTips = newAttributeToolTips;
            }

            if (_hoverToolTip != null)
            {
                ImageViewer.LayerObjects.MoveToTop(_hoverToolTip.TextLayerObject);
            }

            // Create error icons now that tooltips exist
            foreach (IAttribute attribute in _attributeToolTips.Keys)
            {
                CreateAttributeErrorIcon(attribute, true);
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="RasterZone"/>s are arranged horizontally
        /// or vertically.
        /// </summary>
        /// <param name="rasterZones">The <see cref="RasterZone"/>s to check for arrangement.</param>
        /// <returns><see langword="true"/> if the zones appear to be arranged horizontally or it is not
        /// clear which way they are arranged, <see langword="false"/> if the zones appear to be
        /// arranged vertically.</returns>
        static bool HasHorizontalOrientation(List<RasterZone> rasterZones)
        {
            // If there are not multiple raster zones, default to horizontal.
            if (rasterZones.Count < 2)
            {
                return true;
            }

            // Default to horizontal.
            bool horizontal = true;

            // Find the horizontal overlap in the raster zones.
            double horizontalOverlap;
            SpatialHintGenerator.GetHintRange(rasterZones, true, out horizontalOverlap);

            if (horizontalOverlap > 0.1)
            {
                // If it appears there is horizontal overlap (indicating a vertical arrangement),
                // test to see if there is more horizontal overlap than vertical overlap.
                double verticalOverlap;
                SpatialHintGenerator.GetHintRange(rasterZones, false, out verticalOverlap);

                if (horizontalOverlap > verticalOverlap)
                {
                    // More horizontal overlap = vertically arranged zones.
                    horizontal = false;
                }
            }

            return horizontal;
        }

        /// <summary>
        /// Sorts the <see cref="IAttribute"/>s contained in the provided dictionaries according to
        /// their positions in the document. The sorting will attempt to identify attributes in a
        /// row across the page and work from left-to-right across the row before dropping down to
        /// the next row.
        /// <para><b>Note</b></para>
        /// This private helper will not include any non-spatial attributes in the results.
        /// </summary>
        /// <param name="attributesByPage">A map of each page to a list of attributes that begin on
        /// the specified page.</param>
        /// <param name="attributeBounds">A map of each attribute to a rectangle describing its
        /// bounds.</param>
        /// <returns>The sorted <see cref="IAttribute"/>s.</returns>
        List<IAttribute> SortAttributesSpatially(Dictionary<int, List<IAttribute>> attributesByPage,
            Dictionary<IAttribute, Rectangle> attributeBounds)
        {
            List<IAttribute> sortedList = new List<IAttribute>();

            // Compile the sorted attributes page-by-page
            for (int i = 1; i <= ImageViewer.PageCount; i++)
            {
                // If there are no attributes on the specified page, move on.
                List<IAttribute> attributesOnPage;
                if (!attributesByPage.TryGetValue(i, out attributesOnPage))
                {
                    continue;
                }

                // Cycle through the attributes on this page attempting to group them into rows.
                List<List<IAttribute>> rows = new List<List<IAttribute>>();
                foreach (IAttribute attribute in attributesOnPage)
                {
                    bool belongsInExistingRow = false;

                    // Check to see if the attribute appears to belong in an existing row.
                    foreach (List<IAttribute> row in rows)
                    {
                        // Compare with each attribute in the existing row to see if it seems to
                        // line up with it.
                        foreach (IAttribute existingAttribute in row)
                        {
                            Rectangle rectangle = attributeBounds[attribute];
                            Rectangle existingRectangle = attributeBounds[existingAttribute];

                            int horizontalOverlap =
                                Math.Min(rectangle.Right, existingRectangle.Right) -
                                Math.Max(rectangle.Left, existingRectangle.Left);

                            int verticalOverlap =
                                Math.Min(rectangle.Bottom, existingRectangle.Bottom) -
                                Math.Max(rectangle.Top, existingRectangle.Top);

                            // If there is more vertical overlap than horizontal overlap,
                            // make the attribute a member of this row.
                            if (verticalOverlap > 0 && verticalOverlap > horizontalOverlap)
                            {
                                row.Add(attribute);
                                belongsInExistingRow = true;
                                break;
                            }
                        }

                        // If the attribute was added to an existing row, don't search any remaining
                        // row.
                        if (belongsInExistingRow)
                        {
                            break;
                        }
                    }

                    // If the attribute didn't belong to an existing row, create a new row for it.
                    if (!belongsInExistingRow)
                    {
                        List<IAttribute> row = new List<IAttribute>();
                        row.Add(attribute);
                        rows.Add(row);
                    }
                }

                // Calculate the Y position of each row.
                Dictionary<int, List<IAttribute>> rowPositions =
                    new Dictionary<int, List<IAttribute>>();
                foreach (List<IAttribute> row in rows)
                {
                    int rowPosition = -1;

                    foreach (IAttribute attribute in row)
                    {
                        int attributePosition = attributeBounds[attribute].Top;

                        if (rowPosition == -1 || attributePosition < rowPosition)
                        {
                            rowPosition = attributePosition;
                        }
                    }

                    if (rowPositions.ContainsKey(rowPosition))
                    {
                        rowPositions[rowPosition].AddRange(row);
                    }
                    else
                    {
                        rowPositions[rowPosition] = row;
                    }
                }

                // Sort the rows top down.
                List<int> sortedRowPositions = new List<int>(rowPositions.Keys);
                sortedRowPositions.Sort();

                // Loop through each row in order to sort the attributes in each from left to right.
                foreach (int rowPosition in sortedRowPositions)
                {
                    List<IAttribute> row = rowPositions[rowPosition];

                    // Calculate the X position of each attribute in the row.
                    Dictionary<int, List<IAttribute>> attributePositions =
                            new Dictionary<int, List<IAttribute>>();
                    foreach (IAttribute attribute in row)
                    {
                        int attributePosition = attributeBounds[attribute].Left;

                        if (attributePositions.ContainsKey(attributePosition))
                        {
                            attributePositions[attributePosition].Add(attribute);
                        }
                        else
                        {
                            attributePositions[attributePosition] = new List<IAttribute>();
                            attributePositions[attributePosition].Add(attribute);
                        }
                    }

                    // Sort the attributes in the row.
                    List<int> sortedAttributePositions = new List<int>(attributePositions.Keys);
                    sortedAttributePositions.Sort();

                    // Add the sorted attributes to the result list.
                    foreach (int attributePosition in sortedAttributePositions)
                    {
                        sortedList.AddRange(attributePositions[attributePosition]);
                    }
                }
            }

            return sortedList;
        }

        /// <summary>
        /// If needed, creates a <see cref="ImageLayerObject"/> to display an error icon indicating
        /// that the field's value is invalid. (If the data is valid, no such icon will be created).
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which an the error icon is to
        /// potentially be associated with. Must not be <see langword="null"/>.</param>
        /// <param name="makeVisible"><see langword="true"/> to make the icon visible right away or
        /// <see langword="false"/> if the icon should be invisible initially.</param>
        void CreateAttributeErrorIcon(IAttribute attribute, bool makeVisible, DataEntryToolTip toolTip = null)
        {
            ExtractException.Assert("ELI25699", "Null argument exception!", attribute != null);

            // Remove any existing error icon in case the attribute's spatial area has changed.
            RemoveAttributeErrorIcon(attribute);

            // If the attribute's data is valid, or it is represented by an indirect hint there is
            // nothing else to do.
            if (AttributeStatusInfo.GetDataValidity(attribute) != DataValidity.Invalid ||
                AttributeStatusInfo.GetHintType(attribute) == HintType.Indirect)
            {
                return;
            }

            // If there is no tooltip for this attribute then anchor the error icon the
            // attribute's raster zones (one anchor point per page), else anchor to the tooltip
            var anchorPoints = new List<(int page, Point anchorPoint, double rotation)>();
            if (toolTip == null && !_attributeToolTips.TryGetValue(attribute, out toolTip)
                || toolTip == null)
            {
                // Groups the attribute's raster zones by page.
                Dictionary<int, List<RasterZone>> rasterZonesByPage =
                    GetAttributeRasterZonesByPage(attribute, false);

                // Create an error icon for each page on which the attribute is present.
                foreach (int page in rasterZonesByPage.Keys)
                {
                    List<RasterZone> rasterZones = rasterZonesByPage[page];

                    // The anchor point for the error icon should be to the right of attribute.
                    var point = GetAnchorPoint(rasterZones, AnchorAlignment.Right, 90,
                        (int)Config.Settings.TooltipFontSize, out double errorIconRotation, out var _);

                    anchorPoints.Add((page, point, errorIconRotation));
                }
            }
            else
            {
                var bounds = toolTip.NormalizedBounds;
                var layerObject = toolTip.TextLayerObject;
                var page = layerObject.PageNumber;
                var orientation = layerObject.Orientation;

                // The anchor point for the error icon should be to the right of attribute's tooltip.
                var point = GetAnchorPoint(bounds, orientation, AnchorAlignment.Right, 90,
                    (int)Config.Settings.TooltipFontSize);
                anchorPoints.Add((page, point, orientation));
            }

            var errorIconsForAttribute = _attributeErrorIcons.GetOrAdd(attribute, _ =>
                new List<ImageLayerObject>());

            foreach (var (page, point, rotation) in anchorPoints)
            {
                // Create the error icon
                ImageLayerObject errorIcon = new ImageLayerObject(ImageViewer, page,
                    "", point, AnchorAlignment.Left,
                    Properties.Resources.LargeErrorIcon, GetPageIconSize(page),
                    (float)rotation)
                {
                    Selectable = false,
                    Visible = makeVisible,
                    CanRender = false
                };
                ImageViewer.LayerObjects.Add(errorIcon, false);

                // NOTE: For now I think the cases where the error icon would extend off-page are so
                // rare that it's not worth handling. But this would be where such a check should
                // be made (see ShowAttributeToolTip).
                errorIconsForAttribute.Add(errorIcon);
            }
        }

        /// <summary>
        /// Finds an anchor point to use to attach an <see cref="AnchoredObject"/> to the specified
        /// list of <see cref="RasterZone"/>s.
        /// </summary>
        /// <param name="rasterZones">The list of <see cref="RasterZone"/>s for
        /// which and anchor point is needed.</param>
        /// <param name="anchorAlignment">The point location along a bounding box of all supplied
        /// zones on which the anchor point should be based.  The bounding box will be relative to
        /// the orientation of the suppled raster zones. (i.e., if the raster zones are upside-down,
        /// anchorAlignment.Top would result in an anchor point at the bottom of the zones as they
        /// appear on the page.</param>
        /// <param name="anchorOffsetAngle">The anchor point will be offset from the anchorAlignment
        /// position at this angle. (relative to the raster zones' orientation, not the page)</param>
        /// <param name="standoffDistance">The number of image pixels away from the bounds of the
        /// raster zones the anchor point should be.</param>
        /// <param name="anchoredObjectRotation">Specifies the rotation (in degrees) that an
        /// associated <see cref="AnchoredObject"/> should be drawn at to match up with the
        /// orientation of the raster zones.  This will be the average rotation of the zones unless
        /// it is sufficiently close to level, in which case the angle will be rounded off to
        /// improve the appearance of the associated <see cref="AnchoredObject"/>.</param>
        /// <param name="bounds">The angled bounding box calculated for the <see paramref="rasterZones"/></param>
        /// <returns>A <see cref="Point"/> to use as the anchor for an <see cref="AnchoredObject"/>.
        /// </returns>
        static Point GetAnchorPoint(
            IList<RasterZone> rasterZones, AnchorAlignment anchorAlignment, double anchorOffsetAngle,
            int standoffDistance, out double anchoredObjectRotation, out RectangleF bounds)
        {
            bounds =
                RasterZone.GetAngledBoundingRectangle(rasterZones, out double averageRotation);

            // Based on the raster zones' dimensions, calculate how far from level the raster zones
            // can be and still have a level tooltip before the tooltip would overlap with one of the
            // raster zones. This calculation assumes the tooltip will be placed half the height of
            // the raster zone above the raster zone.
            double roundingCuttoffAngle = (180.0 / Math.PI) * GeometryMethods.GetAngle(
                new PointF(0, 0), new PointF(bounds.Width, standoffDistance));

            // Allow a maximum of 5 degrees of departure from level even if a greater angle was
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

            return GetAnchorPoint(bounds, averageRotation, anchorAlignment, anchorOffsetAngle,
                standoffDistance);
        }

        /// <summary>
        /// Finds an anchor point to use to attach an <see cref="AnchoredObject"/> to the specified
        /// <see cref="RectangleF"/>.
        /// </summary>
        /// <param name="bounds">The <see cref="RectangleF"/> for which the anchor point is needed.</param>
        /// <param name="anchorAlignment">The point location along the supplied bounds
        /// on which the anchor point should be based. The alignment will be relative to
        /// the value of <see paramref="rotation"/> (i.e., if the rotation is 180,
        /// anchorAlignment.Top would result in an anchor point at the bottom of the rectange as it
        /// appear on the page.</param>
        /// <param name="anchorOffsetAngle">The anchor point will be offset from the anchorAlignment
        /// position at this angle. (relative to the rectangle's orientation, not the page)</param>
        /// <param name="standoffDistance">The number of image pixels away from the bounds of the
        /// the anchor point should be.</param>
        /// <returns>A <see cref="Point"/> to use as the anchor for an <see cref="AnchoredObject"/>.
        /// </returns>
        static Point GetAnchorPoint(
            RectangleF bounds, double rotation, AnchorAlignment anchorAlignment, double anchorOffsetAngle,
            int standoffDistance)
        {
            // Find the reference location for the anchor point using the specified AnchorAlignment.
            Point[] anchorPoint = { Point.Round(bounds.Location) };

            switch (anchorAlignment)
            {
                case AnchorAlignment.LeftTop:
                    // Nothing to do
                    break;
                case AnchorAlignment.Top:
                    anchorPoint[0].Offset((int)bounds.Width / 2, 0);
                    break;
                case AnchorAlignment.RightTop:
                    anchorPoint[0].Offset((int)bounds.Width, 0);
                    break;
                case AnchorAlignment.Right:
                    anchorPoint[0].Offset((int)bounds.Width, (int)bounds.Height / 2);
                    break;
                case AnchorAlignment.RightBottom:
                    anchorPoint[0].Offset((int)(int)bounds.Width, (int)bounds.Height);
                    break;
                case AnchorAlignment.Bottom:
                    anchorPoint[0].Offset((int)bounds.Width / 2, (int)bounds.Height);
                    break;
                case AnchorAlignment.LeftBottom:
                    anchorPoint[0].Offset(0, (int)bounds.Height);
                    break;
                case AnchorAlignment.Left:
                    anchorPoint[0].Offset(0, (int)bounds.Height / 2);
                    break;
                case AnchorAlignment.Center:
                    anchorPoint[0].Offset((int)bounds.Width / 2, (int)bounds.Height / 2);
                    break;
            }

            using (Matrix transform = new Matrix())
            {
                // Offset the anchor point by the standoffDistance in the direction specified.
                transform.Rotate((float)anchorOffsetAngle);
                Point[] offset = { new Point(0, -standoffDistance) };
                transform.TransformVectors(offset);
                anchorPoint[0].Offset(offset[0]);

                // Rotate the anchor point back into the image coordinate system.
                transform.Reset();
                transform.Rotate((float)rotation);
                transform.TransformPoints(anchorPoint);
            }

            return anchorPoint[0];
        }

        /// <summary>
        /// Creates highlights for all <see cref="IAttribute"/>s in the provided 
        /// <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/>s for which highlights should be
        /// generated. Must not be <see langword="null"/>.</param>
        /// <param name="preCreatedHighlights">A <see cref="HighlightDictionary"/> that may contain
        /// pre-created highlights for the <see paramref="attributes"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected void CreateAllAttributeHighlights(IUnknownVector attributes,
            HighlightDictionary preCreatedHighlights)
        {
            ExtractException.Assert("ELI25174", "Null argument exception!", attributes != null);

            // Loop through each attribute and compile the raster zones from each.
            foreach (IAttribute attribute in
                DataEntryMethods.ToAttributeEnumerable(attributes, true))
            {
                // If this attribute is not visible in the DEP, don't create a highlight.
                if (!AttributeStatusInfo.IsAttributeViewable(attribute))
                {
                    continue;
                }

                IDataEntryControl owningControl =
                    AttributeStatusInfo.GetStatusInfo(attribute).OwningControl;

                ExtractException.Assert("ELI25139",
                    "Unable to show highlight for unmapped attribute!", owningControl != null);

                // Determine whether the highlight should be visible be default based on
                // ShowAllHighlights and whether the highlight is associated with an indirect
                // hint.
                bool makeVisible = _dataEntryApp.ShowAllHighlights;
                if (makeVisible)
                {
                    if (AttributeStatusInfo.GetHintType(attribute) == HintType.Indirect)
                    {
                        makeVisible = false;
                    }
                }

                // Attempt to apply a pre-created highlight if it is available.
                if (!ApplyPreCreatedHighlight(attribute, makeVisible, preCreatedHighlights))
                {
                    // Otherwise create the highlight for this attribute.
                    SetAttributeHighlight(attribute, makeVisible);
                }
            }
        }

        /// <summary>
        /// Attempts to apply a pre-created highlight if it is available. Also creates a an error
        /// icon if necessary.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the highlight should
        /// be applied.</param>
        /// <param name="makeVisible"><see langword="true"/> to make the icon visible right away or
        /// <see langword="false"/> if the icon should be invisible initially.</param>
        /// <param name="preCreatedHighlightDictionary">A <see cref="HighlightDictionary"/> that may
        /// contain pre-created highlights for the <see paramref="attribute"/>.</param>
        /// <returns><see langword="true"/> if a pre-created highlight was successfully applied;
        /// otherwise, <see langword="false"/></returns>
        bool ApplyPreCreatedHighlight(IAttribute attribute, bool makeVisible,
            HighlightDictionary preCreatedHighlightDictionary)
        {
            // Look up any pre-created highlight.
            List<CompositeHighlightLayerObject> preCreatedHighlights = null;
            if (preCreatedHighlightDictionary != null &&
                preCreatedHighlightDictionary.TryGetValue(attribute, out preCreatedHighlights))
            {
                // If this highlight is to be a hint or is to be accepted, the pre-created highlight
                // cannot be used. Dispose of it.
                if (AttributeStatusInfo.GetHintType(attribute) != HintType.None ||
                    AttributeStatusInfo.IsAccepted(attribute))
                {
                    CollectionMethods.ClearAndDispose(preCreatedHighlights);
                    return false;
                }

                // Assign the pre-created highlight and make it visible (if necessary).
                _attributeHighlights[attribute] = preCreatedHighlights;

                foreach (var highlight in preCreatedHighlights)
                {
                    _highlightAttributes[highlight] = attribute;
                    ImageViewer.LayerObjects.Add(highlight, false);

                    if (makeVisible)
                    {
                        highlight.Visible = true;
                    }
                }

                CreateAttributeErrorIcon(attribute, makeVisible);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the highlights for the currently active <see cref="IAttribute"/>(s).
        /// </summary>
        void RefreshActiveControlHighlights()
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
                    // ShowAllHighlights, how many attributes are selected, and whether the
                    // spatial info represents an indirect hint.
                    if (_dataEntryApp.ShowAllHighlights)
                    {
                        if (_controlSelectionState[_activeDataControl].Attributes.Count == 1 &&
                            _controlSelectionState[_activeDataControl].Attributes[0] == attribute)
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
        void RemoveAttributeHighlight(IAttribute attribute)
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

                    if (ImageViewer.LayerObjects.Contains(highlight))
                    {
                        ImageViewer.LayerObjects.Remove(highlight, true, false);
                    }
                    else
                    {
                        highlight.Dispose();
                    }
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
        void RemoveAttributeToolTip(IAttribute attribute)
        {
            ExtractException.Assert("ELI25176", "Null argument exception!", attribute != null);

            // Check if the attribute is the _hoverAttribute and remove the _hoverToolTip if so.
            if (attribute != null && attribute == _hoverAttribute && _hoverToolTip != null)
            {
                _hoverToolTip.Dispose();
                _hoverToolTip = null;
            }

            // Check if the attribute is one of the attributes displaying a tooltip; remove and
            // dispose of the tooltip if so.
            DataEntryToolTip toolTip;
            if (_attributeToolTips.TryGetValue(attribute, out toolTip))
            {
                if (toolTip != null)
                {
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
        void RemoveAttributeErrorIcon(IAttribute attribute)
        {
            ExtractException.Assert("ELI25702", "Null argument exception!", attribute != null);

            List<ImageLayerObject> errorIcons;
            if (_attributeErrorIcons.TryGetValue(attribute, out errorIcons))
            {
                foreach (ImageLayerObject errorIcon in errorIcons)
                {
                    if (ImageViewer.LayerObjects.Contains(errorIcon))
                    {
                        ImageViewer.LayerObjects.Remove(errorIcon, true, false);
                    }
                    else
                    {
                        errorIcon.Dispose();
                    }
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
        void ShowErrorIcon(IAttribute attribute, bool show)
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
        /// Displays or hides error icon for all invalid <see cref="IAttribute"/>s.
        /// </summary>
        /// <param name="show"><see langword="true"/> to display the error icons,
        /// <see langword="false"/> to hide them.</param>
        void ShowAllErrorIcons(bool show)
        {
            foreach (List<ImageLayerObject> errorIconList in _attributeErrorIcons.Values)
            {
                foreach (ImageLayerObject errorIcon in errorIconList)
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
        ImageLayerObject GetErrorIconOnPage(IAttribute attribute, int page)
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
        /// Shows or hides the highlight for the specified <see cref="IAttribute"/>. If the
        /// attribute has an associated error icon, it is also shown or hidden.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to show or hide.</param>
        /// <param name="show"><see langword="true"/> to show the highlight; <see langword="false"/>
        /// to hide it.</param>
        void ShowAttributeHighlights(IAttribute attribute, bool show)
        {
            ShowErrorIcon(attribute, show && !_temporarilyHidingTooltips);

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
        /// Prevents all <see cref="IDataEntryControl"/>s and the <see cref="ImageViewer"/> from
        /// updating (redrawing) until the lock is released.
        /// </summary>
        /// <param name="lockUpdates"><see langword="true"/> to lock all controls from updating
        /// or <see langword="false"/> to release the lock and allow updates again.</param>
        void LockControlUpdates(bool lockUpdates)
        {
            if (Parent == null || lockUpdates == _controlUpdatesLocked)
            {
                // If the DEP is not yet added to a form or the requested state is the same as the
                // current state, there is nothing to do.
                return;
            }
            else
            {
                _controlUpdatesLocked = lockUpdates;
            }

            // Lock or unlock the ImageViewer using Begin/EndUpdate
            if (ImageViewer != null)
            {
                if (lockUpdates)
                {
                    ImageViewer.BeginUpdate();
                }
                else
                {
                    ImageViewer.EndUpdate();
                }
            }

            // Lock the DEP's parent (a Panel) to prevent scrolling
            FormsMethods.LockControlUpdate(Parent, lockUpdates);

            // Lock each visible control.
            foreach (IDataEntryControl dataControl in _dataControls)
            {
                Control control = (Control)dataControl;
                if (control.Visible)
                {
                    FormsMethods.LockControlUpdate(control, lockUpdates);
                }
            }

            // If unlocking the updates, refresh the control now to show changes since the lock was
            // applied.
            if (!lockUpdates)
            {
                Refresh();
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
        static IEnumerable<IAttribute> GetViewableAttributesInControl(IDataEntryControl dataEntryControl,
            IUnknownVector attributes)
        {
            ExtractException.Assert("ELI25356", "Null argument exception!", dataEntryControl != null);
            ExtractException.Assert("ELI25357", "Null argument exception!", attributes != null);

            // Initialize a list of viewable attributes.
            List<IAttribute> viewableAttributes = new List<IAttribute>();

            // Traverse the tree of attributes looking for viewable ones owned by dataEntryControl.
            foreach (IAttribute attribute in
                DataEntryMethods.ToAttributeEnumerable(attributes, true))
            {
                if (AttributeStatusInfo.GetOwningControl(attribute) == dataEntryControl &&
                    AttributeStatusInfo.IsAttributeViewable(attribute))
                {
                    viewableAttributes.Add(attribute);
                }
            }

            return viewableAttributes;
        }

        /// <summary>
        /// Performs any operations that need to occur after ImageFileChanged has been called for
        /// a newly loaded document.
        /// </summary>
        void FinalizeDocumentLoad()
        {
            try
            {
                if (ImageViewer != null && ImageViewer.IsImageAvailable)
                {
                    // Initialize _currentlySelectedGroupAttribute as null. 
                    _currentlySelectedGroupAttribute = null;
                    _lastNavigationViaTabKey = true;
                }

                OnItemSelectionChanged();

                ExecuteOnIdle("ELI34419", () => AttributeStatusInfo.UndoManager.TrackOperations = true);
                ExecuteOnIdle("ELI34420", () => _dirty = false);
                // Forget all LastAppliedStringValues that are currently being remembered to ensure
                // that they don't get used later on after the value has been changed to something
                // else.
                ExecuteOnIdle("ELI37382", () => AttributeStatusInfo.ForgetLastAppliedStringValues());

                ExecuteOnIdle("ELI39651", () =>
                {
                    // Don't signal the update to have ended until we are ready to initialize control state/selection
                    // Otherwise, events such as keystrokes can be processed between the time UpdateEnded is signaled
                    // and selection is initialized and prevent the selection initialization expected below.
                    // Specifically, in the PaginationUtility project, HandlePaginationSeparator_DocumentDataPanelRequest
                    // is dependent on this this come after any potential windows messages are handled in the document
                    // load process, but before selection is initialized.
                    OnUpdateEnded(new EventArgs());

                    _isDocumentLoaded = true;

                    // Loading + events up thru the UpdateEnded event can trigger focus events.
                    // Wait until any potential focus-grabbing operations have completed before
                    // enabling controls.
                    // Selection will be initialized per _initialSelection below.
                    EnableControls(true);

                    // If _initialSelection is set to anything other than DoNotReset, initialize
                    // field selection.
                    if (Active
                        && ImageViewer?.IsImageAvailable == true
                        && _initialSelection != FieldSelection.DoNotReset)
                    {
                        if (_initialSelection == FieldSelection.Error)
                        {
                            GoToNextInvalid(includeWarnings: true, loop: true);
                        }
                        else
                        {
                            AdvanceToNextTabStop(_initialSelection != FieldSelection.Last, viaTabKey: false);
                        }
                    }
                    else
                    {
                        ClearSelection();
                    }

                    _initialSelection = FieldSelection.DoNotReset;

                    // By default, all query executions from this point forward should be considered
                    // to be the result of a manual data update.
                    AttributeStatusInfo.QueryExecutionContext = ExecutionContext.OnUpdate;
                    OnDocumentLoaded();
                });

                if (_attributes != null && _attributes.Size() > 0 &&
                    AttributeStatusInfo.IsLoggingEnabled(LogCategories.DataLoad))
                {
                    ExecuteOnIdle("ELI38345", () => AttributeStatusInfo.Logger.LogEvent(
                        LogCategories.DataLoad, null,
                        "END: ----------------------------------------------------"));
                }

                // If testing performance, commit each document as soon as it is loaded.
                if (AttributeStatusInfo.PerformanceTesting)
                {
                    ExecuteOnIdle("ELI36156", () => DataEntryApplication.Commit());
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30037", ex);
            }
        }

        /// <summary>
        /// Enables/disables all data entry controls. Controls will only be enabled pending the
        /// availability of a document and the state of the <see cref="IDataEntryControl.Disabled"/> property.
        /// </summary>
        /// <param name="enable"></param>
        void EnableControls(bool enable)
        {
            enable = enable && ImageViewer?.IsImageAvailable == true;

            foreach (IDataEntryControl dataControl in _dataControls)
            {
                Control control = (Control)dataControl;
                control.Enabled = enable && !dataControl.Disabled;
            }

            foreach (Control control in _nonDataControls)
            {
                control.Enabled = enable;
            }
        }

        /// <summary>
        /// Determines the size error icons overlaid onto the document image should be given the DPI
        /// of the specified <see paramref="page"/>.
        /// </summary>
        /// <param name="page">The page for which the size is needed.</param>
        Size GetPageIconSize(int pageNumber)
        {
            _errorIconSizes.Clear();

            Size iconSize;
            if (_errorIconSizes.TryGetValue(pageNumber, out iconSize))
            {
                return iconSize;
            }
            else if (ImageViewer.IsImageAvailable)
            {
                // Calculate the size the error icon for invalid data should be on each
                // page and create a SpatialPageInfo entry for each page.
                for (int page = 1; page <= ImageViewer.PageCount; page++)
                {
                    var pageProperties = ImageViewer.GetPageProperties(page);

                    _errorIconSizes[page] = new Size(
                        (int)(_ERROR_ICON_SIZE * pageProperties.XResolution),
                        (int)(_ERROR_ICON_SIZE * pageProperties.YResolution));
                }

                return _errorIconSizes[pageNumber];
            }
            else
            {
                new ExtractException("ELI41690", "Failed to look up page size.").Log();
                return new Size((int)(_ERROR_ICON_SIZE * 300), (int)(_ERROR_ICON_SIZE * 300));
            }
        }

        /// <summary>
        /// Executes the provided delegate only after the DEP's message pump is completely empty.
        /// This differs from simply using BeginInvoke to execute it via the message pump since
        /// messages already in the queue may result in further messages getting queued. Therefore,
        /// ExecuteOnIdle ensures that all other messages that are to occur as part of the current
        /// message chain occur before the provided delegate.
        /// </summary>
        /// <param name="eliCode">The ELI code to attribute to any exception.</param>
        /// <param name="action">The action to execute once the DEP's message pump is empty.</param>
        public void ExecuteOnIdle(string eliCode, Action action)
        {
            try
            {
                if (_isIdle)
                {
                    this.SafeBeginInvoke(eliCode, () => action());
                }
                else
                {
                    _idleCommands.Enqueue(new Tuple<Action, string>(action, eliCode));
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31024", ex);
            }
        }

        /// <summary>
        /// Displays a message to the user via a tooltip.  The tooltip will be displayed at the
        /// current cursor location and remain for 5 seconds.
        /// </summary>
        /// <param name="message">The message to be displayed to the user.</param>
        void ShowUserNotificationTooltip(string message)
        {
            if (AttributeStatusInfo.IsLoggingEnabled(LogCategories.TooltipNotification))
            {
                AttributeStatusInfo.Logger.LogEvent(LogCategories.TooltipNotification, null, message);
            }

            // Re-create the tooltip every time-- otherwise sometimes the text of the tooltip
            // doesn't seem to be properly updated and/or the tooltip will disappear really quickly.
            if (_userNotificationTooltip != null)
            {
                _userNotificationTooltip.Dispose();
            }
            _userNotificationTooltip = new ToolTip();

            // Since the angular highlight cursor extends above the cursor position and would
            // otherwise be drawn on top of the tooltip, shift the tooltip below the cursor
            // position if the angular highlight cursor tool is active.
            Point toolTipPosition = ImageViewer.TopLevelControl.PointToClient(MousePosition);
            if (ImageViewer.CursorTool == CursorTool.AngularHighlight)
            {
                toolTipPosition.Offset(0, 35);
            }

            _userNotificationTooltip.Show(message, ImageViewer.TopLevelControl, toolTipPosition, 5000);
        }

        /// <summary>
        /// Registers for events.
        /// </summary>
        void RegisterForEvents()
        {
            _imageViewer.CursorToolChanged += HandleCursorToolChanged;
            _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;
            _imageViewer.OcrTextHighlighted += HandleOcrTextHighlighted;
            _imageViewer.PreviewKeyDown += HandleImageViewerPreviewKeyDown;
            _imageViewer.MouseDown += HandleImageViewerMouseDown;
            _imageViewer.ZoomChanged += HandleImageViewerZoomChanged;
            _imageViewer.ScrollPositionChanged += HandleImageViewerScrollPositionsChanged;
            _imageViewer.PageChanged += HandleImageViewerPageChanged;
            _imageViewer.FitModeChanged += HandleImageViewerFitModeChanged;
            if (_imageViewer.CursorTool == CursorTool.SelectLayerObject)
            {
                _imageViewer.CursorEnteredLayerObject += HandleCursorEnteredLayerObject;
                _imageViewer.CursorLeftLayerObject += HandleCursorLeftLayerObject;
            }

            // While this event is not from the _imageViewer, it uses the _imageViewer. This
            // instance may be part of a doc type configuration that is not currently active.
            // _imageViewer will be set only when this instance is active.
            _dataEntryApp.ShowAllHighlightsChanged += HandleShowAllHighlightsChanged;
        }

        /// <summary>
        /// Unregisters for events.
        /// </summary>
        void UnregisterForEvents()
        {
            // [DataEntry:1078]
            // This will get called in the process of closing and depending on the configuration
            // when closing, the image viewer may already have been disposed of.
            if (!_imageViewer.IsDisposed)
            {
                _imageViewer.CursorToolChanged -= HandleCursorToolChanged;
                _imageViewer.LayerObjects.LayerObjectAdded -= HandleLayerObjectAdded;
                _imageViewer.OcrTextHighlighted -= HandleOcrTextHighlighted;
                _imageViewer.PreviewKeyDown -= HandleImageViewerPreviewKeyDown;
                _imageViewer.MouseDown -= HandleImageViewerMouseDown;
                _imageViewer.ZoomChanged -= HandleImageViewerZoomChanged;
                _imageViewer.ScrollPositionChanged -= HandleImageViewerScrollPositionsChanged;
                _imageViewer.PageChanged -= HandleImageViewerPageChanged;
                _imageViewer.FitModeChanged -= HandleImageViewerFitModeChanged;
                if (_imageViewer.CursorTool == CursorTool.SelectLayerObject)
                {
                    _imageViewer.CursorEnteredLayerObject -= HandleCursorEnteredLayerObject;
                    _imageViewer.CursorLeftLayerObject -= HandleCursorLeftLayerObject;
                }
            }

            _dataEntryApp.ShowAllHighlightsChanged -= HandleShowAllHighlightsChanged;
        }

        /// <summary>
        /// Implements ability to dump all control properties to a text
        /// file. All child controls will be recursively dumped as well (including the rows/columns
        /// of and DataGridViews).
        /// https://extract.atlassian.net/browse/ISSUE-13640
        /// </summary>
        /// <param name="m">The <see cref="Message"/> to be processed as a potential start of a
        /// property dump drag/drop operation.</param>
        void ProcessPropertyDumpDragDrop(Message m)
        {
            // Get the position of the mouse in screen coordinates.
            Point mousePosition = new Point((int)((uint)m.LParam & 0x0000FFFF),
                                            (int)((uint)m.LParam & 0xFFFF0000) >> 16);

            // Mouse + Ctrl down within ClientRectangle
            if (m.Msg == WindowsMessage.LeftButtonDown &&
                Control.ModifierKeys.HasFlag(Keys.Control) &&
                ClientRectangle.Contains(mousePosition))
            {
                if (_propertyDumpTarget == null)
                {
                    _propertyDumpTarget = FormsMethods.GetClickedControl(m);
                }
            }
            else if (m.Msg == WindowsMessage.LeftButtonUp)
            {
                _propertyDumpTarget = null;
            }
            else if (m.Msg == WindowsMessage.MouseMove)
            {
                // If _propertyDumpTarget has been assigned by mouse down and the mouse is no longer
                // within ClientRectangle, start a drag drop operation.
                if (_propertyDumpTarget != null && !ClientRectangle.Contains(mousePosition))
                {
                    string propertyListing = _propertyDumpTarget.GetPropertyListing();

                    // Create a DataObject to represent the property dump either as a string or a
                    // file.
                    var dragData = new DataObject();
                    dragData.SetText(propertyListing);
                    using (var tempFile = new TemporaryFile(
                        null, _propertyDumpTarget.Name + ".txt", null, false))
                    {
                        File.WriteAllText(tempFile.FileName, propertyListing);

                        var dragFileCollection = new StringCollection();
                        dragFileCollection.Add(tempFile.FileName);
                        dragData.SetFileDropList(dragFileCollection);

                        DoDragDrop(dragData, DragDropEffects.Copy);
                        _propertyDumpTarget = null;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the case that an ancestor control has been added or removed to the
        /// current chain of ancestor controls. Event registration is updated to prevent
        /// handling of attribute changes when the DEP is not loaded into a verification UI.
        /// </summary>
        void ProcessAncestorChange()
        {
            UpdateAnscestors();

            if (TopLevelControl == null)
            {
                // Don't allow PreFilterMessage to be called when the DEP is not loaded.
                Application.RemoveMessageFilter(this);

                AttributeStatusInfo.AttributeInitialized -= HandleAttributeInitialized;
                AttributeStatusInfo.ViewedStateChanged -= HandleViewedStateChanged;
                AttributeStatusInfo.ValidationStateChanged -= HandleValidationStateChanged;
            }
            else
            {
                // So that PreFilterMessage is called
                Application.AddMessageFilter(this);

                AttributeStatusInfo.AttributeInitialized += HandleAttributeInitialized;
                AttributeStatusInfo.ViewedStateChanged += HandleViewedStateChanged;
                AttributeStatusInfo.ValidationStateChanged += HandleValidationStateChanged;
            }
        }

        List<Control> _ancestors = new List<Control>();

        /// <summary>
        /// Updates the WinForm controls known to be ancestors to this DEP and updates
        /// <see cref="Control.ParentChanged"/> event registrations to be notified when the 
        /// ancestor chain is updated again.
        /// </summary>
        void UpdateAnscestors()
        {
            var ancestors = new List<Control>();
            for (var ancestor = Parent; ancestor != null; ancestor = ancestor.Parent)
            {
                ancestors.Add(ancestor);
            }

            var oldAncestors = _ancestors.Except(ancestors);
            foreach (var oldAncestor in oldAncestors)
            {
                oldAncestor.ParentChanged -= HandleAncestor_ParentChanged;
            }

            var newAncestors = ancestors.Except(_ancestors);
            foreach (var newAncestor in newAncestors)
            {
                newAncestor.ParentChanged += HandleAncestor_ParentChanged;
            }

            _ancestors = ancestors;
        }

        /// <summary>
        /// Handles the case that the parent to one of this panel's ancestors has changed.
        /// This may occur when the panel is added or removed from the verification application form.
        /// </summary>
        void HandleAncestor_ParentChanged(object sender, EventArgs e)
        {
            try
            {
                ProcessAncestorChange();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49856");
            }
        }

        /// <summary>
        /// Removes the highlights, tooltip and validation icons for the active control from the image viewer
        /// </summary>
        void RemoveActiveAttributeHighlights()
        {
            try
            {
                foreach (var attr in GetActiveAttributes())
                {
                    ShowAttributeHighlights(attr, false);
                    RemoveAttributeToolTip(attr);
                }
                ImageViewer?.Invalidate();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50239");
            }
        }

        #endregion Private Members
    }
}
