// SelectWithUI.cpp : Implementation of CSelectWithUI
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "SelectWithUI.h"
#include "SelectWithUIDlg.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CSelectWithUI
//-------------------------------------------------------------------------------------------------
CSelectWithUI::CSelectWithUI()
: m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CSelectWithUI::~CSelectWithUI()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16321");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectWithUI::InterfaceSupportsErrorInfo(REFIID riid)
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
STDMETHODIMP CSelectWithUI::raw_ProcessOutput(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
											  IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		validateLicense();

		IIUnknownVectorPtr ipOriginAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI10492", ipOriginAttributes != NULL);
		IIUnknownVectorPtr ipReturnAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI10493", ipReturnAttributes != NULL);

		// Do not show the dialog if no attribute available
		if (ipOriginAttributes->Size() > 0)
		{
			// show dlg
			SelectWithUIDlg showUIDlg(ipOriginAttributes, ipReturnAttributes);

			if (showUIDlg.DoModal() == IDOK)
			{
				// clear the in/out vector
				ipOriginAttributes->Clear();
				long nReturnSize = ipReturnAttributes->Size();
				// Fill it with the values we want to return
				int i;
				for(i = 0; i < nReturnSize; i++)
				{
					ipOriginAttributes->PushBack(ipReturnAttributes->At(i));
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05043")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectWithUI::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19557", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Select with UI").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05044")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectWithUI::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_SelectWithUI;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectWithUI::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectWithUI::Load(IStream *pStream)
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
			UCLIDException ue( "ELI07749", "Unable to load newer SelectWithUI Output Handler." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07751");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectWithUI::Save(IStream *pStream, BOOL fClearDirty)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07752");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectWithUI::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectWithUI::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CSelectWithUI::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectWithUI::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_SelectWithUI);
		ASSERT_RESOURCE_ALLOCATION("ELI05273", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05274");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CSelectWithUI::validateLicense()
{
	static const unsigned long SELECT_WITH_UI_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(SELECT_WITH_UI_ID, "ELI05045", "Select With UI Output Handler");
}
//-------------------------------------------------------------------------------------------------
