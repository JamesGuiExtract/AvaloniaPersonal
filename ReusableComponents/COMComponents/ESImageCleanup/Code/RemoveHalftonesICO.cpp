// RemoveHalftonesICO.cpp : Implementation of CRemoveHalftonesICO

#include "stdafx.h"
#include "RemoveHalftonesICO.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CRemoveHalftonesICO
//-------------------------------------------------------------------------------------------------
CRemoveHalftonesICO::CRemoveHalftonesICO() :
m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CRemoveHalftonesICO::~CRemoveHalftonesICO()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17146");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveHalftonesICO::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRemoveHalftonesICO,
		&IID_IPersistStream,
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
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveHalftonesICO::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17739", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t("Remove halftones").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17147")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveHalftonesICO::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// nothing to copy
	try
	{
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17148");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveHalftonesICO::raw_Clone(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17740", pObject != __nullptr);

		// create a copyable object pointer
		ICopyableObjectPtr ipObjCopy(CLSID_RemoveHalftonesICO);
		ASSERT_RESOURCE_ALLOCATION("ELI17149", ipObjCopy != __nullptr);

		// set the IUnknownPtr to the current object
		IUnknownPtr ipUnk = this;
		ASSERT_RESOURCE_ALLOCATION("ELI17218", ipUnk != __nullptr);

		// copy to the copyable object pointer
		ipObjCopy->CopyFrom(ipUnk);

		// return the new RemoveHalftonesICO object 
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17150");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveHalftonesICO::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17640", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17641");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveHalftonesICO::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17735", pClassID != __nullptr);

		*pClassID = CLSID_RemoveHalftonesICO;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17736");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveHalftonesICO::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveHalftonesICO::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17737", pStream != __nullptr);

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// read the version number from the stream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// check the version number
		if (nDataVersion > gnCurrentVersion)
		{
			UCLIDException ue("ELI17151", "Unable to load newer RemoveHalftoneICO.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17152");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveHalftonesICO::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17738", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// write the version number to the stream
		dataWriter << gnCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (asCppBool(fClearDirty))
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17153");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveHalftonesICO::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	if (pcbSize == NULL)
		return E_POINTER;
		
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IImageCleanupOperation Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveHalftonesICO::Perform(void* pciRepair)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License first
		validateLicense();

		// wrap the ClearImage repair pointer in smart pointer
		ICiRepairPtr ipciRepair((ICiRepair*) pciRepair);
		ASSERT_RESOURCE_ALLOCATION("ELI17154", ipciRepair != __nullptr);

		// perform the ClearImage RemoveHalftone method
		ipciRepair->RemoveHalftone();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17155");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CRemoveHalftonesICO::validateLicense()
{
	VALIDATE_LICENSE( gnIMAGE_CLEANUP_ENGINE_FEATURE, "ELI17156", "Remove Halftones ICO" );
}
//-------------------------------------------------------------------------------------------------