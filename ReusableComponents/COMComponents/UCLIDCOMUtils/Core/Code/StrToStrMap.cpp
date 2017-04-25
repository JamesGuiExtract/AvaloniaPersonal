// StrToStrMap.cpp : Implementation of CStrToStrMap
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "StrToStrMap.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Version 2: Added CaseSensitive
const unsigned long gnCurrentVersion = 2;
const long gnMAX_MERGE_COUNT = 1000;

//-------------------------------------------------------------------------------------------------
// CStrToStrMap
//-------------------------------------------------------------------------------------------------
CStrToStrMap::CStrToStrMap()
: m_bCaseSensitive(true)
, m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CStrToStrMap::~CStrToStrMap()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16519");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IStrToStrMap,
		&IID_ILicensedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IStrToStrMap
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::Set(BSTR key, BSTR value)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		stringCSIS csisKey(asString(key), m_bCaseSensitive);
		m_mapKeyToValue[csisKey] = asString(value);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04253");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::GetValue(BSTR key, BSTR *pValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		stringCSIS csisKey(asString(key), m_bCaseSensitive);

		map<stringCSIS, string>::iterator it = m_mapKeyToValue.find(csisKey);
		if (it != m_mapKeyToValue.end())
		{
			*pValue = _bstr_t(it->second.c_str()).Detach();
		}
		else
		{
			UCLIDException ue("ELI04260", "Map does not contain the specific key!");
			ue.addDebugInfo("Key", asString(key));
			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04254");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::Contains(BSTR key, VARIANT_BOOL *bFound)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		ASSERT_ARGUMENT("ELI28401", bFound != __nullptr);

		stringCSIS csisKey(asString(key), m_bCaseSensitive);

		*bFound = asVariantBool(m_mapKeyToValue.find(csisKey) != m_mapKeyToValue.end());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04255");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::RemoveItem(BSTR key)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		stringCSIS csisKey(asString(key), m_bCaseSensitive);

		map<stringCSIS, string>::iterator it = m_mapKeyToValue.find(csisKey);
		if (it != m_mapKeyToValue.end())
		{
			m_mapKeyToValue.erase(it);

			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04256");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		clear();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04257");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::GetKeys(IVariantVector **pKeys)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28402", pKeys != __nullptr);
		
		UCLID_COMUTILSLib::IVariantVectorPtr ipKeys(CLSID_VariantVector);
		if (ipKeys == __nullptr)
		{
			throw UCLIDException("ELI04372", "Unable to create VariantVector object!");
		}

		map<stringCSIS, string>::iterator it = m_mapKeyToValue.begin();
		for (; it != m_mapKeyToValue.end(); it++)
		{
			ipKeys->PushBack(it->first.c_str());
		}

		*pKeys = (IVariantVector*) ipKeys.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04258");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::get_Size(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28403", pVal != __nullptr);
		
		*pVal = m_mapKeyToValue.size();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04259");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::GetKeyValue(long nIndex, BSTR *pstrKey, BSTR *pstrValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate the license first
		validateLicense();

		ASSERT_ARGUMENT("ELI28404", pstrKey != __nullptr);
		ASSERT_ARGUMENT("ELI28405", pstrValue != __nullptr);
		
		// ensure that the index is valid
		if ((unsigned long) nIndex >= m_mapKeyToValue.size())
		{
			UCLIDException ue("ELI15348", "Invalid map index!");
			ue.addDebugInfo("nIndex", nIndex);
			ue.addDebugInfo("Map size", m_mapKeyToValue.size());
			throw ue;
		}

		// find the entry of the map at the specified index and return the key/value
		map<stringCSIS, string>::iterator iter = m_mapKeyToValue.begin();
		for (int i = 0; i < nIndex; i++)
		{
			iter++;
		}

		*pstrKey = _bstr_t(iter->first.c_str()).Detach();
		*pstrValue = _bstr_t(iter->second.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04476");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::RenameKey(BSTR bstrKey, BSTR bstrNewKeyName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate the license first
		validateLicense();
		
		string strKey = asString(bstrKey);
		stringCSIS csisKey(strKey, m_bCaseSensitive);
		string strNewKeyName = asString(bstrNewKeyName);
		stringCSIS csisNewKey(asString(bstrNewKeyName), m_bCaseSensitive);

		// only process this request if the new key and the old key are different
		if (csisKey != csisNewKey)
		{
			// ensure that the specified key is valid
			map<stringCSIS, string>::iterator iter1 = m_mapKeyToValue.find(csisKey);
			if (iter1 == m_mapKeyToValue.end())
			{
				UCLIDException ue("ELI04530", "Invalid map key!");
				ue.addDebugInfo("Key", strKey);
				throw ue;
			}

			// ensure that the new key is not already in the map
			map<stringCSIS, string>::iterator iter2 = m_mapKeyToValue.find(csisNewKey);
			if (iter2 != m_mapKeyToValue.end())
			{
				UCLIDException ue("ELI04531", "Specified key already exists in map!");
				ue.addDebugInfo("Old Key", strKey);
				ue.addDebugInfo("New Key", strNewKeyName);
				throw ue;
			}

			// add the new entry to the map
			m_mapKeyToValue[csisNewKey] = iter1->second;

			// remove the original entry from the map
			m_mapKeyToValue.erase(iter1);

			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04529");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::Merge(IStrToStrMap *pMapToMerge, EMergeMethod eMergeMethod)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_COMUTILSLib::IStrToStrMapPtr ipMapToMerge(pMapToMerge);
		ASSERT_ARGUMENT("ELI20194", ipMapToMerge != __nullptr);

		// validate license
		validateLicense();

		long nSize = ipMapToMerge->Size;
		for (long i = 0; i < nSize; i++)
		{
			// Take each mapping from pMapToMerge, and merge the key/value pairs in one-by-one
			_bstr_t bstrKey;
			_bstr_t bstrValue;

			ipMapToMerge->GetKeyValue(i, bstrKey.GetAddress(), bstrValue.GetAddress());

			mergeKeyValue(bstrKey, bstrValue, eMergeMethod);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20195");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::MergeKeyValue(BSTR bstrKey, BSTR bstrValue, EMergeMethod eMergeMethod)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20196", bstrKey != __nullptr);
		ASSERT_ARGUMENT("ELI20197", bstrValue != __nullptr);

		// validate license
		validateLicense();

		mergeKeyValue(bstrKey, bstrValue, eMergeMethod);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20198");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::GetAllKeyValuePairs(IIUnknownVector** ppPairs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI31903", ppPairs != __nullptr);

		// validate license
		validateLicense();

		UCLID_COMUTILSLib::IIUnknownVectorPtr ipPairs(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI31904", ipPairs != __nullptr);
		for(map<stringCSIS, string>::iterator it = m_mapKeyToValue.begin();
			it != m_mapKeyToValue.end(); it++)
		{
			UCLID_COMUTILSLib::IStringPairPtr ipPair(CLSID_StringPair);
			ASSERT_RESOURCE_ALLOCATION("ELI31905", ipPair != __nullptr);
			
			ipPair->SetKeyValuePair(it->first.c_str(), it->second.c_str());
			ipPairs->PushBack(ipPair);
		}

		*ppPairs = (IIUnknownVector*) ipPairs.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31906");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::get_CaseSensitive(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

		try
	{
		ASSERT_ARGUMENT("ELI43212", pbVal != __nullptr);

		*pbVal = asVariantBool(m_bCaseSensitive);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43213");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::put_CaseSensitive(VARIANT_BOOL bVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		bool bCaseSensitive = asCppBool(bVal);
		ASSERT_RUNTIME_CONDITION("ELI43210",
			(m_bCaseSensitive == bCaseSensitive) || (m_mapKeyToValue.size() == 0),
			"Cannot alter map case-sensitivity while populated.");

		m_bCaseSensitive = asCppBool(bVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43211");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_StrToStrMap;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28406", pStream != __nullptr);

		// clear the internal map
		m_mapKeyToValue.clear();
		
		// read the number of entries to the stream
		long nNumEntries = 0;

		m_bCaseSensitive = true;

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
			UCLIDException ue( "ELI07669", "Unable to load newer StringToStringMap." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> nNumEntries;
		}
		if (nDataVersion >= 2)
		{
			dataReader >> m_bCaseSensitive;
		}

		// for each map entry, read from the stream the name of the entry
		// as well as the value 
		for (int i = 0; i < nNumEntries; i++)
		{
			// read the name from the stream
			CComBSTR bstrKey;
			bstrKey.ReadFromStream(pStream);
			// read the name from the stream
			CComBSTR bstrValue;
			bstrValue.ReadFromStream(pStream);

			// store the key/value pair to the map
			stringCSIS stdstrKey(asString(bstrKey), m_bCaseSensitive);
			m_mapKeyToValue[stdstrKey] = asString(bstrValue);
		}

		// set the dirty flag to false as we've just loaded the object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04727");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28407", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;
		
		// write the number of entries to the stream
		long nNumEntries = m_mapKeyToValue.size();
		dataWriter << nNumEntries;
		dataWriter << m_bCaseSensitive;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// for each map entry, write to the stream the name of the entry
		// and the value
		map<stringCSIS, string>::iterator iter;
		for (iter = m_mapKeyToValue.begin(); iter != m_mapKeyToValue.end(); iter++)
		{
			// write the key to the stream
			CComBSTR bstrKey = iter->first.c_str();
			bstrKey.WriteToStream(pStream);

			// write the value to the stream
			CComBSTR bstrValue = iter->second.c_str();
			bstrValue.WriteToStream(pStream);
		}

		// clear the flag as specified
		if (fClearDirty) m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04728");


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the other map object
		UCLID_COMUTILSLib::IStrToStrMapPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI19349", ipSource != __nullptr);

		clear();

		m_bCaseSensitive = asCppBool(ipSource->CaseSensitive);

		long lSize = ipSource->Size;
		for(long i = 0; i < lSize; i++)
		{
			CComBSTR bstrKey, bstrValue;
			ipSource->GetKeyValue(i, &bstrKey, &bstrValue);
			stringCSIS csisKey(asString(bstrKey), m_bCaseSensitive);
			m_mapKeyToValue[csisKey] = asString(bstrValue);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08211");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToStrMap::Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a new map
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_StrToStrMap);
		ASSERT_RESOURCE_ALLOCATION("ELI19355", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new map to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19313");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CStrToStrMap::mergeKeyValue(const _bstr_t &bstrKey, const _bstr_t &bstrValue, 
								 EMergeMethod eMergeMethod)
{
	string strKey = asString(bstrKey);
	ASSERT_ARGUMENT("ELI20191", !strKey.empty());

	stringCSIS csisKey(strKey, m_bCaseSensitive);

	string strValue = asString(bstrValue);

	switch (eMergeMethod)
	{
	case kKeepOriginal:
		{
			// Only add the new mapping if there is not already a value at this key
			if (m_mapKeyToValue.find(csisKey) == m_mapKeyToValue.end())
			{
				m_mapKeyToValue[csisKey] = strValue;
			}
		}
		break;

	case kOverwriteOriginal:
		{
			// Always add the new mapping regardless of whether there is already a value at this key
			m_mapKeyToValue[csisKey] = strValue;
		}
		break;

	case kAppend:
		{
			// Create a string to represent the base name of the key (excludes anything 
			// this function appends to the keyname)
			string strKeyBase(strKey);

			// Remove any previous addition from mergeKeyValue so that duplicate keys
			// will create the sequence "key", "key_1", "key_2" instead of 
			// "key", "key_1", "key_1_1"
			int nUnderscorePos = strKeyBase.find_last_of("_");
			if (nUnderscorePos != string::npos)
			{
				strKeyBase = strKeyBase.substr(0, nUnderscorePos);
				strKey = strKeyBase;
			}

			// If there is already a member at this key, append _[i] to the key name,
			// where i is iterated until we find a key that is not already in the map 
			for (int i = 1; i < gnMAX_MERGE_COUNT; i++)
			{
				csisKey = stringCSIS(strKey, m_bCaseSensitive);

				// Search the map for the key
				map<stringCSIS, string>::iterator it = m_mapKeyToValue.find(csisKey);

				// If we didn't find the key then we can break from the loop and set the
				// value
				if (it == m_mapKeyToValue.end())
				{
					break;
				}

				// If we already have a derivative of strKeyBase in the map whose value
				// matches the incoming value, we can go ahead and just return
				if (it->second == strValue)
				{
					return;
				}

				// Value doesn't match, create the new key and recheck
				strKey = strKeyBase + "_" + asString(i);
			}

			// Store the new value at the unique keyname we have created
			m_mapKeyToValue[csisKey] = strValue;
		}
		break;
	}
}
//-------------------------------------------------------------------------------------------------
void CStrToStrMap::clear()
{
	m_bCaseSensitive = true;
	m_mapKeyToValue.clear();

	m_bDirty = true;
}
//-------------------------------------------------------------------------------------------------
void CStrToStrMap::validateLicense()
{
	static const unsigned long STRTOSTRMAP_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( STRTOSTRMAP_COMPONENT_ID, "ELI04252", "StrToStrMap" );
}
//-------------------------------------------------------------------------------------------------
