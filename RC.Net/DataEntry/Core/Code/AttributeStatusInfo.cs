using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Specifies whether an <see cref="IAttribute"/> is a hint and, if so, what type of
    /// hint that it is.
    /// </summary>
    public enum HintType
    {
        /// <summary>
        /// The <see cref="IAttribute"/> is truly spatial (not a hint).
        /// </summary>
        None = 0,

        /// <summary>
        /// The <see cref="IAttribute"/> is a direct hint (specifies the region one would expect to
        /// find the data related to this hint.
        /// </summary>
        Direct = 1,

        /// <summary>
        /// The <see cref="IAttribute"/> is an indirect hint (specifies spatial clues that will help
        /// determine where the <see cref="IAttribute"/>'s data may be found, but not necessarily
        /// where one would expect to actually find the data).
        /// </summary>
        Indirect = 2
    }

    /// <summary>
    /// Specifies under what circumstances an <see cref="IAttribute"/> should be included in the tab
    /// order.
    /// </summary>
    public enum TabStopMode
    {
        /// <summary>
        /// The <see cref="IAttribute"/> should always be in the tab order (assuming it is viewable).
        /// </summary>
        Always = 0,

        /// <summary>
        /// The <see cref="IAttribute"/> should only be included in the tab order if it is populated
        /// with a non-empty value or its data is marked as invalid.
        /// </summary>
        OnlyWhenPopulatedOrInvalid = 1,

        /// <summary>
        /// The <see cref="IAttribute"/> should only be included in the tab order if its data is
        /// marked as invalid (regardless of whether the value is non-empty).
        /// </summary>
        OnlyWhenInvalid = 2,

        /// <summary>
        /// The <see cref="IAttribute"/> should never be included in the tab order.
        /// </summary>
        Never = 3
    }

    /// <summary>
    /// Specifies whether a particular data element is valid or not.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    [Flags]
    public enum DataValidity
    {
        /// <summary>
        /// The data is valid.
        /// </summary>
        Valid = 1,

        /// <summary>
        /// The data is invalid.
        /// </summary>
        Invalid = 2,

        /// <summary>
        /// The data is suspect; there is reason to believe it is not valid.
        /// </summary>
        ValidationWarning = 4
    }

    /// <summary>
    /// An object that represents the current state of a particular <see cref="IAttribute"/>
    /// This includes whether the attribute's data has been validated, fully propagated and whether it
    /// has been viewed by the user. The object is intended to occupy the 
    /// IAttribute.DataObject field of the <see cref="IAttribute"/> it is associated
    /// with.
    /// </summary>
    [Guid("BC86C004-F2C7-4a90-80F7-F6C49B201AD4")]
    [ProgId("Extract.DataEntry.AttributeStatusInfo")]
    [ComVisible(true)]
    public partial class AttributeStatusInfo : IPersistStream, ICopyableObject
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(AttributeStatusInfo).ToString();

        /// <summary>
        /// The current version of this object.
        /// <para><b>Versions 2:</b></para>
        /// Added persistence of _isAccepted.
        /// <para><b>Versions 3:</b></para>
        /// Added persistence of _hintEnabled.
        /// </summary>
        const int _CURRENT_VERSION = 3;

        #endregion Constants

        #region Static fields

        /// <summary>
        /// The filename of the currently open document.
        /// </summary>
        [ThreadStatic]
        static string _sourceDocName;

        /// <summary>
        /// Used to expand path tags.
        /// </summary>
        [ThreadStatic]
        static IPathTags _pathTags;

        /// <summary>
        /// The active attribute hierarchy.
        /// </summary>
        [ThreadStatic]
        static IUnknownVector _attributes;

        /// <summary>
        /// The current base <see cref="ExecutionContext"/> for use in enforcing the
        /// ExecutionExemptions attribute for <see cref="DataEntryQuery"/>s.
        /// https://extract.atlassian.net/browse/ISSUE-15342
        /// </summary>
        [ThreadStatic]
        static ExecutionContext _queryExecutionContext;

        /// <summary>
        /// A database(s) available for use in validation or auto-update queries; The key is the
        /// connection name (blank for default connection).
        /// </summary>
        [ThreadStatic]
        static Dictionary<string, DbConnection> _dbConnections;

        /// <summary>
        /// Caches the info object for each <see cref="IAttribute"/> for quick reference later on.
        /// </summary>
        [ThreadStatic]
        static Dictionary<IAttribute, AttributeStatusInfo> _statusInfoMap;

        /// <summary>
        /// A dictionary that keeps track of which attribute collection each attribute belongs to.
        /// Used to help in assigning _parentAttribute fields.
        /// </summary>
        [ThreadStatic]
        static Dictionary<IUnknownVector, IAttribute> _subAttributesToParentMap;

        /// <summary>
        /// A dictionary of auto-update triggers that exist on the attributes stored in the keys of
        /// this dictionary.
        /// </summary>
        [ThreadStatic]
        static Dictionary<IAttribute, AutoUpdateTrigger> _autoUpdateTriggers;

        /// <summary>
        /// A dictionary of validation triggers that exist on the attributes stored in the keys of
        /// this dictionary.
        /// </summary>
        [ThreadStatic]
        static Dictionary<IAttribute, AutoUpdateTrigger> _validationTriggers;

        /// <summary>
        /// Keeps track of the attributes that have been modified since the last time EndEdit was
        /// called. Each modified attribute is assigned a KeyValuePair which keeps track of whether
        /// the spatial information has changed and what the original attribute value was in case
        /// it needs to be reverted.
        /// </summary>
        [ThreadStatic]
        static Dictionary<IAttribute, KeyValuePair<bool, SpatialString>> _attributesBeingModified;

        /// <summary>
        /// Keeps track of the attributes that have been modified since the last time EndEdit was
        /// called for the purposes of ensuring all are re-validated based on the updated value.
        /// </summary>
        [ThreadStatic]
        static HashSet<IAttribute> _attributesToValidate;

        /// <summary>
        /// Prevents recursion of EndEdit while <see cref="_attributesBeingModified"/> is being
        /// processed.
        /// </summary>
        [ThreadStatic]
        static bool _endEditRecursionBlock;

        /// <summary>
        /// Keeps track of current number of nested calls into EndEdit (should never be > 2).
        /// </summary>
        [ThreadStatic]
        static int _endEditReferenceCount;

        /// <summary>
        /// Specifies whether validation triggers are currently enabled.
        /// </summary>
        [ThreadStatic]
        static bool _validationTriggersEnabled;

        /// <summary>
        /// Manages change history and provides ability to undo changes.
        /// </summary>
        [ThreadStatic]
        static UndoManager _undoManager;

        /// <summary>
        /// Indicates whether auto-update queries should be disabled. The queries will not be loaded
        /// for any attributes.
        /// </summary>
        [ThreadStatic]
        static bool _disableAutoUpdateQueries;

        /// <summary>
        /// Indicates whether validation queries should be disabled.
        /// </summary>
        [ThreadStatic]
        static bool _disableValidationQueries;

        /// <summary>
        /// Indicates whether auto-update queries should be temporarily prevented from updating text.
        /// The queries will still be loaded for all attributes, but they will not be triggered
        /// while blocked. Un-blocking the queries will not execute the queries that would have been
        /// triggered while blocked.
        /// Queries that are using the <see cref="DataEntryQuery.TargetProperty"/> property to
        /// update something other than the control's text/value will continue to execute.
        /// </summary>
        [ThreadStatic]
        static bool _blockAutoUpdateQueries;

        /// <summary>
        /// Indicates whether auto-update and validation queries should be temporarily prevented
        /// from updating data. The queries will still be loaded for all attributes, but they will
        /// not be triggered until un-paused, at which point all queries that would have been
        /// triggered while paused will executed.
        /// </summary>
        [ThreadStatic]
        static bool _pauseQueries;

        /// <summary>
        /// An instance of the <see cref="T:Logger"/> class used to log input and events to the data
        /// entry verification UI.
        /// </summary>
        [ThreadStatic]
        static Logger _logger;

        /// <summary>
        /// Indicates whether the current thread is about to end; This can be used to prevent
        /// unnecessary operations from running.
        /// </summary>
        [ThreadStatic]
        static bool _threadEnding;

        /// <summary>
        /// Indicates whether cached data should be be shared with with other threads in this process
        /// (for the purpose of background loading efficiency).
        /// </summary>
        [ThreadStatic]
        static bool _processWideDataCache;

        /// <summary>
        /// Registered event handlers for the <see cref="DataReset"/> event.
        /// </summary>
        static ThreadSpecificEventHandler<EventArgs> _dataResetHandler =
            new ThreadSpecificEventHandler<EventArgs>();

        /// <summary>
        /// Registered event handlers for the <see cref="QueryCacheCleared"/> event.
        /// </summary>
        static ThreadSpecificEventHandler<EventArgs> _queryCacheClearHandler =
            new ThreadSpecificEventHandler<EventArgs>();

        /// <summary>
        /// Registered event handlers for the <see cref="AttributeInitialized"/> event.
        /// </summary>
        static ThreadSpecificEventHandler<AttributeInitializedEventArgs> _attributeInitializedHandler =
            new ThreadSpecificEventHandler<AttributeInitializedEventArgs>();

        /// <summary>
        /// Registered event handlers for the <see cref="ViewedStateChanged"/> event.
        /// </summary>
        static ThreadSpecificEventHandler<ViewedStateChangedEventArgs> _viewedStateChangedHandler =
            new ThreadSpecificEventHandler<ViewedStateChangedEventArgs>();

        /// <summary>
        /// Registered event handlers for the <see cref="ValidationStateChanged"/> event.
        /// </summary>
        static ThreadSpecificEventHandler<ValidationStateChangedEventArgs> _validationStateChangedHandler =
            new ThreadSpecificEventHandler<ValidationStateChangedEventArgs>();

        /// <summary>
        /// Registered event handlers for the <see cref="EditEnded"/> event.
        /// </summary>
        static ThreadSpecificEventHandler<EventArgs> _editEndedHandler =
            new ThreadSpecificEventHandler<EventArgs>();

        /// <summary>
        /// Registered event handlers for the <see cref="QueryDelayEnded"/> event.
        /// </summary>
        static ThreadSpecificEventHandler<EventArgs> _queryDelayEndedHandler =
            new ThreadSpecificEventHandler<EventArgs>();

        #endregion static fields

        #region Instance fields

        /// <summary>
        /// Indicates whether the object has been modified since being loaded via the 
        /// IPersistStream interface. This is an int because that is the return type of 
        /// IPersistStream::IsDirty in order to support COM values of <see cref="HResult.Ok"/> and 
        /// <see cref="HResult.False"/>.
        /// </summary>
        int _dirty;

        /// <summary>
        /// The control in charge of displaying the attribute.
        /// </summary>
        IDataEntryControl _owningControl;

        /// <summary>
        /// The template to use to create new per-instance validator instances.
        /// </summary>
        IDataEntryValidator _validatorTemplate;

        /// <summary>
        /// The validator used to validate the attribute's data.
        /// </summary>
        IDataEntryValidator _validator;

        /// <summary>
        /// Indicates whether the user has viewed the attribute's data.
        /// </summary>
        bool _hasBeenViewed;

        /// <summary>
        /// Whether this attribute has been propagated (i.e., its children have been mapped to
        /// any dependent child controls.
        /// </summary>
        bool _hasBeenPropagated;

        /// <summary>
        /// <see langword="true"/> if the attribute's data is viewable in the DEP, 
        /// <see langword="false"/> otherwise.
        /// </summary>
        bool _isViewable;

        /// <summary>
        /// A <see cref="DataValidity"/> value indicating whether the attribute's value is valid.
        /// </summary>
        DataValidity _dataValidity = DataValidity.Valid;

        /// <summary>
        /// <see langword="true"/> if data validation should be performed on the attribute,
        /// <see langword="false"/> if the attribute should always be considered valid.
        /// </summary>
        bool _validationEnabled = true;

        /// <summary>
        /// A string that allows attributes to be sorted by compared to other display order values.
        /// </summary>
        string _displayOrder;

        /// <summary>
        /// Specifies whether the <see cref="IAttribute"/> is a hint and, if so, what type
        /// of hint that it is.
        /// </summary>
        HintType _hintType;

        /// <summary>
        /// Specifies the spatial area that defines a hint.
        /// </summary>
        IEnumerable<Extract.Imaging.RasterZone> _hintRasterZones;

        /// <summary>
        /// Specifies whether the user has accepted the highlight associated with the attribute.
        /// </summary>
        bool _isAccepted;

        /// <summary>
        /// Specifies whether hints are enabled for the attribute. The fact that hints are enabled
        /// doesn't necessarily mean the attribute has one.
        /// </summary>
        bool _hintEnabled = true;

        /// <summary>
        /// Specifies the parent of this attribute (if one exists).
        /// </summary>
        IAttribute _parentAttribute;

        /// <summary>
        /// A query which will cause the attribute's value to automatically be updated using values
        /// from other attributes and/or a database query.
        /// </summary>
        string _autoUpdateQuery;

        /// <summary>
        /// A query which will cause the validation list to be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        string _validationQuery;

        /// <summary>
        /// Keeps track of if an AttributeValueModified value event is currently being raised to
        /// prevent recursion via autoUpdateQueries.
        /// </summary>
        bool _raisingAttributeValueModified;

        /// <summary>
        /// Specifies under what circumstances the attribute should serve as a tab stop.
        /// </summary>
        TabStopMode _tabStopMode = TabStopMode.Always;

        /// <summary>
        /// Specifies whether the attribute should be persisted in output.
        /// </summary>
        bool _persistAttribute = true;

        /// <summary>
        /// A list of <see cref="IAttribute"/>s that are contained within a group owned by this
        /// <see cref="IAttribute"/>.  Will be <see langword="null"/> for attributes in controls
        /// that don't support tab groups or an empty list for attributes that don't own a group
        /// in a control that supports tab groups.
        /// </summary>
        List<IAttribute> _tabGroup;

        /// <summary>
        /// Keeps track of the last string value applied to this attribute programmatically (on load
        /// or via an auto-update query). Used to help ensure the correct value gets applied to
        /// fields that may not yet be ready to accept the value (i.e. combo boxes where the list of
        /// possible values has not yet been set/updated).
        /// </summary>
        string _lastAppliedStringValue;

        /// <summary>
        /// Indicates whether this attribute has been mapped to a control.
        /// </summary>
        bool _isMapped;

        /// <summary>
        /// Indicates whether the attribute has been initialized and has not been deleted.
        /// </summary>
        bool _initialized;

        #endregion Instance fields

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="AttributeStatusInfo"/> instance.  This constructor should never
        /// be called directly by an outside class except via the <see cref="IPersistStream"/>
        /// COM interface.
        /// </summary>
        public AttributeStatusInfo()
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
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI24485", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24493", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets a value indicating whether field has been mapped to a control
        /// </summary>
        public bool IsMapped
        {
            get
            {
                return _isMapped;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the attribute has been initialized and has not been
        /// deleted.
        /// </summary>
        /// <value><see langword="true"/> if this attribute is initialized and has not been deleted;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool IsInitialized
        {
            get
            {
                return _initialized;
            }
        }

        /// <summary>
        /// Gets the <see cref="IDataEntryControl"/> in charge of displaying the associated
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <value>The <see cref="IDataEntryControl"/> in charge of displaying the associated
        /// <see cref="IAttribute"/>.</value>
        /// <returns>The <see cref="IDataEntryControl"/> in charge of displaying the associated
        /// <see cref="IAttribute"/>.</returns>
        public IDataEntryControl OwningControl
        {
            get
            {
                return _owningControl;
            }

            set
            {
                _owningControl = value;
                if (value != null)
                {
                    _isMapped = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IDataEntryValidator"/> used to validate the associated 
        /// <see cref="IAttribute"/>'s data.
        /// </summary>
        /// <returns>The <see cref="IDataEntryValidator"/> used to validate the associated 
        /// <see cref="IAttribute"/>'s data.</returns>
        public IDataEntryValidator Validator
        {
            get
            {
                return _validator;
            }
        }

        /// <summary>
        /// Specifies under what circumstances the <see cref="IAttribute"/> should serve as a tab
        /// stop.
        /// </summary>
        /// <value>A <see cref="TabStopMode"/> value indicating when the attribute should serve as a
        /// tab stop.</value>
        /// <returns>A <see cref="TabStopMode"/> value indicating when the attribute will serve as a
        /// tab stop.</returns>
        public TabStopMode TabStopMode
        {
            get
            {
                return _tabStopMode;
            }

            set
            {
                _tabStopMode = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the attribute should be persisted in output.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="IAttribute"/> should be persisted in
        /// output; <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if the <see cref="IAttribute"/> will be persisted in
        /// output; <see langword="false"/> otherwise.</returns>
        public bool PersistAttribute
        {
            get
            {
                return _persistAttribute;
            }

            set
            {
                _persistAttribute = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the attribute should be persisted in output.
        /// NOTE: Unlike the HasBeenViewedOrIsNotViewable method, this property does not take into
        /// account whether the attribute is viewable; the raw field value is returned here.
        /// </summary>
        public bool HasBeenViewed
        {

            get
            {
                return _hasBeenViewed;
            }

            set
            {
                _hasBeenViewed = value;
            }
        }


        /// <summary>
        /// Gets or sets a <see langword="string"/> that allows <see cref="IAttribute"/>s to be
        /// sorted by compared to other <see cref="IAttribute"/>'s <see cref="DisplayOrder"/>values.
        /// <see cref="IAttribute"/>s will be sorted from lowest to highest using the result of
        /// <see cref="String.Compare(string, string, StringComparison)"/>.
        /// </summary>
        /// <value>A <see langword="string"/> that allows <see cref="IAttribute"/>s to be
        /// sorted by compared to other <see cref="IAttribute"/>'s <see cref="DisplayOrder"/>values.
        /// </value>
        /// <returns>A <see langword="string"/> that allows <see cref="IAttribute"/>s to be
        /// sorted by compared to other <see cref="IAttribute"/>'s <see cref="DisplayOrder"/>values.
        /// </returns>
        public string DisplayOrder
        {
            get
            {
                return _displayOrder;
            }

            set
            {
                _displayOrder = value;
            }
        }

        /// <summary>
        /// A query which will cause the <see cref="IAttribute"/>'s value to automatically be
        /// updated using values from other <see cref="IAttribute"/>s and/or a database query.
        /// </summary>
        public string AutoUpdateQuery
        {
            get
            {
                return _autoUpdateQuery;
            }

            set
            {
                _autoUpdateQuery = value;
            }
        }

        /// <summary>
        /// A query which will cause the <see cref="IAttribute"/>'s validation list to be
        /// automatically updated using values from other <see cref="IAttribute"/>s and/or a
        /// database query.
        /// </summary>
        public string ValidationQuery
        {
            get
            {
                return _validationQuery;
            }

            set
            {
                _validationQuery = value;
            }
        }

        /// <summary>
        /// Gets or sets a programmatically applied attribute value (on load or via an auto-update
        /// query) that should be ensured after validation queries have all been applied. Used to
        /// address fields that may not yet be ready to accept the value (i.e. combo boxes where
        /// the list of possible values has not yet been set/updated).
        /// </summary>
        /// <value>
        /// The last string value applied to this attribute programmatically.
        /// </value>
        public string LastAppliedStringValue
        {
            get
            {
                return _lastAppliedStringValue;
            }

            set
            {
                _lastAppliedStringValue = value;
            }
        }

        /// <summary>
        /// Indicates whether an <see cref="EndEdit"/> call is currently being processed.
        /// </summary>
        /// <returns><see langword="false"/> if an EndEdit call is currently being processed; otherwise,
        /// <see langword="false"/>.</returns>
        public static bool EndEditInProgress
        {
            get
            {
                return _endEditReferenceCount > 0;
            }
        }

        #endregion Properties

        #region Static Members

        /// <summary>
        /// Gets <see cref="UndoManager"/> which tracks change history and provides ability to undo
        /// changes.
        /// </summary>
        /// <value>The <see cref="UndoManager"/>.</value>
        public static UndoManager UndoManager
        {
            get
            {
                if (_undoManager == null)
                {
                    InitializeStatics();
                }

                return _undoManager;
            }
        }

        /// <summary>
        /// Gets or set an instance of the <see cref="T:Logger"/> class used to log input and events
        /// to the data entry verification UI.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="T:Logger"/> class used to log input and events to the data
        /// entry verification UI. <see langword="null"/> if logging is not active.
        /// </value>
        public static Logger Logger
        {
            get
            {
                return _logger;
            }

            set
            {
                _logger = value;
            }
        }

        /// <summary>
        /// Gets the filename of the currently open document.
        /// </summary>
        /// <returns>The filename of the currently open document.</returns>
        [ComVisible(false)]
        public static string SourceDocName
        {
            get
            {
                return _sourceDocName;
            }
        }

        /// <summary>
        /// Gets an <see cref="IPathTags"/> instance to expands path tags.
        /// </summary>
        /// <returns>An <see cref="IPathTags"/> instance.</returns>
        [ComVisible(false)]
        public static IPathTags PathTags
        {
            get
            {
                return _pathTags;
            }
        }

        /// <summary>
        /// Gets the current base <see cref="ExecutionContext"/> for use in enforcing the
        /// ExecutionExemptions attribute for <see cref="DataEntryQuery"/>s.
        /// https://extract.atlassian.net/browse/ISSUE-15342
        /// </summary>
        public static ExecutionContext QueryExecutionContext
        {
            get
            {
                return _queryExecutionContext;
            }

            set
            {
                _queryExecutionContext = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto-update queries should be disabled.
        /// The queries will not be loaded for any attributes.
        /// </summary>
        /// <value><see langword="true"/> to disable auto-update queries; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public static bool DisableAutoUpdateQueries
        {
            get
            {
                return _disableAutoUpdateQueries;
            }

            set
            {
                _disableAutoUpdateQueries = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto-update queries should be temporarily
        /// prevented from updating text. The queries will still be loaded for all attributes, but
        /// they will not be triggered while blocked. Un-blocking the queries will not execute the
        /// queries that would have been triggered while blocked.
        /// Queries that are using the <see cref="DataEntryQuery.TargetProperty"/> property to
        /// update something other than the control's text/value will continue to execute.
        /// </summary>
        /// <value><see langword="true"/> to block auto-update queries; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public static bool BlockAutoUpdateQueries
        {
            get
            {
                return _blockAutoUpdateQueries;
            }

            set
            {
                _blockAutoUpdateQueries = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto-update and validation queries should be
        /// temporarily prevented from updating data. The queries will still be loaded for all
        /// attributes, but they will not be triggered until un-paused, at which point all queries
        /// that would have been triggered while paused will executed.
        /// </summary>
        /// <value><see langword="true"/> to pause auto-update queries; <see langword="false"/> to
        /// resume.
        /// </value>
        public static bool PauseQueries
        {
            get
            {
                return _pauseQueries;
            }

            set
            {
                try
                {
                    if (value != _pauseQueries)
                    {
                        _pauseQueries = value;
                        if (!_pauseQueries)
                        {
                            OnQueryDelayEnded();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37926");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether validation queries should be disabled.
        /// </summary>
        /// <value><see langword="true"/> to disable validation queries; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        public static bool DisableValidationQueries
        {
            get
            {
                return _disableValidationQueries;
            }

            set
            {
                _disableValidationQueries = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current thread is about to end; This can
        /// be used to prevent unnecessary operations from running.
        /// <b><para>Note</para></b>
        /// This value can only be set to <c>true</c> not <c>false</c>.
        /// </summary>
        /// <value><c>true</c> if the thread is ending; otherwise, <c>false</c>.
        /// </value>
        [ComVisible(false)]
        public static bool ThreadEnding
        {
            get
            {
                return _threadEnding;
            }

            set
            {
                ExtractException.Assert("ELI44740", "Cannot clear ThreadEnding status.", value);
                _threadEnding = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether cached data should be be shared with with other
        /// threads in this process (for the purpose of background loading efficiency).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [block automatic update queries]; otherwise, <c>false</c>.
        /// </value>
        public static bool ProcessWideDataCache
        {
            get
            {
                return _processWideDataCache;
            }

            set
            {
                _processWideDataCache = value;
            }
        }

        /// <summary>
        /// Indicates if logging is currently enabled for the specified <see paramref="category"/>.
        /// </summary>
        /// <param name="category">The <see cref="LogCategories"/> to check for whether logging is
        /// enabled.</param>
        /// <returns><see langword="true"/> if logging is enabled for the specified category;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        [ComVisible(false)]
        public static bool IsLoggingEnabled(LogCategories category)
        {
            return (Logger != null && Logger.LogCategories.HasFlag(category));
        }

        /// <summary>
        /// Returns the <see cref="AttributeStatusInfo"/> object associated with the provided
        /// <see cref="IAttribute"/>.  A new <see cref="AttributeStatusInfo"/> instance is
        /// created if necessary.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which an 
        /// <see cref="AttributeStatusInfo"/> instance is needed.</param>
        /// <returns>The <see cref="AttributeStatusInfo"/> instance associated with the provided
        /// <see cref="IAttribute"/>.</returns>
        [ComVisible(false)]
        public static AttributeStatusInfo GetStatusInfo(IAttribute attribute)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI26109", _OBJECT_NAME);

                InitializeStatics();

                ExtractException.Assert("ELI29196", "Null attribute exception!", attribute != null);

                AttributeStatusInfo statusInfo;
                if (!_statusInfoMap.TryGetValue(attribute, out statusInfo))
                {
                    statusInfo = attribute.DataObject as AttributeStatusInfo;

                    if (statusInfo == null)
                    {
                        statusInfo = new AttributeStatusInfo();
                        attribute.DataObject = statusInfo;
                    }

                    _statusInfoMap[attribute] = statusInfo;
                }

                return statusInfo;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24477", ex);
            }
        }

        /// <summary>
        /// Clears the internal cache used for efficient lookups of <see cref="AttributeStatusInfo"/>
        /// objects. This should be every time new data is loaded and called with
        /// <see langword="null"/> every time a document is closed (or <see cref="IAttribute"/>s are
        /// otherwise unloaded).
        /// </summary>
        /// <param name="sourceDocName">The name of the currently open document.</param>
        /// <param name="attributes">The active <see cref="IAttribute"/> hierarchy.</param>
        /// <param name="dbConnection">Any database available for use in validation or
        /// auto-update queries; (Can be <see langword="null"/> if not required).</param>
        [ComVisible(false)]
        public static void ResetData(string sourceDocName, IUnknownVector attributes,
            DbConnection dbConnection)
        {
            var dbConnections = new Dictionary<string, DbConnection>();
            dbConnections[""] = dbConnection;

            ResetData(sourceDocName, attributes, dbConnections, null);
        }

        /// <summary>
        /// Clears the internal cache used for efficient lookups of <see cref="AttributeStatusInfo"/>
        /// objects. This should be every time new data is loaded and called with
        /// <see langword="null"/> every time a document is closed (or <see cref="IAttribute"/>s are
        /// otherwise unloaded).
        /// </summary>
        /// <param name="sourceDocName">The name of the currently open document.</param>
        /// <param name="attributes">The active <see cref="IAttribute"/> hierarchy.</param>
        /// <param name="dbConnections">Any database(s) available for use in validation or
        /// auto-update queries; The key is the connection name (blank for default connection).
        /// (Can be <see langword="null"/> if not required).</param>
        /// <param name="pathTags">An <see cref="IPathTags"/> instance to be used to expand tags if
        /// anything other than the SourceDocName tag is needed; Otherwise, <see langword="null"/>.
        /// </param>
        [ComVisible(false)]
        public static void ResetData(string sourceDocName, IUnknownVector attributes,
            Dictionary<string, DbConnection> dbConnections, IPathTags pathTags)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI26133", _OBJECT_NAME);

                InitializeStatics();

                string traceFileName = "";
                try
                {
                    if (Logger != null && Logger.LogToMemory &&
                        !string.IsNullOrWhiteSpace(_sourceDocName))
                    {
                        // Generate the trace output filename.
                        for (int i = 0; i < 8; i++)
                        {
                            string format = "yyyy-MM-dd HH-mm-ss";
                            // If the previous name was taken, add more precision to the datetime
                            // stamp.
                            if (i > 0)
                            {
                                format += "." + new string('f', i);
                            }
                            traceFileName = _sourceDocName + "." +
                                DateTime.Now.ToString(format, CultureInfo.InvariantCulture) + ".trace";
                            if (!File.Exists(traceFileName))
                            {
                                // This filename is unused; use it.
                                break;
                            }
                        }

                        Logger.SaveLoggedData(traceFileName);
                        Logger.ClearLoggedData();
                    }
                }
                catch (Exception ex)
                {
                    var ee = new ExtractException("ELI38344", "Failed to output trace log.", ex);
                    ee.AddDebugData("Log filename", traceFileName, false);
                    ee.Log();
                }

                // Make sure _all_ the attributes are released
                ReleaseAttributes(_statusInfoMap.Keys.ToIUnknownVector<IAttribute>());

                // Starting now query executions should be considered to be in the context of a
                // document load. This context will only change once the DataEntryControlHost has
                // finished initializing the DEP, at which point the context will be changed to
                // OnUpdate.
                _queryExecutionContext = ExecutionContext.OnLoad;
                _statusInfoMap.Clear();
                _subAttributesToParentMap.Clear();
                _attributesBeingModified.Clear();
                _attributesToValidate.Clear();
                _endEditReferenceCount = 0;
                _endEditRecursionBlock = false;
                if (pathTags != null)
                {
                    _pathTags = pathTags;
                }
                else
                {
                    _pathTags = (string.IsNullOrEmpty(sourceDocName))
                        ? new FileActionManagerPathTags()
                        : new FileActionManagerPathTags(null, sourceDocName);
                }

                // Ensure data entry queries no longer react to changes in the attribute hierarchy.
                AttributeQueryNode.UnregisterAll();

                foreach (AutoUpdateTrigger autoUpdateTrigger in _autoUpdateTriggers.Values)
                {
                    autoUpdateTrigger.Dispose();
                }
                _autoUpdateTriggers.Clear();

                foreach (AutoUpdateTrigger validationTrigger in _validationTriggers.Values)
                {
                    validationTrigger.Dispose();
                }
                _validationTriggers.Clear();

                DataEntryQuery.DisposeDeclarations();

                // _undoManager.ClearHistory() call will release attributes that were deleted by the
                // user (and thus are no longer in the _attributes hierarchy), so don't call it until
                // the end of this ResetData.
                _undoManager.ClearHistory();

                // Release the attributes still in the _attributes hierarchy.
                if (_attributes != null)
                {
                    ReleaseAttributes(_attributes);
                }

                _attributes = attributes;
                _sourceDocName = sourceDocName;
                _dbConnections = dbConnections;

                OnDataReset();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25624", ex);
            }
        }

        /// <summary>
        /// Initializes for query (includes resetting the current threads AttributeStatusInfo data).
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
        /// to initialize.</param>
        /// <param name="sourceDocName">The source document name to which the attributes are
        /// affiliated with.</param>
        /// <param name="dbConnection">The database to use for the queries. (Can be
        /// <see langword="null"/> if not required).</param>
        [ComVisible(false)]
        public static void InitializeForQuery(IUnknownVector attributes, string sourceDocName,
            DbConnection dbConnection)
        {
            try
            {
                var dbConnections = new Dictionary<string, DbConnection>();
                dbConnections[""] = dbConnection;

                AttributeStatusInfo.ResetData(sourceDocName, attributes, dbConnections, null);
                Initialize(attributes);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37785");
            }
        }

        /// <summary>
        /// Loads the specified attributes in to the <see paramref="fieldModels"/> to enable the
        /// data to be transformed as they would be they UI without loading into the UI.
        /// <para><b>Note</b></para>
        /// While this results in all queries (both auto-update and validation) executing, it does
        /// not explicitly validate all fields so, for example, ValidationPatterns will not have
        /// been considered.
        /// Also, while this call re-orders attributes, it does not prune any unmapped or
        /// un-persistable attributes.
        /// </summary>
        /// <param name="attributes">The attributes to load.</param>
        /// <param name="sourceDocName">Name of the source document.</param>
        /// <param name="dbConnections">The database(s) to use for the queries. (Can be
        /// <see langword="null"/> if not required).</param>
        /// <param name="fieldModels">The <see cref="BackgroundFieldModel"/>s that represent
        /// the controls in the DEP.</param>
        /// <param name="pathTags">The <see cref="IPathTags"/> instance that should be used to
        /// expand any path tag expressions in data queries.</param>
        [ComVisible(false)]
        public static void ExecuteNoUILoad(IUnknownVector attributes, string sourceDocName,
            Dictionary<string, DbConnection> dbConnections, IEnumerable<BackgroundFieldModel> fieldModels,
            IPathTags pathTags)
        {
            try
            {
                AttributeStatusInfo.ResetData(sourceDocName, attributes, dbConnections, pathTags);
                EnableValidationTriggers(false);
                Initialize(attributes, fieldModels);
                EnableValidationTriggers(true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37785");
            }
        }

        /// <summary>
        /// Initializes for query (includes resetting the current threads AttributeStatusInfo data).
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
        /// to initialize.</param>
        /// <param name="sourceDocName">The source document name to which the attributes are
        /// affiliated with.</param>
        /// <param name="dbConnection">The database to use for the queries. (Can be
        /// <see langword="null"/> if not required).</param>
        /// <param name="pathTags">The <see cref="IPathTags"/> to use if anything more than the
        /// SourceDocName is needed for the expansion; otherwise, <see langword="null"/>.</param>
        [ComVisible(false)]
        public static void InitializeForQuery(IUnknownVector attributes, string sourceDocName,
            DbConnection dbConnection, IPathTags pathTags)
        {
            try
            {
                var dbConnections = new Dictionary<string, DbConnection>();
                dbConnections[""] = dbConnection;

                AttributeStatusInfo.ResetData(sourceDocName, attributes, dbConnections, pathTags);
                Initialize(attributes);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37786");
            }
        }

        /// <summary>
        /// Initializes for query (includes resetting the current threads AttributeStatusInfo data).
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
        /// to initialize.</param>
        /// <param name="sourceDocName">The source document name to which the attributes are
        /// affiliated with.</param>
        /// <param name="dbConnections">The database(s) to use for the queries. (Can be
        /// <see langword="null"/> if not required).</param>
        /// <param name="pathTags">The <see cref="IPathTags"/> to use if anything more than the
        /// SourceDocName is needed for the expansion; otherwise, <see langword="null"/>.</param>
        [ComVisible(false)]
        public static void InitializeForQuery(IUnknownVector attributes, string sourceDocName,
            Dictionary<string, DbConnection> dbConnections, IPathTags pathTags)
        {
            try
            {
                AttributeStatusInfo.ResetData(sourceDocName, attributes, dbConnections, pathTags);
                Initialize(attributes);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35962");
            }
        }

        /// <summary>
        /// Initializes an attribute hierarchy unaffiliated with any data entry controls. May be
        /// called to prepare attributes for use within a <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of
        /// <see cref="IAttribute"/>s to initialize.</param>
        /// <param name="fieldModels">If initializing in the background, the
        /// <see cref="BackgroundFieldModel"/>s that represent the controls in the DEP.
        /// Should be <c>null</c> if loading into a DEP.</param>
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void Initialize(IUnknownVector attributes,
            IEnumerable<BackgroundFieldModel> fieldModels = null)
        {
            try
            {
                // If using attribute models instead of a DEP, create a new attribute for any
                // AutoCreate model that does not already have one.
                if (fieldModels != null)
                {
                    var attributeNameSet = new HashSet<string>(attributes
                        .ToIEnumerable<IAttribute>()
                        .Select(attribute => attribute.Name));

                    foreach (var fieldModel in fieldModels.Where(model => model.AutoCreate))
                    {
                        if (attributeNameSet.Add(fieldModel.Name))
                        {
                            var newAttribute = new UCLID_AFCORELib.Attribute();
                            newAttribute.Name = fieldModel.Name;
                            attributes.PushBack(newAttribute);
                        }
                    }
                }

                int attributeCount = attributes.Size();
                for (int i = 0; i < attributeCount; i++)
                {
                    IAttribute attribute = (IAttribute)attributes.At(i);
                    var fieldModel = fieldModels?.SingleOrDefault(child => child.Name == attribute.Name);

                    AttributeStatusInfo.Initialize(attribute, attributes, null, fieldModel?.DisplayOrder,
                        false, TabStopMode.Never, new DataEntryValidator(), fieldModel?.AutoUpdateQuery,
                        fieldModel?.ValidationQuery);

                    Initialize(attribute.SubAttributes, fieldModel?.Children);

                    if (fieldModel != null)
                    {
                        // Initialize the status info and validator as the corresponding DEP control
                        // would have done.
                        var statusInfo = GetStatusInfo(attribute);
                        statusInfo._isMapped = true;
                        statusInfo._isViewable = fieldModel.IsViewable;
                        statusInfo.PersistAttribute = fieldModel.PersistAttribute;
                        var validator = (DataEntryValidator)statusInfo.Validator;
                        validator.ValidationErrorMessage = fieldModel.ValidationErrorMessage;
                        validator.ValidationPattern = fieldModel.ValidationPattern;
                        validator.CorrectCase = fieldModel.ValidationCorrectsCase;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35963");
            }
        }

        /// <summary>
        /// Initializes an <see cref="IAttribute"/> of the specified <see paramref="attributeName"/>
        /// along with an associated <see cref="AttributeStatusInfo"/> instance.
        /// </summary>
        /// <param name="attributeName">The name for the <see cref="IAttribute"/> to be generated.
        /// </param>
        /// <param name="sourceAttributes">The vector of <see cref="IAttribute"/>s to which the
        /// generated <see cref="IAttribute"/> should be added.</param>
        /// <param name="owningControl">The <see cref="IDataEntryControl"/> in charge of displaying 
        /// the generated <see cref="IAttribute"/>.</param>
        [SuppressMessage("Microsoft.Interoperability", "CA1407:AvoidStaticMembersInComVisibleTypes")]
        public static IAttribute Initialize(string attributeName, IUnknownVector sourceAttributes,
            IDataEntryControl owningControl)
        {
            try
            {
                IAttribute attribute = new AttributeClass();
                attribute.Name = attributeName;
                Initialize(attribute, sourceAttributes, owningControl,
                    null, false, null, null, null, null, true);

                return attribute;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39185");
            }
        }

        /// <summary>
        /// Initializes an <see cref="IAttribute"/> of the specified <see paramref="attributeName"/>
        /// along with an associated <see cref="AttributeStatusInfo"/> instance.
        /// </summary>
        /// <param name="attributeName">The name for the <see cref="IAttribute"/> to be generated.
        /// </param>
        /// <param name="sourceAttributes">The vector of <see cref="IAttribute"/>s to which the
        /// generated <see cref="IAttribute"/> should be added.</param>
        /// <param name="owningControl">The <see cref="IDataEntryControl"/> in charge of displaying 
        /// the generated <see cref="IAttribute"/>.</param>
        /// <param name="displayOrder">An enumerable of <c>int</c> that allows the 
        /// <see cref="IAttribute"/> to be sorted by comparing to the display order of other
        /// <see cref="IAttribute"/>s.
        /// <see cref="DisplayOrder"/>values. Specify <see langword="null"/> to allow the 
        /// <see cref="IAttribute"/> to keep any display order it already has.</param>
        /// <param name="considerPropagated"><see langword="true"/> to consider the 
        /// <see cref="IAttribute"/> already propagated; <see langword="false"/> otherwise.</param>
        /// <param name="validatorTemplate">A template to be used as the master for any per-attribute
        /// <see cref="IDataEntryValidator"/> created to validate the attribute's data.
        /// Can be <see langword="null"/> to keep the existing validator or if data validation is
        /// not required.</param>
        /// <param name="tabStopMode">A <see cref="TabStopMode"/> value indicating under what
        /// circumstances the attribute should serve as a tab stop. Can be <see langword="null"/> to
        /// keep the existing tabStopMode setting.</param>
        /// <param name="autoUpdateQuery">A query which will cause the <see cref="IAttribute"/>'s
        /// value to automatically be updated using values from other <see cref="IAttribute"/>s
        /// and/or a database query.</param>
        /// <param name="validationQuery">A query which will cause the validation list for the 
        /// validator associated with the attribute to be updated using values from other
        /// <see cref="IAttribute"/>'s and/or a database query.</param>
        [SuppressMessage("Microsoft.Interoperability", "CA1407:AvoidStaticMembersInComVisibleTypes")]
        public static IAttribute Initialize(string attributeName, IUnknownVector sourceAttributes,
            IDataEntryControl owningControl, IEnumerable<int> displayOrder, bool considerPropagated,
            TabStopMode? tabStopMode, IDataEntryValidator validatorTemplate, string autoUpdateQuery,
            string validationQuery)
        {
            try
            {
                IAttribute attribute = new AttributeClass();
                attribute.Name = attributeName;
                Initialize(attribute, sourceAttributes, owningControl, displayOrder, considerPropagated,
                    tabStopMode, validatorTemplate, autoUpdateQuery, validationQuery, true);

                return attribute;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39186");
            }
        }

        /// <summary>
        /// Initializes a <see cref="AttributeStatusInfo"/> instance for the specified
        /// <see cref="IAttribute"/> with the specified parameters.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which an 
        /// <see cref="AttributeStatusInfo"/> instance is being initialized.</param>
        /// <param name="sourceAttributes">The vector of <see cref="IAttribute"/>s to which the
        /// specified <see cref="IAttribute"/> is a member.</param>
        /// <param name="owningControl">The <see cref="IDataEntryControl"/> in charge of displaying 
        /// the specified <see cref="IAttribute"/>.</param>
        [ComVisible(false)]
        public static void Initialize(IAttribute attribute, IUnknownVector sourceAttributes,
            IDataEntryControl owningControl)
        {
            Initialize(attribute, sourceAttributes, owningControl, null, false, null, null, null, null);
        }

        /// <summary>
        /// Initializes a <see cref="AttributeStatusInfo"/> instance for the specified
        /// <see cref="IAttribute"/> with the specified parameters.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which an 
        /// <see cref="AttributeStatusInfo"/> instance is being initialized.</param>
        /// <param name="sourceAttributes">The vector of <see cref="IAttribute"/>s to which the
        /// specified <see cref="IAttribute"/> is a member.</param>
        /// <param name="owningControl">The <see cref="IDataEntryControl"/> in charge of displaying 
        /// the specified <see cref="IAttribute"/>.</param>
        /// <param name="displayOrder">A <see langword="string"/> that allows the 
        /// <see cref="IAttribute"/> to be sorted by compared to other <see cref="IAttribute"/>'s 
        /// <see cref="DisplayOrder"/>values. Specify <see langword="null"/> to allow the 
        /// <see cref="IAttribute"/> to keep any display order it already has.</param>
        /// <param name="considerPropagated"><see langword="true"/> to consider the 
        /// <see cref="IAttribute"/> already propagated; <see langword="false"/> otherwise.</param>
        /// <param name="validatorTemplate">A template to be used as the master for any per-attribute
        /// <see cref="IDataEntryValidator"/> created to validate the attribute's data.
        /// Can be <see langword="null"/> to keep the existing validator or if data validation is
        /// not required.</param>
        /// <param name="tabStopMode">A <see cref="TabStopMode"/> value indicating under what
        /// circumstances the attribute should serve as a tab stop. Can be <see langword="null"/> to
        /// keep the existing tabStopMode setting.</param>
        /// <param name="autoUpdateQuery">A query which will cause the <see cref="IAttribute"/>'s
        /// value to automatically be updated using values from other <see cref="IAttribute"/>s
        /// and/or a database query.</param>
        /// <param name="validationQuery">A query which will cause the validation list for the 
        /// validator associated with the attribute to be updated using values from other
        /// <see cref="IAttribute"/>'s and/or a database query.</param>
        /// <param name="newAttribute"><see langword="true"/> if <see paramref="attribute"/> is
        /// newly generated (the blank value should not be considered a purposely applied blank
        /// value).</param>
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static void Initialize(IAttribute attribute, IUnknownVector sourceAttributes,
            IDataEntryControl owningControl, IEnumerable<int> displayOrder, bool considerPropagated,
            TabStopMode? tabStopMode, IDataEntryValidator validatorTemplate, string autoUpdateQuery,
            string validationQuery, bool newAttribute = false)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI26134", _OBJECT_NAME);

                if (IsLoggingEnabled(LogCategories.AttributeInitialized))
                {
                    Logger.LogEvent(LogCategories.AttributeInitialized, attribute,
                        owningControl as Control, attribute.Value.String);
                }

                InitializeStatics();

                // Create a new statusInfo instance (or retrieve an existing one).
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                // https://extract.atlassian.net/browse/ISSUE-12548
                // The data entry framework does not support having the same attribute mapped to two
                // different controls simultaneously.
                if (statusInfo._owningControl != null)
                {
                    ExtractException.Assert("ELI37798", "Invalid configuration; an attribute has " +
                        "been mapped to multiple controls.",
                        statusInfo._owningControl == owningControl, "Attribute", attribute.Name,
                        "Control 1", ((Control)statusInfo._owningControl).Name,
                        "Control 2", ((Control)owningControl).Name);
                }

                statusInfo._owningControl = owningControl;
                statusInfo._isMapped = (owningControl !=null);

                if (owningControl != null && displayOrder == null)
                {
                    displayOrder = DataEntryMethods.GetTabIndices((Control)owningControl);
                }

                // Check to see if the display order should be set.
                bool reorder = false;
                if (displayOrder != null)
                {
                    // [DataEntry:1004]
                    // Since this value will be compared using the string class, pad zeros so that tab
                    string textDisplayOrder = string.Join(".",
                        displayOrder.Select(index => string.Format(CultureInfo.InvariantCulture, "{0:D3}", index)));

                    // If the displayOrder value has changed, sourceAttributes need to be reordered.
                    if (statusInfo._displayOrder != textDisplayOrder)
                    {
                        reorder = true;
                        statusInfo._displayOrder = textDisplayOrder;
                    }
                }

                // If the display order hasn't changed, but the attribute doesn't yet exist in
                // sourceAttributes, reordering needs to happen so that it gets added.
                if (!reorder)
                {
                    if (sourceAttributes.Size() == 0)
                    {
                        reorder = true;
                    }
                    else
                    {
                        int index = -1;
                        sourceAttributes.FindByReference(attribute, 0, ref index);
                        if (index < 0)
                        {
                            reorder = true;
                        }
                    }
                }

                if (reorder)
                {
                    DataEntryMethods.ReorderAttributes(sourceAttributes,
                            DataEntryMethods.AttributeAsVector(attribute));
                }

                // Set/update the propagated status if necessary.
                if (considerPropagated && !statusInfo._hasBeenPropagated)
                {
                    statusInfo._hasBeenPropagated = true;
                }

                // Set/update the validator if necessary.
                if (validatorTemplate != null && validatorTemplate != statusInfo._validatorTemplate)
                {
                    statusInfo._validatorTemplate = validatorTemplate;
                }

                if (statusInfo._validatorTemplate != null && statusInfo._validator == null)
                {
                    // [DataEntry:861]
                    // Recent changes to validation in the DataEntry framework now require
                    // validators to have a 1 to 1 relationship with attribute it is validating so
                    // long as the validation is attribute specific.
                    statusInfo._validator = statusInfo._validatorTemplate.GetPerAttributeInstance();
                }

                // Update the tabStopMode if necessary
                if (tabStopMode != null && tabStopMode.Value != statusInfo._tabStopMode)
                {
                    statusInfo._tabStopMode = tabStopMode.Value;

                    if (tabStopMode == TabStopMode.OnlyWhenInvalid)
                    {
                        // If an attribute will be skipped in the tab order unless invalid, mark
                        // it as viewed. Otherwise tabbing through all fields in a document will
                        // leave fields using OnlyWhenInvalid mode as unread. 
                        statusInfo._hasBeenViewed = true;
                    }
                }

                // If the attribute's source attributes have known parent, use it to generate the
                // attribute to parent mapping.
                IAttribute parentAttribute;
                if (_subAttributesToParentMap.TryGetValue(sourceAttributes, out parentAttribute))
                {
                    statusInfo._parentAttribute = parentAttribute;
                }

                // Add a mapping for the attribute's subattributes for future reference.
                _subAttributesToParentMap[attribute.SubAttributes] = attribute;

                // Raise the AttributeInitialized event if it hasn't already been raised for this
                // attribute.
                // [DataEntry:876]
                // On attribute initialized needs to be called before LoadDataQueries so that
                // an undo memento for adding the attribute is added to the undo stack before an
                // undo memento for modifying the value (as LoadDataQueries may do).
                if (!statusInfo._initialized)
                {
                    statusInfo._initialized = true;

                    // https://extract.atlassian.net/browse/ISSUE-13506
                    // https://extract.atlassian.net/browse/ISSUE-12547
                    // Attributes (even newly constructed ones) have a non-null value. We don't want
                    // to treat the default blank value as a value that needs to be applied; this
                    // can end up clearing the intended value for this new attribute. Set
                    // LastAppliedStringValue only if we are dealing with a previously existing
                    // attribute.
                    if (!newAttribute)
                    {
                        // Keep track of previously applied values, in case the field control isn't
                        // yet prepared to accept the value. (i.e. combo box whose item list has not
                        // yet been updated/initialized)
                        statusInfo.LastAppliedStringValue = attribute.Value.String;
                    }

                    _undoManager.AddMemento(new DataEntryAddedAttributeMemento(attribute));

                    OnAttributeInitialized(attribute, sourceAttributes, owningControl);
                }

                LoadDataQueries(attribute, sourceAttributes, autoUpdateQuery, validationQuery, statusInfo);

                // [DataEntry:173] Trim any whitespace from the beginning and end.
                // [DataEntry:167]
                // Accessing the value also ensures the value is accessed and, thus, created.
                attribute.Value.Trim(" \t\r\n", " \t\r\n");
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24684", ex);
            }
        }

        /// <summary>
        /// Gets whether validation triggers are currently enabled.
        /// </summary>
        /// <value><see langword="true"/> if validation triggers are currently enabled; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public static bool ValidationTriggersEnabled
        {
            get
            {
                return _validationTriggersEnabled;
            }
        }

        /// <summary>
        /// Specifies whether validation triggers should be enabled or disabled.
        /// </summary>
        /// <param name="enable"><see langword="true"/> if validation triggers are to be enabled,
        /// <see langword="false"/> otherwise.
        /// <para><b>Note</b></para>
        /// Specifying <see langword="false"/> only prevents newly created triggers from
        /// registering for immediate evaluation. It does not prevent already registered triggers
        /// from updating the validation status based on the modification of trigger attributes.
        /// </param>
        [ComVisible(false)]
        public static void EnableValidationTriggers(bool enable)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI29046", _OBJECT_NAME);

                if (enable != _validationTriggersEnabled)
                {
                    _validationTriggersEnabled = enable;

                    // If validation triggers are being enabled, try to register all triggers.
                    if (_validationTriggersEnabled)
                    {
                        RefreshValidation();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29047", ex);
            }
        }

        /// <summary>
        /// Triggers the execution of all auto-update queries.
        /// </summary>
        [ComVisible(false)]
        public static void RefreshAutoUpdateValues()
        {
            var executionContext = QueryExecutionContext;

            try
            {
                QueryExecutionContext = ExecutionContext.OnRefresh;

                foreach (AutoUpdateTrigger autoUpdateTrigger in _autoUpdateTriggers.Values)
                {
                    autoUpdateTrigger.UpdateValue();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41573");
            }
            finally
            {
                QueryExecutionContext = executionContext;
            }
        }

        /// <summary>
        /// Triggers the execution of all validation queries.
        /// </summary>
        [ComVisible(false)]
        public static void RefreshValidation()
        {
            var executionContext = QueryExecutionContext;

            try
            {
                QueryExecutionContext = ExecutionContext.OnRefresh;

                foreach (AutoUpdateTrigger validationTrigger in _validationTriggers.Values)
                {
                    validationTrigger.UpdateValue();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41574");
            }
            finally
            {
                QueryExecutionContext = executionContext;
            }
        }

        /// <summary>
        /// Clears any data cached by the <see cref="DataEntryQuery"/>s of any auto-update or
        /// validation queries being tracked.
        /// </summary>
        [ComVisible(false)]
        public static void ClearQueryCache()
        {
            try
            {
                OnQueryCacheCleared();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38247");
            }
        }

        /// <summary>
        /// Clears process-wide cache data
        /// </summary>
        [ComVisible(false)]
        public static void ClearProcessWideCache()
        {
            try
            {
                ClearedProcessWideCache?.Invoke(null, new EventArgs());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45502");
            }
        }

        /// <overloads>Applies the specified value to the specified <see cref="IAttribute"/> and
        /// raises <see cref="AttributeValueModified"/> when appropriate.</overloads>
        /// <summary>
        /// Applies the specified value to the specified <see cref="IAttribute"/> and raises
        /// <see cref="AttributeValueModified"/> when appropriate.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose value is to be updated.
        /// </param>
        /// <param name="value">The <see cref="SpatialString"/> value to apply to the
        /// <see cref="IAttribute"/>.</param>
        /// <param name="acceptSpatialInfo"><see langword="true"/> if the attribute's value should
        /// be marked as accepted, <see langword="false"/> if it should be left as-is.</param>
        /// <param name="endOfEdit"><see langword="true"/> if this change represents the end of the
        /// current edit, <see langword="false"/> if it is or may be part of an ongoing edit.
        /// </param>
        [ComVisible(false)]
        public static void SetValue(IAttribute attribute, SpatialString value,
            bool acceptSpatialInfo, bool endOfEdit)
        {
            // Use modifiedAttributeMemento as a copy of the original value in case of an error as
            // well as for use by the UndoManager so we do not make two copies of the spatial string.
            DataEntryModifiedAttributeMemento modifiedAttributeMemento = null;

            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI27093", _OBJECT_NAME);

                modifiedAttributeMemento = new DataEntryModifiedAttributeMemento(attribute);

                // AddMemento needs to be called before changing the value so that the
                // DataEntryModifiedAttributeMemento knows of the attribute's original value.
                _undoManager.AddMemento(modifiedAttributeMemento);

                attribute.Value = value;

                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                // https://extract.atlassian.net/browse/ISSUE-12662
                // Whether or not EndEditInProgress, track modified attributes for the purpose of
                // keeping track of which attributes need to be validated.
                _attributesToValidate.Add(attribute);

                if (EndEditInProgress)
                {
                    // If EndEdit is currently being processed (and this is a result of it), raise
                    // the non-incremental modification event now.
                    statusInfo.OnAttributeValueModified(attribute, false, acceptSpatialInfo, true);
                }
                else
                {
                    if (!endOfEdit)
                    {
                        // If not the end of the edit raise an incremental modification event and
                        // queue the value for an eventual non-incremental event.
                        statusInfo.OnAttributeValueModified(attribute, true, acceptSpatialInfo, true);
                    }

                    // Ensure there is a spatial change recorded for this attribute in
                    //_attributesBeingModified.
                    RecordSpatialChange(attribute, modifiedAttributeMemento.OriginalValue);

                    // After queuing the modification, call EndEdit if directed.
                    if (endOfEdit)
                    {
                        EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                // If there was an exception applying the value, restore the original value to
                // prevent exceptions from continuously being generated.
                if (modifiedAttributeMemento != null)
                {
                    try
                    {
                        attribute.Value = modifiedAttributeMemento.OriginalValue;

                        // After setting the value, refresh the value and raise
                        // AttributeValueModified to notify the host of the change.
                        AttributeStatusInfo statusInfo =
                            AttributeStatusInfo.GetStatusInfo(attribute);
                        statusInfo.OwningControl.RefreshAttributes(true, attribute);
                        statusInfo.OnAttributeValueModified(attribute, false, false, true);
                    }
                    catch (Exception ex2)
                    {
                        ExtractException.Log("ELI27094", ex2);
                    }
                }

                throw ExtractException.AsExtractException("ELI26090", ex);
            }
        }

        /// <summary>
        /// Applies the specified <see paramref="value"/> to the specified
        /// <see paramref="propertyName"/> of the UI element representing the specified
        /// <see paramref="attribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the UI element property
        /// is to be updated.</param>
        /// <param name="propertyName">The name of the property to update. Can be a nested property
        /// such as "OwningColumn.Width".</param>
        /// <param name="value">The <see cref="string"/> representation of the value to apply.
        /// </param>
        [ComVisible(false)]
        public static void SetPropertyValue(IAttribute attribute, string propertyName, string value)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                // These values will be set for each generation in the specified property name.
                object element = null;
                Type elementType = null;
                PropertyInfo property = null;

                foreach (string currentProperty in propertyName.Split('.'))
                {
                    // The root element will be the UI element directly associated with the
                    // attribute.
                    element = (property == null)
                        ? statusInfo.OwningControl.GetAttributeUIElement(attribute)
                        : property.GetValue(element, null);

                    elementType = element.GetType();
                    property = elementType.GetProperty(currentProperty);
                }

                // The property we have after looping through all generations is the one for which
                // we need to apply the value.
                property.SetValue(element, value.ConvertToType(property.PropertyType), null);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37288");
            }
        }

        /// <summary>
        /// Applies the specified value to the specified <see cref="IAttribute"/> and raises
        /// <see cref="AttributeValueModified"/> when appropriate.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose value is to be updated.
        /// </param>
        /// <param name="value">The <see langword="string"/> value to apply to the
        /// <see cref="IAttribute"/>.</param>
        /// <param name="acceptSpatialInfo"><see langword="true"/> if the attribute's value should
        /// be marked as accepted, <see langword="false"/> if it should be left as-is.</param>
        /// <param name="endOfEdit"><see langword="true"/> if this change represents the end of the
        /// current edit, <see langword="false"/> if it is or may be part of an ongoing edit.
        /// </param>
        [ComVisible(false)]
        public static void SetValue(IAttribute attribute, string value, bool acceptSpatialInfo,
            bool endOfEdit)
        {
            // Use modifiedAttributeMemento as a copy of the original value in case of an error as
            // well as for use by the UndoManager so we do not make two copies of the spatial string.
            DataEntryModifiedAttributeMemento modifiedAttributeMemento = null;

            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI26135", _OBJECT_NAME);

                // https://extract.atlassian.net/browse/ISSUE-12555
                // Ensure leading spaces are removed when applying values from auto-complete since
                // we artificially prepending spaces to auto-complete values allow the space bar to
                // trigger auto-complete.
                if (value.Length > 0 && value[0] == ' ' && FormsMethods.IsAutoCompleteDisplayed())
                {
                    value = value.Substring(1);
                }

                // Don't do anything if the specified value matches the existing value.
                if (attribute.Value.String != value)
                {
                    modifiedAttributeMemento = new DataEntryModifiedAttributeMemento(attribute);

                    // AddMemento needs to be called before changing the value so that the
                    // DataEntryModifiedAttributeMemento knows of the attribute's original value.
                    _undoManager.AddMemento(new DataEntryModifiedAttributeMemento(attribute));

                    // If the attribute doesn't contain any spatial information, just
                    // change the text.
                    if (attribute.Value.GetMode() == ESpatialStringMode.kNonSpatialMode)
                    {
                        attribute.Value.ReplaceAndDowngradeToNonSpatial(value);
                    }
                    // If the attribute contains spatial information, it needs to be converted to
                    // hybrid mode to update the text.
                    else
                    {
                        attribute.Value.ReplaceAndDowngradeToHybrid(value);
                    }

                    AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                    // https://extract.atlassian.net/browse/ISSUE-12662
                    // Whether or not EndEditInProgress, track modified attributes for the purpose of
                    // keeping track of which attributes need to be validated.
                    _attributesToValidate.Add(attribute);

                    if (EndEditInProgress)
                    {
                        // If EndEdit is currently being processed (and this is a result of it), raise
                        // the non-incremental modification event now.
                        statusInfo.OnAttributeValueModified(attribute, false, acceptSpatialInfo, false);
                    }
                    else
                    {
                        // Raise an incremental modification event and queue the value for an
                        // eventual non-incremental event.
                        statusInfo.OnAttributeValueModified(attribute, true, acceptSpatialInfo, false);

                        if (!_attributesBeingModified.ContainsKey(attribute))
                        {
                            _attributesBeingModified[attribute] = new KeyValuePair<bool, SpatialString>(
                                false, modifiedAttributeMemento.OriginalValue);
                        }
                    }
                }

                // Call EndEdit if directed (whether or not the value actually changed).
                if (endOfEdit)
                {
                    EndEdit();
                }
            }
            catch (Exception ex)
            {
                // If there was an exception applying the value, restore the original value to
                // prevent exceptions from continuously being generated.
                if (modifiedAttributeMemento != null)
                {
                    try
                    {
                        attribute.Value = modifiedAttributeMemento.OriginalValue;

                        // After setting the value, refresh the value and raise
                        // AttributeValueModified to notify the host of the change.
                        AttributeStatusInfo statusInfo =
                            AttributeStatusInfo.GetStatusInfo(attribute);
                        statusInfo.OwningControl.RefreshAttributes(false, attribute);
                        statusInfo.OnAttributeValueModified(attribute, false, false, false);
                    }
                    catch (Exception ex2)
                    {
                        ExtractException.Log("ELI27095", ex2);
                    }
                }

                throw ExtractException.AsExtractException("ELI26093", ex);
            }
        }

        /// <summary>
        /// Deletes the specified attribute from the DataEntry framework. This releases all mappings
        /// and events tied to the attribute.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to delete from the system.</param>
        [ComVisible(false)]
        public static void DeleteAttribute(IAttribute attribute)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI26136", _OBJECT_NAME);

                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                MarkAsUnInitialized(attribute);

                // Dispose of any auto-update trigger for the attribute.
                AutoUpdateTrigger autoUpdateTrigger = null;
                if (_autoUpdateTriggers.TryGetValue(attribute, out autoUpdateTrigger))
                {
                    _autoUpdateTriggers.Remove(attribute);
                    autoUpdateTrigger.Dispose();
                }

                // Dispose of any validation trigger for the attribute.
                AutoUpdateTrigger validationTrigger = null;
                if (_validationTriggers.TryGetValue(attribute, out validationTrigger))
                {
                    _validationTriggers.Remove(attribute);
                    validationTrigger.Dispose();
                }

                // Set the now disposed of validator to null so that if this attribute is later
                // resurrected via Undo, the validator will be re-initialized.
                statusInfo._validator = null;

                IUnknownVector subAttributes = attribute.SubAttributes;
                _subAttributesToParentMap.Remove(attribute.SubAttributes);

                // Recursively process each sub-attribute and process it as well.
                int count = subAttributes.Size();
                for (int i = 0; i < count; i++)
                {
                    // Since each attribute will be removed from subAttributes when DeleteAttributes
                    // is called, always delete from the first index since the vector will be one
                    // smaller with each iteration.
                    DeleteAttribute((IAttribute)subAttributes.At(0));
                }

                // Remove the attribute from the overall attribute hierarchy.
                IUnknownVector parentCollection;
                if (statusInfo._parentAttribute != null)
                {
                    parentCollection = statusInfo._parentAttribute.SubAttributes;
                }
                else
                {
                    parentCollection = _attributes;
                }

                int index = -1;
                parentCollection.FindByReference(attribute, 0, ref index);

                // TODO: There are some rare instances where the attribute is not found in the
                // parent collection (index == -1). Really, the only side effect should be that
                // FinalReleaseComObject won't be called for it... I don't think this warrants
                // spending time tracking down or logging, at least for now.
                if (index >= 0)
                {
                    // [DataEntry:693]
                    // Send a DeletedAttributeMemento to the undo manager even if it is not currently
                    // tracking operations so that the DataObject can be nulled on the
                    // attribute when the history is cleared in ResetData
                    _undoManager.AddMemento(
                        new DataEntryDeletedAttributeMemento(attribute, parentCollection, index));
                    parentCollection.RemoveValue(attribute);

                    // Raise the AttributeDeleted event last otherwise it can cause the hosts' count
                    // of invalid and unviewed attributes to be off.
                    statusInfo.OnAttributeDeleted(attribute);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26107", ex);
            }
        }

        /// <overloads>
        /// Tests to see if the provided <see cref="IAttribute"/> meets any validation requirements
        /// the associated <see cref="DataEntryValidator"/> has.
        /// </overloads>
        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> meets any validation requirements
        /// the associated <see cref="DataEntryValidator"/> has.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to validate.</param>
        /// <param name="throwException">If <see langword="true"/> the method will throw an
        /// exception if the provided value does not meet validation requirements.</param>
        /// <returns>A <see cref="DataValidity"/>value indicating whether 
        /// <see paramref="attribute"/>'s value is currently valid.</returns>
        [ComVisible(false)]
        public static DataValidity Validate(IAttribute attribute, bool throwException)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI29200", _OBJECT_NAME);

                IDataEntryValidator validator = GetStatusInfo(attribute).Validator;
                if (validator == null)
                {
                    return DataValidity.Valid;
                }
                else
                {
                    return validator.Validate(attribute, throwException);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29176", ex);
            }
        }

        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> meets any validation requirements
        /// the associated <see cref="DataEntryValidator"/> has. If valid, uses the validator to
        /// match the casing in the validator and to remove extra whitespace.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to validate.</param>
        /// <param name="throwException">If <see langword="true"/> if the method will throw an
        /// exception if the provided value does not meet validation requirements.</param>
        /// <param name="correctedValue">A corrected value applied to the attribute or
        /// <see langword="null"/> if no changes to the attribute's value were made.</param>
        /// <returns>A <see cref="DataValidity"/>value indicating whether 
        /// <see paramref="attribute"/>'s value is currently valid.</returns>
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public static DataValidity Validate(IAttribute attribute, bool throwException,
            out string correctedValue)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI29201", _OBJECT_NAME);

                correctedValue = null;

                IDataEntryValidator validator = GetStatusInfo(attribute).Validator;
                if (validator == null)
                {
                    return DataValidity.Valid;
                }
                else
                {
                    return validator.Validate(attribute, throwException, out correctedValue);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29199", ex);
            }
        }

        /// <summary>
        /// Checks to see if the specified <see cref="IAttribute"/>'s data has been viewed by the
        /// user. (This usually means that the control displaying the attribute has received focus).
        /// <para><b>NOTE:</b></para>
        /// An <see cref="IAttribute"/> is always considered as "viewed" if it is un-viewable.
        /// (See <see cref="IsViewable"/>).
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to be checked for whether it has
        /// been viewed.</param>
        /// <param name="recursive"><see langword="false"/> to check only the specified
        /// <see cref="IAttribute"/>, <see langword="true"/> to check all descendant
        /// <see cref="IAttribute"/>s as well.</param>
        /// <returns><see langword="true"/> if the <see cref="IAttribute"/> has been viewed, or
        /// <see langword="false"/> if it has not.</returns>
        [ComVisible(false)]
        public static bool HasBeenViewedOrIsNotViewable(IAttribute attribute, bool recursive)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                if (statusInfo._isViewable && !statusInfo._hasBeenViewed)
                {
                    // This attribute has not been viewed.
                    return false;
                }
                else if (recursive)
                {
                    // Check to see that all subattributes have been viewed as well.
                    return AttributeScanner<bool>.Scan(attribute.SubAttributes, null,
                        ConfirmDataViewed, true, true, true, null);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24480", ex);
            }
        }

        /// <summary>
        /// Checks to see if the specified <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>'s (and their sub-attributes) have been viewed by the user. If 
        /// not, the first <see cref="IAttribute"/> that has not been viewed is specified.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/> to be checked for whether it has
        /// been viewed.</param>
        /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
        /// the point at which the scan should be started with each attribute further down the
        /// the stack being a descendant to the previous <see cref="IAttribute"/> in the stack.
        /// </param>
        /// <param name="forward"><see langword="true"/> to scan forward through the attribute 
        /// hierarchy, <see langword="false"/> to scan backward.</param>
        /// <param name="loop"><see langword="true"/> to resume scanning from the beginning of
        /// the <see cref="IAttribute"/>s (back to the starting point) if the end was reached 
        /// successfully, <see langword="false"/> to end the scan once the end of the 
        /// <see cref="IAttribute"/> vector is reached.</param>
        /// <returns>A stack of <see cref="IAttribute"/>s
        /// where the first attribute in the stack represents the root-level attribute
        /// the unviewed attribute is descended from, and each successive attribute represents
        /// a sub-attribute to the previous until the final attribute is the first unviewed 
        /// attribute.</returns>
        [ComVisible(false)]
        public static Stack<IAttribute> FindNextUnviewedAttribute(IUnknownVector attributes,
            Stack<IAttribute> startingPoint, bool forward, bool loop)
        {
            try
            {
                Stack<IAttribute> unviewedAttributes = new Stack<IAttribute>();

                if (!AttributeScanner<bool>.Scan(attributes, startingPoint, ConfirmDataViewed, true,
                    forward, loop, unviewedAttributes))
                {
                    return unviewedAttributes;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24647", ex);
            }
        }

        /// <summary>
        /// Marks the specified <see cref="IAttribute"/> as viewed or not viewed.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> which should be marked as viewed
        /// or not viewed.</param>
        /// <param name="hasBeenViewed"><see langword="true"/> to indicate that the 
        /// <see cref="IAttribute"/> has been viewed, <see langword="false"/> to indicate it has
        /// not been viewed.</param>
        [ComVisible(false)]
        public static void MarkAsViewed(IAttribute attribute, bool hasBeenViewed)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                // If the viewed status of the attribute has changed from its previous value,
                // raise the ViewedStateChanged event to notify listeners of the new status.
                // Ensure an item's viewed status doesn't change after it has been deleted (i.e.,
                // it no longer exists.
                if (statusInfo._isViewable && statusInfo._hasBeenViewed != hasBeenViewed &&
                    statusInfo._initialized)
                {
                    // AddMemento needs to be called before changing the status so that the
                    // DataEntryAttributeStatusChangeMemento knows of the attribute's original status.
                    _undoManager.AddMemento(new DataEntryAttributeStatusChangeMemento(attribute));

                    statusInfo._hasBeenViewed = hasBeenViewed;

                    OnViewedStateChanged(attribute, hasBeenViewed);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24481", ex);
            }
        }

        /// <summary>
        /// Finds the first attribute in the specified <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>'s to find one matching a <see cref="DataValidity"/>
        /// in <see paramref="targetValidity"/>. 
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s 
        /// to be checked for whether its data is valid.</param>
        /// <param name="targetValidity">The <see cref="DataValidity"/> value(s) sought.</param>
        /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
        /// the point at which the scan should be started with each attribute further down the
        /// the stack being a descendant to the previous <see cref="IAttribute"/> in the stack.
        /// </param>
        /// <param name="forward"><see langword="true"/> to scan forward through the attribute 
        /// hierarchy, <see langword="false"/> to scan backward.</param>
        /// <param name="loop"><see langword="true"/> to resume scanning from the beginning of
        /// the <see cref="IAttribute"/>s (back to the starting point) if the end was reached 
        /// successfully, <see langword="false"/> to end the scan once the end of the 
        /// <see cref="IAttribute"/> vector is reached.</param>
        /// <returns>A stack of <see cref="IAttribute"/>s
        /// where the first attribute in the stack represents the root-level attribute
        /// the first attribute matching targetValidity is descended from, and each successive
        /// attribute represents a sub-attribute to the previous until the final attribute is
        /// the first attribute matching targetValidity.
        /// </returns>
        [ComVisible(false)]
        public static Stack<IAttribute> FindNextAttributeByValidity(IUnknownVector attributes,
            DataValidity targetValidity, Stack<IAttribute> startingPoint, bool forward, bool loop)
        {
            try
            {
                Stack<IAttribute> invalidAttributes = new Stack<IAttribute>();

                // Keep scanning as long as an attribute's data DataValidityDoesNotMatch
                // the targetValidity.
                bool scanResult =
                    AttributeScanner<DataValidity>.Scan(attributes, startingPoint,
                        DataValidityDoesNotMatch, targetValidity, forward, loop, invalidAttributes);

                if (!scanResult)
                {
                    return invalidAttributes;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24410", ex);
            }
        }

        /// <summary>
        /// Specifies whether the data associated with the specified attribute as valid. This
        /// applies only to the associated attribute's data, not to any subattributes.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose validity is being set.
        /// </param>
        /// <param name="dataValidity">A <see cref="DataValidity"/> value indicating whether the
        /// attribute's value is valid, invalid or a validation warning.</param>
        [ComVisible(false)]
        public static void SetDataValidity(IAttribute attribute, DataValidity dataValidity)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                // If the validation status of the attribute has changed from its previous value,
                // raise the ValidationStateChanged event to notify listeners of the new status.
                // [DataEntry:876]
                // Ensure an item's validity doesn't change after it has been deleted (i.e., it is
                // no longer initialized)
                if (statusInfo._dataValidity != dataValidity && statusInfo._initialized)
                {
                    statusInfo._dataValidity = dataValidity;

                    OnValidationStateChanged(attribute, dataValidity);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24494", ex);
            }
        }

        /// <summary>
        /// Checks to see if the specified <see cref="IAttribute"/>'s data is valid.
        /// <para><b>NOTE:</b></para>
        /// An <see cref="IAttribute"/> is never considered invalid if it is not viewable.
        /// (See <see cref="IsViewable"/>).
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to be checked.</param>
        /// <returns>A <see cref="DataValidity"/> value indicating whether the attribute's value is
        /// valid.</returns>
        [ComVisible(false)]
        public static DataValidity GetDataValidity(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return (statusInfo._isViewable) ? statusInfo._dataValidity : DataValidity.Valid;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24919", ex);
            }
        }

        /// <summary>
        /// Enables or disables data validation on the specified attribute.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the data validation
        /// should be enabled or disabled.</param>
        /// <param name="enable"><see langword="true"/> if the data validation should be enabled;
        /// <see langword="false"/> otherwise.</param>
        [ComVisible(false)]
        public static void EnableValidation(IAttribute attribute, bool enable)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                statusInfo._validationEnabled = enable;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26963", ex);
            }
        }

        /// <summary>
        /// Gets whether data validation is enabled on the specified <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to be checked.</param>
        /// <returns><see langword="true"/> if data validation is enabled <see langword="false"/>
        /// if it is not.</returns>
        [ComVisible(false)]
        public static bool IsValidationEnabled(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._validationEnabled;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26964", ex);
            }
        }

        /// <summary>
        /// Checks to see whether the associated <see cref="IAttribute"/> has been fully propagated
        /// into <see cref="IDataEntryControl"/>s.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/> to be checked for whether it has
        /// been propagated.</param>
        /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
        /// the point at which the scan should be started with each attribute further down the
        /// the stack being a descendant to the previous <see cref="IAttribute"/> in the stack.
        /// </param>
        /// <param name="unPropagatedAttributes">A stack of <see cref="IAttribute"/>s
        /// where the first attribute in the stack represents the root-level attribute
        /// the first unpropagated attribute is descended from, and each successive attribute 
        /// represents a sub-attribute to the previous until the final attribute is the first 
        /// unpropagated attribute.
        /// </param>
        /// <returns><see langword="true"/> if the <see cref="IAttribute"/> and all subattributes
        /// have been flagged as propagated; <see langword="false"/> otherwise.</returns>
        [ComVisible(false)]
        public static bool HasBeenPropagated(IUnknownVector attributes,
            Stack<IAttribute> startingPoint, Stack<IAttribute> unPropagatedAttributes)
        {
            try
            {
                return AttributeScanner<bool>.Scan(attributes, startingPoint, ConfirmHasBeenPropagated,
                    true, true, true, unPropagatedAttributes);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24450", ex);
            }
        }

        /// <summary>
        /// Mark the data associated with the specified <see cref="IAttribute"/> as propagated.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the data should be marked
        /// propagated or not propagated.</param>
        /// <param name="propagated"><see langword="true"/> to indicate the 
        /// <see cref="IAttribute"/> has been propagated, <see langword="false"/> to indicate the
        /// <see cref="IAttribute"/> has not been propagated.</param>
        /// <param name="recursive">If <see langword="false"/> only the specified 
        /// <see cref="IAttribute"/> will be marked as propagated.  If <see langword="true"/> all 
        /// descendants <see cref="IAttribute"/>s will be marked as propagated as well.</param>
        [ComVisible(false)]
        public static void MarkAsPropagated(IAttribute attribute, bool propagated, bool recursive)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                statusInfo._hasBeenPropagated = propagated;

                if (recursive)
                {
                    // Mark all descendant attributes as propagated as well.
                    AttributeScanner<bool>.Scan(attribute.SubAttributes, null, MarkAsPropagated,
                        propagated, true, true, null);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24478", ex);
            }
        }

        /// <summary>
        /// Specifies whether the specified <see cref="IAttribute"/> is viewable or not.
        /// "Viewable" means the value can be displayed, but is not necessarily currently displayed
        /// depending on which attributes are propagated.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose viewable status is to be
        /// set.</param>
        /// <param name="isViewable"><see langword="true"/> if the <see cref="IAttribute"/> is
        /// viewable, <see langword="false"/> if it is not.</param>
        /// <returns><see langword="true"/> in all cases to continue the scan.</returns>
        [ComVisible(false)]
        public static bool MarkAsViewable(IAttribute attribute, bool isViewable)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                if (statusInfo._isViewable != isViewable)
                {
                    // Don't allow any attribute that belongs to a disabled data entry control to
                    // be marked as viewable since the value will not be selectable or editable.
                    if (isViewable &&
                        statusInfo._owningControl != null && statusInfo._owningControl.Disabled)
                    {
                        return true;
                    }

                    statusInfo._isViewable = isViewable;

                    // If the attribute was not previously viewable, it would not have been
                    // considered unviewed.  Now it will be considered unviewed so the 
                    // ViewedStateChanged event needs to be raised (assuming the attribute
                    // is actually unviewed).
                    if (!statusInfo._hasBeenViewed)
                    {
                        OnViewedStateChanged(attribute, false);
                    }

                    // https://extract.atlassian.net/browse/ISSUE-12812
                    // Since non-viewable attributes will not be considered invalid, ensure the
                    // validation status is up-to-date for any attribute whose viewed status has
                    // been changed since the original document load.
                    if (statusInfo._owningControl != null &&
                        !statusInfo._owningControl.DataEntryControlHost.ChangingData)
                    {
                        AttributeStatusInfo.Validate(attribute, false);
                        if (isViewable)
                        {
                            // If the attribute has been made viewable, make sure the control is
                            // accurately indicating the current validation state.
                            statusInfo._owningControl.RefreshAttributes(false, attribute);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24856", ex);
            }
        }

        /// <summary>
        /// Indicates whether the specified <see cref="IAttribute"/> is viewable in the 
        /// <see cref="DataEntryControlHost"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose viewable status is to be
        /// checked.</param>
        /// <returns><see langword="true"/> if the specified <see cref="IAttribute"/> is viewable;
        /// <see langword="false"/> if it is not.</returns>
        [ComVisible(false)]
        public static bool IsViewable(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._isViewable;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25138", ex);
            }
        }

        /// <summary>
        /// Finds the first <see cref="IAttribute"/> that is a tabstop in the DEP after the
        /// specified starting point.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/>s from which a tabstop
        /// <see cref="IAttribute"/> is to be sought.</param>
        /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
        /// the point at which the scan should be started with each attribute further down the
        /// the stack being a descendant to the previous <see cref="IAttribute"/> in the stack.
        /// </param>
        /// <param name="forward"><see langword="true"/> to scan forward through the
        /// <see cref="IAttribute"/>s, <see langword="false"/> to scan backward.</param>
        /// <returns>A genealogy of <see cref="IAttribute"/>s specifying the next tabstop 
        /// <see cref="IAttribute"/>.</returns>
        [ComVisible(false)]
        public static Stack<IAttribute> GetNextTabStopAttribute(IUnknownVector attributes,
            Stack<IAttribute> startingPoint, bool forward)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI28823", _OBJECT_NAME);

                Stack<IAttribute> nextTabStopAttributeGenealogy = new Stack<IAttribute>();

                if (!AttributeScanner<bool>.Scan(
                        attributes, startingPoint, ConfirmIsTabStop, false, forward, true,
                        nextTabStopAttributeGenealogy))
                {
                    return nextTabStopAttributeGenealogy;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28817", ex);
            }
        }

        /// <summary>
        /// Finds the first <see cref="IAttribute"/> representing the next tab group after the
        /// specified starting point.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/>s from which a tab group
        /// <see cref="IAttribute"/> is to be sought.</param>
        /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
        /// the point at which the scan should be started with each attribute further down the
        /// the stack being a descendant to the previous <see cref="IAttribute"/> in the stack.
        /// </param>
        /// <param name="forward"><see langword="true"/> to scan forward through the
        /// <see cref="IAttribute"/>s, <see langword="false"/> to scan backward.</param>
        /// <returns>A genealogy of <see cref="IAttribute"/>s specifying the next tab group
        /// <see cref="IAttribute"/> in a control which supports tab groups or the next
        /// tab stop attribute in a control that does not support tab stops.</returns>
        [ComVisible(false)]
        public static Stack<IAttribute> GetNextTabGroupAttribute(IUnknownVector attributes,
            Stack<IAttribute> startingPoint, bool forward)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI28824", _OBJECT_NAME);

                Stack<IAttribute> nextTabGroupAttributeGenealogy = new Stack<IAttribute>();

                if (!AttributeScanner<bool>.Scan(
                        attributes, startingPoint, ConfirmIsTabGroup, false, forward, true,
                        nextTabGroupAttributeGenealogy))
                {
                    return nextTabGroupAttributeGenealogy;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28816", ex);
            }
        }

        /// <summary>
        /// Finds the first <see cref="IAttribute"/> representing the next tab group or tab after
        /// the specified starting point (whichever comes first).
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/>s from which a tabstop
        /// <see cref="IAttribute"/> is to be sought.</param>
        /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
        /// the point at which the scan should be started with each attribute further down the
        /// the stack being a descendant to the previous <see cref="IAttribute"/> in the stack.
        /// </param>
        /// <param name="forward"><see langword="true"/> to scan forward through the
        /// <see cref="IAttribute"/>s, <see langword="false"/> to scan backward.</param>
        /// <returns>A genealogy of <see cref="IAttribute"/>s specifying the next tabgroup or
        /// tabstop <see cref="IAttribute"/> (whichever comes first). If no such attribute is found
        /// in control indicated by <see paramref="startingPoint"/>, the result will by found in the
        /// same manner as <see cref="GetNextTabGroupAttribute"/>.</returns>
        [ComVisible(false)]
        public static Stack<IAttribute> GetNextTabStopOrGroupAttribute(IUnknownVector attributes,
            Stack<IAttribute> startingPoint, bool forward)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI28825", _OBJECT_NAME);

                // Find the control indicated by startingPoint.
                IDataEntryControl startingControl = null;
                if (startingPoint != null)
                {
                    foreach (IAttribute attribute in startingPoint)
                    {
                        startingControl = AttributeStatusInfo.GetOwningControl(attribute);
                    }
                }

                Stack<IAttribute> nextTabStopOrGroupAttributeGenealogy = new Stack<IAttribute>();

                // Find the next tab stop or group attribute
                if (!AttributeScanner<bool>.Scan(
                        attributes, startingPoint, ConfirmIsTabStopOrGroup, false, forward, true,
                        nextTabStopOrGroupAttributeGenealogy))
                {
                    // Find the attribute and control indicated by the result.
                    IAttribute endingAttribute = null;
                    IDataEntryControl endingControl = null;
                    foreach (IAttribute attribute in nextTabStopOrGroupAttributeGenealogy)
                    {
                        endingAttribute = attribute;
                        endingControl = AttributeStatusInfo.GetOwningControl(attribute);
                    }

                    // Return the result if still in the same control.
                    if (startingControl == endingControl)
                    {
                        return nextTabStopOrGroupAttributeGenealogy;
                    }
                    else
                    {
                        // If the result is in a different control, return the result only if the
                        // control does not support tab groups or the result represents a tab group.
                        List<IAttribute> tabGroup =
                            AttributeStatusInfo.GetAttributeTabGroup(endingAttribute);
                        if (tabGroup == null || tabGroup.Count > 0)
                        {
                            return nextTabStopOrGroupAttributeGenealogy;
                        }

                        // Otherwise, search instead for the next tab group.
                        return GetNextTabGroupAttribute(attributes,
                            nextTabStopOrGroupAttributeGenealogy, forward);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24984", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="HintType"/> associated with the <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose hint type is to be checked.
        /// </param>
        /// <returns>The <see cref="HintType"/> associated with the <see cref="IAttribute"/>.
        /// </returns>
        [ComVisible(false)]
        public static HintType GetHintType(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._hintType;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25230", ex);
            }
        }

        /// <summary>
        /// Sets the <see cref="HintType"/> associated with the <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose hint type is to be set.
        /// </param>
        /// <param name="hintType">The <see cref="HintType"/> to be associated with the 
        /// <see cref="IAttribute"/>.</param>
        [ComVisible(false)]
        public static void SetHintType(IAttribute attribute, HintType hintType)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                if (statusInfo._hintType != hintType)
                {
                    // AddMemento needs to be called before changing the status so that the
                    // DataEntryAttributeStatusChangeMemento knows of the attribute's original status.
                    _undoManager.AddMemento(new DataEntryAttributeStatusChangeMemento(attribute));

                    statusInfo._hintType = hintType;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25359", ex);
            }
        }

        /// <summary>
        /// Gets whether the user has accepted the value of the <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to be checked.</param>
        /// <returns><see langword="true"/> if the user has accepted the value of the
        /// <see cref="IAttribute"/>; <see langword="false"/> otherwise.
        /// </returns>
        [ComVisible(false)]
        public static bool IsAccepted(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._isAccepted;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25388", ex);
            }
        }

        /// <summary>
        /// Sets whether the <see cref="IAttribute"/>'s spatial info has been accepted by the user.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose spatial info is to be 
        /// accepted/unaccepted.</param>
        /// <param name="accept"><see langword="true"/> to indicated the value of the
        /// <see cref="IAttribute"/> has been accepted by the user; <see langword="false"/>
        /// otherwise.</param>
        [ComVisible(false)]
        public static void AcceptValue(IAttribute attribute, bool accept)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                if (statusInfo._isAccepted != accept)
                {
                    // AddMemento needs to be called before changing the status so that the
                    // DataEntryAttributeStatusChangeMemento knows of the attribute's original status.
                    _undoManager.AddMemento(new DataEntryAttributeStatusChangeMemento(attribute));

                    statusInfo._isAccepted = accept;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25389", ex);
            }
        }

        /// <summary>
        /// Gets whether hints are enabled for the <see cref="IAttribute"/>. The fact that hints
        /// are enabled doesn't necessarily mean the attribute has one.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose hint enabled status is to be
        /// checked.</param>
        /// <returns><see langword="true"/> if hints are enabled for the specified
        /// <see cref="IAttribute"/>; <see langword="false"/> otherwise.
        /// </returns>
        [ComVisible(false)]
        public static bool HintEnabled(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._hintEnabled;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25979", ex);
            }
        }

        /// <summary>
        /// Sets whether hints are enabled for the <see cref="IAttribute"/>. The fact that hints
        /// are enabled doesn't necessarily mean the attribute has one.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose hint enabled status is to be
        /// set.</param>
        /// <param name="hintEnabled"><see langword="true"/> to enable hints for the specified
        /// <see cref="IAttribute"/>; <see langword="false"/> otherwise.
        /// </param>
        [ComVisible(false)]
        public static void EnableHint(IAttribute attribute, bool hintEnabled)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                if (statusInfo._hintEnabled != hintEnabled)
                {
                    // AddMemento needs to be called before changing the status so that the
                    // DataEntryAttributeStatusChangeMemento knows of the attribute's original status.
                    _undoManager.AddMemento(new DataEntryAttributeStatusChangeMemento(attribute));

                    statusInfo._hintEnabled = hintEnabled;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25980", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="IDataEntryControl"/> that the <see cref="IAttribute"/> is associated
        /// with.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose owning control is to be
        /// checked.</param>
        /// <returns>The <see cref="IDataEntryControl"/> that the specified <see cref="IAttribute"/>
        /// is associated with.
        /// </returns>
        [ComVisible(false)]
        public static IDataEntryControl GetOwningControl(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._owningControl;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25350", ex);
            }
        }

        /// <summary>
        /// Sets the <see cref="Extract.Imaging.RasterZone"/>s that define a hint for the specified
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the spatial hint is
        /// defined.</param>
        /// <param name="rasterZones">A list of <see cref="Extract.Imaging.RasterZone"/>s 
        /// that define the spatial hint.</param>
        [ComVisible(false)]
        public static void SetHintRasterZones(IAttribute attribute,
            IEnumerable<Extract.Imaging.RasterZone> rasterZones)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                statusInfo._hintRasterZones = rasterZones;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25501", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="Extract.Imaging.RasterZone"/>s that define a hint for the specified
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the spatial hint is needed.
        /// </param>
        /// <returns>A list of <see cref="Extract.Imaging.RasterZone"/>s that define the spatial hint.
        /// </returns>
        [ComVisible(false)]
        public static IEnumerable<Extract.Imaging.RasterZone> GetHintRasterZones(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._hintRasterZones;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25506", ex);
            }
        }

        /// <summary>
        /// Retrieves the parent <see cref="IAttribute"/> of the specified attribute.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose parent is needed.</param>
        /// <returns>The parent <see cref="IAttribute"/> if there is one, <see langword="null"/> if
        /// there is not.</returns>
        [ComVisible(false)]
        public static IAttribute GetParentAttribute(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._parentAttribute;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26097", ex);
            }
        }

        /// <summary>
        /// Indicates whether the attribute will be persisted in output.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose persistence status is needed.
        /// </param>
        /// <returns><see langword="true"/> if the <see cref="IAttribute"/> will be persisted in
        /// output; <see langword="false"/> otherwise.</returns>
        [ComVisible(false)]
        public static bool IsAttributePersistable(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._persistAttribute;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26588", ex);
            }
        }

        /// <summary>
        /// Specifies whether the attribute should be persisted in output.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose persistence status is to be
        /// changed.</param>
        /// <param name="persistAttribute"><see langword="true"/> if the <see cref="IAttribute"/>
        /// should be persisted in output; <see langword="false"/> otherwise.</param>
        [ComVisible(false)]
        public static void SetAttributeAsPersistable(IAttribute attribute, bool persistAttribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                statusInfo._persistAttribute = persistAttribute;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26589", ex);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="IAttribute"/>s that are contained within a group owned by this
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose tab group is requested.
        /// </param>
        /// <returns>A list of <see cref="IAttribute"/>s that are contained within a group owned by this
        /// <see cref="IAttribute"/>. Will be <see langword="null"/> for attributes in controls
        /// that don't support tab groups or an empty list for attributes that don't own a group
        /// in a control that supports tab groups.</returns>
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<IAttribute> GetAttributeTabGroup(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._tabGroup;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28812", ex);
            }
        }

        /// <summary>
        /// Sets a list of <see cref="IAttribute"/>s that are contained within a group owned by this
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose tab group is being assigned.
        /// </param>
        /// <param name="tabGroup">The list of attributes in the tab group owned by
        /// <see paramref="attribute"/>.</param>
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static void SetAttributeTabGroup(IAttribute attribute, List<IAttribute> tabGroup)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                ExtractException.Assert("ELI29048",
                    "A tab group must be associated with an enabled control!",
                    statusInfo.OwningControl != null && !statusInfo.OwningControl.Disabled);

                statusInfo._tabGroup = tabGroup;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28813", ex);
            }
        }

        /// <summary>
        /// Gets whether the specified <see cref="IAttribute"/> currently represents a tab stop.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose tab stop status is being
        /// checked.</param>
        /// <returns><see langword="true"/> if the attribute currently represents a tab stop,
        /// <see langword="false"/> otherwise.</returns>
        [ComVisible(false)]
        public static bool IsAttributeTabStop(IAttribute attribute)
        {
            try
            {
                return ConfirmIsTabStop(attribute, GetStatusInfo(attribute), true);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29131", ex);
            }
        }

        /// <summary>
        /// Removes all spatial info associated with the <see cref="IAttribute"/> (including any
        /// hint).
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose spatial info is to be
        /// removed.</param>
        /// <returns><see langword="true"/> if spatial info was removed, <see langword="false"/>
        /// if there was no spatial info to remove.</returns>
        [ComVisible(false)]
        public static bool RemoveSpatialInfo(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                bool spatialInfoRemoved = false;

                var modifiedAttributeMemento = new DataEntryModifiedAttributeMemento(attribute);

                // AddMemento needs to be called before changing the value so that the
                // DataEntryModifiedAttributeMemento knows of the attribute's original value.
                _undoManager.AddMemento(modifiedAttributeMemento);

                // Removing spatial info will not trigger an EndEdit call to separate this as an
                // independent operation but it should considered one.
                _undoManager.StartNewOperation();

                // If the attribute has spatial information (a highlight), remove it and
                // flag the attribute so that hints are not created in its place.
                if (attribute.Value.HasSpatialInfo())
                {
                    attribute.Value.DowngradeToNonSpatialMode();

                    spatialInfoRemoved = true;
                }
                // If the attribute has an associated hint, remove the hint and flag the
                // attribute so that hints are not re-created. 
                else if (statusInfo._hintType != HintType.None)
                {
                    spatialInfoRemoved = true;
                }

                if (spatialInfoRemoved)
                {
                    // AddMemento needs to be called before changing the status so that the
                    // DataEntryAttributeStatusChangeMemento knows of the attribute's original status.
                    _undoManager.AddMemento(new DataEntryAttributeStatusChangeMemento(attribute));

                    statusInfo._hintEnabled = false;

                    // Notify listeners that spatial info has changed.
                    statusInfo.OnAttributeValueModified(attribute, true, false, true);

                    // Ensure there is a spatial change recorded for this attribute in
                    //_attributesBeingModified.
                    RecordSpatialChange(attribute, modifiedAttributeMemento.OriginalValue);
                }

                return spatialInfoRemoved;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27320", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified <see paramref="attribute"/> has spatial info (including
        /// hints).
        /// </summary>
        /// <param name="attribute">The  <see cref="IAttribute"/> to be checked for whether it has
        /// spatial info.</param>
        /// <param name="requireDirect"><see langword="true"/> to require that the attribute value
        /// itself is spatial or that it's hint is a direct hint; <see langword="false"/> if an
        /// itself hint is allowed to satisfy this check.</param>
        /// <returns><see langword="true"/> the specified <see paramref="attribute"/> has spatial
        /// info; otherwise, <see langword="false"/>.
        /// </returns>
        [ComVisible(false)]
        public static bool HasSpatialInfo(IAttribute attribute, bool requireDirect)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                // [DataEntry:1192, 1194]
                // If this attribute has been deleted, don't consider it to have any spatial info.
                if (!statusInfo._initialized)
                {
                    return false;
                }

                if (attribute.Value.HasSpatialInfo())
                {
                    return true;
                }

                // If the attribute value itself didn't have any spatial info, hints can still
                // qualify. (depending upon whether the caller wants indirect hints to qualify)
                if (statusInfo._hintType == HintType.None || !statusInfo._hintEnabled)
                {
                    return false;
                }

                return (!requireDirect || statusInfo._hintType == HintType.Direct);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35390");
            }
        }

        /// <summary>
        /// Forgets all <see cref="LastAppliedStringValue"/>s that are currently being remembered to
        /// ensure that they don't get used later on after the value has been changed to something else.
        /// </summary>
        [ComVisible(false)]
        public static void ForgetLastAppliedStringValues()
        {
            try
            {
                // LastAppliedStringValues should be remembered throughout the initial loading of a
                // document regardless of calls into ForgetLastAppliedStringValues.
                // _undoManager.TrackOperations will be false during document load, so it can be
                // used as an indication of a loading document.
                if (!UndoManager.TrackOperations)
                {
                    return;
                }

                foreach (AttributeStatusInfo statusInfo in _statusInfoMap.Values)
                {
                    statusInfo.LastAppliedStringValue = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37381");
            }
        }

        /// <summary>
        /// Returns the a list of the <see cref="IAttribute"/>s which match the specified query
        /// applied to the specified root attribute or <see langword="null"/> if the query
        /// references the root of the attribute hierarchy.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> on which the query is to be
        /// executed.</param>
        /// <param name="query">The query to execute on the specified <see cref="IAttribute"/>.
        /// </param>
        /// <returns>The <see cref="IUnknownVector"/> of all <see cref="IAttribute"/>s matching the
        /// query.</returns>
        [ComVisible(false)]
        // Returning a list so the count can be checked results can be accessed by index.
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<IAttribute> ResolveAttributeQuery(IAttribute rootAttribute, string query)
        {
            try
            {
                InitializeStatics();

                List<IAttribute> results = new List<IAttribute>();

                // Trim off any leading slash.
                if (!string.IsNullOrEmpty(query) && query[0] == '/')
                {
                    query = query.Remove(0, 1);
                }

                // If there is nothing left in the query, return the root attribute.
                if (string.IsNullOrEmpty(query) || query == ".")
                {
                    // Return the rootAttribute if the query is null.
                    results.Add(rootAttribute);
                    return results;
                }
                // If the parent attribute is specified, return the result of running the remaining
                // query on the parent attribute.
                else if (query.StartsWith("..", StringComparison.Ordinal))
                {
                    ExtractException.Assert("ELI26141", "Invalid attribute query!",
                        rootAttribute != null);

                    results.AddRange(ResolveAttributeQuery(GetStatusInfo(rootAttribute)._parentAttribute,
                        (query.Length > 3) ? query.Substring(3) : null));
                }
                // Otherwise, apply the next element of the query.
                else
                {
                    // Determine the remaining query after this element.
                    string nextQuery = null;
                    int queryEnd = query.IndexOf('/');
                    if (queryEnd != -1)
                    {
                        nextQuery = query.Substring(queryEnd);
                        query = query.Substring(0, queryEnd);
                    }

                    // If we are at the root of the hierarchy, _attributes needs to be used for the
                    // query as there will be no rootAttribute.
                    IUnknownVector attributesToQuery =
                        (rootAttribute == null) ? _attributes : rootAttribute.SubAttributes;

                    if (null == attributesToQuery)
                    {
                        return results;
                    }

                    // Apply the remaining query to all attributes matching the current query.
                    int count = attributesToQuery.Size();
                    if (count > 0)
                    {
                        // Syntax: [AttributeNameFilter](=[AttributeValueFilter])(@[AttributeTypeFilter])
                        var filters = query.Split('@');
                        ExtractException.Assert("ELI38207",
                            "'@' character is reserved as a type filter delimiter in attribute queries",
                            filters.Count() <= 2, "Query", query);
                        string typeFilter = (filters.Length == 1) ? "" : filters[1];

                        filters = filters[0].Split('=');
                        ExtractException.Assert("ELI38208",
                            "'=' character is reserved as a value filter delimiter in attribute queries",
                            filters.Count() <= 2, "Query", query);
                        string nameFilter = filters[0];
                        string valueFilter = (filters.Length == 1) ? "" : filters[1];

                        for (int i = 0; i < count; i++)
                        {
                            IAttribute attribute = (IAttribute)attributesToQuery.At(i);
                            if (null == attribute)
                            {
                                continue;
                            }

                            // Check if the attribute should be eliminated by a name filter
                            if (!string.IsNullOrEmpty(nameFilter) && nameFilter != "*" &&
                                !nameFilter.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            // Check if the attribute should be eliminated by a value filter
                            if (!string.IsNullOrEmpty(valueFilter) && !valueFilter.Equals(
                                attribute.Value.String, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            // Check if the attribute should be eliminated based a type filter.
                            if (!string.IsNullOrEmpty(typeFilter) && typeFilter != "*")
                            {
                                var types = attribute.Type.Split('+').Distinct();
                                if (!types.Contains(typeFilter, StringComparer.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                            }

                            // Select the attribute if it has not been eliminated by any of the
                            // filters.
                            results.AddRange(
                                ResolveAttributeQuery(attribute, nextQuery));
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26102", ex);
                ee.AddDebugData("rootAttribute",
                    (rootAttribute == null) ? "null" : rootAttribute.Name, false);
                ee.AddDebugData("query", query, false);
                throw ee;
            }
        }

        /// <summary>
        /// End the current edit by raising AttributeValueModified with IncrementalUpdate = false.
        /// </summary>
        [ComVisible(false)]
        public static void EndEdit()
        {
            try
            {
                InitializeStatics();

                if (_endEditRecursionBlock)
                {
                    // As after as I can tell, this is never reached; all recursion appears to
                    // originate from OnEditEnded. However, since recursion while
                    // _endEditRecursionBlock is true would cause an exception due to
                    // _attributesBeingModified being modified while being iterated, keep this check
                    // in place.
                    return;
                }

                try
                {
                    _endEditRecursionBlock = true;
                    _endEditReferenceCount++;

                    RaiseValueModifedForAttributesBeingModified();

                    // https://extract.atlassian.net/browse/ISSUE-12662
                    // Now that all queries that are to be triggered have triggered, explicitly
                    // validate all attributes that have been modified; attributes not currently
                    // displayed will not otherwise be validated unless they use a query that
                    // explicitly references one of the modified attributes. For example, a regex
                    // pattern or validation list that is produced independent of the currently
                    // value will not otherwise get validated.
                    ValidateAllModifiedAttributes();

                    _attributesBeingModified.Clear();
                    _attributesToValidate.Clear();
                }
                finally
                {
                    _endEditRecursionBlock = false;

                    // https://extract.atlassian.net/browse/ISSUE-12549
                    // OnEditEnded can cause recursive calls into this method; only the first call
                    // of OnEditEnded is needed.
                    if (_endEditReferenceCount == 1)
                    {
                        try
                        {
                            OnEditEnded();

                            // Any time EndEdit is called, consider it the end of an operation.
                            _undoManager.StartNewOperation();
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractLog("ELI37634");
                        }
                        finally
                        {
                            // Ensure _endEditReferenceCount always gets set back to zero.
                            _endEditReferenceCount = 0;
                        }
                    }
                    else
                    {
                        _endEditReferenceCount--;
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // Keep track of all attributes whose values need to be restored due to a failed
                    // update.
                    Dictionary<IDataEntryControl, List<IAttribute>> controlsToRefresh =
                        new Dictionary<IDataEntryControl, List<IAttribute>>();
                    bool refreshSpatialInfo = false;

                    // If there was an exception applying any value, restore the original value for
                    // all modified attributes to prevent exceptions from continuously being
                    // generated.
                    foreach (KeyValuePair<IAttribute, KeyValuePair<bool, SpatialString>>
                        modifiedAttribute in _attributesBeingModified)
                    {
                        IAttribute attribute = modifiedAttribute.Key;
                        attribute.Value = modifiedAttribute.Value.Value;

                        // Record whether spatial info has changed for any of the attributes.
                        refreshSpatialInfo |= modifiedAttribute.Value.Key;

                        // After setting the value, refresh the value and raise
                        // AttributeValueModified to notify the host of the change.
                        AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);

                        List<IAttribute> attributeCollection = null;
                        if (!controlsToRefresh.TryGetValue(statusInfo.OwningControl,
                            out attributeCollection))
                        {
                            attributeCollection = new List<IAttribute>();
                            controlsToRefresh[statusInfo.OwningControl] = attributeCollection;
                        }

                        attributeCollection.Add(attribute);
                        statusInfo.OnAttributeValueModified(attribute, false, false, true);
                    }

                    // Refresh all attributes whose values have been restored.
                    foreach (KeyValuePair<IDataEntryControl, List<IAttribute>>
                        controlToRefresh in controlsToRefresh)
                    {
                        controlToRefresh.Key.RefreshAttributes(refreshSpatialInfo,
                            controlToRefresh.Value.ToArray());
                    }
                }
                catch (Exception ex2)
                {
                    ExtractException.Log("ELI27096", ex2);
                }

                // If an exception occurred, clear any pending modifications so they are not applied
                // on a subsequent edit event.
                _attributesBeingModified.Clear();
                _attributesToValidate.Clear();

                throw ExtractException.AsExtractException("ELI26118", ex);
            }
        }

        /// <summary>
        /// Allows all <see cref="IAttribute"/> COM objects in the supplied vector to be garbage
        /// collected by setting the DataObject property to null and thereby removing an untracked
        /// (I think) circular reference. (If this is not done, handles are leaked.)
        /// Setting the DataObject to null rather than calling Marshal.FinalReleaseComObject prevents
        /// "COM object that has been separated from its underlying RCW cannot be used" exceptions.
        /// https://extract.atlassian.net/browse/ISSUE-13345
        /// <b><para>Important:</para></b>
        /// Call DeleteAttribute rather than ReleaseAttributes for any attributes removed while the
        /// DEP is current loaded or in the process of being loaded. DeleteAttribute will raise
        /// proper events to remove references to the Attribute, but if ReleaseAttributes is used,
        /// those reference may remain.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> containing the
        /// <see cref="IAttribute"/>s that need to be freed.
        /// </param>
        /// <param name="miscUtils">Pass any existing instance of to avoid another having to be
        /// created or <c>null</c> if no such instance currently exists.</param>
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void ReleaseAttributes(IUnknownVector attributes, MiscUtils miscUtils = null)
        {
            try
            {
                if (attributes.Size() > 0)
                {
                    if (miscUtils == null)
                    {
                        miscUtils = new MiscUtils();
                    }

                    foreach (IAttribute attribute in
                        DataEntryMethods.ToAttributeEnumerable(attributes, true))
                    {
                        ReleaseAttributes(attribute.SubAttributes, miscUtils);

                        // This will release the reference to the DataObject (thereby preventing memory
                        // leak issues), while saving the persistable elements of the status info object
                        // so that they are persisted with the attribute itself.
                        attribute.StowDataObject(miscUtils);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27846", ex);
            }
        }

        /// <summary>
        /// Executes disposal of any thread-local or thread-static objects just prior to the UI
        /// thread closing.
        /// </summary>
        [ComVisible(false)]
        public static void DisposeThread()
        {
            try
            {
                // Ensure all AttributeStatusInfos are released; these may otherwise hold references
                // to objects thereby preventing them from being finalized.
                ResetData(null, null, null);

                // This unregisters event handlers that may otherwise hold references to objects
                // thereby preventing them from being finalized.
                _dataResetHandler.DisposeThread();
                _queryCacheClearHandler.DisposeThread();
                _attributeInitializedHandler.DisposeThread();
                _viewedStateChangedHandler.DisposeThread();
                _validationStateChangedHandler.DisposeThread();
                _editEndedHandler.DisposeThread();
                _queryDelayEndedHandler.DisposeThread();
            }
            catch (Exception ex)
            {
                // Exceptions should not be thrown during disposal.
                ex.ExtractLog("ELI41626");
            }
        }

        #endregion Static Members

        #region Events

        /// <summary>
        /// Raised whenever ResetData is called.
        /// <para><b>Note</b></para>
        /// The static members of this class are ThreadStatic. Event handlers will only be invoked
        /// on the thread on which they were added. Ensure that the event is registered on all
        /// threads that need to handle it.
        /// </summary>
        public static event EventHandler<EventArgs> DataReset
        {
            add
            {
                _dataResetHandler.AddEventHandler(value);
            }

            remove
            {
                _dataResetHandler.RemoveEventHander(value);
            }
        }

        /// <summary>
        /// An event that indicates an attribute is being initialized into an 
        /// <see cref="IDataEntryControl"/>.
        /// <para><b>Note</b></para>
        /// The static members of this class are ThreadStatic. Event handlers will only be invoked
        /// on the thread on which they were added. Ensure that the event is registered on all
        /// threads that need to handle it.
        /// </summary>
        public static event EventHandler<AttributeInitializedEventArgs> AttributeInitialized
        {
            add
            {
                _attributeInitializedHandler.AddEventHandler(value);
            }

            remove
            {
                _attributeInitializedHandler.RemoveEventHander(value);
            }
        }

        /// <summary>
        /// Fired to notify listeners that an <see cref="IAttribute"/> that was previously marked 
        /// as unviewed has now been marked as viewed (or vice-versa).
        /// <para><b>Note</b></para>
        /// The static members of this class are ThreadStatic. Event handlers will only be invoked
        /// on the thread on which they were added. Ensure that the event is registered on all
        /// threads that need to handle it.
        /// </summary>
        public static event EventHandler<ViewedStateChangedEventArgs> ViewedStateChanged
        {
            add
            {
                _viewedStateChangedHandler.AddEventHandler(value);
            }

            remove
            {
                _viewedStateChangedHandler.RemoveEventHander(value);
            }
        }

        /// <summary>
        /// Fired to notify listeners that an <see cref="IAttribute"/> that was previously marked 
        /// as having invalid data has now been marked as valid (or vice-versa).
        /// <para><b>Note</b></para>
        /// The static members of this class are ThreadStatic. Event handlers will only be invoked
        /// on the thread on which they were added. Ensure that the event is registered on all
        /// threads that need to handle it.
        /// </summary>
        public static event EventHandler<ValidationStateChangedEventArgs> ValidationStateChanged
        {
            add
            {
                _validationStateChangedHandler.AddEventHandler(value);
            }

            remove
            {
                _validationStateChangedHandler.RemoveEventHander(value);
            }
        }

        /// <summary>
        /// Raised at the end of an <see cref="EndEdit"/> call.
        /// <para><b>Note</b></para>
        /// The static members of this class are ThreadStatic. Event handlers will only be invoked
        /// on the thread on which they were added. Ensure that the event is registered on all
        /// threads that need to handle it.
        /// </summary>
        public static event EventHandler<EventArgs> EditEnded
        {
            add
            {
                _editEndedHandler.AddEventHandler(value);
            }

            remove
            {
                _editEndedHandler.RemoveEventHander(value);
            }
        }

        /// <summary>
        /// Raised to notify listeners that a delay of query execution that had been in place has
        /// been removed.
        /// <para><b>Note</b></para>
        /// The static members of this class are ThreadStatic. Event handlers will only be invoked
        /// on the thread on which they were added. Ensure that the event is registered on all
        /// threads that need to handle it.
        /// </summary>
        public static event EventHandler<EventArgs> QueryDelayEnded
        {
            add
            {
                _queryDelayEndedHandler.AddEventHandler(value);
            }

            remove
            {
                _queryDelayEndedHandler.RemoveEventHander(value);
            }
        }

        /// <summary>
        /// Raised to notify listeners that a request to clear all cached query data has been
        /// issued.
        /// </summary>
        public static event EventHandler<EventArgs> QueryCacheCleared
        {
            add
            {
                _queryCacheClearHandler.AddEventHandler(value);
            }

            remove
            {
                _queryCacheClearHandler.RemoveEventHander(value);
            }
        }

        /// <summary>
        /// Raised to notify listeners that a request to clear all process-wide cache data.
        /// NOTE: Handlers of this event should be thread safe by taking into account some threads
        /// may be adding data to the cache as this is called on another.
        /// </summary>
        public static event EventHandler<EventArgs> ClearedProcessWideCache;

        /// <summary>
        /// Raised to notify listeners that an Attribute's value was modified.
        /// </summary>
        public event EventHandler<AttributeValueModifiedEventArgs> AttributeValueModified;

        /// <summary>
        /// Raised to notify listeners that an Attribute was deleted.
        /// </summary>
        public event EventHandler<AttributeDeletedEventArgs> AttributeDeleted;

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Ensures all ThreadStatic variables are initialized.
        /// </summary>
        static void InitializeStatics()
        {
            if (_pathTags == null)
            {
                _pathTags = new FileActionManagerPathTags();
                _statusInfoMap = new Dictionary<IAttribute, AttributeStatusInfo>();
                _subAttributesToParentMap = new Dictionary<IUnknownVector, IAttribute>();
                _autoUpdateTriggers = new Dictionary<IAttribute, AutoUpdateTrigger>();
                _validationTriggers = new Dictionary<IAttribute, AutoUpdateTrigger>();
                _attributesBeingModified =
                    new Dictionary<IAttribute, KeyValuePair<bool, SpatialString>>();
                _attributesToValidate = new HashSet<IAttribute>();
                _undoManager = new UndoManager();
                _endEditReferenceCount = 0;
                _endEditRecursionBlock = false;
            }
        }

        /// <summary>
        /// Marks the specified <see pararef="attribute"/> and as uninitialized as well as all its
        /// descendants since an attribute cannot be initialize if its parent is not.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to mark as uninitialized.</param>
        static void MarkAsUnInitialized(IAttribute attribute)
        {
            AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

            statusInfo._initialized = false;

            IUnknownVector subAttributes = attribute.SubAttributes;

            // Recursively mark all sub-attributes as uninitialized
            int count = subAttributes.Size();
            for (int i = 0; i < count; i++)
            {
                MarkAsUnInitialized((IAttribute)subAttributes.At(i));
            }
        }

        /// <summary>
        /// Raises the <see cref="DataReset"/> event.
        /// </summary>
        static void OnDataReset()
        {
            var eventHandler = _dataResetHandler.ThreadEventHandler;
            if (eventHandler != null)
            {
                eventHandler(null, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="ViewedStateChanged"/> event.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> associated with the event.</param>
        /// <param name="dataIsViewed"><see langword="true"/> if the <see cref="IAttribute"/>'s data
        /// has now been marked as viewed, <see langword="false"/> if it has now been marked as 
        /// unviewed.</param>
        static void OnViewedStateChanged(IAttribute attribute, bool dataIsViewed)
        {
            var eventHandler = _viewedStateChangedHandler.ThreadEventHandler;
            if (eventHandler != null)
            {
                eventHandler(null, new ViewedStateChangedEventArgs(attribute, dataIsViewed));
            }
        }

        /// <summary>
        /// Raises the <see cref="ValidationStateChanged"/> event. 
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> associated with the event.</param>
        /// <param name="dataValidity">A <see cref="DataValidity"/> value indicating whether the
        /// attribute's value is now valid.</param>
        static void OnValidationStateChanged(IAttribute attribute, DataValidity dataValidity)
        {
            var eventHandler = _validationStateChangedHandler.ThreadEventHandler;
            if (eventHandler != null)
            {
                eventHandler(null, new ValidationStateChangedEventArgs(attribute, dataValidity));
            }
        }

        /// <summary>
        /// Raises the <see cref="AttributeInitialized"/> event.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> being initialized.</param>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>s from which the attribute is from.</param>
        /// <param name="dataEntryControl">The <see cref="IDataEntryControl"/> that the
        /// <see cref="IAttribute"/> is associated with.</param>
        static void OnAttributeInitialized(IAttribute attribute,
            IUnknownVector sourceAttributes, IDataEntryControl dataEntryControl)
        {
            var eventHandler = _attributeInitializedHandler.ThreadEventHandler;
            if (eventHandler != null)
            {
                eventHandler(null,
                    new AttributeInitializedEventArgs(attribute, sourceAttributes, dataEntryControl));
            }
        }
        
        /// <summary>
        /// Raises the <see cref="EditEnded"/> event.
        /// </summary>
        static void OnEditEnded()
        {
            var eventHandler = _editEndedHandler.ThreadEventHandler;
            if (eventHandler != null)
            {
                eventHandler(null, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="QueryDelayEnded"/> event.
        /// </summary>
        static void OnQueryDelayEnded()
        {
            var eventHandler = _queryDelayEndedHandler.ThreadEventHandler;
            if (eventHandler != null)
            {
                eventHandler(null, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="QueryCacheCleared"/> event.
        /// </summary>
        static void OnQueryCacheCleared()
        {
            var eventHandler = _queryCacheClearHandler.ThreadEventHandler;
            if (eventHandler != null)
            {
                eventHandler(null, new EventArgs());
            }
        }

        /// <summary>
        /// Loads the auto-update and validation data queries for the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute for which queries should be loaded.</param>
        /// <param name="sourceAttributes">The vector of <see cref="IAttribute"/>s to which the
        /// specified <see cref="IAttribute"/> is a member.</param>
        /// <param name="autoUpdateQuery">A query which will cause the <see cref="IAttribute"/>'s
        /// value to automatically be updated using values from other <see cref="IAttribute"/>s
        /// and/or a database query.</param>
        /// <param name="validationQuery">A query which will cause the validation list for the 
        /// validator associated with the attribute to be updated using values from other
        /// <see cref="IAttribute"/>'s and/or a database query.</param>
        /// <param name="statusInfo">The <see cref="AttributeStatusInfo"/> for this
        /// <see paramref="attribute"/>.</param>
        static void LoadDataQueries(IAttribute attribute, IUnknownVector sourceAttributes,
            string autoUpdateQuery, string validationQuery, AttributeStatusInfo statusInfo)
        {
            if (!DisableAutoUpdateQueries)
            {
                // Find any existing auto-update trigger.
                AutoUpdateTrigger existingAutoUpdateTrigger = null;
                _autoUpdateTriggers.TryGetValue(attribute, out existingAutoUpdateTrigger);

                if ((autoUpdateQuery != null && autoUpdateQuery != statusInfo._autoUpdateQuery) ||
                    (statusInfo._autoUpdateQuery != null && existingAutoUpdateTrigger == null))
                {
                    // Dispose of any previously existing auto-update trigger.
                    if (existingAutoUpdateTrigger != null)
                    {
                        existingAutoUpdateTrigger.Dispose();
                        _autoUpdateTriggers.Remove(attribute);
                    }

                    if (autoUpdateQuery != null)
                    {
                        statusInfo._autoUpdateQuery = autoUpdateQuery;
                    }

                    if (!string.IsNullOrEmpty(statusInfo._autoUpdateQuery))
                    {
                        // We need to ensure that the attribute is a part of the sourceAttributes
                        // in order for AutoUpdateTrigger to creation to work. When creating a new
                        // attribute, this won't be the case.  Add it now, even though it will still
                        // need to be re-ordered later.
                        sourceAttributes.PushBackIfNotContained(attribute);

                        _autoUpdateTriggers[attribute] = new AutoUpdateTrigger(attribute,
                            statusInfo._autoUpdateQuery, _dbConnections, false);
                    }
                }
            }

            if (!DisableValidationQueries && !ThreadEnding)
            {
                // Find any existing validation trigger.
                AutoUpdateTrigger existingValidationTrigger = null;
                _validationTriggers.TryGetValue(attribute, out existingValidationTrigger);

                if ((validationQuery != null && validationQuery != statusInfo._validationQuery) ||
                    (statusInfo._validationQuery != null && existingValidationTrigger == null))
                {
                    // Dispose of any previously existing validation trigger.
                    if (existingValidationTrigger != null)
                    {
                        existingValidationTrigger.Dispose();
                        _validationTriggers.Remove(attribute);
                    }

                    if (validationQuery != null)
                    {
                        statusInfo._validationQuery = validationQuery;
                    }

                    if (!string.IsNullOrEmpty(statusInfo._validationQuery))
                    {
                        // We need to ensure that the attribute is a part of the sourceAttributes
                        // in order for AutoUpdateTrigger to creation to work. When creating a new
                        // attribute, this won't be the case.  Add it now, even though it will still
                        // need to be re-ordered later.
                        sourceAttributes.PushBackIfNotContained(attribute);

                        _validationTriggers[attribute] = new AutoUpdateTrigger(attribute,
                            statusInfo._validationQuery, _dbConnections, true);
                    }
                }
                else
                {
                    // If a validation trigger is in place, use it to update the control's
                    // validation list now since by virtue of the fact that the attribute is being
                    // re-initialized, the control was likely previously displaying a different
                    // attribute with a different validation list.
                    AutoUpdateTrigger validationTrigger = null;
                    if (_validationTriggers.TryGetValue(attribute, out validationTrigger))
                    {
                        validationTrigger.UpdateValue();
                    }
                }
            }
        }

        /// <summary>
        /// Updates the validation status of all attributes in <see cref="_attributesToValidate"/>.
        /// </summary>
        static void ValidateAllModifiedAttributes()
        {
            // Retrieve a separate copy of the current _attributesBeingModified so any modification
            // to _attributesBeingModified while executing this method does not cause errors
            // iterating _attributesBeingModified.
            var modifiedAttributes = _attributesToValidate.ToArray();

            // Validate each modified attribute.
            foreach (IAttribute attribute in modifiedAttributes)
            {
                // Use throwException = false to update the validation status without
                // actually throwing an error.
                Validate(attribute, false);
            }
        }

        /// <summary>
        /// Raises the <see cref="AttributeValueModified"/> event for every entry in
        /// <see cref="_attributesBeingModified"/> modified for attributes being modified.
        /// </summary>
        static void RaiseValueModifedForAttributesBeingModified()
        {
            // https://extract.atlassian.net/browse/ISSUE-12662
            // Retrieve a separate copy of the current _attributesBeingModified so that
            // we can continue to add to _attributesBeingModified as auto-update queries are
            // triggered here.
            Dictionary<IAttribute, KeyValuePair<bool, SpatialString>> attributesBeingModified =
                _attributesBeingModified.ToDictionary(
                    (entry) => entry.Key, (entry) => entry.Value);

            // If attributes have been modified since the last end edit, raise 
            // IncrementalUpdate == false AttributeValueModified events now.
            foreach (KeyValuePair<IAttribute, KeyValuePair<bool, SpatialString>>
                modifiedAttribute in attributesBeingModified)
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(modifiedAttribute.Key);
                statusInfo.OnAttributeValueModified(modifiedAttribute.Key, false, false,
                    modifiedAttribute.Value.Key);
            }
        }

        /// <summary>
        /// Ensures there is a spatial change in <see cref="_attributesBeingModified"/> for the
        /// specified <see paramref="attribute"/>.
        /// </summary>
        /// <param name="attribute">The attribute that has been modified spatially.</param>
        /// <param name="originalValue">The original value of <see paramref="attribute"/>.</param>
        static void RecordSpatialChange(IAttribute attribute, SpatialString originalValue)
        {
            KeyValuePair<bool, SpatialString> existingModification;
            if (_attributesBeingModified.TryGetValue(attribute, out existingModification))
            {
                // If the attribute already exists in _attributesBeingModified, but is
                // marked as a non-spatial change, mark it as a spatial change instead.
                if (existingModification.Key == false)
                {
                    _attributesBeingModified[attribute] =
                        new KeyValuePair<bool, SpatialString>(
                            true, existingModification.Value);
                }
            }
            else
            {
                // If the attribute has not been added to _attributesBeingModified, add it.
                _attributesBeingModified[attribute] = new KeyValuePair<bool, SpatialString>(
                    true, originalValue);
            }
        }

        /// <summary>
        /// Raises the AttributeValueModified event.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose value was modified.</param>
        /// <param name="incrementalUpdate"><see langword="true"/>if the modification is part of an
        /// ongoing edit; <see langword="false"/> if the edit has finished.</param>
        /// <param name="acceptSpatialInfo"><see langword="true"/> if the modification should
        /// trigger the <see cref="IAttribute"/>'s spatial info to be accepted,
        /// <see langword="false"/> if the spatial info acceptance state should be left as is.
        /// </param>
        /// <param name="spatialInfoChanged"><see langword="true"/> if the spatial info for the
        /// <see cref="IAttribute"/> has changed, <see langword="false"/> if only the text has
        /// changed.</param>
        void OnAttributeValueModified(IAttribute attribute, bool incrementalUpdate,
            bool acceptSpatialInfo, bool spatialInfoChanged)
        {
            if (!incrementalUpdate && IsLoggingEnabled(LogCategories.AttributeUpdated))
            {
                Logger.LogEvent(LogCategories.AttributeUpdated, attribute, attribute.Value.String);
            }

            var eventHandler = AttributeValueModified;
            bool alreadyRaisingAttributeValueModified = _raisingAttributeValueModified;

            if (eventHandler != null)
            {
                try
                {
                    // [DataEntry:1133]
                    // If we are already raising the AttributeValueModified event, raise it again
                    // to ensure that any queries that depend on the value return the correct
                    // results. However, set incrementalUpdate to true in this case to prevent
                    // the change from causing the original auto-update trigger from being fired
                    // again (thus preventing infinite recursion).
                    incrementalUpdate |= alreadyRaisingAttributeValueModified;

                    _raisingAttributeValueModified = true;

                    eventHandler(this,
                        new AttributeValueModifiedEventArgs(
                            attribute, incrementalUpdate, acceptSpatialInfo, spatialInfoChanged));
                }
                finally
                {
                    if (!alreadyRaisingAttributeValueModified)
                    {
                        _raisingAttributeValueModified = false;
                    }
                }
            }
        }

        /// <summary>
        /// Raises the AttributeDeleted event.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> that was deleted.</param>
        void OnAttributeDeleted(IAttribute attribute)
        {
            if (IsLoggingEnabled(LogCategories.AttributeDeleted))
            {
                Logger.LogEvent(LogCategories.AttributeDeleted, attribute, attribute.Value.String);
            }

            var eventHandler = AttributeDeleted;
            if (eventHandler != null)
            {
                eventHandler(this, new AttributeDeletedEventArgs(attribute));
            }
        }

        #endregion Private Methods

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = this.GetType().GUID;
        }

        /// <summary>
        /// Checks if the object for changes since it was last saved.
        /// </summary>
        /// <returns><see langword="true"/> if the object has changes since it was last saved;
        /// <see langword="false"/> otherwise.</returns>
        public int IsDirty()
        {
            return _dirty;
        }

        /// <summary>
        /// Initializes an object from the <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the settings from the memory stream
                    _hasBeenViewed = reader.ReadBoolean();

                    if (reader.Version >= 2)
                    {
                        _isAccepted = reader.ReadBoolean();
                    }

                    if (reader.Version >= 3)
                    {
                        _hintEnabled = reader.ReadBoolean();
                    }
                }

                _dirty = HResult.False;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI24395", 
                    "Error loading AttributeStatusInfo settings.", ex);
            }
        }

        /// <summary>
        /// Saves the <see cref="AttributeStatusInfo"/> object into the specified 
        /// <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> and indicates whether the object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langref="true"/>, the flag should be cleared. If 
        /// <see langref="false"/>, the flag should be left unchanged.</param>
        public void Save(IStream stream, bool clearDirty)
        {
            try
            {
                ExtractException.Assert("ELI24397", "Memory stream is null!", stream != null);

                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Save the settings that cannot be reconstructed using the DataEntryControlHost
                    writer.Write(_hasBeenViewed);
                    writer.Write(_isAccepted);
                    writer.Write(_hintEnabled);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = HResult.False;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI24398", 
                    "Error saving data entry application settings.", ex);
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// <para>NOTE: Not implemented.</para>
        /// </summary>
        /// <param name="size">Will always be <see cref="HResult.NotImplemented"/> to indicate this
        /// method is not implemented.
        /// </param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion IPersistStream Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the current <see cref="AttributeStatusInfo"/> instance.
        /// </summary>
        /// <returns>A copy of the current <see cref="AttributeStatusInfo"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                AttributeStatusInfo clone = new AttributeStatusInfo();

                clone.CopyFrom(this);

                return clone;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI24909", "Clone failed.", ex);
            }
        }

        /// <summary>
        /// Copies the value of the provided <see cref="AttributeStatusInfo"/> instance into the 
        /// current one.
        /// </summary>
        /// <param name="pObject">The object to copy from.</param>
        /// <exception cref="ExtractException">If the supplied object is not of type
        /// <see cref="AttributeStatusInfo"/>.</exception>
        public void CopyFrom(object pObject)
        {
            try
            {
                ExtractException.Assert("ELI24983", "Cannot copy from an object of a different type!",
                    pObject.GetType() == this.GetType());

                AttributeStatusInfo source = (AttributeStatusInfo)pObject;

                // Copy fields from source
                _hasBeenPropagated = source._hasBeenPropagated;
                _isViewable = source._isViewable;
                _displayOrder = source._displayOrder;
                _hasBeenViewed = source._hasBeenViewed;
                _dataValidity = source._dataValidity;
                _owningControl = source._owningControl;
                _isMapped = source._isMapped;
                _hintType = source._hintType;
                _hintRasterZones = source._hintRasterZones;
                _isAccepted = source._isAccepted;
                _hintEnabled = source._hintEnabled;
                _parentAttribute = source._parentAttribute;
                _tabStopMode = source._tabStopMode;
                _persistAttribute = source._persistAttribute;
                
                // _raisingAttributeValueModified is intentionally not copied.
                // _validator cannot be copied; Each control instance must supply its own.
                // Do not copy _autoUpdateQuery or _validationQuery since a query is specified to
                // its location in the attribute hierarchy and persisting the query values will
                // prevent new triggers from being created when the attribute is re-initialized.
                _raisingAttributeValueModified = false;
                _validator = null;
                _autoUpdateQuery = null;
                _validationQuery = null;
                _lastAppliedStringValue = null;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI24911", 
                    "Unable to copy AttributeStatusInfo", ex);
            }
        }

        #endregion ICopyableObject Members
    }
}