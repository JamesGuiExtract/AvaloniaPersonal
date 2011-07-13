// GetNextELICodeDlg.cpp : implementation file
//

#pragma warning(disable:4786)

#include "stdafx.h"
#include "GetNextELICode.h"
#include "GetNextELICodeDlg.h"

#include <io.h>
#include <process.h>

#include <StringTokenizer.h>
#include <cpputil.h>

#include <iostream>
#include <fstream>
#include <string>
using namespace std;

#define WM_ICON_NOTIFY (WM_APP+10)

//-------------------------------------------------------------------------------------------------
// CGetNextELICodeDlg dialog
//-------------------------------------------------------------------------------------------------
CGetNextELICodeDlg::CGetNextELICodeDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CGetNextELICodeDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CGetNextELICodeDlg)
	m_Status = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}
//-------------------------------------------------------------------------------------------------
void CGetNextELICodeDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CGetNextELICodeDlg)
	DDX_Control(pDX, IDC_EDIT_ELI, m_EditELI);
	DDX_Text(pDX, IDC_STATIC_STATUS, m_Status);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CGetNextELICodeDlg, CDialog)
	//{{AFX_MSG_MAP(CGetNextELICodeDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_MESSAGE(WM_ICON_NOTIFY, OnTrayNotification)
	ON_WM_TIMER()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CGetNextELICodeDlg message handlers
//-------------------------------------------------------------------------------------------------
HHOOK hHook = NULL;

