// FileSupplyingMgmtRole.cpp : Implementation of CFileSupplyingMgmtRole

#include "stdafx.h"
#include "UCLIDFileProcessing.h"
#include "FilePriorityHelper.h"
#include "FileSupplyingMgmtRole.h"
#include "FP_UI_Notifications.h"
#include "FileSupplyingRecord.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>
#include <FAMUtilsConstants.h>

//-------------------------------------------------------------------------------------------------
// Preprocessor directives
//-------------------------------------------------------------------------------------------------
// In debug mode, allow injection of exceptions to simulate failures
// Comment or uncomment the #define line below, depending upon whether exceptions should be
// injected.
#ifdef _DEBUG
//#define INJECT_FS_EVENT_HANDLING_EXCEPTIONS
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;
// Version 2: Added m_bSkipPageCount

//-------------------------------------------------------------------------------------------------
// SupplierThreadData class
//-------------------------------------------------------------------------------------------------
SupplierThreadData::SupplierThreadData(UCLID_FILEPROCESSINGLib::IFileSupplier *pFS,
	UCLID_FILEPROCESSINGLib::IFileSupplierTarget *pFST,
	UCLID_FILEPROCESSINGLib::IFAMTagManager *pFAMTM,
	IFileProcessingDB *pDB, long nActionID, bool displayExceptions)
	: m_ipFileSupplier(pFS), m_ipFileSupplierTarget(pFST), m_ipFAMTagManager(pFAMTM), m_ipDB(pDB),
		m_nActionID(nActionID), m_bDisplayExceptions(displayExceptions)
{
	// verify non-NULL arguments
	ASSERT_ARGUMENT("ELI13761", m_ipFileSupplier != __nullptr);
	ASSERT_ARGUMENT("ELI13762", m_ipFileSupplierTarget != __nullptr);
	ASSERT_ARGUMENT("ELI14436", m_ipFAMTagManager != __nullptr);
	ASSERT_ARGUMENT("ELI33970", m_ipDB != __nullptr);
}

//-------------------------------------------------------------------------------------------------
UINT CFileSupplyingMgmtRole::fileSupplyingThreadProc(void *pData)
{
	SupplierThreadData* pSupplierThreadData;
	try
	{
		// initialize COM for this thread
		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		// get the threadData object.
		pSupplierThreadData = static_cast<SupplierThreadData*>(pData);

		// signal that the thread has started
		pSupplierThreadData->m_threadStartedEvent.signal();

		// start the file supplier
		// NOTE: this is in its own try/catch block in order to guarantee that we can guarantee the signaling
		// of the threadEndedEvent below the catch block.
		try
		{
			// Get the File Supplier
			UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFS = pSupplierThreadData->m_ipFileSupplier;
			ASSERT_RESOURCE_ALLOCATION("ELI15640", ipFS != __nullptr );
			ipFS->Start(pSupplierThreadData->m_ipFileSupplierTarget, 
				pSupplierThreadData->m_ipFAMTagManager, pSupplierThreadData->m_ipDB,
				pSupplierThreadData->m_nActionID);
		}
		catch (...)
		{
			uex::logOrDisplayCurrent("ELI14242",
				pSupplierThreadData && pSupplierThreadData->m_bDisplayExceptions);
		}

		// signal that the thread has ended
		pSupplierThreadData->m_threadEndedEvent.signal();

		// uninitialize COM for this thread
		CoUninitialize();
	}
	catch (...)
	{
		uex::logOrDisplayCurrent("ELI19420",
			pSupplierThreadData && pSupplierThreadData->m_bDisplayExceptions);
	}

	return 0;
}

