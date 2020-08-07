using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// An <see cref="CheckBox"/> and <see cref="IDataEntryControl"/> extension that allows easy
    /// implementation of custom actions in a DEP using a CheckBox that supports validation queries
    /// and/or queries to update the CheckBox text.
    /// </summary>
    public partial class DataEntryCheckBox : CheckBox, ICheckBoxObject, IDataEntryAttributeControl, IRequiresErrorProvider
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataEntryCheckBox).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name used to identify the <see cref="IAttribute"/> to be associated with the CheckBox.
        /// If not specified, a placeholder name will be assigned and PersistAttribute will be
        /// assumed to be false.
        /// </summary>
        string _attributeName;

        /// <summary>
        /// The attribute mapped to this control.
        /// </summary>
        IAttribute _attribute;

        /// <summary>
        /// The <see cref="DataEntryControlHost"/> to which this control belongs
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// Used to specify the data entry control which is mapped to the parent of the attribute 
        /// to which this CheckBox is to be mapped.
        /// </summary>
        IDataEntryControl _parentDataEntryControl;

        /// <summary>
        /// The selection mode to use when multiple attributes are found which match the attribute 
        /// name for this control.
        /// </summary>
        MultipleMatchSelectionMode _multipleMatchSelectionMode =
            MultipleMatchSelectionMode.None;

        /// <summary>
        /// Specifies under what circumstances the control's attribute should serve as a tab stop.
        /// </summary>
        TabStopMode _tabStopMode = TabStopMode.Always;

        /// <summary>
        /// Specifies whether the control should remain disabled at all times.
        /// </summary>
        bool _disabled;

        /// <summary>
        /// Specifies whether descendant attributes in other controls should be highlighted when
        /// this attribute is selected.
        /// </summary>
        bool _highlightSelectionInChildControls;

        /// <summary>
        /// The error provider to be used to indicate data validation problems to the user.
        /// </summary>
        ErrorProvider _validationErrorProvider;

        /// <summary>
        /// The error provider to be used to indicate data validation warnings to the user.
        /// </summary>
        ErrorProvider _validationWarningErrorProvider;

        /// <summary>
        /// The template object to be used as a model for per-attribute validation objects.
        /// </summary>
        DataEntryValidator _validatorTemplate = new DataEntryValidator();

        /// <summary>
        /// The validator currently being used to validate the control's attribute.
        /// </summary>
        IDataEntryValidator _activeValidator;

        /// <summary>
        /// A query which will cause value to automatically be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        string _autoUpdateQuery;

        /// <summary>
        /// A query which will cause the validation list to be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        string _validationQuery;

        /// <summary>
        /// Specifies whether the mapped attribute's value should be saved.
        /// </summary>
        bool _persistAttribute = true;

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        /// <summary>
        /// Indicates whether the text box is currently active.
        /// </summary>
        bool _isActive;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryCheckBox"/> class.
        /// </summary>
        public DataEntryCheckBox()  
            : base()
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
                    LicenseIdName.DataEntryCoreComponents, "ELI50203", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50204", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name identifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryCheckBox"/>.
        /// <para><b>NOTE</b></para>
        /// If not specified, a placeholder name will be assigned and PersistAttribute will be
        /// assumed to be false.
        /// </summary>
        /// <value>Sets the name identifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryCheckBox"/>.
        /// </value>
        /// <returns>The name identifying the <see cref="IAttribute"/> to be associated with the 
        /// <see cref="DataEntryCheckBox"/>.</returns>
        [Category("Data Entry CheckBox")]
        public string AttributeName
        {
            get
            {
                return _attributeName;
            }

            set
            {
                _attributeName = value;
            }
        }

        /// <summary>
        /// Gets or sets the selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryCheckBox"/>.
        /// </summary>
        /// <value>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryCheckBox"/>.</value>
        /// <returns>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryCheckBox"/>.</returns>
        [Category("Data Entry CheckBox")]
        [DefaultValue(MultipleMatchSelectionMode.None)]
        public MultipleMatchSelectionMode MultipleMatchSelectionMode
        {
            get
            {
                return _multipleMatchSelectionMode;
            }

            set
            {
                try
                {
                    if (value != _multipleMatchSelectionMode)
                    {
                        ExtractException.Assert("ELI50205",
                            "Invalid MultipleMatchSelectionMode for a CheckBox.",
                            value != MultipleMatchSelectionMode.All);

                        _multipleMatchSelectionMode = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI50206");
                }
            }
        }

        /// <summary>
        /// Not supported by DataEntryCheckBox; will always be <c>null</c>.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string ValidationPattern
        {
            get
            {
                return null;
            }

            set
            {
                ExtractException.Assert("ELI50223",
                    "ValidationPattern is not supported for the CheckBox control; use ValidationQuery if validation is required",
                    string.IsNullOrEmpty(value));
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the CheckBox's validation list to be automatically
        /// updated using values from other <see cref="IAttribute"/>'s and/or a database query.
        /// Every time an attribute specified in the query is modified, this query will be 
        /// re-evaluated and used to update the validation list.
        /// </summary>
        /// <value>A query which will cause the validation list to be automatically updated using
        /// values from other <see cref="IAttribute"/>'s and/or a database query.</value>
        /// <returns>A query being used to automatically update the validation list using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</returns>
        [Category("Data Entry CheckBox")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor)), Localizable(true)]
        [DefaultValue(null)]
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
        /// Gets or sets the error message that should be displayed if validation of this field fails
        /// NOTE: Only <see cref="ValidationQuery"/> is supported for check box validation
        /// (<see cref="ValidationPattern"/> cannot be used)
        /// </summary>
        [Category("Data Entry CheckBox")]
        public string ValidationErrorMessage
        {
            get
            {
                try
                {
                    return _validatorTemplate.ValidationErrorMessage;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI50209", ex);
                }
            }

            set
            {
                try
                {
                    _validatorTemplate.ValidationErrorMessage = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI50210", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the CheckBox's value to automatically be updated
        /// using values from other <see cref="IAttribute"/>'s and/or a database query". Every time
        /// an attribute specified in the query is modified, this query will be re-evaluated and
        /// used to update the value.
        /// </summary>
        /// <value>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</value>
        /// <returns>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</returns>
        [Category("Data Entry CheckBox")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor)), Localizable(true)]
        [DefaultValue(null)]
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
        /// Specifies under what circumstances the <see cref="DataEntryCheckBox"/>'s
        /// <see cref="IAttribute"/> should serve as a tab stop.
        /// </summary>
        /// <value>A <see cref="TabStopMode"/> value indicating when the attribute should serve as a
        /// tab stop.</value>
        /// <returns>A <see cref="TabStopMode"/> value indicating when the attribute will serve as a
        /// tab stop.</returns>
        [Category("Data Entry CheckBox")]
        [DefaultValue(TabStopMode.Always)]
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
        /// Gets or sets whether the mapped <see cref="IAttribute"/>'s value should be saved.
        /// </summary>
        /// <value><see langword="true"/> to save the attribute's value; <see langword="false"/>
        /// otherwise.
        /// </value>
        /// <returns><see langword="true"/> if the attribute's value will be saved;
        /// <see langword="false"/> otherwise.</returns>
        [Category("Data Entry CheckBox")]
        [DefaultValue(true)]
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
        /// The attribute value representing this control in the checked state.
        /// The comparison will be made case-insensitively when setting the checked state based on
        /// attribute value.
        /// </summary>
        [Category("Data Entry CheckBox")]
        [DefaultValue("True")]
        public string CheckedValue
        {
            get;
            set;
        } = "True";

        /// <summary>
        /// The attribute value representing this control in the un-checked state.
        /// The comparison will be made case-insensitively when setting the checked state based on
        /// attribute value.
        /// </summary>
        [Category("Data Entry CheckBox")]
        [DefaultValue("False")]
        public string UncheckedValue
        {
            get;
            set;
        } = "False";

        /// <summary>
        /// <c>true</c> if this check box should default to the checked state if the attribute value
        /// matches neither the <see cref="CheckedValue"/> nor <see cref="UncheckedValue"/>.
        /// </summary>
        [Category("Data Entry CheckBox")]
        [DefaultValue(false)]
        public bool DefaultCheckedState
        {
            get;
            set;
        }

        #endregion Properties

        #region IRequiresErrorProvider Members

        /// <summary>
        /// Specifies the standard <see cref="ErrorProvider"/>s that should be used to 
        /// display data validation errors.
        /// </summary>
        /// <param name="validationErrorProvider">The standard <see cref="ErrorProvider"/> that
        /// should be used to display data validation errors.</param>
        /// <param name="validationWarningErrorProvider">The <see cref="ErrorProvider"/> that should
        /// be used to display data validation warnings.</param>
        public void SetErrorProviders(ErrorProvider validationErrorProvider,
            ErrorProvider validationWarningErrorProvider)
        {
            _validationErrorProvider = validationErrorProvider;
            _validationWarningErrorProvider = validationWarningErrorProvider;
        }

        #endregion IRequiresErrorProvider Members

        #region IDataEntryControl Members

        /// <summary>
        /// Fired whenever the set of selected or active <see cref="IAttribute"/>(s) for a control
        /// changes. This can occur as part of the <see cref="PropagateAttributes"/> event, when
        /// new attribute(s) are created via a swipe or when a new element of the control becomes 
        /// active.
        /// </summary>
        public event EventHandler<AttributesSelectedEventArgs> AttributesSelected;

        /// <summary>
        /// Fired to request that the and <see cref="IAttribute"/> or <see cref="IAttribute"/>(s) be
        /// propagated to any dependent controls.  This will be in response to an 
        /// <see cref="IAttribute"/> having been modified (i.e., via a swipe or loading a document). 
        /// The event will provide the updated <see cref="IAttribute"/>(s) to registered listeners.
        /// </summary>
        public event EventHandler<AttributesEventArgs> PropagateAttributes;

        /// <summary>
        /// Gets or sets the <see cref="DataEntryControlHost"/> to which this CheckBox belongs
        /// </summary>
        /// <value>The <see cref="DataEntryControlHost"/> to which this CheckBox belongs.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataEntryControlHost DataEntryControlHost
        {
            get
            {
                return _dataEntryControlHost;
            }
            set
            {
                _dataEntryControlHost = value;
            }
        }

        /// <summary>
        /// If the <see cref="DataEntryCheckBox"/> is not intended to operate on root-level data, 
        /// this property must be used to specify the <see cref="IDataEntryControl"/> which is 
        /// mapped to the parent of the <see cref="IAttribute"/>(s) to which the current 
        /// <see cref="DataEntryCheckBox"/> is to be mapped.  The specified 
        /// <see cref="IDataEntryControl"/> must be contained in the same 
        /// <see cref="DataEntryControlHost"/> as this <see cref="DataEntryCheckBox"/>. If the 
        /// <see cref="DataEntryCheckBox"/> is to be mapped to a root-level <see cref="IAttribute"/>, 
        /// this property must be set to <see langref="null"/>.</summary>
        /// <seealso cref="IDataEntryControl"/>
        [Category("Data Entry CheckBox")]
        [DefaultValue(null)]
        public IDataEntryControl ParentDataEntryControl
        {
            get
            {
                return _parentDataEntryControl;
            }

            set
            {
                _parentDataEntryControl = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the control should remain disabled at all times.
        /// <para><b>Note</b></para>
        /// If disabled, mapped data will not be validated.
        /// </summary>
        /// <value><see langword="true"/> if the control should remain disabled,
        /// <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if the control will remain disabled,
        /// <see langword="false"/> otherwise.</returns>
        [Category("Data Entry CheckBox")]
        [DefaultValue(false)]
        public bool Disabled
        {
            get
            {
                return _disabled;
            }

            set
            {
                _disabled = value;
            }
        }

        /// <summary>
        /// Gets or sets whether descendant attributes in other controls should be highlighted when
        /// this attribute is selected.
        /// </summary>
        /// <value><see langword="true"/> if descendant attributes in other controls should be
        /// highlighted when this attribute is selected; <see langword="false"/> otherwise.</value>
        [Category("Data Entry Control")]
        [DefaultValue(false)]
        public bool HighlightSelectionInChildControls
        {
            get
            {
                return _highlightSelectionInChildControls;
            }

            set
            {
                _highlightSelectionInChildControls = value;
            }
        }

        /// <summary>
        /// Specifies the domain of <see cref="IAttribute"/>s from which the 
        /// <see cref="IDataEntryControl"/> should find the <see cref="IAttribute"/> 
        /// to which it should be mapped. <see cref="DataEntryCheckBox"/> will always use the
        /// first match (if available).
        /// </summary>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> instance of 
        /// <see cref="IAttribute"/>s from which the <see cref="DataEntryCheckBox"/> 
        /// should find its corresponding <see cref="IAttribute"/>.  Can be an empty vector, but 
        /// must not be <see langword="null"/>.</param>
        /// <seealso cref="IDataEntryControl"/>
        public void SetAttributes(IUnknownVector sourceAttributes)
        {
            try
            {
                // [DataEntry:298]
                // If the CheckBox has nothing to propagate, disable it since any data entered would
                // not be mapped into the attribute hierarchy.
                // Also, prevent it from being enabled if explicitly disabled via the
                // IDataEntryControl interface.
                Enabled = (sourceAttributes != null && !_disabled);

                if (sourceAttributes == null)
                {
                    // If the source attribute is null, clear existing data and do not attempt to
                    // map to a new attribute.
                    _attribute = null;
                }
                else
                {
                    // Attempt to find a mapped attribute from the provided vector.  Create a new 
                    // attribute if no such attribute can be found.
                    _attribute = DataEntryMethods.InitializeAttribute(_attributeName,
                        _multipleMatchSelectionMode, !string.IsNullOrEmpty(_attributeName),
                        sourceAttributes, null, this, null, false, _tabStopMode, _validatorTemplate,
                        _autoUpdateQuery, _validationQuery);

                    // Update the combo box using the new attribute's validator (if there is one).
                    if (_attribute != null)
                    {
                        _activeValidator = AttributeStatusInfo.GetStatusInfo(_attribute).Validator;
                    }

                    if (Visible)
                    {
                        // Mark the attribute as visible if the CheckBox is visible
                        AttributeStatusInfo.MarkAsViewable(_attribute, true);

                        // [DataEntry:327] If this control is active, ensure the attribute is
                        // marked as viewed.
                        if (_isActive)
                        {
                            AttributeStatusInfo.MarkAsViewed(_attribute, true);
                        }
                    }

                    // If not persisting the attribute, mark the attribute accordingly.
                    if (!_persistAttribute)
                    {
                        AttributeStatusInfo.SetAttributeAsPersistable(_attribute, false);
                    }
                }

                // Raise the OnAttributeChanged event to notify the host of the currently selected
                // attribute.
                OnAttributeChanged();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50211", ex);
            }
        }

        /// <summary>
        /// Handles the case that this <see cref="DataEntryCheckBox"/>'s 
        /// <see cref="ParentDataEntryControl"/> has requested that a new <see cref="IAttribute"/> 
        /// be propagated.  The <see cref="DataEntryCheckBox"/> will re-map its control appropriately.
        /// The <see cref="DataEntryCheckBox"/> will mark the <see cref="AttributeStatusInfo"/> instance 
        /// associated with any <see cref="IAttribute"/> it propagates as propagated.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="AttributesEventArgs"/> that contains the event data.
        /// </param>
        /// <seealso cref="IDataEntryControl"/>
        public void HandlePropagateAttributes(object sender, AttributesEventArgs e)
        {
            try
            {
                // An attribute can be mapped if there is one and only one attribute to be
                // propagated.
                if (e.Attributes != null && e.Attributes.Size() == 1)
                {
                    Enabled = !_disabled;

                    // This is a dependent child to the sender. Re-map this control using the
                    // updated attribute's children.
                    IAttribute propagatedAttribute = (IAttribute)e.Attributes.At(0);

                    // If we found a single attribute to use for mapping, mark the attribute
                    // as propagated.
                    AttributeStatusInfo.MarkAsPropagated(propagatedAttribute, true, false);

                    // If an attribute name is specified, this is a "child" control intended to act on
                    // a sub-attribute of the provided attribute. If no attribute name is specified,
                    // this is a "sibling" control likely intended to display details of the currently
                    // selected attribute in a multi-selection control.
                    if (!string.IsNullOrEmpty(_attributeName))
                    {
                        SetAttributes(propagatedAttribute.SubAttributes);
                    }
                    else
                    {
                        // This is a dependent sibling to the sender. Re-map this control using the
                        // updated attribute itself.
                        SetAttributes(e.Attributes);
                    }
                }
                else
                {
                    // If there is more than one parent attribute or no parent attributes, the group
                    // box cannot be mapped and should propagate null so all dependent controls are
                    // unmapped.
                    SetAttributes(null);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50212", ex);
            }
        }

        /// <summary>
        /// This method has no effect for a <see cref="DataEntryCheckBox"/> except to validate that
        /// the specified <see cref="IAttribute"/> is mapped.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose sub-attributes should be
        /// propagated to any child controls. If <see langword="null"/>, the currently selected
        /// <see cref="IAttribute"/> should be repropagated if there is a single attribute.
        /// <para><b>Requirements</b></para>If non-<see langword="null"/>, the specified 
        /// <see cref="IAttribute"/> must be known to be mapped to the 
        /// <see cref="IDataEntryControl"/>.</param>
        /// <param name="selectAttribute">If <see langword="true"/>, the specified 
        /// <see cref="IAttribute"/> will also be selected within the <see cref="IDataEntryControl"/>.
        /// If <see langword="false"/>, the previous selection will remain even if a different
        /// <see cref="IAttribute"/> was propagated.
        /// </param>
        /// <param name="selectTabGroup">If <see langword="true"/> all <see cref="IAttribute"/>s in
        /// the specified <see cref="IAttribute"/>'s tab group are to be selected,
        /// <see langword="false"/> otherwise.</param>
        /// <seealso cref="IDataEntryControl"/>
        public void PropagateAttribute(IAttribute attribute, bool selectAttribute,
            bool selectTabGroup)
        {
            ExtractException.Assert("ELI50213", "Unexpected attribute!",
                attribute == null || attribute == _attribute);
        }

        /// <summary>
        /// <see cref="DataEntryCheckBox"/> does not have any implementation for this method.
        /// </summary>
        public virtual void RefreshAttributes()
        {
            RefreshAttributes(true, _attribute);
        }

        /// <summary>
        /// <see cref="DataEntryCheckBox"/> does not have any implementation for this method.
        /// </summary>
        /// <param name="spatialInfoUpdated"><see langword="true"/> if the attribute's spatial info
        /// has changed so that hints can be updated; <see langword="false"/> if the attribute's
        /// spatial info has not changed.</param>
        /// <param name="attributes">The <see cref="IAttribute"/>s whose values should be refreshed.
        /// </param>
        public void RefreshAttributes(bool spatialInfoUpdated, params IAttribute[] attributes)
        {
            try
            {
                if (_attribute != null && attributes.Contains(_attribute))
                {
                    UpdateCheckBoxState();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50214", ex);
            }
        }

        /// <summary>
        /// Gets the UI element associated with the specified <see paramref="attribute"/>. This may
        /// be a type of <see cref="Control"/> or it may also be <see cref="DataGridViewElement"/>
        /// such as a <see cref="DataGridViewCell"/> if the <see paramref="attribute"/>'s owning
        /// control is a table control.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the UI element is needed.
        /// </param>
        /// <returns>The UI element.</returns>
        public object GetAttributeUIElement(IAttribute attribute)
        {
            return this;
        }

        /// <summary>
        /// Creates a <see cref="DataEntryCheckBoxBackgroundFieldModel"/> for representing this control during
        /// a background data load.
        /// </summary>
        public BackgroundFieldModel GetBackgroundFieldModel()
        {
            try
            {
                var fieldModel = new DataEntryCheckBoxBackgroundFieldModel()
                {
                    Name = AttributeName,
                    ParentAttributeControl = ParentDataEntryControl,
                    AutoUpdateQuery = AutoUpdateQuery,
                    ValidationQuery = ValidationQuery,
                    DisplayOrder = DataEntryMethods.GetTabIndices(this),
                    IsViewable = Visible,
                    ValidationErrorMessage = ValidationErrorMessage,
                    PersistAttribute = PersistAttribute,
                    ValidationPattern = ValidationPattern,

                    // DataEntryCheckBoxBackgroundFieldModel specific fields
                    CheckedValue = CheckedValue,
                    UncheckedValue = UncheckedValue,
                    DefaultCheckedState = DefaultCheckedState
                };

                return fieldModel;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50218");
            }
        }

        #endregion IDataEntryControl Members

        #region IDataEntryAttributeControl Properties

        /// <summary>
        /// The attribute mapped to this control.
        /// </summary>
        public IAttribute Attribute => _attribute;

        #endregion IDataEntryAttributeControl Properties

        #region Unused IDataEntryControl Members

        /// <summary>
        /// This event is not raised by <see cref="DataEntryCheckBox"/>.
        /// </summary>
        public event EventHandler<SwipingStateChangedEventArgs> SwipingStateChanged
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryControl interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// This event is not raised by <see cref="DataEntryCheckBox"/>.
        /// </summary>
        public event EventHandler<QueryDraggedDataSupportedEventArgs> QueryDraggedDataSupported
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryControl interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// This event is not raised by <see cref="DataEntryCheckBox"/>.
        /// </summary>
        public event EventHandler<EventArgs> UpdateStarted
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryControl interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// This event is not raised by <see cref="DataEntryCheckBox"/>.
        /// </summary>
        public event EventHandler<EventArgs> UpdateEnded
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryControl interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// <see cref="DataEntryCheckBox"/> does not support swiping; the value of this property
        /// will always be <see langword="false"/>.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SupportsSwiping
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                {
                    throw new ExtractException("ELI50215",
                        "DataEntryCheckBox does not support swiping!");
                }
            }
        }

        /// <summary>
        /// <see cref="DataEntryCheckBox"/> does not support pasting; the value of this property
        /// will always be <see langword="false"/>.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ClearClipboardOnPaste
        {
            get
            {
                return false;
            }

            set
            {
                throw new ExtractException("ELI50216",
                    "DataEntryCheckBox does not support pasting!");
            }
        }

        /// <summary>
        /// Activates or inactivates the <see cref="DataEntryTextBox"/>.
        /// </summary>
        /// <param name="setActive">If <see langref="true"/>, the <see cref="DataEntryTextBox"/>
        /// should visually indicate that it is active. If <see langref="false"/> the 
        /// <see cref="DataEntryTextBox"/> should not visually indicate that it is active.</param>
        /// <param name="color">The <see cref="Color"/> that should be used to indicate active 
        /// status (unused if setActive is <see langword="false"/>).</param>
        /// <seealso cref="IDataEntryControl"/>
        public void IndicateActive(bool setActive, Color color)
        {
            try
            {
                _isActive = (setActive && _attribute != null);

                BackColor = _isActive ? color : Color.Transparent;

                Invalidate();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50222");
            }
        }

        /// <summary>
        /// <see cref="DataEntryCheckBox"/> does not have any implementation for this method.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if called.</throws>
        /// <param name="swipedText">Unused.</param>
        public bool ProcessSwipedText(UCLID_RASTERANDOCRMGMTLib.SpatialString swipedText)
        {
            throw new ExtractException("ELI50217", "DataEntryCheckBox does not support swiping!");
        }

        /// <summary>
        /// <see cref="DataEntryCheckBox"/> doesn't cache any data.
        /// </summary>
        public void ClearCachedData()
        {
        }

        /// <summary>
        /// <see cref="DataEntryCheckBox"/> does not have any implementation for this method.
        /// <param name="selectionState">Unused.</param>
        /// </summary>
        public void ApplySelection(SelectionState selectionState)
        {
        }

        #endregion Unused IDataEntryControl Members

        #region Overrides

        /// <summary>
        /// Raises the <see cref="CheckBox.CheckedChanged"/> event.
        /// Overridden in order to update the attribute value based on the new checked state.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCheckedChanged(EventArgs e)
        {
            try
            {
                base.OnCheckedChanged(e);

                if (_attribute != null)
                {
                    AttributeStatusInfo.SetValue(_attribute, 
                        Checked ? CheckedValue : UncheckedValue, 
                        acceptSpatialInfo: false,
                        endOfEdit: true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50219");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.VisibleChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            try
            {
                base.OnVisibleChanged(e);

                // https://extract.atlassian.net/browse/ISSUE-12812
                // If the visibility of this control is changed after document load, the viewable
                // status of the attribute needs to be updated so that highlights and validation
                // are applied correctly.
                if (_attribute != null &&
                    DataEntryControlHost != null && !DataEntryControlHost.ChangingData)
                {
                    AttributeStatusInfo.MarkAsViewable(_attribute, Visible);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50220");
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed;
        /// otherwise, <see langword="false"/>.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Private Members

        /// <overloads>
        /// Tests to see if the provided <see cref="IAttribute"/> meets all specified 
        /// validation requirements the implementing class has.
        /// </overloads>
        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> meets all specified 
        /// validation requirements the implementing class has.
        /// </summary>
        /// <returns>A <see cref="DataValidity"/>value indicating whether the mapped attribute's
        /// value is currently valid.
        /// </returns>
        /// <throws><see cref="DataEntryValidationException"/> if the <see cref="IAttribute"/>'s 
        /// data fails to match any validation requirements it has and
        /// <see paramref="throwException"/> is <see langword="true"/></throws>
        public DataValidity Validate()
        {
            try
            {
                // If there is no mapped attribute, the control is disabled or there is not active
                // validator, the data cannot be validated.
                if (_disabled || _attribute == null || _activeValidator == null)
                {
                    return DataValidity.Valid;
                }

                DataValidity dataValidity = _activeValidator.Validate(_attribute, false);

                return dataValidity;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50221", ex);
            }
        }

        /// <summary>
        /// Updates the text value of this control and raise the events that need to be raised
        /// in conjunction with an attribute change (<see cref="AttributesSelected"/> and
        /// <see cref="PropagateAttributes"/>).
        /// </summary>
        void OnAttributeChanged()
        {
            UpdateCheckBoxState();

            // Raise the PropagateAttributes event so that any descendants of this data control can
            // re-map themselves.
            OnPropagateAttributes();
        }

        /// <summary>
        /// Updates the checked state of this check box based on the current attribute value.
        /// </summary>
        void UpdateCheckBoxState()
        {
            if (_attribute != null)
            {
                Checked = NormalizeValue(this, _attribute);
            }

            // In case the value itself didn't change but the validation list did,
            // explicitly check validation here.
            UpdateValidation();

            // Raise the AttributesSelected event to re-notify the host of the spatial
            // information associated with the attribute in case the spatial info has
            // changed.
            OnAttributesSelected();
        }

        /// <summary>
        /// This control will force any attribute value (or lack of value) to either
        /// <see cref="CheckedValue"/> or <see cref="UncheckedValue"/>. This method sets the
        /// <see paramref="attribute"/> value according to the <see paramref="checkBox"/>
        /// configuration.
        /// </summary>
        /// <returns><c>true</c> if the attribute value results in the control being in the checked
        /// state; <c>false</c> if it results in the control being in the unchecked state.</returns>
        static bool NormalizeValue(ICheckBoxObject checkBox, IAttribute attribute)
        {
            try
            {
                bool checkedState;
                var stringValue = attribute?.Value.String;

                if (stringValue.Equals(checkBox.CheckedValue, StringComparison.OrdinalIgnoreCase))
                {
                    checkedState = true;
                    stringValue = checkBox.CheckedValue;
                }
                else if (stringValue.Equals(checkBox.UncheckedValue, StringComparison.OrdinalIgnoreCase))
                {
                    checkedState = false;
                    stringValue = checkBox.UncheckedValue;
                }
                else
                {
                    checkedState = checkBox.DefaultCheckedState;
                    stringValue = checkedState ? checkBox.CheckedValue : checkBox.UncheckedValue;
                }

                AttributeStatusInfo.SetValue(attribute, stringValue,
                    acceptSpatialInfo: false, endOfEdit: true);

                return checkedState;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50245");
            }
        }

        /// <summary>
        /// Re-validates the control's data and updates validation error icon as appropriate.
        /// </summary>
        void UpdateValidation()
        {
            if (_validationErrorProvider != null || _validationWarningErrorProvider != null)
            {
                DataValidity dataValidity = Validate();

                if (_validationErrorProvider != null)
                {
                    _validationErrorProvider.SetError(this, dataValidity == DataValidity.Invalid ?
                        _activeValidator.ValidationErrorMessage : "");
                }

                if (_validationWarningErrorProvider != null)
                {
                    _validationWarningErrorProvider.SetError(this,
                        dataValidity == DataValidity.ValidationWarning ?
                            _activeValidator.ValidationErrorMessage : "");
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="AttributesSelected"/> event.
        /// </summary>
        void OnAttributesSelected()
        {
            if (AttributesSelected != null)
            {
                var selectionState = SelectionState.Create(this);
                AttributesSelected(this, new AttributesSelectedEventArgs(selectionState));
            }
        }

        /// <summary>
        /// Raises the <see cref="PropagateAttributes"/> event.
        /// </summary>
        void OnPropagateAttributes()
        {
            if (PropagateAttributes != null)
            {
                if (_attribute == null)
                {
                    // If the CheckBox is not currently mapped to an attribute, it should propagate
                    // null so all dependent controls are unmapped.
                    PropagateAttributes(this,
                        new AttributesEventArgs(null));
                }
                else
                {
                    // Propagate the mapped attribute.
                    PropagateAttributes(this,
                        new AttributesEventArgs(
                            DataEntryMethods.AttributeAsVector(_attribute)));
                }
            }
            else if (_attribute != null)
            {
                // If there are no dependent controls registered to receive this event, consider
                // the attribute propagated.
                AttributeStatusInfo.MarkAsPropagated(_attribute, true, true);
            }
        }

        #endregion Private Members
    }
}
