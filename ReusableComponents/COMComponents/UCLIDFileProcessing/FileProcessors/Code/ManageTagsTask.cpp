//==================================================================================================
//
// COPYRIGHT (c) 2009 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ManageTagsTask.cpp
//
// PURPOSE:	A file processing task that will allow managing file tags
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#include "stdafx.h"
#include "FileProcessors.h"
#include "ManageTagsTask.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ByteStream.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

// component description
const string gstrMANAGE_TAGS_COMPONENT_DESCRIPTION = "Core: Manage tags";

//--------------------------------------------------------------------------------------------------
// CManageTagsTask
//--------------------------------------------------------------------------------------------------
CManageTagsTask::CManageTagsTask() :
m_operationType(kOperationApplyTags),
m_ipVecTags(NULL),
m_bDirty(false)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI27441");
}
//--------------------------------------------------------------------------------------------------
CManageTagsTask::~CManageTagsTask()
{
	try
	{
		// Ensure the vector is released
		m_ipVecTags = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27442");
}
//--------------------------------------------------------------------------------------------------
void CManageTagsTask::FinalRelease()
{
	try
	{
		// Ensure the vector is released
		m_ipVecTags = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27510");
}

//--------------------------------------------------------------------------------------------------
// IManageTagsTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::put_Operation(EManageTagsOperationType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		// Store the new operation type
		m_operationType = newVal;

		// Set the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27443");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::get_Operation(EManageTagsOperationType* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI27444", pVal != NULL);

		// Get the operation type
		*pVal = m_operationType;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27445");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::put_Tags(IVariantVector* pvecTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		IVariantVectorPtr ipVecTags(pvecTags);
		
		m_ipVecTags = ipVecTags;

		// Set the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27447");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::get_Tags(IVariantVector** ppvecTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI27448", ppvecTags != NULL);

		// Get a shallow copy of the vector
		IVariantVectorPtr ipShallowCopy(m_ipVecTags);

		// Return the variant vector
		*ppvecTags = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27450");
}

//--------------------------------------------------------------------------------------------------
// ICategorizedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::raw_GetComponentDescription(BSTR* pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI27451", pstrComponentDescription != NULL);
		
		*pstrComponentDescription = 
			_bstr_t(gstrMANAGE_TAGS_COMPONENT_DESCRIPTION.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27452");
	
	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ICopyableObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::raw_CopyFrom(IUnknown* pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// get the ManageTagsTask object
		UCLID_FILEPROCESSORSLib::IManageTagsTaskPtr ipManageTagsTask(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI27453", ipManageTagsTask != NULL);

		// Copy the operation from the object
		m_operationType = (EManageTagsOperationType) ipManageTagsTask->Operation;

		// Clone the tags from the object
		ICopyableObjectPtr ipCopy = ipManageTagsTask->Tags;
		if (ipCopy != NULL)
		{
			m_ipVecTags = ipCopy->Clone();
		}
		else
		{
			m_ipVecTags = NULL;
		}

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27455");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::raw_Clone(IUnknown** ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate license first
		validateLicense();

		// Ensure that the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI27456", ppObject != NULL);

		// Get the copyable object interface
		ICopyableObjectPtr ipObjCopy(CLSID_ManageTagsTask);
		ASSERT_RESOURCE_ALLOCATION("ELI27457", ipObjCopy != NULL);

		// Create a shallow copy
		IUnknownPtr ipUnknown(this);
		ASSERT_RESOURCE_ALLOCATION("ELI27458", ipUnknown != NULL);
		ipObjCopy->CopyFrom(ipUnknown);

		// Return the new ManageTagsTask to the caller
		*ppObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27459");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27460");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::raw_ProcessFile(IFileRecord* pFileRecord, long nActionID,
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();
		// Check for NULL parameters
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_ARGUMENT("ELI27461", ipDB != NULL);
		ASSERT_ARGUMENT("ELI27462", pResult != NULL);
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI31339", ipFileRecord != __nullptr);

		long nFileID = ipFileRecord->FileID;

		// Validate the tags [LRCAU #5447]
		validateTags(ipDB);

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		// Perform the specified operation
		switch(m_operationType)
		{
		case kOperationApplyTags:
			addTagsToFile(ipDB, nFileID);
			break;

		case kOperationRemoveTags:
			removeTagsFromFile(ipDB, nFileID);
			break;

		case kOperationToggleTags:
			toggleTagsOnFile(ipDB, nFileID);
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI27463");
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27464")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27465");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27466");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31187", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31188");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI27467", pbValue != NULL);

		try
		{
			// Check license
			validateLicense();

			// If no exception was thrown, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27468");
}

//--------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::raw_IsConfigured(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// Ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI27469", pbValue != NULL);

		// Configured if:
		// 1. There is a tags vector
		// 2. The tag vector is not empty
		bool bIsConfigured = m_ipVecTags != NULL && m_ipVecTags->Size > 0;

		*pbValue = asVariantBool(bIsConfigured);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27470");
}

//--------------------------------------------------------------------------------------------------
// IPersistStream
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::GetClassID(CLSID* pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI27471", pClassID != NULL);

		*pClassID = CLSID_ManageTagsTask;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27472");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::Load(IStream* pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();

		// clear the options
		m_operationType = (EManageTagsOperationType) kOperationApplyTags;
		m_ipVecTags = NULL;
		
		// use a smart pointer for the IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI27473", ipStream != NULL);

		// read the bytestream data from the IStream object
		long nDataLength = 0;
		HANDLE_HRESULT(ipStream->Read(&nDataLength, sizeof(nDataLength), NULL), "ELI27474", 
			"Unable to read object size from stream.", ipStream, IID_IStream);
		ByteStream data(nDataLength);
		HANDLE_HRESULT(ipStream->Read(data.getData(), nDataLength, NULL), "ELI27475", 
			"Unable to read object from stream.", ipStream, IID_IStream);

		// read the data version
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// check if file is a newer version than this object can use
		if (nDataVersion > gnCurrentVersion)
		{
			// throw exception
			UCLIDException ue("ELI27476", "Unable to load newer ManageTagsTask.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// read the settings from the stream
		long lTemp;
		dataReader >> lTemp;
		m_operationType = (EManageTagsOperationType) lTemp;

		// Read the tag vector from the stream
		IPersistStreamPtr ipObj = NULL;
		readObjectFromStream(ipObj, pStream, "ELI27513");
		m_ipVecTags = ipObj;

		// clear the dirty flag since a new object was loaded
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27477");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::Save(IStream* pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();

		// create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;
		dataWriter << (long) m_operationType;

		// flush the data to the stream
		dataWriter.flushToByteStream();

		// use a smart pointer for IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI27478", ipStream != NULL);

		// write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		HANDLE_HRESULT(ipStream->Write(&nDataLength, sizeof(nDataLength), NULL), "ELI27479", 
			"Unable to write object size to stream.", ipStream, IID_IStream);
		HANDLE_HRESULT(ipStream->Write(data.getData(), nDataLength, NULL), "ELI27480", 
			"Unable to write object to stream.", ipStream, IID_IStream);

		// If there is a tags vector, write it to the stream
		if (m_ipVecTags != NULL)
		{
			IPersistStreamPtr ipPersist = m_ipVecTags;
			ASSERT_RESOURCE_ALLOCATION("ELI27514", ipPersist != NULL);

			writeObjectToStream(ipPersist, pStream, "ELI27515", fClearDirty);
		}

		// clear the flag as specified
		if(fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27481");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::GetSizeMax(ULARGE_INTEGER* pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return E_NOTIMPL;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::InterfaceSupportsErrorInfo(REFIID riid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		static const IID* arr[] = 
		{
			&IID_IManageTagsTask,
			&IID_ICategorizedComponent,
			&IID_ICopyableObject,
			&IID_IFileProcessingTask,
			&IID_ILicensedComponent,
			&IID_IMustBeConfiguredObject,
			&IID_IAccessRequired
		};

		for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
			{
				return S_OK;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27482");

	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// Private functions
//--------------------------------------------------------------------------------------------------
void CManageTagsTask::validateLicense()
{
	// ensure that add watermark is licensed
	VALIDATE_LICENSE(gnFILE_ACTION_MANAGER_OBJECTS, "ELI27483", "Manage Tags Task");
}
//--------------------------------------------------------------------------------------------------
void CManageTagsTask::validateTags(const IFileProcessingDBPtr& ipDB)
{
	try
	{
		// Get the list of tags from the database
		IVariantVectorPtr ipVecTags = ipDB->GetTagNames();
		ASSERT_RESOURCE_ALLOCATION("ELI27703", ipVecTags != NULL);

		// Get the size of the tags to operate on
		long lSize = m_ipVecTags->Size;
		for (long i=0; i < lSize; i++)
		{
			_variant_t vtTagName = m_ipVecTags->Item[i];
			if (ipVecTags->Contains(vtTagName) == VARIANT_FALSE)
			{
				UCLIDException uex("ELI27704", "Invalid tag: Tag no longer exists in database.");
				uex.addDebugInfo("Database Name", asString(ipDB->GetDatabaseName()));
				uex.addDebugInfo("Database Server", asString(ipDB->GetDatabaseServer()));
				uex.addDebugInfo("Tag Name", asString(vtTagName.bstrVal));
				throw uex;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27705");
}
//--------------------------------------------------------------------------------------------------
void CManageTagsTask::addTagsToFile(const IFileProcessingDBPtr &ipDB, long nFileID)
{
	try
	{
		ASSERT_ARGUMENT("ELI27486", ipDB != NULL);

		if (m_ipVecTags == NULL)
		{
			throw UCLIDException("ELI27511", "No tags to add to the file.");
		}
			
		// Loop through each tag and add it to the specified file
		long lSize = m_ipVecTags->Size;
		for (long i=0; i < lSize; i++)
		{
			ipDB->TagFile(nFileID, m_ipVecTags->Item[i].bstrVal);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27487");
}
//--------------------------------------------------------------------------------------------------
void CManageTagsTask::removeTagsFromFile(const IFileProcessingDBPtr &ipDB, long nFileID)
{
	try
	{
		ASSERT_ARGUMENT("ELI27488", ipDB != NULL);

		if (m_ipVecTags == NULL)
		{
			throw UCLIDException ("ELI27512", "No tags to remove from the file.");
		}
			
		// Loop through each tag and remove it from the specified file
		long lSize = m_ipVecTags->Size;
		for (long i=0; i < lSize; i++)
		{
			ipDB->UntagFile(nFileID, m_ipVecTags->Item[i].bstrVal);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27489");
}
//--------------------------------------------------------------------------------------------------
void CManageTagsTask::toggleTagsOnFile(const IFileProcessingDBPtr &ipDB, long nFileID)
{
	try
	{
		ASSERT_ARGUMENT("ELI27490", ipDB != NULL);

		if (m_ipVecTags == NULL)
		{
			throw UCLIDException ("ELI27516", "No tags to toggle on the file.");
		}
			
		// Loop through each tag and toggle it for the specified file
		long lSize = m_ipVecTags->Size;
		for (long i=0; i < lSize; i++)
		{
			ipDB->ToggleTagOnFile(nFileID, m_ipVecTags->Item[i].bstrVal);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27491");
}
//--------------------------------------------------------------------------------------------------
