// RSDSplitter.cpp : Implementation of CRSDSplitter
#include "stdafx.h"
#include "AFSplitters.h"
#include "RSDSplitter.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <AFTagManager.h>
#include <ComponentLicenseIDs.h>

#include <io.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Version 2: Added CIdentifiableObject
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CRSDSplitter
//-------------------------------------------------------------------------------------------------
CRSDSplitter::CRSDSplitter()
: m_strRSDFileName(""),
  m_bDirty(false)
{
	try
	{
		m_ipRuleSet.m_obj = NULL;

		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI07847", m_ipMiscUtils!= __nullptr);

		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI07495", m_ipAFUtility != __nullptr);

		m_bCacheRSD = asCppBool(m_ipAFUtility->ShouldCacheRSD);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05778")
}
//-------------------------------------------------------------------------------------------------
CRSDSplitter::~CRSDSplitter()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16327");
}

//-------------------------------------------------------------------------------------------------
// IInterfaceSupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRSDSplitter,
		&IID_IAttributeSplitter,
		&IID_IPersistStream,
		&IID_ICopyableObject,
		&IID_ICategorizedComponent,
		&IID_ILicensedComponent,
		&IID_IMustBeConfiguredObject,
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
// IRSDSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::get_RSDFileName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strRSDFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05767")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::put_RSDFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		string strNewVal = asString(newVal);

		if (strNewVal.empty())
		{
			throw UCLIDException("ELI49814", "Please provide a .rsd file name.");
		}

		// [FlexIDSCore:5276]
		// With a simple rule-writing license, only encryped (etf) files are allowed to be referenced,
		// not the customer's own rules.
		if (isLimitedLicense())
		{
			bool isEtf = false;
			if (strNewVal.length() > 4)
			{
				string strExt = strNewVal.substr(strNewVal.length() - 4);
				makeLowerCase(strExt);
				isEtf = (strExt == ".etf");
			}

			if (!isEtf)
			{
				UCLIDException ue("ELI37170", "License validation error.\r\n\r\n"
					"Referencing unencrypted rulesets with \"Find from RSD file\" is not allowed with "
					"a simple rule-writing license.");
				throw ue;
			}
		}

		m_strRSDFileName = strNewVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05768")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::raw_SplitAttribute(IAttribute *pAttribute, IAFDocument *pAFDoc, 
											  IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI32930", ipAFDoc);

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
			UCLIDException ue("ELI07490", "Circular reference detected between RSD files in RSDSplitter object!");
			ue.addDebugInfo("RSD File", strRSDFile);
			throw ue;
		}

		// get current attribute's value
		IAttributePtr ipTopLevelAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI19104", ipTopLevelAttribute != __nullptr);

		// Copy the current AFDoc so that tags and RSD stack will be copied
		IAFDocumentPtr ipAFSplitterDoc = ipAFDoc->PartialClone(VARIANT_FALSE, VARIANT_FALSE);
		ASSERT_RESOURCE_ALLOCATION("ELI07618", ipAFSplitterDoc != __nullptr);

		ipAFSplitterDoc->Attribute = ipTopLevelAttribute;

		// pass the value into the rule set for further extraction
		IIUnknownVectorPtr ipSubAttrValues 
			= getRuleSet(strRSDFile)->ExecuteRulesOnText(ipAFSplitterDoc, NULL, "", NULL);

		// Clear the cache if necessary
		if (!m_bCacheRSD)
		{
			m_ipRuleSet.Clear();
		}

		if (ipSubAttrValues)
		{
			// store found sub attributes in the original attribute
			ipTopLevelAttribute->SubAttributes = ipSubAttrValues;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05758")
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19566", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Split attributes using RSD file").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05759")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		UCLID_AFSPLITTERSLib::IRSDSplitterPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08234", ipSource !=NULL);

		m_strRSDFileName = asString(ipSource->GetRSDFileName());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08235")
		
	return S_OK;

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_RSDSplitter);
		ASSERT_RESOURCE_ALLOCATION("ELI05762", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05763");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI19115", pbValue != __nullptr)
		
		*pbValue = m_strRSDFileName.empty() ? VARIANT_FALSE : VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05766");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_RSDSplitter;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset all the member variables
		m_strRSDFileName = "";

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
			UCLIDException ue( "ELI07636", "Unable to load newer RSDSplitter." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_strRSDFileName;
		}

		if (nDataVersion >= 2)
		{
			// Load the GUID for the IIdentifiableObject interface.
			loadGUID(pStream);
		}
			
		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05764");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::Save(IStream *pStream, BOOL fClearDirty)
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

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05765");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI19116", pbValue != __nullptr);
		
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19117");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRSDSplitter::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33560")
}

//-------------------------------------------------------------------------------------------------
// private / helper methods
//-------------------------------------------------------------------------------------------------
IRuleSetPtr CRSDSplitter::getRuleSet(string strRSDFile)
{
	// Create the rule set if not already created
	if (m_ipRuleSet.m_obj == NULL)
	{
		m_ipRuleSet.m_obj.CreateInstance(CLSID_RuleSet);
		ASSERT_RESOURCE_ALLOCATION("ELI05779", m_ipRuleSet.m_obj != __nullptr);
	}

	// init rule set from current rsd file, performing any auto-encrypt actions
	// as necessary
	_bstr_t _bstrRSDFile(strRSDFile.c_str());
	m_ipMiscUtils->AutoEncryptFile(_bstrRSDFile, 
		_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));
	validateFileOrFolderExistence(strRSDFile);

	m_ipRuleSet.loadObjectFromFile(strRSDFile);

	return m_ipRuleSet.m_obj;
}
//-------------------------------------------------------------------------------------------------
void CRSDSplitter::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_WRITING_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI05757", "RSD Splitter" );
}
//-------------------------------------------------------------------------------------------------
bool CRSDSplitter::isLimitedLicense()
{
	try
	{
		VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI35647", "RSD Splitter");
	}
	catch (...)
	{
		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------