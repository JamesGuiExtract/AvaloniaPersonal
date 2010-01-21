using System.Collections.Generic;
using System.IO;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Rules;
using System;

using RedactionLayerObject = Extract.Imaging.Forms.Redaction;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a set of commands that assist a rule form in the verification user interface.
    /// </summary>
    public class VerificationRuleFormHelper : IRuleFormHelper
    {
        #region Fields

        readonly ImageViewer _imageViewer;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationRuleFormHelper"/> class.
        /// </summary>
        public VerificationRuleFormHelper(ImageViewer imageViewer)
        {
            _imageViewer = imageViewer;
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
                SpatialString ocrResults = new SpatialString();

                if (_imageViewer.IsImageAvailable)
                {
                    string ussFile = _imageViewer.ImageFile + ".uss";
                    if (File.Exists(ussFile))
                    {
                        ocrResults.LoadFrom(ussFile, false);
                    }
                }

                return ocrResults;
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
        /// <returns><see langword="true"/> if the specified <paramref name="match"/> has 
        /// already been found; <see langword="false"/> has not yet been found.</returns>
        public bool IsDuplicate(MatchResult match)
        {
            try
            {
                // Get the raster zones of all redactions
                RasterZoneCollection redactionZones = new RasterZoneCollection();
                foreach (LayerObject layerObject in _imageViewer.LayerObjects)
                {
                    RedactionLayerObject redaction = layerObject as RedactionLayerObject;
                    if (redaction != null)
                    {
                        redactionZones.AddRange(redaction.GetRasterZones());
                    }
                }

                // Calculate the overlap between the redaction and the match result
                RasterZoneCollection matchZones = new RasterZoneCollection(match.RasterZones);
                double overlapArea = redactionZones.GetAreaOverlappingWith(matchZones);

                // If the overlap is greater than 95%, it is duplicate
                return overlapArea / matchZones.GetArea() > 0.95;

                
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29244", ex);
            }
        }

        

        #endregion IRuleFormHelper Members

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
                Dictionary<int, List<RasterZone>> pageToZones = 
                    RasterZone.SplitZonesByPage(e.Match.RasterZones);
                foreach (KeyValuePair<int, List<RasterZone>> pair in pageToZones)
                {
                    RedactionLayerObject redaction = new RedactionLayerObject(_imageViewer,
                        pair.Key, e.Match.Text, pair.Value);
                    _imageViewer.LayerObjects.Add(redaction);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29245", ex);
            }
        }
    }
}
