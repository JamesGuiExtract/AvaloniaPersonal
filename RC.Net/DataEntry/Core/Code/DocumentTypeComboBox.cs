using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    public interface ICanSuppressFocusEvents
    {
        event EventHandler<CancelEventArgs> LosingFocus;
    }

    /// <summary>
    /// A <see cref="ComboBox"/> that implements <see cref="IDataEntryAutoCompleteControl"/> to provide suggested
    /// values for selecting DocumentTypes
    /// </summary>
    public partial class DocumentTypeComboBox : ComboBox, ICanSuppressFocusEvents, IDataEntryAutoCompleteControl, IRequiresErrorProvider
    {
        static readonly string _OBJECT_NAME = typeof(DocumentTypeComboBox).ToString();

        readonly ComboBoxStyle _SUPPORTED_COMBO_BOX_STYLE = ComboBoxStyle.DropDown;
        readonly DataEntryAutoCompleteMode _SUPPORTED_AUTO_COMPLETE_MODE = DataEntryAutoCompleteMode.SuggestLucene;

        // ErrorProviders used to indicate data validation errors/warnings to the user.
        ErrorProvider _validationErrorProvider;
        ErrorProvider _validationWarningErrorProvider;

        // To figure out licensing req
        bool _inDesignMode;

        IDataEntryValidator _validator;

        // Reusable object to support using DataEntryValidator
        IAttribute _attribute;

        // Component that handles the drop-down list
        LuceneAutoSuggest _luceneAutoSuggest;

        /// <summary>
        /// Initializes a new <see cref="DocumentTypeComboBox"/> instance.
        /// </summary>
        public DocumentTypeComboBox()
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
                    LicenseIdName.DataEntryCoreComponents, "ELI50136", _OBJECT_NAME);

                InitializeComponent();

                _luceneAutoSuggest = new LuceneAutoSuggest(this);
                _luceneAutoSuggest.Validating += Handle_LuceneAutoSuggestValidating;
                _luceneAutoSuggest.DropDownClosed += Handle_LuceneAutoSuggestDropDownClosed;
                _luceneAutoSuggest.GotFocus += Handle_LuceneAutoSuggestGotFocus;
                _luceneAutoSuggest.LostFocus += Handle_LuceneAutoSuggestLostFocus;
                _attribute = new AttributeClass { Name = "DocumentType" };
                _attribute.Value.CreateNonSpatialString("", "None");
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50137", ex);
            }
        }

        /// <summary>
        /// Raised when this control gets focus
        /// </summary>
        new public event EventHandler GotFocus;

        /// <summary>
        /// Rased when this control loses focus
        /// </summary>
        new public event EventHandler LostFocus;

        /// <summary>
        /// Raised after this control loses focus but before the <see cref="LostFocus"/> event
        /// is raised to allow that event to be suppressed
        /// </summary>
        public event EventHandler<CancelEventArgs> LosingFocus;

        /// <summary>
        /// The <see cref="DataEntryAutoCompleteMode"/> that this control uses
        /// </summary>
        public new DataEntryAutoCompleteMode AutoCompleteMode
        {
            get => _SUPPORTED_AUTO_COMPLETE_MODE;
            set
            {
                try
                {
                    if (value != _SUPPORTED_AUTO_COMPLETE_MODE)
                    {
                        throw new NotSupportedException(UtilityMethods.FormatCurrent($"DocumentTypeComboBox doesn't support DataEntryAutoCompleteMode {value}"));
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI50138");
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
                        throw new NotSupportedException(UtilityMethods.FormatCurrent($"DocumentTypeComboBox doesn't support ComboBoxStyle {value}"));
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI50139");
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
                return _luceneAutoSuggest.DroppedDown;
            }
        }

        /// <summary>
        /// Gets or sets the index specifying the currently selected item.
        /// </summary>
        public override int SelectedIndex
        {
            get
            {
                return base.SelectedIndex;
            }
            set
            {
                try
                {
                    if (SelectedIndex != value)
                    {
                        // Prevent list from showing during DEP initialization
                        // by setting the text before the base method does it.
                        // This way when Text is set it will be ignored by the
                        // LuceneAutoSuggest
                        if (value == -1)
                        {
                            _luceneAutoSuggest.SetText("", ignoreNextTextChangedEvent: true);
                        }
                        else
                        {
                            if (value >= 0 && value < Items.Count)
                            {
                                _luceneAutoSuggest.SetText(Items[value].ToString(), ignoreNextTextChangedEvent: true);
                            }
                        }

                        base.SelectedIndex = value;
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI50140");
                }
            }
        }

        /// <summary>
        /// Get/set the text. This will correct the case and trim the value if needed to match a valid value
        /// </summary>
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                try
                {
                    var newValue = value;
                    var dataValidity = Validate(newValue, true, out var correctedValue);

                    // If a corrected value was returned, use that instead of the supplied value
                    if (!string.IsNullOrEmpty(correctedValue))
                    {
                        newValue = correctedValue;
                    }
                    if (newValue != base.Text)
                    {
                        // Set the text of the LuceneAutoSuggest to prevent the drop-down from
                        // being triggered
                        _luceneAutoSuggest.SetText(newValue, ignoreNextTextChangedEvent: true);
                        base.Text = newValue;
                        UpdateValidation(dataValidity);
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI50141");
                }
            }
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
                _luceneAutoSuggest.SetListBackColor(value);
            }
        }

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
        /// Sets the valid values for this control
        /// </summary>
        public void SetAutoCompleteValues(IEnumerable<string> validValues)
        {
            try
            {
                var validValueMatrix = validValues
                    .Select(docType => new[] { docType })
                    .ToArray();
                if (validValueMatrix.Length == 0)
                {
                    _validator = null;
                }
                else if (_validator == null)
                {
                    var validator = new DataEntryValidator();
                    validator.SetAutoCompleteValues(validValueMatrix);
                    _validator = validator;
                    UpdateComboBoxItems();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50142");
            }
        }

        /// <summary>
        /// This registers an <see cref="IMessageFilter"/> with the supplied <see cref="DataEntryControlHost"/> to enable the drop down to function correctly
        /// </summary>
        /// <param name="value">The <see cref="DataEntryControlHost"/> that this control will filter messages for</param>
        public void SetDataEntryControlHost(DataEntryControlHost value)
        {
            _luceneAutoSuggest.SetDataEntryControlHost(value);
        }

        /// <summary>
        /// Raises the <see cref="LostFocus"/> event
        /// </summary>
        protected override void OnLostFocus(EventArgs e)
        {
            try
            {
                var cancelEvent = new CancelEventArgs();
                LosingFocus?.Invoke(this, cancelEvent);

                if (!cancelEvent.Cancel)
                {
                    LostFocus?.Invoke(this, e);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50166");
            }
        }

        /// <summary>
        /// Raises the <see cref="GotFocus"/> event
        /// </summary>
        protected override void OnGotFocus(EventArgs e)
        {
            try
            {
                GotFocus?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50167");
            }
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
                    OnDropDown(EventArgs.Empty);
                    e.Handled = true;
                }

                base.OnKeyDown(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI50143", ex);
            }
        }

        /// <summary>
        /// Overridden to disable validation of the ComboBox text. The ComboBox validates when clicking on the lucene child control, which is too soon
        /// </summary>
        /// <param name="e"></param>
        protected override void OnValidating(CancelEventArgs e)
        {
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
                    case WM_CTLCOLORLISTBOX:
                    case WM_REFLECT + WM_DRAWITEM:
                    case WM_REFLECT + WM_MEASUREITEM:
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
        void UpdateValidation(DataValidity dataValidity)
        {
            if (_validationErrorProvider != null || _validationWarningErrorProvider != null)
            {
                if (_validationErrorProvider != null)
                {
                    _validationErrorProvider.SetError(this, dataValidity == DataValidity.Invalid ?
                        _validator.ValidationErrorMessage : "");
                }

                if (_validationWarningErrorProvider != null)
                {
                    _validationWarningErrorProvider.SetError(this,
                        dataValidity == DataValidity.ValidationWarning ?
                            _validator.ValidationErrorMessage : "");
                }
            }
        }

        DataValidity Validate(string inputValue, bool correctValue, out string correctedValue)
        {
            if (_validator == null)
            {
                correctedValue = null;
                return DataValidity.Valid;
            }

            _attribute.Value.ReplaceAndDowngradeToNonSpatial(inputValue);

            DataValidity dataValidity;
            if (correctValue)
            {
                dataValidity = _validator.Validate(_attribute, false, out correctedValue);
            }
            else
            {
                dataValidity = _validator.Validate(_attribute, false);
                correctedValue = null;
            }

            return dataValidity;
        }

        void UpdateComboBoxItems()
        {
            var autoCompleteValues = _validator.GetAutoCompleteValues();
            _luceneAutoSuggest.UpdateAutoCompleteList(_validator.AutoCompleteValuesWithSynonyms);

            Items.Clear();
            if (autoCompleteValues != null)
            {
                Items.AddRange(autoCompleteValues);
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
                GotFocus?.Invoke(this, e);
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
                LostFocus?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50171");
            }
        }
    }
}
