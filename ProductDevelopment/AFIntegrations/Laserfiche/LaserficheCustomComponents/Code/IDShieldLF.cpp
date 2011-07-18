// IDShieldLF.cpp : Implmentation for CIDShieldLF

#include "stdafx.h"
#include "IDShieldLF.h"
#include "SelectRepositoryDlg.h"
#include "RepositorySettingsPP.h"
#include "RedactionSettingsPP.h"
#include "AboutPP.h"
#include "WaitDlg.h"
#include "ProgressDlg.h"
#include "VerifyToolbar.h"
#include "ServiceSettingsDlg.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <RegistryPersistenceMgr.h>
#include <INIFilePersistenceMgr.h>
#include <RegConstants.h>
#include <StringTokenizer.h>
#include <LFMiscUtils.h>
#include <TemporaryFileName.h>
#include <EncryptionEngine.h>
#include <LoginDlg.h>

#include <psapi.h>

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static const string gstrPRODUCT_NAME			= "ID Shield for Laserfiche";

// Registry keys & values
static const string gstrREG_SOFTWARE_ROOT			= "Software";
static const string gstrREG_LASERFICHE_KEY			= "\\Extract Systems\\Laserfiche";
static const string gstrREG_CONSOLE_KEY				= gstrREG_LASERFICHE_KEY + "\\Admin Console";
static const string gstrREG_SERVICE_KEY				= gstrREG_LASERFICHE_KEY + "\\Service";
static const string gstrREG_VERIFY_WINDOW_KEY		= gstrREG_LASERFICHE_KEY + "\\Verification Window";
static const string gstrREG_CLIENT_LOGINS_KEY		= gstrREG_LASERFICHE_KEY + "\\Client Logins";
static const string gstrREG_SERVER					= "Server";
static const string gstrREG_REPOSITORY				= "Repository";
static const string gstrREG_USER					= "User";
static const string gstrREG_PASSWORD				= "Password";
static const string gstrREG_SEARCH_INTERVAL			= "SearchInterval";
static const string gstrREG_THREAD_COUNT			= "Thread Count";
static const string gstrREG_LF7_HIDDEN_PROMPTS		= "\\Laserfiche\\Client\\Profile\\HiddenDialogs";
static const string gstrREG_LF8_HIDDEN_PROMPTS		= "\\Laserfiche\\Client8\\Profile\\HiddenDialogs";
static const string gstrREG_LF_SAVEDOC_PROMPT		= "ConfirmSaveDocument";
static const string gstrREG_VERIFY_WINDOW_LEFT		= "Left";
static const string gstrREG_VERIFY_WINDOW_TOP		= "Top";
static const string gstrREG_MAX_DOCS_TO_PROCESS		= "MaxDocsToProcess";

// ID Shield tag names
static const string gstrTAG_SETTINGS				= "ID Shield - Settings";
static const string	gstrTAG_PENDING_PROCESSING		= "ID Shield - Needs Processing";
static const string	gstrTAG_PROCESSED				= "ID Shield - Processed";
static const string	gstrTAG_PENDING_VERIFICATION	= "ID Shield - Needs Verification";
static const string	gstrTAG_VERIFYING				= "ID Shield - Verifying";
static const string	gstrTAG_VERIFIED				= "ID Shield - Verified";
static const string	gstrTAG_FAILED_PROCESSING		= "ID Shield - Failed Processing";

// IDShieldLF.ini sections & values
static const string gstrSETTINGS_FILE				= "IDShieldLF.ini";
static const string gstrINI_SECTION_REDACTION		= "Redaction";
static const string gstrINI_SECTION_VERIFICATION	= "Verification";
static const string gstrINI_SEARCH_SECTION			= "Search";
static const string gstrINI_MASTERRSD				= "MasterRuleset";
static const string gstrINI_HCDATA					= "RedactHighConfidenceData";
static const string gstrINI_MCDATA					= "RedactMediumConfidenceData";
static const string gstrINI_LCDATA					= "RedactLowConfidenceData";
static const string gstrINI_AUTO_TAG				= "AutoTag";
static const string gstrINI_TAG_ALL					= "TagAll";
static const string gstrINI_ON_DEMAND				= "OnDemand";
static const string gstrINI_ENSURE_TEXT_REDACTIONS	= "EnsureTextRedactions";
static const string gstrINI_DOCUMENT_SEARCH_TYPE	= "DocumentSearchType"; 
static const string gstrINI_FOLDER_SEARCH_TYPE		= "FolderSearchType";
static const string gstrINI_ALL_SEARCH_TYPE			= "AllSearchType";

// Attribute types
static const _bstr_t gbstrHCDATA					= "HCData"; 
static const _bstr_t gbstrMCDATA					= "MCData";
static const _bstr_t gbstrLCDATA					= "LCData";
static const _bstr_t gbstrCLUES						= "Clues";

static const int gnCONNECTION_PROMPT_RETRIES		= 3;

// Progress status step counts
static const int gnREDACTION_PREP_STEPS			= 1;
static const int gnREDACTION_EXEC_STEPS			= 6;
static const int gnREDACTION_APPLY_STEPS		= 3;
static const int gnREDACTION_TOTAL_STEPS		= gnREDACTION_PREP_STEPS +
												  gnREDACTION_EXEC_STEPS +
												  gnREDACTION_APPLY_STEPS;

// Color for clue highlights
static const OLE_COLOR gcolorCLUE_HIGHLIGHT		= 0x0001FFFE;

static const int gnMAX_BACKGROUND_SEARCH_INTERVAL_DEFAULT	= 60 * CLOCKS_PER_SEC; // 1 minute
static const int gnMAX_DOCS_TO_PROCESS_DEFAULT				=	50000;

static const string gstrDEFAULT_DOCUMENT_SEARCH_TYPE	= "BD";
static const string gstrDEFAULT_FOLDER_SEARCH_TYPE		= "F";
static const string gstrDEFAULT_ALL_SEARCH_TYPE			= "FBD";

// Key for encyrpting/decrypting the Laserfiche password stored in the registry
const unsigned long	gulPasswordKey0 = 0x006999DB;
const unsigned long	gulPasswordKey1 = 0xC6724a97;
const unsigned long	gulPasswordKey2 = 0xB53241C4;
const unsigned long	gulPasswordKey3 = 0x1033F924;

static const string gstrADMIN_MUTEX_NAME		= "ID Shield for Laserfiche Administrator Console";
static const string gstrSERVICE_MUTEX_NAME		= "ID Shield for Laserfiche Service Console";
static const string gstrCLIENT_TASKS_MUTEX_NAME	= "ID Shield for Laserfiche";

//--------------------------------------------------------------------------------------------------
// Statics
//--------------------------------------------------------------------------------------------------

volatile bool CIDShieldLF::m_bStopService = false;
volatile LONG CIDShieldLF::m_nRunningThreads = 0;
long CIDShieldLF::m_nBackgroundSearchInterval = gnMAX_BACKGROUND_SEARCH_INTERVAL_DEFAULT;
Win32Event CIDShieldLF::m_eventWorkerThreadsDone;
Win32Event CIDShieldLF::m_eventServiceStarted;
Win32Event CIDShieldLF::m_eventServiceStopped;
Win32Event CIDShieldLF::m_eventWorkerThreadFailed;
auto_ptr<CLFItemCollection> CIDShieldLF::m_apDocumentsToProcess;
CMutex CIDShieldLF::m_mutexLFOperations;

