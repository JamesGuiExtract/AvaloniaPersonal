// EliminateDuplicates.cpp : Implementation of CEliminateDuplicates
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "EliminateDuplicates.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CEliminateDuplicates
//-------------------------------------------------------------------------------------------------
CEliminateDuplicates::CEliminateDuplicates()
: m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CEliminateDuplicates::~CEliminateDuplicates()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16311");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEliminateDuplicates::InterfaceSupportsErrorInfo(REFIID riid)
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
STDMETHODIMP CEliminateDuplicates::raw_ProcessOutput(IIUnknownVector* pAttributes,
													 IAFDocument *pAFDoc,
													 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		validateLicense();

		// Loop through the vector and discard duplicate attributes.
		IIUnknownVectorPtr ipOrignAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI05048", ipOrignAttributes != __nullptr);

		for (long n = 0; n < ipOrignAttributes->Size(); n++)
		{
			// Retrieve this Attribute
			IAttributePtr ipOriginAttr = ipOrignAttributes->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI15672", ipOriginAttr != __nullptr);

			// Check remaining Attributes in the vector
			// If any duplicates are found, discard them
			for (int i = n + 1; i < ipOrignAttributes->Size(); i++)
			{
				// Retrieve this Attribute
				IAttributePtr ipNextAttr = ipOrignAttributes->At( i );
				ASSERT_RESOURCE_ALLOCATION("ELI15673", ipNextAttr != __nullptr);

				// Check for duplicate
				if (ipNextAttr->IsNonSpatialMatch( ipOriginAttr ) == VARIANT_TRUE)
				{
					// Discard this Attribute and decrement index
					ipOrignAttributes->Remove( i );
					i--;
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05032")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEliminateDuplicates::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19542", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Eliminate duplicates").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05033")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEliminateDuplicates::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_EliminateDuplicates;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEliminateDuplicates::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEliminateDuplicates::Load(IStream *pStream)
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
			UCLIDException ue( "ELI07753", "Unable to load newer EliminateDuplicates Output Handler." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07754");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEliminateDuplicates::Save(IStream *pStream, BOOL fClearDirty)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07755");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEliminateDuplicates::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEliminateDuplicates::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CEliminateDuplicates::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEliminateDuplicates::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_EliminateDuplicates);
		ASSERT_RESOURCE_ALLOCATION("ELI05265", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05266");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CEliminateDuplicates::validateLicense()
{
	static const unsigned long ELIMINATE_DUPLICATES_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(ELIMINATE_DUPLICATES_ID, "ELI05031", "Eliminate Duplicates Output Handler");
}
//-------------------------------------------------------------------------------------------------
