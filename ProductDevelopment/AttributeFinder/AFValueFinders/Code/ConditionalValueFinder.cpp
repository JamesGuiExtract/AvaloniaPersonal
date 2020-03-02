// ConditionalValueFinder.cpp : Implementation of CConditionalValueFinder
#include "stdafx.h"
#include "AFValueFinders.h"
#include "ConditionalValueFinder.h"
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
// CConditionalValueFinder
//-------------------------------------------------------------------------------------------------
CConditionalValueFinder::CConditionalValueFinder() 
: m_bInvertCondition(false),
  m_ipCondition(NULL),
  m_ipRule(NULL)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10717")
}
//-------------------------------------------------------------------------------------------------
CConditionalValueFinder::~CConditionalValueFinder()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16340");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IConditionalRule,
		&IID_IAttributeFindingRule,
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
STDMETHODIMP CConditionalValueFinder::raw_GetRuleIID(IID* pIID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pIID = IID_IAttributeFindingRule;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10718")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::raw_GetCondition(IAFCondition** ppCondition)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI10721", ppCondition != __nullptr);

		CComQIPtr<IAFCondition> ipTemp = m_ipCondition;
		ipTemp.CopyTo(ppCondition);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10720")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::raw_SetCondition(IAFCondition* pCondition)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IAFConditionPtr ipTmp(pCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI10719", ipTmp != __nullptr);
		m_ipCondition = ipTmp;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10722")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::raw_GetRule(IUnknown** ppRule)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI10723", ppRule != __nullptr);

		CComPtr<IUnknown> ipTemp;
		ipTemp = m_ipRule;
		ipTemp.CopyTo(ppRule);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10724")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::raw_SetRule(IUnknown* pRule)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IUnknownPtr ipTmp(pRule);
		ASSERT_RESOURCE_ALLOCATION("ELI10725", ipTmp != __nullptr);

		IAttributeFindingRulePtr ipAttributeFindingRule = ipTmp;
		if(ipAttributeFindingRule == __nullptr)
		{
			UCLIDException ue("ELI10726", "Specified Object is not an Attribute Finding Rule.");
			throw ue;
		}
		m_ipRule = ipTmp;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10727")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::get_InvertCondition(VARIANT_BOOL * pbRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pbRetVal = asVariantBool(m_bInvertCondition);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10755")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::put_InvertCondition(VARIANT_BOOL bNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bInvertCondition = asCppBool(bNewVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10756")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::raw_GetCategoryName(BSTR *pstrCategoryName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pstrCategoryName = get_bstr_t(AFAPI_VALUE_FINDERS_CATEGORYNAME).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19388")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
													IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{		
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI10733", ipAFDoc != __nullptr);

		// pass the value into the rule set for further extraction
		IIUnknownVectorPtr ipAttributes = __nullptr;

		// use a smart pointer for the progress status object
		IProgressStatusPtr ipProgressStatus = pProgressStatus;

		// calculate the progress items count
		const long lPROGRESS_ITEMS_PER_CONDITION = 1;
		const long lPROGRESS_ITEMS_PER_VALUE_FINDER = 1;
		const long lTOTAL_PROGRESS_ITEMS = lPROGRESS_ITEMS_PER_CONDITION +
			lPROGRESS_ITEMS_PER_VALUE_FINDER;

		// initialize the progress status if the caller requested it
		if (ipProgressStatus)
		{
			ipProgressStatus->InitProgressStatus("Initializing conditional value finder",
				0, lTOTAL_PROGRESS_ITEMS, VARIANT_TRUE);

			ipProgressStatus->StartNextItemGroup("Processing value finder condition",
				lPROGRESS_ITEMS_PER_CONDITION); 
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

		// update progress status if it exists
		if (ipProgressStatus)
		{
			string strItemGroupName = string(bCondition ? "Executing" : "Not executing") +
				" value finder";
			ipProgressStatus->StartNextItemGroup(get_bstr_t(strItemGroupName), 
				lPROGRESS_ITEMS_PER_VALUE_FINDER);
		}
		
		if(bCondition)
		{
			PROFILE_RULE_OBJECT("", "", m_ipRule, 0)

			// parse the text passing in the sub progress object if it exists
			ipAttributes = m_ipRule->ParseText(ipAFDoc, 
				(ipProgressStatus ? ipProgressStatus->SubProgressStatus : NULL) );
		}
		else
		{
			ipAttributes.CreateInstance(CLSID_IUnknownVector);
		}

		// mark progress status as complete if it exists
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}

		// return the vector
		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10730");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ConditionalValueFinder;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::Load(IStream *pStream)
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
		readObjectFromStream(ipObj1, pStream, "ELI10743");
		m_ipCondition = ipObj1;

		IPersistStreamPtr ipObj2;
		readObjectFromStream(ipObj2, pStream, "ELI10744");
		m_ipRule = ipObj2;

		if (nDataVersion >= 2)
		{
			// Load the GUID for the IIdentifiableObject interface.
			loadGUID(pStream);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10728");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::Save(IStream *pStream, BOOL fClearDirty)
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
		ASSERT_RESOURCE_ALLOCATION("ELI10739", ipObj != __nullptr);
		writeObjectToStream(ipObj, pStream, "ELI10740", fClearDirty);

		ipObj = m_ipRule;
		ASSERT_RESOURCE_ALLOCATION("ELI10741", ipObj != __nullptr);
		writeObjectToStream(ipObj, pStream, "ELI10742", fClearDirty);

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10729");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19576", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Conditionally find value").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10731");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::raw_IsConfigured(VARIANT_BOOL * pbValue)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10732");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CConditionalValueFinder::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		IConditionalRulePtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI10734", ipSource != __nullptr);

		m_bInvertCondition = asCppBool(ipSource->InvertCondition);

		m_ipRule = ipSource->GetRule();
		m_ipCondition = ipSource->GetCondition();

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10735");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_ConditionalValueFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI10736", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10737");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalValueFinder::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33565")
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CConditionalValueFinder::validateLicense()
{
	static const unsigned long CONDITIONAL_VALUE_FINDER_COMPONENT_ID = 
		gnRULE_WRITING_CORE_OBJECTS;

	VALIDATE_LICENSE( CONDITIONAL_VALUE_FINDER_COMPONENT_ID, "ELI10738", 
					"Conditionally Find Value" );
}
//-------------------------------------------------------------------------------------------------
