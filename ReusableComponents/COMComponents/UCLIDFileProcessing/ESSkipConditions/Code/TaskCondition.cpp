// TaskCondition.cpp : Implementation of CTaskCondition

#include "stdafx.h"
#include "TaskCondition.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CTaskCondition
//-------------------------------------------------------------------------------------------------
CTaskCondition::CTaskCondition() :
	m_bLogExceptions(true)
{
	try
	{
		m_ipFAMTaskExecutor.CreateInstance(CLSID_FileProcessingTaskExecutor);
		ASSERT_RESOURCE_ALLOCATION("ELI20159", m_ipFAMTaskExecutor != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI20160");
}
//-------------------------------------------------------------------------------------------------
CTaskCondition::~CTaskCondition()
{
	try
	{
		m_ipTask = __nullptr;
		m_ipFAMTaskExecutor = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20076");
}
//-------------------------------------------------------------------------------------------------
HRESULT CTaskCondition::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CTaskCondition::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// ITaskCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::get_Task(IFileProcessingTask **ppVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20120", ppVal != __nullptr);

		validateLicense();

		IFileProcessingTaskPtr ipShallowCopy = m_ipTask;
		*ppVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20119");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::put_Task(IFileProcessingTask *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20121", pNewVal != __nullptr);

		validateLicense();

		m_ipTask = pNewVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20462");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::get_LogExceptions(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20169", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bLogExceptions);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20170");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::put_LogExceptions(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bLogExceptions = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20171");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IFAMCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::raw_FileMatchesFAMCondition(IFileRecord* pFileRecord, IFileProcessingDB* pFPDB, 
	long lActionID, IFAMTagManager* pFAMTM, VARIANT_BOOL* pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20082", pFAMTM != __nullptr);
		ASSERT_ARGUMENT("ELI20083", pRetVal != __nullptr);
		ASSERT_ARGUMENT("ELI31354", pFileRecord != __nullptr);

		validateLicense();

		// Ensure the task to execute has been set
		if (m_ipTask == __nullptr)
		{
			UCLIDException ue("ELI20157", "Task has not been configured for task condition to evaluate!");
			throw ue;
		}

		// Insert the task into a vector of object with description objects for processing
		IIUnknownVectorPtr ipTasks(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI20161", ipTasks != __nullptr);

		IObjectWithDescriptionPtr ipTaskOWD(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI20167", ipTaskOWD != __nullptr);

		ipTaskOWD->Object = m_ipTask;
		ipTasks->PushBack(ipTaskOWD);

		try
		{
			try
			{
				// Execute the task.
				// The condition is satisfied if the task completed without exception or cancellation
				EFileProcessingResult eResult = m_ipFAMTaskExecutor->InitProcessClose(
					pFileRecord, ipTasks, lActionID, pFPDB, pFAMTM, __nullptr, __nullptr,
					VARIANT_FALSE);
				*pRetVal = asVariantBool(eResult == kProcessingSuccessful);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20162");
		}
		catch (UCLIDException &ue)
		{
			// Exception was thrown; condition is false
			*pRetVal = VARIANT_FALSE;

			// Log the exception only if the user wants logs of failed condition tasks
			if (m_bLogExceptions)
			{
				UCLIDException uexOuter("ELI20173", 
					"Application trace: Task condition executed and evaluated as false", ue);
				uexOuter.log();
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20103");

	return S_OK;
}
	
//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31221", pbResult != __nullptr);

		*pbResult = m_ipTask->RequiresAdminAccess();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31222");
}

//--------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_ITaskCondition,
			&IID_IFAMCondition,
			&IID_IPersistStream,
			&IID_ICopyableObject,
			&IID_ICategorizedComponent,
			&IID_IMustBeConfiguredObject,
			&IID_ILicensedComponent,
			&IID_ISpecifyPropertyPages,
			&IID_IParallelizableTask,
			&IID_IIdentifiableObject
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20100")

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20101", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20102");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20093", pClassID != __nullptr);

		*pClassID = CLSID_TaskCondition;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20094");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20095");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IStreamPtr ipStream(pStream);
		ASSERT_ARGUMENT("ELI20096", ipStream != __nullptr);

		// Check license state
		validateLicense();

		// Reset existing members
		m_ipTask = __nullptr;
		m_bLogExceptions = true;
		
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		HANDLE_HRESULT(ipStream->Read(&nDataLength, sizeof(nDataLength), NULL), "ELI20180",
			"Unable to read object size from stream.", ipStream, __uuidof(IStream));
		ByteStream data(nDataLength);
		HANDLE_HRESULT(ipStream->Read(data.getData(), nDataLength, NULL), "ELI20182",
			"Unable to read object from stream.", ipStream, __uuidof(IStream));
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI20097", "Unable to load newer task condition!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read data members
		dataReader >> m_bLogExceptions;

		// Read the file processing task from the stream
		IPersistStreamPtr ipTaskStream;
		readObjectFromStream(ipTaskStream, ipStream, "ELI20142");
		m_ipTask = ipTaskStream;
		
		if (nDataVersion > 1)
		{
			loadGUID(ipStream);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20104");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IStreamPtr ipStream(pStream);
		ASSERT_ARGUMENT("ELI20098", ipStream != __nullptr);

		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCurrentVersion;
		
		// Write data members;
		dataWriter << m_bLogExceptions;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		HANDLE_HRESULT(ipStream->Write(&nDataLength, sizeof(nDataLength), NULL), "ELI20183",
			"Unable to write object size to stream.", ipStream, __uuidof(IStream));
		HANDLE_HRESULT(ipStream->Write(data.getData(), nDataLength, NULL), "ELI20185",
			"Unable to write object to stream.", ipStream, __uuidof(IStream));

		// Write the file processing task to the stream
		IPersistStreamPtr ipTaskStream = m_ipTask;
		ASSERT_RESOURCE_ALLOCATION("ELI20143", ipTaskStream != __nullptr);
		writeObjectToStream(ipTaskStream, ipStream, "ELI20144", fClearDirty);

		saveGUID(ipStream);

		// Clear the flag as specified
		if (fClearDirty == TRUE)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20099");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20090", pObject != __nullptr);
			
		// Validate license
		validateLicense();

		EXTRACT_FAMCONDITIONSLib::ITaskConditionPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI20088", ipCopyThis != __nullptr);

		m_bLogExceptions = asCppBool(ipCopyThis->LogExceptions);

		if (ipCopyThis->Task == NULL)
		{
			m_ipTask = __nullptr;
		}
		else
		{
			// If Task is not NULL, obtain a clone
			ICopyableObjectPtr ipCopyableTask(ipCopyThis->Task);
			ASSERT_RESOURCE_ALLOCATION("ELI20145", ipCopyableTask != __nullptr);
			m_ipTask = ipCopyableTask->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI20146", m_ipTask != __nullptr);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20089");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20105", pObject != __nullptr);

		// Validate license
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_TaskCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI20091", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20092");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI20084", pbValue != __nullptr);

		// Check license
		validateLicense();

		// Default to unconfigured
		*pbValue = VARIANT_FALSE;

		// Must have a task to be configured
		if (m_ipTask)
		{
			IMustBeConfiguredObjectPtr ipTaskConfig = m_ipTask;

			// If the task doesn't implement IMustBeConfiguredObject or if 
			// IMustBeConfiguredObject::IsConfigured == VARIANT_TRUE, consider
			// this condition configured
			if (ipTaskConfig == __nullptr || ipTaskConfig->IsConfigured())
			{
				*pbValue = VARIANT_TRUE;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20085");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20086", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Task condition").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20087")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// IParallelizableTask Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::raw_ProcessWorkItem(IWorkItemRecord *pWorkItem, long nActionID,
		IFAMTagManager* pFAMTM, IFileProcessingDB* pDB, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// Check license
		validateLicense();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37153");	
}

//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::get_Parallelize(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		IParallelizableTaskPtr ipTask(m_ipTask);
		*pVal = (ipTask == __nullptr) ? VARIANT_FALSE : ipTask->Parallelize;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37150");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::put_Parallelize(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// This is only parallelizable if the contained task is parallelizable

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37151");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskCondition::get_InstanceGUID(GUID * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*pVal = getGUID();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37152");
}
//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CTaskCondition::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI20077", "Task FAM Condition");
}
//-------------------------------------------------------------------------------------------------
