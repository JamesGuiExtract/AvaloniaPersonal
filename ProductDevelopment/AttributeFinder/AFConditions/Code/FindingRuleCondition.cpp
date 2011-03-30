// FindingRuleCondition.cpp : Implementation of CFindingRuleCondition

#include "stdafx.h"
#include "FindingRuleCondition.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CFindingRuleCondition
//--------------------------------------------------------------------------------------------------
CFindingRuleCondition::CFindingRuleCondition() :
	m_bDirty(false),
	m_ipAFRule(__nullptr)
{
}
//--------------------------------------------------------------------------------------------------
CFindingRuleCondition::~CFindingRuleCondition()
{
	try
	{
		m_ipAFRule = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18214");
}
//--------------------------------------------------------------------------------------------------
HRESULT CFindingRuleCondition::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CFindingRuleCondition::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// IFindingRuleCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::get_AFRule(IAttributeFindingRule **ppRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI18316", ppRetVal != __nullptr);

		// Return a shallow copy
		IAttributeFindingRulePtr ipAFRule(m_ipAFRule);
		*ppRetVal = ipAFRule.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18279")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::put_AFRule(IAttributeFindingRule *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI18321", pNewVal != __nullptr);

		m_ipAFRule = pNewVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18281")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAFCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::raw_ProcessCondition(IAFDocument *pAFDoc, VARIANT_BOOL* pbRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Assert parameters and resources
		ASSERT_ARGUMENT("ELI18216", pAFDoc != __nullptr);
		ASSERT_ARGUMENT("ELI18217", pbRetVal != __nullptr);
		ASSERT_RESOURCE_ALLOCATION("ELI18294", m_ipAFRule != __nullptr);
		
		if (m_ipAFRule == __nullptr)
		{
			UCLIDException ue("ELI18322", "Finding Rule Condition: No rule defined!");
			throw ue;
		}

		// Create a copy of the document to run the rules on
		ICopyableObjectPtr ipCopyObj = pAFDoc;
		ASSERT_RESOURCE_ALLOCATION("ELI18295", ipCopyObj != __nullptr);
		IAFDocumentPtr ipDocCopy = ipCopyObj->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI18296", ipDocCopy != __nullptr);
		
		// Parse the supplied document with the configured attribute finding rule
		IIUnknownVectorPtr ipAttributes = m_ipAFRule->ParseText(ipDocCopy, __nullptr);
		ASSERT_RESOURCE_ALLOCATION("ELI18297", ipAttributes != __nullptr);
		
		// Return VARIANT_TRUE if the finding rule returned a vector with at least one value
		*pbRetVal = asVariantBool(ipAttributes->Size() > 0);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18218")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI18317", pbValue != __nullptr);

		// Default to unconfigured
		*pbValue = VARIANT_FALSE;

		// Must have a rule to be configured
		if (m_ipAFRule)
		{
			IMustBeConfiguredObjectPtr ipRuleConfig = m_ipAFRule;

			// If the rule doesn't implement IMustBeConfiguredObject or if 
			// IMustBeConfiguredObject::IsConfigured == VARIANT_TRUE, consider
			// this condition configured
			if (ipRuleConfig == __nullptr || ipRuleConfig->IsConfigured())
			{
				*pbValue = VARIANT_TRUE;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18221");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18222", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Finding rule condition").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18224")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_AFCONDITIONSLib::IFindingRuleConditionPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI18225", ipCopyThis != __nullptr);
		
		if (ipCopyThis->AFRule == __nullptr)
		{
			m_ipAFRule = __nullptr;
		}
		else
		{
			// If AFRule is not __nullptr, obtain a clone
			ICopyableObjectPtr ipCopyableRule(ipCopyThis->AFRule);
			ASSERT_RESOURCE_ALLOCATION("ELI18318", ipCopyableRule != __nullptr);
			m_ipAFRule = ipCopyableRule->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI18319", m_ipAFRule != __nullptr);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18248");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI18229", pObject != __nullptr);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_FindingRuleCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI18227", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18228");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18231", pClassID != __nullptr);

		*pClassID = CLSID_FindingRuleCondition;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18230");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18232");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI18233", pStream != __nullptr);

		// Clear the existing rule
		m_ipAFRule = __nullptr;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), __nullptr);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, __nullptr);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI18234", "Unable to load newer rule finding condition!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read the attribute finding rule from the stream
		IPersistStreamPtr ipRuleStream;
		readObjectFromStream(ipRuleStream, pStream, "ELI18263");
		m_ipAFRule = ipRuleStream;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18235");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI18236", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), __nullptr);
		pStream->Write(data.getData(), nDataLength, __nullptr);

		// Write the attribute finding rule to the stream
		IPersistStreamPtr ipRuleStream = m_ipAFRule;
		ASSERT_RESOURCE_ALLOCATION("ELI18264", ipRuleStream != __nullptr);
		writeObjectToStream(ipRuleStream, pStream, "ELI18265", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18239");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}


//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18219", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18220");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IFindingRuleCondition,
			&IID_IAFCondition,
			&IID_IPersistStream,
			&IID_ICategorizedComponent,
			&IID_ISpecifyPropertyPages,
			&IID_ICopyableObject,
			&IID_IMustBeConfiguredObject,
			&IID_ILicensedComponent
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18215")

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CFindingRuleCondition::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI18240", "Finding Rule Condition");
}
//-------------------------------------------------------------------------------------------------