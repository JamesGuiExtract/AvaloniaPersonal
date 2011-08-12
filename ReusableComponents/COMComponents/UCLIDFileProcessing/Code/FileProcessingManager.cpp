// FileProcessingManager.cpp : Implementation of CFileProcessingManager

#include "stdafx.h"
#include "UCLIDFileProcessing.h"
#include "FileProcessingManager.h"
#include "FP_UI_Notifications.h"
#include "FileProcessingUtils.h"
#include "CommonConstants.h"

#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <cpputil.h>
#include <FAMUtilsConstants.h>
#include <FileProcessingConfigMgr.h>
#include <LicenseMgmt.h>
#include <misc.h>
#include <RegistryPersistenceMgr.h>
#include <StopWatch.h>
#include <StringTokenizer.h>
#include <TextFunctionExpander.h>
#include <UCLIDException.h>

using namespace ADODB;

DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const std::string gstrSTREAM_NAME = "FileProcessingManager";

//-------------------------------------------------------------------------------------------------
// CFileProcessingManager
//-------------------------------------------------------------------------------------------------
CFileProcessingManager::CFileProcessingManager()
: m_ipFPMDB(NULL),
m_strPreviousDBServer(""),
m_strPreviousDBName(""),
m_isDBConnectionReady(false),
m_nNumberOfFilesToExecute(0),
m_bCancelling(false),
m_bRecordFAMSessions(false),
m_bIsAuthenticated(false),
m_nMaxFilesFromDB(gnMAX_NUMBER_OF_FILES_FROM_DB)
{
	try
	{
		// create the file supplying mgmt role object
		m_ipFSMgmtRole.CreateInstance(CLSID_FileSupplyingMgmtRole);
		ASSERT_RESOURCE_ALLOCATION("ELI14213", m_ipFSMgmtRole != __nullptr);

		// create the file processing mgmt role object
		m_ipFPMgmtRole.CreateInstance(CLSID_FileProcessingMgmtRole);
		ASSERT_RESOURCE_ALLOCATION("ELI14311", m_ipFPMgmtRole != __nullptr);

		// create the FPMTagManager object
		m_ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI14398", m_ipFAMTagManager != __nullptr);

		// reset the state of this object
		clear();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10662");
}
//-------------------------------------------------------------------------------------------------
CFileProcessingManager::~CFileProcessingManager()
{
	try
	{
		clear();
		m_ipFPMDB = __nullptr;
		m_ipFSMgmtRole = __nullptr;
		m_ipFPMgmtRole = __nullptr;
		m_ipFAMTagManager = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14256")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFileProcessingManager,
		&IID_IRoleNotifyFAM,
		&IID_IPersistStream,
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
// IFileProcessingManager
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::ShowUI(VARIANT_BOOL bRunOnInit, VARIANT_BOOL bCloseOnComplete, VARIANT_BOOL bForceClose, int iNumDocsToExecute, void * pFRM) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr ipFileProcMgr(this);
		m_apDlg = unique_ptr<FileProcessingDlg>(new FileProcessingDlg(ipFileProcMgr, getFPMDB(), pFRM));

		m_apDlg->setRecordManager(&m_recordMgr);

		m_apDlg->setRunOnInit(bRunOnInit == VARIANT_TRUE);
		m_apDlg->setCloseOnComplete(bCloseOnComplete == VARIANT_TRUE);
		m_apDlg->setForceCloseOnComplete(bForceClose == VARIANT_TRUE);
		m_nNumberOfFilesToExecute = iNumDocsToExecute;

		m_apDlg->DoModal();
		m_apDlg.reset();

		// Call the clear method to make sure everything is cleaned up
		clear();

		// Release the memory for Roles
		// This will allow FileProcessingManager to get destroyed since these
		// contain smart pointer to the FileProcessingManager
		m_ipFPMgmtRole = __nullptr;
		m_ipFSMgmtRole = __nullptr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08846")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::StartProcessing()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Set the record manager object in Processing management role
		// Record manager must be defined otherwise each file fails (P13 #3887)
		m_ipFPMgmtRole->SetRecordMgr(&m_recordMgr);

		// if this method is being called when processing has already started
		// that means that the processing has been paused, and we need to unpause it
		if (m_bProcessing || m_bSupplying)
		{
			if (m_bPaused)
			{
				// update the internal flag to note that the processing is no longer paused
				m_bPaused = false;

				// resume the file processing role
				UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipProcessingActionMgmtRole =
					getActionMgmtRole(m_ipFPMgmtRole);
				if (ipProcessingActionMgmtRole->Enabled == VARIANT_TRUE)
				{
					ipProcessingActionMgmtRole->Resume();
				}

				// resume the file supplying role
				UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipSupplyingActionMgmtRole =
					getActionMgmtRole(m_ipFSMgmtRole);
				if (ipSupplyingActionMgmtRole->Enabled == VARIANT_TRUE)
				{
					ipSupplyingActionMgmtRole->Resume();
				}

				return S_OK;
			}
			else
			{
				// this method should not be called when processing is already taking place.
				throw UCLIDException("ELI12733", "StartProcessing() cannot be called when processing is currently taking place!");
			}
		}

		// Make sure that the FPS file includes directory information (P13 #4502)
		if ((m_strFPSFileName != "") && (!isAbsolutePath(m_strFPSFileName)))
		{
			m_strFPSFileName = buildAbsolutePath(m_strFPSFileName);
		}

		// Expand the action name
		string strExpandedAction = getExpandedActionName();

		// Validate that the action name exists in the database (auto-create if that setting is set)
		long lActionId = getFPMDB()->AutoCreateAction(strExpandedAction.c_str());

		// start the processing
		m_bPaused = false;
		m_bCancelling = false;

		// Reset the DB Connection
		getFPMDB()->ResetDBConnection();

		// Check whether or not FAM Session history should be recorded
		m_bRecordFAMSessions = asString(
			getFPMDB()->GetDBInfoSetting(gstrSTORE_FAM_SESSION_HISTORY.c_str(), VARIANT_TRUE)) == "1";

		// Set the number of files to process
		m_recordMgr.setNumberOfFilesToProcess(m_nNumberOfFilesToExecute);

		// Set the max number of files to get from the DB
		m_recordMgr.setMaxNumberOfFilesFromDB(m_nMaxFilesFromDB);

		// if there is a dialog set it to receive status updates
		if(m_apDlg.get() != __nullptr)
		{
			m_recordMgr.setDlg(m_apDlg->m_hWnd);
		}

		// clear all the records in the file processing record manager
		// (i.e. clear the queue of files to process)
		m_recordMgr.clear(true);

		// get pointer to the RoleNotifyFAM interface
		UCLID_FILEPROCESSINGLib::IRoleNotifyFAMPtr ipThis(this);

		// Log start processing information
		EStartStopStatus eStatus = kStart;
		logStatusInfo(eStatus);

		UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipSupplyingActionMgmtRole =
			getActionMgmtRole(m_ipFSMgmtRole);

		UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipProcessingActionMgmtRole =
				getActionMgmtRole(m_ipFPMgmtRole);
		
		// Register this FAM as active (allows for files stuck processing to be reverted)
		m_ipFPMDB->RegisterActiveFAM(lActionId, ipSupplyingActionMgmtRole->Enabled,
			ipProcessingActionMgmtRole->Enabled);
		
		// Try/catch in case of a failure to start processing so UnregisterProcessingFAM can be
		// called and to stop supplying if it was started.
		try
		{
			// start the file supplying
			if (ipSupplyingActionMgmtRole->Enabled == VARIANT_TRUE)
			{
				ipSupplyingActionMgmtRole->Start(m_ipFPMDB, lActionId, strExpandedAction.c_str(), 
					(long) (m_apDlg.get() == NULL ? NULL : m_apDlg->m_hWnd), m_ipFAMTagManager, ipThis,
					m_strFPSFileName.c_str());

				// Set flag indicating that supplying was started
				m_bSupplying = true;
			}

			// start the file processing
			if (ipProcessingActionMgmtRole->Enabled == VARIANT_TRUE)
			{
				// set flag to indicate that processing was started
				m_bProcessing = true;

				m_ipFPMgmtRole->OkToStopWhenQueueIsEmpty = m_bSupplying ? VARIANT_FALSE : VARIANT_TRUE;

				ipProcessingActionMgmtRole->Start(m_ipFPMDB, lActionId, strExpandedAction.c_str(), 
					(long) (m_apDlg.get() == NULL ? NULL : m_apDlg->m_hWnd), m_ipFAMTagManager,
					ipThis, m_strFPSFileName.c_str());
			}
		}
		catch (...)
		{
			try
			{
				// If an exception is thrown while starting the file processing the supplying needs to be stopped
				if ( m_bSupplying )
				{
					ipSupplyingActionMgmtRole->Stop();
				}

				m_ipFPMDB->UnregisterActiveFAM();
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33185");

			// Rethrow the exception
			throw;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08847")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
UINT CFileProcessingManager::handleStopRequestAsynchronously(void *pData)
{
	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		{

			CFileProcessingManager *pFPM = static_cast<CFileProcessingManager *>(pData);
			ASSERT_ARGUMENT("ELI13900", pFPM != __nullptr);

			// notify all file suppliers to stop supplying
			UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipSupplyingActionMgmtRole =
				pFPM->getActionMgmtRole(pFPM->m_ipFSMgmtRole);
			ASSERT_RESOURCE_ALLOCATION("ELI14273", ipSupplyingActionMgmtRole != __nullptr);
			if (ipSupplyingActionMgmtRole->Enabled == VARIANT_TRUE)
			{
				ipSupplyingActionMgmtRole->Stop();
			}

			// notify all file processors to stop processing
			UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipProcessingActionMgmtRole =
				pFPM->getActionMgmtRole(pFPM->m_ipFPMgmtRole);
			ASSERT_RESOURCE_ALLOCATION("ELI14312", ipProcessingActionMgmtRole != __nullptr);
			if (ipProcessingActionMgmtRole->Enabled == VARIANT_TRUE)
			{
				ipProcessingActionMgmtRole->Stop();
			}
		}

		CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13899")

	return 0;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::StopProcessing()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// If we are already cancelling, we are done.
		if (m_bCancelling)
		{
			return S_OK;
		}

		// ensure that processing is currently taking place
		if (!m_bProcessing && !m_bSupplying)
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI12734")
		}

		// ensure that the processing is not currently paused
		if (m_bPaused)
		{
			throw UCLIDException("ELI12735", "Processing cannot be stopped when it is in a paused state!");
		}

		// Set the flag that cancelling has started
		m_bCancelling = true;

		// Log stop processing information
		EStartStopStatus eStatus = kBeginStop;
		logStatusInfo(eStatus);

		// handle the stop request asynchronously.  This has to be done asynchronously because
		// during the "stop" process, the FPM sends messages to the FPM-UI to update status
		// and notify of file processing completion, etc, and that would create a deadlock
		// if the stop is handled synchronously because this method is called when the Stop
		// button is pressed in the FPM UI.
		AfxBeginThread(handleStopRequestAsynchronously, this);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08848")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::LoadFrom(BSTR strFullFileName, VARIANT_BOOL bSetDirtyFlagToTrue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strFileName = asString(strFullFileName);
		
		// ensure the file exists
		validateFileOrFolderExistence(strFileName);

		// Read the object from the file
		IPersistStreamPtr ipPersistStream = getThisAsCOMPtr();
		ASSERT_RESOURCE_ALLOCATION("ELI17794", ipPersistStream != __nullptr);
		_bstr_t _bstrStreamName = get_bstr_t(gstrSTREAM_NAME);
		readObjectFromFile(ipPersistStream, strFullFileName, _bstrStreamName);

		// Update the filename associated with this File Action Manager
		m_strFPSFileName = strFileName;

		// mark this object as dirty depending upon bSetDirtyFlagToTrue
		m_bDirty = asCppBool(bSetDirtyFlagToTrue);

		// Wait for the file to be accessible
		waitForFileAccess(strFileName, giMODE_READ_ONLY);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19281");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::SaveTo(BSTR strFullFileName, VARIANT_BOOL bClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		writeObjectToFile(this, strFullFileName, get_bstr_t(gstrSTREAM_NAME), asCppBool(bClearDirty));

		// mark this object as dirty depending upon bDontChangeDirtyFlag
		if (bClearDirty == VARIANT_TRUE)
		{
			m_bDirty = false;

			// update the filename associated with this File ProcessingManager
			// NOTE: we only want to update the filename when bClearDirty is
			// true because this method gets called for "temporary saving" 
			// with the auto-save-on-timer feature.  The auto-save method
			// calls to this method will have bClearDirty set to false.
			m_strFPSFileName = asString(strFullFileName);
		}

		// Wait until the file is readable
		waitForStgFileAccess(strFullFileName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19282");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_FPSFileName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI17264", pVal != __nullptr);

		*pVal = _bstr_t(m_strFPSFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11008")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::put_FPSFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		_bstr_t tmp(newVal);

		m_strFPSFileName = tmp;

		// NOTE: we do not need to set the dirty flag because we did not change
		// any persistent data members.
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11997")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_MaxStoredRecords(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_recordMgr.getMaxStoredRecords();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12455")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::put_MaxStoredRecords(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_recordMgr.setMaxStoredRecords(newVal);

		// NOTE: we do not need to set the dirty flag because we did not change
		// any persistent data members.
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12456")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_RestrictNumStoredRecords(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_recordMgr.getRestrictNumStoredRecords() ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12457")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::put_RestrictNumStoredRecords(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		m_recordMgr.setRestrictNumStoredRecords(newVal == VARIANT_TRUE);

		// NOTE: we do not need to set the dirty flag because we did not change
		// any persistent data members.
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12458")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::PauseProcessing()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bPaused = true;

		// pause all the processing threads
		vector< ProcessingThreadData * >::iterator iter;
		for (iter = m_vecProcessingThreadData.begin(); iter != m_vecProcessingThreadData.end(); iter++)
		{
			ProcessingThreadData *pData = *iter;
			CWinThread* pThread = pData->m_pThread;
			ASSERT_RESOURCE_ALLOCATION("ELI25248", pThread != __nullptr);
			pThread->SuspendThread();
		}

		// pause the file supplying
		UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipSupplyingActionMgmtRole =
			getActionMgmtRole(m_ipFSMgmtRole);
		if (ipSupplyingActionMgmtRole->Enabled == VARIANT_TRUE)
		{
			ipSupplyingActionMgmtRole->Pause();
		}

		// pause the file processing
		UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipProcessingActionMgmtRole =
			getActionMgmtRole(m_ipFPMgmtRole);
		if (ipProcessingActionMgmtRole->Enabled == VARIANT_TRUE)
		{
			ipProcessingActionMgmtRole->Pause();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12729")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_ProcessingStarted(/*[out, retval]*/ VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28476", pbValue != __nullptr);
	
		*pbValue = asVariantBool(m_bProcessing || m_bSupplying);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12730")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_ProcessingPaused(/*[out, retval]*/ VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pbValue = m_bPaused ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12731")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::LoadFilesFromFile(BSTR bstrFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string strListFileName = asString( bstrFileName );
		// TODO: implement backward compatibility
		// m_vecIndividualFilesList.clear();
		// getFileListFromFile( strListFileName, m_vecIndividualFilesList );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13413");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_ActionName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI17265", pVal != __nullptr);

		*pVal = _bstr_t(m_strAction.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14017")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::put_ActionName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store new action name
		m_strAction = asString( newVal );

		// Set dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15020")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_DisplayOfStatisticsEnabled(VARIANT_BOOL * newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		*newVal = m_bDisplayOfStatisticsEnabled ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14145")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::put_DisplayOfStatisticsEnabled(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		m_bDisplayOfStatisticsEnabled = (newVal==VARIANT_TRUE);
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14146")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		clear();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14155")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::ValidateStatus(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get the current status of the db connection
		string strCurrDBStatus = asString(getFPMDB()->GetCurrentConnectionStatus());

		// Set the DBConnectionReady flag to true if DBStatus is Connection Established
		m_isDBConnectionReady  = strCurrDBStatus == gstrCONNECTION_ESTABLISHED;

		// If the database can not be connected 	
		if(!m_isDBConnectionReady)
		{
			UCLIDException ue("ELI14908", "Unable to connect to the database!");
			throw ue;
		}

		// If there is no action specified
		if (m_strAction == "")
		{
			UCLIDException ue("ELI14362", "An action should be specified in action tab!");
			throw ue;
		}

		// Validate the action name unless it contains a function to expand
		if (m_strAction.find('$') == string::npos && !isActionNameInDatabase(m_strAction))
		{
			string strDebugInfo = "The action '" + m_strAction + "' does not exist in database.";
			UCLIDException ue("ELI15658", strDebugInfo.c_str());
			throw ue;
		}

		// If all of three check boxes in action page haven't been checked
		if (getActionMgmtRole(m_ipFSMgmtRole)->Enabled == VARIANT_FALSE && 
			getActionMgmtRole(m_ipFPMgmtRole)->Enabled == VARIANT_FALSE &&
			!m_bDisplayOfStatisticsEnabled)
		{
			UCLIDException ue("ELI14363", "At least one check box should be checked in action tab!");
			throw ue;
		}

		// If the Processing status or Supplying status is not valid
		getActionMgmtRole(m_ipFSMgmtRole)->ValidateStatus();
		getActionMgmtRole(m_ipFPMgmtRole)->ValidateStatus(); 
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14182");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_FileSupplyingMgmtRole(IFileSupplyingMgmtRole **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		CComQIPtr<IFileSupplyingMgmtRole> ipFSMgmtRole = m_ipFSMgmtRole;
		ASSERT_RESOURCE_ALLOCATION("ELI14266", ipFSMgmtRole != __nullptr);

		*pVal = ipFSMgmtRole.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14195")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_FileProcessingMgmtRole(IFileProcessingMgmtRole **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		CComQIPtr<IFileProcessingMgmtRole> ipFPMgmtRole = m_ipFPMgmtRole;
		ASSERT_RESOURCE_ALLOCATION("ELI14313", ipFPMgmtRole != __nullptr);

		*pVal = ipFPMgmtRole.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14314")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::GetActionIDFromName(BSTR bstrActionName, long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Make sure the pVal is not NULL
		ASSERT_ARGUMENT("ELI18574", pVal != __nullptr);

		// Check licensing
		validateLicense();

		// If there is no action specified
		if (!bstrActionName || bstrActionName == _bstr_t(""))
		{
			UCLIDException ue("ELI14482", "An action name must be specified!");
			throw ue;
		}

		// Return action ID as zero if the database connection is not ready
		if (!m_isDBConnectionReady)
		{
			*pVal = 0;

			return S_OK;
		}

		// Get the action id from the database
		*pVal = getFPMDB()->GetActionID(bstrActionName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14481")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_DatabaseServer(/*[out, retval]*/ BSTR *pVal)
{
	try
	{
		ASSERT_ARGUMENT("ELI17483", pVal != __nullptr);

		*pVal = getFPMDB()->DatabaseServer.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17482");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::put_DatabaseServer(/*[in]*/ BSTR newVal)
{
	try
	{
		// convert the newVal to std::string
		string strNewVal = asString(newVal);

		// check if the newVal is not blank and if it is a new server name
		if (strNewVal != "" && strNewVal != m_strPreviousDBServer)
		{
			// as long as the last server was not blank then
			// the server has changed, set dirty flag
			// [p13 #4581 & #4580]
			if (m_strPreviousDBServer != "")
			{
				m_bDirty = true;
			}

			// anytime server name changes, change the last server to new name
			m_strPreviousDBServer = strNewVal;
		}

		getFPMDB()->DatabaseServer = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17481");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_DatabaseName(/*[out, retval]*/ BSTR *pVal)
{
	try
	{
		ASSERT_ARGUMENT("ELI17480", pVal != __nullptr);

		*pVal = getFPMDB()->DatabaseName.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17479");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::put_DatabaseName(/*[in]*/ BSTR newVal)
{
	try
	{
		// convert the newVal to std::string
		string strNewVal = asString(newVal);

		// check if the newVal is not blank and if it is a new database name
		if (strNewVal != "" && strNewVal != m_strPreviousDBName)
		{
			// as long as the last database was not blank then
			// the database has changed, set dirty flag
			// [p13 #4581 & #4580]
			if (m_strPreviousDBName != "")
			{
				m_bDirty = true;
			}

			// anytime database name changes, change the last database to new name
			m_strPreviousDBName = strNewVal;
		}

		getFPMDB()->DatabaseName = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17478");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::GetCounts(long *plNumFilesProcessedSuccessfully, long *plNumProcessingErrors,
											   long *plNumFilesSupplied, long *plNumSupplyingErrors)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (plNumFilesProcessedSuccessfully != __nullptr)
		{
			*plNumFilesProcessedSuccessfully = m_recordMgr.getNumberOfFilesProcessedSuccessfully();
		}
		if (plNumProcessingErrors != __nullptr)
		{
			*plNumProcessingErrors = m_recordMgr.getNumberOfFilesFailed();
		}

		// Check if there is a supplying manager
		if (m_ipFSMgmtRole != __nullptr)
		{
			m_ipFSMgmtRole->GetSupplyingCounts(plNumFilesSupplied, plNumSupplyingErrors);
		}
		else
		{
			// No supplying manager so set supplying values to 0
			if (plNumFilesSupplied != __nullptr)
			{
				plNumFilesSupplied = 0;
			}
			if (plNumSupplyingErrors != __nullptr)
			{
				plNumSupplyingErrors = 0;
			}
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28468")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_IsDBPasswordRequired(VARIANT_BOOL* pvbIsDBPasswordRequired)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI28473", pvbIsDBPasswordRequired != __nullptr);

		*pvbIsDBPasswordRequired = asVariantBool(isDBPasswordRequired());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28474")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::GetExpandedActionName(BSTR *pbstrAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI29117", pbstrAction != __nullptr);

		*pbstrAction = _bstr_t(getExpandedActionName().c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29178");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::put_NumberOfDocsToProcess(long lNumberOfDocsToProcess)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_nNumberOfFilesToExecute = lNumberOfDocsToProcess;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29179");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_IsUserAuthenticationRequired(
	VARIANT_BOOL* pvbAuthenticationRequired)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI29190", pvbAuthenticationRequired != __nullptr);

		// Set the return value
		*pvbAuthenticationRequired = asVariantBool(isUserAuthenticationRequired());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29191");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::ProcessSingleFile(BSTR bstrSourceDocName, VARIANT_BOOL vbQueue,
		VARIANT_BOOL vbProcess, VARIANT_BOOL vbForceProcessing, EFilePriority eFilePriority)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		try
		{
			try
			{
				validateLicense();

				if (!authenticateForProcessing())
				{
					// If a password was required to run, but the user did not enter it
					// correctly, return immediately without processing.
					return S_OK;
				}

				bool bQueue = asCppBool(vbQueue);
				bool bProcess = asCppBool(vbProcess);
				bool bForceProcessing = asCppBool(vbForceProcessing);

				if (bForceProcessing && !bQueue)
				{
					UCLIDException ue("ELI29543",
						"Cannot force file status to pending if not queueing!");
					throw ue;
				}

				// Expand the action name (note this also sets the FPSFileDir value for the
				// tag manager) [LRCAU #5813]
				_bstr_t bstrActionName(getExpandedActionName().c_str());

				// Register as an active FAM (allows for stuck files to be reverted)
				long nActionId = getFPMDB()->GetActionID(bstrActionName);
				getFPMDB()->RegisterActiveFAM(nActionId, vbQueue, vbProcess);

				// Validate that the action name exists in the database (auto-create if that setting is set)
				getFPMDB()->AutoCreateAction(bstrActionName);

				UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord = __nullptr;
				if (bQueue)
				{
					// If queueing, attempt to add the file to the database.
					VARIANT_BOOL vbAlreadyExists;
					UCLID_FILEPROCESSINGLib::EActionStatus easOriginal;
					ipFileRecord = getFPMDB()->AddFile(bstrSourceDocName, bstrActionName,
						(UCLID_FILEPROCESSINGLib::EFilePriority)eFilePriority, vbForceProcessing,
						VARIANT_FALSE, UCLID_FILEPROCESSINGLib::kActionPending, &vbAlreadyExists,
						&easOriginal);
				}
				else if (bProcess)
				{
					// If not queueing, but processing, attempt to retrieve an existing record for this
					// file.
					ipFileRecord = getFPMDB()->GetFileRecord(bstrSourceDocName, bstrActionName);
				}

				if (bProcess)
				{
					if (ipFileRecord == __nullptr)
					{
						UCLIDException ue("ELI29544", "The file cannot be processed because it has not "
							"been queued!");
						throw ue;
					}

					// m_ipFPMgmtRole needs a record manager to be able to process files.
					m_ipFPMgmtRole->SetRecordMgr(&m_recordMgr);

					// Process the specified file only (don't go to the database to retrieve any other
					// pending files regardless of priority).
					m_ipFPMgmtRole->ProcessSingleFile(ipFileRecord, getFPMDB(), m_ipFAMTagManager);
				}

				getFPMDB()->UnregisterActiveFAM();

				return S_OK;
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29561");
		}
		catch (UCLIDException& ue)
		{
			try
			{
				getFPMDB()->UnregisterActiveFAM();

				ue.addDebugInfo("Database Server", asString(getFPMDB()->DatabaseServer));
				ue.addDebugInfo("Database Name", asString(getFPMDB()->DatabaseName));
				ue.addDebugInfo("Action", m_strAction);
				ue.addDebugInfo("Filename", asString(bstrSourceDocName));
			}
			catch (...){}

			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29562");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::AuthenticateForProcessing(VARIANT_BOOL* pvbAuthenticated)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI29564", pvbAuthenticated != __nullptr);

		*pvbAuthenticated = asVariantBool(authenticateForProcessing());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29563");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::get_MaxFilesFromDB(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI32143", pVal != __nullptr);

		*pVal = m_nMaxFilesFromDB;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32144");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::put_MaxFilesFromDB(long newVal)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		if (newVal < gnNUM_FILES_LOWER_RANGE || newVal > gnNUM_FILES_UPPER_RANGE)
		{
			UCLIDException ue("ELI32146", "Value was out of range.");
			ue.addDebugInfo("Lower Bound", gnNUM_FILES_LOWER_RANGE);
			ue.addDebugInfo("Upper Bound", gnNUM_FILES_UPPER_RANGE);
			ue.addDebugInfo("Value Specified", newVal);
			throw ue;
		}

		m_bDirty = m_nMaxFilesFromDB != newVal;
		m_nMaxFilesFromDB = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32145");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::AuthenticateService(BSTR bstrValue)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		m_bIsAuthenticated = IS_VALID_PRIVATE_LICENSE(asString(bstrValue));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32366");
}

//-------------------------------------------------------------------------------------------------
// IRoleNotifyFAM Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::NotifyProcessingCompleted(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	m_bProcessing = false;
	
	if ( !m_bSupplying )
	{
		// Unregister Active FAM to reset file back to previous state if any remaining
		m_ipFPMDB->UnregisterActiveFAM();

		// Log processing has finished information
		EStartStopStatus eStatus = kEndStop;
		logStatusInfo(eStatus);

		// Only post a message to the dialog if it exists
		if (m_apDlg.get() != __nullptr)
		{
			::PostMessage(m_apDlg->m_hWnd, FP_PROCESSING_COMPLETE, 0, 0);
		}
	}

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::NotifySupplyingCompleted(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	m_bSupplying = false;

	if ( !m_bProcessing )
	{
		// Unregister Active FAM to reset file back to previous state if any remaining
		m_ipFPMDB->UnregisterActiveFAM();

		// Log processing has finished information
		EStartStopStatus eStatus = kEndStop;
		logStatusInfo(eStatus);

		// Only post a message to the dialog if it exists
		if (m_apDlg.get() != __nullptr)
		{
			::PostMessage(m_apDlg->m_hWnd, FP_PROCESSING_COMPLETE, 0, 0);
		}
	}
	else
	{
		m_ipFPMgmtRole->OkToStopWhenQueueIsEmpty = VARIANT_TRUE;
	}

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::NotifyProcessingCancelling()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	StopProcessing();

	// Only post a message to the dialog if it exists
	if (m_apDlg.get() != __nullptr)
	{
		::PostMessage(m_apDlg->m_hWnd, FP_PROCESSING_CANCELLING, 0, 0);
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Private functions
//-------------------------------------------------------------------------------------------------
void CFileProcessingManager::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFILE_ACTION_MANAGER_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI08828", "File Action Manager");
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr CFileProcessingManager::getFPMDB()
{
	// Create FAM Database object if needed
	if ( m_ipFPMDB == __nullptr )
	{
		// Create the FPM Database object
		m_ipFPMDB.CreateInstance(CLSID_FileProcessingDB);
		ASSERT_RESOURCE_ALLOCATION("ELI13982", m_ipFPMDB != __nullptr );

		// Tell the Record Manager about the database
		m_recordMgr.setFPMDB(m_ipFPMDB);
	}
	return m_ipFPMDB;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr CFileProcessingManager::getActionMgmtRole(IUnknownPtr ipUnknown)
{
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipMgmtRole = ipUnknown;
	ASSERT_RESOURCE_ALLOCATION("ELI14202", ipMgmtRole != __nullptr);
	return ipMgmtRole;
}
//-------------------------------------------------------------------------------------------------
string CFileProcessingManager::getExpandedActionName()
{
	// Set the FPS file directory for tag manager
	m_ipFAMTagManager->FPSFileDir = getDirectoryFromFullPath(m_strFPSFileName).c_str();

	// Expand the tags
	return CFileProcessingUtils::ExpandTagsAndTFE(m_ipFAMTagManager, m_strAction, "");
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingManager::clear()
{
	try
	{
		m_bProcessing = false;
		m_bSupplying = false;
		m_bPaused = false;

		// clear the file processing role
		// it doesn't need to be an error if NULL
		if ( m_ipFPMgmtRole != __nullptr )
		{
			getActionMgmtRole(m_ipFPMgmtRole)->Clear();
		}

		// reset the action name
		m_strAction = "";

		// clear the file supplying role
		// it doesn't need to be an error if NULL
		if ( m_ipFSMgmtRole != __nullptr )
		{
			getActionMgmtRole(m_ipFSMgmtRole)->Clear();
		}

		// reset the high level enable/disable flags
		m_bDisplayOfStatisticsEnabled = false;

		// reset the file name
		m_strFPSFileName = "";

		// reset the record manager
		m_recordMgr.clear(true);

		// clear the last server and database string
		m_strPreviousDBServer = "";
		m_strPreviousDBName = "";

		// reset the database config file
		getFPMDB()->DatabaseServer = "";
		getFPMDB()->DatabaseName = "";
		getFPMDB()->ResetDBConnection();

		m_nMaxFilesFromDB = gnMAX_NUMBER_OF_FILES_FROM_DB;

		m_bDirty = false;
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI14257", "Unable to clear data!", ue);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CFileProcessingManager::getFileProcessorsData()
{
	// get the file supplying mgmt role
	IIUnknownVectorPtr ipFileProcessorsData = m_ipFPMgmtRole->FileProcessors;
	ASSERT_RESOURCE_ALLOCATION("ELI14346", ipFileProcessorsData != __nullptr);

	return ipFileProcessorsData;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CFileProcessingManager::getFileSuppliersData()
{
	// get the file supplying mgmt role
	IIUnknownVectorPtr ipFileSuppliersData = m_ipFSMgmtRole->FileSuppliers;
	ASSERT_RESOURCE_ALLOCATION("ELI19430", ipFileSuppliersData != __nullptr);

	return ipFileSuppliersData;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingManager::isActionNameInDatabase(const string& strAction)
{
	try
	{
		getFPMDB()->GetActionID( strAction.c_str() );
	}
	catch(...)
	{
		return false;
	}

	return true;		
}
//-------------------------------------------------------------------------------------------------
void CFileProcessingManager::logStatusInfo(EStartStopStatus eStatus)
{
	switch(eStatus)
	{
	case kStart:
		{
			// Log the info that processing is starting processing
			UCLIDException ue("ELI15680", "Application trace: File Action Manager has started processing.");
			ue.addDebugInfo("FPS File",
				m_strFPSFileName.empty() ? "<Not Saved>" : m_strFPSFileName);
			ue.log();

			if (m_bRecordFAMSessions)
			{
				// Record the FAM session start
				getFPMDB()->RecordFAMSessionStart(m_strFPSFileName.c_str());
			}
		}
		break;
	case kBeginStop:
		{
			// Log the info that processing is beginning to stop processing
			UCLIDException ue("ELI28831", "Application trace: File Action Manager is stopping processing.");
			ue.addDebugInfo("FPS File",
				m_strFPSFileName.empty() ? "<Not Saved>" : m_strFPSFileName);
			ue.log();
		}
		break;
	case kEndStop:
		{
			// Log the info that processing has stopped processing
			UCLIDException ue("ELI15678", "Application trace: File Action Manager has stopped processing.");
			ue.addDebugInfo("FPS File",
				m_strFPSFileName.empty() ? "<Not Saved>" : m_strFPSFileName);
			ue.log();

			if (m_bRecordFAMSessions)
			{
				// Record the FAM session stop
				getFPMDB()->RecordFAMSessionStop();
			}
		}
		break;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI15677");
	}
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingManager::isUserAuthenticationRequired()
{
	// Check if authentication is required
	bool bRequire = asString(getFPMDB()->GetDBInfoSetting(
		gstrREQUIRE_AUTHENTICATION_BEFORE_RUN.c_str(), VARIANT_TRUE)) == "1";

	if (bRequire)
	{
		bRequire = !(m_bIsAuthenticated && asCppBool(getFPMDB()->CanSkipAuthenticationOnThisMachine()));
	}

	return bRequire;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingManager::isDBPasswordRequired()
{
	if (m_bIsAuthenticated && getFPMDB()->CanSkipAuthenticationOnThisMachine())
	{
		return false;
	}

	// Get the AccessRequires interface for the management role to check if Admin access is required
	IAccessRequiredPtr ipAccess(m_ipFPMgmtRole);
	ASSERT_RESOURCE_ALLOCATION("ELI31274", ipAccess != __nullptr);
		
	return ipAccess->RequiresAdminAccess() == VARIANT_TRUE;
}
//-------------------------------------------------------------------------------------------------
bool CFileProcessingManager::authenticateForProcessing()
{
	// Check if a user login is required before running
	if (isUserAuthenticationRequired())
	{
		VARIANT_BOOL vbCancelled;
		
		// Show the DB login prompt for the current user
		if (!asCppBool(getFPMDB()->ShowLogin(VARIANT_FALSE, &vbCancelled)))
		{
			// Check if the user cancelled, only warn about invalid password
			// if they didn't cancel [LRCAU #5419]
			if (!asCppBool(vbCancelled))
			{
				HWND hParent = NULL;
				CWnd *pWnd = AfxGetMainWnd();
				if (pWnd)
				{
					hParent = pWnd->m_hWnd;
				}

				MessageBox(hParent, "Incorrect password", "Invalid Login", MB_OK);
			}
			return false;
		}
	}

	// Check if authentication is needed for processing skipped files [LRCAU #5413]
	if (isDBPasswordRequired())
	{
		// Show the DB login prompt for admin
		VARIANT_BOOL vbCancelled;
		if (getFPMDB()->ShowLogin(VARIANT_TRUE, &vbCancelled) == VARIANT_FALSE)
		{
			// Check if the user cancelled, only warn about invalid password
			// if they didn't cancel [LRCAU #5419]
			if (vbCancelled == VARIANT_FALSE)
			{
				HWND hParent = NULL;
				CWnd *pWnd = AfxGetMainWnd();
				if (pWnd)
				{
					hParent = pWnd->m_hWnd;
				}

				MessageBox(hParent, "Admin password invalid.",
					"Authentication Failed", MB_OK | MB_ICONERROR);
			}
			return false;
		}
	}

	// Either authentication was not required or the password(s) were correctly entered.
	return true;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr CFileProcessingManager::getThisAsCOMPtr()
{
	UCLID_FILEPROCESSINGLib::IFileProcessingManagerPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI17033", ipThis != __nullptr);
	return ipThis;
}
//-------------------------------------------------------------------------------------------------
