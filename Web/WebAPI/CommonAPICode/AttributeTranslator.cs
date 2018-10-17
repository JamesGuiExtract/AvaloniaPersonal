﻿using Extract;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using WebAPI.Models;

namespace WebAPI
{
    /// <summary>
    /// Used to provide translation between attributes represented via <see cref="DocumentDataInput"/> 
    /// or <see cref="DocumentDataPatch"/> and COM <see cref="IAttribute"/>s.
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
        /// Initializes a new instance of the <see cref="AttributeTranslator"/> class to replace all
        /// existing attribute data for the document.
        /// </summary>
        /// <param name="sourceDocName">Name of the source document.</param>
        /// <param name="inputDocumentData">The data that should replace the document's exising
        /// attribute data.</param>
        public AttributeTranslator(string sourceDocName, DocumentDataInput inputDocumentData)
        {
            try
            {
                InitializeSourceDocument(sourceDocName);

                _attributes = new IUnknownVector();

                foreach (var attribute in inputDocumentData.Attributes)
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeTranslator"/> class to
        /// add/update/delete attributes into the existing document attributes.
        /// </summary>
        /// <param name="sourceDocName">Name of the source document.</param>
        /// <param name="attributes">The existing document attributes</param>
        /// <param name="documentAttributeSet">Specific attributes to Add/Update/Delete.</param>
        public AttributeTranslator(string sourceDocName, IUnknownVector attributes, DocumentDataPatch documentAttributeSet)
        {
            try
            {
                InitializeSourceDocument(sourceDocName);

                var existingAttributes = attributes.Enumerate()
                    .ToDictionary(a => ((IIdentifiableObject)a.attribute).InstanceGUID, a => a);

                foreach (var patchAttribute in documentAttributeSet.Attributes)
                {
                    Guid guid = Guid.Empty;

                    switch (patchAttribute.Operation)
                    {
                        case PatchOperation.Create:
                            {
                                if (string.IsNullOrWhiteSpace(patchAttribute.ParentAttributeID))
                                {
                                    attributes.PushBack(ConvertAttribute(patchAttribute));
                                }
                                else
                                {
                                    var parent = GetAttribute(patchAttribute.ParentAttributeID, existingAttributes);

                                    parent.attribute.SubAttributes.PushBack(ConvertAttribute(patchAttribute));
                                }
                            }
                            break;

                        case PatchOperation.Update:
                            {
                                var target = GetAttribute(patchAttribute.ID, existingAttributes);

                                var updatedAttribute = ConvertAttribute(patchAttribute, target.attribute);

                                // Because CopyFrom will replace the SubAttributes member while the DocumentDataPatch
                                // instance is not intended to (nor is capable) of defining changes to children, save
                                // off the existing children in order to re-attach them to the parent attribute.
                                var children = target.attribute.SubAttributes;

                                var copyable = (ICopyableObject)target.attribute;
                                copyable.CopyFrom(updatedAttribute);
                                target.attribute.SubAttributes = children;
                            }
                            break;

                        case PatchOperation.Delete:
                            {
                                var target = GetAttribute(patchAttribute.ID, existingAttributes);

                                if (target.parent == null)
                                {
                                    attributes.RemoveValue(target.attribute);
                                }
                                else
                                {
                                    target.parent.SubAttributes.RemoveValue(target.attribute);
                                }
                            }
                            break;
                    }
                }

                _attributes = attributes;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46315");
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
        /// Initializes the spatial info for the specified document name such that spatial strings
        /// can be properly created.
        /// </summary>
        /// <param name="sourceDocName">Name of the source document.</param>
        void InitializeSourceDocument(string sourceDocName)
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
        }

        /// <summary>
        /// Converts the specified <see cref="DocumentAttribute"/> to a COM <see cref="IAttribute"/>
        /// and optionally maps it into targetComAttribute or else creates a new attribute.
        /// When mapping into an exising attribute, existing field values will be left as is unless specified
        /// in the incoming data Position data which, if specified, which will entirely replace any existing
        /// spatial data. Spatial data can be specified via either <see cref="SpatialLineZone"/>
        /// or <see cref="SpatialLineBounds"/>, but if both are specified data from
        /// <see cref="SpatialLineBounds"/> will be ignored.
        /// </summary>
        /// <param name="attributeModel">The <see cref="DocumentAttribute"/> to convert.</param>
        /// <param name="targetComAttribute">The <see cref="IAttribute"/> to be updated or <c>null</c>
        /// to retrun a new COM attribute.</param>
        /// <returns>A COM IAttribute representation of the attribute.
        /// This value will always be a hybrid attribute if spatial, never a true spatial attribute.
        /// </returns>
        IAttribute ConvertAttribute(DocumentAttributeCore attributeModel, IAttribute targetComAttribute = null)
        {
            targetComAttribute = targetComAttribute ?? new AttributeClass();

            if (!string.IsNullOrWhiteSpace(attributeModel.ID))
            {
                targetComAttribute.SetGUID(Guid.Parse(attributeModel.ID));
            }

            if (attributeModel.Name == "Data")
            {
                string confidenceLevel = attributeModel.ConfidenceLevel ??
                    AttributeMapper.DetermineRedactionConfidence(targetComAttribute.Name);

                if (confidenceLevel == "High")
                {
                    targetComAttribute.Name = "HCData";
                }
                else if (confidenceLevel == "Medium")
                {
                    targetComAttribute.Name = "MCData";
                }
                else if (confidenceLevel == "Low")
                {
                    targetComAttribute.Name = "LCData";
                }
                else if (confidenceLevel == "Manual")
                {
                    targetComAttribute.Name = "Manual";
                }
            }
            else if (attributeModel.Name != null)
            {
                targetComAttribute.Name = attributeModel.Name;
            }

            if (attributeModel.Type != null)
            {
                targetComAttribute.Type = attributeModel.Type;
            }

            string value = attributeModel.Value ?? targetComAttribute.Value.String;
            targetComAttribute.Value = new SpatialString();

            bool hasPositionInfo = 
                attributeModel.HasPositionInfo ?? targetComAttribute.Value.HasSpatialInfo();
            if (hasPositionInfo)
            {
                HTTPError.AssertRequest("ELI46400", attributeModel.SpatialPosition?.LineInfo.Count > 0,
                    "Spatial attribute missing spatial data", ("ID", attributeModel.ID, true));

                IUnknownVector rasterZones = new IUnknownVector();

                foreach (var line in attributeModel.SpatialPosition.LineInfo)
                {
                    RasterZone rasterZone = new RasterZone();

                    // When applying data from the provided attributeModel, SpatialLineZones, if supplied
                    // should take priority over SpatialLineBounds.
                    if (line.SpatialLineZone is SpatialLineZone zone)
                    {
                        rasterZone.CreateFromData(
                            zone.StartX,
                            zone.StartY,
                            zone.EndX,
                            zone.EndY,
                            zone.Height,
                            zone.PageNumber);
                    }
                    else if (line.SpatialLineBounds is SpatialLineBounds bounds)
                    {
                        var rect = new LongRectangle()
                        {
                            Left = bounds.Left,
                            Top = bounds.Top,
                            Right = bounds.Right,
                            Bottom = bounds.Bottom
                        };

                        rasterZone.CreateFromLongRectangle(rect, bounds.PageNumber);
                    }
                    else
                    {
                        var error = new HTTPError("ELI46399", StatusCodes.Status400BadRequest,
                            "Spatial attribute missing spatial data");
                        error.AddDebugData("ID", attributeModel.ID, false);
                        throw error;
                    }

                    rasterZones.PushBack(rasterZone);
                }

                targetComAttribute.Value.CreateHybridString(rasterZones, value, _sourceDocName, _sourceDocPageInfo);
            }
            else
            {
                targetComAttribute.Value.CreateNonSpatialString(value, _sourceDocName);
            }

            return targetComAttribute;
        }

        /// <summary>
        /// Converts the specified <see cref="DocumentAttribute"/> to a COM <see cref="IAttribute"/>.
        /// NOTE: This method assumes spatial data is specified via a <see cref="SpatialLineZone"/>
        /// (not <see cref="SpatialLineBounds"/>).
        /// </summary>
        /// <param name="attribute">The <see cref="DocumentAttribute"/> to convert.</param>
        /// <returns>A COM IAttribute representation of the attribute.
        /// This value will always be a hybrid attribute if spatial, never a true spatial attribute.
        /// </returns>
        IAttribute ConvertAttribute(DocumentAttribute attribute)
        {
            var comAttribute = ConvertAttribute((DocumentAttributeCore)attribute);

            if (attribute.ChildAttributes != null)
            {
                comAttribute.SubAttributes = new IUnknownVector();
                foreach (var subAttribute in attribute.ChildAttributes)
                {
                    comAttribute.SubAttributes.PushBack(ConvertAttribute(subAttribute));
                }
            }

            return comAttribute;
        }

        /// <summary>
        /// Gets an attribute and its parent from attributes by
        /// guidString that specifies the target attribute's ID.
        /// </summary>
        /// <param name="guidString">The unique identifier string for the attribute.</param>
        /// <param name="attributes">The attributes to search</param>
        /// <returns></returns>
        static (IAttribute attribute, IAttribute parent) GetAttribute(string guidString,
            Dictionary<Guid, (IAttribute, IAttribute)> attributes)
        {
            HTTPError.AssertRequest("ELI46385", Guid.TryParse(guidString, out Guid guid),
                "Attribute ID invalid; GUID expected", ("GUID", guidString, false));

            HTTPError.Assert("ELI46386", StatusCodes.Status404NotFound,
                attributes.TryGetValue(guid, out (IAttribute, IAttribute) attribute),
                "Attribute not found", ("GUID", guidString, false));

            return attribute;
        }

        #endregion Private Members
    }
}
