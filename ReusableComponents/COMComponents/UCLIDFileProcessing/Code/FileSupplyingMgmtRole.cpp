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
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// Macro for logging queue event exceptions in the UI and in the file 
//-------------------------------------------------------------------------------------------------
#define FS_MGMT_CATCH_AND_LOG_ALL_EXCEPTIONS(strELI, eFSRecordType) \
	catch (UCLIDException& ue) \
	{ \
		m_nSupplyingErrors++; \
		ue.addDebugInfo("CatchID", strELI); \
		postQueueEventFailedNotification(bstrFile, strFSDescription, strPriority, eFSRecordType, ue); \
		if ( stopSupplingIfDBNotConnected() ) \
		{ \
			UCLIDException ueConnection("ELI25314", "Unable to connect to database. Supplying stopped.", ue); \
			ueConnection.display(); \
		} \
		else \
		{ \
			ue.log(); \
		} \
	} \
	catch (_com_error& e) \
	{ \
		m_nSupplyingErrors++; \
		UCLIDException ue; \
		_bstr_t _bstrDescription = e.Description(); \
		char *pszDescription = _bstrDescription; \
		if (pszDescription) \
			ue.createFromString(strELI, pszDescription); \
		else \
			ue.createFromString(strELI, "COM exception caught!"); \
		ue.addHresult(e.Error()); \
		ue.addDebugInfo("err.WCode", e.WCode()); \
		ue.addDebugInfo("CatchID", strELI); \
		postQueueEventFailedNotification(bstrFile, strFSDescription, strPriority, eFSRecordType, ue); \
		if ( stopSupplingIfDBNotConnected() ) \
		{ \
			UCLIDException ueConnection("ELI25315", "Unable to connect to database. Supplying stopped.", ue); \
			ueConnection.display(); \
		} \
		else \
		{ \
			ue.log(); \
		} \
	} \
	catch (COleDispatchException *pEx) \
	{ \
		m_nSupplyingErrors++; \
		UCLIDException ue; \
		ue.createFromString(strELI, (LPCTSTR) pEx->m_strDescription); \
		ue.addDebugInfo("Error Code", pEx->m_wCode); \
		ue.addDebugInfo("CatchID", strELI); \
		pEx->Delete(); \
		postQueueEventFailedNotification(bstrFile, strFSDescription, strPriority, eFSRecordType, ue); \
		if ( stopSupplingIfDBNotConnected() ) \
		{ \
			UCLIDException ueConnection("ELI25316", "Unable to connect to database. Supplying stopped.", ue); \
			ueConnection.display(); \
		} \
		else \
		{ \
			ue.log(); \
		} \
	} \
	catch (COleDispatchException& ex) \
	{ \
		m_nSupplyingErrors++; \
		UCLIDException ue; \
		ue.createFromString(strELI, (LPCTSTR) ex.m_strDescription); \
		ue.addDebugInfo("Error Code", ex.m_wCode); \
		ue.addDebugInfo("CatchID", strELI); \
		postQueueEventFailedNotification(bstrFile, strFSDescription, strPriority, eFSRecordType, ue); \
		if ( stopSupplingIfDBNotConnected() ) \
		{ \
			UCLIDException ueConnection("ELI25317", "Unable to connect to database. Supplying stopped.", ue); \
			ueConnection.display(); \
		} \
		else \
		{ \
			ue.log(); \
		} \
	} \
	catch (COleException& ex) \
	{ \
		m_nSupplyingErrors++; \
		char pszCause[256] = {0}; \
		ex.GetErrorMessage(pszCause, 255); \
		UCLIDException ue; \
		ue.createFromString(strELI, pszCause); \
		ue.addDebugInfo("Status Code", ex.m_sc); \
		ue.addDebugInfo("CatchID", strELI); \
		postQueueEventFailedNotification(bstrFile, strFSDescription, strPriority, eFSRecordType, ue); \
		if ( stopSupplingIfDBNotConnected() ) \
		{ \
			UCLIDException ueConnection("ELI25318", "Unable to connect to database. Supplying stopped.", ue); \
			ueConnection.display(); \
		} \
		else \
		{ \
			ue.log(); \
		} \
	} \
	catch (CException* pEx) \
	{ \
		m_nSupplyingErrors++; \
		char pszCause[256] = {0}; \
		pEx->GetErrorMessage(pszCause, 255); \
		pEx->Delete(); \
		UCLIDException ue; \
		ue.createFromString(strELI, pszCause); \
		ue.addDebugInfo("CatchID", strELI); \
		postQueueEventFailedNotification(bstrFile, strFSDescription, strPriority, eFSRecordType, ue); \
		if ( stopSupplingIfDBNotConnected() ) \
		{ \
			UCLIDException ueConnection("ELI25319", "Unable to connect to database. Supplying stopped.", ue); \
			ueConnection.display(); \
		} \
		else \
		{ \
			ue.log(); \
		} \
	} \
	catch (...) \
	{ \
		m_nSupplyingErrors++; \
		UCLIDException ue(strELI, "Unexpected exception caught."); \
		ue.addDebugInfo("CatchID", strELI); \
		postQueueEventFailedNotification(bstrFile, strFSDescription, strPriority, eFSRecordType, ue); \
		if ( stopSupplingIfDBNotConnected() ) \
		{ \
			UCLIDException ueConnection("ELI25320", "Unable to connect to database. Supplying stopped.", ue); \
			ueConnection.display(); \
		} \
		else \
		{ \
			ue.log(); \
		} \
	}

