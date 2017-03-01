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
// Version 2: Added CIdentifiableObject
// Version 3: Added support for multiple attribute names
const unsigned long gnCurrentVersion = 3;

//-------------------------------------------------------------------------------------------------
// CFindFromRSD
//-------------------------------------------------------------------------------------------------
CFindFromRSD::CFindFromRSD()
: m_cachedRuleSet(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()),
  m_ipAttributeNames(CLSID_VariantVector)
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
// IFindFromRSD
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::get_AttributeNames(IVariantVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		IVariantVectorPtr ipShallowCopy = m_ipAttributeNames;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10224")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::put_AttributeNames(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipAttributeNames = newVal;
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

		// [FlexIDSCore:5276]
		// With a simple rule-writing license, only encryped (etf) files are allowed to be referenced,
		// not the customer's own rules.
		if (isLimitedLicense())
		{
			bool isEtf = false;
			if (strTmp.length() > 4)
			{
				string strExt = strTmp.substr(strTmp.length() - 4);
				makeLowerCase(strExt);
				isEtf = (strExt == ".etf");
			}

			if (!isEtf)
			{
				UCLIDException ue("ELI35642", "License validation error.\r\n\r\n"
					"Referencing unencrypted rulesets with \"Find from RSD file\" is not allowed with "
					"a simple rule-writing license.");
				throw ue;
			}
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
		
		if (nDataVersion <= 2)
		{
			std::string strAttributeName;
			dataReader >> strAttributeName;
			_bstr_t _bstrAttributeName(strAttributeName.c_str());
			m_ipAttributeNames->PushBack(_bstrAttributeName);
		}
		dataReader >> m_strRSDFileName;

		if (nDataVersion >= 2)
		{
			// Load the GUID for the IIdentifiableObject interface.
			loadGUID(pStream);
		}

		if (nDataVersion >= 3)
		{
			IPersistStreamPtr ipObj;
			readObjectFromStream( ipObj, pStream, "ELI39950" );
			ASSERT_RESOURCE_ALLOCATION("ELI39951", ipObj != __nullptr);
			m_ipAttributeNames = ipObj;
		}


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
		dataWriter << m_strRSDFileName;
		
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

		// Write the attribute names
		IPersistStreamPtr ipObj( m_ipAttributeNames );
		ASSERT_RESOURCE_ALLOCATION("ELI39952", ipObj != __nullptr);
		writeObjectToStream( ipObj, pStream, "ELI39953", fClearDirty );

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

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI10921", ipAFDoc != __nullptr);

		// the specified RSD file may contain tags that need to be
		// expanded - so expand any tags therein
		AFTagManager tagMgr;
		string strRSDFile = tagMgr.expandTagsAndFunctions(m_strRSDFileName, ipAFDoc);

		// NOTE: The following check is to prevent infinite loops that
		// for instance can be caused by a RSD file using itself as the splitter
		// ensure that the ruleset is not already executing by checking 
		// the AFDoc stack
		if (asCppBool(ipAFDoc->IsRSDFileExecuting(_bstr_t(strRSDFile.c_str()))))
		{
			UCLIDException ue("ELI19383", "Circular reference detected between RSD files in FindFromRSD object!");
			ue.addDebugInfo("RSD File", strRSDFile);
			throw ue;
		}

		// Create the ruleset if necessary
		if(m_cachedRuleSet.m_obj == NULL)
		{	
			m_cachedRuleSet.m_obj.CreateInstance(CLSID_RuleSet);
			ASSERT_RESOURCE_ALLOCATION("ELI10957", m_cachedRuleSet.m_obj != __nullptr);
		}

		// load/reload the ruleset if necessary
		m_cachedRuleSet.loadObjectFromFile(strRSDFile);

		// Make a copy of the AFDocument for the doc to run on.
		// There is no need to clone the AFDoc attribute hierarchy which can only ever be modified
		// if InputFinder is used and InputFinder will return a clone of the attributes, not the
		// attributes themselves.
		IAFDocumentPtr ipDocCopy = ipAFDoc->PartialClone(VARIANT_FALSE, VARIANT_TRUE);
		ASSERT_RESOURCE_ALLOCATION("ELI10923", ipDocCopy != __nullptr);

		IIUnknownVectorPtr ipAttributes;
		// pass the value into the rule set for further extraction
		if (m_ipAttributeNames->Size == 0)
		{
			// If no attribute names specified, pass null=find-all
			ipAttributes = m_cachedRuleSet.m_obj->ExecuteRulesOnText(ipDocCopy, __nullptr, "", NULL);
		}
		else
		{
			ipAttributes = m_cachedRuleSet.m_obj->ExecuteRulesOnText(ipDocCopy, m_ipAttributeNames, "", NULL);
		}


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

		if(m_strRSDFileName.length() == 0)
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

		ICopyableObjectPtr ipCopyObj = ipSource->GetAttributeNames();
		ASSERT_RESOURCE_ALLOCATION("ELI39954", ipCopyObj != __nullptr);
		m_ipAttributeNames = ipCopyObj->Clone();
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
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSD::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33568")
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CFindFromRSD::validateLicense()
{
	static const unsigned long FIND_FROM_RSD_COMPONENT_ID = gnRULE_WRITING_CORE_OBJECTS;

	VALIDATE_LICENSE( FIND_FROM_RSD_COMPONENT_ID, "ELI10238", "Find From RSD File" );
}
//-------------------------------------------------------------------------------------------------
bool CFindFromRSD::isLimitedLicense()
{
	try
	{
		VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI35645", "Find From RSD File");
	}
	catch (...)
	{
		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
