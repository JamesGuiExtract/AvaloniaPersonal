// DespeckleICO.cpp : Implementation of CDespeckleICO

#include "stdafx.h"
#include "DespeckleICO.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

// default value for the NoiseSize setting
// NOTE: 3 seems to do a decent job on most images without being too aggressive (settings
//		 as low as 5 could potentially remove punctuation marks)
const long glDEFAULT_NOISE_SIZE = 3;

//-------------------------------------------------------------------------------------------------
// CDespeckleICO
//-------------------------------------------------------------------------------------------------
CDespeckleICO::CDespeckleICO() :
m_lNoiseSize(glDEFAULT_NOISE_SIZE),
m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CDespeckleICO::~CDespeckleICO()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17407");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDespeckleICO,
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
STDMETHODIMP CDespeckleICO::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17489", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t("Despeckle").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17408")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ESImageCleanupLib::IDespeckleICOPtr ipCopyThis(pObject);	
		ASSERT_RESOURCE_ALLOCATION("ELI17414", ipCopyThis != __nullptr);

		m_lNoiseSize = ipCopyThis->NoiseSize;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17409");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::raw_Clone(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19446", pObject != __nullptr);

		// create a copyable object pointer
		ICopyableObjectPtr ipObjCopy(CLSID_DespeckleICO);
		ASSERT_RESOURCE_ALLOCATION("ELI17415", ipObjCopy != __nullptr);

		// set the IUnknownPtr to the current object
		IUnknownPtr ipUnk = this;
		ASSERT_RESOURCE_ALLOCATION("ELI17416", ipUnk != __nullptr);

		// copy to the copyable object pointer
		ipObjCopy->CopyFrom(ipUnk);

		// return the new DespeckleICO object 
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17417");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI17490", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17575");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17491", pClassID != __nullptr);

		*pClassID = CLSID_DespeckleICO;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17830");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17492", pStream != __nullptr);

		// reset the noise size to default value
		m_lNoiseSize = glDEFAULT_NOISE_SIZE;

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
			UCLIDException ue("ELI17418", "Unable to load newer DespeckleICO.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// read the noise size value from the stream
		dataReader >> m_lNoiseSize;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17419");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17493", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// write the version number to the stream
		dataWriter << gnCurrentVersion;

		// write the noise size to the stream
		dataWriter << m_lNoiseSize;

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19445");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IImageCleanupOperation Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::Perform(void* pciRepair)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License first
		validateLicense();

		// wrap the ClearImage repair pointer in smart pointer
		ICiRepairPtr ipciRepair((ICiRepair*) pciRepair);
		ASSERT_RESOURCE_ALLOCATION("ELI17420", ipciRepair != __nullptr);

		// perform the ClearImage CleanNoise method
		ipciRepair->CleanNoise(m_lNoiseSize);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17421");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IDespeckleICO
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::get_NoiseSize(long* plNoiseSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17494", plNoiseSize != __nullptr);

		*plNoiseSize = m_lNoiseSize;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17412");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDespeckleICO::put_NoiseSize(long lNoiseSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (lNoiseSize < 0)
		{
			UCLIDException ue("ELI17410", "Noise size must be a positive value!");
			ue.addDebugInfo("Noise size", lNoiseSize);
			throw ue;
		}

		m_lNoiseSize = lNoiseSize;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17411");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CDespeckleICO::validateLicense()
{
	VALIDATE_LICENSE( gnIMAGE_CLEANUP_ENGINE_FEATURE, "ELI17413", "Despeckle ICO" );
}
//-------------------------------------------------------------------------------------------------