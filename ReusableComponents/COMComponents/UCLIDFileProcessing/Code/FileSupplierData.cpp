// FileSupplierData.cpp : Implementation of CFileSupplierData

#include "stdafx.h"
#include "FileSupplierData.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CFileSupplierData
//-------------------------------------------------------------------------------------------------
CFileSupplierData::CFileSupplierData()
: m_ipFileSupplier(NULL),
  m_bForceProcessing(false),
  m_eSupplierStatus(kInactiveStatus),
  m_ePriority(kPriorityDefault),
  m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CFileSupplierData::~CFileSupplierData()
{
	try
	{
		m_ipFileSupplier = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16530");
}
//-------------------------------------------------------------------------------------------------
void CFileSupplierData::FinalRelease()
{
	try
	{
		m_ipFileSupplier = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27589");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IFileSupplierData
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI20450", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20451");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create local smart pointer
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI13704", ipCopyThis != __nullptr);

		// Save File Supplier object-with-description
		m_ipFileSupplier = ipCopyThis->FileSupplier;

		// Save Force Processing flag
		m_bForceProcessing = (ipCopyThis->ForceProcessing == VARIANT_TRUE);

		// Save status
		m_eSupplierStatus = (EFileSupplierStatus)ipCopyThis->FileSupplierStatus;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13705");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI20452", pObject != __nullptr);

		// Create new object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_FileSupplierData );
		ASSERT_RESOURCE_ALLOCATION("ELI13706", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13707");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IFileSupplierData
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::get_FileSupplier(IObjectWithDescription **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI20455", pVal != __nullptr);

		// Create File Supplier, if needed
		if (m_ipFileSupplier == __nullptr)
		{
			m_ipFileSupplier.CreateInstance( CLSID_ObjectWithDescription );
			ASSERT_RESOURCE_ALLOCATION( "ELI13709", m_ipFileSupplier != __nullptr );
		}

		// Provide reference to File Supplier
		IObjectWithDescriptionPtr ipShallowCopy = m_ipFileSupplier;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13710")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::put_FileSupplier(IObjectWithDescription* newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Save new File Supplier
		m_ipFileSupplier = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13711")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::get_ForceProcessing(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI20456", pVal != __nullptr);

		// Return setting
		*pVal = m_bForceProcessing ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13712")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::put_ForceProcessing(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Save new setting
		m_bForceProcessing = (newVal == VARIANT_TRUE);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13713")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::get_FileSupplierStatus(EFileSupplierStatus *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI20457", pVal != __nullptr);

		// Return status
		*pVal = m_eSupplierStatus;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13714")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::put_FileSupplierStatus(EFileSupplierStatus newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Save new status
		m_eSupplierStatus = newVal;

		// NOTE: we do not need to set the dirty flag because we did not change
		// any persistent data members.
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13715")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::get_Priority(EFilePriority *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI28015", pVal != __nullptr);

		// Return priority
		*pVal = m_ePriority;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27587");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::put_Priority(EFilePriority newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// Save new priority
		m_ePriority = newVal;

		// Set the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27588");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20458", pClassID != __nullptr);

		*pClassID = CLSID_FileSupplierData;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20459");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// if the data directly owned by this object is dirty, then return true
		if (m_bDirty)
		{
			return S_OK;
		}

		// check if any of the container objects are dirty
		IPersistStreamPtr ipStream = m_ipFileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI14170", ipStream != __nullptr);
		if (ipStream->IsDirty() == S_OK)
		{
			return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20463");

	// if we reached here, that means this object is not dirty
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20460", pStream != __nullptr);

		// Reset member variables
		m_ipFileSupplier = __nullptr;
		m_bForceProcessing = false;
		m_eSupplierStatus = kInactiveStatus;
		m_ePriority = kPriorityDefault;

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
			UCLIDException ue("ELI13735", "Unable to load newer FileSupplierData component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read the Force Processing flag
		dataReader >> m_bForceProcessing;

		// DO NOT Read the Status

		// If version 2 or greater, read the priority
		if (nDataVersion >= 2)
		{
			// Read the priority
			long lTemp;
			dataReader >> lTemp;
			m_ePriority = (EFilePriority)lTemp;
		}

		// Read in the File Supplier with its Description
		IPersistStreamPtr ipFSObject;
		readObjectFromStream( ipFSObject, pStream, "ELI13736");
		m_ipFileSupplier = ipFSObject;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13737");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI20461", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write version number, Force Processing flag, and Priority 
		// DO NOT write File Supplier status
		dataWriter << gnCurrentVersion;
		dataWriter << m_bForceProcessing;
		dataWriter << (long)m_ePriority;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Write out the File Supplier
		IPersistStreamPtr ipFSObject = m_ipFileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI13738", ipFSObject != __nullptr);
		writeObjectToStream( ipFSObject, pStream, "ELI13739", fClearDirty );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13740");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplierData::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CFileSupplierData::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI13708", "File Supplier Data" );
}
//-------------------------------------------------------------------------------------------------
