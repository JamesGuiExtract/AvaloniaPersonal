// QueryBasedAS.cpp : Implementation of CQueryBasedAS

#include "stdafx.h"
#include "QueryBasedAS.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CQueryBasedAS
//-------------------------------------------------------------------------------------------------
CQueryBasedAS::CQueryBasedAS()
:	m_bDirty(false),
	m_strQueryText("")
{
	try
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI13315", m_ipAFUtility != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13316")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IQueryBasedAS,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_ILicensedComponent,
		&IID_IAttributeSelector
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_QueryBasedAS;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// validate license
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
			UCLIDException ue( "ELI13311", 
				"Unable to load newer Query Based Attribute Selector" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}
		// load data here
		dataReader >> m_strQueryText;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13267");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// validate license
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;
		dataWriter << m_strQueryText;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13274");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		return E_NOTIMPL;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13273");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::raw_SelectAttributes(IIUnknownVector * pAttrIn, IAFDocument * pAFDoc, IIUnknownVector * * pAttrOut)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// validate license
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pAttrIn);
		ASSERT_RESOURCE_ALLOCATION("ELI13318", ipAttributes != __nullptr);

		// Query the attributes
		IIUnknownVectorPtr ipFoundAttributes = m_ipAFUtility->QueryAttributes(ipAttributes, 
			_bstr_t(m_strQueryText.c_str()), VARIANT_FALSE);

		CComQIPtr<IIUnknownVector> ipOut(ipFoundAttributes);
		ipOut.CopyTo(pAttrOut);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13272");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		ASSERT_ARGUMENT("ELI19631", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Query attribute selector").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13271");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// validate license
		validateLicense();

		// create a new instance of the EntityNameDataScorer
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_QueryBasedAS);
		ASSERT_RESOURCE_ALLOCATION("ELI13275", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13270");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// validate license
		validateLicense();
		UCLID_AFSELECTORSLib::IQueryBasedASPtr ipFrom(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI13310", ipFrom != __nullptr );

		m_strQueryText = asString( ipFrom->QueryText );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13269");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IQueryBasedAS
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::get_QueryText(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pVal = get_bstr_t(m_strQueryText).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13308");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::put_QueryText(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strQueryText = asString( newVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13309");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// // IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryBasedAS::raw_IsConfigured(VARIANT_BOOL * bConfigured)
{
	try
	{
		*bConfigured = m_strQueryText.empty() ? VARIANT_FALSE : VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13346");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CQueryBasedAS::validateLicense()
{
	static const unsigned long QUERY_BASED_AS_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( QUERY_BASED_AS_ID, "ELI13356", "Query Based Attribute Selector" );
}
//-------------------------------------------------------------------------------------------------
