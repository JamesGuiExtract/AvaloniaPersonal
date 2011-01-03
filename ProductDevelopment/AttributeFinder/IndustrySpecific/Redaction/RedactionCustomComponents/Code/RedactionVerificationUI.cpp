// RedactionVerificationUI.cpp : Implementation of CRedactionVerificationUI
#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "RedactionVerificationUI.h"
#include "RedactionCCConstants.h"

// For registry path information
#include <Common.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 4;

//-------------------------------------------------------------------------------------------------
// CRedactionVerificationUI
//-------------------------------------------------------------------------------------------------
CRedactionVerificationUI::CRedactionVerificationUI()
: m_bDirty(false)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI11275")
}
//-------------------------------------------------------------------------------------------------
CRedactionVerificationUI::~CRedactionVerificationUI()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI11156")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRedactionVerificationUI,
		&IID_IFileProcessingTask,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
		&IID_IAccessRequired
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IRedactionVerificationUI
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::ShowUI(BSTR strFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11158");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_ReviewAllPages(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		*pVal = m_UISettings.getReviewAllPages() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13205");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_ReviewAllPages(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		m_UISettings.setReviewAllPages( newVal == VARIANT_TRUE );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13206");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_AlwaysOutputImage(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		*pVal = m_UISettings.getAlwaysOutputImage() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13215");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_AlwaysOutputImage(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		m_UISettings.setAlwaysOutputImage( newVal == VARIANT_TRUE );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13216");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_OutputImageName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		*pVal = _bstr_t( m_UISettings.getOutputImageName().c_str() ).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13217");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_OutputImageName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		string strFileName = asString(newVal);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI24871", ipFAMTagManager != NULL);

		// Make sure the file name contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFileName.c_str()) == VARIANT_TRUE)
		{
			UCLIDException ue("ELI15035", "The output file name contains invalid tags!");
			ue.addDebugInfo("Output file", strFileName);
			throw ue;
		}

		// Set the output image name
		m_UISettings.setOutputImageName(strFileName);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13218");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_AlwaysOutputMeta(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		*pVal = m_UISettings.getAlwaysOutputMeta() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13219");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_AlwaysOutputMeta(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		m_UISettings.setAlwaysOutputMeta( newVal == VARIANT_TRUE );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13220");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_MetaOutputName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		*pVal = _bstr_t( m_UISettings.getMetaOutputName().c_str() ).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13221");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_MetaOutputName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		string strFileName = asString(newVal);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI15036", ipFAMTagManager != NULL);

		// Make sure the file name contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFileName.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI15037", "The metadata output file name contains invalid tags!");
			ue.addDebugInfo("Metadata file", strFileName);
			throw ue;
		}

		// Set the meta output file name
		m_UISettings.setMetaOutputName( strFileName);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13222");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_CarryForwardAnnotations(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Return setting to caller
		*pVal = m_UISettings.getCarryForwardAnnotations() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14593");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_CarryForwardAnnotations(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Check license state
		validateLicense();

		// Save new setting
		m_UISettings.setCarryForwardAnnotations( newVal == VARIANT_TRUE );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14594");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_ApplyRedactionsAsAnnotations(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Return setting to caller
		*pVal = m_UISettings.getApplyRedactionsAsAnnotations() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14595");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_ApplyRedactionsAsAnnotations(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Check license state
		validateLicense();

		// Save new setting
		m_UISettings.setApplyRedactionsAsAnnotations( newVal == VARIANT_TRUE );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14596");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_CollectFeedback(VARIANT_BOOL* pvbCollectFeedback)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// ensure the argument is non-NULL
		ASSERT_ARGUMENT("ELI20026", pvbCollectFeedback != NULL);

		// Return setting to caller
		*pvbCollectFeedback = asVariantBool( m_UISettings.getCollectFeedback() );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20038");

	return S_OK;

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_CollectFeedback(VARIANT_BOOL vbCollectFeedback)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Check license state
		validateLicense();

		// Save new setting
		m_UISettings.setCollectFeedback( asCppBool(vbCollectFeedback) );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20039");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_FeedbackCollectOption(
	EFeedbackCollectOption *peFeedbackCollectOption)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI20040", peFeedbackCollectOption != NULL);

		// get the output version
		*peFeedbackCollectOption = 
			(EFeedbackCollectOption) m_UISettings.getFeedbackCollectOption();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20041");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_FeedbackCollectOption(
	EFeedbackCollectOption eFeedbackCollectOption)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_UISettings.setFeedbackCollectOption(
			(UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption) eFeedbackCollectOption);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20042");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_FeedbackDataFolder(BSTR *pbstrFeedbackDataFolder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI20043", pbstrFeedbackDataFolder != NULL);
		
		// get the output version
		*pbstrFeedbackDataFolder = get_bstr_t( m_UISettings.getFeedbackDataFolder() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20044");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_FeedbackDataFolder(BSTR bstrFeedbackDataFolder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_UISettings.setFeedbackDataFolder( asString(bstrFeedbackDataFolder) );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20045");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_CollectFeedbackImage(
	VARIANT_BOOL *pvbCollectFeedbackImage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI20051", pvbCollectFeedbackImage != NULL);

		// get the output version
		*pvbCollectFeedbackImage = asVariantBool( m_UISettings.getCollectFeedbackImage() );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20046");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_CollectFeedbackImage(
	VARIANT_BOOL vbCollectFeedbackImage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_UISettings.setCollectFeedbackImage( asCppBool(vbCollectFeedbackImage) );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20047");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_FeedbackOriginalFilenames(
	VARIANT_BOOL *pvbFeedbackOriginalFilenames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI20048", pvbFeedbackOriginalFilenames != NULL);

		// get the output version
		*pvbFeedbackOriginalFilenames = 
			asVariantBool( m_UISettings.getFeedbackOriginalFilenames() );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20049");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_FeedbackOriginalFilenames(
	VARIANT_BOOL vbFeedbackOriginalFilenames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_UISettings.setFeedbackOriginalFilenames( asCppBool(vbFeedbackOriginalFilenames) );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20050");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_RequireRedactionTypes(
	VARIANT_BOOL* pvbRequireRedactionTypes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20266", pvbRequireRedactionTypes != NULL);

		// Check licensing
		validateLicense();

		*pvbRequireRedactionTypes = asVariantBool(m_UISettings.getRequireRedactionTypes());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20264");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_RequireRedactionTypes(
	VARIANT_BOOL vbRequireRedactionTypes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_UISettings.setRequireRedactionTypes(asCppBool(vbRequireRedactionTypes));
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20265");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_InputDataFile(BSTR *pbstrInputDataFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24649", pbstrInputDataFile != NULL);

		// Check license
		validateLicense();

		*pbstrInputDataFile = _bstr_t(m_UISettings.getInputDataFile().c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24650")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_InputDataFile(BSTR bstrInputDataFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_UISettings.setInputDataFile( asString(bstrInputDataFile) );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24651")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_InputRedactedImage(VARIANT_BOOL *pvbInputRedactedImage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24652", pvbInputRedactedImage != NULL);

		// Check license
		validateLicense();

		*pvbInputRedactedImage = asVariantBool(m_UISettings.getInputRedactedImage());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24653")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_InputRedactedImage(VARIANT_BOOL vbInputRedactedImage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_UISettings.setInputRedactedImage(vbInputRedactedImage == VARIANT_TRUE);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24654")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_RequireExemptionCodes(VARIANT_BOOL *pvbRequireExemptionCodes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24655", pvbRequireExemptionCodes != NULL);

		// Check license
		validateLicense();

		*pvbRequireExemptionCodes = asVariantBool(m_UISettings.getRequireExemptionCodes());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24656")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_RequireExemptionCodes(VARIANT_BOOL vbRequireExemptionCodes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_UISettings.setRequireExemptionCodes(vbRequireExemptionCodes == VARIANT_TRUE);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24657")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_RedactionText(BSTR *pbstrRedactionText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24658", pbstrRedactionText != NULL);

		// Check license
		validateLicense();

		*pbstrRedactionText = _bstr_t(m_UISettings.getRedactionText().c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24659")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_RedactionText(BSTR bstrRedactionText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_UISettings.setRedactionText( asString(bstrRedactionText) );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24660")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_BorderColor(long *plBorderColor)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24661", plBorderColor != NULL);

		// Check license
		validateLicense();

		*plBorderColor = m_UISettings.getBorderColor();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24662")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_BorderColor(long lBorderColor)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_UISettings.setBorderColor(lBorderColor);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24663")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_FillColor(long *plFillColor)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24664", plFillColor != NULL);

		// Check license
		validateLicense();

		*plFillColor = m_UISettings.getFillColor();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24665")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_FillColor(long lFillColor)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_UISettings.setFillColor(lFillColor);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24666")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_FontName(BSTR *pbstrFontName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24667", pbstrFontName != NULL);

		// Check license
		validateLicense();

		*pbstrFontName = _bstr_t(m_UISettings.getFont().lfFaceName).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24668")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_FontName(BSTR bstrFontName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// Get the current font
		LOGFONT lgFont = m_UISettings.getFont();

		// Set the new font name
		lstrcpyn(lgFont.lfFaceName, asString(bstrFontName).c_str(), LF_FACESIZE);

		// Store the new font
		m_UISettings.setFont(lgFont);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24669")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_IsBold(VARIANT_BOOL *pvbBold)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24670", pvbBold != NULL);

		// Check license
		validateLicense();

		*pvbBold = asVariantBool(m_UISettings.getFont().lfWeight >= FW_BOLD);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24671")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_IsBold(VARIANT_BOOL vbBold)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// Get the current font
		LOGFONT lgFont = m_UISettings.getFont();

		// Set the new font weight
		lgFont.lfWeight = vbBold == VARIANT_TRUE ? FW_BOLD : FW_NORMAL;

		// Store the new font
		m_UISettings.setFont(lgFont);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24672")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_IsItalic(VARIANT_BOOL *pvbItalic)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24673", pvbItalic != NULL);

		// Check license
		validateLicense();

		*pvbItalic = asVariantBool(m_UISettings.getFont().lfItalic == gucIS_ITALIC);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24674")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_IsItalic(VARIANT_BOOL vbItalic)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// Get the current font
		LOGFONT lgFont = m_UISettings.getFont();

		// Set the new font weight
		lgFont.lfItalic = vbItalic == VARIANT_TRUE ? gucIS_ITALIC : 0;

		// Store the new font
		m_UISettings.setFont(lgFont);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24675")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::get_FontSize(long *plFontSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI24676", plFontSize != NULL);

		// Check license
		validateLicense();

		*plFontSize = m_UISettings.getFontSize();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24677")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::put_FontSize(long lFontSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_UISettings.setFontSize((int) lFontSize);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24678")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT( "ELI18329", pbValue != NULL );

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18328");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IFileProcessingTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		throw UCLIDException("ELI28223", 
			"Legacy verification task no longer supported. Use verify image task instead.");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11240")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::raw_ProcessFile(BSTR bstrFileFullName, long nFileID,
	long nActionID, IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		throw UCLIDException("ELI28224", 
			"Legacy verification task no longer supported. Use verify image task instead.");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11243")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11403")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11239")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31201", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31202");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pstrComponentDescription = _bstr_t("Redaction: Verify and redact image (legacy)").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12865");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy( CLSID_RedactionVerificationUI );
		ASSERT_RESOURCE_ALLOCATION( "ELI11285", ipObjCopy != NULL );

		IUnknownPtr ipUnk(this);
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11286");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IRedactionVerificationUIPtr ipSource( pObject );
		ASSERT_RESOURCE_ALLOCATION( "ELI11287", ipSource != NULL );

		m_UISettings.setReviewAllPages( ipSource->ReviewAllPages == VARIANT_TRUE );

		// Retrieve the require redaction types and exemption codes settings
		m_UISettings.setRequireRedactionTypes( asCppBool(ipSource->RequireRedactionTypes) );
		m_UISettings.setRequireExemptionCodes( asCppBool(ipSource->RequireExemptionCodes) );

		// Retrieve input image settings
		m_UISettings.setInputDataFile( asString(ipSource->InputDataFile) );

		// Retrieve image output and metadata settings
		m_UISettings.setAlwaysOutputImage( ipSource->AlwaysOutputImage == VARIANT_TRUE );
		m_UISettings.setOutputImageName( asString(ipSource->OutputImageName));
		m_UISettings.setInputRedactedImage( asCppBool(ipSource->InputRedactedImage) );
		m_UISettings.setAlwaysOutputMeta( ipSource->AlwaysOutputMeta == VARIANT_TRUE );
		m_UISettings.setMetaOutputName( asString(ipSource->MetaOutputName ));

		// Retrieve annotation settings
		m_UISettings.setCarryForwardAnnotations( 
			ipSource->CarryForwardAnnotations == VARIANT_TRUE );
		m_UISettings.setApplyRedactionsAsAnnotations( 
			ipSource->ApplyRedactionsAsAnnotations == VARIANT_TRUE );

		// Retrieve feedback collection settings
		m_UISettings.setCollectFeedback( asCppBool(ipSource->CollectFeedback) );
		m_UISettings.setFeedbackCollectOption(ipSource->FeedbackCollectOption);
		m_UISettings.setFeedbackDataFolder( asString(ipSource->FeedbackDataFolder) );
		m_UISettings.setCollectFeedbackImage( asCppBool(ipSource->CollectFeedbackImage) );
		m_UISettings.setFeedbackOriginalFilenames( asCppBool(ipSource->FeedbackOriginalFilenames) );

		// Retrieve redaction appearance settings
		m_UISettings.setRedactionText( asString(ipSource->RedactionText) );
		m_UISettings.setBorderColor(ipSource->BorderColor);
		m_UISettings.setFillColor(ipSource->FillColor);
		
		// Retrieve font settings
		LOGFONT lgFont = {0};
		lstrcpyn(lgFont.lfFaceName, ipSource->FontName, LF_FACESIZE);
		lgFont.lfItalic = ipSource->IsItalic == VARIANT_TRUE ? gucIS_ITALIC : 0;
		lgFont.lfWeight = ipSource->IsBold == VARIANT_TRUE ? FW_BOLD : FW_NORMAL;
		m_UISettings.setFont(lgFont);
		m_UISettings.setFontSize(ipSource->FontSize);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11288");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_RedactionVerificationUI;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

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
			UCLIDException ue( "ELI11283", 
				"Unable to load newer Redaction Verification UI object!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion == 2)
		{
			// not used any more
			string strINIFileName = "";
			dataReader >> strINIFileName;
		}

		// Load collected UI settings
		if ( nDataVersion >= 3 )
		{
			m_UISettings.Load(pStream);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11281");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );

		dataWriter << gnCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		m_UISettings.Save( pStream );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11282");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionVerificationUI::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CRedactionVerificationUI::validateLicense()
{
	static const unsigned long THIS_ID = gnIDSHIELD_VERIFICATION_OBJECT;

	VALIDATE_LICENSE( THIS_ID, "ELI11157", "Redaction Verification UI" );
}
//-------------------------------------------------------------------------------------------------
