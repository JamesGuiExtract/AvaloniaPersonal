// StringPair.cpp : Implementation of CStringPair
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "StringPair.h"

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
// CStringPair
//--------------------------------------------------------------------------------------------------
CStringPair::CStringPair()
: m_strKey(""),
  m_strValue(""),
  m_bDirty(false)
{
}
//--------------------------------------------------------------------------------------------------
CStringPair::~CStringPair()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16517");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IStringPair,
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
// IStringPair
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::get_StringKey(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strKey.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04236");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::put_StringKey(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_strKey = asString( newVal );
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04237");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::get_StringValue(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strValue.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04238");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::put_StringValue(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_strValue = asString( newVal );
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04239");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::SetKeyValuePair(BSTR bstrKey, BSTR bstrVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_strKey = asString(bstrKey);
		m_strValue = asString(bstrVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31689");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::GetKeyValuePair(BSTR* pbstrKey, BSTR* pbstrVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI31690", pbstrKey != __nullptr);
		ASSERT_ARGUMENT("ELI31691", pbstrVal != __nullptr);

		validateLicense();

		*pbstrKey = _bstr_t(m_strKey.c_str()).Detach();
		*pbstrVal = _bstr_t(m_strValue.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31692");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_StringPair;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_strKey = "";
		m_strValue = "";

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
			UCLIDException ue( "ELI07667", "Unable to load newer StringPair." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_strKey;
			dataReader >> m_strValue;
		}
		
		// set the dirty flag to false as we've just loaded the object
		m_bDirty = false;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04937");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;
		dataWriter << m_strKey;
		dataWriter << m_strValue;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);
		
		// clear the flag as specified
		if (fClearDirty) m_bDirty = false;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04938");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_COMUTILSLib::IStringPairPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08316", ipSource != __nullptr);

		_bstr_t bstrKey;
		_bstr_t bstrValue;
		ipSource->GetKeyValuePair(bstrKey.GetAddress(), bstrValue.GetAddress());

		m_strKey = asString(bstrKey);
		m_strValue = asString(bstrValue);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08317");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// create a new variant vector
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy(CLSID_StringPair);
		ASSERT_RESOURCE_ALLOCATION("ELI08375", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04943");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CStringPair::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CStringPair::validateLicense()
{
	static const unsigned long STRING_PAIR_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE(STRING_PAIR_COMPONENT_ID, "ELI04235", "String Pair");
}
//--------------------------------------------------------------------------------------------------
