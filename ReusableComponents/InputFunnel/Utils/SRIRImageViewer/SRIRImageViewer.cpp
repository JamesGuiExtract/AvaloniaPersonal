// SRIRImageViewer.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "SRIRImageViewer.h"
#include "SRIRImageViewerDlg.h"

#include <Win32GlobalAtom.h>
#include <Win32Util.h>
#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <IConfigurationSettingsPersistenceMgr.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <SRIRConstants.h>

#include <io.h>
#include <string>
#include <vector>
#include <map>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

CComModule _Module;
const char *gpszTIFFileDescription = "TIF Image File";
const char *gpszTIFFileExtension = ".tif";
const char *gpszLoadImageMsgName = "UCLIDSRIRLoadNewImageMessage";
const char *gpszExecScriptMsgName = "UCLIDSRIRExecScriptMessage";
const char *gpszCloseViewerMsgName = "UCLIDSRIRCloseViewerMessage";
const char *gpszOCRToFileMsgName = "UCLIDSRIROCRToFileMessage";
const char *gpszOCRToClipboardMsgName = "UCLIDSRIROCRToClipboard";
const char *gpszOCRToMessageBoxMsgName = "UCLIDSRIROCRToMessageBoxMessage";

const long gnMaxWindowNameSize = 512; 

static const std::string gstrCmdLineRegister = "/r";
static const std::string gstrCmdLineUnRegister = "/u";
static const std::string gstrCmdLineDisplaySwipedTextInMessageBox = "/m";
static const std::string gstrCmdLineWriteSwipedTextToFile = "/o";
static const std::string gstrCmdLineWriteSwipedTextToClipboard = "/c";
static const std::string gstrCmdLineDisplaySearchDialog = "/s";
static const std::string gstrCmdLineReuseWindow = "/l";
static const std::string gstrCmdLineExecScript = "/e";
static const std::string gstrCmdLineCloseAll = "/closeall";
static const std::string gstrCmdLineHelp = "/?";

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CSRIRImageViewerApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSRIRImageViewerApp, CWinApp)
	//{{AFX_MSG_MAP(CSRIRImageViewerApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CSRIRImageViewerApp construction
