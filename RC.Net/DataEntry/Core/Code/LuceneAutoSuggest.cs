using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.DataEntry
{
    class LuceneAutoSuggest: IMessageFilter, IDisposable
    {
        #region Fields

        Form _ancestorForm;
        Control _control;
        IDataEntryAutoCompleteControl _dataEntryControl;
        ListBox _listBoxChild;
        bool _msgFilterActive = false;
        Lazy<LuceneSuggestionProvider<KeyValuePair<string, List<string>>>> _providerSource;
        Action _unregister;
        int _ignoreTextChange;

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
                    Provider = s => _providerSource.Value.GetSuggestions(s,
                        excludeLowScoring: true);

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
        private Func<string, IEnumerable<object>> Provider { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Private constructor to handle common ctor work
        /// </summary>
        /// <param name="control">The editing control: Either a ComboBox or a TextBoxBase</param>
        /// <param name="dataEntryControl">The <see cref="IDataEntryAutoCompleteControl"/> used
        /// to determine whether to react to text changes with suggestions or not</param>
        private LuceneAutoSuggest(Control control, IDataEntryAutoCompleteControl dataEntryControl)
        {
            try
            {
                ExtractException.Assert("ELI45347", "Unknown type of control",
                    control is ComboBox || control is TextBoxBase);

                _control = control;
                _dataEntryControl = dataEntryControl;

                // Set up all the events we need to handle
                control.TextChanged += HandleControl_TextChanged;
                control.LostFocus += HandleControl_LostFocus;
                control.MouseDown += HandleControl_MouseDown;
                control.HandleDestroyed += HandleControl_HandleDestroyed;

                // Set up event unregistration
                _unregister = () =>
                {
                    control.TextChanged -= HandleControl_TextChanged;
                    control.LostFocus -= HandleControl_LostFocus;
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
        /// Creates a suggestion drop-down for a <see cref="DataEntryTextBox"/>
        /// </summary>
        /// <param name="control">The control for which to display the suggestions</param>
        public LuceneAutoSuggest(DataEntryTextBox control)
            : this(control, control)
        { }

        /// <summary>
        /// Creates a suggestion drop-down for a <see cref="DataEntryComboBox"/>
        /// </summary>
        /// <param name="control">The control for which to display the suggestions</param>
        public LuceneAutoSuggest(DataEntryComboBox control)
            : this(control, control)
        {
            try
            {
                // Set up additional events we need to handle
                control.SelectionChangeCommitted += Handle_SelectionChangeCommitted;

                // Set up event unregistration
                _unregister += () =>
                {
                    control.SelectionChangeCommitted -= Handle_SelectionChangeCommitted;
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45349");
            }
        }

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

        #region Event Handlers

        void Handle_SelectionChangeCommitted(object sender, EventArgs e)
        {
            _ignoreTextChange++;
        }

        void HandleControl_HandleDestroyed(object sender, EventArgs e)
        {
            try
            {
                if (_msgFilterActive)
                {
                    if (_dataEntryControl is IDataEntryControl dec)
                    {
                        dec?.DataEntryControlHost?.RemoveMessageFilter(this);
                    }
                }
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
                HideTheList();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45351");
            }
        }

        void HandleControl_LostFocus(object sender, EventArgs e)
        {
            try
            {
                if (_listBoxChild != null && !_listBoxChild.Focused)
                {
                    HideTheList();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45352");
            }
        }

        void HandleControl_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_ignoreTextChange > 0)
                {
                    _ignoreTextChange = 0;
                    return;
                }

                if (!_control.Visible
                    || !_control.Focused)
                {
                    return;
                }

                InitListControl();

                if (_listBoxChild == null)
                {
                    return;
                }

                string searchText = _control.Text;

                _listBoxChild.Items.Clear();

                if (!string.IsNullOrEmpty(searchText))
                {
                    var suggestions = Provider?.Invoke(searchText)?.ToArray();
                    if (suggestions != null)
                    {
                        _listBoxChild.Items.AddRange(suggestions);
                    }
                }

                if (_listBoxChild.Items.Count > 0)
                {
                    Point putItHere = _ancestorForm.PointToClient(
                        _control.Parent.PointToScreen(new Point(_control.Left, _control.Bottom)));

                    _listBoxChild.Left = putItHere.X;
                    _listBoxChild.Top = putItHere.Y;
                    _listBoxChild.Width = _control.Width;
                    _ancestorForm.Controls.SetChildIndex(_listBoxChild, 0);
                    _listBoxChild.Show();

                    int totalItemHeight = _listBoxChild.ItemHeight * (_listBoxChild.Items.Count + 1);
                    if ((FormsMethods.GetVisibleScrollbars(_listBoxChild) & ScrollBars.Horizontal) != 0)
                    {
                        totalItemHeight += SystemInformation.HorizontalScrollBarHeight;
                    }
                    _listBoxChild.Height = Math.Min(_ancestorForm.ClientSize.Height - _listBoxChild.Top, totalItemHeight);

                    // Select first item
                    _listBoxChild.SelectedIndex = 0;
                }
                else
                {
                    HideTheList();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45353");
            }
        }


        private void HandleListBoxChild_Click(object sender, EventArgs e)
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

        void InitListControl()
        {
            if (_dataEntryControl?.AutoCompleteMode
                != DataEntryAutoCompleteMode.SuggestLucene)
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
                    // Setup a message filter so we can listen to the keyboard/mouse
                    if (!_msgFilterActive)
                    {
                        if (_dataEntryControl is IDataEntryControl dec)
                        {
                            dec?.DataEntryControlHost?.AddMessageFilter(this);
                        }

                        _msgFilterActive = true;
                    }

                    _listBoxChild = new ListBox
                    {
                        HorizontalScrollbar = true
                    };
                    _listBoxChild.Click += HandleListBoxChild_Click;
                    _ancestorForm.Controls.Add(_listBoxChild);
                }
            }
        }

        private void HideTheList()
        {
            _listBoxChild?.Hide();
        }

        /// <summary>
        /// Copy the selection from the list-box into the text control
        /// </summary>
        private void CopySelection()
        {
            var selectedItem = _listBoxChild.SelectedItem;
            if (selectedItem != null)
            {
                if (_control is ComboBox combo)
                {
                    combo.SelectedItem = selectedItem;
                    combo.SelectAll();
                }
                else if (_control is TextBoxBase textBox)
                {
                    textBox.Text = _listBoxChild.SelectedItem.ToString();
                    textBox.SelectAll();
                }
                HideTheList();
            }
        }

        #endregion Private Methods

        #region IMessageFilter

        public bool PreFilterMessage(ref Message m)
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
                        pos = _ancestorForm.PointToClient(pos);

                        if (_listBoxChild != null && (pos.X < _listBoxChild.Left || pos.X > _listBoxChild.Right
                            || pos.Y < _listBoxChild.Top || pos.Y > _listBoxChild.Bottom))
                        {
                            HideTheList();
                        }
                    }
                    break;

                case WindowsMessage.KeyDown:
                    if (_listBoxChild != null && _listBoxChild.Visible)
                    {
                        switch ((Keys)m.WParam)
                        {
                            case Keys.Escape:
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
                                return false;

                            case Keys.Tab:
                                CopySelection();
                                return false;
                        }
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

            return false;
        }

        #endregion IMessageFilter

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (ProviderSource != null && ProviderSource.IsValueCreated)
                    {
                        ProviderSource.Value?.Dispose();
                    }

                    _unregister?.Invoke();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        internal void UpdateAutoCompleteList(Dictionary<string, List<string>> autoCompleteValues)
        {
            ProviderSource = new Lazy<LuceneSuggestionProvider<KeyValuePair<string, List<string>>>>(
                () => new LuceneSuggestionProvider<KeyValuePair<string, List<string>>>(
                    autoCompleteValues.AsEnumerable(),
                    s => s.Key,
                    s => Enumerable.Repeat(new KeyValuePair<string, string>("Name", s.Key), 1)
                        .Concat(s.Value.Select(aka => new KeyValuePair<string, string>("AKA", aka)))));
        }
        #endregion
    }
}