using System.Collections.Generic;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataCaptureStats
{
    public delegate IEnumerable<AccuracyDetail> CompareAttributesFunc(IUnknownVector expected, IUnknownVector found);
    public delegate bool IncludeFoundDocumentFunc(IAttribute document);
    public delegate bool DocumentsMatchFunc(IAttribute expected, IAttribute found, IEnumerable<int> deletedPagesFromAllExpected);
}
