// RemoveSpatialInfo.cpp : Implementation of CRemoveSpatialInfo
#include "stdafx.h"
#include "AFPreProcessors.h"
#include "RemoveSpatialInfo.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

// current version
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CRemoveSpatialInfo
//-------------------------------------------------------------------------------------------------
CRemoveSpatialInfo::CRemoveSpatialInfo()
:m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CRemoveSpatialInfo::~CRemoveSpatialInfo()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16325");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRemoveSpatialInfo,
		&IID_IDocumentPreprocessor,
		&IID_IAttributeModifyingRule,
		&IID_IOutputHandler,
		&IID_IPersistStream,
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
// IRemoveSpatialInfo
//-------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
// IDocumentPreprocessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::raw_Process(IAFDocument* pDocument, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAFDocumentPtr ipAFDoc(pDocument);
		ASSERT_ARGUMENT("ELI08019", ipAFDoc != __nullptr);

		// get the spatial string
		ISpatialStringPtr ipInputText = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI10041", ipInputText != __nullptr);

		removeSpatialInfo(ipInputText);

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10042");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::raw_ProcessOutput(IIUnknownVector* pAttributes, IAFDocument *pAFDoc, 
												   IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI10053", ipAttributes != __nullptr);

		int i;
		for(i = 0; i < ipAttributes->Size(); i++)
		{
			IAttributePtr ipAttribute = ipAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI10059", ipAttribute != __nullptr);
			removeSpatialInfo(ipAttribute);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10054")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput,
												 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI10055", ipAttribute != __nullptr );

		removeSpatialInfo(ipAttribute);

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10056");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_RemoveSpatialInfo;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::Load(IStream *pStream)
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
			UCLIDException ue("ELI10043", "Unable to load newer RemoveSpatialInfo component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}
		
		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10044");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
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

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10045");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19560", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Remove spatial information").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10046");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
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
STDMETHODIMP CRemoveSpatialInfo::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFPREPROCESSORSLib::IRemoveSpatialInfoPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI10047", ipSource!=NULL);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10048");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSpatialInfo::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_RemoveSpatialInfo);
		ASSERT_RESOURCE_ALLOCATION("ELI10049", ipObjCopy != __nullptr);
		
		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10050");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CRemoveSpatialInfo::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI10051", "Remove Spatial Info" );
}
//-------------------------------------------------------------------------------------------------
void CRemoveSpatialInfo::removeSpatialInfo(ISpatialStringPtr ipSS)
{
	ASSERT_ARGUMENT("ELI25940", ipSS != __nullptr);

	// Downgrade to non-spatial will remove all spatial info
	ipSS->DowngradeToNonSpatialMode();
}
//-------------------------------------------------------------------------------------------------
void CRemoveSpatialInfo::removeSpatialInfo(IAttributePtr ipAttribute)
{
	IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;

	int i;
	for(i = 0; i < ipSubAttributes->Size(); i++)
	{
		IAttributePtr ipSubAttribute = ipSubAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI10058", ipSubAttribute != __nullptr);
		// recurse on sub attributes
		removeSpatialInfo(ipSubAttribute);
	}

	// clear spatial info
	removeSpatialInfo(ipAttribute->Value);
}
//-------------------------------------------------------------------------------------------------
