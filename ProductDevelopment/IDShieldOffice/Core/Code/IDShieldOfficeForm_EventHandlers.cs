using Extract;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Rules;
using Extract.Utilities.Forms;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Security.Permissions;
using System.Windows.Forms;
using TD.SandDock;

namespace IDShieldOffice
{
    partial class IDShieldOfficeForm
    {
        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Set shortcut keys
                _imageViewer.Shortcuts[Keys.H] = _imageViewer.ToggleRedactionTool;
                _imageViewer.Shortcuts[Keys.F12] = ShowPreferencesDialog;
                _imageViewer.Shortcuts[Keys.F | Keys.Control] = ShowWordOrPatternListRuleForm;
                _imageViewer.Shortcuts[Keys.D | Keys.Control] = ShowDataTypeRuleForm;
                _imageViewer.Shortcuts[Keys.K | Keys.Control] = ShowBracketedTextFinderWindow;
                _imageViewer.Shortcuts[Keys.S | Keys.Control] = SelectSave;
                _imageViewer.Shortcuts[Keys.B | Keys.Control] = ApplyBatesNumbers;
                _imageViewer.Shortcuts[Keys.F10] = ToggleObjectPropertiesGridDockableWindow;
                _imageViewer.Shortcuts[Keys.L | Keys.Control] = ToggleLayersDockableWindow;
                _imageViewer.Shortcuts[Keys.F1] = ShowIdsoHelp;

                // Establish connections with image viewer components
                _imageViewer.EstablishConnections(this);

                // Make sure the dockable panes begin closed-- otherwise ID Shield Office may launch such
                // that the pane visibility does not match the assosiated controls' states.
                _layersDockableWindow.Close();
                _objectPropertyGridDockableWindow.Close();

                // Check if the form should be reset [IDSD #235 - JDS]
                if (!ResetForm)
                {
                    // Load the saved user toolstrip settings
                    // NOTE: If you are noticing strange toolstrip behavior, try commenting out
                    // this line and rerunning the application
                    ToolStripManager.LoadSettings(this);

                    // Show the dockable windows as per registry settings
                    SetObjectPropertiesGridDockableWindowVisible(RegistryManager.PropertyGridVisible);
                    SetLayersDockableWindowVisible(RegistryManager.ShowLayerObjectsVisible);

                    // [IDSD:196] Open the form with the position and size set per the registry settings.
                    // Do this regardless of whether the window will be maximized so that it will restore
                    // to the size used the last time the window was in the "normal" state.
                    DesktopBounds = new Rectangle(
                        new Point(RegistryManager.DefaultWindowPositionX,
                                  RegistryManager.DefaultWindowPositionY),
                        new Size(RegistryManager.DefaultWindowWidth,
                                 RegistryManager.DefaultWindowHeight));

                    if (RegistryManager.DefaultWindowMaximized)
                    {
                        // [IDSD:196] Maximize the window if the registry setting indicates ID Shield 
                        // Office should launch maximized.
                        WindowState = FormWindowState.Maximized;
                    }
                }
                else
                {
                    // Reset the stored form position and maximized state
                    RegistryManager.DefaultWindowMaximized = false;
                    RegistryManager.DefaultWindowPositionX = DesktopBounds.Left;
                    RegistryManager.DefaultWindowPositionY = DesktopBounds.Top;
                    RegistryManager.DefaultWindowWidth = DesktopBounds.Width;
                    RegistryManager.DefaultWindowHeight = DesktopBounds.Height;

                    // Make sure the dockable windows are hidden
                    SetObjectPropertiesGridDockableWindowVisible(false);
                    SetLayersDockableWindowVisible(false);
                }

