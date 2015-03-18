// SpatiallySortAttributes.cpp : Implementation of CSpatiallySortAttributes
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "SpatiallySortAttributes.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CSpatiallySortAttributes
//-------------------------------------------------------------------------------------------------
CSpatiallySortAttributes::CSpatiallySortAttributes()
: m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CSpatiallySortAttributes::~CSpatiallySortAttributes()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI37928");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallySortAttributes::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOutputHandler,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
		&IID_IIdentifiableObject,
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
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallySortAttributes::raw_ProcessOutput(IIUnknownVector* pAttributes,
													 IAFDocument *pAFDoc,
													 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI37942", ipAttributes != __nullptr);

		ISortComparePtr ipCompare(CLSID_SpatiallyCompareAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI37943", ipCompare != __nullptr);
		
		ipAttributes->Sort(ipCompare);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37932")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallySortAttributes::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI37933", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Spatially sort attributes").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37934")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallySortAttributes::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_SpatiallySortAttributes;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallySortAttributes::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallySortAttributes::Load(IStream *pStream)
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

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI37935", "Unable to load newer SpatiallySortAttributes Output Handler." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Load the GUID for the IIdentifiableObject interface.
		loadGUID(pStream);

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37936");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallySortAttributes::Save(IStream *pStream, BOOL fClearDirty)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37937");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallySortAttributes::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallySortAttributes::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CSpatiallySortAttributes::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallySortAttributes::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_SpatiallySortAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI37938", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37939");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatiallySortAttributes::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37940")
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CSpatiallySortAttributes::validateLicense()
{
	static const unsigned long SPATIALLYSORTATTRIBUTES_DUPLICATES_ID = gnRULE_WRITING_CORE_OBJECTS;

	VALIDATE_LICENSE(SPATIALLYSORTATTRIBUTES_DUPLICATES_ID, "ELI37941", "Spatially Sort Attributes Output Handler");
}
//-------------------------------------------------------------------------------------------------
