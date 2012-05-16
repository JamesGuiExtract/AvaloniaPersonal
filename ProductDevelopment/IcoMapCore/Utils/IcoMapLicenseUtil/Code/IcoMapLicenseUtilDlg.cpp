// IcoMapLicenseUtilDlg.cpp : implementation file
//

#include "stdafx.h"

#pragma warning( disable : 4786 )

#include "IcoMapLicenseUtil.h"
#include "IcoMapLicenseUtilDlg.h"
#include "IcoMapOptions.h"
#include "SafeNetLicenseMgr.h"

#include <cpputil.h>
#include <ShellExecuteThread.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CIcoMapLicenseUtilDlg dialog
//-------------------------------------------------------------------------------------------------
CIcoMapLicenseUtilDlg::CIcoMapLicenseUtilDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CIcoMapLicenseUtilDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CIcoMapLicenseUtilDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}
//-------------------------------------------------------------------------------------------------
void CIcoMapLicenseUtilDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CIcoMapLicenseUtilDlg)
	DDX_Control(pDX, IDC_LIMIT_TEXT, m_staticLimitText);
	DDX_Control(pDX, IDC_ICOMAP_USER_LIMIT, m_editIcoMapUserLimit);
	DDX_Control(pDX, IDC_SERVER_STATUS, m_btnServerStatus);
	DDX_Control(pDX, IDC_LM_TEXT, m_staticLMText);
	DDX_Control(pDX, IDC_LM_SERVER, m_editLMServer);
	DDX_Control(pDX, IDC_NODE_LOCKED, m_checkNodeLocked);
	DDX_Control(pDX, IDC_CONCURRENT, m_checkConcurrent);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(CIcoMapLicenseUtilDlg, CDialog)
	//{{AFX_MSG_MAP(CIcoMapLicenseUtilDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_CONCURRENT, OnLicenseType)
	ON_BN_CLICKED(IDC_SERVER_STATUS, OnServerStatus)
	ON_BN_CLICKED(IDC_NODE_LOCKED, OnLicenseType)
	ON_EN_CHANGE(IDC_LM_SERVER, OnChangeLmServer)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CIcoMapLicenseUtilDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CIcoMapLicenseUtilDlg::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();
		
		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon
		
		string strServerName = m_snlcSafeNetCfg.getContactServerName();
		if ( strServerName == "SP_LOCAL_MODE" )
		{
			m_editLMServer.SetWindowText( "" );
		}
		else
		{
			m_editLMServer.SetWindowText( strServerName.c_str() );
		}
		// Set up the licensing mode
		if ( IcoMapOptions::sGetInstance().getLicenseManagementMode() == kConcurrent )
		{
			m_checkConcurrent.SetCheck(BST_CHECKED);
			m_checkNodeLocked.SetCheck(BST_UNCHECKED);
			m_editIcoMapUserLimit.SetWindowText( "" );
		}
		else
		{
			m_checkConcurrent.SetCheck(BST_UNCHECKED);
			m_checkNodeLocked.SetCheck(BST_CHECKED);
		}
		
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12613");

	// TODO: Add extra initialization here
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CIcoMapLicenseUtilDlg::OnPaint() 
{
	try
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12614");
}

//-------------------------------------------------------------------------------------------------
// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CIcoMapLicenseUtilDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CIcoMapLicenseUtilDlg::OnLicenseType() 
{
	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12615");
	
}
//-------------------------------------------------------------------------------------------------
void CIcoMapLicenseUtilDlg::OnOK() 
{
	try
	{
		string strServerName = "SP_LOCAL_MODE";
		if ( m_checkConcurrent.GetCheck() == BST_CHECKED )
		{
			CString czServerName;
			m_editLMServer.GetWindowText(czServerName);
			strServerName = czServerName;
			IcoMapOptions::sGetInstance().setLicenseManagementMode(kConcurrent);
		}
		else
		{
			IcoMapOptions::sGetInstance().setLicenseManagementMode(kNodeLocked);
		}
		m_snlcSafeNetCfg.setServerName(strServerName);
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12616");
}
//-------------------------------------------------------------------------------------------------
void CIcoMapLicenseUtilDlg::OnServerStatus() 
{
	try
	{
		// Want to have the server status use the current value of the edit box
		CString czServerName;
		m_editLMServer.GetWindowText(czServerName);
		string strServerName = czServerName;
		
		if ( strServerName == "" )
		{
			strServerName = getComputerName();
		}
		// This gets a license from the server if one is available and gets the soft limit
		// if a license cannot be obtained there is an exception and the 

		// Temporarily change the server to the value in the edit box
		string strCurrServerName = m_snlcSafeNetCfg.getContactServerName();
			
		m_snlcSafeNetCfg.setServerName( strServerName );
		{
			try
			{
				SafeNetLicenseMgr snlmLicense( gusblIcoMap );
				DWORD dwSoftLimit = snlmLicense.getCellValue(gdcellIcoMapUserLimit);
				string strSoftLimit = asString(dwSoftLimit);
				m_editIcoMapUserLimit.SetWindowText( strSoftLimit.c_str());
			}
			catch(...)
			{
				// if any exceptions it means the key on that machine was not avaliable
				// it is sufficent for there to be no user limits shown
			}

		}
		// restore the server setting
		m_snlcSafeNetCfg.setServerName( strCurrServerName );

		// Sentinel server Monitor http Address
		string strMonitorAddress = "http://" + strServerName + ":6002";

		// Create the ThreadDataStruct obj
		ThreadDataStruct ThreadData(this->GetSafeHwnd(), "open", strMonitorAddress.c_str(), 
			NULL, "", SW_SHOWNORMAL);

		// Call the ShellExecuteThread to execute in a separate thread 
		ShellExecuteThread shellExecute(&ThreadData);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12617");
}
//-------------------------------------------------------------------------------------------------
void CIcoMapLicenseUtilDlg::OnChangeLmServer() 
{
	m_editIcoMapUserLimit.SetWindowText("");
}
//-------------------------------------------------------------------------------------------------
// Helper Functions
//-------------------------------------------------------------------------------------------------
void CIcoMapLicenseUtilDlg::updateControls()
{
	int nLicenseType = m_checkConcurrent.GetCheck();
	if ( nLicenseType == BST_CHECKED )
	{
		m_editLMServer.EnableWindow(TRUE);
		m_staticLMText.EnableWindow(TRUE);	
		m_btnServerStatus.EnableWindow(TRUE);
		m_editIcoMapUserLimit.EnableWindow(TRUE);
		m_staticLimitText.EnableWindow(TRUE);
	}	
	else
	{
		m_editLMServer.EnableWindow(FALSE);
		m_staticLMText.EnableWindow(FALSE);	
		m_btnServerStatus.EnableWindow(FALSE);
		m_editIcoMapUserLimit.EnableWindow(FALSE);
		m_staticLimitText.EnableWindow(FALSE);
	}
}
