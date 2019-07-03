using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Nuance.OmniPage.CSDK.ArgTypes;
using Nuance.OmniPage.CSDK.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="BarcodeFinder"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("4992FA70-1FF6-4805-B9FE-2C48AB7149E2")]
    [CLSCompliant(false)]
    public interface IBarcodeFinder : IAttributeFindingRule, ICategorizedComponent,
        ICopyableObject, ILicensedComponent, IPersistStream, IIdentifiableObject,
        IConfigurableObject
    {
        IVariantVector Types { get; set; }
    }

    /// <summary>
    /// An <see cref="IAttributeFindingRule"/> that adds the <see cref="T:AFDocument.Attribute"/>
    /// (and children) as a literal output attribute.
    /// </summary>
    [ComVisible(true)]
    [Guid("CF66A9BD-2371-4E62-968D-AE8463329D4F")]
    [CLSCompliant(false)]
    public class BarcodeFinder : IdentifiableObject, IBarcodeFinder
    {
        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Barcode finder";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.Barcodes;

        /// <summary>
        /// Nuances engine is not capable of searching for any given barcode type in the same pass as
        /// all other bar code types. This array groups bar types into passes such that we don't attempt
        /// recognition of bar types in passes with incompatible types. This includes all valid types
        /// but does not in anyway represent the currently configured types.
        /// </summary>
        static BAR_TYPE[][] _PASS_SEQUENCE =
            {
                new[] {
                    BAR_TYPE.BAR_EAN,
                    BAR_TYPE.BAR_UPC_A,
                    BAR_TYPE.BAR_UPC_E,
                    BAR_TYPE.BAR_C39,
                    BAR_TYPE.BAR_C39_EXT,
                    BAR_TYPE.BAR_C128,
                    BAR_TYPE.BAR_CB,
                    BAR_TYPE.BAR_A2of5,
                    BAR_TYPE.BAR_UCC128,
                    BAR_TYPE.BAR_C93,
                    BAR_TYPE.BAR_PDF417,
                    BAR_TYPE.BAR_DMATRIX,
                    BAR_TYPE.BAR_QR,
                    BAR_TYPE.BAR_CODE11,
                    BAR_TYPE.BAR_BOOKLAND,
                    BAR_TYPE.BAR_EAN14,
                    BAR_TYPE.BAR_SSCC18
                },
                new[] {
                    BAR_TYPE.BAR_POSTNET,
                    BAR_TYPE.BAR_4STATE_USPS,
                    BAR_TYPE.BAR_4STATE_AUSPOST
                },
                new[] {
                    BAR_TYPE.BAR_DATABAR_LTD,
                    BAR_TYPE.BAR_DATABAR_EXP
                },
                new[] {
                    BAR_TYPE.BAR_C39_NSS,  // Off by default
                    BAR_TYPE.BAR_PATCH     // Off by default
                },
                new[] {
                    BAR_TYPE.BAR_2of5
                },
                new[] {
                    BAR_TYPE.BAR_ITF
                },
                new[] {
                    BAR_TYPE.BAR_ITF14
                },
                new[] {
                    BAR_TYPE.BAR_PLANET
                },
                new[] {
                    BAR_TYPE.BAR_ITAPOST25
                },
                new[] {
                    BAR_TYPE.BAR_MAT25,  // Off by default
                    BAR_TYPE.BAR_MSI     // Off by default
                }
        };

        /// <summary>
        /// The default list of barcode types to be found.
        /// </summary>
        static BAR_TYPE[] _DEFAULT_TYPES =
            new[] {
                BAR_TYPE.BAR_EAN,
                BAR_TYPE.BAR_UPC_A,
                BAR_TYPE.BAR_UPC_E,
                BAR_TYPE.BAR_C39,
                BAR_TYPE.BAR_C39_EXT,
                BAR_TYPE.BAR_C128,
                BAR_TYPE.BAR_CB,
                BAR_TYPE.BAR_A2of5,
                BAR_TYPE.BAR_UCC128,
                BAR_TYPE.BAR_C93,
                BAR_TYPE.BAR_PDF417,
                BAR_TYPE.BAR_DMATRIX,
                BAR_TYPE.BAR_QR,
                BAR_TYPE.BAR_CODE11,
                BAR_TYPE.BAR_BOOKLAND,
                BAR_TYPE.BAR_EAN14,
                BAR_TYPE.BAR_SSCC18,
                BAR_TYPE.BAR_ITF,
                BAR_TYPE.BAR_POSTNET,
                BAR_TYPE.BAR_4STATE_USPS,
                BAR_TYPE.BAR_4STATE_AUSPOST,
                BAR_TYPE.BAR_2of5,
                BAR_TYPE.BAR_PLANET,
                BAR_TYPE.BAR_ITAPOST25,
                BAR_TYPE.BAR_ITF14,
                BAR_TYPE.BAR_DATABAR_LTD,
                BAR_TYPE.BAR_DATABAR_EXP
        };

        /// <summary>
        /// The makeup value of characters that should be counted as text.
        /// </summary>
        static MAKEUP[] _TEXT_CHARS = new[]
            {
                MAKEUP.R_NORMTEXT,
                MAKEUP.R_VERTTEXT,
                MAKEUP.R_LEFTTEXT,
                MAKEUP.R_RTLTEXT
            };

        /// <summary>
        /// The makeup value of characters that should indicate the end of a single barcode result.
        /// </summary>
        static MAKEUP[] _END_CHARS = new[]
            {
                MAKEUP.R_ENDOFLINE,
                MAKEUP.R_ENDOFPARA,
                MAKEUP.R_ENDOFZONE,
                MAKEUP.R_ENDOFPAGE,
                MAKEUP.R_ENDOFROW
            };

        /// <summary>
        /// Nuance mask to retrieve only confidence from character error level (and exclude the suspect word flag).
        /// </summary>
        const int RE_ERROR_LEVEL_MASK = ~0x80;

        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="BarcodeFinder"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Represents the barcode types currently configured to be found.
        /// </summary>
        BAR_TYPE[] _types = _DEFAULT_TYPES.ToArray();

        /// <summary>
        /// Maps each barcode type to the pass number in which this rule will attempt to find it.
        /// </summary>
        Dictionary<BAR_TYPE, int> _passMapping;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeFinder"/> class.
        /// </summary>
        public BarcodeFinder()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46681");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeFinder"/> class as a copy of
        /// the specified <see paramref="BarcodeFinder"/>.
        /// </summary>
        /// <param name="BarcodeFinder">The <see cref="BarcodeFinder"/> from which
        /// settings should be copied.</param>
        public BarcodeFinder(BarcodeFinder BarcodeFinder)
        {
            try
            {
                CopyFrom(BarcodeFinder);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46682");
            }
        }

        #endregion Constructors

        #region IBarcodeFinder

        /// <summary>
        /// Gets an <see cref="IVariantVector"/> of the barcode types currently configured to be found.
        /// </summary>
        public IVariantVector Types
        {
            get
            {
                try
                {
                    return _types
                        ?.Cast<int>()
                        .ToVariantVector<int>();
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI46990", "Failed to get configured barcode types");
                }
            }

            set
            {
                try
                {
                    var newTypes = value
                        .ToIEnumerable<BAR_TYPE>()
                        .OrderBy(type => type)
                        .ToArray();

                    if (!newTypes.SequenceEqual(_types.OrderBy(type => type)))
                    {
                        _types = newTypes;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI46991", "Failed to configure barcode types");
                }
            }
        }

        #endregion IBarcodeFinder

        #region IAttributeFindingRule

        /// <summary>
        /// Parses the <see paramref="pDocument"/> and returns a vector of found
        /// <see cref="ComAttribute"/> objects.
        /// </summary>
        /// <param name="pDocument">The <see cref="AFDocument"/> to parse.</param>
        /// <param name="pProgressStatus">The <see cref="ProgressStatus"/> to indicate processing
        /// progress.</param>
        /// <returns>An <see cref="IUnknownVector"/> of found <see cref="ComAttribute"/>s.</returns>
        public IUnknownVector ParseText(AFDocument pDocument, ProgressStatus pProgressStatus)
        {
            try
            {
                var results = new IUnknownVector();

                // Rather than allow a non-spatial source end up throwing an exception, simply return nothing.
                if (!pDocument.Text.HasSpatialInfo())
                {
                    return results;
                }

                Engine.SetLicenseKey(null, "9d478fe171d5");
                Engine.Init("Extract Systems", "Extract Systems");

                var passes = GetPassSequence(_types);
                for (int passNumber = 0; passNumber < passes.Length; passNumber++)
                {
                    try
                    {
                        FindBarcodes(pDocument, passes[passNumber], results);
                    }
                    catch (Exception ex)
                    {
                        var ee = ex.AsExtract("ELI47004");
                        ee.AddDebugData("Pass", passNumber + 1, false);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46683", "Failed to find bar codes.");
            }
            finally
            {
                try
                {
                    Engine.ForceQuit();
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI47001");
                }
            }
        }

        /// <summary>
        /// Returns the barcodes types that exist on <see paramref="pDocument"/> based on a single
        /// recognition pass for the specified <see paramref="barCodeTypes"/>. Note that it is
        /// assumed that the specified bar code types are valid recognition targets as part of a
        /// single pass.
        /// </summary>
        /// <param name="pDocument">The <see cref="AFDocument"/> to search.</param>
        /// <param name="barCodeTypes">The <see cref="BAR_TYPE"/>s to search for.</param>
        /// <param name="results">An array of search results to append to.</param>
        static void FindBarcodes(AFDocument pDocument, BAR_TYPE[] barCodeTypes, IUnknownVector results)
        {
            using (SettingCollection settings = new SettingCollection())
            {
                settings.DefaultRecognitionModule = Nuance.OmniPage.CSDK.ArgTypes.RECOGNITIONMODULE.RM_BAR;
                settings.DTXTOutputformat = DTXTOUTPUTFORMATS.DTXT_TXTS;
                settings.BarTypes.Current = barCodeTypes;

                var pagesToSearch = pDocument.Text.GetPages(true, "[BLANK]")
                    .ToIEnumerable<SpatialString>()
                    .Select(p => p.GetFirstPageNumber());
                foreach (var page in pagesToSearch)
                {
                    FindBarcodes(pDocument.Text.SourceDocName, page, settings, results);
                }
            }
        }

        /// <summary>
        /// Returns the barcodes types that exist on the specified document page based on a single
        /// recognition pass for the specified <see paramref="barCodeTypes"/>. Note that it is
        /// assumed that the specified bar code types are valid recognition targets as part of a
        /// single pass.
        /// </summary>
        /// <param name="sourceDocName">The document to search.</param>
        /// <param name="page">The page number to search</param>
        /// <param name="pDocument">The <see cref="AFDocument"/> to search.</param>
        /// <param name="barCodeTypes">The <see cref="BAR_TYPE"/>s to search for.</param>
        /// <param name="results">An array of search results to append to.</param>
        static void FindBarcodes(string sourceDocName, int page, SettingCollection settings, IUnknownVector results)
        {
            try
            {
                var spatialPageInfos = new LongToObjectMap();

                using (Page imagePage = new Page(sourceDocName, page - 1, settings))
                {
                    imagePage.Preprocess();
                    imagePage.Recognize();

                    var letters = imagePage[IMAGEINDEX.II_CURRENT].GetLetters();
                    var currentZoneLetters = new List<LETTER>();

                    foreach(var letter in letters)
                    {
                        if (_TEXT_CHARS.Any(type => letter.makeup.HasFlag(type)))
                        {
                            currentZoneLetters.Add(letter);
                        }

                        if (_END_CHARS.Any(type => letter.makeup.HasFlag(type)))
                        {
                            if (currentZoneLetters.Any())
                            {
                                results.PushBack(CreateAttribute(currentZoneLetters, imagePage, page, sourceDocName, spatialPageInfos));
                                currentZoneLetters.Clear();
                            }
                        }
                    }

                    if (currentZoneLetters.Any())
                    {
                        results.PushBack(CreateAttribute(currentZoneLetters, imagePage, page, sourceDocName, spatialPageInfos));
                    }
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI46993", "Failed to search page for barcodes.", ex);
                ee.AddDebugData("FileName", sourceDocName, false);
                ee.AddDebugData("Page", page, false);
                throw ee;
            }
        }

        /// <summary>
        /// Creates an <see cref="IAttribute"/> to represent the <see paramref="letters"/> recognized
        /// from a barcode.
        /// </summary>
        static IAttribute CreateAttribute(IEnumerable<LETTER> letters, Page imagePage, int pageNumber,
            string sourceDocName, LongToObjectMap spatialPageInfos)
        {
            if (!spatialPageInfos.Contains(pageNumber))
            {
                InitSpatialPageInfo(imagePage, pageNumber, spatialPageInfos);
            }

            var comLetters = letters.Select(zoneLetter =>
            {
                var letter = new Letter();
                letter.Guess1 = (char)zoneLetter.code;
                letter.Bottom = zoneLetter.top + zoneLetter.height;
                letter.CharConfidence = 100 - Math.Min(99, zoneLetter.err & RE_ERROR_LEVEL_MASK);
                letter.FontSize = (int)zoneLetter.pointSize;
                letter.IsBold = (zoneLetter.fontAttrib & FONTATTRIB.R_BOLD) > 0;
                letter.IsItalic = (zoneLetter.fontAttrib & FONTATTRIB.R_ITALIC) > 0;
                letter.IsSpatialChar = !char.IsWhiteSpace(zoneLetter.code);
                letter.IsSubScript = (zoneLetter.fontAttrib & FONTATTRIB.R_SUBSCRIPT) > 0;
                letter.IsSuperScript = (zoneLetter.fontAttrib & FONTATTRIB.R_SUPERSCRIPT) > 0;
                letter.IsUnderline = (zoneLetter.fontAttrib & FONTATTRIB.R_UNDERLINE) > 0;
                letter.Left = zoneLetter.left;
                letter.PageNumber = pageNumber;
                letter.Right = zoneLetter.left + zoneLetter.width;
                letter.Top = zoneLetter.top;
                return letter;
            }).ToList();

            if (!comLetters.Last().IsSpatialChar)
            {
                comLetters.Remove(comLetters.Last());
            }

            comLetters.Last().IsEndOfParagraph = true;
            comLetters.Last().IsEndOfZone = true;

            var spatialString = new SpatialString();
            spatialString.CreateFromILetters(comLetters.ToIUnknownVector<ILetter>(), sourceDocName, spatialPageInfos);

            var attribute = new AttributeClass();
            attribute.Value = spatialString;
            attribute.Type = LETTER.RH_BARTYPE(letters.First().info).ToString();

            return attribute;
        }

        #endregion IAttributeFindingRule

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration of the barcode rule object.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI46978", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (BarcodeFinder)Clone();

                using (BarcodeFinderSettingsDialog dlg = new BarcodeFinderSettingsDialog(cloneOfThis))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dlg.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46979", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="BarcodeFinder"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="BarcodeFinder"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new BarcodeFinder(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46684",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="BarcodeFinder"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as BarcodeFinder;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to BarcodeFinder");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46685",
                    "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        #endregion ICopyableObject Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(_LICENSE_ID);
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns><see cref="HResult.Ok"/> if changes have been made;
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    _types = reader
                        .ReadInt32Array()
                        .Select(type => (BAR_TYPE)type) // .Cast<BAR_TYPE>() doesn't work for some reason
                        .ToArray();

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46686",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the object should reset
        /// its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(_types.Cast<int>().ToArray());

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                // Save the GUID for the IIdentifiableObject interface.
                SaveGuid(stream);

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46687",
                    "Failed to save '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            throw new NotImplementedException();
        }

        #endregion IPersistStream Members

        #region Internal Members

        /// <summary>
        /// Nuance's engine is not capable of searching for any given barcode type in the same pass as
        /// all other bar code types. This method groups the specified bar types into passes such
        /// that we don't attempt recognition of bar types in passes with incompatible types. 
        /// </summary>
        static internal BAR_TYPE[][] GetPassSequence(BAR_TYPE[] types)
        {
            var passSequence = _PASS_SEQUENCE
                .Select(pass => pass
                    .Where(type => types.Contains(type))
                    .ToArray())
                .Where(pass => pass.Any())
                .ToArray();

            return passSequence;
        }

        /// <summary>
        /// Maps each barcode type to the pass number in which this rule will attempt to find it.
        /// </summary>
        internal Dictionary<BAR_TYPE, int> PassMapping
        {
            get
            {
                try
                {
                    int passNumber = 0;

                    if (_passMapping == null)
                    {
                        var passMapping = new Dictionary<BAR_TYPE, int>();

                        foreach (var pass in _PASS_SEQUENCE)
                        {
                            passNumber++;

                            foreach (var type in pass)
                            {
                                passMapping[type] = passNumber;
                            }
                        }

                        _passMapping = passMapping;
                    }

                    return _passMapping;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI46992");
                }
            }
        }

        #endregion Internal Members

        #region Private Members

        /// <summary>
        /// Initialize the <see cref="SpatialPageInfo"/> within <see paramref="spatialPageInfos"/>
        /// for the current page (to be used by any recognized MICR attributes).
        /// </summary>
        static void InitSpatialPageInfo(Page imagePage, int pageNumber, LongToObjectMap spatialPageInfos)
        {
            var size = imagePage[IMAGEINDEX.II_CURRENT].ImageInfo.Size;
            var pageInfo = new SpatialPageInfo();
            var deskew = Math.Atan2(imagePage.PreprocessInfo.Slope, 1000) * (180.0 / Math.PI);
            EOrientation orientation = EOrientation.kRotNone;
            switch (imagePage.PreprocessInfo.Rotation)
            {
                case IMG_ROTATE.ROT_NO:
                    orientation = EOrientation.kRotNone;
                    break;

                case IMG_ROTATE.ROT_RIGHT:
                    orientation = EOrientation.kRotRight;
                    break;

                case IMG_ROTATE.ROT_DOWN:
                    orientation = EOrientation.kRotDown;
                    break;

                case IMG_ROTATE.ROT_LEFT:
                    orientation = EOrientation.kRotLeft;
                    break;

                default:
                    throw new ExtractException("ELI46901", "");
            }
            var width = (orientation == EOrientation.kRotNone || orientation == EOrientation.kRotDown)
                ? size.cx
                : size.cy;
            var height = (orientation == EOrientation.kRotNone || orientation == EOrientation.kRotDown)
                ? size.cy
                : size.cx;
            pageInfo.Initialize(width, height, orientation, deskew);

            spatialPageInfos.Set(pageNumber, pageInfo);
        }

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.ValueFindersGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.ValueFindersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="BarcodeFinder"/> instance into this one.
        /// </summary><param name="source">The <see cref="BarcodeFinder"/> from which to copy.
        /// </param>
        // Even though this currently does nothing, this method is here to keep the ICopyableObject
        // pattern consistent. Block FXCop warnings related to the fact this currently does nothing.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "source")]
        void CopyFrom(BarcodeFinder source)
        {
            _types = source.Types
                .ToIEnumerable<BAR_TYPE>()
                .ToArray();
        }

        #endregion Private Members

        
        // The code below was used to determine which barcode types can be used in the same pass.
        // In case we want to try to update the _PASS_SEQUENCE later, this could be used.
        //
        //passNumber = 0;
        //foreach (var pass in _PASS_SEQUENCE)
        //{
        //    passNumber++;
        //    Debug.WriteLine($"------------------- Pass {passNumber} ----------------");

        //    foreach (var type in Enum.GetValues(typeof(BAR_TYPE)).Cast<BAR_TYPE>()
        //        .Except(pass)
        //        .Except(_PASS_SEQUENCE[0])
        //        .Except(new[] { BAR_TYPE.BAR_SIZE }))
        //    {
        //        try
        //        {
        //            TestBarTypes(pass.Concat(new[] { type })
        //                .ToArray());

        //            Debug.WriteLine($">>>>>>>>>> {type} can be added to pass {passNumber}");
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine(ex.Message);
        //        }
        //    }
        //}
        //
        //static void TestBarTypes(BAR_TYPE[] barCodeTypes)
        //{
        //    using (SettingCollection settings = new SettingCollection())
        //    {
        //        settings.DefaultRecognitionModule = Nuance.OmniPage.CSDK.ArgTypes.RECOGNITIONMODULE.RM_BAR;
        //        settings.DTXTOutputformat = DTXTOUTPUTFORMATS.DTXT_TXTS;
        //        settings.BarTypes.Current = barCodeTypes;
        //    }
        //}
    }
}
