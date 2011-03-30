// ShortInputValidator.cpp : Implementation of CShortInputValidator
#include "stdafx.h"
#include "GeneralIV.h"
#include "ShortInputValidator.h"
#include "GeneralInputTypes.h"
#include "IntegerInputValidator.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CShortInputValidator
//-------------------------------------------------------------------------------------------------
CShortInputValidator::CShortInputValidator()
: m_bDirty(false)
{
	try
	{
		// Create an instance of IntegerInputValidator
		m_ipIIV.CreateInstance( __uuidof(IntegerInputValidator) );
		ASSERT_RESOURCE_ALLOCATION( "ELI03813", m_ipIIV != __nullptr );
		
		// Set appropriate limits
		setDefaults();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI03814")
}
//-------------------------------------------------------------------------------------------------
CShortInputValidator::~CShortInputValidator()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16445");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IShortInputValidator,
		&IID_IPersistStream,
		&IID_ICopyableObject,
		&IID_IInputValidator
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_ShortInputValidator);
		ASSERT_RESOURCE_ALLOCATION("ELI08369", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04878");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19624", pstrComponentDescription != __nullptr)

		// Retrieve definition
		*pstrComponentDescription = _bstr_t( 
			gstrSHORT_INPUT_TYPE.c_str() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03810")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::raw_ValidateInput(ITextInput * pTextInput, 
													 VARIANT_BOOL * pbSuccessful)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// QI to the Input Validator
		IInputValidatorPtr ipIIV( m_ipIIV );

		// Do validation with IntegerIV data member
		ipIIV->raw_ValidateInput( pTextInput, pbSuccessful );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03819")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::raw_GetInputType(BSTR * pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT hr = S_OK;

	try
	{
		ASSERT_ARGUMENT("ELI19625", pstrInputType != __nullptr)

		// Check license
		validateLicense();

		// Just provide the component description
		hr = raw_GetComponentDescription( pstrInputType );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03820")

	return hr;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ShortInputValidator;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// check m_bDirty flag first, if it's not dirty then
		// check all objects owned by this object
		HRESULT hr = m_bDirty ? S_OK : S_FALSE;
		if (!m_bDirty)
		{
			IPersistStreamPtr ipPersistStream(m_ipIIV);
			if (ipPersistStream==NULL)
			{
				throw UCLIDException("ELI04805", "Object does not support persistence!");
			}
			
			hr = ipPersistStream->IsDirty();
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04806");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset member variables
		m_ipIIV = __nullptr;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, NULL );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;
		if (nDataVersion >= 1)
		{
		}

		// Load the Integer member object from
		// the stream
		IPersistStreamPtr ipObj;
		readObjectFromStream(ipObj, pStream, "ELI09977");
		m_ipIIV = ipObj;

		// clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04692");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Set current version number
		const unsigned long nCurrentVersion = 1;

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << nCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// write the IntegerInputValidator member object to 
		// the stream
		IPersistStreamPtr ipObj = m_ipIIV;
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI04693", "Integer InputValidator does not support persistence!");
		}
		else
		{
			writeObjectToStream(ipObj, pStream, "ELI09932", fClearDirty);
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04691");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CShortInputValidator::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CShortInputValidator::setDefaults()
{
	// Turn on limits
	m_ipIIV->put_HasMin( VARIANT_TRUE );
	m_ipIIV->put_HasMax( VARIANT_TRUE );

	// Set limits
	m_ipIIV->put_Min( -32768 );
	m_ipIIV->put_Max( 32767 );
}
//-------------------------------------------------------------------------------------------------
void CShortInputValidator::validateLicense()
{
	static const unsigned long SHORTIV_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( SHORTIV_COMPONENT_ID, "ELI03809",
		"Short Integer Input Validator" );
}
//-------------------------------------------------------------------------------------------------
