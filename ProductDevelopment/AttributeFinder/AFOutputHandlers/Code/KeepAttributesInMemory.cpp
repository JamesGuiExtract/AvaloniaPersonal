// KeepAttributesInMemory.cpp : Implementation of CKeepAttributesInMemory
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "KeepAttributesInMemory.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CKeepAttributesInMemory
//-------------------------------------------------------------------------------------------------
CKeepAttributesInMemory::CKeepAttributesInMemory()
: m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CKeepAttributesInMemory::~CKeepAttributesInMemory()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16312");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CKeepAttributesInMemory::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOutputHandler,
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
STDMETHODIMP CKeepAttributesInMemory::raw_ProcessOutput(IIUnknownVector* pAttributes,
														IAFDocument *pAFDoc,
														IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		validateLicense();

		// store the vector of attributes in memory for future access
		m_ipvecAttributes = pAttributes;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06155")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CKeepAttributesInMemory::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CKeepAttributesInMemory::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_KeepAttributesInMemory;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CKeepAttributesInMemory::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CKeepAttributesInMemory::Load(IStream *pStream)
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
			UCLIDException ue( "ELI07756", 
				"Unable to load newer KeepAttributesInMemory Output Handler!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07757");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CKeepAttributesInMemory::Save(IStream *pStream, BOOL fClearDirty)
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

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07758");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CKeepAttributesInMemory::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IKeepAttributesInMemory
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CKeepAttributesInMemory::GetAttributes(IIUnknownVector* *pvecAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// return a reference to the current attributes vector
		if (m_ipvecAttributes)
		{
			IIUnknownVectorPtr ipShallowCopy = m_ipvecAttributes;
			*pvecAttributes = ipShallowCopy.Detach();
		}
		else
		{
			*pvecAttributes = NULL;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06156")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CKeepAttributesInMemory::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI06154", "KeepAttributesInMemory");
}
//-------------------------------------------------------------------------------------------------
