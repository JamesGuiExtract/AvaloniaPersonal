using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
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
        #region RedactionGridViewRow Fields

        /// <summary>
        /// The attribute to which the row corresponds.
        /// </summary>
        ComAttribute _attribute;

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

        #endregion RedactionGridViewRow Fields

        #region RedactionGridViewRow Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionGridViewRow"/> class.
        /// </summary>
        public RedactionGridViewRow(LayerObject layerObject, string text, string category,
            string type)
            : this(new LayerObject[] { layerObject }, text, category, type, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionGridViewRow"/> class.
        /// </summary>
        public RedactionGridViewRow(IEnumerable<LayerObject> layerObjects, string text, 
            string category, string type) : this(layerObjects, text, category, type, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionGridViewRow"/> class.
        /// </summary>
        RedactionGridViewRow(IEnumerable<LayerObject> layerObjects, string text, string category, 
            string type, ComAttribute attribute, ExemptionCodeList exemptions)
        {
            _attribute = attribute;
            _layerObjects = new List<LayerObject>(layerObjects);
            _text = text;
            _category = category;
            _type = type;
            _firstPage = GetFirstPageNumber();
            _exemptions = exemptions ?? new ExemptionCodeList();
        }

        #endregion RedactionGridViewRow Constructors

        #region RedactionGridViewRow Properties

        /// <summary>
        /// Gets the COM attribute associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <value>The COM attribute associated with the <see cref="RedactionGridViewRow"/>.</value>
        internal ComAttribute ComAttribute
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

        #endregion RedactionGridViewRow Properties

        #region RedactionGridViewRow Methods

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
        /// attribute.
        /// </summary>
        /// <param name="attribute">The attribute from which to create a 
        /// <see cref="RedactionGridViewRow"/>.</param>
        /// <param name="imageViewer">The image viewer on which layer objects should be added.</param>
        /// <param name="masterCodes">The master list of valid exemption codes and categories.</param>
        /// <param name="level">The confidence level of <paramref name="attribute"/>.</param>
        /// <returns>A <see cref="RedactionGridViewRow"/> with information from the specified 
        /// <paramref name="attribute"/> or <see langword="null"/> if the attribute does not 
        /// contain spatial information.</returns>
        [CLSCompliant(false)]
        public static RedactionGridViewRow FromComAttribute(ComAttribute attribute,
            ImageViewer imageViewer, MasterExemptionCodeList masterCodes, ConfidenceLevel level)
        {
            try
            {
                // Can only create row for spatial attribute
                SpatialString value = attribute.Value;
                if (!value.HasSpatialInfo())
                {
                    return null;
                }

                // Get the data for the row from the attribute
                List<LayerObject> layerObjects = 
                    GetLayerObjectsFromSpatialString(value, imageViewer, level);
                string text = StringMethods.ConvertLiteralToDisplay(value.String);
                string category = attribute.Name;
                string type = attribute.Type;
                ExemptionCodeList exemptions = GetExemptionsFromComAttribute(attribute, masterCodes);

                return new RedactionGridViewRow(layerObjects, text, category, type, attribute, exemptions);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26759",
                    "Unable to create grid row for attribute.", ex);
            }
        }

        /// <summary>
        /// Gets the exemption code list from an exemption codes attribute.
        /// </summary>
        /// <param name="attribute">An exemption codes attribute.</param>
        /// <param name="masterCodes">The master collection of valid exemption codes.</param>
        /// <returns>The exemption code list created from <paramref name="attribute"/>.</returns>
        static ExemptionCodeList GetExemptionsFromComAttribute(ComAttribute attribute,
            MasterExemptionCodeList masterCodes)
        {
            ComAttribute exemptionsAttribute = GetExemptionsComAttribute(attribute.SubAttributes);
            if (exemptionsAttribute == null)
            {
                return new ExemptionCodeList();
            }

            string category = exemptionsAttribute.Type;
            string codes = exemptionsAttribute.Value.String;

            return ExemptionCodeList.Parse(category, codes, masterCodes);
        }

        /// <summary>
        /// Creates a COM attribute from the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>A COM attribute created from the <see cref="RedactionGridViewRow"/>.</returns>
        [CLSCompliant(false)]
        public ComAttribute ToComAttribute(string sourceDocName, LongToObjectMap pageInfoMap)
        {
            try
            {
                if (_attribute == null)
                {
                    // Create the spatial string
                    SpatialString value = GetSpatialString(sourceDocName, pageInfoMap);

                    // Create the attribute
                    ComAttribute attribute = new ComAttribute();
                    attribute.Value = value;
                    attribute.Name = _category;
                    attribute.Type = _type;
                    
                    // Set exemptions
                    SetExemptions(attribute, sourceDocName);

                    _attribute = attribute;
                }
                else
                {
                    if (_layerObjectsDirty)
                    {
                        _attribute.Value = GetSpatialString(sourceDocName, pageInfoMap);
                    }
                    if (_typeDirty)
                    {
                        _attribute.Type = _type;
                    }
                    if (_exemptionsDirty)
                    {
                        // Set exemptions
                        SetExemptions(_attribute, sourceDocName);
                    }
                }

                _layerObjectsDirty = false;
                _typeDirty = false;
                _exemptionsDirty = false;

                return _attribute;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26924",
                    "Unable to create attribute from redaction grid row.", ex);
            }
        }

        /// <summary>
        /// Creates a spatial string from the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <param name="sourceDocName">The source document name of the spatial string.</param>
        /// <param name="pageInfoMap">The map of pages to spatial information for the string.</param>
        /// <returns>A spatial string created from the <see cref="RedactionGridViewRow"/>.</returns>
        SpatialString GetSpatialString(string sourceDocName, LongToObjectMap pageInfoMap)
        {
            // Gets the raster zones and text for the attribute
            IUnknownVector rasterZones = GetRasterZones();
            string text = StringMethods.ConvertDisplayToLiteral(_text);

            // Create the spatial string
            SpatialString value = new SpatialString();
            value.CreateHybridString(rasterZones, text, sourceDocName, pageInfoMap);
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
        /// Sets the exemption codes for the specified COM attribute.
        /// </summary>
        /// <param name="attribute">The COM attribute to assign exemption codes.</param>
        /// <param name="sourceDocName">The name of the source document.</param>
        void SetExemptions(ComAttribute attribute, string sourceDocName)
        {
            // If there are no exemption codes to assign, just remove the exemption codes attribute.
            if (_exemptions.IsEmpty)
            {
                RemoveExemptionsComAttribute(attribute);
                return;
            }

            // Get the attribute's exemption codes attribute
            IUnknownVector subattributes = attribute.SubAttributes;
            ComAttribute exemptionsAttribute = GetExemptionsComAttribute(subattributes);
            if (exemptionsAttribute == null)
            {
                // Append a new exemption codes COM attribute
                exemptionsAttribute = new ComAttribute();
                exemptionsAttribute.Name = "ExemptionCodes";
                subattributes.PushBack(exemptionsAttribute);
            }
            
            // Set the exemption codes
            SpatialString value = new SpatialString();
            value.CreateNonSpatialString(_exemptions.ToString(), sourceDocName);
            exemptionsAttribute.Value = value;
            exemptionsAttribute.Type = _exemptions.Category;
        }

        /// <summary>
        /// Removes exemption codes from the specified COM attribute.
        /// </summary>
        /// <param name="attribute">The COM attribute from which to remove exemption codes.</param>
        static void RemoveExemptionsComAttribute(ComAttribute attribute)
        {
            IUnknownVector subattributes = attribute.SubAttributes;
            int count = subattributes.Size();
            for (int i = 0; i < count; i++)
            {
                ComAttribute subattribute = (ComAttribute) subattributes.At(i);
                if (subattribute.Name == "ExemptionCodes")
                {
                    subattributes.Remove(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Retrieves the exemption codes attribute from a vector of attributes.
        /// </summary>
        /// <param name="attributes">The attributes to check.</param>
        /// <returns>The exemption code attribute if it is one of the <paramref name="attributes"/>;
        /// otherwise returns <see langword="null"/>.</returns>
        static ComAttribute GetExemptionsComAttribute(IUnknownVector attributes)
        {
            if (attributes != null)
            {
                int count = attributes.Size();
                for (int i = 0; i < count; i++)
                {
                    ComAttribute subattribute = (ComAttribute)attributes.At(i);
                    if (subattribute.Name == "ExemptionCodes")
                    {
                        return subattribute;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates layer objects that correspond to the specified spatial string.
        /// </summary>
        /// <param name="spatialString">The spatial string from which to create the layer object.
        /// </param>
        /// <param name="imageViewer">The image viewer on which the spatial string appears.</param>
        /// <param name="level">The confidence level of the layer objects to create.</param>
        /// <returns>A layer object that corresponds to <paramref name="value"/>.</returns>
        static List<LayerObject> GetLayerObjectsFromSpatialString(SpatialString spatialString,
            ImageViewer imageViewer, ConfidenceLevel level)
        {
            // Get the raster zones of the spatial string, organized by page
            Dictionary<int, List<RasterZone>> pagesToZones = GetRasterZonesByPage(spatialString);

            // Create a layer object for each page of raster zones
            return CreateLayerObjects(pagesToZones, imageViewer, level);
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
        /// <param name="pagesToZones">Pages of raster zones for which to create 
        /// <see cref="LayerObject"/>s.</param>
        /// <param name="imageViewer">The image viewer to which each <see cref="LayerObject"/> 
        /// will be associated.</param>
        /// <param name="level">The confidence level of the layer objects to create.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="LayerObject"/> created from the 
        /// specified raster zones, one for each page of <paramref name="pagesToZones"/>.</returns>
        static List<LayerObject> CreateLayerObjects(
            Dictionary<int, List<RasterZone>> pagesToZones, ImageViewer imageViewer, 
            ConfidenceLevel level)
        {
            // Iterate over each raster zone
            LayerObject previous = null;
            List<LayerObject> layerObjects = new List<LayerObject>(pagesToZones.Count);
            foreach (KeyValuePair<int, List<RasterZone>> pair in pagesToZones)
            {
                // Create a redaction and add it to the result collection
                RedactionLayerObject redaction = new RedactionLayerObject(imageViewer,
                    pair.Key, new string[] { "Redaction" }, pair.Value);
                redaction.Color = level.Color;
                redaction.CanRender = level.Output;
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

        #endregion RedactionGridViewRow Methods
    }
}
