using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
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
    /// An <see cref="Button"/> and <see cref="IDataEntryControl"/> extension that allows easy
    /// implementation of custom actions in a DEP using a button that supports validation queries
    /// and/or queries to update the button text.
    /// </summary>
    public partial class DataEntryButton : Button, IDataEntrySingleAttributeControl, IRequiresErrorProvider
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataEntryButton).ToString();

        /// <summary>
        /// The default flash rate in milliseconds.
        /// </summary>
        const int _DEFAULT_FLASH_INTERVAL = 500;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name used to identify the <see cref="IAttribute"/> to be associated with the button.
        /// If not specified, a placeholder name will be assigned and PersistAttribute will be
        /// assumed to be false.
        /// </summary>
        string _attributeName;

        /// <summary>
        /// Indicates whether the button was explicitly assigned an attribute name. Otherwise, a
        /// placeholder name will be assigned.
        /// </summary>
        bool? _hasSpecifiedAttributeName;

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
        /// to which this button is to be mapped.
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
        bool _persistAttribute;

        /// <summary>
        /// Indicates whether the button is currently flashing.
        /// </summary>
        bool _isFlashing;

        /// <summary>
        /// A timer used to trigger alternating the background color for the purposes of
        /// implementing the <see cref="Flash"/> property.
        /// </summary>
        Timer _flashTimer = new Timer();

        /// <summary>
        /// When flashing, keeps track of the normal background color.
        /// </summary>
        Color _normalBackgroundColor;

        /// <summary>
        /// When flashing, keeps track of the background color that should alternate with
        /// <see cref="_normalBackgroundColor"/>.
        /// </summary>
        Color _flashingBackgroundColor;

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryButton"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        public DataEntryButton()
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
                    LicenseIdName.DataEntryCoreComponents, "ELI37378", _OBJECT_NAME);

                _flashTimer.Interval = _DEFAULT_FLASH_INTERVAL;

                InitializeComponent();

                _flashTimer.Tick += new EventHandler(HandleFlashTimer_Tick);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI37354", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name identifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryButton"/>.
        /// <para><b>NOTE</b></para>
        /// If not specified, a placeholder name will be assigned and PersistAttribute will be
        /// assumed to be false.
        /// </summary>
        /// <value>Sets the name identifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryButton"/>.
        /// </value>
        /// <returns>The name identifying the <see cref="IAttribute"/> to be associated with the 
        /// <see cref="DataEntryButton"/>.</returns>
        [Category("Data Entry Button")]
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
        /// <see cref="DataEntryButton"/>.
        /// </summary>
        /// <value>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryButton"/>.</value>
        /// <returns>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryButton"/>.</returns>
        [Category("Data Entry Button")]
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
                        ExtractException.Assert("ELI37355",
                            "Invalid MultipleMatchSelectionMode for a button.",
                            value != MultipleMatchSelectionMode.All);

                        _multipleMatchSelectionMode = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37368");
                }
            }
        }

        /// <summary>
        /// Gets or set a regular expression the data entered in a control must match prior to being 
        /// saved.
        /// </summary>
        /// <value>A regular expression the data entered in a control must match prior to being 
        /// saved. <see langword="null"/> to remove any existing validation pattern requirement.</value>
        /// <returns>A regular expression the data entered in a control must match prior to being 
        /// saved. <see langword="null"/> if there is no validation pattern set.</returns>
        [Category("Data Entry Button")]
        [DefaultValue(null)]
        public string ValidationPattern
        {
            get
            {
                try
                {
                    return _validatorTemplate.ValidationPattern;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI37358", ex);
                }
            }

            set
            {
                try
                {
                    _validatorTemplate.ValidationPattern = value;
                }
                catch (ExtractException ex)
                {
                    throw ExtractException.AsExtractException("ELI37359", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the button's validation list to be automatically
        /// updated using values from other <see cref="IAttribute"/>'s and/or a database query.
        /// Every time an attribute specified in the query is modified, this query will be 
        /// re-evaluated and used to update the validation list.
        /// </summary>
        /// <value>A query which will cause the validation list to be automatically updated using
        /// values from other <see cref="IAttribute"/>'s and/or a database query.</value>
        /// <returns>A query being used to automatically update the validation list using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</returns>
        [Category("Data Entry Button")]
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
        /// Gets or sets the error message that should be displayed until the button has been clicked.
        /// </summary>
        /// <value>The tooltip text that should be displayed for an error provider icon until the
        /// button has been clicked.</value>
        /// <returns>The tooltip text that is be displayed for an error provider icon until the
        /// button has been clicked.</returns>
        [Category("Data Entry Button")]
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
                    throw ExtractException.AsExtractException("ELI37356", ex);
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
                    throw ExtractException.AsExtractException("ELI37357", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the button's value to automatically be updated
        /// using values from other <see cref="IAttribute"/>'s and/or a database query". Every time
        /// an attribute specified in the query is modified, this query will be re-evaluated and
        /// used to update the value.
        /// </summary>
        /// <value>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</value>
        /// <returns>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</returns>
        [Category("Data Entry Button")]
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
        /// Specifies under what circumstances the <see cref="DataEntryButton"/>'s
        /// <see cref="IAttribute"/> should serve as a tab stop.
        /// </summary>
        /// <value>A <see cref="TabStopMode"/> value indicating when the attribute should serve as a
        /// tab stop.</value>
        /// <returns>A <see cref="TabStopMode"/> value indicating when the attribute will serve as a
        /// tab stop.</returns>
        [Category("Data Entry Button")]
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
        [Category("Data Entry Button")]
        [DefaultValue(false)]
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
        /// Gets or sets a value indicating whether this <see cref="DataEntryButton"/> is flashing.
        /// </summary>
        /// <value><see langword="true"/> if flashing; otherwise, <see langword="false"/>.
        /// </value>
        [Category("Data Entry Button")]
        [DefaultValue(false)]
        public bool Flash
        {
            get
            {
                return _isFlashing;
            }

            set
            {
                try
                {
                    if (value != _isFlashing)
                    {
                        _isFlashing = value;

                        if (!_inDesignMode)
                        {
                            if (value)
                            {
                                // When flashing the button's background color should alternate
                                // between the previously specified background color and one that
                                // is lighter than the specified background color.
                                _normalBackgroundColor = BackColor;
                                _flashingBackgroundColor = (FlashColor == Color.Empty)
                                    ? ControlPaint.LightLight(_normalBackgroundColor)
                                    : FlashColor;
                                _flashTimer.Enabled = true;
                            }
                            else
                            {
                                _flashTimer.Enabled = false;
                                base.BackColor = _normalBackgroundColor;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI37375");
                }
            }
        }

        /// <summary>
        /// Gets or sets the color that should be alternated with <see cref="Control.BackColor"/>
        /// when <see cref="Flash"/> is <see langword="true"/>. If <see cref="Color.Empty"/>, a
        /// lighter version of the <see cref="BackColor"/> will be used.
        /// </summary>
        /// <value>
        /// </value>
        [Category("Data Entry Button")]
        [DefaultValue("Empty")]
        public Color FlashColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the rate in milliseconds that the button will flash when
        /// <see cref="Flash"/> is <see langword="true"/>.
        /// </summary>
        /// <value>
        /// The rate in milliseconds that the button will flash when <see cref="Flash"/> is 
        /// <see langword="true"/>.
        /// </value>
        [Category("Data Entry Button")]
        [DefaultValue(_DEFAULT_FLASH_INTERVAL)]
        public int FlashInterval
        {
            get
            {
                return _flashTimer.Interval;
            }

            set
            {
                _flashTimer.Interval = value;
            }
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
        /// Gets or sets the <see cref="DataEntryControlHost"/> to which this button belongs
        /// </summary>
        /// <value>The <see cref="DataEntryControlHost"/> to which this button belongs.</value>
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
        /// If the <see cref="DataEntryButton"/> is not intended to operate on root-level data, 
        /// this property must be used to specify the <see cref="IDataEntryControl"/> which is 
        /// mapped to the parent of the <see cref="IAttribute"/>(s) to which the current 
        /// <see cref="DataEntryButton"/> is to be mapped.  The specified 
        /// <see cref="IDataEntryControl"/> must be contained in the same 
        /// <see cref="DataEntryControlHost"/> as this <see cref="DataEntryButton"/>. If the 
        /// <see cref="DataEntryButton"/> is to be mapped to a root-level <see cref="IAttribute"/>, 
        /// this property must be set to <see langref="null"/>.</summary>
        /// <seealso cref="IDataEntryControl"/>
        [Category("Data Entry Button")]
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
        [Category("Data Entry Button")]
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
        /// to which it should be mapped. <see cref="DataEntryButton"/> will always use the
        /// first match (if available).
        /// </summary>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> instance of 
        /// <see cref="IAttribute"/>s from which the <see cref="DataEntryButton"/> 
        /// should find its corresponding <see cref="IAttribute"/>.  Can be an empty vector, but 
        /// must not be <see langword="null"/>.</param>
        /// <seealso cref="IDataEntryControl"/>
        public void SetAttributes(IUnknownVector sourceAttributes)
        {
            try
            {
                if (!_hasSpecifiedAttributeName.HasValue)
                {
                    if (string.IsNullOrEmpty(_attributeName))
                    {
                        _attributeName = "PlaceholderAttribute_" + this.Name;
                        _hasSpecifiedAttributeName = false;
                    }
                    else
                    {
                        _hasSpecifiedAttributeName = true;
                    }
                }

                // [DataEntry:298]
                // If the button has nothing to propagate, disable it since any data entered would
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

                    if (!_hasSpecifiedAttributeName.Value)
                    {
                        // If not explicitly mapped to an attribute, don't persist placeholder
                        // attributes in output-- the attribute itself is only a placeholder for the
                        // control in the tab order.
                        AttributeStatusInfo.SetAttributeAsPersistable(_attribute, false);

                        // Consider the attribute fully propagated as it should never have children.
                        AttributeStatusInfo.MarkAsPropagated(_attribute, true, true);
                    }
                    else
                    {
                        if (Visible)
                        {
                            // Mark the attribute as visible if the button is visible
                            AttributeStatusInfo.MarkAsViewable(_attribute, true);
                        }

                        // If not persisting the attribute, mark the attribute accordingly.
                        if (!_persistAttribute)
                        {
                            AttributeStatusInfo.SetAttributeAsPersistable(_attribute, false);
                        }
                    }
                }

                // Raise the OnAttributeChanged event to notify the host of the currently selected
                // attribute.
                OnAttributeChanged();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI37360", ex);
            }
        }

        /// <summary>
        /// Handles the case that this <see cref="DataEntryButton"/>'s 
        /// <see cref="ParentDataEntryControl"/> has requested that a new <see cref="IAttribute"/> 
        /// be propagated.  The <see cref="DataEntryButton"/> will re-map its control appropriately.
        /// The <see cref="DataEntryButton"/> will mark the <see cref="AttributeStatusInfo"/> instance 
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
                throw ExtractException.AsExtractException("ELI37361", ex);
            }
        }

        /// <summary>
        /// This method has no effect for a <see cref="DataEntryButton"/> except to validate that
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
            ExtractException.Assert("ELI37362", "Unexpected attribute!",
                attribute == null || attribute == _attribute);
        }

        /// <summary>
        /// <see cref="DataEntryButton"/> does not have any implementation for this method.
        /// </summary>
        public virtual void RefreshAttributes()
        {
            RefreshAttributes(true, _attribute);
        }

        /// <summary>
        /// <see cref="DataEntryButton"/> does not have any implementation for this method.
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
                    // Don't update the value if the value hasn't actually changed. Doing so is not
                    // only in-efficient but it can cause un-intended side effects if an
                    // auto-complete list is active.
                    // Also, only allow Text to be blanked out if the button has been explicitly
                    // mapped to an attribute.
                    string newValue = _attribute.Value?.String ?? "";
                    if (_hasSpecifiedAttributeName != null &&
                        (_hasSpecifiedAttributeName.Value || !string.IsNullOrWhiteSpace(newValue)) &&
                        (newValue != Text || spatialInfoUpdated))
                    {
                        Text = newValue;
                    }

                    // In case the value itself didn't change but the validation list did,
                    // explicitly check validation here.
                    UpdateValidation();

                    // Raise the AttributesSelected event to re-notify the host of the spatial
                    // information associated with the attribute in case the spatial info has
                    // changed.
                    OnAttributesSelected();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI37373", ex);
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
        /// <param name="elementName">If a related element is required, the property name of an
        /// object relative to the object mapped directly to the attribute. Multiple references may
        /// be chained by separating with a period. E.g., While a specific attribute may be mapped
        /// to a DataGridViewRow, an elementName of "DataGridView.VerticalScrollBar" could be used
        /// to refer to the scroll bar for the grid.</param>
        /// <returns>The UI element.</returns>
        public object GetAttributeUIElement(IAttribute attribute, string elementName)
        {
            try
            {
                return string.IsNullOrEmpty(elementName)
                    ? this
                    : this.GetProperty(elementName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50315");
            }
        }

        /// <summary>
        /// Creates a <see cref="BackgroundFieldModel"/> for representing this control during
        /// a background data load.
        /// </summary>
        public BackgroundFieldModel GetBackgroundFieldModel()
        {
            try
            {
                var fieldModel = new BackgroundFieldModel()
                {
                    Name = AttributeName,
                    OwningControl = this,
                    OwningControlModel = new BackgroundControlModel(this),
                    AutoUpdateQuery = AutoUpdateQuery,
                    ValidationQuery = ValidationQuery,
                    DisplayOrder = DataEntryMethods.GetTabIndices(this),
                    IsViewable = Visible,
                    ValidationErrorMessage = ValidationErrorMessage,
                    PersistAttribute = PersistAttribute,
                    ValidationPattern = ValidationPattern
                };

                return fieldModel;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45505");
            }
        }

        /// <summary>
        /// The single attribute mapped to this control
        /// </summary>
        public IAttribute Attribute => _attribute;

        /// <summary>
        /// The attributes mapped to this control.
        /// (In the case of this control, there will only be one)
        /// </summary>
        public IEnumerable<IAttribute> Attributes => new[] { _attribute };

        #endregion IDataEntryControl Members

        #region Unused IDataEntryControl Members

        /// <summary>
        /// This event is not raised by <see cref="DataEntryButton"/>.
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
        /// This event is not raised by <see cref="DataEntryButton"/>.
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
        /// This event is not raised by <see cref="DataEntryButton"/>.
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
        /// This event is not raised by <see cref="DataEntryButton"/>.
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
        /// <see cref="DataEntryButton"/> does not support swiping; the value of this property
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
                    throw new ExtractException("ELI37363",
                        "DataEntryButton does not support swiping!");
                }
            }
        }

        /// <summary>
        /// <see cref="DataEntryButton"/> does not support pasting; the value of this property
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
                throw new ExtractException("ELI37364",
                    "DataEntryButton does not support pasting!");
            }
        }

        /// <summary>
        /// <see cref="DataEntryButton"/> does not have any implementation for this method.
        /// </summary>
        /// <param name="setActive">Unused.</param>
        /// <param name="color">Unused.</param>
        public void IndicateActive(bool setActive, Color color)
        {
        }

        /// <summary>
        /// <see cref="DataEntryButton"/> does not have any implementation for this method.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if called.</throws>
        /// <param name="swipedText">Unused.</param>
        public bool ProcessSwipedText(UCLID_RASTERANDOCRMGMTLib.SpatialString swipedText)
        {
            throw new ExtractException("ELI37366", "DataEntryButton does not support swiping!");
        }

        /// <summary>
        /// <see cref="DataEntryButton"/> doesn't cache any data.
        /// </summary>
        public void ClearCachedData()
        {
        }

        /// <summary>
        /// <see cref="DataEntryButton"/> does not have any implementation for this method.
        /// <param name="selectionState">Unused.</param>
        /// </summary>
        public void ApplySelection(SelectionState selectionState)
        {
        }

        #endregion Unused IDataEntryControl Members

        #region Overrides

        /// <summary>
        /// Gets or sets the background color of the control.
        /// </summary>
        /// <returns>A <see cref="T:System.Drawing.Color"/> value representing the background color.
        /// </returns>
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                bool flashing = Flash;

                try
                {
                    // In order to properly apply a new background color, flashing needs to first be stopped.
                    if (flashing)
                    {
                        Flash = false;
                    }

                    base.BackColor = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37376");
                }
                finally
                {
                    // Resume flashing if it had been flashing.
                    Flash = flashing;
                }
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
                ex.ExtractDisplay("ELI37915");
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

                if (_flashTimer != null)
                {
                    _flashTimer.Dispose();
                    _flashTimer = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Timer.Tick"/> event of the <see cref="_flashTimer"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFlashTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Apply the background color to the base class so as not to get tangled in the
                // BackColor override which assumes the BackColor change is not coming from the
                // flashing process.
                if (base.BackColor == _normalBackgroundColor)
                {
                    base.BackColor = _flashingBackgroundColor;
                }
                else
                {
                    base.BackColor = _normalBackgroundColor;
                }
            }
            catch (Exception ex)
            {
                Flash = false;
                ex.ExtractDisplay("ELI37377");
            }
        }

        #endregion Event Handlers

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
                throw ExtractException.AsExtractException("ELI37367", ex);
            }
        }

        /// <summary>
        /// Updates the text value of this control and raise the events that need to be raised
        /// in conjunction with an attribute change (<see cref="AttributesSelected"/> and
        /// <see cref="PropagateAttributes"/>).
        /// </summary>
        void OnAttributeChanged()
        {
            // Display the attribute text.
            // Only allow Text to be blanked out if the button has been explicitly mapped to an
            // attribute.
            if (_attribute?.Value != null &&
                _hasSpecifiedAttributeName != null &&
                (_hasSpecifiedAttributeName.Value || !string.IsNullOrWhiteSpace(_attribute.Value.String)))
            {
                Text = _attribute.Value.String;
            }

            // Display a validation error icon if needed.
            UpdateValidation();

            // Raise the AttributesSelected event to signal that the spatial information
            // associated with this control has changed.
            OnAttributesSelected();

            // Raise the PropagateAttributes event so that any descendants of this data control can
            // re-map themselves.
            OnPropagateAttributes();
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
            if (this.AttributesSelected != null)
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
                    // If the button is not currently mapped to an attribute, it should propagate
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
