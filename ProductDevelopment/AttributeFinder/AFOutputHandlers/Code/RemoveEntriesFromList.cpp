// RemoveEntriesFromList.cpp : Implementation of CRemoveEntriesFromList

#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "RemoveEntriesFromList.h"

#include <UCLIDException.h>
#include <CommentedTextFileReader.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

#include <fstream>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CRemoveEntriesFromList
//-------------------------------------------------------------------------------------------------
CRemoveEntriesFromList::CRemoveEntriesFromList()
: m_ipEntriesList(CLSID_VariantVector),
  m_bDirty(false),
  m_bCaseSensitive(false)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI06760", m_ipEntriesList!=NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI06761")
}
//-------------------------------------------------------------------------------------------------
CRemoveEntriesFromList::~CRemoveEntriesFromList()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16315");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOutputHandler,
		&IID_IRemoveEntriesFromList,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
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
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::raw_ProcessOutput(IIUnknownVector* pAttributes,
													   IAFDocument *pAFDoc,
													   IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// Validate license first
		validateLicense();

		IIUnknownVectorPtr ipOrignAttributes(pAttributes);
		ASSERT_ARGUMENT("ELI06772", ipOrignAttributes != __nullptr);
		// create an empty vector
		IIUnknownVectorPtr ipReturnAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI06770", ipReturnAttributes != __nullptr);

		// Get a list of values that includes values from any specified files.
		IVariantVectorPtr ipExpandedEntriesList =
			m_cachedListLoader.expandList(m_ipEntriesList, pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI30064", ipExpandedEntriesList != __nullptr);

		// go through all attributes and valid them
		long nSize = ipOrignAttributes->Size();
		for (long n=0; n<nSize; n++)
		{
			bool bFoundMatch = false;
			IAttributePtr ipAttr(ipOrignAttributes->At(n));
			ASSERT_RESOURCE_ALLOCATION("ELI06771", ipAttr != __nullptr);

			// Retrieve the value
			ISpatialStringPtr ipValue = ipAttr->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI15527", ipValue != __nullptr);

			// if the attribute value is one of the entries
			// in the list, do not add it to the return list
			string strFoundValue = ipValue->String;
			long nSize = ipExpandedEntriesList->Size;
			for (long m = 0; m < nSize; m++)
			{
				string strEntryToBeRemoved = asString(_bstr_t(ipExpandedEntriesList->GetItem(m)));
				if (!m_bCaseSensitive)
				{
					::makeUpperCase(strFoundValue);
					::makeUpperCase(strEntryToBeRemoved);
				}
				if (strFoundValue == strEntryToBeRemoved)
				{
					// found this value exists in the entry list
					bFoundMatch = true;
					// break out of the inner for loop
					break;
				}
			}
			
			if (bFoundMatch)
			{
				// go to next item
				continue;
			}
			ipReturnAttributes->PushBack(ipAttr);
		}

		// clear the in/out vector
		ipOrignAttributes->Clear();
		long nReturnSize = ipReturnAttributes->Size();
		// Fill it with the values we want to return
		int i;
		for(i = 0; i < nReturnSize; i++)
		{
			ipOrignAttributes->PushBack(ipReturnAttributes->At(i));
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06773")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IRemoveEntriesFromList
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06774");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		m_bCaseSensitive = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06775");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::get_EntryList(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Provide list to caller
		IVariantVectorPtr ipShallowCopy = m_ipEntriesList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06776");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::put_EntryList(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Set old object to null
		if (m_ipEntriesList != __nullptr) 
		{
			m_ipEntriesList = __nullptr;
		}

		// Store new list
		m_ipEntriesList = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06777");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::LoadEntriesFromFile(BSTR strFileFullName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Clear contents of list
		m_ipEntriesList->Clear();
		
		string strFileName = asString(strFileFullName);
		ifstream ifs(strFileName.c_str());
		CommentedTextFileReader fileReader(ifs, "//", true);
		string strLine("");
		while (!ifs.eof())
		{
			strLine = fileReader.getLineText();
			if (!strLine.empty())
			{
				// Add this item to the list
				m_ipEntriesList->PushBack( _bstr_t( strLine.c_str() ) );
			}
		};

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06778");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::SaveEntriesToFile(BSTR strFileFullName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		string strFileToSave = asString( strFileFullName );

		// Always overwrite if the file exists
		ofstream ofs( strFileToSave.c_str(), ios::out | ios::trunc );
		
		// Iterate through the vector
		string strValue("");
		long nSize = m_ipEntriesList->Size;
		for (long n = 0; n < nSize; n++)
		{
			// Retrieve this entry
			strValue = asString(_bstr_t(m_ipEntriesList->GetItem(n)));

			// Save the entry to the file
			ofs << strValue << endl;
		}

		// Close the stream
		ofs.close();
		waitForFileToBeReadable(strFileToSave);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06779");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19551", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Remove entries from list").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12818");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_RemoveEntriesFromList;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_bCaseSensitive = false;

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
			UCLIDException ue( "ELI07771", 
				"Unable to load newer RemoveEntriesFromList Output Handler!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if(nDataVersion > 1)
		{
			dataReader >> m_bCaseSensitive;
		}

		// Read the Entries list
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI09959");
		if (ipObj == __nullptr)
		{
			throw UCLIDException( "ELI07774", 
				"Variant Vector for Entries List object could not be read from stream!" );
		}
		else
		{
			m_ipEntriesList = ipObj;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07772");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_bCaseSensitive;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Separately write the Entries list to the IStream object
		IPersistStreamPtr ipPersistentObj( m_ipEntriesList );
		if (ipPersistentObj == __nullptr)
		{
			throw UCLIDException( "ELI07775", 
				"Remove Entries From List object does not support persistence!" );
		}
		else
		{
			::writeObjectToStream( ipPersistentObj, pStream, "ELI09914", fClearDirty );
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07773");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CRemoveEntriesFromList::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		UCLID_AFOUTPUTHANDLERSLib::IRemoveEntriesFromListPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08229", ipSource != __nullptr);

		// Copy list of strings
		// Not sure if there should be an assert here
		ICopyableObjectPtr ipObjectEntryList = ipSource->GetEntryList();
		if (ipObjectEntryList)
		{
			m_ipEntriesList = ipObjectEntryList->Clone();
		}

		// Copy case-sensitivity flag
		m_bCaseSensitive = ipSource->GetIsCaseSensitive() == VARIANT_TRUE ? true : false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08231");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create a new object
		ICopyableObjectPtr ipObjCopy( CLSID_RemoveEntriesFromList );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06782");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveEntriesFromList::raw_IsConfigured(VARIANT_BOOL * bConfigured)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// If list of entries is non-empty, object is configured
		*bConfigured = (m_ipEntriesList->Size > 0) ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06783");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CRemoveEntriesFromList::validateLicense()
{
	static const unsigned long REMOVE_ENTRIES_FROM_LIST_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( REMOVE_ENTRIES_FROM_LIST_ID, "ELI06784", 
		"Remove Entries From List Output Handler");
}
//-------------------------------------------------------------------------------------------------
