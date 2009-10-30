// RedactionTask.cpp : Implementation of CRedactionTask
#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "RedactionTask.h"
#include "RedactionCCUtils.h"
#include "RedactionCCConstants.h"
#include "IDShieldData.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <StopWatch.h>
#include <MiscLeadUtils.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CRedactionTask
//-------------------------------------------------------------------------------------------------
CRedactionTask::CRedactionTask()
:	m_ipAFUtility(NULL),
	m_ipAttributeNames(CLSID_VariantVector),
	m_bDirty(false),
	m_bCarryForwardAnnotations(false),
	m_bApplyRedactionsAsAnnotations(false),
	m_ipIDShieldDB(NULL),
	m_bUseRedactedImage(false)
{
	ASSERT_RESOURCE_ALLOCATION("ELI19993", m_ipAttributeNames != NULL);

	// set members to their inital states
	clear();

	// Add the default selected Attributes to the collection (P16 #2751)
	m_ipAttributeNames->PushBack("HCData");
	m_ipAttributeNames->PushBack("MCData");
	m_ipAttributeNames->PushBack("LCData");
	m_ipAttributeNames->PushBack("Manual");
}
//-------------------------------------------------------------------------------------------------
CRedactionTask::~CRedactionTask()
{
	try
	{
		m_ipAFUtility = NULL;
		m_ipAttributeNames = NULL;
		m_ipIDShieldDB = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16479");
}
//-------------------------------------------------------------------------------------------------
HRESULT CRedactionTask::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CRedactionTask::FinalRelease()
{
	try
	{
		m_ipAFUtility = NULL;
		m_ipAttributeNames = NULL;
		m_ipIDShieldDB = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28331");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRedactionTask,
		&IID_IFileProcessingTask,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_ILicensedComponent
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IFileProcessingTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_Init()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17789");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_ProcessFile(BSTR bstrFileFullName, long nFileID, long nActionID,
	IFAMTagManager* pTagManager, IFileProcessingDB* pDB, IProgressStatus* pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult* pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	INIT_EXCEPTION_AND_TRACING("MLI00529");

	try
	{
		// Check license
		validateLicense();
		_lastCodePos = "10";

		// Start the a stop watch to track processing time
		StopWatch swProcessingTime;
		swProcessingTime.start();
		_lastCodePos = "20";

		IFileProcessingDBPtr ipFAMDB(pDB);
		ASSERT_ARGUMENT("ELI19080", ipFAMDB != NULL);
		ASSERT_ARGUMENT("ELI17928", bstrFileFullName != NULL);
		ASSERT_ARGUMENT("ELI17929", pResult != NULL);

		// Create an smart FAM Tag Pointer
		IFAMTagManagerPtr ipFAMTagManager = pTagManager;
		ASSERT_RESOURCE_ALLOCATION("ELI15012", ipFAMTagManager != NULL);
		_lastCodePos = "30";

		// input file for processing
		string strInputFile = asString(bstrFileFullName);
		ASSERT_ARGUMENT("ELI17930", !strInputFile.empty());
		_lastCodePos = "40";

		// Default to successful completion
		*pResult = kProcessingSuccessful;
		_lastCodePos = "50";

		// check if file was uss and if so get image name by removing the .uss
		string strExt = getExtensionFromFullPath(strInputFile, true);
		string strImageName = strInputFile;
		if (strExt == ".uss")
		{
			_lastCodePos = "60";
			strImageName = getFileNameWithoutExtension(strInputFile, false);
			strImageName = getAbsoluteFileName(strInputFile, strImageName);
		}
		else if (strExt == ".voa")
		{
			_lastCodePos = "70";
			strImageName = getFileNameWithoutExtension(strInputFile, false);
			strImageName = getAbsoluteFileName(strInputFile, strImageName);
		}
		_lastCodePos = "100";

		// Expand tags and text functions to get the output name
		string strOutputName = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(
			ipFAMTagManager, m_strOutputFileName, strImageName);

		// Set the image to redact to the input image name
		string strImageToRedact = strImageName;

		// Use redacted image as backdrop if option is specified
		if (m_bUseRedactedImage && isFileOrFolderValid(strOutputName))
		{
			strImageToRedact = strOutputName;
		}

		// Validate input image exists
		::validateFileOrFolderExistence(strImageName);

		// Create AFDocument
		UCLID_AFCORELib::IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI09172", ipAFDoc != NULL);

		// Get Text element for AFDoc
		ISpatialStringPtr ipText = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI14967", ipText != NULL);

		// Set the AFDoc SourceDocName property
		ipText->SourceDocName = strImageName.c_str();

		_lastCodePos = "110";
		// Create Found attributes vector
		IIUnknownVectorPtr ipFoundAttr (CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI08997", ipFoundAttr != NULL);

		// Initialize the idShield data class
		IDShieldData idsData;

		_lastCodePos = "120";
		IIUnknownVectorPtr ipVOAAttr(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI12718", ipVOAAttr != NULL);

		// Expand tags and text functions to get the VOA file name
		string strVOAFileName = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(
			ipFAMTagManager, m_strVOAFileName, strImageName);
		_lastCodePos = "130";

		// Throw exception if VOA file doesn't exist
		if (!isValidFile(strVOAFileName))
		{
			UCLIDException ue("ELI28237", "VOA file not found.");
			ue.addDebugInfo("File name", strVOAFileName);
			throw ue;
		}
		_lastCodePos = "140";

		// Load voa
		ipVOAAttr->LoadFrom(strVOAFileName.c_str(), VARIANT_FALSE);

		// Calculate the counts from the loaded voa file
		idsData.calculateFromVector(ipVOAAttr, m_setAttributeNames);
		_lastCodePos = "150";

		// check to see if all of the loaded attributes will be used
		if (m_ipAttributeNames != NULL)
		{
			_lastCodePos = "160";
			long nNumAttr = m_ipAttributeNames->Size;
			string strQuery = "";
			for (int i = 0; i < nNumAttr; i++)
			{
				_lastCodePos = "170-" + asString(i);

				if (i > 0)
				{
					strQuery += "|";
				}
				string strCurrName = asString(_bstr_t(m_ipAttributeNames->GetItem(i)));
				strQuery += strCurrName;
			}
			_lastCodePos = "180";
			ipFoundAttr = getAFUtility()->QueryAttributes(ipVOAAttr, 
				strQuery.c_str(),VARIANT_FALSE);
		}
		else
		{
			_lastCodePos = "190";
			ipFoundAttr = ipVOAAttr;
		}


		_lastCodePos = "200";

		// Get the text color
		COLORREF crTextColor = invertColor(m_redactionAppearance.m_crFillColor);

		// Process Results
		// Create vector for zones to redact
		vector<PageRasterZone> vecZones;
		long nNumAttr = ipFoundAttr->Size();
		for (int i = 0; i < nNumAttr; i++)
		{
			_lastCodePos = "250-" + asString(i);
			IAttributePtr ipAttr = ipFoundAttr->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI09001", ipAttr != NULL);

			// Get the found value
			ISpatialStringPtr ipValue = ipAttr->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI09002", ipValue != NULL);
			_lastCodePos = "260";

			// Only cover area if value is spatial
			if (ipValue->HasSpatialInfo() == VARIANT_TRUE)
			{
				// Get the text associated with this attribute
				string strText = CRedactionCustomComponentsUtils::ExpandRedactionTags(
					m_redactionAppearance.m_strText, "", asString(ipAttr->Type));
				_lastCodePos = "270";

				// Get the Raster zones to redact
				IIUnknownVectorPtr ipRasterZones = ipValue->GetOriginalImageRasterZones();
				ASSERT_RESOURCE_ALLOCATION("ELI09180", ipRasterZones != NULL);
				_lastCodePos = "280";

				// Add to the vector of zones to redact
				long lZoneCount = ipRasterZones->Size();
				for (long j = 0; j < lZoneCount; j++)
				{
					// Bring raster zone
					IRasterZonePtr ipRasterZone = ipRasterZones->At(j);
					ASSERT_RESOURCE_ALLOCATION("ELI24862", ipRasterZone != NULL);

					// Construct the page raster zone
					PageRasterZone zone;
					zone.m_crBorderColor = m_redactionAppearance.m_crBorderColor;
					zone.m_crFillColor = m_redactionAppearance.m_crFillColor;
					zone.m_crTextColor = crTextColor;
					zone.m_font = m_redactionAppearance.m_lgFont;
					zone.m_iPointSize = m_redactionAppearance.m_iPointSize;
					zone.m_strText = strText;
					ipRasterZone->GetData(&(zone.m_nStartX), &(zone.m_nStartY), &(zone.m_nEndX),
						&(zone.m_nEndY), &(zone.m_nHeight), &(zone.m_nPage));

					// Add to the vector of zones to redact
					vecZones.push_back(zone);
				}
			}
			_lastCodePos = "290";
		}
		_lastCodePos = "300";

		// modify AFDocument's text's source document name to be image
		// name if the input source document is a USS file
		if (strExt == ".uss")
		{
			_lastCodePos = "310";

			// Retrieve text from AFDocument
			ISpatialStringPtr ipText = ipAFDoc->Text;
			ASSERT_RESOURCE_ALLOCATION("ELI15635", ipText != NULL);

			// Updated the source name to the image name
			ipText->SourceDocName = strImageName.c_str();
		}
		_lastCodePos = "320";

		// check to see if output path exists, if not try to create
		string strOutputPath = getDirectoryFromFullPath(strOutputName, false);
		if (!::isFileOrFolderValid(strOutputPath))
		{
			_lastCodePos = "340";

			// Create OutputPath
			createDirectory(strOutputPath);
		}
		_lastCodePos = "350";

		// Save redactions
		fillImageArea(strImageToRedact.c_str(), strOutputName.c_str(), vecZones, 
			m_bCarryForwardAnnotations, m_bApplyRedactionsAsAnnotations);
		_lastCodePos = "400";

		// Set the FAMDB pointer
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr ipIDSDB = getIDShieldDBPtr();
		ipIDSDB->FAMDB = ipFAMDB;

		// Stop the stop watch
		swProcessingTime.stop();

		// Add the IDShieldData record to the database
		ipIDSDB->AddIDShieldData(nFileID, VARIANT_FALSE, swProcessingTime.getElapsedTime(), 
			idsData.m_lNumHCDataFound, idsData.m_lNumMCDataFound, idsData.m_lNumLCDataFound, 
			idsData.m_lNumCluesFound, idsData.m_lTotalRedactions, idsData.m_lTotalManualRedactions);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09881")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17787");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17788");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI19820", pbValue != NULL);

		try
		{
			// Check license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch (...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19821");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_GetComponentDescription(BSTR* pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28222", pstrComponentDescription != NULL);

		*pstrComponentDescription = _bstr_t("Redaction: Create redacted image").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12813");
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_Clone(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI28321", pObject != NULL);

		// validate license first
		validateLicense();

		// create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_RedactionTask);
		ASSERT_RESOURCE_ALLOCATION("ELI09864", ipObjCopy != NULL);

		IUnknownPtr ipUnk(this);
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09865");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_CopyFrom(IUnknown* pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IRedactionTaskPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI09876", ipSource != NULL);

		m_strOutputFileName = asString(ipSource->OutputFileName);

		// Clear the Attributes set
		m_setAttributeNames.clear();

		if (ipSource->AttributeNames != NULL)
		{
			m_ipAttributeNames = ipSource->AttributeNames;
			fillAttributeSet(m_ipAttributeNames, m_setAttributeNames);
		}
		else
		{
			m_ipAttributeNames = NULL;
		}

		// Retrieve VOA file name
		m_strVOAFileName = asString(ipSource->VOAFileName);

		// Retrieve annotation settings
		m_bCarryForwardAnnotations = asCppBool(ipSource->CarryForwardAnnotations);
		m_bApplyRedactionsAsAnnotations = asCppBool(ipSource->ApplyRedactionsAsAnnotations);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09866");
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::raw_IsConfigured(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI28322", pbValue != NULL);

		// Check license
		validateLicense();

		*pbValue = asVariantBool(!m_strOutputFileName.empty());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09005");
}

//-------------------------------------------------------------------------------------------------
// IRedactionTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_OutputFileName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI28324", pVal != NULL);

		validateLicense();

		*pVal = _bstr_t(m_strOutputFileName.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09871");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_OutputFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();
			
		string strFileName = asString(newVal);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI15030", ipFAMTagManager != NULL);

		// Make sure the file name contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFileName.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI15031", "The output file name contains invalid tags.");
			ue.addDebugInfo("Output file", strFileName);
			throw ue;
		}

		// Assign the output file name
		m_strOutputFileName = strFileName;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09872");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_AttributeNames(IVariantVector** ppVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI26085", ppVal != NULL);

		validateLicense();
		
		*ppVal = NULL;
		if (m_ipAttributeNames != NULL)
		{
			// Get a ShallowCopyableObject ptr for the current name list
			IShallowCopyablePtr ipObjSource = m_ipAttributeNames;
			ASSERT_RESOURCE_ALLOCATION("ELI15339", ipObjSource != NULL);

			// Shallow copy the attribute names
			IVariantVectorPtr ipObjCloned = ipObjSource->ShallowCopy();
			ASSERT_RESOURCE_ALLOCATION("ELI15340", ipObjCloned != NULL);

			// set the return value to the shallow copied object
			*ppVal = ipObjCloned.Detach();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11783");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_AttributeNames(IVariantVector *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();
		
		m_ipAttributeNames = pVal;
		
		m_setAttributeNames.clear();
		if (m_ipAttributeNames != NULL)
		{
			fillAttributeSet(m_ipAttributeNames, m_setAttributeNames);
		}
		
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11784");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_VOAFileName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28326", pVal != NULL);

		validateLicense();

		*pVal = _bstr_t(m_strVOAFileName.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12717");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_VOAFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
			
		string strFileName = asString(newVal);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI15032", ipFAMTagManager != NULL);

		// Make sure the file name contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFileName.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI15033", "The VOA file name contains invalid tags.");
			ue.addDebugInfo("VOA file", strFileName);
			throw ue;
		}

		// Assign the voa file name
		m_strVOAFileName = strFileName;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12716");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_CarryForwardAnnotations(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28327", pVal != NULL);

		// Check license state
		validateLicense();

		// Return setting to caller
		*pVal = asVariantBool(m_bCarryForwardAnnotations);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14597");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_CarryForwardAnnotations(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Check license state
		validateLicense();

		// Save new setting
		m_bCarryForwardAnnotations = asCppBool(newVal);
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14598");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_ApplyRedactionsAsAnnotations(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28328", pVal != NULL);

		// Check license state
		validateLicense();

		// Return setting to caller
		*pVal = asVariantBool(m_bApplyRedactionsAsAnnotations);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14599");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_ApplyRedactionsAsAnnotations(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Check license state
		validateLicense();

		// Save new setting
		m_bApplyRedactionsAsAnnotations = asCppBool(newVal);
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14600");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_UseRedactedImage(VARIANT_BOOL* pvbUseRedactedImage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24751", pvbUseRedactedImage != NULL);

		// Check license state
		validateLicense();

		// Return setting to caller
		*pvbUseRedactedImage = asVariantBool(m_bUseRedactedImage);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24720")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_UseRedactedImage(VARIANT_BOOL vbUseRedactedImage)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24721")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_RedactionText(BSTR* pbstrRedactionText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24752", pbstrRedactionText != NULL);

		// Check license state
		validateLicense();

		// Return setting to caller
		*pbstrRedactionText = _bstr_t(m_redactionAppearance.m_strText.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24722")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_RedactionText(BSTR bstrRedactionText)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24723")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_BorderColor(long* plBorderColor)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24753", plBorderColor != NULL);

		// Check license state
		validateLicense();

		// Return setting to caller
		*plBorderColor = m_redactionAppearance.m_crBorderColor;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24724")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_BorderColor(long lBorderColor)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24725")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_FillColor(long* plFillColor)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24754", plFillColor != NULL);

		// Check license state
		validateLicense();

		// Return setting to caller
		*plFillColor = m_redactionAppearance.m_crFillColor;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24726")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_FillColor(long lFillColor)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24727")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_FontName(BSTR* pbstrFontName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24755", pbstrFontName != NULL);

		// Check license state
		validateLicense();

		// Return setting to caller
		*pbstrFontName = _bstr_t(m_redactionAppearance.m_lgFont.lfFaceName).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24728")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_FontName(BSTR bstrFontName)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24729")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_IsBold(VARIANT_BOOL* pvbBold)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24867", pvbBold != NULL);

		// Check license state
		validateLicense();

		// Return setting to caller
		*pvbBold = asVariantBool(m_redactionAppearance.m_lgFont.lfWeight >= FW_BOLD);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24730")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_IsBold(VARIANT_BOOL vbBold)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24731")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_IsItalic(VARIANT_BOOL* pvbItalic)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24868", pvbItalic != NULL);
		
		// Check license state
		validateLicense();

		// Return setting to caller
		*pvbItalic = asVariantBool(m_redactionAppearance.m_lgFont.lfItalic == gucIS_ITALIC);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24732")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_IsItalic(VARIANT_BOOL vbItalic)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24733")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::get_FontSize(long* plFontSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24869", plFontSize != NULL);

		// Check license state
		validateLicense();

		// Return setting to caller
		*plFontSize = m_redactionAppearance.m_iPointSize;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24734")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::put_FontSize(long lFontSize)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24735")
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::GetClassID(CLSID* pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_RedactionTask;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::Load(IStream* pStream)
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
			UCLIDException ue("ELI11002", 
				"Unable to load newer redaction task.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Input/output files
		dataReader >> m_strOutputFileName;
		dataReader >> m_strVOAFileName;

		// Annotation settings
		dataReader >> m_bCarryForwardAnnotations;
		dataReader >> m_bApplyRedactionsAsAnnotations;

		// Attribute names
		bool bAttributeNames;
		dataReader >> bAttributeNames;
		if (bAttributeNames)
		{
			IPersistStreamPtr ipObj;
			readObjectFromStream(ipObj, pStream, "ELI11779");
			m_ipAttributeNames = ipObj;
			fillAttributeSet(m_ipAttributeNames, m_setAttributeNames);
		}
		else
		{
			m_ipAttributeNames = NULL;
		}

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

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11003");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::Save(IStream* pStream, BOOL fClearDirty)
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

		// Input/output files
		dataWriter << m_strOutputFileName;
		dataWriter << m_strVOAFileName;

		// Annotation settings
		dataWriter << m_bCarryForwardAnnotations;
		dataWriter << m_bApplyRedactionsAsAnnotations;

		// Attribute names
		bool bAttributeNames = (m_ipAttributeNames != NULL);
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
			writeObjectToStream(ipObj, pStream, "ELI11780", fClearDirty);
		}
		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11004");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRedactionTask::GetSizeMax(ULARGE_INTEGER* pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CRedactionTask::clear()
{
	m_strOutputFileName = gstrDEFAULT_REDACTED_IMAGE_FILENAME;
	m_strVOAFileName = gstrDEFAULT_TARGET_FILENAME;
	m_bCarryForwardAnnotations = false;
	m_bApplyRedactionsAsAnnotations = false;

	// Use the original image
	m_bUseRedactedImage = false;
	
	// Reset to default values
	m_redactionAppearance.reset();
	m_setAttributeNames.clear();
}
//-------------------------------------------------------------------------------------------------
IAFUtilityPtr CRedactionTask::getAFUtility()
{
	if (m_ipAFUtility == NULL)
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI09877", m_ipAFUtility != NULL);
	}
	return m_ipAFUtility;
}
//-------------------------------------------------------------------------------------------------
void CRedactionTask::validateLicense()
{
	VALIDATE_LICENSE(gnIDSHIELD_AUTOREDACTION_OBJECT, "ELI09999", "Redaction File Processor");
}
//-------------------------------------------------------------------------------------------------
void CRedactionTask::fillAttributeSet(IVariantVectorPtr ipAttributeNames, set<string>& rsetAttributeNames)
{
	long nSize = ipAttributeNames->Size;
	for (long n = 0; n < nSize; n++)
	{
		rsetAttributeNames.insert(asString(ipAttributeNames->Item[n].bstrVal));
	}
}
//-------------------------------------------------------------------------------------------------
UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr CRedactionTask::getIDShieldDBPtr()
{
	if (m_ipIDShieldDB == NULL)
	{
		m_ipIDShieldDB.CreateInstance(CLSID_IDShieldProductDBMgr);
		ASSERT_RESOURCE_ALLOCATION("ELI19794", m_ipIDShieldDB != NULL);		
	}
	return m_ipIDShieldDB;
}
//-------------------------------------------------------------------------------------------------
