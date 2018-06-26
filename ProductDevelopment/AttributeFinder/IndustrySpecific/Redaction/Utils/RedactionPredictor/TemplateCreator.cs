using Extract;
using Extract.Utilities;
using Nuance.OmniPage.CSDK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace RedactionPredictor
{
    public partial class Templates
    {
        /// <summary>
        /// The path to the EncryptFile application
        /// </summary>
        static readonly string _ENCRYPT_FILE_APPLICATION =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "EncryptFile.exe");

        static void CreateTemplate(string imagePath, int pageNum, string voaPath, string outputDir)
        {
            var voa = new IUnknownVectorClass();
            if (File.Exists(voaPath))
            {
                voa.LoadFrom(voaPath, false);
            }
            CreateTemplate(imagePath, pageNum, voa, outputDir);
        }

        public static void CreateTemplate(string imagePath, int pageNum, IUnknownVector voa, string templateLibrary)
        {
            IntPtr pageHandle = IntPtr.Zero;
            try
            {
                ThrowIfFails(() => RecAPI.kRecSetLicense(null, "9d478fe171d5"), "ELI44687", "Unable to license Nuance API");
                ThrowIfFails(() => RecAPI.kRecInit(null, null), "ELI44688", "Unable to initialize Nuance engine");
                ThrowIfFails(() => RecAPI.kRecLoadImgF(0, imagePath, out pageHandle, pageNum - 1), "ELI44689", "Unable to load image",
                    new KeyValuePair<string, string>("Image path", imagePath));
                ThrowIfFails(() => RecAPI.kRecSetPageDescription(0, PAGEDESCRIPTION.LZ_FORM), "ELI44695", "Unable to set page description");
                ThrowIfFails(() => RecAPI.kRecLocateZones(0, pageHandle), "ELI44696", "Unable to locate zones");

                // Sometimes this will fail because the image is inappropriate for making a template
                // E.g., will return IMG_ANCHORNOTFOUND_ERR, "Too few anchors were found or none."
                // Log an error if it isn't this case but don't fail completely
                RECERR rc = RECERR.REC_OK;
                try
                {
                    ThrowIfFails(() => rc = RecAPI.kRecCreateFormTemplate(0, pageHandle),
                        "ELI44690", "Unable to create template",
                        new KeyValuePair<string, string>("Image path", imagePath));
                }
                catch (ExtractException uex)
                {
                    if (rc != RECERR.IMG_ANCHORNOTFOUND_ERR)
                    {
                        uex.Log();
                    }
                    return;
                }

                if (voa.Size() > 0)
                {
                    int numZones = 0;
                    ThrowIfFails(() => RecAPI.kRecGetZoneCount(pageHandle, out numZones), "ELI46067", "Failed to get zone count");
                    int zoneIndex = numZones;
                    int voaIndex = 0; // For use as a feature

                    foreach (var a in voa.ToIEnumerable<IAttribute>())
                    {
                        var spatialString = a.Value;
                        if (!spatialString.HasSpatialInfo() || pageNum != spatialString.GetFirstPageNumber())
                        {
                            continue;
                        }
                        var pageBounds = spatialString.GetOCRImagePageBounds(pageNum);
                        var pageSubstring = spatialString.GetSpecifiedPages(pageNum, pageNum);
                        var rects = pageSubstring
                            .GetOCRImageRasterZones()
                            .ToIEnumerable<RasterZone>()
                            .Select(z => z.GetRectangularBounds(pageBounds))
                            .OrderBy(r => r.Top)
                            .ThenBy(r => r.Left);

                        ZONE zone = null;
                        int prevTop = 0;
                        int prevLeft = 0;
                        int prevRight = 0;
                        int prevBottom = 0;
                        foreach (var lRect in rects)
                        {
                            if (zone == null)
                            {
                                var rect = new RECT { top = lRect.Top, left = lRect.Left, right = lRect.Right, bottom = lRect.Bottom };
                                zone = new ZONE
                                {
                                    fm = FILLINGMETHOD.FM_OMNIFONT,
                                    rm = RECOGNITIONMODULE.RM_AUTO,
                                    type = ZONETYPE.WT_FLOW,
                                    rectBBox = rect
                                };

                                ThrowIfFails(() => RecAPI.kRecInsertZone(pageHandle, IMAGEINDEX.II_CURRENT, zone, zoneIndex), "ELI46068", "Failed to insert zone");
                                ThrowIfFails(() => RecAPI.kRecSetZoneName(pageHandle, zoneIndex, a.Name), "ELI46069", "Failed to set zone name");
                                ThrowIfFails(() => RecAPI.kRecSetZoneAttribute(pageHandle, zoneIndex, "Type", a.Type), "ELI46070", "Failed to set Type attribute");
                                ThrowIfFails(() => RecAPI.kRecSetZoneAttribute(pageHandle, zoneIndex, "VoaIndex", voaIndex.AsString()), "ELI46071", "Failed to set VoaIndex attribute");
                            }
                            else
                            {
                                // Ensure 'pizza box' (rectangles must touch)

                                // Top needs to be at least as high as previous bottom
                                int top = Math.Min(lRect.Top, prevBottom);

                                // Left needs to be at least as far left as prev right
                                int left = Math.Min(lRect.Left, prevRight);

                                // Right needs to be at least as far right as prev left
                                int right = Math.Max(lRect.Right, prevLeft);

                                int bottom = lRect.Bottom;
                                var rect = new RECT { top = top, left = left, right = right, bottom = bottom };

                                try
                                {
                                    ThrowIfFails(() => RecAPI.kRecAddZoneRect(pageHandle, IMAGEINDEX.II_CURRENT, rect, zoneIndex),
                                        "ELI46072", "Failed to add zone rect",
                                        new KeyValuePair<string, string>("Image path", imagePath),
                                        new KeyValuePair<string, string>("Page number", pageNum.ToString(CultureInfo.InvariantCulture)));
                                }
                                catch (ExtractException uex)
                                {
                                    uex.Log();

                                    // Sometimes zones won't go together even with the above adjustments
                                    // In this case, use the overall bounds
                                    var bounds = pageSubstring.GetOCRImageBounds();
                                    var boundsRect = new RECT { top = bounds.Top, left = bounds.Left, right = bounds.Right, bottom = bounds.Bottom };
                                    ThrowIfFails(() => RecAPI.kRecAddZoneRect(pageHandle, IMAGEINDEX.II_CURRENT, boundsRect, zoneIndex),
                                        "ELI46097", "Failed to add zone rect",
                                        new KeyValuePair<string, string>("Image path", imagePath),
                                        new KeyValuePair<string, string>("Page number", pageNum.ToString(CultureInfo.InvariantCulture)));
                                }
                            }

                            prevTop = lRect.Top;
                            prevLeft = lRect.Left;
                            prevRight = lRect.Right;
                            prevBottom = lRect.Bottom;
                        }
                        var boundingRect = zone.rectBBox;

                        // Find overlapping form zone and add its index as an attribute so that this, probably larger,
                        // zone can be added as a subattribute and used by the rules on pages that this template is applied to
                        for (int i = 0; i < numZones; i++)
                        {
                            ZONE otherZone = null;
                            ThrowIfFails(() => RecAPI.kRecGetZoneInfo(pageHandle, IMAGEINDEX.II_CURRENT, out otherZone, i), "ELI46073", "Failed to get zone info");
                            var otherRect = otherZone.rectBBox;
                            if (otherZone.type != ZONETYPE.WT_IGNORE &&
                                boundingRect.left < otherRect.right && boundingRect.right > otherRect.left &&
                                boundingRect.top < otherRect.bottom && boundingRect.bottom > otherRect.top )
                            {
                                string formFieldName = null;
                                ThrowIfFails(() => RecAPI.kRecGetZoneName(pageHandle, i, out formFieldName), "ELI46074", "Failed to get zone name");

                                // Template creator/finder needs to handle empty form field names
                                // https://extract.atlassian.net/browse/ISSUE-14918
                                if (string.IsNullOrEmpty(formFieldName))
                                {
                                    ThrowIfFails(() => RecAPI.kRecSetZoneName(pageHandle, i, "NO_NAME"), "ELI46075", "Failed to set zone name");
                                }
                                ThrowIfFails(() => RecAPI.kRecSetZoneAttribute(pageHandle, zoneIndex, "FormField", i.AsString()), "ELI46076", "Failed to set FormField attribute");
                                break;
                            }
                        }

                        zoneIndex++;
                        voaIndex++;
                    }
                }

                using (var zoneFile = new TemporaryFile(true))
                {
                    ThrowIfFails(() => RecAPI.kRecSaveFormTemplate(0, pageHandle, zoneFile.FileName), "ELI44691", "Unable to save template",
                        new KeyValuePair<string, string>("Image path", imagePath));

                    // Load template library
                    TemporaryFile zipFile = null;
                    string zipFileName = templateLibrary;
                    try
                    {
                        if (string.Equals(Path.GetExtension(templateLibrary), ".etf", StringComparison.OrdinalIgnoreCase))
                        {
                            zipFile = new TemporaryFile(true);
                            zipFileName = zipFile.FileName;

                            _miscUtils.Value.AutoEncryptFile(templateLibrary, _AUTO_ENCRYPT_KEY);
                            if (File.Exists(templateLibrary))
                            {
                                var str = _miscUtils.Value.GetBase64StringFromFile(templateLibrary);
                                var bytes = Convert.FromBase64String(str);
                                File.WriteAllBytes(zipFileName, bytes);
                            }
                        }

                        // If library file doesn't exist, ensure that the folder exists
                        if (!File.Exists(templateLibrary))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(templateLibrary));
                        }

                        using (var zipArchive = ZipFile.Open(zipFileName, ZipArchiveMode.Update))
                        {
                            var prefix = Path.GetFileName(imagePath) + "." + pageNum.ToString("D3");
                            var rank = 1;
                            var entryName = prefix + "." + rank.ToString("D8") + ".zon";
                            while (zipArchive.GetEntry(entryName) != null)
                            {
                                entryName = prefix + "." + (++rank).ToString("D8") + ".zon";
                            }
                            zipArchive.CreateEntryFromFile(zoneFile.FileName, entryName);
                        }
                    }
                    finally
                    {
                        if (zipFile != null)
                        {
                            int exitCode = SystemMethods.RunExecutable(
                                _ENCRYPT_FILE_APPLICATION,
                                new[] { zipFile.FileName, templateLibrary }, createNoWindow: true);
                            ExtractException.Assert("ELI46061", "Failed to create output file", exitCode == 0,
                                "Destination file", templateLibrary);
                            zipFile.Dispose();
                        }
                    }
                }
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
                RecAPI.kRecGetLastError(out int errExt, out string errStr);
                RecAPI.kRecGetErrorUIText(rc, errExt, errStr, out string errUIText);
                var uex = new ExtractException(eli, message);
                uex.AddDebugData("Error code", rc, false);
                uex.AddDebugData("Extended error code", errExt, false);
                uex.AddDebugData("Error text", errUIText, false);
                foreach (var kv in debugData)
                    uex.AddDebugData(kv.Key, kv.Value, false);
                throw uex;
            }
        }
    }
}
