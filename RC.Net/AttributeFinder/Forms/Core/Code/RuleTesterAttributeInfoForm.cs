using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_COMUTILSLib;
using ComAttribute = UCLID_AFCORELib.Attribute;
using Extract.AttributeFinder.Rules;

namespace Extract.AttributeFinder.Forms
{
    /// <summary>
    /// Possible states for the _attributeValueComboBox object. Defines what text is shown
    /// in the _attributeValueTextBox
    /// </summary>
    enum AttributeValueType
    {
        Value = 0,
        OriginalOCRIncludeIntersectingChars = 1,
        OriginalOCRExcludeIntersectingChars = 2,
        OriginalOCRIncludeIntersectingWords = 3,
        OriginalOCRExcludeIntersectingWords = 4,
        OriginalOCRIncludeIntersectingLines = 5,
        OriginalOCRExcludeIntersectingLines = 6
    }

    /// <summary>
    /// Form used to view attribute info
    /// </summary>
    [ComVisible(true)]
    [Guid("96B4D3F3-94D4-46BC-94EE-E7F7D2857AA2")]
    [CLSCompliant(false)]
    public partial class RuleTesterAttributeInfoForm : Form, IRuleTesterAttributeInfo
    {
        #region Fields

        ComAttribute _attribute;
        SpatialString _originalOCR;
        SpatialString _attributeValue;

        /// <summary>
        /// Tracks temp files that need to be deleted when this form is disposed
        /// </summary>
        EventHandler Cleanup;

        #endregion Fields

        #region Constructors 

        /// <summary>
        /// Initializes readable names for <see cref="AttributeValueType"/>
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static RuleTesterAttributeInfoForm()
        {
            try
            {
                AttributeValueType.Value.SetReadableValue("Value");
                AttributeValueType.OriginalOCRIncludeIntersectingChars
                    .SetReadableValue("Original OCR, including intersecting characters");
                AttributeValueType.OriginalOCRExcludeIntersectingChars
                    .SetReadableValue("Original OCR, excluding intersecting characters");
                AttributeValueType.OriginalOCRIncludeIntersectingWords
                    .SetReadableValue("Original OCR, including intersecting words");
                AttributeValueType.OriginalOCRExcludeIntersectingWords
                    .SetReadableValue("Original OCR, excluding intersecting words");
                AttributeValueType.OriginalOCRIncludeIntersectingLines
                    .SetReadableValue("Original OCR, including intersecting lines");
                AttributeValueType.OriginalOCRExcludeIntersectingLines
                    .SetReadableValue("Original OCR, excluding intersecting lines");
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41741");
            }
        }

