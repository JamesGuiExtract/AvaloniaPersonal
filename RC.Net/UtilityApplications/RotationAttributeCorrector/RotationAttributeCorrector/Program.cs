using Extract;
using Extract.Licensing;
using System;
using System.IO;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace RotationAttributeCorrector
{
    class Program
    {
        static readonly string _ATTRIBUTE_STORAGE_MANAGER_GUID =
            typeof(AttributeStorageManagerClass).GUID.ToString("B");

        static void Main(string[] args)
        {
            string exceptionLogFile = (args.Length == 3 && args[1].Equals("/ef", StringComparison.InvariantCultureIgnoreCase))
                ? args[2]
                : null;

            try
            {
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                bool madeChanges = false;
                string fileName = args[0];

                var uss = new SpatialString();
                uss.LoadFrom(fileName + ".uss", false);

                var attributes = new IUnknownVector();
                attributes.LoadFrom(fileName + ".voa", false);

                int count = attributes.Size();
                for (int i = 0; i < count; i++)
                {
                    var attribute = (IAttribute)attributes.At(i);
                    var value = attribute.Value;
                    if (value.HasSpatialInfo())
                    {
                        int page = value.GetFirstPageNumber();
                        var attributePageInfo = value.GetPageInfo(page);

                        if (attributePageInfo.Orientation != EOrientation.kRotNone ||
                            (Math.Abs(attributePageInfo.Deskew - 0) > 0.05))
                        {
                            var ocrPageInfo = uss.GetPageInfo(page);

                            if (attributePageInfo.Orientation == ocrPageInfo.Orientation &&
                                (Math.Abs(attributePageInfo.Deskew - ocrPageInfo.Deskew) <= 0.05))
                            {
                                SpatialPageInfo pageInfo = new SpatialPageInfo();
                                int width = ocrPageInfo.Width;
                                int height = ocrPageInfo.Height;
                                pageInfo.Initialize(width, height, EOrientation.kRotNone, 0);

                                value.SetPageInfo(page, pageInfo);
                                madeChanges = true;
                            }
                        }
                    }
                }

                if (madeChanges)
                {
                    attributes.SaveTo(fileName + ".fixed.voa", false, _ATTRIBUTE_STORAGE_MANAGER_GUID);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI44804");
                ee.Log(exceptionLogFile);
            }
        }
    }
}
