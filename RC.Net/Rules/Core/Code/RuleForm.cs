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

using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.Rules
{
    /// <summary>
    /// Base class for all of the IDShield Office Rule forms.
    /// </summary>
    public partial class RuleForm : Form
    {
        #region Constants

        /// <summary>
        /// Default text that will be displayed in the <see cref="RuleForm"/>.
        /// </summary>
        const string _CAPTION_TEXT = "Find or redact";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(RuleForm).ToString();

        /// <summary>
        /// The color to be used for find results.
        /// </summary>
        static readonly Color _FIND_COLOR = Color.LimeGreen;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The tag that will be added to search results.
        /// </summary>
        public static readonly string SearchResultTag = "Search result";

        /// <summary>
        /// The rule to run in this form.
        /// </summary>
        readonly IRule _rule;

        /// <summary>
        /// Used to query external source about OCR results, duplicate clues &amp; redactions, and 
        /// whether to include clues in the search results.
        /// </summary>
        readonly IRuleFormHelper _helper;

        /// <summary>
        /// The image viewer on which the results will be displayed.
        /// </summary>
        readonly ImageViewer _imageViewer;

        /// <summary>
        /// The search results collection. 
        /// </summary>
        List<FindResult> _findResults;

        /// <summary>
        /// The collection of matches.
        /// </summary>
        MatchResultCollection _matchResults;

        /// <summary>
        /// The index of the next find result to display.
        /// </summary>
        int _nextFindResult;

        /// <summary>
        /// The property page interface for the current rules property page.
        /// </summary>
        IPropertyPage _propertyPage;

        /// <summary>
        /// Flag to indicate whether a find operation has already been initiated.
        /// </summary>
        bool _finding;

        /// <summary>
        /// Flag to indicate that the last find result has been redacted.
        /// </summary>
        bool _haveRedactedLastFindResult;

        /// <summary>
        /// The total number of clues in _findResults
        /// </summary>
        int _numTotalClues;

        /// <summary>
        /// The number of new clues in _findResults
        /// </summary>
        int _numNewClues;

        /// <summary>
        /// The total number of matches in _findResults
        /// </summary>
        int _numTotalMatches;

        /// <summary>
        /// The number of new matches in _findResults
        /// </summary>
        int _numNewMatches;

        /// <summary>
        /// The number of clues that have been iterated through
        /// </summary>
        int _currentClue;

        /// <summary>
        /// The number of matches that have been iterated through
        /// </summary>
        int _currentMatch;

        /// <summary>
        /// Indicates whether we are displaying just new clues and redactions that resulted from a 
        /// redact all operation or whether we are viewing all results from a find operation.
        /// </summary>
        bool _showingRedactAllResults;

        /// <summary>
        /// Indicates the MatchType of the currently displayed result or null if no
        /// result is currently displayed.
        /// </summary>
        MatchType? _currentMatchType;

        /// <summary>
        /// <see langword="true"/> if clues should be shown as part of the search results; 
        /// <see langword="false"/> if clues should be hidden.
        /// </summary>
        bool _showClues;

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when matches are found.
        /// </summary>
        public event EventHandler<MatchesFoundEventArgs> MatchesFound;

        /// <summary>
        /// Occurs when the user chooses to redact a match.
        /// </summary>
        public event EventHandler<MatchRedactedEventArgs> MatchRedacted;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="RuleForm"/> with the specified
        /// string appended to the form title.
        /// </summary>
        /// <param name="titleText">The text to append to the title bar.</param>
        /// <param name="rule">The <see cref="IRule"/> to run in
        /// the find/redact form.</param>
        /// <param name="imageViewer">The image viewer on which the results of searches should be 
        /// shown.</param>
        /// <param name="helper">Provides OCR and other assistance to the rule form.</param>
        /// <param name="owner">The IDShield office form to associate with
        /// this find/redact form.</param>
        [CLSCompliant(false)]
        public RuleForm(string titleText, IRule rule,
            ImageViewer imageViewer, IRuleFormHelper helper, Form owner)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.RedactionCoreObjects, "ELI23198",
                    _OBJECT_NAME);

                InitializeComponent();

                // Set the form caption
                Text = _CAPTION_TEXT + (string.IsNullOrEmpty(titleText) ? "" : (" - " + titleText));

                // Store parameters
                _imageViewer = imageViewer;
                _helper = helper;
                _rule = rule;
                Owner = owner;

                // Add the image file changed event handler
                _imageViewer.ImageFileChanged += HandleImageFileChanged;
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

        #region Properties

        /// <summary>
        /// Gets or sets whether clues should be displayed.
        /// </summary>
        /// <value><see langword="true"/> if clues should be displayed;
        /// <see langword="false"/> if clues should not be displayed.</value>
        public bool ShowClues
        {
            get
            {
                return _showClues;
            }
            set
            {
                try
                {
                    if (_showClues != value)
                    {
                        _showClues = value;

                        // Update the button states
                        UpdateButtonStates();

                        // Update the results table
                        UpdateResults();

                        // Update the status bar message.
                        UpdateStatusBar();
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI22927", ex);
                }
            }
        }

        #endregion Properties

        #region On Events

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
                    FormBorderStyle = FormBorderStyle.FixedDialog;
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
                Hide();

                // Restore the focus to its owner [FIDSC #3952]
                Owner.Focus();
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
        /// Raises the <see cref="MatchesFound"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="MatchesFound"/> 
        /// event.</param>
        protected virtual void OnMatchesFound(MatchesFoundEventArgs e)
        {
            if (MatchesFound != null)
            {
                MatchesFound(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="MatchRedacted"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="MatchRedacted"/> 
        /// event.</param>
        protected virtual void OnMatchRedacted(MatchRedactedEventArgs e)
        {
            if (MatchRedacted != null)
            {
                MatchRedacted(this, e);
            }
        }

        #endregion On Events

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> which sent the event.</param>
        /// <param name="e">An <see cref="ImageFileChangedEventArgs"/>
        /// that contains event data.</param>
        void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
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
                ExtractException.Display("ELI22315", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="IPropertyPage.PropertyPageModified"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> which sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
        void HandlePropertyPageModified(object sender, EventArgs e)
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
        void HandleResetButton(object sender, EventArgs e)
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
        void HandleFindNextButton(object sender, EventArgs e)
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
        void HandleRedactButton(object sender, EventArgs e)
        {
            try
            {
                // Get the find result
                FindResult findResult = _findResults[_nextFindResult-1];

                // Create a match result for this find result
                IEnumerable<RasterZone> zones = findResult.CompositeMatch.GetRasterZones();
                MatchType type = findResult.MatchResult.MatchType;
                string text = findResult.MatchResult.Text;
                string rule = findResult.MatchResult.FindingRule;
                MatchResult matchResult = new MatchResult(zones, type, text, rule);
                
                // Raise the MatchRedacted event
                OnMatchRedacted(new MatchRedactedEventArgs(matchResult));

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
        void HandleRedactAllButton(object sender, EventArgs e)
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
                            // Raise the MatchRedacted event
                            OnMatchRedacted(new MatchRedactedEventArgs(matchResult));
                        }
                    }
                }

                // Invalidate the form so the redaction is drawn
                _imageViewer.Invalidate();

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
        void HandleCloseButton(object sender, EventArgs e)
        {
            // Reset the find dialog before hiding the form
            ResetFindDialog();

            // Hide the form
            Hide();
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

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Resets the find dialog and clears the currently visible match (if there is one)
        /// from the image window.
        /// </summary>
        void ResetFindDialog()
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
        void ClearCurrentMatch()
        {
            _currentMatchType = null;

            // If there is a result to remove, remove it
            if (_nextFindResult > 0 && _nextFindResult <= _findResults.Count)
            {
                // Remove the currently visible find result if possible
                if (_imageViewer.LayerObjects.Contains(_findResults[_nextFindResult - 1].CompositeMatch.Id))
                {
                    _imageViewer.LayerObjects.Remove(_findResults[_nextFindResult - 1].CompositeMatch);
                }

                // Invalidate the form so it will redraw
                _imageViewer.Invalidate();
            }
        }

        /// <summary>
        /// Attempts to update <see cref="_matchResults"/>.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="_matchResults"/> can be updated 
        /// or was already updated; <see langword="false"/> if the property page is invalid.
        /// </returns>
        bool TryUpdateMatches()
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
                    Refresh();

                    // Get the ocr results
                    SpatialString ocrOutput = _helper.GetOcrResults();
                    if (ocrOutput == null)
                    {
                        // False indicates no matches have been updated
                        return false;
                    }

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
                    // _showingRedactAllResults.  Make sure to do this before raising the 
                    // MatchesFound event so that all clues don't appear to be duplicates.
                    CountFindResults(MatchType.Clue, _showingRedactAllResults,
                        out _numTotalClues, out _numNewClues);
                    CountFindResults(MatchType.Match, _showingRedactAllResults,
                        out _numTotalMatches, out _numNewMatches);

                    // Raise the MatchesFound event
                    OnMatchesFound(new MatchesFoundEventArgs(_matchResults.AsReadOnly()));
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
        void CountFindResults(MatchType type, bool removeDuplicates, out int totalCount, 
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
                    bool isDuplicate = _helper.IsDuplicate(_findResults[i].MatchResult);
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
        void DisplayNextMatch()
        {
            // Clear any current match
            ClearCurrentMatch();

            // Ensure there is a find to display
            if (_findResults != null && _nextFindResult < _findResults.Count)
            {
                // Get the next find result to display (if clues are hidden skip clues)
                FindResult findResult = _findResults[_nextFindResult++];
                while (!ShowClues
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
                    _imageViewer.LayerObjects.Add(findResult.CompositeMatch);

                    // Center the image viewer around the first composite highlight
                    // in the match results
                    _imageViewer.CenterOnLayerObjects(findResult.CompositeMatch);
                }
            }

            // Invalidate the image viewer to ensure the composite highlight is displayed
            _imageViewer.Invalidate();
        }

        /// <summary>
        /// Updates the enabled/disabled state of the buttons on the
        /// <see cref="RuleForm"/>.
        /// </summary>
        void UpdateButtonStates()
        {
            // Find next is enabled if all of the following are true:
            // 1) There is an open image
            // 2) There is a valid property page 
            // 3) There are more matches to display
            _findNextButton.Enabled = _imageViewer.IsImageAvailable &&
                                      (_propertyPage == null || _propertyPage.IsValid) &&
                                      (_findResults == null || _nextFindResult < _findResults.Count);

            // If clues are not visible, also need to check that there is at least one
            // remaining find result that is not a clue
            // [IDSD:288] Added check to disable redact button when the current find result is a clue.
            if (_findNextButton.Enabled && !ShowClues && _findResults != null)
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
            _redactButton.Enabled = _findResults != null && !_showingRedactAllResults 
                && _currentMatchType == MatchType.Match && !_haveRedactedLastFindResult;

            // Redact all is enabled if an image is open and there is a valid property page
            _redactAllButton.Enabled = _imageViewer.IsImageAvailable && 
                                       (_propertyPage == null || _propertyPage.IsValid);

            // Reset is enabled if a search has begun (_findResults != null)
            _resetButton.Enabled = _findResults != null;
        }

        /// <summary>
        /// Updates the rule results table.
        /// </summary>
        void UpdateResults()
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

                if (_rule.UsesClues && ShowClues)
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
        void UpdateStatusBar()
        {
            if (_finding)
            {
                // If a search is in progress, indicate it in the status bar.
                _toolStripStatusLabel.Text = "Searching...";
                return;
            }

            if (_findResults == null)
            {
                // If no results are available, add the table title and nothing else.
                _toolStripStatusLabel.Text = "";
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

            _toolStripStatusLabel.Text = status.ToString();
        }

        /// <summary>
        /// Will split the list of <see cref="MatchResult"/> objects into individual
        /// <see cref="FindResult"/> pieces on separate pages.
        /// </summary>
        /// <param name="matchResults">The list of match results to split.</param>
        /// <returns>A list of <see cref="FindResult"/> objects.</returns>
        List<FindResult> SplitMatchResultsByPage(IEnumerable<MatchResult> matchResults)
        {
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
                        new CompositeHighlightLayerObject(_imageViewer, pair.Key,
                            new string[] { SearchResultTag }, pair.Value, _FIND_COLOR);
                    compositeHighlight.Selectable = false;
                    compositeHighlight.CanRender = false;
                    compositeHighlight.OutlineColor = _FIND_COLOR;

                    // Add the find result to the collection
                    findResults.Add(new FindResult(compositeHighlight, matchResult));
                }
            }

            // Sort the collection of find results
            findResults.Sort();

            // Return the list of find results
            return findResults;
        }

        #endregion Methods
    }
}