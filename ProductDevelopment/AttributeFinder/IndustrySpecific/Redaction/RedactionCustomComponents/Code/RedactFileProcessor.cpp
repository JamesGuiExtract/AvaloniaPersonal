// RedactFileProcessor.cpp : Implementation of CRedactFileProcessor
#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "RedactFileProcessor.h"
#include "RedactionCCConstants.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 8;

//-------------------------------------------------------------------------------------------------
// CRedactFileProcessor
//-------------------------------------------------------------------------------------------------
CRedactFileProcessor::CRedactFileProcessor()
:	m_ipAttributeNames(NULL),
	m_bUseVOA(false),
	m_bCarryForwardAnnotations(false),
	m_bApplyRedactionsAsAnnotations(false),
	m_bUseRedactedImage(false)
{
	// set members to their iniital states
	clear();

	// Create the Attribute Names collection
	m_ipAttributeNames.CreateInstance(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI28238", m_ipAttributeNames != __nullptr);

	// Add the default selected Attributes to the collection (P16 #2751)
	m_ipAttributeNames->PushBack("HCData");
	m_ipAttributeNames->PushBack("MCData");
	m_ipAttributeNames->PushBack("LCData");
}
//-------------------------------------------------------------------------------------------------
CRedactFileProcessor::~CRedactFileProcessor()
{
	try
	{
		m_ipAttributeNames = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28239");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRedactFileProcessor,
		&IID_IFileProcessingTask,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_ILicensedComponent,
		&IID_IAccessRequired
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IFileProcessingTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		throw UCLIDException("ELI28225", 
			"Legacy redaction task no longer supported. Use create redacted image task instead.");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28333");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::raw_ProcessFile(IFileRecord* pFileRecord, long nActionID,
	IFAMTagManager* pTagManager, IFileProcessingDB* pDB, IProgressStatus* pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult* pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		throw UCLIDException("ELI28226", 
			"Legacy redaction task no longer supported. Use create redacted image task instead.");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09881")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

		try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28240");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28241");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31197", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31198");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI28242", pbValue != __nullptr);

		try
		{
			// Check license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28243");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28244", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t("Redaction: Redact image without verification (legacy)").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28245");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_RedactFileProcessor);
		ASSERT_RESOURCE_ALLOCATION("ELI28246", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk(this);
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28247");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IRedactFileProcessorPtr ipSource( pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI28248", ipSource != __nullptr);

		m_strRuleFileName = asString(ipSource->RuleFileName);
		m_strOutputFileName = asString(ipSource->OutputFileName);
		m_bReadFromUSS = ipSource->ReadFromUSS == VARIANT_TRUE;

		// Clear the Attributes set
		m_setAttributeNames.clear();

		if (ipSource->AttributeNames != __nullptr)
		{
			m_ipAttributeNames = ipSource->AttributeNames;
			fillAttributeSet(m_ipAttributeNames, m_setAttributeNames);
		}
		else
		{
			m_ipAttributeNames = __nullptr;
		}

		// Retrieve setting for CreateOutputFile
		m_lCreateIfRedact = ipSource->CreateOutputFile;
		m_bUseVOA = ipSource->UseVOA == VARIANT_TRUE;
		m_strVOAFileName = asString(ipSource->VOAFileName);

		// Retrieve annotation settings
		m_bCarryForwardAnnotations = (ipSource->CarryForwardAnnotations == VARIANT_TRUE);
		m_bApplyRedactionsAsAnnotations = (ipSource->ApplyRedactionsAsAnnotations == VARIANT_TRUE);

		// Retrieve whether to use redacted image
		m_bUseRedactedImage = asCppBool(ipSource->UseRedactedImage);

		// Retrieve redaction appearance settings
		m_redactionAppearance.m_strText = asString(ipSource->RedactionText);
		m_redactionAppearance.m_crBorderColor = ipSource->BorderColor;
		m_redactionAppearance.m_crFillColor = ipSource->FillColor;
		
		// Retrieve font settings
		lstrcpyn(m_redactionAppearance.m_lgFont.lfFaceName, ipSource->FontName, LF_FACESIZE);
		m_redactionAppearance.m_lgFont.lfItalic = ipSource->IsItalic == VARIANT_TRUE ? gucIS_ITALIC : 0;
		m_redactionAppearance.m_lgFont.lfWeight = 
			ipSource->IsBold == VARIANT_TRUE ? FW_BOLD : FW_NORMAL;
		m_redactionAppearance.m_iPointSize = ipSource->FontSize;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28249");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::raw_IsConfigured(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbValue = asVariantBool(!m_strRuleFileName.empty() && !m_strOutputFileName.empty());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28250");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IRedactFileProcessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_RuleFileName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strRuleFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28251")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_RuleFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();
			
		string strFileName = asString(newVal);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI28252", ipFAMTagManager != __nullptr);

		// Make sure the file name contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFileName.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI28253", "The rules file name contains invalid tags!");
			ue.addDebugInfo("Rules file", strFileName);
			throw ue;
		}

		// Assign the rule file name
		m_strRuleFileName = strFileName;
		m_bDirty = true;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28254");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_OutputFileName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strOutputFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28255");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_OutputFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();
			
		string strFileName = asString(newVal);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI28256", ipFAMTagManager != __nullptr);

		// Make sure the file name contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFileName.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI28257", "The output file name contains invalid tags!");
			ue.addDebugInfo("Output file", strFileName);
			throw ue;
		}

		// Assign the output file name
		m_strOutputFileName = strFileName;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28258");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_ReadFromUSS(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bReadFromUSS);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28334");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_ReadFromUSS(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();

		m_bReadFromUSS = newVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28260");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_AttributeNames(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI28261", pVal != __nullptr);

		validateLicense();
		
		*pVal = NULL;
		if (m_ipAttributeNames != __nullptr)
		{
			// Get a ShallowCopyableObject ptr for the current name list
			IShallowCopyablePtr ipObjSource = m_ipAttributeNames;
			ASSERT_RESOURCE_ALLOCATION("ELI28262", ipObjSource != __nullptr);

			// Shallow copy the attribute names
			IVariantVectorPtr ipObjCloned = ipObjSource->ShallowCopy();
			ASSERT_RESOURCE_ALLOCATION("ELI28263", ipObjCloned != __nullptr);

			// set the return value to the shallow copied object
			*pVal = ipObjCloned.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28264");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_AttributeNames(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();
		
		m_ipAttributeNames = newVal;
		
		if (m_ipAttributeNames != __nullptr)
		{
			fillAttributeSet(m_ipAttributeNames, m_setAttributeNames);
		}
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28265");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_CreateOutputFile(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_lCreateIfRedact;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28266");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_CreateOutputFile(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();

		m_lCreateIfRedact = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28267");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_UseVOA(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bUseVOA);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28268");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_UseVOA(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bUseVOA = newVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28269");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_VOAFileName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strVOAFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28270");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_VOAFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
			
		string strFileName = asString(newVal);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI28271", ipFAMTagManager != __nullptr);

		// Make sure the file name contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFileName.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI28272", "The VOA file name contains invalid tags!");
			ue.addDebugInfo("VOA file", strFileName);
			throw ue;
		}

		// Assign the voa file name
		m_strVOAFileName = strFileName;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28273");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_CarryForwardAnnotations(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Return setting to caller
		*pVal = asVariantBool(m_bCarryForwardAnnotations);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28274");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_CarryForwardAnnotations(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Check license state
		validateLicense();

		// Save new setting
		m_bCarryForwardAnnotations = (newVal == VARIANT_TRUE);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28275");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_ApplyRedactionsAsAnnotations(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Return setting to caller
		*pVal = asVariantBool(m_bApplyRedactionsAsAnnotations);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28276");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_ApplyRedactionsAsAnnotations(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Check license state
		validateLicense();

		// Save new setting
		m_bApplyRedactionsAsAnnotations = (newVal == VARIANT_TRUE);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28277");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_UseRedactedImage(VARIANT_BOOL* pvbUseRedactedImage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28278", pvbUseRedactedImage != __nullptr);

		// Check license state
		validateLicense();

		// Return setting to caller
		*pvbUseRedactedImage = asVariantBool(m_bUseRedactedImage);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28279")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_UseRedactedImage(VARIANT_BOOL vbUseRedactedImage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_bUseRedactedImage = asCppBool(vbUseRedactedImage);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28280")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_RedactionText(BSTR* pbstrRedactionText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28281", pbstrRedactionText != __nullptr);

		// Check license state
		validateLicense();

		// Return setting to caller
		*pbstrRedactionText = _bstr_t(m_redactionAppearance.m_strText.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28282")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_RedactionText(BSTR bstrRedactionText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_redactionAppearance.m_strText = asString(bstrRedactionText);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28283")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_BorderColor(long* plBorderColor)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28284", plBorderColor != __nullptr);

		// Check license state
		validateLicense();

		// Return setting to caller
		*plBorderColor = m_redactionAppearance.m_crBorderColor;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28285")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_BorderColor(long lBorderColor)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_redactionAppearance.m_crBorderColor = lBorderColor;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28286")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_FillColor(long* plFillColor)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28287", plFillColor != __nullptr);

		// Check license state
		validateLicense();

		// Return setting to caller
		*plFillColor = m_redactionAppearance.m_crFillColor;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28288")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_FillColor(long lFillColor)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_redactionAppearance.m_crFillColor = lFillColor;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28289")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_FontName(BSTR* pbstrFontName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28290", pbstrFontName != __nullptr);

		// Check license state
		validateLicense();

		// Return setting to caller
		*pbstrFontName = _bstr_t(m_redactionAppearance.m_lgFont.lfFaceName).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28291")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_FontName(BSTR bstrFontName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		lstrcpyn(m_redactionAppearance.m_lgFont.lfFaceName, asString(bstrFontName).c_str(), LF_FACESIZE);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28292")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_IsBold(VARIANT_BOOL* pvbBold)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28293", pvbBold != __nullptr);

		// Check license state
		validateLicense();

		// Return setting to caller
		*pvbBold = asVariantBool(m_redactionAppearance.m_lgFont.lfWeight >= FW_BOLD);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28294")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_IsBold(VARIANT_BOOL vbBold)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_redactionAppearance.m_lgFont.lfWeight = vbBold == VARIANT_TRUE ? FW_BOLD : FW_NORMAL;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28295")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_IsItalic(VARIANT_BOOL* pvbItalic)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28296", pvbItalic != __nullptr);
		
		// Check license state
		validateLicense();

		// Return setting to caller
		*pvbItalic = asVariantBool(m_redactionAppearance.m_lgFont.lfItalic == gucIS_ITALIC);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28297")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_IsItalic(VARIANT_BOOL vbItalic)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_redactionAppearance.m_lgFont.lfItalic = vbItalic == VARIANT_TRUE ? gucIS_ITALIC : 0;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28298")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::get_FontSize(long* plFontSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28299", plFontSize != __nullptr);

		// Check license state
		validateLicense();

		// Return setting to caller
		*plFontSize = m_redactionAppearance.m_iPointSize;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28300")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::put_FontSize(long lFontSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_redactionAppearance.m_iPointSize = lFontSize;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28301")
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::GetClassID(CLSID* pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_RedactFileProcessor;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Version 3: 
//   Added m_lCreateIfRedact
// Version 5:
//   Added m_bCarryForwardAnnotations and m_bApplyRedactionsAsAnnotations
// Version 6:
//   Added m_bAlwaysContinueProcessing
// Version 7:
//   Removed m_bAlwaysContinueProcessing
// Version 8:
//   Added m_bUseRedactedImage and m_redactionAppearance
STDMETHODIMP CRedactFileProcessor::Load(IStream* pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset member variables
		clear();

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
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI28302", 
				"Unable to load newer Redact File Processor!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		dataReader >> m_strRuleFileName;
		dataReader >> m_strOutputFileName;
		dataReader >> m_bReadFromUSS;

		if (nDataVersion >= 3)
		{
			dataReader >> m_lCreateIfRedact;
		}
		if (nDataVersion >= 4)
		{
			dataReader >> m_bUseVOA;
			dataReader >> m_strVOAFileName;
		}

		// Load Annotation settings
		if (nDataVersion >= 5)
		{
			// Pre-existing annotations will be saved in the output file
			dataReader >> m_bCarryForwardAnnotations;
			// Redactions will be saved as annotations in the output file
			dataReader >> m_bApplyRedactionsAsAnnotations;
		}

		// Ignore setting for Always Continue Processing
		bool bTemp = true;
		if (nDataVersion == 6)
		{
			dataReader >> bTemp;
		}

		// Provide warning message to user if loaded setting expects to 
		// continue processing tasks only if a redaction is found (P16 #2676)
		if (!bTemp)
		{
			string strText = "You have opened an FPS file that uses a setting that "
				"is no longer supported.  The Redact images (no verification) task "
				"no longer supports the \"Continue to the next task only if the "
				"image contains redactions\" feature.\r\n\r\nIn ID Shield 6.0 similar "
				"behavior can be obtained by using conditional tasks.  Please review "
				"the \"Upgrading to ID Shield 6.0\" section of the product documentation "
				"for more details.";
			MessageBox(NULL, strText.c_str(), "Warning", MB_OK | MB_ICONWARNING);
		}

		if (nDataVersion >= 2)
		{
			// if true there is an Attribute Names vector to load otherwise there is not
			bool bAttributeNames;
			dataReader >> bAttributeNames;
			if (bAttributeNames)
			{
				IPersistStreamPtr ipObj;
				readObjectFromStream(ipObj, pStream, "ELI28303");
				m_ipAttributeNames = ipObj;
				fillAttributeSet(m_ipAttributeNames, m_setAttributeNames);
			}
			else
			{
				m_ipAttributeNames = __nullptr;
			}
		}

		if (nDataVersion >= 8)
		{
			// Legislation guard
			dataReader >> m_bUseRedactedImage;

			// Redaction appearance
			dataReader >> m_redactionAppearance.m_strText;
			dataReader >> m_redactionAppearance.m_crBorderColor;
			dataReader >> m_redactionAppearance.m_crFillColor;

			string strFontName;
			dataReader >> strFontName;
			lstrcpyn(m_redactionAppearance.m_lgFont.lfFaceName, strFontName.c_str(), LF_FACESIZE);
			
			bool bItalic;
			dataReader >> bItalic;
			m_redactionAppearance.m_lgFont.lfItalic = bItalic ? gucIS_ITALIC : 0;

			bool bBold;
			dataReader >> bBold;
			m_redactionAppearance.m_lgFont.lfWeight = bBold ? FW_BOLD : FW_NORMAL;

			long lTemp;
			dataReader >> lTemp;
			m_redactionAppearance.m_iPointSize = (int) lTemp;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28304");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::Save(IStream* pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;

		dataWriter << m_strRuleFileName;
		dataWriter << m_strOutputFileName;
		dataWriter << m_bReadFromUSS;
		dataWriter << m_lCreateIfRedact;
		dataWriter << m_bUseVOA;
		dataWriter << m_strVOAFileName;

		// Save annotation settings
		dataWriter << m_bCarryForwardAnnotations;
		dataWriter << m_bApplyRedactionsAsAnnotations;

		// Save flag indicating AttributeNames stored in stream
		bool bAttributeNames = (m_ipAttributeNames != __nullptr);
		dataWriter << bAttributeNames;

		// Legislation guard
		dataWriter << m_bUseRedactedImage;

		// Redaction text
		dataWriter << m_redactionAppearance.m_strText;

		// Save redaction color options
		dataWriter << m_redactionAppearance.m_crBorderColor;
		dataWriter << m_redactionAppearance.m_crFillColor;

		// Save font options
		dataWriter << string(m_redactionAppearance.m_lgFont.lfFaceName);
		dataWriter << (m_redactionAppearance.m_lgFont.lfItalic == gucIS_ITALIC);
		dataWriter << asCppBool(m_redactionAppearance.m_lgFont.lfWeight >= FW_BOLD);
		dataWriter << (long) m_redactionAppearance.m_iPointSize;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		if (bAttributeNames)
		{
			// Only load Attribute Names if they exist
			IPersistStreamPtr ipObj = m_ipAttributeNames;
			writeObjectToStream(ipObj, pStream, "ELI28305", fClearDirty);
		}
		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28306");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactFileProcessor::GetSizeMax(ULARGE_INTEGER* pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CRedactFileProcessor::clear()
{
	m_strRuleFileName = "";
	m_strOutputFileName = gstrDEFAULT_REDACTED_IMAGE_FILENAME;
	m_bReadFromUSS = true;
	m_bUseVOA = false;
	m_strVOAFileName = "";
	m_bCarryForwardAnnotations = false;
	m_bApplyRedactionsAsAnnotations = false;

	// Create output file only if redactable data was found
	m_lCreateIfRedact = 1;

	// Use the original image
	m_bUseRedactedImage = false;
	
	// Reset to default values
	m_redactionAppearance.reset();
}
//-------------------------------------------------------------------------------------------------
void CRedactFileProcessor::validateLicense()
{
	VALIDATE_LICENSE(gnIDSHIELD_AUTOREDACTION_OBJECT, "ELI28307", "Legacy Redaction File Processor");
}
//-------------------------------------------------------------------------------------------------
void CRedactFileProcessor::fillAttributeSet(IVariantVectorPtr ipAttributeNames, set<string>& rsetAttributeNames)
{
	long nSize = ipAttributeNames->Size;
	for (long n = 0; n < nSize; n++)
	{
		rsetAttributeNames.insert(asString(ipAttributeNames->Item[n].bstrVal));
	}
}
//-------------------------------------------------------------------------------------------------
