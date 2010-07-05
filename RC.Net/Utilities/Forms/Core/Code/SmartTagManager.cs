using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// The <see cref="EventArgs"/> used by the <see cref="SmartTagManager.ApplyingValue"/>
    /// event.
    /// </summary>
    public class SmartTagApplyingValueEventArgs : EventArgs
    {
        /// <summary>
        /// The text value to be applied to the active text control.
        /// </summary>
        string _value;

        /// <summary>
        /// <see langword="true"/> if a specified smart tag name was selected and the value being
        /// applied is the associated tag value, <see langword="false"/> if a recognized tag name
        /// was not selected and the text being applied is whatever text the user typed while the
        /// <see cref="SmartTagManager"/> was active.
        /// </summary>
        readonly bool _smartTagSelected;

        /// <summary>
        /// Initialized a new <see cref="SmartTagApplyingValueEventArgs"/> instance.
        /// </summary>
        /// <param name="value">The text value to be applied to the active text control.</param>
        /// <param name="smartTagSelected"><see langword="true"/> if a specified smart tag name
        /// was selected and the value being applied is the associated tag value,
        /// <see langword="false"/> if a recognized tag name was not selected and the text being
        /// applied is whatever text the user typed while the <see cref="SmartTagManager"/> was active.
        /// </param>
        public SmartTagApplyingValueEventArgs(string value, bool smartTagSelected)
        {
            _value = value;
            _smartTagSelected = smartTagSelected;
        }

        /// <summary>
        /// Gets or sets the text value to be applied to the active text control.
        /// </summary>
        /// <value>
        /// The text to be applied to that active text control. Can be modified if any
        /// interpretation of the text value is needed (ie, the text value is a query).
        /// </value>
        public string Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// Gets whether a recognized smart tag was selected.
        /// </summary>
        /// <value><see langword="true"/> if a specified smart tag name was selected and the value
        /// being applied is the associated tag value, <see langword="false"/> if a recognized tag
        /// name was not selected and the text being applied is whatever text the user typed while the
        /// <see cref="SmartTagManager"/> was active.</value>
        public bool SmartTagSelected
        {
            get
            {
                return _smartTagSelected;
            }
        }
    }

    /// <summary>
    /// Monitors a specified <see cref="Control"/> and its decendants for macros entered into any
    /// <see cref="TextBoxBase"/> derivate denoted by a word beginning with a period and replaces
    /// any specified smart tag macro with its associated value.
    /// </summary>
    public partial class SmartTagManager : TextBox
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(SmartTagManager).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="Control"/> whose hierarchy will be monitored for smart tag entry.
        /// </summary>
        Control _parentControl;

        /// <summary>
        /// The smart tag names mapped to the associated values.
        /// </summary>
        Dictionary<string, string> _smartTags = new Dictionary<string, string>();

        /// <summary>
        /// A <see cref="TextBoxBase"/> derivative where a smart tag is currently being entered.
        /// </summary>
        TextBoxBase _activeTextControl;

        /// <summary>
        /// The position in _activeTextControl that a smart tag is being entered.
        /// </summary>
        int _activeTextPosition;

        /// <summary>
        /// The length of the active smart tag text.
        /// </summary>
        int _activeTextLength;

        #endregion Fields

        #region Delegates

        /// <summary>
        /// Delegate for a function that takes two <see langword="bool"/> parameters.
        /// </summary>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        delegate void TwoBoolParametersDelegate(bool arg1, bool arg2);

        #endregion Delegates

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="SmartTagManager"/> instance.
        /// </summary>
        /// <param name="parentControl">The <see cref="Control"/> whose hierarchy will be monitored
        /// for smart tag entry.</param>
        /// <param name="smartTags"></param>
        public SmartTagManager(Control parentControl, Dictionary<string, string> smartTags)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28883",
                    _OBJECT_NAME);

                // No borders so the control will blend in with the active text control.
                BorderStyle = BorderStyle.None;

                // Initialize the auto-complete list
                UpdateSmartTags(smartTags);

                // Register for necesssary events to watch for smart-tag entry.
                _parentControl = parentControl;
                RegisterControlHierarchy(parentControl);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28882", ex);
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised whenever the <see cref="SmartTagManager"/> is applying text back to the active
        /// text control. If the value being applied needs further interpretation (ie, the value is
        /// a query), the value can be modified via this event.
        /// </summary>
        public event EventHandler<SmartTagApplyingValueEventArgs> ApplyingValue;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets whether the <see cref="SmartTagManager"/> is currently active.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="SmartTagManager"/> is currently active
        /// and handling input on behalf of the active text control, <see langword="false"/>
        /// otherwise.</returns>
        public bool IsActive
        {
            get
            {
                return _activeTextControl != null;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Updates the available set of smart tags.
        /// </summary>
        /// <param name="smartTags">The new set of smart tags. Cannot be <see langword="null"/>.
        /// </param>
        public void UpdateSmartTags(Dictionary<string, string> smartTags)
        {
            try
            {
                ExtractException.Assert("ELI28898", "Smart tag list not specified!",
                    smartTags != null);

                // Update _smartkey dictionary and the auto-complete list with a sorted list of tag
                // names.
                _smartTags.Clear();

                if (smartTags.Count > 0)
                {
                    AutoCompleteStringCollection autoCompleteList = new AutoCompleteStringCollection();
                    List<string> smartTagNames = new List<string>(smartTags.Keys);
                    smartTagNames.Sort();
                    foreach (string smartTagName in smartTagNames)
                    {
                        string tagKey = "." + smartTagName.ToLower(CultureInfo.CurrentCulture);
                        autoCompleteList.Add(tagKey);
                        _smartTags[tagKey] = smartTags[smartTagName];
                    }

                    AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    AutoCompleteSource = AutoCompleteSource.CustomSource;
                    AutoCompleteCustomSource = autoCompleteList;
                }
                else if (AutoCompleteMode != AutoCompleteMode.None)
                {
                    AutoCompleteMode = AutoCompleteMode.None;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28874", ex);
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Control.PreviewKeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="PreviewKeyDownEventArgs"/> containing the event data.
        /// </param>
        // This event handler has undergone a security review.
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            try
            {
                // Close the smart tag auto-complete if the user hits escape.
                if (e.KeyCode == Keys.Escape)
                {
                    TryApplyTag(true, true);
                }
                // Allow enter key to act as a selection of the current value.
                else if (e.KeyCode == Keys.Enter)
                {
                    TryApplyTag(true, false);
                }
                else
                {
                    base.OnPreviewKeyDown(e);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28892", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.LostFocus"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            try
            {
                base.OnLostFocus(e);

                // If the smart tag manager has lost focus, ensure whatever has been entered thus
                // far is applied to any active text control.
                // Invoke TryApplyTag to run asynchronously from the mesage que to conform with
                // Mircrosoft guidelines for calling Focus as specified on this page:
                // http://msdn.microsoft.com/en-us/library/system.windows.forms.control.enter.aspx
                BeginInvoke(new TwoBoolParametersDelegate(TryApplyTag),
                    new object[] { true, false });
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28899", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.KeyUp"/> event.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            try
            {
                base.OnKeyUp(e);

                // If text has changed and the auto-complete list is no longer visible we want to
                // apply the smart tag. 
                // [DataEntry:936]
                // This used to be called in OnTextChanged. However, there are some situations in
                // which the OnTextChanged method did not seem to be called. In addition to being
                // more consistent, using OnKeyUp has the advantage that the auto-complete list
                // will have already closed if it is going to close and, therefore, TryApplyTag can
                // be called directly rather than invoking it.
                if (IsActive)
                {
                    TryApplyTag(false, false);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28900", ex);
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.ControlAdded"/> event for _parentControl or any
        /// controls it contains.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="ControlEventArgs"/> that contains the event data.</param>
        void HandleChildControlAdded(object sender, ControlEventArgs e)
        {
            try
            {
                RegisterControlHierarchy(e.Control);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28870", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.ControlRemoved"/> event for _parentControl or any
        /// controls it contains.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="ControlEventArgs"/> that contains the event data.</param>
        void HandleChildControlRemoved(object sender, ControlEventArgs e)
        {
            try
            {
                UnRegisterControlHierarchy(e.Control);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28871", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.GotFocus"/> event for any <see cref="TextBoxBase"/>
        /// derivative in _parentControl's hierarchy.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleTextBoxGotFocus(object sender, EventArgs e)
        {
            try
            {
                // Track text changes in the focused control in order to take over input if the user
                // begins to enter a smart tag.
                TextBoxBase focusedTextControl = (TextBoxBase)sender;
                focusedTextControl.TextChanged += HandleTextBoxControlTextChanged;
                focusedTextControl.LostFocus += HandleFocusedTextControlLostFocus;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28872", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.LostFocus"/> event for any <see cref="TextBoxBase"/>
        /// derivative in _parentControl's hierarchy.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleFocusedTextControlLostFocus(object sender, EventArgs e)
        {
            try
            {
                // Stop tracking text changes when a control loses focus.
                TextBoxBase lastTextControl = (TextBoxBase)sender;
                lastTextControl.TextChanged -= HandleTextBoxControlTextChanged;
                lastTextControl.LostFocus -= HandleFocusedTextControlLostFocus;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28873", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the TextChanged event for any <see cref="TextBoxBase"/> control in 
        /// _parentControl's hierarchy that currently has focus.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleTextBoxControlTextChanged(object sender, EventArgs e)
        {
            try
            {
                if (IsActive)
                {
                    return;
                }

                TextBoxBase textControl = (TextBoxBase)sender;

                bool allowInitialTagSelection = false;
                int smartTagPosition = -1;
                string activeText = "";
                string currentText = textControl.Text.Substring(0, textControl.SelectionStart);
                int textLength = currentText.Length;

                // Check to see if the user has used a period to start a smart tag.
                if (currentText.Equals(".", StringComparison.Ordinal) ||
                    (textLength >= 2 && currentText[textLength - 1] == '.' &&
                     char.IsWhiteSpace(currentText[textLength - 2])))
                {
                    smartTagPosition = textControl.SelectionStart - 1;
                    activeText = ".";
                    allowInitialTagSelection = true;
                }
                // Check to see if the user has modified a text that is a potential smart tag.
                else
                {
                    int index = currentText.LastIndexOf('.');
                    if (index == 0 || (index > 0 && char.IsWhiteSpace(currentText[index - 1])))
                    {
                        activeText = currentText.Substring(index);
                        foreach (string tagName in _smartTags.Keys)
                        {
                            if (tagName.StartsWith(activeText, StringComparison.OrdinalIgnoreCase))
                            {
                                smartTagPosition = index;
                            }
                        }
                    }
                }

                // If smart tag creation or edit was detected, activate the smart tag control.
                if (smartTagPosition >= 0)
                {
                    // Initialize the font and background color so that the control blends in with
                    // the active text control.
                    Font = textControl.Font;
                    BackColor = textControl.BackColor;
                    textControl.Controls.Add(this);

                    // Send characters from the portion of the smart tag already typed in the text
                    // control to activate the smart tag control's auto-complete list.
                    Text = "";
                    Focus();
                    foreach (char character in activeText.ToCharArray())
                    {
                        KeyMethods.SendCharacterToControl(character, this);
                    }

                    // The SmartTagManager is now active; initialize the active text fields so that
                    // text changes are repeated back to the active text control.
                    _activeTextPosition = smartTagPosition;
                    _activeTextLength = activeText.Length;
                    _activeTextControl = textControl;

                    // If a default smart tag selection is not allowed (for an edit), remove any
                    // initial selection.
                    if (!allowInitialTagSelection && SelectionLength > 0)
                    {
                        KeyMethods.SendKeyToControl((int)Keys.Back, false, false, false, null);
                    }
                    // Otherwise update the text control to reflect the automatically suggested tag.
                    else if (SelectionLength > 0)
                    {
                        ApplyText(Text);
                    }

                    // Position the smart tag control so that the text exactly matches up with the
                    // text being typed in active text control.
                    PositionControl();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28869", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();

                // In case of an exception, ensure the smart tag window doesn't hang around.
                SafeDeactivate();
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Register to receive <see cref="Control.ControlAdded"/> and
        /// <see cref="Control.ControlRemoved"/> events from all controls in
        /// <see paramref="control"/>' hierarchy so that the entire hierachy can be tracked even as
        /// it changed. Any <see cref="TextBoxBase"/> derivatives in the heirarchy are registered
        /// for <see cref="Control.GotFocus"/> events so that they can be monitored for smart tag
        /// creation.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> whose entire hierarchy should be
        /// monitored for smart tag creation.</param>
        void RegisterControlHierarchy(Control control)
        {
            if (control != this)
            {
                control.ControlAdded += HandleChildControlAdded;
                control.ControlRemoved += HandleChildControlRemoved;

                if (control is TextBoxBase)
                {
                    control.GotFocus += HandleTextBoxGotFocus;
                }

                foreach (Control childControl in control.Controls)
                {
                    RegisterControlHierarchy(childControl);
                }
            }
        }

        /// <summary>
        /// Unregisters events so that <see paramref="control"/>'s hierarchy is no longer monitored
        /// for smart tag creation.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> whose hierarchy should no longer be
        /// monitored for smart tag creation.</param>
        void UnRegisterControlHierarchy(Control control)
        {
            if (control != this)
            {
                control.ControlAdded -= HandleChildControlAdded;
                control.ControlRemoved -= HandleChildControlRemoved;

                if (control is TextBoxBase)
                {
                    control.GotFocus -= HandleTextBoxGotFocus;
                }

                foreach (Control childControl in control.Controls)
                {
                    UnRegisterControlHierarchy(childControl);
                }
            }
        }

        /// <summary>
        /// Attempts to apply the current value back to the active text control. If the value
        /// matches that of a tag name, the associated value will be applied; otherwise the
        /// current text will be applied.
        /// </summary>
        /// <param name="forceClose"><see langword="true"/> if the smart tag manager should be
        /// deactivated whether or not the auto-complete list is still visible,
        /// <see langword="false"/> if it should remain open if the auto-complete list is open.
        /// </param>
        /// <param name="cancel"><see langword="true"/> if smart tag insertion should be cancelled
        /// the currently entered text should be applied without looking up the tag value,
        /// <see langword="false"/> otherwise.</param>
        void TryApplyTag(bool forceClose, bool cancel)
        {
            try
            {
                // If not active, there is nothing that can be applied.
                if (!IsActive)
                {
                    return;
                }

                // If cancelling or the auto-complete list is no longer being displayed, check to
                // see if the current text matches a configured tag and deactivate the manager.
                if (forceClose || !NativeMethods.IsAutoCompleteDisplayed())
                {
                    // Set _activeTextControl to null right away to prevent possibility of recursion. 
                    TextBoxBase textControl = _activeTextControl;
                    _activeTextControl = null;

                    if (!cancel)
                    {
                        // Look up the smart tag value
                        bool smartTagSelected = true;
                        string lowerCaseText = Text.ToLower(CultureInfo.CurrentCulture);
                        string value;
                        if (!_smartTags.TryGetValue(lowerCaseText, out value))
                        {
                            smartTagSelected = false;
                            value = Text;
                        }

                        // Raise the ApplyingValue event to allow listeners to update the value.
                        SmartTagApplyingValueEventArgs applyingValueEventArgs =
                            new SmartTagApplyingValueEventArgs(value, smartTagSelected);
                        OnApplyingValue(applyingValueEventArgs);

                        // Replace text in the active text control with the smart tag's value.
                        ApplyText(applyingValueEventArgs.Value);
                    }
                    // If smart tag entry was cancelled, just apply the currently entered value.
                    else
                    {
                        // Remove any auto-suggested text.
                        if (SelectionLength > 0)
                        {
                            Text = Text.Remove(SelectionStart, SelectionLength);
                        }

                        ApplyText(Text);
                    }
                    
                    // Give the active text control focus before removing the smart tag control so
                    // that focus doesn't jump elsewhere in the process.
                    textControl.Focus();
                    textControl.Controls.Remove(this);
                }
                // The smart tag manager will remain active-- update the active text control's text
                // and check the position of the smart tag control to ensure the text lines up
                // properly.
                else
                {
                    ApplyText(Text);
                    PositionControl();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee =
                    new ExtractException("ELI28880", "Failed to insert smart tag!", ex);
                ee.Display();

                // In case of an exception, ensure the smart tag window doesn't hang around.
                SafeDeactivate();
            }
        }

        /// <summary>
        /// Applies the specified text to the specified text control.
        /// </summary>
        /// <param name="text">The <see langword="string"/> to apply to the active text control at
        /// the specified position.</param>
        void ApplyText(string text)
        {
            TextBoxBase textControl = (TextBoxBase)Parent;

            textControl.Text =
                        textControl.Text.Remove(_activeTextPosition, _activeTextLength);
            textControl.Text =
                textControl.Text.Insert(_activeTextPosition, text);

            _activeTextLength = text.Length;
            textControl.Select(_activeTextPosition + _activeTextLength, 0);
        }

        /// <summary>
        /// Positions and sizes the the smart tag control so that text entered into this control
        /// appears as if it is being entered into the active text control.
        /// </summary>
        void PositionControl()
        {
            // [DataEntry:836, 873]
            // Ensure the manager is still active.
            if (Parent == null)
            {
                return;
            }
            
            TextBoxBase textControl = (TextBoxBase)Parent;

            // Find the point on screen to place this control.
            Point newLocation = textControl.GetPositionFromCharIndex(_activeTextPosition);
            if (Location != newLocation)
            {
                Location = newLocation;
            }

            // Calculate the size the control needs to be so that it fits exactly the text it
            // contains without obscuring any other text in textControl.
            Size regionSize = TextRenderer.MeasureText(Text, Font, Size, 
                TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding);

            // TODO: Can't figure out why measure text is returning a result that is consistently
            // too wide (I've tried a number of TextFormatFlags combinations). Taking off six
            // pixels of width compensates (on my system, at least).
            regionSize.Width -= 6;
            
            // Don't actually resize the control since that messed up the size of the auto-complete
            // list. Instead, update the Region property so that only the portion of the control
            // that contains text is drawn.
            Region = new Region(new Rectangle(ClientRectangle.Location, regionSize));
        }

        /// <summary>
        /// Deactivates (hides) this control without the possibility of raising an exception.
        /// <para><b>Note</b></para>
        /// This call should only be used in the context of handling an exception.
        /// </summary>
        void SafeDeactivate()
        {
            try
            {
                if (_activeTextControl != null)
                {
                    _activeTextControl.TextChanged -= HandleTextBoxControlTextChanged;
                    _activeTextControl.LostFocus -= HandleFocusedTextControlLostFocus;
                }

                _activeTextControl = null;
                
                if (Parent != null)
                {
                    Parent.Controls.Remove(this);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI28901", ex);   
            }
        }

        /// <summary>
        /// Raises <see cref="ApplyingValue"/> event.
        /// </summary>
        /// <param name="smartTagApplyingValueEventArgs">The 
        /// <see cref="SmartTagApplyingValueEventArgs"/> for the <see cref="ApplyingValue"/> event.
        /// </param>
        void OnApplyingValue(SmartTagApplyingValueEventArgs smartTagApplyingValueEventArgs)
        {
            if (ApplyingValue != null)
            {
                ApplyingValue(this, smartTagApplyingValueEventArgs);
            }
        }

        #endregion Private Members
    }
}
