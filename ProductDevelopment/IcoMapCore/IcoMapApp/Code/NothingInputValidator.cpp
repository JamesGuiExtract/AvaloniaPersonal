// NothingInputValidator.cpp : Implementation of CNothingInputValidator
#include "stdafx.h"
#include "IcoMapApp.h"
#include "NothingInputValidator.h"

#include <UCLIDException.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputValidator::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputValidator,
		&IID_ICategorizedComponent,
		&IID_IPersistStream,
		&IID_INothingInputValidator
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// CNothingInputValidator
//-------------------------------------------------------------------------------------------------
CNothingInputValidator::CNothingInputValidator()
: m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CNothingInputValidator::~CNothingInputValidator()
{
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputValidator::raw_ValidateInput(ITextInput *pTextInput, VARIANT_BOOL *pbSuccessful)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pbSuccessful = VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12570")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputValidator::raw_GetInputType(BSTR *pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT ret = S_OK;
	try
	{
		// Return this input validator description
		ret = raw_GetComponentDescription(pstrInputType);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12571")
	
	return ret;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputValidator::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{	
	try
	{
		*pbstrComponentDescription = _bstr_t("Nothing").copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12572")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputValidator::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_NothingInputValidator;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputValidator::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputValidator::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// TODO: When this object has member variables, code needs
		// to be added here to load those member variable values from a stream
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, NULL );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12573");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputValidator::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// TODO: When this object has member variables, code needs
		// to be added here to save those member variable values to a stream
		// Set current version number
		const unsigned long nCurrentVersion = 1;

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << nCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12574");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputValidator::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
