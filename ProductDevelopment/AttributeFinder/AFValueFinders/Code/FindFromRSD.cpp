// FindFromRSD.cpp : Implementation of CFindFromRSD
#include "stdafx.h"
#include "AFValueFinders.h"
#include "FindFromRSD.h"
#include "..\\..\\AFCore\\Code\\Common.h"
#include "..\..\AFCore\Code\RuleSetLoader.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <Misc.h>
#include <CachedObjectFromFile.h>
#include <AFTagManager.h>
#include <ComponentLicenseIDs.h>

#include <io.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CFindFromRSD
//-------------------------------------------------------------------------------------------------
CFindFromRSD::CFindFromRSD()
: m_cachedRuleSet(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str())
{
	try
	{
		m_cachedRuleSet.m_obj = NULL;

		IAFUtilityPtr ipAFUtility(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI10241", ipAFUtility != __nullptr);

		m_bCacheRSD = asCppBool(ipAFUtility->ShouldCacheRSD);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10223")
}
//-------------------------------------------------------------------------------------------------
CFindFromRSD::~CFindFromRSD()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16344");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFindFromRSD,
		&IID_IAttributeFindingRule,
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
// IFindFromRSD
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::get_AttributeName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		*pVal = _bstr_t(m_strAttributeName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10224")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::put_AttributeName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strTmp = asString( newVal );

		if (strTmp.empty())
		{
			throw UCLIDException("ELI10225", "Please provide a valid attribute name.");
		}

		// Validate the attribute name
		if (!isValidIdentifier(strTmp))
		{
			UCLIDException uex("ELI28578", "Invalid attribute name.");
			uex.addDebugInfo("Invalid Name", strTmp);
			throw uex;
		}

		m_strAttributeName = strTmp;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10226")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::get_RSDFileName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		*pVal = _bstr_t(m_strRSDFileName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10227")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::put_RSDFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strTmp = asString( newVal );

		if (strTmp.empty())
		{
			throw UCLIDException("ELI10228", "Please provide a .rsd file name.");
		}

		m_strRSDFileName = strTmp;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10229")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_FindFromRSD;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::Load(IStream *pStream)
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
		
		dataReader >> m_strAttributeName;
		dataReader >> m_strRSDFileName;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10230");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strAttributeName;
		dataWriter << m_strRSDFileName;
		
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10231");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
										 IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{		
		validateLicense();

		// the specified RSD file may contain tags that need to be
		// expanded - so expand any tags therein
		AFTagManager tagMgr;
		string strRSDFile = tagMgr.expandTagsAndFunctions(m_strRSDFileName, pAFDoc);

		// NOTE: The following check is to prevent infinite loops that
		// for instance can be caused by a RSD file using itself as the splitter
		// ensure that the ruleset is not already executing by checking 
		// in the Rule Execution Environment
		if (m_ipRuleExecutionEnv == __nullptr)
		{
			m_ipRuleExecutionEnv.CreateInstance(CLSID_RuleExecutionEnv);
			ASSERT_RESOURCE_ALLOCATION("ELI19382", m_ipRuleExecutionEnv != __nullptr);
		}

		if (m_ipRuleExecutionEnv->IsRSDFileExecuting(strRSDFile.c_str()) ==
			VARIANT_TRUE)
		{
			UCLIDException ue("ELI19383", "Circular reference detected between RSD files in FindFromRSD object!");
			ue.addDebugInfo("RSD File", strRSDFile);
			throw ue;
		}

		// register a new rule execution session
		IRuleExecutionSessionPtr ipSession(CLSID_RuleExecutionSession);
		ASSERT_RESOURCE_ALLOCATION("ELI19384", ipSession != __nullptr);
		ipSession->SetRSDFileName(strRSDFile.c_str());
	
		// Create the ruleset if necessary
		if(m_cachedRuleSet.m_obj == NULL)
		{	
			m_cachedRuleSet.m_obj.CreateInstance(CLSID_RuleSet);
			ASSERT_RESOURCE_ALLOCATION("ELI10957", m_cachedRuleSet.m_obj != __nullptr);
		}
		// load/reload the ruleset if necessary
		m_cachedRuleSet.loadObjectFromFile(strRSDFile);

		// make a copy of the AFDocument for the doc to run on
		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI10921", ipAFDoc != __nullptr);
		ICopyableObjectPtr ipCopyObj = ipAFDoc;
		ASSERT_RESOURCE_ALLOCATION("ELI10922", ipCopyObj != __nullptr);
		IAFDocumentPtr ipDocCopy = ipCopyObj->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI10923", ipDocCopy != __nullptr);

		IVariantVectorPtr ipAttributeNames(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI10245", ipAttributeNames != __nullptr);

		_bstr_t _bstrAttributeName(m_strAttributeName.c_str());
		ipAttributeNames->PushBack(_bstrAttributeName);

		// pass the value into the rule set for further extraction
		IIUnknownVectorPtr ipAttributes 
			= m_cachedRuleSet.m_obj->ExecuteRulesOnText(ipDocCopy, ipAttributeNames, NULL);

		// Clear the cache if necessary
		if (!m_bCacheRSD)
		{
			m_cachedRuleSet.Clear();
		}

		// return the vector
		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10232");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19579", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Find from RSD file").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10233");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::raw_IsConfigured(VARIANT_BOOL * pbValue)
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

		// if the attribute is invalid
		if(m_strAttributeName.length() == 0)
		{
			bConfigured = false;
		}
		else if(m_strRSDFileName.length() == 0)
		{
			bConfigured = false;
		}

		// if the filename is invalid
	
		*pbValue = bConfigured ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10234");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CFindFromRSD::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IFindFromRSDPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI10235", ipSource != __nullptr);

		m_strAttributeName = asString(ipSource->AttributeName);
		m_strRSDFileName = asString(ipSource->RSDFileName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10236");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_FindFromRSD);
		ASSERT_RESOURCE_ALLOCATION("ELI10239", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10237");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CFindFromRSD::validateLicense()
{
	static const unsigned long FIND_FROM_RSD_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( FIND_FROM_RSD_COMPONENT_ID, "ELI10238", "Find From RSD File" );
}
//-------------------------------------------------------------------------------------------------