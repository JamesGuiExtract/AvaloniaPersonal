// TestHarnessDlg.cpp : implementation file
//

#include "stdafx.h"
#include "TestHarness.h"
#include "TestHarnessDlg.h"

#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <RegistryPersistenceMgr.h>
#include <cpputil.h>

#include <RegConstants.h>
#include <LoadFileDlgThread.h>

#include <io.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern CTestHarnessApp theApp;

const int giTIMERID = 1002;

const string gstrLAST_FILE_REG_KEY = "LastTCLFile";

UINT WM_USER_TESTS_FINISHED = WM_USER + 1234;

//-------------------------------------------------------------------------------------------------
// CTestHarnessDlg dialog
//-------------------------------------------------------------------------------------------------
CTestHarnessDlg::CTestHarnessDlg(CWnd* pParent /*=NULL*/)
: CDialog(CTestHarnessDlg::IDD, pParent),
  m_ipTestHarness(CLSID_TestHarness), 
  m_bRunning(false),
  m_pThread(NULL)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(theApp.m_hInstance);
	
	try
	{
		//{{AFX_DATA_INIT(CTestHarnessDlg)
		m_zFilename = _T("");
		m_nFileType = -1;
		m_zTestFolder = _T("");
		//}}AFX_DATA_INIT
		// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
		m_hIcon = AfxGetApp()->LoadIcon(IDI_ICON_TEST);
		
		// ensure that the test harness object was created successfully
		ASSERT_RESOURCE_ALLOCATION("ELI04907", m_ipTestHarness!=NULL);

		// create an instance of the configuration persistence manager
		m_apCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER,
			gstrREG_ROOT_KEY + "\\TestingFramework\\TestHarness"));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04906")
}
//-------------------------------------------------------------------------------------------------
CTestHarnessDlg::~CTestHarnessDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16505");
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTestHarnessDlg)
	DDX_Control(pDX, IDC_BTN_BROWSE, m_btnBrowse);
	DDX_Control(pDX, IDC_RADIO_TCL, m_radioTCL);
	DDX_Control(pDX, IDC_RADIO_ITC, m_radioITC);
	DDX_Control(pDX, IDC_BTN_OPEN_NOTEPAD, m_btnNotepad);
	DDX_Control(pDX, IDC_BTN_INTERACTIVE_TESTS, m_btnInteractiveTests);
	DDX_Control(pDX, IDC_BTN_AUTOMATED_TESTS, m_btnAutomatedTests);
	DDX_Control(pDX, IDC_BTN_ALL_TESTS, m_btnAllTests);
	DDX_Text(pDX, IDC_EDIT_TCLFILENAME, m_zFilename);
	DDX_Radio(pDX, IDC_RADIO_TCL, m_nFileType);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTestHarnessDlg, CDialog)
	//{{AFX_MSG_MAP(CTestHarnessDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BTN_ALL_TESTS, OnBtnAllTests)
	ON_BN_CLICKED(IDC_BTN_AUTOMATED_TESTS, OnBtnAutomatedTests)
	ON_BN_CLICKED(IDC_BTN_BROWSE, OnBtnBrowse)
	ON_BN_CLICKED(IDC_BTN_INTERACTIVE_TESTS, OnBtnInteractiveTests)
	ON_EN_UPDATE(IDC_EDIT_TCLFILENAME, OnChangeEditFilename)
	ON_BN_CLICKED(IDC_RADIO_TCL, OnRadioTcl)
	ON_BN_CLICKED(IDC_RADIO_ITC, OnRadioItc)
	ON_BN_CLICKED(IDC_BTN_OPEN_NOTEPAD, OnBtnOpenNotepad)
	ON_WM_TIMER()
	ON_WM_CLOSE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CTestHarnessDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CTestHarnessDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(theApp.m_hInstance);
	
	try
	{
		// set notepad icon for the button
		m_btnNotepad.SetIcon(::LoadIcon(theApp.m_hInstance, MAKEINTRESOURCE(IDI_ICON_NOTEPAD)));

		// disable all running test buttons
		m_btnAllTests.EnableWindow(FALSE);
		m_btnAutomatedTests.EnableWindow(FALSE);
		m_btnInteractiveTests.EnableWindow(FALSE);

		// set default to select tcl file
		m_nFileType = 0;

		// set to run Automated tests if a file name was on the command line
		bool bStartAutomatedTests = !m_zFilename.IsEmpty();

		// load the name of the last selected TCL file if possible
		if (m_zFilename.IsEmpty() && m_apCfgMgr->keyExists("\\", gstrLAST_FILE_REG_KEY))
		{
			m_zFilename = m_apCfgMgr->getKeyValue("\\", gstrLAST_FILE_REG_KEY).c_str();
		}

		UpdateData(FALSE);
		OnChangeEditFilename();
		
		// The tester results window usually obscures this window
		// when this window comes up, or if the user clicks on the
		// tester window, so, set this window as the foreground window
		SetForegroundWindow();

		// if it requires to start an automated test, do it
		// after OnInitDialog() call is finished.
		if (bStartAutomatedTests)
		{
			// set a timer
			SetTimer(giTIMERID, 500, NULL);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04908")
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CTestHarnessDlg::OnPaint() 
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
HCURSOR CTestHarnessDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnChangeEditFilename() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(theApp.m_hInstance);
	
	try
	{
		UpdateData(TRUE);

		// make sure the file name is not empty and the file exists
		BOOL bEnable = asMFCBool(!m_zFilename.IsEmpty() && isValidFile(m_zFilename.GetString()));

		BOOL bIsTCLFile = m_nFileType==0 ? TRUE : FALSE;

		// disable all running test buttons
		m_btnAllTests.EnableWindow(bEnable && bIsTCLFile);
		m_btnAutomatedTests.EnableWindow(bEnable && bIsTCLFile);
		m_btnInteractiveTests.EnableWindow(bEnable);
		m_btnNotepad.EnableWindow(bEnable);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04909")
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnBtnAllTests() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(theApp.m_hInstance);
	
	try
	{
		UpdateData(TRUE);
		disableUI();
		m_bRunning = true;
		m_pThread = AfxBeginThread(runAllTests, this);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04910")
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnBtnAutomatedTests() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(theApp.m_hInstance);
	
	try
	{
		UpdateData(TRUE);
		disableUI();
		m_bRunning = true;
		m_pThread = AfxBeginThread(runAutoTests, this);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04911")
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnBtnBrowse() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(theApp.m_hInstance);
	
	try
	{
		UpdateData(TRUE);
		// check whether its TCL or ITC file
		CString zFileType("TCL files (*.tcl)|*.tcl||");
		if (m_nFileType==1)
		{
			zFileType = "ITC files (*.itc)|*.itc||";
		}

		CFileDialog openDialog(TRUE, NULL, NULL, OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			zFileType, this);
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&openDialog);

		if (tfd.doModal() == IDOK)
		{
			// get file name
			m_zFilename = openDialog.GetPathName();
			UpdateData(FALSE);

			// store the filename in the registy for loading next time
			m_apCfgMgr->setKeyValue("\\", gstrLAST_FILE_REG_KEY, 
				(LPCTSTR) m_zFilename);

			// enable/disable buttons
			OnChangeEditFilename();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04912")
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnBtnInteractiveTests() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(theApp.m_hInstance);
	
	try
	{
		UpdateData(TRUE);
		disableUI();
		m_bRunning = true;
		m_pThread = AfxBeginThread(runInteractiveTests, this);
		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04913")
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnRadioTcl() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(theApp.m_hInstance);
	
	try
	{
		UpdateData(TRUE);
		
		OnChangeEditFilename();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05593")
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnRadioItc() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(theApp.m_hInstance);
	
	try
	{
		UpdateData(TRUE);
		
		OnChangeEditFilename();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05594")
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnCancel()
{
	// purpose of having this function here is to prevent
	// user from closing the dialog by pressing Escape key
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnOK()
{
	// purpose of having this function here is to prevent
	// user from closing the dialog by pressing Enter key
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnClose() 
{	
	if (m_bRunning)
		{
			if (AfxMessageBox("Tests are currently running\nIt is recommended that you wait for completion\nbefore closing the application otherwise data \nmight be lost.\n\nForce Quit Anyway?", MB_YESNO) == IDNO)
			{
				return;
			}
		}
	// remember last opened file
	if (!m_zFilename.IsEmpty())
	{
		m_apCfgMgr->setKeyValue("\\", gstrLAST_FILE_REG_KEY, (LPCTSTR)m_zFilename);
	}

	CDialog::OnClose();
	// this function is usually called by clicking the X on the
	// top-right corner of the dialog to close the dialog.
	// Since we leave OnCancel() implementation empty, this is
	// the place to call CDialog::OnCancel() to close the window.
	CDialog::OnCancel();
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnBtnOpenNotepad() 
{
	AFX_MANAGE_STATE(AfxGetModuleState())
	TemporaryResourceOverride resourceOverride(theApp.m_hInstance);

	try
	{
		UpdateData(TRUE);

		if (!m_zFilename.IsEmpty()) 
		{
			// get window system32 path
			char pszSystemDir[MAX_PATH];
			::GetSystemDirectory(pszSystemDir, MAX_PATH);
			
			string strCommand(pszSystemDir);
			strCommand += "\\Notepad.exe ";
			strCommand += m_zFilename;
			// run Notepad.exe with this file
			::runEXE(strCommand);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07302");
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::OnTimer(UINT nIDEvent) 
{
	try
	{
		// if we received a timer event to perform an auto-save, do the auto-save
		if (nIDEvent == giTIMERID)
		{
			// stop the timer before the automated test runs
			KillTimer(nIDEvent);

			// Start the automated tests in a separate thread to support 
			// UI responsiveness (P16 #2359)
			OnBtnAutomatedTests();
		}

		CDialog::OnTimer(nIDEvent);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07400")
}
//-------------------------------------------------------------------------------------------------
LRESULT CTestHarnessDlg::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam) 
{
	if (message == WM_USER_TESTS_FINISHED)
	{
		// This function will re-enable the appropriate buttons
		
		if(m_pThread)
		{
			WaitForSingleObject(m_pThread->m_hThread, 10000);
		}
		m_pThread = NULL;
		m_radioTCL.EnableWindow(TRUE);
		m_radioITC.EnableWindow(TRUE);
		m_btnBrowse.EnableWindow(TRUE);
		OnChangeEditFilename();
		m_bRunning = false;
	}
	
	return CDialog::DefWindowProc(message, wParam, lParam);
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::runAutomatedTests()
{
	if (m_nFileType == 0)
	{
		// set tcl file
		m_ipTestHarness->SetTCLFile(_bstr_t(m_zFilename));
		
		m_ipTestHarness->RunAutomatedTests();
	}
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::runAutoTests()
{
	runAutomatedTests();
	PostMessage(WM_USER_TESTS_FINISHED);
}
//-------------------------------------------------------------------------------------------------
UINT CTestHarnessDlg::runAutoTests(LPVOID pParam)
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{
		CTestHarnessDlg* pDlg = (CTestHarnessDlg*)pParam;
		pDlg->runAutoTests();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10006");
	CoUninitialize();
	return 0;
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::runInteractiveTests()
{
	if (m_nFileType == 0)
	{
		// set tcl file
		m_ipTestHarness->SetTCLFile(_bstr_t(m_zFilename));
		
		m_ipTestHarness->RunInteractiveTests();
	}
	else if (m_nFileType == 1)
	{
		// set itc file
		m_ipTestHarness->ExecuteITCFile(_bstr_t(m_zFilename));
	}
	PostMessage(WM_USER_TESTS_FINISHED);
}
//-------------------------------------------------------------------------------------------------
UINT CTestHarnessDlg::runInteractiveTests(LPVOID pParam)
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{
		CTestHarnessDlg* pDlg = (CTestHarnessDlg*)pParam;
		pDlg->runInteractiveTests();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19376");
	CoUninitialize();

	return 0;
}
//-------------------------------------------------------------------------------------------------	
void CTestHarnessDlg::runAllTests()
{
	if (m_nFileType == 0)
	{
		// set tcl file
		m_ipTestHarness->SetTCLFile(_bstr_t(m_zFilename));
		
		m_ipTestHarness->RunAllTests();
	}
	PostMessage(WM_USER_TESTS_FINISHED);
}
//-------------------------------------------------------------------------------------------------
UINT CTestHarnessDlg::runAllTests(LPVOID pParam)
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{
		CTestHarnessDlg* pDlg = (CTestHarnessDlg*)pParam;
		pDlg->runAllTests();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19377");
	CoUninitialize();

	return 0;
}
//-------------------------------------------------------------------------------------------------
void CTestHarnessDlg::disableUI()
{
	m_btnInteractiveTests.EnableWindow(FALSE);
		m_btnAutomatedTests.EnableWindow(FALSE);
		m_btnAllTests.EnableWindow(FALSE);
		m_btnNotepad.EnableWindow(FALSE);
		m_radioTCL.EnableWindow(FALSE);
		m_radioITC.EnableWindow(FALSE);
		m_btnBrowse.EnableWindow(FALSE);
}