
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "LongToObjectMap.h"

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
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CLongToObjectMap
//-------------------------------------------------------------------------------------------------
CLongToObjectMap::CLongToObjectMap()
: m_bDirty(false)
, m_bReadonly(false)
{
}
//-------------------------------------------------------------------------------------------------
CLongToObjectMap::~CLongToObjectMap()
{
	try
	{
		m_mapKeyToValue.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16511");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILongToObjectMap,
		&IID_ILicensedComponent,
		&IID_IShallowCopyable,
		&IID_ICopyableObject
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
STDMETHODIMP CLongToObjectMap::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_LongToObjectMap;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		HRESULT hr = m_bDirty ? S_OK : S_FALSE;
		if (!m_bDirty)
		{
			// iterate through the map and find out for each object
			map<long, IUnknownPtr>::iterator itMap = m_mapKeyToValue.begin();
			for (; itMap != m_mapKeyToValue.end(); itMap++)
			{
				IPersistStreamPtr ipPersistStream(itMap->second);
				if (ipPersistStream == __nullptr)
				{
					UCLIDException ue("ELI04776", "Object does not support persistence!");
					ue.addDebugInfo("Key", itMap->first);
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04775")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// clear the internal map
		m_mapKeyToValue.clear();
		
		// Clear the variables first
		long nNumEntries = 0;

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
			UCLIDException ue( "ELI07668", "Unable to load newer LongToObjectMap." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> nNumEntries;
		}

		// for each map entry, read from the stream the name of the entry
		// as well as the value which is an unknown object
		for (int i = 0; i < nNumEntries; i++)
		{
			// read the key from the stream
			long nKey;
			pStream->Read(&nKey, sizeof(nKey), NULL);

			// read the object from the stream
			IPersistStreamPtr ipObj;
			readObjectFromStream(ipObj, pStream, "ELI09980");

			// write the name/value pair to the map
			m_mapKeyToValue[nKey] = ipObj;
		}

		// set the dirty flag to false as we've just loaded the object
		m_bDirty = false;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04698");	
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;

		// write the number of entries to the stream
		long nNumEntries = m_mapKeyToValue.size();
		dataWriter << nNumEntries;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// for each map entry, write to the stream the name of the entry
		// and the value object associated with that entry
		std::map<long, IUnknownPtr>::const_iterator iter;
		for (iter = m_mapKeyToValue.begin(); iter != m_mapKeyToValue.end(); iter++)
		{
		
			// write the key value to the stream
			pStream->Write(&(iter->first), sizeof(iter->first), NULL);
			// write the the object to the stream
			IPersistStreamPtr ipObj = iter->second;
			if (ipObj == __nullptr)
			{
				UCLIDException ue("ELI04702", "Object does not support persistence!");
				ue.addDebugInfo("Key", iter->first);
				throw ue;
			}
			else
			{
				writeObjectToStream(ipObj, pStream, "ELI09935", fClearDirty);
			}
		}

		// clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04699");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IStrToObjectMap
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::Set(long key, IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25998", pObject != __nullptr);

		// validate license
		validateLicense();

		if (m_bReadonly)
		{
			throw UCLIDException("ELI36293", "Cannot modify readonly object map.");
		}
		
		// set value in map for specified key
		m_mapKeyToValue[key] = pObject;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04369");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::GetValue(long key, IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25999", pObject != __nullptr);

		// validate license
		validateLicense();
		
		// get value of specified entry in map
		map<long, IUnknownPtr>::iterator it = m_mapKeyToValue.find(key);
		if (it != m_mapKeyToValue.end())
		{
			IUnknownPtr ipShallowCopy = it->second;
			*pObject = ipShallowCopy.Detach();
		}
		else
		{
			UCLIDException ue("ELI04370", "Map does not contain the specific key!");
			ue.addDebugInfo("Key", key);
			throw ue;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04368");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::Contains(long key, VARIANT_BOOL *bFound)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26000", bFound != __nullptr);

		// validate license
		validateLicense();
		
		// return true if the specified key is found in the map
		*bFound = asVariantBool(m_mapKeyToValue.find(key) != m_mapKeyToValue.end());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04367");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::RemoveItem(long key)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		if (m_bReadonly)
		{
			throw UCLIDException("ELI36294", "Cannot modify readonly object map.");
		}
		
		// remove the specified entry from the map.
		// if it already doesn't exist in the map, then just return

		map<long, IUnknownPtr>::iterator it = m_mapKeyToValue.find(key);
		if (it != m_mapKeyToValue.end())
		{
			m_mapKeyToValue.erase(it);
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04366");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		if (m_bReadonly)
		{
			throw UCLIDException("ELI36295", "Cannot modify readonly object map.");
		}

		clear();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04365");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::GetKeys(IVariantVector **pKeys)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26004", pKeys != __nullptr);

		// validate license
		validateLicense();

		// create a variant vector of all the keys in the map
		UCLID_COMUTILSLib::IVariantVectorPtr ipKeys(CLSID_VariantVector);
		if (ipKeys == __nullptr)
		{
			throw UCLIDException("ELI04371", "Unable to create VariantVector object!");
		}

		map<long, IUnknownPtr>::iterator it = m_mapKeyToValue.begin();
		for (; it != m_mapKeyToValue.end(); it++)
		{
			ipKeys->PushBack(it->first);
		}

		*pKeys = (IVariantVector*) ipKeys.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04364");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::get_Size(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license and return size of current map
		validateLicense();
		*pVal = m_mapKeyToValue.size();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04363");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::GetKeyValue(long nIndex, long *pstrKey, IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26002", pstrKey != __nullptr);
		ASSERT_ARGUMENT("ELI26003", pObject != __nullptr);

		// validate license and return size of current map
		validateLicense();

		// ensure that the index is valid
		if ((unsigned long) nIndex >= m_mapKeyToValue.size())
		{
			UCLIDException ue("ELI04477", "Invalid map index!");
			ue.addDebugInfo("nIndex", nIndex);
			ue.addDebugInfo("Map size", m_mapKeyToValue.size());
			throw ue;
		}

		// find the entry of the map at the specified index and return the key/value
		map<long, IUnknownPtr>::iterator iter = m_mapKeyToValue.begin();
		for (int i = 0; i < nIndex; i++)
		{
			iter++;
		}

		*pstrKey = iter->first;
		IUnknownPtr ipShallowCopy = iter->second;
		*pObject = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04475");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::RenameKey(long key, long newKeyName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate the license first
		validateLicense();

		if (m_bReadonly)
		{
			throw UCLIDException("ELI36296", "Cannot modify readonly object map.");
		}
		
		// only process this request if the new key and the old key are different
		if (key != newKeyName)
		{
			// ensure that the specified key is valid
			map<long, IUnknownPtr>::iterator iter1 = m_mapKeyToValue.find(key);
			if (iter1 == m_mapKeyToValue.end())
			{
				UCLIDException ue("ELI04532", "Invalid map key!");
				ue.addDebugInfo("Key", key);
				throw ue;
			}

			// ensure that the new key is not already in the map
			map<long, IUnknownPtr>::iterator iter2 = m_mapKeyToValue.find(newKeyName);
			if (iter2 != m_mapKeyToValue.end())
			{
				UCLIDException ue("ELI04533", "Specified key already exists in map!");
				ue.addDebugInfo("Old Key", key);
				ue.addDebugInfo("New Key", newKeyName);
				throw ue;
			}

			// add the new entry to the map
			m_mapKeyToValue[newKeyName] = iter1->second;

			// remove the original entry from the map
			m_mapKeyToValue.erase(iter1);

			m_bDirty = true;
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04534");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::SetReadonly()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate the license first
		validateLicense();

		m_bReadonly = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36297");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CLongToObjectMap::CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		UCLID_COMUTILSLib::ILongToObjectMapPtr ipSource( pObject );
		ASSERT_RESOURCE_ALLOCATION("ELI08212", ipSource != __nullptr);

		if (m_bReadonly)
		{
			throw UCLIDException("ELI36298", "Cannot modify readonly object map.");
		}

		// Clear this map object
		clear();

		UCLID_COMUTILSLib::IVariantVectorPtr ipKeys = ipSource->GetKeys();
		long lSize = ipKeys->Size;
		for(long i = 0; i < lSize; i++)
		{
			long key = (long)ipKeys->Item[i];
			IUnknownPtr ipUnk = ipSource->GetValue(key);

			// If this object can be cloned then clone it
			UCLID_COMUTILSLib::ICopyableObjectPtr ipValue = ipUnk;
			if (ipValue != __nullptr)
			{
				IUnknownPtr ipCopy = ipValue->Clone();
				ASSERT_RESOURCE_ALLOCATION("ELI25893", ipCopy != __nullptr);
				m_mapKeyToValue[key] = ipCopy;
			}
			else
			{
				// If the object does not support ICopyable just make a shallow copy
				m_mapKeyToValue[key] = ipUnk;
			}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08213");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26001", pObject != __nullptr);

		// Check license state
		validateLicense();

		// Create a new map
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy(CLSID_LongToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI08374", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new map to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05026");
}

//-------------------------------------------------------------------------------------------------
// IShallowCopyable
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToObjectMap::ShallowCopy(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25919", pObject != __nullptr);

		UCLID_COMUTILSLib::ILongToObjectMapPtr ipNewMap(CLSID_LongToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI25920", ipNewMap != __nullptr);

		// Just copy the map values from this object to the new object
		for (map<long, IUnknownPtr>::iterator it = m_mapKeyToValue.begin();
			it != m_mapKeyToValue.end(); it++)
		{
			ipNewMap->Set(it->first, it->second);
		}

		*pObject = ipNewMap.Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25731");
}

//-------------------------------------------------------------------------------------------------
// Helper function
//-------------------------------------------------------------------------------------------------
void CLongToObjectMap::validateLicense()
{
	static const unsigned long LONGTOOBJECTMAP_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( LONGTOOBJECTMAP_COMPONENT_ID, "ELI04362", "LongToObjectMap" );
}
//-------------------------------------------------------------------------------------------------
void CLongToObjectMap::clear()
{
	try
	{
		// empty the map.  This will also cause the reference count of
		// the associated objects in the map to be decremented.
		m_mapKeyToValue.clear();

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25997");
}
//-------------------------------------------------------------------------------------------------