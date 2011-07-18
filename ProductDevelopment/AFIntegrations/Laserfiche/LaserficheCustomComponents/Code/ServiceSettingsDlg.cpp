// ServiceSettingsDlg.cpp : implementation file
//
#include "stdafx.h"
#include "LaserficheCustomComponents.h"
#include "ServiceSettingsDlg.h"
#include "IDShieldLF.h"

#include <LFMiscUtils.h>
#include <CppUtil.h>
#include <UCLIDException.h>
#include <StringTokenizer.h>
#include <ValueRestorer.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static const string gstrSERVICE_NAME				= "Extract Systems ID Shield for Laserfiche";
static const int gnSERVICE_ERROR					= 0;
static const int gnSTATUS_TIMER						= 1;
static const int gnTIMER_INTERVAL					= 1000;

//--------------------------------------------------------------------------------------------------
// CServiceSettingsDlg
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CServiceSettingsDlg, CDialog)

CServiceSettingsDlg::CServiceSettingsDlg(CIDShieldLF *pIDShieldLF, CWnd* pParent /*=NULL*/)
	: CDialog(CServiceSettingsDlg::IDD, pParent)
	, CIDShieldLFHelper(pIDShieldLF)
	, m_zRepository(_T(""))
	, m_zUser(_T(""))
	, m_zPassword(_T(""))
	, m_zStatus(_T(""))
	, m_hServiceManager(NULL)
	, m_hService(NULL)
	, m_bAutoStart(FALSE)
	, m_zThreads(_T(""))
	, m_dwServiceStatus(gnSERVICE_ERROR)
{
	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20955");
}
//--------------------------------------------------------------------------------------------------
CServiceSettingsDlg::~CServiceSettingsDlg()
{
	try
	{
		if (m_hService)
		{
			CloseServiceHandle(m_hService);
		}
		if (m_hServiceManager)
		{
			CloseServiceHandle(m_hServiceManager);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20949");
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CServiceSettingsDlg, CDialog)
	ON_WM_TIMER()
	ON_BN_CLICKED(IDC_BTN_START, &CServiceSettingsDlg::OnBnClickedBtnStart)
	ON_BN_CLICKED(IDC_BTN_STOP, &CServiceSettingsDlg::OnBnClickedBtnStop)
	ON_BN_CLICKED(IDC_RADIO_MAX_THREADS, &CServiceSettingsDlg::OnUpdateThreads)
	ON_BN_CLICKED(IDC_RADIO_THREADS, &CServiceSettingsDlg::OnUpdateThreads)
	ON_NOTIFY(UDN_DELTAPOS, IDC_SPIN_THREADS, &CServiceSettingsDlg::OnDeltaposSpinThreads)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CMB_REPOSITORY, m_cmbRepository);
	DDX_Text(pDX, IDC_EDIT_USER, m_zUser);
	DDX_Text(pDX, IDC_EDIT_PASSWORD, m_zPassword);
	DDX_Text(pDX, IDC_EDIT_STATUS, m_zStatus);
	DDX_Check(pDX, IDC_CHK_AUTO_START, m_bAutoStart);
	DDX_Text(pDX, IDC_EDIT_THREADS, m_zThreads);
}
//--------------------------------------------------------------------------------------------------
BOOL CServiceSettingsDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		try
		{
			try
			{
				initServiceHandles();
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21030");
		}
		catch (UCLIDException &ue)
		{
			// If there was an error initializing the service handles, display the error and exit.
			ue.display();
			PostMessage(WM_CLOSE);
			return TRUE;
		}

		m_pIDShieldLF->m_apDlgWait.reset(new CWaitDlg("Searching for Laserfiche repositories..."));
		ASSERT_RESOURCE_ALLOCATION("ELI21868", m_pIDShieldLF->m_apDlgWait.get() != NULL);

		// Retrieve the available Laserfiche repositories
		getAvailableRepositories(m_vecRepositoryList, m_mapServers);

		m_pIDShieldLF->m_apDlgWait->hide();

		// Retrieve any connection settings cached to the registry
		string strServer = m_pIDShieldLF->m_apLocalMachineRegSettings->getKeyValue(gstrREG_SERVICE_KEY, gstrREG_SERVER, "");
		string strRepository = m_pIDShieldLF->m_apLocalMachineRegSettings->getKeyValue(gstrREG_SERVICE_KEY, gstrREG_REPOSITORY, "");
		string strUser = m_pIDShieldLF->m_apLocalMachineRegSettings->getKeyValue(gstrREG_SERVICE_KEY, gstrREG_USER, "");
		string strPassword = m_pIDShieldLF->m_apLocalMachineRegSettings->getKeyValue(gstrREG_SERVICE_KEY, gstrREG_PASSWORD);
		if (!strPassword.empty())
		{
			m_pIDShieldLF->decryptPassword(strPassword);
		}
		string strThreads = m_pIDShieldLF->m_apLocalMachineRegSettings->getKeyValue(gstrREG_SERVICE_KEY, gstrREG_THREAD_COUNT, "1");

		// Display the loaded settings
		setUIValues(strServer, strRepository, strUser, strPassword, strThreads);
		
		updateStatus();

		// Start a timer to check on the status of the service once per second
		if (m_hService != NULL)
		{
			SetTimer(gnSTATUS_TIMER, gnTIMER_INTERVAL, NULL);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20950");

	return TRUE;
}
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::OnOK()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		try
		{
			saveSettings();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20961");
	}
	catch (UCLIDException &ue)
	{
		// Don't call CDialog::OnOK() if we were unable to save successfully
		ue.display();
		return;
	}

	CDialog::OnOK();
}

