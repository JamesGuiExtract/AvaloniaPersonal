using Extract;
using WebAPI.Models;
using System;
using System.Collections.Generic;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using static WebAPI.Utils;

using WorkflowType = UCLID_FILEPROCESSINGLib.EWorkflowType;

namespace WebAPI
{
    /// <summary>
    /// This class maps FAM Attribute to Web API DocumentAttribute
    /// </summary>
    public class AttributeMapper
    {
        IUnknownVector _attributes;
        WorkflowType _workflowType;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="attributes">attribute set to map to DocumentAttributeSet</param>
        /// <param name="workflowType">type of workflow</param>
        public AttributeMapper(IUnknownVector attributes, WorkflowType workflowType)
        {
            _attributes = attributes;
            _workflowType = workflowType;
        }

        /// <summary>
        /// Is the attribute name an IDShield name?
        /// </summary>
        /// <param name="name">name value</param>
        /// <returns>true iff it is an IDShield name</returns>
        static private bool IsIdShieldName(string name)
        {
            if (name.IsEquivalent("HCData") ||
                name.IsEquivalent("MCData") ||
                name.IsEquivalent("LCData") ||
                name.IsEquivalent("Clues")  ||
                name.IsEquivalent("Manual") )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// For IDShield there is some moderate translation of the name that is performed here.
        /// </summary>
        /// <param name="originalName">attribute name</param>
        private string DetermineName(string originalName)
        {
            var name = originalName;
            if (IsIdShieldName(name) || _workflowType == WorkflowType.kRedaction)
            {
                if (name.IsEquivalent("HCData") ||
                    name.IsEquivalent("MCData") ||
                    name.IsEquivalent("LCData") ||
                    name.IsEquivalent("Manual") )
                {
                    name = "Data";
                }
                else if (name.IsEquivalent("Clues"))
                {
                    // This is done to ensure that differently cased variants of "clues" get translated reliably
                    // into "Clues" always.
                    name = "Clues";
                }
            }

            return name;
        }

        private static  Dictionary<string, string> mapRedactionConfidenceValues = 
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"HCData", Enum.GetName(typeof(ConfidenceLevel), ConfidenceLevel.High) },
                {"MCData", Enum.GetName(typeof(ConfidenceLevel), ConfidenceLevel.Medium) },
                {"LCData", Enum.GetName(typeof(ConfidenceLevel), ConfidenceLevel.Low)},
                {"Manual", Enum.GetName(typeof(ConfidenceLevel), ConfidenceLevel.Manual) }
            };

        /// <summary>
        /// determines redaction confidence - Note that ORIGINAL name is used here!
        /// Note that this is based on ConfidenceLevel enumeration, but returned as the string name
        /// </summary>
        /// <param name="originalName">original Attribute.Name</param>
        /// <returns>confidence level string</returns>
        static internal string DetermineRedactionConfidence(string originalName)
        {
            string confidence;
            var found = mapRedactionConfidenceValues.TryGetValue(originalName, out confidence);
            if (found)
            {
                return confidence;
            }

            return Enum.GetName(typeof(ConfidenceLevel), ConfidenceLevel.NotApplicable);
        }

        /// <summary>
        /// map an IAttribute to a (top-level) DocumentAtribute instance
        /// </summary>
        /// <param name="attr">Attribute instance</param>
        /// <param name="verboseSpatialData"><c>false</c> to include only the spatial data needed for
        /// extract software to represent spatial strings; <c>true</c> to include data that may be
        /// useful to 3rd party integrators.</param>
        /// <returns>DocumentAttribute object</returns>
        private DocumentAttribute MapAttribute(IAttribute attr, bool verboseSpatialData)
        {
            var docAttr = new DocumentAttribute();

            try
            {
                docAttr.ID = ((IIdentifiableObject)attr).InstanceGUID.ToString();
                docAttr.Name = DetermineName(attr.Name);
                docAttr.Type = attr.Type;

                SpatialString spatialString = attr.Value;
                HTTPError.Assert("ELI46365", spatialString != null,
                    "spatial string is null", ("Attribute", docAttr.Name, false));
                docAttr.Value = spatialString.String;

                int minConf = 0;
                int maxConf = 0;
                int avgConf = 0;
                spatialString.GetCharConfidence(ref minConf, ref maxConf, ref avgConf);
                docAttr.AverageCharacterConfidence = avgConf;

                docAttr.ConfidenceLevel = DetermineRedactionConfidence(attr.Name);
                docAttr.HasPositionInfo = spatialString.HasSpatialInfo();

                docAttr.SpatialPosition = spatialString.SetSpatialPosition(verboseSpatialData);

                // Process subattributes, if any
                var childAttrs = attr.SubAttributes;

                docAttr.ChildAttributes = new List<DocumentAttribute>();
                if (childAttrs != null)
                {
                    var childAttrsCount = childAttrs.Size();
                    if (childAttrsCount > 0)
                    {
                        for (int j = 0; j < childAttrsCount; ++j)
                        {
                            var childAttr = (IAttribute)childAttrs.At(j);

                            // Do not transfer metadata attributes to the web API.
                            if (childAttr.Name.StartsWith("_"))
                            {
                                continue;
                            }

                            var childDocAttr = MapAttribute(childAttr, verboseSpatialData);
                            docAttr.ChildAttributes.Add(childDocAttr);
                        }
                    }
                }

                return docAttr;
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Exception: {ex.Message}, while processing attribute: {docAttr.Name}"), "ELI43253");
                throw;
            }
        }

        /// <summary>
        /// map IUnknownVector (of iAttribute) to a document attribute set.
        /// </summary>
        /// <param name="includeNonSpatial"><c>true</c> to include non-spatial attributes in the resulting data;
        /// otherwise, <c>false</c>. NOTE: If false, a non-spatial attribute will be excluded even if it has
        /// spatial children.</param>
        /// <param name="verboseSpatialData"><c>false</c> to include only the spatial data needed for
        /// extract software to represent spatial strings; <c>true</c> to include data that may be
        /// useful to 3rd party integrators.</param>
        /// <returns>corresponding DocumentAttributeSet</returns>
        public DocumentDataResult MapAttributesToDocumentAttributeSet(bool includeNonSpatial, bool verboseSpatialData)
        {
            IAttribute attr = null;

            try
            {
                var rootDocAttrSet = new DocumentDataResult();

                for (int i = 0; i < _attributes.Size(); ++i)
                {
                    attr = (IAttribute)_attributes.At(i);

                    // Do not transfer metadata attributes to the web API.
                    if (attr.Name.StartsWith("_") || (!includeNonSpatial && !attr.Value.HasSpatialInfo()))
                    {
                        continue;
                    }

                    var docAttr = MapAttribute(attr, verboseSpatialData);
                    rootDocAttrSet.Attributes.Add(docAttr);
                }

                return rootDocAttrSet;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46395");
            }
        }
    }
}
