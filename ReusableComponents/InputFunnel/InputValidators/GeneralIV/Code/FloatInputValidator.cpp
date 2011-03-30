// FloatInputValidator.cpp : Implementation of CFloatInputValidator
#include "stdafx.h"
#include "GeneralIV.h"
#include "FloatInputValidator.h"
#include "GeneralInputTypes.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CFloatInputValidator
//-------------------------------------------------------------------------------------------------
CFloatInputValidator::CFloatInputValidator()
: m_bDirty(false)
{
	try
	{
		// Create an instance of DoubleInputValidator
		m_ipDIV.CreateInstance( __uuidof(DoubleInputValidator) );
		ASSERT_RESOURCE_ALLOCATION( "ELI03815", m_ipDIV != __nullptr );
		
		// Set appropriate limits
		setDefaults();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI03816")
}
//-------------------------------------------------------------------------------------------------
CFloatInputValidator::~CFloatInputValidator()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16443");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFloatInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFloatInputValidator,
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
STDMETHODIMP CFloatInputValidator::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFloatInputValidator::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_FloatInputValidator);
		ASSERT_RESOURCE_ALLOCATION("ELI08367", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04874");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFloatInputValidator::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_FloatInputValidator;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFloatInputValidator::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// check m_bDirty flag first, if it's not dirty then
		// check all objects owned by this object
		HRESULT hr = m_bDirty ? S_OK : S_FALSE;
		if (!m_bDirty)
		{
			IPersistStreamPtr ipPersistStream(m_ipDIV);
			if (ipPersistStream==NULL)
			{
				throw UCLIDException("ELI04803", "Object does not support persistence!");
			}
			
			hr = ipPersistStream->IsDirty();
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04804");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFloatInputValidator::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset member variables
		m_ipDIV = __nullptr;

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

		// Load the DoubleInputValidator member object from
		// the stream
		IPersistStreamPtr ipObj;
		readObjectFromStream(ipObj, pStream, "ELI09976");
		m_ipDIV = ipObj;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04683");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFloatInputValidator::Save(IStream *pStream, BOOL fClearDirty)
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

		// write the DoubleInputValidator member object to 
		// the stream
		IPersistStreamPtr ipObj = m_ipDIV;
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI04684", "Double InputValidator does not support persistence!");
		}
		else
		{
			writeObjectToStream(ipObj, pStream, "ELI09931", fClearDirty);
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04682");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFloatInputValidator::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFloatInputValidator::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CFloatInputValidator::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19620", pstrComponentDescription != __nullptr)

		// Retrieve definition
		*pstrComponentDescription = _bstr_t( 
			gstrFLOAT_INPUT_TYPE.c_str() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03811")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFloatInputValidator::raw_ValidateInput(ITextInput * pTextInput, 
													 VARIANT_BOOL * pbSuccessful)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// QI to the Input Validator
		IInputValidatorPtr ipDIV( m_ipDIV );

		// Do validation with DoubleIV data member
		ipDIV->raw_ValidateInput( pTextInput, pbSuccessful );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03817")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFloatInputValidator::raw_GetInputType(BSTR * pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT hr = S_OK;

	try
	{
		ASSERT_ARGUMENT("ELI19621", pstrInputType != __nullptr)

		// Check license
		validateLicense();

		// Just provide the component description
		hr = raw_GetComponentDescription( pstrInputType );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03818")

	return hr;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CFloatInputValidator::setDefaults()
{
	// Turn on limits
	m_ipDIV->put_HasMin( VARIANT_TRUE );
	m_ipDIV->put_HasMax( VARIANT_TRUE );

	// Set limits
	m_ipDIV->put_Min( -3.4e+38 );
	m_ipDIV->put_Max( 3.4e+38 );
}
//-------------------------------------------------------------------------------------------------
void CFloatInputValidator::validateLicense()
{
	static const unsigned long FLOATIV_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( FLOATIV_COMPONENT_ID, "ELI03812",
		"Float Input Validator" );
}
//-------------------------------------------------------------------------------------------------
