using Extract;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Rules;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Leadtools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace IDShieldOffice
{
    /// <summary>
    /// The main form for the ID Shield Office application.
    /// </summary>
    public partial class IDShieldOfficeForm : Form, IRuleFormHelper
    {
        #region Fields

        /// <summary>
        /// Boolean flag to indicate whether the objectPropertiesGrid window is visible.
        /// </summary>
        bool _objectPropertiesGridWindowVisible;

        /// <summary>
        /// Boolean flag to indicate whether the layers window is visible.
        /// </summary>
        bool _layersWindowVisible;

        /// <summary>
        /// The manager object for performing OCR on the opened image files.
        /// </summary>
        AsynchronousOcrManager _ocrManager;

        /// <summary>
        /// Stores the last exception object from the OCR manager.
        /// </summary>
        ExtractException _lastOcrException;

        /// <summary>
        /// The "Save As" file dialog.
        /// </summary>
        SaveFileDialog _saveAsDialog;

        /// <summary>
        /// The form for finding and redacting bracketed text.
        /// </summary>
        RuleForm _bracketedTextRuleForm;

        /// <summary>
        /// The form for finding and redacting a word/pattern list.
        /// </summary>
        RuleForm _wordOrPatternListRuleForm;

        /// <summary>
        /// The form for finding and redacting specific data types.
        /// </summary>
        RuleForm _dataTypeRuleForm;

        /// <summary>
        /// The user-specified settings for the ID Shield Office application.
        /// </summary>
        readonly UserPreferences _userPreferences;

        /// <summary>
        /// The dialog for setting user preferences.
        /// </summary>
        PropertyPageForm _userPreferencesDialog;

        /// <summary>
        /// The history of all modifications to the currently open document.
        /// </summary>
        readonly ModificationHistory _modifications;

        /// <summary>
        /// Whether the currently open file has been saved or not.
        /// <see langword="true"/> if the file has been modified since the last save;
        /// <see langword="false"/> if the file has not been modified since the last save.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Indicates that the current open file is a temporary file.
        /// </summary>
        bool _tempFile;

        /// <summary>
        /// The path where the current file will be saved to.
        /// </summary>
        string _outputFilePath;

        /// <summary>
        /// The output format of the last save as operation.
        /// </summary>
        OutputFormat _outputFormat;

        /// <summary>
        /// Indicates whether the ID Shield Office form has been loaded.
        /// <see langword="true"/> if the form has been loaded;
        /// <see langword="false"/> if the form has not been loaded;
        /// </summary>
        bool _isLoaded;

        /// <summary>
        /// Indicates whether ID Shield Office should reset the form and tool strip locations.
        /// </summary>
        bool _resetForm;

        /// <summary>
        /// File to be opened when the IDSO form has loaded.
        /// </summary>
        readonly string _fileToOpen;

        #endregion Fields

        #region Constants

        /// <summary>
        /// The main string displayed in the title bar of the ID Shield office application.
        /// </summary>
        const string _IDSHIELD_OFFICE_TITLE = "ID Shield Office";

        /// <summary>
        /// The percentage of overlap tolerance for adding objects.
        /// </summary>
        const double _OBJECT_OVERLAP_TOLERANCE = 0.95;

        /// <summary>
        /// The format string for displaying the OCR progress before it is complete.
        /// </summary>
        const string _OCR_PROGRESS_FORMAT = "OCR {0:0%}";

        /// <summary>
        /// The string to display when the OCR has completed.
        /// </summary>
        const string _OCR_PROGRESS_COMPLETE = "OCR Complete!";

        /// <summary>
        /// The string to display when an OCR error has occurred.
        /// </summary>
        const string _OCR_PROGRESS_ERROR = "OCR Error Occurred!";

        /// <summary>
        /// The string to display when an OCR event has been canceled.
        /// </summary>
        const string _OCR_PROGRESS_CANCELED = "OCR Canceled!";

        /// <summary>
        /// Value indicating that an error has occurred in OCR so the ocrProgressStatusLabel
        /// should display the error string.
        /// </summary>
        const double _OCR_DISPLAY_PROGRESS_ERROR = -42.0;

        /// <summary>
        /// Value indicating that the OCR status text should no longer be displayed.
        /// </summary>
        const double _OCR_DISPLAY_PROGRESS_NONE = -43.0;

        /// <summary>
        /// The filter string containing the file formats that ID Shield Office supports saving
        /// </summary>
        const string _OUTPUT_FORMAT_FILTER =
            "TIFF files (*.tif)|*.tif|" +
            "PDF files (*.pdf)|*.pdf|" +
            "IDSO files (*.idso)|*.idso||";

        /// <summary>
        /// The filter string containing the file formats that ID Shield Office supports saving
        /// for temporary files.
        /// </summary>
        const string _TEMPFILE_OUTPUT_FORMAT_FILTER =
            "TIFF files (*.tif)|*.tif|" +
            "PDF files (*.pdf)|*.pdf||";

        /// <summary>
        /// The extensions that ID Shield Office supports saving indexed by the output format 
        /// filter index.
        /// </summary>
        static readonly string[] _OUTPUT_FORMAT_EXTENSION = 
            new string[] { ".tif", ".pdf", ".idso" };

        /// <summary>
        /// The water mark to place on an image when using an evaluation license.
        /// </summary>
        static readonly string _TRIAL_WATERMARK_TEXT =
            "CREATED USING EVALUATION" + Environment.NewLine + "COPY OF ID SHIELD OFFICE";

        /// <summary>
        /// The url to the ID Shield Office help file.
        /// </summary>
        public static readonly string HelpFileUrl = 
            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"..\Help\IDShieldOffice.chm");

        /// <summary>
        /// The index to the printing section of the ID Shield Office help file
        /// </summary>
        const string _PRINTING_HELP_INDEX = "temporary images";

        /// <summary>
        /// The keyword for the help sections that contains examples of document tags.
        /// </summary>
        const string _TAGS_EXAMPLE_HELP_KEYWORD = "Examples_of_Output_File_Settings.htm";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(IDShieldOfficeForm).ToString();

        /// <summary>
        /// An array of <see cref="Boolean"/> values indexed by zero-based page number indicating 
        /// whether that page has been visited during this document's session.
        /// </summary>
        BitArray _pagesVisited;
        
        #endregion Constants

        #region Delegates

        /// <summary>
        /// Delegate for a function that takes a single <see cref="double"/> as a paramater.
        /// </summary>
        /// <param name="value">The parameter for the delegate method.</param>
        delegate void DoubleParameterDelegate(double value);

        #endregion Delegates

        #region Constructors

        /// <overloads>Initializes a new instance of the <see cref="IDShieldOfficeForm"/> 
        /// class.</overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="IDShieldOfficeForm"/> class.
        /// </summary>
        public IDShieldOfficeForm()
            : this(null, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IDShieldOfficeForm"/> class opened 
        /// with the specified image file.
        /// </summary>
        /// <param name="fileName">The image file or ID Shield Office data file to open.
        /// <see langword="null"/> if no file should be opened.</param>
        public IDShieldOfficeForm(string fileName)
            : this(fileName, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IDShieldOfficeForm"/> class opened 
        /// with the specified image file.
        /// </summary>
        /// <param name="fileName">The image file or ID Shield Office data file to open.
        /// <see langword="null"/> if no file should be opened.</param>
        /// <param name="tempFile">If <see langword="true"/> the file is a temporary file and 
        /// should be deleted when the image file changes and also should not be added to the MRU 
        /// list. If <see langword="false"/> then the file will not be deleted and will be added 
        /// to the MRU list.</param>
        public IDShieldOfficeForm(string fileName, bool tempFile)
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                InitializeComponent();

                // Validate the IDSO license
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23065",
                    _OBJECT_NAME);

                // Check if IDSO is in trial mode
                if (LicenseUtilities.IsTemporaryLicense(LicenseIdName.IdShieldOfficeObject))
                {
                    // Set the watermark
                    _imageViewer.Watermark = _TRIAL_WATERMARK_TEXT;
                }

                // Read the user preferences object from the registry
                _userPreferences = UserPreferences.FromRegistry(_imageViewer);

                // Create the OCR manager
                _ocrManager = new AsynchronousOcrManager(_userPreferences.OcrTradeoff);

                // Register for OcrManager events
                _ocrManager.OcrProgressUpdate += HandleOcrProgressUpdate;
                _ocrManager.OcrError += HandleOcrError;

                // Register for layer objects collection events
                _imageViewer.LayerObjects.LayerObjectVisibilityChanged += HandleLayerObjectsVisibilityChanged;
                _imageViewer.LayerObjects.LayerObjectChanged += HandleLayerObjectChanged;
                _imageViewer.LayerObjects.LayerObjectDeleted += HandleLayerObjectDeleted;
                _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;
                _imageViewer.LayerObjects.DeletingLayerObjects += HandleDeletingLayerObjects;
                _imageViewer.LayerObjects.Selection.LayerObjectAdded += HandleSelectionLayerObjectAdded;
                _imageViewer.LayerObjects.Selection.LayerObjectDeleted += HandleSelectionLayerObjectDeleted;

                // Add IDSO to the image file type list for the image viewer
                _imageViewer.OpenImageFileTypeFilterList.Insert(0, "IDSO files (*.idso)|*.idso|");
                _imageViewer.OpenImageFileTypeFilterIndex++;

                // Add ID Shield printer to the disallowed printers list
                _imageViewer.AddDisallowedPrinter("ID Shield");

                // Instantiate the modifications history
                _modifications = new ModificationHistory(_imageViewer);

                // Add the modification history loaded event handler
                _modifications.ModificationHistoryLoaded += HandleModificationHistoryLoaded;

                // If the user has a temporary license, set the default user-action status message 
                // to indicate the number of days left in the user's evaluation.
                if (LicenseUtilities.IsTemporaryLicense(LicenseIdName.IdShieldOfficeObject))
                {
                    // If no other status needs to be displayed and an evaluation license is being
                    // used, indicate the number of days remaining in the evaluation.
                    DateTime date =
                        LicenseUtilities.GetExpirationDate(LicenseIdName.IdShieldOfficeObject);

                    // Calculate the days remaining in the evaluation.
                    int daysLeft = (date - DateTime.Now).Days;

                    // Build time remaining in the evaluation for the label
                    StringBuilder sb = new StringBuilder("Evaluation expires ");
                    sb.Append(daysLeft == 0 ? "today." :
                        "in " + daysLeft.ToString(CultureInfo.CurrentCulture) + " day(s).");
                    _imageViewer.DefaultStatusMessage = sb.ToString();
                }

                // Get the initial file to open
                _fileToOpen = fileName;

                // Set the temp file flag
                _tempFile = tempFile;           
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI21977",
                    "Failed to initialize ID Shield Office form!", ex);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Toggles the display of the object properties grid window.
        /// </summary>
        void ToggleObjectPropertiesGridDockableWindow()
        {
            // Set the object properties pane visibility to its opposite value
            SetObjectPropertiesGridDockableWindowVisible(!_objectPropertiesGridWindowVisible);
        }

        /// <summary>
        /// Shows/hides the object properties pane and updates related controls accordingly.
        /// <param name="makeVisible"><see langword="true"/> makes the oject properties pane visible;
        /// <see langword="false"/> hides the object properties pane.</param>
        /// </summary>
        void SetObjectPropertiesGridDockableWindowVisible(bool makeVisible)
        {
            if (makeVisible == _objectPropertiesGridWindowVisible)
            {
                // If the current visibility matches makeVisible, there's nothing to do; return. 
                return;
            }
            else
            {
                _objectPropertiesGridWindowVisible = makeVisible;
            }

            // If now true, show the window, otherwise close it.
            if (_objectPropertiesGridWindowVisible)
            {
                _objectPropertyGridDockableWindow.Open();
            }
            else
            {
                _objectPropertyGridDockableWindow.Close();
            }

            // Set the checked state of the menu item and button
            _showObjectPropertiesGridWindowToolStripMenuItem.Checked =
                _objectPropertiesGridWindowVisible;
            _showObjectPropertyGridWindowToolStripButton.Checked =
                _objectPropertiesGridWindowVisible;

            // Store the current setting in the registry
            RegistryManager.PropertyGridVisible = _objectPropertiesGridWindowVisible;
        }

        /// <summary>
        /// Toggles the display of the layers window.
        /// </summary>
        void ToggleLayersDockableWindow()
        {
            // Change the layers pane visibility to its opposite value
            SetLayersDockableWindowVisible(!_layersWindowVisible);
        }

        /// <summary>
        /// Shows/hides the layer selection pane and updates related controls accordingly.
        /// <param name="makeVisible"><see langword="true"/> makes the layer selection pane visible;
        /// <see langword="false"/> hides the layer selection pane.</param>
        /// </summary>
        void SetLayersDockableWindowVisible(bool makeVisible)
        {
            if (makeVisible == _layersWindowVisible)
            {
                // If the current visibility matches makeVisible, there's nothing to do; return.
                return;
            }
            else
            {
                _layersWindowVisible = makeVisible;
            }

            // If now true, show the window, otherwise close it
            if (_layersWindowVisible)
            {
                _layersDockableWindow.Open();
            }
            else
            {
                _layersDockableWindow.Close();
            }

            // Set the checked state of the menu item and button
            _showLayersWindowToolStripMenuItem.Checked = _layersWindowVisible;
            _showLayersWindowToolStripButton.Checked = _layersWindowVisible;

            // Store the current setting in the registry
            RegistryManager.ShowLayerObjectsVisible = _layersWindowVisible;
        }

        /// <summary>
        /// Private method for handling updating of the OCR Progress status
        /// bar item.  Handles the case of updating the UI from a separate
        /// thread by checking whether invoke is required and if it is, calling
        /// UpdateProgressStatus via a
        /// <see cref="Control.BeginInvoke(System.Delegate, object[])"/> call.
        /// </summary>
        /// <param name="progress">A <see cref="double"/> containing the
        /// current OCR progress percentage.</param>
        void UpdateProgressStatus(double progress)
        {
            // If not running in the UI thread than an invoke is required
            if (InvokeRequired)
            {
                // Call UpdateProgressStatus via BeginInvoke (Asynchronous call)
                BeginInvoke(new DoubleParameterDelegate(UpdateProgressStatus),
                    new object[] {progress});

                // Just return as the BeginInvoke will take care of updating the UI
                return;
            }

            // Invoke not required, we are running in the UI thread, it is now safe
            // to update the OCR progress status label

            // If progress == _OCR_DISPLAY_PROGRESS_ERROR, set the error label
            if (progress == _OCR_DISPLAY_PROGRESS_ERROR)
            {
                // Set the text to error and change the back color to red
                _ocrProgressStatusLabel.Text = _OCR_PROGRESS_ERROR;
                _ocrProgressStatusLabel.ForeColor = Color.Crimson;
                _ocrProgressStatusLabel.ToolTipText = _lastOcrException != null ?
                    _lastOcrException.Message : "See error log";
            }
            else if (progress == _OCR_DISPLAY_PROGRESS_NONE)
            {
                // Clear the OCR status text
                _ocrProgressStatusLabel.Text = null;
                _ocrProgressStatusLabel.ToolTipText = null;
            }
            else if (progress == AsynchronousOcrManager.OcrCanceledProgressStatusValue)
            {
                if (_imageViewer.IsImageAvailable)
                {
                    // Only display the cancel message if an image is available.
                    _ocrProgressStatusLabel.Text = _OCR_PROGRESS_CANCELED;
                    _ocrProgressStatusLabel.ForeColor = Color.Blue;
                }
                else
                {
                    // If there is no image available, clear the status bar text.
                    _ocrProgressStatusLabel.Text = null;
                    _ocrProgressStatusLabel.ToolTipText = null;
                }
            }
            else
            {
                // Reset the tool tip text
                _ocrProgressStatusLabel.ToolTipText = null;

                // Set the progress status to 100%
                bool complete = progress == 1.0;

                // If ocr is finished, display complete, otherwise display the percentage
                _ocrProgressStatusLabel.Text = complete ? _OCR_PROGRESS_COMPLETE :
                    string.Format(CultureInfo.CurrentCulture, _OCR_PROGRESS_FORMAT, progress);

                // Set the back color back to normal
                _ocrProgressStatusLabel.ForeColor = complete ? Color.Green :
                    _userActionToolStripStatusLabel.ForeColor;
            }
        }

        /// <summary>
        /// Adds a collection of clues to the currently open document as long as the clue
        /// does not overlap another existing clue.
        /// </summary>
        /// <param name="matches">The collection of <see cref="MatchResult"/> objects
        /// that contains the clues to add.
        /// <para><b>Note:</b></para>
        /// Any match result with a <see cref="MatchType"/> of Match will be ignored.</param>
        internal void AddClues(IEnumerable<MatchResult> matches)
        {
            foreach (MatchResult match in matches)
            {
                // Only add the clues
                if (match.MatchType == MatchType.Match)
                {
                    continue;
                }

                // Get a list of clues that haven't already been added to this image.
                List<Clue> cluesToAdd = CreateIfNotDuplicate<Clue>(match.RasterZones, match.FindingRule);

                // If at least one of the clue pieces did not overlap, then add the clues
                if (cluesToAdd != null)
                {
                    // Add and link objects if needed
                    if (cluesToAdd.Count > 1)
                    {
                        for (int i = cluesToAdd.Count - 1; i > 0; i--)
                        {
                            // Set the text of this clue
                            cluesToAdd[i].Text = match.Text;

                            _imageViewer.LayerObjects.Add(cluesToAdd[i]);
                            cluesToAdd[i].AddLink(cluesToAdd[i - 1]);
                        }
                    }

                    // Set the text of the first clue
                    cluesToAdd[0].Text = match.Text;

                    // This fires the image viewer LayerObjectVisibilityChangedEvent to ensure
                    // the clues layer is turned on if need be.
                    cluesToAdd[0].Visible = true;

                    // Add the first object
                    _imageViewer.LayerObjects.Add(cluesToAdd[0]);
                }
            }
        }

        /// <summary>
        /// Adds a new Redaction to the currently open document as long as the new
        /// redaction does not overlap an existing redaction.
        /// </summary>
        /// <param name="rasterZones">The collection of
        /// <see cref="Extract.Imaging.RasterZone"/>
        /// objects that make up this redaction.</param>
        /// <param name="comment">The comment associated with the redaction.</param>
        internal void AddRedaction(IEnumerable<RasterZone> rasterZones, string comment)
        {
            // Get a list of clues that haven't already been added to this image.
            List<Redaction> redactionsToAdd = CreateIfNotDuplicate<Redaction>(rasterZones, comment);

            // If at least one of the redaction pieces did not overlap, then add the redactions.
            if (redactionsToAdd != null)
            {
                // Add the redactions to the image viewer
                foreach (Redaction redaction in redactionsToAdd)
                {
                    _imageViewer.LayerObjects.Add(redaction);
                }

                // Link objects if needed
                if (redactionsToAdd.Count > 1)
                {
                    for (int i = redactionsToAdd.Count - 1; i > 0; i--)
                    {
                        redactionsToAdd[i].AddLink(redactionsToAdd[i - 1]);
                    }
                }
            }
        }

        /// <summary>
        /// Creates Redactions or Clues for all specified raster zones that do not duplicate any
        /// existing Clue or Redaction.
        /// </summary>
        /// <typeparam name="T">The type of object to create.  Must be of type <see cref="Clue"/> or
        /// <see cref="Redaction"/>.</typeparam>
        /// <param name="rasterZones">The set of RasterZones for which clues or
        /// redactions should be tested for duplicates and added.</param>
        /// <param name="comment">The comment to be associated with the clues or redactions.</param>
        /// <returns><see langword="null"/> if no unique clue or redaction could be created. 
        /// Otherwise a list of non-overlapping clues or redactions is returned.</returns>
        internal List<T> CreateIfNotDuplicate<T>(IEnumerable<RasterZone> rasterZones, string comment) 
            where T : CompositeHighlightLayerObject
        {
            // Default overlap to true.
            bool allObjectsOverlapExisting = true;

            // The list used to store non-overlapping objects 
            List<T> objectsToAdd = new List<T>();

            // Add the new object (need to split the object into zones on different pages first).
            foreach (KeyValuePair<int, List<RasterZone>> pair in RasterZone.SplitZonesByPage(rasterZones))
            {
                T objectToAdd = null;

                if (typeof(T) == typeof(Clue))
                {
                    // Create a new clue.
                    Clue clueToAdd = new Clue(_imageViewer, pair.Key, comment, pair.Value);
                    objectToAdd = clueToAdd as T;
                }
                else if (typeof(T) == typeof(Redaction))
                {
                    // Create a new redaction.
                    Redaction redactionToAdd = new Redaction(_imageViewer, pair.Key, comment,
                        pair.Value, _imageViewer.DefaultRedactionFillColor);
                    objectToAdd = redactionToAdd as T;
                }
                
                ExtractException.Assert("ELI23310", "Failed to create object!", objectToAdd != null);

                // Add the object to the list of objects
                objectsToAdd.Add(objectToAdd);

                // Need to keep checking overlap as long as all the objects are overlapping
                // if one page of the object has no overlap then the new object is not a duplicate
                if (allObjectsOverlapExisting)
                {
                    // Build a list of all raster zones for existing objects of the same type.
                    List<RasterZone> existingZones = new List<RasterZone>();
                    foreach (T existingObject in _imageViewer.GetLayeredObjectsOnPage(
                        pair.Key, null, ArgumentRequirement.Any, new Type[] { typeof(T) },
                        ArgumentRequirement.All, null, ArgumentRequirement.All))
                    {
                        existingZones.AddRange(existingObject.GetRasterZones());
                    }

                    // Compute the area of overlap
                    double areaOfOverlap = objectToAdd.GetAreaOverlappingWith(existingZones);

                    // Check for overlap
                    allObjectsOverlapExisting =
                        (areaOfOverlap / objectToAdd.Area() > _OBJECT_OVERLAP_TOLERANCE);
                }
            }

            return allObjectsOverlapExisting ? null : objectsToAdd;
        }

        /// <summary>
        /// Updates the button and menu item enabled states for the IDShieldOffice Form
        /// </summary>
        void UpdateButtonAndMenuItemState()
        {
            // Get isImageAvailable
            bool isImageAvailable = _imageViewer.IsImageAvailable;

            // Update redact page menu item state
            _redactEntirePageToolStripMenuItem.Enabled = isImageAvailable;

            // Update the file save menu item and button state
            // Enabled if:
            // 1. Image is available
            // 2. Either output format is not IDSO or file is not a temporary file
            bool canSave = isImageAvailable
                && (_userPreferences.OutputFormat != OutputFormat.Idso || !_tempFile);
            _saveToolStripButton.Enabled = canSave;
            _saveToolStripMenuItem.Enabled = canSave;

            // Update the save as file state
            _saveAsToolStripMenuItem.Enabled = isImageAvailable;

            // Update the file properties menu item state
            _propertiesToolStripMenuItem.Enabled = isImageAvailable;

            // Update the page setup menu item state
            _pageSetupToolStripMenuItem.Enabled = isImageAvailable;

            UpdateBatesNumberControls(false);
        }

        /// <summary>
        /// Updates the bates number related controls to reflect the current document state.
        /// <param name="turnBatesLayerOn"> <see langword="true"/> forces the bates number
        /// layer to be turned on as part of this call.</param>
        /// </summary>
        void UpdateBatesNumberControls(bool turnBatesLayerOn)
        {
            // Update the apply bates number menu item and button state
            // 1. Image must be available
            // 2. Must not contain Bates numbers already
            bool canApplyBates = _imageViewer.IsImageAvailable
                && _imageViewer.GetLayeredObjects(
                new string[] { BatesNumberManager._BATES_NUMBER_TAG }, ArgumentRequirement.All,
                new Type[] { typeof(TextLayerObject) }, ArgumentRequirement.All,
                null, ArgumentRequirement.All).Count == 0;
            _applyBatesNumberToolStripButton.Enabled = canApplyBates;
            _applyBatesNumberToolStripMenuItem.Enabled = canApplyBates;

            if (turnBatesLayerOn && !_showBatesNumberCheckBox.Checked)
            {
                _showBatesNumberCheckBox.Checked = true;
            }
        }

        /// <summary>
        /// Saves the current image and layer objects to the last output location, or else to a 
        /// location specified by the user.
        /// </summary>
        void SelectSave()
        {
            if (_imageViewer.IsImageAvailable)
            {
                Save(false);
            }
        }

        /// <summary>
        /// Saves the current image and layer objects to the specified output location
        /// in the specified format.
        /// <param name="showSaveAsPrompt"><see langword="true"/> to prompt user for where to save 
        /// the file. <see langword="false"/> results in saving without prompt if possible.</param>
        /// <returns> <see langword="true"/> if the save was attempted. <see langword="false"/> if
        /// the user elected to cancel the operation during a prompt before saving.</returns>
        /// </summary>
        bool Save(bool showSaveAsPrompt)
        {
            try
            {
                // Default the return value to true.
                bool result = true;

                // Prompt the user if necessary before saving
                if (!PromptBeforeOutput())
                {
                    // User cancelled-- return immediately without saving.
                    return false;
                }

                // Save the image file
                if (showSaveAsPrompt || _outputFilePath == null)
                {
                    // User can cancel from SaveAs prompt, thus false could be returned here.
                    result = SaveAs();
                }
                else
                {
                    SaveImage(_outputFilePath, _outputFormat);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI23246", "Failed to save document!", ex);
            }
        }

        /// <summary>
        /// Prompts the user to select a location to save the current file to.  This is a help 
        /// function to Save() and should not be called directly in most cases.
        /// <returns><see langword="true"/> if the save was executed; <see langword="false"/> if 
        /// the user cancelled the operation.</returns>
        /// </summary>
        bool SaveAs()
        {
            try
            {
                // Create the file dialog if not already created
                if (_saveAsDialog == null)
                {
                    _saveAsDialog = new SaveFileDialog();
                    _saveAsDialog.Title = "Save document";
                    _saveAsDialog.OverwritePrompt = false;
                    _saveAsDialog.FileOk += HandleFileOk;
                }

                // Set the output filter based on whether it is a temp file or not
                _saveAsDialog.Filter = _tempFile ? _TEMPFILE_OUTPUT_FORMAT_FILTER : _OUTPUT_FORMAT_FILTER;

                // Set the default file path and format
                OutputFormat format;
                if (!string.IsNullOrEmpty(_outputFilePath))
                {
                    _saveAsDialog.FileName = _outputFilePath;
                    _saveAsDialog.InitialDirectory = Path.GetDirectoryName(_outputFilePath);
                    format = _outputFormat;
                }
                else
                {
                    // Check for temp file
                    if (_tempFile)
                    {
                        // Set the initial file name to empty string and path to MyDocuments
                        _saveAsDialog.FileName = "";
                        _saveAsDialog.InitialDirectory = Environment.GetFolderPath(
                            Environment.SpecialFolder.MyDocuments);
                        format = _userPreferences.OutputFormat != OutputFormat.Idso ?
                            _userPreferences.OutputFormat : OutputFormat.Tif;
                    }
                    else
                    {
                        // Get the output path
                        string outputPath = _userPreferences.GetFullOutputPath();
                        if (string.IsNullOrEmpty(outputPath))
                        {
                            // Set the initial file name to the source document's name and
                            // let the OS handle the initial directory. [IDSO #333]
                            _saveAsDialog.FileName = 
                                Path.GetFileNameWithoutExtension(_imageViewer.ImageFile) +
                                _OUTPUT_FORMAT_EXTENSION[(int)_userPreferences.OutputFormat];
                        }
                        else
                        {
                            // Use the output path for file name and directory
                            _saveAsDialog.FileName = outputPath;
                            _saveAsDialog.InitialDirectory = Path.GetDirectoryName(outputPath);
                        }

                        format = _userPreferences.OutputFormat;
                    }
                }

                switch (format)
                {
                    case OutputFormat.Tif:
                        _saveAsDialog.FilterIndex = 1;
                        break;

                    case OutputFormat.Pdf:
                        _saveAsDialog.FilterIndex = 2;
                        break;

                    case OutputFormat.Idso:
                        _saveAsDialog.FilterIndex = 3;
                        break;

                    default:
                        ExtractException.ThrowLogicException("ELI23230");
                        break;
                }

                do
                {
                    // Show the dialog and bail if the user selected cancel
                    if (_saveAsDialog.ShowDialog() == DialogResult.Cancel)
                    {
                        return false;
                    }
                    else
                    {
                        // Check if saving to the currently open file
                        if (_saveAsDialog.FileName.Equals(
                            _imageViewer.ImageFile, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("Cannot save to the currently open file."
                                + Environment.NewLine + Environment.NewLine
                                + "Please choose a different file to save to.",
                                "Cannot Overwrite Open File", MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                        }
                        else
                        {
                            // File name is okay, break from the loop
                            break;
                        }
                    }
                }
                while (true);

                // Check what output format was selected
                string extension = GetSelectedOutputExtension();
                if (extension == ".idso")
                {
                    // Store the output path for the IDSO file
                    _outputFilePath = _imageViewer.ImageFile + ".idso";
                    _outputFormat = OutputFormat.Idso;
                }
                else
                {
                    // Store the output file name and format
                    _outputFilePath = _saveAsDialog.FileName;
                    _outputFormat = extension == ".tif" ?
                        OutputFormat.Tif : OutputFormat.Pdf;
                }

                // Save the image
                SaveImage(_outputFilePath, _outputFormat);

                return true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23232", ex);
            }
        }

        /// <summary>
        /// Saves the image to the specified location in the specified file format.
        /// </summary>
        /// <param name="fileName">The full path where the file should be saved.</param>
        /// <param name="format">The file format in which the file should be saved.</param>
        internal void SaveImage(string fileName, OutputFormat format)
        {
            // Refresh the form
            Refresh();

            if (format == OutputFormat.Idso)
            {
                _modifications.Save();
            }
            else
            {
                // Save the image, applying evaluation license watermark if necessary
                _imageViewer.SaveImage(fileName, format == OutputFormat.Tif ?
                    RasterImageFormat.CcittGroup4 : RasterImageFormat.RasPdfG4);

                // Save the idso if necessary
                if (_userPreferences.SaveIdsoWithImage && !_tempFile)
                {
                    _modifications.Save();
                }
            }

            // Reset the dirty flag
            Dirty = false;
        }

        /// <summary>
        /// Shows the IDSO help file, opened to the Tags section.
        /// </summary>
        /// <param name="parent">The parent of the Help dialog box.</param>
        public static void ShowDocTagsHelp(Control parent)
        {
            Help.ShowHelp(parent, HelpFileUrl, _TAGS_EXAMPLE_HELP_KEYWORD);
        }

        /// <summary>
        /// Prompts the user that the current file is has not been saved and gives them the
        /// option to save the file, not save and continue, or cancel.
        /// </summary>
        /// <returns>A <see cref="DialogResult"/> containing the result of the message box.</returns>
        DialogResult PromptForDirtyFile()
        {
            return MessageBox.Show(this, "File has not been saved, would you like to save now?",
                "File Not Saved", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0);
        }

        /// <summary>
        /// Prompts the user as necessary before saving or printing.
        /// </summary>
        /// <returns><see langword="true"/> if the document should be saved or printed; 
        /// <see langword="false"/> if the document should not be saved or printed.</returns>
        bool PromptBeforeOutput()
        {
            // Prompt as necessary based on:
            // 1) The "verify all pages" option
            // 2) The "require Bates numbers" option
            // 3) Whether any layer objects are partially off the page limits
            return PromptForVerifiedAllPages() &&
                _userPreferences.BatesNumberManager.PromptForRequiredBatesNumber() &&
                PromptForOffPageLayerObjects();
        }

        /// <summary>
        /// Prompts the user if all pages haven't been verified and corresponding option is set.
        /// </summary>
        /// <returns><see langword="true"/> if at least one page has not been verified and the 
        /// document is dirty; <see langword="false"/> if all the pages have been verified or the 
        /// document is not dirty.</returns>
        bool PromptForVerifiedAllPages()
        {
            // No need to prompt if:
            // 1) The option isn't set
            // 2) The document isn't dirty
            if (!_userPreferences.VerifyAllPages || !_dirty)
            {
                return true;
            }

            // If all pages were visited, we are done
            int firstUnvisitedPage = GetFirstUnvisitedPage();
            if (firstUnvisitedPage < 1)
            {
                return true;
            }

            // Display the prompt
            string message =
                "ID Shield Office has been configured to require all pages of a document to be "
                + "visited before any redacted output can be produced."
                + Environment.NewLine + Environment.NewLine
                + "You have not visited all pages of this document. Are you sure you want to continue?"
                + Environment.NewLine + Environment.NewLine
                + "Click Yes to continue." + Environment.NewLine
                + "Click No to return to the document. ID Shield Office will take you to the first "
                + "unvisited page on the document.";

            DialogResult result = MessageBox.Show(message, 
                "Continue without visiting all pages?", MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 0);

            // Check if the user's response was no
            if (result == DialogResult.No)
            {
                // Go to the first unvisited page of the document.
                _imageViewer.PageNumber = firstUnvisitedPage;
            }

            // Return the user's response
            return result == DialogResult.Yes;
        }

        /// <summary>
        /// Retrieves the first unvisited page.
        /// </summary>
        /// <returns>The one-based page number of the first unvisited page, or -1 if all the pages 
        /// have been visited.</returns>
        int GetFirstUnvisitedPage()
        {
            for (int i = 0; i < _pagesVisited.Length; i++)
            {
                // Check if this page wasn't visited
                if (!_pagesVisited[i])
                {
                    // Return this page number (one-based)
                    return i + 1;
                }
            }

            // If this point was reached, all the pages have been visited
            return -1;
        }

        /// <summary>
        /// Prompts the user if any movable layer objects are partially off the page.
        /// </summary>
        /// <returns><see langword="true"/> if all movable layer objects are fully on the page or 
        /// if the user chose to continue with layer objects off the page; <see langword="false"/> 
        /// if there are layer objects partially off the page and the user chose to cancel.
        /// </returns>
        bool PromptForOffPageLayerObjects()
        {
            // Check if any layer object is not fully on the page
            LayerObject offPageObject = GetFirstOffPageMovableObject();

            // If no layer objects are off the page, we are done.
            if (offPageObject == null)
            {
                return true;
            }

            // Prepare the message text
            string text = 
                "At least one redaction or Bates number is outside the visible page limits. "
                + "Are you sure you want to continue?"
                + Environment.NewLine + Environment.NewLine
                + "Click Yes to continue."
                + Environment.NewLine
                + "Click No to be taken to the first object outside page limits.";

            // Prompt the user
            DialogResult result = MessageBox.Show(text, "Continue with partially visible layer objects?", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 0);

            // Center on the layer object if necessary
            if (result == DialogResult.No)
            {
                _imageViewer.CenterOnLayerObjects(offPageObject);
            }

            // Return the user's response
            return result == DialogResult.Yes;
        }

        /// <summary>
        /// Returns the first movable layer object that is partially off the page limits.
        /// </summary>
        /// <returns>The first movable layer object that is partially off the page limits.
        /// </returns>
        LayerObject GetFirstOffPageMovableObject()
        {
            // If there are no layer objects this is easy.
            if (_imageViewer.LayerObjects.Count == 0)
            {
                return null;
            }

            // Iterate through the sorted collection
            foreach (LayerObject layerObject in _imageViewer.LayerObjects.GetSortedCollection())
            {
                // If this object isn't movable, skip it.
                if (!layerObject.Movable)
                {
                    continue;
                }


                ImagePageProperties properties = 
                    _imageViewer.GetPageProperties(layerObject.PageNumber);

                // Check if the layer object is off the page at all
                if (!properties.Contains(layerObject.GetBounds()))
                {
                    // This layer object was off the page
                    return layerObject;
                }
            }

            // If this point was reached, no layer object is off the page
            return null;
        }

        /// <summary>
        /// Updates the form caption with the currently opened image and adds a "*" if
        /// the currently opened image is dirty.
        /// </summary>
        void UpdateCaption()
        {
            StringBuilder sb = new StringBuilder(_IDSHIELD_OFFICE_TITLE);

            if (_imageViewer.IsImageAvailable)
            {
                sb.Append(" - ");
                sb.Append(_imageViewer.ImageFile);

                if (Dirty)
                {
                    sb.Append("*");
                }
            }

            Text = sb.ToString();
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the <see cref="Extract.Imaging.AsynchronousOcrManager"/> associated with
        /// this <see cref="IDShieldOfficeForm"/>.
        /// </summary>
        /// <returns>An <see cref="Extract.Imaging.AsynchronousOcrManager"/>.</returns>
        internal AsynchronousOcrManager OcrManager
        {
            get
            {
                return _ocrManager;
            }
        }

        /// <summary>
        /// Gets the <see cref="ImageViewer"/> associated with this
        /// <see cref="IDShieldOfficeForm"/>.
        /// </summary>
        /// <returns>An <see cref="ImageViewer"/>.</returns>
        internal ImageViewer ImageViewer
        {
            get
            {
                return _imageViewer;
            }
        }

        /// <summary>
        /// Gets whether clues are currently visible on the image viewer.
        /// </summary>
        /// <returns>Whether clues are currently visible on the image viewer.</returns>
        internal bool CluesVisible
        {
            get
            {
                return _showCluesCheckBox.Checked;
            }
        }

        /// <summary>
        /// Gets or sets the dirty flag.
        /// </summary>
        /// <returns>The value of the dirty flag.</returns>
        /// <value>The value of the dirty flag.</value>
        bool Dirty
        {
            get
            {
                return _dirty;
            }
            set
            {
                _dirty = value;
                UpdateCaption();
            }
        }

        /// <summary>
        /// Gets or sets the flag to indicate whether the form and toolstrip locations
        /// should be reset when the forms loads.
        /// <para><b>Note:</b></para>
        /// This flag is only checked when the form is first loaded, setting it after the form
        /// has loaded will do nothing.
        /// </summary>
        /// <returns>Whether the form and toolstrip locations should be reset when the
        /// form loads. If <see langword="true"/> then the locations will be reset; if
        /// <see langword="false"/> then they will not be reset.</returns>
        /// <value>Whether the form and toolstrip locations should be reset when the
        /// form loads. If <see langword="true"/> then the locations will be reset; if
        /// <see langword="false"/> then they will not be reset.</value>
        [Browsable(false)]
        internal bool ResetForm
        {
            get
            {
                return _resetForm;
            }
            set
            {
                _resetForm = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="ModificationHistory"/> for this instance of IDSO.
        /// </summary>
        /// <returns>The <see cref="ModificationHistory"/> for this instance of IDSO.</returns>
        [Browsable(false)]
        internal ModificationHistory Modification
        {
            get
            {
                return _modifications;
            }
        }

        /// <summary>
        /// Gets the data type rule form.
        /// </summary>
        /// <returns>The ata type rule form.</returns>
        [Browsable(false)]
        internal RuleForm DataTypeRuleForm
        {
            get
            {
                return _dataTypeRuleForm;
            }
        }

        /// <summary>
        /// Gets the user preferences.
        /// </summary>
        /// <returns>The user preferences.</returns>
        [Browsable(false)]
        internal UserPreferences UserPreferences
        {
            get
            {
                return _userPreferences;
            }
        }

        #endregion Properties

        #region IRuleFormHelper Members

        /// <summary>
        /// Retrieves the optical character recognition (OCR) results for the rule form to use.
        /// </summary>
        /// <returns>The optical character recognition (OCR) results for the rule form to use.
        /// </returns>
        [CLSCompliant(false)]
        public SpatialString GetOcrResults()
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    // Get the current image name
                    string imageFileName = _imageViewer.ImageFile;

                    // Wait for OCR to complete
                    while (!OcrManager.OcrFinished)
                    {
                        Application.DoEvents();

                        // If while waiting the image file was changed, then exit this find
                        if (imageFileName != _imageViewer.ImageFile)
                        {
                            return null;
                        }

                        // [IDSD:344]
                        // Wait a tenth of a second between checks for OCR information so that
                        // we don't burn CPU unnecessarily while waiting for OCR result.
                        System.Threading.Thread.Sleep(100);
                    }

                    // Get the SpatialString
                    return OcrManager.GetOcrSpatialString();
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29204",
                    "Unable to get OCR results.", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified match has already been found.
        /// </summary>
        /// <param name="match">The match to check for duplication.</param>
        /// <returns><see langword="true"/> if the specified <paramref name="match"/> has 
        /// already been found; <see langword="false"/> has not yet been found.</returns>
        public bool IsDuplicate(MatchResult match)
        {
            try
            {
                bool isDuplicate = true;
                if (match.MatchType == MatchType.Clue)
                {
                    List<Clue> clues = CreateIfNotDuplicate<Clue>(match.RasterZones, "");
                    if (clues != null)
                    {
                        isDuplicate = false;
                        CollectionMethods.ClearAndDispose(clues);
                    }
                }
                else
                {
                    List<Redaction> redactions = CreateIfNotDuplicate<Redaction>(match.RasterZones, "");
                    if (redactions != null)
                    {
                        isDuplicate = false;
                        CollectionMethods.ClearAndDispose(redactions);
                    }
                }

                return isDuplicate;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29205", ex);
            }
        }

        #endregion IRuleFormHelper Members
    }
}