//-------------------------------------------------------------------------------------------------
// SupplierThreadData class
//-------------------------------------------------------------------------------------------------
SupplierThreadData::SupplierThreadData(UCLID_FILEPROCESSINGLib::IFileSupplier *pFS,
	UCLID_FILEPROCESSINGLib::IFileSupplierTarget *pFST,
	UCLID_FILEPROCESSINGLib::IFAMTagManager *pFAMTM)
	:m_ipFileSupplier(pFS), m_ipFileSupplierTarget(pFST),  m_ipFAMTagManager(pFAMTM)
{
	// verify non-NULL arguments
	ASSERT_ARGUMENT("ELI13761", pFS != NULL);
	ASSERT_ARGUMENT("ELI13762", pFST != NULL);
	ASSERT_ARGUMENT("ELI14436", pFAMTM != NULL);
}

//-------------------------------------------------------------------------------------------------
UINT CFileSupplyingMgmtRole::fileSupplyingThreadProc(void *pData)
{
	try
	{
		// initialize COM for this thread
		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		// get the threadData object.
		SupplierThreadData *pSupplierThreadData = static_cast<SupplierThreadData *>(pData);

		// signal that the thread has started
		pSupplierThreadData->m_threadStartedEvent.signal();

		// start the file supplier
		// NOTE: this is in its own try/catch block in order to guarantee that we can guarantee the signaling
		// of the threadEndedEvent below the catch block.
		try
		{
			// Get the File Supplier
			UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFS = pSupplierThreadData->m_ipFileSupplier;
			ASSERT_RESOURCE_ALLOCATION("ELI15640", ipFS != NULL );
			ipFS->Start(pSupplierThreadData->m_ipFileSupplierTarget, 
				pSupplierThreadData->m_ipFAMTagManager);
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14242")

		// signal that the thread has ended
		pSupplierThreadData->m_threadEndedEvent.signal();

		// uninitialize COM for this thread
		CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19420")

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
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI14200")
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
			ASSERT_ARGUMENT("ELI14245", m_ipFileSuppliers != NULL);
			ASSERT_ARGUMENT("ELI14246", m_ipFileSuppliers->Size() > 0);

			m_ipRoleNotifyFAM = pRoleNotifyFAM;
			ASSERT_RESOURCE_ALLOCATION("ELI14523", m_ipRoleNotifyFAM != NULL );

			// release any memory that may have been allocated to managely previously spawned-off
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
				ASSERT_RESOURCE_ALLOCATION("ELI13768", ipFileSupplierData != NULL);

				// get the object with description
				IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
				ASSERT_RESOURCE_ALLOCATION("ELI13788", ipFSObjWithDesc != NULL);

				if (ipFSObjWithDesc->Enabled == VARIANT_TRUE)
				{
					// get the file supplier
					UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFileSupplier = ipFSObjWithDesc->Object;
					ASSERT_RESOURCE_ALLOCATION("ELI13770", ipFileSupplier != NULL);

					// get access to the IFileSupplierTarget interface on this object
					UCLID_FILEPROCESSINGLib::IFileSupplierTargetPtr ipTarget = this;
					ASSERT_RESOURCE_ALLOCATION("ELI13775", ipTarget);

					// create the thread data structure
					auto_ptr<SupplierThreadData> apThreadData(new SupplierThreadData(ipFileSupplier, ipTarget, getFAMTagManager()));

					// start the file supplier thread
					AfxBeginThread(fileSupplyingThreadProc, apThreadData.get());

					// wait for the file supplier thread to start
					apThreadData.get()->m_threadStartedEvent.wait();

					// push the thread data object to m_vecSupplyingThreadData so that it can be released later
					m_vecSupplyingThreadData.push_back(apThreadData.release());

					// Update status and notify dialog
					ipFileSupplierData->PutFileSupplierStatus( 
						UCLID_FILEPROCESSINGLib::kActiveStatus );

					::PostMessage( m_hWndOfUI, 
						FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kActiveStatus, 
						(LPARAM)ipFileSupplier.GetInterfacePtr());
				}
			}
		}
		catch (UCLIDException &ue)
		{
			UCLIDException uexOuter("ELI13759", "Unable to start all file suppliers!", ue);
			throw uexOuter;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14229")

	return S_OK;
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
		ASSERT_ARGUMENT("ELI14224", m_ipFileSuppliers != NULL);
		ASSERT_ARGUMENT("ELI14247", m_ipFileSuppliers->Size() > 0);
		ASSERT_ARGUMENT("ELI14524", m_ipRoleNotifyFAM != NULL );

		// notify all file suppliers to stop supplying
		long nNumFileSuppliers = m_ipFileSuppliers->Size();
		for (long n = 0; n < nNumFileSuppliers; n++)
		{
			// get the nth file supplier data object
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFileSupplierData = m_ipFileSuppliers->At(n);
			ASSERT_RESOURCE_ALLOCATION("ELI13895", ipFileSupplierData != NULL);

			// get the object with description
			IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
			ASSERT_RESOURCE_ALLOCATION("ELI13896", ipFSObjWithDesc != NULL);

			// If the supplier is enabled and its status is not kDoneStatus
			// (No need to update the status if one supplier is done)
			if (ipFSObjWithDesc->Enabled == VARIANT_TRUE && 
				ipFileSupplierData->FileSupplierStatus != kDoneStatus)
			{
				// get the file supplier
				UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFileSupplier = ipFSObjWithDesc->Object;
				ASSERT_RESOURCE_ALLOCATION("ELI13897", ipFileSupplier != NULL);

				ipFileSupplier->Stop();

				// Update status and notify dialog
				ipFileSupplierData->PutFileSupplierStatus( UCLID_FILEPROCESSINGLib::kStoppedStatus );

				::PostMessage( m_hWndOfUI, 
					FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kStoppedStatus, 
					(LPARAM)ipFileSupplier.GetInterfacePtr());
			}
		}
		// Notify the FAM that supplying is complete
		m_ipRoleNotifyFAM->NotifySupplyingCompleted();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14230")

	return S_OK;
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
		ASSERT_ARGUMENT("ELI14226", m_ipFileSuppliers != NULL);
		ASSERT_ARGUMENT("ELI14248", m_ipFileSuppliers->Size() > 0);

		// Update status and notify dialog
		long lCount = m_ipFileSuppliers->Size();
		for (int i = 0; i < lCount; i++)
		{
			// get the ith file supplier data object
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFileSupplierData = m_ipFileSuppliers->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI14237", ipFileSupplierData != NULL);

			// get the object with description
			IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
			ASSERT_RESOURCE_ALLOCATION("ELI14238", ipFSObjWithDesc != NULL);

			// If the supplier is enabled and its status is not kDoneStatus
			// (No need to update the status if one supplier is done)
			if (ipFSObjWithDesc->Enabled == VARIANT_TRUE && 
				ipFileSupplierData->FileSupplierStatus != kDoneStatus)
			{
				// Retrieve this File Supplier object
				UCLID_FILEPROCESSINGLib::IFileSupplierPtr	ipFS = ipFSObjWithDesc->Object;
				ASSERT_RESOURCE_ALLOCATION("ELI14029", ipFS != NULL);

				// pause the file supplier
				ipFS->Pause();

				// Set status to Paused
				ipFileSupplierData->PutFileSupplierStatus( UCLID_FILEPROCESSINGLib::kPausedStatus );

				// Notify the dialog
				::PostMessage( m_hWndOfUI, 
					FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kPausedStatus, 
					(LPARAM)ipFS.GetInterfacePtr() );
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14231")

	return S_OK;
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
		ASSERT_ARGUMENT("ELI14228", m_ipFileSuppliers != NULL);
		ASSERT_ARGUMENT("ELI14249", m_ipFileSuppliers->Size() > 0);

		// Update status and notify dialog
		long lCount = m_ipFileSuppliers->Size();
		for (int i = 0; i < lCount; i++)
		{
			// get the ith file supplier data object
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFileSupplierData = m_ipFileSuppliers->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI14239", ipFileSupplierData != NULL);

			// get the object with description
			IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
			ASSERT_RESOURCE_ALLOCATION("ELI14240", ipFSObjWithDesc != NULL);

			// If the supplier is enabled and its status is not kDoneStatus
			// (No need to update the status if one supplier is done)
			if (ipFSObjWithDesc->Enabled == VARIANT_TRUE && 
				ipFileSupplierData->FileSupplierStatus != kDoneStatus)
			{
				// Retrieve this File Supplier object
				UCLID_FILEPROCESSINGLib::IFileSupplierPtr	ipFS = ipFSObjWithDesc->Object;
				ASSERT_RESOURCE_ALLOCATION("ELI14241", ipFS != NULL);

				// pause the file supplier
				ipFS->Resume();

				// Set status to Active
				ipFileSupplierData->PutFileSupplierStatus( UCLID_FILEPROCESSINGLib::kActiveStatus );

				// Notify dialog of status change
				::PostMessage( m_hWndOfUI, 
					FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kActiveStatus, 
					(LPARAM) ipFS.GetInterfacePtr() );
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14232")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::get_Enabled(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		*pVal = ( asVariantBool(m_bEnabled) );
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
					ASSERT_RESOURCE_ALLOCATION("ELI14373", ipFileSupplierData != NULL);

					// get the object with description
					IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
					ASSERT_RESOURCE_ALLOCATION("ELI14374", ipFSObjWithDesc != NULL);

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
		if (m_ipFileSuppliers == NULL)
		{
			m_ipFileSuppliers.CreateInstance( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI13699", m_ipFileSuppliers != NULL );
		}

		IIUnknownVectorPtr ipShallowCopy = m_ipFileSuppliers;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13700")

	return S_OK;
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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13701")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::get_FAMCondition(IObjectWithDescription* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		if (m_ipFAMCondition == NULL)
		{
			m_ipFAMCondition.CreateInstance( CLSID_ObjectWithDescription );
			ASSERT_RESOURCE_ALLOCATION( "ELI13532", m_ipFAMCondition != NULL );
		}

		IObjectWithDescriptionPtr ipShallowCopy = m_ipFAMCondition;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13533")

	return S_OK;
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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13534")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::SetDirty(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		m_bDirty = (newVal == VARIANT_TRUE);
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

		if (plNumSupplied != NULL)
		{
			*plNumSupplied = m_nFilesSupplied;
		}
		if (plNumSupplyingErrors != NULL)
		{
			*plNumSupplyingErrors = m_nSupplyingErrors;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28472")
}

//-------------------------------------------------------------------------------------------------
// IFileSupplierTarget
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::NotifyFileAdded(BSTR bstrFile, IFileSupplier *pSupplier)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Protect access to this method
		CSingleLock lg(&m_mutex, TRUE);

		// Get the description that was entered in the FPM when the FileSupplier was added to the list of file suppliers
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSData = getFileSupplierData(pSupplier);
		ASSERT_RESOURCE_ALLOCATION("ELI14006", ipFSData != NULL);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI14007", ipObjectWithDescription != NULL);
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
				ePriority, vbForceProcessing, VARIANT_FALSE,
				UCLID_FILEPROCESSINGLib::kActionPending, &vbAlreadyExists, &easPrev );	

			bool bAlreadyExists = asCppBool(vbAlreadyExists);

			// Create and fill the FileSupplyingRecord to be passed to PostMessage
			// Using an auto pointer in order to prevent a memory leak due to exceptions
			auto_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
			apFileSupRec->m_eFSRecordType = kFileAdded;
			apFileSupRec->m_strFSDescription = strFSDescription;
			apFileSupRec->m_strOriginalFileName = strFile;
			apFileSupRec->m_ulFileID = asUnsignedLong(asString(ipFileRecord->FileID));
			apFileSupRec->m_ePreviousActionStatus = easPrev;
			apFileSupRec->m_ulNumPages = ipFileRecord->Pages;
			apFileSupRec->m_bAlreadyExisted = bAlreadyExists;
			apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
			apFileSupRec->m_strPriority = strPriority;

			// Increment the number of files supplied if:
			// 1. The file did not already exist OR
			// 2. The file existed and ForceProcessing is on
			if (!bAlreadyExists || vbForceProcessing == VARIANT_TRUE)
			{
				m_nFilesSupplied++;
			}

			// Post the message which will be handled by the FPDlg's OnQueueEvent method
			::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
		}
		FS_MGMT_CATCH_AND_LOG_ALL_EXCEPTIONS("ELI13792", kFileAdded)
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14927")

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
		ASSERT_RESOURCE_ALLOCATION("ELI14008", ipFSData != NULL);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI14009", ipObjectWithDescription != NULL);
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
		
			// Create and fill the FileSupplyingRecord to be passed to PostMessage
			// Using an auto pointer in order to prevent a memory leak due to exceptions
			auto_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
			apFileSupRec->m_eFSRecordType = kFileRemoved;
			apFileSupRec->m_strFSDescription = strFSDescription;
			apFileSupRec->m_strOriginalFileName = strFile;
			apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
			apFileSupRec->m_strPriority = strPriority;
			
			// Post the message which will be handled by the FPDlg's OnQueueEvent method
			::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
		}
		FS_MGMT_CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14931", kFileRemoved)
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

		// reference the bstrOldFile with a variable called bstrFile.  This is necessary
		// for the FS_MGMT_CATCH_AND_LOG_ALL_EXCEPTIONS macro below
		BSTR& bstrFile = bstrOldFile;

		// Get the description that was entered in the FPM when the FileSupplier was added to the list of file suppliers
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSData = getFileSupplierData(pSupplier);
		ASSERT_RESOURCE_ALLOCATION("ELI14011", ipFSData != NULL);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI14012", ipObjectWithDescription != NULL);
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

			// Create and fill the FileSupplyingRecord to be passed to PostMessage
			// Using an auto pointer in order to prevent a memory leak due to exceptions
			auto_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());

			// Create the FileRecordPtr to catch AddFile's return value.
			UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord(CLSID_FileRecord);

			bool bIsAdded = false;
			if (!bSkipNew)
			{
				VARIANT_BOOL bAlreadyExists;
				UCLID_FILEPROCESSINGLib::EActionStatus easPrev;
				// Add the new filename to the db
				ipFileRecord = getFPMDB()->AddFile(bstrNewSimplifiedName, m_strAction.c_str(), 
					ePriority, ipFSData->ForceProcessing, VARIANT_FALSE,
					UCLID_FILEPROCESSINGLib::kActionPending, &bAlreadyExists, &easPrev );		
				apFileSupRec->m_ePreviousActionStatus = easPrev;
				apFileSupRec->m_bAlreadyExisted = (bAlreadyExists == VARIANT_TRUE);
				bIsAdded = true;
			}

			if (!bSkipOld)
			{
				// Remove the old file from the database
				getFPMDB()->RemoveFile(bstrOldSimplifiedName, m_strAction.c_str());
			}
		
			// Fill the FileSupplyingRecord to be passed to PostMessage
			apFileSupRec->m_eFSRecordType = kFileRenamed;
			apFileSupRec->m_strFSDescription = strFSDescription;
			apFileSupRec->m_strOriginalFileName = strOldFile;
			apFileSupRec->m_strNewFileName = strNewFile;
			apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
			apFileSupRec->m_strPriority = strPriority;

			if (bIsAdded)
			{
				// If the file was added, we have useful info in ipFileRecord
				apFileSupRec->m_ulFileID = asUnsignedLong(asString(ipFileRecord->FileID));
				apFileSupRec->m_ulNumPages = ipFileRecord->Pages;
			}

			// Post the message which will be handled by the FPDlg's OnQueueEvent method
			::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
		}
		FS_MGMT_CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14937", kFileRenamed)
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
		ASSERT_RESOURCE_ALLOCATION("ELI14014", ipFSData != NULL);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI14015", ipObjectWithDescription != NULL);
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

			// Add the file to the database with the Mofified Flag set
			VARIANT_BOOL bAlreadyExists;
			UCLID_FILEPROCESSINGLib::EActionStatus easPrev;
			ipFileRecord = getFPMDB()->AddFile(bstrSimplifiedName, m_strAction.c_str(),
				ePriority, ipFSData->ForceProcessing, VARIANT_TRUE,
				UCLID_FILEPROCESSINGLib::kActionPending, &bAlreadyExists, &easPrev );
		
			// Create and fill the FileSupplyingRecord to be passed to PostMessage
			// Using an auto pointer in order to prevent a memory leak due to exceptions
			auto_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
			apFileSupRec->m_bAlreadyExisted = (bAlreadyExists == VARIANT_TRUE);
			apFileSupRec->m_eFSRecordType = kFileModified;
			apFileSupRec->m_strFSDescription = strFSDescription;
			apFileSupRec->m_strOriginalFileName = strFile;
			apFileSupRec->m_ulFileID = asUnsignedLong(asString(ipFileRecord->FileID));;
			apFileSupRec->m_ePreviousActionStatus = easPrev;
			apFileSupRec->m_ulNumPages = ipFileRecord->Pages;
			apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
			apFileSupRec->m_strPriority = strPriority;

			// Post the message which will be handled by the FPDlg's OnQueueEvent method
			::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
		}
		FS_MGMT_CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14936", kFileModified)
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

		// reference the bstrFolder with a variable called bstrFile.  This is necessary
		// for the FS_MGMT_CATCH_AND_LOG_ALL_EXCEPTIONS macro below
		BSTR& bstrFile = bstrFolder;

		// Get the description that was entered in the FPM when the FileSupplier was added to the list of file suppliers
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSData = getFileSupplierData(pSupplier);
		ASSERT_RESOURCE_ALLOCATION("ELI19426", ipFSData != NULL);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI19427", ipObjectWithDescription != NULL);
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

			// Create and fill the FileSupplyingRecord to be passed to PostMessage
			// Using an auto pointer in order to prevent a memory leak due to exceptions
			auto_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
			apFileSupRec->m_eFSRecordType = kFolderRemoved;
			apFileSupRec->m_strFSDescription = asString(ipObjectWithDescription->Description);
			apFileSupRec->m_strOriginalFileName = asString(bstrFolder);
			apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
			apFileSupRec->m_strPriority = strPriority;

			// Post the message which will be handled by the FPDlg's OnQueueEvent method
			::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
		}
		FS_MGMT_CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14940", kFolderRemoved)
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

		// reference the bstrFolder with a variable called bstrFile.  This is necessary
		// for the FS_MGMT_CATCH_AND_LOG_ALL_EXCEPTIONS macro below
		BSTR& bstrFile = bstrOldFolder;

		// Get the description that was entered in the FPM when the FileSupplier was added to the list of file suppliers
		UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr ipFSData = getFileSupplierData(pSupplier);
		ASSERT_RESOURCE_ALLOCATION("ELI14944", ipFSData != NULL);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFSData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI14945", ipObjectWithDescription != NULL);
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
			//		 This currently doesnt do anything

			// TODO: Need to set-up like it was done in FileRename once implementation is done
			// Create and fill the FileSupplyingRecord to be passed to PostMessage
			// Using an auto pointer in order to prevent a memory leak due to exceptions
			auto_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
			apFileSupRec->m_eFSRecordType = kFolderRenamed;
			apFileSupRec->m_strFSDescription = strFSDescription;
			apFileSupRec->m_strOriginalFileName = asString(bstrOldFolder);
			apFileSupRec->m_strNewFileName = asString(bstrNewFolder);
			apFileSupRec->m_eQueueEventStatus = kQueueEventHandled;
			apFileSupRec->m_strPriority = strPriority;

			// Create and fill the FileSupplyingRecord to be passed to PostMessage
			::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM)apFileSupRec.release(), 0);
		}
		FS_MGMT_CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14941", kFolderRenamed)
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
	// Create and fill the FileSupplyingRecord to be passed to PostMessage
	// Using an auto pointer in order to prevent a memory leak due to exceptions
	auto_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
	apFileSupRec->m_eFSRecordType = eFSRecordType;
	apFileSupRec->m_strFSDescription = strFSDescription;
	apFileSupRec->m_strOriginalFileName = asString(bstrFile);
	apFileSupRec->m_eQueueEventStatus = kQueueEventReceived;
	apFileSupRec->m_strPriority = strPriority;

	// Post the message which will be handled by the FPDlg's OnQueueEvent method
	::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
}
//-------------------------------------------------------------------------------------------------
void CFileSupplyingMgmtRole::postQueueEventFailedNotification(BSTR bstrFile, 
															  const string& strFSDescription,
															  const string& strPriority,
															  EFileSupplyingRecordType eFSRecordType,
															  const UCLIDException& ue)
{
	// Create and fill the FileSupplyingRecord to be passed to PostMessage
	// Using an auto pointer in order to prevent a memory leak due to exceptions
	auto_ptr<FileSupplyingRecord> apFileSupRec(new FileSupplyingRecord());
	apFileSupRec->m_eFSRecordType = eFSRecordType;
	apFileSupRec->m_strFSDescription = strFSDescription;
	apFileSupRec->m_strOriginalFileName = asString(bstrFile);
	apFileSupRec->m_eQueueEventStatus = kQueueEventFailed;
	apFileSupRec->m_ueException = ue;
	apFileSupRec->m_strPriority = strPriority;

	// Post the message which will be handled by the FPDlg's OnQueueEvent method
	::PostMessage(m_hWndOfUI, FP_QUEUE_EVENT, (WPARAM) apFileSupRec.release(), 0);
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileSupplyingMgmtRole::NotifyFileSupplyingDone(IFileSupplier *pSupplier)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI14525", m_ipRoleNotifyFAM != NULL );

		validateLicense();

		// Protect access to this method
		CSingleLock lg(&m_mutex, TRUE);

		// Find the FileSupplierData object for this Supplier
		long lCount = m_ipFileSuppliers->Size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this File Supplier Data object
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr	ipFSD = m_ipFileSuppliers->At( i );
			ASSERT_RESOURCE_ALLOCATION("ELI13998", ipFSD != NULL);

			// Retrieve Object With Description
			IObjectWithDescriptionPtr ipOWD = ipFSD->GetFileSupplier();
			ASSERT_RESOURCE_ALLOCATION("ELI15641", ipOWD != NULL);

			// Retrieve File Supplier
			UCLID_FILEPROCESSINGLib::IFileSupplierPtr	ipFS = ipOWD->GetObjectA();
			ASSERT_RESOURCE_ALLOCATION("ELI13999", ipFS != NULL);

			// Compare File Supplier items
			if (ipFS == pSupplier)
			{
				// Update status and notify dialog
				ipFSD->PutFileSupplierStatus( UCLID_FILEPROCESSINGLib::kDoneStatus );
				::PostMessage( m_hWndOfUI, 
					FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kDoneStatus, 
					(LPARAM)ipFS.GetInterfacePtr() );
				
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

	try
	{
		ASSERT_ARGUMENT("ELI14526", m_ipRoleNotifyFAM != NULL );

		validateLicense();

		// Protect access to this method
		CSingleLock lg(&m_mutex, TRUE);

		// Find the FileSupplierData object for this Supplier
		long lCount = m_ipFileSuppliers->Size();
		for (int i = 0; i < lCount; i++)
		{
			// Retrieve this File Supplier Data object
			UCLID_FILEPROCESSINGLib::IFileSupplierDataPtr	ipFSD = m_ipFileSuppliers->At( i );
			ASSERT_RESOURCE_ALLOCATION("ELI14125", ipFSD != NULL);

			// Retrieve Object With Description
			IObjectWithDescriptionPtr ipOWD = ipFSD->GetFileSupplier();
			ASSERT_RESOURCE_ALLOCATION("ELI15642", ipOWD != NULL);

			// Retrieve File Supplier
			UCLID_FILEPROCESSINGLib::IFileSupplierPtr	ipFS = ipOWD->GetObjectA();
			ASSERT_RESOURCE_ALLOCATION("ELI14124", ipFS != NULL);

			// Compare File Supplier items
			if (ipFS == pSupplier)
			{
				// Update status and notify dialog
				ipFSD->PutFileSupplierStatus( UCLID_FILEPROCESSINGLib::kDoneStatus );
				::PostMessage( m_hWndOfUI, 
					FP_SUPPLIER_STATUS_CHANGE, UCLID_FILEPROCESSINGLib::kDoneStatus, 
					(LPARAM)ipFS.GetInterfacePtr() );

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

		UCLIDException ue;
		ue.createFromString("ELI14122", asString(strError));
		throw ue;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14123");

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
		if (m_ipFileSuppliers != NULL)
		{
			IPersistStreamPtr ipFSStream = m_ipFileSuppliers;
			ASSERT_RESOURCE_ALLOCATION("ELI14254", ipFSStream != NULL);
			if (ipFSStream->IsDirty() == S_OK)
			{
				return S_OK;
			}
		}

		// check if the FAM condition obj-with-desc is dirty
		if (m_ipFAMCondition != NULL)
		{
			IPersistStreamPtr ipFSStream = m_ipFAMCondition;
			ASSERT_RESOURCE_ALLOCATION("ELI14255", ipFSStream != NULL);
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

		// Read in the collected File Suppliers
		IPersistStreamPtr ipFSObj;
		readObjectFromStream( ipFSObj, pStream, "ELI14447" );
		m_ipFileSuppliers = ipFSObj;

		// Read in the FAM Condition with its Description
		IPersistStreamPtr ipFAMObj;
		readObjectFromStream( ipFAMObj, pStream, "ELI14448" );
		m_ipFAMCondition = ipFAMObj;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19391");

	return S_OK;
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
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Make sure that File Suppliers has been initialized
		if ( m_ipFileSuppliers == NULL  )
		{
			m_ipFileSuppliers.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI14585", m_ipFileSuppliers != NULL );
		}
		
		// Save the File Suppliers
		IPersistStreamPtr ipFSObj = m_ipFileSuppliers;
		ASSERT_RESOURCE_ALLOCATION( "ELI14449", ipFSObj != NULL );
		writeObjectToStream( ipFSObj, pStream, "ELI14450", fClearDirty );

		// Make sure the FAMCondition has been allocated
		if ( m_ipFAMCondition == NULL )
		{
			m_ipFAMCondition.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI14584", m_ipFAMCondition != NULL );
		}
		
		// Save the FAM Condition
		IPersistStreamPtr ipFAMObj = m_ipFAMCondition;
		ASSERT_RESOURCE_ALLOCATION( "ELI14451", ipFAMObj != NULL );
		writeObjectToStream( ipFAMObj, pStream, "ELI14452", fClearDirty );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19392");

	return S_OK;
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
		ASSERT_RESOURCE_ALLOCATION("ELI13995", ipFileSupplierData != NULL);

		// get the object with description
		IObjectWithDescriptionPtr ipObjectWithDescription = ipFileSupplierData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI13996", ipObjectWithDescription != NULL);

		// get the file supplier
		UCLID_FILEPROCESSINGLib::IFileSupplierPtr ipFileSupplier = ipObjectWithDescription->Object;
		ASSERT_RESOURCE_ALLOCATION("ELI13997", ipFileSupplier != NULL);

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
	if (m_ipFAMCondition != NULL)
	{
		// get the FAM condition object
		UCLID_FILEPROCESSINGLib::IFAMConditionPtr ipFAMCondition = m_ipFAMCondition->Object;

		// check if a skip condition has been specified and is enabled
		if (ipFAMCondition != NULL && m_ipFAMCondition->Enabled == VARIANT_TRUE)
		{
			VARIANT_BOOL vbMatch = ipFAMCondition->FileMatchesFAMCondition(
				strFile.c_str(), getFPMDB(), -1, m_lActionId, getFAMTagManager());
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
		ASSERT_RESOURCE_ALLOCATION("ELI15214", ipFileSupplierData != NULL);

		// Get the object with description
		IObjectWithDescriptionPtr ipFSObjWithDesc = ipFileSupplierData->FileSupplier;
		ASSERT_RESOURCE_ALLOCATION("ELI15215", ipFSObjWithDesc != NULL);

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
	ASSERT_RESOURCE_ALLOCATION("ELI14402", ipTagManager != NULL);
	return ipTagManager;
}
//-------------------------------------------------------------------------------------------------
void CFileSupplyingMgmtRole::clear()
{
	// Reset the files supplies and failed counts
	m_nFilesSupplied = 0;
	m_nSupplyingErrors = 0;

	m_pDB = NULL;

	m_ipFAMCondition = NULL;
	if (m_ipFileSuppliers != NULL)
	{
		m_ipFileSuppliers->Clear();
		m_ipFileSuppliers = NULL;
	}

	m_bEnabled = false;

	m_bDirty = false;

	//  Initialize the RoleNotifyFam to NULL
	m_ipRoleNotifyFAM = NULL;
	
	m_nFinishedSupplierCount = 0;

	releaseSupplyingThreadDataObjects();
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr CFileSupplyingMgmtRole::getThisAsFileActionMgmtRole()
{
	UCLID_FILEPROCESSINGLib::IFileActionMgmtRolePtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI25313", ipThis != NULL);

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
