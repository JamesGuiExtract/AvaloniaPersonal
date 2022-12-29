using Extract.ErrorHandling;
using System;
using UCLID_AFCORELib;

namespace ExtractDataExplorer.Models
{
    public class AttributeModel
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public string? Type { get; set; }
        public int Page { get; set; }
        public Guid ID { get; }
        public Guid ParentID { get; }
        public int Index { get; }

        public AttributeModel(IAttribute attribute, Guid id, Guid? parentID, int rank)
        {
            _ = attribute ?? throw new ArgumentNullException(nameof(attribute));

            Name = attribute.Name;
            Value = attribute.Value.String;
            Type = attribute.Type;
            Page = attribute.Value.HasSpatialInfo() ? attribute.Value.GetFirstPageNumber() : 0;
            ID = id;
            ParentID = parentID ?? Guid.Empty;
            Index = rank;
        }
    }
}
