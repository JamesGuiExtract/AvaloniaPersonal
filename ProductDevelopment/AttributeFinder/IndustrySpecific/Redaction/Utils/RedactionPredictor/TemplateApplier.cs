using Extract;
using Extract.AttributeFinder;
using Extract.AttributeFinder.Rules;
using Extract.Utilities;
using Nuance.OmniPage.CSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static void ApplyTemplate(string templateDir, string imagePath, string voaPath)
        {
            IntPtr[] templates = null;
            IntPtr fileHandle = IntPtr.Zero;
            try
            {
                var voa = new IUnknownVectorClass();
                if (Directory.Exists(templateDir))
                {
                    ThrowIfFails(() => RecAPI.kRecSetLicense(null, "9d478fe171d5"), "ELI44697", "Unable to license Nuance API");
                    ThrowIfFails(() => RecAPI.kRecInit(null, null), "ELI44692", "Unable to initialize Nuance engine");
                    var templateFiles = Directory.GetFiles(templateDir, "*.tpt");
                    templates = templateFiles.Select(templatePath =>
                        {
                            IntPtr templateHandle = IntPtr.Zero;
                            ThrowIfFails(() => RecAPI.kRecLoadFormTemplate(0, out templateHandle, templatePath), "ELI44693", "Unable to load template",
                                new KeyValuePair<string, string>("Path", templatePath));
                            return templateHandle;
                        }).ToArray();
                }

                if (templates != null)
                {
                    int pageCount = 0;
                    ThrowIfFails(() => RecAPI.kRecOpenImgFile(imagePath, out fileHandle, FILEOPENMODE.IMGF_READ, IMF_FORMAT.FF_TIFNO), "ELI44722", "Unable to open image",
                        new KeyValuePair<string, string>("Image path", imagePath));
                    ThrowIfFails(() => RecAPI.kRecGetImgFilePageCount(fileHandle, out pageCount), "ELI44723", "Unable to obtain page count",
                        new KeyValuePair<string, string>("Image path", imagePath));

                    // Build page info map
                    var pageInfoMap = BuildPageInfoMap(fileHandle, imagePath, pageCount);

                    for (int pageNum = 1; pageNum <= pageCount; pageNum++)
                    {
                        ApplyTemplateToPage(templates, fileHandle, imagePath, pageNum, pageInfoMap, voa);
                    }

                    {
                        var protofeatureRulesetPath = Path.Combine(templateDir, "protofeatureCreator.rsd");
                        var ruleset = LoadOrBuildProtofeatureCreatorRuleset(protofeatureRulesetPath);
                        var uss = new SpatialStringClass();
                        if (File.Exists(imagePath + ".uss"))
                        {
                            uss.LoadFrom(imagePath + ".uss", false);
                        }
                        var attribute = new AttributeClass { Value = uss, SubAttributes = voa };
                        var doc = new AFDocumentClass { Attribute = attribute };
                        voa = (IUnknownVectorClass)ruleset.ExecuteRulesOnText(doc, null, null, null);
                    }

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
                        var uss = new SpatialStringClass();
                        if (File.Exists(imagePath + ".uss"))
                        {
                            uss.LoadFrom(imagePath + ".uss", false);
                        }
                        var attribute = new AttributeClass { Value = uss, SubAttributes = voa };
                        var doc = new AFDocumentClass { Attribute = attribute };
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
                }

                voa.SaveTo(voaPath, true, typeof(AttributeStorageManagerClass).GUID.ToString("B"));
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI44726");
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
                        foreach (var template in templates)
                        {
                            RecAPI.kRecFreeFormTemplate(template);
                        }
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

        private static RuleSetClass LoadOrBuildAttributeClassifierRuleset(string attributeClassifierRulesetPath)
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
                                res.AddSubattributeComponents("ThisAttributeShouldBeClassified", "", "", false, false, false, false, false, false);
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

        private static RuleSetClass LoadOrBuildProtofeatureCreatorRuleset(string protofeatureCreatorRulesetPath)
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
                    res.AddSubattributeComponents("Bitmap", "es:Bitmap(20, 5, .)", "Feature", false, true, false, false, true, false);
                    return res;
                })()
            };
            ruleset.SaveTo(protofeatureCreatorRulesetPath, true);
            return ruleset;
        }

        private static LongToObjectMap BuildPageInfoMap(IntPtr fileHandle, string imagePath, int pageCount)
        {
            var pageInfoMap = new LongToObjectMap();
            for (int pageNum = 1; pageNum <= pageCount; pageNum++)
            {
                var rotate = IMG_ROTATE.ROT_NO;
                var pageHandle = IntPtr.Zero;
                double deskew = 0;
                try
                {
                    ThrowIfFails(() => RecAPI.kRecLoadImg(0, fileHandle, out pageHandle, pageNum - 1), "ELI44694", "Unable to load image page",
                        new KeyValuePair<string, string>("File name", imagePath),
                        new KeyValuePair<string, string>("Page number", pageNum.AsString()));

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
                    pageInfoMap.Set(pageNum, pageInfo);
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
            return pageInfoMap;
        }

        private static void ApplyTemplateToPage(IntPtr[] templates, IntPtr fileHandle, string imagePath, int pageNum, LongToObjectMap pageInfoMap, IUnknownVector voa)
        {
            IntPtr pageHandle = IntPtr.Zero;
            IntPtr formTmplCollection = IntPtr.Zero;
            try
            {
                string matchName = null;
                IntPtr bestMatchingID = IntPtr.Zero;

                ThrowIfFails(() => RecAPI.kRecLoadImg(0, fileHandle, out pageHandle, pageNum - 1), "ELI44724", "Unable to load image page",
                    new KeyValuePair<string, string>("File name", imagePath),
                    new KeyValuePair<string, string>("Page number", pageNum.AsString()));
                RecAPI.kRecFindFormTemplate(0, pageHandle, templates, out formTmplCollection, out bestMatchingID, out var confidence, out var numMatching);
                if (numMatching > 0)
                {
                    RecAPI.kRecGetMatchingInfo(bestMatchingID, out matchName);
                }
                else
                {
                    formTmplCollection = IntPtr.Zero;
                }

                if (matchName != null)
                {
                    File.AppendAllLines("d:\\templatesApplied.txt", new[] { "", matchName, imagePath, pageNum.AsString() });
                    Console.WriteLine(matchName);

                    RecAPI.kRecApplyFormTemplateEx(0, pageHandle, bestMatchingID);
                    RecAPI.kRecGetZoneCount(pageHandle, out int numZones);
                    for (int i=0; i < numZones; i++)
                    {
                        RecAPI.kRecGetZoneAttribute(pageHandle, i, "VoaIndex", out string voaIndex);
                        if (!string.IsNullOrEmpty(voaIndex))
                        {
                            RecAPI.kRecGetZoneAttribute(pageHandle, i, "Type", out string attributeType);
                            RecAPI.kRecGetZoneName(pageHandle, i, out string attributeName);
                            RecAPI.kRecGetZoneInfo(pageHandle, IMAGEINDEX.II_CURRENT, out var userZone, i);
                            var spatialString = ZoneToSpatialString(userZone, " ", imagePath, pageNum, pageInfoMap);

                            var attribute = new AttributeClass
                            {
                                Name = attributeName,
                                Type = attributeType ?? "",
                                Value = spatialString
                            };
                            voa.PushBack(attribute);

                            var templateNameSpatialString = new SpatialStringClass();
                            templateNameSpatialString.CreateNonSpatialString(matchName, imagePath);
                            var templateNameAttribute = new AttributeClass
                            {
                                Name = "TemplateName",
                                Type = "UselessAsFeature",
                                Value = templateNameSpatialString
                            };
                            attribute.SubAttributes.PushBack(templateNameAttribute);

                            var voaIndexSpatialString = new SpatialStringClass();
                            voaIndexSpatialString.CreateNonSpatialString(voaIndex, imagePath);
                            var voaIndexAttribute = new AttributeClass
                            {
                                Name = "AttributeIndex",
                                Type = "UselessAsFeature",
                                Value = voaIndexSpatialString
                            };
                            attribute.SubAttributes.PushBack(voaIndexAttribute);

                            var typeSpatialString = new SpatialStringClass();
                            typeSpatialString.CreateNonSpatialString(attributeType, imagePath);
                            var typeAttribute = new AttributeClass
                            {
                                Name = "TypeFromTemplate",
                                Type = "Feature",
                                Value = typeSpatialString
                            };
                            attribute.SubAttributes.PushBack(typeAttribute);

                            RecAPI.kRecGetZoneAttribute(pageHandle, i, "FormField", out string formField);
                            if (formField != null && int.TryParse(formField, out int zoneIndex))
                            {
                                RecAPI.kRecGetZoneInfo(pageHandle, IMAGEINDEX.II_CURRENT, out var formFieldZone, zoneIndex);
                                RecAPI.kRecGetZoneName(pageHandle, zoneIndex, out string fieldName);
                                spatialString = ZoneToSpatialString(formFieldZone, fieldName, imagePath, pageNum, pageInfoMap);
                                var formFieldAttribute = new AttributeClass
                                {
                                    Name = "FormField",
                                    Value = spatialString
                                };
                                attribute.SubAttributes.PushBack(formFieldAttribute);
                            }
                        }
                    }

                    RecAPI.kRecRecognize(0, pageHandle);
                    RecAPI.kRecGetOCRZoneCount(pageHandle, out var ocrZoneCount);
                    for (int i = 0; i < ocrZoneCount; i++)
                    {
                        RecAPI.kRecGetOCRZoneName(pageHandle, i, out var ocrZoneName);
                        RecAPI.kRecGetOCRZoneText(0, pageHandle, i, out var text);
                        RecAPI.kRecGetOCRZoneInfo(pageHandle, IMAGEINDEX.II_CURRENT, out var ocrZone, i);

                        if (string.IsNullOrEmpty(text))
                        {
                            text = " ";
                        }
                        var spatialString = ZoneToSpatialString(ocrZone, text, imagePath, pageNum, pageInfoMap);
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
            finally
            {
                try
                {
                    if (pageHandle != IntPtr.Zero)
                    {
                        RecAPI.kRecFreeImg(pageHandle);
                    }
                    //if (formTmplCollection != IntPtr.Zero)
                    //{
                    //    RecAPI.kRecFreeFormTemplateCollection(formTmplCollection);
                    //}
                }
                catch { }
            }
        }

        private static SpatialString ZoneToSpatialString(ZONE userZone, string value, string imagePath, int pageNum, LongToObjectMap pageInfoMap)
        {
            var sourceRect = userZone.rectBBox;
            var rect = new LongRectangleClass();
            rect.SetBounds(sourceRect.left, sourceRect.top, sourceRect.right, sourceRect.bottom);
            var zone = new RasterZoneClass();
            zone.CreateFromLongRectangle(rect, pageNum);
            var spatialString = new SpatialStringClass();
            spatialString.CreatePseudoSpatialString(zone, value, imagePath, pageInfoMap);
            return spatialString;
        }
    }
}
