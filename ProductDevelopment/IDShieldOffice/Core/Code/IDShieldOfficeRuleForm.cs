using Extract;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace IDShieldOffice
{
    /// <summary>
    /// Base class for all of the IDShield Office Rule forms.
    /// </summary>
    internal partial class IDShieldOfficeRuleForm : Form
    {
        #region Constants

        /// <summary>
        /// Default text that will be displayed in the <see cref="IDShieldOfficeRuleForm"/>.
        /// </summary>
        private static readonly string _CAPTION_TEXT = "Find or redact";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(IDShieldOfficeRuleForm).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The IDShield office form that the find/redact form is associated with.
        /// </summary>
        private IDShieldOfficeForm _idShieldOfficeForm;

        /// <summary>
        /// The rule to run in this form.
        /// </summary>
        private IIDShieldOfficeRule _rule;

        /// <summary>
        /// The search results collection. 
        /// </summary>
        private List<FindResult> _findResults;

        /// <summary>
        /// The colletion of matches.
        /// </summary>
        private List<MatchResult> _matchResults;

        /// <summary>
        /// The index of the next find result to display.
        /// </summary>
        private int _nextFindResult;

        /// <summary>
        /// The property page interface for the current rules property page.
        /// </summary>
        private IPropertyPage _propertyPage;

        /// <summary>
        /// Flag to indicate whether a find operation has already been initiated.
        /// </summary>
        private bool _finding;

        /// <summary>
        /// Flag to indicate that the last find result has been redacted.
        /// </summary>
        private bool _haveRedactedLastFindResult;

        /// <summary>
        /// The total number of clues in _findResults
        /// </summary>
        private int _numTotalClues;

        /// <summary>
        /// The number of new clues in _findResults
        /// </summary>
        private int _numNewClues;

        /// <summary>
        /// The total number of matches in _findResults
        /// </summary>
        private int _numTotalMatches;

        /// <summary>
        /// The number of new matches in _findResults
        /// </summary>
        private int _numNewMatches;

        /// <summary>
        /// The number of clues that have been iterated through
        /// </summary>
        private int _currentClue;

        /// <summary>
        /// The number of matches that have been iterated through
        /// </summary>
        private int _currentMatch;

        /// <summary>
        /// Indicates whether we are displaying just new clues and redactions that resulted from a 
        /// redact all operation or whether we are viewing all results from a find operation.
        /// </summary>
        private bool _showingRedactAllResults;

        /// <summary>
        /// Indicates the MatchType of the currently displayed result or null if no
        /// result is currently displayed.
        /// </summary>
        private MatchType? _currentMatchType;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="IDShieldOfficeRuleForm"/> with the specified
        /// string appended to the form title.
        /// </summary>
        /// <param name="titleText">The text to append to the title bar.</param>
        /// <param name="rule">The <see cref="IIDShieldOfficeRule"/> to run in
        /// the find/redact form.</param>
        /// <param name="idShieldOfficeForm">The IDShield office form to associate with
        /// this find/redact form.</param>
        public IDShieldOfficeRuleForm(string titleText, IIDShieldOfficeRule rule,
            IDShieldOfficeForm idShieldOfficeForm) : base()
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel()); 
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23198",
                    _OBJECT_NAME);

                InitializeComponent();

                // Set the form caption
                this.Text = IDShieldOfficeRuleForm._CAPTION_TEXT +
                    (string.IsNullOrEmpty(titleText) ? "" : (" - " + titleText));

                // Set the IDShield Office form
                _idShieldOfficeForm = idShieldOfficeForm;
                
                // Set the rule
                _rule = rule;

                // Set the IDSO form as the owner of this form
                this.Owner = _idShieldOfficeForm;

                // Add the image file changed event handler
                _idShieldOfficeForm.ImageViewer.ImageFileChanged += HandleImageFileChanged;

                // Add the layer object visibility changed event handler
                _idShieldOfficeForm.ImageViewer.LayerObjects.LayerObjectVisibilityChanged
                    += HandleLayerObjectVisibilityChanged;
            }
            catch (Exception ex)
            {
                ExtractException ee =  new ExtractException("ELI22123",
                    "Failed to initialize ID Shield Office rule form!", ex);
                ee.AddDebugData("Rule", rule != null ? rule.ToString() : "null", false);
                throw ee;
            }
        }

        #endregion Constructors

        #region Event handlers

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// Also adds the appropriate property page to the top panel and resizes the form.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
#if DEBUG
                // [IDSD:268]
                // Guard against the designer inappropriately modifying SplitterDistance or other values.
                ExtractException.Assert("ELI23320", 
                    "It appears the designer has made unintended changes to the IDShieldOfficRuleForm. " +
                    "Please verify control size and positioning", _splitContainer.SplitterDistance == 0);