//--------------------------------------------------------------------------------------------------
// Message Handlers
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::OnTimer(UINT_PTR nIDEvent)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Check on the status of the service and update the UI accordingly
		if (nIDEvent == gnSTATUS_TIMER)
		{
			updateStatus();
		}

		CDialog::OnTimer(nIDEvent);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20965");
}
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::OnBnClickedBtnStart()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Attempt to save current settings
		saveSettings();

		// If settings saved correctly, start the service.
		if (!StartService(m_hService, 0, NULL))
		{
			UCLIDException ue("ELI20967", "Failed to start service!");
			ue.addWin32ErrorInfo();
			ue.display();
		}

		// Update UI to reflect new status
		updateStatus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20966");
}
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::OnBnClickedBtnStop()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		SERVICE_STATUS_PROCESS ssp;
		memset(&ssp, 0, sizeof(SERVICE_STATUS_PROCESS));

		BOOL bStopped = ControlService(m_hService, SERVICE_CONTROL_STOP, (LPSERVICE_STATUS) &ssp);
		if (!asCppBool(bStopped))
		{
			UCLIDException ue("ELI20968", "Failed to stop service!");
			ue.addWin32ErrorInfo();
			ue.display();
		}

		// Update UI to reflect new status
		updateStatus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20969");
}
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::OnUpdateThreads()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// If the radio button selection concerning number of threads has been changed,
		// enable or disable the thread count controls as necessary.
		bool bSpecifyThreads = asCppBool(((CButton *)GetDlgItem(IDC_RADIO_THREADS))->GetCheck());

		GetDlgItem(IDC_EDIT_THREADS)->EnableWindow(asMFCBool(bSpecifyThreads));
		GetDlgItem(IDC_SPIN_THREADS)->EnableWindow(asMFCBool(bSpecifyThreads));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20970");
}
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::OnDeltaposSpinThreads(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UpdateData(TRUE);

		LPNMUPDOWN pNMUpDown = reinterpret_cast<LPNMUPDOWN>(pNMHDR);
	
		// Modify the thread count appropriately
		int nThreads = asLong(m_zThreads.GetString());
		nThreads -= pNMUpDown->iDelta;

		if (nThreads > 0)
		{
			// Only update the new value back to the screen if it is > 0
			m_zThreads.Format("%i", nThreads);
			UpdateData(FALSE);
		}

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20975");
}

