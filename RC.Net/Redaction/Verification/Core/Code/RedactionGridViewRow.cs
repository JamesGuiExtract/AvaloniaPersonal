using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
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

        #endregion RedactionGridViewRow Fields

        #region RedactionGridViewRow Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionGridViewRow"/> class.
        /// </summary>
        public RedactionGridViewRow(LayerObject layerObject, string text, string category,
            string type)
            : this(new LayerObject[] { layerObject }, text, category, type, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionGridViewRow"/> class.
        /// </summary>
        public RedactionGridViewRow(IEnumerable<LayerObject> layerObjects, string text, 
            string category, string type) : this(layerObjects, text, category, type, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionGridViewRow"/> class.
        /// </summary>
        RedactionGridViewRow(IEnumerable<LayerObject> layerObjects, string text, string category, 
            string type, ComAttribute attribute)
        {
            _attribute = attribute;
            _layerObjects = new List<LayerObject>(layerObjects);
            _text = text;
            _category = category;
            _type = type;
            _firstPage = GetFirstPageNumber();
            _exemptions = new ExemptionCodeList();
        }

        #endregion RedactionGridViewRow Constructors

        #region RedactionGridViewRow Properties

        /// <summary>
        /// Gets or sets the attribute associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <value>The attribute associated with the <see cref="RedactionGridViewRow"/>.</value>
        /// <returns>The attribute associated with the <see cref="RedactionGridViewRow"/>.</returns>
        [CLSCompliant(false)]
        public ComAttribute Attribute
        {
            get
            {
                // TODO: If dirty, recreate attribute

                return _attribute;
            }
            set
            {
                _attribute = value;
            }
        }

        /// <summary>
        /// Gets the layer objects associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>The layer objects associated with the <see cref="RedactionGridViewRow"/>.
        /// </returns>
        public IEnumerable<LayerObject> LayerObjects
        {
            get
            {
                return _layerObjects;
            }
        }

        /// <summary>
        /// Gets the number of layer objects associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>The number of layer objects associated with the <see cref="RedactionGridViewRow"/>.</returns>
        public int LayerObjectCount
        {
            get
            {
                return _layerObjects.Count;
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
                _type = value;
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
                _exemptions = value;
            }
        }

        #endregion RedactionGridViewRow Properties

        #region RedactionGridViewRow Methods

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
        /// <returns>A <see cref="RedactionGridViewRow"/> with information from the specified 
        /// <paramref name="attribute"/> or <see langword="null"/> if the attribute does not 
        /// contain spatial information.</returns>
        [CLSCompliant(false)]
        public static RedactionGridViewRow FromAttribute(ComAttribute attribute, 
            ImageViewer imageViewer)
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
                List<LayerObject> layerObjects = GetLayerObjectsFromSpatialString(value, imageViewer);
                string text = StringMethods.ConvertLiteralToDisplay(value.String);
                string category = attribute.Name;
                string type = attribute.Type;

                return new RedactionGridViewRow(layerObjects, text, category, type, attribute);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26759",
                    "Unable to create grid row for attribute.", ex);
            }
        }

        /// <summary>
        /// Creates layer objects that correspond to the specified spatial string.
        /// </summary>
        /// <param name="spatialString">The spatial string from which to create the layer object.
        /// </param>
        /// <param name="imageViewer">The image viewer on which the spatial string appears.</param>
        /// <returns>A layer object that corresponds to <paramref name="value"/>.</returns>
        static List<LayerObject> GetLayerObjectsFromSpatialString(SpatialString spatialString, 
            ImageViewer imageViewer)
        {
            // Get the raster zones of the spatial string, organized by page
            Dictionary<int, List<RasterZone>> pagesToZones = GetRasterZonesByPage(spatialString);

            // Create a layer object for each page of raster zones
            return CreateLayerObjects(pagesToZones, imageViewer);
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
        /// <returns>A <see cref="List{T}"/> of <see cref="LayerObject"/> created from the 
        /// specified raster zones, one for each page of <paramref name="pagesToZones"/>.</returns>
        static List<LayerObject> CreateLayerObjects(
            Dictionary<int, List<RasterZone>> pagesToZones, ImageViewer imageViewer)
        {
            // Iterate over each raster zone
            LayerObject previous = null;
            List<LayerObject> layerObjects = new List<LayerObject>(pagesToZones.Count);
            foreach (KeyValuePair<int, List<RasterZone>> pair in pagesToZones)
            {
                // Create a redaction and add it to the result collection
                RedactionLayerObject redaction = new RedactionLayerObject(imageViewer,
                    pair.Key, new string[] { "Redaction" }, pair.Value);
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
