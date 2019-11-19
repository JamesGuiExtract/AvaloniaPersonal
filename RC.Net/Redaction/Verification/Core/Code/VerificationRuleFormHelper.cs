using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Rules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using RedactionLayerObject = Extract.Imaging.Forms.Redaction;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a set of commands that assist a rule form in the verification user interface.
    /// </summary>
    public class VerificationRuleFormHelper : IRuleFormHelper
    {
        #region Constants

        /// <summary>
        /// Tag indicating a redaction was the result of a rule form match.
        /// </summary>
        public static readonly string RedactedMatchTag = "RedactedMatch";

        #endregion Constants

        #region Fields

        readonly ImageViewer _imageViewer;

        RedactionGridView _redactionGridView;

        /// <summary>
        /// The collective <see cref="RasterZone"/>s of all redactions currently on the document.
        /// </summary>
        RasterZoneCollection _existingRasterZones;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationRuleFormHelper"/> class.
        /// </summary>
        public VerificationRuleFormHelper(ImageViewer imageViewer, RedactionGridView redactionGridView)
        {
            _imageViewer = imageViewer;
            _redactionGridView = redactionGridView;
        }

        #endregion Constructors

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
                ThreadSafeSpatialString ocrResults = _imageViewer.OcrData;

                // If OCR data isn't already loaded, check to see if the image viewer is loading
                // data in the background.
                if (ocrResults == null && _imageViewer.IsLoadingData)
                {
                    // If data is loading, display a progress box and wait for the OCRing to complete.
                    if (!_imageViewer.WaitForOcrData())
                    {
                        return null;
                    }

                    ocrResults = _imageViewer.OcrData;
                }

                if (ocrResults == null)
                {
                    MessageBox.Show("The 'Find and redact' feature can't be used on this document because OCR data is not available.",
                        "No OCR data", MessageBoxButtons.OK, MessageBoxIcon.None,
                        MessageBoxDefaultButton.Button1, 0);
                    
                    return null;
                }
                
                return ocrResults.SpatialString;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29241", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified match has already been found.
        /// </summary>
        /// <param name="match">The match to check for duplication.</param>
        /// <param name="usePreviousSourceRedactions"><see langword="true"/> if the method can
        /// assume the document contains the same data as on the previous call to this method,
        /// <see langword="false"/> if the method should rescan the document.</param>
        /// <returns><see langword="true"/> if the specified <paramref name="match"/> has 
        /// already been found; <see langword="false"/> has not yet been found.</returns>
        public bool IsDuplicate(MatchResult match, bool usePreviousSourceRedactions)
        {
            try
            {
                // Get the raster zones of all redactions if they have not yet been retrieved or the
                // caller indicates the document document redactions should be re-scanned.
                if (_existingRasterZones == null || !usePreviousSourceRedactions)
                {
                    _existingRasterZones = new RasterZoneCollection();
                    
                    // [FlexIDSCore:5167]
                    // When looking for duplicate redactions, consider only redactions that are
                    // turned on.
                    foreach (RedactionLayerObject redaction in _redactionGridView.Rows
                        .OfType<RedactionGridViewRow>()
                        .Where(row => row.Redacted)
                        .SelectMany(row => row.LayerObjects.OfType<RedactionLayerObject>()))
                    {
                        _existingRasterZones.AddRange(redaction.GetRasterZones());
                    }
                }

                // Calculate the overlap between the redaction and the match result
                RasterZoneCollection matchZones = new RasterZoneCollection(match.RasterZones);
                double overlapArea = _existingRasterZones.GetAreaOverlappingWith(matchZones);

                // If the overlap is greater than 95%, it is duplicate
                return overlapArea / matchZones.GetArea() > 0.95;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29244", ex);
            }
        }

        #endregion IRuleFormHelper Members

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="RuleForm.MatchRedacted"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="RuleForm.MatchRedacted"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="RuleForm.MatchRedacted"/> event.</param>
        internal void HandleMatchRedacted(object sender, MatchRedactedEventArgs e)
        {
            try
            {
                if (!IsDuplicate(e.Match, !e.FirstMatch))
                {
                    try
                    {
                        // [FlexIDSCore:5014]
                        // Select the first match, but not subsequent ones.
                        if (!e.FirstMatch)
                        {
                            _redactionGridView.PreventSelection();
                        }

                        Dictionary<int, List<RasterZone>> pageToZones =
                                    RasterZone.SplitZonesByPage(e.Match.RasterZones);
                        foreach (KeyValuePair<int, List<RasterZone>> pair in pageToZones)
                        {
                            RedactionLayerObject redaction = new RedactionLayerObject(_imageViewer,
                                pair.Key, new string[] { RedactedMatchTag }, e.Match.Text, pair.Value);
                            _imageViewer.LayerObjects.Add(redaction);

                            // So that this loop can call IsDuplicate with
                            // usePreviousSourceRedactions == true when FirstMatch is false, update 
                            // _existingRasterZones as matches are redacted.
                            _existingRasterZones.AddRange(pair.Value);
                        }
                    }
                    finally
                    {
                        if (!e.FirstMatch)
                        {
                            _redactionGridView.AllowSelection();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29245", ex);
            }
        }

        #endregion Event Handlers
    }
}