//-------------------------------------------------------------------------------------------------
// CFileSupplyingMgmtRole
//-------------------------------------------------------------------------------------------------
CFileSupplyingMgmtRole::CFileSupplyingMgmtRole()
{
	try
	{
		// clear internal data
		clear();
	}
	catch (...)
	{
		throw uex::fromCurrent("ELI14200");
	}
}
//-------------------------------------------------------------------------------------------------
CFileSupplyingMgmtRole::~CFileSupplyingMgmtRole()
{
	try
	{
		// clear internal data
		clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14243")
}
//-------------------------------------------------------------------------------------------------
HRESULT CFileSupplyingMgmtRole::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CFileSupplyingMgmtRole::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFileActionMgmtRole,
		&IID_IFileSupplyingMgmtRole,
		&IID_IFileSupplierTarget,
		&IID_ILicensedComponent,
		&IID_IPersistStream
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
STDMETHODIMP CFileSupplyingMgmtRole::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
	{
		return E_POINTER;
	}

	try
	{
		// validate license
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
// IFileActionMgmtRole interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::Start(IFileProcessingDB* pDB, long lActionId, 
	BSTR bstrAction, long hWndOfUI, IFAMTagManager* pTagManager, IRoleNotifyFAM* pRoleNotifyFAM,
	BSTR bstrFpsFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		try
		{
			validateLicense();

			// check pre-conditions
			ASSERT_ARGUMENT("ELI14244", m_bEnabled == true);
			ASSERT_ARGUMENT("ELI14245", m_ipFileSuppliers != __nullptr);
			ASSERT_ARGUMENT("ELI14246", m_ipFileSuppliers->Size() > 0);

			m_ipRoleNotifyFAM = pRoleNotifyFAM;
			ASSERT_RESOURCE_ALLOCATION("ELI14523", m_ipRoleNotifyFAM != __nullptr );

			// release any memory that may have been allocated to manage previously spawned-off
			// file supplier threads
			releaseSupplyingThreadDataObjects();

			// store the pointer to the DB so that subsequent calls to getFPDB() will work correctly
			m_pDB = pDB;

			// store the pointer to the TagManager so that subsequent calls to getFPMTagManager() will work
			m_pFAMTagManager = pTagManager;

			// remember the action name and id
			m_strAction = asString(bstrAction);
			m_lActionId = lActionId;

			// remember the handle of the UI so that messages can be sent to it
			m_hWndOfUI = (HWND) hWndOfUI;

			// Reset m_nFinishedSupplierCount to zero before suppliers start
			m_nFinishedSupplierCount = 0;

			// Set the number of enabled suppliers
			m_nEnabledSupplierCount = getEnabledSupplierCount();

			// iterate through the file suppliers and spawn individual threads for each of them
			// to supply files
			long nNumFileSuppliers = m_ipFileSuppliers->Size();
			for (long n = 0; n < nNumFileSuppliers; n++)
			{
				// get the nth file supplier data object
				UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFileSupplierData = m_ipFileSuppliers->At(n);
				ASSERT_RESOURCE_ALLOCATION("ELI13768", ipFileSupplierData != __nullptr);

				// get the object with description
				IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
				ASSERT_RESOURCE_ALLOCATION("ELI13788", ipFSObjWithDesc != __nullptr);

				if (ipFSObjWithDesc->Enabled == VARIANT_TRUE)
				{
					// get the file supplier
					UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFileSupplier = ipFSObjWithDesc->Object;
					ASSERT_RESOURCE_ALLOCATION("ELI13770", ipFileSupplier != __nullptr);

					// get access to the IFileSupplierTarget interface on this object
					UCLID_FILEPROCESSINGLib::IFileSupplierTargetPtr ipTarget = this;
					ASSERT_RESOURCE_ALLOCATION("ELI13775", ipTarget);

					// create the thread data structure
					auto apThreadData = std::make_unique<SupplierThreadData>(
						ipFileSupplier, ipTarget, getFAMTagManager(), pDB, lActionId,
						m_hWndOfUI != __nullptr);

					// start the file supplier thread
					AfxBeginThread(fileSupplyingThreadProc, apThreadData.get());

					// wait for the file supplier thread to start
					apThreadData.get()->m_threadStartedEvent.wait();

					// push the thread data object to m_vecSupplyingThreadData so that it can be released later
					m_vecSupplyingThreadData.push_back(apThreadData.release());

					// Update status and notify dialog
					ipFileSupplierData->PutFileSupplierStatus( 
						UCLID_FILEPROCESSINGLib::kActiveStatus );

					if (m_hWndOfUI != __nullptr)
					{
						::PostMessage(m_hWndOfUI, 
							FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kActiveStatus, 
							(LPARAM)ipFileSupplier.GetInterfacePtr());
					}
				}
			}
		}
		catch (UCLIDException &ue)
		{
			UCLIDException uexOuter("ELI13759", "Unable to start all file suppliers!", ue);
			throw uexOuter;
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14229")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::Stop(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// NOTE: Do not call validateLicense here because we want to be able 
		//   to gracefully stop processing even if the license state is corrupted.
		// validateLicense();

		// check pre-conditions
		ASSERT_ARGUMENT("ELI14223", m_bEnabled == true);
		ASSERT_ARGUMENT("ELI14224", m_ipFileSuppliers != __nullptr);
		ASSERT_ARGUMENT("ELI14247", m_ipFileSuppliers->Size() > 0);
		ASSERT_ARGUMENT("ELI14524", m_ipRoleNotifyFAM != __nullptr );

		// notify all file suppliers to stop supplying
		long nNumFileSuppliers = m_ipFileSuppliers->Size();
		for (long n = 0; n < nNumFileSuppliers; n++)
		{
			// get the nth file supplier data object
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFileSupplierData = m_ipFileSuppliers->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI13895", ipFileSupplierData != __nullptr);

			// get the object with description
			IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
			ASSERT_RESOURCE_ALLOCATION("ELI13896", ipFSObjWithDesc != __nullptr);

			// If the supplier is enabled and its status is not kDoneStatus
			// (No need to update the status if one supplier is done)
			if (ipFSObjWithDesc->Enabled == VARIANT_TRUE && 
				ipFileSupplierData->FileSupplierStatus != kDoneStatus)
			{
				// get the file supplier
				UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFileSupplier = ipFSObjWithDesc->Object;
				ASSERT_RESOURCE_ALLOCATION("ELI13897", ipFileSupplier != __nullptr);

				ipFileSupplier->Stop();

				// Update status and notify dialog
				ipFileSupplierData->PutFileSupplierStatus( UCLID_FILEPROCESSINGLib::kStoppedStatus );

				if (m_hWndOfUI != __nullptr)
				{
					::PostMessage( m_hWndOfUI, 
						FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kStoppedStatus, 
						(LPARAM)ipFileSupplier.GetInterfacePtr());
				}
			}
		}
		// Notify the FAM that supplying is complete
		m_ipRoleNotifyFAM->NotifySupplyingCompleted();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14230")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::Pause(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// check pre-conditions
		ASSERT_ARGUMENT("ELI14225", m_bEnabled == true);
		ASSERT_ARGUMENT("ELI14226", m_ipFileSuppliers != __nullptr);
		ASSERT_ARGUMENT("ELI14248", m_ipFileSuppliers->Size() > 0);

		// Update status and notify dialog
		long lCount = m_ipFileSuppliers->Size();
		for (int i = 0; i < lCount; i++)
		{
			// get the ith file supplier data object
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFileSupplierData = m_ipFileSuppliers->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI14237", ipFileSupplierData != __nullptr);

			// get the object with description
			IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
			ASSERT_RESOURCE_ALLOCATION("ELI14238", ipFSObjWithDesc != __nullptr);

			// If the supplier is enabled and its status is not kDoneStatus
			// (No need to update the status if one supplier is done)
			if (ipFSObjWithDesc->Enabled == VARIANT_TRUE && 
				ipFileSupplierData->FileSupplierStatus != kDoneStatus)
			{
				// Retrieve this File Supplier object
				UCLID_FILEPROCESSINGLib::IFileSupplierPtr	ipFS = ipFSObjWithDesc->Object;
				ASSERT_RESOURCE_ALLOCATION("ELI14029", ipFS != __nullptr);

				// pause the file supplier
				ipFS->Pause();

				// Set status to Paused
				ipFileSupplierData->PutFileSupplierStatus( UCLID_FILEPROCESSINGLib::kPausedStatus );

				if (m_hWndOfUI != __nullptr)
				{
					// Notify the dialog
					::PostMessage( m_hWndOfUI, 
						FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kPausedStatus, 
						(LPARAM)ipFS.GetInterfacePtr() );
				}
			}
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14231")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::Resume(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// check pre-conditions
		ASSERT_ARGUMENT("ELI14227", m_bEnabled == true);
		ASSERT_ARGUMENT("ELI14228", m_ipFileSuppliers != __nullptr);
		ASSERT_ARGUMENT("ELI14249", m_ipFileSuppliers->Size() > 0);

		// Update status and notify dialog
		long lCount = m_ipFileSuppliers->Size();
		for (int i = 0; i < lCount; i++)
		{
			// get the ith file supplier data object
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFileSupplierData = m_ipFileSuppliers->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI14239", ipFileSupplierData != __nullptr);

			// get the object with description
			IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
			ASSERT_RESOURCE_ALLOCATION("ELI14240", ipFSObjWithDesc != __nullptr);

			// If the supplier is enabled and its status is not kDoneStatus
			// (No need to update the status if one supplier is done)
			if (ipFSObjWithDesc->Enabled == VARIANT_TRUE && 
				ipFileSupplierData->FileSupplierStatus != kDoneStatus)
			{
				// Retrieve this File Supplier object
				UCLID_FILEPROCESSINGLib::IFileSupplierPtr	ipFS = ipFSObjWithDesc->Object;
				ASSERT_RESOURCE_ALLOCATION("ELI14241", ipFS != __nullptr);

				// pause the file supplier
				ipFS->Resume();

				// Set status to Active
				ipFileSupplierData->PutFileSupplierStatus( UCLID_FILEPROCESSINGLib::kActiveStatus );

				if (m_hWndOfUI != __nullptr)
				{
					// Notify dialog of status change
					::PostMessage( m_hWndOfUI, 
						FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kActiveStatus, 
						(LPARAM) ipFS.GetInterfacePtr() );
				}
			}
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14232")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::get_Enabled(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		*pVal = ( asVariantBool(m_bEnabled) );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14141")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::put_Enabled(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		m_bEnabled = (newVal == VARIANT_TRUE);
		
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14142")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::Clear(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// call the internal method
		clear();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14235")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::ValidateStatus(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_bEnabled)
		{
			// Initialize isValid to false
			bool isValid = false;

			// If there is more than one supplier
			if (m_ipFileSuppliers && m_ipFileSuppliers->Size() > 0)
			{
				long nNumFileSuppliers = m_ipFileSuppliers->Size();

				for (long n = 0; n < nNumFileSuppliers; n++)
				{
					// get the nth file supplier data object
					UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFileSupplierData = m_ipFileSuppliers->At(n);
					ASSERT_RESOURCE_ALLOCATION("ELI14373", ipFileSupplierData != __nullptr);

					// get the object with description
					IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
					ASSERT_RESOURCE_ALLOCATION("ELI14374", ipFSObjWithDesc != __nullptr);

					if (ipFSObjWithDesc->Enabled == VARIANT_TRUE)
					{
						isValid = true;
						break;
					}
				}
			}
			if (!isValid)
			{
				UCLIDException ue("ELI14360", "At least one file supplier should be specified and be enabled!");
				throw ue;
			}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14357")
}

//-------------------------------------------------------------------------------------------------
// IFileSupplyingMgmtRole interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::get_FileSuppliers(IIUnknownVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create collection, if needed
		if (m_ipFileSuppliers == __nullptr)
		{
			m_ipFileSuppliers.CreateInstance( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI13699", m_ipFileSuppliers != __nullptr );
		}

		IIUnknownVectorPtr ipShallowCopy = m_ipFileSuppliers;
		*pVal = ipShallowCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13700")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::put_FileSuppliers(IIUnknownVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Save collection
		m_ipFileSuppliers = newVal;

		m_bDirty = true;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13701")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::get_FAMCondition(IObjectWithDescription* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		if (m_ipFAMCondition == __nullptr)
		{
			m_ipFAMCondition.CreateInstance( CLSID_ObjectWithDescription );
			ASSERT_RESOURCE_ALLOCATION( "ELI13532", m_ipFAMCondition != __nullptr );
		}

		IObjectWithDescriptionPtr ipShallowCopy = m_ipFAMCondition;
		*pVal = ipShallowCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13533")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::put_FAMCondition(IObjectWithDescription *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		m_ipFAMCondition = newVal;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13534")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::SetDirty(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		m_bDirty = (newVal == VARIANT_TRUE);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19428")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::GetSupplyingCounts(long *plNumSupplied, long *plNumSupplyingErrors)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Not validating license since this call is just grabbing statistics

		if (plNumSupplied != __nullptr)
		{
			*plNumSupplied = m_nFilesSupplied;
		}
		if (plNumSupplyingErrors != __nullptr)
		{
			*plNumSupplyingErrors = m_nSupplyingErrors;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28472")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::get_SkipPageCount(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI35093", pVal != __nullptr);

		*pVal = asVariantBool(m_bSkipPageCount);

		return S_OK;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35094")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::put_SkipPageCount(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bSkipPageCount = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35095")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::get_StopOnFileSupplierFailure(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bStopOnFileSupplierFailure);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI54249")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::put_StopOnFileSupplierFailure(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		m_bStopOnFileSupplierFailure = asCppBool(newVal);

		// NOTE: we do not need to set the dirty flag because we did not change
		// any persistent data members.
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI54250")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IFileSupplierTarget
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::NotifyFileAdded(BSTR bstrFile, IFileSupplier *pSupplier,
	IFileRecord** ppFileRecord)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI33968", ppFileRecord != __nullptr);

		validateLicense();

		// Protect access to this method
		CSingleLock lg(&m_mutex, TRUE);

		// Get the description that was entered in the FPM when the FileSupplier was added to the list of file suppliers
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSData = getFileSupplierData(pSupplier);
		ASSERT_RESOURCE_ALLOCATION("ELI14006", ipFSData != __nullptr);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI14007", ipObjectWithDescription != __nullptr);
		string strFSDescription = ipObjectWithDescription->Description;

		// Get the file priority
		UCLID_FILEPROCESSINGLib::EFilePriority ePriority = ipFSData->Priority;
		string strPriority = getPriorityString(ePriority);

		try
		{
			// post the queue-event received notification
			postQueueEventReceivedNotification(bstrFile, strFSDescription, strPriority, kFileAdded);

			#ifdef INJECT_FS_EVENT_HANDLING_EXCEPTIONS
				// Defect Injection - randomly simulate failure in handling queue event
				if (rand() % 5 == 0)
				{
					string strMsg = "Random exception #";
					strMsg += asString(rand() % 1000);
					strMsg += ".";
					throw UCLIDException("ELI14932", strMsg);
				}
			#endif

			// Simplify file name if the file name contains relative 
			// path such as"\.", "\\.."
			string strFile = asString(bstrFile);
			simplifyPathName(strFile);
			_bstr_t bstrSimplifiedName = get_bstr_t(strFile);
			
			// check if the file matches the FAM condition
			if (fileMatchesFAMCondition(strFile))
			{
				return S_OK;
			}
			
			// Add the file to the database
			VARIANT_BOOL vbAlreadyExists;
			VARIANT_BOOL vbForceProcessing = ipFSData->ForceProcessing;
			UCLID_FILEPROCESSINGLib::EActionStatus easPrev;
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);
			ipFileRecord = getFPMDB()->AddFile(bstrSimplifiedName, m_strAction.c_str(), 
				-1, ePriority, vbForceProcessing, VARIANT_FALSE,
				UCLID_FILEPROCESSINGLib::kActionPending, asVariantBool(m_bSkipPageCount),
				&vbAlreadyExists, &easPrev );	

			bool bAlreadyExists = asCppBool(vbAlreadyExists);

			// Increment the number of files supplied if:
			// 1. The file did not already exist OR
			// 2. The file existed and ForceProcessing is on
			if (!bAlreadyExists || vbForceProcessing == VARIANT_TRUE)
			{
				m_nFilesSupplied++;
			}

			if (m_hWndOfUI != __nullptr)
			{
				// Create and fill the FileSupplyingRecord to be passed to PostMessage
				// Using an auto pointer in order to prevent a memory leak due to exceptions
				unique_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
				apFileSupRec->m_eFSRecordType = kFileAdded;
				apFileSupRec->m_strFSDescription = strFSDescription;
				apFileSupRec->m_strOriginalFileName = strFile;
				apFileSupRec->m_ulFileID = (unsigned long) ipFileRecord->FileID;
				apFileSupRec->m_ePreviousActionStatus = easPrev;
				apFileSupRec->m_ulNumPages = ipFileRecord->Pages;
				apFileSupRec->m_bAlreadyExisted = bAlreadyExists;
				apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
				apFileSupRec->m_strPriority = strPriority;

				// Post the message which will be handled by the FPDlg's OnQueueEvent method
				::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
			}

			*ppFileRecord = (IFileRecord*)ipFileRecord.Detach();
		}
		catch (...)
		{
			handleFileSupplyingException("ELI13792", kFileAdded, bstrFile, strFSDescription, strPriority);
		}
	}
	catch (...)
	{
		uex::logOrDisplayCurrent("ELI14927", m_hWndOfUI != __nullptr);
	}

	return S_OK;  // we don't want the notification methods to return an error code.
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::NotifyFileRemoved(BSTR bstrFile, IFileSupplier *pSupplier)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Protect access to this method
		CSingleLock lg(&m_mutex, TRUE);

		// Get the description that was entered in the FPM when the FileSupplier was 
		// added to the list of file suppliers
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSData = getFileSupplierData(pSupplier);
		ASSERT_RESOURCE_ALLOCATION("ELI14008", ipFSData != __nullptr);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI14009", ipObjectWithDescription != __nullptr);
		string strFSDescription = ipObjectWithDescription->Description;

		// Get the file priority
		string strPriority = getPriorityString(ipFSData->Priority);

		try
		{
			// post the queue-event received notification
			postQueueEventReceivedNotification(bstrFile, strFSDescription, strPriority, kFileRemoved);

			#ifdef INJECT_FS_EVENT_HANDLING_EXCEPTIONS
				// Defect Injection - randomly simulate failure in handling queue event
				if (rand() % 5 == 0)
				{
					string strMsg = "Random exception #";
					strMsg += asString(rand() % 1000);
					strMsg += ".";
					throw UCLIDException("ELI14933", strMsg);
				}
			#endif

			// Get the name and simplify file name if the file 
			// name contains "\.", "\\.."
			string strFile = asString(bstrFile);
			simplifyPathName(strFile);
			_bstr_t bstrSimplifiedName = get_bstr_t(strFile);
			
			// check if the file matches the FAM condition
			if (fileMatchesFAMCondition(strFile))
			{
				return S_OK;
			}

			// Remove the files from the database
			getFPMDB()->RemoveFile(bstrSimplifiedName, m_strAction.c_str());
		
			if (m_hWndOfUI != __nullptr)
			{
				// Create and fill the FileSupplyingRecord to be passed to PostMessage
				// Using an auto pointer in order to prevent a memory leak due to exceptions
				unique_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
				apFileSupRec->m_eFSRecordType = kFileRemoved;
				apFileSupRec->m_strFSDescription = strFSDescription;
				apFileSupRec->m_strOriginalFileName = strFile;
				apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
				apFileSupRec->m_strPriority = strPriority;

				// Post the message which will be handled by the FPDlg's OnQueueEvent method
				::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
			}
		}
		catch (...)
		{
			handleFileSupplyingException("ELI14931", kFileRemoved, bstrFile, strFSDescription, strPriority);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14010");

	return S_OK;  // we don't want the notification methods to return an error code.
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::NotifyFileRenamed(BSTR bstrOldFile, BSTR bstrNewFile, 
													   IFileSupplier *pSupplier)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Protect access to this method
		CSingleLock lg(&m_mutex, TRUE);

		// Get the description that was entered in the FPM when the FileSupplier was added to the list of file suppliers
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSData = getFileSupplierData(pSupplier);
		ASSERT_RESOURCE_ALLOCATION("ELI14011", ipFSData != __nullptr);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI14012", ipObjectWithDescription != __nullptr);
		string strFSDescription = ipObjectWithDescription->Description;

		// Get the file priority
		UCLID_FILEPROCESSINGLib::EFilePriority ePriority = ipFSData->Priority;
		string strPriority = getPriorityString(ePriority);

		try
		{
			// post the queue-event received notification
			postQueueEventReceivedNotification(bstrOldFile, strFSDescription, strPriority, kFileRenamed);

			#ifdef INJECT_FS_EVENT_HANDLING_EXCEPTIONS
				// Defect Injection - randomly simulate failure in handling queue event
				if (rand() % 5 == 0)
				{
					string strMsg = "Random exception #";
					strMsg += asString(rand() % 1000);
					strMsg += ".";
					throw UCLIDException("ELI14938", strMsg);
				}
			#endif

			// Get the old name and simplify file name if the old 
			// name contains "\.", "\\.."
			string strOldFile = asString(bstrOldFile);
			simplifyPathName(strOldFile);
			_bstr_t bstrOldSimplifiedName = get_bstr_t(strOldFile);

			// Determine if the old file name matches the FAM condition
			bool bSkipOld = fileMatchesFAMCondition(strOldFile);
			
			// Get the new name and simplify file name if the new 
			// name contains "\.", "\\.."
			string strNewFile = asString(bstrNewFile);
			simplifyPathName(strNewFile);
			_bstr_t bstrNewSimplifiedName = get_bstr_t(strNewFile);

			// Determine if new file name matches the FAM condition
			bool bSkipNew = fileMatchesFAMCondition(strNewFile);

			

			// Create the FileRecordPtr to catch AddFile's return value.
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);

			UCLID_FILEPROCESSINGLib::EActionStatus easPrev;
			VARIANT_BOOL bAlreadyExists;
			bool bIsAdded = false;
			if (!bSkipNew)
			{
				// Add the new filename to the db
				ipFileRecord = getFPMDB()->AddFile(bstrNewSimplifiedName, m_strAction.c_str(), 
					-1, ePriority, ipFSData->ForceProcessing, VARIANT_FALSE,
					UCLID_FILEPROCESSINGLib::kActionPending, asVariantBool(m_bSkipPageCount),
					&bAlreadyExists, &easPrev );		
				bIsAdded = true;
			}

			if (!bSkipOld)
			{
				// Remove the old file from the database
				getFPMDB()->RemoveFile(bstrOldSimplifiedName, m_strAction.c_str());
			}
		
			if (m_hWndOfUI != __nullptr)
			{
				// Create and fill the FileSupplyingRecord to be passed to PostMessage
				// Using an auto pointer in order to prevent a memory leak due to exceptions
				unique_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
				apFileSupRec->m_eFSRecordType = kFileRenamed;
				apFileSupRec->m_strFSDescription = strFSDescription;
				apFileSupRec->m_strOriginalFileName = strOldFile;
				apFileSupRec->m_strNewFileName = strNewFile;
				apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
				apFileSupRec->m_strPriority = strPriority;

				if (bIsAdded)
				{
					// If added get the previous status and whether it already existed
					apFileSupRec->m_ePreviousActionStatus = easPrev;
					apFileSupRec->m_bAlreadyExisted = asCppBool(bAlreadyExists);

					// If the file was added, we have useful info in ipFileRecord
					apFileSupRec->m_ulFileID = (unsigned long) ipFileRecord->FileID;
					apFileSupRec->m_ulNumPages = ipFileRecord->Pages;
				}

				// Post the message which will be handled by the FPDlg's OnQueueEvent method
				::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
			}
		}
		catch (...)
		{
			handleFileSupplyingException("ELI14937", kFileRenamed, bstrOldFile, strFSDescription, strPriority);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14013");

	return S_OK;  // we don't want the notification methods to return an error code.
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::NotifyFileModified(BSTR bstrFile, IFileSupplier *pSupplier)
{
	try
	{
		validateLicense();

		// Protect access to this method
		CSingleLock lg(&m_mutex, TRUE);

		// Get the description that was entered in the FPM when the FileSupplier was added to the list of file suppliers
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSData = getFileSupplierData(pSupplier);
		ASSERT_RESOURCE_ALLOCATION("ELI14014", ipFSData != __nullptr);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI14015", ipObjectWithDescription != __nullptr);
		string strFSDescription = ipObjectWithDescription->Description;

		// Get the file priority
		UCLID_FILEPROCESSINGLib::EFilePriority ePriority = ipFSData->Priority;
		string strPriority = getPriorityString(ePriority);

		try
		{
			// post the queue-event received notification
			postQueueEventReceivedNotification(bstrFile, strFSDescription, strPriority, kFileModified);

			#ifdef INJECT_FS_EVENT_HANDLING_EXCEPTIONS
				// Defect Injection - randomly simulate failure in handling queue event
				if (rand() % 5 == 0)
				{
					string strMsg = "Random exception #";
					strMsg += asString(rand() % 1000);
					strMsg += ".";
					throw UCLIDException("ELI14935", strMsg);
				}
			#endif

			// Get the name and simplify file name if the file 
			// name contains "\.", "\\.."
			string strFile = asString(bstrFile);
			simplifyPathName(strFile);
			_bstr_t bstrSimplifiedName = get_bstr_t(strFile);
			
			// check if the file matches the FAM condition
			if (fileMatchesFAMCondition(strFile))
			{
				return S_OK;
			}

			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);

			// Add the file to the database with the Modified Flag set
			VARIANT_BOOL bAlreadyExists;
			UCLID_FILEPROCESSINGLib::EActionStatus easPrev;
			ipFileRecord = getFPMDB()->AddFile(bstrSimplifiedName, m_strAction.c_str(),
				-1, ePriority, ipFSData->ForceProcessing, VARIANT_TRUE,
				UCLID_FILEPROCESSINGLib::kActionPending, asVariantBool(m_bSkipPageCount),
				&bAlreadyExists, &easPrev );
		
			if (m_hWndOfUI != __nullptr)
			{
				// Create and fill the FileSupplyingRecord to be passed to PostMessage
				// Using an auto pointer in order to prevent a memory leak due to exceptions
				unique_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
				apFileSupRec->m_bAlreadyExisted = (bAlreadyExists == VARIANT_TRUE);
				apFileSupRec->m_eFSRecordType = kFileModified;
				apFileSupRec->m_strFSDescription = strFSDescription;
				apFileSupRec->m_strOriginalFileName = strFile;
				apFileSupRec->m_ulFileID = (unsigned long) ipFileRecord->FileID;
				apFileSupRec->m_ePreviousActionStatus = easPrev;
				apFileSupRec->m_ulNumPages = ipFileRecord->Pages;
				apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
				apFileSupRec->m_strPriority = strPriority;
				// Post the message which will be handled by the FPDlg's OnQueueEvent method
				::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
			}
		}
		catch (...)
		{
			handleFileSupplyingException("ELI14936", kFileModified, bstrFile, strFSDescription, strPriority);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14016");

	return S_OK;  // we don't want the notification methods to return an error code.
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::NotifyFolderDeleted(BSTR bstrFolder, IFileSupplier *pSupplier)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Protect access to this method
		CSingleLock lg(&m_mutex, TRUE);

		// Get the description that was entered in the FPM when the FileSupplier was added to the list of file suppliers
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSData = getFileSupplierData(pSupplier);
		ASSERT_RESOURCE_ALLOCATION("ELI19426", ipFSData != __nullptr);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI19427", ipObjectWithDescription != __nullptr);
		string strFSDescription = ipObjectWithDescription->Description;

		// Get the file priority
		string strPriority = getPriorityString(ipFSData->Priority);

		try
		{
			// post the queue-event received notification
			postQueueEventReceivedNotification(bstrFolder, strFSDescription, strPriority, kFolderRemoved);

			#ifdef INJECT_FS_EVENT_HANDLING_EXCEPTIONS
				// Defect Injection - randomly simulate failure in handling queue event
				if (rand() % 5 == 0)
				{
					string strMsg = "Random exception #";
					strMsg += asString(rand() % 1000);
					strMsg += ".";
					throw UCLIDException("ELI14942", strMsg);
				}
			#endif

			// remove the folder from the database
			getFPMDB()->RemoveFolder(bstrFolder, m_strAction.c_str());

			if (m_hWndOfUI != __nullptr)
			{
				// Create and fill the FileSupplyingRecord to be passed to PostMessage
				// Using an auto pointer in order to prevent a memory leak due to exceptions
				unique_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
				apFileSupRec->m_eFSRecordType = kFolderRemoved;
				apFileSupRec->m_strFSDescription = asString(ipObjectWithDescription->Description);
				apFileSupRec->m_strOriginalFileName = asString(bstrFolder);
				apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
				apFileSupRec->m_strPriority = strPriority;

				// Post the message which will be handled by the FPDlg's OnQueueEvent method
				::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
			}
		}
		catch (...)
		{
			handleFileSupplyingException("ELI14940", kFolderRemoved, bstrFolder, strFSDescription, strPriority);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14148");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::NotifyFolderRenamed(BSTR bstrOldFolder, BSTR bstrNewFolder, IFileSupplier *pSupplier)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Protect access to this method
		CSingleLock lg(&m_mutex, TRUE);

		// Get the description that was entered in the FPM when the FileSupplier was added to the list of file suppliers
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSData = getFileSupplierData(pSupplier);
		ASSERT_RESOURCE_ALLOCATION("ELI14944", ipFSData != __nullptr);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI14945", ipObjectWithDescription != __nullptr);
		string strFSDescription = ipObjectWithDescription->Description;

		// Get the file priority
		string strPriority = getPriorityString(ipFSData->Priority);

		try
		{
			// post the queue-event received notification
			postQueueEventReceivedNotification(bstrOldFolder, strFSDescription, strPriority, kFolderRenamed);

			#ifdef INJECT_FS_EVENT_HANDLING_EXCEPTIONS
				// Defect Injection - randomly simulate failure in handling queue event
				if (rand() % 5 == 0)
				{
					string strMsg = "Random exception #";
					strMsg += asString(rand() % 1000);
					strMsg += ".";
					throw UCLIDException("ELI14943", strMsg);
				}
			#endif


			// TODO: Database end needs to be implemented
			//		 This currently doesn't do anything

			if (m_hWndOfUI != __nullptr)
			{
				// TODO: Need to set-up like it was done in FileRename once implementation is done
				// Create and fill the FileSupplyingRecord to be passed to PostMessage
				// Using an auto pointer in order to prevent a memory leak due to exceptions
				unique_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
				apFileSupRec->m_eFSRecordType = kFolderRenamed;
				apFileSupRec->m_strFSDescription = strFSDescription;
				apFileSupRec->m_strOriginalFileName = asString(bstrOldFolder);
				apFileSupRec->m_strNewFileName = asString(bstrNewFolder);
				apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
				apFileSupRec->m_strPriority = strPriority;

				// Create and fill the FileSupplyingRecord to be passed to PostMessage
				::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM)apFileSupRec.release(), 0);
			}
		}
		catch (...)
		{
			handleFileSupplyingException("ELI14941", kFolderRenamed, bstrOldFolder, strFSDescription, strPriority);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14149");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CFileSupplyingMgmtRole::postQueueEventReceivedNotification(BSTR bstrFile, 
																const string& strFSDescription,
																const string& strPriority,
																EFileSupplyingRecordType eFSRecordType)
{
	if (m_hWndOfUI != __nullptr)
	{
		// Create and fill the FileSupplyingRecord to be passed to PostMessage
		// Using an auto pointer in order to prevent a memory leak due to exceptions
		unique_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
		apFileSupRec->m_eFSRecordType = eFSRecordType;
		apFileSupRec->m_strFSDescription = strFSDescription;
		apFileSupRec->m_strOriginalFileName = asString(bstrFile);
		apFileSupRec->m_eQueueEventStatus = kQueueEventReceived;
		apFileSupRec->m_strPriority = strPriority;

		// Post the message which will be handled by the FPDlg's OnQueueEvent method
		::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
	}
}
//-------------------------------------------------------------------------------------------------
void CFileSupplyingMgmtRole::postQueueEventFailedNotification(BSTR bstrFile, 
															  const string& strFSDescription,
															  const string& strPriority,
															  EFileSupplyingRecordType eFSRecordType,
															  const UCLIDException& ue)
{
	if (m_hWndOfUI != __nullptr)
	{
		// Create and fill the FileSupplyingRecord to be passed to PostMessage
		// Using an auto pointer in order to prevent a memory leak due to exceptions
		unique_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
		apFileSupRec->m_eFSRecordType = eFSRecordType;
		apFileSupRec->m_strFSDescription = strFSDescription;
		apFileSupRec->m_strOriginalFileName = asString(bstrFile);
		apFileSupRec->m_eQueueEventStatus = kQueueEventFailed;
		apFileSupRec->m_ueException = ue;
		apFileSupRec->m_strPriority = strPriority;

		// Post the message which will be handled by the FPDlg's OnQueueEvent method
		::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
	}
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::NotifyFileSupplyingDone(IFileSupplier *pSupplier)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI14525", m_ipRoleNotifyFAM != __nullptr );

		validateLicense();

		// Protect access to this method
		CSingleLock lg(&m_mutex, TRUE);

		// Find the FileSupplierData object for this Supplier
		long lCount = m_ipFileSuppliers->Size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this File Supplier Data object
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSD = m_ipFileSuppliers->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI13998", ipFSD != __nullptr);

			// Retrieve Object With Description
			IObjectWithDescriptionPtr ipOWD = ipFSD->GetFileSupplier();
			ASSERT_RESOURCE_ALLOCATION("ELI15641", ipOWD != __nullptr);

			// Retrieve File Supplier
			UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFS = ipOWD->Object;
			ASSERT_RESOURCE_ALLOCATION("ELI13999", ipFS != __nullptr);

			// Compare File Supplier items
			if (ipFS == pSupplier)
			{
				// Update status and notify dialog
				ipFSD->PutFileSupplierStatus( UCLID_FILEPROCESSINGLib::kDoneStatus );
				if (m_hWndOfUI != __nullptr)
				{
					::PostMessage( m_hWndOfUI, 
						FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kDoneStatus, 
						(LPARAM)ipFS.GetInterfacePtr() );
				}
				
				// Add to the count of finished suppliers
				m_nFinishedSupplierCount++;
				
				// if all are finished
				if ( m_nFinishedSupplierCount >= m_nEnabledSupplierCount )
				{
					// Notify the FAM that supplying is complete
					m_ipRoleNotifyFAM->NotifySupplyingCompleted();
				}
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19419")

	return S_OK;  // we don't want the notification methods to return an error code.
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::NotifyFileSupplyingFailed(IFileSupplier *pSupplier, BSTR strError)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	UCLIDException originalException;
	originalException.createFromString("ELI14122", asString(strError));

	UCLIDException* newException = __nullptr;
	try
	{
		ASSERT_ARGUMENT("ELI14526", m_ipRoleNotifyFAM != __nullptr );

		validateLicense();

		// Protect access to this method
		CSingleLock lg(&m_mutex, TRUE);

		// Find the FileSupplierData object for this Supplier
		long lCount = m_ipFileSuppliers->Size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this File Supplier Data object
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSD = m_ipFileSuppliers->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI14125", ipFSD != __nullptr);

			// Retrieve Object With Description
			IObjectWithDescriptionPtr ipOWD = ipFSD->GetFileSupplier();
			ASSERT_RESOURCE_ALLOCATION("ELI15642", ipOWD != __nullptr);

			// Retrieve File Supplier
			UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFS = ipOWD->Object;
			ASSERT_RESOURCE_ALLOCATION("ELI14124", ipFS != __nullptr);

			// Compare File Supplier items
			if (ipFS == pSupplier)
			{
				// Update status and notify dialog
				ipFSD->PutFileSupplierStatus( UCLID_FILEPROCESSINGLib::kDoneStatus );
				if (m_hWndOfUI != __nullptr)
				{
					::PostMessage( m_hWndOfUI, 
						FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kDoneStatus, 
						(LPARAM)ipFS.GetInterfacePtr() );
				}

				// Add to the count of suppliers that have finished
				m_nFinishedSupplierCount++;

				// if all are finished
				if ( m_nFinishedSupplierCount >= m_nEnabledSupplierCount )
				{
					// Notify the FAM that Supplying is complete
					m_ipRoleNotifyFAM->NotifySupplyingCompleted();
				}
			}
		}
	}
	catch (...)
	{
		newException = &uex::fromCurrent("ELI14123");
	};

	bool displayExceptions = m_hWndOfUI != __nullptr;
	if (displayExceptions)
	{
		originalException.display();
		if (newException)
		{
			newException->display();
		}
	}
	else
	{
		originalException.log();
		if (newException)
		{
			newException->log();
		}
	}

	// If configured to stop processing on a file supplier failure, then stop via cancel processing
	// https://extract.atlassian.net/browse/ISSUE-19209
	if (m_bStopOnFileSupplierFailure)
	{
		m_ipRoleNotifyFAM->NotifyProcessingCancelling();
	}

	return S_OK;  // we don't want the notification methods to return an error code.
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_FileSupplyingMgmtRole;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{

		// if the directly held data is dirty, then indicate to the caller that
		// this object is dirty
		if (m_bDirty)
		{
			return S_OK;
		}

		// check if the file suppliers vector object is dirty
		if (m_ipFileSuppliers != __nullptr)
		{
			IPersistStreamPtr ipFSStream = m_ipFileSuppliers;
			ASSERT_RESOURCE_ALLOCATION("ELI14254", ipFSStream != __nullptr);
			if (ipFSStream->IsDirty() == S_OK)
			{
				return S_OK;
			}
		}

		// check if the FAM condition obj-with-desc is dirty
		if (m_ipFAMCondition != __nullptr)
		{
			IPersistStreamPtr ipFSStream = m_ipFAMCondition;
			ASSERT_RESOURCE_ALLOCATION("ELI14255", ipFSStream != __nullptr);
			if (ipFSStream->IsDirty() == S_OK)
			{
				return S_OK;
			}
		}

		// if we reached here, it means that the object is not dirty
		// indicate to the caller that this object is not dirty
		return S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30416");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset all the member variables
		clear();

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
			UCLIDException ue( "ELI14433", "Unable to load newer File Supplying Management Role." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Read Enabled status
		dataReader >> m_bEnabled;
		if (nDataVersion >= 2)
		{
			dataReader >> m_bSkipPageCount;
		}

		// Read in the collected File Suppliers
		IPersistStreamPtr ipFSObj;
		readObjectFromStream( ipFSObj, pStream, "ELI14447" );
		m_ipFileSuppliers = ipFSObj;

		// Read in the FAM Condition with its Description
		IPersistStreamPtr ipFAMObj;
		readObjectFromStream( ipFAMObj, pStream, "ELI14448" );
		m_ipFAMCondition = ipFAMObj;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19391");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::Save(IStream *pStream, BOOL fClearDirty)
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

		// Save the enabled flag
		dataWriter << m_bEnabled;
		dataWriter << m_bSkipPageCount;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Make sure that File Suppliers has been initialized
		if ( m_ipFileSuppliers == __nullptr  )
		{
			m_ipFileSuppliers.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI14585", m_ipFileSuppliers != __nullptr );
		}
		
		// Save the File Suppliers
		IPersistStreamPtr ipFSObj = m_ipFileSuppliers;
		ASSERT_RESOURCE_ALLOCATION( "ELI14449", ipFSObj != __nullptr );
		writeObjectToStream( ipFSObj, pStream, "ELI14450", fClearDirty );

		// Make sure the FAMCondition has been allocated
		if ( m_ipFAMCondition == __nullptr )
		{
			m_ipFAMCondition.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI14584", m_ipFAMCondition != __nullptr );
		}
		
		// Save the FAM Condition
		IPersistStreamPtr ipFAMObj = m_ipFAMCondition;
		ASSERT_RESOURCE_ALLOCATION( "ELI14451", ipFAMObj != __nullptr );
		writeObjectToStream( ipFAMObj, pStream, "ELI14452", fClearDirty );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19392");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CFileSupplyingMgmtRole::releaseSupplyingThreadDataObjects()
{
	// release memory allocated for SupplierThreadData objects referenced by the pointers 
	// in m_vecSupplyingThreadData
	vector<SupplierThreadData *>::const_iterator iter;
	for (iter = m_vecSupplyingThreadData.begin(); iter != m_vecSupplyingThreadData.end(); iter++)
	{
		// get the thread data object
		SupplierThreadData *pSupplierThreadData = *iter;
		
		// if the thread has not yet ended, there's an internal logic error
		if (!pSupplierThreadData->m_threadEndedEvent.isSignaled())
		{
			UCLIDException ue("ELI19421", "Internal error: File supplier thread active, but associated thread data object is being deleted!");
			ue.log();
		}

		// release the memory
		delete pSupplierThreadData;
	}

	// clear the vector
	m_vecSupplyingThreadData.clear();
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr CFileSupplyingMgmtRole::getFileSupplierData(UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFS)
{
	long nNumFileSuppliers = m_ipFileSuppliers->Size();
	for (long n = 0; n < nNumFileSuppliers; n++)
	{
		// get the nth file supplier data object
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFileSupplierData = m_ipFileSuppliers->At(n);
		ASSERT_RESOURCE_ALLOCATION("ELI13995", ipFileSupplierData != __nullptr);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFileSupplierData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI13996", ipObjectWithDescription != __nullptr);

		// get the file supplier
		UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFileSupplier = ipObjectWithDescription->Object;
		ASSERT_RESOURCE_ALLOCATION("ELI13997", ipFileSupplier != __nullptr);

		if ( ipFileSupplier == ipFS )
		{
			return ipFileSupplierData;
		}
	}
	return NULL;
}
//-------------------------------------------------------------------------------------------------
bool CFileSupplyingMgmtRole::fileMatchesFAMCondition(const string& strFile)
{
	// check to see if the file meets the FAM condition.  If so, ignore this notification
	if (m_ipFAMCondition != __nullptr)
	{
		// get the FAM condition object
		UCLID_FILEPROCESSINGLib::IFAMConditionPtr ipFAMCondition = m_ipFAMCondition->Object;

		// check if a skip condition has been specified and is enabled
		if (ipFAMCondition != __nullptr && m_ipFAMCondition->Enabled == VARIANT_TRUE)
		{
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);
			ASSERT_RESOURCE_ALLOCATION("ELI31356", ipFileRecord != __nullptr);
			ipFileRecord->Name = strFile.c_str();
			ipFileRecord->FileID = -1;
			VARIANT_BOOL vbMatch = ipFAMCondition->FileMatchesFAMCondition(
				ipFileRecord, getFPMDB(), m_lActionId, getFAMTagManager());
			if (vbMatch == VARIANT_TRUE)
			{
				return true;
			}
		}
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
long CFileSupplyingMgmtRole::getEnabledSupplierCount()
{
	// Get number of suppliers
	long nNumFileSuppliers = m_ipFileSuppliers->Size();

	// The number of enabled suppliers
	long nNumEnabledSuppliers = nNumFileSuppliers;

	for (long n = 0; n < nNumFileSuppliers; n++)
	{
		// Get the nth file supplier data object
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFileSupplierData = m_ipFileSuppliers->At(n);
		ASSERT_RESOURCE_ALLOCATION("ELI15214", ipFileSupplierData != __nullptr);

		// Get the object with description
		IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI15215", ipFSObjWithDesc != __nullptr);

		if (ipFSObjWithDesc->Enabled == VARIANT_FALSE)
		{
			// Decrease the number of enabled suppliers
			nNumEnabledSuppliers--;
		}
	}

	return nNumEnabledSuppliers;
}
//-------------------------------------------------------------------------------------------------
void CFileSupplyingMgmtRole::validateLicense()
{
	// use the same ID as the file action manager.  If the FAM is licensed, so is this.
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI14275", "File Supplying Management Role");
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr CFileSupplyingMgmtRole::getFPMDB()
{
	// ensure that the db pointer is not NULL
	if (m_pDB == NULL)
	{
		throw UCLIDException("ELI14210", "No database available!");
	}

	return m_pDB;
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr CFileSupplyingMgmtRole::getFAMTagManager()
{
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipTagManager = m_pFAMTagManager;
	ASSERT_RESOURCE_ALLOCATION("ELI14402", ipTagManager != __nullptr);
	return ipTagManager;
}
//-------------------------------------------------------------------------------------------------
void CFileSupplyingMgmtRole::clear()
{
	// Reset the files supplies and failed counts
	m_nFilesSupplied = 0;
	m_nSupplyingErrors = 0;

	m_pDB = NULL;

	m_ipFAMCondition = __nullptr;
	if (m_ipFileSuppliers != __nullptr)
	{
		m_ipFileSuppliers->Clear();
		m_ipFileSuppliers = __nullptr;
	}

	m_bEnabled = false;
	m_bSkipPageCount = false;

	m_bDirty = false;

	//  Initialize the RoleNotifyFam to NULL
	m_ipRoleNotifyFAM = __nullptr;
	
	m_nFinishedSupplierCount = 0;

	m_bStopOnFileSupplierFailure = false;

	releaseSupplyingThreadDataObjects();
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr CFileSupplyingMgmtRole::getThisAsFileActionMgmtRole()
{
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI25313", ipThis != __nullptr);

	return ipThis;	
}
//-------------------------------------------------------------------------------------------------
bool CFileSupplyingMgmtRole::stopSupplingIfDBNotConnected()
{
	try
	{
		// Get the current database status if it is connection established then it is in a good 
		// state. Use try catch block to allow calling stop if an exception is thrown.
		try
		{
			if ( asString(getFPMDB()->GetCurrentConnectionStatus()) == gstrCONNECTION_ESTABLISHED)
			{ 
				return false;
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25322");

		// Always want to at least try to execute this if the database is in a bad state
		getThisAsFileActionMgmtRole()->Stop();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25321");

	// Supplying should be stopped
	return true;
}
//-------------------------------------------------------------------------------------------------
void CFileSupplyingMgmtRole::handleFileSupplyingException(
	const string& eliCode,
	EFileSupplyingRecordType eFSRecordType,
	BSTR bstrFile,
	const string& strFSDescription,
	const string& strPriority)
{
	m_nSupplyingErrors++;
	UCLIDException ue = uex::fromCurrent(eliCode);
	ue.addDebugInfo("FileName", asString(bstrFile));

	postQueueEventFailedNotification(bstrFile, strFSDescription, strPriority, eFSRecordType, ue);

	if (stopSupplingIfDBNotConnected())
	{
		UCLIDException ueConnection("ELI25314", "Unable to connect to database. Supplying stopped.", ue);
		if (m_hWndOfUI)
		{
			ueConnection.display();
		}
		else
		{
			ueConnection.log();
		}
	}
	else
	{
		ue.log();
	}
}
//-------------------------------------------------------------------------------------------------
