using Extract;
using Extract.AttributeFinder;
using Extract.AttributeFinder.Rules;
using Extract.Utilities;
using Nuance.OmniPage.CSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using UCLID_AFCORELib;
using UCLID_AFOUTPUTHANDLERSLib;
using UCLID_AFSELECTORSLib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace RedactionPredictor
{
    public partial class Templates
    {
        const string _AUTO_ENCRYPT_KEY = @"Software\Extract Systems\AttributeFinder\Settings\AutoEncrypt";
        static ThreadLocal<MiscUtils> _miscUtils = new ThreadLocal<MiscUtils>(() => new MiscUtilsClass());

        static void ApplyTemplate(string templateLibrary, string imagePath, string pageRange, string voaPath, bool classifyAttributes, bool outputAllFormFields)
        {
            IntPtr[] templates = null;
            IntPtr fileHandle = IntPtr.Zero;
            try
            {
                ThrowIfFails(() => RecAPI.kRecSetLicense(null, "9d478fe171d5"), "ELI44697", "Unable to license Nuance API");
                ThrowIfFails(() => RecAPI.kRecInit(null, null), "ELI44692", "Unable to initialize Nuance engine");

                // Load template library
                if (string.Equals(Path.GetExtension(templateLibrary), ".etf", StringComparison.OrdinalIgnoreCase))
                {
                    _miscUtils.Value.AutoEncryptFile(templateLibrary, _AUTO_ENCRYPT_KEY);

                    var str = _miscUtils.Value.GetBase64StringFromFile(templateLibrary);
                    var bytes = Convert.FromBase64String(str);
                    using (var zipFile = new TemporaryFile(true))
                    {
                        File.WriteAllBytes(zipFile.FileName, bytes);
                        ThrowIfFails(() => RecAPI.kRecLoadFormTemplateLibrary(0, zipFile.FileName, true, out templates), "ELI46098", "Unable to load template library");
                    }
                }
                else
                {
                    ThrowIfFails(() => RecAPI.kRecLoadFormTemplateLibrary(0, templateLibrary, true, out templates), "ELI46060", "Unable to load template library");
                }

                // Use templates
                var voa = new IUnknownVectorClass();
                if (templates != null)
                {
                    int pageCount = 0;
                    ThrowIfFails(() => RecAPI.kRecOpenImgFile(imagePath, out fileHandle, FILEOPENMODE.IMGF_READ, IMF_FORMAT.FF_SIZE), "ELI44722", "Unable to open image",
                        new KeyValuePair<string, string>("Image path", imagePath));
                    ThrowIfFails(() => RecAPI.kRecGetImgFilePageCount(fileHandle, out pageCount), "ELI44723", "Unable to obtain page count",
                        new KeyValuePair<string, string>("Image path", imagePath));

                    if (pageRange == null)
                    {
                        // Build page info map
                        var pageInfoMap = BuildPageInfoMap(fileHandle, imagePath, pageCount);

                        for (int pageNum = 1; pageNum <= pageCount; pageNum++)
                        {
                            ApplyTemplateToPage(templates, fileHandle, imagePath, pageNum, pageInfoMap, voa, outputAllFormFields);
                        }
                    }
                    else
                    {
                        UtilityMethods.ValidatePageNumbers(pageRange);
                        var pages = UtilityMethods.GetPageNumbersFromString(pageRange, pageCount, true);

                        // Build page info map
                        var pageInfoMap = BuildPageInfoMap(fileHandle, imagePath, pages);

                        foreach (int pageNum in pages)
                        {
                            ApplyTemplateToPage(templates, fileHandle, imagePath, pageNum, pageInfoMap, voa, outputAllFormFields);
                        }
                    }

                    if (classifyAttributes)
                    {
                        voa = ClassifyAttributes(imagePath, voa, Path.GetDirectoryName(templateLibrary));
                    }
                }

                voa.SaveTo(voaPath, true, typeof(AttributeStorageManagerClass).GUID.ToString("B"));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44726");
            }
            finally
            {
                try
                {
                    if (fileHandle != IntPtr.Zero)
                    {
                        RecAPI.kRecCloseImgFile(fileHandle);
                    }

                    if (templates != null)
                    {
                        RecAPI.kRecFreeFormTemplateArray(templates);
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

        static void ApplyTemplateToPage(IntPtr[] templates, IntPtr fileHandle, string imagePath, int pageNum, LongToObjectMap pageInfoMap, IUnknownVector voa, bool outputAllFormFields)
        {
            IntPtr pageHandle = IntPtr.Zero;
            IntPtr formTmplCollection = IntPtr.Zero;
            try
            {
                string matchName = null;
                IntPtr bestMatchingID = IntPtr.Zero;

                pageHandle = LoadImagePage(imagePath, fileHandle, pageNum);

                int confidence = 0, numMatching = 0;
                var rc = RecAPI.kRecFindFormTemplate(0, pageHandle, templates, out formTmplCollection, out bestMatchingID, out confidence, out numMatching);
                if (rc != RECERR.REC_OK)
                {
                    return;
                }

                if (numMatching > 0)
                {
                    ThrowIfFails(() => RecAPI.kRecGetMatchingInfo(bestMatchingID, out matchName), "ELI46079", "Failed to get matching info");
                }

                if (matchName != null)
                {
                    rc = RecAPI.kRecApplyFormTemplateEx(0, pageHandle, bestMatchingID);
                    if (rc != RECERR.REC_OK)
                    {
                        return;
                    }

                    int numZones = 0;
                    ThrowIfFails(() => RecAPI.kRecGetZoneCount(pageHandle, out numZones), "ELI46081", "Failed to get zone count");
                    for (int i=0; i < numZones; i++)
                    {
                        // Create an attribute for each zone that was added to the template (originates from the VOA the template was created from)
                        RecAPI.kRecGetZoneAttribute(pageHandle, i, "VoaIndex", out string voaIndex);
                        if (!string.IsNullOrEmpty(voaIndex))
                        {
                            // Create the top-level attribute
                            RecAPI.kRecGetZoneAttribute(pageHandle, i, "Type", out string attributeType);
                            string attributeName = null;
                            ThrowIfFails(() => RecAPI.kRecGetZoneName(pageHandle, i, out attributeName), "ELI46084", "Failed to get zone name");
                            RECT[] rects = null;
                            ThrowIfFails(() => RecAPI.kRecGetZoneLayout(pageHandle, IMAGEINDEX.II_CURRENT, out rects, i), "ELI46086", "Failed to get zone layout");
                            var spatialString = ZoneToSpatialString(rects, " ", imagePath, pageNum, pageInfoMap);

                            var attribute = new AttributeClass
                            {
                                Name = attributeName,
                                Type = attributeType ?? "",
                                Value = spatialString
                            };
                            voa.PushBack(attribute);

                            // Add a subattribute that contains the template name (GUID)
                            var templateNameSpatialString = new SpatialStringClass();
                            templateNameSpatialString.CreateNonSpatialString(matchName, imagePath);
                            var templateNameAttribute = new AttributeClass
                            {
                                Name = "TemplateName",
                                Value = templateNameSpatialString
                            };
                            attribute.SubAttributes.PushBack(templateNameAttribute);

                            var voaIndexSpatialString = new SpatialStringClass();
                            voaIndexSpatialString.CreateNonSpatialString(voaIndex, imagePath);
                            var voaIndexAttribute = new AttributeClass
                            {
                                Name = "AttributeIndex",
                                Value = voaIndexSpatialString
                            };
                            attribute.SubAttributes.PushBack(voaIndexAttribute);

                            var typeSpatialString = new SpatialStringClass();
                            typeSpatialString.CreateNonSpatialString(attributeType, imagePath);
                            var typeAttribute = new AttributeClass
                            {
                                Name = "TypeFromTemplate",
                                Value = typeSpatialString
                            };
                            attribute.SubAttributes.PushBack(typeAttribute);

                            RecAPI.kRecGetZoneAttribute(pageHandle, i, "FormField", out string formField);
                            if (formField != null && int.TryParse(formField, out int zoneIndex))
                            {
                                ThrowIfFails(() => RecAPI.kRecGetZoneLayout(pageHandle, IMAGEINDEX.II_CURRENT, out rects, zoneIndex), "ELI46089", "Failed to get zone layout");
                                string fieldName = null;
                                ThrowIfFails(() => RecAPI.kRecGetZoneName(pageHandle, zoneIndex, out fieldName), "ELI46090", "Failed to get zone name");
                                spatialString = ZoneToSpatialString(rects, fieldName, imagePath, pageNum, pageInfoMap);
                                var formFieldAttribute = new AttributeClass
                                {
                                    Name = "FormField",
                                    Value = spatialString
                                };
                                attribute.SubAttributes.PushBack(formFieldAttribute);
                            }
                        }
                    }

                    if (outputAllFormFields)
                    {
                        ThrowIfFails(() => RecAPI.kRecRecognize(0, pageHandle), "ELI46091", "Failed to recognize text");
                        int ocrZoneCount = 0;
                        ThrowIfFails(() => RecAPI.kRecGetOCRZoneCount(pageHandle, out ocrZoneCount), "ELI46092", "Failed to get OCR zone count");
                        for (int i = 0; i < ocrZoneCount; i++)
                        {
                            string ocrZoneName = null;
                            ThrowIfFails(() => RecAPI.kRecGetOCRZoneName(pageHandle, i, out ocrZoneName), "ELI46093", "Failed to get OCR zone name");
                            RecAPI.kRecGetOCRZoneText(0, pageHandle, i, out string text);
                            RECT[] rects = null;
                            ThrowIfFails(() => RecAPI.kRecGetOCRZoneLayout(pageHandle, IMAGEINDEX.II_CURRENT, out rects, i), "ELI46096", "Failed to get OCR zone layout");

                            if (string.IsNullOrEmpty(text))
                            {
                                text = " ";
                            }
                            var spatialString = ZoneToSpatialString(rects, text, imagePath, pageNum, pageInfoMap);
                            var formFieldAttribute = new AttributeClass
                            {
                                Name = "_FormField",
                                Value = spatialString
                            };

                            var fieldNameSpatialString = new SpatialStringClass();
                            fieldNameSpatialString.CreateNonSpatialString(ocrZoneName, imagePath);
                            var fieldNameAttribute = new AttributeClass
                            {
                                Name = "FormFieldName",
                                Value = fieldNameSpatialString
                            };
                            formFieldAttribute.SubAttributes.PushBack(fieldNameAttribute);
                            voa.PushBack(formFieldAttribute);
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
            }
        }

        static IntPtr LoadImagePage(string imagePath, IntPtr fileHandle, int pageNum)
        {
            IntPtr pageHandle = IntPtr.Zero;
            RECERR rc = RecAPI.kRecLoadImg(0, fileHandle, out pageHandle, pageNum - 1);
            // Added IMG_DPI_WARN for https://extract.atlassian.net/browse/ISSUE-15073
            // (since there seems to be no issue with processing these, low-DPI images)
            if (rc != RECERR.REC_OK && rc != RECERR.IMF_PASSWORD_WARN && rc != RECERR.IMG_NOMORE_WARN
                && rc != RECERR.IMF_READ_WARN && rc != RECERR.IMF_COMP_WARN && rc != RECERR.IMG_DPI_WARN)
            {
                // Determine whether this page was able to be loaded despite errors in the document
                bool fail = pageHandle == IntPtr.Zero;

                // Create a scary or friendly exception based on whether page was loaded
                string eli;
                string message;
                if (fail)
                {
                    eli = "ELI44724";
                    message = "Unable to load image page";
                }
                else
                {
                    eli = "ELI45219";
                    message = "Application trace: Image loaded successfully but contains errors.";
                }
                var ue = new ExtractException(eli, message);
                ue.AddDebugData("File name", imagePath, false);
                ue.AddDebugData("Page number", pageNum.AsString(), false);

                if (fail)
                {
                    throw ue;
                }
                else
                {
                    ue.Log();
                }
            }

            return pageHandle;
        }

        static SpatialPageInfo BuildPageInfo(IntPtr fileHandle, string imagePath, int pageNum)
        {
            var rotate = IMG_ROTATE.ROT_NO;
            var pageHandle = IntPtr.Zero;
            double deskew = 0;
            try
            {
                pageHandle = LoadImagePage(imagePath, fileHandle, pageNum);

                RecAPI.kRecDetectImgSkew(0, pageHandle, out int slope, out rotate);
                deskew = Math.Atan2(slope, 1000) * 180 / Math.PI;

                RecAPI.kRecGetImgInfo(0, pageHandle, IMAGEINDEX.II_CURRENT, out var info);

                var pageInfo = new SpatialPageInfoClass();
                EOrientation orient = EOrientation.kRotNone;
                switch (rotate)
                {
                    case IMG_ROTATE.ROT_NO:
                        orient = EOrientation.kRotNone;
                        break;

                    case IMG_ROTATE.ROT_RIGHT:
                        orient = EOrientation.kRotRight;
                        break;

                    case IMG_ROTATE.ROT_DOWN:
                        orient = EOrientation.kRotDown;
                        break;

                    case IMG_ROTATE.ROT_LEFT:
                        orient = EOrientation.kRotLeft;
                        break;
                }
                pageInfo.Initialize(info.Size.cx, info.Size.cy, orient, deskew);
                return pageInfo;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44694");
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
            }
        }

        static LongToObjectMap BuildPageInfoMap(IntPtr fileHandle, string imagePath, int pageCount)
        {
            var pageInfoMap = new LongToObjectMap();
            for (int pageNum = 1; pageNum <= pageCount; pageNum++)
            {
                pageInfoMap.Set(pageNum, BuildPageInfo(fileHandle, imagePath, pageNum));
            }
            return pageInfoMap;
        }

        static LongToObjectMap BuildPageInfoMap(IntPtr fileHandle, string imagePath, IEnumerable<int> pages)
        {
            var pageInfoMap = new LongToObjectMap();
            foreach (int pageNum in pages)
            {
                pageInfoMap.Set(pageNum, BuildPageInfo(fileHandle, imagePath, pageNum));
            }
            return pageInfoMap;
        }

        static SpatialString ZoneToSpatialString(RECT[] rects, string value, string imagePath, int pageNum, LongToObjectMap pageInfoMap)
        {
            var zones = rects.Select(sourceRect =>
            {
                var rect = new LongRectangleClass();
                rect.SetBounds(sourceRect.left, sourceRect.top, sourceRect.right, sourceRect.bottom);
                var zone = new RasterZoneClass();
                zone.CreateFromLongRectangle(rect, pageNum);
                return zone;
            })
            .ToIUnknownVector();

            var spatialString = new SpatialStringClass();

            // Template creator/finder needs to handle empty form field names
            // https://extract.atlassian.net/browse/ISSUE-14918
            if (string.IsNullOrEmpty(value))
            {
                value = " ";
            }

            if (zones.Size() == 1)
            {
                spatialString.CreatePseudoSpatialString((RasterZone)zones.At(0), value, imagePath, pageInfoMap);
            }
            else
            {
                spatialString.CreateHybridString(zones, value, imagePath, pageInfoMap);
            }

            return spatialString;
        }

        static IUnknownVectorClass ClassifyAttributes(string imagePath, IUnknownVectorClass voa, string templateDir)
        {
            var uss = new SpatialStringClass();
            if (File.Exists(imagePath + ".uss"))
            {
                uss.LoadFrom(imagePath + ".uss", false);
            }

            var protofeatureRulesetPath = Path.Combine(templateDir, "protofeatureCreator.rsd");
            var protofeatureCreator = LoadOrBuildProtofeatureCreatorRuleset(protofeatureRulesetPath);
            var attribute = new AttributeClass { Value = uss, SubAttributes = voa };
            var doc = new AFDocumentClass { Attribute = attribute };
            voa = (IUnknownVectorClass)protofeatureCreator.ExecuteRulesOnText(doc, null, null, null);

            bool classifierIsReady = false;
            var attributeClassifierPath = Path.Combine(templateDir, "attributeClassifier.lm");
            if (File.Exists(attributeClassifierPath))
            {
                var lm = LearningMachine.Load(attributeClassifierPath);
                classifierIsReady = lm.IsTrained;
            }

            if (classifierIsReady)
            {
                var attributeClassifierRulesetPath = Path.Combine(templateDir, "attributeClassifier.rsd");
                var ruleset = LoadOrBuildAttributeClassifierRuleset(attributeClassifierRulesetPath);
                doc.Attribute.SubAttributes = voa;
                voa = (IUnknownVectorClass)ruleset.ExecuteRulesOnText(doc, null, null, null);
                var afutil = new AFUtilityClass();
                foreach (var a in voa.ToIEnumerable<IAttribute>())
                {
                    var res = afutil.QueryAttributes(a.SubAttributes, "AttributeType", false);
                    var classAttribute = res.Size() > 0 ? (IAttribute)res.At(0) : null;
                    if (classAttribute != null)
                    {
                        var attributeClass = classAttribute.Value.String;
                        if (string.IsNullOrEmpty(attributeClass))
                        {
                            a.Name = "_LM_" + a.Name;
                        }
                        else
                        {
                            a.Type = attributeClass;
                        }
                    }
                }
            }

            return voa;
        }

        static RuleSetClass LoadOrBuildAttributeClassifierRuleset(string attributeClassifierRulesetPath)
        {
            var ruleset = new RuleSetClass();
            if (File.Exists(attributeClassifierRulesetPath))
            {
                ruleset.LoadFrom(attributeClassifierRulesetPath, false);
                return ruleset;
            }

            // Else build and save
            ((IRunMode)ruleset).RunMode = ERuleSetRunMode.kPassInputVOAToOutput;
            ruleset.FKBVersion = "Latest";
            ruleset.IsSwipingRule = true;
            ruleset.ForInternalUseOnly = false;
            ruleset.GlobalOutputHandler = new ObjectWithDescriptionClass
            {
                Description = "Classify attributes",
                Object = new OutputHandlerSequenceClass
                {
                    ObjectsVector = new[]
                    {
                        new ObjectWithDescriptionClass
                        {
                            Description = "Create a ThisAttributeShouldBeClassified subattribute when name does not start with underscore",
                            Object = new Func<object>(() =>
                            {
                                var res =new CreateAttribute
                                {
                                    Root = "/*/*[not(/AttributeType)][not(starts-with(name(), '_'))]"
                                };
                                res.AddSubAttributeComponents("ThisAttributeShouldBeClassified", "", "", false, false, false, false, false, false);
                                return res;
                            })()
                        },
                        new ObjectWithDescriptionClass
                        {
                            Description = "Run classifier on *{ThisAttributeShouldBeClassified}",
                            Object = new Func<object>(() =>
                            {
                                var res = new RunObjectOnQueryClass
                                {
                                    AttributeQuery = "*{ThisAttributeShouldBeClassified}",
                                };
                                res.SetObjectAndIID(typeof(IOutputHandler).GUID,
                                    new LearningMachineOutputHandler
                                    {
                                        SavedMachinePath = @"<RSDFileDir>\attributeClassifier.lm",
                                        PreserveInputAttributes = true
                                    });
                                return res;
                            })()
                        },
                        new ObjectWithDescriptionClass
                        {
                            Description = "Remove */ThisAttributeShouldBeClassified",
                            Object = new RemoveSubAttributesClass
                            {
                                 AttributeSelector = (IAttributeSelector) new QueryBasedASClass
                                 {
                                     QueryText = "*/ThisAttributeShouldBeClassified"
                                 }
                            }
                        }
                    }.ToIUnknownVector()
                }
            };
            ruleset.SaveTo(attributeClassifierRulesetPath, true);
            return ruleset;
        }

        static RuleSetClass LoadOrBuildProtofeatureCreatorRuleset(string protofeatureCreatorRulesetPath)
        {
            var ruleset = new RuleSetClass();
            if (File.Exists(protofeatureCreatorRulesetPath))
            {
                ruleset.LoadFrom(protofeatureCreatorRulesetPath, false);
                return ruleset;
            }

            // Else build and save
            ((IRunMode)ruleset).RunMode = ERuleSetRunMode.kPassInputVOAToOutput;
            ruleset.FKBVersion = "Latest";
            ruleset.IsSwipingRule = true;
            ruleset.ForInternalUseOnly = false;
            ruleset.GlobalOutputHandler = new ObjectWithDescriptionClass
            {
                Description = "Create protofeatures",
                Object = new Func<object>(() =>
                {
                    var res =new CreateAttribute
                    {
                        Root = "/*/*[not(starts-with(name(), '_'))] "
                    };
                    res.AddSubAttributeComponents("Bitmap", "es:Bitmap(20, 5, ., -1, 1)", "Feature", false, true, false, false, true, false);
                    return res;
                })()
            };
            ruleset.SaveTo(protofeatureCreatorRulesetPath, true);
            return ruleset;
        }
    }
}