                // Indicate the the form has been loaded.
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21978", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.Shown"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnShown(EventArgs e)
        {
            try
            {
                base.OnShown(e);

                // Open the specified image
                if (!string.IsNullOrEmpty(_fileToOpen))
                {
                    // Open the specified image, do not update the MRU list if this is a temp file
                    _imageViewer.OpenImage(_fileToOpen, !_tempFile);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23260", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// <see langword="false"/> if the character was not processed.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                // Don't process shortcut keys if text-based controls are in focus
                if (!_pageNavigationToolStripTextBox.Focused && 
                    !_objectPropertyGrid.ContainsFocus)
                {
                    // Allow the image viewer to handle this keyboard shortcut.
                    if (_imageViewer.Shortcuts.ProcessKey(keyData))
                    {
                        return true;
                    }
                }

                // This key was not processed, bubble it up to the base class.
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22602", ex);
                return false;
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.AsynchronousOcrManager.OcrProgressUpdate"/>
        /// event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.AsynchronousOcrManager.OcrProgressUpdate"/>
        /// event.</param>
        /// <param name="e">An <see cref="OcrProgressUpdateEventArgs"/> that contains
        /// the event data.</param>
        void HandleOcrProgressUpdate(object sender, OcrProgressUpdateEventArgs e)
        {
            // Update the OCR progress status label
            UpdateProgressStatus(e.ProgressPercent);
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.AsynchronousOcrManager.OcrError"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.AsynchronousOcrManager.OcrError"/>
        /// event.</param>
        /// <param name="e">An <see cref="OcrErrorEventArgs"/> that contains the event data.</param>
        void HandleOcrError(object sender, OcrErrorEventArgs e)
        {
            // Store the exception.  Do this before updating the progress status
            _lastOcrException = e.Exception;

            // Update the OCR progress status label to show error
            UpdateProgressStatus(_OCR_DISPLAY_PROGRESS_ERROR);
        }

        /// <summary>
        /// Raises the <see cref="Form.Closing"/> event.
        /// </summary>
        /// <param name="e">An <see cref="CancelEventArgs"/> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (Dirty)
                {
                    // Prompt the user if the file is dirty
                    DialogResult result = PromptForDirtyFile();
                    if (result == DialogResult.Yes)
                    {
                        // If user canceled the save, cancel the closing event
                        if (!Save(false))
                        {
                            // Cancel the event
                            e.Cancel = true;
                        }
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        // Cancel the event
                        e.Cancel = true;
                    }
                }

                // Check if the event was canceled
                if (!e.Cancel)
                {
                    // Ensure the OCR Manager is cleaned up as the Form is closing
                    // NOTE: This should be done before the form is closed so that any
                    // running OCR thread does not try to update the UI after the UI is closed.
                    if (_ocrManager != null)
                    {
                        _ocrManager.OcrProgressUpdate -= HandleOcrProgressUpdate;
                        _ocrManager.OcrError -= HandleOcrError;

                        _ocrManager.Dispose();
                        _ocrManager = null;
                    }

                    // Save the toolstrip settings
                    ToolStripManager.SaveSettings(this);
                }

                // Call the base class
                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21980", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.OnMove"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnMove(EventArgs e)
        {
            try
            {
                base.OnMove(e);

                if (_isLoaded && WindowState == FormWindowState.Normal)
                {
                    // If the user moved the form, store the new position.
                    RegistryManager.DefaultWindowPositionX = DesktopLocation.X;
                    RegistryManager.DefaultWindowPositionY = DesktopLocation.Y;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23228", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.OnResize"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            try
            {
                base.OnResize(e);

                if (_isLoaded && WindowState != FormWindowState.Minimized)
                {
                    if (WindowState == FormWindowState.Maximized)
                    {
                        // If the user maximized the form, set the form to default to maximized,
                        // but don't adjust the default form size to use in normal mode.
                        RegistryManager.DefaultWindowMaximized = true;
                    }
                    else if (WindowState == FormWindowState.Normal)
                    {
                        // If the user restored or moved the form in normal mode, store
                        // the new size as the default size.
                        RegistryManager.DefaultWindowMaximized = false;
                        RegistryManager.DefaultWindowWidth = Size.Width;
                        RegistryManager.DefaultWindowHeight = Size.Height;
                    }

                    // If there is an image open in the image viewer then restore the previous
                    // scroll position - [DNRCAU #262 - JDS]
                    if (_imageViewer.IsImageAvailable)
                    {
                        _imageViewer.RestoreScrollPosition();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23229", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles redacting the entire page
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleRedactEntirePageMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                if (_imageViewer != null && _imageViewer.IsImageAvailable)
                {
                    // Get a rectangle that contains the page
                    Rectangle pageRectangle = new Rectangle(0, 0, _imageViewer.ImageWidth,
                        _imageViewer.ImageHeight);

                    // Add the redaction (this will check for overlap and will not add
                    // the redaction if it overlaps an existing one).
                    AddRedaction(new RasterZone[] {
                        new RasterZone(pageRectangle, _imageViewer.PageNumber) }, 
                        LayerObject.ManualComment);

                    // Invalidate the image viewer so it will refresh
                    _imageViewer.Invalidate();
                }
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI22325", ex).Display();
            }
        }

        /// <summary>
        /// Handles showing of the bracketed text rule form.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleShowBracketedTextFinderWindow(object sender, EventArgs e)
        {
            try
            {
                ShowBracketedTextFinderWindow();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22062", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Displays the bracketed text rule form.
        /// </summary>
        void ShowBracketedTextFinderWindow()
        {
            // Create the form if it has not been created yet
            if (_bracketedTextRuleForm == null)
            {
                _bracketedTextRuleForm = CreateRuleForm("Bracketed Text", 
                    new BracketedTextRule());
            }

            // Show the bracketed text rule form.
            _bracketedTextRuleForm.Show();

            // Ensure that form is not minimized
            _bracketedTextRuleForm.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// Handles the showing of the word or pattern list rule form.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleShowWordOrPatternListRuleForm(object sender, EventArgs e)
        {
            try
            {
                ShowWordOrPatternListRuleForm();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22099", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Displays the word or pattern list rule form.
        /// </summary>
        void ShowWordOrPatternListRuleForm()
        {
            if (_wordOrPatternListRuleForm == null)
            {
                _wordOrPatternListRuleForm = CreateRuleForm("Words/patterns", 
                    new WordOrPatternListRule());
            }

            // Show the word or pattern list rule form
            _wordOrPatternListRuleForm.Show();

            // Ensure that form is not minimized
            _wordOrPatternListRuleForm.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// Handles showing of the data type rule form.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleShowDataTypeRuleForm(object sender, EventArgs e)
        {
            try
            {
                ShowDataTypeRuleForm();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22108", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Displays the data type rule form.
        /// </summary>
        void ShowDataTypeRuleForm()
        {
            if (_dataTypeRuleForm == null)
            {
                _dataTypeRuleForm = CreateRuleForm("Data types",
                    new DataTypeRule());
            }

            // Show the data type rule form
            _dataTypeRuleForm.Show();

            // Ensure that form is not minimized
            _dataTypeRuleForm.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// Creates an <see cref="RuleForm"/> with the specified title and rule.
        /// </summary>
        /// <param name="title">The text to display on the rule form's title bar.</param>
        /// <param name="rule">The rule that the form executes.</param>
        /// <returns>An <see cref="RuleForm"/> with the specified 
        /// <paramref name="title"/> and <paramref name="rule"/>.</returns>
        RuleForm CreateRuleForm(string title, IRule rule)
        {
            RuleForm form = new RuleForm(title, rule, _imageViewer, this, this);
            form.ShowClues = CluesVisible;
            form.MatchesFound += HandleMatchesFound;
            form.MatchRedacted += HandleMatchRedacted;

            return form;
        }

        /// <summary>
        /// Handles the <see cref="RuleForm.MatchesFound"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="RuleForm.MatchesFound"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="RuleForm.MatchesFound"/> event.</param>
        void HandleMatchesFound(object sender, MatchesFoundEventArgs e)
        {
            try
            {
                AddClues(e.Matches);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29195", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="RuleForm.MatchRedacted"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="RuleForm.MatchRedacted"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="RuleForm.MatchRedacted"/> event.</param>
        void HandleMatchRedacted(object sender, MatchRedactedEventArgs e)
        {
            try
            {
                AddRedaction(e.Match.RasterZones, e.Match.FindingRule);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29189", ex);
            }
        }

        /// <summary>
        /// Handles showing or hiding of the Properties grid window.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleShowObjectPropertyGridWindow(object sender, EventArgs e)
        {
            try
            {
                ToggleObjectPropertiesGridDockableWindow();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21981", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.AddDebugData("Sender", sender != null ? sender.ToString() : "Null", true);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles toggling of the show bates numbers checkbox.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleShowBatesNumbersCheckboxChanged(object sender, EventArgs e)
        {
            try
            {
                // Set the visible flag
                _imageViewer.SetVisibleStateForSpecifiedLayerObjects(
                    new string[] { BatesNumberManager._BATES_NUMBER_TAG },
                    new Type[] { typeof(TextLayerObject) }, _showBatesNumberCheckBox.Checked);

                // Invalidate the image viewer to update the visibility
                _imageViewer.Invalidate();
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI22245", ex).Display();
            }
        }

        /// <summary>
        /// Handles toggling of the show clues checkbox.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleShowCluesCheckboxChanged(object sender, EventArgs e)
        {
            try
            {
                // Set the visible flag
                bool cluesVisible = _showCluesCheckBox.Checked;
                _imageViewer.SetVisibleStateForSpecifiedLayerObjects(null, 
                    new Type[] { typeof(Clue) }, cluesVisible);

                // Update the rule forms
                if (_bracketedTextRuleForm != null)
                {
                    _bracketedTextRuleForm.ShowClues = cluesVisible;
                }
                if (_wordOrPatternListRuleForm != null)
                {
                    _wordOrPatternListRuleForm.ShowClues = cluesVisible;
                }
                if (_dataTypeRuleForm != null)
                {
                    _dataTypeRuleForm.ShowClues = cluesVisible;
                }

                // Invalidate the image viewer to update the visibility
                _imageViewer.Invalidate();
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI22246", ex).Display();
            }
        }

        /// <summary>
        /// Handles toggling of the show redactions checkbox.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleShowRedactionsCheckboxChanged(object sender, EventArgs e)
        {
            try
            {
                // Set the visible flag
                _imageViewer.SetVisibleStateForSpecifiedLayerObjects(null,
                    new Type[] { typeof(Redaction) }, _showRedactionsCheckBox.Checked);

                // Invalidate the image viewer to update the visibility
                _imageViewer.Invalidate();
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI22247", ex).Display();
            }
        }


        /// <summary>
        /// Handles checking/unchecking all of the layer objects to show check boxes.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleCheckOrUncheckAllLayerObjects(object sender, EventArgs e)
        {
            try
            {
                // Check whether the check all or uncheck all button sent the event
                bool check = sender.Equals(_checkAllLayersToolStripButton);

                // Get each of the check boxes in the control
                foreach (Control control in _layersViewPanel.Controls)
                {
                    // Ensure the control is a check box
                    CheckBox checkBox = control as CheckBox;
                    if (checkBox != null)
                    {
                        // Set the appropriate check state
                        checkBox.Checked = check;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI22248", ex).Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Form.Closing"/> event for the Properties grid window.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="DockControlClosingEventArgs"/> that
        /// contains the event data.</param>
        void objectPropertyGridDockableWindow_Closing(object sender,
            DockControlClosingEventArgs e)
        {
            try
            {
                // Call SetObjectPropertiesGridDockableWindowVisible so that the controls & registry setting
                // update along with the pane visibility.
                SetObjectPropertiesGridDockableWindowVisible(false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21986", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles showing or hiding of the Layers window.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleShowLayersWindow(object sender, EventArgs e)
        {
            try
            {
                ToggleLayersDockableWindow();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21983", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Form.Closing"/> event for the Layers window.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="DockControlClosingEventArgs"/> that
        /// contains the event data.</param>
        void layersDockableWindow_Closing(object sender, DockControlClosingEventArgs e)
        {
            try
            {
                // Call ShowLayersDockableWindow so that the controls & registry setting
                // update along with the pane visibility.
                SetLayersDockableWindowVisible(false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22038", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event for the
        /// exitToolStripMenuItem.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Close the form 
                Close();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21985", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DoubleClick"/>
        /// </summary>
        /// <param name="sender">The object which sent the <see cref="Control.DoubleClick"/>
        /// event.</param>
        /// <param name="e">The <see cref="EventArgs"/> data associated with the
        /// <see cref="Control.DoubleClick"/> event.</param>
        void ocrProgressStatusLabel_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (_ocrProgressStatusLabel.Text == _OCR_PROGRESS_ERROR
                        && _lastOcrException != null)
                {
                    _lastOcrException.Display();
                }
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI22320", ex).Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event of the preference menu item.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="ToolStripItem.Click"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="ToolStripItem.Click"/> 
        /// event.</param>
        void HandlePreferencesToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowPreferencesDialog();
        }

        /// <summary>
        /// Displays a dialog that allows the user to specify preferences for the application.
        /// </summary>
        void ShowPreferencesDialog()
        {
            try
            {
                // Create the preferences dialog if not already created
                if (_userPreferencesDialog == null)
                {
                    _userPreferencesDialog = new PropertyPageForm("User Preferences",
                        (IPropertyPage)_userPreferences.PropertyPage);
                    ComponentResourceManager resources =
                        new ComponentResourceManager(typeof(IDShieldOfficeForm));
                    _userPreferencesDialog.Icon =
                        ((Icon)(resources.GetObject("$this.Icon")));
                }

                // Display the dialog
                DialogResult result = _userPreferencesDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _userPreferences.WriteToRegistry();

                    _ocrManager.Tradeoff = _userPreferences.OcrTradeoff;
                }
                else
                {
                    _userPreferences.ReadFromRegistry();
                }

                // Update the button state
                UpdateButtonAndMenuItemState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22321", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        void HandleImageViewerImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                // If the current file is a temporary file and we are opening a different file
                // then attempt to delete it
                if (_tempFile && !e.FileName.Equals(_fileToOpen, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        // Since this is only valid for the first file _fileToOpen
                        // contains the filename
                        if (File.Exists(_fileToOpen))
                        {
                            File.Delete(_fileToOpen);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Just log any exceptions while attempting to delete the file
                        ExtractException.Log("ELI23259", ex);
                    }

                    // Set the temp flag back to false (it is only valid for the very first image
                    // that was open from the command line)
                    _tempFile = false;
                }

                // Reset the last ocr exception variable
                _lastOcrException = null;

                // Check that there is a new open image (it is possible that
                // this event was triggered by a call to close file)
                if (_imageViewer.IsImageAvailable)
                {
                    // Begin the OCR of the new image file
                    _ocrManager.OcrFile(_imageViewer.ImageFile, 1, _imageViewer.PageCount);

                    // Initialize the pages visited array
                    _pagesVisited = new BitArray(_imageViewer.PageCount);
                }
                else
                {
                    // Cancel the OCR
                    _ocrManager.CancelOcrOperation();
                    UpdateProgressStatus(_OCR_DISPLAY_PROGRESS_NONE);

                    // Clear the pages visited array
                    _pagesVisited = null;
                }

                // Set the dirty flag to false
                Dirty = false;

                // Reset the output file path
                _outputFilePath = null;

                // Update the button and menu item states
                UpdateButtonAndMenuItemState();

                // Update the forms caption
                UpdateCaption();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22322", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileClosing"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/></param> that sent the event.
        /// <param name="e">The event data associated with the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileClosing"/> event.</param>
        void HandleImageViewerImageFileClosing(object sender, ImageFileClosingEventArgs e)
        {
            try
            {
                if (Dirty)
                {
                    // Prompt the user to save the dirty file
                    DialogResult result = PromptForDirtyFile();
                    if (result == DialogResult.Yes)
                    {
                        if (!Save(false))
                        {
                            // Cancel the event
                            e.Cancel = true;
                        }
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        // Cancel the event
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23249", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.CursorToolChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.CursorToolChanged"/> event.</param>
        void HandleImageViewerCursorToolChanged(object sender, CursorToolChangedEventArgs e)
        {
            try
            {
                // If the cursor tool is a redaction tool, ensure the redaction layer
                // is visible to the user
                if (e.CursorTool == CursorTool.AngularRedaction
                    || e.CursorTool == CursorTool.RectangularRedaction)
                {
                    if (!_showRedactionsCheckBox.Checked)
                    {
                        _showRedactionsCheckBox.Checked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22354", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.OpeningImage"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> which sent the event.</param>
        /// <param name="e">An <see cref="OpeningImageEventArgs"/> associated with
        /// the event.</param>
        void HandleImageViewerOpeningImage(object sender, OpeningImageEventArgs e)
        {
            try
            {
                if (Path.GetExtension(e.FileName).Equals(".idso", StringComparison.OrdinalIgnoreCase))
                {
                    // Cancel this file open
                    e.Cancel = true;

                    // Call open image with the image file name
                    _imageViewer.OpenImage(Path.GetDirectoryName(e.FileName)
                        + Path.DirectorySeparatorChar
                        + Path.GetFileNameWithoutExtension(e.FileName), e.UpdateMruList);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23097", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.LoadingNewImage"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> which sent the event.</param>
        /// <param name="e">An <see cref="LoadingNewImageEventArgs"/> associated with
        /// the event.</param>
        void HandleImageViewerLoadingNewImage(object sender, LoadingNewImageEventArgs e)
        {
            try
            {
                // Clear the ocr progress status label
                _ocrProgressStatusLabel.Text = "";

                // Update the caption
                UpdateCaption();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23250", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.PageChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.PageChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.PageChanged"/> event.</param>
        void HandleImageViewerPageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                // Mark that this page has been visited
                _pagesVisited[e.PageNumber - 1] = true;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23306", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.DisplayingPrintDialog"/> 
        /// event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.DisplayingPrintDialog"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.DisplayingPrintDialog"/> event.</param>
        void HandleImageViewerDisplayingPrintDialog(object sender, 
            DisplayingPrintDialogEventArgs e)
        {
            try
            {
                // Prompt the user if necessary before printing
                if (!PromptBeforeOutput())
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23307", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ModificationHistory.ModificationHistoryLoaded"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> which sent the event.</param>
        /// <param name="e">An <see cref="ModificationHistoryLoadedEventArgs"/>
        /// associated with the event.</param>
        void HandleModificationHistoryLoaded(object sender,
            ModificationHistoryLoadedEventArgs e)
        {
            try
            {
                // Set the visibility flags for the layer objects
                SetLayerObjectsVisibility();

                // Reset the dirty flag after the modification history is loaded
                Dirty = false;

                UpdateButtonAndMenuItemState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23172", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Sets the visibility of layer objects after they have been loaded.
        /// </summary>
        void SetLayerObjectsVisibility()
        {
            // Get the visibility states of the layer objects
            bool batesVisible = _showBatesNumberCheckBox.Checked;
            bool cluesVisible = _showCluesCheckBox.Checked;
            bool redactionsVisible = _showRedactionsCheckBox.Checked;
            
            // If all the layer objects are visible, we are done.
            if (batesVisible && cluesVisible && redactionsVisible)
            {
                return;
            }

            // Turn off the visibility of any layer objects that aren't visible
            foreach (LayerObject layerObject in _imageViewer.LayerObjects)
            {
                // Turn off the visibility of Bates numbers
                if (!batesVisible && layerObject is TextLayerObject
                    && layerObject.Tags.Contains(BatesNumberManager._BATES_NUMBER_TAG))
                {
                    layerObject.Visible = false;
                    continue;
                }

                // Turn off the visibility of Clues
                if (!cluesVisible && layerObject is Clue)
                {
                    layerObject.Visible = false;
                    continue;
                }

                // Turn off the visibility of redactions
                if (!redactionsVisible && layerObject is Redaction)
                {
                    layerObject.Visible = false;
                    continue;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event of the preference menu item.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="ToolStripItem.Click"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="ToolStripItem.Click"/> 
        /// event.</param>
        void HandleApplyBatesNumberToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                ApplyBatesNumbers();
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI22323", ex).Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event of the preference menu item.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="ToolStripItem.Click"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="ToolStripItem.Click"/> 
        /// event.</param>
        void HandleApplyBatesNumberToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                ApplyBatesNumbers();
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI22324", ex).Display();
            }
        }

        /// <summary>
        /// Applies Bates numbers to the currently open document.
        /// </summary>
        internal void ApplyBatesNumbers()
        {
            // Only apply bates numbers if there is an image viewer, an image is open, and
            // bates numbers have not already been applied
            if (_imageViewer != null && _imageViewer.IsImageAvailable
                && _imageViewer.GetLayeredObjects(
                new string[] { BatesNumberManager._BATES_NUMBER_TAG }, ArgumentRequirement.All,
                new Type[] { typeof(TextLayerObject) }, ArgumentRequirement.All,
                null, ArgumentRequirement.All).Count == 0)
            {
                _userPreferences.BatesNumberManager.ApplyBatesNumbers();
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectVisibilityChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="LayerObjectsCollection.LayerObjectVisibilityChanged"/> event.
        /// </param>
        /// <param name="e">The event data associated with the
        /// <see cref="LayerObjectsCollection.LayerObjectVisibilityChanged"/> event.</param>
        void HandleLayerObjectsVisibilityChanged(object sender,
            LayerObjectVisibilityChangedEventArgs e)
        {
            // Check if it was a Bates number
            if (e.LayerObject is TextLayerObject
                && e.LayerObject.Tags.Contains(BatesNumberManager._BATES_NUMBER_TAG))
            {
                if (e.LayerObject.Visible != _showBatesNumberCheckBox.Checked)
                {
                    _showBatesNumberCheckBox.Checked = e.LayerObject.Visible;
                }
            }
            // Check if it was a redaction object
            else if(e.LayerObject is Redaction)
            {
                if (e.LayerObject.Visible != _showRedactionsCheckBox.Checked)
                {
                    _showRedactionsCheckBox.Checked = e.LayerObject.Visible;
                }
            }
            // Check if it was a clue
            else if (e.LayerObject is Clue)
            {
                if (e.LayerObject.Visible != _showCluesCheckBox.Checked)
                {
                    _showCluesCheckBox.Checked = e.LayerObject.Visible;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> that sent the event.</param>
        /// <param name="e">An <see cref="LayerObjectDeletedEventArgs"/> associated
        /// with the event.</param>
        void HandleLayerObjectDeleted(object sender, LayerObjectDeletedEventArgs e)
        {
            try
            {
                // Do not set the dirty flag for search results [IDSD #231 - JDS]
                if (!e.LayerObject.Tags.Contains(RuleForm.SearchResultTag))
                {
                    // Set the dirty flag
                    Dirty = true;
                }

                UpdateButtonAndMenuItemState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23173", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> that sent the event.</param>
        /// <param name="e">An <see cref="LayerObjectAddedEventArgs"/> associated
        /// with the event.</param>
        void HandleLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                // Get the layer object that was added
                LayerObject layerObject = e.LayerObject;

                // Only modify check boxes if we are not loading an IDSO file
                if (!_modifications.LoadingIdso)
                {
                    // Ensure that the appropriate layer is turned on since a new object is
                    // being added [IDSD #347 and #348]
                    if (layerObject is Redaction)
                    {
                        // If object being added is a redaction, ensure the redaction layer
                        // is turned on
                        if (!_showRedactionsCheckBox.Checked)
                        {
                            _showRedactionsCheckBox.Checked = true;
                        }
                    }
                    else if (layerObject is Clue)
                    {
                        // If object being added is a clue, ensure the clue layer is turned on
                        if (!_showCluesCheckBox.Checked)
                        {
                            _showCluesCheckBox.Checked = true;
                        }
                    }
                    else if (layerObject is TextLayerObject)
                    {
                        // If object being added is a bates number, ensure bates number layer
                        // is turned on
                        if (layerObject.Tags.Contains(BatesNumberManager._BATES_NUMBER_TAG))
                        {
                            if (!_showBatesNumberCheckBox.Checked)
                            {
                                _showBatesNumberCheckBox.Checked = true;
                            }
                        }
                    }
                }

                // Do not set the dirty flag for search results [IDSD #231 - JDS]
                if (!layerObject.Tags.Contains(RuleForm.SearchResultTag))
                {
                    // Set the dirty flag
                    Dirty = true;
                }

                UpdateButtonAndMenuItemState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23177", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.DeletingLayerObjects"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.DeletingLayerObjects"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.DeletingLayerObjects"/> event.</param>
        void HandleDeletingLayerObjects(object sender, DeletingLayerObjectsEventArgs e)
        {
            try
            {
                // Get a collection of all the Bates numbers
                int count = 0;
                foreach (LayerObject layerObject in e.LayerObjects)
                {
                    if (layerObject.Tags.Contains(BatesNumberManager._BATES_NUMBER_TAG))
                    {
                        count++;
                    }
                }

                // If all or none of the Bates numbers have been selected, we are done.
                if (count == 0 || count == _imageViewer.PageCount)
                {
                    return;                      
                }

                // Create the prompt message
                string message =
                    "The selection contains a Bates number. ID Shield Office does not allow" +
                    Environment.NewLine +
                    "Bates numbers to be deleted on individual pages. If you remove the" +
                    Environment.NewLine +
                    "Bates number from one page, ID Shield Office will remove the Bates" +
                    Environment.NewLine +
                    "numbers on all pages." +
                    Environment.NewLine + Environment.NewLine +
                    "Are you sure you want to delete all the Bates numbers on this document?";

                // Prompt the user
                DialogResult result = MessageBox.Show(message, "Delete all Bates numbers?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1, 0);

                // Cancel if user chose No.
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                // Add all the unselected Bates numbers
                foreach (LayerObject layerObject in _imageViewer.LayerObjects)
                {
                    if (!e.LayerObjects.Contains(layerObject)
                        && layerObject.Tags.Contains(BatesNumberManager._BATES_NUMBER_TAG))
                    {
                        e.LayerObjects.Add(layerObject);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22598", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        void HandleSelectionLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                _objectPropertyGrid.SelectedObjects = 
                    _imageViewer.LayerObjects.Selection.ToArray();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22697", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.</param>
        void HandleLayerObjectChanged(object sender, LayerObjectChangedEventArgs e)
        {
            try
            {
                // Do not set the dirty flag for search results [IDSD #231 - JDS]
                if (!e.LayerObject.Tags.Contains(RuleForm.SearchResultTag))
                {
                    // Set the dirty flag
                    Dirty = true;
                }

                _objectPropertyGrid.Refresh();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22699", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        void HandleSelectionLayerObjectDeleted(object sender, LayerObjectDeletedEventArgs e)
        {
            try
            {
                _objectPropertyGrid.SelectedObjects =
                    _imageViewer.LayerObjects.Selection.ToArray();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22698", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="PropertyGrid.PropertyValueChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="PropertyGrid.PropertyValueChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="PropertyGrid.PropertyValueChanged"/> event.</param>
        void HandleObjectPropertyGridPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            try
            {
                _imageViewer.Invalidate();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22701", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event for the Page setup menu item.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="ToolStripItem.Click"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="ToolStripItem.Click"/> 
        /// event.</param>
        void HandlePageSetupMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Show the page setup dialog
                _imageViewer.ShowPrintPageSetupDialog();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23031", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event for the About ID Shield Office
        /// menu item.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="ToolStripItem.Click"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="ToolStripItem.Click"/> 
        /// event.</param>
        void HandleAboutIDShieldOfficeMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Show the about dialog
                using (AboutIDShieldOfficeForm aboutForm = new AboutIDShieldOfficeForm())
                {
                    aboutForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23075", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleSaveToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                Save(false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22873", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleSaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Save(false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22901", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandlePropertiesToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                {
                    messageBox.AddStandardButtons(MessageBoxButtons.OK);
                    messageBox.Caption = "ID Shield Office data file";
                    messageBox.Text = _modifications.ToXmlString();
                    messageBox.Show();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23007", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleSaveAsToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Save(true);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23016", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="FileDialog.FileOk"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="FileDialog.FileOk"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="FileDialog.FileOk"/> event.</param>
        void HandleFileOk(object sender, CancelEventArgs e)
        {
            string extension = GetSelectedOutputExtension();

            // Append the extension if it wasn't already
            if (!_saveAsDialog.FileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                _saveAsDialog.FileName += extension;
            }

            // Check if this was an IDSO file
            if (extension == ".idso")
            {
                // Check whether the user specifed IDSO matches the expected (mandatory) file
                string idsoFile = _imageViewer.ImageFile + ".idso";
                if (!_saveAsDialog.FileName.Equals(idsoFile, StringComparison.OrdinalIgnoreCase))
                {
                    // Prepare the message to the user
                    string text = 
                        "The ID Shield Office data file for this image can only be saved as:" + 
                        Environment.NewLine + idsoFile + Environment.NewLine + "Proceed?";

                    // Prompt the user whether to continue
                    e.Cancel = PromptToCancel(text);
                }

                return;
            }

            // Check if the file already exists
            if (File.Exists(_saveAsDialog.FileName))
            {
                // Prepare warning message
                string text = "File " + _saveAsDialog.FileName + " already exists." +
                    Environment.NewLine + "Do you want to replace it?";

                // Prompt the user whether to overwrite
                e.Cancel = PromptToCancel(text);
            }
        }

        /// <summary>
        /// Prompts the user whether or not to cancel using the specified text.
        /// </summary>
        /// <param name="text">The text to display to the user</param>
        /// <returns><see langword="true"/> if the user chose to cancel; <see langword="false"/> 
        /// if the user chose to continue.</returns>
        static bool PromptToCancel(string text)
        {
            // Prompt the user
            DialogResult result = MessageBox.Show(text, "Save document", MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 0);

            // Check if the user chose to cancel
            return result == DialogResult.No;
        }

        /// <summary>
        /// Gets the extension of the output format that the user selected from the Save As dialog.
        /// </summary>
        string GetSelectedOutputExtension()
        {
            return _OUTPUT_FORMAT_EXTENSION[_saveAsDialog.FilterIndex - 1];
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleIdShieldOfficeHelpToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                ShowIdsoHelp();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23178", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Shows the IDSO help file to the user.
        /// </summary>
        void ShowIdsoHelp()
        {
            Help.ShowHelp(this, HelpFileUrl);
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleRegularExpressionHelpToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                UserHelpMethods.ShowRegexHelp(this);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23179", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.FileOpenError"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.FileOpenError"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.FileOpenError"/> event.</param>
        void HandleFileOpenError(object sender, FileOpenErrorEventArgs e)
        {
            if (Extract.Utilities.FileSystemMethods.HasImageFileExtension(e.FileName))
            {
                // If the file's extension is one we are supposed to be able to handle,
                // return and allow the exception to be thrown.
                return;
            }

            string defaultApplicationName = "a different application" + Environment.NewLine +
                "(e.g. Microsoft Word, Microsoft Excel, Corel Wordperfect, etc)";

            // Determine the name of the application associated with this file type.
            string applicationName =
                NativeMethods.GetAssociatedApplication(Path.GetExtension(e.FileName));
            if (String.IsNullOrEmpty(applicationName))
            {
                // If an associated application was not found, generalize the message.
                applicationName = defaultApplicationName;
            }

            using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
            {
                // Prepare a friendly message for the user about the problem.
                messageBox.Text = "You are attempting to open a file associated with " +
                    applicationName + "." + Environment.NewLine +
                    "ID Shield Office can only open image and PDF files." +
                    Environment.NewLine + Environment.NewLine +
                    (applicationName == defaultApplicationName ?
                        "Would you like to know how to open documents from those applications" +
                        Environment.NewLine +
                        "in ID Shield Office for redaction purposes?"
                        :
                        "To redact this document, open the document with " + applicationName +
                        " and then print it to the ID Shield printer.");

                messageBox.Caption = "Could not open " + Path.GetFileName(e.FileName);
                messageBox.StandardIcon = MessageBoxIcon.Information;
                if (applicationName == defaultApplicationName)
                {
                    // If there is not associated extention, only allow the user to get help or cancel.
                    messageBox.AddButton("Yes", "Help", true);
                    messageBox.AddButton("No", "Cancel", false);
                }
                else
                {
                    // If there was an associated extension, allow the user a chance to the file
                    // with it.
                    messageBox.AddButton("Open file with " + applicationName, "Open", true);
                    messageBox.AddButton("See help for more information", "Help", false);
                    messageBox.AddButton("Cancel", "Cancel", false);
                }

                // Prompt the user.
                string result = messageBox.Show();

                if (result == "Open")
                {
                    // Open the document with the associated application.
                    System.Diagnostics.Process.Start(e.FileName);
                }
                else if (result == "Help")
                {
                    // Open ID Shield Office's help page about printing.
                    Help.ShowHelp(this, HelpFileUrl, HelpNavigator.KeywordIndex, 
                        _PRINTING_HELP_INDEX);
                }
            }

            // We've handled the exception.
            e.Cancel = true;

            // Since the exception will no longer be thrown, log it so there is a record of the error.
            e.ExtractException.Log();
        }
    }
}
