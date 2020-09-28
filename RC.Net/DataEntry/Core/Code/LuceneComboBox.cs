using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="ComboBox"/> that uses a <see cref="LuceneAutoSuggest"/> to display suggestions
    /// </summary>
    public partial class LuceneComboBox : ComboBox, IDataEntryAutoCompleteControl, IRequiresErrorProvider
    {
        readonly ComboBoxStyle _SUPPORTED_COMBO_BOX_STYLE = ComboBoxStyle.DropDown;
        readonly DataEntryAutoCompleteMode _SUPPORTED_AUTO_COMPLETE_MODE = DataEntryAutoCompleteMode.SuggestLucene;

        // The DataEntryControlHost to which this control belongs
        DataEntryControlHost _dataEntryControlHost;

        // ErrorProviders used to indicate data validation errors/warnings to the user.
        ErrorProvider _validationErrorProvider;
        ErrorProvider _validationWarningErrorProvider;

        // Indicates whether an update of the auto-complete list was requested, but the update was
        // postponed because DataEntryControlHost.UpdateInProgress returned true
        bool _autoCompleteUpdatePending;

        // Maintain private value to avoid using base class for anything
        int _selectedIndex = -1;

        // The component that handles the drop-down list
        LuceneAutoSuggest _luceneAutoSuggest;

        /// <summary>
        /// Initializes a new <see cref="LuceneComboBox"/> instance.
        /// </summary>
        protected private LuceneComboBox()
        {
            try
            {
                InDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                // Load licenses in design mode
                if (InDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI50136", GetType().ToString());

                _luceneAutoSuggest = new LuceneAutoSuggest(this);
                _luceneAutoSuggest.Validating += Handle_LuceneAutoSuggestValidating;
                _luceneAutoSuggest.DropDownClosed += Handle_LuceneAutoSuggestDropDownClosed;
                _luceneAutoSuggest.GotFocus += Handle_LuceneAutoSuggestGotFocus;
                _luceneAutoSuggest.LostFocus += Handle_LuceneAutoSuggestLostFocus;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50137", ex);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataEntryControlHost"/> to which this control belongs
        /// </summary>
        /// <value>The <see cref="DataEntryControlHost"/> to which this control belongs.</value>
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
                try
                {
                    if (_dataEntryControlHost != value)
                    {
                        _dataEntryControlHost = value;
                        LuceneAutoSuggest?.SetDataEntryControlHost(value);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI50224");
                }
            }
        }

        /// <summary>
        /// Determines whether to show the list as soon as this control gets focus
        /// </summary>
        [Category("Data Entry Control")]
        [DefaultValue(AutoDropDownMode.Never)]
        public AutoDropDownMode AutoDropDownMode { get; set; } = AutoDropDownMode.Never;

        /// <summary>
        /// When <c>false</c> the best match will be automatically selected in the list while typing.
        /// When <c>true</c> arrow keys or the mouse must be used to select an item.
        /// </summary>
        [Category("Data Entry Control")]
        [DefaultValue(true)]
        public bool AutomaticallySelectBestMatchingItem { get; set; } = true;

        /// <summary>
        /// The <see cref="DataEntryAutoCompleteMode"/> that this control uses
        /// </summary>
        [Category("Data Entry Control")]
        [DefaultValue(DataEntryAutoCompleteMode.SuggestLucene)]
        public new DataEntryAutoCompleteMode AutoCompleteMode
        {
            get => _SUPPORTED_AUTO_COMPLETE_MODE;
            set
            {
                try
                {
                    if (value != _SUPPORTED_AUTO_COMPLETE_MODE)
                    {
                        throw new NotSupportedException(UtilityMethods.FormatCurrent($"LuceneComboBox doesn't support DataEntryAutoCompleteMode {value}"));
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI50138");
                }
            }
        }

        /// <summary>
        /// The <see cref="ComboBoxStyle"/> that this control uses
        /// </summary>
        public new ComboBoxStyle DropDownStyle
        {
            get => _SUPPORTED_COMBO_BOX_STYLE;
            set
            {
                try
                {
                    if (value != _SUPPORTED_COMBO_BOX_STYLE)
                    {
                        throw new NotSupportedException(UtilityMethods.FormatCurrent($"LuceneComboBox doesn't support ComboBoxStyle {value}"));
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI50139");
                }
            }
        }

        /// <summary>
        /// Whether the list of items is displayed
        /// </summary>
        new public bool DroppedDown
        {
            get
            {
                return LuceneAutoSuggest.DroppedDown;
            }
        }

        /// <summary>
        /// The strings available to select in this control
        /// </summary>
        /// <remarks>
        /// This is implemented to hide the base class method so that the base class Item collection can remain empty.
        /// </remarks>
        public new IList<string> Items
        {
            get
            {
                return ActiveValidator?.GetAutoCompleteValues() ?? new string[0];
            }
        }

        /// <summary>
        /// Gets or sets the index specifying the currently selected item.
        /// </summary>
        /// <remarks>
        /// This is implemented to hide the base class method so that the base class Item collection can remain
        /// empty. If this were an override then the base class would call this and it would receive an invalid index.
        /// </remarks>
        public new int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                try
                {
                    string text = "";
                    if (value != -1)
                    {
                        var items = Items;
                        if (value >= 0 && value < items.Count)
                        {
                            text = items[value];
                        }
                    }

                    if (_selectedIndex != value || Text != text)
                    {
                        _selectedIndex = value;

                        // Set flag while updating the text to avoid generating/searching the list twice
                        try
                        {
                            UpdatingSelectedIndex = true;
                            Text = text;
                        }
                        finally
                        {
                            UpdatingSelectedIndex = false;
                        }

                        OnSelectedIndexChanged(EventArgs.Empty);
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI50140");
                }
            }
        }

        /// <summary>
        /// Gets or sets the text associated with this <see cref="DataEntryTextBox"/>.
        /// </summary>
        /// <value>The text associated with this <see cref="DataEntryTextBox"/>.</value>
        /// <returns>The text associated with this <see cref="DataEntryTextBox"/>.</returns>
        public override string Text
        {
            get
            {
                try
                {
                    return FormatForTextGetter(base.Text);
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI26011");
                    return "";
                }
            }

            set
            {
                try
                {
                    value = FormatForTextSetter(value);

                    if (value != base.Text)
                    {
                        // Set the text of the LuceneAutoSuggest to prevent the drop-down from
                        // being triggered
                        LuceneAutoSuggest.SetText(value, ignoreNextTextChangedEvent: true);

                        // If not already updating the selected index, update it now
                        if (!UpdatingSelectedIndex)
                        {
                            _selectedIndex = Items.IndexOf(value ?? "");
                        }

                        base.Text = value;
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI26012");
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, formats a value to be returned from the Text property
        /// </summary>
        /// <param name="text">The text to be formatted</param>
        protected virtual string FormatForTextGetter(string text)
        {
            return text;
        }

        /// <summary>
        /// When overriden in a derived class, formats a value to be displayed in the control
        /// </summary>
        /// <param name="text">The text to be formatted</param>
        protected virtual string FormatForTextSetter(string text)
        {
            return text;
        }

        /// <summary>
        /// Set the background color of the edit box and the list control
        /// </summary>
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
                LuceneAutoSuggest.SetListBackColor(value);
            }
        }

        /// <summary>
        /// Set while updating the SelectedIndex so that Text can be updating without
        /// generating/searching the list again
        /// </summary>
        protected private bool UpdatingSelectedIndex { get; set; }

        /// <summary>
        /// Whether the current instance is running in design mode
        /// </summary>
        protected private bool InDesignMode { get; }

        /// <summary>
        /// The component that handles the drop-down list
        /// </summary>
        protected private LuceneAutoSuggest LuceneAutoSuggest => _luceneAutoSuggest;

        /// <summary>
        /// The validator currently being used to validate the control's value
        /// </summary>
        protected private IDataEntryValidator ActiveValidator { get; set; }

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

        /// <summary>
        /// Raises the <see cref="Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> containing the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                // [DataEntry:295]
                // Ctrl + A is not implemented by the base ComboBox class.
                if (e.KeyCode == Keys.A && e.Control)
                {
                    SelectAll();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
                {
                    if (!DroppedDown)
                    {
                        OnDropDown(EventArgs.Empty);
                        e.Handled = true;
                    }
                }

                base.OnKeyDown(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI50143", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.Validating"/> event
        /// </summary>
        /// <remarks>
        /// Overridden to disable validation of the ComboBox text while the suggestion list is displayed.
        /// ComboBox validates when clicking on the lucene child control, which is too soon
        /// </remarks>
        protected override void OnValidating(CancelEventArgs e)
        {
            if (!DroppedDown)
            {
                base.OnValidating(e);
            }
        }

        /// <summary>
        /// Prevent the built-in drop down list from showing
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            try
            {

                const int
                    WM_USER = 0x0400,
                    WM_REFLECT = WM_USER + 0x1C00,
                    WM_CTLCOLORLISTBOX = 0x0134,
                    WM_DRAWITEM = 0x002B,
                    WM_MEASUREITEM = 0x002C,
                    WM_LBUTTONDOWN = 0x0201,
                    WM_LBUTTONDBLCLK = 0x0203;

                switch (m.Msg)
                {
                    // Ignore messages that would show the native drop-down
                    case WM_CTLCOLORLISTBOX:
                    case WM_REFLECT + WM_DRAWITEM:
                    case WM_REFLECT + WM_MEASUREITEM:
                        break;

                    // Make double-click behave like normal click to mimic base class behavior
                    case WM_LBUTTONDBLCLK:
                        Focus();
                        if (DroppedDown)
                        {
                            OnDropDownClosed(EventArgs.Empty);
                        }
                        else
                        {
                            OnDropDown(EventArgs.Empty);
                        }
                        break;
                    case WM_LBUTTONDOWN:
                        if (!DroppedDown)
                        {
                            Focus();
                            OnDropDown(EventArgs.Empty);
                        }
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50163");
            }
        }

        // Re-validates the control's data and updates validation error icon as appropriate.
        protected void UpdateValidation(DataValidity dataValidity)
        {
            if (_validationErrorProvider != null || _validationWarningErrorProvider != null)
            {
                if (_validationErrorProvider != null)
                {
                    _validationErrorProvider.SetError(this, dataValidity == DataValidity.Invalid ?
                        ActiveValidator.ValidationErrorMessage : "");
                }

                if (_validationWarningErrorProvider != null)
                {
                    _validationWarningErrorProvider.SetError(this,
                        dataValidity == DataValidity.ValidationWarning ?
                            ActiveValidator.ValidationErrorMessage : "");
                }
            }
        }


        protected void UpdateComboBoxItems()
        {
            // If the host reports that an update is in progress, delay updating the auto-complete
            // list since the update may otherwise result in the auto-complete list being changed
            // multiple times before the update is over.
            if (DataEntryControlHost != null && DataEntryControlHost.UpdateInProgress)
            {
                if (!_autoCompleteUpdatePending)
                {
                    _autoCompleteUpdatePending = true;
                    DataEntryControlHost.UpdateEnded += Handle_DataEntryControlHostUpdateEnded;
                }

                return;
            }
            else if (_autoCompleteUpdatePending)
            {
                DataEntryControlHost.UpdateEnded -= Handle_DataEntryControlHostUpdateEnded;
                _autoCompleteUpdatePending = false;
            }

            LuceneAutoSuggest.UpdateAutoCompleteList(ActiveValidator.AutoCompleteValuesWithSynonyms);
        }

        /// <summary>
        /// Handles the case the a significant update of control data as reported by
        /// <see cref="DataEntryControlHost"/> UpdateInProgress has ended.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void Handle_DataEntryControlHostUpdateEnded(object sender, EventArgs e)
        {
            try
            {
                DataEntryControlHost.UpdateEnded -= Handle_DataEntryControlHostUpdateEnded;
                _autoCompleteUpdatePending = false;

                UpdateComboBoxItems();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30112", ex);
            }
        }

        void Handle_LuceneAutoSuggestValidating(object sender, CancelEventArgs e)
        {
            try
            {
                base.OnValidating(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50168");
            }
        }

        void Handle_LuceneAutoSuggestDropDownClosed(object sender, EventArgs e)
        {
            try
            {
                OnDropDownClosed(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50169");
            }
        }

        void Handle_LuceneAutoSuggestGotFocus(object sender, EventArgs e)
        {
            try
            {
                OnGotFocus(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50170");
            }
        }

        void Handle_LuceneAutoSuggestLostFocus(object sender, EventArgs e)
        {
            try
            {
                base.OnLostFocus(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50171");
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _luceneAutoSuggest.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
