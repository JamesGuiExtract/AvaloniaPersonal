// EnhanceOCR.cpp : Implementation of CEnhanceOCR

#include "stdafx.h"
#include "EnhanceOCR.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <ExtractZoneAsImage.h>
#include <StringTokenizer.h>
#include <RegExLoader.h>
#include <RuleSetProfiler.h>
#include <AFTagManager.h>
#include <LeadToolsLicenseRestrictor.h>

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

static CMutex gMutex;
static bool gbLoggedUnlicensedException = false;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCURRENT_VERSION				= 1;
// How many total shades are available for purposes of image filtering.
const unsigned long gnFILTER_SHADE_COUNT			= 256;
// The color white (used to "erase" image areas)
const COLORREF gnCOLOR_WHITE						= RGB(255, 255, 255);
// The amount of additional confidence that should be associated with the original result to help
// prevent cases when the original result was correct and replaced with something that is not.
const long gnORIGINAL_RESULT_CONF_BIAS				= 10;
// The amount of additional confidence that should be associated with a result that matches the
// preferred format over once that does not.
const long gnPREFERRED_FORMAT_CONF_BIAS				= 30;
// An OCR result must be this much better than the existing result's confidence for the existing
// result to be thrown out.
const long gnSUBSTANTIALLY_BETTER_CONF_MARGIN		= 10;
// An OCR result must be within this much of the existing result's confidence to be considered.
const long gnMINIMUM_CONF_MARGIN					= 7;
// A space character
const CPPLetter gLETTER_SPACE =
	CPPLetter(' ', ' ', ' ', 0, 0, 0, 0, 0, false, false, false, 0, 0, 0);
//-------------------------------------------------------------------------------------------------
// A gaussian filter variation that weights all pixels x pixels from the center the same as all
// pixels x + 1 and the same as all pixels x + 2, etc. This is a good general filter that can
// make text more readable to the OCR engine in a variety contexts depending upon the threshold
// used.
const L_INT gltFilter[49] =
{
    0,  0,  3,  3,  3,  0,  0,
	0,  3,  4,  4,  4,  3,  0,
	3,  4,  6,  6,  6,  4,  3,
	3,  4,  6, 48,  6,  4,  3,
	3,  4,  6,  6,  6,  4,  3,
	0,  3,  4,  4,  4,  3,  0,
    0,  0,  3,  3,  3,  0,  0
};
//-------------------------------------------------------------------------------------------------
// A larger version of gltFilter
const L_INT gltLargeFilter[81] =
{
	0,  0,  1,  1,  1,  1,  1,  0,  0,
    0,  1,  1,  2,  2,  2,  1,  1,  0,
	1,  1,  2,  3,  3,  3,  2,  1,  1,
	1,  2,  3,  5,  5,  5,  3,  2,  1,
	1,  2,  3,  5, 36,  5,  3,  2,  1,
	1,  2,  3,  5,  5,  5,  3,  2,  1,
	1,  1,  2,  3,  3,  3,  2,  1,  1,
    0,  1,  1,  2,  2,  2,  1,  1,  0,
	0,  0,  1,  1,  1,  1,  1,  0,  0
};
//-------------------------------------------------------------------------------------------------
// A smaller verion of gltFilter
const L_INT gltSmallFilter[25] =
{
	3,  4,  4,  4,  3,
	4,  6,  6,  6,  4,
	4,  6, 48,  6,  4,
	4,  6,  6,  6,  4,
	3,  4,  4,  4,  3
};
//-------------------------------------------------------------------------------------------------
// Level 1 (or low setting) filter package
string g_arrL1Filters[] = {
	"medium-45"};				// Versatile
//-------------------------------------------------------------------------------------------------
// Level 2 (or medium setting) filter package
string g_arrL2Filters[] = {
	"medium-45",				// Versatile
	"despeckle",				// For speckles
	"small-20",					// Aliased diffuse
	"medium-85"};				// For heavily shaded regions
//-------------------------------------------------------------------------------------------------
// Level 3 (or high setting) filter package
// NOTE: Though the filters medium-85 or medium-15 are not likely to produce good OCR, for the
// zones they do improve, best results are obtained by running them before most others since
// running other filters on these zones first sometimes produces incorrect results that have high
// enough confidence to prevent the correct result from being used later on. These two filters
// rarely produce OCR results for zones that aren't helped by these filters so they tend to do
// little harm by being run earlier in the sequence-- the biggest harm is slower performance since
// these two filters end up being run for any zones that they do not improve.
string g_arrL3Filters[] = {
	"medium-45",				// Versatile
	"medium-85",				// For heavily shaded regions (see note above)
	"medium-15",				// For very diffuse/broken text (see note above)
	"small-60",					// Smudged or overexposed text
	"despeckle",				// For speckles
	"small-20"};
//-------------------------------------------------------------------------------------------------
// Filter package tailored to work best on regions with halftones or speckles.
string g_arrHalftoneSpeckledFilters[] = {
	"medium-45",				// Versatile
	"medium-85",				// For heavily shaded regions
	"medium-55->large-45"};		// For speckles
//-------------------------------------------------------------------------------------------------
// Filter package tailored to work best on regions with aliased or diffuse text
string g_arrAliasedDiffuseFilters[] = {
	"small-20",					// For aliased or diffuse text.
	"medium-40",				// Versatile
	"small-25"};				// For aliased or diffuse text.
//-------------------------------------------------------------------------------------------------
// Filter package tailored to work best on regions with smudged or overexposed text
string g_arrSmudgedFilters[] = {
	"small-60",					// Smudged or overexposed text
	"gaussian-1"};				// Smudged or overexposed text
//-------------------------------------------------------------------------------------------------
// A factor to be used to normalize character widths based upon the general effect various filters
// have on the resulting character width.
map<string, double> createAlgorithmWidthFactorMap()
{
	map<string, double> mapWidthFactor;

	// 12.73 = average width among filters
	mapWidthFactor["medium-45"] = 12.73 / 12.45;
	mapWidthFactor["medium-85"]	= 12.73 / 12.78;
	mapWidthFactor["gaussian-1+medium-15"]	= 12.73 / 13.48;
	mapWidthFactor["medium-15"]	= 12.73 / 13.48;
	mapWidthFactor["small-60"] = 12.73 / 11.99;
	mapWidthFactor["despeckle"]	= 12.73 / 12.37;
	mapWidthFactor["small-20"] = 12.73 / 13.37;
	mapWidthFactor["medium-55+large-45"] = 12.73 / 12.49;
	mapWidthFactor["gaussian-4"] = 12.73 / 12.89;

	return mapWidthFactor;
}
map<string, double> g_mapALGORITHM_WIDTH_FACTOR = createAlgorithmWidthFactorMap();

//-------------------------------------------------------------------------------------------------
// Global Functions
//-------------------------------------------------------------------------------------------------
static bool isWordChar(unsigned short usChar)
{
	char c = (char)usChar;

	return (c >= 'A' && c <= 'Z') ||
		   (c >= 'a' && c <= 'z') ||
		   (c >= '0' && c <= '9') ||
		   c == '%';
}

