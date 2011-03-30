// RemoveInvalidEntries.cpp : Implementation of CRemoveInvalidEntries
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "RemoveInvalidEntries.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CRemoveInvalidEntries
//-------------------------------------------------------------------------------------------------
CRemoveInvalidEntries::CRemoveInvalidEntries()
: m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CRemoveInvalidEntries::~CRemoveInvalidEntries()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16317");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveInvalidEntries::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOutputHandler,
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
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveInvalidEntries::raw_ProcessOutput(IIUnknownVector* pAttributes,
													  IAFDocument *pAFDoc,
													  IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		validateLicense();

		IIUnknownVectorPtr ipOrignAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI05046", ipOrignAttributes != __nullptr);
		// create an empty vector
		IIUnknownVectorPtr ipReturnAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI06734", ipReturnAttributes != __nullptr);

		// go through all attributes and valid them
		long nSize = ipOrignAttributes->Size();
		// create a dummy ITextInput object
		ITextInputPtr ipTextInput(CLSID_TextInput);
		ASSERT_RESOURCE_ALLOCATION("ELI05047", ipTextInput != __nullptr);
		for (long n=0; n<nSize; n++)
		{
			IAttributePtr ipAttr(ipOrignAttributes->At(n));
			ASSERT_RESOURCE_ALLOCATION("ELI06735", ipAttr != __nullptr);

			IInputValidatorPtr ipInputValidator = ipAttr->GetInputValidator();
			// if the attribute has an input validator
			if (ipInputValidator)
			{
				// Get the attribute value
				ISpatialStringPtr ipValue = ipAttr->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI15528", ipValue != __nullptr);

				// init text input with the value of the attribute
				ipTextInput->InitTextInput(NULL, ipValue->String);

				// validate the attribute and only add the valid 
				// attribute to the returning vector
				if (ipInputValidator->ValidateInput(ipTextInput) == VARIANT_FALSE)
				{
					continue;
				}
			}
			
			// if the attribute does not have any associated input validator,
			// then the attribute is considered as a valid one.
			ipReturnAttributes->PushBack(ipAttr);
		}

		// clear the in/out vector
		ipOrignAttributes->Clear();
		long nReturnSize = ipReturnAttributes->Size();
		// Fill it with the values we want to return
		int i;
		for(i = 0; i < nReturnSize; i++)
		{
			ipOrignAttributes->PushBack(ipReturnAttributes->At(i));
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05034")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveInvalidEntries::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19552", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Remove invalid entries").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05035")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveInvalidEntries::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_RemoveInvalidEntries;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveInvalidEntries::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveInvalidEntries::Load(IStream *pStream)
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
			UCLIDException ue( "ELI07759", "Unable to load newer RemoveInvalidEntries Output Handler." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07760");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveInvalidEntries::Save(IStream *pStream, BOOL fClearDirty)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07761");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveInvalidEntries::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveInvalidEntries::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveInvalidEntries::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveInvalidEntries::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_RemoveInvalidEntries);
		ASSERT_RESOURCE_ALLOCATION("ELI05267", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05268");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CRemoveInvalidEntries::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI05036", 
		"Remove Invalid Entries Output Handler" );
}
//-------------------------------------------------------------------------------------------------
