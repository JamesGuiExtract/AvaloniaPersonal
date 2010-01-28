using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;
using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;
using RedactionLayerObject = Extract.Imaging.Forms.Redaction;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents the row of a <see cref="RedactionGridView"/>.
    /// </summary>
    public class RedactionGridViewRow
    {
        #region Fields

        /// <summary>
        /// The attribute to which the row corresponds.
        /// </summary>
        RedactionItem _attribute;

        /// <summary>
        /// The layer objects to which the row corresponds.
        /// </summary>
        readonly List<LayerObject> _layerObjects;

        /// <summary>
        /// The text of the redaction.
        /// </summary>
        readonly string _text;

        /// <summary>
        /// The category of the redaction (e.g. Man, Clue, etc.)
        /// </summary>
        readonly string _category;

        /// <summary>
        /// The type of the redaction (e.g. SSN, Driver's license number, etc.)
        /// </summary>
        string _type;

        /// <summary>
        /// Gets the first page number of the redaction.
        /// </summary>
        int _firstPage;

        /// <summary>
        /// Exemption codes associated with the redaction.
        /// </summary>
        ExemptionCodeList _exemptions;

        /// <summary>
        /// <see langword="true"/> if the row should be redacted; <see langword="false"/> if the 
        /// row should not be redacted.
        /// </summary>
        bool _redacted;

        /// <summary>
        /// The confidence level of the attribute in this row.
        /// </summary>
        readonly ConfidenceLevel _confidenceLevel;

        /// <summary>
        /// <see langword="true"/> if this row has been visited; <see langword="false"/> if it has 
        /// not been visited.
        /// </summary>
        bool _visited;

        /// <summary>
        /// <see langword="true"/> if <see cref="_layerObjects"/> has been modified; 
        /// <see langword="false"/> if it has not been modified.
        /// </summary>
        bool _layerObjectsDirty;

        /// <summary>
        /// <see langword="true"/> if <see cref="_type"/> has been modified; 
        /// <see langword="false"/> if it has not been modified.
        /// </summary>
        bool _typeDirty;

        /// <summary>
        /// <see langword="true"/> if <see cref="_exemptions"/> has been modified; 
        /// <see langword="false"/> if it has not been modified.
        /// </summary>
        bool _exemptionsDirty;

        /// <summary>
        /// <see langword="true"/> if <see cref="_redacted"/> has been modified; 
        /// <see langword="false"/> if it has not been modified.
        /// </summary>
        bool _redactedDirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionGridViewRow"/> class.
        /// </summary>
        public RedactionGridViewRow(LayerObject layerObject, string text, string category, 
            string type, ConfidenceLevel confidenceLevel)
            : this(new LayerObject[] { layerObject }, text, category, type, null, null, true, 
            confidenceLevel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionGridViewRow"/> class.
        /// </summary>
        public RedactionGridViewRow(IEnumerable<LayerObject> layerObjects, string text,
            string category, string type, ConfidenceLevel confidenceLevel)
            : this(layerObjects, text, category, type, null, null, true, confidenceLevel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionGridViewRow"/> class.
        /// </summary>
        RedactionGridViewRow(IEnumerable<LayerObject> layerObjects, string text, string category, 
            string type, RedactionItem attribute, ExemptionCodeList exemptions, bool redacted, 
            ConfidenceLevel confidenceLevel)
        {
            _attribute = attribute;
            _layerObjects = new List<LayerObject>(layerObjects);
            _text = text;
            _category = category;
            _type = type;
            _firstPage = GetFirstPageNumber();
            _exemptions = exemptions ?? new ExemptionCodeList();
            _redacted = redacted;
            _confidenceLevel = confidenceLevel;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="RedactionItem"/> associated with the 
        /// <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <value>The <see cref="RedactionItem"/> associated with the 
        /// <see cref="RedactionGridViewRow"/>.</value>
        internal RedactionItem RedactionItem
        {
            get
            {
                return _attribute;
            }
        }

        /// <summary>
        /// Gets the layer objects associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>The layer objects associated with the <see cref="RedactionGridViewRow"/>.
        /// </returns>
        public ReadOnlyCollection<LayerObject> LayerObjects
        {
            get
            {
                return _layerObjects.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets or sets whether the row should be redacted.
        /// </summary>
        /// <value><see langword="true"/> if the row should be redacted;
        /// <see langword="false"/> if the row should not be redacted.</value>
        public bool Redacted
        {
            get 
            {
                return _redacted;
            }
            set
            {
                try
                {
                    if (_redacted != value)
                    {
                        _redacted = value;

                        Color color = _redacted ? 
                            RedactionGridView.ToggledRedactionColor : Color.Transparent;
                        foreach (RedactionLayerObject redaction in _layerObjects)
                        {
                            redaction.Color = color;
                        }

                        _redactedDirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28799", ex);
                }
            }
        }

        /// <summary>
        /// Gets the text associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>The text associated with the <see cref="RedactionGridViewRow"/>.</returns>
        public string Text
        {
            get
            {
                return _text;
            }
        }

        /// <summary>
        /// Gets the category associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>The category associated with the <see cref="RedactionGridViewRow"/>.</returns>
        public string Category
        {
            get
            {
                return _category;
            }
        }

        /// <summary>
        /// Gets or sets the type associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <value>The type associated with the <see cref="RedactionGridViewRow"/>.</value>
        /// <returns>The type associated with the <see cref="RedactionGridViewRow"/>.</returns>
        public string RedactionType
        {
            get
            {
                return _type;
            }
            set
            {
                try
                {
                    if (_type != value)
                    {
                        _type = value;

                        _typeDirty = true;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI26925",
                        "Unable to set redaction type.", ex);
                    ee.AddDebugData("Type", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets the page number associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>The page number associated with the <see cref="RedactionGridViewRow"/>.
        /// </returns>
        public int PageNumber
        {
            get
            {
                return _firstPage;
            }
        }

        /// <summary>
        /// Gets or sets the exemption codes associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <value>The exemption codes associated with the <see cref="RedactionGridViewRow"/>.
        /// </value>
        /// <returns>The exemption codes associated with the <see cref="RedactionGridViewRow"/>.
        /// </returns>
        public ExemptionCodeList Exemptions
        {
            get
            {
                return _exemptions;
            }
            set
            {
                if (_exemptions != value)
                {
                    _exemptions = value;

                    _exemptionsDirty = true;
                }
            }
        }

        /// <summary>
        /// Gets whether a warning should be displayed if this row is redacted.
        /// </summary>
        /// <value><see langword="true"/> if a warning should be displayed if this row is redacted;
        /// <see langword="false"/> if a warning should not be displayed if this row is redacted.</value>
        public bool WarnIfRedacted
        {
            get
            {
                return _confidenceLevel.WarnIfRedacted;
            }
        }

        /// <summary>
        /// Gets whether a warning should be displayed if this row is unredacted.
        /// </summary>
        /// <value><see langword="true"/> if a warning should be displayed if this row is unredacted;
        /// <see langword="false"/> if a warning should not be displayed if this row is unredacted.</value>
        public bool WarnIfNotRedacted
        {
            get
            {
                return _confidenceLevel.WarnIfNotRedacted;
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="RedactionGridViewRow"/> has been visited.
        /// </summary>
        /// <value><see langword="true"/> if the row has been visited;
        /// <see langword="false"/> if the row has not been visited.</value>
        public bool Visited
        {
            get
            {
                return _visited;
            }
            set
            {
                _visited = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the layer objects associated with the row have changed.
        /// </summary>
        /// <value><see langword="true"/> if the layer objects have changed;
        /// <see langword="false"/> if they haven't changed.</value>
        /// <returns><see langword="true"/> if the layer objects have changed;
        /// <see langword="false"/> if they haven't changed.</returns>
        public bool LayerObjectsDirty
        {
            get
            {
                return _layerObjectsDirty;
            }
            set
            {
                _layerObjectsDirty = value;
            }
        }

        /// <summary>
        /// Gets whether the row has been added since the last save.
        /// </summary>
        /// <value><see langword="true"/> if the row has been added since the last save;
        /// <see langword="false"/> if the row existed prior to the last save.</value>
        public bool IsNew
        {
            get
            {
                return _attribute == null;
            }
        }

        /// <summary>
        /// Gets or sets whether the row has been modified since the last save.
        /// </summary>
        /// <value><see langword="true"/> if the row has been modified since the last save;
        /// <see langword="false"/> if the row has not been modified since the last save.</value>
        public bool IsModified
        {
            get
            {
                return _exemptionsDirty || _layerObjectsDirty || _typeDirty || _redactedDirty;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether the specified layer object is associated with the 
        /// <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <param name="layerObject">The layer object to check.</param>
        /// <returns><see langword="true"/> if <paramref name="layerObject"/> is associated with 
        /// the <see cref="RedactionGridViewRow"/>; <see langword="false"/> if it is not 
        /// associated with the layer object.</returns>
        public bool ContainsLayerObject(LayerObject layerObject)
        {
            try
            {
                foreach (LayerObject current in _layerObjects)
                {
                    if (current.Id == layerObject.Id)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26953",
                    "Unable to determine if layer object is associated with grid row.", ex);
            }
        }

        /// <summary>
        /// Attempts to remove the specified layer object. Returns <see langword="false"/> on 
        /// failure.
        /// </summary>
        /// <param name="layerObject">The layer object to remove.</param>
        /// <returns><see langword="true"/> if <paramref name="layerObject"/> was removed; 
        /// <see langword="false"/> if <paramref name="layerObject"/> doesn't exist in the 
        /// <see cref="RedactionGridViewRow"/> or some other error was encountered.</returns>
        public bool TryRemoveLayerObject(LayerObject layerObject)
        {
            try
            {
                // Iterate through each layer object
                for (int i = 0; i < _layerObjects.Count; i++)
                {
                    LayerObject current = _layerObjects[i];
                    if (current.Id == layerObject.Id)
                    {
                        // Found the layer object, remove it.
                        _layerObjects.RemoveAt(i);

                        // If this was the first page number, recalculate.
                        if (current.PageNumber == _firstPage)
                        {
                            _firstPage = GetFirstPageNumber();
                        }

                        _layerObjectsDirty = true;

                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            // The layer object didn't exist, return false
            return false;
        }

        /// <summary>
        /// Creates a <see cref="RedactionGridViewRow"/> with information from the specified 
        /// <see cref="SensitiveItem"/>.
        /// </summary>
        /// <param name="item">The verification item from which to create a 
        /// <see cref="RedactionGridViewRow"/>.</param>
        /// <param name="imageViewer">The image viewer used for creating layer objects.</param>
        /// <param name="masterCodes">The list of valid exemption codes.</param>
        /// <returns>A <see cref="RedactionGridViewRow"/> with information from the specified 
        /// <paramref name="item"/>.</returns>
        public static RedactionGridViewRow FromSensitiveItem(SensitiveItem item, 
            ImageViewer imageViewer, MasterExemptionCodeList masterCodes)
        {
            try
            {
                // Can only create row for spatial attribute
                RedactionItem attribute = item.Attribute;
                SpatialString value = attribute.SpatialString;
                if (!value.HasSpatialInfo())
                {
                    throw new ExtractException("ELI28073",
                        "Cannot create row from non-spatial attribute.");
                }

                // Get the data for the row from the attribute
                List<LayerObject> layerObjects = 
                    GetLayerObjectsFromSpatialString(value, imageViewer, item);
                string text = StringMethods.ConvertLiteralToDisplay(value.String);
                string category = attribute.Category;
                string type = attribute.RedactionType;
                ExemptionCodeList exemptions = attribute.GetExemptions(masterCodes);
                bool redacted = attribute.Redacted;
                ConfidenceLevel level = item.Level;

                return new RedactionGridViewRow(layerObjects, text, category, type, 
                    attribute, exemptions, redacted, level);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26759",
                    "Unable to create grid row for attribute.", ex);
            }
        }

        /// <summary>
        /// Creates a <see cref="Extract.Redaction.RedactionItem"/> from the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>A <see cref="Extract.Redaction.RedactionItem"/> created from the 
        /// <see cref="RedactionGridViewRow"/>.</returns>
        [CLSCompliant(false)]
        public RedactionItem SaveRedactionItem(string sourceDocument, LongToObjectMap pageInfoMap)
        {
            try
            {
                // Create a new attribute or update the existing one.
                if (_attribute == null)
                {
                    // Create the attribute
                    _attribute = CreateRedactionItem(sourceDocument, pageInfoMap);

                    ResetDirtyFlag();
                }
                else if (IsModified)
                {
                    // Update the attribute
                    _attribute = GetUpdatedRedactionItem(sourceDocument, pageInfoMap);

                    ResetDirtyFlag();
                }

                return _attribute;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26924",
                    "Unable to create attribute from redaction grid row.", ex);
            }
        }

        /// <summary>
        /// Creates a <see cref="Extract.Redaction.RedactionItem"/> that corresponds to the row.
        /// </summary>
        /// <param name="sourceDocument">The source document to use for the new attribute.</param>
        /// <param name="pageInfoMap">The page infomation map to use for the new attribute.</param>
        /// <returns>A new <see cref="Extract.Redaction.RedactionItem"/> that corresponds to the row.</returns>
        RedactionItem CreateRedactionItem(string sourceDocument, LongToObjectMap pageInfoMap)
        {
            // Create the attribute
            ComAttribute attribute = new ComAttribute();
            attribute.Value = GetSpatialString(sourceDocument, pageInfoMap);
            attribute.Name = _category;
            attribute.Type = _type;
                    
            return new RedactionItem(attribute, _exemptions, sourceDocument, _redacted);
        }

        /// <summary>
        /// Creates a <see cref="Extract.Redaction.RedactionItem"/> from the current row and <see cref="_attribute"/>.
        /// </summary>
        /// <param name="sourceDocument">The source document to use for the new attribute.</param>
        /// <param name="pageInfoMap">The page infomation map to use for the new attribute.</param>
        /// <returns>A <see cref="Extract.Redaction.RedactionItem"/> that corresponds to the row.</returns>
        RedactionItem GetUpdatedRedactionItem(string sourceDocument, LongToObjectMap pageInfoMap)
        {
            // Don't modify the original attribute as it may be in use elsewhere
            ICopyableObject copy = (ICopyableObject)_attribute.ComAttribute;
            ComAttribute attribute = (ComAttribute)copy.Clone();

            // Update the value
            if (_layerObjectsDirty)
            {
                attribute.Value = GetSpatialString(sourceDocument, pageInfoMap);
            }
            else
            {
                attribute.Value.SourceDocName = sourceDocument;
            }

            // Update the type
            if (_typeDirty)
            {
                attribute.Type = _type;
            }

            // Update the exemption codes
            ExemptionCodeList exemptions = _exemptionsDirty ? _exemptions : null;

            return new RedactionItem(attribute, exemptions, sourceDocument, _redacted);
        }

        /// <summary>
        /// Marks the <see cref="RedactionGridViewRow"/> attribute as unmodified since the last save.
        /// </summary>
        void ResetDirtyFlag()
        {
            _layerObjectsDirty = false;
            _typeDirty = false;
            _exemptionsDirty = false;
        }

        /// <summary>
        /// Creates a spatial string from the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <param name="sourceDocument">The source document name of the spatial string.</param>
        /// <param name="pageInfoMap">The map of pages to spatial information for the string.</param>
        /// <returns>A spatial string created from the <see cref="RedactionGridViewRow"/>.</returns>
        SpatialString GetSpatialString(string sourceDocument, LongToObjectMap pageInfoMap)
        {
            // Gets the raster zones and text for the attribute
            IUnknownVector rasterZones = GetRasterZones();
            string text = StringMethods.ConvertDisplayToLiteral(_text);

            // Create the spatial string
            SpatialString value = new SpatialString();
            value.CreateHybridString(rasterZones, text, sourceDocument, pageInfoMap);
            return value;
        }

        /// <summary>
        /// Gets the raster zones of <see cref="_layerObjects"/>.
        /// </summary>
        /// <returns>The raster zones of <see cref="_layerObjects"/>.</returns>
        IUnknownVector GetRasterZones()
        {
            // Iterate through the layer objects
            IUnknownVector vector = new IUnknownVector();
            foreach (LayerObject layerObject in _layerObjects)
            {
                RedactionLayerObject redaction = layerObject as RedactionLayerObject;
                if (redaction != null)
                {
                    // Append the raster zones of the redaction layer object
                    foreach (RasterZone zone in redaction.GetRasterZones())
                    {
                        vector.PushBack(zone.ToComRasterZone());
                    }
                }
            }

            return vector;
        }

        /// <summary>
        /// Creates layer objects that correspond to the specified spatial string.
        /// </summary>
        /// <param name="spatialString">The spatial string from which to create the layer objects.
        /// </param>
        /// <param name="imageViewer">The image viewer on which the spatial string appears.</param>
        /// <param name="item">Sensitive data that corresponds to the 
        /// <paramref name="spatialString"/>.</param>
        /// <returns>Layer objects that correspond to <paramref name="spatialString"/>.</returns>
        static List<LayerObject> GetLayerObjectsFromSpatialString(SpatialString spatialString,
            ImageViewer imageViewer, SensitiveItem item)
        {
            // Get the raster zones of the spatial string, organized by page
            Dictionary<int, List<RasterZone>> pagesToZones = GetRasterZonesByPage(spatialString);

            // Create a layer object for each page of raster zones
            return CreateLayerObjects(pagesToZones, imageViewer, item);
        }

        /// <summary>
        /// Gets the raster zones of the specified <paramref name="spatialString"/>, organized by
        /// page.
        /// </summary>
        /// <param name="spatialString">The spatial string from which raster zones should be 
        /// generated.</param>
        /// <returns>A <see cref="Dictionary{T,T}"/> that maps page numbers to collections of 
        /// <see cref="RasterZone"/>s of <paramref name="spatialString"/>.</returns>
        static Dictionary<int, List<RasterZone>> GetRasterZonesByPage(SpatialString spatialString)
        {
            // Get the raster zones as an IUnknownVector
            IUnknownVector vector = spatialString.GetOriginalImageRasterZones();

            // Prepare to store the result
            Dictionary<int, List<RasterZone>> pagesToZones = new Dictionary<int, List<RasterZone>>();

            // Iterate through each raster zone
            int size = vector.Size();
            for (int i = 0; i < size; i++)
            {
                ComRasterZone zone = (ComRasterZone)vector.At(i);
                RasterZone rasterZone = new RasterZone(zone);

                // If this page doesn't already exist, add a collection for it
                if (!pagesToZones.ContainsKey(rasterZone.PageNumber))
                {
                    pagesToZones[rasterZone.PageNumber] = new List<RasterZone>();
                }

                // Add this raster zone
                pagesToZones[rasterZone.PageNumber].Add(rasterZone);
            }

            return pagesToZones;
        }

        /// <summary>
        /// Creates a <see cref="LayerObject"/> for each page of <see cref="RasterZone"/>s in
        /// <paramref name="pagesToZones"/>.
        /// </summary>
        /// <param name="pagesToZones">Raster zones indexed by page from which to create 
        /// <see cref="LayerObject"/>s.</param>
        /// <param name="imageViewer">The image viewer to which each <see cref="LayerObject"/> 
        /// will be associated.</param>
        /// <param name="item">The sensitive item that corresponds to the layer objects to create.
        /// </param>
        /// <returns>A <see cref="List{T}"/> of <see cref="LayerObject"/> created from the 
        /// specified raster zones, one for each page of <paramref name="pagesToZones"/>.</returns>
        static List<LayerObject> CreateLayerObjects(Dictionary<int, List<RasterZone>> pagesToZones,
            ImageViewer imageViewer, SensitiveItem item)
        {
            // Iterate over each raster zone
            LayerObject previous = null;
            List<LayerObject> layerObjects = new List<LayerObject>(pagesToZones.Count);
            foreach (KeyValuePair<int, List<RasterZone>> pair in pagesToZones)
            {
                // Create a redaction and add it to the result collection
                RedactionLayerObject redaction = new RedactionLayerObject(imageViewer,
                    pair.Key, new string[] { "Redaction" }, pair.Value);
                redaction.BorderColor = item.Level.Color;
                redaction.Color = item.Attribute.Redacted ? 
                    RedactionGridView.ToggledRedactionColor : Color.Transparent;
                redaction.CanRender = item.Level.Output;

                layerObjects.Add(redaction);

                // If necessary, link to the previous page
                if (previous != null)
                {
                    previous.AddLink(redaction);
                }

                // Store this as the previous redaction
                previous = redaction;
            }

            return layerObjects;
        }

        /// <summary>
        /// Gets the first page number of the layer objects in <see cref="_layerObjects"/>.
        /// </summary>
        /// <returns>The first page number of the layer objects in <see cref="_layerObjects"/>.
        /// </returns>
        int GetFirstPageNumber()
        {
            // If there are no layer objects, return -1
            if (_layerObjects.Count <= 0)
            {
                return -1;
            }

            // Iterate over each layer object
            int page = _layerObjects[0].PageNumber;
            for (int i = 1; i < _layerObjects.Count; i++)
            {
                // If this page is earlier, store it
                int currentPage = _layerObjects[i].PageNumber;
                if (page > currentPage)
                {
                    page = currentPage;
                }
            }

            return page;
        }

        #endregion Methods
    }
}
