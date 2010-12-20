// AFConvertVOAToXMLTask.cpp : Implementation of CAFConvertVOAToXMLTask
#include "stdafx.h"
#include "AFFileProcessors.h"
#include "AFConvertVOAToXMLTask.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <RegistryPersistenceMgr.h>
#include <ComUtils.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CAFConvertVOAToXMLTask
//-------------------------------------------------------------------------------------------------
CAFConvertVOAToXMLTask::CAFConvertVOAToXMLTask()
:	m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CAFConvertVOAToXMLTask::~CAFConvertVOAToXMLTask()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI26252");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAFConvertVOAToXMLTask,
		&IID_IFileProcessingTask,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
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
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26253");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::raw_ProcessFile(BSTR strFileFullName, long nFileID, long nActionID,
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		IFAMTagManagerPtr ipTagManager(pTagManager);
		ASSERT_ARGUMENT("ELI26287", ipTagManager != NULL);
		ASSERT_ARGUMENT("ELI26255", pResult != NULL);

		// Input file for processing
		string strSourceDoc = asString(strFileFullName);
		ASSERT_ARGUMENT("ELI26256", strSourceDoc.empty() == false);

		// Check license
		validateLicense();

		// Expand the tags for the VOA file
		_bstr_t bstrVoaFile = ipTagManager->ExpandTags(m_strVOAFile.c_str(), strSourceDoc.c_str());

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		// Create a vector to hold the attributes and load them from the VOA file
		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI26297", ipAttributes != NULL);
		ipAttributes->LoadFrom(bstrVoaFile, VARIANT_FALSE);

		// Create a fake AFDocument object for the output handler
		ISpatialStringPtr ipString(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI26314", ipString != NULL);
		ipString->CreateNonSpatialString("Fake", strSourceDoc.c_str());
		IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI26315", ipAFDoc != NULL);
		ipAFDoc->Text = ipString;

		// Get the XML Output handler as an output handler
		IOutputHandlerPtr ipOutput = getXMLOutputHandler();
		ASSERT_RESOURCE_ALLOCATION("ELI26296", ipOutput != NULL);

		// Write the XML file
		ipOutput->ProcessOutput(ipAttributes, ipAFDoc, pProgressStatus);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26259")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26260");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26261");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31170", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31169");
}

//-------------------------------------------------------------------------------------------------
// IAFConvertVOAToXMLTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::get_VOAFile(BSTR *pbstrVOAFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26262", pbstrVOAFile != NULL);
		validateLicense();

		*pbstrVOAFile = _bstr_t(m_strVOAFile.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26263")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::put_VOAFile(BSTR bstrVOAFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strVOAFile = asString(bstrVOAFile);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26264")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::get_XMLOutputHandler(IUnknown **ppXMLOutputHandler)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26268", ppXMLOutputHandler != NULL);

		validateLicense();
		
		IOutputToXMLPtr ipShallowCopy = getXMLOutputHandler();

		*ppXMLOutputHandler = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26269");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::put_XMLOutputHandler(IUnknown *pXMLOutputHandler)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		IOutputToXMLPtr ipOutputToXML = pXMLOutputHandler;
		ASSERT_ARGUMENT("ELI26270", ipOutputToXML != NULL);
		
		validateLicense();
		
		m_ipXMLOutputHandler = ipOutputToXML;

		// Ensure that the new output handler is restricting the tag set to FAM tags
		m_ipXMLOutputHandler->FAMTags = VARIANT_TRUE;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26271");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26272", pstrComponentDescription != NULL);

		*pstrComponentDescription = _bstr_t("Core: Convert VOA to XML").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26273");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFFILEPROCESSORSLib::IAFConvertVOAToXMLTaskPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI26274", ipSource != NULL);

		// Copy the file name from the source
		m_strVOAFile = asString(ipSource->VOAFile);

		// Get the XML Output handler as a copyable object and clone it
		ICopyableObjectPtr ipCopy = ipSource->XMLOutputHandler;
		ASSERT_RESOURCE_ALLOCATION("ELI26291", ipCopy != NULL);
		m_ipXMLOutputHandler = ipCopy->Clone();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26275");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26276", pObject != NULL);

		// Validate license first
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_AFConvertVOAToXMLTask);
		ASSERT_RESOURCE_ALLOCATION("ELI26277", ipObjCopy != NULL);

		IUnknownPtr ipUnk(this);
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26278");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// Configured if:
		// 1. VOA file name is at least 5 characters
		// 2. XML Output Handler is configured
		bool bConfigured = m_strVOAFile.length() > 4;

		// If the file names are configured then check the XML output handler
		if (bConfigured)
		{
			// Get the XML output handler as a configurable object
			IMustBeConfiguredObjectPtr ipMustBeConfigured = getXMLOutputHandler();

			// Check if it is not null and is configured
			bConfigured = ipMustBeConfigured != NULL
				&& asCppBool(ipMustBeConfigured->IsConfigured());
		}

		// Return the value
		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26279");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI26280", pbValue != NULL);

		try
		{
			// check license
			validateLicense();

			// if no exception was thrown, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26281");

	return S_OK;

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	*pClassID = CLSID_AFConvertVOAToXMLTask;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		// If the dirty flag is false, need to ensure the
		// XML output handler is not dirty
		if (!m_bDirty)
		{
			// Check the XML output handler
			IPersistStreamPtr ipOutput = getXMLOutputHandler();
			ASSERT_RESOURCE_ALLOCATION("ELI26294", ipOutput != NULL);

			// Return the dirty state of the XML output handler
			return ipOutput->IsDirty();
		}
		else
		{
			// Dirty flag is true, return S_OK to indicate dirty
			return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26295");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

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
			UCLIDException ue( "ELI26282", 
				"Unable to load newer Run Object On Query Output Handler!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Read the VOA file name
		dataReader >> m_strVOAFile;

		// Read the XML output handler from the stream
		IPersistStreamPtr ipOutputToXML;
		readObjectFromStream(ipOutputToXML, pStream, "ELI26241");
		m_ipXMLOutputHandler = ipOutputToXML;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26283");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;

		// Write the VOA file name
		dataWriter << m_strVOAFile;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Write the xml output handler to the stream
		IPersistStreamPtr ipOuputXmlStream = m_ipXMLOutputHandler;
		ASSERT_RESOURCE_ALLOCATION("ELI26242", ipOuputXmlStream != NULL);
		writeObjectToStream(ipOuputXmlStream, pStream, "ELI26316", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26284");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTask::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CAFConvertVOAToXMLTask::validateLicense()
{
	VALIDATE_LICENSE( gnFILE_ACTION_MANAGER_OBJECTS, "ELI26285", 
					"Attribute Finder Convert VOA To XML Task" );
}
//-------------------------------------------------------------------------------------------------
IOutputToXMLPtr CAFConvertVOAToXMLTask::getXMLOutputHandler()
{
	if (m_ipXMLOutputHandler == NULL)
	{
		m_ipXMLOutputHandler.CreateInstance(CLSID_OutputToXML);
		ASSERT_RESOURCE_ALLOCATION("ELI26286", m_ipXMLOutputHandler != NULL);

		// Set the xml output handler tag set to FAM tags
		m_ipXMLOutputHandler->FAMTags = VARIANT_TRUE;
	}

	return m_ipXMLOutputHandler;
}
//-------------------------------------------------------------------------------------------------
