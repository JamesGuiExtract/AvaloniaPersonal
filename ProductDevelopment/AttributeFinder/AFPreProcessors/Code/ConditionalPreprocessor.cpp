// ConditionalPreprocessor.cpp : Implementation of CConditionalPreprocessor
#include "stdafx.h"
#include "AFPreProcessors.h"
#include "ConditionalPreprocessor.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>

#include <io.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CConditionalPreprocessor
//-------------------------------------------------------------------------------------------------
CConditionalPreprocessor::CConditionalPreprocessor() 
: m_bInvertCondition(false),
  m_ipCondition(NULL),
  m_ipRule(NULL)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10827")
}
//-------------------------------------------------------------------------------------------------
CConditionalPreprocessor::~CConditionalPreprocessor()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16323");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IConditionalRule,
		&IID_IDocumentPreprocessor,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_ILicensedComponent
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
STDMETHODIMP CConditionalPreprocessor::raw_GetRuleIID(IID* pIID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pIID = IID_IDocumentPreprocessor;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10853")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::raw_GetCondition(IAFCondition** ppCondition)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI10854", ppCondition != NULL);

		CComQIPtr<IAFCondition> ipTemp = m_ipCondition;
		ipTemp.CopyTo(ppCondition);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10828")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::raw_SetCondition(IAFCondition* pCondition)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IAFConditionPtr ipTmp(pCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI10829", ipTmp != NULL);
		m_ipCondition = ipTmp;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10855")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::raw_GetRule(IUnknown** ppRule)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI10830", ppRule != NULL);

		CComPtr<IUnknown> ipTemp;
		ipTemp = m_ipRule;
		ipTemp.CopyTo(ppRule);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10831")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::raw_SetRule(IUnknown* pRule)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IUnknownPtr ipTmp(pRule);
		ASSERT_RESOURCE_ALLOCATION("ELI10832", ipTmp != NULL);

		ipTmp->QueryInterface(IID_IDocumentPreprocessor, (void**)&pRule);
		if(pRule == NULL)
		{
			UCLIDException ue("ELI10833", "Specified Object is not a Document Preprocessor.");
			throw ue;
		}
		m_ipRule = ipTmp;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10834")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::get_InvertCondition(VARIANT_BOOL * pbRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pbRetVal = asVariantBool(m_bInvertCondition);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10835")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::put_InvertCondition(VARIANT_BOOL bNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bInvertCondition = asCppBool(bNewVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10836")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::raw_GetCategoryName(BSTR *pstrCategoryName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pstrCategoryName = get_bstr_t(AFAPI_DOCUMENT_PREPROCESSORS_CATEGORYNAME).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10837")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IDocumentPreprocessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::raw_Process(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{		
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI10838", ipAFDoc != NULL);

		// use a smart pointer for the progress status object
		IProgressStatusPtr ipProgressStatus = pProgressStatus;

		// calculate the progress items count
		const long lPROGRESS_ITEMS_PER_PREPROCESSOR_CONDITION = 1;
		const long lPROGRESS_ITEMS_PER_PREPROCESSOR = 1;
		const long lTOTAL_PROGRESS_ITEMS = lPROGRESS_ITEMS_PER_PREPROCESSOR_CONDITION +
			lPROGRESS_ITEMS_PER_PREPROCESSOR;

		// initialize the progress status if the caller requested it
		if (ipProgressStatus)
		{
			ipProgressStatus->InitProgressStatus("Initializing conditional preprocess",
				0, lTOTAL_PROGRESS_ITEMS, VARIANT_TRUE);

			ipProgressStatus->StartNextItemGroup("Processing preprocessor condition",
				lPROGRESS_ITEMS_PER_PREPROCESSOR_CONDITION); 
		}

		bool bCondition = asCppBool( m_ipCondition->ProcessCondition(ipAFDoc) );

		if(m_bInvertCondition)
		{
			bCondition = !bCondition;
		}
		
		// update progress status if it exists
		if (ipProgressStatus)
		{
			string strItemGroupName = string(bCondition ? "Executing" : "Not executing") +
				" preprocessor";

			ipProgressStatus->StartNextItemGroup(get_bstr_t(strItemGroupName),
				lPROGRESS_ITEMS_PER_PREPROCESSOR);
		}

		if(bCondition)
		{
			// run this preprocessor and pass in the sub progress object 
			// if the caller requested progress status updates
			m_ipRule->Process(ipAFDoc, 
				(ipProgressStatus ? ipProgressStatus->SubProgressStatus : NULL) );
		}

		// mark progress status as complete if it exists
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10839");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ConditionalPreprocessor;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::Load(IStream *pStream)
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
		readObjectFromStream(ipObj1, pStream, "ELI10840");
		m_ipCondition = ipObj1;

		IPersistStreamPtr ipObj2;
		readObjectFromStream(ipObj2, pStream, "ELI10841");
		m_ipRule = ipObj2;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10842");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::Save(IStream *pStream, BOOL fClearDirty)
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
		ASSERT_RESOURCE_ALLOCATION("ELI10843", ipObj != NULL);
		writeObjectToStream(ipObj, pStream, "ELI10856", fClearDirty);

		ipObj = m_ipRule;
		ASSERT_RESOURCE_ALLOCATION("ELI10844", ipObj != NULL);
		writeObjectToStream(ipObj, pStream, "ELI10857", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10845");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19558", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Conditionally preprocess").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10846");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::raw_IsConfigured(VARIANT_BOOL * pbValue)
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

		if (m_ipRule == NULL ||
			m_ipCondition == NULL)
		{
			bConfigured = false;
		}
	
		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10847");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CConditionalPreprocessor::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		IConditionalRulePtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI10848", ipSource != NULL);

		m_bInvertCondition = asCppBool(ipSource->InvertCondition);

		m_ipRule = ipSource->GetRule();
		m_ipCondition = ipSource->GetCondition();

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10849");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalPreprocessor::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_ConditionalPreprocessor);
		ASSERT_RESOURCE_ALLOCATION("ELI10852", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10850");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CConditionalPreprocessor::validateLicense()
{
	static const unsigned long CONDITIONAL_DOCUMENT_PREPROCESSOR_COMPONENT_ID = 
		gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( CONDITIONAL_DOCUMENT_PREPROCESSOR_COMPONENT_ID, "ELI10851", 
					"Conditionally Preprocess" );
}
//-------------------------------------------------------------------------------------------------
