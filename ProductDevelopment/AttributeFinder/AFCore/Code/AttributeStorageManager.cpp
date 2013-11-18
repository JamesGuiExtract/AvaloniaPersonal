#include "stdafx.h"
#include "AFCore.h"
#include "AttributeStorageManager.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CAttributeStorageManager
//--------------------------------------------------------------------------------------------------
CAttributeStorageManager::CAttributeStorageManager()
: m_bDirty(false)
{
	try
	{
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36320");
}
//--------------------------------------------------------------------------------------------------
CAttributeStorageManager::~CAttributeStorageManager()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36321");
}
//-------------------------------------------------------------------------------------------------
HRESULT CAttributeStorageManager::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CAttributeStorageManager::FinalRelease()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36322");
}

//-------------------------------------------------------------------------------------------------
// IStorageManager
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeStorageManager::raw_PrepareForStorage(IIUnknownVector *pDataToStore)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pDataToStore);
		ASSERT_ARGUMENT("ELI36323", ipAttributes != __nullptr);
		
		m_mapPageInfosToPersist.clear();

		// Populate m_mapPageInfosToPersist with a map of all unique page info instances to the
		// indexs of the attribute(s) that use each.
		long nAttributeIndex = 0;
		prepareAttributesForStorage(ipAttributes, nAttributeIndex);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36324");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeStorageManager::raw_InitFromStorage(IIUnknownVector *pDataToInit)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pDataToInit);
		ASSERT_ARGUMENT("ELI36325", ipAttributes != __nullptr);

		// Uses m_mapLoadedPageInfos to assign the correct page info instance to each loaded
		// attribute from the page infos that were loaded from this VOA file.
		long nAttributeIndex = 0;
		initAttributesFromStorage(ipAttributes, nAttributeIndex);
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36326");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeStorageManager::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI36327", pbValue != __nullptr);

		try
        {
            // Check license
            validateLicense();

            // If no exception, then pbValue is true
            *pbValue = VARIANT_TRUE;
        }
        catch (...)
        {
            *pbValue = VARIANT_FALSE;
        }

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36328");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeStorageManager::GetClassID(CLSID* pClassID)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    *pClassID = CLSID_AttributeStorageManager;

    return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeStorageManager::IsDirty(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeStorageManager::Load(IStream* pStream)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

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

        // Check for newer version
        if (nDataVersion > gnCurrentVersion)
        {
            UCLIDException ue("ELI36329", 
                "Unable to load newer attribute storage manager.");
            ue.addDebugInfo("Current Version", gnCurrentVersion);
            ue.addDebugInfo("Version to Load", nDataVersion);
            throw ue;
        }

		m_mapLoadedPageInfos.clear();

		long nPageInfoCount = 0;
		dataReader >> nPageInfoCount;

		// Populate a map that ties the index of each page info instance (yet to be loaded) to the
		// indexes of the attribute(s) that will use them.
		map<long, set<long>> mapLoadedIndexes;
		for (long nPageInfoIndex = 0; nPageInfoIndex < nPageInfoCount; nPageInfoIndex++)
		{
			long nIndexCount = 0;
			dataReader >> nIndexCount;

			// For each page info instance to be loaded, read the indexes of the attributes that
			// will use it.
			for (long i = 0; i < nIndexCount; i++)
			{
				long nAttributeIndex = 0;
				dataReader >> nAttributeIndex;

				mapLoadedIndexes[nPageInfoIndex].insert(nAttributeIndex);
			}
		}

		// Now load each page info from disk and use mapLoadedIndexes to associate it with the
		// correct attribute indexes in m_mapLoadedPageInfos.
		for (long nPageInfoIndex = 0; nPageInfoIndex < nPageInfoCount; nPageInfoIndex++)
		{
			IPersistStreamPtr ipObj;
			readObjectFromStream(ipObj, pStream, "ELI36330");
			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI36331", "Unable to read object from stream!");
			}

			ILongToObjectMapPtr ipPageInfos = ipObj;
			if (ipPageInfos == __nullptr)
			{
				throw UCLIDException("ELI36332", "Loaded object was of an unexpected type.");
			}

			// Assign the set of attribute indexes to the loaded ipPageInfos instance in
			// m_mapLoadedPageInfos.
			set<long> &setAttributeIndexes = mapLoadedIndexes[nPageInfoIndex];
			for (set<long>::iterator iter = setAttributeIndexes.begin();
				 iter != setAttributeIndexes.end(); iter++)
			{
				m_mapLoadedPageInfos[*iter] = ipPageInfos;
			}
		}

        // Clear the dirty flag as we've loaded a fresh object
        m_bDirty = false;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36333");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeStorageManager::Save(IStream* pStream, BOOL fClearDirty)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license state
        validateLicense();

        // Create a bytestream and stream this object's data into it
        ByteStream data;
        ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
        dataWriter << gnCurrentVersion;

		long nPageInfoCount = m_mapPageInfosToPersist.size();
		dataWriter << nPageInfoCount;
		
		// Iterate each page info instance in m_mapPageInfosToPersist to stream the indexes of the
		// attribute(s) that use it.
		for (map<ILongToObjectMapPtr, set<long>>::iterator iter = m_mapPageInfosToPersist.begin();
			 iter != m_mapPageInfosToPersist.end(); iter++)
		{
			set<long> &setAttributeIndexes = iter->second;

			long nAttributeCount = setAttributeIndexes.size();
			dataWriter << nAttributeCount;

			for (set<long>::iterator iterIndexes = setAttributeIndexes.begin();
				 iterIndexes != setAttributeIndexes.end(); iterIndexes++)
			{
				dataWriter << *iterIndexes;
			}
		}

		dataWriter.flushToByteStream();
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// After writing the indexes to the stream, write the page info instances.
		for (map<ILongToObjectMapPtr, set<long>>::iterator iter = m_mapPageInfosToPersist.begin();
			 iter != m_mapPageInfosToPersist.end(); iter++)
		{
			IPersistStreamPtr ipObj = iter->first;

			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI36334", "Object in vector does not support persistence!");
			}

			writeObjectToStream(ipObj, pStream, "ELI36335", fClearDirty);
		}

        // Clear the flag as specified
        if (fClearDirty)
        {
            m_bDirty = false;
        }

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36336");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeStorageManager::GetSizeMax(ULARGE_INTEGER* pcbSize)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())
    
    return E_NOTIMPL;
}

