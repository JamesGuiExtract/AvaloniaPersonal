using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.DataEntry
{
    class LuceneAutoSuggest: IMessageFilter, IDisposable
    {
        #region Fields

        Form _ancestorForm;
        Control _control;
        IDataEntryAutoCompleteControl _dataEntryAutoCompleteControl;
        ListBox _listBoxChild;
        bool _msgFilterActive = false;
        Lazy<LuceneSuggestionProvider<KeyValuePair<string, List<string>>>> _providerSource;

        // Lambda to unregister from events registered in the ctor
        Action _unregister;

        // Flags used to suppress some redundant events
        bool _ignoreTextChange;
        bool _ignoreMouseDown;
        bool _suppressListBoxChildLostFocusEvent;
        bool _ignoreControlGotFocusEvent;

        // The value to revert to if the dropped down is canceled (closed with escape or lost focus)
        string _acceptedText;

        // Provides message filtering capability
        DataEntryControlHost _dataEntryControlHost;

        // The color to use for the list
        Color _listBoxChildBackColor;

        // List of values for displaying without a search
        Lazy<object[]> _autoCompleteValues;

        // To detect redundant calls
        bool _disposedValue = false;

        #endregion Fields

        #region Properties

        // The object that manages the Lucene index
        private Lazy<LuceneSuggestionProvider<KeyValuePair<string, List<string>>>> ProviderSource
        {
            get
            {
                return _providerSource;
            }
            set
            {
                if (_providerSource != value)
                {
                    var old = _providerSource;

                    _providerSource = value;

                    Provider = search => _providerSource.Value.GetSuggestions(search.Text,
                        maxSuggestions: search.MaxSuggestions ?? int.MaxValue,
                        excludeLowScoring: search.ExcludeLowScoring);

                    if (old != null && old.IsValueCreated)
                    {
                        old.Value.Dispose();
                    }

                    // Start lazy instantiation to reduce delay when user starts to type
                    // (Use of lazy objects helps prevent the UI from opening sluggishly
                    //  https://extract.atlassian.net/browse/ISSUE-15673
                    // )
                    Task.Factory.StartNew(() => _providerSource.Value);
                }
            }
        }

        /// <summary>
        /// Function that returns suggestions based on a search string
        /// </summary>
        private Func<Search, IEnumerable<object>> Provider { get; set; }

        /// <summary>
        /// Whether the suggestion list is showing
        /// </summary>
        public bool DroppedDown { get; private set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Private constructor to handle common ctor work
        /// </summary>
        /// <param name="control">The editing control: Either a LuceneComboBox or a TextBoxBase</param>
        /// <param name="dataEntryAutoCompleteControl">The <see cref="IDataEntryAutoCompleteControl"/> used
        /// to determine whether to react to text changes with suggestions or not</param>
        private LuceneAutoSuggest(Control control, IDataEntryAutoCompleteControl dataEntryAutoCompleteControl)
        {
            try
            {
                ExtractException.Assert("ELI45347", "Unknown type of control",
                    control is LuceneComboBox || control is TextBoxBase);

                _control = control;
                _listBoxChildBackColor = control.BackColor;
                _dataEntryAutoCompleteControl = dataEntryAutoCompleteControl;

                // Set up all the events we need to handle
                var textChangedEventSequence =
                    Observable.FromEventPattern(h => control.TextChanged += h, h => control.TextChanged -= h)
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .Subscribe(HandleControl_TextChanged);
                control.LostFocus += HandleControl_LostFocus;
                control.GotFocus += HandleControl_GotFocus;
                control.MouseDown += HandleControl_MouseDown;
                control.HandleDestroyed += HandleControl_HandleDestroyed;

                // Set up event unregistration
                _unregister = () =>
                {
                    textChangedEventSequence.Dispose();
                    control.LostFocus -= HandleControl_LostFocus;
                    control.GotFocus -= HandleControl_GotFocus;
                    control.MouseDown -= HandleControl_MouseDown;
                    control.HandleDestroyed -= HandleControl_HandleDestroyed;
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45348");
            }
        }

        /// <summary>
        /// Creates a suggestion drop-down for a <see cref="LuceneComboBox"/>
        /// </summary>
        /// <param name="control">The control for which to display the suggestions</param>
        public LuceneAutoSuggest(LuceneComboBox comboBox)
            : this(comboBox, comboBox)
        {
            try
            {
                // Set up additional events needed for combo boxes
                comboBox.DropDown += HandleControl_DropDown;
                comboBox.DropDownClosed += HandleControl_DropDownClosed;

                // Set up event unregistration
                _unregister += () =>
                {
                    comboBox.DropDown -= HandleControl_DropDown;
                    comboBox.DropDownClosed -= HandleControl_DropDownClosed;
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50129");
            }
        }

        /// <summary>
        /// Creates a suggestion drop-down for a <see cref="DataEntryTextBox"/>
        /// </summary>
        /// <param name="control">The control for which to display the suggestions</param>
        public LuceneAutoSuggest(DataEntryTextBox control)
            : this(control, control)
        { }

        /// <summary>
        /// Creates a suggestion drop-down for a <see cref="TextBox"/> that is, e.g., the
        /// editing control for a <see cref="DataGridView"/>
        /// </summary>
        /// <param name="control">The control for which to display the suggestions</param>
        /// <param name="dataEntryControl">The <see cref="IDataEntryAutoCompleteControl"/> used
        /// to determine whether to react to text changes with suggestions or not</param>
        public LuceneAutoSuggest(TextBox control, IDataEntryAutoCompleteControl dataEntryControl)
            : this((Control)control, dataEntryControl)
        {
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Sets internal state so that text can be reverted on cancel (e.g., pressing escape while the list is displayed)
        /// </summary>
        /// <param name="text">The value to be reverted to on cancel</param>
        /// <param name="ignoreNextTextChangedEvent">If <c>true</c> the next TextChanged event from the editing control
        /// will not show the suggestion list</param>
        public void SetText(string text, bool ignoreNextTextChangedEvent)
        {
            _ignoreTextChange = ignoreNextTextChangedEvent;
            _acceptedText = text;
        }

        /// <summary>
        /// Sets the <see cref="DataEntryControlHost"/> that will use this instance as as <see cref="IMessageFilter"/>
        /// </summary>
        internal void SetDataEntryControlHost(DataEntryControlHost value)
        {
            if (_msgFilterActive)
            {
                _dataEntryControlHost?.RemoveMessageFilter(this);
                value?.AddMessageFilter(this);
            }
            _dataEntryControlHost = value;
        }

        #endregion Public Methods

        #region Events

        /// <summary>
        /// Raised when an item is selected from the suggestion list
        /// </summary>
        public event EventHandler<CancelEventArgs> Validating;

        /// <summary>
        /// Raised when the suggestion list is closed
        /// </summary>
        public event EventHandler DropDownClosed;

        /// <summary>
        /// Raised when the edit control or the suggestion list gets focus
        /// </summary>
        public event EventHandler GotFocus;

        /// <summary>
        /// Raised when the edit control or the suggestion list loses focus
        /// </summary>
        public event EventHandler LostFocus;

        #endregion

        #region Event Handlers

        void HandleControl_HandleDestroyed(object sender, EventArgs e)
        {
            try
            {
                RemoveMessageFilter();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45350");
            }
        }

        void HandleControl_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (!_ignoreMouseDown)
                {
                    HideTheList();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45351");
            }
        }

        // Support auto-drop-down-on-focus
        void HandleControl_GotFocus(object sender, EventArgs e)
        {
            try
            {
                if (_ignoreControlGotFocusEvent)
                {
                    return;
                }

                if (_dataEntryAutoCompleteControl.AutoDropDownMode == AutoDropDownMode.Always
                    || _dataEntryAutoCompleteControl.AutoDropDownMode == AutoDropDownMode.WhenEmpty
                        && string.IsNullOrWhiteSpace(_control.Text))
                {
                    // Mouse clicks fail to auto-drop the list unless this is begin-invoked, I think because
                    // something the DataEntryControlHost does causes the list to close immediately
                    _control.SafeBeginInvoke("ELI50388",
                        () => DropDown(new Search(null, _dataEntryAutoCompleteControl), true),
                        displayExceptions: false);
                }

            }
            catch (Exception ex)
            {
                // Avoid potential infinite loop of exception display by logging any exception caught here
                // https://extract.atlassian.net/browse/ISSUE-17208
                ex.ExtractLog("ELI50384");
            }
        }

        void HandleControl_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (DroppedDown && !_listBoxChild.Focused)
                {
                    HideTheList();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45352");
            }
        }

        void HandleListBoxChild_GotFocus(object sender, EventArgs e)
        {
            try
            {
                GotFocus?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                // Avoid potential infinite loop of exception display by logging any exception caught here
                // https://extract.atlassian.net/browse/ISSUE-17208
                ex.ExtractLog("ELI50158");
            }
        }

        void HandleListBoxChild_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (_suppressListBoxChildLostFocusEvent)
                {
                    _suppressListBoxChildLostFocusEvent = false;
                    return;
                }

                LostFocus?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50159");
            }
        }


        void HandleControl_DropDown(object sender, EventArgs e)
        {
            try
            {
                _ignoreMouseDown = true;
                DropDown(new Search(null, _dataEntryAutoCompleteControl), true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50160");
            }
            finally
            {
                _ignoreMouseDown = false;
            }
        }

        void HandleControl_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                HideTheList();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50165");
            }
        }

        void HandleControl_TextChanged(EventPattern<object> _)
        {
            try
            {
                _control.SafeBeginInvoke("ELI50397", () =>
                {
                    DropDown(new Search(_control.Text, _dataEntryAutoCompleteControl), false);
                });
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45353");
            }
        }

        void HandleListBoxChild_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender is ListBox)
                {
                    // Copy selection to the control
                    CopySelection();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45354");
            }
        }

        #endregion Event Handlers

        #region Private Methods

        void SetupMessageFilter()
        {
            if (!_msgFilterActive)
            {
               _dataEntryControlHost?.AddMessageFilter(this);
                _msgFilterActive = true;
            }
        }

        void RemoveMessageFilter()
        {
            if (_msgFilterActive)
            {
                _dataEntryControlHost?.RemoveMessageFilter(this);
                _msgFilterActive = false;
            }
        }

        void InitListControl()
        {
            if (_dataEntryAutoCompleteControl.AutoCompleteMode != DataEntryAutoCompleteMode.SuggestLucene)
            {
                _listBoxChild = null;
            }
            else if (_listBoxChild == null)
            {
                // Find most distant ancestor so that the list can extend as far as it needs to
                _ancestorForm = _control.GetAncestors()
                    .OfType<Form>()
                    .FirstOrDefault();

                if (_ancestorForm != null)
                {
                    SetupMessageFilter();

                    _listBoxChild = new ListBox
                    {
                        HorizontalScrollbar = true,
                        BackColor = _listBoxChildBackColor,
                        IntegralHeight = false, // Set so that the height can be adjusted for horizontal scroll bar
                        Height = 13 // Small value so that the list doesn't initially appear larger than its eventual size
                    };
                    _ancestorForm.Controls.Add(_listBoxChild);
                }
            }
        }

        void ShowTheList()
        {
            if (!DroppedDown)
            {
                _listBoxChild.Show();
                _listBoxChild.Click += HandleListBoxChild_Click;
                _listBoxChild.GotFocus += HandleListBoxChild_GotFocus;
                _listBoxChild.LostFocus += HandleListBoxChild_LostFocus;
                DroppedDown = true;
            }
        }

        void HideTheList()
        {
            if (DroppedDown)
            {
                DroppedDown = false;
                _listBoxChild.Click -= HandleListBoxChild_Click;
                _listBoxChild.GotFocus -= HandleListBoxChild_GotFocus;
                _listBoxChild.LostFocus -= HandleListBoxChild_LostFocus;
                _listBoxChild.Hide();
                DropDownClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        // Show suggestions list. Select current item if specified.
        void DropDown(Search search, bool selectCurrentItemIfPossible)
        {
            var forTextChangedEvent = search.Text != null;
            if (forTextChangedEvent)
            {
                if (_ignoreTextChange)
                {
                    _ignoreTextChange = false;
                    return;
                }

                // Allow user to delete the value without triggering the list to show
                string searchText = _control.Text;
                if (searchText != null && searchText.Length == 0 && !DroppedDown)
                {
                    _acceptedText = null;
                    return;
                }
            }

            // TODO: For DataGridView cells the parent is null until some
            // text has been typed so figure out a way to update the control state
            // or arrow keys won't be able to drop down the list
            if (!_control.Visible || !_control.Focused || _control.Parent == null)
            {
                return;
            }

            InitListControl();

            if (_listBoxChild == null)
            {
                return;
            }

            var suggestions = search.ShowEntireList
                ? _autoCompleteValues?.Value
                : Provider?.Invoke(search)?.ToArray();

            _listBoxChild.Items.Clear();
            if (suggestions != null)
            {
                _listBoxChild.Items.AddRange(suggestions);
            }

            // Show the list even if it's empty when the control is a ComboBox because otherwise the button will appear to be broken
            if (_listBoxChild.Items.Count > 0 || _control is LuceneComboBox)
            {
                Point putItHere = _ancestorForm.PointToClient(
                    _control.Parent.PointToScreen(new Point(_control.Left, _control.Bottom)));

                _listBoxChild.Left = putItHere.X;
                _listBoxChild.Top = putItHere.Y;
                _listBoxChild.Width = _control.Width;
                _ancestorForm.Controls.SetChildIndex(_listBoxChild, 0);

                ShowTheList();

                int totalItemHeight = _listBoxChild.PreferredHeight;
                if ((FormsMethods.GetVisibleScrollBars(_listBoxChild) & ScrollBars.Horizontal) != 0)
                {
                    totalItemHeight += SystemInformation.HorizontalScrollBarHeight;
                }
                _listBoxChild.Height = Math.Min(_ancestorForm.ClientSize.Height - _listBoxChild.Top, totalItemHeight);

                // Select the current or best matching item when explicit selection is not required
                if (_listBoxChild.Items.Count > 0
                    && (selectCurrentItemIfPossible || _dataEntryAutoCompleteControl.AutomaticallySelectBestMatchingItem))
                {
                    string currentText = _control.Text ?? "";
                    int idxOfCurrentText = _listBoxChild.Items.IndexOf(currentText);
                    if (idxOfCurrentText < 0 && !selectCurrentItemIfPossible)
                    {
                        idxOfCurrentText = 0;
                    }
                    _listBoxChild.SelectedIndex = idxOfCurrentText;
                }
            }
            else
            {
                HideTheList();
            }
        }

        /// <summary>
        /// Copy the selection from the list-box into the text control
        /// </summary>
        void CopySelection()
        {
            var selectedItem = _listBoxChild.SelectedItem;
            if (selectedItem != null)
            {
                _ignoreTextChange = true;

                if (_control is LuceneComboBox combo)
                {
                    // Setting the Text vs SelectedIndex property of the combo behaves differently wrt auto update queries
                    // Maybe this could be changed but for now just search for the item's index
                    var text = _listBoxChild.SelectedItem.ToString();
                    combo.SelectedIndex = combo.Items.IndexOf(text);
                    combo.SelectAll();
                }
                else if (_control is TextBoxBase textBox)
                {
                    textBox.Text = _listBoxChild.SelectedItem.ToString();
                    textBox.SelectAll();
                }

                var cancelEventArgs = new CancelEventArgs();
                Validating?.Invoke(this, cancelEventArgs);
                if (cancelEventArgs.Cancel)
                {
                    return;
                }
                _acceptedText = _control.Text;

                _suppressListBoxChildLostFocusEvent = true;
                HideTheList();

                // Put focus back on the edit control so that user can type
                try
                {
                    // Set this flag to prevent the list from dropping down again if AutoDropDownMode is Always
                    _ignoreControlGotFocusEvent = true;
                    _control.Focus();
                }
                finally
                {
                    _ignoreControlGotFocusEvent = false;
                }
            }
        }

        internal void SetListBackColor(Color value)
        {
            _listBoxChildBackColor = value;
            if (_listBoxChild != null)
            {
                _listBoxChild.BackColor = value;
            }
        }

        void RevertSelection()
        {
            _control.Text = _acceptedText;
            if (_control is LuceneComboBox combo)
            {
                combo.SelectAll();
            }
            else if (_control is TextBoxBase textBox)
            {
                textBox.SelectAll();
            }
        }


        #endregion Private Methods

        #region IMessageFilter

        public bool PreFilterMessage(ref Message m)
        {
            if (DroppedDown)
            {
                bool clientClick = false;
                switch (m.Msg)
                {
                    // Handle clicking on client areas
                    case WindowsMessage.LeftButtonDown:
                    case WindowsMessage.RightButtonDown:
                    case WindowsMessage.MiddleButtonDown:
                        clientClick = true;
                        goto case WindowsMessage.NonClientLeftButtonDown;

                    // Handle clicking on non-client areas, like scrollbars
                    case WindowsMessage.NonClientLeftButtonDown:
                    case WindowsMessage.NonClientRightButtonDown:
                    case WindowsMessage.NonClientMiddleButtonDown:
                        if (_ancestorForm != null)
                        {
                            var points = m.LParam.ToInt32();
                            var pos = new Point(points & 0xFFFF, points >> 16);

                            // Adjust point to screen coordinates if this was a client area click
                            var ctrl = Control.FromHandle(m.HWnd);
                            if (clientClick && ctrl != null)
                            {
                                pos = ctrl.PointToScreen(pos);
                            }

                            // Adjust point to parent coordinates
                            var posOnAncestor = _ancestorForm.PointToClient(pos);

                            if (_listBoxChild != null &&
                                ( posOnAncestor.X < _listBoxChild.Left || posOnAncestor.X > _listBoxChild.Right
                                || posOnAncestor.Y < _listBoxChild.Top || posOnAncestor.Y > _listBoxChild.Bottom))
                            {
                                HideTheList();

                                var parent = _control.Parent;
                                if (parent != null)
                                {
                                    var posOnParent = parent.PointToClient(pos);

                                    // Consider this click handled if clicking on the text control.
                                    // This allows the combo box button to be used to close the list
                                    if (posOnParent.X > _control.Left && posOnParent.X < _control.Right
                                        && posOnParent.Y > _control.Top && posOnParent.Y < _control.Bottom)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                        break;

                    case WindowsMessage.KeyDown:
                        switch ((Keys)m.WParam)
                        {
                            case Keys.Escape:
                                RevertSelection();
                                HideTheList();
                                return true;

                            case Keys.Up:
                            case Keys.Down:
                                // Change selection
                                int NewIx = _listBoxChild.SelectedIndex + ((Keys)m.WParam == Keys.Up ? -1 : 1);

                                // Keep the index valid!
                                if (NewIx >= 0 && NewIx < _listBoxChild.Items.Count)
                                {
                                    _listBoxChild.SelectedIndex = NewIx;
                                }
                                return true;

                            case Keys.Return:
                                CopySelection();
                                return true; // Prevent data grid view row change

                            case Keys.Tab:
                                CopySelection();
                                return false;
                        }
                        break;

                    case WindowsMessage.MouseWheel:
                        if (_listBoxChild != null)
                        {
                            if (_listBoxChild.ClientRectangle.Contains(
                                _listBoxChild.PointToClient(Control.MousePosition)))
                            {
                                WindowsMessage.SendMessage(_listBoxChild.Handle, m.Msg, m.WParam, m.LParam);
                                return true;
                            }
                            else
                            {
                                HideTheList();
                            }
                        }
                        break;
                }
            }

            return false;
        }

        #endregion IMessageFilter

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (ProviderSource != null && ProviderSource.IsValueCreated)
                    {
                        ProviderSource.Value?.Dispose();
                    }

                    _unregister?.Invoke();
                    RemoveMessageFilter();
                }

                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        internal void UpdateAutoCompleteList(IEnumerable<KeyValuePair<string, List<string>>> autoCompleteValues)
        {
            // Avoid using the supplied IEnumerable at a later time in case it is being changed on a different thread
            // https://extract.atlassian.net/browse/ISSUE-17243
            var safeAutoCompleteValues = autoCompleteValues?.ToList();

            // Save entire list for quick display (no need to wait for index to be created)
            // Always create a new lazy instance to avoid a bad index exception
            // https://extract.atlassian.net/browse/ISSUE-17243
            _autoCompleteValues = new Lazy<object[]>(() =>
            {
                var objectArray = new object[safeAutoCompleteValues?.Count ?? 0];
                for (int i = 0; i < objectArray.Length; i++)
                {
                    objectArray[i] = safeAutoCompleteValues[i].Key;
                }
                return objectArray;
            }, System.Threading.LazyThreadSafetyMode.PublicationOnly);

            ProviderSource = new Lazy<LuceneSuggestionProvider<KeyValuePair<string, List<string>>>>(
                () => new LuceneSuggestionProvider<KeyValuePair<string, List<string>>>(
                    safeAutoCompleteValues,
                    s => s.Key,
                    s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s.Key), 1)
                        .Concat(s.Value.Select(aka => new KeyValuePair<string, string>("AKA", aka)))),
                System.Threading.LazyThreadSafetyMode.PublicationOnly);
        }
        #endregion

        #region Private Classes

        class Search
        {
            public string Text { get; }
            public bool ShowEntireList { get; }
            public int? MaxSuggestions { get; }
            public bool ExcludeLowScoring { get; }

            public Search(string text, IDataEntryAutoCompleteControl control)
            {
                Text = text;
                ShowEntireList = string.IsNullOrWhiteSpace(text);
                MaxSuggestions = control.LimitNumberOfSuggestions;
                ExcludeLowScoring = !control.ShowLowScoringSuggestions;
            }
        }

        #endregion
    }
}