// ConditionalTask.cpp : Implementation of CConditionalTask

#include "stdafx.h"
#include "FileProcessors.h"
#include "ConditionalTask.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <FAMHelperFunctions.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// global variables / static variables / externs
extern CComModule _Module;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//--------------------------------------------------------------------------------------------------
// CConditionalTask
//--------------------------------------------------------------------------------------------------
CConditionalTask::CConditionalTask()
:	m_bDirty(false),
	m_ipFAMCondition(NULL),
	m_ipTasksForTrue(NULL),
	m_ipTasksForFalse(NULL),
	m_ipFAMTaskExecutor(NULL),
	m_ipMiscUtils(NULL)
{
	try
	{
		m_ipFAMTaskExecutor.CreateInstance(CLSID_FileProcessingTaskExecutor);
		ASSERT_RESOURCE_ALLOCATION("ELI17767", m_ipFAMTaskExecutor != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17941");
}
//--------------------------------------------------------------------------------------------------
CConditionalTask::~CConditionalTask()
{
	try
	{
		// Set COM pointers to NULL
		m_ipFAMCondition = __nullptr;
		m_ipTasksForTrue = __nullptr;
		m_ipTasksForFalse = __nullptr;
		m_ipFAMTaskExecutor = __nullptr;
		m_ipMiscUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16610")
}
//--------------------------------------------------------------------------------------------------
HRESULT CConditionalTask::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CConditionalTask::FinalRelease()
{
	try
	{
		// Set COM pointers to NULL
		m_ipFAMCondition = __nullptr;
		m_ipTasksForTrue = __nullptr;
		m_ipTasksForFalse = __nullptr;
		m_ipFAMTaskExecutor = __nullptr;
		m_ipMiscUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27261");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_IFileProcessingTask,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_IConditionalTask,
		&IID_IParallelizableTask,
		&IID_IIdentifiableObject
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
	{
		return E_POINTER;
	}

	try
	{
		// Validate the license
		validateLicense();
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
STDMETHODIMP CConditionalTask::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19611", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t("Core: Conditionally execute task(s)").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16112")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create the other Conditional Task object
		UCLID_FILEPROCESSORSLib::IConditionalTaskPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI16170", ipSource != __nullptr);

		// Copy the FAMCondition
		ICopyableObjectPtr ipCopyableObject = ipSource->FAMCondition;
		ASSERT_RESOURCE_ALLOCATION("ELI16624", ipCopyableObject != __nullptr);
		m_ipFAMCondition = ipCopyableObject->Clone();

		// Copy the collection of tasks to be executed if condition is true
		ipCopyableObject = ipSource->TasksForConditionTrue;
		ASSERT_RESOURCE_ALLOCATION("ELI16625", ipCopyableObject != __nullptr);
		m_ipTasksForTrue = ipCopyableObject->Clone();

		// Copy the collection of tasks to be executed if condition is false
		ipCopyableObject = ipSource->TasksForConditionFalse;
		ASSERT_RESOURCE_ALLOCATION("ELI16626", ipCopyableObject != __nullptr);
		m_ipTasksForFalse = ipCopyableObject->Clone();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16171");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_ConditionalTask);
		ASSERT_RESOURCE_ALLOCATION("ELI16172", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16173");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IClipboardCopyable
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_NotifyCopiedFromClipboard()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check the condition object itself
		if (m_ipFAMCondition != __nullptr)
		{
			IClipboardCopyablePtr ipClip = m_ipFAMCondition->Object;
			if (ipClip != __nullptr)
			{
				ipClip->NotifyCopiedFromClipboard();
			}
		}

		// Check each object in the conditional lists
		notifyClipboardCopiedForTask(m_ipTasksForTrue);
		notifyClipboardCopiedForTask(m_ipTasksForFalse);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27260");
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Default state to Configured
		bool bConfigured = true;

		// The FAMCondition must be defined
		if ((m_ipFAMCondition == __nullptr) || (m_ipFAMCondition->Object == NULL))
		{
			bConfigured = false;
		}
		else
		{
			// At least one task must be enabled
			bConfigured = getMiscUtils()->CountEnabledObjectsIn(m_ipTasksForTrue) > 0
				|| getMiscUtils()->CountEnabledObjectsIn(m_ipTasksForFalse) > 0;
		}

		// Return result to caller
		*pbValue = asVariantBool( bConfigured );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16265");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Not needed... true/false tasks will be initialized/closed per file
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16580");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_ProcessFile(IFileRecord* pFileRecord, long nActionID,
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17908", pTagManager != __nullptr);
		ASSERT_ARGUMENT("ELI17910", pResult != __nullptr);
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI31337", ipFileRecord != __nullptr);

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		// Retrieve the FAM Condition from the Object-With-Description
		IFAMConditionPtr ipFAMCondition = m_ipFAMCondition->Object;
		ASSERT_ARGUMENT( "ELI17041", ipFAMCondition != __nullptr );

		// Set weighting for condition progress status.
		// At this point we can't know the number of tasks to execute.  Guesstimate 
		// task execution will take twice as long as the evaluation of the condition.
		const long nNUM_PROGRESS_ITEMS_CONDITION_EVALUATION = 1;
		const long nNUM_PROGRESS_ITEMS_TASK_EXECUTION = 2;
		long nTOTAL_PROGRESS_ITEMS = nNUM_PROGRESS_ITEMS_CONDITION_EVALUATION +
									 nNUM_PROGRESS_ITEMS_TASK_EXECUTION;

		// No need to assert pProgressStatus-- NULL is valid; it means progress status information is not requested
		IProgressStatusPtr ipProgressStatus = pProgressStatus;
		if (ipProgressStatus)
		{
			// Initialize progress status
			ipProgressStatus->InitProgressStatus("", 0, nTOTAL_PROGRESS_ITEMS, VARIANT_TRUE);
			ipProgressStatus->StartNextItemGroup("Evaluating condition...", nNUM_PROGRESS_ITEMS_CONDITION_EVALUATION);
		}

		// Exercise the FAM Condition
		bool bConditionSatisfied = asCppBool(ipFAMCondition->FileMatchesFAMCondition(
			ipFileRecord, pDB, nActionID, pTagManager));

		// Kick off progress status for task execution
		IProgressStatusPtr ipSubProgressStatus = __nullptr;
		if (ipProgressStatus)
		{
			string strMessage = "Executing tasks for ";
			strMessage += (bConditionSatisfied ? "satisfied" : "unsatisfied");
			strMessage += " condition...";

			ipProgressStatus->StartNextItemGroup(strMessage.c_str(), nNUM_PROGRESS_ITEMS_TASK_EXECUTION);
			ipSubProgressStatus = ipProgressStatus->SubProgressStatus;
		}

		if (bConditionSatisfied)
		{
			// Ensure there are tasks to execute before attempting to execute them
			if (m_ipTasksForTrue != __nullptr && m_ipTasksForTrue->Size() > 0)
			{
				// Execute true tasks
				*pResult = m_ipFAMTaskExecutor->InitProcessClose(ipFileRecord, 
					m_ipTasksForTrue, nActionID, pDB, pTagManager, ipSubProgressStatus,
					bCancelRequested);
			}
		}
		else
		{
			// Ensure there are tasks to execute before attempting to execute them
			if (m_ipTasksForFalse != __nullptr && m_ipTasksForFalse->Size() > 0)
			{
				// Execute false tasks
				*pResult = m_ipFAMTaskExecutor->InitProcessClose(ipFileRecord, 
					m_ipTasksForFalse, nActionID, pDB, pTagManager, ipSubProgressStatus,
					bCancelRequested);
			}
		}

		// Updated progress status to indicate completion
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16194")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Need to inform Executor to pass on cancel request as necessary
		m_ipFAMTaskExecutor->Cancel();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19442");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Not needed... true/false tasks will be initialized/closed per file
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16582");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_Standby(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{		
		ASSERT_ARGUMENT("ELI33900", pVal != __nullptr);

		*pVal = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33901");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::get_MinStackSize(unsigned long *pnMinStackSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35007", pnMinStackSize != __nullptr);

		validateLicense();

		unsigned long ulMinStackSize = 0;

		// Check the MinStackSize parameter for all "true" and "false" tasks so that the returned
		// value is the maximum of any task that may be called by this condition.
		if (m_ipTasksForTrue != __nullptr)
		{
			int nTaskCount = m_ipTasksForTrue->Size();
			for (int i = 0; i < nTaskCount ; i++)
			{
				IObjectWithDescriptionPtr ipOWD = m_ipTasksForTrue->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI35033", ipOWD != __nullptr);

				UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipFileProcessor(ipOWD->Object);
				ASSERT_RESOURCE_ALLOCATION("ELI35034", ipFileProcessor != __nullptr);

				ulMinStackSize = max(ulMinStackSize, ipFileProcessor->MinStackSize);
			}
		}

		if (m_ipTasksForFalse != __nullptr)
		{
			int nTaskCount = m_ipTasksForFalse->Size();
			for (int i = 0; i < nTaskCount ; i++)
			{
				IObjectWithDescriptionPtr ipOWD = m_ipTasksForFalse->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI35035", ipOWD != __nullptr);

				UCLID_FILEPROCESSINGLib::IFileProcessingTaskPtr ipFileProcessor(ipOWD->Object);
				ASSERT_RESOURCE_ALLOCATION("ELI35036", ipFileProcessor != __nullptr);

				ulMinStackSize = max(ulMinStackSize, ipFileProcessor->MinStackSize);
			}
		}

		*pnMinStackSize = ulMinStackSize;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35008");
}
	
//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31179", pbResult != __nullptr);

		*pbResult = asVariantBool(checkForRequiresAdminAccess(m_ipFAMCondition));

		if (*pbResult == VARIANT_FALSE)
		{
			*pbResult = (checkForRequiresAdminAccess(m_ipTasksForTrue) || 
				checkForRequiresAdminAccess(m_ipTasksForFalse)) ? VARIANT_TRUE : VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31180");
}

//-------------------------------------------------------------------------------------------------
// IConditionalTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::get_FAMCondition(IObjectWithDescription* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Create the condition object-with-description if needed
		if (m_ipFAMCondition == __nullptr)
		{
			m_ipFAMCondition.CreateInstance( CLSID_ObjectWithDescription );
			ASSERT_RESOURCE_ALLOCATION( "ELI16121", m_ipFAMCondition != __nullptr );
		}

		// Return shallow copy of the OWD
		IObjectWithDescriptionPtr ipShallowCopy = m_ipFAMCondition;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16122")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::put_FAMCondition(IObjectWithDescription *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Store provided OWD and set the dirty flag
		m_ipFAMCondition = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16123")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::get_TasksForConditionTrue(IIUnknownVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create collection, if needed
		if (m_ipTasksForTrue == __nullptr)
		{
			m_ipTasksForTrue.CreateInstance( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI16124", m_ipTasksForTrue != __nullptr );
		}

		// Provide shallow copy to caller
		IIUnknownVectorPtr ipShallowCopy = m_ipTasksForTrue;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16125")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::put_TasksForConditionTrue(IIUnknownVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Save collection and update dirty flag
		m_ipTasksForTrue = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16126")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::get_TasksForConditionFalse(IIUnknownVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create collection, if needed
		if (m_ipTasksForFalse == __nullptr)
		{
			m_ipTasksForFalse.CreateInstance( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI16127", m_ipTasksForFalse != __nullptr );
		}

		// Provide shallow copy to caller
		IIUnknownVectorPtr ipShallowCopy = m_ipTasksForFalse;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16128")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::put_TasksForConditionFalse(IIUnknownVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Save collection and update dirty flag
		m_ipTasksForFalse = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16129")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ConditionalTask;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Is the conditional itself dirty?
		if (m_bDirty)
		{
			// S_OK implies dirty
			return S_OK;
		}
		// Are any of the sub-tasks dirty?
		else
		{
			// Check dirty state of each object
			bool bDirty = asCppBool(getMiscUtils()->IsAnyObjectDirty3( m_ipFAMCondition, 
				m_ipTasksForTrue, m_ipTasksForFalse ));
			if (bDirty)
			{
				// S_OK implies dirty
				return S_OK;
			}
		}

		// S_FALSE implies not dirty
		return S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16631");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Reset member variables
		m_ipFAMCondition = __nullptr;
		m_ipTasksForTrue = __nullptr;
		m_ipTasksForFalse = __nullptr;

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
			UCLIDException ue("ELI16182", "Unable to load newer Conditional Task component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read in the FAM Condition with its Description
		IPersistStreamPtr ipFAMObj;
		readObjectFromStream( ipFAMObj, pStream, "ELI16185" );
		m_ipFAMCondition = ipFAMObj;

		// Read in the collection of tasks to be executed if Condition is true
		IPersistStreamPtr ipTrueObj;
		readObjectFromStream( ipTrueObj, pStream, "ELI16186" );
		m_ipTasksForTrue = ipTrueObj;

		// Read in the collection of tasks to be executed if Condition is false
		IPersistStreamPtr ipFalseObj;
		readObjectFromStream( ipFalseObj, pStream, "ELI16187" );
		m_ipTasksForFalse = ipFalseObj;

		if (nDataVersion > 1)
		{
			loadGUID(pStream);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16183");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Save version information
		dataWriter << gnCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Write the FAM Condition
		IPersistStreamPtr ipFAMObj = m_ipFAMCondition;
		ASSERT_RESOURCE_ALLOCATION( "ELI16188", ipFAMObj != __nullptr );
		writeObjectToStream( ipFAMObj, pStream, "ELI16189", fClearDirty );

		// Write the collection of tasks for Condition = true
		IPersistStreamPtr ipTrueObj = m_ipTasksForTrue;
		ASSERT_RESOURCE_ALLOCATION( "ELI16190", ipTrueObj != __nullptr );
		writeObjectToStream( ipTrueObj, pStream, "ELI16191", fClearDirty );

		// Write the collection of tasks for Condition = false
		IPersistStreamPtr ipFalseObj = m_ipTasksForFalse;
		ASSERT_RESOURCE_ALLOCATION( "ELI16192", ipFalseObj != __nullptr );
		writeObjectToStream( ipFalseObj, pStream, "ELI16193", fClearDirty );

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16184");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IParallelizableTask Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::raw_ProcessWorkItem(IWorkItemRecord *pWorkItem, long nActionID,
		IFAMTagManager* pFAMTM, IFileProcessingDB* pDB, IProgressStatus *pProgressStatus)
{
	try
	{
		// This does not create work items
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37146");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::get_Parallelize(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// check all contained objects to determine if this parallelizable
		// Check the condition object
		IParallelizableTaskPtr ipParallelTask = m_ipFAMCondition->Object;
		bool bParallelizable = false;
		if (ipParallelTask != __nullptr)
		{
			bParallelizable = asCppBool(ipParallelTask->Parallelize);
		}
		if (!bParallelizable)
		{
			// Check the false tasks
			bParallelizable = containsParallelizableTask(m_ipTasksForFalse);
			if (!bParallelizable)
			{
				//check the true tasks
				bParallelizable = containsParallelizableTask(m_ipTasksForTrue);
			}
		}

		*pVal = asVariantBool(bParallelizable);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37147");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::put_Parallelize(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// The conditional task is only parallelizable if it contains a parallelizable task

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37148");
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTask::get_InstanceGUID(GUID * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*pVal = getGUID();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37149");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CConditionalTask::getMiscUtils()
{
	if (m_ipMiscUtils == __nullptr)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI19441", m_ipMiscUtils != __nullptr );
	}
	
	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
void CConditionalTask::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFILE_ACTION_MANAGER_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI16109", "Conditional Task File Processor" );
}
//-------------------------------------------------------------------------------------------------
void CConditionalTask::notifyClipboardCopiedForTask(const IIUnknownVectorPtr& ipTasks)
{
	try
	{
		if (ipTasks == __nullptr)
		{
			return;
		}

		// Get the number of tasks
		long nCount = ipTasks->Size();

		// Check each item in the task list
		for (long i = 0; i < nCount; i++)
		{
			// Get the OWD from the task list
			IObjectWithDescriptionPtr ipTemp = ipTasks->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI27263", ipTemp != __nullptr);
			
			// Check for OWD containing ClipboardCopyable object
			IClipboardCopyablePtr ipClip = ipTemp->Object;
			if (ipClip != __nullptr)
			{
				// Notify that the object has been copied
				ipClip->NotifyCopiedFromClipboard();
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27262");
}
//-------------------------------------------------------------------------------------------------
bool CConditionalTask::containsParallelizableTask(IIUnknownVectorPtr ipTaskList)
{
	long nSize = ipTaskList->Size();
	for (long i = 0; i < nSize; i++ )
	{
		IObjectWithDescriptionPtr ipOWD = ipTaskList->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI37266", ipOWD != __nullptr);

		IParallelizableTaskPtr ipTask = ipOWD->Object;
		if (ipTask != __nullptr)
		{
			// Only one needs to be parallelizable.
			if (ipTask->Parallelize == VARIANT_TRUE)
			{
				return true;
			}
		}
	}
	return false;
}