#endif

                base.OnLoad(e);

                // Get the property page
                Control propertyPage = _rule.PropertyPage;

                // Cast the property page to an IPropertyPage interface and store it
                _propertyPage = propertyPage as IPropertyPage;

                // If there is a property page, then add it and resize the form to fit it
                if (_propertyPage != null)
                {
                    // Add the handler for property page modifications
                    _propertyPage.PropertyPageModified += HandlePropertyPageModified;

                    FormsMethods.DockControlIntoContainer(propertyPage, this,
                        _splitContainer.Panel1);

                    _propertyPage.SetFocusToFirstControl();
                }
                else
                {
                    // If there is no property page then make the form non-resizeable
                    this.FormBorderStyle = FormBorderStyle.FixedDialog;
                }

                // Update the button states
                UpdateButtonStates();

                // Update the results table
                UpdateResults();

                // Update the status bar message.
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22121", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.Closing"/> event.
        /// </summary>
        /// <param name="e">An <see cref="CancelEventArgs"/> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                // Cancel this event since we do not want to dispose of the form, simple hide it
                e.Cancel = true;

                // Reset the find dialog before hiding
                ResetFindDialog();

                // Hide the form
                this.Hide();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22122", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.VisibleChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            try
            {
                base.OnVisibleChanged(e);

                // Update the button states
                UpdateButtonStates();

                // Update the results table
                UpdateResults();

                // Update the status bar message.
                UpdateStatusBar();

                // Set the focus to the property page
                if (_propertyPage != null)
                {
                    _propertyPage.SetFocusToFirstControl();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22216", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> which sent the event.</param>
        /// <param name="e">An <see cref="ImageFileChangedEventArgs"/>
        /// that contains event data.</param>
        private void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                // Reset the find dialog
                ResetFindDialog();

                // Update the button states
                UpdateButtonStates();

                // Update the results table
                UpdateResults();

                // Update the status bar message.
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI22315", ex).Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectVisibilityChanged"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> which sent the event.</param>
        /// <param name="e">An <see cref="LayerObjectVisibilityChangedEventArgs"/>
        /// that contains event data.</param>
        private void HandleLayerObjectVisibilityChanged(object sender,
            LayerObjectVisibilityChangedEventArgs e)
        {
            try
            {
                // Update the button states
                UpdateButtonStates();

                // Update the results table
                UpdateResults();

                // Update the status bar message.
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22927", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="IPropertyPage.PropertyPageModified"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
        private void HandlePropertyPageModified(object sender, EventArgs e)
        {
            try
            {
                // Update the button states
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22217", ex);
            }
        }

        /// <summary>
        /// Handles the Reset button click event.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
        private void HandleResetButton(object sender, EventArgs e)
        {
            try
            {
                // Reset the find dialog
                ResetFindDialog();

                // Update the button states
                UpdateButtonStates();

                // Update the results table
                UpdateResults();

                // Update the status bar message.
                UpdateStatusBar();

                // Set the focus to the property page
                if (_propertyPage != null)
                {
                    _propertyPage.SetFocusToFirstControl();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22126", ex);
                ee.AddDebugData("Event Args", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the FindNext button click event.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
        private void HandleFindNextButton(object sender, EventArgs e)
        {
            try
            {
                // Halt here if matches cannot be updated
                if (!TryUpdateMatches())
                {
                    // Reset the find dialog
                    ResetFindDialog();

                    return;
                }

                // Display the next match
                DisplayNextMatch();

                // Update the button states
                UpdateButtonStates();

                // Update the results table
                UpdateResults();

                // Update the status bar message.
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22127", ex);
                ee.AddDebugData("Event Args", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the Redact button click event.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
        private void HandleRedactButton(object sender, EventArgs e)
        {
            try
            {
                // Get the image viewer
                ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

                // Get the find result
                FindResult findResult = _findResults[_nextFindResult-1];

                // Get the composite highlight from the find result
                CompositeHighlightLayerObject compositeMatch = findResult.CompositeMatch;

                // Create a new redaction from the find result
                Redaction redaction = new Redaction(imageViewer, compositeMatch.PageNumber,
                    findResult.MatchResult.FindingRule, compositeMatch.GetRasterZones(),
                    imageViewer.DefaultRedactionFillColor);

                // Add the id of the object to the find result
                findResult.RedactionId = redaction.Id;

                // Find if there are any other redactions to link this object with
                foreach (FindResult result in _findResults)
                {
                    // Check for another find result from the same match
                    if (result.MatchResult == findResult.MatchResult && result != findResult)
                    {
                        // Try to get the redaction associated with this link
                        Redaction redactionToLink =
                            imageViewer.LayerObjects.TryGetLayerObject(result.RedactionId ?? -1)
                            as Redaction;

                        // If there was a redaction, add a link to it
                        if (redactionToLink != null)
                        {
                            redaction.AddLink(redactionToLink);
                        }
                    }
                }

                // Add the new redaction to the image viewer
                imageViewer.LayerObjects.Add(redaction);

                // Invalidate the form so the redaction is drawn
                imageViewer.Invalidate();

                // Check if at the last match result (do this before the call to display next match)
                if (_findResults != null && _nextFindResult >= _findResults.Count)
                {
                    // If at last match set redacted last find result to true
                    _haveRedactedLastFindResult = true;
                }

                // Move to the next match
                DisplayNextMatch();

                // Update the button states
                UpdateButtonStates();

                // Update the status bar message.
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22128", ex);
                ee.AddDebugData("Event Args", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the RedactAll button click event.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
        private void HandleRedactAllButton(object sender, EventArgs e)
        {
            try
            {
                // Reset all counts and find results before running the redact all operation
                // so that the counts are calculated correctly and the first newly redacted result
                // or newly found clue is displayed once the operation is complete
                ResetFindDialog();

                _showingRedactAllResults = true;

                // Halt here if matches cannot be updated
                if (!TryUpdateMatches())
                {
                    // Reset the find dialog
                    ResetFindDialog();

                    return;
                }
                
                // Show the wait cursor
                using (new TemporaryWaitCursor())
                {
                    // Loop through all of the match results
                    foreach (MatchResult matchResult in _matchResults)
                    {
                        // If the match result is a match then redact it
                        if (matchResult.MatchType == MatchType.Match)
                        {
                            _idShieldOfficeForm.AddRedaction(matchResult.RasterZones,
                                matchResult.FindingRule);
                        }
                    }
                }

                // Invalidate the form so the redaction is drawn
                _idShieldOfficeForm.ImageViewer.Invalidate();

                if (_numNewMatches > 0 || _numNewClues > 0)
                {
                    // Display the first newly redacted result of newly found clue (if there were any).
                    DisplayNextMatch();
                }

                // Update the button states
                UpdateButtonStates();

                // Update the results table
                UpdateResults();

                // Update the status bar message.
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22129", ex);
                ee.AddDebugData("Event Args", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the Close button click event.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
        private void HandleCloseButton(object sender, EventArgs e)
        {
            // Reset the find dialog before hiding the form
            ResetFindDialog();

            // Hide the form
            this.Hide();

            // Give IDSO the focus
            _idShieldOfficeForm.Focus();
        }

//        /// <summary>
//        /// This should eventually  be able to be used to prevent resizing of columns.
//        /// However, I was not able to get this to be called. (IDSD:304)
//        /// </summary>
//        /// <param name="sender">Temp</param>
//        /// <param name="e">Temp</param>
//        void HandleResultsListColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
//        {
//            try
//            {
//                e.Cancel = true;
//                e.NewWidth = this._resultsList.Columns[e.ColumnIndex].Width;
//            }
//            catch (Exception ex)
//            {
//                ExtractException ee = ExtractException.AsExtractException("ELI23323", ex);
//                ee.AddDebugData("Event Args", e, false);
//                ee.Display();
//            }
//        }

        #endregion Event handlers

        #region Methods

        /// <summary>
        /// Resets the find dialog and clears the currently visible match (if there is one)
        /// from the image window.
        /// </summary>
        private void ResetFindDialog()
        {
            // Clear any current match
            ClearCurrentMatch();

            // Clear any find results
            if (_findResults != null)
            {
                foreach (FindResult findResult in _findResults)
                {
                    findResult.CompositeMatch.Dispose();
                }

                _findResults.Clear();
                _findResults = null;
            }

            // Set the next match pointer back to 0 
            _nextFindResult = 0;

            // Clear any match results
            if (_matchResults != null)
            {
                _matchResults.Clear();
                _matchResults = null;
            }

            // Enable the property page if it exists
            Control propertyPage = _propertyPage as Control;
            if (propertyPage != null)
            {
                propertyPage.Enabled = true;
            }

            _showingRedactAllResults = false;
            _haveRedactedLastFindResult = false;
            _currentMatchType = null;
            _numTotalClues = 0;
            _numNewClues = 0;
            _numTotalMatches = 0;
            _numNewMatches = 0;
            _currentClue = 0;
            _currentMatch = 0;

            // Update the results table
            UpdateResults();

            // Update the status bar message.
            UpdateStatusBar();
        }

        /// <summary>
        /// Clears the currently displayed match
        /// </summary>
        private void ClearCurrentMatch()
        {
            _currentMatchType = null;

            // If there is a result to remove, remove it
            if (_nextFindResult > 0 && _nextFindResult <= _findResults.Count)
            {
                ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

                // Remove the currently visible find result if possible
                if (imageViewer.LayerObjects.Contains(_findResults[_nextFindResult - 1].CompositeMatch.Id))
                {
                    imageViewer.LayerObjects.Remove(_findResults[_nextFindResult - 1].CompositeMatch);
                }

                // Invalidate the form so it will redraw
                imageViewer.Invalidate();
            }
        }

        /// <summary>
        /// Attempts to update <see cref="_matchResults"/>.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="_matchResults"/> can be updated 
        /// or was already updated; <see langword="false"/> if the property page is invalid.
        /// </returns>
        private bool TryUpdateMatches()
        {
            // If currently in the midst of a find operation, just return false
            if (_finding)
            {
                return false;
            }

            if (_findResults == null)
            {
                // If there is a property page, ensure that the changes
                // have been saved and that they are valid.
                if (_propertyPage != null)
                {
                    if (_propertyPage.IsDirty)
                    {
                        // This will display a message if the property page
                        // is not valid.
                        _propertyPage.Apply();
                    }

                    // Check that the property page was valid
                    if (!_propertyPage.IsValid)
                    {
                        return false;
                    }

                    // Disable the property page if it exists
                    Control propertyPage = _propertyPage as Control;
                    if (propertyPage != null)
                    {
                        propertyPage.Enabled = false;
                    }
                }

                try
                {
                    // Set the finding flag to true
                    _finding = true;

                    // Clear results and status bar as soon as a find starts
                    UpdateResults();
                    UpdateStatusBar();
                    this.Refresh();

                    // Set the wait cursor
                    using (new TemporaryWaitCursor())
                    {
                        // Get the current image name
                        string imageFileName = _idShieldOfficeForm.ImageViewer.ImageFile;

                        // Wait for OCR to complete
                        while (!_idShieldOfficeForm.OcrManager.OcrFinished)
                        {
                            Application.DoEvents();

                            // If while waiting the image file was changed, then exit this find
                            if (imageFileName != _idShieldOfficeForm.ImageViewer.ImageFile)
                            {
                                // False indicates no matches have been updated
                                return false;
                            }

                            // [IDSD:344]
                            // Wait a tenth of a second between checks for OCR information so that
                            // we don't burn CPU unnecessarily while waiting for OCR result.
                            System.Threading.Thread.Sleep(100);
                        }

                        // Get the SpatialString
                        UCLID_RASTERANDOCRMGMTLib.SpatialString ocrOutput =
                            _idShieldOfficeForm.OcrManager.GetOcrSpatialString();

                        // Get the match results
                        _matchResults = _rule.GetMatches(ocrOutput);

                        if (_matchResults.Count == 0)
                        {
                            MessageBox.Show("No matches found!", "No Matches", MessageBoxButtons.OK,
                                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
                            return false;
                        }

                        // Sort the results
                        _matchResults.Sort();

                        // Create the find result collection by splitting the match results
                        _findResults = SplitMatchResultsByPage(_matchResults);

                        // Count the number of matches and clues and remove any duplicates as per
                        // _showingRedactAllResults.  Make sure to do this before the AddClues
                        // call so that all clues don't appear to be duplicates.
                        CountFindResults(MatchType.Clue, _showingRedactAllResults,
                            out _numTotalClues, out _numNewClues);
                        CountFindResults(MatchType.Match, _showingRedactAllResults,
                            out _numTotalMatches, out _numNewMatches);

                        // Add all the clues to the document. 
                        _idShieldOfficeForm.AddClues(_matchResults);
                    }
                }
                finally
                {
                    // After the find is complete, set the finding flag to false
                    _finding = false;
                }
            }

            return true;
        }

        /// <summary>
        /// Counts the total number of find results that are of the specified type.
        /// </summary>
        /// <param name="type">The <see cref="MatchType"/> a find result must be to be counted.</param>
        /// <param name="removeDuplicates">If <see langword="true"/>, removes results that are 
        /// already present in the image viewer. NOTE: If <see langword="false"/>, the newCount
        /// parameter will not be calculated and will always return 0.</param>
        /// <param name="totalCount">Returns the total number of find results of the specified 
        /// type.</param>
        /// <param name="newCount">Returns the number of find results found that were not
        /// already present in the image viewer.</param>
        private void CountFindResults(MatchType type, bool removeDuplicates, out int totalCount, 
            out int newCount)
        {
            // Initialize counts to zero.
            totalCount = 0;
            newCount = 0;

            // Cycle through all find results.
            for (int i = 0; i < _findResults.Count; i++)
            {
                if (_findResults[i].MatchResult.MatchType != type)
                {
                    // If this find result isn't of the right type, skip it.
                    continue;
                }

                // Count the result.
                totalCount++;

                if (removeDuplicates)
                {
                    // If removeDuplicates is true, remove this result from _findResults if it 
                    // is a duplicate, count it as new if it is not a duplicate.
                    bool isDuplicate;

                    if (type == MatchType.Clue)
                    {
                        // This found clue is a duplicate if we were unable to create a new unique
                        // clue in the image viewer.
                        isDuplicate = (_idShieldOfficeForm.CreateIfNotDuplicate<Clue>(
                            _findResults[i].CompositeMatch.GetRasterZones(), "") == null);
                    }
                    else
                    {
                        // This found redaction is a duplicate if we were unable to create a new unique
                        // redaction in the image viewer.
                        isDuplicate = (_idShieldOfficeForm.CreateIfNotDuplicate<Redaction>(
                            _findResults[i].CompositeMatch.GetRasterZones(), "") == null);
                    }

                    if (isDuplicate)
                    {
                        // Remove the duplicate.
                        _findResults.RemoveAt(i);
                        i--;
                        continue;
                    }

                    // Count the newly found result.
                    newCount++;
                }
            }
        }

        /// <summary>
        /// Clears the current displayed match, and displays the next match (if there is a match
        /// remaining to display)
        /// </summary>
        private void DisplayNextMatch()
        {
            // Clear any current match
            ClearCurrentMatch();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Ensure there is a find to display
            if (_findResults != null && _nextFindResult < _findResults.Count)
            {
                // Get the next find result to display (if clues are hidden skip clues)
                FindResult findResult = _findResults[_nextFindResult++];
                while (!_idShieldOfficeForm.CluesVisible
                    && findResult.MatchResult.MatchType == MatchType.Clue
                    && _nextFindResult < _findResults.Count)
                {
                    // Indicate that we've iterated through this clue even if it not displayed.
                    _currentClue++;
                    findResult = _findResults[_nextFindResult++];
                }

                // Ensure that there is an item to display
                if (_nextFindResult <= _findResults.Count)
                {
                    if (findResult.MatchResult.MatchType == MatchType.Clue)
                    {
                        // Indicate the current find result for the status bar message.
                        _currentClue++;
                        _currentMatchType = MatchType.Clue;
                    }
                    else
                    {
                        // Indicate the current clue for the status bar message.
                        _currentMatch++;
                        _currentMatchType = MatchType.Match;
                    }

                    // Add the find result to the image viewer and center on it
                    imageViewer.LayerObjects.Add(findResult.CompositeMatch);

                    // Center the image viewer around the first composite highlight
                    // in the match results
                    imageViewer.CenterOnLayerObjects(findResult.CompositeMatch);
                }
            }

            // Invalidate the image viewer to ensure the composite highlight is displayed
            imageViewer.Invalidate();
        }

        /// <summary>
        /// Updates the enabled/disabled state of the buttons on the
        /// <see cref="IDShieldOfficeRuleForm"/>.
        /// </summary>
        private void UpdateButtonStates()
        {
            // Find next is enabled if all of the following are true:
            // 1) There is an open image
            // 2) There is a valid property page 
            // 3) There are more matches to display
            _findNextButton.Enabled = _idShieldOfficeForm.ImageViewer.IsImageAvailable &&
                (_propertyPage == null || _propertyPage.IsValid) &&
                (_findResults == null || _nextFindResult < _findResults.Count);

            // If clues are not visible, also need to check that there is at least one
            // remaining find result that is not a clue
            // [IDSD:288] Added check to disable redact button when the current find result is a clue.
            if (_findNextButton.Enabled && !_idShieldOfficeForm.CluesVisible && _findResults != null)
            {
                int count = _nextFindResult;
                while (count < _findResults.Count && 
                       _findResults[count].MatchResult.MatchType == MatchType.Clue)
                {
                    count++;
                }

                _findNextButton.Enabled = count < _findResults.Count;
            }

            // Redact is enabled if all of the following are true:
            // 1) A search has begun (_findResults != null)
            // 2) We are not showing redact all results
            // 3) The current result is a match
            // 4) Have not redacted the last match result yet
            _redactButton.Enabled = _findResults != null && 
                !_showingRedactAllResults && _currentMatchType == MatchType.Match &&
                !_haveRedactedLastFindResult;

            // Redact all is enabled if an image is open and there is a valid property page
            _redactAllButton.Enabled = _idShieldOfficeForm.ImageViewer.IsImageAvailable && 
                (_propertyPage == null || _propertyPage.IsValid);

            // Reset is enabled if a search has begun (_findResults != null)
            _resetButton.Enabled = _findResults != null;
        }

        /// <summary>
        /// Updates the rule results table.
        /// </summary>
        private void UpdateResults()
        {
            // Clear any existing result.
            _resultsList.DataSource = null;

            // Create a new data table to hold the results
            DataTable dt = new DataTable("Results");
            dt.Locale = CultureInfo.CurrentUICulture;

            // Add the results column and two empty rows
            dt.Columns.Add("Results");
            dt.Rows.Add(dt.NewRow());
            dt.Rows.Add(dt.NewRow());

            // If there are find results, add the data to the table
            if (_findResults != null)
            {
                if (_showingRedactAllResults)
                {
                    // Label first row as redactions
                    dt.Rows[0][0] = "Redactions";

                    // If showing redact all results, we need Added and Previously redacted columns
                    dt.Columns.Add("Added");
                    dt.Columns.Add("Previously existing");

                    // Add the new and previously existing redaction totals.
                    dt.Rows[0][1] = _numNewMatches;
                    dt.Rows[0][2] = _numTotalMatches - _numNewMatches;
                }
                else
                {
                    // Label first row as match results
                    dt.Rows[0][0] = "Matches";
                }

                // Add the total column
                dt.Columns.Add("Total");

                // Update the total column for matches/redactions
                dt.Rows[0]["Total"] = _numTotalMatches;

                if (_rule.UsesClues && _idShieldOfficeForm.CluesVisible)
                {
                    // Label the clue row
                    dt.Rows[1][0] = "Clues";

                    if (_showingRedactAllResults)
                    {
                        // If showing redact all results, add the new and previously existing clue totals.
                        dt.Rows[1][1] = _numNewClues;
                        dt.Rows[1][2] = _numTotalClues - _numNewClues;
                    }

                    // Update the total column for clues
                    dt.Rows[1]["Total"] = _numTotalClues;
                }
            }

            _resultsList.DataSource = dt;
        }

        /// <summary>
        /// Updates the message on the status bar to reflect the current state of the search.
        /// </summary>
        private void UpdateStatusBar()
        {
            if (_finding)
            {
                // If a search is in progress, indicate it in the status bar.
                this._toolStripStatusLabel.Text = "Searching...";
                return;
            }

            if (_findResults == null)
            {
                // If no results are available, add the table title and nothing else.
                this._toolStripStatusLabel.Text = "";
                return;
            }

            StringBuilder status = new StringBuilder();

            if (_currentMatchType == MatchType.Match)
            {
                // A match is currently being displayed; indicate which one.
                int numMatches = _showingRedactAllResults ? _numNewMatches : _numTotalMatches;

                status.Append("Displaying");
                status.Append(_showingRedactAllResults ? " new redaction " : " match ");
                status.Append(_currentMatch.ToString(CultureInfo.CurrentCulture));
                status.Append(" of " + numMatches.ToString(CultureInfo.CurrentCulture));
                status.Append(".");
            }
            else if (_currentMatchType == MatchType.Clue)
            {
                // A clue is currently being displayed. Indicate which one.
                int numClues = _showingRedactAllResults ? _numNewClues : _numTotalClues;

                status.Append("Displaying");
                status.Append(_showingRedactAllResults ? " new clue " : " clue ");
                status.Append(_currentClue.ToString(CultureInfo.CurrentCulture));
                status.Append(" of " + numClues.ToString(CultureInfo.CurrentCulture));
                status.Append(".");
            }

            this._toolStripStatusLabel.Text = status.ToString();
        }

        /// <summary>
        /// Will split the list of <see cref="MatchResult"/> objects into individual
        /// <see cref="FindResult"/> pieces on separate pages.
        /// </summary>
        /// <param name="matchResults">The list of match results to split.</param>
        /// <returns>A list of <see cref="FindResult"/> objects.</returns>
        private List<FindResult> SplitMatchResultsByPage(List<MatchResult> matchResults)
        {
            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Build the list of find results
            List<FindResult> findResults = new List<FindResult>();
            foreach (MatchResult matchResult in matchResults)
            {
                // Split the match results raster zones by pages
                foreach (KeyValuePair<int, List<RasterZone>> pair in
                    RasterZone.SplitZonesByPage(matchResult.RasterZones))
                {
                    // Create a composite highlight with the appropriate search tags,
                    // find color, selectable set to false and can render to false
                    CompositeHighlightLayerObject compositeHighlight =
                        new CompositeHighlightLayerObject(imageViewer, pair.Key,
                        IDShieldOfficeForm._SEARCH_RESULT_TAGS, pair.Value,
                        IDShieldOfficeForm._FIND_COLOR);
                    compositeHighlight.Selectable = false;
                    compositeHighlight.CanRender = false;
                    compositeHighlight.OutlineColor = IDShieldOfficeForm._FIND_COLOR;

                    // Add the find result to the collection
                    findResults.Add(new FindResult(compositeHighlight, matchResult));
                }
            }

            // Sort the collection of find results
            findResults.Sort();

            // Return the list of find results
            return findResults;
        }

        #endregion
    }
}