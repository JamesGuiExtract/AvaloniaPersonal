// ConditionalOutputHandler.cpp : Implementation of CConditionalOutputHandler
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "ConditionalOutputHandler.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>
#include <RuleSetProfiler.h>

#include <io.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Version 2: Added CIdentifiableObject
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CConditionalOutputHandler
//-------------------------------------------------------------------------------------------------
CConditionalOutputHandler::CConditionalOutputHandler() 
: m_bInvertCondition(false),
  m_ipCondition(NULL),
  m_ipRule(NULL)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10859")
}
//-------------------------------------------------------------------------------------------------
CConditionalOutputHandler::~CConditionalOutputHandler()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16310");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IConditionalRule,
		&IID_IOutputHandler,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_ILicensedComponent,
		&IID_IIdentifiableObject
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IConditionalRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_GetRuleIID(IID* pIID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pIID = IID_IOutputHandler;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10860")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_GetCondition(IAFCondition** ppCondition)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI10889", ppCondition != __nullptr);

		CComQIPtr<IAFCondition> ipTemp = m_ipCondition;
		ipTemp.CopyTo(ppCondition);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10861")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_SetCondition(IAFCondition* pCondition)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IAFConditionPtr ipTmp(pCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI10864", ipTmp != __nullptr);
		m_ipCondition = ipTmp;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10862")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_GetRule(IUnknown** ppRule)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI10865", ppRule != __nullptr);

		CComPtr<IUnknown> ipTemp;
		ipTemp = m_ipRule;
		ipTemp.CopyTo(ppRule);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10863")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_SetRule(IUnknown* pRule)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IUnknownPtr ipTmp(pRule);
		ASSERT_RESOURCE_ALLOCATION("ELI10866", ipTmp != __nullptr);

		IOutputHandlerPtr ipOutputHandler = ipTmp;
		if(ipOutputHandler == __nullptr)
		{
			UCLIDException ue("ELI10867", "Specified Object is not an Output Handler!");
			throw ue;
		}
		m_ipRule = ipTmp;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10868")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::get_InvertCondition(VARIANT_BOOL * pbRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pbRetVal = asVariantBool(m_bInvertCondition);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10869")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::put_InvertCondition(VARIANT_BOOL bNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bInvertCondition = asCppBool(bNewVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10870")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_GetCategoryName(BSTR *pstrCategoryName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pstrCategoryName = _bstr_t(AFAPI_OUTPUT_HANDLERS_CATEGORYNAME.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10871")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_ProcessOutput(IIUnknownVector *pAttributes, 
														  IAFDocument* pAFDoc, 
														  IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{		
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI10872", ipAFDoc != __nullptr);

		// pass the value into the rule set for further extraction
		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI10890", ipAttributes != __nullptr);

		// initialize progress item counts
		const long lPROGRESS_ITEMS_PER_PROCESS_CONDITION = 1;
		const long lPROGRESS_ITEMS_PER_OUTPUT_HANDLER_PROCESS = 1;
		const long lTOTAL_PROGRESS_ITEMS = lPROGRESS_ITEMS_PER_PROCESS_CONDITION + 
			lPROGRESS_ITEMS_PER_OUTPUT_HANDLER_PROCESS;

		// use a smart pointer for the progress status interface
		IProgressStatusPtr ipProgressStatus = pProgressStatus;

		// check if progress status is desired
		if (ipProgressStatus)
		{
			// initiate the progress status
			ipProgressStatus->InitProgressStatus("Starting conditional output handler", 0, 
				lTOTAL_PROGRESS_ITEMS, VARIANT_FALSE);
			ipProgressStatus->StartNextItemGroup("Processing condition", 
				lPROGRESS_ITEMS_PER_PROCESS_CONDITION);
		}

		bool bCondition;
		{
			PROFILE_RULE_OBJECT("", "", m_ipCondition, 0)

			bCondition = asCppBool( m_ipCondition->ProcessCondition(ipAFDoc) );
		}

		if(m_bInvertCondition)
		{
			bCondition = !bCondition;
		}
		
		if(bCondition)
		{
			// indicate output handler is starting if progress status is being recorded
			if (ipProgressStatus)
			{
				ipProgressStatus->StartNextItemGroup("Processing output handler", 
					lPROGRESS_ITEMS_PER_OUTPUT_HANDLER_PROCESS);
			}

			PROFILE_RULE_OBJECT("", "", m_ipRule, 0)

			m_ipRule->ProcessOutput(ipAttributes, ipAFDoc, NULL);
		}

		// mark progress status as completed if progress is being recorded
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10873");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ConditionalOutputHandler;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::Load(IStream *pStream)
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

		dataReader >>  m_bInvertCondition;

		// read the object
		IPersistStreamPtr ipObj1;
		readObjectFromStream(ipObj1, pStream, "ELI10874");
		m_ipCondition = ipObj1;

		IPersistStreamPtr ipObj2;
		readObjectFromStream(ipObj2, pStream, "ELI10875");
		m_ipRule = ipObj2;

		if (nDataVersion >= 2)
		{
			// Load the GUID for the IIdentifiableObject interface.
			loadGUID(pStream);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10876");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::Save(IStream *pStream, BOOL fClearDirty)
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

		dataWriter << m_bInvertCondition;
		
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		IPersistStreamPtr ipObj = m_ipCondition;
		ASSERT_RESOURCE_ALLOCATION("ELI10877", ipObj != __nullptr);
		writeObjectToStream(ipObj, pStream, "ELI10878", fClearDirty);

		ipObj = m_ipRule;
		ASSERT_RESOURCE_ALLOCATION("ELI10879", ipObj != __nullptr);
		writeObjectToStream(ipObj, pStream, "ELI10880", fClearDirty);

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10881");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19541", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Conditionally handle output").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10882");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();
		bool bConfigured = true;

		if (m_ipRule == __nullptr ||
			m_ipCondition == __nullptr)
		{
			bConfigured = false;
		}
	
		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10883");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		IConditionalRulePtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI10884", ipSource != __nullptr);

		m_bInvertCondition = asCppBool(ipSource->InvertCondition);

		m_ipRule = ipSource->GetRule();
		m_ipCondition = ipSource->GetCondition();

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10885");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_ConditionalOutputHandler);
		ASSERT_RESOURCE_ALLOCATION("ELI10886", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10887");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalOutputHandler::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33526")
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CConditionalOutputHandler::validateLicense()
{
	static const unsigned long CONDITIONAL_OUTPUT_HANDLER_COMPONENT_ID = gnRULE_WRITING_CORE_OBJECTS;

	VALIDATE_LICENSE( CONDITIONAL_OUTPUT_HANDLER_COMPONENT_ID, "ELI10888", 
					"Conditionally Handle Output" );
}
//-------------------------------------------------------------------------------------------------