        /// <summary>
        /// Constructor for form
        /// </summary>
        public RuleTesterAttributeInfoForm()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41742");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Make it easy to copy the attribute's value to the clipboard
                BeginInvoke((MethodInvoker)(() =>
                {
                    _attributeValueTextBox.Focus();
                    _attributeValueTextBox.SelectAll();
                }));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41740");
            }
        }

        #endregion Overrides

        #region IRuleTesterAttributeInfo Members

        /// <summary>
        /// Method used to show attribute info
        /// </summary>
        /// <param name="pAttribute">The attribute.</param>
        /// <param name="pOriginalOCR">The <see cref="SpatialString"/> to be used as the original OCR</param>
        /// <param name="nHandle">The handle to the parent window</param>
        public void RuleTesterAttributeInfo(ComAttribute pAttribute, SpatialString pOriginalOCR, int nHandle)
        {
            try
            {
                _attribute = pAttribute;
                _originalOCR = pOriginalOCR;
                _attributeNameTextBox.Text = _attribute.Name;
                _attributeTypeTextBox.Text = _attribute.Type;

                _attributeValueComboBox.InitializeWithReadableEnum<AttributeValueType>(true);
                _attributeValueComboBox.SelectedIndex = 0;

                // Display the dialog centered on the parent
                NativeWindow parentWindow = new NativeWindow();
                parentWindow.AssignHandle((IntPtr)nHandle);
                ShowDialog(parentWindow);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI41743", ex);
            }
        }

        /// <summary>
        /// Shows the value in uss file viewer.
        /// </summary>
        /// <param name="pValue">The value to show.</param>
        public void ShowValueInUSSFileViewer(SpatialString pValue)
        {
            try
            {
                string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".uss");
                pValue.SaveTo(tempFile, true, false);
                string exe = Path.Combine(FileSystemMethods.CommonComponentsPath, "UssFileViewer.exe");
                string args = tempFile;

                var p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.FileName = exe;
                p.StartInfo.Arguments = args;

                // Delete temp file when uss viewer closes
                p.EnableRaisingEvents = true;
                p.Exited += delegate
                    {
                        try
                        {
                            FileSystemMethods.DeleteFile(tempFile);
                        }
                        catch { }
                    };

                // Delete temp file when this form closes too (the USS file viewer doesn't need it once it has loaded)
                Cleanup += delegate { FileSystemMethods.DeleteFile(tempFile); };

                // Run the viewer
                p.Start();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41724");
            }
        }

        /// <summary>
        /// Shows the original OCR 'underneath' the attribute
        /// </summary>
        /// <param name="pAttribute">The attribute.</param>
        /// <param name="pOriginalOCR">The <see cref="SpatialString"/> to be used as the original OCR</param>
        /// <param name="bIncludeIntersecting">if set to <c>true</c> chars/words/lines that intersect the raster
        /// zone boundaries will be included, else excluded</param>
        /// <param name="entityType">Type of the spatial entity to include/exclude.</param>
        public void ShowOriginalOCRInUSSFileViewer(ComAttribute pAttribute, SpatialString pOriginalOCR,
            bool bIncludeIntersecting, ESpatialEntity entityType)
        {
            try
            {
                SpatialString value = GetOriginalOCR(pAttribute, pOriginalOCR, bIncludeIntersecting, entityType);
                ShowValueInUSSFileViewer(value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41725");
            }
        }

        /// <summary>
        /// Shows the attribute history
        /// </summary>
        /// <param name="pAttribute">The attribute.</param>
        /// <param name="nHandle">The handle to the parent window</param>
        public void ShowAttributeHistory(ComAttribute pAttribute, int nHandle)
        {
            try
            {
                var form = new RuleTesterAttributeHistoryForm(pAttribute);

                // Display the dialog centered on the parent
                NativeWindow parentWindow = new NativeWindow();
                parentWindow.AssignHandle((IntPtr)nHandle);
                form.ShowDialog(parentWindow);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41773");
            }
        }

        #endregion IRuleTesterAttributeInfo Members

        #region Event Handlers

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _attributeValueComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleAttributeValueComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _attributeValue = GetAttributeValueForSelection();
                _attributeValueTextBox.Text = _attributeValue.String;

                int min = 0, max = 0, avg = 0;
                _attributeValue.GetCharConfidence(ref min, ref max, ref avg);
                _characterConfidenceTextBox.Text =
                    UtilityMethods.FormatInvariant($"Min: {min} Max: {max} Avg: {avg}");

                FillSpatialContentChart(_attributeValue);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41719");
            }
        }

        /// <summary>
        /// Handles the Click event of the _showValueInUSSViewerButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleShowValueInUSSViewerButton_Click(object sender, EventArgs e)
        {
            try
            {
                ShowValueInUSSFileViewer(_attributeValue);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41720");
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Gets the original OCR result 'underneath' the attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="originalOCR">The <see cref="SpatialString"/> to be used as the original OCR</param>
        /// <param name="includeIntersecting">if set to <c>true</c> include intersecting enities, else exclude them.</param>
        /// <param name="entityType">Type of the spatial entity to include/exclude.</param>
        /// <returns>A <see cref="SpatialString"/> constructed from the substrings of <see paramref="originalOCR"/>
        /// that correspond spatially to the raster zones of <see paramref="attribute"/> </returns>
        static SpatialString GetOriginalOCR(ComAttribute attribute, SpatialString originalOCR,
            bool includeIntersecting, ESpatialEntity entityType)
        {
            try
            {
                if (!originalOCR.HasSpatialInfo())
                {
                    return new SpatialString();
                }

                AFDocument doc = new AFDocument();
                doc.Text = originalOCR;
                var attributeCopy = (ComAttribute)((ICopyableObject)attribute).Clone();
                var searcher = new ExtractOcrTextInImageArea
                {
                    IncludeTextOnBoundary = includeIntersecting,
                    SpatialEntityType = entityType,
                    UseOriginalDocumentOcr = false,
                    UseOverallBounds = false
                };
                searcher.ModifyValue(attributeCopy, doc, null);
                return attributeCopy.Value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49995");
            }
        }

        /// <summary>
        /// Gets the attribute value based on the current selected option in the <see cref="_attributeValueComboBox"/>
        /// </summary>
        /// <returns>The spatial string appropriate for the current selection</returns>
        SpatialString GetAttributeValueForSelection()
        {
            try
            {
                var valueType = _attributeValueComboBox.ToEnumValue<AttributeValueType>();
                if (valueType == AttributeValueType.Value)
                {
                    return _attribute.Value;
                }

                ESpatialEntity entity = ESpatialEntity.kNoEntity;
                bool includeTextOnBoundary = true;
                switch (valueType)
                {
                    case AttributeValueType.OriginalOCRIncludeIntersectingChars:
                        entity = ESpatialEntity.kCharacter;
                        break;
                    case AttributeValueType.OriginalOCRExcludeIntersectingChars:
                        entity = ESpatialEntity.kCharacter;
                        includeTextOnBoundary = false;
                        break;
                    case AttributeValueType.OriginalOCRIncludeIntersectingWords:
                        entity = ESpatialEntity.kWord;
                        break;
                    case AttributeValueType.OriginalOCRExcludeIntersectingWords:
                        entity = ESpatialEntity.kWord;
                        includeTextOnBoundary = false;
                        break;
                    case AttributeValueType.OriginalOCRIncludeIntersectingLines:
                        entity = ESpatialEntity.kLine;
                        break;
                    case AttributeValueType.OriginalOCRExcludeIntersectingLines:
                        entity = ESpatialEntity.kLine;
                        includeTextOnBoundary = false;
                        break;
                }
                return GetOriginalOCR(_attribute, _originalOCR, includeTextOnBoundary, entity);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41744");
            }
        }

        /// <summary>
        /// Graphs the spatial content (% of black pixels per line) of every raster zone of
        /// <see paramref="attributeValue"/> 
        /// </summary>
        /// <param name="attributeValue">The attribute value.</param>
        void FillSpatialContentChart(SpatialString attributeValue)
        {
            try
            {
                _spatialContentChart.Series[0].Points.Clear();

                if (attributeValue.HasSpatialInfo())
                {
                    var data = attributeValue.GetOriginalImageRasterZones()
                        .ToIEnumerable<RasterZone>()
                        .SelectMany(zone =>
                        {
                            var imageUtils = new UCLID_IMAGEUTILSLib.ImageUtilsClass();
                            var imageStats =
                                imageUtils.GetImageStats(attributeValue.SourceDocName, zone);
                            int width = imageStats.Width;
                            return imageStats.FGPixelsInRow
                                .ToIEnumerable<int>()
                                .Select(pixels => 100 * pixels / width);
                        });
                    foreach (var percent in data)
                    {
                        _spatialContentChart.Series[0].Points.AddY(percent);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41745");
            }
        }

        #endregion Methods
    }
}