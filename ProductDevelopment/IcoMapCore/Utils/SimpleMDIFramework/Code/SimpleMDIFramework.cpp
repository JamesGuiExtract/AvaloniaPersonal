// SimpleMDIFramework.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "SimpleMDIFramework.h"

#include "MainFrm.h"
#include "ChildFrm.h"
#include "IpFrame.h"
#include "SimpleMDIFrameworkDoc.h"
#include "SimpleMDIFrameworkView.h"

#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb"
using namespace UCLID_COMLMLib;
#import "..\..\..\..\..\ReusableComponents\COMComponents\VariantCollection\Code\VariantCollection.dll"
#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDIUnknownVector\Code\UCLIDIUnknownVector.tlb"
#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDDistanceConverter\Code\UCLIDDistanceConverter.tlb"
#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDFeatureMgmt\Code\UCLIDFeatureMgmt.tlb"
#import "..\..\..\IcoMapInterfaces\Code\IcoMapInterfaces.tlb"
#import "..\..\..\IcoMapApp\Code\IcoMapApp.tlb"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkApp

BEGIN_MESSAGE_MAP(CSimpleMDIFrameworkApp, CWinApp)
	//{{AFX_MSG_MAP(CSimpleMDIFrameworkApp)
	ON_COMMAND(ID_ICOMAP, OnIcomap)
	//}}AFX_MSG_MAP
	// Standard file based document commands
	ON_COMMAND(ID_FILE_NEW, CWinApp::OnFileNew)
	ON_COMMAND(ID_FILE_OPEN, CWinApp::OnFileOpen)
	// Standard print setup command
	ON_COMMAND(ID_FILE_PRINT_SETUP, CWinApp::OnFilePrintSetup)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkApp construction

CSimpleMDIFrameworkApp::CSimpleMDIFrameworkApp()
{
	if (CoInitializeEx(NULL, COINIT_MULTITHREADED) != S_OK)
		AfxMessageBox("Unable to initialize COM environment!");
}

CSimpleMDIFrameworkApp::~CSimpleMDIFrameworkApp()
{
	CoUninitialize();
}

/////////////////////////////////////////////////////////////////////////////
// The one and only CSimpleMDIFrameworkApp object

CSimpleMDIFrameworkApp theApp;

// This identifier was generated to be statistically unique for your app.
// You may change it if you prefer to choose a specific identifier.

// {5F5FBDF3-BEC2-4A63-92AE-BAB2A7F95267}
static const CLSID clsid =
{ 0x5f5fbdf3, 0xbec2, 0x4a63, { 0x92, 0xae, 0xba, 0xb2, 0xa7, 0xf9, 0x52, 0x67 } };

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkApp initialization

BOOL CSimpleMDIFrameworkApp::InitInstance()
{
	// Initialize OLE libraries
	if (!AfxOleInit())
	{
		AfxMessageBox(IDP_OLE_INIT_FAILED);
		return FALSE;
	}

	AfxEnableControlContainer();

	// Standard initialization
	// If you are not using these features and wish to reduce the size
	//  of your final executable, you should remove from the following
	//  the specific initialization routines you do not need.

#ifdef _AFXDLL
	Enable3dControls();			// Call this when using MFC in a shared DLL
#else
	Enable3dControlsStatic();	// Call this when linking to MFC statically
#endif

	// Change the registry key under which our settings are stored.
	// TODO: You should modify this string to be something appropriate
	// such as the name of your company or organization.
	SetRegistryKey(_T("Local AppWizard-Generated Applications"));

	LoadStdProfileSettings();  // Load standard INI file options (including MRU)

	// Register the application's document templates.  Document templates
	//  serve as the connection between documents, frame windows and views.

	CMultiDocTemplate* pDocTemplate;
	pDocTemplate = new CMultiDocTemplate(
		IDR_SIMPLETYPE,
		RUNTIME_CLASS(CSimpleMDIFrameworkDoc),
		RUNTIME_CLASS(CChildFrame), // custom MDI child frame
		RUNTIME_CLASS(CSimpleMDIFrameworkView));
	pDocTemplate->SetServerInfo(
		IDR_SIMPLETYPE_SRVR_EMB, IDR_SIMPLETYPE_SRVR_IP,
		RUNTIME_CLASS(CInPlaceFrame));
	AddDocTemplate(pDocTemplate);

	// Connect the COleTemplateServer to the document template.
	//  The COleTemplateServer creates new documents on behalf
	//  of requesting OLE containers by using information
	//  specified in the document template.
	m_server.ConnectTemplate(clsid, pDocTemplate, FALSE);

	// Register all OLE server factories as running.  This enables the
	//  OLE libraries to create objects from other applications.
	COleTemplateServer::RegisterAll();
		// Note: MDI applications register all server objects without regard
		//  to the /Embedding or /Automation on the command line.

	// create main MDI Frame window
	CMainFrame* pMainFrame = new CMainFrame;
	if (!pMainFrame->LoadFrame(IDR_MAINFRAME))
		return FALSE;
	m_pMainWnd = pMainFrame;

	// Parse command line for standard shell commands, DDE, file open
	CCommandLineInfo cmdInfo;
	ParseCommandLine(cmdInfo);

	// Check to see if launched as OLE server
	if (cmdInfo.m_bRunEmbedded || cmdInfo.m_bRunAutomated)
	{
		// Application was run with /Embedding or /Automation.  Don't show the
		//  main window in this case.
		return TRUE;
	}

	// When a server application is launched stand-alone, it is a good idea
	//  to update the system registry in case it has been damaged.
	m_server.UpdateRegistry(OAT_INPLACE_SERVER);

	// Dispatch commands specified on the command line
	if (!ProcessShellCommand(cmdInfo))
		return FALSE;

	// The main window has been initialized, so show and update it.
	pMainFrame->ShowWindow(m_nCmdShow);
	pMainFrame->UpdateWindow();

	return TRUE;
}


/////////////////////////////////////////////////////////////////////////////
// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	//{{AFX_DATA(CAboutDlg)
	enum { IDD = IDD_ABOUTBOX };
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAboutDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	//{{AFX_MSG(CAboutDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CAboutDlg)
	//}}AFX_DATA_INIT
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

// App command to run the dialog
void CSimpleMDIFrameworkApp::OnAppAbout()
{
	CAboutDlg aboutDlg;
	aboutDlg.DoModal();
}

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkApp message handlers

void CSimpleMDIFrameworkApp::OnIcomap() 
{
	// Declare license object
	IUCLIDComponentLMPtr ipLicense( __uuidof(UCLIDComponentLM) );
	
	ipLicense->Initialize( "AW247YHUG8" );

	ICOMAPINTERFACESLib::IIcoMapApplicationPtr ptrIcoMap;
	ptrIcoMap.CreateInstance(__uuidof(ICOMAPAPPLib::IcoMap));
	ptrIcoMap->SetAttributeManager(NULL);
	ptrIcoMap->SetDisplayAdapter(NULL);
	ptrIcoMap->ShowIcoMapWindow(TRUE);
	ptrIcoMap.Detach();
}
