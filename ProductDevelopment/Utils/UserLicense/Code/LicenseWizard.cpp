// LicenseWizard.cpp : implementation file

#include "stdafx.h"
#include "UserLicense.h"
#include "LicenseWizard.h"
#include "LicenseRequest.h"
#include "Step1.h"
#include "Step2.h"
#include "Step3Automatic.h"
#include "Step3Manual.h"

#include <cpputil.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// CAboutDlg class
//-------------------------------------------------------------------------------------------------
class CAboutDlg : public CDialog
{
public:
	CAboutDlg(const string& strVersion);

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
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	string	m_strVersion;
};

//-------------------------------------------------------------------------------------------------
// CAboutDlg
//-------------------------------------------------------------------------------------------------
CAboutDlg::CAboutDlg(const string& strVersion) 
: CDialog(CAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CAboutDlg)
	//}}AFX_DATA_INIT
	m_strVersion = strVersion;
}
//-------------------------------------------------------------------------------------------------
void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
		// No message handlers
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CAboutDlg Message Handlers
//-------------------------------------------------------------------------------------------------
BOOL CAboutDlg::OnInitDialog() 
{
	try
	{	
		CDialog::OnInitDialog();
		
		// Provide the Version string
		SetDlgItemText( IDC_EDIT_VERSION, m_strVersion.c_str() );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07865")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

//--------------------------------------------------------------------------------------------------
// CLicenseWizard
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CLicenseWizard, CPropertySheet)
//--------------------------------------------------------------------------------------------------
CLicenseWizard::CLicenseWizard(UINT nIDCaption, CWnd* pParentWnd, UINT iSelectPage)
	:CPropertySheet(nIDCaption, pParentWnd, iSelectPage)
{
}
//--------------------------------------------------------------------------------------------------
CLicenseWizard::CLicenseWizard(LPCTSTR pszCaption, CWnd* pParentWnd, UINT iSelectPage)
	:CPropertySheet(pszCaption, pParentWnd, iSelectPage)
{
}
//--------------------------------------------------------------------------------------------------
CLicenseWizard::~CLicenseWizard()
{
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CLicenseWizard, CPropertySheet)
	ON_WM_SYSCOMMAND()
	ON_BN_CLICKED(IDC_BTN_OPEN_LICENSE_FOLDER, OnOpenLicenseFolder)
END_MESSAGE_MAP()	

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
BOOL CLicenseWizard::OnInitDialog()
{
	BOOL bResult;
	
	try
	{
		bResult = CPropertySheet::OnInitDialog();

		// Add "About..." menu item to system menu.

		// IDM_ABOUTBOX must be in the system command range.
		ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
		ASSERT(IDM_ABOUTBOX < 0xF000);

		CMenu* pSysMenu = GetSystemMenu(FALSE);
		if (pSysMenu != NULL)
		{
			CString strAboutMenu;
			strAboutMenu.LoadString(IDS_ABOUTBOX);
			if (!strAboutMenu.IsEmpty())
			{
				pSysMenu->AppendMenu(MF_SEPARATOR);
				pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
			}
		}

		// Set the icon for the application
		HICON hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

		SetIcon(hIcon , TRUE);		// Set big icon
		SetIcon(hIcon , FALSE);		// Set small icon

		// [LegacyRCAndUtils:6075]
		// Add button to make it convenient to get to the license folder.
		// Position the button so it is aligned vertically with the other buttons on the wizard
		// sheet, but is on the left-hand side.
		CRect rectButton;
		GetDlgItem(IDCANCEL)->GetWindowRect(rectButton);
		ScreenToClient(rectButton);		

		CRect rectWizardSheet;
		GetTabControl()->GetWindowRect(rectWizardSheet);
		ScreenToClient(rectWizardSheet);

		rectButton.left = rectWizardSheet.left;
		// Make the button wide enough for "Open License Folder"
		rectButton.right = rectWizardSheet.left + 120; 

		m_BtnOpenLicenseFolder.Create("Open License Folder",
			BS_PUSHBUTTON|WS_CHILD|WS_VISIBLE|WS_TABSTOP, rectButton, this,
			IDC_BTN_OPEN_LICENSE_FOLDER);
		m_BtnOpenLicenseFolder.SetFont(GetFont());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23351")

	return bResult;
}
//--------------------------------------------------------------------------------------------------
int CLicenseWizard::DoModal()
{
	int nRes;

	try
	{
		// Disable the help button
		m_psh.dwFlags &= ~(PSH_HASHELP);

		// Create a license request instance.
		CLicenseRequest licenseRequest;

		// Create the property pages
		CStep1 pageStep1(licenseRequest);
		CStep2 pageStep2(licenseRequest);
		CStep3Automatic pageStep3Automatic(licenseRequest);
		CStep3Manual pageStep3Manual(licenseRequest);

		// Add the pages to the wizard
		AddPage(&pageStep1);
		AddPage(&pageStep2);
		AddPage(&pageStep3Automatic);
		AddPage(&pageStep3Manual);

		// Set the wizard mode.
		SetWizardMode();

		// Run the wizard.
		nRes = CPropertySheet::DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23329");

	return nRes;
}

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
void CLicenseWizard::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		// Retrieve version information from this EXE
		string strVersion( "UserLicense Version " );
		strVersion += ::getFileVersion( getCurrentProcessEXEFullPath() );
		
		// Display the About box with version information
		CAboutDlg dlgAbout( strVersion.c_str() );
		dlgAbout.DoModal();
	}
	else
	{
		CPropertySheet::OnSysCommand(nID, lParam);
	}
}
//--------------------------------------------------------------------------------------------------
void CLicenseWizard::OnOpenLicenseFolder()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		shellOpenDocument(getExtractLicenseFilesPath());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32572");
}
//--------------------------------------------------------------------------------------------------
