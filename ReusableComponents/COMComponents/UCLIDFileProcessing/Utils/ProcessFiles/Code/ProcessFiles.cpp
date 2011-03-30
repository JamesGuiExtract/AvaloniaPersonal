// ProcessFiles.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ProcessFiles.h"

#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <RegistryPersistenceMgr.h>
#include <Win32Util.h>
#include <FailureDetectionAndReportingMgr.h>
#include <comutils.h>
#include <FileRecoveryManager.h>
#include <ComponentLicenseIDs.h>
#include <memory>


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CProcessFilesApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CProcessFilesApp, CWinApp)
	//{{AFX_MSG_MAP(CProcessFilesApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CProcessFilesApp construction
//-------------------------------------------------------------------------------------------------
CProcessFilesApp::CProcessFilesApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CProcessFilesApp object
//-------------------------------------------------------------------------------------------------
CProcessFilesApp theApp;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const char *gpszFPSFileDescription = "Extract Systems FPS File";
const char *gpszFPSFileExtension = ".fps";
const string gstrRECOVERY_PROMPT =
	"It appears that there are unsaved changes from a previously open File Action Manager.  "
	"This can happen because of an application crash or other error.  Would you like to attempt "
	"recovering the File Action Manager settings from your previous session?";

//-------------------------------------------------------------------------------------------------
// CProcessFilesApp initialization
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage =	"Usage: ProcessFiles.exe [OPTIONS]\n"
						"OPTIONS:\n"
						"/r - register .fps files to open by default with ProcessFiles.exe then exit\n"
						"/u - unregister .fps files to not open with ProcessFiles.exe then exit\n"
						"/sd <Server name> <Database name> - sets inital server and database in FAM to <Server Name>\n"
						"\tand <Database name>. This option has to be the only option specified.\n"
						"/? - display usage information\n"
						"<filename> [/s][/c][/fc][/m:<nnn>][/d <Directory Name>][/l<List File Name>][/service][/sleep:<ms>]\n"
						"\t\topen ProcessFiles.exe with the specified file\n"
						"\t/s - automatically start processing with the settings from <filename>\n"
						"\t/c - automatically close the dialog after a processing batch successfully \n"
						"\t\t(No files fail) completes\n"
						"\t/m:<nnn> - will process <nnn> documents and stop\n"
						"\t/fc - automatically close the dialog after a processing batch with or without failures\n"
						"\t/d <Directory Name> - changes the scope to folder with the given directory name\n"
						"\t\tthis option requires a .fps file with exactly one 'Files from folder' file supplier\n"
						"\t\tthis option can not be specified with the /l option\n"
						"\t\tIt is recommended that the /service switch is used with this option\n"
						"\t\toverriding folder specified in <filename>\n"
						"\t/l <ListFileName> - loads list of files to process from <ListFileName>\n"
						"\t\tthis option requires a .fps file with exactly one 'Files from dynamic list' file supplier\n"
						"\t\tthis option cannot be specified with the /d option\n"
						"\t\tIt is recommended that the /service switch is used with this option\n"
						"\t/service - disables the autosave of the FPS file, will close when processing completes\n"
						"\t\tand will stop processing and close if it receives a close message\n"
						"\t/sleep:<ms> - ProcessFiles.exe will wait <ms> milliseconds before launching";
	AfxMessageBox(strUsage.c_str());
}
//-------------------------------------------------------------------------------------------------
IFileSupplierPtr getFileSupplier(IFileSupplyingMgmtRolePtr ipSuppyingRole )
{
	ASSERT_ARGUMENT("ELI15393", ipSuppyingRole != __nullptr );

	// The supplierMgmt role must have a folder FS in it to work
	//Get the list of file suppliers
	IIUnknownVectorPtr ipSuppliers = ipSuppyingRole->FileSuppliers;
	ASSERT_RESOURCE_ALLOCATION("ELI15384", ipSuppliers != __nullptr );

	long nSuppliers = ipSuppliers->Size();
	if ( nSuppliers != 1 )
	{
		UCLIDException ue("ELI15385", "There must be only one supplier defined in the FPS file.");
		ue.addDebugInfo("# Suppliers", nSuppliers );
		throw ue;
	}

	IFileSupplierDataPtr ipSupplierData = ipSuppliers->At(0);
	ASSERT_RESOURCE_ALLOCATION("ELI15391", ipSupplierData != __nullptr );

	// Get the file supplier in the list
	IObjectWithDescriptionPtr ipObjFS = ipSupplierData->FileSupplier;
	ASSERT_RESOURCE_ALLOCATION("ELI15394", ipObjFS != __nullptr );

	return ipObjFS->GetObjectA();
}
//-------------------------------------------------------------------------------------------------
void setupFolderSupplier ( IFileProcessingManagerPtr ipFPM, std::string strFolder )
{
	ASSERT_ARGUMENT("ELI15381", ipFPM != __nullptr );
	
	// Get the supplying management role
	IFileSupplyingMgmtRolePtr ipSupplierRole = ipFPM->FileSupplyingMgmtRole;
	ASSERT_RESOURCE_ALLOCATION("ELI15382", ipSupplierRole != __nullptr );

	// Get the file supplier in the list
	IFolderFSPtr ipFolderFS = getFileSupplier(ipSupplierRole);

	if ( ipFolderFS == __nullptr )
	{
		UCLIDException ue("ELI15383", "There must be only a 'Files from folder' file supplier defined in the FPS file.");
		throw ue;
	}

	// Reset the Folder to the command line switc folder
	ipFolderFS->FolderName = strFolder.c_str();
}
//-------------------------------------------------------------------------------------------------
void setupDynamicListSupplier(IFileProcessingManagerPtr ipFPM, std::string strFileName )
{
	ASSERT_ARGUMENT("ELI15386", ipFPM != __nullptr );
	
	// Get the supplying management role
	IFileSupplyingMgmtRolePtr ipSupplierRole = ipFPM->FileSupplyingMgmtRole;
	ASSERT_RESOURCE_ALLOCATION("ELI15387", ipSupplierRole != __nullptr );

	// Get the file supplier in the list
	IDynamicFileListFSPtr ipDynamicFS = getFileSupplier(ipSupplierRole);

	if ( ipDynamicFS == __nullptr )
	{
		UCLIDException ue("ELI15390", "There must be only a 'Files from dynamic list' file supplier defined in the FPS file.");
		throw ue;
	}

	// Reset the Filename to the command line switch filelist
	ipDynamicFS->FileName = strFileName.c_str();
}
//-------------------------------------------------------------------------------------------------
BOOL CProcessFilesApp::InitInstance()
{
	// Apartment threading is being used to support certain .Net UI elements [DNRCAU #328]
	CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);

	bool bAbnormalExit = false;

	try
	{
		try
		{
			// Set up the exception handling aspect.
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler(&exceptionDlg);

			// Every time this application starts, re-register the file
			// associations if the file associations don't exist.  If the
			// file associations already exist, then do nothing.
			// This way, registration will happen the very first time
			// the user runs this application (even if the installation
			// program's call to this application with /r argument failed).
			// NOTE: the registration is not forced because we are passing
			// "true" for bSkipIfKeysExist.
			registerFileAssociations(gpszFPSFileExtension, gpszFPSFileDescription, 
				getAppFullPath(), true);

			// if appropriate command line arguments have been provided
			// register or unregister FPS file related settings
			// or display usage
			// as appropriate, and return
			if (__argc == 2)
			{
				if (_stricmp(__argv[1], "/r") == 0)
				{
					// force registration of file associations because
					// the /r argument was specifically provided
					// NOTE: the registration is forced by passing "false" for
					// bSkipIfKeysExist
					registerFileAssociations(gpszFPSFileExtension, 
						gpszFPSFileDescription, getAppFullPath(), false);
					return FALSE;
				}
				else if (_stricmp(__argv[1], "/u") == 0)
				{
					// unregister settings and return.
					unregisterFileAssociations(gpszFPSFileExtension,
						gpszFPSFileDescription);
					return FALSE;
				}
				else if (_stricmp(__argv[1], "/?") == 0)
				{
					// display usage and return.
					usage();
					return FALSE;
				}
			}

			string strFileName("");
			string strDirName("");
			string strFileListName("");
			string strServer("");
			string strDatabase("");
			bool bRunOnInit = false;
			bool bCloseOnComplete = false;
			bool bForceCloseOnComplete = false;
			bool bRunningAsService = false;
			int iExecuteCount = 0;
			int iSleepTime = 0;

			if (__argc >= 2)
			{
				// Check for the /sd switch
				if (_stricmp(__argv[1], "/sd") == 0)
				{
					// This is the only switch allowed
					if (__argc != 4)
					{
						usage();
						return FALSE;
					}
					strServer = __argv[2];
					strDatabase = __argv[3];
				}
				else
				{
					// Build the absolute path to the file name so that
					// it will be loaded with the fully qualified path name
					strFileName =  buildAbsolutePath(__argv[1]);

					int i;
					for (i = 2; i < __argc; i++)
					{
						// Check for /s - Auto start switch
						if (_stricmp(__argv[i], "/s") == 0)
						{
							bRunOnInit = true;
						}
						// Check for /c - Close on complete switch
						else if (_stricmp(__argv[i], "/c") == 0)
						{
							// as per [p13 #4858] if
							// the user has already specified /fc
							// then display usage and exit
							if (bForceCloseOnComplete)
							{
								// display usage and return
								usage();
								return FALSE;
							}
							bCloseOnComplete = true;
						}
						// Check for /fc - force close switch
						else if ( _stricmp(__argv[i], "/fc") == 0)
						{
							// as per [p13 #4858] if
							// the user has already specified /c
							// then display usage and exit
							if (bCloseOnComplete)
							{
								// display usage and return
								usage();
								return FALSE;
							}
							bForceCloseOnComplete = true;
						}
						// Check for /d <Folder Name> switch
						else if ( _stricmp(__argv[i], "/d") == 0 )
						{
							// Must have folder name following this switch
							if ( i+1 < __argc )
							{
								i++;
								strDirName = __argv[i];

								// Check that the folder does exist
								if ( !isValidFolder(strDirName) )
								{
									usage();
									return FALSE;
								}
							}
							else
							{
								// there is a missing argument
								usage();
								return FALSE;
							}
						}
						// Check for the /m:<nnn> switch
						else if ( _strnicmp( __argv[i], "/m:", 3) == 0)
						{					
							string strTmp = __argv[i];

							// Extract the number
							iExecuteCount = atoi(strTmp.substr(3).c_str());
						}
						// Check for the /l <FileListFile> - list file
						else if ( _stricmp(__argv[i], "/l") == 0 )
						{
							// Must have the name of the file list file
							if ( i+1 < __argc )
							{
								i++;
								strFileListName = __argv[i];

								// Make sure the file exists
								if ( !isValidFile(strFileListName) )
								{
									usage();
									return FALSE;
								}
							}
							else
							{
								// there is a missing argument
								usage();
								return FALSE;
							}
						}
						// Check for the service switch
						else if ( _stricmp(__argv[i], "/service") == 0 )
						{
							bRunningAsService = true;
						}
						// [P13:4821] Check for sleep time specification
						else if ( _strnicmp( __argv[i], "/sleep:", 7) == 0)
						{
							string strSleepTime = __argv[i];
							// Retrieve the portion of the argument following "/sleep:"
							strSleepTime = strSleepTime.substr(7);
							
							try
							{
								try
								{
									// Attempt to convert the string to an int.
									iSleepTime = asLong(strSleepTime);

									// Ensure a positive value was specified
									if (iSleepTime < 0)
									{
										UCLIDException ue("ELI20318", "Sleep time cannot be negative!");
										ue.addDebugInfo("Specified value", strSleepTime);
										throw ue;
									}
								}
								CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20316");
							}
							catch (UCLIDException &ue)
							{
								// Indicate a bad sleep value was specified
								UCLIDException uexOuter("ELI20317","Invalid sleep value specified!", ue);
								uexOuter.display();
								usage();
								return FALSE;
							}
						}
						else
						{
							usage();
							return FALSE;
						}
					}
				}
			}

			// There should be no more than 10 arguments
			// and if there are 2 or more and the /sd was not specified at this point there must be a valid
			// FPS file specified
			if (__argc > 10 || ((__argc >= 2) && strServer.empty() && !isValidFile(strFileName)))
			{
				usage();
				return FALSE;
			}

			// /l and /d cannot be specified the same time
			if ( strDirName != "" && strFileListName != "" )
			{
				usage();
				return FALSE;
			}

			// Wait the specified amount of time before allowing the application to commence
			if (iSleepTime > 0)
			{
				Sleep(iSleepTime);
			}

			// notify the Failure Detection and Reporting System that this
			// application has started running
			FailureDetectionAndReportingMgr::notifyApplicationRunning();

			// start the FDRS ping thread
			FailureDetectionAndReportingMgr::startFDRSPingThread();

			// Load license file(s)
			LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
			validateLicense();

			IFileProcessingManagerPtr ipFileProcMgr(CLSID_FileProcessingManager);
			ASSERT_RESOURCE_ALLOCATION("ELI08894", ipFileProcMgr != __nullptr);

			// Create an FileRecoveryManager object
			unique_ptr<FileRecoveryManager> apFRM(__nullptr);

			if ( !bRunningAsService )
			{
				// setup the FileRecoveryManager
				apFRM.reset(new FileRecoveryManager(".tmp"));

				// if a file Processing recovery file exists, then ask the user if they
				// want to recover FAM settings
				string strRecoveryFileName;
				if (apFRM->recoveryFileExists(strRecoveryFileName))
				{
					int iResult = MessageBox(NULL, gstrRECOVERY_PROMPT.c_str(), "Recovery", 
						MB_YESNO|MB_ICONQUESTION);

					if (iResult == IDYES)
					{
						// load the FAM settings from the FAM recovery file
						// NOTE: We are setting the bSetDirtyFlagToTrue flag to VARIANT_TRUE
						// so that the user is prompted for saving when they try
						// to close the FAM window
						ipFileProcMgr->LoadFrom(strRecoveryFileName.c_str(), VARIANT_TRUE);
					}
					else if ( strFileName != "" )
					{
						// load the FAM settings from the specified file
						ipFileProcMgr->LoadFrom(_bstr_t(strFileName.c_str()), VARIANT_FALSE);
					}

					// at this point, regardless of whether the user decided to
					// recover the file or not, the recovery file should be deleted.
					apFRM->deleteRecoveryFile(strRecoveryFileName);
				}
				else if ( strFileName != "" )
				{
					// load the FAM settings from the specified file
					ipFileProcMgr->LoadFrom(_bstr_t(strFileName.c_str()), VARIANT_FALSE);
				}
			}
			else if (strFileName != "")
			{
				// load the FAM settings from the specified file
				ipFileProcMgr->LoadFrom(_bstr_t(strFileName.c_str()), VARIANT_FALSE);
			}

			// if the directory name is not empty need to change the folder FS to this directory
			if ( strDirName != "" )
			{
				// set up the folder file supplier
				::setupFolderSupplier(ipFileProcMgr, strDirName );
			}

			// if the file list name is not empty need to change the DynamicFileListFS to this file
			if ( strFileListName != "" )
			{
				// set up the Dynamic File supplier
				::setupDynamicListSupplier(ipFileProcMgr, strFileListName );
			}

			// if the server is not empty it was specified on the command line
			if (!strServer.empty())
			{
				ipFileProcMgr->DatabaseServer = strServer.c_str();
				ipFileProcMgr->DatabaseName = strDatabase.c_str();
			}

			// Show the UI
			ipFileProcMgr->ShowUI( bRunOnInit ? VARIANT_TRUE : VARIANT_FALSE, 
				bCloseOnComplete ? VARIANT_TRUE : VARIANT_FALSE, bForceCloseOnComplete 
				? VARIANT_TRUE : VARIANT_FALSE, iExecuteCount, apFRM.get() );

			// notify the FDRS that the application exited normally
			FailureDetectionAndReportingMgr::notifyApplicationNormallyExited();

			// Stop FDRS Ping Thread
			FailureDetectionAndReportingMgr::stopFDRSPingThread();
		}
		catch (...)
		{
			// set the flag indicating that we are exiting abnormally
			bAbnormalExit = true;

			// rethrow the exception
			throw;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08895");

	// we are notifying the FDRS here that the application exited abnormally
	// because all displayed exceptions are notified to the FDRS, and we
	// want the exception-displayed event to appear before the abnormal-exit event
	if (bAbnormalExit)
	{
		// notify the FDRS that the application exited abnormally
		FailureDetectionAndReportingMgr::notifyApplicationAbnormallyExited();

		// Stop FDRS Ping Thread
		FailureDetectionAndReportingMgr::stopFDRSPingThread();
	}

	// Work around for [P13 #4766]
	//CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}

//-------------------------------------------------------------------------------------------------
// Private Functions
//-------------------------------------------------------------------------------------------------
void CProcessFilesApp::validateLicense()
{
	static const unsigned long PROCESS_FILES_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( PROCESS_FILES_COMPONENT_ID, "ELI10663", "Process Files" );
}
//-------------------------------------------------------------------------------------------------