//--------------------------------------------------------------------------------------------------
// Private members
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::initServiceHandles()
{
	// Open a handle to the service manager
	m_hServiceManager = OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
	if (m_hServiceManager == NULL)
	{
		UCLIDException("ELI20952", "Unable to access Windows Services!");
	}

	// Open a handle to our service
	m_hService = OpenService(m_hServiceManager, gstrSERVICE_NAME.c_str(), SERVICE_ALL_ACCESS); 
	if (m_hService == NULL)
	{
		string strException = "Error accessing \"" + gstrSERVICE_NAME + "\" service!\r\n\r\n" +
			"Please ensure the service has been installed on this machine.";

		throw UCLIDException("ELI20953", strException);
	}

	// Initialize the service status
	getServiceStatus(true);
}
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::updateStatus()
{
	// If an exception is displayed in updateStatus, execution may be on hold when another
	// updateStatus call comes in.  Don't do any further updateStatus checks until the 
	// initial call returns.
	static volatile bool bUpdatingStatus = false;
	if (bUpdatingStatus == false)
	{
		bUpdatingStatus = true;
		ValueRestorer<volatile bool> restorer(bUpdatingStatus, false);

		m_dwServiceStatus = getServiceStatus();

		// Set the service status message
		switch (m_dwServiceStatus)
		{
			case gnSERVICE_ERROR:			m_zStatus = "Error!";		break;
			case SERVICE_STOPPED:			m_zStatus = "Stopped";		break;
			case SERVICE_START_PENDING:		m_zStatus = "Starting...";	break;
			case SERVICE_STOP_PENDING:		m_zStatus = "Stopping...";	break;
			case SERVICE_RUNNING:			m_zStatus = "Running";		break;
			case SERVICE_CONTINUE_PENDING:  m_zStatus = "Pending";		break;
			case SERVICE_PAUSE_PENDING:		m_zStatus = "Pausing...";	break;
			case SERVICE_PAUSED:			m_zStatus = "Paused";		break;
		}

		// Enable/disable controls depending on the status of the service.
		if (m_dwServiceStatus == gnSERVICE_ERROR ||
			m_dwServiceStatus == SERVICE_STOPPED)
		{
			GetDlgItem(IDC_CMB_REPOSITORY)->EnableWindow(TRUE);
			GetDlgItem(IDC_EDIT_USER)->EnableWindow(TRUE);
			GetDlgItem(IDC_EDIT_PASSWORD)->EnableWindow(TRUE);
			GetDlgItem(IDC_RADIO_MAX_THREADS)->EnableWindow(TRUE);
			GetDlgItem(IDC_RADIO_THREADS)->EnableWindow(TRUE);
			bool bSpecifyThreads = asCppBool(((CButton *)GetDlgItem(IDC_RADIO_THREADS))->GetCheck());
			GetDlgItem(IDC_EDIT_THREADS)->EnableWindow(asMFCBool(bSpecifyThreads));
			GetDlgItem(IDC_SPIN_THREADS)->EnableWindow(asMFCBool(bSpecifyThreads));
			GetDlgItem(IDC_BTN_START)->EnableWindow(m_dwServiceStatus == SERVICE_STOPPED);
			GetDlgItem(IDC_BTN_STOP)->EnableWindow(FALSE);
		}
		else // SERVICE_RUNNING and all non-stopped statuses
		{
			GetDlgItem(IDC_CMB_REPOSITORY)->EnableWindow(FALSE);
			GetDlgItem(IDC_EDIT_USER)->EnableWindow(FALSE);
			GetDlgItem(IDC_EDIT_PASSWORD)->EnableWindow(FALSE);
			GetDlgItem(IDC_RADIO_MAX_THREADS)->EnableWindow(FALSE);
			GetDlgItem(IDC_RADIO_THREADS)->EnableWindow(FALSE);
			GetDlgItem(IDC_EDIT_THREADS)->EnableWindow(FALSE);
			GetDlgItem(IDC_BTN_START)->EnableWindow(FALSE);
			GetDlgItem(IDC_BTN_STOP)->EnableWindow(m_dwServiceStatus == SERVICE_RUNNING);
		}
	}

	// Call SetWindowText for the status field instead of UpdateData(FALSE) so changes to other UI
	// controls since the last update aren't discarded
	GetDlgItem(IDC_EDIT_STATUS)->SetWindowText(m_zStatus);
}
//--------------------------------------------------------------------------------------------------
DWORD CServiceSettingsDlg::getServiceStatus(bool bThrowOnError/* = false*/)
{
	LPSERVICE_STATUS_PROCESS pStatus = NULL;
	try
	{
		DWORD dwBytesNeeded = 0;
		QueryServiceStatusEx(m_hService, SC_STATUS_PROCESS_INFO, NULL, 0, &dwBytesNeeded);
		if (GetLastError() != ERROR_INSUFFICIENT_BUFFER)
		{
			if (bThrowOnError)
			{
				string strException = "Error accessing \"" + gstrSERVICE_NAME + "\" service!\r\n\r\n" +
					"Please ensure the service has been installed on this machine.";

				UCLIDException ue("ELI21042", strException);
				ue.addWin32ErrorInfo();
				throw ue;
			}
			
			return gnSERVICE_ERROR;
		}

		pStatus = (LPSERVICE_STATUS_PROCESS)LocalAlloc(LPTR, dwBytesNeeded); 
		ASSERT_RESOURCE_ALLOCATION("ELI20974", pStatus != NULL);
		memset(pStatus, 0, dwBytesNeeded);

		if (QueryServiceStatusEx(m_hService, SC_STATUS_PROCESS_INFO, (LPBYTE)pStatus, 
				dwBytesNeeded, &dwBytesNeeded))
		{
			static DWORD dwLastStatus = SERVICE_STOPPED;
			DWORD dwStatus = pStatus->dwCurrentState;

			// If the service was not stopped on the last getServiceStatus call, but it is now,
			// report an error if the error code is set to ERROR_EXCEPTION_IN_SERVICE.
			if (dwLastStatus != SERVICE_STOPPED && pStatus->dwWin32ExitCode == ERROR_EXCEPTION_IN_SERVICE)
			{
				UCLIDException ue("ELI21646", "The service failed!");
				ue.display();
			}

			dwLastStatus = dwStatus;

			LocalFree(pStatus);
			return dwStatus;
		}
		else
		{
			LocalFree(pStatus);

			if (bThrowOnError)
			{
				string strException = "Error accessing \"" + gstrSERVICE_NAME + "\" service!\r\n\r\n" +
					"Please ensure the service has been installed on this machine.";

				UCLIDException ue("ELI20954", strException);
				ue.addWin32ErrorInfo();
				throw ue;
			}

			return gnSERVICE_ERROR;
		}
	}
	catch (...)
	{
		if (pStatus != NULL)
		{
			LocalFree(pStatus);
		}

		throw;
	}
}
//--------------------------------------------------------------------------------------------------
bool CServiceSettingsDlg::isAutoStart()
{
	// Declare outside try scope so that it can be freed in case of an exception
	LPQUERY_SERVICE_CONFIG pServiceConfig = NULL;
	
	try
	{
		DWORD dwBytesNeeded = 0;
		QueryServiceConfig(m_hService, NULL, 0, &dwBytesNeeded);
		if (GetLastError() != ERROR_INSUFFICIENT_BUFFER)
		{
			// If we failed to query the configuration, don't throw an exception.
			// Just consider this as not configured for auto-start
			return false;
		}

		pServiceConfig = (LPQUERY_SERVICE_CONFIG)LocalAlloc(LPTR, dwBytesNeeded); 
		ASSERT_RESOURCE_ALLOCATION("ELI21716", pServiceConfig != NULL);
		memset(pServiceConfig, 0, dwBytesNeeded);
		
		if (QueryServiceConfig(m_hService, pServiceConfig, dwBytesNeeded, &dwBytesNeeded))
		{
			// If we successfully queried the configuration, return true if the start type
			// is automatic
			bool bIsAutoStart = (pServiceConfig->dwStartType == SERVICE_AUTO_START);
			LocalFree(pServiceConfig);
			return bIsAutoStart;
		}
		else
		{
			// If we failed to query the configuration, don't throw an exception.
			// Just consider this as not configured for auto-start
			return false;
		}
	}
	catch (...)
	{
		if (pServiceConfig != NULL)
		{
			LocalFree(pServiceConfig);
		}

		throw;
	}
}
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::setAutoStart(bool bEnable)
{
	if (bEnable != isAutoStart())
	{
		if (ChangeServiceConfig(m_hService, SERVICE_NO_CHANGE, 
				bEnable ? SERVICE_AUTO_START : SERVICE_DEMAND_START, 
				SERVICE_NO_CHANGE,
				NULL, NULL, NULL, NULL, NULL, NULL, NULL) == FALSE)
		{
			UCLIDException ue("ELI20973", "Failed to change service start type!");
			ue.addWin32ErrorInfo();
			throw ue;
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::setUIValues(const string &strServer, const string &strRepository, 
						const string &strUser, const string &strPassword, const string &strThreads)
{
	// If a default server name is specified, check to see if the server name matches up with 
	// that of the corresponding available repository
	string strRepositoryDisplay = strRepository; 
	if (!strServer.empty())
	{
		if (m_mapServers[strRepository] != strServer)
		{
			// If the server name doesn't match up with an available repository, specify the
			// full path to the login dialog.
			strRepositoryDisplay = strServer + "/" + strRepository;
		}
	}

	// Set the UI control value data.
	m_zRepository = strRepositoryDisplay.c_str();
	m_zUser = strUser.c_str();
	m_zPassword = strPassword.c_str();

	// Initialize the repository list
	m_cmbRepository.Clear();
	for each (string strRepositoryEntry in m_vecRepositoryList)
	{
		m_cmbRepository.AddString(strRepositoryEntry.c_str());
	}

	// Add and select the currently configured repository if necessary
	if (m_zRepository.IsEmpty() == false)
	{
		int nSelRepository = m_cmbRepository.FindStringExact(-1, m_zRepository);

		if (nSelRepository == CB_ERR)
		{
			nSelRepository = m_cmbRepository.AddString(m_zRepository);
		}

		m_cmbRepository.SetCurSel(nSelRepository);
	}
	else if (m_cmbRepository.GetCount() > 0 )
	{
		m_cmbRepository.SetCurSel(0);
	}

	int nThreadCount = 0;
	try
	{
		nThreadCount = asLong(strThreads);
	}
	catch (...)
	{
		// If strThreads cannot be converted to a number, don't pass on an exception, just
		// leave the thread count 0;
	}

	if (nThreadCount <= 0)
	{
		// Optimal number of threads
		((CButton *)GetDlgItem(IDC_RADIO_MAX_THREADS))->SetCheck(BST_CHECKED);
		((CButton *)GetDlgItem(IDC_RADIO_THREADS))->SetCheck(BST_UNCHECKED);
		// If a specific number of threads has not been specified, initialize
		// the manual thread count box with 1 in case they decide to use it.
		m_zThreads = "1";
	}
	else
	{
		// User specified number of threads
		((CButton *)GetDlgItem(IDC_RADIO_MAX_THREADS))->SetCheck(BST_UNCHECKED);
		((CButton *)GetDlgItem(IDC_RADIO_THREADS))->SetCheck(BST_CHECKED);
		m_zThreads = strThreads.c_str();
	}

	m_bAutoStart = asMFCBool(isAutoStart());

	UpdateData(FALSE);
}
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::getUIValues(string &rstrServer, string &rstrRepository, 
								      string &rstrUser, string &rstrPassword, string &rstrThreads)
{
	UpdateData(TRUE);

	m_cmbRepository.GetWindowText(m_zRepository);

	rstrRepository = m_zRepository.GetString();
	rstrUser = m_zUser.GetString();
	rstrPassword = m_zPassword.GetString();

	// Calculate the server name based on the rstrRepository value.
	rstrServer = "";
	replace(rstrRepository.begin(), rstrRepository.end(), '\\', '/');
	vector<string> vecTokens;

	StringTokenizer::sGetTokens(rstrRepository, '/', vecTokens);

	if (vecTokens.size() == 1)
	{
		// No server specified, obtain it from m_mapServers
		rstrServer = m_mapServers[rstrRepository];
	}
	else if (vecTokens.size() == 2)
	{
		rstrServer = vecTokens[0];
		rstrRepository = vecTokens[1];
	}
	else
	{
		UCLIDException ue("ELI20947", "Invalid repository name!");
		ue.addDebugInfo("Repository", rstrRepository);
		throw ue;
	}

	if (asCppBool(((CButton *)GetDlgItem(IDC_RADIO_MAX_THREADS))->GetCheck()))
	{
		rstrThreads = "0";
	}
	else
	{
		rstrThreads = m_zThreads.GetString();
	}
}
//--------------------------------------------------------------------------------------------------
void CServiceSettingsDlg::saveSettings()
{
	try
	{
		try
		{
			string strServer, strRepository, strUser, strPassword, strThreads;
			getUIValues(strServer, strRepository, strUser, strPassword, strThreads);

			// Log in and out of the repository to test the login settings.
			ILFConnectionPtr ipConnection = connectToRepository(strServer, strRepository, strUser, strPassword);
			ASSERT_RESOURCE_ALLOCATION("ELI20960", ipConnection != NULL);

			ipConnection->Terminate();

			// If the login was successful, store the settings.
			m_pIDShieldLF->m_apLocalMachineRegSettings->setKeyValue(
				gstrREG_SERVICE_KEY, gstrREG_SERVER, strServer);
			m_pIDShieldLF->m_apLocalMachineRegSettings->setKeyValue(
				gstrREG_SERVICE_KEY, gstrREG_REPOSITORY, strRepository);
			m_pIDShieldLF->m_apLocalMachineRegSettings->setKeyValue(
				gstrREG_SERVICE_KEY, gstrREG_USER, strUser);
			// Store the password encrypted.
			m_pIDShieldLF->encryptPassword(strPassword);
			m_pIDShieldLF->m_apLocalMachineRegSettings->setKeyValue(
				gstrREG_SERVICE_KEY, gstrREG_PASSWORD, strPassword);
			
			// For now, disable the setting of thread counts from the service console.
			// The initial release of ID Shield for Laserfiche is to be single-threaded.
	//		m_pIDShieldLF->m_apLocalMachineRegSettings->setKeyValue(
	//			gstrREG_SERVICE_KEY, gstrREG_THREAD_COUNT, strThreads);

			setAutoStart(asCppBool(m_bAutoStart));
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20956");
	}
	catch (UCLIDException &ue)
	{
		// Clear the password box if the login failed
		m_zPassword = "";
		UpdateData(FALSE);

		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------