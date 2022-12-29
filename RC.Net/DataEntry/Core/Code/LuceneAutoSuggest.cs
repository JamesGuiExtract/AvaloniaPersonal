using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Extract.DataEntry
{
    class LuceneAutoSuggest: IMessageFilter, IDisposable
    {
        #region Fields

        Form _ancestorForm;
        readonly Control _control;
        readonly IDataEntryAutoCompleteControl _dataEntryAutoCompleteControl;
        ElementHost _listBoxHost;
        ListBoxChild _listBoxChild;
        bool _msgFilterActive = false;
        Lazy<LuceneSuggestionProvider<KeyValuePair<string, List<string>>>> _providerSource;

        // Lambda to unregister from events registered in the ctor
        readonly Action _unregister;

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
        System.Windows.Media.SolidColorBrush _listBoxChildBackground;

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
                    Provider = search =>
                    {
                        try
                        {
                            return value.Value.GetSuggestions(search.Text,
                                maybeMaxSuggestions: search.MaxSuggestions,
                                excludeLowScoring: search.ExcludeLowScoring)
                            .ToArray(); // Materialize the enumerable here to make sure any exceptions are handled appropriately
                        }
                        catch (Exception) when (value.Value.IsDisposed)
                        {
                            // Ignore exception caused by the provider getting disposed of while searching
                            return new object[0];
                        }
                    };

                    if (old != null && old.IsValueCreated)
                    {
                        old.Value.Dispose();
                    }

                    // Start lazy instantiation to reduce delay when user starts to type
                    // Use of lazy objects helps prevent the UI from opening sluggishly
                    //  https://extract.atlassian.net/browse/ISSUE-15673
                    //
                    // Don't start this process if the UI isn't visible because in pagination there are many DEP copies run
                    // that no user will interact with
                    if (_control.GetAncestors().LastOrDefault() is not InvisibleForm)
                    {
                        Task.Factory.StartNew(() => value.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Function that returns suggestions based on a search string
        /// </summary>
        private Func<Search, object[]> Provider { get; set; }

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
                SetListBackColor(control.BackColor);
                _dataEntryAutoCompleteControl = dataEntryAutoCompleteControl;

                // Set up event handlers
                var textChangedHandler = BuildTextChangedEventObserver().Subscribe();
                control.LostFocus += HandleControl_LostFocus;
                control.GotFocus += HandleControl_GotFocus;
                control.MouseDown += HandleControl_MouseDown;
                control.HandleDestroyed += HandleControl_HandleDestroyed;

                // Set up event handler unregistration
                _unregister = () =>
                {
                    textChangedHandler.Dispose();
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

        IObservable<Unit> BuildTextChangedEventObserver()
        {
            return Observable.FromEventPattern(h => _control.TextChanged += h, h => _control.TextChanged -= h)
                .Where(_ => IsDropDownRequired(true))
                .Select(_ => new Search(_control.Text, _dataEntryAutoCompleteControl))
                // Ignore very rapidly typed keys until the burst is over
                // This will cause the transforms that follow to run on the threadpool
                .Throttle(TimeSpan.FromMilliseconds(200), Scheduler.Default)
                .Select(search => Observable.FromAsync(async () => await GetSuggestions(search)))
                .Switch() // Keep only the latest result
                .Select(suggestions =>
                {
                    // Current thread is from the thread pool so need to invoke on the UI thread
                    _control.SafeBeginInvoke("ELI50397", () => DropDown(suggestions, false));
                    return Unit.Default;
                })
                .Catch<Unit, Exception>(ex =>
                {
                    ex.ExtractDisplay("ELI51407");
                    return BuildTextChangedEventObserver(); // Resubscribe to event after error
                });
        }

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
        async void HandleControl_GotFocus(object sender, EventArgs e)
        {
            try
            {
                if (_ignoreControlGotFocusEvent)
                {
                    return;
                }

                var dropDownOnFocus = _dataEntryAutoCompleteControl.AutoDropDownMode == AutoDropDownMode.Always
                    || _dataEntryAutoCompleteControl.AutoDropDownMode == AutoDropDownMode.WhenEmpty
                        && string.IsNullOrWhiteSpace(_control.Text);

                if (dropDownOnFocus && IsDropDownRequired())
                {
                    var suggestions = await GetSuggestions(new Search(null, _dataEntryAutoCompleteControl));
                    // Mouse clicks fail to auto-drop the list unless this is begin-invoked, I think because
                    // something the DataEntryControlHost does causes the list to close immediately
                    if (_control.IsHandleCreated) // In case the form is being closed
                    {
                        _control.SafeBeginInvoke("ELI50388", () => DropDown(suggestions, true), displayExceptions: false);
                    }
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
                if (DroppedDown && !_listBoxHost.ContainsFocus)
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


        async void HandleControl_DropDown(object sender, EventArgs e)
        {
            try
            {
                _ignoreMouseDown = true;

                if (IsDropDownRequired())
                {
                    var suggestions = await GetSuggestions(new Search(null, _dataEntryAutoCompleteControl));
                    DropDown(suggestions, selectCurrentItemIfPossible: true);
                }
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

        void HandleListBoxChild_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender is System.Windows.Controls.ListBox)
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
                _listBoxHost = null;
            }
            else if (_listBoxHost == null)
            {
                // Find most distant ancestor so that the list can extend as far as it needs to
                _ancestorForm = _control.GetAncestors()
                    .OfType<Form>()
                    .FirstOrDefault();

                if (_ancestorForm != null)
                {
                    SetupMessageFilter();

                    _listBoxHost = new ElementHost
                    {
                        Margin = Padding.Empty
                    };
                    _listBoxChild = new ListBoxChild
                    {
                        Background = _listBoxChildBackground
                    };
                    _listBoxHost.Child = _listBoxChild;
                    _ancestorForm.Controls.Add(_listBoxHost);
                }
            }
        }

        void ShowTheList()
        {
            if (!DroppedDown)
            {
                _listBoxHost.Show();
                _listBoxChild.MouseUp += HandleListBoxChild_Click;
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
                _listBoxChild.MouseUp -= HandleListBoxChild_Click;
                _listBoxChild.GotFocus -= HandleListBoxChild_GotFocus;
                _listBoxChild.LostFocus -= HandleListBoxChild_LostFocus;
                _listBoxHost.Hide();
                DropDownClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        bool IsDropDownRequired(bool forTextChangedEvent = false)
        {
            if (forTextChangedEvent)
            {
                if (_ignoreTextChange)
                {
                    _ignoreTextChange = false;
                    return false;
                }

                // Allow user to delete the value without triggering the list to show
                string searchText = _control.Text;
                if (searchText != null && searchText.Length == 0 && !DroppedDown)
                {
                    _acceptedText = null;
                    return false;
                }
            }

            // TODO: For DataGridView cells the parent is null until some
            // text has been typed so figure out a way to update the control state
            // or arrow keys won't be able to drop down the list
            if (!_control.Visible || !_control.Focused || _control.Parent == null)
            {
                return false;
            }

            return true;
        }

        async Task<object[]> GetSuggestions(Search search)
        {
            var suggestions = await Task.Run(() => search.ShowEntireList
                ? _autoCompleteValues?.Value
                : Provider?.Invoke(search));

            return suggestions;
        }

        // Show suggestions from list using lucene query or show entire list if searchText is null
        void DropDown(object[] suggestions, bool selectCurrentItemIfPossible)
        {
            // Double-check that the control is still enabled
            if (!_control.Visible || !_control.Focused || _control.Parent == null)
            {
                return;
            }

            InitListControl();

            if (_listBoxChild == null)
            {
                return;
            }

            _listBoxChild.SelectedIndex = -1;
            _listBoxChild.ItemsSource = suggestions;

            // Show the list even if it's empty when the control is a ComboBox because otherwise the button will appear to be broken
            if (_listBoxChild.Items.Count > 0 || _control is LuceneComboBox)
            {
                Point putItHere = _ancestorForm.PointToClient(
                    _control.Parent.PointToScreen(new Point(_control.Left, _control.Bottom)));

                _listBoxHost.Left = putItHere.X;
                _listBoxHost.Top = putItHere.Y;
                _listBoxHost.Width = _control.Width;
                _listBoxHost.Height = _ancestorForm.ClientSize.Height - _listBoxHost.Top;
                _ancestorForm.Controls.SetChildIndex(_listBoxHost, 0);

                _listBoxChild.Visibility = System.Windows.Visibility.Hidden;

                ShowTheList();

                _listBoxHost.Height = (int)Math.Ceiling(_listBoxChild.DesiredSize.Height);

                _listBoxChild.Visibility = System.Windows.Visibility.Visible;

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

                    SelectListItem(idxOfCurrentText);
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
            _listBoxChildBackground = new(System.Windows.Media.Color.FromArgb(value.A, value.R, value.G, value.B));

            if (_listBoxChild != null)
            {
                _listBoxChild.Background = _listBoxChildBackground;
            }
        }

        void RevertSelection()
        {
            // If there is no difference in the current text and the accepted text then reverting won't cause
            // a text change event that needs ignoring
            _ignoreTextChange = _control.Text != _acceptedText;
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

        private void SelectListItem(int idx)
        {
            if (idx >= 0 && idx < _listBoxChild.Items.Count)
            {
                _listBoxChild.SelectedIndex = idx;
                _listBoxChild.ScrollIntoView(_listBoxChild.Items.GetItemAt(idx));
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
                            if (clientClick && Control.FromHandle(m.HWnd) is Control ctrl)
                            {
                                pos = ctrl.PointToScreen(pos);
                            }
                            else if (clientClick && m.HWnd == ((System.Windows.Interop.HwndSource)System.Windows.PresentationSource.FromVisual(_listBoxChild)).Handle)
                            {
                                var point = new System.Windows.Point(pos.X, pos.Y);
                                point = _listBoxChild.PointToScreen(point);
                                pos = new Point((int)point.X, (int)point.Y);
                            }

                            // Adjust point to parent coordinates
                            var posOnAncestor = _ancestorForm.PointToClient(pos);

                            if (_listBoxHost != null &&
                                ( posOnAncestor.X < _listBoxHost.Left || posOnAncestor.X > _listBoxHost.Right
                                || posOnAncestor.Y < _listBoxHost.Top || posOnAncestor.Y > _listBoxHost.Bottom))
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
                                // Send focus back to the edit control so that down arrow works after escape
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
                                return true;

                            case Keys.Up:
                            case Keys.Down:
                                int newIdx = _listBoxChild.SelectedIndex + ((Keys)m.WParam == Keys.Up ? -1 : 1);
                                SelectListItem(newIdx);
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
                        if (_listBoxHost != null && _control != null)
                        {
                            if (_control.ClientRectangle.Contains(
                                    _control.PointToClient(Control.MousePosition)))
                            {
                                return false;
                            }
                            else if (!_listBoxHost.ClientRectangle.Contains(
                                    _listBoxHost.PointToClient(Control.MousePosition)))
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

                    _listBoxHost?.Dispose();
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