using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace ExtractDataExplorer.Models
{
    public class AttributeModel
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public string? Type { get; set; }
        public int Page { get; set; }
        public IList<Polygon> Zones { get; }
        public string SourceDocName { get; }
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
            Zones = attribute.Value.HasSpatialInfo() ? GetZones(attribute.Value) : Array.Empty<Polygon>();
            SourceDocName = attribute.Value.SourceDocName;
            ID = id;
            ParentID = parentID ?? Guid.Empty;
            Index = rank;
        }

        static IList<Polygon> GetZones(SpatialString value)
        {
            var comZones = value
                .GetOriginalImageRasterZones()
                .ToIEnumerable<IRasterZone>();

            return comZones
                .Select(zone =>
                    new Polygon(zone.PageNumber, zone.GetBoundaryPoints()
                    .ToIEnumerable<IDoublePoint>()
                    .Select(comPoint => new PointF(x: (float)comPoint.X, y: (float)comPoint.Y))
                    .ToList()))
                .ToList();
        }
    }
}
