// RSDFileCondition.cpp : Implementation of CRSDFileCondition
#include "stdafx.h"
#include "AFConditions.h"
#include "RSDFileCondition.h"

#include <AFCategories.h>
#include <Common.h>
#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <Misc.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CRSDFileCondition
//-------------------------------------------------------------------------------------------------
CRSDFileCondition::CRSDFileCondition()
: m_bNewFileName(false)
{
	try
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI10918", m_ipAFUtility != __nullptr);

		m_bCacheRSD = asCppBool(m_ipAFUtility->ShouldCacheRSD);

		m_cachedRuleSet.m_obj = __nullptr;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10891");
}
//-------------------------------------------------------------------------------------------------
CRSDFileCondition::~CRSDFileCondition()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16297");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRSDFileCondition,
		&IID_IAFCondition,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ISpecifyPropertyPages,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_ILicensedComponent
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
// IRSDFileCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::get_RSDFileName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		*pVal = _bstr_t( m_strRSDFileName.c_str() ).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10893")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::put_RSDFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		string strNewVal = asString(newVal);
		if(strNewVal.empty())
		{
			UCLIDException ue("ELI10892", "Cannot specify an empty .rsd file name.");
			throw ue;
		}
		if (strNewVal != m_strRSDFileName)
		{
			m_bNewFileName = true;
			m_strRSDFileName = strNewVal;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10894")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAFCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::raw_ProcessCondition(IAFDocument *pAFDoc, VARIANT_BOOL* pbRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// create a copy of the document to run the rules on
		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI10897", ipAFDoc != __nullptr);
		ICopyableObjectPtr ipCopyObj = ipAFDoc;
		ASSERT_RESOURCE_ALLOCATION("ELI10916", ipCopyObj != __nullptr);
		IAFDocumentPtr ipDocCopy = ipCopyObj->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI10917", ipDocCopy != __nullptr);

		// the specified RSD file may contain tags that need to be
		// expanded - so expand any tags therein
		string strRSDFile = m_ipAFUtility->ExpandTags(m_strRSDFileName.c_str(),
			pAFDoc);

		// NOTE: The following check is to prevent infinite loops that
		// for instance can be caused by a RSD file using itself as the splitter
		// ensure that the ruleset is not already executing by checking 
		// in the Rule Execution Environment
		if (m_ipRuleExecutionEnv == __nullptr)
		{
			m_ipRuleExecutionEnv.CreateInstance(CLSID_RuleExecutionEnv);
			ASSERT_RESOURCE_ALLOCATION("ELI10242", m_ipRuleExecutionEnv != __nullptr);
		}

		if (m_ipRuleExecutionEnv->IsRSDFileExecuting(strRSDFile.c_str()) ==
			VARIANT_TRUE)
		{
			UCLIDException ue("ELI10243", "Circular reference detected between RSD files in FindFromRSD object.");
			ue.addDebugInfo("RSD File", strRSDFile);
			throw ue;
		}

		// register a new rule execution session
		IRuleExecutionSessionPtr ipSession(CLSID_RuleExecutionSession);
		ASSERT_RESOURCE_ALLOCATION("ELI10244", ipSession != __nullptr);
		ipSession->SetRSDFileName(strRSDFile.c_str());

		// init rule set from current rsd file, performing any auto-encrypt actions
		// as necessary
		autoEncryptFile(strRSDFile, gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str());
		validateFileOrFolderExistence(strRSDFile);

		// create the rule set if necessary
		if(m_cachedRuleSet.m_obj == __nullptr)
		{
			m_cachedRuleSet.m_obj.CreateInstance(CLSID_RuleSet);
			ASSERT_RESOURCE_ALLOCATION("ELI10908", m_cachedRuleSet.m_obj != __nullptr);
		}
		// reload the ruleset if necessary
		m_cachedRuleSet.loadObjectFromFile(strRSDFile);

		// pass the value into the rule set for further extraction
		IIUnknownVectorPtr ipAttributes 
			= m_cachedRuleSet.m_obj->ExecuteRulesOnText(ipDocCopy, __nullptr, __nullptr);

		// Clear the cache if necessary
		if (!m_bCacheRSD)
		{
			m_cachedRuleSet.Clear();
		}

		// If any attributes are found this condition passes
		bool bPass = ipAttributes->Size() != 0;
		
		*pbRetVal = bPass ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10896")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == __nullptr)
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
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == __nullptr)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();
		bool bIsConfigured = true;

		if(m_strRSDFileName.empty())
		{
			bIsConfigured = false;
		}
		*pbValue = bIsConfigured ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10899");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19539", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("RSD file condition").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10900")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFCONDITIONSLib::IRSDFileConditionPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION( "ELI10901", ipSource != __nullptr );
		
		string strNewVal = ipSource->RSDFileName;
		if (strNewVal != m_strRSDFileName)
		{
			m_bNewFileName = true;
			m_strRSDFileName = strNewVal;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12858");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_RSDFileCondition );
		ASSERT_RESOURCE_ALLOCATION( "ELI10902", ipObjCopy != __nullptr );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom( ipUnk );
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10903");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pClassID = CLSID_RSDFileCondition;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12859");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12860");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), __nullptr);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, __nullptr);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		dataReader >> m_strRSDFileName;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI10904", 
				"Unable to load newer RSDFile Condition." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10905");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strRSDFileName;

		dataWriter.flushToByteStream();
	
		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), __nullptr );
		pStream->Write( data.getData(), nDataLength, __nullptr );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10906");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDFileCondition::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return E_NOTIMPL;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12861");
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CRSDFileCondition::validateLicense()
{
	static const unsigned long RSD_FILE_CONDITION_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( RSD_FILE_CONDITION_ID, "ELI10907", 
		"RSD File Condition" );
}
//-------------------------------------------------------------------------------------------------