//--------------------------------------------------------------------------------------------------
// CIDShieldLF
//--------------------------------------------------------------------------------------------------
CIDShieldLF::CIDShieldLF()
	: m_nHelperReferenceCount(0) 
	, m_bAttachedToClient(false)
	, m_eConnectionMode(kDisconnected)
	, m_apDlgWait(new CWaitDlg())
	, m_ipClient(NULL)
	, m_ipConnection(NULL)
	, m_ipDatabase(NULL)
	, m_ipSettingsFolder(NULL)
	, m_ipSettingsFile(NULL)
	, m_apLocalSettingsFile(NULL)
	, m_apSettingsMgr(NULL)
	, m_apPropertySheet(NULL)
	, m_hwndClient(NULL)
	, m_appageRepository(NULL)
	, m_appageRedaction(NULL)
	, m_appageAbout(NULL)
	, m_nMaxDocsToProcess(gnMAX_DOCS_TO_PROCESS_DEFAULT)
{
	try
	{
		// Initialize an object to handle HKEY_CURRENT_USER settings
		m_apCurrentUserRegSettings = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER, gstrREG_SOFTWARE_ROOT));
		ASSERT_RESOURCE_ALLOCATION("ELI20722", m_apCurrentUserRegSettings.get() != NULL);

		// Initialize an object to handle HKEY_LOCAL_MACHINE settings
		m_apLocalMachineRegSettings = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_LOCAL_MACHINE, gstrREG_SOFTWARE_ROOT));
		ASSERT_RESOURCE_ALLOCATION("ELI20945", m_apLocalMachineRegSettings.get() != NULL);

		// Load the max documents that are processable at one time via a Client session.
		string strMaxDocsToProcess = 
			m_apLocalMachineRegSettings->getKeyValue(gstrREG_LASERFICHE_KEY,
			gstrREG_MAX_DOCS_TO_PROCESS, gnMAX_DOCS_TO_PROCESS_DEFAULT);
		m_nMaxDocsToProcess = asLong(strMaxDocsToProcess);

		// Initialize deafult search types
		m_RepositorySettings.strDocumentSearchType = gstrDEFAULT_DOCUMENT_SEARCH_TYPE;
		m_RepositorySettings.strFolderSearchType = gstrDEFAULT_FOLDER_SEARCH_TYPE;
		m_RepositorySettings.strAllSearchType = gstrDEFAULT_ALL_SEARCH_TYPE;

		// Until a helper is created, keep the helper event flagged as "done".
		m_eventHelpersDone.signal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20721");
}
//--------------------------------------------------------------------------------------------------
CIDShieldLF::~CIDShieldLF()
{
	try
	{
		// Ensure any helper classes that depend on this instance end before this instance is allowed
		// to end.
		m_eventHelpersDone.wait();

		// Close the connection and release resources.
		disconnect();

		// Dispose of the wait dialog
		m_apDlgWait.reset();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20715");
}
//--------------------------------------------------------------------------------------------------
HRESULT CIDShieldLF::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IIDShieldLF
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::ConnectPrompt(EConnectionMode eConnectionMode, VARIANT_BOOL *pbSuccess)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20733", pbSuccess != NULL);

		validateLicense(eConnectionMode);

		// default to false
		*pbSuccess = VARIANT_FALSE;

		// Allow only one client task or admin console instance to run at a time.
		string strMutex = (eConnectionMode == kAdministrator) ? gstrADMIN_MUTEX_NAME 
															  : gstrCLIENT_TASKS_MUTEX_NAME;
		CMutex namedMutex(TRUE, strMutex.c_str());
		if (GetLastError() == ERROR_ALREADY_EXISTS)
		{
			UCLIDException ue("ELI21358", gstrPRODUCT_NAME + " is already running!");
			ue.addDebugInfo("Application", strMutex);
			throw ue;
		}

		// Retrieve any connection settings cached to the registry
		string strServer = m_apCurrentUserRegSettings->getKeyValue(gstrREG_CONSOLE_KEY, gstrREG_SERVER, "");
		string strRepository = m_apCurrentUserRegSettings->getKeyValue(gstrREG_CONSOLE_KEY, gstrREG_REPOSITORY, "");
		string strUser = m_apCurrentUserRegSettings->getKeyValue(gstrREG_CONSOLE_KEY, gstrREG_USER);

		// Initialize a wait dialog
		m_apDlgWait.reset(new CWaitDlg(NULL));
		ASSERT_RESOURCE_ALLOCATION("ELI21867", m_apDlgWait.get() != NULL);
		m_apDlgWait->showMessage("Searching for Laserfiche repositories...");

		// Retrieve the available Laserfiche repositories
		vector<string> vecRepositories;
		map<string, string> mapPaths;
		getAvailableRepositories(vecRepositories, mapPaths);

		INT_PTR nStatus = IDOK;

		// This loop prompts for login information and attempts to login using the information provided.
		// If the login is unsuccessful, the loop will repeat by redisplaying the login box until
		// gnCONNECTION_PROMPT_RETRIES is reached.
		for (int nTry = 0; 
			nStatus == IDOK && m_ipConnection == NULL && nTry < gnCONNECTION_PROMPT_RETRIES;
			nTry++)
		{
			try
			{
				// Reset the password box on each attempt
				strPassword.clear();
				// Hide the wait dialog if it is showing.  Do not close it as that may 
				// relinquish the foreground status to another process preventing the login
				// screen from appearing on top.
				m_apDlgWait->hide();

				// Create a login prompt and populate the list of available repositories
				CSelectRepositoryDlg dlgLogin;
				dlgLogin.SetRepositoryList(vecRepositories, mapPaths);

				// Prompt the user
				nStatus = dlgLogin.GetLoginInfo(strServer, strRepository, strUser, strPassword);
				if (nStatus == IDOK)
				{
					m_apDlgWait->showMessage("Logging in...");

					// If the user requests a login, attempt to login
					m_ipConnection = connectToRepository(strServer, strRepository, strUser, strPassword);

					// If no exceptions were thrown, cache the login information so it comes up
					// automatically next time.
					m_apCurrentUserRegSettings->setKeyValue(gstrREG_CONSOLE_KEY, gstrREG_SERVER, strServer);
					m_apCurrentUserRegSettings->setKeyValue(gstrREG_CONSOLE_KEY, gstrREG_REPOSITORY, strRepository);
					m_apCurrentUserRegSettings->setKeyValue(gstrREG_CONSOLE_KEY, gstrREG_USER, strUser);
				}
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20778");
		}

		if (nStatus == IDOK && m_ipConnection != NULL)
		{
			// If a connection was established, validate it. 
			// (validateConnection will close m_apDlgWait in case of an exception)
			validateConnection(eConnectionMode);

			*pbSuccess = VARIANT_TRUE;
		}

		m_apDlgWait->hide();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20730");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::ConnectToRepository(BSTR bstrServer, BSTR bstrRepository, BSTR bstrUser, 
			BSTR bstrPassword, EConnectionMode eConnectionMode)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense(eConnectionMode);

		// Allow only one client task or admin console instance to run at a time.
		string strMutex = (eConnectionMode == kAdministrator) ? gstrADMIN_MUTEX_NAME 
															  : gstrCLIENT_TASKS_MUTEX_NAME;
		CMutex namedMutex(TRUE, strMutex.c_str());
		if (GetLastError() == ERROR_ALREADY_EXISTS)
		{
			UCLIDException ue("ELI21359", gstrPRODUCT_NAME + " is already running!");
			ue.addDebugInfo("Application", strMutex);
			throw ue;
		}

		// Attempt to login
		m_ipConnection = connectToRepository(asString(bstrServer), asString(bstrRepository),
											 asString(bstrUser), asString(bstrPassword));

		if (m_ipConnection)
		{
			// If a connection was established, validate it.
			validateConnection(eConnectionMode);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20848");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::ConnectToActiveClient(EConnectionMode eConnectionMode,
												VARIANT_BOOL *pbSuccess)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI21898", pbSuccess != NULL);
		// default return value to VARIANT_FALSE
		*pbSuccess = VARIANT_FALSE;

		validateLicense(eConnectionMode);

		// Allow only one client task instance to run at a time.
		CMutex namedMutex(TRUE, gstrCLIENT_TASKS_MUTEX_NAME.c_str());
		if (GetLastError() == ERROR_ALREADY_EXISTS)
		{
			UCLIDException ue("ELI21356", gstrPRODUCT_NAME + " is already running!");
			ue.addDebugInfo("Application", gstrCLIENT_TASKS_MUTEX_NAME);
			throw ue;
		}

		// Find the client's window handle
		m_hwndClient = FindWindow(gzLFCLIENT_CLASS, NULL);
		ASSERT_RESOURCE_ALLOCATION("ELI20866", m_hwndClient != NULL);

		m_apDlgWait.reset(new CWaitDlg(CWnd::FromHandle(m_hwndClient)));
		ASSERT_RESOURCE_ALLOCATION("ELI21859", m_apDlgWait.get() != NULL);

		m_apDlgWait->showMessage("Logging in...");

		// Attempt to attach to an active instance of the client.
		m_ipClient.GetActiveObject(CLSID_Document);
		if (m_ipClient == NULL)
		{
			throw UCLIDException("ELI20587", 
				"No running instance of the Laserfiche Client could be found!");
		}

		// [FlexIDSIntegrations:2] Make sure we can't find another instance of the client
		if (FindWindowEx(NULL, m_hwndClient, gzLFCLIENT_CLASS, NULL) != NULL)
		{
			throw UCLIDException("ELI21071", "Multiple Laserfiche Clients are open! Please close "
				"extra instances of the Laserfiche Client prior to using ID Shield.");
		}

		// To find the version of the client we are connecting to, start by getting the process id
		DWORD dwProcessID;
		if (GetWindowThreadProcessId(m_hwndClient, &dwProcessID) == 0)
		{
			throw UCLIDException("ELI21889", "Failure accessing Laserfiche Client!");
		}

		// Use the process ID to open a handle to the process
		HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, dwProcessID);
        ASSERT_RESOURCE_ALLOCATION("ELI21888", hProcess != 0);

		// Use the process handle to retrieve the file name.
		char pszClientFilename[MAX_PATH];
		if (GetModuleFileNameEx(hProcess, NULL, pszClientFilename, MAX_PATH) == 0)
		{
			CloseHandle(hProcess);
			throw UCLIDException("ELI21887", "Failure accessing Laserfiche Client!");
		}
		CloseHandle(hProcess);
		
		// Finally, use the process file name to retrieve the client version.
		m_strClientVersion = getFileVersion(pszClientFilename);

		UCLIDException ueTrace("ELI21890", "Application trace: Connecting to Laserfiche Client ver. " +
			m_strClientVersion);
		ueTrace.log();

		try
		{
			try
			{
				// Retrieve the Client's open repository
				m_ipDatabase = m_ipClient->GetDatabase();

				if (m_ipDatabase != NULL)
				{
					// If we successfully got the Client's open database, 
					// retrieve the Client's open connection.
					m_ipConnection = m_ipDatabase->CurrentConnection;

					m_bAttachedToClient = true;
				}
				// [FlexIDSIntegrations:103]
				// If we are attempting connect to a Laserfiche 8 Client, we will need to establish an independent
				// connection since the ILFClient interface is unable to provide its existing connection to us
				// as an LFSO72 interface instance.
				else if (m_strClientVersion.substr(0, 1) == "8")
				{
					string strServer;
					string strRepository;
					string strUser;

					// [FlexIDSIntegrations:216]
					// Retrieve information about the client connection using the LFSO namespace
					// that corresponds with the client version.
					bool bGotRepositoryInfo = false;
					if (m_strClientVersion.substr(2, 1) == "0")
					{
						bGotRepositoryInfo = GetLoginInfoFrom80(strServer, strRepository, strUser);
					}
					else if (m_strClientVersion.substr(2, 1) == "1")
					{
						bGotRepositoryInfo = GetLoginInfoFrom81(strServer, strRepository, strUser);
					}
					
					// To increase chances of being able to work with future versions of Laserfiche
					// without modifying code, allow default LF login info to be specified in the
					// registry if we were unable to obtain it programatically.
					if (!bGotRepositoryInfo)
					{
						string strDefaultLogin = 
							m_apCurrentUserRegSettings->getKeyValue(gstrREG_CLIENT_LOGINS_KEY, "", "");

						if (!strDefaultLogin.empty())
						{
							vector<string> vecTokens;
							StringTokenizer st('/');
							st.parse(strDefaultLogin, vecTokens);

							if (vecTokens.size() == 3)
							{
								strServer = vecTokens[0];
								strRepository = vecTokens[1];
								strUser = vecTokens[2];
								bGotRepositoryInfo = true;
							}
						}
					}

					if (!bGotRepositoryInfo)
					{
						throw UCLIDException("ELI21891", "Please be sure you have selected a specific "
							"repository or element of a repository before starting an ID Shield operation!");
					}
					
					string strPassword = "";
					string strRegKey = strServer + "/" + strRepository + "/" + strUser;
						
					// Check to see if we have stored a password for this server/repository/user
					if (m_apCurrentUserRegSettings->keyExists(gstrREG_CLIENT_LOGINS_KEY, strRegKey))
					{
						strPassword = m_apCurrentUserRegSettings->getKeyValue(
							gstrREG_CLIENT_LOGINS_KEY, strRegKey, "");
						decryptPassword(strPassword);

						try
						{
							// If we stored a password, attempt to login.
							m_ipConnection = connectToRepository(
								strServer, strRepository, strUser, strPassword);
						}
						CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21899")
					}

					// If we don't have a stored password, or we failed to login with the password,
					// prompt the user for the password. (make up to gnCONNECTION_PROMPT_RETRIES attempts)
					for (int nTry = 0; 
						 m_ipConnection == NULL && nTry < gnCONNECTION_PROMPT_RETRIES;
						 nTry++)
					{
						CLoginDlg dlgLogin("Enter your Laserfiche password", strUser, true, 
							CWnd::FromHandle(m_hwndClient));

						// Hide the "Logging in..." message while we prompt for the user's 
						// password.
						m_apDlgWait->hide();

						if (dlgLogin.DoModal() == IDOK)
						{
							m_apDlgWait->show();

							strPassword = dlgLogin.m_zPassword.GetString();
						}
						else
						{
							return S_OK;
						}

						try
						{
							// Attempt a login with the entered password
							m_ipConnection = connectToRepository(
								strServer, strRepository, strUser, strPassword);

							encryptPassword(strPassword);
							m_apCurrentUserRegSettings->setKeyValue(
								gstrREG_CLIENT_LOGINS_KEY, strRegKey, strPassword);
						}
						CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21897");

						m_ipDatabase = m_ipConnection->Database;
					}
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21892");
		}
		catch (UCLIDException &ue)
		{
			if (m_apDlgWait.get() != NULL)
			{
				m_apDlgWait->hide();
			}

			UCLIDException uexOuter("ELI21893", "Unable to access Laserfiche repository!", ue);
			throw uexOuter;
		}

		if (m_ipConnection == NULL)
		{
			if (m_apDlgWait.get() != NULL)
			{
				m_apDlgWait->hide();
			}

			throw UCLIDException("ELI20589", "Unable to connect to Laserfiche repository!");
		}

		// Validates that the existing connection has all privileges required by eConnectionMode
		validateConnection(eConnectionMode);

		// Sending the Laserfiche Client to the back and back to the front appears to solve some
		// cases in which getSelectedDocuments fails to retrieve the selected documents.
		::SetWindowPos(m_hwndClient, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE|SWP_NOSIZE);
		::SetWindowPos(m_hwndClient, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE|SWP_NOSIZE);

		m_apDlgWait->hide();
		
		// CATCH_ALL_AND_RETURN_AS_COM_ERROR returns S_FALSE in the
		// case that no exception is caught.  Set return value and return here.
		*pbSuccess = VARIANT_TRUE;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20849")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::Disconnect(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		disconnect();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20843");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::ShowAdminConsole(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		try
		{
			validateLicense(kAdministrator);

			// [FlexIDSIntegrations: 42]
			// Allow only one Admin Console instance to be displayed at a time.
			CMutex namedMutex(TRUE, gstrADMIN_MUTEX_NAME.c_str());
			if (GetLastError() == ERROR_ALREADY_EXISTS)
			{
				UCLIDException ue("ELI21357", gstrPRODUCT_NAME + " is already running!");
				ue.addDebugInfo("Application", gstrADMIN_MUTEX_NAME);
				throw ue;
			}

			// Create the property pages
			m_appageRepository.reset(new CRepositorySettingsPP(this));
			ASSERT_RESOURCE_ALLOCATION("ELI21860", m_appageRepository.get() != NULL);

			m_appageRedaction.reset(new CRedactionSettingsPP(this));
			ASSERT_RESOURCE_ALLOCATION("ELI21861", m_appageRedaction.get() != NULL);

			m_appageAbout.reset(new CAboutPP());
			ASSERT_RESOURCE_ALLOCATION("ELI21863", m_appageAbout.get() != NULL);

			// Create the property sheet
			m_apPropertySheet.reset(new CPropertySheet("ID Shield Administration Console"));
			ASSERT_RESOURCE_ALLOCATION("ELI21864", m_apPropertySheet.get() != NULL);

			// Add the Repository page which will always be present (the redaction page will be added
			// later as needed)
			m_apPropertySheet->AddPage(m_appageRepository.get());
			m_apPropertySheet->AddPage(m_appageAbout.get());

			// Display
			m_apPropertySheet->DoModal();
			
			// Destroy the page and sheet objects
			m_appageRepository.reset();
			m_appageRedaction.reset();
			m_appageAbout.reset();
			m_apPropertySheet.reset();
		}
		catch (...)
		{
			// Ensure we don't leave the IDShieldLF.ini file locked
			safeDispose(m_ipSettingsFile);

			throw;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20714");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::RedactSelected(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense(kProcess);

		processSelected(true);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI21041");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::SubmitSelectedForRedaction(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense(kSubmit);

		processSelected(false);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20911");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::VerifySelected(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense(kVerify);

		// Allow only one client task instance to run at a time.
		CMutex namedMutex(TRUE, gstrCLIENT_TASKS_MUTEX_NAME.c_str());
		if (GetLastError() == ERROR_ALREADY_EXISTS)
		{
			UCLIDException ue("ELI21360", gstrPRODUCT_NAME + " is already running!");
			ue.addDebugInfo("Application", gstrCLIENT_TASKS_MUTEX_NAME);
			throw ue;
		}

		// The wait dialog needs to be closed here to give forground status back to Laserfiche
		// Otherwise, Laserfiche 8 doesn't seem to report selected folders within the
		// findSelectedDocuments call.
		m_apDlgWait->close();

		// Display a WaitDlg while collecting the currently selected documents. 
		m_apDlgWait->showMessage("Collecting Documents...");

		CLFItemCollection documentsToVerify(m_ipClient, m_ipDatabase);
		documentsToVerify.findSelectedDocuments(m_RepositorySettings.strDocumentSearchType,
			gstrTAG_PENDING_VERIFICATION);

		// This is one place where it is better to use close() instead of hide() for the 
		// wait dialog.  If we use close, it keeps ID Shield as the foreground process which
		// is nice if an exception is thrown, but is also allows Laserfiche to fall out
		// of the foreground after the first document is displayed.
		m_apDlgWait->close();

		// [FlexIDSIntegrations:8] Prompt if there were no documents to act on.
		if (documentsToVerify.getCount() == 0)
		{
			MessageBox(m_hwndClient, 
					   "There are no documents in the current selection to verify!",
					   gstrPRODUCT_NAME.c_str(), MB_OK|MB_ICONINFORMATION);
			return S_OK;
		}

		// Begin verifying the active documents.
		verify(documentsToVerify);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20888");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::ShowServiceConsole(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense(kService);

		// Allow only one Admin Console instance to be displayed at a time.
		CMutex namedMutex(TRUE, gstrSERVICE_MUTEX_NAME.c_str());
		if (GetLastError() == ERROR_ALREADY_EXISTS)
		{
			UCLIDException ue("ELI21363", gstrPRODUCT_NAME + " is already running!");
			ue.addDebugInfo("Application", gstrSERVICE_MUTEX_NAME);
			throw ue;
		}

		CServiceSettingsDlg dlgService(this);
		dlgService.DoModal();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20920");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::StartBackgroundProcessing(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Initialize license
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		validateLicense(kService);

		// Set up the exception handling aspect.
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler(&exceptionDlg);

		UCLIDException ueTrace("ELI21750", "Application trace: Starting background service.");
		ueTrace.log();

		// Initialize running thread count and stop flag
		m_bStopService = false;
		m_nRunningThreads = 0;

		// Retrieve the login credentials from the registry
		string strServer = 
			m_apLocalMachineRegSettings->getKeyValue(gstrREG_SERVICE_KEY, gstrREG_SERVER, "");
		string strRepository = 
			m_apLocalMachineRegSettings->getKeyValue(gstrREG_SERVICE_KEY, gstrREG_REPOSITORY, "");
		string strUser = 
			m_apLocalMachineRegSettings->getKeyValue(gstrREG_SERVICE_KEY, gstrREG_USER, "");
		string strPassword = 
			m_apLocalMachineRegSettings->getKeyValue(gstrREG_SERVICE_KEY, gstrREG_PASSWORD, "");
		decryptPassword(strPassword);

		// Attempt and validate a connection to Laserfiche.
		m_ipConnection = connectToRepository(strServer, strRepository, strUser, strPassword);

		validateConnection(kService);

		AfxBeginThread(runBackgroundMasterThread, this);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI21778");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::StopBackgroundProcessing(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		UCLIDException ueTrace("ELI21751", "Application trace: Stopping background service.");
		ueTrace.log();

		// Set the stop flag
		m_bStopService = true;

		// Wait for all worker threads to stop processing
		m_eventServiceStopped.wait();

		// Disconnect from the repository.
		disconnect();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20922");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldLF::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IIDShieldLF
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}

		return S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20716")
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::validateConnection()
{
	try
	{
		try
		{
			ASSERT_RESOURCE_ALLOCATION("ELI20746", m_ipConnection != NULL);

			if (m_ipDatabase == NULL)
			{
				m_ipDatabase = m_ipConnection->Database;
				ASSERT_RESOURCE_ALLOCATION("ELI20757", m_ipDatabase != NULL);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20756");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException uexOuter("ELI20755", "Laserfiche connection error!", ue);
		throw uexOuter;
	}
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::validateConnection(EConnectionMode eConnectionMode)
{
	try
	{
		try
		{
			// Validate the basic connection
			validateConnection();

			// Ensure all necessary rights and privileges are available for this connection mode.
			validateRepositoryPrivileges(eConnectionMode);

			// Load and validate the settings for this repository
			validateRepositorySettings(eConnectionMode);

			m_eConnectionMode = eConnectionMode;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20744");
	}
	catch (UCLIDException &ue)
	{
		try
		{
			// If there was a problem, attempt to close any active connection, but don't throw any
			// exception generated while disconnecting.
			disconnect();
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21017");

		UCLIDException uexOuter("ELI20745", "Failed to validate Laserfiche connection!", ue);
		throw uexOuter;
	}
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::validateRepositoryPrivileges(EConnectionMode eConnectionMode)
{
	try
	{
		try
		{
			validateConnection();

			// Connections are sorted in order of most to least required privileges so that each
			// mode requires the permissions of modes further down the list
			switch (eConnectionMode)
			{
			case kAdministrator:
				if (m_ipConnection->GetHasPrivilege(LFSO72Lib::PRIVILEGE_TEMPLATE) == VARIANT_FALSE)
				{
					throw UCLIDException("ELI20748", "Missing \"Manage Metadata\" privilege!");
				}
				// Missing break intentional
			case kService:
			case kProcess:
				if (m_ipConnection->GetHasFeatureRight(FEATURE_RIGHT_EXPORT) == VARIANT_FALSE)
				{
					throw UCLIDException("ELI20747", "Missing \"Export\" feature right!");
				}
				// Missing break intentional
			case kVerify:
			case kSubmit:
			case kConnect:
				if (m_ipConnection->GetHasFeatureRight(FEATURE_RIGHT_SCAN) == VARIANT_FALSE)
				{
					throw UCLIDException("ELI20750", "Missing \"Properties\" feature right!");
				}
				if (m_ipConnection->GetHasFeatureRight(FEATURE_RIGHT_SEARCH) == VARIANT_FALSE)
				{
					throw UCLIDException("ELI20752", "Missing \"Search\" feature right!");
				}
				break;
			default:
				throw UCLIDException("ELI21050", "Invalid connection mode!");
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20751");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException uexOuter("ELI21036", "Inadequate Laserfiche repository rights or privileges!", 
			ue);
		throw uexOuter;
	}
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::validateRepositorySettings(EConnectionMode eConnectionMode)
{
	try
	{
		try
		{
			validateConnection();

			// Check for any missing required tags
			vector<string> vecMissingTags = findMissingTags();
				
			if (!vecMissingTags.empty())
			{
				UCLIDException ue("ELI20877", 
					"The repository does not contain all required ID Shield tags!");
				try
				{
					for each (string strTag in vecMissingTags)
					{
						ue.addDebugInfo("Missing Tag", strTag);
					}
				}
				catch (...){}

				throw ue;
			}

			// Attempt to find the settings folder
			m_ipSettingsFolder = getSettingsFolder();
			
			// Verify proper access to the settings folder
			verifyHasRight(m_ipSettingsFolder, ACCESS_READ,
					"Missing \"Read\" access rights for ID Shield settings folder!");

			loadSettings();
			
			// [FlexIDSIntegrations:139] Since only the service, on-demand processing, or
			// administration console do anything with the rules, only validate the existence of
			// the rules for these connection types.
			if (eConnectionMode == kAdministrator ||
				eConnectionMode == kService ||
				eConnectionMode == kProcess)
			{
				// Confirm ruleset existence
				validateFileOrFolderExistence(m_RepositorySettings.strMasterRSD);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20772");
	}
	catch (UCLIDException &ue)
	{
		// Ensure we don't leave the IDShieldLF.ini file locked
		safeDispose(m_ipSettingsFile);

		// Repository settings do not need to be validated for the administration console to run.
		// The settings are tested only to see if the warning about existing ID Shield
		// users/services should be displayed.
		if (eConnectionMode != kAdministrator)
		{
			UCLIDException uexOuter("ELI20773", "ID Shield repository is not properly configured!\r\n\r\n"
				"Please verify the repository configuration via the ID Shield Administration Console.",
				ue);
			throw uexOuter;
		}
	}
}
//--------------------------------------------------------------------------------------------------
ILFFolderPtr CIDShieldLF::getSettingsFolder(bool bRefresh/* = false*/)
{
	try
	{
		try
		{
			validateConnection();

			if (bRefresh && m_ipSettingsFolder != NULL)
			{
				// If bRefresh, force the existing folder to NULL to trigger another search.
				m_ipSettingsFolder->Dispose();
				m_ipSettingsFolder = NULL;
			}
			else if (m_ipSettingsFolder)
			{
				// If we already have a settings folder, return it.
				return m_ipSettingsFolder;
			}

			// Search for a folder tagged gstrTAG_SETTINGS
			CLFItemCollection settingsFolderSearch(m_ipDatabase);
			settingsFolderSearch.find(NULL, m_RepositorySettings.strFolderSearchType, true, 
				gstrTAG_SETTINGS);

			// Ensure at most one folder is tagged gstrTAG_SETTINGS
			if (settingsFolderSearch.getCount() > 1)
			{
				// Do not change this ELI code... RepositorySettingsPP depends on it.
				throw UCLIDException("ELI20767", 
					"Multiple folders are tagged \"" + gstrTAG_SETTINGS + "\"!");
			}
		
			// Retrieve the one and only search result.
			m_ipSettingsFolder = settingsFolderSearch.getNextItem();
			ASSERT_RESOURCE_ALLOCATION("ELI20771", m_ipSettingsFolder != NULL);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20753");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException uexOuter("ELI20754", "Error locating ID Shield settings folder!", ue);
		throw uexOuter;
	}
	
	return m_ipSettingsFolder;
}
//--------------------------------------------------------------------------------------------------
ILFDocumentPtr CIDShieldLF::getSettingsFile(bool bRefresh/* = false*/)
{
	// Outside catch scope so that it can be used in catch handler for debug information.
	string strSettingsFilePath;

	try
	{
		try
		{
			if (m_ipSettingsFolder == NULL)
			{
				throw UCLIDException("ELI20883", "Settings folder not found!");
			}

			if (bRefresh && m_ipSettingsFile != NULL)
			{
				// If bRefresh, force the existing folder to NULL to trigger another search.
				m_ipSettingsFile->Dispose();
				m_ipSettingsFile = NULL;
			}
				
			if (m_ipSettingsFile == NULL)
			{
				// Determine the full path name of the settings file
				strSettingsFilePath = asString(m_ipSettingsFolder->FindFullPath()) + "\\" + 
					gstrSETTINGS_FILE;

				try
				{
					try
					{
						m_ipSettingsFile = 
							m_ipDatabase->GetEntryByPath(get_bstr_t(strSettingsFilePath));
						ASSERT_RESOURCE_ALLOCATION("ELI21020", m_ipSettingsFile != NULL)
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21021")
				}
				catch (UCLIDException &ue) 
				{
					// Administrator mode does not require that we find the settings file to run.
					if (m_eConnectionMode == kAdministrator)
					{
						ue.log();
					}
					else
					{
						UCLIDException ue("ELI20842", "Could not locate ID Shield settings!");
						ue.addDebugInfo("Settings Path", strSettingsFilePath);
						throw ue;
					}
				}

				if (m_ipSettingsFile != NULL && m_eConnectionMode == kAdministrator)
				{
					try
					{
						// If in administrator mode, lock the ini file to prevent multiple
						// administration consoles from running at the same time.
						m_ipSettingsFile->LockObject(LOCK_TYPE_WRITE);
					}
					catch (...)
					{
						UCLIDException ue("ELI20847", "ID Shield settings are locked!\r\n\r\n"
							"The Administration Console may be running on another machine.");
						throw ue;
					}
				}
			}
			
			// If the settings file doesn't exist, attempt to create it.
			if (m_ipSettingsFile == NULL)
			{
				m_ipSettingsFile.CreateInstance(CLSID_LFDocument);
				ASSERT_RESOURCE_ALLOCATION("ELI20803", m_ipSettingsFile != NULL);

				ILFCollectionPtr ipVolumes = m_ipDatabase->GetAllVolumes();
				ASSERT_RESOURCE_ALLOCATION("ELI20807", ipVolumes != NULL);
				
				// Find the first mounted volume to create the ini file in.
				ILFVolumePtr ipVolume = NULL;
				long nCount = ipVolumes->Count;
				for (long i = 1; i <= nCount; i++)
				{
					ipVolume = ipVolumes->Item[i];
					ASSERT_RESOURCE_ALLOCATION("ELI20806", ipVolume != NULL);

					if (asCppBool(ipVolume->IsMounted))
					{
						break;
					}
					else
					{
						ipVolume = NULL;
					}
				}

				ASSERT_RESOURCE_ALLOCATION("ELI21035", ipVolume != NULL);
				m_ipSettingsFile->Create(get_bstr_t(gstrSETTINGS_FILE), m_ipSettingsFolder,
					ipVolume, VARIANT_FALSE);

				// lock the ini file to prevent multiple administration consoles from running at the
				// same time.
				m_ipSettingsFile->LockObject(LOCK_TYPE_WRITE);
				
				// Create a page to store the settings in.
				ILFDocumentPagesPtr ipPages = m_ipSettingsFile->Pages;
				ASSERT_RESOURCE_ALLOCATION("ELI20808", ipPages != NULL);
				ipPages->CreateNewPage(1);
				
				m_ipSettingsFile->Update();
				// Keep the file locked to keep admin console from logging in from 2 machines
				// at once.
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20881");
	}
	catch (UCLIDException &ue)
	{
		safeDispose(m_ipSettingsFile);

		throw ue;
	}

	return m_ipSettingsFolder;
}
//--------------------------------------------------------------------------------------------------
vector<string> CIDShieldLF::findMissingTags()
{
	validateConnection();

	vector<string> vecMissingTags;

	// Generate a vector of required tags
	vector<string> vecRequiredTags;
	vecRequiredTags.push_back(gstrTAG_SETTINGS);
	vecRequiredTags.push_back(gstrTAG_PENDING_PROCESSING);
	vecRequiredTags.push_back(gstrTAG_PROCESSED);
	vecRequiredTags.push_back(gstrTAG_PENDING_VERIFICATION);
	vecRequiredTags.push_back(gstrTAG_VERIFYING);
	vecRequiredTags.push_back(gstrTAG_VERIFIED);
	vecRequiredTags.push_back(gstrTAG_FAILED_PROCESSING);

	// For each required tag not in the repository, add it to vecMissingTags
	for each (string strTag in vecRequiredTags)
	{
		try
		{
			if (m_ipDatabase->GetTagByName(get_bstr_t(strTag)) == NULL)
			{
				vecMissingTags.push_back(strTag);
			}
		}
		catch (...)
		{
			// In most if not all circumstances, if the tag doesn't exist, GetTagByName will throw
			// and exception rather than return NULL.
			vecMissingTags.push_back(strTag);
		}
	}

	return vecMissingTags;
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::createTags(const vector<string> &vecTagsToCreate)
{
	validateConnection();

	// For each specified tag, add it to the repository
	for each (string strTag in vecTagsToCreate)
	{
		ILFTagPtr ipNewTag(CLSID_LFTag);
		ASSERT_RESOURCE_ALLOCATION("ELI20781", ipNewTag != NULL);

		ipNewTag->Create(m_ipDatabase, get_bstr_t(strTag));
	}
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::loadSettings(bool bReload/* = false*/)
{
	try
	{
		getSettingsFolder();
		getSettingsFile(bReload);

		// The ID Shield settings are on page 1 of the settings file.
		ILFDocumentPagesPtr ipPages = m_ipSettingsFile->Pages;
		ASSERT_RESOURCE_ALLOCATION("ELI20797", ipPages != NULL);

		ILFPagePtr ipPage = ipPages->Item[1];
		ASSERT_RESOURCE_ALLOCATION("ELI20798", ipPage != NULL);

		// Export the settings to a temporary file where they can be accessed and manipulated
		// via an INIFilePersistenceMgr object.
		m_apLocalSettingsFile.reset(new TemporaryFileName("", ".ini", true));
		ASSERT_RESOURCE_ALLOCATION("ELI20801", m_apLocalSettingsFile.get() != NULL);

		ILFTextPtr ipText = ipPage->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI20810", ipText != NULL);

		writeToFile(asString(ipText->Text), m_apLocalSettingsFile->getName());

		// Create the INIFilePersistenceMgr to read/write the settings to/from the temporary
		// file.
		m_apSettingsMgr.reset(new INIFilePersistenceMgr(m_apLocalSettingsFile->getName()));
		ASSERT_RESOURCE_ALLOCATION("ELI20802", m_apSettingsMgr.get() != NULL);

		// Read all the settings into the m_RepositorySettings struct
		m_RepositorySettings.strMasterRSD = m_apSettingsMgr->getKeyValue(
			gstrINI_SECTION_REDACTION, gstrINI_MASTERRSD);

		string strHCData = m_apSettingsMgr->getKeyValue(gstrINI_SECTION_REDACTION, gstrINI_HCDATA, "");
		m_RepositorySettings.bRedactHCData = (strHCData.empty() ? true : asCppBool(strHCData));

		string strMCData = m_apSettingsMgr->getKeyValue(gstrINI_SECTION_REDACTION, gstrINI_MCDATA, "");
		m_RepositorySettings.bRedactMCData = (strMCData.empty() ? true : asCppBool(strMCData));

		string strLCData = m_apSettingsMgr->getKeyValue(gstrINI_SECTION_REDACTION, gstrINI_LCDATA, "");
		m_RepositorySettings.bRedactLCData = (strLCData.empty() ? true : asCppBool(strLCData));

		string strAutoTag = m_apSettingsMgr->getKeyValue(gstrINI_SECTION_VERIFICATION, gstrINI_AUTO_TAG, "");
		m_RepositorySettings.bAutoTagForVerify = (strAutoTag.empty() ? true : asCppBool(strAutoTag));

		string strTagAll = m_apSettingsMgr->getKeyValue(gstrINI_SECTION_VERIFICATION, gstrINI_TAG_ALL, "");
		m_RepositorySettings.bTagAllForVerify = (strTagAll.empty() ? true : asCppBool(strTagAll));

		string strOnDemand = m_apSettingsMgr->getKeyValue(gstrINI_SECTION_VERIFICATION, gstrINI_ON_DEMAND, "");
		m_RepositorySettings.bOnDemandVerify = (strOnDemand.empty() ? true : asCppBool(strOnDemand));

		// [FlexIDSIntegrations:51]
		string strEnsureTextRedactions =
			m_apSettingsMgr->getKeyValue(gstrINI_SECTION_VERIFICATION, gstrINI_ENSURE_TEXT_REDACTIONS, "");
		m_RepositorySettings.bEnsureTextRedactions =
			(strEnsureTextRedactions.empty() ? true : asCppBool(strEnsureTextRedactions));

		// [FlexIDSIntegrations:137] For searches in a Laserfiche repository, a type of "B" needs
		// to be used to find documents without a template.  But because at least one customer
		// is experiencing slow search performance using the "D" type, the search types used can now be
		// specified in the ini file to allow customers flexibility in how searches are performed.
		// (The search types are not exposed in the admin console UI, however)
		string strDocumentSearchType = m_apSettingsMgr->getKeyValue(
				gstrINI_SEARCH_SECTION, gstrINI_DOCUMENT_SEARCH_TYPE, "");
		m_RepositorySettings.strDocumentSearchType = 
			(strDocumentSearchType.empty() ? gstrDEFAULT_DOCUMENT_SEARCH_TYPE : strDocumentSearchType);

		string strFolderSearchType = m_apSettingsMgr->getKeyValue(
				gstrINI_SEARCH_SECTION, gstrINI_FOLDER_SEARCH_TYPE, "");
		m_RepositorySettings.strFolderSearchType = 
			(strFolderSearchType.empty() ? gstrDEFAULT_FOLDER_SEARCH_TYPE : strFolderSearchType);

		string strAllSearchType = m_apSettingsMgr->getKeyValue(
				gstrINI_SEARCH_SECTION, gstrINI_ALL_SEARCH_TYPE, "");
		m_RepositorySettings.strAllSearchType =
			(strAllSearchType.empty() ? gstrDEFAULT_ALL_SEARCH_TYPE : strAllSearchType);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20793");
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::saveSettings()
{
	// Declare the settings file text outside the try scope so it can be freed in the case
	// of an exception
	ILFTextPtr ipSettingsText = NULL;

	try
	{
		try
		{
			getSettingsFolder();

			// Ensure login mode of kAdministrator before allowing a save.
			if (m_eConnectionMode != kAdministrator)
			{
				throw UCLIDException("ELI20845", 
					"Internal error: Can't save settings if not in admin mode!");
			}

			// Ensure all necessary objects are available
			if (m_ipSettingsFile == NULL)
			{
				throw UCLIDException("ELI20828", "Repository settings not initialized!");
			}
			if (m_apLocalSettingsFile.get() == NULL)
			{
				throw UCLIDException("ELI20829", "Repository settings not initialized!");
			}
			if (m_apSettingsMgr.get() == NULL)
			{
				throw UCLIDException("ELI20830", "Repository settings not initialized!");
			}

			// Apply the settings from m_RepositorySettings to the temporary ini file
			m_apSettingsMgr->setKeyValue(gstrINI_SECTION_REDACTION, gstrINI_MASTERRSD,
				m_RepositorySettings.strMasterRSD);

			m_apSettingsMgr->setKeyValue(gstrINI_SECTION_REDACTION, gstrINI_HCDATA, 
				asString(m_RepositorySettings.bRedactHCData));

			m_apSettingsMgr->setKeyValue(gstrINI_SECTION_REDACTION, gstrINI_MCDATA, 
				asString( m_RepositorySettings.bRedactMCData));

			m_apSettingsMgr->setKeyValue(gstrINI_SECTION_REDACTION, gstrINI_LCDATA, 
				asString(m_RepositorySettings.bRedactLCData));

			m_apSettingsMgr->setKeyValue(gstrINI_SECTION_VERIFICATION, gstrINI_AUTO_TAG, 
				asString(m_RepositorySettings.bAutoTagForVerify));

			m_apSettingsMgr->setKeyValue(gstrINI_SECTION_VERIFICATION, gstrINI_TAG_ALL,
				asString(m_RepositorySettings.bTagAllForVerify));

			m_apSettingsMgr->setKeyValue(gstrINI_SECTION_VERIFICATION, gstrINI_ON_DEMAND,
				asString(m_RepositorySettings.bOnDemandVerify));

			// [FlexIDSIntegrations:51]
			m_apSettingsMgr->setKeyValue(gstrINI_SECTION_VERIFICATION, 
				gstrINI_ENSURE_TEXT_REDACTIONS, 
				asString(m_RepositorySettings.bEnsureTextRedactions));

			// [FlexIDSIntegrations:137] Save search type settings. While these settings are not available
			// in the UI, by saving the default types to the ini file, it will make  it easier for
			// customers to edit the types if necessary.
			m_apSettingsMgr->setKeyValue(gstrINI_SEARCH_SECTION, gstrINI_DOCUMENT_SEARCH_TYPE,
				m_RepositorySettings.strDocumentSearchType);

			m_apSettingsMgr->setKeyValue(gstrINI_SEARCH_SECTION, gstrINI_FOLDER_SEARCH_TYPE,
				m_RepositorySettings.strFolderSearchType);
			
			m_apSettingsMgr->setKeyValue(gstrINI_SEARCH_SECTION, gstrINI_ALL_SEARCH_TYPE,
				m_RepositorySettings.strAllSearchType);

			// Copy the resulting settings data in the temporary ini file into the repository's
			// ini file.
			string strSettings = getTextFileContentsAsString(m_apLocalSettingsFile->getName());
			// Unless we remove the carriage returns from the file we save, saving and loading the
			// settings file results in "\r" being replaced by "\r\n" and we end up accumulating blank
			// lines.  Therefore, remove the carriage returns before storing the file in Laserfiche.
			replaceVariable(strSettings, "\r", "");

			m_ipSettingsFile->LockObject(LOCK_TYPE_WRITE);

			ILFDocumentPagesPtr ipPages = m_ipSettingsFile->Pages;
			ASSERT_RESOURCE_ALLOCATION("ELI20832", ipPages != NULL);

			ILFPagePtr ipPage = ipPages->Item[1];
			ASSERT_RESOURCE_ALLOCATION("ELI20833", ipPage != NULL);

			ipSettingsText = ipPage->Text;
			ASSERT_RESOURCE_ALLOCATION("ELI20834", ipSettingsText != NULL);
			
			ipSettingsText->LockObject(LOCK_TYPE_WRITE);
			ipSettingsText->Text = get_bstr_t(strSettings);
			ipSettingsText->Update();
			ipSettingsText->UnlockObject();
			// Do not unlock settings file
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20826");
	}
	catch (UCLIDException &ue)
	{
		safeDispose(ipSettingsText);

		UCLIDException uexOuter("ELI20827", "Failed to save repository settings!", ue);
		throw uexOuter;
	}
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::showRedactionTab(bool bShow/* = true*/)
{
	if (bShow)
	{
		if (m_apPropertySheet->GetPageIndex(m_appageRedaction.get()) == -1)
		{
			// Ensure settings are loaded
			loadSettings();

			// Show the redaction tab if it is not already showing
			m_apPropertySheet->AddPage(m_appageRedaction.get());
		}
		else
		{
			// [FlexIDSIntegrations:64]
			// Every time showRedactionTab is called with bShow == true, it is an indicator that 
			// settings have changed, reload them.
			loadSettings(true);

			// Redisplay the redaction tab to reflect newly loaded settings.
			m_apPropertySheet->RemovePage(m_appageRedaction.get());
			m_appageRedaction.reset(new CRedactionSettingsPP(this));
			ASSERT_RESOURCE_ALLOCATION("ELI21865", m_appageRedaction.get() != NULL);
			m_apPropertySheet->AddPage(m_appageRedaction.get());
		}

		// Remove and re-add the about page to ensure it remains the last tab
		m_apPropertySheet->RemovePage(m_appageAbout.get());
		m_apPropertySheet->AddPage(m_appageAbout.get());
	}
	else
	{
		// Hide the redaction tab if it is currently showing
		if (m_apPropertySheet->GetPageIndex(m_appageRedaction.get()) != -1)
		{
			m_apPropertySheet->RemovePage(m_appageRedaction.get());
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::processSelected(bool bOnDemand)
{
	// Declared outside try scope so the documents they contain can be disposed in case of exception.
	list<CLFItem> listDocumentsToProcess;

	try
	{
		try
		{
			// Allow only one client task instance to run at a time.
			CMutex namedMutex(TRUE, gstrCLIENT_TASKS_MUTEX_NAME.c_str());
			if (GetLastError() == ERROR_ALREADY_EXISTS)
			{
				UCLIDException ue("ELI21355", gstrPRODUCT_NAME + " is already running!");
				ue.addDebugInfo("Application", gstrCLIENT_TASKS_MUTEX_NAME);
				throw ue;
			}

			// The wait dialog needs to be closed here to give forground status back to Laserfiche
			// Otherwise, Laserfiche 8 doesn't seem to report selected folders within the
			// getSelectedDocuments call.
			m_apDlgWait->close();

			// Display a WaitDlg while collecting the currently selected documents. 
			m_apDlgWait->showMessage("Collecting Documents...");

			string strTagsToFind = (bOnDemand ? "" : "!" + gstrTAG_PENDING_PROCESSING);
			CLFItemCollection::getSelectedDocuments(m_ipClient, m_ipDatabase, 
				strTagsToFind, m_RepositorySettings.strDocumentSearchType, listDocumentsToProcess);
			
			// Remove documents from list depending upon rights, lock status and processing status
			// (prepareListForRedaction will close the wait dialog)
			size_t nOrigDocCount = listDocumentsToProcess.size();

			m_apDlgWait->hide();

			if (prepareListForRedaction(listDocumentsToProcess, bOnDemand) == false)
			{
				// User cancelled processing
				return;
			}

			long nCount = (long) listDocumentsToProcess.size();
			if (nCount == 0)
			{
				MessageBox(m_hwndClient,
					(nOrigDocCount > 0) ? 
					"There are no more documents in the current selection to process for redactions!" :
					"There are no documents in the current selection to process for redactions!",
					gstrPRODUCT_NAME.c_str(), MB_OK|MB_ICONINFORMATION);
				return;
			}

			// create the progress status object
			IProgressStatusPtr ipProgressStatus(CLSID_ProgressStatus);
			ASSERT_RESOURCE_ALLOCATION("ELI20872", ipProgressStatus != NULL);

			bool bProcessingError = false;
			Win32Event eventStop;
			CProgressDlg dlgProgress(ipProgressStatus, m_hwndClient, eventStop.getHandle());

			ipProgressStatus->InitProgressStatus("Redacting Documents...", 0, nCount, VARIANT_TRUE);

			// This loop cycles through each document and processes each
			long nProcessedCount = 1;
			list<CLFItem>::iterator iter = listDocumentsToProcess.begin();
			while (iter != listDocumentsToProcess.end())
			{
				// If the user cancelled, remove all items from the list that have not yet been processed.
				if (eventStop.isSignaled())
				{
					listDocumentsToProcess.erase(iter, listDocumentsToProcess.end());
					break;
				}

				string strStatus = string(bOnDemand ? "Redacting " : "Submitting ") + 
					"document " + asString(nProcessedCount++) + " of " + asString(nCount);
				ipProgressStatus->StartNextItemGroup(get_bstr_t(strStatus), 1);

				ILFDocumentPtr ipDocument = iter->getItem(m_ipDatabase);
				ASSERT_RESOURCE_ALLOCATION("ELI21759", ipDocument != NULL);

				if (bOnDemand)
				{
					bool bNeedsVerification;
					if (processDocument(ipDocument, true, &bNeedsVerification
						, ipProgressStatus->SubProgressStatus) == false)
					{
						bProcessingError = true;
						cancel(listDocumentsToProcess, ipProgressStatus);
						break;
					}
					
					if (!bNeedsVerification)
					{
						iter = listDocumentsToProcess.erase(iter);
						ipProgressStatus->CompleteCurrentItemGroup();
						continue;
					}
				}
				else // submitting
				{
					if (submitDocument(ipDocument) == false)
					{
						bProcessingError = true;
						cancel(listDocumentsToProcess, ipProgressStatus);
						break;
					}

					iter->unloadItem();
				}

				ipProgressStatus->CompleteCurrentItemGroup();
				iter ++;
			}

			dlgProgress.Close();

			if (bOnDemand && listDocumentsToProcess.size() > 0)
			{
				if (eventStop.isSignaled())
				{
					int nVerify = MessageBox(m_hwndClient, "Do you wish to verify the documents that "
						"completed processing now?", gstrPRODUCT_NAME.c_str(), MB_ICONQUESTION | MB_YESNO);

					if (nVerify == IDNO)
					{
						return;
					}
				}

				// This is one place where it is better to use close() instead of hide() for the 
				// wait dialog.  If we use close, it keeps ID Shield as the foreground process which
				// is nice if an exception is thrown, but is also allows Laserfiche to fall out
				// of the foreground after the first document is displayed.
				m_apDlgWait->close();

				verify(listDocumentsToProcess);
			}
			else if (eventStop.isSignaled())
			{
				string strMessage(string(bOnDemand ? "Processing" : "Submission") + " Stopped!");
				::MessageBox(m_hwndClient, strMessage.c_str(), gstrPRODUCT_NAME.c_str(), 
					MB_OK | MB_ICONINFORMATION);
			}
			else if (!bProcessingError)
			{
				string strMessage(string(bOnDemand ? "Processing" : "Submission") + " Complete!");
				::MessageBox(m_hwndClient, strMessage.c_str(), gstrPRODUCT_NAME.c_str(), 
					MB_OK | MB_ICONINFORMATION);
			}
		}
		catch (...)
		{
			// If there were any problems, attempt to unlock any remaining documents.
			safeDisposeDocuments(listDocumentsToProcess);
			m_apDlgWait.reset();

			throw;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20850")
}
//--------------------------------------------------------------------------------------------------
bool CIDShieldLF::prepareListForRedaction(list<CLFItem> &rlistDocuments, bool bOnDemand)
{
	try
	{
		// Initialize counts of various documents states to zero
		int nInsufficientPermissions = 0;
		int nLocked = 0;
		int nProcessed = 0;
		int nFailed = 0;
		int nNoImage = 0;

		// Collections of document indexes that may need to be removed from the overall collection.
		vector<list<CLFItem>::iterator> vecProcessed;
		vector<list<CLFItem>::iterator> vecFailed;

		long nTotal = (long) rlistDocuments.size();

		if (nTotal > m_nMaxDocsToProcess)
		{
			string strPrompt;
			if (bOnDemand)
			{
				strPrompt = asString(nTotal) + " documents have been selected, but only " + 
					asString(m_nMaxDocsToProcess) + " can be processed at one time.\r\n\r\n" +
					"Do you wish to process the first " + asString(m_nMaxDocsToProcess) +
					" documents in the selection now?";
			}
			else
			{
				strPrompt = asString(nTotal) + " documents in the selection are not already " +
					"awaiting background processing, but only " + 
					asString(m_nMaxDocsToProcess) + " can be submitted at one time.\r\n\r\n" +
					"Do you wish to submit the first " + asString(m_nMaxDocsToProcess) +
					" documents in the selection now?";
			}

			int nAnswer = MessageBox(m_hwndClient, strPrompt.c_str(), gstrPRODUCT_NAME.c_str(), 
				MB_ICONWARNING|MB_YESNO);
			if (nAnswer == IDNO)
			{
				cancel(rlistDocuments);
				return false;
			}

			nTotal = m_nMaxDocsToProcess;
		}

		// create the progress status object
		IProgressStatusPtr ipProgressStatus(CLSID_ProgressStatus);
		ASSERT_RESOURCE_ALLOCATION("ELI21959", ipProgressStatus != NULL);

		Win32Event eventStop;
		CProgressDlg dlgProgress(ipProgressStatus, m_hwndClient, eventStop.getHandle());

		_bstr_t bstrStatus = "Organizing " + get_bstr_t(asString(nTotal)) + " documents...";
		ipProgressStatus->InitProgressStatus(bstrStatus, 0, nTotal, VARIANT_TRUE);

		// Loop through each document checking to see if it is in a special state.  Update
		// counts/collection as necessary.
		long nOrganizedCount = 0;
		list<CLFItem>::iterator iter = rlistDocuments.begin();
		while (iter != rlistDocuments.end())
		{
			if (nOrganizedCount >= m_nMaxDocsToProcess)
			{
				// If we've processed the maximum number of documents, remove the remaining
				// documents from the list.
				rlistDocuments.erase(iter, rlistDocuments.end());
				break;
			}
			nOrganizedCount++;

			ipProgressStatus->StartNextItemGroup(bstrStatus, 1);

			ILFDocumentPtr ipDocument = NULL;

			try
			{
				ipDocument = iter->getItem(m_ipDatabase);
				ASSERT_RESOURCE_ALLOCATION("ELI20864", ipDocument != NULL);

				// Ensure the document has an image that can be redacted.
				if (ipDocument->FindHasImagePages() == VARIANT_FALSE)
				{
					nNoImage ++;
					iter = rlistDocuments.erase(iter);
					continue;
				}

				// Ensure necessary permissions
				verifyHasRight(ipDocument, ACCESS_READ);
				verifyHasRight(ipDocument, ACCESS_ANNOTATE);
				verifyHasRight(ipDocument, ACCESS_WRITE_METADATA);
			}
			catch (...)
			{
				// Any exception that ends up here indicates inadequate rights or permissions
				nInsufficientPermissions ++;
				iter = rlistDocuments.erase(iter);
				continue;
			}

			try
			{
				// Attempting to lock the document appears to be the only reliable way to 
				// check for an existing lock and is also the behavior we want at this point.
				ipDocument->LockObject(LOCK_TYPE_WRITE);
			}
			catch (...)
			{
				nLocked ++;
				iter = rlistDocuments.erase(iter);
				continue;
			}

			// Check for documents that were already processed
			if (hasTag(ipDocument, gstrTAG_PROCESSED))
			{
				nProcessed ++;
				vecProcessed.push_back(iter);
			}
			// Check for documents that failed during processing
			else if (hasTag(ipDocument, gstrTAG_FAILED_PROCESSING))
			{
				nFailed ++;
				vecFailed.push_back(iter);
			}

			iter ++;

			if (eventStop.isSignaled())
			{
				cancel(rlistDocuments);
				return false;
			}
		}

		dlgProgress.Close();

		// Prompt concerning conditions that prevent processing of documents
		string strPrompt;
		if (nInsufficientPermissions > 0)
		{
			strPrompt = "You do not have sufficient access or permissions to process " +
				asString(nInsufficientPermissions) + " of " + asString(nTotal) +
				" selected document(s).\r\n";
		}

		if (nNoImage > 0)
		{
			strPrompt += asString(nNoImage) + " of " + asString(nTotal) +
				" selected document(s) do not contain images and cannot be redacted.\r\n";
		}

		if (nLocked > 0)
		{
			strPrompt += asString(nLocked) + " of " + asString(nTotal) +
				" selected document(s) are locked.  ID Shield may be processing these document(s) " +
				" on another machine.\r\n";
		}

		if (!strPrompt.empty())
		{
			strPrompt += "\r\nDo you wish to process the remaining document(s)?";
			
			if (MessageBox(m_hwndClient, strPrompt.c_str(), gstrPRODUCT_NAME.c_str(), 
				MB_ICONWARNING|MB_YESNO) == IDNO)
			{
				cancel(rlistDocuments);
				return false;
			}
		}

		// Determine how many documents remain in the selection set after the disqualifications have
		// been processed.
		long nRemaining = nTotal - nInsufficientPermissions - nNoImage - nLocked;

		// Prompt for whether to re-process documents that have already been processed
		if (nProcessed > 0)
		{
			if (nRemaining < nTotal)
			{
				strPrompt = asString(nProcessed) + " of " + asString(nRemaining) + 
					" remaining selected document(s) " +
					"have already been processed.  Do you wish to reprocess these document(s)?";
			}
			else
			{
				strPrompt = asString(nProcessed) + " of " + asString(nTotal)+ " selected document(s) " +
					"have already been processed.  Do you wish to reprocess these document(s)?";
			}

			int nAnswer = MessageBox(m_hwndClient, strPrompt.c_str(), gstrPRODUCT_NAME.c_str(), 
				MB_ICONWARNING|MB_YESNOCANCEL);
			if (nAnswer == IDCANCEL)
			{
				cancel(rlistDocuments);
				return false;
			}
			else if (nAnswer == IDNO)
			{
				for each (list<CLFItem>::iterator iter in vecProcessed)
				{
					rlistDocuments.erase(iter);
				}
				// Update the remaining count to exclude already processed documents.
				nRemaining = (long) rlistDocuments.size(); 
			}
		}

		// Prompt for whether to re-process documents that previously failed processing
		if (nFailed > 0)
		{
			if (nRemaining < nTotal)
			{
				strPrompt = asString(nFailed) + " of " + asString(nRemaining) + 
					" remaining selected document(s) " +
					"failed to process correctly in a previous redaction attempt. " +
					"Do you wish to reprocess these document(s)?";
			}
			else
			{
				strPrompt = asString(nFailed) + " of " + asString(nTotal)+ " selected document(s) " +
					"failed to process correctly in a previous redaction attempt. " +
					"Do you wish to reprocess these document(s)?";
			}

			int nAnswer = MessageBox(m_hwndClient, strPrompt.c_str(), gstrPRODUCT_NAME.c_str(), 
				MB_ICONWARNING|MB_YESNOCANCEL);
			if (nAnswer == IDCANCEL)
			{
				cancel(rlistDocuments);
				return false;
			}
			else if (nAnswer == IDNO)
			{
				for each (list<CLFItem>::iterator iter in vecFailed)
				{
					rlistDocuments.erase(iter);
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20854");

	return true;
}
//--------------------------------------------------------------------------------------------------
bool CIDShieldLF::processDocument(ILFDocumentPtr &ripDocument, bool bOnDemand, 
								  bool *pbVerifyNow/* = NULL*/, 
								  IProgressStatusPtr ipProgressStatus/* = NULL*/)
{
	INIT_EXCEPTION_AND_TRACING("MLI00875");

	try
	{
		try
		{
			CSingleLock lock(&m_mutexLFOperations, TRUE);

			ASSERT_ARGUMENT("ELI20871", ripDocument != NULL);

			_lastCodePos = "10";
		
			// Remove clue highlights and all tags that apply to processed documents
			removeAnnotations(ripDocument, gcolorCLUE_HIGHLIGHT);
			removeTag(ripDocument, gstrTAG_PROCESSED);
			removeTag(ripDocument, gstrTAG_PENDING_VERIFICATION);
			removeTag(ripDocument, gstrTAG_VERIFIED);
			removeTag(ripDocument, gstrTAG_FAILED_PROCESSING);

			_lastCodePos = "20";

			// Release m_mutexLFOperations so that the AF engine can run multi-threaded
			// and so that we don't deadlock when redactDocument takes out a lock
			// against this same mutex.
			lock.Unlock();
			
			// Calculate & apply redactions
			bool bSensitiveDataFound = redactDocument(ripDocument, ipProgressStatus);

			// Lock m_mutexLFOperations again as we update the Laserfiche document
			lock.Lock();

			_lastCodePos = "30";

			// Remove pending processing tag which will only be present if the document was
			// submitted for background redaction
			removeTag(ripDocument, gstrTAG_PENDING_PROCESSING);

			// Tag as successfully processed.
			addTag(ripDocument, gstrTAG_PROCESSED);

			_lastCodePos = "40";

			// Tag for verification as necessary and determine whether verification should be 
			// imminent.
			bool bVerifyNow = bOnDemand && m_RepositorySettings.bOnDemandVerify;
			
			if (m_RepositorySettings.bAutoTagForVerify && 
				(m_RepositorySettings.bTagAllForVerify || bSensitiveDataFound))
			{
				addTag(ripDocument, gstrTAG_PENDING_VERIFICATION);
			}
			else
			{
				bVerifyNow = false;
			}

			_lastCodePos = "50";

			ripDocument->Update();
			ripDocument->UnlockObject();

			_lastCodePos = "60";

			if (pbVerifyNow != NULL)
			{
				*pbVerifyNow = bVerifyNow;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20870");
	}
	catch (UCLIDException &ue)
	{
		// If the document failed to process, make sure it isn't brought up for verification.
		if (pbVerifyNow != NULL)
		{
			*pbVerifyNow = false;
		}

		string strMessage = "Failed to process document!";
		
		// Attempt to generate better debug info and tag the document as failed.
		try
		{
			strMessage = "Failed to process document \"" + asString(ripDocument->Name) + "\"!";

			try
			{
				removeAnnotations(ripDocument, gcolorCLUE_HIGHLIGHT);
			}
			// Don't throw any further exceptions if we failed to remove the clue highlights
			catch (...) {}

			removeTag(ripDocument, gstrTAG_PENDING_PROCESSING);
			string strTagText = ue.getTopText() + " (" + ue.getTopELI() + ")";
			addTag(ripDocument, gstrTAG_FAILED_PROCESSING, strTagText);

			ripDocument->Update();
		}
		// Only log exceptions in post-failure processing of the document, don't throw it.
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21022");

		safeDispose(ripDocument);

		UCLIDException uexOuter("ELI20874", strMessage, ue);
		
		// If this is being processed in the foreground, prompt for whether to continue processing.
		if (bOnDemand)
		{
			uexOuter.display();

			int nContinue = MessageBox(m_hwndClient, "Do you wish to continue processing the "
				"remaining document(s)?", gstrPRODUCT_NAME.c_str(), MB_ICONQUESTION|MB_YESNO);

			if (nContinue == IDNO)
			{
				return false;
			}
		}
		else
		{
			uexOuter.log();
		}
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
bool CIDShieldLF::submitDocument(ILFDocumentPtr &ripDocument)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI20913", ripDocument != NULL);

			// Remove clue highlights and all tags that apply to processed documents
			removeAnnotations(ripDocument, gcolorCLUE_HIGHLIGHT);
			removeTag(ripDocument, gstrTAG_PROCESSED);
			removeTag(ripDocument, gstrTAG_PENDING_VERIFICATION);
			removeTag(ripDocument, gstrTAG_VERIFIED);
			removeTag(ripDocument, gstrTAG_FAILED_PROCESSING);

			// Add tag marking the document for background redaction.
			addTag(ripDocument, gstrTAG_PENDING_PROCESSING);

			ripDocument->Update();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20914");
	}
	catch (UCLIDException &ue)
	{
		string strMessage = "Failed to submit document!";
		try
		{
			strMessage = "Failed to submit document \"" + asString(ripDocument->Name) + "\"!";
		}
		// Ignore any exception trying to retrieve the document name, just use the generic
		// error message in that case
		catch (...) {}

		safeDispose(ripDocument);

		UCLIDException uexOuter("ELI20915", strMessage, ue);
		uexOuter.display();
		
		// Prompt for whether to continue submission
		int nContinue = MessageBox(m_hwndClient, "Do you wish to continue processing the "
			"remaining document(s)?", gstrPRODUCT_NAME.c_str(), MB_ICONQUESTION|MB_YESNO);

		if (nContinue == IDNO)
		{
			return false;
		}
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
bool CIDShieldLF::redactDocument(ILFDocumentPtr ipDocument, IProgressStatusPtr ipProgressStatus)
{
	bool bSensitiveDataFound = false;
	vector<ILFPagePtr> vecPages;

	INIT_EXCEPTION_AND_TRACING("MLI00872");

	try
	{
		ASSERT_ARGUMENT("ELI20496", ipDocument != NULL);

		CSingleLock lock(&m_mutexLFOperations, TRUE);

		_lastCodePos = "10";

		// Initialize progress status if it was provided
		if (ipProgressStatus)
		{
			ipProgressStatus->InitProgressStatus("Preparing file...", 0, gnREDACTION_TOTAL_STEPS, 
				VARIANT_TRUE);
			ipProgressStatus->StartNextItemGroup("Preparing file...", gnREDACTION_PREP_STEPS);
		}

		// make sure the rsd file exists
		validateFileOrFolderExistence(m_RepositorySettings.strMasterRSD);

		_lastCodePos = "20";

		_bstr_t bstrRulesFile = get_bstr_t(m_RepositorySettings.strMasterRSD);

		_lastCodePos = "30";

		// Export the Laserfiche document to a temporary local file so they can be processed
		// with the AFEngine.
		ILFDocumentPagesPtr ipPages = ipDocument->GetPages();
		ASSERT_RESOURCE_ALLOCATION("ELI20497", ipPages != NULL);

		_lastCodePos = "40";

		ipPages->MarkAllPages();

		_lastCodePos = "50";

		IDocumentExporterPtr ipExporter(CLSID_DocumentExporter);
		ASSERT_RESOURCE_ALLOCATION("ELI20499", ipExporter != NULL);

		ipExporter->AddSourcePages(ipPages);
		ipExporter->Format = DOCUMENT_FORMAT_TIFF;

		_lastCodePos = "60";

		// Due to threading issue with TemporaryFileName (LegacyRCAndUtils:4975), pass in a prefix
		// consisting of the thread id to prevent problems for now.
		string strSuffix = asString(GetCurrentThreadId())+ ".tif";
		TemporaryFileName tempFile("", strSuffix.c_str(), true);		
		bstr_t bstrTempFile = get_bstr_t(tempFile.getName());

		_lastCodePos = "70";

		ipExporter->ExportToFile(bstrTempFile);

		// Release m_mutexLFOperations so that the AF engine can run multi-threaded
		lock.Unlock();

		_lastCodePos = "80";

		if (ipProgressStatus)
		{
			_bstr_t bstrStatus = "Calculating redactions for \"" + ipDocument->GetName() + "\"";
			ipProgressStatus->StartNextItemGroup(bstrStatus, gnREDACTION_EXEC_STEPS);
		}

		// Create AFDocument for attribute finding
		IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI20504", ipAFDoc != NULL);

		// Create an AFEngine instance to do the processing
		IAttributeFinderEnginePtr ipAFEngine(CLSID_AttributeFinderEngine);
		ASSERT_RESOURCE_ALLOCATION("ELI20505", ipAFEngine != NULL);

		_lastCodePos = "90";

		// Find attributes from the source document
		IIUnknownVectorPtr ipAttributes = ipAFEngine->FindAttributes(ipAFDoc, 
			bstrTempFile, -1, bstrRulesFile, NULL, 
			ipProgressStatus ? ipProgressStatus->SubProgressStatus : NULL);
		ASSERT_RESOURCE_ALLOCATION("ELI20506", ipAttributes != NULL);

		_lastCodePos = "100";

		if (ipProgressStatus)
		{
			_bstr_t bstrStatus = "Applying redactions to \"" + ipDocument->GetName() + "\"";
			ipProgressStatus->StartNextItemGroup(bstrStatus, gnREDACTION_APPLY_STEPS);
		}

		long nAttributeCount = ipAttributes->Size();
		IProgressStatusPtr ipApplyingStatus = 
			ipProgressStatus ? ipProgressStatus->SubProgressStatus : NULL;
		if (ipApplyingStatus != NULL)
		{
			ipApplyingStatus->InitProgressStatus("Updating document...", 0, nAttributeCount, VARIANT_TRUE);
		}

		map<long long, vector<ILFImageBlockAnnotationPtr> > mapExistingAnnotations;

		// Lock m_mutexLFOperations again as we update the Laserfiche document
		lock.Lock();
		
		// IMPORTANT:
		// In order for addAnnotation to access the annotation from mapExistingAnnotations
		// the page objects from which they came must still exist.  Load each page object
		// into a vector for the duration of processing on this document.
		long nPageCount = ipPages->Count;
		vector<ILFPagePtr> vecPages(nPageCount);
		for (long i = 1; i <= nPageCount; i++)
		{
			ILFPagePtr ipPage = ipPages->Item[i];
			ASSERT_RESOURCE_ALLOCATION("ELI21495", ipPage != NULL);

			vecPages[i-1] = ipPage;
		}

		_lastCodePos = "110";
		
		// Loop through all attributes that were found and apply them to the Laserfiche document
		// as required
		for (long i = 0; i < nAttributeCount; i++)
		{
			_lastCodePos = "120";
			if (ipApplyingStatus != NULL)
			{
				ipApplyingStatus->StartNextItemGroup("Updating document...", 1);
			}

			IAttributePtr ipAttribute = ipAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI20508", ipAttribute != NULL);

			ISpatialStringPtr ipValue = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI21004", ipValue != NULL);

			_lastCodePos = "130";

			if (ipValue->GetMode() == kNonSpatialMode)
			{
				// Cannot apply non-spatial attribute
				continue;
			}

			IIUnknownVectorPtr ipLines = ipValue->GetLines();
			ASSERT_RESOURCE_ALLOCATION("ELI21055", ipLines != NULL);

			_lastCodePos = "140";
			
			long nLineCount = ipLines->Size();
			for (long i = 0; i < nLineCount; i++)
			{
				_lastCodePos = "150";

				ISpatialStringPtr ipLine = ipLines->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI21056", ipLine != NULL);

				if (ipLine->GetMode() == kSpatialMode)
				{
					// Convert a spatial attribute to hybrid to get one single image area to use.
					ipLine->DownGradeToHybridMode();
				}

				_lastCodePos = "160";

				// Get the attribute bounds
				ILongRectanglePtr ipBounds = ipLine->GetBounds();
				ASSERT_RESOURCE_ALLOCATION("ELI20513", ipBounds != NULL);

				// [FlexIDSIntegrations:145] The bounds of the attribute needs to be rotated to
				// compensate for rotation that has been applied within Laserfiche.
				long nPage = ipLine->GetFirstPageNumber();
				ILFPagePtr ipPage = vecPages[nPage - 1];
				ASSERT_RESOURCE_ALLOCATION("ELI24623", ipPage != NULL);

				ILFImagePtr ipImage = ipPage->GetImage();
				ASSERT_RESOURCE_ALLOCATION("ELI24624", ipImage != NULL);

				ISpatialPageInfoPtr ipPageInfo = ipLine->GetPageInfo(nPage);
				ASSERT_RESOURCE_ALLOCATION("ELI24625", ipPageInfo != NULL)

				ipBounds->Rotate(ipPageInfo->Width , ipPageInfo->Height, -ipImage->Rotation);

				vector<ILFImageBlockAnnotationPtr> vecExistingRedactions = 
					getRedactionsOnPage(ipPage, mapExistingAnnotations);

				_lastCodePos = "170";

				// Return bSensitiveDataFound = true if any of these attributes were found,
				// regardless if the user has chosen to redact the found type.
				if (ipAttribute->Name == gbstrHCDATA ||
					ipAttribute->Name == gbstrMCDATA ||
					ipAttribute->Name == gbstrLCDATA ||
					ipAttribute->Name == gbstrCLUES)
				{
					bSensitiveDataFound = true;

					if ((m_RepositorySettings.bRedactHCData && ipAttribute->Name == gbstrHCDATA) ||
						(m_RepositorySettings.bRedactMCData && ipAttribute->Name == gbstrMCDATA) ||
						(m_RepositorySettings.bRedactLCData && ipAttribute->Name == gbstrLCDATA))
					{
						_lastCodePos = "180";

						// Apply a linked text & image redaction to the document without duplicating
						// a redaction that is already in vecExistingAnnotations
						addAnnotation(ipPage, ipBounds, true, true, gcolorDEFAULT, 
									  &vecExistingRedactions);
					}
					else if (m_RepositorySettings.bAutoTagForVerify)
					{
						_lastCodePos = "190";

						// Apply an image highlight to the attribute whether it is something we
						// detected as a clue or sensitive data the the user has not configured
						// to redact.
						addAnnotation(ipPage, ipBounds, false, false, gcolorCLUE_HIGHLIGHT);
					}
				}

				_lastCodePos = "200";
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20500");

	return bSensitiveDataFound;
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::verify(CLFItemCollection &rdocumentsToVerify)
{
	// Display the verification toolbar.  The Toolbar will direct the verification process
	CVerifyToolbar::doVerification(this, rdocumentsToVerify);
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::verify(list<CLFItem> &rlistDocumentsToVerify)
{
	// Display the verification toolbar.  The Toolbar will direct the verification process
	CVerifyToolbar::doVerification(this, rlistDocumentsToVerify);
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::cancel(list<CLFItem> &rlistDocuments, 
						 IProgressStatusPtr ipProgressStatus/* = NULL*/)
{
	if (ipProgressStatus != NULL)
	{
		ipProgressStatus->InitProgressStatus("Cancelling...", 0, 1, VARIANT_TRUE);
	}

	// Dispose of all documents
	rlistDocuments.clear();
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::safeDisposeDocuments(list<CLFItem> &rlistDocuments)
{
	try
	{
		// Dispose of all documents
		for each(CLFItem LFItem in rlistDocuments)
		{
			try
			{
				LFItem.unloadItem();
			}
			catch (...) {}  // catch and eat all exception unloading here.
		}

		rlistDocuments.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21018");
}
//--------------------------------------------------------------------------------------------------
UINT CIDShieldLF::runBackgroundMasterThread(LPVOID pData)
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	CIDShieldLF *pIDShieldLF = NULL;

	try
	{
		try
		{
			m_eventServiceStopped.reset();
			m_eventServiceStarted.signal();

			pIDShieldLF = (CIDShieldLF *)pData;
			if (pIDShieldLF == NULL)
			{
				throw UCLIDException("ELI20929", "Internal error: ID Shield engine not found!");
			}

			// Reset stop flag to false.
			m_bStopService = false;

			// Load the search frequency to be used by the background service.
			if (pIDShieldLF->m_apLocalMachineRegSettings->keyExists(
				gstrREG_SERVICE_KEY, gstrREG_SEARCH_INTERVAL))
			{
				string strBackgroundSearchInterval = 
					pIDShieldLF->m_apLocalMachineRegSettings->getKeyValue(
					gstrREG_SERVICE_KEY, gstrREG_SEARCH_INTERVAL, gnMAX_BACKGROUND_SEARCH_INTERVAL_DEFAULT);
				m_nBackgroundSearchInterval = asLong(strBackgroundSearchInterval);
			}

			// Get count of number of worker threads.
			string strThreadCount = "1";
			if (pIDShieldLF->m_apLocalMachineRegSettings->keyExists(gstrREG_SERVICE_KEY, 
																	gstrREG_THREAD_COUNT))
			{
				strThreadCount = pIDShieldLF->m_apLocalMachineRegSettings->getKeyValue(
					gstrREG_SERVICE_KEY, gstrREG_THREAD_COUNT, strThreadCount);
			}

			int nThreadCount = asLong(strThreadCount);
			if (nThreadCount <= 0)
			{
				// Due to mutex protection of LFSO API operations, we won't make the best usage of
				// CPU without creating somewhat more threads than we have cores available.
				nThreadCount = int (1.5 * (double) getNumLogicalProcessors());
			}

			// Loop searches for all documents pending verification then kicks off worker threads
			// to redact the documents.  One loop will last until the worker threads have processed
			// all documents that were found to be pending.
			while (m_bStopService == false)
			{	
				clock_t clkSearchTime = clock();

				m_eventWorkerThreadsDone.reset();
				m_eventWorkerThreadFailed.reset();

				m_apDocumentsToProcess.reset(new CLFItemCollection(pIDShieldLF->m_ipDatabase));
				ASSERT_RESOURCE_ALLOCATION("ELI21866", m_apDocumentsToProcess.get() != NULL);

				m_apDocumentsToProcess->find(NULL,
					pIDShieldLF->m_RepositorySettings.strDocumentSearchType, true, 
					gstrTAG_PENDING_PROCESSING);

				m_apDocumentsToProcess->waitForSearchToComplete();

				if (m_apDocumentsToProcess->getCount() > 0)
				{
					string strMessage = "Application trace: Found " + 
						asString(m_apDocumentsToProcess->getCount()) + " documents to process.";
					UCLIDException ueTrace("ELI21732", strMessage);
					ueTrace.addDebugInfo("Thread count", nThreadCount);
					ueTrace.log();
					
					for (int i = 0; i < nThreadCount; i++)
					{
						AfxBeginThread(runBackgroundWorkerThread, pIDShieldLF);
					}

					// Wait for worker threads to finish processing the documents
					m_eventWorkerThreadsDone.wait();

					UCLIDException ueTrace2("ELI21733",
						"Application trace: Finished processing documents in collection");
					ueTrace2.log();
				}

				// Do not search the database more frequently than m_nBackgroundSearchInterval
				clock_t clkNow = clock();
				while (!m_bStopService && clkSearchTime + m_nBackgroundSearchInterval > clkNow)
				{
					Sleep(CLOCKS_PER_SEC);
					clkNow = clock();
				}
			}

			m_apDocumentsToProcess.reset();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20925");
	}
	catch (UCLIDException &ue)
	{
		// Log any exceptions we caught at the top level of background processing, but throw
		// the exception out to force the service to stop.
		ue.log();

		// Free any active LFItemCollection
		try
		{
			m_apDocumentsToProcess.reset();
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21762");

		// Free the IDShieldLF instance
		try
		{
			if (pIDShieldLF != NULL)
			{
				delete pIDShieldLF;
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21763");

		CoUninitialize();

		// Ensure the entire process ends (including worker threads... if they are running)
		ExitProcess(1);

		// Not sure if this will be called. It is okay if it is not since we have already logged the exception.
		throw ue;
	}

	// Signal that the background thread has finished and can be stopped.
	m_eventServiceStarted.reset();
	m_eventServiceStopped.signal();

	CoUninitialize();

	return 0;
}
//--------------------------------------------------------------------------------------------------
UINT CIDShieldLF::runBackgroundWorkerThread(LPVOID pData)
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	CIDShieldLF *pIDShieldLF = NULL;

	INIT_EXCEPTION_AND_TRACING("MLI00876");

	try
	{
		try
		{
			_lastCodePos = "10";

			pIDShieldLF = (CIDShieldLF *)pData;
			if (pIDShieldLF == NULL)
			{
				throw UCLIDException("ELI20930", "Internal error: ID Shield engine not found!");
			}

			if (m_apDocumentsToProcess.get() == NULL)
			{
				throw UCLIDException("ELI21295", 
					"Internal error: Repository item collection not initialized!");
			}

			InterlockedIncrement(&m_nRunningThreads);

			_lastCodePos = "20";

			// Loop pulls one document off the set of documents to be redacted in the background
			// and repeats until all documents in the set have been processed
			ILFDocumentPtr ipDocument = m_apDocumentsToProcess->getNextItem();
			while (m_bStopService == false && 
				   !m_eventWorkerThreadFailed.isSignaled() &&
				   ipDocument != NULL)
			{
				CSingleLock lock(&m_mutexLFOperations, TRUE);

				_lastCodePos = "30";
				try
				{
					try
					{
						_lastCodePos = "40";
						// Check permissions and locks before attempting to process.
						if (checkCanRedact(ipDocument))
						{
							_lastCodePos = "50";
							
							// Release m_mutexLFOperations so that the AF engine can run multi-threaded
							// and so that we don't deadlock when processDocument takes out a lock
							// against this same mutex.
							lock.Unlock();

							pIDShieldLF->processDocument(ipDocument, false);

							// Lock m_mutexLFOperations again as we dispose of the Laserfiche document
							lock.Lock();
						}

						_lastCodePos = "60";

						// A failure in processDocument can result in ipDocument being disposed
						// of and set to NULL
						if (ipDocument != NULL)
						{
							_lastCodePos = "70";
							ipDocument->Dispose();
						}
						_lastCodePos = "80";
					}
					CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20924");
				}
				catch (UCLIDException &ue)
				{
					safeDispose(ipDocument);

					ue.log();
				}

				// Unlock here to prevent any possiblity of deadlock in getNextItem
				lock.Unlock();

				ipDocument = m_apDocumentsToProcess->getNextItem();
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20926");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException uexOuter("ELI21764", "Worker thread failed!", ue);
		uexOuter.log();

		m_eventWorkerThreadFailed.signal();
	}

	// Indicate that this thread is done processing and signal the master thread if this was
	// the last thread running.
	InterlockedDecrement(&m_nRunningThreads);
	if (m_nRunningThreads == 0)
	{
		m_eventWorkerThreadsDone.signal();
	}

	CoUninitialize();

	return 0;
}
//--------------------------------------------------------------------------------------------------
bool CIDShieldLF::checkCanRedact(ILFDocumentPtr ipDocument)
{
	ASSERT_ARGUMENT("ELI20933", ipDocument != NULL);

	try
	{
		verifyHasRight(ipDocument, ACCESS_READ);
		verifyHasRight(ipDocument, ACCESS_ANNOTATE);
		verifyHasRight(ipDocument, ACCESS_WRITE_METADATA);

		if (hasTag(ipDocument, gstrTAG_PENDING_PROCESSING) == false)
		{
			return false;
		}

		if (ipDocument->FindHasImagePages() == VARIANT_FALSE)
		{
			return false;
		}

		ipDocument->LockObject(LOCK_TYPE_WRITE);
	}
	catch (...)
	{
		return false;
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
vector<ILFImageBlockAnnotationPtr> CIDShieldLF::getRedactionsOnPage(ILFPagePtr ipPage, 
							map<long long, vector<ILFImageBlockAnnotationPtr> > &rmapDocumentCache)
{
	ASSERT_ARGUMENT("ELI21492", ipPage != NULL);

	vector<ILFImageBlockAnnotationPtr> vecRedactions;

	// Check to see if this page's redactions are in the provided cache.
	if (rmapDocumentCache.find(ipPage->ID) != rmapDocumentCache.end())
	{
		// This page's redactions are already cached.  Return the cached vector.
		vecRedactions = rmapDocumentCache[ipPage->ID];
	}
	else
	{
		// Populate vecRedactions with the page's redactions
		long nRedactionCount = ipPage->ImageBlackoutCount;
		for (long i = 1; i <= nRedactionCount; i++)
		{
			ILFImageBlockAnnotationPtr ipImageRedaction = ipPage->ImageBlackout[i];
			ASSERT_RESOURCE_ALLOCATION("ELI21491", ipImageRedaction != NULL);

			vecRedactions.push_back(ipImageRedaction);
		}

		// Add the vector to the cache so it can be retrieve later on without accessing the page.
		rmapDocumentCache[ipPage->ID] = vecRedactions;
	}

	return vecRedactions;
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::disconnect()
{
	try
	{
		// Don't close, only hide the wait dialog.  This helps ensure exceptions are displayed as
		// the foreground window.
		if (m_apDlgWait.get() != NULL)
		{
			m_apDlgWait->hide();
		}

		// Dispose of Laserfiche objects
		if (m_ipSettingsFile != NULL)
		{
			m_ipSettingsFile->Dispose();  
			m_ipSettingsFile = NULL;
		}

		if (m_ipSettingsFolder != NULL)
		{
			m_ipSettingsFolder->Dispose();
			m_ipSettingsFolder = NULL;
		}

		// Terminate the connection unless we attached to the open connection of a client, in which case
		// we want the user to be able to continue using that connection.
		if (m_ipConnection != NULL)
		{
			if (!m_bAttachedToClient)
			{
				m_ipConnection->Terminate();
			}
			m_ipConnection = NULL;
		}
		
		m_bAttachedToClient = false;
		m_hwndClient = NULL;

		m_ipDatabase = NULL;
		m_ipClient = NULL;

		m_apSettingsMgr.reset();
		m_apLocalSettingsFile.reset();
		m_apCurrentUserRegSettings.reset();
		m_apLocalMachineRegSettings.reset();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20844");
}
//--------------------------------------------------------------------------------------------------
ByteStream &CIDShieldLF::getPasswordKey()
{
	static ByteStream bytesPasswordKey;
	if (bytesPasswordKey.getLength() == 0)
	{
		ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, bytesPasswordKey);

		bsm << gulPasswordKey0;
		bsm << gulPasswordKey1;
		bsm << gulPasswordKey2;
		bsm << gulPasswordKey3;
		bsm.flushToByteStream(8);
	}

	return bytesPasswordKey;
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::encryptPassword(string &rstrPassword)
{
	ByteStream bytesPassword;
	ByteStreamManipulator bsmPassword(ByteStreamManipulator::kWrite, bytesPassword);
	bsmPassword << rstrPassword;
	bsmPassword.flushToByteStream(8);

	EncryptionEngine ee;
	ByteStream bytesEncrypted;
	ee.encrypt(bytesEncrypted, bytesPassword, getPasswordKey());
	rstrPassword = bytesEncrypted.asString();
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::decryptPassword(string &rstrPassword)
{
	EncryptionEngine ee;
	ByteStream bytesPassword;
	ee.decrypt(bytesPassword, ByteStream(rstrPassword), getPasswordKey());

	ByteStreamManipulator bsmPassword(ByteStreamManipulator::kRead, bytesPassword);
	bsmPassword >> rstrPassword;
}
//--------------------------------------------------------------------------------------------------
void CIDShieldLF::validateLicense(EConnectionMode eConnectionMode)
{
	// Verify the license against IDs that correspond to specified eConnectionMode.
	switch (eConnectionMode)
	{
		case kService:
			{
			VALIDATE_LICENSE(gnLASERFICHE_SERVICE_REDACTION, 
				"ELI20719", "ID Shield for Laserfiche Background Redaction");
			}
			break;

		case kAdministrator:
		case kProcess:
			{
				VALIDATE_LICENSE(gnLASERFICHE_DESKTOP_REDACTION, 
					"ELI21838", "ID Shield for Laserfiche Desktop Redaction");
			}
			break;

		// [FlexIDSIntegrations:138] Connection type kSubmit should only require
		// a verification license (and not a desktop license)
		case kVerify:
		case kSubmit:
		case kConnect:
			{
				VALIDATE_LICENSE(gnLASERFICHE_VERIFICATION, 
					"ELI21848", "ID Shield for Laserfiche Verification");
			}
			break;
		case kDisconnected:
		default:
			{
				// In all cases we need to know which LicenseID to validate. If eConnectionMode
				// is not determinant, this is a problem.
				THROW_LOGIC_ERROR_EXCEPTION("ELI21843");
			}
	}
}
//--------------------------------------------------------------------------------------------------
bool CIDShieldLF::GetLoginInfoFrom80(string &rstrServer, string &rstrRepository, string &rstrUser)
{
	//Retrieve information about the client connection using the specified namespace's interfaces.
	LFSO80Lib::ILFDatabasePtr ipDatabase = m_ipClient->GetDatabase();
	if (ipDatabase == NULL)
	{
		// [FlexIDSIntegrations:114] The ILFClient inteface seems picky concerning having focus
		// to work. If we were not able to get the database, setting focus to the Client and
		// then try again.
		SetFocus(m_hwndClient);
		
		ipDatabase = m_ipClient->GetDatabase();

		if (ipDatabase == NULL)
		{
			return false;
		}
	}

	LFSO80Lib::ILFServerPtr ipServer = ipDatabase->Server;
	ASSERT_RESOURCE_ALLOCATION("ELI21894", ipServer != NULL);

	LFSO80Lib::ILFConnectionPtr ipConnection = ipDatabase->CurrentConnection;
	ASSERT_RESOURCE_ALLOCATION("ELI21895", ipConnection != NULL);

	rstrServer = asString(ipServer->Name);
	rstrRepository = asString(ipDatabase->Name);
	rstrUser = asString(ipConnection->UserName);

	return true;
}
//--------------------------------------------------------------------------------------------------
bool CIDShieldLF::GetLoginInfoFrom81(string &rstrServer, string &rstrRepository, string &rstrUser)
{
	//Retrieve information about the client connection using the specified namespace's interfaces.
	LFSO81Lib::ILFDatabasePtr ipDatabase = m_ipClient->GetDatabase();
	if (ipDatabase == NULL)
	{
		// [FlexIDSIntegrations:114] The ILFClient inteface seems picky concerning having focus
		// to work. If we were not able to get the database, setting focus to the Client and
		// then try again.
		SetFocus(m_hwndClient);
		
		ipDatabase = m_ipClient->GetDatabase();

		if (ipDatabase == NULL)
		{
			return false;
		}
	}

	LFSO81Lib::ILFServerPtr ipServer = ipDatabase->Server;
	ASSERT_RESOURCE_ALLOCATION("ELI31064", ipServer != NULL);

	LFSO81Lib::ILFConnectionPtr ipConnection = ipDatabase->CurrentConnection;
	ASSERT_RESOURCE_ALLOCATION("ELI31065", ipConnection != NULL);

	rstrServer = asString(ipServer->Name);
	rstrRepository = asString(ipDatabase->Name);
	rstrUser = asString(ipConnection->UserName);

	return true;
}