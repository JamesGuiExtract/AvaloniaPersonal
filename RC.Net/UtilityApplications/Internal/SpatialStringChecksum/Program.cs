using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_AFCORELib;
using ComAttribute = UCLID_AFCORELib.Attribute;
using UCLID_COMUTILSLib;

namespace SpatialStringChecksum
{
    /// <summary>
    /// Iterates all data in a USS file for the purpose of comparing the data to another.
    /// Takes one mandatory command line parameter (uss file name) and one optional (/tree).
    /// The output will be a voa file of the same name as the uss file (except extension).
    /// If /tree is not used, the VOA file will have single "Checksum" attribute which is a hash
    /// of all data in the uss file. If tree is used, the VOA file will also have attributes
    /// listing all fields in values in the uss file. This can then be converted to XML for
    /// comparison purposes. The voa files produced when /tree is used can be quite large.
    /// </summary>
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        static bool _outputTree;

        /// <summary>
        /// 
        /// </summary>
        static uint _hash = 0;

        static void Main(string[] args)
        {
            // Load the license files from folder
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

            IUnknownVector vector = new IUnknownVector();

            string fileName = args[0];
            _outputTree = (args.Length > 1 && args[1].Equals("/tree", StringComparison.InvariantCultureIgnoreCase));

            SpatialString ussString = new SpatialString();
            ussString.LoadFrom(fileName, false);

            CheckField(vector, "Text", ussString.String);
            CheckField(vector, "Checksum", ussString.OCREngineVersion);
            CheckField(vector, "SourceDocName", ussString.SourceDocName);

            var lettersVector = CreateScope(vector, "Letters");
            for (int i = 0; i < ussString.Size; i++)
            {
                Letter letter = ussString.GetOCRImageLetter(i);

                var letterVector = CreateScope(lettersVector, "Letter");
                CheckField(letterVector, "Left", letter.Left);
                CheckField(letterVector, "Top", letter.Top);
                CheckField(letterVector, "Right", letter.Right);
                CheckField(letterVector, "Bottom", letter.Bottom);
                CheckField(letterVector, "CharConfidence", letter.CharConfidence);
                CheckField(letterVector, "FontSize", letter.FontSize);
                CheckField(letterVector, "Guess1", letter.Guess1);
                CheckField(letterVector, "IsBold", letter.IsBold);
                CheckField(letterVector, "IsEndOfParagraph", letter.IsEndOfParagraph);
                CheckField(letterVector, "IsEndOfZone", letter.IsEndOfZone);
                CheckField(letterVector, "IsSpatialChar", letter.IsSpatialChar);
                CheckField(letterVector, "PageNumber", letter.PageNumber);
            }

            var ocrZonesVector = CreateScope(vector, "OCR_raster_zones");
            foreach (RasterZone rasterZone in
                ussString.GetOCRImageRasterZones().ToIEnumerable<RasterZone>())
            {
                var ocrZoneVector = CreateScope(ocrZonesVector, "Zone");
                CheckField(ocrZoneVector, "StartX", rasterZone.StartX);
                CheckField(ocrZoneVector, "StartY", rasterZone.StartY);
                CheckField(ocrZoneVector, "EndX", rasterZone.EndX);
                CheckField(ocrZoneVector, "EndY", rasterZone.EndY);
                CheckField(ocrZoneVector, "Height", rasterZone.Height);
            }

            var originalZonesVector = CreateScope(vector, "Original_raster_zones");
            foreach (RasterZone rasterZone in
                ussString.GetOriginalImageRasterZones().ToIEnumerable<RasterZone>())
            {
                var ocrZoneVector = CreateScope(originalZonesVector, "Zone");
                CheckField(ocrZoneVector, "StartX", rasterZone.StartX);
                CheckField(ocrZoneVector, "StartY", rasterZone.StartY);
                CheckField(ocrZoneVector, "EndX", rasterZone.EndX);
                CheckField(ocrZoneVector, "EndY", rasterZone.EndY);
                CheckField(ocrZoneVector, "Height", rasterZone.Height);
            }

            var pagesVector = CreateScope(vector, "PageInfo");
            for (int i = ussString.GetFirstPageNumber(); i <= ussString.GetLastPageNumber(); i++)
            {
                try
                {
                    SpatialPageInfo pageInfo = ussString.GetPageInfo(i);

                    var pageVector = CreateScope(pagesVector, "Page");

                    CheckField(pageVector, "Height", pageInfo.Height);
                    CheckField(pageVector, "Width", pageInfo.Width);
                    CheckField(pageVector, "Deskew", pageInfo.Deskew);
                    CheckField(pageVector, "Orientation", pageInfo.Orientation);
                }
                catch { }
            }

            ComAttribute attribute = new ComAttribute();
            attribute.Name = "Checksum";
            attribute.Value.ReplaceAndDowngradeToNonSpatial(_hash.ToString());
            vector.PushBack(attribute);

            vector.SaveTo(Path.Combine(Path.GetDirectoryName(fileName),
                Path.GetFileNameWithoutExtension(fileName) + ".voa"), false, 
                "");
        }

        static void CheckField(IUnknownVector vector, string name, object value)
        {
            _hash ^= (uint)value.GetHashCode();

            if (_outputTree)
            {
                ComAttribute attribute = new ComAttribute();
                attribute.Name = name;
                attribute.Value.ReplaceAndDowngradeToNonSpatial(value.ToString());
                vector.PushBack(attribute);
            }
        }

        static IUnknownVector CreateScope(IUnknownVector vector, string name)
        {
            _hash = _hash << 1 | _hash >> (32 - 1);

            if (_outputTree)
            {
                ComAttribute attribute = new ComAttribute();
                attribute.Name = name;
                attribute.Value.ReplaceAndDowngradeToNonSpatial("");
                vector.PushBack(attribute);

                return attribute.SubAttributes;
            }

            return null;
        }
    }
}
