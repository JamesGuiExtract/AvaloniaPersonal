using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Nuance.OmniPage.CSDK.ArgTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCRLib;
using ComAttribute = UCLID_AFCORELib.Attribute;
using OCRParam = Extract.Utilities.Union<(int key, int value), (int key, double value), (string key, int value), (string key, double value), (string key, string value)>;

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
        /// <summary>
        /// Gets an <see cref="IVariantVector"/> of the barcode types currently configured to be found.
        /// </summary>
        IVariantVector Types { get; set; }

        /// <summary>
        /// Whether to use OCRParameters specified in the containing ruleset or the input spatial string
        /// (if <c>true</c>) or to use only built-in parameters (if <c>false</c>)
        /// </summary>
        bool InheritOCRParameters { get; set; }
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
        const int _CURRENT_VERSION = 2;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.Barcodes;

        static readonly BAR_TYPE[] _indexToBarType = new BAR_TYPE[]
            {
                BAR_TYPE.BAR_EAN,
                BAR_TYPE.BAR_UPC_A,
                BAR_TYPE.BAR_UPC_E,
                BAR_TYPE.BAR_ITF,
                BAR_TYPE.BAR_C39,
                BAR_TYPE.BAR_C39_EXT,
                BAR_TYPE.BAR_C128,
                BAR_TYPE.BAR_CB,
                BAR_TYPE.BAR_POSTNET,
                BAR_TYPE.BAR_A2of5,
                BAR_TYPE.BAR_UCC128,
                BAR_TYPE.BAR_2of5,
                BAR_TYPE.BAR_C93,
                BAR_TYPE.BAR_PATCH,
                BAR_TYPE.BAR_PDF417,
                BAR_TYPE.BAR_PLANET,
                BAR_TYPE.BAR_DMATRIX,
                BAR_TYPE.BAR_C39_NSS,
                BAR_TYPE.BAR_QR,
                BAR_TYPE.BAR_MAT25,
                BAR_TYPE.BAR_CODE11,
                BAR_TYPE.BAR_ITAPOST25,
                BAR_TYPE.BAR_MSI,
                BAR_TYPE.BAR_BOOKLAND,
                BAR_TYPE.BAR_ITF14,
                BAR_TYPE.BAR_EAN14,
                BAR_TYPE.BAR_SSCC18,
                BAR_TYPE.BAR_DATABAR_LTD,
                BAR_TYPE.BAR_DATABAR_EXP,
                BAR_TYPE.BAR_4STATE_USPS,
                BAR_TYPE.BAR_4STATE_AUSPOST,
            };

        static readonly Dictionary<BAR_TYPE, int> _barcodeTypeToMatrixIndex =
            _indexToBarType
            .Select((barType, idx) => (barType: barType, idx: idx))
            .ToDictionary(t => t.barType, t => t.idx);

        // From Nuance 20 RecAPI help file
        // BAR barcode recognition module page
        static readonly byte[][] _barcodeTypeCompatibilityMatrix = new byte[][]
        {
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 },
            new byte [] { 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 0 },
            new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 0 },
            new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 0 },
            new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 0, 0, 0 },
            new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0 },
            new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0 },
            new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0 },
            new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
            new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }
        };


        /// <summary>
        /// These are not necessarily the types that will be searched when using auto mode
        /// but they were the ones that were returned using default OCR engine settings when I tried it
        /// </summary>
        internal static readonly BAR_TYPE[] AutoBarTypes =
            new BAR_TYPE[]
                {
                  BAR_TYPE.BAR_EAN,
                  BAR_TYPE.BAR_UPC_A,
                  BAR_TYPE.BAR_UPC_E,
                  BAR_TYPE.BAR_ITF,
                  BAR_TYPE.BAR_C39,
                  BAR_TYPE.BAR_C39_EXT,
                  BAR_TYPE.BAR_C128,
                  BAR_TYPE.BAR_CB,
                  BAR_TYPE.BAR_UCC128,
                  BAR_TYPE.BAR_C93,
                  BAR_TYPE.BAR_PDF417,
                  BAR_TYPE.BAR_DMATRIX,
                  BAR_TYPE.BAR_QR,
                  BAR_TYPE.BAR_BOOKLAND,
                  BAR_TYPE.BAR_EAN14,
                  BAR_TYPE.BAR_SSCC18,
                };

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
        BAR_TYPE[] _typesInternal = AutoBarTypes.ToArray();

        /// <summary>
        /// Cache of pass sequence
        /// </summary>
        private List<List<BAR_TYPE>> _passSequence;

        BAR_TYPE[] _types
        {
            get
            {
                return _typesInternal;
            }
            set
            {
                _typesInternal = value;
                _passSequence = GetPassSequence(_typesInternal);
            }
        }

        static ThreadLocal<ScansoftOCRClass> _ocrEngine = new ThreadLocal<ScansoftOCRClass>(() =>
        {
            var engine = new ScansoftOCRClass();
            engine.InitPrivateLicense(LicenseUtilities.GetMapLabelValue(new MapLabel()));
            return engine;
        });

        bool _inheritOCRParameters;

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
        /// the specified <see paramref="barcodeFinder"/>.
        /// </summary>
        /// <param name="barcodeFinder">The <see cref="BarcodeFinder"/> from which
        /// settings should be copied.</param>
        public BarcodeFinder(BarcodeFinder barcodeFinder)
        {
            try
            {
                CopyFrom(barcodeFinder);
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

        /// <summary>
        /// Whether to use OCRParameters specified in the containing ruleset or the input spatial string
        /// (if <c>true</c>) or to use only built-in parameters (if <c>false</c>)
        /// </summary>
        public bool InheritOCRParameters
        {
            get
            {
                return _inheritOCRParameters;
            }
            set
            {
                _dirty = _dirty || _inheritOCRParameters != value;
                _inheritOCRParameters = value;
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

                foreach (var pass in _passSequence)
                {
                    try
                    {
                        FindBarcodes(pDocument, pass, results);
                    }
                    catch (Exception ex)
                    {
                        var ee = ex.AsExtract("ELI47004");
                        var readable = string.Join(",", pass.Select(t => t.ToString()));
                        ee.AddDebugData("Pass", readable, false);
                        throw ee;
                    }
                }

                results.ReportMemoryUsage();
                return results;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46683", "Failed to find bar codes.");
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
        void FindBarcodes(AFDocument pDocument, List<BAR_TYPE> barCodeTypes, IUnknownVector results)
        {
            var pagesToSearch = string.Join(",",
                pDocument.Text.GetPages(false, "")
                .ToIEnumerable<SpatialString>()
                .Select(p => p.GetFirstPageNumber()));

            IOCRParameters ocrParams = GetOCRParams(pDocument, barCodeTypes);
            var recognized =
                _ocrEngine.Value.RecognizeTextInImage2(pDocument.Text.SourceDocName,
                    pagesToSearch, true, null, ocrParams);


            var resultVector =
                (from line in recognized.GetLines().ToIEnumerable<SpatialString>()
                 where line.HasSpatialInfo()
                 let barType = GetBarType(line)
                 where barType.HasValue
                 select new AttributeClass
                 {
                     Value = line,
                     Type = barType.ToString()
                 }).ToIUnknownVector();

            results.Append(resultVector);
        }

        private static BAR_TYPE? GetBarType(SpatialString line)
        {
            Letter letter = new LetterClass();
            line.GetNextOCRImageSpatialLetter(0, ref letter);
            if (Enum.IsDefined(typeof(BAR_TYPE), letter.Guess2))
            {
                return (BAR_TYPE)letter.Guess2;
            }
            else
            {
                return null;
            }

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

                    if (reader.Version >= 2)
                    {
                        InheritOCRParameters = reader.ReadBoolean();
                    }

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
                    writer.Write(InheritOCRParameters);

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
        /// Get the number of passes that this list of types will require
        /// </summary>
        /// <param name="barTypes">The collection of types to search for</param>
        public int GetNumberOfPasses(IEnumerable<BAR_TYPE> barTypes)
        {
            try
            {
                return GetPassSequence(barTypes).Count;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46992");
            }
        }
        #endregion Internal Members

        #region Private Members

        static bool IsCompatible(HashSet<BAR_TYPE> existing, BAR_TYPE candidate)
        {
            if (existing.Count == 0)
            {
                return true;
            }

            var restricted = new HashSet<BAR_TYPE>(
                _barcodeTypeCompatibilityMatrix[_barcodeTypeToMatrixIndex[candidate]]
                .Select((b, i) => (incompatible: b == 0, barType: _indexToBarType[i]))
                .Where(x => x.incompatible)
                .Select(x => x.barType));

            restricted.IntersectWith(existing);
            return restricted.Count == 0;
        }

        /// <summary>
        /// Nuance's engine is not capable of searching for any given barcode type in the same pass as
        /// all other bar code types. This method groups the specified bar types into passes such
        /// that we don't attempt recognition of bar types in passes with incompatible types. 
        /// </summary>
        static List<List<BAR_TYPE>> GetPassSequence(IEnumerable<BAR_TYPE> barTypes)
        {
            var barTypesRemaining = new HashSet<BAR_TYPE>(barTypes);
            var sequence = new List<List<BAR_TYPE>>();

            while (barTypesRemaining.Count > 0)
            {
                var thisPass = new HashSet<BAR_TYPE>();
                foreach (var t in barTypesRemaining.ToList())
                {
                    if (IsCompatible(thisPass, t))
                    {
                        thisPass.Add(t);
                        barTypesRemaining.Remove(t);
                    }
                }
                sequence.Add(thisPass.ToList());
            }

            return sequence;
        }

        IOCRParameters GetOCRParams(AFDocument pDocument, List<BAR_TYPE> barTypes)
        {
            bool truf<T> (T _) { return true; }

            var ocrParams = InheritOCRParameters
                ?  ((IHasOCRParameters)pDocument).OCRParameters
                    .ToIEnumerable()
                    .Where(p => p.Match(
                        kv => (EOCRParameter)kv.key != EOCRParameter.kOCRType,
                        truf, truf, truf, truf))
                    .ToList()
                : new List<OCRParam>();

            ocrParams.Add(new OCRParam(((int)EOCRParameter.kOCRType, (int)EOCRFindType.kFindBarcodesOnly)));

            if (barTypes != null)
            {
                ocrParams.AddRange(barTypes.Select(t => new OCRParam(((int)EOCRParameter.kBarCodeType, (int)t))));
            }

            return ocrParams.ToOCRParameters();
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
        void CopyFrom(BarcodeFinder source)
        {
            _types = source.Types
                .ToIEnumerable<BAR_TYPE>()
                .ToArray();

            InheritOCRParameters = source.InheritOCRParameters;
        }

        #endregion Private Members
    }
}
