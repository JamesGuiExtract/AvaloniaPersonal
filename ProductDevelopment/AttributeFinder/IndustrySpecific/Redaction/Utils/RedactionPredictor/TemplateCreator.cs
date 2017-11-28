using Extract;
using Extract.Utilities;
using Nuance.OmniPage.CSDK;
using System;
using System.Collections.Generic;
using System.IO;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace RedactionPredictor
{
    public partial class Templates
    {
        static void CreateTemplate(string imagePath, int pageNum, string voaPath, string outputDir)
        {
            var voa = new IUnknownVectorClass();
            if (File.Exists(voaPath))
            {
                voa.LoadFrom(voaPath, false);
            }
            CreateTemplate(imagePath, pageNum, voa, outputDir);
        }

        public static void CreateTemplate(string imagePath, int pageNum, IUnknownVector voa, string outputDir)
        {
            IntPtr pageHandle = IntPtr.Zero;
            try
            {
                Directory.CreateDirectory(outputDir);

                ThrowIfFails(() => RecAPI.kRecSetLicense(null, "9d478fe171d5"), "ELI44687", "Unable to license Nuance API");
                ThrowIfFails(() => RecAPI.kRecInit(null, null), "ELI44688", "Unable to initialize Nuance engine");
                ThrowIfFails(() => RecAPI.kRecLoadImgF(0, imagePath, out pageHandle, pageNum - 1), "ELI44689", "Unable to load image",
                    new KeyValuePair<string, string>("Image path", imagePath));
                ThrowIfFails(() => RecAPI.kRecSetPageDescription(0, PAGEDESCRIPTION.LZ_FORM), "ELI44695", "Unable to set page description");
                ThrowIfFails(() => RecAPI.kRecLocateZones(0, pageHandle), "ELI44696", "Unable to locate zones");
                ThrowIfFails(() => RecAPI.kRecCreateFormTemplate(0, pageHandle), "ELI44690", "Unable to create template",
                    new KeyValuePair<string, string>("Image path", imagePath));

                var outputName = Guid.NewGuid().ToString("D") + ".tpt";
                var templatePath = Path.Combine(outputDir, outputName);
                while (File.Exists(templatePath))
                {
                    outputName = Guid.NewGuid().ToString("D") + ".tpt";
                    templatePath = Path.Combine(outputDir, outputName);
                }
                var templateVoaPath = Path.ChangeExtension(templatePath, ".voa");

                if (voa.Size() > 0)
                {
                    RecAPI.kRecGetZoneCount(pageHandle, out int numZones);
                    //RecAPI.kRecGetOCRZoneCount(pageHandle, out int numOCRZones);
                    int zoneIndex = numZones;
                    int voaIndex = 0; // For use as a feature
                    foreach (var a in voa.ToIEnumerable<IAttribute>())
                    {
                        var spatialString = a.Value;
                        if (pageNum != spatialString.GetFirstPageNumber())
                        {
                            continue;
                        }
                        var lRect = spatialString.GetSpecifiedPages(pageNum, pageNum).GetOCRImageBounds();
                        var rect = new RECT { top = lRect.Top, left = lRect.Left, right = lRect.Right, bottom = lRect.Bottom };

                        ZONE zone = new ZONE
                        {
                            fm = FILLINGMETHOD.FM_OMNIFONT,
                            rm = RECOGNITIONMODULE.RM_AUTO,
                            type = ZONETYPE.WT_FLOW,
                            rectBBox = rect
                        };
                        RecAPI.kRecInsertZone(pageHandle, IMAGEINDEX.II_CURRENT, zone, zoneIndex);
                        RecAPI.kRecSetZoneName(pageHandle, zoneIndex, a.Name);
                        RecAPI.kRecSetZoneAttribute(pageHandle, zoneIndex, "Type", a.Type);
                        RecAPI.kRecSetZoneAttribute(pageHandle, zoneIndex, "VoaIndex", voaIndex.AsString());

                        // Find overlapping form zone and add its index as an attribute so that this, probably larger,
                        // zone can be added as a subattribute and used by the rules on pages that this template is applied to
                        for (int i = 0; i < numZones; i++)
                        {
                            RecAPI.kRecGetZoneInfo(pageHandle, IMAGEINDEX.II_CURRENT, out var otherZone, i);
                            var otherRect = otherZone.rectBBox;
                            if (otherZone.type != ZONETYPE.WT_IGNORE &&
                                rect.left < otherRect.right && rect.right > otherRect.left &&
                                rect.top < otherRect.bottom && rect.bottom > otherRect.top )
                            {
                                RecAPI.kRecGetZoneName(pageHandle, i, out string formFieldName);

                                // Template creator/finder needs to handle empty form field names
                                // https://extract.atlassian.net/browse/ISSUE-14918
                                if (string.IsNullOrEmpty(formFieldName))
                                {
                                    RecAPI.kRecSetZoneName(pageHandle, i, "NO_NAME");
                                }
                                RecAPI.kRecSetZoneAttribute(pageHandle, zoneIndex, "FormField", i.AsString());
                                break;
                            }
                        }

                        zoneIndex++;
                        voaIndex++;
                    }
                }

                ThrowIfFails(() => RecAPI.kRecSaveFormTemplate(0, pageHandle, templatePath), "ELI44691", "Unable to save template",
                    new KeyValuePair<string, string>("Image path", imagePath));

                voa.SaveTo(templateVoaPath, true, typeof(AttributeStorageManagerClass).GUID.ToString("B"));
            }
            finally
            {
                try
                {
                    if (pageHandle != IntPtr.Zero)
                    {
                        RecAPI.kRecFreeImg(pageHandle);
                    }
                }
                catch { }

                try
                {
                    RecAPI.kRecQuit();
                }
                catch { }
            }
        }

        private static void ThrowIfFails(Func<RECERR> recApiMethod, string eli, string message, params KeyValuePair<string, string>[] debugData)
        {
            RECERR rc = recApiMethod();
            if (rc != RECERR.REC_OK)
            {
                var uex = new ExtractException(eli, message);
                foreach (var kv in debugData)
                    uex.AddDebugData(kv.Key, kv.Value, false);
                throw uex;
            }
        }
    }
}
