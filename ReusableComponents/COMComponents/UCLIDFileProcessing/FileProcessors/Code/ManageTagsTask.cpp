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
#include "ManageTagsConstants.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ByteStream.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

// component description
const string gstrMANAGE_TAGS_COMPONENT_DESCRIPTION = "Core: Manage tags";

const string gstrDYNAMIC_DESCRIPTION = "Dynamically added by the Manage tags task.";

//--------------------------------------------------------------------------------------------------
// CManageTagsTask
//--------------------------------------------------------------------------------------------------
CManageTagsTask::CManageTagsTask() :
m_operationType(kOperationApplyTags),
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
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27442");
}
//--------------------------------------------------------------------------------------------------
void CManageTagsTask::FinalRelease()
{
	try
	{
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

		ASSERT_ARGUMENT("ELI27444", pVal != __nullptr);

		// Get the operation type
		*pVal = m_operationType;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27445");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::put_Tags(BSTR bstrTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		StringTokenizer::sGetTokens(asString(bstrTags), gstrTAG_DELIMITER, m_vecTags);
		
		// Set the dirty flag
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27447");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTask::get_Tags(BSTR* pbstrTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI27448", pbstrTags != __nullptr);


		// Return the variant vector
		*pbstrTags = _bstr_t(tokenizeTags().c_str()).Detach();

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
		ASSERT_ARGUMENT("ELI27451", pstrComponentDescription != __nullptr);
		
		*pstrComponentDescription = 
			_bstr_t(gstrMANAGE_TAGS_COMPONENT_DESCRIPTION.c_str()).Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27452");
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
		ASSERT_RESOURCE_ALLOCATION("ELI27453", ipManageTagsTask != __nullptr);

		// Copy the operation from the object
		m_operationType = (EManageTagsOperationType) ipManageTagsTask->Operation;

		// Copy the tags
		string strTags = asString(ipManageTagsTask->Tags);
		StringTokenizer::sGetTokens(strTags, gstrTAG_DELIMITER, m_vecTags);

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
		ASSERT_ARGUMENT("ELI27456", ppObject != __nullptr);

		// Get the copyable object interface
		ICopyableObjectPtr ipObjCopy(CLSID_ManageTagsTask);
		ASSERT_RESOURCE_ALLOCATION("ELI27457", ipObjCopy != __nullptr);

		// Create a shallow copy
		IUnknownPtr ipUnknown(this);
		ASSERT_RESOURCE_ALLOCATION("ELI27458", ipUnknown != __nullptr);
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
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI27461", ipDB != __nullptr);
		ASSERT_ARGUMENT("ELI27462", pResult != __nullptr);
		ASSERT_ARGUMENT("ELI31339", ipFileRecord != __nullptr);
		ASSERT_ARGUMENT("ELI31997", pTagManager != __nullptr);

		// Get file ID and source doc name
		long nFileID = ipFileRecord->FileID;
		string strSourceDoc = asString(ipFileRecord->Name);

		// Build expanded list of tags
		vector<string> vecTags;
		for(vector<string>::iterator it = m_vecTags.begin(); it != m_vecTags.end(); it++)
		{
			// Check if string contains expansion symbols
			if (it->find_first_of("<$") != string::npos)
			{
				vecTags.push_back(CFileProcessorsUtils::ExpandTagsAndTFE(
					pTagManager, *it, strSourceDoc));
			}
			else
			{
				vecTags.push_back(*it);
			}
		}

		// Validate/add tags as needed (Do not add tags if removing)
		validateAndAddTags(vecTags, ipDB, m_operationType != kOperationRemoveTags);

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		// Perform the specified operation
		switch(m_operationType)
		{
		case kOperationApplyTags:
			addTagsToFile(vecTags, ipDB, nFileID);
			break;

		case kOperationRemoveTags:
			removeTagsFromFile(vecTags, ipDB, nFileID);
			break;

		case kOperationToggleTags:
			toggleTagsOnFile(vecTags, ipDB, nFileID);
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI27463");
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27464");
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
		ASSERT_ARGUMENT("ELI27467", pbValue != __nullptr);

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
		ASSERT_ARGUMENT("ELI27469", pbValue != __nullptr);

		// Configured if:
		// 1. The tag vector is not empty
		*pbValue = asVariantBool(!m_vecTags.empty());

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
		ASSERT_ARGUMENT("ELI27471", pClassID != __nullptr);

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
		m_vecTags.clear();

		// clear the options
		m_operationType = (EManageTagsOperationType) kOperationApplyTags;
		
		// use a smart pointer for the IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI27473", ipStream != __nullptr);

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

		if (nDataVersion == 1)
		{
			// Read the tag vector from the stream
			IPersistStreamPtr ipObj = __nullptr;
			readObjectFromStream(ipObj, pStream, "ELI27513");
			IVariantVectorPtr ipTags = ipObj;
			ASSERT_RESOURCE_ALLOCATION("ELI31986", ipTags != __nullptr);

			long nSize = ipTags->Size;
			for(int i=0; i < nSize; i++)
			{
				m_vecTags.push_back(asString(ipTags->Item[i].bstrVal));
			}
		}
		else
		{
			string strTemp;
			dataReader >> strTemp;
			StringTokenizer::sGetTokens(strTemp, gstrTAG_DELIMITER, m_vecTags);
		}

		// clear the dirty flag since a new object was loaded
		m_bDirty = false;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27477");
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
		dataWriter << tokenizeTags();

		// flush the data to the stream
		dataWriter.flushToByteStream();

		// use a smart pointer for IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI27478", ipStream != __nullptr);

		// write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		HANDLE_HRESULT(ipStream->Write(&nDataLength, sizeof(nDataLength), NULL), "ELI27479", 
			"Unable to write object size to stream.", ipStream, IID_IStream);
		HANDLE_HRESULT(ipStream->Write(data.getData(), nDataLength, NULL), "ELI27480", 
			"Unable to write object to stream.", ipStream, IID_IStream);

		// clear the flag as specified
		if(fClearDirty)
		{
			m_bDirty = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27481");
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
//--------------------------------------------------------------------------------------------------
void CManageTagsTask::validateAndAddTags(const vector<string>& vecTags,
	const IFileProcessingDBPtr& ipDB, bool bAddIfMissing)
{
	try
	{
		bool bAllowDynamicTags = asCppBool(ipDB->AllowDynamicTagCreation());

		// Get the list of tags from the database
		IVariantVectorPtr ipVecTags = ipDB->GetTagNames();
		ASSERT_RESOURCE_ALLOCATION("ELI27703", ipVecTags != __nullptr);

		for(vector<string>::const_iterator it = vecTags.begin(); it != vecTags.end(); it++)
		{
			if (ipVecTags->Contains(it->c_str()) == VARIANT_FALSE)
			{
				if (!bAllowDynamicTags)
				{
					UCLIDException uex("ELI27704", "Invalid tag: Tag does not exist in database.");
					uex.addDebugInfo("Database Name", asString(ipDB->GetDatabaseName()));
					uex.addDebugInfo("Database Server", asString(ipDB->GetDatabaseServer()));
					uex.addDebugInfo("Tag Name", *it);
					throw uex;
				}
				else if(bAddIfMissing)
				{
					ipDB->AddTag(it->c_str(), gstrDYNAMIC_DESCRIPTION.c_str(), VARIANT_FALSE); 
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27705");
}
//--------------------------------------------------------------------------------------------------
void CManageTagsTask::addTagsToFile(const vector<string>& vecTags,
	const IFileProcessingDBPtr &ipDB, long nFileID)
{
	try
	{
		ASSERT_ARGUMENT("ELI27486", ipDB != __nullptr);

		if (m_vecTags.empty())
		{
			throw UCLIDException("ELI27511", "No tags to add to the file.");
		}
			
		for(vector<string>::const_iterator it = vecTags.begin(); it != vecTags.end(); it++)
		{
			ipDB->TagFile(nFileID, it->c_str());
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27487");
}
//--------------------------------------------------------------------------------------------------
void CManageTagsTask::removeTagsFromFile(const vector<string>& vecTags,
	const IFileProcessingDBPtr &ipDB, long nFileID)
{
	try
	{
		ASSERT_ARGUMENT("ELI27488", ipDB != __nullptr);

		if (m_vecTags.empty())
		{
			throw UCLIDException ("ELI27512", "No tags to remove from the file.");
		}
			
		// Loop through each tag and remove it from the specified file
		for(vector<string>::const_iterator it = vecTags.begin(); it != vecTags.end(); it++)
		{
			ipDB->UntagFile(nFileID, it->c_str());
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27489");
}
//--------------------------------------------------------------------------------------------------
void CManageTagsTask::toggleTagsOnFile(const vector<string>& vecTags,
	const IFileProcessingDBPtr &ipDB, long nFileID)
{
	try
	{
		ASSERT_ARGUMENT("ELI27490", ipDB != __nullptr);

		if (m_vecTags.empty())
		{
			throw UCLIDException ("ELI27516", "No tags to toggle on the file.");
		}
			
		// Loop through each tag and toggle it for the specified file
		for(vector<string>::const_iterator it = vecTags.begin(); it != vecTags.end(); it++)
		{
			ipDB->ToggleTagOnFile(nFileID, it->c_str());
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27491");
}
//--------------------------------------------------------------------------------------------------
string CManageTagsTask::tokenizeTags()
{
	string strResult;
	if (m_vecTags.size() > 0)
	{
		vector<string>::iterator it = m_vecTags.begin();
		strResult = *it;
		for(++it; it != m_vecTags.end(); it++)
		{
			strResult += gstrTAG_DELIMITER;
			strResult += *it;
		}
	}

	return strResult;
}
//--------------------------------------------------------------------------------------------------