//-------------------------------------------------------------------------------------------------
LRESULT CALLBACK asd (int nCode, WPARAM wParam, LPARAM lParam)
{
	CWPSTRUCT *pData = (CWPSTRUCT *) lParam;

	if (pData->lParam == WM_KEYDOWN)
	{
		Beep(1000, 100);
	}
	
	if (wParam == VK_LWIN)
	{
		Beep(1000, 100);
	}

	return CallNextHookEx(hHook, nCode, wParam, lParam);
}
//-------------------------------------------------------------------------------------------------
BOOL CGetNextELICodeDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	// set the timer to 
	// hide the window as the command is to only be accessed via the system tray icon
	setStatusText("Initializing...");
	SetTimer(1001, 1, NULL);
	
	// create the system tray icon
	if (!m_SystemTrayIcon.Create(this, WM_ICON_NOTIFY, "ELI/MLI Generator", m_hIcon, IDR_MENU_TRAY_POPUP))
	{
		MessageBox("ERROR: Unable to create system tray icon!");
		SendMessage(WM_CLOSE);
	}

	// register a to filter all keyboard messages...
	//hHook = SetWindowsHookEx(WH_KEYBOARD, asd, NULL, GetCurrentThreadId());
	//hHook = SetWindowsHookEx(WH_CALLWNDPROC, asd, NULL, NULL);

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
LRESULT CGetNextELICodeDlg::OnTrayNotification(WPARAM wParam, LPARAM lParam)
{
	return m_SystemTrayIcon.OnTrayNotification(wParam, lParam);
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CGetNextELICodeDlg::OnPaint() 
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, (WPARAM) dc.GetSafeHdc(), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}
//-------------------------------------------------------------------------------------------------
// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CGetNextELICodeDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CGetNextELICodeDlg::setStatusText(const CString& zText)
{
	m_Status = zText;
	UpdateData(FALSE);
}
//-------------------------------------------------------------------------------------------------
// I M P O R T A N T :
// EACH LINE OF THE ELI FILE USES THE FOLLOWING FORMAT:
// ELI,Username,Computer,Date,Time,FileName,LineNumber
void CGetNextELICodeDlg::initializeLIGenerationProcess(ELocationIdentifierType eLIType, bool bDisplayDialogs) 
{
	// TODO: break this large method into several smaller methods
	// TODO: improve this method to use exception handling, instead of message boxes

	// Establish the name of the file we will be checking out, updating and, checking back in,
	// and the abbreviation for the type of location identifier.
	string strVSSFileName;
	string strLIAbbreviation;

	switch (eLIType)
	{
	case kExceptionLocationIdentifier:
		strVSSFileName = "UCLIDExceptionLocationIdentifiers.dat";
		strLIAbbreviation = "ELI";
		break;

	case kMethodLocationIdentifier:
		strVSSFileName = "ExtractMethodLocationIdentifiers.dat";
		strLIAbbreviation = "MLI";
		break;

	default:
		MessageBox("Invalid location identifier type!");
		return;
	}

	setStatusText("Initializing...");
	// change the current directory to c:\temp...so that if this program is being
	// run from the network, there will be no clashes between concurrent users
	string strTempDir;
	getTempDir(strTempDir);
	if (!SetCurrentDirectory(strTempDir.c_str()))
	{
		MessageBox("Unable to change current directory to c:\\temp!\nPlease create this directory for temporary files, and retry.", "ERROR", MB_ICONEXCLAMATION);
		return;
	}

	// find the Visual C++ studio window so that the new ELI can be pasted into the editor
	// at the current cursor location
	HWND hMSVCPP = NULL;
	
	// If called with bDisplayDialogs false the caller is using the clip board so 
	// don't need to find running instance of Visual Studio
	if (bDisplayDialogs)
	{
		// hide this window so that the calling window gets focus
		HWND hLastChild = NULL;
		HWND hNextFind;
		bool bFound = false;
		do 
		{
			hNextFind = ::FindWindowEx(NULL, hLastChild, NULL, NULL);
			if (hNextFind != NULL)
			{
				char pszTemp[MAX_PATH];
				if (::GetWindowText(hNextFind, pszTemp, sizeof(pszTemp)) != 0)
				{
					// if the window's title contains the word "Microsoft Visual Studio", 
					// and does not contain "2005" then accept it as the window 
					// we are looking for.
					if ((strstr(pszTemp, "Microsoft Visual Studio") != NULL) && 
						(strstr(pszTemp, "2005") == NULL))
					{
						if (hMSVCPP == NULL)
						{
							hMSVCPP = hNextFind;
						}
						else
						{
							// there's more than 1 visual studio window open...we won't know
							// where to paste.
							MessageBox("More than one instance of Microsoft Visual Studio is currently opened.  Pasting of the code will therefore have to be done manually.");
							hMSVCPP = NULL;
							break;
						}
					}
				}

				// search for all windows after the current window
				hLastChild = hNextFind;
			}
		} while (hNextFind != NULL);
	}

	// delete temporary files
	setStatusText("Deleting temporary files...");
	string strTempFile = ".\\";
	strTempFile += strVSSFileName;
	string strMsg;
	if (_access(strTempFile.c_str(), 00) == 0)
	{
		// remove the readonly attribute on the file so that it can be deleted
		if (!SetFileAttributes(strTempFile.c_str(), FILE_ATTRIBUTE_NORMAL))
		{
			strMsg = string("Unable to remove readonly attribute from file ") + strTempFile + string("!");
			MessageBox(strMsg.c_str(), "ERROR", MB_ICONEXCLAMATION);
			return;
		}

		// the file exists, try to delete it
		try
		{
			deleteFile(strTempFile.c_str(), false, false);
		}
		catch (...)
		{
			strMsg = string("Unable to delete temporary file ") + strTempFile + string("!");
			MessageBox(strMsg.c_str(), "ERROR", MB_ICONEXCLAMATION);
			return;
		}
	}

	// check out the latest .dat file
	{
		strMsg = string("Checking out ") + strVSSFileName + string(" ...");
		setStatusText(strMsg.c_str());

		string strCommand = "ss Checkout $/Engineering/ProductDevelopment/Common/";
		strCommand += strVSSFileName;

		// Allow 15 second timeout
		runEXE( strCommand, "", 15000 );

		// ensure that the file was checked out (try to see if we can read/write it)
		// if we can read/write the file, that means that VSS is done creating it.
		if (_access(strTempFile.c_str(), 06) != 0) // read permission
		{
			MessageBox("File not checked out properly!");
			return;
		}
	}

	// open the file in read-only mode and determine the last identifier used.
	string strLastLICode;
	strMsg = string("Reading ") + strVSSFileName + string(" ...");
	setStatusText(strMsg.c_str());
	{
		ifstream infile(strTempFile.c_str());
		if (!infile)
		{
			MessageBox("Unable to open file for reading!", "ERROR", MB_ICONEXCLAMATION);
			return;
		}

		string strTemp;
		unsigned long ulLineNum = 0;
		do
		{
			// get a line from the file
			if (getline(infile, strTemp))
			{
				ulLineNum++;

				// process the line...it better have exactly
				// 7 fields, otherwise, do not continue
				vector<string> vecTokens;
				StringTokenizer tokenizer;
				tokenizer.parse(strTemp, vecTokens);
				if (vecTokens.size() != 7 && strTemp != "")
				{
					string strMessage = "The file has been corrupted (see line ";
					strMessage += asString(ulLineNum);
					strMessage += ")!";
					MessageBox(strMessage.c_str(), "ERROR", MB_ICONEXCLAMATION);
					return; 
				}

				// save the last eli used
				strLastLICode = vecTokens[0];
			}
		} while (!infile.eof());
	}

	// increment the last identifier and 
	// write the new information to the file
	strMsg = string("Updating ") + strVSSFileName + string(" ...");
	setStatusText(strMsg.c_str());
	string strNewLICode;
	{
		strNewLICode = incrementNumericalSuffix(strLastLICode);
		ofstream outfile(strTempFile.c_str(), ios::app);
		string strComma = ",";

		outfile << strNewLICode << strComma;
		outfile << getCurrentUserName() << strComma;
		outfile << getComputerName() << strComma;
		outfile << getDateAsString() << strComma;
		outfile << getTimeAsString() << strComma;
		outfile << /* The source file name not known at this time << */ strComma;
		outfile << /* line number not known at this time << */ endl;
		outfile.close();
		waitForFileAccess(strTempFile, giMODE_READ_ONLY);
	}

	// check in the file..
	strMsg = string("Checking-in ") + strVSSFileName + string(" ...");
	setStatusText(strMsg.c_str());
	{
		string strCommand = "ss checkin $/Engineering/ProductDevelopment/Common/";
		strCommand += strVSSFileName;
		strCommand += " -I-";
		
		// Allow 15 second timeout
		runEXE( strCommand, "", 15000 );
	}
	
	// verify successful checkin of the file...
	// and delete the file after checkin.
	strMsg = string("Verifying successful check-in of ") + strVSSFileName + string(" ...");
	setStatusText(strMsg.c_str());
	if (_access(strTempFile.c_str(), 00) != 0)
	{
		strMsg = string("The file ") + strVSSFileName + string(" does not exist after check-in!");
		MessageBox(strMsg.c_str(), "ERROR", MB_ICONEXCLAMATION);
		return;
	}
	else
	{
		// the file exists, make sure the read-only flag is turned on
		// by trying to write to the file and ensuring that it fails
		while (_access(strTempFile.c_str(), 02) == 0)
		{
			// the file is still in a writable form - this means that the checkin did not work
			// properly!
			///MessageBox("The file UCLIDExceptionLocationIdentifiers.dat could not be checked-in properly!", "ERROR", MB_ICONEXCLAMATION);
			Sleep(500);
		}

		// ok, the file's readonly flag is turned on - so checkin must have worked.
		// remove the readonly attribute on the file so that it can be deleted
		if (!SetFileAttributes(strVSSFileName.c_str(), FILE_ATTRIBUTE_NORMAL))
		{
			strMsg = string("Unable to remove readonly attribute from file ") + strVSSFileName + string("!");
			MessageBox(strMsg.c_str(), "ERROR", MB_ICONEXCLAMATION);
			return;
		}

		// delete the file now
		try
		{
			deleteFile(strTempFile.c_str(), false, false);
		}
		catch (...)
		{
			string strMessage = "Unable to delete temporary file ";
			strMessage += strTempFile;
			MessageBox(strMessage.c_str(), "ERROR", MB_ICONEXCLAMATION);
			return;
		}
	}

	// let user know about succesful ELI generation
	// and copy the ELI to the clipboard
	strNewLICode.insert(0, "\"");
	strNewLICode.append("\"");
	m_EditELI.SetWindowText(strNewLICode.c_str());
	m_EditELI.SetSel(0, -1);
	m_EditELI.Copy();

	// send the paste command to the Microsoft Visual Studio window.
//	if (hMSVCPP)
//	{
//		// paste the code into the microsoft visual studio window
//		ShowWindow(FALSE);
//		// Find the MDIClient child window of hMSVCPP
//		// Find the EzMdiContainer child window of MDIClient
//		::SendMessage(hMSVCPP, WM_COMMAND, 57637, 0);
//		::SetForegroundWindow(hMSVCPP);
//	}
//	else
	{
		// If called with bDisplayDialogs false the caller is using the clip board so 
		// don't need to find running instance of Visual Studio
		if ( bDisplayDialogs )
		{

			string strMessage = "A new ";
			strMessage += strLIAbbreviation;
			strMessage += " code has been generated.";
			strMessage += "\nThe new code is: ";
			strMessage += strNewLICode;
			strMessage += "\nPlease remember to paste this manually in your source code.";
			MessageBox(strMessage.c_str());
		}
	}
}
//-------------------------------------------------------------------------------------------------
BOOL CGetNextELICodeDlg::OnCommand(WPARAM wParam, LPARAM lParam) 
{
	if (wParam == ID_UCLIDDEVELOPMENTSERVICES_EXIT)
	{
		//UnhookWindowsHookEx(hHook);
		SendMessage(WM_CLOSE);
		return TRUE;
	}
	else if (wParam == ID_UCLIDDEVELOPMENTSERVICES_INSERT_ELI)
	{
		ShowWindow(TRUE);
		initializeLIGenerationProcess(kExceptionLocationIdentifier, lParam == 0);
		ShowWindow(FALSE);
		return TRUE;
	}
	else if (wParam == ID_UCLIDDEVELOPMENTSERVICES_INSERT_MLI)
	{
		ShowWindow(TRUE);
		initializeLIGenerationProcess(kMethodLocationIdentifier, lParam == 0);
		ShowWindow(FALSE);
		return TRUE;
	}
	else
	{
		return CDialog::OnCommand(wParam, lParam);
	}
}
//-------------------------------------------------------------------------------------------------
void CGetNextELICodeDlg::OnTimer(UINT nIDEvent) 
{
	KillTimer(nIDEvent);
	Sleep(1000);// cosmetic
	ShowWindow(FALSE);
	
	CDialog::OnTimer(nIDEvent);
}
//-------------------------------------------------------------------------------------------------
BOOL CGetNextELICodeDlg::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo) 
{
	// TODO: Add your specialized code here and/or call the base class
	
	return CDialog::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
}
//-------------------------------------------------------------------------------------------------
