// SmoothCharactersICO.cpp : Implementation of CSmoothCharactersICO

#include "stdafx.h"
#include "SmoothCharactersICO.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CSmoothCharactersICO
//-------------------------------------------------------------------------------------------------
CSmoothCharactersICO::CSmoothCharactersICO() :
m_lSmoothType(ciSmoothLightenEdges), // default to lighten edges
m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CSmoothCharactersICO::~CSmoothCharactersICO()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17806");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISmoothCharactersICO,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IImageCleanupOperation,
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
STDMETHODIMP CSmoothCharactersICO::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17807", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t("Smooth characters").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17808")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ESImageCleanupLib::ISmoothCharactersICOPtr ipCopyThis(pObject);	
		ASSERT_RESOURCE_ALLOCATION("ELI17809", ipCopyThis != __nullptr);

		m_lSmoothType = ipCopyThis->SmoothType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17810");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::raw_Clone(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17811", pObject != __nullptr);

		// create a copyable object pointer
		ICopyableObjectPtr ipObjCopy(CLSID_SmoothCharactersICO);
		ASSERT_RESOURCE_ALLOCATION("ELI17812", ipObjCopy != __nullptr);

		// set the IUnknownPtr to the current object
		IUnknownPtr ipUnk = this;
		ASSERT_RESOURCE_ALLOCATION("ELI17813", ipUnk != __nullptr);

		// copy to the copyable object pointer
		ipObjCopy->CopyFrom(ipUnk);

		// return the new SmoothCharactersICO object 
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17814");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI17815", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17816");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17817", pClassID != __nullptr);

		*pClassID = CLSID_SmoothCharactersICO;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17829");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17818", pStream != __nullptr);

		// reset the smooth type to default value
		m_lSmoothType = ciSmoothLightenEdges;

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
			UCLIDException ue("ELI17819", "Unable to load newer SmoothCharactersICO.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// read the smooth type value from the stream
		dataReader >> m_lSmoothType;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17820");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17821", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// write the version number to the stream
		dataWriter << gnCurrentVersion;

		// write the smooth type
		dataWriter << m_lSmoothType;

		// flush the stream
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17822");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IImageCleanupOperation Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::Perform(void* pciRepair)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License first
		validateLicense();

		// wrap the ClearImage repair pointer in smart pointer
		ICiRepairPtr ipciRepair((ICiRepair*) pciRepair);
		ASSERT_RESOURCE_ALLOCATION("ELI17823", ipciRepair != __nullptr);

		// perform the ClearImage SmoothCharacters method
		ipciRepair->SmoothCharacters((ESmoothType)m_lSmoothType);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17824");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ISmoothCharactersICO
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::get_SmoothType(long* plSmoothType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17825", plSmoothType != __nullptr);

		*plSmoothType = m_lSmoothType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17802");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSmoothCharactersICO::put_SmoothType(long lSmoothType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// lSmoothType muse either be ciSmoothLightenEdges or ciSmoothDarkenEdges
		if (lSmoothType != ciSmoothLightenEdges && lSmoothType != ciSmoothDarkenEdges)
		{
			UCLIDException ue("ELI17803", "Invalid value for CharacterSmooth type!");
			ue.addDebugInfo("Smooth Type", lSmoothType);
			throw ue;
		}

		m_lSmoothType = lSmoothType;

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17804");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CSmoothCharactersICO::validateLicense()
{
	VALIDATE_LICENSE( gnIMAGE_CLEANUP_ENGINE_FEATURE, "ELI17805", "SmoothCharacters ICO" );
}
//-------------------------------------------------------------------------------------------------