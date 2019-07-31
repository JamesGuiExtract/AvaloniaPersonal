using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
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
                switch (bstrOperationType.ToUpperInvariant())
                {
                    case OperationType.ModifyOperation:
                        return Modify(bstrFileName, nPageNumber, pAttribute, bstrDefinition);
                    default:
                        throw new ExtractException("ELI46746", "Unknown operation type: " + bstrOperationType);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47072", "Failed to process attribute");
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="fileName")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="pageNumber")]
        private static ComAttribute Modify(string fileName, int pageNumber, ComAttribute attribute, string operation)
        {
            dynamic model = JObject.Parse(operation);
            if (model.AutoShrinkRedactionZones != null)
            {
                var oh = new AutoShrinkRedactionZonesModel(OperationType.Modify)
                    .Build(model.AutoShrinkRedactionZones);

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

    [Obfuscation(Feature = "renaming", Exclude = true)]
    public abstract class Context
    {
        public Context ParentContext { get; set; }

        public Context ClosestAncestor(Func<Context, bool> test)
        {
            try
            {
                if (test(ParentContext))
                {
                    return ParentContext;
                }
                else
                {
                    return ParentContext?.ClosestAncestor(test);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47076");
            }
        }
    }

    [Obfuscation(Feature = "renaming", Exclude = true)]
    public abstract class OperationType : Context
    {
        public const string ModifyOperation = "MODIFY";
        public static OperationType Modify { get; set; } = ModifyType.INSTANCE;

        private class ModifyType : OperationType
        {
            private ModifyType() { }
            public static ModifyType INSTANCE => new ModifyType();
        }
    }

    [Obfuscation(Feature = "renaming", Exclude = true)]
    public abstract class RuleObjectModel : Context
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="context")]
        protected RuleObjectModel(Context context) { }

        public abstract dynamic Build(dynamic expando);
    }

    [CLSCompliant(false)]
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class AutoShrinkRedactionZonesModel : RuleObjectModel
    {
        public AutoShrinkRedactionZonesModel(Context context) : base(context) { }

        override public dynamic Build(dynamic expando)
        {
            try
            {
                var x = new AutoShrinkRedactionZones();
                if (expando != null)
                {
                    x.AttributeSelector = AttributeSelectorModel
                        .Resolve(this, expando.AttributeSelector)
                        .Build(expando);
                }
                return x;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47073");
            }
        }
    }
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public abstract class AttributeSelectorModel: RuleObjectModel
    {
        protected AttributeSelectorModel(Context context) : base(context) { }

        public static AttributeSelectorModel Resolve(Context context, dynamic expando)
        {
            try
            {
                if (expando == null)
                {
                    return new QueryBasedASModel(context);
                }
                else if (expando.AFQuerySelector != null)
                {
                    return new QueryBasedASModel(context);
                }
                throw new ExtractException("ELI46748", "Unknown AttributeSelector type. Path: " + ((JObject)expando).Path);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47074");
            }
        }
    }

    [CLSCompliant(false)]
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class QueryBasedASModel: AttributeSelectorModel
    {
        public QueryBasedASModel(Context context) : base(context) { }

        override public dynamic Build(dynamic expando)
        {
            try
            {
                var x = new QueryBasedASClass();

                if (expando?.QueryText is string query)
                {
                    x.QueryText = query;
                }
                else
                {
                    var operationType = ClosestAncestor(c => c is OperationType);
                    if (operationType == OperationType.Modify)
                    {
                        x.QueryText = "*";
                    }
                    else
                    {
                        x.QueryText = "HCData|MCData|LCData";
                    }
                }

                return x;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47075");
            }
        }
    }
}
