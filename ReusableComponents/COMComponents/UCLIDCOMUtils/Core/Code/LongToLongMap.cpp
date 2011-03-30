// LongToLongMap.cpp : Implementation of CLongToLongMap
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "LongToLongMap.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
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
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CLongToLongMap
//-------------------------------------------------------------------------------------------------
CLongToLongMap::CLongToLongMap()
: m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CLongToLongMap::~CLongToLongMap()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16510");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILongToLongMap,
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
// ILongToLongMap
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::Set(long key, long value)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_mapKeyToValue[key] = value;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10638");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::GetValue(long key, long *pValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		map<long, long>::iterator it = m_mapKeyToValue.find(key);
		if (it != m_mapKeyToValue.end())
		{
			*pValue = it->second;
		}
		else
		{
			UCLIDException ue("ELI10639", "Map does not contain the specific key!");
			ue.addDebugInfo("Key", key);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10640");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::Contains(long key, VARIANT_BOOL *bFound)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		*bFound = VARIANT_FALSE;

		map<long, long>::iterator it = m_mapKeyToValue.find(key);
		if (it != m_mapKeyToValue.end())
		{
			*bFound = VARIANT_TRUE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10641");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::RemoveItem(long key)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		map<long, long>::iterator it = m_mapKeyToValue.find(key);
		if (it != m_mapKeyToValue.end())
		{
			m_mapKeyToValue.erase(it);

			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10642");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_mapKeyToValue.clear();

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10643");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::GetKeys(IVariantVector **pKeys)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		UCLID_COMUTILSLib::IVariantVectorPtr ipKeys(CLSID_VariantVector);
		if (ipKeys == __nullptr)
		{
			throw UCLIDException("ELI10644", "Unable to create VariantVector object!");
		}

		map<long, long>::iterator it = m_mapKeyToValue.begin();
		for (; it != m_mapKeyToValue.end(); it++)
		{
			ipKeys->PushBack(it->first);
		}

		CComQIPtr<IVariantVector> ipVec(ipKeys);
		*pKeys = ipVec.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10645");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::get_Size(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		*pVal = m_mapKeyToValue.size();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10646");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::GetKeyValue(long nIndex, long *pKey, long *pValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate the license first
		validateLicense();
		
		// ensure that the index is valid
		if ((unsigned long) nIndex >= m_mapKeyToValue.size())
		{
			UCLIDException ue("ELI10647", "Invalid map index!");
			ue.addDebugInfo("nIndex", nIndex);
			ue.addDebugInfo("Map size", m_mapKeyToValue.size());
			throw ue;
		}

		// find the entry of the map at the specified index and return the key/value
		map<long, long>::iterator iter = m_mapKeyToValue.begin();
		for (int i = 0; i < nIndex; i++)
		{
			iter++;
		}

		*pKey = iter->first;
		*pValue = iter->second;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10648");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::RenameKey(long key, long newKeyName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate the license first
		validateLicense();
		
		
		// only process this request if the new key and the old key are different
		if (key != newKeyName)
		{
			// ensure that the specified key is valid
			map<long, long>::iterator iter1 = m_mapKeyToValue.find(key);
			if (iter1 == m_mapKeyToValue.end())
			{
				UCLIDException ue("ELI10649", "Invalid map key!");
				ue.addDebugInfo("Key", key);
				throw ue;
			}

			// ensure that the new key is not already in the map
			map<long, long>::iterator iter2 = m_mapKeyToValue.find(newKeyName);
			if (iter2 != m_mapKeyToValue.end())
			{
				UCLIDException ue("ELI10650", "Specified key already exists in map!");
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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10651");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CLongToLongMap::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_LongToLongMap;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// clear the internal map
		m_mapKeyToValue.clear();
		
		// read the number of entries to the stream
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
			UCLIDException ue( "ELI10652", "Unable to load newer LongToLongMap." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> nNumEntries;
		}

		// for each map entry, read from the stream the name of the entry
		// as well as the value 
		for (int i = 0; i < nNumEntries; i++)
		{
			// read the name from the stream
			long key;
			dataReader >> key;
			// read the name from the stream
			long value;
			dataReader >> value;

			// store the key/value pair to the map
			m_mapKeyToValue[key] = value;
		}

		// set the dirty flag to false as we've just loaded the object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10653");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::Save(IStream *pStream, BOOL fClearDirty)
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

		// for each map entry, write to the stream the name of the entry
		// and the value
		map<long, long>::iterator iter;
		for (iter = m_mapKeyToValue.begin(); iter != m_mapKeyToValue.end(); iter++)
		{
			// write the key to the stream
			long key = iter->first;
			dataWriter << key;

			// write the value to the stream
			long value = iter->second;
			dataWriter << value;
		}


		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		

		// clear the flag as specified
		if (fClearDirty) m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10654");


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the other map object
		UCLID_COMUTILSLib::ILongToLongMapPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08210", ipSource != __nullptr);

		Clear();

		int i = 0;
		for(i = 0; i < ipSource->GetSize(); i++)
		{
			long key, value;
			ipSource->GetKeyValue(i, &key, &value);
			m_mapKeyToValue[key] = value;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10655");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLongToLongMap::Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a new map
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_LongToLongMap);
		ASSERT_RESOURCE_ALLOCATION("ELI08376", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new map to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05394");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper function
//-------------------------------------------------------------------------------------------------
void CLongToLongMap::validateLicense()
{
	static const unsigned long LONGTOLONGMAP_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( LONGTOLONGMAP_COMPONENT_ID, "ELI10656", "LongToLongMap" );
}
//-------------------------------------------------------------------------------------------------
