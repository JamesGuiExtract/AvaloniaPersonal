using Extract;
using System;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using WebAPI.Models;

namespace WebAPI
{
    /// <summary>
    ///  Used to provide translation between attributes represented via
    ///  <see cref="BareDocumentAttributeSet"/> and COM <see cref="IAttribute"/>s.
    /// </summary>
    public class AttributeTranslator
    {
        #region Fields

        IUnknownVector _attributes;
        string _sourceDocName;
        LongToObjectMap _sourceDocPageInfo;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeTranslator"/> class.
        /// </summary>
        /// <param name="documentAttributeSet">The document attribute set.</param>
        /// <param name="sourceDocName">Name of the source document.</param>
        public AttributeTranslator(string sourceDocName, BareDocumentAttributeSet documentAttributeSet)
        {
            try
            {
                _sourceDocName = sourceDocName;
                _sourceDocPageInfo = new LongToObjectMap();

                var sourceDocString = new SpatialString();
                sourceDocString.LoadFrom(_sourceDocName + ".uss", false);
                var pageVector = sourceDocString.SpatialPageInfos.GetKeys();

                int length = pageVector.Size;
                for (int i = 0; i < length; i++)
                {
                    int page = (int)pageVector[i];

                    var oldPageInfo = sourceDocString.GetPageInfo(page);

                    // Create the spatial page info for this page
                    SpatialPageInfo pageInfo = new SpatialPageInfo();
                    pageInfo.Initialize(oldPageInfo.Width, oldPageInfo.Height, EOrientation.kRotNone, 0);

                    // Add it to the map
                    _sourceDocPageInfo.Set(page, pageInfo);
                }

                _attributes = new IUnknownVector();

                foreach (DocumentAttribute attribute in documentAttributeSet.Attributes)
                {
                    IAttribute comAttribute = ConvertAttribute(attribute);

                    _attributes.PushBack(comAttribute);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI45085");
                throw ee;
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets this instance's data as an <see cref="IUnknownVector"/> of (COM) <see cref="IAttribute"/>s.
        /// </summary>
        public IUnknownVector ComAttributes
        {
            get
            {
                return _attributes;
            }
        }

        #endregion Constructors

        #region Private Members

        /// <summary>
        /// Converts the specified <see cref="DocumentAttribute"/> to a COM <see cref="IAttribute"/>.
        /// NOTE: This method assumes spatial data is specified via a <see cref="SpatialLineZone"/>
        /// (not <see cref="SpatialLineBounds"/>).
        /// </summary>
        /// <param name="attribute">The <see cref="DocumentAttribute"/> to convert.</param>
        /// <returns>A COM <see cref="IAttribute"/> representation of <see paramref="attribute"/>.
        /// This value will always be a hybrid attribute if spatial, never a true spatial attribute.
        /// </returns>
        IAttribute ConvertAttribute(DocumentAttribute attribute)
        {
            IAttribute comAttribute = new AttributeClass();

            if (attribute.Name == "Data")
            {
                if (attribute.ConfidenceLevel == "High")
                {
                    comAttribute.Name = "HCData";
                }
                else if (attribute.ConfidenceLevel == "Medium")
                {
                    comAttribute.Name = "MCData";
                }
                else if (attribute.ConfidenceLevel == "Low")
                {
                    comAttribute.Name = "LCData";
                }
                else
                {
                    comAttribute.Name = "Manual";
                }
            }
            else
            {
                comAttribute.Name = attribute.Name;
            }
            comAttribute.Type = attribute.Type;

            comAttribute.Value = new SpatialString();

            if (attribute.HasPositionInfo)
            {
                IUnknownVector rasterZones = new IUnknownVector();

                foreach (var zone in attribute.SpatialPosition.LineInfo.Select(line => line.SpatialLineZone))
                {
                    RasterZone rasterZone = new RasterZone();

                    rasterZone.CreateFromData(
                        zone.StartX,
                        zone.StartY,
                        zone.EndX,
                        zone.EndY,
                        zone.Height,
                        zone.PageNumber);

                    rasterZones.PushBack(rasterZone);
                }

                comAttribute.Value.CreateHybridString(rasterZones, attribute.Value, _sourceDocName, _sourceDocPageInfo);
            }
            else
            {
                comAttribute.Value.CreateNonSpatialString(attribute.Value, _sourceDocName);
            }

            comAttribute.SubAttributes = new IUnknownVector();
            foreach (var subAttribute in attribute.ChildAttributes)
            {
                comAttribute.SubAttributes.PushBack(ConvertAttribute(subAttribute));
            }

            return comAttribute;
        }

        #endregion Private Members
    }
}
