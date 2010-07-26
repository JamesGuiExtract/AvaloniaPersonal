
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "StrToObjectMap.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <ComponentLicenseIDs.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CStrToObjectMap
//-------------------------------------------------------------------------------------------------
CStrToObjectMap::CStrToObjectMap()
:	m_bDirty(false),
	m_bCaseSensitive(true)
{
}
//-------------------------------------------------------------------------------------------------
CStrToObjectMap::~CStrToObjectMap()
{
	try
	{
		// Set each IUnknownPtr in the map to NULL
		for (map<stringCSIS, IUnknownPtr>::iterator it = m_mapKeyToValue.begin();
			it != m_mapKeyToValue.end(); it++)
		{
			it->second = NULL;
		}
		m_mapKeyToValue.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16518");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IStrToObjectMap,
		&IID_ICopyableObject,
		&IID_IShallowCopyable,
		&IID_ILicensedComponent,
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
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_StrToObjectMap;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		HRESULT hr = m_bDirty ? S_OK : S_FALSE;
		if (!m_bDirty)
		{
			// iterate through the map and find out for each object
			map<stringCSIS, IUnknownPtr>::iterator itMap = m_mapKeyToValue.begin();
			for (; itMap != m_mapKeyToValue.end(); itMap++)
			{
				IPersistStreamPtr ipPersistStream(itMap->second);
				if (ipPersistStream == NULL)
				{
					UCLIDException ue("ELI19309", "Object does not support persistence!");
					ue.addDebugInfo("Key", static_cast<string>(itMap->first));
					throw ue;
				}

				hr = ipPersistStream->IsDirty();
				if (hr == S_OK)
				{
					// once there's a dirty object, break out of the loop 
					break;
				}
			}
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19308")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28408", pStream != NULL);

		// clear the internal map
		m_mapKeyToValue.clear();

		// Clear the variables first
		long nNumEntries = 0;

		// Set case sensitive flag to default value
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
			UCLIDException ue( "ELI19343", "Unable to load newer StringToObjectMap." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> nNumEntries;
		}
		if ( nDataVersion >= 2 )
		{
			dataReader >> m_bCaseSensitive;
		}

		// for each map entry, read from the stream the name of the entry
		// as well as the value which is an unknown object
		for (int i = 0; i < nNumEntries; i++)
		{
			// read the name from the stream
			CComBSTR bstrName;
			bstrName.ReadFromStream(pStream);

			// read the object from the stream
			IPersistStreamPtr ipObj;
			readObjectFromStream(ipObj, pStream, "ELI09982");

			// write the name/value pair to the map
			stringCSIS stdstrKey( asString(bstrName), m_bCaseSensitive);
			m_mapKeyToValue[stdstrKey] = ipObj;
		}

		// set the dirty flag to false as we've just loaded the object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19304");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28409", pStream != NULL);

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
		// and the value object associated with that entry
		std::map<stringCSIS, IUnknownPtr>::const_iterator iter;
		for (iter = m_mapKeyToValue.begin(); iter != m_mapKeyToValue.end(); iter++)
		{
			// write the name to the stream
			CComBSTR bstrName = iter->first.c_str();
			bstrName.WriteToStream(pStream);

			// write the the object to the stream
			IPersistStreamPtr ipObj = iter->second;
			if (ipObj == NULL)
			{
				UCLIDException ue("ELI19306", "Object does not support persistence!");
				ue.addDebugInfo("Key", static_cast<string>(iter->first));
				throw ue;
			}
			else
			{
				writeObjectToStream(ipObj, pStream, "ELI09937", fClearDirty);
			}
		}

		// clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19305");


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IStrToObjectMap
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::Set(BSTR key, IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// set value in map for specified key
		stringCSIS stdstrKey ( asString (key), m_bCaseSensitive );
		m_mapKeyToValue[stdstrKey] = pObject;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19295");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::GetValue(BSTR key, IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI28410", pObject != NULL);

		// get value of specified entry in map
		stringCSIS stdstrKey ( asString ( key ), m_bCaseSensitive );

		map<stringCSIS, IUnknownPtr>::iterator it = m_mapKeyToValue.find(stdstrKey);
		if (it != m_mapKeyToValue.end())
		{
			IUnknownPtr ipShallowCopy = it->second;
			*pObject = ipShallowCopy.Detach();
		}
		else
		{
			UCLIDException ue("ELI11225", "Map does not contain the specific key!");
			ue.addDebugInfo("Key", static_cast<string>(stdstrKey));
			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11226");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::Contains(BSTR key, VARIANT_BOOL *bFound)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26041", bFound != NULL);

		// validate license
		validateLicense();

		stringCSIS stdstrKey ( asString(key), m_bCaseSensitive );

		// return true if the specified key is found in the map
		*bFound = asVariantBool(m_mapKeyToValue.find(stdstrKey) != m_mapKeyToValue.end());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19294");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::RemoveItem(BSTR key)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// remove the specified entry from the map.
		// if it already doesn't exist in the map, then just return
		stringCSIS stdstrKey ( asString (key), m_bCaseSensitive );

		map<stringCSIS, IUnknownPtr>::iterator it = m_mapKeyToValue.find(stdstrKey);
		if (it != m_mapKeyToValue.end())
		{
			m_mapKeyToValue.erase(it);
			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19293");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		clear();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19292");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::GetKeys(IVariantVector **pKeys)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26042", pKeys != NULL);

		// validate license
		validateLicense();

		// create a variant vector of all the keys in the map
		UCLID_COMUTILSLib::IVariantVectorPtr ipKeys(CLSID_VariantVector);
		if (ipKeys == NULL)
		{
			throw UCLIDException("ELI19296", "Unable to create VariantVector object!");
		}

		map<stringCSIS, IUnknownPtr>::iterator it = m_mapKeyToValue.begin();
		for (; it != m_mapKeyToValue.end(); it++)
		{
			ipKeys->PushBack(_bstr_t(it->first.c_str()));
		}

		*pKeys = (IVariantVector*) ipKeys.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19291");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::get_Size(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26043", pVal != NULL);

		// validate license and return size of current map
		validateLicense();
		*pVal = m_mapKeyToValue.size();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19290");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::GetKeyValue(long nIndex, BSTR *pstrKey, IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26044", pstrKey != NULL);
		ASSERT_ARGUMENT("ELI26045", pObject != NULL);

		// validate license and return size of current map
		validateLicense();

		// ensure that the index is valid
		if ((unsigned long) nIndex >= m_mapKeyToValue.size())
		{
			UCLIDException ue("ELI15347", "Invalid map index!");
			ue.addDebugInfo("nIndex", nIndex);
			ue.addDebugInfo("Map size", m_mapKeyToValue.size());
			throw ue;
		}

		// find the entry of the map at the specified index and return the key/value
		map<stringCSIS, IUnknownPtr>::iterator iter = m_mapKeyToValue.begin();
		for (long i=0; i < nIndex; i++)
		{
			iter++;
		}

		*pstrKey = _bstr_t(iter->first.c_str()).Detach();
		IUnknownPtr ipShallowCopy = iter->second;
		*pObject = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19297");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::RenameKey(BSTR strKey, BSTR strNewKeyName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate the license first
		validateLicense();
		
		stringCSIS stdstrKey ( asString(strKey), m_bCaseSensitive );
		stringCSIS stdstrNewKeyName (asString(strNewKeyName), m_bCaseSensitive );
		
		// only process this request if the new key and the old key are different
		if (stdstrKey != stdstrNewKeyName)
		{
			// ensure that the specified key is valid
			map<stringCSIS, IUnknownPtr>::iterator iter1 = m_mapKeyToValue.find(stdstrKey);
			if (iter1 == m_mapKeyToValue.end())
			{
				UCLIDException ue("ELI19298", "Invalid map key!");
				ue.addDebugInfo("Key", static_cast<string>(stdstrKey) );
				throw ue;
			}

			// ensure that the new key is not already in the map
			map<stringCSIS, IUnknownPtr>::iterator iter2 = m_mapKeyToValue.find(stdstrNewKeyName);
			if (iter2 != m_mapKeyToValue.end())
			{
				UCLIDException ue("ELI19299", "Specified key already exists in map!");
				ue.addDebugInfo("Old Key", static_cast<string>(stdstrKey));
				ue.addDebugInfo("New Key", static_cast<string>(stdstrNewKeyName));
				throw ue;
			}

			// add the new entry to the map
			m_mapKeyToValue[stdstrNewKeyName] = iter1->second;

			// remove the original entry from the map
			m_mapKeyToValue.erase(iter1);

			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19300");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::get_CaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26046", pVal != NULL);

		// validate license 
		validateLicense();

		*pVal = asVariantBool(m_bCaseSensitive);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10475");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::put_CaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// Check for change of case sensitive to case insensitive
		bool bNewCaseSensitivity = asCppBool(newVal);
		if ( (m_bCaseSensitive != bNewCaseSensitivity))
		{
			// If changing sensitivity, clear the map
			clear();
		}
		m_bCaseSensitive = bNewCaseSensitivity;
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10476");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::TryGetValue(BSTR bstrKey, IUnknown **ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI26398", ppObject != NULL);

		validateLicense();

		stringCSIS strKey(asString(bstrKey), m_bCaseSensitive);

		// Search for the key
		map<stringCSIS, IUnknownPtr>::iterator it = m_mapKeyToValue.find(strKey);
		if (it != m_mapKeyToValue.end())
		{
			// If the map contains the key, then return the object
			IUnknownPtr ipShallowCopy = it->second;
			*ppObject = ipShallowCopy.Detach();
		}
		else
		{
			// Map does not contain key, return NULL
			*ppObject = NULL;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26399");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the other map object
		UCLID_COMUTILSLib::IStrToObjectMapPtr ipSource( pObject );
		ASSERT_RESOURCE_ALLOCATION("ELI19350", ipSource != NULL);
		clear();

		// Set Casesensitivity to same as source
		m_bCaseSensitive = asCppBool(ipSource->CaseSensitive);

		long lSize = ipSource->Size;
		for(long i = 0; i < lSize; i++)
		{
			CComBSTR bstrKey;
			IUnknownPtr ipUnk;
			ipSource->GetKeyValue(i, &bstrKey, &ipUnk);

			UCLID_COMUTILSLib::ICopyableObjectPtr ipCopier(ipUnk);
			ASSERT_RESOURCE_ALLOCATION("ELI26047", ipCopier != NULL);

			// Create a deep copy of the objects
			ipUnk = ipCopier->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI26048", ipUnk != NULL);

			m_mapKeyToValue[ stringCSIS( asString(bstrKey), m_bCaseSensitive )] = ipUnk;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19351");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a new map
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_StrToObjectMap );
		ASSERT_RESOURCE_ALLOCATION("ELI19354", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new map to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19311");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IShallowCopyable
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStrToObjectMap::ShallowCopy(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26049", pObject != NULL);

		// Check license state
		validateLicense();

		UCLID_COMUTILSLib::IStrToObjectMapPtr ipNewMap(CLSID_StrToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI26050", ipNewMap != NULL);

		// Set Casesensitivity to same as this
		ipNewMap->CaseSensitive = asVariantBool(m_bCaseSensitive);

		// Shallow copy this map to the new map
		for (map<stringCSIS, IUnknownPtr>::iterator it = m_mapKeyToValue.begin();
			it != m_mapKeyToValue.end(); it++)
		{
			ipNewMap->Set(it->first.c_str(), it->second);
		}

		// Return the new map
		*pObject = ipNewMap.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25732");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CStrToObjectMap::validateLicense()
{
	static const unsigned long STRTOOBJECTMAP_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( STRTOOBJECTMAP_COMPONENT_ID, "ELI19289", "StrToObjectMap" );
}
//-------------------------------------------------------------------------------------------------
void CStrToObjectMap::clear()
{
	try
	{
		// empty the map.  This will also cause the reference count of
		// the associated objects in the map to be decremented.
		m_mapKeyToValue.clear();

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26051");
}
//-------------------------------------------------------------------------------------------------
