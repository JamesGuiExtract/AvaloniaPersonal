using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using SpatialPageInfo = UCLID_RASTERANDOCRMGMTLib.SpatialPageInfo;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.Demo_Pagination
{
    /// <summary>
    /// A demo implementation of a <see cref="IPaginationDocumentDataPanel"/>. Intended for used against
    /// the Demo_LabDE FAM database.
    /// </summary>
    internal partial class PaginationDocumentDataPanel : UserControl, IPaginationDocumentDataPanel
    {
        #region Fields

        /// <summary>
        /// The windows message code for a set focus event.
        /// </summary>
        const int WM_SETFOCUS = 0x00000008;

        /// <summary>
        /// A regex pattern to check for a valid MRN format.
        /// </summary>
        static Regex _mrnRegex = new Regex(@"(?<=\[)\d+(?=\])", RegexOptions.Compiled);

        /// <summary>
        /// A regex pattern to check for a valid date format.
        /// </summary>
        static Regex _dateRegex = new Regex(@"^\d{2}/\d{2}/\d{4}$", RegexOptions.Compiled);

        /// <summary>
        /// The data associated with this panel.
        /// </summary>
        Demo_PaginationDocumentData _documentData;

        /// <summary>
        /// Used to display error glyph for fields with invalid data.
        /// </summary>
        ErrorProvider _errorProvider;

        /// <summary>
        /// The image viewer being used to display the document.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// Prevents recursion of <see cref="HandleMRNTextChanged"/>.
        /// </summary>
        bool _updatingMRN;

        /// <summary>
        /// Indicates whether data has finished loading in order to avoid unnecessary
        /// handling of data changes as data loads.
        /// </summary>
        bool _loaded;

        /// <summary>
        /// Prevents recursion of <see cref="HandleOcrTextHighlighted"/> when processing
        /// a swipe.
        /// </summary>
        bool _processingSwipe;

        /// <summary>
        /// The OCR manager to be used to recognize text from image swipes.
        /// </summary>
        SynchronousOcrManager _ocrManager = new SynchronousOcrManager();

        /// <summary>
        /// To be able to assign <see cref="_swipeTargetControl"/> correctly, keeps track of
        /// the last control activated.
        /// </summary>
        Control _lastActive;

        /// <summary>
        /// The control that should receive the contents of an image viewer swipe.
        /// </summary>
        Control _swipeTargetControl;

        /// <summary>
        /// For demo purposes, used to demo how we could, based on other logical documents,
        /// provide default data for a newly created logical document.
        /// </summary>
        List<string> _defaultData;

        /// <summary>
        /// The doc-type specific panel for the "Referral Letter" doc type.
        /// </summary>
        ReferralPanel _referralPanel;

        /// <summary>
        /// The doc-type specific panel for the "Lab Results doc type.
        /// </summary>
        LabResultsPanel _labResultsPanel;

        /// <summary>
        /// The doc-type specific panel for the "Type and Screen" doc type.
        /// </summary>
        BloodTypePanel _bloodTypePanel;

        /// <summary>
        /// The doc-type specific panel for the "History and Physical" doc type.
        /// </summary>
        HistoryAndPhysicalPanel _historyAndPhysicalPanel;

        /// <summary>
        /// The doc-type specific panel for the "Radiology" doc type.
        /// </summary>
        RadiologyPanel _radiologyPanel;

        /// <summary>
        /// The doc-type specific panel for the "Insurance" doc type.
        /// </summary>
        InsurancePanel _insurancePanel;

        /// <summary>
        /// The currently active doc-type specific panel.
        /// </summary>
        SectionPanel _activeSectionPanel;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationDocumentDataPanel"/> class.
        /// </summary>
        public PaginationDocumentDataPanel()
        {
            try
            {
                InitializeComponent();

                _errorProvider = new ErrorProvider();
                _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                _patientFirstTextBox.SetErrorGlyphPosition(_errorProvider);
                _patientLastTextBox.SetErrorGlyphPosition(_errorProvider);
                _patientDOBTextBox.SetErrorGlyphPosition(_errorProvider);
                _patientMRNComboBox.SetErrorGlyphPosition(_errorProvider);
                _documentDateTextBox.SetErrorGlyphPosition(_errorProvider);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41366");
            }
        }

        #endregion Constructors

        #region IPaginationDocumentDataPanel

        /// <summary>
        /// Raised to indicate the panel is requesting a specific image page to be loaded.
        /// </summary>
        public event EventHandler<PageLoadRequestEventArgs> PageLoadRequest
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Indicates that the availability of an operation for the <see cref="Undo"/> method to
        /// revert has changed.
        /// </summary>
        public event EventHandler<EventArgs> UndoAvailabilityChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Indicates that the availability of an operation for the <see cref="Redo"/> method to
        /// redo has changed.
        /// </summary>
        public event EventHandler<EventArgs> RedoAvailabilityChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// The <see cref="UserControl"/> to be displayed for viewing/editing of document data.
        /// </summary>
        public UserControl PanelControl
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether advanced data entry operations (such as undo/redo) are
        /// supported.
        /// </summary>
        /// <value><see langword="true"/> if advanced data entry operations (such as undo/redo) are
        /// supported; otherwise,<see langword="false"/>.
        /// </value>
        public bool AdvancedDataEntryOperationsSupported
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether an undo operation is available.
        /// </summary>
        /// <value>
        /// <c>true</c> if an undo operation is available; otherwise, <c>false</c>.
        /// </value>
        public bool UndoOperationAvailable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether an redo operation is available.
        /// </summary>
        /// <value>
        /// <c>true</c> if an redo operation is available; otherwise, <c>false</c>.
        /// </value>
        public bool RedoOperationAvailable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Loads the specified <see paramref="data"/>.
        /// </summary>
        /// <param name="data">The data to load.</param>
        public void LoadData(PaginationDocumentData data)
        {
            try
            {
                _loaded = false;

                if (_documentData != null)
                {
                    _documentData.DataReverted -= HandleDocumentData_DataReverted;
                }

                if (data == null)
                {
                    _documentTypeComboBox.Text = "";
                    _documentDateTextBox.Text = "";
                    _patientFirstTextBox.Text = "";
                    _patientMiddleTextBox.Text = "";
                    _patientLastTextBox.Text = "";
                    _patientDOBTextBox.Text = "";
                    _patientSexComboBox.Text = "";
                    _patientMRNComboBox.Items.Clear();
                    _patientMRNComboBox.Text = "";
                    Enabled = false;
                }
                else
                {
                    _documentData = (Demo_PaginationDocumentData)data;
                    _documentData.DataReverted += HandleDocumentData_DataReverted;

                    _documentTypeComboBox.Text = _documentData.DocumentType;
                    _documentDateTextBox.Text = _documentData.DocumentDate;
                    _documentCommentTextBox.Text = _documentData.DocumentComment;
                    _patientFirstTextBox.Text = _documentData.PatientFirst;
                    _patientMiddleTextBox.Text = _documentData.PatientMiddle;
                    _patientLastTextBox.Text = _documentData.PatientLast;
                    _patientDOBTextBox.Text = _documentData.PatientDOB;
                    _patientSexComboBox.Text = _documentData.PatientSex;
                    _patientMRNComboBox.Items.Clear();
                    _patientMRNComboBox.Text = _documentData.PatientMRN;

                    ApplyDocType();

                    if (_activeSectionPanel != null)
                    {
                        _activeSectionPanel.LoadData(_documentData);
                    }

                    if (string.IsNullOrWhiteSpace(_documentData.DocumentType))
                    {
                        _defaultData = null;
                    }

                    _loaded = true;
                    Enabled = true;

                    UpdateAllValidation();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39733");
            }
        }

        /// <summary>
        /// Applies any data to the specified <see paramref="data"/>.
        /// <para><b>Note</b></para>
        /// In addition to returning <see langword="false"/>, it is the implementor's responsibility
        /// to notify the user of any problems with the data that needs to be corrected before it
        /// can be saved.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="validateData"><see langword="true"/> if the <see paramref="data"/> should
        /// be validated for errors when saving; otherwise, <see langwor="false"/>.</param>
        /// <returns><see langword="true"/> if the data was saved correctly or
        /// <see langword="false"/> if corrections are needed before it can be saved.</returns>
        public bool SaveData(PaginationDocumentData data, bool validateData)
        {
            try
            {
                if (data != null)
                {
                    var documentData = (Demo_PaginationDocumentData)data;

                    if (validateData)
                    {
                        var validatedFields = new Control[]
                        {
                            _patientFirstTextBox,
                            _patientLastTextBox,
                            _patientDOBTextBox,
                            _patientMRNComboBox
                        };

                        foreach (Control control in validatedFields)
                        {
                            string error = _errorProvider.GetError(control);
                            if (!string.IsNullOrWhiteSpace(error))
                            {
                                UtilityMethods.ShowMessageBox(error, "Error", true);
                                return false;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(_documentTypeComboBox.Text))
                    {
                        documentData.DocumentType = _documentTypeComboBox.Text;
                        documentData.DocumentDate = _documentDateTextBox.Text;
                        documentData.DocumentComment = _documentCommentTextBox.Text;
                        documentData.PatientFirst = _patientFirstTextBox.Text;
                        documentData.PatientMiddle = _patientMiddleTextBox.Text;
                        documentData.PatientLast = _patientLastTextBox.Text;
                        documentData.PatientDOB = _patientDOBTextBox.Text;
                        documentData.PatientSex = _patientSexComboBox.Text;
                        documentData.PatientMRN = _patientMRNComboBox.Text;
                    }

                    if (!documentData.DataError && !string.IsNullOrWhiteSpace(documentData.PatientMRN))
                    {
                        _defaultData = new List<string>(new[]
                        {
                            documentData.PatientFirst,
                            documentData.PatientLast,
                            documentData.PatientDOB
                        });
                    }

                    if (_activeSectionPanel != null)
                    {
                        return _activeSectionPanel.SaveData(documentData, validateData);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39734");
            }
        }

        /// <summary>
        /// Clears the state of all data associated with the previously loaded document.
        /// </summary>
        public void ClearData()
        {
            try
            {
                _loaded = false;
                Enabled = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41492");
            }
        }

        /// <summary>
        /// Gets a <see cref="PaginationDocumentData"/> instance based on the provided
        /// <see paramref="attributes"/>.
        /// </summary>
        /// <param name="attributes">The VOA data for while a <see cref="PaginationDocumentData"/>
        /// instance is needed.</param>
        /// <param name="sourceDocName">The name of the source document for which data is being
        /// loaded.</param>
        /// <param name="fileProcessingDB"></param>
        /// <param name="imageViewer"></param>
        /// <returns>The <see cref="PaginationDocumentData"/> instance.</returns>
        public PaginationDocumentData GetDocumentData(IUnknownVector attributes,
            string sourceDocName, FileProcessingDB fileProcessingDB, ImageViewer imageViewer)
        {
            try
            {
                _imageViewer = imageViewer;
                var documentData = new Demo_PaginationDocumentData(
                    attributes, fileProcessingDB, imageViewer);

                if (_defaultData != null && string.IsNullOrWhiteSpace(documentData.PatientFirst) &&
                    imageViewer != null && imageViewer.ImageFile.Contains("FullPacket"))
                {
                    documentData.PatientFirst = _defaultData[0];
                    documentData.PatientLast = _defaultData[1];
                    documentData.PatientDOB = _defaultData[2];
                }
                else
                {
                    _defaultData = null;
                }

                return documentData;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39732");
            }
        }

        /// <summary>
        /// Provides a message to be displayed.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void ShowMessage(string message)
        {
            // Unused
        }

        /// <summary>
        /// Performs an undo operation.
        /// </summary>
        public void Undo()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Performs a redo operation.
        /// </summary>
        public void Redo()
        {
            throw new NotSupportedException();
        }

        #endregion IPaginationDocumentDataPanel

        #region Overrides

        /// <summary>
        /// Processes windows messages.
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message" /> to process.</param>
        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == WM_SETFOCUS && _lastActive != null)
                {
                    _swipeTargetControl = _lastActive;
                }
                else if (ActiveControl != null)
                {
                    _lastActive = ActiveControl;
                }

                base.WndProc(ref m);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41367");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Enter" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnEnter(EventArgs e)
        {
            try
            {
                base.OnEnter(e);

                _imageViewer.AllowHighlight = true;
                _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;
                _imageViewer.OcrTextHighlighted += HandleOcrTextHighlighted;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI0");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Leave" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLeave(EventArgs e)
        {
            _imageViewer.AllowHighlight = false;

            base.OnLeave(e);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the case that OCR text is highlighted in the image viewer. (i.e., "swiped" with
        /// the word highlight tool.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.OcrTextEventArgs"/> instance
        /// containing the event data.</param>
        void HandleOcrTextHighlighted(object sender, OcrTextEventArgs e)
        {
            try
            {
                _processingSwipe = true;

                Focus();

                if (_swipeTargetControl != null)
                {
                    if (!string.IsNullOrWhiteSpace(_swipeTargetControl.Text))
                    {
                        return;
                    }

                    string swipedText = e.OcrData.SpatialString.String;
                    if (_swipeTargetControl == _patientFirstTextBox)
                    {
                        string[] words = swipedText.Split(new[] { ' ' }, StringSplitOptions.None);
                        if (words.Length == 2)
                        {
                            _patientFirstTextBox.Text = words[0];
                            _patientLastTextBox.Text = words[1];

                            _patientFirstTextBox.Focus();
                            return;
                        }
                    }

                    _swipeTargetControl.Text = e.OcrData.SpatialString.String;
                    _swipeTargetControl.Focus();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49967");
            }
            finally
            {
                _processingSwipe = false;
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.LayerObjectAdded"/> event (occurs when a swipe has occurred).
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="LayerObjectAddedEventArgs"/> instance containing the event data.</param>
        void HandleLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            // Keep track of whether this event kicks off processing of a swipe.
            bool startedSwipeProcessing = false;

            try
            {
                Focus();

                if (!_imageViewer.LayerObjects.Contains(e.LayerObject))
                {
                    return;
                }
                else if (_swipeTargetControl == null)
                {
                    _imageViewer.LayerObjects.Remove(e.LayerObject);
                }

                // Don't attempt to process the event as a swipe if the user cancelled a swipe after
                // starting it, if a swipe is already being processed, or if the active control does
                // not support swiping.
                if (!_processingSwipe)
                {
                    List<RasterZone> highlightedZones = new List<RasterZone>();

                    // Angular and rectangular swipes will be highlights.
                    Highlight highlight = e.LayerObject as Highlight;
                    if (highlight != null)
                    {
                        highlightedZones.Add(highlight.ToRasterZone());
                    }
                    else
                    {
                        // Word highlighter swipes will be CompositeHighlightLayerObjects.
                        CompositeHighlightLayerObject compositeHighlight =
                            e.LayerObject as CompositeHighlightLayerObject;
                        if (compositeHighlight != null)
                        {
                            highlightedZones.AddRange(compositeHighlight.GetRasterZones());
                        }
                    }

                    if (highlightedZones.Count > 0)
                    {
                        startedSwipeProcessing = true;
                        _processingSwipe = true;

                        _imageViewer.LayerObjects.Remove(e.LayerObject, true, false);

                        // Recognize the text in the highlight's raster zone and send it to the active
                        // data control for processing.
                        using (new TemporaryWaitCursor())
                        {
                            SpatialString ocrText = null;

                            try
                            {

                                foreach (RasterZone zone in highlightedZones)
                                {
                                    // [DataEntry:294] Keep the angle threshold small so long swipes
                                    // on slightly skewed docs don't include more text than intended.
                                    SpatialString zoneOcrText = _ocrManager.GetOcrText(
                                        _imageViewer.ImageFile, zone, 0.2);

                                    if (ocrText == null)
                                    {
                                        ocrText = zoneOcrText;
                                    }
                                    else
                                    {
                                        ocrText.Append(zoneOcrText);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ExtractException.Log("ELI41368", ex);

                                return;
                            }

                            // If a highlight was created using the auto-fit mode of the word
                            // highlight tool, create a hybrid string whose spatial area is the full
                            // area of the "swipe" rather than just what OCR'd. (allows an easy way
                            // to add spatial info for a field.)
                            if (_imageViewer.CursorTool == CursorTool.WordHighlight)
                            {
                                // Create unrotated/skewed spatial page info for the resulting
                                // hybrid string.
                                var spatialPageInfos = new LongToObjectMap();
                                var spatialPageInfo = new SpatialPageInfo();
                                spatialPageInfo.Initialize(_imageViewer.ImageWidth, _imageViewer.ImageHeight, 0, 0);
                                spatialPageInfos.Set(_imageViewer.PageNumber, spatialPageInfo);

                                // Create the hybrid result using the spatial data from the swipe
                                // with the text from the OCR attempt.
                                var hybridOcrText = new SpatialString();
                                hybridOcrText.CreateHybridString(
                                    highlightedZones
                                        .Select(rasterZone => rasterZone.ToComRasterZone())
                                        .ToIUnknownVector(),
                                    (ocrText == null) ? "" : ocrText.String,
                                    _imageViewer.ImageFile, spatialPageInfos);
                                ocrText = hybridOcrText;
                            }
                            else
                            {
                                // If no OCR results were produced, notify the user.
                                if (ocrText == null || string.IsNullOrEmpty(ocrText.String))
                                {
                                    //ShowUserNotificationTooltip("No text was recognized.");
                                    return;
                                }
                            }

                            _swipeTargetControl.Text = ocrText.String;
                            _swipeTargetControl.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If this event kicked off processing of a swipe, refresh the image viewer so the
                // swipe highlight is removed.
                if (startedSwipeProcessing)
                {
                    try
                    {
                        _imageViewer.Invalidate();
                    }
                    catch (Exception ex2)
                    {
                        ExtractException.Display("ELI41369", ex2);
                    }
                }

                // It should be highly unlikely to catch an exception here; most processes
                // that can result in an exception from swiping should trigger a user notification
                // tooltip.
                ExtractException.Display("ELI41370", ex);
            }
            finally
            {
                if (startedSwipeProcessing)
                {
                    // Only if startedSwipeProcessing is set are we dealing with the original swipe.
                    // Otherwise, the layer is a side effect of a swipe and the swipe event is still
                    // in progress.
                    _processingSwipe = false;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationDocumentData.DataReverted"/> event of
        /// <see cref="_documentData"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleDocumentData_DataReverted(object sender, EventArgs e)
        {
            try
            {
                UpdateAllValidation();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41371");
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of 
        /// <see cref="_documentTypeComboBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleDocumentTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_loaded)
                {
                    ApplyDocType();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41372");
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the _documentDateTextBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleDocumentDateTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_loaded)
                {
                    _documentData.DocumentDate = _documentDateTextBox.Text;

                    DateTime dateTime;
                    if (string.IsNullOrWhiteSpace(_documentDateTextBox.Text) ||
                        (_dateRegex.IsMatch(_documentDateTextBox.Text) &&
                         DateTime.TryParse(_documentDateTextBox.Text, out dateTime) && dateTime < DateTime.Now))
                    {
                        _documentDateTextBox.SetError(_errorProvider, "");
                        _documentData.SetValidity("DocumentDate", true);
                    }
                    else
                    {
                        _documentDateTextBox.SetError(_errorProvider, "Please enter a valid date.");
                        _documentData.SetValidity("DocumentDate", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI0");
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the _patientFirstTextBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandlePatientFirstTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_loaded)
                {
                    _documentData.PatientFirst = _patientFirstTextBox.Text;

                    UpdateMRN();
                    UpdateValidation(_patientFirstTextBox, "PatientFirst",
                        "Patient first name does not match records.");

                    UpdateMRNValidity();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI0");
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the _patientMiddleTextBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandlePatientMiddleTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_loaded)
                {
                    _documentData.PatientMiddle = _patientMiddleTextBox.Text;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI0");
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the _patientLastTextBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandlePatientLastTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_loaded)
                {
                    _documentData.PatientLast = _patientLastTextBox.Text;

                    UpdateMRN();
                    UpdateValidation(_patientLastTextBox, "PatientLast",
                        "Patient last name does not match records.");

                    UpdateMRNValidity();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI0");
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the _patientDOBTextBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandlePatientDOBTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_loaded)
                {
                    _documentData.PatientDOB = _patientDOBTextBox.Text;

                    UpdateMRN();
                    UpdateValidation(_patientDOBTextBox, "PatientDOB",
                        "Patient DOB does not match records.");

                    UpdateMRNValidity();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI0");
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _patientSexComboBox
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandlePatientSexComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_loaded)
                {
                    _documentData.PatientSex = _patientSexComboBox.Text;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI0");
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the _patientMRNTextBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandlePatientMRNTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_loaded)
                {
                    _documentData.PatientMRN = _patientMRNComboBox.Text;

                    UpdateAllValidation();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI0");
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of <see cref="_patientMRNComboBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandlePatientMRNTextBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                HandleMRNTextChanged();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI0");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Handles the case that <see cref="_patientMRNComboBox"/>'s text has changed.
        /// </summary>
        void HandleMRNTextChanged()
        {
            try
            {
                if (_updatingMRN || !_loaded)
                {
                    return;
                }

                string mrn = _patientMRNComboBox.Text;
                var match = _mrnRegex.Match(mrn);
                if (match.Success)
                {
                    _updatingMRN = true;

                    try
                    {
                        mrn = match.Value;
                        _patientMRNComboBox.DroppedDown = false;
                        this.SafeBeginInvoke("ELI41373", () =>
                        {
                            _patientMRNComboBox.Text = mrn;
                        });
                    }
                    finally
                    {
                        _updatingMRN = false;
                    }
                }

                _documentData.PatientMRN = mrn;

                if (_documentData.IsMRNValid())
                {
                    _patientMRNComboBox.SetError(_errorProvider, "");

                    UpdateValidation(_patientFirstTextBox, "PatientFirst",
                        "Patient first name does not match records.");
                    UpdateValidation(_patientLastTextBox, "PatientLast",
                        "Patient last name does not match records.");
                    UpdateValidation(_patientDOBTextBox, "PatientDOB",
                        "Patient DOB does not match records.");
                }
                else
                {
                    _patientMRNComboBox.SetError(_errorProvider, "MRN not found in patient records");

                    UpdateValidation(_patientFirstTextBox, "PatientFirst",
                        "Patient first name does not match records.");
                    UpdateValidation(_patientLastTextBox, "PatientLast",
                        "Patient last name does not match records.");
                    UpdateValidation(_patientDOBTextBox, "PatientDOB",
                        "Patient DOB does not match records.");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41374");
            }
        }

        /// <summary>
        /// Updates all validation.
        /// </summary>
        void UpdateAllValidation()
        {
            UpdateMRN();
            UpdateMRNValidity();

            UpdateValidation(_patientFirstTextBox, "PatientFirst",
                "Patient first name does not match records.");
            UpdateValidation(_patientLastTextBox, "PatientLast",
                "Patient last name does not match records.");
            UpdateValidation(_patientDOBTextBox, "PatientDOB",
                "Patient DOB does not match records.");
        }

        /// <summary>
        /// Activates the appropriate doc type specific panel based on the current doc type.
        /// </summary>
        void ApplyDocType()
        {
            SectionPanel newPanel = null;

            _documentData.DocumentType = _documentTypeComboBox.Text;
            if (_documentTypeComboBox.Text.Equals("Referral Letter", StringComparison.OrdinalIgnoreCase))
            {
                _referralPanel = _referralPanel ?? new ReferralPanel();
                newPanel = _referralPanel;
            }
            else if (_documentTypeComboBox.Text.Equals("Lab Results", StringComparison.OrdinalIgnoreCase))
            {
                _labResultsPanel = _labResultsPanel ?? new LabResultsPanel();
                newPanel = _labResultsPanel;
            }
            else if (_documentTypeComboBox.Text.Equals("Type and Screen", StringComparison.OrdinalIgnoreCase))
            {
                _bloodTypePanel = _bloodTypePanel ?? new BloodTypePanel();
                newPanel = _bloodTypePanel;
            }
            else if (_documentTypeComboBox.Text.Equals("History and Physical", StringComparison.OrdinalIgnoreCase))
            {
                _historyAndPhysicalPanel = _historyAndPhysicalPanel ?? new HistoryAndPhysicalPanel();
                newPanel = _historyAndPhysicalPanel;
            }
            else if (_documentTypeComboBox.Text.Equals("Radiology", StringComparison.OrdinalIgnoreCase))
            {
                _radiologyPanel = _radiologyPanel ?? new RadiologyPanel();
                newPanel = _radiologyPanel;
            }
            else if (_documentTypeComboBox.Text.Equals("Insurance", StringComparison.OrdinalIgnoreCase))
            {
                _insurancePanel = _insurancePanel ?? new InsurancePanel();
                newPanel = _insurancePanel;
            }

            if (_activeSectionPanel != newPanel)
            {
                if (_activeSectionPanel != null)
                {
                    _activeSectionPanel.SaveData(_documentData, false);
                    _tableLayoutPanel.Controls.Remove(_activeSectionPanel);
                }

                _activeSectionPanel = newPanel;

                if (_activeSectionPanel != null)
                {
                    _tableLayoutPanel.Controls.Add(_activeSectionPanel);
                    _tableLayoutPanel.SetCellPosition(_activeSectionPanel, new TableLayoutPanelCellPosition(0, 0));
                    _activeSectionPanel.Dock = DockStyle.Fill;
                    _activeSectionPanel.TabIndex = 0;

                    _activeSectionPanel.ErrorProvider = _errorProvider;
                    _activeSectionPanel.LoadData(_documentData);
                }
            }
        }

        /// <summary>
        /// If possible, automatically populates the MRN from the database based on the currently
        /// entered patient demographic info.
        /// </summary>
        void UpdateMRN()
        {
            if (_loaded && string.IsNullOrWhiteSpace(_patientMRNComboBox.Text))
            {
                string mrn = _documentData.LookupMRN(_patientFirstTextBox.Text,
                    _patientLastTextBox.Text, _patientDOBTextBox.Text);
                if (!string.IsNullOrWhiteSpace(mrn))
                {
                    var match = _mrnRegex.Match(_patientFirstTextBox.Text);
                    if (!match.Success || match.Value != mrn)
                    {
                        _patientMRNComboBox.Text = mrn;
                    }
                }
            }
        }

        /// <summary>
        /// Validates the currently entered MRN and populates an auto-complete list.
        /// </summary>
        void UpdateMRNValidity()
        {
            if (_documentData.IsMRNValid())
            {
                _patientMRNComboBox.SetError(_errorProvider, "");
            }
            else
            {
                _patientMRNComboBox.SetError(_errorProvider, "MRN not found in patient records");
            }

            var possibleMRNs = _documentData
                .GetPossibleMRNs()
                .ToArray();

            if (!_patientMRNComboBox.Items
                .OfType<string>()
                .SequenceEqual(possibleMRNs))
            {
                string lastMRN = _patientMRNComboBox.Text;
                _patientMRNComboBox.Items.Clear();
                _patientMRNComboBox.Items.AddRange(possibleMRNs);

                EnsureMRNValue(lastMRN);
            }
        }

        /// <summary>
        /// After resetting the auto-complete values, ensures the last entered MRN remains populated.
        /// </summary>
        void EnsureMRNValue(string mrnValue)
        {
            if (!string.IsNullOrWhiteSpace(mrnValue))
            {
                _patientMRNComboBox.DroppedDown = false;
                if (IsHandleCreated)
                {
                    this.SafeBeginInvoke("ELI41375", () =>
                    {
                        _patientMRNComboBox.Text = mrnValue;
                    });
                }
            }
        }

        /// <summary>
        /// Validates the specified <see paramref="textBox"/>.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to validate.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="error">The error message that should be applied if the field is invalid.</param>
        void UpdateValidation(TextBox textBox, string fieldName, string error)
        {
            if (fieldName == "PatientDOB")
            {
                textBox.SetError(_errorProvider, _documentData.IsDateValid(fieldName)
                    ? ""
                    : error);
            }
            else
            {
                textBox.SetError(_errorProvider, _documentData.IsValueValid(fieldName)
                    ? ""
                    : error);
            }
        }

        /// <summary>
        /// Updates the document data status.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void UpdateDocumentDataStatus(PaginationDocumentData data)
        {
            try
            {
                // Loading the data will update the document status.
                LoadData(data);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41493");
            }
        }

        /// <summary>
        /// Toggles whether or not tooltip(s) for the active fields are currently visible.
        /// </summary>
        public void ToggleHideTooltips()
        {
            // Nothing to do
        }

        #endregion Private Members
    }
}