//---------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//---------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeStorageManager::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IStorageManager,
			&IID_ILicensedComponent,
			&IID_IPersistStream,		
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36337")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CAttributeStorageManager::prepareAttributesForStorage(IIUnknownVectorPtr ipAttributes,
	long &rnAttributeIndex)
{
	// Iterate through each attribute in the vector in order to populate m_mapPageInfosToPersist
	// with the unique spatial page info instances that need to be streamed.
	long nSize = ipAttributes->Size();
	for (long i = 0; i < nSize; i++)
	{
		UCLID_AFCORELib::IAttributePtr ipAttribute = ipAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI36338", ipAttribute != __nullptr);

		ISpatialStringPtr ipValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI36339", ipValue != __nullptr);

		// Ignore any non-spatial attributes.
		if (asCppBool(ipValue->HasSpatialInfo()))
		{
			ILongToObjectMapPtr ipSpatialPageInfos = ipValue->SpatialPageInfos;
			ASSERT_RESOURCE_ALLOCATION("ELI36340", ipSpatialPageInfos != __nullptr);

			m_mapPageInfosToPersist[(ILongToObjectMap*)ipSpatialPageInfos].insert(rnAttributeIndex++);

			// Clear the SpatialPageInfo map to be persisted so to prevent the same spatial page
			// infos from being persisted multiple times.
			ipValue->SpatialPageInfos = ILongToObjectMapPtr(CLSID_LongToObjectMap);
		}

		// Prepare any sub-attributes as well.
		prepareAttributesForStorage(ipAttribute->SubAttributes, rnAttributeIndex);
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeStorageManager::initAttributesFromStorage(IIUnknownVectorPtr ipAttributes,
	long &rnAttributeIndex)
{
	// Iterate through all loaded attributes and assign the correct page info instance loaded
	// from the VOA file.
	long nSize = ipAttributes->Size();
	for (long i = 0; i < nSize; i++)
	{
		UCLID_AFCORELib::IAttributePtr ipAttribute = ipAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI36341", ipAttribute != __nullptr);

		ISpatialStringPtr ipValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI36342", ipValue != __nullptr);

		// Ignore any non-spatial attributes.
		if (asCppBool(ipValue->HasSpatialInfo()))
		{
			ILongToObjectMapPtr ipSpatialPageInfos = m_mapLoadedPageInfos[rnAttributeIndex++];
			ASSERT_RESOURCE_ALLOCATION("ELI36343", ipSpatialPageInfos != __nullptr);

			ipValue->SpatialPageInfos = ipSpatialPageInfos;
		}

		initAttributesFromStorage(ipAttribute->SubAttributes, rnAttributeIndex);
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeStorageManager::validateLicense()
{
	VALIDATE_LICENSE(gnRULE_WRITING_CORE_OBJECTS, "ELI36344", "AttributeStorageManager");
}