//-------------------------------------------------------------------------------------------------
CSRIRImageViewerApp::CSRIRImageViewerApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CSRIRImageViewerApp object
//-------------------------------------------------------------------------------------------------
CSRIRImageViewerApp theApp;
//-------------------------------------------------------------------------------------------------
// CSRIRImageViewerApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CSRIRImageViewerApp::InitInstance()
{
	try
	{
		AfxEnableControlContainer();

		// Every time this application starts, re-register the file
		// associations if the file associations don't exist.  If the
		// file associations already exist, then do nothing.
		// This way, registration will happen the very first time
		// the user runs this application (even if the installation
		// program's call to this application with /r argument failed).
		// NOTE: the registration is not forced because we are passing
		// "true" for bSkipIfKeysExist.
		registerFileAssociations(gpszTIFFileExtension, gpszTIFFileDescription, 
			getAppFullPath(), true);

		// Use the exception dialog
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler( &exceptionDlg );

		// default to writing text to the clipboard
		EOCRTextHandlingType eOCRTextHandlingType = kNone;
		// This is only used for the /o option
		string strOCRTextFile = "";

		bool bDisplaySearch = false;
		bool bReuse = false;
		bool bExecScript = false;
		string strScriptFileName = "";
		
		bool bCloseAll = false;

		bool bFileNameSpecified = false;
		string strFileName = "";

		m_bWindowCreated = false;

		// some applications (like Outlook!) pass the image name argument
		// without surrounding quotes when trying to launch this application
		// So, we get multiple arguments passed to us when a file like
		// C:\Program Files\My App\Temp.jpg
		// is passed to our application
		// To deal with this problem, we will try to put all the arguments
		// together into a string, seperated with spaces.  If the resulting
		// string is a valid filename, then lets assume that the surrounding
		// quotes were ommitted by the calling application.
		
		// compute the concatenation of all arguments
		string strTemp;
		int i;
		for (i = 1; i < __argc; i++)
		{
			// add a space if needed
			if (!strTemp.empty())
			{
				strTemp += " ";
			}

			// append
			strTemp += __argv[i];
		}

		// if the args all put together represent a valid filename,
		// then assume that the caller just wants to open the file
		if (isValidFile(strTemp))
		{
			bFileNameSpecified = true;
			strFileName = strTemp;
		}
		else
		{
			// if appropriate command line arguments have been provided
			// register or unregister TIF file related settings
			// as appropriate, and return
			for (i = 1; i < __argc; i++)
			{
				string strArg = __argv[i];
				if (strArg == gstrCmdLineRegister)
				{
					// force registration of file associations because
					// the /r argument was specifically provided
					// NOTE: the registration is forced by passing "false" for
					// bSkipIfKeysExist
					registerFileAssociations(gpszTIFFileExtension, 
						gpszTIFFileDescription, getAppFullPath(), false);
					return FALSE;
				}
				else if (strArg == gstrCmdLineUnRegister)
				{
					// unregister settings and return.
					unregisterFileAssociations(gpszTIFFileExtension,
						gpszTIFFileDescription);
					return FALSE;
				}
				else if (strArg == gstrCmdLineDisplaySwipedTextInMessageBox)
				{
					// Use the MessageBox PTH
					eOCRTextHandlingType = kDisplayOCRTextInMessageBox;
				}
				else if (strArg == gstrCmdLineWriteSwipedTextToFile)
				{
					// Use the MessageBox PTH
					eOCRTextHandlingType = kWriteOCRTextToFile;
					if (i == __argc - 1)
					{
						UCLIDException ue("ELI11863", "The /o option requires an additional argument.");
						throw ue;
					}
					i++;
					strOCRTextFile = __argv[i];
				}
				else if (strArg == gstrCmdLineWriteSwipedTextToClipboard)
				{
					// Use the Clipboard PTH
					eOCRTextHandlingType = kWriteOCRTextToClipboard;
				}
				else if (strArg == gstrCmdLineDisplaySearchDialog)
				{
					// Display the search window
					bDisplaySearch = true;
				}
				else if (strArg == gstrCmdLineReuseWindow)
				{
					// Reuse a current window if one can be found
					bReuse = true;
				}
				else if (strArg == gstrCmdLineExecScript)
				{
					// Reuse a current window if one can be found
					bExecScript = true;
					if (i == __argc - 1)
					{
						UCLIDException ue("ELI11858", "The /e option requires an additional argument.");
						throw ue;
					}
					i++;
					strScriptFileName = __argv[i];
				}
				else if (strArg == gstrCmdLineCloseAll)
				{
					bCloseAll = true;
				}
				else if (_strcmpi(__argv[i], "/?") == 0)
				{
					AfxMessageBox(getUsage().c_str());
					return FALSE;
				}
				else
				{
					// if a filename is specified it must be the last argument
					if (i != __argc-1)
					{
						UCLIDException ue("ELI11844", "Invalid Argument!");
						ue.addDebugInfo("Arg", __argv[i]);
						throw ue;
						
					}
					//bFileNameSpecified = true;
					strFileName = __argv[i];

					//Has to be at least 5 chars long (a.tif is shortest valid filename) 
					//This also checks for out of bounds for the -3
					if(strFileName.size() > 4)
					{
						if(strFileName[strFileName.size()-3] != '.')
						{
							if(! isFileOrFolderValid(strFileName))
							{
								//We know that it isnt a valid file name
								UCLIDException ue("ELI13575", "File not found!");
								ue.addDebugInfo("Entered filename:", strFileName);
								throw ue;					
							}
							else
							{	
								bFileNameSpecified = true;
							}
						}
						else
						{
							//Otherwise it is a long argument (/ab or the like)
							UCLIDException ue("ELI13576", "Invalid Argument!");
							ue.addDebugInfo("Argument was:", strFileName);
							throw ue;
						}
					}
					else
					{
						UCLIDException ue("ELI13586", "Invalid Argument!");
						ue.addDebugInfo("Argument was:", strFileName);
						throw ue;
					}
				}
			}
		}

		// Here we register with windows the message that we will use to 
		// communicate between instances of the SRIRImageViewer
		// this is necessary for the /l option
		m_uiMsgLoadImage = registerMessage(gpszLoadImageMsgName);
		m_uiMsgExecScript = registerMessage(gpszExecScriptMsgName);
		m_uiMsgCloseViewer = registerMessage(gpszCloseViewerMsgName);
		m_uiMsgOCRToFile = registerMessage(gpszOCRToFileMsgName);
		m_uiMsgOCRToClipboard = registerMessage(gpszOCRToClipboardMsgName);
		m_uiMsgOCRToMessageBox = registerMessage(gpszOCRToMessageBoxMsgName);

		bool bCreateAndUse = true;

		// if the /l option has been specified
		if (bReuse)
		{
			EnumWindows(enumSRIRImageWindows, (LPARAM)this);
			if (m_vecWindowHandles.size() > 0)
			{
				bCreateAndUse = false;
			}
		}

		if (bCreateAndUse)
		{
			// initialize COM
			//CoInitializeEx(NULL, COINIT_MULTITHREADED);
			// This is being used instead of multithreaded version because
			// This app uses the Spot Recognition Window that uses an OCX
			// that will not work with the multithreaded option
			CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);

			// initialize license
			LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

			// These must be called in order
			// create the spot recognition window
			ISpotRecognitionWindowPtr m_ipSRIR;
			m_ipSRIR.CreateInstance(CLSID_SpotRecognitionWindow);
			ASSERT_RESOURCE_ALLOCATION("ELI06300", m_ipSRIR != NULL);

			// Cast the SRIR to an InputReceiver
			IInputReceiverPtr m_ipInputReceiver = m_ipSRIR;
			ASSERT_RESOURCE_ALLOCATION("ELI06301", m_ipSRIR != NULL);
			// enable text input
			m_ipInputReceiver->EnableInput("Text", "");
			// show the spot recognition window
			m_ipInputReceiver->ShowWindow(VARIANT_TRUE);

			// create an instance of the OCR engine
			IOCREnginePtr m_ipOCREngine;
			m_ipOCREngine.CreateInstance(CLSID_ScansoftOCR);
			ASSERT_RESOURCE_ALLOCATION("ELI06305", m_ipOCREngine != NULL);

			// initialize the private license
			IPrivateLicensedComponentPtr ipScansoftEngine = m_ipOCREngine;
			ASSERT_RESOURCE_ALLOCATION("ELI06306", ipScansoftEngine != NULL);
			ipScansoftEngine->InitPrivateLicense(LICENSE_MGMT_PASSWORD.c_str());

			// set the OCR engine in the SRIR
			m_ipInputReceiver->SetOCREngine(m_ipOCREngine);

			// Create a new SRIRImageViewer and display it
			m_apDlg = auto_ptr<CSRIRImageViewerDlg>(new CSRIRImageViewerDlg(m_ipSRIR, bDisplaySearch));
			// Default to displaying text in a message box
			m_apDlg->writeOCRTextToMessageBox();

			// set this window as a paragraph text handler
			IIUnknownVectorPtr ipvecPTHs(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI06332", ipvecPTHs != NULL);
			IParagraphTextHandlerPtr ipPTH = (IParagraphTextHandler*)m_apDlg.get();
			ipvecPTHs->PushBack(ipPTH);
			m_ipSRIR->SetParagraphTextHandlers(ipvecPTHs);

			// set this window as the event handler
			m_ipInputReceiver->SetEventHandler((IIREventHandler*)m_apDlg.get());

			// very important
			m_pMainWnd = m_apDlg.get();

			m_vecWindowHandles.push_back(m_apDlg->m_hWnd);

			m_bWindowCreated = true;
		}

		if (bFileNameSpecified)
		{
			m_uiMsgToSend = m_uiMsgLoadImage;
			m_strMessageFileName = strFileName;
			unsigned int ui;
			for (ui = 0; ui < m_vecWindowHandles.size(); ui++)
			{
				sendWindowMsg( m_vecWindowHandles[ui] );
			}
		}

		if (bExecScript)
		{
			m_uiMsgToSend = m_uiMsgExecScript;
			m_strMessageFileName = strScriptFileName;
			unsigned int ui;
			for (ui = 0; ui < m_vecWindowHandles.size(); ui++)
			{
				sendWindowMsg( m_vecWindowHandles[ui] );
			}
		}

		if (bCloseAll)
		{
			m_uiMsgToSend = m_uiMsgCloseViewer;
			unsigned int ui;
			for (ui = 0; ui < m_vecWindowHandles.size(); ui++)
			{
				sendWindowMsg( m_vecWindowHandles[ui] );
			}
		}

		if (eOCRTextHandlingType != kNone)
		{
			switch(eOCRTextHandlingType)
			{
			case kWriteOCRTextToClipboard:
				m_uiMsgToSend = m_uiMsgOCRToClipboard;
				break;
			case kDisplayOCRTextInMessageBox:
				m_uiMsgToSend = m_uiMsgOCRToMessageBox;
				break;
			case kWriteOCRTextToFile:
				m_uiMsgToSend = m_uiMsgOCRToFile;
				m_strMessageFileName = strOCRTextFile;
				break;
			default:
				THROW_LOGIC_ERROR_EXCEPTION("ELI12114");
			}

			unsigned int ui;
			for (ui = 0; ui < m_vecWindowHandles.size(); ui++)
			{
				sendWindowMsg( m_vecWindowHandles[ui] );
			}
		}

		// if we are using this window return true (do not exit app)
		// if we are just updating existing viewers return false (exit the app)
		if (bCreateAndUse)
		{
			return TRUE;
		}
		else
		{
			return FALSE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07407")

	// The only way this point is reached is if an exception is thrown in which case 
	// we return false to exit the application
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
int CSRIRImageViewerApp::ExitInstance() 
{
	try
	{
		// Perform the sutdown that is only necessary if a SRIRImageViewerDlg was
		// created
		if (m_bWindowCreated)
		{
			m_apDlg.reset(NULL);
			// These are unecessary for a reason to be determined but they do cause a beep
	//		IInputManagerSingletonPtr ipInputMgrSingleton(CLSID_InputManagerSingleton);
	//		ipInputMgrSingleton->DeleteInstance();

			CoUninitialize();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11832");

	return CWinApp::ExitInstance();
}
//-------------------------------------------------------------------------------------------------
BOOL CSRIRImageViewerApp::PreTranslateMessage(MSG* pMsg) 
{
	// In this method we are screening for a message from 
	// another SRIRImageViewer telling us to load another image
	try
	{
		try
		{
			if (pMsg->message == m_uiMsgLoadImage)
			{
				if (m_apDlg.get())
				{
					Win32GlobalAtom atom;
					ATOM winAtom = static_cast<ATOM>(pMsg->wParam);
					// here we attach the winAtom so that is 
					// automatically released when atom goes out of 
					// scope
					atom.attach(winAtom);
					m_apDlg->openImage(atom.getName());
				}
			}
			else if (pMsg->message == m_uiMsgExecScript)
			{
				if (m_apDlg.get())
				{
					Win32GlobalAtom atom;
					ATOM winAtom = static_cast<ATOM>(pMsg->wParam);
					// here we attach the winAtom so that is 
					// automatically released when atom goes out of 
					// scope
					atom.attach(winAtom);
					m_apDlg->execScript(atom.getName());
				}
			}
			else if (pMsg->message == m_uiMsgCloseViewer)
			{
				if (m_apDlg.get())
				{
					::PostQuitMessage(0);
				}
			}
			else if (pMsg->message == m_uiMsgOCRToFile)
			{
				if (m_apDlg.get())
				{
					Win32GlobalAtom atom;
					ATOM winAtom = static_cast<ATOM>(pMsg->wParam);
					// here we attach the winAtom so that is 
					// automatically released when atom goes out of 
					// scope
					atom.attach(winAtom);
					m_apDlg->writeOCRTextToFile(atom.getName());
				}
			}
			else if (pMsg->message == m_uiMsgOCRToClipboard)
			{
				if (m_apDlg.get())
				{
					m_apDlg->writeOCRTextToClipboard();
				}
			}
			else if (pMsg->message == m_uiMsgOCRToMessageBox)
			{
				if (m_apDlg.get())
				{
					m_apDlg->writeOCRTextToMessageBox();
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI11836");
	}
	catch(UCLIDException ue)
	{
		// P13 #4627 - when trying to load a bad image there is an exception thrown.  catch
		// and display the exception then return true to indicate the pMsg has been handled
		ue.display();
		return TRUE;
	}

	return CWinApp::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
// Private:
//-------------------------------------------------------------------------------------------------
const string CSRIRImageViewerApp::getUsage()
{
	static string strUsage = "ImageViewer.exe [options] [<filename>]\n"
							 "options:\n"
								"\t/r - register the .tif file extension such that .tif files\n"
								"\topen by default with the Image Viewer\n"
								"\t/u - unregister the .tif file extension such that .tif files\n"
								"\tdo NOT open by default with the Image Viewer\n"
								"\t/m - Text that is OCR'ed in the Image Viewer will be\n"
								"\tdisplayed in a message box rather than copied to the clipboard\n"
								"\t/o <ocrfile> - Text that is OCR'ed in the Image Viewer will be\n"
								"\tbe written to <ocrfile> rather than copied to the clipboard\n"
								"\t/s - display the search window\n"
								"\t/l - reuse a current image viewer if one already exists\n"
								"\t/h <ctrlid> - hide a toolbar control on the Image Viewer\n"
								"\t\t<ctrlid> - to see the valid values for <ctrlid> use the /c option\n"
								"\t/e <scriptfile> - execute script commands specified in <scriptfile>\n"
								"\t/c - display the valid control IDs\n"
								"\t/p <position> - Set the position and size of the image viewer\n"
								"\t\t<position> - position can be one of the following:\n"
								"\t\tFull - display the window in fullscreen\n"
								"\t\tTop - display the window in the top half of the screen\n"
								"\t\tLeft - display the window in the left half of the screen\n"
								"\t\tRight - display the window in the right half of the screen\n"
								"\t\tBottom - display the window in the bottom half of the screen\n"
								"\t/closeall - close any currently open Image Viewer\n"
								"\t/? - show help\n"
							 "<filename> - the name of a file to be opened by default\n";
	return strUsage;
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerApp::sendWindowMsg(HWND hWnd)
{
	if (m_uiMsgToSend == m_uiMsgCloseViewer)
	{
		int nRet = ::PostMessage(hWnd, m_uiMsgCloseViewer, 0, 0);
	}
	else if (m_uiMsgToSend == m_uiMsgLoadImage)
	{
		Win32GlobalAtom atom(m_strMessageFileName.c_str());
		// Note that we detach the atom here
		// this means that the reciever of this message is
		// required to release it.
		int nRet = ::PostMessage(hWnd, m_uiMsgLoadImage, atom.detach(), 0);
	}
	else if (m_uiMsgToSend == m_uiMsgExecScript)
	{
		Win32GlobalAtom atom(m_strMessageFileName.c_str());
		// Note that we detach the atom here
		// this means that the reciever of this message is
		// required to release it.
		int nRet = ::PostMessage(hWnd, m_uiMsgExecScript, atom.detach(), 0);
	}
	else if (m_uiMsgToSend == m_uiMsgOCRToFile)
	{
		Win32GlobalAtom atom(m_strMessageFileName.c_str());
		// Note that we detach the atom here
		// this means that the reciever of this message is
		// required to release it.
		int nRet = ::PostMessage(hWnd, m_uiMsgOCRToFile, atom.detach(), 0);
	}
	else if (m_uiMsgToSend == m_uiMsgOCRToClipboard)
	{
		int nRet = ::PostMessage(hWnd, m_uiMsgOCRToClipboard, 0, 0);
	}
	else if (m_uiMsgToSend == m_uiMsgOCRToMessageBox)
	{
		int nRet = ::PostMessage(hWnd, m_uiMsgOCRToMessageBox, 0, 0);
	}
	else 
	{
		UCLIDException ue("ELI12115", "Invalid SRIR Message!");
		ue.addDebugInfo("MsgId", m_uiMsgToSend);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerApp::addToWindowHandleVector(HWND hWnd)
{
	m_vecWindowHandles.push_back(hWnd);
}
//-------------------------------------------------------------------------------------------------
BOOL CALLBACK CSRIRImageViewerApp::enumSRIRImageWindows(HWND hWnd, LPARAM lParam)
{
	char buf[gnMaxWindowNameSize];
	long nRet = GetWindowText(hWnd, buf, gnMaxWindowNameSize);

	// check if the title of this window matches the Spot Recognition Window's title [P16 #2930]
	if (strstr(buf, gstrSPOT_RECOGNITION_WINDOW_TITLE.c_str()) != NULL)
	{
		CSRIRImageViewerApp* pApp = (CSRIRImageViewerApp*)lParam;
		pApp->addToWindowHandleVector(hWnd);

		// "#32770" is the WNDCLASS name of a dialog and that is what we 
		// are looking for
		//	HWND hWnd = ::FindWindow(MAKEINTATOM(32770), "SRIRImageViewer");
	}
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
UINT CSRIRImageViewerApp::registerMessage(const char* szMsgName)
{
	UINT uiMsgId = RegisterWindowMessage(szMsgName);
	if (uiMsgId == 0)
	{
		UCLIDException ue("ELI12113", "Unable to register Windows Message.");
		ue.addDebugInfo("MsgName", szMsgName);
		throw ue;
	}
	return uiMsgId;
}

/*

  else if (strArg == gstrCmdLineHideButtons)
			{
				// /h reqires an additional argument that is the button
				// to be hidden so if there are no more arguments
				// we throw an exception
				if (i == __argc - 1)
				{
					UCLIDException ue("ELI11858", "The /h option requires an additional argument.");
					throw ue;
				}
				i++;

				// Following argument is of the form a:b:c... 
				// where a, b, c are 0-relative button numbers
				string strButtons = __argv[i];

				// Tokenize the string
				std::vector<std::string>	vecButtonTokens;
				StringTokenizer::sGetTokens( strButtons, ':', vecButtonTokens );
				int nNumOfTokens = vecButtonTokens.size();
				int i;
				for (i = 0; i < nNumOfTokens; i++)
				{
					// Add each button to the collection
					vecButtonsToHide.push_back( vecButtonTokens[i] );
				}
			}
			else if (strArg == gstrCmdLineDisplayButtonIDs)
			{
				AfxMessageBox(getControlIDString().c_str());
				return FALSE;
			}
			else if (strArg == gstrCmdLineSetWindowPosition)
			{
				if (i == __argc - 1)
				{
					UCLIDException ue("ELI12046", "The /p option requires an additional argument.");
					throw ue;
				}
				i++;
				strPos = __argv[i];

				if (strPos == "Full")
				{
					eWindowPos = kWindowFull;
				}
				else if(strPos == "Left")
				{
					eWindowPos = kWindowLeft;
				}
				else if(strPos == "Right")
				{
					eWindowPos = kWindowRight;
				}
				else if(strPos == "Top")
				{
					eWindowPos = kWindowTop;
				}
				else if(strPos == "Bottom")
				{
					eWindowPos = kWindowBottom;
				}
				else
				{
					UCLIDException ue("ELI12047", "Invalid window position specification.");
					ue.addDebugInfo("Position", strPos);
					throw ue;
				}
			}



  //-------------------------------------------------------------------------------------------------
const string CSRIRImageViewerApp::getControlIDString()
{
	static string strID = "Control IDs for the /h option\n"
							"0 - Open\n"
							"1 - Save\n"
							"2 - ZoomWindow\n"
							"3 - ZoomIn\n"
							"4 - ZoomOut\n"
							"5 - ZoomPrevious\n"
							"6 - ZoomNext\n"
							"7 - ZoomExtents\n"
							"8 - Pan\n"
							"9 - SelectText\n"
							"10 - SetHighlightHeight\n"
							"11 - EditZoneText\n"
							"12 - DeleteEntries\n"
							"13 - ParagraphText\n"
							"14 - OpenSubImage\n"
							"15 - RotateCounterClockwise\n"
							"16 - RotateClockwise\n"
							"17 - FirstPage\n"
							"18 - LastPage\n"
							"19 - PrevPage\n"
							"20 - NextPage\n"
							"21 - EditPage\n";
							"22 - Print\n";
	return strID;
}
//-------------------------------------------------------------------------------------------------
ESRIRToolbarCtrl CSRIRImageViewerApp::getControlId(std::string strControl)
{
	static bool bInit = false;
	static map<string, ESRIRToolbarCtrl> controlMap;
	if (!bInit)
	{
		controlMap["0"] = kBtnOpenImage;
		controlMap["1"] = kBtnSave;
		controlMap["2"] = kBtnZoomWindow;
		controlMap["3"] = kBtnZoomIn;
		controlMap["4"] = kBtnZoomOut;
		controlMap["5"] = kBtnZoomPrevious;
		controlMap["6"] = kBtnZoomNext;
		controlMap["7"] = kBtnZoomExtents;
		controlMap["8"] = kBtnPan;
		controlMap["9"] = kBtnSelectText;
		controlMap["10"] = kBtnSetHighlightHeight;
		controlMap["11"] = kBtnEditZoneText;
		controlMap["12"] = kBtnDeleteEntities;
		controlMap["13"] = kBtnPTH;
		controlMap["14"] = kBtnOpenSubImgInWindow;
		controlMap["15"] = kBtnRotateCounterClockwise;
		controlMap["16"] = kBtnRotateClockwise;
		controlMap["17"] = kBtnFirstPage;
		controlMap["18"] = kBtnLastPage;
		controlMap["19"] = kBtnPrevPage;
		controlMap["20"] = kBtnNextPage;
		controlMap["21"] = kEditPageNum;
		controlMap["22"] = kBtnPrint;

		bInit = true;
	}
	map<string, ESRIRToolbarCtrl>::iterator it = controlMap.find(strControl);
	if (it == controlMap.end())
	{
		UCLIDException ue("ELI11859", "Invalid /h argument value.");
		ue.addDebugInfo("Arg", strControl);
	}
	return it->second;		
}
//-------------------------------------------------------------------------------------------------
*/
