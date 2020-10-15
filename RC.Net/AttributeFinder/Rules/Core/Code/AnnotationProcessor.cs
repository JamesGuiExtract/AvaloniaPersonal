using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UCLID_AFCORELib;
using UCLID_AFSELECTORSLib;
using UCLID_COMUTILSLib;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Rules
{
    [ComVisible(true)]
    [Guid("1AEBE03C-C621-456C-B98B-DA8CF5B7553C")]
    [CLSCompliant(false)]
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class AnnotationProcessor: IAnnotationProcessor
    {
        /// <summary>
        /// Processes an attribute according to a json definition (e.g., a rule object)
        /// </summary>
        /// <param name="bstrFileName">The path to the source image to be used, if needed</param>
        /// <param name="nPageNumber">The page number of the image to use</param>
        /// <param name="pAttribute">The <see cref="ComAttribute"/> to be processed</param>
        /// <param name="bstrOperationType">The type of operation (e.g., "modify")</param>
        /// <param name="bstrDefinition">The operation's definition as JSON</param>
        /// <returns>A modified <see cref="ComAttribute"/></returns>
        public ComAttribute ProcessAttribute(string bstrFileName, int nPageNumber, ComAttribute pAttribute, string bstrOperationType, string bstrDefinition)
        {
            try
            {
                if (bstrOperationType.Equals("Modify", StringComparison.OrdinalIgnoreCase))
                {
                    return Modify(pAttribute, bstrDefinition);
                }
                else
                {
                    throw new ExtractException("ELI46746", "Unknown operation type: " + bstrOperationType);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47072", "Failed to process attribute");
            }
        }

        // TODO: Support deserializing any rule object rather than only supporting AutoShrinkRedactionZones
        // For now, ensure that any attribute will be shrunk, regardless of name
        private static ComAttribute Modify(ComAttribute attribute, string operation)
        {
            dynamic model = JObject.Parse(operation);
            if (model.AutoShrinkRedactionZones != null)
            {
                var oh = new AutoShrinkRedactionZones
                {
                    AttributeSelector = new QueryBasedASClass { QueryText = "*" }
                };

                return ModifyWithOutputHandler(attribute, oh);
            }
            throw new ExtractException("ELI46747", "Unknown operation. Path: " + ((JObject)model).Path);
        }

        private static ComAttribute ModifyWithOutputHandler(ComAttribute attribute, IOutputHandler oh)
        {
            var attributes = new IUnknownVectorClass();
            attributes.PushBack(attribute);
            var afdoc = new AFDocumentClass
            {
                Attribute = attribute
            };

            oh.ProcessOutput(attributes, afdoc, null);
            return attributes.Size() > 0 ? (ComAttribute)attributes.At(0) : null;
        }
    }
}
