using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A single-attribute control intended to manage two levels of <see cref="IAttribute"/>s 
    /// where each row in the table contains the value of either the control's primary mapped
    /// <see cref="IAttribute"/> or a sub-<see cref="IAttribute"/> to the primary 
    /// <see cref="IAttribute"/>.</summary>
    public partial class DataEntryTwoColumnTable : DataEntryTableBase
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataEntryTwoColumnTable).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The value that appears in the table's only column header.
        /// </summary> 
        string _displayName;

        /// <summary>
        /// The selection mode to use when multiple attributes are found which match the attribute 
        /// name for this control.
        /// </summary>
        MultipleMatchSelectionMode _multipleMatchSelectionMode =
            MultipleMatchSelectionMode.None;

        /// <summary>
        /// Indicates whether swiping should be allowed when an individual cell is selected.
        /// </summary>
        bool _cellSwipingEnabled = true;

        /// <summary>
        /// Indicates whether swiping should be allowed when the entire table is selected.
        /// </summary>
        bool _tableSwipingEnabled;

        /// <summary>
        /// 
        /// </summary>
        string _tableFormattingRuleFileName;

        /// <summary>
        /// The formatting rule to be used when processing text imaging swipes for the entire table.
        /// </summary>
        IRuleSet _tableFormattingRule;

        /// <summary>
        /// The domain of attributes to which this control's attribute(s) belong.
        /// </summary>
        IUnknownVector _sourceAttributes;

        /// <summary>
        /// The attribute mapped to this control.
        /// </summary>
        IAttribute _attribute;

        /// <summary>
        /// The collection of rows that will populate the table.
        /// </summary>
        Collection<DataEntryTableRow> _rows = new Collection<DataEntryTableRow>();

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryTwoColumnTable"/> instance.
        /// </summary>
        public DataEntryTwoColumnTable()
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
                    LicenseIdName.DataEntryCoreComponents, "ELI24491", _OBJECT_NAME);

                InitializeComponent();

                // This control does not currently support the addition or deletion of rows.
                base.AllowUserToAddRows = false;
                base.AllowUserToDeleteRows = false;

                // Since the number of rows is known at design time, scroll bars are likely not
                // desired. Default to none.
                base.ScrollBars = ScrollBars.None;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24324", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the value that appears in the table's only column header.
        /// </summary>
        /// <value>The value that appears in the table's only column header.</value>
        /// <returns>The value that appears in the table's only column header.</returns>
        [Category("Data Entry Table")]
        public string DisplayName
        {
            get
            {
                return _displayName;
            }

            set
            {
                try
                {
                    _displayName = value;

                    // As long as the control has been initialized, apply the new display name to 
                    // the one and only column header.
                    if (base.Columns.Count == 1)
                    {
                        base.Columns[0].HeaderText = _displayName;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24335", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryTwoColumnTable"/>.
        /// </summary>
        /// <value>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryTwoColumnTable"/>.</value>
        /// <returns>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryTwoColumnTable"/>.</returns>
        [Category("Data Entry Table")]
        [DefaultValue(MultipleMatchSelectionMode.None)]
        public MultipleMatchSelectionMode MultipleMatchSelectionMode
        {
            get
            {
                return _multipleMatchSelectionMode;
            }

            set
            {
                _multipleMatchSelectionMode = value;
            }
        }

        /// <summary>
        /// Indicates whether swiping should be allowed when an individual cell is selected.
        /// </summary>
        /// <value><see langword="true"/> if the table should allow swiping when an individual cell
        /// is selected, <see langword="false"/> if it should not.</value>
        /// <returns><see langword="true"/> if the table allows swiping when an individual cell
        /// is selected, <see langword="false"/> if it does not.</returns>
        [Category("Data Entry Table")]
        [DefaultValue(true)]
        public bool CellSwipingEnabled
        {
            get
            {
                return _cellSwipingEnabled;
            }

            set
            {
                _cellSwipingEnabled = value;
            }
        }

        /// <summary>
        /// Indicates whether swiping should be allowed when the entire table is selected.
        /// </summary>
        /// <value><see langword="true"/> if the table should allow swiping when the entire table
        /// is selected, <see langword="false"/> if it should not.</value>
        /// <returns><see langword="true"/> if the table allows swiping when the entire table
        /// is selected, <see langword="false"/> if it does not.</returns>
        [Category("Data Entry Table")]
        [DefaultValue(false)]
        public bool TableSwipingEnabled
        {
            get
            {
                return _tableSwipingEnabled;
            }

            set
            {
                _tableSwipingEnabled = value;
            }
        }

        /// <summary>
        /// Specifies the filename of an <see cref="IRuleSet"/> that should be used to reformat or
        /// split <see cref="SpatialString"/> content passed into <see cref="ProcessSwipedText"/>
        /// that is intended to populate the entire table.
        /// </summary>
        /// <value>The filename of the <see cref="IRuleSet"/> to be used.</value>
        /// <returns>The filename of the <see cref="IRuleSet"/> to be used.</returns>
        [Category("Data Entry Table")]
        [DefaultValue(null)]
        public string TableFormattingRuleFile
        {
            get
            {
                return _tableFormattingRuleFileName;
            }

            set
            {
                try
                {
                    // If a formatting rule is specified, attempt to load an attribute finding rule.
                    if (!_inDesignMode && !string.IsNullOrEmpty(value))
                    {
                        _tableFormattingRule = (IRuleSet)new RuleSetClass();
                        _tableFormattingRule.LoadFrom(DataEntryMethods.ResolvePath(value), false);
                    }
                    else
                    {
                        _tableFormattingRule = null;
                    }

                    _tableFormattingRuleFileName = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24326", ex);
                }
            }
        }

        /// <summary>
        /// The collection of <see cref="DataEntryTableRow"/>s to be used to populate the table.
        /// This collection needs to be used in place of the underlying 
        /// <see cref="DataGridView.Rows"/> collection in order to pre-define the rows to appear 
        /// and to allow Extract Systems data entry specific row settings to be applied.
        /// </summary>
        /// <value>The collection of <see cref="DataEntryTableRow"/>s to be used to populate the 
        /// table.</value>
        /// <returns>The collection of <see cref="DataEntryTableRow"/>s to be used to populate the 
        /// table.</returns>
        [Category("Data Entry Table")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new Collection<DataEntryTableRow> Rows
        {
            get
            {
                return _rows;
            }
        }

        /// <summary>
        /// Specifies whether GetSpatialHint will attempt to generate a hint by indicating the other
        /// <see cref="IAttribute"/>s sharing the same column.
        /// </summary>
        /// <value><see langword="true"/> if the table should attempt to generate column hints when
        /// possible; <see langword="false"/> if the table should never attempt to generate column
        /// hints.</value>
        /// <returns><see langword="true"/> if the table is configured to generate column hints when
        /// possible; <see langword="false"/> if the table is not configured to generate column
        /// hints.</returns>
        [Category("Data Entry Table")]
        [DefaultValue(true)]
        public new bool ColumnHintsEnabled
        {
            get
            {
                return base.ColumnHintsEnabled;
            }

            set
            {
                base.ColumnHintsEnabled = value;
            }
        }

        #endregion Properties

        // These are properties from tbe base class (DataGridView) that should not be used as part
        // of a DataEntryTwoColumnTable.
        #region Concealed Properties

        /// <summary>
        /// Gets the table's <see cref="DataGridViewColumnCollection"/> collection.  The collection 
        /// should not be modified in the designer or by an outide caller. A 
        /// <see cref="DataEntryTwoColumnTable"/> will create a single column internally.
        /// </summary>
        /// <returns>The table's <see cref="DataGridViewColumnCollection"/> collection.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new DataGridViewColumnCollection Columns
        {
            get
            {
                return base.Columns;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the option to add rows is displayed to the user.
        /// <para><b>Requirements</b></para>
        /// Must be <see langword="false"/> as the <see cref="DataEntryTwoColumnTable"/> does 
        /// not support user-deleted rows.
        /// </summary>
        /// <value>Cannot be set to <see langword="true"/> as the table does not support
        /// user-added rows.</value>
        /// <returns><see langword="false"/> as the control does not support user-added
        /// rows.</returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AllowUserToAddRows
        {
            get
            {
                return base.AllowUserToAddRows;
            }

            set
            {
                ExtractException.Assert("ELI24320",
                    "A DataEntryTwoColumnTable does support user added rows", value == false);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the option to delete rows is displayed to the user.
        /// <para><b>Requirements</b></para>
        /// Must be <see langword="false"/> as the <see cref="DataEntryTwoColumnTable"/> does 
        /// not support user-deleted rows. 
        /// </summary>
        /// <value>Cannot be set to <see langword="true"/> as the table does not support
        /// user-deleted rows.</value>
        /// <returns><see langword="false"/> as the control does not support user-deleted rows.
        /// </returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AllowUserToDeleteRows
        {
            get
            {
                return base.AllowUserToDeleteRows;
            }

            set
            {
                ExtractException.Assert("ELI24321",
                    "A DataEntryTwoColumnTable does support the deletion of rows", value == false);
            }
        }

        #endregion Concealed Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Control.HandleCreated"/> event in order to verify that the table
        /// has been properly configured before data entry commences.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            try
            {
                base.OnHandleCreated(e);

                // If we are not in design mode and the control is being made visible for the first
                // time, verify the table is properly configured and create the context menu.
                if (!_inDesignMode)
                {
                    ExtractException.Assert("ELI24375",
                        "Table swiping is enabled, but no table formatting rule was specified!",
                        !this.TableSwipingEnabled || _tableFormattingRule != null);
                }

                InitializeColumn();
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI24327", ex).Display();
            }
        }

        /// <summary>
        /// Highlights the currently selected <see cref="IAttribute"/>s in the image viewer and
        /// propagates the selected row(s) as appropriate.
        /// </summary>
        protected override void ProcessSelectionChange()
        {
            try
            {
                base.ProcessSelectionChange();

                // Create a vector to store the attributes in the currently selected row(s).
                IUnknownVector selectedAttributes = (IUnknownVector)new IUnknownVectorClass();

                // If the selection is on a cell-by-cell basis rather, we need to compile the
                // attributes corresponding to each cell for the AttributesSelected event.
                foreach (DataGridViewCell selectedCell in base.SelectedCells)
                {
                    IAttribute attribute = DataEntryTableBase.GetAttribute(selectedCell);
                    if (attribute != null)
                    {
                        selectedAttributes.PushBack(attribute);
                    }
                }

                // Include all the attributes for the specifically selected cells in the 
                // spatial info, not any children of those attributes. Show tooltips only
                // if one attribute is selected.
                OnAttributesSelected(selectedAttributes, false, selectedAttributes.Size() == 1,
                    null);
               
                // Update the swiping state based on the current selection.
                OnSwipingStateChanged();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24328", ex);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Component.Site"/> of the <see cref="DataEntryTwoColumnTable"/>.
        /// </summary>
        /// <value>The <see cref="Component.Site"/> of the <see cref="DataEntryTwoColumnTable"/>.</value>
        /// <returns>The <see cref="Component.Site"/> of the <see cref="DataEntryTwoColumnTable"/>.</returns>
        public override ISite Site
        {
            get
            {
                return base.Site;
            }

            set
            {
                // The Site property is set by the designer.  By registering for the 
                // IComponentChangeService's ComponentChanged event, we can be notified when
                // this DataEntryTwoColumnTable rows collection has changed and use it to update
                // the underlying DataGridView classes' row collection appropriately.
                try
                {
                    // Only enable handling of ComponentChanged if in design mode.  Otherwise, just
                    // set Site on the base and return.
                    if (value == null || !value.DesignMode)
                    {
                        base.Site = value;
                        return;
                    }

                    // If registered with the current site for the IComponentChangeService's
                    // ComponentChanged event, unregister the event.
                    IComponentChangeService componentChangeService =
                        (IComponentChangeService)GetService(typeof(IComponentChangeService));

                    if (componentChangeService != null)
                    {
                        componentChangeService.ComponentChanged -= HandleComponentChanged;
                    }

                    base.Site = value;

                    // Register with the new site for the IComponentChangeService's ComponentChanged 
                    // event.
                    componentChangeService =
                        (IComponentChangeService)GetService(typeof(IComponentChangeService));

                    if (componentChangeService != null)
                    {
                        componentChangeService.ComponentChanged += HandleComponentChanged;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.AsExtractException("ELI24264", ex).Display();
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.Layout"/> event. This is overridden to apply the 
        /// <see cref="DataEntryTwoColumnTable"/>'s <see cref="Rows"/> collection to the underlying
        /// <see cref="DataGridView.Rows"/> collection. Since <see cref="DataEntryTwoColumnTable"/>'s 
        /// <see cref="Rows"/> collection is not deserialized until after the 
        /// <see cref="Control.HandleCreated"/> event, the rows cannot be applied until the 
        /// <see cref="Control.Layout"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            try
            {
                base.OnLayout(e);

                // If the Layout event is targetting this control and the DataGridView rows count
                // does not match the number of this classes's rows collection, apply the row
                // collection now.
                if (e.AffectedControl == this && _rows.Count != base.Rows.Count)
                {
                    // Suspend layout since UpdateRows itself will otherwise cause a layout event
                    // as each row is added.
                    base.SuspendLayout();

                    InitializeColumn();

                    UpdateRows();

                    base.ResumeLayout();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24337", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the case that a component has been changed. This is handled apply changes
        /// made to the <see cref="Rows"/> collection in the designer so that the changes can be
        /// applied to the underlying <see cref="DataGridView.Rows"/> collection.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="ComponentChangedEventArgs"/> that contains the event data.
        /// </param>
        void HandleComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            try
            {
                if (e != null && e.Member != null && e.Member.Name == "Rows")
                {
                    // Apply changes to the rows from the designer to the table itself.
                    UpdateRows();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24263", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region IDataEntryControl Properties

        /// <summary>
        /// Indicates whether the <see cref="DataEntryTwoColumnTable"/> will currently accept as 
        /// input the <see cref="SpatialString"/> associated with an image swipe. 
        /// <para><b>Note:</b></para>
        /// The <see cref="CellSwipingEnabled"/> and <see cref="TableSwipingEnabled"/> properties 
        /// should be used to configure swiping on the <see cref="DataEntryTwoColumnTable"/> in 
        /// place of the <see cref="SupportsSwiping"/> and is why this property is not browseable. 
        /// If this property is set to <see langword="true"/>, both 
        /// and <see cref="TableSwipingEnabled"/> will be set to <see langword="true"/>; likewise 
        /// setting <see cref="SupportsSwiping"/> to <see langword="false"/> will set both
        /// <see cref="CellSwipingEnabled"/> and <see cref="TableSwipingEnabled"/> to 
        /// <see langword="false"/>.
        /// </summary>
        /// <value><see langword="true"/> if the table will not currently accept input via a
        /// swipe, <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if the table will not currently accept input via a
        /// swipe, <see langword="false"/> otherwise.</returns>
        /// <seealso cref="IDataEntryControl"/>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool SupportsSwiping
        {
            get 
            {
                // If no attribute has been mapped, swiping is not currently supported.
                if (!base.Enabled || _attribute == null)
                {
                    return false;
                }

                if (base.AreAllCellsSelected(false))
                {
                    // Table selection
                    return _tableSwipingEnabled;
                }
                else if (base.SelectedCells.Count > 0)
                {
                    // Cell selection
                    return _cellSwipingEnabled;
                }
                else
                {
                    // No selection
                    return false;
                }
            }

            set 
            {
                _tableSwipingEnabled = value;
                _cellSwipingEnabled = value;
            }
        }
        
        #endregion IDataEntryControl Properties

        #region IDataEntryControl Methods

        /// <summary>
        /// Specifies the domain of <see cref="IAttribute"/>s from which the 
        /// <see cref="DataEntryTwoColumnTable"/> should find the <see cref="IAttribute"/> 
        /// to which it should be mapped (based on the AttributeName property). 
        /// </summary>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> instance of 
        /// <see cref="IAttribute"/>s from which the <see cref="DataEntryTwoColumnTable"/> 
        /// should find its corresponding <see cref="IAttribute"/>. Can be an empty vector, but 
        /// must not be <see langword="null"/>.</param>
        /// <seealso cref="IDataEntryControl"/>
        public override void SetAttributes(IUnknownVector sourceAttributes)
        {
            try
            {
                // Any existing attributes need to be cleared before loading the new attributes
                // so that the current attributes don't get overwritten by auto-update queries
                // in the process of loading.
                if (_sourceAttributes != null && sourceAttributes != null)
                {
                    SetAttributes(null);
                }
                
                // [DataEntry:298]
                // If the table isn't assigned any data, disable it since any data entered would
                // not be mapped into the attribute hierarchy.
                // Also, prevent it from being enabled if explicitly disabled via the
                // IDataEntryControl interface.
                base.Enabled = (sourceAttributes != null && !base.Disabled);

                _sourceAttributes = sourceAttributes;

                if (sourceAttributes == null)
                {
                    // If no data is being assigned, clear the existing attribute mappings and do not
                    // attempt to map a new attribute.
                    ClearAttributeMappings(true);

                    _attribute = null;
                }
                else
                {
                    // Attempt to find a mapped attribute from the provided vector.  Create a new 
                    // attribute if no such attribute can be found.
                    _attribute = DataEntryMethods.InitializeAttribute(base.AttributeName,
                        _multipleMatchSelectionMode, !string.IsNullOrEmpty(base.AttributeName),
                        sourceAttributes, null, this, 0, false, TabStopMode.Never, null, null, null);
                }

                // Use the primarily mapped attribute map the attribute for each row.
                ApplyAttribute();

                // Selecting all cells makes table look more "disabled".
                if (base.Disabled)
                {
                    base.SelectAll();
                }

                // Update the swiping state based on the current selection.
                OnSwipingStateChanged();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24261", ex);
            }
        }

        /// <summary>
        /// Processes the supplied <see cref="SpatialString"/> as input.
        /// </summary>
        /// <param name="swipedText">The <see cref="SpatialString"/> representing the
        /// recognized text in the swiped image area.</param>
        /// <returns><see langword="true"/> if the control was able to use the swiped text;
        /// <see langword="false"/> if it could not be used.</returns>
        /// <seealso cref="IDataEntryControl"/>
        public override bool ProcessSwipedText(SpatialString swipedText)
        {
            try
            {
                // If the table is disabled or no attribute has been mapped, swiping is not
                // currently supported.
                if (!base.Enabled || _attribute == null)
                {
                    return false;
                }

                // [DataEntry:912]
                // Close edit mode before attempting to process swiped data
                if (EditingControl != null)
                {
                    EndEdit();
                }

                if (base.AreAllCellsSelected(false))
                {
                    // Table selection mode.
                    return ProcessTableSwipe(swipedText);
                }
                else if (base.SelectedCells.Count > 0)
                {
                    // Cell selection mode.
                    return ProcessCellSwipe(swipedText);
                }

                return false;
            }
            catch (Exception ex)
            {
                try
                {
                    // If an exception was thrown while processing a swipe, refresh hints for the
                    // table since the hints may not be valid at this point.
                    base.UpdateHints(true);
                }
                catch (Exception ex2)
                {
                    ExtractException.Log("ELI27099", ex2);
                }

                throw ExtractException.AsExtractException("ELI24274", ex);
            }
        }

        /// <summary>
        /// Any data that was cached should be cleared;  This is called when a document is unloaded.
        /// If controls fail to clear COM objects, errors may result if that data is accessed when
        /// a subsequent document is loaded.
        /// </summary>
        public override void ClearCachedData()
        {
            try
            {
                // Ensure attributes are no longer referenced by any of the cells.
                ClearAttributeMappings(true);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29212", ex);
            }
        }

        /// <summary>
        /// Requests that the <see cref="IDataEntryControl"/> refresh all <see cref="IAttribute"/>
        /// values to the screen.
        /// </summary>
        public override void RefreshAttributes()
        {
            try
            {
                SetAttributes(_sourceAttributes);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31067", ex);
            }
        }

        #endregion IDataEntryControl Methods

        #region Private Members

        /// <summary>
        /// Uses the specied table formatting rule to apply the swiped text to all rows in the 
        /// table.
        /// </summary>
        /// <param name="swipedText">The OCR'd text from the image swipe.</param>
        bool ProcessTableSwipe(SpatialString swipedText)
        {
            ExtractException.Assert("ELI26144", "Uninitialized data!",
                _sourceAttributes != null && _attribute != null);

            IAttribute newAttribute = DataEntryMethods.RunFormattingRule(
                _tableFormattingRule, swipedText, base.AttributeName, _multipleMatchSelectionMode);

            // If a qualifying attribute was found in the rule's results, apply it.
            if (newAttribute != null)
            {
                int index = -1;
                if (_sourceAttributes.Size() > 0)
                {
                    _sourceAttributes.FindByReference(_attribute, 0, ref index);

                    if (index >= 0)
                    {
                        // Remove the old attribute.
                        AttributeStatusInfo.DeleteAttribute(_attribute);
                    }
                }

                _attribute = newAttribute;

                // Add the new attribute from the source attribute vector.
                _sourceAttributes.Insert(index, _attribute);

                // Initialize the newly swiped attribute's status information.
                AttributeStatusInfo.Initialize(newAttribute, _sourceAttributes, this, 0, false,
                   TabStopMode.Never, null, null, null);

                // Use the primarily mapped attribute map the attribute for each row.
                ApplyAttribute();

                // Fire selection change event to update the highlights to reflect the 
                // swiped data.
                OnSelectionChanged(new EventArgs());

                return true;
            }
            else
            {
                // If no attribute was returned from the rule, return false to indicate formatting
                // was not successful.
                return false;
            }
        }

        /// <summary>
        /// Process swiped text as input into the currently selected cell.
        /// <para><b>Requirements:</b></para>
        /// One (and only one) cell may be selected.
        /// </summary>
        /// <param name="swipedText">The OCR'd text from the image swipe.</param>
        /// <throws><see cref="ExtractException"/> if more than one cell is selected.</throws>
        bool ProcessCellSwipe(SpatialString swipedText)
        {
            ExtractException.Assert("ELI24265",
                        "Cell swiping is supported only for one cell at a time!", 
                        base.SelectedCells.Count == 1);

            ExtractException.Assert("ELI26145", "Uninitialized data!",
                _sourceAttributes != null && _attribute != null);

            // Cell selecton mode. The swipe can be applied either via the results of a
            // column formatting rule or the swiped text value can be applied directly to
            // the cell's mapped attribute.
            DataEntryTableRow selectedRow = (DataEntryTableRow)base.CurrentRow;
            IDataEntryTableCell selectedDataEntryCell = (IDataEntryTableCell)base.CurrentCell;

            // Process the swiped text with a formatting rule (if available).
            if (selectedRow.FormattingRule != null)
            {
                // Select the attribute name to look for from the rule results. (Could be based on
                // a sub-attribute name or the name of the table's primary attribute).
                string attributeName = (selectedRow.AttributeName == ".")
                    ? base.AttributeName : selectedRow.AttributeName;

                IAttribute attribute = DataEntryMethods.RunFormattingRule(
                    selectedRow.FormattingRule, swipedText, attributeName,
                    selectedRow.MultipleMatchSelectionMode);

                // Use the value of the found attribute only if the found attribute has a non-empty
                // value.
                if (attribute != null && !string.IsNullOrEmpty(attribute.Value.String))
                {
                    swipedText = attribute.Value;
                }
            }

            // If there is an active text box editing control, swipe into the current
            // selection rather than replacing the entire value.
            DataGridViewTextBoxEditingControl textBoxEditingControl =
                EditingControl as DataGridViewTextBoxEditingControl;
            int selectionStart = -1;
            int selectionLength = -1;
            if (textBoxEditingControl != null)
            {
                // Keep track of what the final selection should be.
                selectionStart = textBoxEditingControl.SelectionStart;
                selectionLength = swipedText.Size;

                IDataEntryTableCell dataEntryCell = CurrentCell as IDataEntryTableCell;

                if (dataEntryCell != null)
                {
                    swipedText = DataEntryMethods.InsertSpatialStringIntoSelection(
                        textBoxEditingControl, dataEntryCell.Attribute.Value, swipedText);
                }
            }

            // Apply the new value directly to the mapped attribute (Don't replace the entire 
            // attribute).
            CurrentCell.Value = swipedText;

            // If an editing control is active, update it to reflect the result of the swipe.
            if (EditingControl != null)
            {
                RefreshEdit();

                if (textBoxEditingControl != null)
                {
                    // Select the newly swiped text.
                    textBoxEditingControl.Select(selectionStart, selectionLength);
                }

                // Forces the caret position to be updated appropriately.
                EditingControl.Focus();
            }

            // Since the spatial information for this cell has changed, spatial hints need to be 
            // updated.
            UpdateHints(false);

            // Raise AttributesSelected to updated the control's highlight.
            OnAttributesSelected(DataEntryMethods.AttributeAsVector(
                    DataEntryTableBase.GetAttribute(selectedDataEntryCell)), false, true, null);

            return true;
        }

        /// <summary>
        /// Updates the underlying <see cref="DataGridView.Rows"/> collection using the 
        /// <see cref="DataEntryTwoColumnTable.Rows"/> collection.
        /// </summary>
        void UpdateRows()
        {
            // Clear the existing DataGridView row collection.
            base.Rows.Clear();

            foreach (DataEntryTableRow row in _rows)
            {
                // Add a DataEntryTableRow using a copy of each row in _rows.  It is important to use
                // a copy and not the original DataEntryTableRow objects since the act of adding
                // the row to the table will set properties in the row object which will prevent it
                // from being re-used to initialize a row at a later time.
                row.HeaderCell = new DataGridViewRowHeaderCell();
                row.HeaderCell.Value = row.Name;
                base.Rows.Add((DataEntryTableRow)row.Clone());
            }

            // Resize the row header so that the name for each row fits within the header.
            base.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }

        /// <summary>
        /// Uses the primarily mapped <see cref="IAttribute"/> to populate all rows in the table.
        /// </summary>
        void ApplyAttribute()
        {
            // Iterate through all rows to set map attrbute for each.
            foreach (DataEntryTableRow row in base.Rows)
            {
                IDataEntryTableCell dataEntryCell = (IDataEntryTableCell)row.Cells[0];

                if (_attribute == null)
                {
                    // If the table is not currently mapped to any attribute, clear the cell.
                    row.Cells[0].Value = "";
                    continue;
                }
                else if (row.AttributeName == ".")
                {
                    // "." indicates that the parent attribute should be used in this row.
                    AttributeStatusInfo.Initialize(_attribute, _sourceAttributes, this, row.Index,
                        false, row.TabStopMode, dataEntryCell.ValidatorTemplate, row.AutoUpdateQuery,
                        row.ValidationQuery);
                    row.Cells[0].Value = _attribute;
                }
                else
                {
                    // Attempts to map an appropriate sub-attribute to the row.
                    row.Cells[0].Value = DataEntryMethods.InitializeAttribute(row.AttributeName,
                        row.MultipleMatchSelectionMode, true, _attribute.SubAttributes, null,
                        this, row.Index, true, row.TabStopMode, dataEntryCell.ValidatorTemplate,
                        row.AutoUpdateQuery, row.ValidationQuery);
                }

                // If not persisting the attribute, mark the attribute accordingly.
                if (!row.PersistAttribute)
                {
                    AttributeStatusInfo.SetAttributeAsPersistable(dataEntryCell.Attribute, false);
                }

                base.MapAttribute(dataEntryCell.Attribute, dataEntryCell);
            }

            // Since the spatial information for this cell has likely changed, spatial hints need 
            // to be updated.
            base.UpdateHints(false);

            if (_attribute == null)
            {
                // If no data is mapped, propagate null to indicate to dependent controls that they
                // are un-mapped.
                OnPropagateAttributes(null);
            }
            else
            {
                // Raise PropagateAttributes to propagate the new attribute to any dependent controls.
                OnPropagateAttributes(DataEntryMethods.AttributeAsVector(_attribute));
            }

            // Fire selection change event to update the highlights to reflect the 
            // new attribute.
            OnSelectionChanged(new EventArgs());
        }

        /// <summary>
        /// Creates and assigns the tables one and only (non-configurable) column.
        /// </summary>
        void InitializeColumn()
        {
            if (Columns.Count == 0)
            {
                // Initialize the table's one and only column.
                DataGridViewColumn column = new DataGridViewColumn();
                column.HeaderText = _displayName;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                column.CellTemplate = new DataEntryTextBoxCell();

                base.Columns.Add(column);
            }
        }

        #endregion Private Members
    }
}
