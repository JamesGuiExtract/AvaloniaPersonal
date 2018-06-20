using Extract;
using Extract.Utilities;
using Nuance.OmniPage.CSDK;
using System;
using System.Collections.Generic;
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
                ThrowIfFails(() => RecAPI.kRecCreateFormTemplate(0, pageHandle), "ELI44690", "Unable to create template",
                    new KeyValuePair<string, string>("Image path", imagePath));

                if (voa.Size() > 0)
                {
                    RecAPI.kRecGetZoneCount(pageHandle, out int numZones);
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
                var uex = new ExtractException(eli, message);
                foreach (var kv in debugData)
                    uex.AddDebugData(kv.Key, kv.Value, false);
                throw uex;
            }
        }
    }
}
