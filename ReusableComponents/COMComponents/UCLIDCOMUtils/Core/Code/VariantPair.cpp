// VariantPair.cpp : Implementation of CVariantPair
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "VariantPair.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CVariantPair
//--------------------------------------------------------------------------------------------------
CVariantPair::CVariantPair()
: m_vtKey(),
  m_vtValue(),
  m_bDirty(false)
{
}
//--------------------------------------------------------------------------------------------------
CVariantPair::~CVariantPair()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI46011");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IVariantPair,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}


//--------------------------------------------------------------------------------------------------
// IVariantPair
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::get_VariantKey(VARIANT *pvtKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		VariantCopy(pvtKey, &m_vtKey);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46012");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::put_VariantKey(VARIANT newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_vtKey = newVal;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46013");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::get_VariantValue(VARIANT *pvtVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		VariantCopy(pvtVal, &m_vtValue);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46014");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::put_VariantValue(VARIANT newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_vtValue = newVal;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46015");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::SetKeyValuePair(VARIANT vtKey, VARIANT vtVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_vtKey = vtKey;
		m_vtValue = vtVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46016");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::GetKeyValuePair(VARIANT* pvtKey, VARIANT* pvtVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI46017", pvtKey != __nullptr);
		ASSERT_ARGUMENT("ELI46018", pvtVal != __nullptr);

		validateLicense();

		VariantCopy(pvtKey, &m_vtKey);
		VariantCopy(pvtVal, &m_vtValue);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46019");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_VariantPair;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_vtKey.Clear();
		m_vtValue.Clear();

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
			UCLIDException ue( "ELI46020", "Unable to load newer VariantPair." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// read the key
		CComVariant vtTemp;
		HRESULT hr = vtTemp.ReadFromStream(pStream);
		if (FAILED(hr))
		{
			UCLIDException ue("ELI46021", "Failed reading from stream!");
			ue.addHresult(hr);
			throw ue;
		}
		m_vtKey = vtTemp;

		// read the value
		hr = vtTemp.ReadFromStream(pStream);
		if (FAILED(hr))
		{
			UCLIDException ue("ELI46022", "Failed reading from stream!");
			ue.addHresult(hr);
			throw ue;
		}
		m_vtValue = vtTemp;
		
		// set the dirty flag to false as we've just loaded the object
		m_bDirty = false;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46023");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		CComVariant vtTemp = m_vtKey;
		HRESULT hr = vtTemp.WriteToStream(pStream);
		if (FAILED(hr))
		{
			UCLIDException ue("ELI46024", "Failed writing to stream!");
			ue.addHresult(hr);
			throw ue;
		}

		vtTemp = m_vtValue;
		hr = vtTemp.WriteToStream(pStream);
		if (FAILED(hr))
		{
			UCLIDException ue("ELI46025", "Failed writing to stream!");
			ue.addHresult(hr);
			throw ue;
		}

		// clear the flag as specified
		if (fClearDirty) m_bDirty = false;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46026");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_COMUTILSLib::IVariantPairPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI46027", ipSource != __nullptr);

		_variant_t vtKey;
		_variant_t vtValue;
		ipSource->GetKeyValuePair(vtKey.GetAddress(), vtValue.GetAddress());

		// Check for types which need an explicit deep copy
		VARTYPE varType = vtKey.vt;
		if (varType == VT_UNKNOWN || varType == VT_DISPATCH)
		{
			UCLID_COMUTILSLib::ICopyableObjectPtr ipCopier(vtKey.punkVal);
			ASSERT_RESOURCE_ALLOCATION("ELI46028", ipCopier != __nullptr);

			IUnknownPtr ipCopy = ipCopier->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI46029", ipCopy != __nullptr);
			vtKey = (IUnknown*)ipCopy;
		}
		varType = vtValue.vt;
		if (varType == VT_UNKNOWN || varType == VT_DISPATCH)
		{
			UCLID_COMUTILSLib::ICopyableObjectPtr ipCopier(vtValue.punkVal);
			ASSERT_RESOURCE_ALLOCATION("ELI46030", ipCopier != __nullptr);

			IUnknownPtr ipCopy = ipCopier->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI46031", ipCopy != __nullptr);
			vtValue = (IUnknown*)ipCopy;
		}
		m_vtKey = vtKey;
		m_vtValue = vtValue;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46032");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// create a new variant vector
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy(CLSID_VariantPair);
		ASSERT_RESOURCE_ALLOCATION("ELI46033", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46034");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantPair::raw_IsLicensed(VARIANT_BOOL * pbValue)
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

//--------------------------------------------------------------------------------------------------
// Helper function
//--------------------------------------------------------------------------------------------------
void CVariantPair::validateLicense()
{
	static const unsigned long VARIANT_PAIR_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE(VARIANT_PAIR_COMPONENT_ID, "ELI46035", "Variant Pair");
}
//--------------------------------------------------------------------------------------------------