//-------------------------------------------------------------------------------------------------
// CEnhanceOCR
//-------------------------------------------------------------------------------------------------
CEnhanceOCR::CEnhanceOCR()
: m_bDirty(false)
, m_pFilters(__nullptr)
, m_nFilterCount(0)
, m_ipProgressStatus(__nullptr)
, m_ipTagUtility(__nullptr)
, m_ipCurrentDoc(__nullptr)
, m_bProcessFullDoc(false)
, m_nCurrentPage(0)
, m_apPageBitmap(__nullptr)
, m_apFilteredBitmapFileName(__nullptr)
, m_ipPageInfoMap(__nullptr)
, m_ipCurrentPageText(__nullptr)
, m_ipCurrentPageInfo(__nullptr)
, m_ipPageRasterZone(__nullptr)
, m_ipPageRect(__nullptr)
, m_nAvgPageCharWidth(0)
, m_ipSpatialStringSearcher(__nullptr)
, m_ipSRICA(__nullptr)
, m_ipPreferredFormatParser(__nullptr)
, m_ipOCREngine(__nullptr)
{
	try
	{
		reset();

		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI36455", m_ipMiscUtils != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36456");
}
//-------------------------------------------------------------------------------------------------
CEnhanceOCR::~CEnhanceOCR()
{
	try
	{
		m_ipProgressStatus = __nullptr;
		m_ipTagUtility = __nullptr;
		m_ipCurrentDoc = __nullptr;
		m_apPageBitmap.reset(__nullptr);
		m_apFilteredBitmapFileName.reset(__nullptr);
		m_ipPageInfoMap =__nullptr;
		m_ipCurrentPageText = __nullptr;
		m_ipCurrentPageInfo = __nullptr;
		m_ipPageRasterZone = __nullptr;
		m_ipPageRect = __nullptr;
		m_ipSpatialStringSearcher = __nullptr;
		m_ipSRICA =__nullptr;
		m_ipPreferredFormatParser = __nullptr;
		m_ipOCREngine = __nullptr;

		m_vecHighConfRects.clear();
		m_vecResults.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36457");
}
//-------------------------------------------------------------------------------------------------
HRESULT CEnhanceOCR::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IEnhanceOCR
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::get_ConfidenceCriteria(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36458", pVal != __nullptr);

		validateLicense();

		*pVal = m_nConfidenceCriteria;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36459")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::put_ConfidenceCriteria(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_nConfidenceCriteria = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36460")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::get_FilterPackage(EFilterPackage *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36461", pVal != __nullptr);

		validateLicense();

		*pVal = (EFilterPackage)m_eFilterPackage;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36462")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::put_FilterPackage(EFilterPackage newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_eFilterPackage = (UCLID_AFVALUEMODIFIERSLib::EFilterPackage)newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36463")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::get_CustomFilterPackage(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36464", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strCustomFilterPackage).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36465")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::put_CustomFilterPackage(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strCustomFilterPackage = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36466")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::get_PreferredFormatRegexFile(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36467", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strPreferredFormatRegexFile).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36468")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::put_PreferredFormatRegexFile(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strPreferredFormatRegexFile = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36469")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::get_CharsToIgnore(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36470", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strCharsToIgnore).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36471")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::put_CharsToIgnore(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strCharsToIgnore = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36472")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::get_OutputFilteredImages(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36473", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bOutputFilteredImages);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36474")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::put_OutputFilteredImages(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bOutputFilteredImages = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36475")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::EnhanceDocument(IAFDocument* pDocument, ITagUtility* pTagUtility, 
										  IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bProcessFullDoc = true;

		IAFDocumentPtr ipAFDoc(pDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI36666", ipAFDoc != __nullptr);

		IHasOCRParametersPtr ipHasOCRParameters(ipAFDoc);
		IOCRParametersPtr ipOCRParameters = ipHasOCRParameters->OCRParameters;

		m_ipTagUtility = pTagUtility;
		ASSERT_RESOURCE_ALLOCATION("ELI36721", m_ipTagUtility != __nullptr);

		m_ipProgressStatus = pProgressStatus;

		enhanceOCR(ipAFDoc, ipOCRParameters);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36667");
	
	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::raw_ModifyValue(IAttribute* pAttribute, 
										 IAFDocument* pOriginInput, 
										 IProgressStatus* pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		IAttributePtr ipAttribute(pAttribute);
		ASSERT_ARGUMENT("ELI36476", ipAttribute != __nullptr);
		ASSERT_ARGUMENT("ELI36477", pOriginInput != __nullptr);

		// check licensing
		validateLicense();

		m_bProcessFullDoc = false;

		IIUnknownVectorPtr ipVector(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI36478", ipVector != __nullptr);

		ipVector->PushBack(ipAttribute);

		// [https://extract.atlassian.net/browse/ISSUE-12051]
		// There appears to be a possible corruption issue perhaps related to underlying
		// LeadTools code. We get random failures when trying to create DC instances on
		// images (ELI24891) and when trying to find lines within SRICA (MLI03285.20).
		// At this point it seems unlikely we will be able to find and fix the underlying cause
		// in time for the 9.8 release. For now, add one retry to (hopefully) mask the issue.
		bool bRetried = false;
		while (true)
		{
			try
			{
				try
				{
					enhanceOCR(ipVector, pOriginInput);

					// If the call succeeded, break out of the retry loop.
					break;
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36718");
			}
			catch (UCLIDException &ue)
			{
				if (!bRetried)
				{
					UCLIDException uexOuter("ELI36719",
						"Enhance OCR attempt failed; retrying...", ue);
						uexOuter.log();

					bRetried = true;
					continue;
				}

				throw ue;
			}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36479");
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::raw_ProcessOutput(IIUnknownVector * pAttributes, IAFDocument * pDoc,
										   IProgressStatus* pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bProcessFullDoc = false;

		// [https://extract.atlassian.net/browse/ISSUE-12051]
		// There appears to be a possible corruption issue perhaps related to underlying
		// LeadTools code. We get random failures when trying to create DC instances on
		// images (ELI24891) and when trying to find lines within SRICA (MLI03285.20).
		// At this point it seems unlikely we will be able to find and fix the underlying cause
		// in time for the 9.8 release. For now, add one retry to (hopefully) mask the issue.
		bool bRetried = false;
		while (true)
		{
			try
			{
				try
				{
					enhanceOCR(pAttributes, pDoc);

					// If the call succeeded, break out of the retry loop.
					break;
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36716");
			}
			catch (UCLIDException &ue)
			{
				if (!bRetried)
				{
					UCLIDException uexOuter("ELI36717",
						"Enhance OCR attempt failed; retrying...", ue);
						uexOuter.log();

					bRetried = true;
					continue;
				}

				throw ue;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36480");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IDocumentPreprocessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::raw_Process(IAFDocument* pDocument, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bProcessFullDoc = false;

		IAFDocumentPtr ipAFDoc(pDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI36481", ipAFDoc != __nullptr);

		IHasOCRParametersPtr ipHasOCRParameters(ipAFDoc);
		IOCRParametersPtr ipOCRParameters = ipHasOCRParameters->OCRParameters;

		m_ipProgressStatus = pProgressStatus;

		enhanceOCR(ipAFDoc, ipOCRParameters);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36482");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI36483", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Enhance OCR").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36484")
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI36485", pbValue != __nullptr);

		bool bConfigured = true;
		if (m_eFilterPackage == kCustom && m_strCustomFilterPackage.empty())
		{
			bConfigured = false;
		}

		*pbValue = asVariantBool(bConfigured);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36486");
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_AFVALUEMODIFIERSLib::IEnhanceOCRPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI36487", ipCopyThis != __nullptr);

		m_nConfidenceCriteria			= ipCopyThis->ConfidenceCriteria;
		m_eFilterPackage				= ipCopyThis->FilterPackage;
		m_strCustomFilterPackage		= asString(ipCopyThis->CustomFilterPackage);
		m_strPreferredFormatRegexFile	= asString(ipCopyThis->PreferredFormatRegexFile);
		m_strCharsToIgnore				= asString(ipCopyThis->CharsToIgnore);
		m_bOutputFilteredImages			= asCppBool(ipCopyThis->OutputFilteredImages);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36488");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI36489", pObject != __nullptr);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_EnhanceOCR);
		ASSERT_RESOURCE_ALLOCATION("ELI36490", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36491");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI36492", pClassID != __nullptr);

		*pClassID = CLSID_EnhanceOCR;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36493");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36494");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI36495", pStream != __nullptr);

		// Reset data members
		reset();
		
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCURRENT_VERSION)
		{
			// Throw exception
			UCLIDException ue("ELI36496", "Unable to load newer enhance OCR instance!");
			ue.addDebugInfo("Current Version", gnCURRENT_VERSION);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read data members from the stream
		dataReader >> m_nConfidenceCriteria;
		long nTemp;
		dataReader >> nTemp;
		m_eFilterPackage = (UCLID_AFVALUEMODIFIERSLib::EFilterPackage)nTemp;
		dataReader >> m_strCustomFilterPackage;
		dataReader >> m_strPreferredFormatRegexFile;
		dataReader >> m_strCharsToIgnore;
		dataReader >> m_bOutputFilteredImages;

		loadGUID(pStream);

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36497");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI36498", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCURRENT_VERSION;

		// Write the data members to the stream
		dataWriter << m_nConfidenceCriteria;
		dataWriter << (long)m_eFilterPackage;
		dataWriter << m_strCustomFilterPackage;
		dataWriter << m_strPreferredFormatRegexFile;
		dataWriter << m_strCharsToIgnore;
		dataWriter << m_bOutputFilteredImages;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36499");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI36500", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36501");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IEnhanceOCR,
			&IID_IAttributeModifyingRule,
			&IID_IOutputHandler,
			&IID_IDocumentPreprocessor,
			&IID_IPersistStream,
			&IID_ICategorizedComponent,
			&IID_ICopyableObject,
			&IID_ILicensedComponent,
			&IID_IIdentifiableObject
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36502")

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCR::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36503")
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::enhanceOCR(IAFDocumentPtr ipAFDoc, IOCRParametersPtr ipOCRParameters)
{
	try
	{
		try
		{
			m_ipCurrentDoc = ipAFDoc;
			ASSERT_RESOURCE_ALLOCATION("ELI36722", m_ipCurrentDoc != __nullptr);

			ISpatialStringPtr ipDocText = ipAFDoc->Text;
			ASSERT_RESOURCE_ALLOCATION("ELI36504", ipDocText != __nullptr);

			m_strSourceDocName = asString(ipDocText->SourceDocName);
			
			string strUSSFilename = m_strSourceDocName + ".uss";
	
			if (!isValidFile(strUSSFilename))
			{
				UCLIDException ue("ELI37626", "USS file is required for Enhance OCR.");
				ue.addDebugInfo("Filename", strUSSFilename);
				throw ue;
			}

			unlockDocumentSupport();

			initializeFilters();

			long nPageCount = getNumberOfPagesInImage(asString(ipDocText->SourceDocName));

			// Initialize progress status object
			if (m_ipProgressStatus != __nullptr)
			{
				// Each filter will count as a progress item, plus an addition item for the initial
				// preparation of the image page.
				m_ipProgressStatus->InitProgressStatus("Enhance OCR", 0,
					nPageCount * (m_nFilterCount + 1), VARIANT_TRUE);
			}

			// Loop to process OCR one page at a time.
			ISpatialStringPtr ipResult = __nullptr;
			for (long nPage = 1; nPage <= nPageCount; nPage++)
			{
				ISpatialStringPtr ipPageText(__nullptr);

				// [https://extract.atlassian.net/browse/ISSUE-12051]
				// There appears to be a possible corruption issue perhaps related to underlying
				// LeadTools code. We get random failures when trying to create DC instances on
				// images (ELI24891) and when trying to find lines within SRICA (MLI03285.20).
				// At this point it seems unlikely we will be able to find and fix the underlying cause
				// in time for the 9.8 release. For now, add one retry to (hopefully) mask the issue.
				bool bRetried = false;
				while (true)
				{
					try
					{
						try
						{
							ipPageText = enhanceOCR(ipAFDoc, nPage, ipOCRParameters);

							// If the call succeeded, break out of the retry loop.
							break;
						}
						CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36714");
					}
					catch (UCLIDException &ue)
					{
						// We really don't know where we are at progress-wise at this point; don't
						// update the progress status any further.
						m_ipProgressStatus = __nullptr;

						if (!bRetried)
						{
							UCLIDException uexOuter("ELI36715",
								"Enhance OCR attempt failed; retrying...", ue);
								uexOuter.log();

							bRetried = true;
							continue;
						}

						throw ue;
					}
				}

				// Append the resulting text to the overall result.
				if (ipPageText != __nullptr && ipPageText->HasSpatialInfo())
				{
					if (ipResult == __nullptr)
					{
						ipResult = ipPageText;
					}
					else
					{
						ipResult->Append(ipPageText);
					}
				}
			}

			if (ipResult == __nullptr)
			{
				ipResult.CreateInstance(CLSID_SpatialString);
				ASSERT_RESOURCE_ALLOCATION("ELI36505", ipResult != __nullptr);
			}

			// Replace the original document text with the processed result.
			ipAFDoc->Text = ipResult;

			if (m_ipProgressStatus != __nullptr)
			{
				m_ipProgressStatus->CompleteCurrentItemGroup();
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36506")
	}
	catch (UCLIDException &ue)
	{
		ue.addDebugInfo("SourceDocName", m_strSourceDocName);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
ISpatialStringPtr CEnhanceOCR::enhanceOCR(IAFDocumentPtr ipAFDoc, long nPage, IOCRParametersPtr ipOCRParameters)
{
	try
	{
		try
		{
			ISpatialStringPtr ipResult = __nullptr;
			vector<unique_ptr<OCRResult>> vecResults;
			vector<OCRResult*> vecBestResults;

			// Initialize variable for processsing nPage.
			ISpatialStringSearcherPtr ipSearcher = setCurrentPage(ipAFDoc, nPage);

			ILongRectanglePtr ipRectToProcess = getPageRect();
			
			// If there is no area on the current page to be processed return right away.
			if (ipRectToProcess == __nullptr)
			{
				return __nullptr;
			}

			// Get the text in the area of the page to process.
			ICopyableObjectPtr ipCopyThis = ipSearcher->GetDataInRegion(ipRectToProcess, VARIANT_FALSE);
			ASSERT_RESOURCE_ALLOCATION("ELI36507", ipCopyThis != __nullptr);

			// Clone this page's text so that it can be manipulated without affecting the original document
			// text. (The original text is to be replaced only by the calling function at the end of
			// processing.)
			ISpatialStringPtr ipPageText = ipCopyThis->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI36508", ipPageText != __nullptr);

			// https://extract.atlassian.net/browse/ISSUE-12087
			// So that the Enhance OCR rule object can be used in re-usable contexts but only
			// provide benefit when licensed, when not licensed simply return the existing page
			// text.
			if (!isEnhanceOCRLicensed())
			{
				return ipPageText;
			}

			// Since the page text will be used against a 1 page filtered copy of the current page,
			// set as page 1.
			bool bHasSpatialInfo = asCppBool(ipPageText->HasSpatialInfo());
			if (bHasSpatialInfo && nPage != -1)
			{
				ipPageText->UpdatePageNumber(1);
				ipPageText->SetPageInfo(1, m_ipCurrentPageInfo);
			}

			// Create another copy of the text that will be used as the basis for the eventual result.
			ipResult = ipCopyThis->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI36509", ipResult != __nullptr);

			// Identify the zones on the the current page to enhance.
			vector<ILongRectanglePtr> vecRectsToEnhance;
			if (bHasSpatialInfo)
			{
				m_vecHighConfRects = removeHighConfidenceText(ipPageText);

				vecRectsToEnhance = prepareImagePage(ipPageText, m_vecHighConfRects);
			}
			else
			{
				vecRectsToEnhance = prepareImagePage(ipPageText, vector<ILongRectanglePtr>());
			}

			// Initialize zones for each area identified for enhancement.
			// Expand the processing zone a bit as this tends to produce better OCR results. Also,
			// the filter used to prepare the image may have removed some pixels around the edges
			// that SRICA won't see.
			// Expand a little more horizontally (with respect to the text orientation) than
			// vertically as expanding vertically too much may pick up content from lines above or
			// below.
			vector<ZoneData> vecZonesToEnhance =
				createZonesFromRects(vecRectsToEnhance, ipSearcher, CSize(4, 2));

			enhanceOCR(vecZonesToEnhance, ipOCRParameters);

			// Apply the enhanced OCR text back to the original page text.
			for each (ZoneData zoneData in vecZonesToEnhance)
			{
				applyZoneResults(ipResult, zoneData);
			}

			return ipResult;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36510")
	}
	catch (UCLIDException &ue)
	{
		ue.addDebugInfo("Page", nPage);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::enhanceOCR(IIUnknownVector *pAttributes, IAFDocument *pDoc)
{
	m_ipCurrentDoc = pDoc;
	ASSERT_RESOURCE_ALLOCATION("ELI36723", m_ipCurrentDoc != __nullptr);

	unlockDocumentSupport();

	initializeFilters();

	IAFDocumentPtr ipDocument(pDoc);
	ASSERT_ARGUMENT("ELI36511", ipDocument != __nullptr);

	IHasOCRParametersPtr ipHasOCRParameters(ipDocument);
	IOCRParametersPtr ipOCRParameters = ipHasOCRParameters->OCRParameters;

	IIUnknownVectorPtr ipAttributes(pAttributes);
	ASSERT_RESOURCE_ALLOCATION("ELI36512", ipAttributes != __nullptr);
	long nAttributeCount = ipAttributes->Size();

	// Loop through all input attributes to sort each attribute line by the page on which it is
	// located.
	map<long, vector<ILongRectanglePtr>> mapZonesToEnhance;
	map<ILongRectanglePtr, IAttributePtr> mapOriginalAttributes;
	for (long nIndex = 0; nIndex < nAttributeCount; nIndex++)
	{
		IAttributePtr ipAttribute = ipAttributes->At(nIndex);
		ASSERT_RESOURCE_ALLOCATION("ELI36513", ipAttribute != __nullptr);

		ISpatialStringPtr ipAttributeValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI36514", ipAttributeValue != __nullptr);

		// Skip this attribute if it is non-spatial
		if (ipAttributeValue->HasSpatialInfo() == VARIANT_FALSE)
		{
			continue;
		}

		IIUnknownVectorPtr ipAttributeZones = ipAttributeValue->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI36515", ipAttributeZones != __nullptr);

		// Loop through all zones to sort each by the page on which it is located.
		long nZoneCount = ipAttributeZones->Size();
		for (long nZone = 0; nZone < nZoneCount; nZone++)
		{
			IRasterZonePtr ipRasterZone = ipAttributeZones->At(nZone);
			ASSERT_RESOURCE_ALLOCATION("ELI36678", ipRasterZone != __nullptr);

			long nPage = ipRasterZone->GetPageNumber();

			ILongRectanglePtr ipPageRect(CLSID_LongRectangle);
			ASSERT_RESOURCE_ALLOCATION("ELI36679", ipPageRect != __nullptr);

			ipPageRect = ipAttributeValue->GetOriginalImagePageBounds(nPage);
			ASSERT_RESOURCE_ALLOCATION("ELI36680", ipPageRect != __nullptr);

			ILongRectanglePtr ipRect = ipRasterZone->GetRectangularBounds(ipPageRect);
			ASSERT_RESOURCE_ALLOCATION("ELI36516", ipRect != __nullptr);

			mapZonesToEnhance[nPage].push_back(ipRect);
			mapOriginalAttributes[ipRect] = ipAttribute;
		}
	}

	// Process each page for which zones have been collected.
	for (map<long, vector<ILongRectanglePtr>>::iterator pageIter = mapZonesToEnhance.begin();
		 pageIter != mapZonesToEnhance.end(); pageIter++)
	{
		long nPage = pageIter->first;

		vector<ILongRectanglePtr> vecRectsToEnhance = pageIter->second;

		// Initialize this page for processing.
		ISpatialStringSearcherPtr ipSearcher = setCurrentPage(ipDocument, nPage);

		// https://extract.atlassian.net/browse/ISSUE-12087
		// So that the Enhance OCR rule object can be used in re-usable contexts but only
		// provide benefit when licensed, when not licensed simply return the existing page
		// text.
		if (isEnhanceOCRLicensed())
		{
			// Shrink each rect down to it's pixel content to minimize the amount of processing that
			// needs to be done.
			vector<ILongRectanglePtr>::iterator vecIter;
			for (vecIter = vecRectsToEnhance.begin(); vecIter != vecRectsToEnhance.end(); )
			{
				ILongRectanglePtr ipRect = *vecIter;
				ASSERT_RESOURCE_ALLOCATION("ELI36517", ipRect != __nullptr);

				UCLID_AFVALUEMODIFIERSLib::ISplitRegionIntoContentAreasPtr ipSRICA = getSRICA();
				ASSERT_RESOURCE_ALLOCATION("ELI36518", ipSRICA != __nullptr);

				ipSRICA->ShrinkToFit(m_strSourceDocName.c_str(), nPage, ipRect);

				// https://extract.atlassian.net/browse/ISSUE-12532
				// Delete if has no area
				CRect rect;
				ipRect->GetBounds(&rect.left, &rect.top, &rect.right, &rect.bottom);
				if (rect.IsRectEmpty())
				{
					vecIter = vecRectsToEnhance.erase(vecIter);
				}
				else
				{
					++vecIter;
				}
			}
			// Remove all but vecRectsToEnhance from the image page. 
			prepareImagePage(vecRectsToEnhance);
		}

		// Initialize zones for each area identified for enhancement.
		vector<ZoneData> vecZonesToEnhance = createZonesFromRects(vecRectsToEnhance, ipSearcher);

		// Process the zones.
		if (isEnhanceOCRLicensed())
		{
			enhanceOCR(vecZonesToEnhance, ipOCRParameters);
		}

		// Loop through each zone to apply the result back to the original attribute's value.
		for each (ZoneData zoneData in vecZonesToEnhance)
		{
			IAttributePtr ipAttribute = mapOriginalAttributes[zoneData.m_ipRect];
			ASSERT_RESOURCE_ALLOCATION("ELI36519", ipAttribute != __nullptr);

			ISpatialStringPtr ipAttributeValue = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI36520", ipAttributeValue != __nullptr);

			applyZoneResults(ipAttributeValue, zoneData);
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::enhanceOCR(vector<ZoneData> &zones, IOCRParametersPtr ipOCRParameters)
{
	// Initialize a set of rects that are still actively being enhanced. Rects may be removed from
	// this set as their corresponding results meet m_nConfidenceCriteria.
	set<ILongRectanglePtr> setActiveRects;
	for each (ZoneData zoneData in zones)
	{
		setActiveRects.insert(zoneData.m_ipProcessingRect);
	}

	// Keeps track of which rects now meet m_nConfidenceCriteria and should not be processed by any
	// subsequent filters.
	vector<ILongRectanglePtr> vecRectsMeetingConfidenceCriteria;
	bool bAreAnyZonesLeftToProcess = zones.size() > 0;

	// Iterate through each filter in the configured filter package.
	for (long nFilterIndex = 0; bAreAnyZonesLeftToProcess && nFilterIndex < m_nFilterCount;
		 nFilterIndex++)
	{
		// Preparing the image page is counted as one progress item, so we can count an item as
		// completed even at the start of the first loop. Each additional filter pass is another
		// progress item.
		if (m_ipProgressStatus != __nullptr)
		{
			m_ipProgressStatus->CompleteProgressItems("Enhance OCR", 1);
		}

		string strFilter = m_pFilters[nFilterIndex];

		// Erase rects that already have results meeting m_nConfidenceCriteria for all letters and
		// remove it from setActiveRects.
		if (!vecRectsMeetingConfidenceCriteria.empty())
		{
			eraseImageZones(*m_apPageBitmap, vecRectsMeetingConfidenceCriteria);

			for each (ILongRectanglePtr ipRect in vecRectsMeetingConfidenceCriteria)
			{
				setActiveRects.erase(ipRect);
			}

			vecRectsMeetingConfidenceCriteria.clear();
		}

		// The first filtered image will have already been generated by prepareImagePage if
		// processing the document page as a whole.
		if (nFilterIndex > 0 || m_apFilteredBitmapFileName.get() == __nullptr)
		{
			generateFilteredImage(strFilter, &setActiveRects);
		}

		// Get a searcher that allows retrieving text that OCR'd from the filtered image for
		// specific image areas.
		ISpatialStringSearcherPtr ipSearcher = OCRFilteredImage(ipOCRParameters);

		// If the filter didn't produce any text, move on to the next filter.
		if (ipSearcher == __nullptr)
		{
			continue;
		}
		
		// Iterate each zone using the OCR'd text from this filter to improve the previously
		// existing text if possible.
		long nZoneCount = zones.size();
		bAreAnyZonesLeftToProcess = false;
		for (long nZone = 0; nZone < nZoneCount; nZone++)
		{
			ZoneData &zoneData = zones[nZone];

			// If this rect already has a result meeting m_nConfidenceCriteria for all letters, skip
			// processing this zone.
			if (zoneData.m_bMeetsConfidenceCriteria)
			{
				continue;
			}

			// Get the newly OCR'd text and use it to populate a new ZoneData instance.
			ZoneData OCRAttemptData;
			ISpatialStringPtr ipOCRResult =
				ipSearcher->GetDataInRegion(zoneData.m_ipRect, VARIANT_FALSE);
			populateResults(OCRAttemptData.m_vecResults, ipOCRResult, false);

			// If this is the first filter and neither the original text or newly OCR'd text have
			// any text for this area, try re-OCR'ing just this specific area. (Sometimes when asked
			// to OCR the whole page, the OCR engine misses some text; especially single characters
			// on their own.)
			if (nFilterIndex == 0 &&
				zoneData.m_vecResults.size() == 0 && OCRAttemptData.m_vecResults.size() == 0)
			{
				ipOCRResult = OCRRegion(zoneData.m_ipRect);
				populateResults(OCRAttemptData.m_vecResults, ipOCRResult, false);
			}

			// Calculate a factor that will be used to normalize the zone's span based upon the
			// effect various filters have on character width in OCR output.
			double dSpanBias =
				(g_mapALGORITHM_WIDTH_FACTOR.find(strFilter) == g_mapALGORITHM_WIDTH_FACTOR.end())
				? g_mapALGORITHM_WIDTH_FACTOR[strFilter]
				: 1.0;
			updateZoneData(OCRAttemptData, 0, dSpanBias);

			// Apply the OCRAttemptData to the zone.
			bool bNowMeetsConfidenceCriteria = applyOCRAttempt(zoneData, OCRAttemptData);

			// If the result for this zone now meets m_nConfidenceCriteria for all letters add the
			// area to a list of areas that won't be processed by any further filters.
			if (bNowMeetsConfidenceCriteria)
			{
				vecRectsMeetingConfidenceCriteria.push_back(zoneData.m_ipProcessingRect);
			}
			else
			{
				bAreAnyZonesLeftToProcess = true;
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::initializeFilters()
{
	if (m_pFilters == __nullptr)
	{
		switch (m_eFilterPackage)
		{
		case kLow:
			m_pFilters = g_arrL1Filters;
			m_nFilterCount = sizeof(g_arrL1Filters) / sizeof(string);
			break;

		case kMedium:
			m_pFilters = g_arrL2Filters;
			m_nFilterCount = sizeof(g_arrL2Filters) / sizeof(string);
			break;

		case kHigh:
			m_pFilters = g_arrL3Filters;
			m_nFilterCount = sizeof(g_arrL3Filters) / sizeof(string);
			break;

		case kHalftoneSpeckled:
			m_pFilters = g_arrHalftoneSpeckledFilters;
			m_nFilterCount = sizeof(g_arrHalftoneSpeckledFilters) / sizeof(string);
			break;

		case kAliasedDiffuse:
			m_pFilters = g_arrAliasedDiffuseFilters;
			m_nFilterCount = sizeof(g_arrAliasedDiffuseFilters) / sizeof(string);
			break;

		case kLinesSmudged:
			m_pFilters = g_arrSmudgedFilters;
			m_nFilterCount = sizeof(g_arrSmudgedFilters) / sizeof(string);
			break;

		case kCustom:
			try
			{
				try
				{
					initializeCustomFilters();
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36724");
			}
			catch (UCLIDException &ue)
			{
				throw UCLIDException("ELI36725", "Failed to initialize custom filters.", ue);
			}
			break;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CEnhanceOCR::initializeCustomFilters()
{
	string strExpandedFileName = expandPathTagsAndFunctions(m_strCustomFilterPackage);
	CommentedTextFileReader& fileReader = getFileReader(strExpandedFileName);

	while (!fileReader.reachedEndOfStream())
	{
		string strLine("");
		strLine = fileReader.getLineText();
		if (!strLine.empty())
		{
			m_vecCustomFilterPackage.push_back(strLine);

			// Parse out the individual filter(s) to be used.
			bool bMergePasses = false;
			vector<string> vecFilters;
			StringTokenizer	st('+');
			st.parse(strLine, vecFilters);

			if (vecFilters.size() == 1)
			{
				// Check if sequencing passes rather than combining them.
				StringTokenizer	st2("->");
				st2.parse(strLine, vecFilters);
			}

			long nFilterCount = vecFilters.size();
			for (long nFilterIndex = 0; nFilterIndex < nFilterCount; nFilterIndex++)
			{
				string strFilter = vecFilters[nFilterIndex];

				// Parse out the bias parameter to be passed on to the filter.
				vector<string> vecParameters;
				StringTokenizer	st('-');
				st.parse(strFilter, vecParameters);
				strFilter = vecParameters[0];

				string strFilterFilename = getDirectoryFromFullPath(strExpandedFileName) +
					"\\" + strFilter + ".dat";
				if (!isValidFile(strFilterFilename))
				{
					strFilterFilename += ".etf";
				}

				if (isValidFile(strFilterFilename))
				{
					if (m_mapCustomFilters.find(strFilter) == m_mapCustomFilters.end())
					{
						initializeCustomFilter(strFilter, strFilterFilename);
					}
				}
			}
		}
	}

	m_nFilterCount = m_vecCustomFilterPackage.size();
	m_pFilters = &m_vecCustomFilterPackage[0];
}
//-------------------------------------------------------------------------------------------------
void CEnhanceOCR::initializeCustomFilter(string strFilterName, string strFilterFilename)
{
	string strExpandedFileName = expandPathTagsAndFunctions(strFilterFilename);
	CommentedTextFileReader& fileReader = getFileReader(strExpandedFileName);
				
	string strFilterDefinition;
	while (!fileReader.reachedEndOfStream())
	{
		strFilterDefinition += fileReader.getLineText();
	}

	vector<string> vecValues;
	StringTokenizer	st(',');
	st.parse(strFilterDefinition, vecValues);

	m_mapCustomFilters[strFilterName] = vector<L_INT>();
	vector<L_INT> &vecFilter = m_mapCustomFilters[strFilterName];
	vecFilter.reserve(vecValues.size());

	for each (string strValue in vecValues)
	{
		vecFilter.push_back(asLong(strValue));
	}
}
//-------------------------------------------------------------------------------------------------
ISpatialStringSearcherPtr CEnhanceOCR::setCurrentPage(IAFDocumentPtr ipDoc, long nPage)
{
	ASSERT_ARGUMENT("ELI36522", ipDoc != __nullptr);

	// Reset any all variables tied to the current page.
	m_nCurrentPage = nPage;
	m_apPageBitmap.reset(__nullptr);
	m_apFilteredBitmapFileName.reset(__nullptr);
	m_ipPageInfoMap = __nullptr;
	m_ipCurrentPageText = __nullptr;
	m_ipCurrentPageInfo = __nullptr;
	m_ipPageRasterZone = __nullptr;			
	m_ipPageRect = __nullptr;
	m_nAvgPageCharWidth = 0;
	m_ipSpatialStringSearcher = __nullptr;
	m_vecResults.clear();
	m_vecHighConfRects.clear();

	ISpatialStringPtr ipDocText = ipDoc->Text;
	ASSERT_RESOURCE_ALLOCATION("ELI36523", ipDocText != __nullptr);

	m_strSourceDocName = ipDocText->SourceDocName;
	m_ipCurrentDoc = ipDoc;

	string strUSSFilename = m_strSourceDocName + ".uss";
			
	if (!isValidFile(strUSSFilename))
	{
		UCLIDException ue("ELI36673", "USS file is required for Enhance OCR .");
		ue.addDebugInfo("Filename", strUSSFilename);
		throw ue;
	}

	// create the rule set if necessary
	if(m_cachedDocText.m_obj == __nullptr)
	{
		m_cachedDocText.m_obj.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI36674", m_cachedDocText.m_obj != __nullptr);
	}

	m_cachedDocText.loadObjectFromFile(strUSSFilename);

	// Retrieve the page text (or create an empty value if no page text exists).
	bool bExistingSpatialInfo = asCppBool(m_cachedDocText.m_obj->HasSpatialInfo());
	if (bExistingSpatialInfo)
	{
		m_ipCurrentPageText = m_cachedDocText.m_obj->GetSpecifiedPages(m_nCurrentPage, m_nCurrentPage);
		ASSERT_RESOURCE_ALLOCATION("ELI36524", m_ipCurrentPageText != __nullptr);

		bExistingSpatialInfo = asCppBool(m_ipCurrentPageText->HasSpatialInfo());
		if (bExistingSpatialInfo)
		{
			m_nAvgPageCharWidth = m_ipCurrentPageText->GetAverageCharWidth();
			m_ipPageInfoMap = m_cachedDocText.m_obj->SpatialPageInfos;
		}
	}
		
	if (!bExistingSpatialInfo)
	{
		m_ipCurrentPageText.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI36525", m_ipCurrentPageText != __nullptr);

		if (m_ipPageInfoMap == __nullptr)
		{
			m_ipPageInfoMap.CreateInstance(CLSID_LongToObjectMap);
			ASSERT_RESOURCE_ALLOCATION("ELI36526", m_ipPageInfoMap != __nullptr);
		}
	}

	// Initialize the page info for the current page.
	bool bExistingPageInfo = asCppBool(m_ipPageInfoMap->Contains(nPage));
	if (bExistingPageInfo)
	{
		m_ipCurrentPageInfo = m_ipPageInfoMap->GetValue(nPage);
		ASSERT_RESOURCE_ALLOCATION("ELI36527", m_ipCurrentPageInfo != __nullptr);
	}
	else
	{
		int nWidth = 0;
		int nHeight = 0;
		getImagePixelHeightAndWidth(m_strSourceDocName, nHeight, nWidth, nPage);

		m_ipCurrentPageInfo.CreateInstance(CLSID_SpatialPageInfo);
		ASSERT_RESOURCE_ALLOCATION("ELI36528", m_ipCurrentPageInfo != __nullptr);

		m_ipCurrentPageInfo->Initialize(nWidth, nHeight, kRotNone, 0);

		m_ipPageInfoMap->Set(nPage, m_ipCurrentPageInfo);
	}

	// Load the current page's image.
	m_apPageBitmap.reset(new LeadToolsBitmap(m_strSourceDocName, nPage, 0));
	ASSERT_RESOURCE_ALLOCATION("ELI36529", m_apPageBitmap.get() != __nullptr);

	ISpatialStringSearcherPtr ipSearcher = getSpatialStringSearcher();
	ASSERT_RESOURCE_ALLOCATION("ELI36735", ipSearcher != __nullptr);

	return ipSearcher;
}
//--------------------------------------------------------------------------------------------------
ILongRectanglePtr CEnhanceOCR::getPageRect()
{
	if (m_ipPageRect == __nullptr)
	{
		if (m_bProcessFullDoc)
		{
			m_ipPageRect.CreateInstance(CLSID_LongRectangle);
			ASSERT_RESOURCE_ALLOCATION("ELI36677", m_ipPageRect != __nullptr);

			if (m_ipCurrentPageInfo->Orientation == kRotNone ||
				m_ipCurrentPageInfo->Orientation == kRotDown)
			{
				m_ipPageRect->SetBounds(0, 0, m_ipCurrentPageInfo->Width, m_ipCurrentPageInfo->Height);
			}
			else
			{
				m_ipPageRect->SetBounds(0, 0, m_ipCurrentPageInfo->Height, m_ipCurrentPageInfo->Width);
			}
		}
		else
		{
			// Initialize the bounds to enhance on the current page.
			ISpatialStringPtr ipAFDocText = m_ipCurrentDoc->Text;
			ASSERT_RESOURCE_ALLOCATION("ELI36675", ipAFDocText != __nullptr);

			ISpatialStringPtr ipPageText =
				ipAFDocText->GetSpecifiedPages(m_nCurrentPage, m_nCurrentPage);
			ASSERT_RESOURCE_ALLOCATION("ELI36676", ipPageText != __nullptr);

			// https://extract.atlassian.net/browse/ISSUE-12093
			// If there is not spatial info on the specified given page, return __nullptr to
			// indicate there is nothing to be processed on this page.
			if (!asCppBool(ipPageText->HasSpatialInfo()))
			{
				return __nullptr;
			}

			m_ipPageRect = ipPageText->GetOriginalImageBounds();
			ASSERT_RESOURCE_ALLOCATION("ELI36530", m_ipPageRect != __nullptr);
		}
	}

	return m_ipPageRect;
}
//--------------------------------------------------------------------------------------------------
IRasterZonePtr CEnhanceOCR::getPageRasterZone()
{
	if (m_ipPageRasterZone == __nullptr)
	{
		ILongRectanglePtr ipPageRect = getPageRect();
		if (ipPageRect == __nullptr)
		{
			return __nullptr;
		}

		m_ipPageRasterZone.CreateInstance(CLSID_RasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI36531", m_ipPageRasterZone != __nullptr);

		// Page 1 since it will be used for processing against a 1 page filtered image.
		m_ipPageRasterZone->CreateFromLongRectangle(ipPageRect, 1);
	}

	return m_ipPageRasterZone;
}
//--------------------------------------------------------------------------------------------------
vector<ILongRectanglePtr> CEnhanceOCR::removeHighConfidenceText(ISpatialStringPtr ipPageText)
{
	long nPage = ipPageText->GetFirstPageNumber();

	ISpatialPageInfoPtr ipPageInfo = ipPageText->GetPageInfo(nPage);
	ASSERT_RESOURCE_ALLOCATION("ELI36532", ipPageInfo != __nullptr);

	ILongRectanglePtr ipPageBounds(CLSID_LongRectangle);
	ASSERT_RESOURCE_ALLOCATION("ELI36533", ipPageBounds);

	ipPageBounds->SetBounds(0, 0, ipPageInfo->Width , ipPageInfo->Height);

	// Find any text in ipPageText, grouped by words, where at least one character's confidence does
	// not meet or exceed m_nConfidenceCriteria.
	IVariantVectorPtr ipBoundaries(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI36534", ipBoundaries != __nullptr);

	// SRICA will treat the boundary value as included in the lower set, not the higher set. We want
	// to treat letters on this boundary as having good enough OCR, so subtract 1.
	ipBoundaries->PushBack(_variant_t(m_nConfidenceCriteria - 1));

	IVariantVectorPtr ipTiers(__nullptr);
	IVariantVectorPtr ipIndices(__nullptr);
	IIUnknownVectorPtr ipZones =
		ipPageText->GetOriginalImageRasterZonesGroupedByConfidence(ipBoundaries,
		VARIANT_TRUE, &ipTiers, &ipIndices);

	ASSERT_RESOURCE_ALLOCATION("ELI36535", ipTiers != __nullptr);
	ASSERT_RESOURCE_ALLOCATION("ELI36536", ipIndices != __nullptr);

	// Iterate each returned section of text in order to sort into high and low confidence; the
	// high confidence areas will be excluded from processing and removed from ipPageText.
	vector<ILongRectanglePtr> vecHighConfidenceZones;
	long nCount = ipZones->Size();
	long nLastIndex = 0;
	long nLastTier = -1;
	IRasterZonePtr ipLastRasterZone = __nullptr;
	map<long, long> mapTextToRemove;
	for (long i = 0; i < nCount; i++)
	{
		// This is the character index where each zone begins.
		long nIndex = ipIndices->GetItem(i).lVal;
			
		// If the last section of text was of high confidence, add it to vecHighConfidenceZones and
		// set it to be removed from ipPageText.
		if (nLastTier == 1)
		{
			ILongRectanglePtr ipRect = ipLastRasterZone->GetRectangularBounds(ipPageBounds);
			ASSERT_RESOURCE_ALLOCATION("ELI36537", ipRect);

			vecHighConfidenceZones.push_back(ipRect);

			// A zone of all whitespace can result in nIndex == nLastIndex after the whitespace is
			// removed. Ignore such zones.
			if (nIndex > nLastIndex)
			{
				ISpatialStringPtr ipValue = ipPageText->GetSubString(nLastIndex, nIndex - 1);
				ASSERT_RESOURCE_ALLOCATION("ELI36538", ipValue != __nullptr);

				// Keep any leading whitespace in the high confidence area in ipPageText.
				string strValue = asString(ipValue->String);
				size_t nLetterCount = strValue.size();
				for (size_t j = 0; j < nLetterCount; j++)
				{
					if (isWhitespaceChar(strValue[j]))
					{
						nLastIndex++;
					}
					else
					{
						break;
					}
				}

				// If there is still remaining high confidence text, add it to mapTextToRemove.
				if (nIndex > nLastIndex)
				{
					strValue = trim(strValue, " \t\r\n", " \t\r\n");

					if (!strValue.empty())
					{
						mapTextToRemove[nLastIndex] =  nLastIndex + strValue.length() - 1;
					}
				}
			}
		}

		ipLastRasterZone = ipZones->At(i);
		_variant_t varTier = ipTiers->GetItem(i);
		nLastTier = varTier.lVal;
		nLastIndex = nIndex;
	}

	// Handle the case that the last zone was high confidence.
	if (nLastTier == 1)
	{
		ILongRectanglePtr ipRect = ipLastRasterZone->GetRectangularBounds(ipPageBounds);
		ASSERT_RESOURCE_ALLOCATION("ELI36539", ipRect);

		vecHighConfidenceZones.push_back(ipRect);
		ipPageText->Remove(nLastIndex, -1);
	}

	// Now that all the text to remove has been collected, remove the text from ipPageText.
	for (map<long, long>::reverse_iterator iter = mapTextToRemove.rbegin();
			iter != mapTextToRemove.rend(); iter++)
	{
		long nStart = iter->first;
		long nEnd = iter->second;
		ipPageText->Remove(iter->first, iter->second);
	}

	return vecHighConfidenceZones;
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::prepareImagePage(vector<ILongRectanglePtr> &vecRectsToEnhance)
{
	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;

		// Convert the image to 16-bit in order for some of the built-in Leadtools filters to work
		// correctly.
		L_INT nRes = L_ColorResBitmap(&m_apPageBitmap->m_hBitmap, &m_apPageBitmap->m_hBitmap,
			sizeof(BITMAPHANDLE), 16, CRF_FIXEDPALETTE, NULL, NULL, 0, NULL, NULL);
		throwExceptionIfNotSuccess(nRes, "ELI36540", "Image processing error");
	}

	// Re-load the original image, but with only the areas that need enhancement. The rest of the
	// image will be blank.
	loadImagePageWithSpecifiedRectsOnly(vecRectsToEnhance);
}
//--------------------------------------------------------------------------------------------------
vector<ILongRectanglePtr> CEnhanceOCR::prepareImagePage(ISpatialStringPtr ipLowConfidenceText, 
														vector<ILongRectanglePtr> vecZonesToIgnore)
{
	LeadToolsLicenseRestrictor leadToolsLicenseGuard;

	removeBlackBorders(&m_apPageBitmap->m_hBitmap);

	eraseImageZones(*m_apPageBitmap, vecZonesToIgnore);

	// Convert the image to 16 bit in order for the filters to work correctly.
	L_INT nRes = L_ColorResBitmap(&m_apPageBitmap->m_hBitmap, &m_apPageBitmap->m_hBitmap,
		sizeof(BITMAPHANDLE), 16, CRF_FIXEDPALETTE, NULL, NULL, 0, NULL, NULL);
	throwExceptionIfNotSuccess(nRes, "ELI36541", "Image processing error");

	set<ILongRectanglePtr> setAreaToPrepare;
	if (!m_bProcessFullDoc)
	{
		setAreaToPrepare.insert(getPageRect());
	}

	// Apply the first filter to the entire image. The default of medium-45 will remove most
	// extraneous pixel content to make it easier to identify content zones.
	generateFilteredImage(m_pFilters[0], m_bProcessFullDoc ? __nullptr : &setAreaToPrepare);
	
	// Identify the image areas to enhance. The provided low confidence text should be the basis of
	// such areas; other will be identified via pixel content of the image.
	vector<ILongRectanglePtr> vecRectsToEnhance = getRectsToEnhance(ipLowConfidenceText);

	// After the areas to enhance have been identified, re-load the original image, but with only
	// the areas that need enhancement. The rest of the image will be blank.
	loadImagePageWithSpecifiedRectsOnly(vecRectsToEnhance);

	return vecRectsToEnhance;
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::removeBlackBorders(pBITMAPHANDLE phBitmap)
{
	LeadToolsLicenseRestrictor leadToolsLicenseGuard;

	BORDERREMOVE borderRemove = {0};
	borderRemove.uStructSize = sizeof(BORDERREMOVE);
	borderRemove.iBorderPercent    = 25;
	borderRemove.iVariance         = 7;
	borderRemove.iWhiteNoiseLength = 10;
	borderRemove.uBorderToRemove   = BORDER_TOP | BORDER_BOTTOM | BORDER_LEFT | BORDER_RIGHT;
	borderRemove.uFlags            = BORDER_USE_VARIANCE;
	borderRemove.uBitmapStructSize = sizeof(BITMAPHANDLE);

	L_INT nRes = L_BorderRemoveBitmap(phBitmap, &borderRemove, NULL, NULL, 0);
	throwExceptionIfNotSuccess(nRes, "ELI36542", "Image processing error");
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::eraseImageZones(LeadToolsBitmap &ltBitmap,
								 vector<ILongRectanglePtr> vecZonesToErase)
{
	LeadtoolsDCManager ltDC;
	ltDC.createFromBitmapHandle(ltBitmap.m_hBitmap);

	// Get a white brush and pen.
	HBRUSH hBrush = m_brushes.getColoredBrush(gnCOLOR_WHITE);
	if (SelectObject(ltDC.m_hDC, hBrush) == NULL)
	{
		UCLIDException ue("ELI36543", "Failed to set fill color.");
		ue.addWin32ErrorInfo();
		throw ue;
	}
	HPEN hPen = m_pens.getColoredPen(gnCOLOR_WHITE);
	if (SelectObject(ltDC.m_hDC, hPen) == NULL)
	{
		UCLIDException ue("ELI36544", "Failed to set border color.");
		ue.addWin32ErrorInfo();
		throw ue;
	}

	// Iterate each zone to erase, and fill each area with white.
	for each (ILongRectanglePtr ipRect in vecZonesToErase)
	{
		long nLeft;
		long nTop;
		long nRight;
		long nBottom;
		ipRect->GetBounds(&nLeft, &nTop, &nRight, &nBottom);

		POINT aPoints[4];
		aPoints[0].x = nLeft;
		aPoints[0].y = nTop;
		aPoints[1].x = nRight;
		aPoints[1].y = nTop;
		aPoints[2].x = nRight;
		aPoints[2].y = nBottom;
		aPoints[3].x = nLeft;
		aPoints[3].y = nBottom;

		if (Polygon(ltDC.m_hDC, (POINT*)&aPoints, 4) == FALSE)
		{
			UCLIDException ue("ELI36545", "Image processing error");
			ue.addWin32ErrorInfo();
			throw ue;
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::generateFilteredImage(string strFilter, set<ILongRectanglePtr> *psetRectsToFilter)
{
	// Work off a copy of the currently loaded m_apPageBitmap
	BITMAPHANDLE hBitmapCopy = { 0 };
	LeadToolsBitmapFreeer bitmapCopyFreer(hBitmapCopy, true);

	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;
		L_INT nRes = L_CopyBitmap(&hBitmapCopy, &m_apPageBitmap->m_hBitmap, sizeof(BITMAPHANDLE));
		throwExceptionIfNotSuccess(nRes, "ELI36546", "Image processing error");
	}
	// Apply the filter to the selected areas (if specified) or otherwise to the entire page.
	if (psetRectsToFilter == __nullptr)
	{
		applyFilters(&hBitmapCopy, strFilter, __nullptr);
	}
	else
	{
		for each (ILongRectanglePtr ipRect in *psetRectsToFilter)
		{
			applyFilters(&hBitmapCopy, strFilter, ipRect);
		}
	}

	// Get a temporary file and save the filtered image to it.
	if (m_bOutputFilteredImages)
	{
		string strFilterFileName =
			getPathAndFileNameWithoutExtension(m_strSourceDocName) + "." + strFilter +
			"." + asString(m_nCurrentPage) +
			getExtensionFromFullPath(m_strSourceDocName);
		// Ensure there is not a previously existing file.
		if (isValidFile(strFilterFileName))
		{
			deleteFile(strFilterFileName, true, false);
		}
		m_apFilteredBitmapFileName.reset(new TemporaryFileName(false, strFilterFileName, false));
	}
	else if (m_apFilteredBitmapFileName.get() == __nullptr)
	{
		m_apFilteredBitmapFileName.reset(new TemporaryFileName(true));
	}

	SAVEFILEOPTION sfOptions = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;

		L_GetDefaultSaveFileOption(&sfOptions, sizeof(sfOptions));
	}
	sfOptions.PageNumber = 1;

	// The OCR engine is able to process bitonal images faster and, in most cases, more accurately.
	// It does seem on some testing that it is more accurate to output in grayscale in some cases
	// such as for aliased of diffuse text, so this may be worth revisiting if trying to fine-tune
	// performance.
	// Specify the file format to ensure it is valid for bitonal images
	// https://extract.atlassian.net/browse/ISSUE-13411
	const long nFORMAT = FILE_CCITT_GROUP4;
	L_INT nCompression = getCompressionFactor(m_apPageBitmap->m_FileInfo.Format);
	saveImagePage(hBitmapCopy, m_apFilteredBitmapFileName->getName().c_str(),
		nFORMAT, nCompression, 1, sfOptions);
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::applyFilters(pBITMAPHANDLE phBitmap, string strFilters, ILongRectanglePtr ipRect)
{
	// If a rect is not specified, apply the filter to the entire page.
	CRect rect;
	if (ipRect != __nullptr)
	{
		ipRect->GetBounds(&rect.left, &rect.top, &rect.right, &rect.bottom);
	}
	else
	{
		rect.left = 0;
		rect.top = 0;
		rect.right = phBitmap->Width;
		rect.bottom = phBitmap->Height;
	}

	// Make a copy of the specified bitmap to work with.
	BITMAPHANDLE hBitmapFilterCopy = {0};
	LeadToolsBitmapFreeer bitmapFilterCopyFreer(hBitmapFilterCopy, true);

	L_INT nRet = 0;

	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;

		nRet = L_CopyBitmapRect(&hBitmapFilterCopy, phBitmap,
			sizeof(BITMAPHANDLE), rect.left, rect.top, rect.Width(), rect.Height());
		throwExceptionIfNotSuccess(nRet, "ELI36547", "Image processing error");
	}

	// If filtering using a combination of multiple filters, we will need to store the result of the
	// first pass in a separate bitmap.
	BITMAPHANDLE hBitmapCopy = {0};
	unique_ptr<LeadToolsBitmapFreeer> apBitmapCopy;

	// Parse out the individual filter(s) to be used.
	bool bCombinePasses = false;
	vector<string> vecFilters;
	StringTokenizer	st('+');
	st.parse(strFilters, vecFilters);

	if (vecFilters.size() == 1)
	{
		// Check if sequencing passes rather than combining them.
		StringTokenizer	st2("->");
		st2.parse(strFilters, vecFilters);
	}
	else
	{
		bCombinePasses = true;
	}

	// Iterate each filter to be used.
	pBITMAPHANDLE phCurrentBitmap;
	long nFilterCount = vecFilters.size();
	for (long nFilterIndex = 0; nFilterIndex < nFilterCount; nFilterIndex++)
	{
		string strFilter = vecFilters[nFilterIndex];

		// Parse out any parameters to be passed on to the filter.
		vector<string> vecParameters;
		StringTokenizer	st('-');
		st.parse(strFilter, vecParameters);
		strFilter = vecParameters[0];

		// If combining the results of two filters, we will apply the first filter to hBitmapCopy.
		if (bCombinePasses && nFilterIndex == 0)
		{
			apBitmapCopy.reset(new LeadToolsBitmapFreeer(hBitmapCopy, true));
			ASSERT_RESOURCE_ALLOCATION("ELI36548", apBitmapCopy.get() != __nullptr);

			phCurrentBitmap = &hBitmapCopy;

			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			L_INT nRet = L_CopyBitmap(phCurrentBitmap, phBitmap, sizeof(BITMAPHANDLE));
			throwExceptionIfNotSuccess(nRet, "ELI36549", "Image processing error");
		}
		else
		{
			phCurrentBitmap = &hBitmapFilterCopy;
		}

		// Check for a custom defined filter first... allow a custom filter of the same name as a
		// built-in one to override the built-in version.
		if (m_mapCustomFilters.find(strFilter) != m_mapCustomFilters.end())
		{
			vector<L_INT> &vecFilter = m_mapCustomFilters[strFilter];
			long nDim = lround(sqrt((double)vecFilter.size()));
			applyFilter(phCurrentBitmap, nDim, &vecFilter[0], asDouble("0." + vecParameters[1]));
		}
		else if (strFilter == "medium")
		{
			applyFilter(phCurrentBitmap, 7, gltFilter, asDouble("0." + vecParameters[1]));
		}
		else if (strFilter == "large")
		{
			applyFilter(phCurrentBitmap, 9, gltLargeFilter, asDouble("0." + vecParameters[1]));
		}
		else if (strFilter == "small")
		{
			applyFilter(phCurrentBitmap, 5, gltSmallFilter, asDouble("0." + vecParameters[1]));
		}
		else if (strFilter == "despeckle")
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			nRet = L_DespeckleBitmap(phCurrentBitmap, 0);
		}
		else if (strFilter == "median")
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			nRet = L_MedianFilterBitmap(phCurrentBitmap, asLong(vecParameters[1]), 0);
		}
		else if (strFilter == "average")
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			nRet = L_AverageFilterBitmap(phCurrentBitmap, asLong(vecParameters[1]), 0);
		}
		else if (strFilter == "gaussian")
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			nRet = L_GaussianFilterBitmap(phCurrentBitmap, asLong(vecParameters[1]), 0);
		}
		else if (strFilter == "sharpen")
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			nRet = L_SharpenBitmap(phCurrentBitmap, asLong(vecParameters[1]) * 20 - 1000, 0);
		}
		else if (strFilter == "smooth")
		{
			SMOOTH smooth = {0};
			smooth.uStructSize = sizeof(SMOOTH);
			smooth.iLength = asLong(vecParameters[1]);
			smooth.pBitmapRegion = phCurrentBitmap;
			smooth.uBitmapStructSize = sizeof(BITMAPHANDLE);
			smooth.uFlags = SMOOTH_SINGLE_REGION | SMOOTH_LEAD_REGION | asLong(vecParameters[2]);

			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			nRet = L_ColorResBitmap(phCurrentBitmap, phCurrentBitmap,
				sizeof(BITMAPHANDLE), 1, CRF_FIXEDPALETTE, NULL, NULL, 0, NULL, NULL);
			throwExceptionIfNotSuccess(nRet, "ELI36550", "Image processing error");

			nRet = L_SmoothBitmap(phCurrentBitmap, &smooth, NULL, NULL, 0);
			throwExceptionIfNotSuccess(nRet, "ELI36551", "Image processing error");

			nRet = L_ColorResBitmap(phCurrentBitmap, phCurrentBitmap,
				sizeof(BITMAPHANDLE), 16, CRF_FIXEDPALETTE, NULL, NULL, 0, NULL, NULL);
			throwExceptionIfNotSuccess(nRet, "ELI36552", "Image processing error");
		}
		else if (strFilter == "min")
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			nRet = L_MinFilterBitmap(phCurrentBitmap, asLong(vecParameters[1]), 0);
		}
		else if (strFilter == "max")
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			nRet = L_MaxFilterBitmap(phCurrentBitmap, asLong(vecParameters[1]), 0);
		}
		else if (strFilter == "highPass")
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			nRet = L_HighPassFilterBitmap(phBitmap, asLong(vecParameters[1]),
					asLong(vecParameters[2]), 0);
		}
		else if (strFilter == "original")
		{
			// Do not filter; just allows OCR another pass in a different context.
		}
		else
		{
			UCLIDException ue("ELI36553", "Undefined image filter.");
			ue.addDebugInfo("Filter", strFilter, true);
			throw ue;
		}

		throwExceptionIfNotSuccess(nRet, "ELI36554", strFilter);
	}

	LeadToolsLicenseRestrictor leadToolsLicenseGuard;

	// If a filter combination is being used, combine the results of the two passes.
	if (bCombinePasses)
	{
		nRet = L_CombineBitmap(&hBitmapFilterCopy, hBitmapFilterCopy.Left,
			hBitmapFilterCopy.Top, hBitmapFilterCopy.Width,
			hBitmapFilterCopy.Height, phCurrentBitmap,
			hBitmapFilterCopy.Left, hBitmapFilterCopy.Top,
			CB_OP_AVG, 0);
		throwExceptionIfNotSuccess(nRet, "ELI36555", "Image processing error");
	}

	// Apply the result back to the input bitmap.
	nRet = L_CombineBitmap(phBitmap, rect.left, rect.top,
		rect.Width(), rect.Height(), &hBitmapFilterCopy, hBitmapFilterCopy.Left,
		hBitmapFilterCopy.Top, CB_DST_0 | CB_OP_ADD, 0);
	throwExceptionIfNotSuccess(nRet, "ELI36556", "Image processing error");
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::applyFilter(pBITMAPHANDLE pBitmap, L_INT nDim, const L_INT pFilter[],
							 double dThreshold)
{
	long nCells = nDim * nDim;
	long nStructSize = SPATIALFLTSIZE(nDim);
	unique_ptr<char> apSpatialFilter(new char[nStructSize]);
	pSPATIALFLT pSpatialFilter = (pSPATIALFLT)apSpatialFilter.get();
	ZeroMemory(pSpatialFilter, nStructSize);
	pSpatialFilter->uStructSize = sizeof(SPATIALFLT);
	pSpatialFilter->fltDim = nDim;

	// After totalling all the weighted pixels from the filter, divide by sum of all weights to get
	// the ratio of black to the total possible black (if all pixels checked by the filter were
	// completely black).
	for (long i = 0; i < nCells; i++)
	{
		pSpatialFilter->fltDivisor += ((L_INT*)pFilter)[i];
	}

	// The filter bias indicates what threshold the above ratio must meet in order for the resulting
	// pixel to be black. A bias value of 0 is equivalent to a threshold of 50% black, so to turn
	// the threshold ratio into a bias, subtract 50%.
	pSpatialFilter->fltBias = (long)(gnFILTER_SHADE_COUNT * (dThreshold - 0.5));
	memcpy(pSpatialFilter->fltMatrix, pFilter, nCells * sizeof(L_INT));
	
	LeadToolsLicenseRestrictor leadToolsLicenseGuard;
	L_INT nRes = L_SpatialFilterBitmap(pBitmap, pSpatialFilter, 0);
	throwExceptionIfNotSuccess(nRes, "ELI36557", "Image processing error.");
}
//--------------------------------------------------------------------------------------------------
vector<ILongRectanglePtr> CEnhanceOCR::getRectsToEnhance(ISpatialStringPtr ipLowQualityText)
{
	// Create a split region into content areas object to locate zones of pixel content.
	IAttributeModifyingRulePtr ipSRICA = getSRICA();
	ASSERT_RESOURCE_ALLOCATION("ELI36558", ipSRICA != __nullptr);

	IAttributePtr ipTempAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI36559", ipTempAttribute != __nullptr);

	IIUnknownVectorPtr ipRasterZones(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI36560", ipRasterZones != __nullptr);

	IRasterZonePtr ipPageRasterZone = getPageRasterZone();

	// If there is no spatial info for this page, return an empty vector.
	if (ipPageRasterZone == __nullptr)
	{
		return vector<ILongRectanglePtr>();
	}

	ipRasterZones->PushBack(ipPageRasterZone);

	ILongToObjectMapPtr ipPageInfoMap(CLSID_LongToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI36585", ipPageInfoMap != __nullptr);

	ipPageInfoMap->Set(1, m_ipCurrentPageInfo);

	string strFilteredImageName = m_apFilteredBitmapFileName->getName();
	ISpatialStringPtr ipPageZone(CLSID_SpatialString);
	ipPageZone->CreateHybridString(
		ipRasterZones, "XXXXX", strFilteredImageName.c_str(), ipPageInfoMap);
	ipTempAttribute->Value = ipPageZone;

	// Provide the low quality text as the document text so that the SRICA object doesn't bother
	// processing areas of high-confidence text.
	IAFDocumentPtr ipAFTempDoc(CLSID_AFDocument);
	ipAFTempDoc->Text = asCppBool(ipLowQualityText->HasSpatialInfo()) ? ipLowQualityText : ipPageZone;
	ipAFTempDoc->Text->SourceDocName = strFilteredImageName.c_str();

	ipSRICA->ModifyValue(ipTempAttribute, ipAFTempDoc, __nullptr);

	IIUnknownVectorPtr ipSubAttributes = ipTempAttribute->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI36562", ipSubAttributes != __nullptr);

	// Iterate the sub-attributes returned. The spatial area of each represents an image area whose
	// OCR text should be enhanced.
	vector<ILongRectanglePtr> vecRectsToEnhance;
	long nCount = ipSubAttributes->Size();
	for (long i = 0; i < nCount; i++)
	{
		IAttributePtr ipChildAttribute = ipSubAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI36563", ipChildAttribute != __nullptr);

		ISpatialStringPtr ipAttributeValue = ipChildAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI36564", ipAttributeValue != __nullptr);

		ILongRectanglePtr ipImageRect = ipAttributeValue->GetOriginalImageBounds();
		ASSERT_RESOURCE_ALLOCATION("ELI36565", ipImageRect != __nullptr);

		vecRectsToEnhance.push_back(ipImageRect);
	}

	return vecRectsToEnhance;
}
//--------------------------------------------------------------------------------------------------
ILongRectanglePtr CEnhanceOCR::inflateRect(ILongRectanglePtr ipRect, long nWidth, long nHeight)
{
	CRect rect;
	ipRect->GetBounds(&rect.left, &rect.top, &rect.right, &rect.bottom);
	
	if (m_ipCurrentPageInfo->Orientation == kRotNone ||
		m_ipCurrentPageInfo->Orientation == kRotDown)
	{
		rect.InflateRect(nWidth, nHeight);
	}
	else
	{
		rect.InflateRect(nHeight, nWidth);
	}

	if (nWidth > 0 || nHeight > 0)
	{
		rect.IntersectRect(rect, CRect(0, 0, m_ipCurrentPageInfo->Width, m_ipCurrentPageInfo->Height));
	}

	if (nWidth < 0 || nHeight < 0)
	{
		rect.NormalizeRect();
	}

	ILongRectanglePtr ipExpandedRect(CLSID_LongRectangle);
	ASSERT_RESOURCE_ALLOCATION("ELI36737", ipExpandedRect != __nullptr);

	ipExpandedRect->SetBounds(rect.left, rect.top, rect.right, rect.bottom);

	return ipExpandedRect;
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::loadImagePageWithSpecifiedRectsOnly(vector<ILongRectanglePtr> &vecRectsToEnhance)
{
	LeadToolsLicenseRestrictor leadToolsLicenseGuard;

	// Clear the image so that just the areas to enhance can be added back in.
	L_INT nRes = L_FillBitmap(&m_apPageBitmap->m_hBitmap, gnCOLOR_WHITE);
	throwExceptionIfNotSuccess(nRes, "ELI36566", "Image processing error");

	// Temporarily reload the original image as the source to copy the areas to enhance from.
	LeadToolsBitmap tempBitmap(m_strSourceDocName, m_nCurrentPage, 0, 16);

	// Loop through each area to enhance and copy it into m_apPageBitmap from the original image. 
	for each (ILongRectanglePtr ipRect in vecRectsToEnhance)
	{
		ASSERT_RESOURCE_ALLOCATION("ELI36567", ipRect != __nullptr);

		CRect rect;
		ipRect->GetBounds(&rect.left, &rect.top, &rect.right, &rect.bottom);

		nRes = L_CombineBitmap(&m_apPageBitmap->m_hBitmap, rect.left, rect.top,
			rect.Width(), rect.Height(), &tempBitmap.m_hBitmap, rect.left, rect.top,
			CB_DST_0 | CB_OP_ADD, 0);
		throwExceptionIfNotSuccess(nRes, "ELI36568", "Image processing error");
	}
}
//--------------------------------------------------------------------------------------------------
vector<CEnhanceOCR::ZoneData> CEnhanceOCR::createZonesFromRects(vector<ILongRectanglePtr> vecRects,
	ISpatialStringSearcherPtr ipSearcher, CSize sizeExpandProcessingRect/* = CSize()*/)
{
	// Iterate each input rect to create a corresponding zone.
	vector<ZoneData> vecZonesToEnhance;
	for each (ILongRectanglePtr ipRect in vecRects)
	{
		ASSERT_RESOURCE_ALLOCATION("ELI36569", ipRect != __nullptr);

		ISpatialStringPtr ipOriginalText = (ipSearcher == __nullptr)
			? ISpatialStringPtr(CLSID_SpatialString)
			: ipSearcher->GetDataInRegion(ipRect, VARIANT_FALSE);
		ASSERT_RESOURCE_ALLOCATION("ELI36570", ipOriginalText != __nullptr);

		// Any bounding whitespace should be ignored.
		if (asCppBool(ipOriginalText->HasSpatialInfo()))
		{
			ipOriginalText->Trim(" \t\r\n", " \t\r\n");

			// https://extract.atlassian.net/browse/ISSUE-12088
			// It seems EnhanceOCR frequently replaces properly found curly braces with incorrect
			// chars. For now, simply ignore zones where a matching pair of curly braces are found.
			long nOpeningBracePos = ipOriginalText->FindFirstInstanceOfString("{", 0);
			if (nOpeningBracePos >=0 &&
				ipOriginalText->FindFirstInstanceOfString("{", nOpeningBracePos) >= 0)
			{
				continue;
			}
		}

		// Create the ZoneData instance.
		ZoneData zone;
		zone.m_ipOriginalText = ipOriginalText;
		zone.m_ipRect = ipRect;

		if (sizeExpandProcessingRect.cx == 0 && sizeExpandProcessingRect.cy == 0)
		{
			zone.m_ipProcessingRect = ipRect;
		}
		else
		{
			// If sizeExpandProcessingRect has been specified, process an area the specified amount
			// larger than m_ipRect.
			zone.m_ipProcessingRect = inflateRect(ipRect,
				sizeExpandProcessingRect.cx / 2, sizeExpandProcessingRect.cx / 2);
		}

		populateResults(zone.m_vecResults, ipOriginalText, true);
		updateZoneData(zone, true, 1.0); // true = this is the original zone.

		vecZonesToEnhance.push_back(zone);
	}

	return vecZonesToEnhance;
}
//--------------------------------------------------------------------------------------------------
ISpatialStringSearcherPtr CEnhanceOCR::OCRFilteredImage(IOCRParametersPtr ipOCRParameters)
{
	if (m_apFilteredBitmapFileName.get() == __nullptr)
	{
		throw UCLIDException("ELI36571", "Image processing error");
	}

	IOCREnginePtr ipOCREngine = getOCREngine();

	ISpatialStringPtr ipOCRText = ipOCREngine->RecognizeTextInImage(
		m_apFilteredBitmapFileName->getName().c_str(),
		1, 1, kNoFilter, "", kAccurate, VARIANT_TRUE, NULL, ipOCRParameters);

	// As long as the OCR of the filtered image produced results, return a searcher that allows
	// results from a given rect to be retrieved.
	if (asCppBool(ipOCRText->HasSpatialInfo()))
	{
		ISpatialPageInfoPtr ipPageInfo = ipOCRText->GetPageInfo(1);
		ASSERT_RESOURCE_ALLOCATION("ELI36572", ipPageInfo != __nullptr);

		// If orientation is not the same as the original image, the OCR engine probably thinks the
		// text is upside down; the text is probably terribly wrong; ignore it.
		if (ipPageInfo->Orientation == m_ipCurrentPageInfo->Orientation)
		{
			// Since only the current page is saved to m_apFilteredBitmapFileName, the page number
			// needs to be updated to reflect m_apFilteredBitmapFileName.
			if (m_nCurrentPage != 1)
			{
				ipOCRText->UpdatePageNumber(m_nCurrentPage);
				// On a one page SpatialString, the SpatialPageInfo will automatically be moved to
				// the new page number.
			}
			ipOCRText->TranslateToNewPageInfo(m_ipPageInfoMap);

			ISpatialStringSearcherPtr ipSearcher(CLSID_SpatialStringSearcher);
			ASSERT_RESOURCE_ALLOCATION("ELI36573", ipSearcher != __nullptr);

			ipSearcher->InitSpatialStringSearcher(ipOCRText, VARIANT_TRUE);
			// Include any chars whose midpoints fall within the specified region.
			ipSearcher->SetUseMidpointsOnly(VARIANT_TRUE);

			// https://extract.atlassian.net/browse/ISSUE-12088
			// Though high confidence text is excluded at the beginning of prepareImagePage, the
			// zones it produces to process may still end up including some area where high
			// confidence text was. To avoid modifying any high confidence text, configure the
			// searcher to ignore these regions.
			for each (ILongRectanglePtr ipRect in m_vecHighConfRects)
			{
				ipSearcher->ExcludeDataInRegion(ipRect);
			}

			return ipSearcher;
		}
	}

	return __nullptr;
}
//--------------------------------------------------------------------------------------------------
ISpatialStringPtr CEnhanceOCR::OCRRegion(ILongRectanglePtr ipRect)
{
	try
	{
		if (m_apFilteredBitmapFileName.get() == __nullptr)
		{
			throw UCLIDException("ELI36574", "Image processing error");
		}

		IOCREnginePtr ipOCREngine = getOCREngine();

		ISpatialStringPtr ipZoneText = ipOCREngine->RecognizeTextInImageZone(
			m_apFilteredBitmapFileName->getName().c_str(), 1, 1, ipRect, 0, kNoFilter, "",
			VARIANT_FALSE, VARIANT_FALSE, VARIANT_TRUE, NULL, NULL);

		if (asCppBool(ipZoneText->HasSpatialInfo()))
		{
			ISpatialPageInfoPtr ipPageInfo = ipZoneText->GetPageInfo(1);
			ASSERT_RESOURCE_ALLOCATION("ELI36575", ipPageInfo != __nullptr);

			// If orientation is not the same as the original image, the OCR engine probably thinks
			// the text is upside down; the text is probably terribly wrong; ignore it.
			if (ipPageInfo->Orientation != m_ipCurrentPageInfo->Orientation)
			{
				return __nullptr;
			}
			else
			{
				// Since only the current page is saved to m_apFilteredBitmapFileName, the page
				// number needs to be updated to reflect m_apFilteredBitmapFileName.
				if (m_nCurrentPage != 1)
				{
					ipZoneText->UpdatePageNumber(m_nCurrentPage);
					// On a one page SpatialString, the SpatialPageInfo will automatically be moved
					// to the new page number.
				}
				ipZoneText->TranslateToNewPageInfo(m_ipPageInfoMap);

				return ipZoneText;
			}
		}
		else
		{
			return __nullptr;
		}

	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36576");
	
	return __nullptr;
}
//--------------------------------------------------------------------------------------------------
bool CEnhanceOCR::applyOCRAttempt(ZoneData& zoneData, ZoneData& OCRAttemptData)
{
	// Weight the zone's confidence with the horizontal span of the OCR text to avoid favoring a
	// result that has high confidence, but that covers only a fraction of text that another result
	// covers.
	double dSpanRatio = (zoneData.m_nSpan > 0)
		? (double)OCRAttemptData.m_nSpan / (double)zoneData.m_nSpan
		: 1.0;
	long ulAdjustedConfidence = (unsigned long)lround(OCRAttemptData.m_nConfidence * dSpanRatio);

	// If this result is substantially better than the existing result simply replace the entire
	// zone's text with the new OCRAttemptData.
	if (ulAdjustedConfidence - gnSUBSTANTIALLY_BETTER_CONF_MARGIN > zoneData.m_nConfidence)
	{
		zoneData.m_vecResults.clear();
		zoneData.m_vecResults.insert(zoneData.m_vecResults.end(),
			OCRAttemptData.m_vecResults.begin(), OCRAttemptData.m_vecResults.end());
		zoneData.m_strResult = OCRAttemptData.m_strResult;
		zoneData.m_nConfidence = OCRAttemptData.m_nConfidence;
		zoneData.m_nSpan = OCRAttemptData.m_nSpan;

		// The new result may be a preferred result
		updateZonePreferredResult(zoneData);
	}
	// If OCRAttemptData is not within gnMINIMUM_CONF_MARGIN of the exising result, ignore it
	// completely.
	else if (ulAdjustedConfidence + gnMINIMUM_CONF_MARGIN < zoneData.m_nConfidence &&
		OCRAttemptData.m_strResult.find(zoneData.m_strResult) == string::npos)
	{
		return false;
	}
	// The new result's confidence is on par with the existing result. Merge the results
	// character-by-character per OCR confidence.
	else
	{
		// Iterate all results in OCRAttemptData
		for (size_t nAttempt = 0; nAttempt < OCRAttemptData.m_vecResults.size(); nAttempt++)
		{
			OCRResult *pAttempt = OCRAttemptData.m_vecResults[nAttempt];
			bool bFoundInsertionPos = false;
			long nInsertionPos = 0;

			// Attempt to find an existing result that corresponds spatially with the OCRAttempt
			// result.
			for (size_t nResult = 0; nResult < zoneData.m_vecResults.size(); nResult++)
			{
				OCRResult *pExistingResult = zoneData.m_vecResults[nResult];

				long nAlignment = pExistingResult->CompareVerticalArea(*pAttempt);
				if (nAlignment == 0)
				{
					// This result corresponds spatially; merge the two. Allow non-word characters
					// to be added to the results only if the confidence of this attempt is the best
					// so far.
					pExistingResult = mergeResults(*pExistingResult, *pAttempt,
						(OCRAttemptData.m_nConfidence == zoneData.m_nConfidence));
					m_vecResults.push_back(unique_ptr<OCRResult>(pExistingResult));
					zoneData.m_vecResults.erase(zoneData.m_vecResults.begin() + nResult);
					zoneData.m_vecResults.insert(zoneData.m_vecResults.begin() + nResult, pExistingResult);

					// This result was merged, so no new result will be added to this zone.
					nInsertionPos = -1;
					break;
				}
				else if (!bFoundInsertionPos && nAlignment < 0)
				{
					// May insert pAttempt to the zone at this position, but see if nAlignment < 0
					// for any subsequent results.
					nInsertionPos = nResult;
					bFoundInsertionPos = true;
				}
				else if (nAlignment > 0)
				{
					// This result belongs after all existing results.
					nInsertionPos = nResult + 1;
				}
			}
			
			// No existing result corresponded spatially with this one; insert the new result
			// separately.
			if (nInsertionPos >= 0)
			{
				if (nInsertionPos == zoneData.m_vecResults.size())
				{
					zoneData.m_vecResults.push_back(pAttempt);
				}
				else
				{
					zoneData.m_vecResults.insert(zoneData.m_vecResults.begin() + nInsertionPos, pAttempt);
				}
			}
		}

		// Update the zone stats, but don't allow the final confidence to be less than either the
		// existing or new zone's confidence. This helps prevent unnecessary "thrashing" of results
		// that tends to degrade the result over multiple iterations.
		long nTempConfidence = max(OCRAttemptData.m_nConfidence, zoneData.m_nConfidence);
		updateZoneData(zoneData, 0, 1.0);
		if (nTempConfidence > zoneData.m_nConfidence)
		{
			zoneData.m_nConfidence = nTempConfidence;
		}
	}

	// Determine whether this result now meets the confidence criteria for all letters.
	if (!zoneData.m_bMeetsConfidenceCriteria)
	{
		if (zoneData.m_vecResults.size() > 0)
		{
			bool bMeetsConfidence = true;
			for each (OCRResult *pResult in zoneData.m_vecResults)
			{
				if (!pResult->AllCharsMeetConfidenceCriteria(m_nConfidenceCriteria))
				{
					bMeetsConfidence = false;
				}
			}

			if (bMeetsConfidence)
			{
				zoneData.m_bMeetsConfidenceCriteria = true;
				return true;
			}
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::populateResults(vector<OCRResult*>& vecResults, ISpatialStringPtr ipText,
								  bool bIsOriginal)
{
	if (ipText != __nullptr && asCppBool(ipText->HasSpatialInfo()))
	{
		long nPage = ipText->GetFirstPageNumber();

		ISpatialPageInfoPtr ipPageInfo = ipText->GetPageInfo(nPage);
		ASSERT_RESOURCE_ALLOCATION("ELI36577", ipPageInfo != __nullptr);

		// If orientation is not the same as the original image, the OCR engine probably thinks the
		// text is upside down; the text is probably terribly wrong; ignore it.
		if (ipPageInfo->Orientation == m_ipCurrentPageInfo->Orientation)
		{
			ICopyableObjectPtr ipCopyThis = ipText;
			ASSERT_RESOURCE_ALLOCATION("ELI36578", ipCopyThis != __nullptr);

			ISpatialStringPtr ipTextCopy = ipCopyThis->Clone();

			// Generate an OCRResult instance from each separate line in ipTextCopy until there is
			// no spatial info left. Multiple lines in ipTextCopy will result in multiple OCRResult
			// instances.
			while (asCppBool(ipTextCopy->HasSpatialInfo()))
			{
				m_vecResults.push_back(unique_ptr<OCRResult>(
					new OCRResult(ipTextCopy, bIsOriginal, bIsOriginal ? "" : m_strCharsToIgnore)));
				if (!m_vecResults.back()->IsEmpty())
				{
					vecResults.push_back(m_vecResults.back().get());
				}
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::updateZoneData(ZoneData& zoneData, bool bIsOriginal, double dSpanBias)
{
	long nLetterCount = 0;
	long nSpatialLetterCount = 0;
	double dConfidenceValue = 0.0;
	double dWordCharRatioValue = 0.0;
	long nRawConfidence = 0;
	zoneData.m_nConfidence = 0;
	zoneData.m_nSpan = 0;
	zoneData.m_strResult = "";

	// Loop through each OCRResult in the zone to generate aggregate stats for the zone.
	for each (OCRResult *pResult in zoneData.m_vecResults)
	{
		if (!pResult->IsEmpty())
		{
			if (bIsOriginal)
			{
				pResult->m_nOverallConfidence += gnORIGINAL_RESULT_CONF_BIAS;
			}
			zoneData.m_nSpan += pResult->Span(dSpanBias, m_nAvgPageCharWidth);
			long nLetters = pResult->m_vecLetters.size();
			nLetterCount += nLetters;
			long nSpatialLetters = pResult->m_nSpatialCharCount;
			nSpatialLetterCount += nSpatialLetters;
			dWordCharRatioValue += (pResult->m_dWordCharRatio * (double)nLetters);
			dConfidenceValue += (pResult->m_nOverallConfidence * (double)nSpatialLetters);

			// If there are multiple results, separate each result with a newline for the overall
			// text result.
			if (!zoneData.m_strResult.empty())
			{
				zoneData.m_strResult + "\r\n";
			}
			zoneData.m_strResult += pResult->ToString();
		}
	}

	if (nSpatialLetterCount > 0)
	{
		double dRawConfidence = dConfidenceValue / nSpatialLetterCount;
		double dAverageRatio = dWordCharRatioValue / nLetterCount;
		zoneData.m_nConfidence = lround(dRawConfidence * dAverageRatio);

		// Store the original zone confidence and span so these can be used for comparison at a
		// later time.
		if (bIsOriginal)
		{
			zoneData.m_nOriginalConfidence = zoneData.m_nConfidence;
			zoneData.m_nOriginalSpan = zoneData.m_nSpan;
		}
	}

	// Set m_pPreferredResult if the current result matches a preferred format.
	updateZonePreferredResult(zoneData);
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::updateZonePreferredResult(ZoneData& zoneData)
{
	// For simplicity, only consider a zone eligible for a preferred result if the zone has only a
	// single result.
	if (zoneData.m_vecResults.size() == 1)
	{
		IRegularExprParserPtr ipPreferredFormatParser = getPreferredFormatRegex();

		// If there is no preferred format, there will be no preferred result.
		if (ipPreferredFormatParser != __nullptr)
		{
			OCRResult *pResult = zoneData.m_vecResults.back();

			// The current result is the preferred result if it doesn't already have a preferred
			// result or the existing preferred result is of lower confidence and this result
			// matches the preferred format.
			if ((zoneData.m_pPreferredResult == __nullptr || 
					pResult->m_nOverallConfidence > zoneData.m_pPreferredResult->m_nOverallConfidence) &&
				asCppBool(ipPreferredFormatParser->StringContainsPattern(zoneData.m_strResult.c_str())))
			{
				zoneData.m_pPreferredResult = pResult;
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
CEnhanceOCR::OCRResult* CEnhanceOCR::mergeResults(const OCRResult& existingResult, 
												const OCRResult& newResult,
												bool bAllowNewNonWordChars)
{
	OCRResult *pMergedOCRResult = new OCRResult();

	// Keeps track of the last letter added to pMergedOCRResult.
	const CPPLetter *pLastLetter = __nullptr;
	
	// Keeps track of the last spatial letter added to pMergedOCRResult.
	const CPPLetter *pLastSpatialLetter = __nullptr;

	// Compile a set of all x-coordinates in the results to be merged where a letter begins.
	set<long> setXPositions;
	for each (const CPPLetter &letter in existingResult.m_vecLetters)
	{
		setXPositions.insert(letter.m_ulLeft);
	}
	for each (const CPPLetter &letter in newResult.m_vecLetters)
	{
		setXPositions.insert(letter.m_ulLeft);
	}

	// Iterate each x-coordinate where letters begin to find the character of highest confidence at
	// that position for either result.
	for each (long nXPos in setXPositions)
	{
		// If a spatial letter has already been added that is to the right of the current position,
		// no further processing needs to be done at this position.
		if (pLastSpatialLetter != __nullptr && nXPos <= (long)pLastSpatialLetter->m_ulLeft)
		{
			continue;
		}

		// The best letter found thus far at nXPos
		const CPPLetter *pBestLetter = __nullptr;
		// The confidence of pBestLetter;
		long nBestConfidence = 0;
		// The center position of pBestLetter;
		unsigned long nBestPos = 0;
		// The OCRResult pBestLetter is originally from. 
		OCRResult *pSourceResult = __nullptr;
		// Indicates whether the initial candidate letter is from the existing result.
		bool bOriginalCandidateFoundInExistingResult = false;

		// This loop will first search the existingResult at nXPos, then newResult at nXPos, then
		// continue to loop the result from which the current candidate letter was not found until
		// it is clear that result doesn't have a better alternative at nXPos.
		for (long i = 0; true; i++)
		{
			// Get a reference to the result currently being searched.
			const OCRResult &currentResult = (i == 0)
				? existingResult : 
					(i == 1) ? newResult :
						bOriginalCandidateFoundInExistingResult ? newResult : existingResult;

			// nLetterIndex will be set to the index of the letter in the currentResult currently
			// being explored as a candidate.
			long nLetterIndex = -1;
			// nLetterPos will be set to the x-coordinate mid-point of any letter currently being
			// explored as a candidate.
			long nLetterPos = -1;

			// If this is the initial time the result is being searched at this position, advance to
			// the first letter at or after nXPos that comes after the last letter added to
			// pMergedOCRResult.
			if (i < 2)
			{
				nLetterIndex = currentResult.AdvanceToPos(nXPos);
				nLetterPos = currentResult.GetPos(nLetterIndex);
				while (nLetterPos >= 0 && pLastSpatialLetter != __nullptr &&
					   nLetterPos <= (long)pLastSpatialLetter->m_ulRight)
				{
					long nNextIndex = currentResult.AdvanceToNextPos(nLetterPos);
					long nNextPos = currentResult.GetPos(nNextIndex);
					if (nNextPos > nLetterPos)
					{
						nLetterIndex = nNextIndex;
						nLetterPos = nNextPos;
					}
					else
					{
						nLetterIndex = -1;
						nLetterPos = -1;
						break;
					}
				}
			}
			// If each result has already been searched, as long as a candidate letter was found,
			// continue searching the result the candidate did not come from to see if any following
			// letters might qualify instead of the candidate.
			else if (pBestLetter != __nullptr)
			{
				long nNextIndex = currentResult.AdvanceToNextPos(nXPos);
				long nNextPos = currentResult.GetPos(nNextIndex);
				if (nNextPos > nXPos && nNextPos <= (long)pBestLetter->m_ulRight)
				{
					nXPos = nNextPos;
					nLetterIndex = nNextIndex;					
					nLetterPos = nNextPos;
				}
				else
				{
					break;
				}
			}
			// But if a candidate has not been found at this position, stop searching and go to the
			// next nXPos.
			else
			{
				break;
			}

			// Do not consider this letter if no letter was found for this x-coordinate.
			if (nLetterIndex == -1)
			{
				continue;
			}

			// Get the CPPLetter at nLetterIndex.
			const CPPLetter *pLetter = currentResult.GetLetter(nLetterIndex);

			// Do not consider this letter if any of the following are true:
			// - The letter found is > 50% overlapping with the last letter added to pMergedOCRResult.
			// - The letter is the same as the last letter added to pMergedOCRResult.
			if ((pLastLetter != __nullptr && pLastSpatialLetter != __nullptr && 
					nLetterPos < (long)pLastSpatialLetter->m_ulRight) ||
				(pLetter == pLastLetter))
			{
				continue;
			}

			if (pBestLetter != __nullptr)
			{
				// If the new candidate is to the left of the last one, disregard the previous
				// candidate.
				if (pLetter->m_ulRight < pBestLetter->m_ulLeft)
				{
					pBestLetter = __nullptr;
				}
				// Do not consider this letter if pBestLetter is spatial while this one is not and
				// pBestLetter is at least 50% overlapping with this whitespace.
				else if (pBestLetter->m_bIsSpatial && !pLetter->m_bIsSpatial &&
						 nBestPos >= pLetter->m_ulLeft && nBestPos <= pLetter->m_ulRight)
				{
					continue;
				}
				// Do not consider this letter if either of the following are true:
				// - pBestLetter is to the left of this this letter
				// - The best candidate is a word character that already exceeds the specified
				//		confidence threshold. Continuing to try to correct text that is already of
				//		a decent confidence doesn't tend to make things better (and often makes it
				//		worse.)
				else if (nLetterPos > (long)pBestLetter->m_ulRight ||
						 (isWordChar(pBestLetter->m_usGuess1) && nBestConfidence > m_nConfidenceCriteria))
				{
					continue;
				}
			}

			// At this point pLetter will be the best candidate as long is it has a higher
			// confidence than any existing candidate.
			long nConfidence = currentResult.GetConfidence(nLetterIndex);
			if (pBestLetter == __nullptr || nConfidence > nBestConfidence)
			{
				nBestConfidence = nConfidence;
				nBestPos = nLetterPos;
				pBestLetter = pLetter;
				pSourceResult = currentResult.GetSourceResult(nLetterIndex);
				if (i == 0)
				{
					bOriginalCandidateFoundInExistingResult = true;
				}
			}
		}

		// Add the best candidate at nXPos to the result.
		if (pBestLetter != __nullptr)
		{
			pMergedOCRResult->AddLetter(*pBestLetter, pSourceResult);

			pLastLetter = pBestLetter;
			if (pBestLetter->m_bIsSpatial)
			{
				pLastSpatialLetter = pBestLetter;
			}
		}
	}

	return pMergedOCRResult;
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::applyZoneResults(ISpatialStringPtr ipOriginalText, const ZoneData& zoneData)
{
	IIUnknownVectorPtr ipResultsVector(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI36733", ipResultsVector != __nullptr);

	// If the zone has a preferred result, unless the current result has a much higher confidence,
	// use the preferred result.
	if (zoneData.m_pPreferredResult != __nullptr && 
		zoneData.m_nConfidence <
			zoneData.m_pPreferredResult->m_nOverallConfidence + gnPREFERRED_FORMAT_CONF_BIAS)
	{
		// Remove any extra whitespace characters that may have resulted from merging results.
		zoneData.m_pPreferredResult->TrimExtraWhiteSpace();
		ISpatialStringPtr ipResult = zoneData.m_pPreferredResult->ToSpatialString(
			m_strSourceDocName, m_nCurrentPage, m_ipPageInfoMap);
		ASSERT_RESOURCE_ALLOCATION("ELI36739", ipResult != __nullptr);

		ipResultsVector->PushBack(ipResult);
	}
	else
	{
		// Aggregate each result into ipZoneResult (delimited by newlines if there are multiple).
		for each (OCRResult *pResultLine in zoneData.m_vecResults)
		{
			// Remove any extra whitespace characters that may have resulted from merging results.
			pResultLine->TrimExtraWhiteSpace();

			ISpatialStringPtr ipResult = pResultLine->ToSpatialString(
				m_strSourceDocName, m_nCurrentPage, m_ipPageInfoMap);
			ASSERT_RESOURCE_ALLOCATION("ELI36738", ipResult != __nullptr);

			ipResultsVector->PushBack(ipResult);
		}
	}

	// https://extract.atlassian.net/browse/ISSUE-12088
	// Apply each result from a zone separately to avoid cases where multiple lines of text are
	// inserted within what was a single line of text (moves text from a lower line into the line
	// above).
	long nCount = ipResultsVector->Size();
	for (long nResult = 0; nResult < nCount; nResult++)
	{
		ISpatialStringPtr ipResult = ipResultsVector->At(nResult);
		ASSERT_RESOURCE_ALLOCATION("ELI36734", ipResult != __nullptr);

		// If ipZoneResult is non-empty, apply it to ipOriginalText.
		if (asCppBool(ipResult->HasSpatialInfo()))
		{
			// If original text was empty, just add ipResult to it.
			if (!asCppBool(ipOriginalText->HasSpatialInfo()))
			{
				ipOriginalText->Append(ipResult);
			}
			// If this zone did not have previously have any value, insert ipResult into
			// ipOriginalText using it's spatial location to determine where it should be inserted.
			else if (!asCppBool(zoneData.m_ipOriginalText->HasSpatialInfo()))
			{
				ipOriginalText->InsertBySpatialPosition(ipResult, VARIANT_FALSE);
			}
			// If this zone had a previous value, remove the previous value and insert ipResult in
			// its place.
			else
			{
				ILongRectanglePtr ipBounds = ipResult->GetOCRImageBounds();
				ASSERT_RESOURCE_ALLOCATION("ELI36731", ipBounds != __nullptr);

				// https://extract.atlassian.net/browse/ISSUE-12088
				// Only remove text within the bounds of the replacement text to avoid removing
				// chars that shouldn't be removed and inserting replacement text in an
				// inappropriate location.
				long nPos = ipOriginalText->RemoveText(
					zoneData.m_ipOriginalText, m_nCurrentPage, ipBounds);

				if (nPos != -1)
				{
					nPos = ipOriginalText->SetSurroundingWhitespace(ipResult, nPos);
					ipOriginalText->Insert(nPos, ipResult);
				}
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
IOCREnginePtr CEnhanceOCR::getOCREngine()
{
	if (m_ipOCREngine == __nullptr)
	{
		m_ipOCREngine.CreateInstance(CLSID_ScansoftOCR);
		ASSERT_RESOURCE_ALLOCATION("ELI36579", m_ipOCREngine != __nullptr );
		
		IPrivateLicensedComponentPtr ipOCREngineLicense(m_ipOCREngine);
		ASSERT_RESOURCE_ALLOCATION("ELI36580", ipOCREngineLicense != __nullptr);
		ipOCREngineLicense->InitPrivateLicense(LICENSE_MGMT_PASSWORD.c_str());
	}

	return m_ipOCREngine;
}
//--------------------------------------------------------------------------------------------------
ISpatialStringSearcherPtr CEnhanceOCR::getSpatialStringSearcher()
{
	if (m_ipSpatialStringSearcher == __nullptr)
	{
		m_ipSpatialStringSearcher.CreateInstance(CLSID_SpatialStringSearcher);
		ASSERT_RESOURCE_ALLOCATION("ELI36581", m_ipSpatialStringSearcher != __nullptr);

		m_ipSpatialStringSearcher->InitSpatialStringSearcher(m_ipCurrentPageText, VARIANT_TRUE);
		// Include any chars whose midpoints fall within the specified region.
		m_ipSpatialStringSearcher->SetUseMidpointsOnly(VARIANT_TRUE);
	}

	return m_ipSpatialStringSearcher;
}
//--------------------------------------------------------------------------------------------------
UCLID_AFVALUEMODIFIERSLib::ISplitRegionIntoContentAreasPtr CEnhanceOCR::getSRICA()
{
	if (m_ipSRICA == __nullptr)
	{
		m_ipSRICA.CreateInstance(CLSID_SplitRegionIntoContentAreas);
		ASSERT_RESOURCE_ALLOCATION("ELI36582", m_ipSRICA != __nullptr);

		m_ipSRICA->UseLines = VARIANT_TRUE;
		m_ipSRICA->OCRThreshold = 0;
		m_ipSRICA->RequiredHorizontalSeparation = 4;
		m_ipSRICA->MinimumWidth = 0.5;
		m_ipSRICA->MinimumHeight = 0.5;
	}

	return m_ipSRICA;
}
//--------------------------------------------------------------------------------------------------
IRegularExprParserPtr CEnhanceOCR::getPreferredFormatRegex()
{
	if (m_ipPreferredFormatParser == __nullptr && !m_strPreferredFormatRegexFile.empty())
	{
		string strExpandedFileName = expandPathTagsAndFunctions(m_strPreferredFormatRegexFile);
		m_cachedRegexLoader.loadObjectFromFile(strExpandedFileName);

		// Use AF Utility instead? (to be able to parse attribute references)
		m_ipPreferredFormatParser = m_ipMiscUtils->GetNewRegExpParserInstance("");
		ASSERT_RESOURCE_ALLOCATION("ELI36583", m_ipPreferredFormatParser != __nullptr);
		
		m_ipPreferredFormatParser->Pattern = m_cachedRegexLoader.m_obj.c_str();
	}

	return m_ipPreferredFormatParser;
}
//--------------------------------------------------------------------------------------------------
string CEnhanceOCR::expandPathTagsAndFunctions(string strFileName)
{
	if (m_ipTagUtility != __nullptr)
	{
		// If a tag manager has been passed in via EnhanceDocument, use it.
		return asString(m_ipTagUtility->ExpandTagsAndFunctions(
			strFileName.c_str(), m_strSourceDocName.c_str(), __nullptr));
	}
	else
	{
		// Otherwise a rule object execution context is assumed; use an AFTagManager to expand tags.
		AFTagManager tagMgr;
		return tagMgr.expandTagsAndFunctions(strFileName, m_ipCurrentDoc);
	}
}
//--------------------------------------------------------------------------------------------------
CommentedTextFileReader CEnhanceOCR::getFileReader(const string& strFilename)
{
	CachedObjectFromFile<string, StringLoader>& filterLoader =
		m_cachedFileLoaders[strFilename];
	
	filterLoader.loadObjectFromFile(strFilename);

	m_cachedFileLines[strFilename].clear();
	StringTokenizer::sGetTokens(filterLoader.m_obj, "\r\n", m_cachedFileLines[strFilename]);

	CommentedTextFileReader fileReader(m_cachedFileLines[strFilename], "//", true);

	return fileReader;
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::reset()
{
	// Return configurable variables to their default values.
	m_nConfidenceCriteria = 60;
	m_eFilterPackage = (UCLID_AFVALUEMODIFIERSLib::EFilterPackage)kMedium;
	m_strCustomFilterPackage = "";
	m_strPreferredFormatRegexFile = "";
	m_strCharsToIgnore = "_";
	m_bOutputFilteredImages = false;
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI36584", "Enhance OCR");
}
//--------------------------------------------------------------------------------------------------
bool CEnhanceOCR::isEnhanceOCRLicensed()
{
	if (LicenseManagement::isLicensed(gnENHANCE_OCR))
	{
		return true;
	}
	else
	{
		CSingleLock lg(&gMutex, TRUE);

		if (!gbLoggedUnlicensedException)
		{
			UCLIDException ue("ELI36729",
				"Application trace: Enhance OCR is not licensed. OCR text is not being enhanced.");
			ue.log();

			gbLoggedUnlicensedException = true;
		}

		return false;
	}
}

//--------------------------------------------------------------------------------------------------
// OCRResult public members
//--------------------------------------------------------------------------------------------------
CEnhanceOCR::OCRResult::OCRResult()
: m_nOverallConfidence(0)
, m_dWordCharRatio(0)
, m_nAverageCharWidth(0)
, m_nSpatialCharCount(0)
, m_bIsOriginal(false)
{
}
//--------------------------------------------------------------------------------------------------
CEnhanceOCR::OCRResult::OCRResult(ISpatialStringPtr ipSpatialString, bool bIsOriginal,
								 string strCharsToIgnore)
: m_nOverallConfidence(0)
, m_dWordCharRatio(0)
, m_nAverageCharWidth(0)
, m_nSpatialCharCount(0)
, m_bIsOriginal(bIsOriginal)
{
	// Any result based on a null or non-spatial string will be "empty".
	if (ipSpatialString == __nullptr || !asCppBool(ipSpatialString->HasSpatialInfo()))
	{
		return;
	}
	
	// The letters for all results will be loaded in terms of OCR coordinates so that it can be
	// assumed that each letter in a word comes to the right of the last regardless of page
	// orientation.
	CPPLetter* pLetters = __nullptr;
	long nNumLetters = -1;
	long nSpatialCharCount = 0;
	long nWordCharCount = 0;
	ipSpatialString->GetOCRImageLetterArray(&nNumLetters, (void**)&pLetters);

	vector<CPPLetter> vecRemainingLetters;
	bool bResultValid = populateLetters(pLetters, nNumLetters, vecRemainingLetters, strCharsToIgnore);

	if (vecRemainingLetters.empty())
	{
		ipSpatialString->Clear();
	}
	else
	{
		ipSpatialString->CreateFromLetterArray(vecRemainingLetters.size(),
			&vecRemainingLetters[0], ipSpatialString->SourceDocName,
			ipSpatialString->SpatialPageInfos);
	}

	if (bResultValid)
	{
		updateStats();
	}
	else
	{
		m_vecLetters.clear();
		m_vecSourceResults.clear();
	}
}
//--------------------------------------------------------------------------------------------------
bool CEnhanceOCR::OCRResult::IsEmpty() const
{
	// If there are no spatial characters, this result cannot be used for the purpose of comparing
	// and merging OCR results. Consider it empty.
	for each (const CPPLetter &letter in m_vecLetters)
	{
		if (letter.m_bIsSpatial)
		{
			return false;
		}
	}
			
	return true;
}
//--------------------------------------------------------------------------------------------------
unsigned long CEnhanceOCR::OCRResult::Span(double dFactor, unsigned long nMaxCharWidth) const
{
	unsigned long ulSpan = 0;

	// Compute the combined width of all spatial chars except that the width of any one char should
	// not be wider than nMaxCharWidth (so that results that read multiple charcters from the page
	// as a single character to not get credit where not is deserved. Results that OCR a bunch of
	// tiny chars such as punctuation are allowed by be de-emphacized)
	for each (const CPPLetter &letter in m_vecLetters)
	{
		if (letter.m_bIsSpatial && letter.m_usGuess1 != '^')
		{
			ulSpan += (nMaxCharWidth > 0)
				? min(nMaxCharWidth, letter.m_ulRight - letter.m_ulLeft)
				: letter.m_ulRight - letter.m_ulLeft;
		}
	}
	
	// dFactor normalizes char width based on the filter currently being used.
	return (unsigned long)lround((double)ulSpan * dFactor);
}
//--------------------------------------------------------------------------------------------------
long CEnhanceOCR::OCRResult::CompareVerticalArea(const OCRResult &other) const
{
	if (m_vecLetters.empty())
	{
		// This result comes first.
		return 1;
	}
	else if (other.m_vecLetters.empty())
	{
		// Other result comes first.
		return -1;
	}

	for each (const CPPLetter &letter in m_vecLetters)
	{
		if (other.GetInsertionPosition(&letter, false, false) != -1)
		{
			// Other result spatially overlaps with this one.
			return 0;
		}
	}

	if (other.m_vecLetters[0].m_ulTop >= m_vecLetters[0].m_ulTop)
	{
		// This result comes first.
		return 1;
	}
	else
	{
		// Other result comes first.
		return -1;
	}
}
//--------------------------------------------------------------------------------------------------
long CEnhanceOCR::OCRResult::AdvanceToPos(long nXPos) const
{ 
	// Return the horizontal midpoint of the letter at nXPos.
	return GetIndexOfPos(nXPos);
}
//--------------------------------------------------------------------------------------------------
long CEnhanceOCR::OCRResult::AdvanceToNextPos(long nXPos) const
{ 
	// Return the index of the letter after the one at nXPos (or the next to the right of nXPos if
	// there is no letter at nXPos).
	long nResultIndex = -1;
	for (long i = (long)m_vecLetters.size() - 2; i >= 0; i--) 
	{
		if ((long)m_vecLetters[i].m_ulLeft <= nXPos && (long)m_vecLetters[i].m_ulRight >= nXPos)
		{
			// We found the letter at this pos. Return the index of the next letter.
			return i + 1;
		}
		else if ((long)m_vecLetters[i].m_ulLeft > nXPos)
		{
			// As long as the left position is to the right of nXPos, make this the tentative result
			// in case there is no letter at nXPos.
			nResultIndex = i;
		}
	}

	return nResultIndex;
}
//--------------------------------------------------------------------------------------------------
const CPPLetter* CEnhanceOCR::OCRResult::GetLetter(long nIndex) const
{
	// Return a pointer the CPPLetter at nXPos.
	if (nIndex >= 0)
	{
		return &m_vecLetters[nIndex];
	}
	else
	{
		return __nullptr;
	}
}
//--------------------------------------------------------------------------------------------------
long CEnhanceOCR::OCRResult::GetPos(long nIndex) const
{
	// Return the confidence of the letter at this index.
	if (m_vecLetters.empty() || nIndex < 0)
	{
		return -1;
	}

	return (long)(m_vecLetters[nIndex].m_ulLeft + m_vecLetters[nIndex].m_ulRight) / 2;
}
//--------------------------------------------------------------------------------------------------
long CEnhanceOCR::OCRResult::GetConfidence(long nIndex) const
{
	// Return the confidence of the letter at this index.
	if (m_vecLetters.empty() || nIndex < 0)
	{
		return -1;
	}

	long nConfidence = m_vecLetters[nIndex].m_bIsSpatial && m_vecLetters[nIndex].m_usGuess1 != '^'
		? lround(((m_vecLetters[nIndex].m_ucCharConfidence + 
				  m_vecSourceResults[nIndex]->m_nOverallConfidence) / 2)
			* m_vecSourceResults[nIndex]->m_dWordCharRatio)
		: 0;
	return nConfidence;
}
//--------------------------------------------------------------------------------------------------
CEnhanceOCR::OCRResult* CEnhanceOCR::OCRResult::GetSourceResult(long nIndex) const
{
	// Return a pointer the original OCRResult instance of the letter at nIndex.
	if (nIndex >= 0)
	{
		return m_vecSourceResults[nIndex];
	}
	else
	{
		return __nullptr;
	}
}
//--------------------------------------------------------------------------------------------------
bool CEnhanceOCR::OCRResult::AllCharsMeetConfidenceCriteria(long nConfidence)
{
	// Do all spatial chars have a confidence of at least nConfidence?
	for (size_t i = 0; i < m_vecLetters.size(); i++)
	{
		if (m_vecLetters[i].m_bIsSpatial && GetConfidence(i) < nConfidence)
		{
			return false;
		}
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::OCRResult::AddLetter(CPPLetter newLetter, CEnhanceOCR::OCRResult *pSourceResult)
{
	// Adds a new letter to this instance. This is assumed to be a new OCR results from a spatial
	// string.
	if (!m_vecLetters.empty() && m_vecLetters.back().m_ulRight >= newLetter.m_ulLeft)
	{
		m_vecLetters.back().m_ulRight = newLetter.m_ulLeft - 1;
	}

	m_vecLetters.push_back(newLetter);
	m_vecSourceResults.push_back(pSourceResult);

	updateStats();
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::OCRResult::TrimExtraWhiteSpace()
{
	// Removes any duplicated whitespace created by merging two results together.
	set<long> lettersToKeep;
	CPPLetter *pLastLetter = __nullptr;

	long nLetterCount = m_vecLetters.size();
	unsigned short usLastLetter = 0;
	for (long nPos = 0; nPos < nLetterCount; nPos ++)
	{
		const CPPLetter& letter = m_vecLetters[nPos];

		if ((char)letter.m_usGuess1 == '^')
		{
			continue;
		}

		if (pLastLetter != __nullptr &&
			(letter.m_usGuess1 == pLastLetter->m_usGuess1) && letter.m_usGuess1 == 32/*space*/)
		{
			continue;
		}

		lettersToKeep.insert(nPos);
		pLastLetter = (CPPLetter *)&letter;
	}

	for (long nPos = nLetterCount - 1; nPos >= 0; nPos--)
	{
		if (lettersToKeep.find(nPos) == lettersToKeep.end())
		{
			m_vecLetters.erase(m_vecLetters.begin() + nPos);
			m_vecSourceResults.erase(m_vecSourceResults.begin() + nPos);
		}
	}
}
//--------------------------------------------------------------------------------------------------
string CEnhanceOCR::OCRResult::ToString() const
{
	// Converts this current instance to a string.
	string strString;
	for each (const CPPLetter &letter in m_vecLetters)
	{
		strString += (char)letter.m_usGuess1;
	}

	return strString;
}
//--------------------------------------------------------------------------------------------------
ISpatialStringPtr CEnhanceOCR::OCRResult::ToSpatialString(string strSourceDocName, long nPage,
														  ILongToObjectMapPtr ipPageInfoMap) const
{
	// Converts this current instance to a SpatialString using the specified SourceDocName, page and
	// PageInfoMap.
	ISpatialStringPtr ipSpatialString(CLSID_SpatialString);

	if (m_vecLetters.size() > 0)
	{
		ipSpatialString->CreateFromLetterArray(m_vecLetters.size(),
			(void *)&m_vecLetters[0], strSourceDocName.c_str(), ipPageInfoMap);
	}

	return ipSpatialString;
}

//--------------------------------------------------------------------------------------------------
// OCRResult private members
//--------------------------------------------------------------------------------------------------
bool CEnhanceOCR::OCRResult::populateLetters(CPPLetter *pLetters, long nNumLetters,
											vector<CPPLetter> &vecRemainingLetters,
											string strCharsToIgnore)
{
	CRect rectLastSpatial;
	vector<CPPLetter> vecNonSpatialLetters;
	CPPLetter *pLastLetter = __nullptr;
	bool bResultValid = true;
	long nInsertionIndex = -1;
	long nPage = -1;

	for (long i = 0; i < nNumLetters; i++)
	{
		CPPLetter &letter = pLetters[i];
		char c = (char)letter.m_usGuess1;

		//https://extract.atlassian.net/browse/ISSUE-12088
		// If the OCR has added a double letter by adding the same letter twice with the same
		// spatial info, divide the spatial info so the first character gets the left half of the
		// spatial area and the second letter gets the right half.
		if (pLastLetter != __nullptr && letter == *pLastLetter)
		{
			unsigned long ulMidpoint = (letter.m_ulLeft + letter.m_ulRight) / 2;
			pLastLetter->m_ulRight = ulMidpoint;
			letter.m_ulLeft = ulMidpoint + 1;
		}

		pLastLetter = &letter;

		// Pretend as if any chars specified in strCharsToIgnore don't exist in pLetters; tread as
		// whitespace instead.
		if (strCharsToIgnore.find(c) != string::npos)
		{
			letter = gLETTER_SPACE;
		}

		if (letter.m_bIsSpatial)
		{
			// nInsertionIndex will indicate where to add this letter to the result (positive),
			// whether this letter belongs in another result because it is on a different line or
			// page (-1), or whether the result is invalid (-2)
			if (nPage == -1)
			{
				nPage = letter.m_usPageNumber;

				// The first spatial letter will be the first letter in the result.
				nInsertionIndex = 0;
			}
			else if (nPage != (long)letter.m_usPageNumber)
			{
				nInsertionIndex = -1;
			}
			else
			{
				nInsertionIndex = GetInsertionPosition(&letter, nInsertionIndex != -1, true);
				if (nInsertionIndex == -2)
				{
					bResultValid = false;
					continue;
				}
			}

			// Collect all letters that belong in a separate result into vecRemainingLetters.
			if (nInsertionIndex == -1)
			{
				// Any subsequent characters that do belong to this result should be separated by a
				// space char.
				if (vecNonSpatialLetters.empty())
				{
					vecNonSpatialLetters.push_back(CPPLetter(gLETTER_SPACE));
				}
				vecRemainingLetters.push_back(letter);
				continue;
			}

			// The letter belongs somewhere before the end of the current result.
			if (nInsertionIndex < (long)m_vecLetters.size())
			{
				m_vecLetters.insert(m_vecLetters.begin() + nInsertionIndex, letter);
				m_vecSourceResults.insert(m_vecSourceResults.begin() + nInsertionIndex, this);
				
				// Since the characters are out of order, there is no good way of knowing where this
				// whitespace should have gone.
				vecNonSpatialLetters.clear();
			}
			// The letter belongs on the end of the result after any pending whitespace.
			else
			{
				// There is pending whitespace. To allow mergeResults to work as effectively as it
				// can, it helps to have spatial info assigned to the non spatial characters.
				// Distribute the non-spatial chars evenly between this letter and the last.
				if (!vecNonSpatialLetters.empty())
				{
					long nSizePerLetter = 0;
					long nPos = letter.m_ulLeft;
					if (nPos > rectLastSpatial.right)
					{
						nSizePerLetter = (nPos - rectLastSpatial.right) / vecNonSpatialLetters.size();
					}

					if (nSizePerLetter > 1)
					{
						rectLastSpatial.OffsetRect(rectLastSpatial.right + 1 - rectLastSpatial.left, 0);
						rectLastSpatial.right = rectLastSpatial.left + nSizePerLetter - 1;

						for each (CPPLetter nonSpatialLetter in vecNonSpatialLetters)
						{
							nonSpatialLetter.m_ulLeft = rectLastSpatial.left;
							nonSpatialLetter.m_ulRight = rectLastSpatial.right;
							nonSpatialLetter.m_ulTop = rectLastSpatial.top;
							nonSpatialLetter.m_ulBottom = rectLastSpatial.bottom;

							rectLastSpatial.OffsetRect(nSizePerLetter, 0);
							m_vecLetters.push_back(nonSpatialLetter);
							m_vecSourceResults.push_back(this);
						}
					}

					vecNonSpatialLetters.clear();
				}

				rectLastSpatial.SetRect(
					letter.m_ulLeft, letter.m_ulTop, letter.m_ulRight, letter.m_ulBottom);
						
				m_vecLetters.push_back(letter);
				m_vecSourceResults.push_back(this);
			}
		}
		// Aggregate any whitespace that is not newlines for insertion before the next spatial char.
		else if (c != '\r' && c != '\n')
		{
			// If the last spatial letter is to be part of a separate result, any whitespace
			// following it should be as well.
			if (nInsertionIndex == -1)
			{
				vecRemainingLetters.push_back(letter);
			}
			else
			{
				if (!rectLastSpatial.IsRectNull())
				{
					vecNonSpatialLetters.push_back(letter);
				}
			}
		}
		// Since each OCRResult should be limited to a single line of text, we don't want any
		// newlines in the result; convert to a space.
		else if (vecNonSpatialLetters.empty())
		{
			vecNonSpatialLetters.push_back(CPPLetter(gLETTER_SPACE));
		}
	}

	return bResultValid;
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCR::OCRResult::updateStats()
{
	m_nSpatialCharCount = 0;
	long nConfidenceTotal = 0;
	long nWordCharCount = 0;
	long nTotalWidth = 0;

	// Collect aggregate data for all letters in the results.
	for each (const CPPLetter &letter in m_vecLetters)
	{
		if (letter.m_bIsSpatial && letter.m_usGuess1 != '^')
		{
			nConfidenceTotal += letter.m_ucCharConfidence;
			m_nSpatialCharCount++;

			char c = (char)letter.m_usGuess1;
			if (isWordChar(c))
			{
				nWordCharCount++;
				nTotalWidth += letter.m_ulRight - letter.m_ulLeft;
			}
		}
	}

	// Average the aggregate data across all spatial/word chars.
	if (m_nSpatialCharCount > 0)
	{
		m_nOverallConfidence = nConfidenceTotal / m_nSpatialCharCount;
		m_dWordCharRatio = (double)nWordCharCount / m_nSpatialCharCount;
	}
	if (nWordCharCount > 0)
	{
		m_nAverageCharWidth = nTotalWidth / nWordCharCount;
	}
}
//--------------------------------------------------------------------------------------------------
long CEnhanceOCR::OCRResult::GetIndexOfPos(long nXPos) const
{
	if (nXPos < 0)
	{
		return -1;
	}

	for (long i = (long)m_vecLetters.size() - 1; i >= 0; i--) 
	{
		if ((long)m_vecLetters[i].m_ulLeft <= nXPos && (long)m_vecLetters[i].m_ulRight >= nXPos)
		{
			return i;
		}
	}

	return -1;
}
//--------------------------------------------------------------------------------------------------
long CEnhanceOCR::OCRResult::GetInsertionPosition(const CPPLetter *pLetter, bool bProbablySameLine,
												  bool bIsLastChar) const
{
	// If nothing has yet been added to this result, the letter should go at index 0.
	if (m_vecLetters.empty())
	{
		return 0;
	}

	// The spatial coordinates of letters often overlap, especially when chars from different
	// results are combined. Compare the midpoint of the char to avoid having to deal with overlap.
	unsigned long ulVerticalMidPoint = (pLetter->m_ulTop + pLetter->m_ulBottom) / 2;
	unsigned long ulHorizontalMidpoint = (pLetter->m_ulLeft + pLetter->m_ulRight) / 2;
	CRect rectLastSpatial(m_vecLetters[0].m_ulLeft, m_vecLetters[0].m_ulTop,
		m_vecLetters[0].m_ulRight, m_vecLetters[0].m_ulBottom);
	long nIndex = 0;
	const CPPLetter *pComparisonLetter = __nullptr;

	// Loop each character already in the result for the last one that is to the left of the new
	// letter.
	for (nIndex = 0; nIndex < (long)m_vecLetters.size(); nIndex++)
	{
		pComparisonLetter = &m_vecLetters[nIndex];
		if (!pComparisonLetter->m_bIsSpatial)
		{
			continue;
		}

		if (bIsLastChar && ulHorizontalMidpoint < pComparisonLetter->m_ulRight)
		{
			// If the new letter doesn't fall to the right of all others, this likely indicates it
			// is in a new line of text.
			bProbablySameLine = false;
			break;
		}
		// Punctuation can be small and sometimes throws off spatial comparisons; ignore punctuation
		// when determining where to insert chars.
		else if (isWordChar(pComparisonLetter->m_usGuess1))
		{
			rectLastSpatial.UnionRect(rectLastSpatial,
				CRect(pComparisonLetter->m_ulLeft,
					pComparisonLetter->m_ulTop,
					pComparisonLetter->m_ulRight,
					pComparisonLetter->m_ulBottom));
		}
	}

	// Insert the letter at the current nIndex if any of the following are true:
	// - The letter is vertically in line with the preceding letter.
	// - The letter is vertically in line with the previous non-punctuation char.
	// - The letter is expected to fall on the same line and is not a word char or the preceding
	//		letter is not spatial.
	if ((ulVerticalMidPoint >= pComparisonLetter->m_ulTop && ulVerticalMidPoint <= pComparisonLetter->m_ulBottom) ||
		((long)ulVerticalMidPoint >= rectLastSpatial.top && (long)ulVerticalMidPoint <= rectLastSpatial.bottom) ||
		(bProbablySameLine && (!isWordChar(pLetter->m_usGuess1) || !isWordChar(pComparisonLetter->m_usGuess1))))
	{
		// But if the letter appears to mostly overlap the previous letter, it is quite likely the
		// result of a bad OCR result recognizing characters where there are none. Consider the
		// result invalid.
		if (ulHorizontalMidpoint < pComparisonLetter->m_ulRight)
		{
			return -2;
		}

		return nIndex;
	}

	// Otherwise, the letter should be added to the end.
	return -1;
}