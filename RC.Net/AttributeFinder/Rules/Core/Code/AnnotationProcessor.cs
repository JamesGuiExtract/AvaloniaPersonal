﻿using Newtonsoft.Json.Linq;
using System;
using UCLID_AFCORELib;
using UCLID_AFSELECTORSLib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    [CLSCompliant(false)]
    public static class AnnotationProcessor
    {
        public static IAttribute ProcessAttribute(string fileName, int pageNum, IAttribute attribute, string operationType, string definition)
        {
            switch (operationType.ToUpperInvariant())
            {
                case OperationType.ModifyOperation:
                    return Modify(fileName, pageNum, attribute, definition);
                default:
                    throw new ExtractException("ELI46746", "Unknown operation type: " + operationType);
            }
        }

        private static IAttribute Modify(string fileName, int pageNum, IAttribute attribute, string operation)
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

        private static IAttribute ModifyWithOutputHandler(IAttribute attribute, IOutputHandler oh)
        {
            var attributes = new IUnknownVectorClass();
            attributes.PushBack(attribute);
            var afdoc = new AFDocumentClass
            {
                Attribute = (UCLID_AFCORELib.Attribute)attribute
            };

            oh.ProcessOutput(attributes, afdoc, null);
            return attributes.Size() > 0 ? (IAttribute)attributes.At(0) : null;
        }
    }

    public abstract class Context
    {
        public Context ParentContext { get; set; }

        public Context ClosestAncestor(Func<Context, bool> test)
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
    }

    public abstract class OperationType : Context
    {
        public const string ModifyOperation = "MODIFY";
        public static OperationType Modify { get; set; } = ModifyType.INSTANCE;

        private class ModifyType : OperationType
        {
            private ModifyType() { }
            public static ModifyType INSTANCE { get; set; } = new ModifyType();
        }
    }

    public abstract class RuleObjectModel : Context
    {
        public RuleObjectModel(Context context) { }

        public abstract dynamic Build(dynamic obj);
    }

    [CLSCompliant(false)]
    public class AutoShrinkRedactionZonesModel : RuleObjectModel
    {
        public AutoShrinkRedactionZonesModel(Context context) : base(context) { }

        override public dynamic Build(dynamic obj)
        {
            var x = new AutoShrinkRedactionZones();
            if (obj != null)
            {
                x.AttributeSelector = AttributeSelectorModel
                    .Resolve(this, obj.AttributeSelector)
                    .Build(obj);
            }
            return x;
        }
    }
    public abstract class AttributeSelectorModel: RuleObjectModel
    {
        public AttributeSelectorModel(Context context) : base(context) { }

        public static AttributeSelectorModel Resolve(Context context, dynamic obj)
        {
            if (obj == null)
            {
                return new QueryBasedASModel(context);
            }
            else if (obj.AFQuerySelector != null)
            {
                return new QueryBasedASModel(context);
            }
            throw new ExtractException("ELI46748", "Unknown AttributeSelector type. Path: " + ((JObject)obj).Path);
        }
    }

    [CLSCompliant(false)]
    public class QueryBasedASModel: AttributeSelectorModel
    {
        public QueryBasedASModel(Context context) : base(context) { }

        override public dynamic Build(dynamic obj)
        {
            var x = new QueryBasedASClass();

            if (obj?.QueryText is string query)
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
    }
}
