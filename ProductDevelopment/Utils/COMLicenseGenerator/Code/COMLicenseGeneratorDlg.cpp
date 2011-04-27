//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	COMLicenseGeneratorDlg.cpp
//
// PURPOSE:	Implementation of the CCOMLicenseGeneratorDlg class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#include "stdafx.h"
#include "COMLicenseGenerator.h"
#include "COMLicenseGeneratorDlg.h"
#include "UCLIDCOMPackages.h"
#include "LMData.h"
#include "Shlobj.h"
#include "SpecialIcoMap.h"
#include "SpecialSimpleRules.h"

#include <cpputil.h>
#include <io.h>
#include <UCLIDException.h>
#include <EncryptionEngine.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <StringTokenizer.h>
#include <XBrowseForFolder.h>
#include <RegistryPersistenceMgr.h>
#include <TImeRollbackPreventer.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CAboutDlg dialog used for App About
//-------------------------------------------------------------------------------------------------
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
//-------------------------------------------------------------------------------------------------
CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CAboutDlg)
	//}}AFX_DATA_INIT
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
// CCOMLicenseGeneratorDlg dialog
//-------------------------------------------------------------------------------------------------
CCOMLicenseGeneratorDlg::CCOMLicenseGeneratorDlg(bool bEnableSDK /*= false*/, 
												 CWnd* pParent /*=NULL*/)
	: CDialog(CCOMLicenseGeneratorDlg::IDD, pParent),
	m_bInitialized(false)
{
	//{{AFX_DATA_INIT(CCOMLicenseGeneratorDlg)
	m_zCode = _T("");
	m_zDate = _T("");
	m_zLicensee = _T("");
	m_zOrganization = _T("");
	m_zFile = _T("");
	m_zUser = _T("");
	m_zComputer = _T("");
	m_bUseComputerName = FALSE;
	m_bUseSerialNumber = TRUE;
	m_bUseMACAddress = FALSE;
	m_zOEM = _T("");
	m_zFolder = _T("");
	m_bUseSpecial = FALSE;
	//}}AFX_DATA_INIT

	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	m_pPackageInfo = NULL;

	// Default to evaluation license
	m_bFullyLicensed = false;

	// Default to UCLID password unless SDK is enabled
	m_bEnableSDK = bEnableSDK;
	m_bRandomPassword = m_bEnableSDK;
	m_bSpecifiedPassword = false;

#ifdef _DEBUG
	// Create registry persistence object
	ma_pSettingsCfgMgr.reset(new RegistryPersistenceMgr(HKEY_CURRENT_USER, gstrUTILITIES_FOLDER));
#endif
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CCOMLicenseGeneratorDlg)
	DDX_Control(pDX, IDC_COMBO_TYPE, m_cboType);
	DDX_Control(pDX, IDC_BUTTON_PASTE, m_ctlPaste);
	DDX_Control(pDX, IDC_BUTTON_COPY, m_ctlCopy);
	DDX_Control(pDX, IDC_BUTTON_GENERATE, m_ctlGenerate);
	DDX_Control(pDX, IDC_LIST_PACKAGES, m_list);
	DDX_Text(pDX, IDC_EDIT_CODE, m_zCode);
	DDX_Text(pDX, IDC_EDIT_DATE, m_zDate);
	DDX_Text(pDX, IDC_EDIT_LICENSEE, m_zLicensee);
	DDX_Text(pDX, IDC_EDIT_ORGANIZATION, m_zOrganization);
	DDX_Control(pDX, IDC_CALENDAR, m_calendar);
	DDX_Text(pDX, IDC_EDIT_FILE, m_zFile);
	DDX_Text(pDX, IDC_EDIT_USER, m_zUser);
	DDX_Text(pDX, IDC_EDIT_COMPUTER, m_zComputer);
	DDX_Check(pDX, IDC_CHECK_COMPUTER, m_bUseComputerName);
	DDX_Check(pDX, IDC_CHECK_DISK, m_bUseSerialNumber);
	DDX_Check(pDX, IDC_CHECK_ADDRESS, m_bUseMACAddress);
	DDX_Text(pDX, IDC_EDIT_OEM, m_zOEM);
	DDX_Text(pDX, IDC_EDIT_FOLDER, m_zFolder);
	DDX_Control(pDX, IDC_BUTTON_PASTE_PASSWORD, m_ctlPastePassword);
	DDX_Control(pDX, IDC_BUTTON_UNLOCK, m_ctlUnlock);
	DDX_Check(pDX, IDC_CHECK_SPECIAL, m_bUseSpecial);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCOMLicenseGeneratorDlg, CDialog)
	//{{AFX_MSG_MAP(CCOMLicenseGeneratorDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_COPY, OnButtonCopy)
	ON_BN_CLICKED(IDC_BUTTON_GENERATE, OnButtonGenerate)
	ON_BN_CLICKED(IDC_RADIO_EVAL, OnRadioEval)
	ON_BN_CLICKED(IDC_RADIO_FULL, OnRadioFull)
	ON_LBN_SELCHANGE(IDC_LIST_PACKAGES, OnSelchangeListPackages)
	ON_EN_CHANGE(IDC_EDIT_LICENSEE, OnChangeEditLicensee)
	ON_EN_CHANGE(IDC_EDIT_ORGANIZATION, OnChangeEditOrganization)
	ON_BN_CLICKED(IDC_BUTTON_PASTE, OnButtonPaste)
	ON_BN_CLICKED(IDC_CHECK_COMPUTER, OnCheckComputer)
	ON_BN_CLICKED(IDC_CHECK_DISK, OnCheckDisk)
	ON_BN_CLICKED(IDC_CHECK_ADDRESS, OnCheckAddress)
	ON_BN_CLICKED(IDC_RADIO_RANDOM, OnRadioRandom)
	ON_BN_CLICKED(IDC_RADIO_UCLID, OnRadioUCLID)
	ON_BN_CLICKED(IDC_RADIO_SPECIFIED, OnRadioSpecified)
	ON_BN_CLICKED(IDC_BUTTON_PASTE_PASSWORD, OnButtonPastePassword)
	ON_BN_CLICKED(IDC_BUTTON_UNLOCK, OnButtonUnlock)
	ON_BN_CLICKED(IDC_BUTTON_USERLICENSE, OnButtonUserlicense)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CCOMLicenseGeneratorDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CCOMLicenseGeneratorDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

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

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	// Create and initialize the COMPackages 
	m_pPackageInfo = new COMPackages();
	m_pPackageInfo->init();

	// Populate the list box
	populateList();
	
	// Populate the combo box and select first item
	populateCombo();
	m_cboType.SetCurSel( 0 );
	
	// Initialize the licensing choices
	initLicensing();

	m_bInitialized = true;

	// Disable the Generate and Copy buttons
	updateButtons();
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
BOOL CCOMLicenseGeneratorDlg::DestroyWindow() 
{
	// Clean up the package info data member
	delete m_pPackageInfo;

	return CDialog::DestroyWindow();
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CCOMLicenseGeneratorDlg::OnPaint() 
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
HCURSOR CCOMLicenseGeneratorDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnButtonCopy() 
{
	try
	{
		// Copy license code string to the clipboard
		CEdit*	pEdit = (CEdit *)GetDlgItem( IDC_EDIT_CODE );
		if (pEdit != NULL)
		{
			// Select entire text string
			pEdit->SetSel( 0, -1 );

			// Copy selected text
			pEdit->Copy();	

			// Remove the selection 
			pEdit->SetSel( 0, 0 );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03881")
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnButtonPaste() 
{
	try
	{
		CEdit*	pEdit = (CEdit *)GetDlgItem( IDC_EDIT_USER );
		if (pEdit != NULL)
		{
			// Set selection to entire text
			pEdit->SetSel( 0, -1 );

			// Paste from the clipboard
			pEdit->Paste();	

			// Copy text to data member
			pEdit->GetWindowText( m_zUser );

			// Set selection to nothing
			pEdit->SetSel( -1, -1 );
		}

		// Add quick validation of user license string
		try
		{
			LMData	lmTemp;
			lmTemp.setUserString( m_zUser.operator LPCTSTR() );

			// Retrieve computer name
			m_zComputer = lmTemp.getComputerName().c_str();
		}
		catch(...)
		{
			// Display message to user
			MessageBox( 
				"The User License string you have provided is not a valid license string!",
				"Error",
				MB_OK | MB_ICONERROR );

			// Clear the user license and computer name strings
			m_zUser = "";
			m_zComputer = "";
		}

		UpdateData( FALSE );
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03888")
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnButtonGenerate() 
{
	try
	{
		// Retrieve settings and check password type
		UpdateData( TRUE );
		int iType = m_cboType.GetCurSel();
		if ((iType < giMinPasswordType) || (iType > giMaxPasswordType))
		{
			return;
		}

		/////////////////////////////////
		// Determine name of license file
		/////////////////////////////////

		// Create the filters string with all the supported extensions
		char pszStrFilter[] = "License Files (*.lic)|*.lic|All Files (*.*)|*.*||";

		// Determine folder for license file
		string	strFolder = getTargetFolder();

		// Construct the default file name
		CString	zFile;
		if (!m_bFullyLicensed)
		{
			// Get expiration date
			CString zExpDate;
			zExpDate = m_timeExpire.Format( "%m-%d-%Y" );

			// "Organization_Name_Date.lic" for Evaluation licenses
			zFile.Format( "%s%s_%s_%s.lic", strFolder.c_str(), m_zOrganization, m_zLicensee, 
				zExpDate );
		}
		else
		{
			// "Organization_Name_Full.lic" for non-expiring licenses
			zFile.Format( "%s%s_%s_Full.lic", strFolder.c_str(), m_zOrganization, m_zLicensee );
		}

		////////////////////////////////////////////
		// Check for write permission in this folder
		////////////////////////////////////////////
		if (!canCreateFile( zFile.operator LPCTSTR() ))
		{
			// Add status message
			CString	zPrompt;
			zPrompt.Format( "Unable to create license file in folder \"%s\" - please navigate to a new folder", 
				strFolder.c_str() );
			MessageBox( zPrompt, "Warning", MB_OK | MB_ICONINFORMATION );
		}

		///////////////////////////////////////////////////////
		// Show the file dialog to select the file to be opened
		///////////////////////////////////////////////////////
		CFileDialog fileDlg( FALSE, NULL, zFile.operator LPCTSTR(), 
			OFN_READONLY | OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT, 
			pszStrFilter, this );
		
		// Present file dialog to the user
		CString zFilePath;
		CString zFileName;
		if (fileDlg.DoModal() == IDOK)
		{
			// Get the selected file complete path and filename
			zFilePath = fileDlg.GetPathName();
			zFileName = fileDlg.GetFileName();

			// Make sure that filename has a .LIC extension
			if (zFileName.Find( ".lic", 0 ) == -1)
			{
				zFilePath += ".lic";
				zFileName += ".lic";
			}
		}
		else
		{
			// Just return if user cancels from File dialog
			return;
		}

		////////////////////////////////
		// Prepare components collection
		////////////////////////////////

		// First clear any previous entries in the collected components vector
		m_vecLicensedComponents.clear();

		// Collect component items for each selected package
		for (int i = 0; i < m_list.GetCount(); i++)
		{
			// Check to see if this item is selected
			if (m_list.GetSel( i ) > 0)
			{
				// Get the package name
				CString	zPackage;
				m_list.GetText( i, zPackage );

				// Trim leading whitespace
				string strPackage = trim( zPackage.operator LPCTSTR(), " ", "" );

				// Get this package's collection of associated components
				std::vector<unsigned long>	vecIDs = 
					m_pPackageInfo->getPackageComponents( strPackage );

				// Add them to the overall collection without duplication
				updateComponentCollection( vecIDs );
			}
		}

		//////////////////////////////////////////////
		// Apply basic settings to license data object
		//////////////////////////////////////////////

		// Create a license data object
		LMData	lm;

		// Issue date
		lm.setIssueDateToToday();

		// Issuer name
		lm.setIssuerName( getCurrentUserName() );

		// Licensee name
		lm.setLicenseeName( m_zLicensee.operator LPCTSTR() );

		// Organization name
		lm.setOrganizationName( m_zOrganization.operator LPCTSTR() );

		// Add user license string computer name setting
		lm.setUseComputerName( m_bUseComputerName ? true : false );

		// Add user license string disk drive serial number
		lm.setUseSerialNumber( m_bUseSerialNumber ? true : false );

		// Add user license string MAC address
		lm.setUseMACAddress( m_bUseMACAddress ? true : false );

		// Add user license string, if needed
		if (m_bUseComputerName || m_bUseSerialNumber || m_bUseMACAddress)
		{
			lm.setUserString( m_zUser.operator LPCTSTR() );
		}

		// Add components
		if (m_bFullyLicensed)
		{
			// Loop through components
			std::vector<unsigned long>::iterator iter;
			for (iter = m_vecLicensedComponents.begin(); 
				iter != m_vecLicensedComponents.end(); iter++)
			{
				// Add this component
				lm.addLicensedComponent( *iter );
			}
		}
		else
		{
			// Loop through components
			std::vector<unsigned long>::iterator iter;
			for (iter = m_vecLicensedComponents.begin(); 
				iter != m_vecLicensedComponents.end(); iter++)
			{
				// Add this component using the expiration time data member
				lm.addUnlicensedComponent( *iter, m_timeExpire );
			}
		}

		////////////////////////////////////////////////
		// Generate or retrieve password for User String
		////////////////////////////////////////////////
		unsigned long ulOEM;
		if (m_bRandomPassword)
		{
			generatePassword();

			// Generate OEM password
			ulOEM = lm.generateOEMPassword( m_ulKey1, m_ulKey2, 
				m_ulKey3, m_ulKey4 );
		}
		else if (!m_bSpecifiedPassword)
		{
			// Just use hard-coded UCLID passwords
			m_ulKey1 = gulUCLIDKey5;
			m_ulKey2 = gulUCLIDKey6;
			m_ulKey3 = gulUCLIDKey7;
			m_ulKey4 = gulUCLIDKey8;
		}
		else
		{
			// Passwords were "specified" via Clipboard, 
			// just compute the OEM password
			ulOEM = lm.generateOEMPassword( m_ulKey1, m_ulKey2, 
				m_ulKey3, m_ulKey4 );
		}

		///////////////////////////
		// Encrypt the license data
		///////////////////////////
		string	strLicenseFile( zFilePath.operator LPCTSTR() );
		string	strResult1;
		string	strResult2;

		// Use password
		strResult1 = lm.compressDataToString( m_ulKey1, m_ulKey2, m_ulKey3, 
			m_ulKey4 );

		// Confirm "special" check box if "Default" is chosen
		if (!m_bRandomPassword && !m_bSpecifiedPassword && m_bUseSpecial && (iType == 0))
		{
			int iResult = MessageBox( 
				"\"Special\" password usage has been checked.  Use \"Default\" passwords?", 
				"Confirm Special Passwords", MB_YESNOCANCEL );
			if (iResult != IDYES)
			{
				// User selected No or Cancel, just return without creating a license file
				return;
			}
		}

		// Get password for UCLID String
		if (!m_bRandomPassword && !m_bSpecifiedPassword && m_bUseSpecial && (iType > 0))
		{
			// Determine which special passwords are needed for the UCLID String
			switch (iType)
			{
			case 1:
				// Special IcoMap passwords
				m_ulKey1 = gulIcoMapKey1;
				m_ulKey2 = gulIcoMapKey2;
				m_ulKey3 = gulIcoMapKey3;
				m_ulKey4 = gulIcoMapKey4;
				break;

			case 2:
				// Special Simple Rule Writing passwords
				m_ulKey1 = gulSimpleRulesKey1;
				m_ulKey2 = gulSimpleRulesKey2;
				m_ulKey3 = gulSimpleRulesKey3;
				m_ulKey4 = gulSimpleRulesKey4;
				break;

			default:
				UCLIDException ue( "ELI12420", "Invalid special password type" );
				ue.addDebugInfo( "Desired Type", iType );
				throw ue;
				break;
			}

			// Build the UCLID String
			strResult2 = lm.compressDataToString( m_ulKey1, m_ulKey2, m_ulKey3, m_ulKey4 );
		}
		else
		{
			// Just use regular UCLID passwords for UCLID String
			strResult2 = lm.compressDataToString( gulUCLIDKey1, gulUCLIDKey2, 
				gulUCLIDKey3, gulUCLIDKey4 );
		}

		// Combine the strings
		bool bResult = lm.zipStringsToFile( strLicenseFile, 
			m_strVersion, strResult1, strResult2 );

		// Display filename and password in edit boxes
		if (bResult)
		{
			// License file and folder
			m_zFile = zFileName;
			m_zFolder = (getDirectoryFromFullPath((LPCTSTR)zFilePath) + "\\").c_str();

			// Passwords
			if (m_bRandomPassword)
			{
				m_zCode.Format( "%ld, %ld, %ld, %ld", m_ulKey1, m_ulKey2, m_ulKey3, 
					m_ulKey4 );

				m_zOEM.Format( "%ld", ulOEM );
			}
			else if (!m_bSpecifiedPassword)
			{
				m_zCode.Format( "%s", "<UCLID Password>" );

				m_zOEM.Format( "%s", "<UCLID OEM>" );
			}
			else
			{
				// Retain specified password
				m_zCode.Format( "%ld, %ld, %ld, %ld", m_ulKey1, m_ulKey2, m_ulKey3, 
					m_ulKey4 );

				// Update the OEM edit box
				m_zOEM.Format( "%ld", ulOEM );
			}

			////////////////////////
			// Prepare Password file
			// if SDK or Specified
			////////////////////////
			if (m_bRandomPassword || m_bSpecifiedPassword)
			{
				// Create password filename from license filename
				string	strPWD = getDirectoryFromFullPath( zFilePath.operator LPCTSTR() ) + 
					"\\" + getFileNameWithoutExtension( zFilePath.operator LPCTSTR() );
				strPWD += ".pwd";

				// Prompt user about file creation
				int iResult = MessageBox( "Create password file?", "Confirm Create", 
					MB_YESNO | MB_ICONQUESTION );

				// Act on response
				if (iResult == IDYES)
				{
					// Create the filters string with all the supported extensions
					char pszStrFilter[] = "Password Files (*.pwd)|*.pwd|All Files (*.*)|*.*||";
				
					CFileDialog fileDlg( FALSE, NULL, strPWD.c_str(), 
						OFN_READONLY | OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT, 
						pszStrFilter, this );
					
					// Present file dialog to the user
					CString zFilePath;
					if (fileDlg.DoModal() == IDOK)
					{
						// Get the selected file complete path and filename
						zFilePath = fileDlg.GetPathName();
						zFileName = fileDlg.GetFileName();

						// Make sure that filename has a .PWD extension
						if (zFilePath.Find( ".pwd", 0 ) == -1)
						{
							zFilePath += ".pwd";
						}

						// Construct password string
						CString	zPassword;
						zPassword.Format( "%ld, %ld, %ld, %ld\r\n%s", m_ulKey1, m_ulKey2, 
							m_ulKey3, m_ulKey4, m_zOEM );

						// Write the passwords to the PWD file
						// overwriting an existing file
						ofstream ofs( (LPCTSTR) zFilePath, ios::out | ios::trunc );
						ofs << (LPCTSTR)zPassword;
						ofs.close();
						waitForFileAccess((LPCTSTR) zFilePath, giMODE_READ_ONLY);
					}
				}
			}

#ifdef _DEBUG
			// Save destination folder
			string	strFolder = getDirectoryFromFullPath( 
				zFilePath.operator LPCTSTR() ) + "\\";
			saveTargetFolder( strFolder );
#endif

			// Refresh the display
			UpdateData( FALSE );
		}
		else
		{
			MessageBox( "Unable to create license file", "Error", 
				MB_OK | MB_ICONEXCLAMATION );
		}

		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03879")
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnButtonPastePassword() 
{
	try
	{
		CEdit*	pEdit = (CEdit *)GetDlgItem( IDC_EDIT_CODE );
		if (pEdit != NULL)
		{
			// Set selection to entire text
			pEdit->SetSel( 0, -1 );

			// Paste from the clipboard
			pEdit->Paste();	

			// Copy text to data member
			pEdit->GetWindowText( m_zCode );

			// Set selection to nothing
			pEdit->SetSel( -1, -1 );
		}

		// Add quick validation of Password string
		try
		{
			// Tokenize the string
			vector<string> vecTokens;
			StringTokenizer	st( ',' );
			st.parse( m_zCode.operator LPCTSTR(), vecTokens );

			// Check token count and tokens
			bool bAllValid = false;
			if (vecTokens.size() == 4)
			{
				// Check for 4 non-zero passwords
				m_ulKey1 = asLong( vecTokens[0] );
				m_ulKey2 = asLong( vecTokens[1] );
				m_ulKey3 = asLong( vecTokens[2] );
				m_ulKey4 = asLong( vecTokens[3] );

				if ((m_ulKey1 != 0) && (m_ulKey2 != 1) &&
					(m_ulKey3 != 0) && (m_ulKey4 != 1))
				{
					bAllValid = true;
				}
			}

			if (!bAllValid)
			{
				// Display message to user
				MessageBox( 
					"The Password string you have provided is not a valid password string!",
					"Error",
					MB_OK | MB_ICONERROR );

				// Clear the password string
				m_zCode = "";
			}
		}
		catch(...)
		{
			// Display message to user
			MessageBox( 
				"The Password string you have provided is not a valid password string!",
				"Error",
				MB_OK | MB_ICONERROR );

			// Clear the password string
			m_zCode = "";
		}

		UpdateData( FALSE );
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07606")
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnButtonUnlock() 
{
	try
	{
		/////////////////////////////////
		// Determine name of license file
		/////////////////////////////////
		
		// Determine folder for Unlock file
		string	strFolder = getTargetFolder();

		// Construct the fully qualified Unlock file name
		string strUnlockFile = strFolder + string( gpszDateTimeUnlockFile );

		////////////////////////////////////////////
		// Check for write permission in this folder
		////////////////////////////////////////////
		if (!canCreateFile( strUnlockFile.c_str() ))
		{
			// Add status message
			CString	zPrompt;
			zPrompt.Format( "Unable to create Unlock file in folder \"%s\" - please navigate to a new folder", 
				strFolder.c_str() );
			MessageBox( zPrompt, "Warning", MB_OK | MB_ICONINFORMATION );
		}

		// Create the filters string with all the supported extensions
		char pszStrFilter[] = "Unlock Files (*.txt)|*.txt|All Files (*.*)|*.*||";

		///////////////////////////////
		// Browse to the default folder
		// and select final folder
		///////////////////////////////
		char pszPath[MAX_PATH + 1];
		if (XBrowseForFolder( m_hWnd, strFolder.c_str(), pszPath, sizeof(pszPath) ))
		{
			// Update path to Unlock file
			strUnlockFile = string( pszPath ) + "\\" + 
				string( gpszDateTimeUnlockFile );
		}
		else
		{
			// Just return if user cancels from Browse dialog
			return;
		}

		//////////////////////
		// Prepare Unlock file
		//////////////////////

		// Create byte stream for desired data
		ByteStream bytes;
		ByteStreamManipulator bytesManipulator( ByteStreamManipulator::kWrite, bytes );

		//////////////////////////////////////
		// Add User License string information
		//////////////////////////////////////

		// Use local LMData object to decrypt user license string
		LMData	lmTemp;
		lmTemp.setUserString( m_zUser.operator LPCTSTR() );

		// Add User Computer Name
		bytesManipulator << lmTemp.getUserComputerName();

		// Add Disk Serial Number
		bytesManipulator << lmTemp.getUserSerialNumber();

		// Add MAC Address
		bytesManipulator << lmTemp.getUserMACAddress();

		// Add Expiration Date
		bytesManipulator << m_timeExpire;

		// Convert information to a stream of bytes
		// with length divisible by 8
		bytesManipulator.flushToByteStream( 8 );

		// Encrypt the byte stream
		try
		{
			// Get the encrypted unlock stream
			string strResult = TimeRollbackPreventer::encryptUnlockStream(bytes);

			// Write the encrypted data to the Unlock file, 
			// overwriting an existing file
			ofstream ofs( strUnlockFile.c_str(), ios::out | ios::trunc );
			ofs << strResult;
			ofs.close();
			waitForFileAccess(strUnlockFile, giMODE_READ_ONLY);

#ifdef _DEBUG
			// Save destination folder
			string	strFolder = string( pszPath ) + "\\";
			saveTargetFolder( strFolder );
#endif
		}
		catch(...)
		{
			// Display message to user
			MessageBox( 
				"Unable to create Unlock License file",
				"Error",
				MB_OK | MB_ICONERROR );
		}

		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07609")
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnRadioEval() 
{
	// Show the expiration date picker
	m_calendar.ShowWindow( SW_SHOW );

	// Update data member
	m_bFullyLicensed = false;
	updateButtons();
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnRadioFull() 
{
	// Hide the expiration date picker
	m_calendar.ShowWindow( SW_HIDE );

	// Update data member
	m_bFullyLicensed = true;
	updateButtons();
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnRadioRandom() 
{
	// Update data members
	m_bRandomPassword = true;
	m_bSpecifiedPassword = false;

	// And check button states
	updateButtons();
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnRadioUCLID() 
{
	// Update data members
	m_bRandomPassword = false;
	m_bSpecifiedPassword = false;

	// And check button states
	updateButtons();
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnRadioSpecified() 
{
	// Update data members
	m_bRandomPassword = false;
	m_bSpecifiedPassword = true;

	// And check button states
	updateButtons();
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnCheckComputer() 
{
	// Store the current setting
	UpdateData( TRUE );

	// Just update button states
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnCheckDisk() 
{
	// Store the current setting
	UpdateData( TRUE );

	// Just update button states
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnCheckAddress() 
{
	// Store the current setting
	UpdateData( TRUE );

	// Just update button states
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnSelchangeListPackages() 
{
	// Check for main package selection
	// and unhighlight this item
	int nItem = m_list.GetCurSel();
	CString zText;
	m_list.GetText( nItem, zText );
	if (zText.GetAt( 0 ) != ' ')
	{
		m_list.SetSel( nItem, FALSE );
	}
	// Special selection behavior - faking single-selection
	// if Control key not pressed
	else if (!isVirtKeyCurrentlyPressed( VK_CONTROL ))
	{
		int iSelCount = m_list.GetSelCount();
		if (iSelCount > 1)
		{
			// Deselect every other list item
			long lSize = m_list.GetCount();
			for (int i = 0; i < lSize; i++)
			{
				// Is this item selected AND not the recent selection
				if ((i != nItem) && (m_list.GetSel( i ) > 0))
				{
					// Deselect it
					m_list.SetSel( i, FALSE );
				}
			}
		}
	}

	// Check to see if button states should be changed
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnChangeEditLicensee() 
{
	// Update the data member
	UpdateData( TRUE );

	// Check to see if button states should be changed
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnChangeEditOrganization() 
{
	// Update the data member
	UpdateData( TRUE );

	// Check for invalid slash character
	if (m_zOrganization.Find( '/' ) != -1)
	{
		MessageBox( "Please remove the invalid slash character \"/\"", "Error" );
		return;
	}

	// Check to see if button states should be changed
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnButtonUserlicense() 
{
	// Find folder for EXE
	string	strFQPath = getCurrentProcessEXEDirectory();

	// Append UserLicense.exe
	strFQPath += "\\UserLicense.exe";

	// Verify that EXE is present
	if (isFileOrFolderValid( strFQPath ))
	{
		// Run the utility
		::runEXE( strFQPath );
	}
	else
	{
		// Display error message
		CString	zPrompt;
		zPrompt.Format( "User License utility is not found\r\nPath = \"%s\"", 
			strFQPath.c_str() );
		MessageBox( zPrompt.operator LPCTSTR(), "Error", MB_OK | MB_ICONINFORMATION );
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::populateCombo() 
{
	// Add ordered items to combo box
	m_cboType.InsertString( 0, "Default" );
	m_cboType.InsertString( 1, "IcoMap" );
	m_cboType.InsertString( 2, "Simple Rules" );
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::populateList() 
{
	// Get list of package names from info member
	std::vector<std::string>	vecPackages;
	vecPackages = m_pPackageInfo->getPackages();

	// Add each package name to the list
	std::vector<std::string>::iterator iter;
	for (iter = vecPackages.begin(); 
		iter != vecPackages.end(); iter++)
	{
		// Trim leading dash to allow subsequent whitespace to remain
		std::string	strName = trim( iter->c_str(), "-", "" );
		m_list.AddString( strName.c_str() );
	}
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::generatePassword() 
{
	// Seed the random number generator
	srand( (unsigned)time( NULL ) );

	// Generate four random numbers
	m_ulKey1 = MAKELONG( rand(), rand() );
	m_ulKey2 = MAKELONG( rand(), rand() );
	m_ulKey3 = MAKELONG( rand(), rand() );
	m_ulKey4 = MAKELONG( rand(), rand() );
}
//-------------------------------------------------------------------------------------------------
std::string	CCOMLicenseGeneratorDlg::getTargetFolder()
{
	string	strFolder;

	// Special treatment for Debug mode
#ifdef _DEBUG

	// Read Component Data folder setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( gstrLICENSE_SECTION, gstrTARGETFOLDER_KEY ))
	{
		// Create key if not found, default to empty string
		ma_pSettingsCfgMgr->createKey( gstrLICENSE_SECTION, gstrTARGETFOLDER_KEY, "" );
		strFolder = "";
	}
	else
	{
		strFolder = ma_pSettingsCfgMgr->getKeyValue( gstrLICENSE_SECTION, gstrTARGETFOLDER_KEY );
	}

#endif

	// Do not allow an empty folder
	if (strFolder.empty())
	{
		// Default to network folder
		strFolder = gpszLicenseFolder;

		// Check write permission
		CString	zFile;
		zFile.Format( "%sX_%ld.lic", strFolder.c_str(), time( NULL ) );
		if (!canCreateFile( zFile.operator LPCTSTR() ))
		{
			// Check for CommonComponents folder
			if (isFileOrFolderValid( gpszCommonComponentsFolderExtract ))
			{
				// Add path and filename for test file
				zFile.Format( "%sX_%ld.lic", gpszCommonComponentsFolderExtract, time( NULL ) );
				if (canCreateFile( zFile.operator LPCTSTR() ))
				{
					return gpszCommonComponentsFolderExtract;
				}
			}
			// Check for CommonComponents folder
			if (isFileOrFolderValid( gpszCommonComponentsFolderUCLID ))
			{
				// Add path and filename for test file
				zFile.Format( "%sX_%ld.lic", gpszCommonComponentsFolderUCLID, time( NULL ) );
				if (canCreateFile( zFile.operator LPCTSTR() ))
				{
					return gpszCommonComponentsFolderUCLID;
				}
			}

		}
	}

	return strFolder;
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::initLicensing() 
{
	// Select the appropriate radio buttons
	CheckRadioButton( IDC_RADIO_FULL, IDC_RADIO_EVAL, 
		m_bFullyLicensed ? IDC_RADIO_FULL : IDC_RADIO_EVAL);

	CheckRadioButton( IDC_RADIO_RANDOM, IDC_RADIO_UCLID, 
		m_bRandomPassword ? IDC_RADIO_RANDOM : IDC_RADIO_UCLID);

	// Disable the Random password option, if appropriate
	if (!m_bEnableSDK)
	{
		((CButton *)GetDlgItem( IDC_RADIO_RANDOM ))->EnableWindow( FALSE );
	}

	// Perhaps hide expiration date picker
	if (m_bFullyLicensed)
	{
		m_calendar.ShowWindow( SW_HIDE );
	}

	// Prepare the expiration time data member
	CTime	timeNow = CTime::GetCurrentTime();

	// Advance the date 30 days (default)
	// Default the expiration date to 30 days after today
	CTimeSpan	span( 30, 0, 0, 0 );
	CTime	timeThen = timeNow + span;

	// Make the expiration date the time then but advanced 
	// to one second before midnight
	CTime	timeAdvance( timeThen.GetYear(), timeThen.GetMonth(), 
		timeThen.GetDay(), 23, 59, 59 );
	m_timeExpire = timeAdvance;

	// Set calendar day to this default expiration date
	m_calendar.SetYear( m_timeExpire.GetYear() );
	m_calendar.SetMonth( m_timeExpire.GetMonth() );
	m_calendar.SetDay( m_timeExpire.GetDay() );

	// Create an appropriate date string and display it in the edit box
	m_zDate = m_timeExpire.Format( "%m/%d/%Y" );

	// Default to the current version number
	LMData	lm;
	m_strVersion = lm.getCurrentVersion();

	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
bool CCOMLicenseGeneratorDlg::isDataValid() 
{
	bool	bValid = true;

	// Retrieve all data items
	UpdateData( TRUE );

	// Licensee must be defined
	if (m_zLicensee.IsEmpty())
	{
		bValid = false;
	}

	// Organization must be defined
	if (m_zOrganization.IsEmpty())
	{
		bValid = false;
	}

	// Expiration date must be defined and valid if not fully licensed
	if (!m_bFullyLicensed)
	{
		// Check for expiration date existence
		if (m_zDate.IsEmpty())
		{
			bValid = false;
		}
	}

	// A non-top level package must be selected
	bool bPackageFound = false;
	for (int i = 0; i < m_list.GetCount(); i++)
	{
		// Check selection state
		if (m_list.GetSel( i ) > 0)
		{
			// Retrieve string and check for leading space(s)
			CString	zText;
			m_list.GetText( i, zText );
			if (zText.GetAt( 0 ) == ' ')
			{
				// A valid package has been selected
				bPackageFound = true;
				break;
			}
		}
	}

	if (!bPackageFound)
	{
		bValid = false;
	}

	// If the UCLID password is being used, 
	// at least one user item must be checked
//	if (!m_bRandomPassword)
//	{
//		if (!m_bUseComputerName && !m_bUseSerialNumber && !m_bUseMACAddress)
//		{
//			bValid = false;
//		}
//	}

	// If any User License items are checked, 
	// then a User License string must be present
	if ((m_bUseComputerName || m_bUseSerialNumber || m_bUseMACAddress) && 
		m_zUser.IsEmpty())
	{
		bValid = false;
	}

	return bValid;
}
//-------------------------------------------------------------------------------------------------
bool CCOMLicenseGeneratorDlg::isUnlockDataValid() 
{
	bool	bValid = true;

	/////////////////////////////////////////////////////////////////////////
	// The following items are required to construct an Unlock License file
	// 1. Expiration Date >= Current Date
	// 2. User License string defined
	//
	// NOTE: A regular license file is created as part of the same process so 
	//       isDataValid() must be true to enable the "Generate..." button.
	/////////////////////////////////////////////////////////////////////////

	// Retrieve all data items
	UpdateData( TRUE );

	// Cannot be Fully licensed
	if (m_bFullyLicensed)
	{
		bValid = false;
	}

	// Expiration date must exist and be greater than current time
	if (m_zDate.IsEmpty() || m_timeExpire < CTime::GetCurrentTime())
	{
		bValid = false;
	}

	// User License string must be present
	// NOTE: Validation is done after Paste
	if (m_zUser.IsEmpty())
	{
		bValid = false;
	}

	return bValid;
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::saveTargetFolder(std::string strFolder)
{
#ifdef _DEBUG
	// Write Target folder setting to registry
	ma_pSettingsCfgMgr->setKeyValue( gstrLICENSE_SECTION, gstrTARGETFOLDER_KEY, strFolder );
#endif
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::updateButtons() 
{
	// Enable the Paste button
	m_ctlPaste.EnableWindow( TRUE );

	// Enable generation of Unlock files only if data valid
	if (isUnlockDataValid())
	{
		m_ctlUnlock.EnableWindow( TRUE );
	}
	else
	{
		m_ctlUnlock.EnableWindow( FALSE );
	}

	// Enable Paste Password button only for Specified Password radio button
	if (m_bSpecifiedPassword)
	{
		m_ctlPastePassword.EnableWindow( TRUE );
	}
	else
	{
		m_ctlPastePassword.EnableWindow( FALSE );
	}

	// Enable Special Password checkbox and combobox only for UCLID Password radio button
	if (!m_bSpecifiedPassword && !m_bRandomPassword)
	{
		((CButton *)GetDlgItem( IDC_CHECK_SPECIAL ))->EnableWindow( TRUE );
		((CComboBox *)GetDlgItem( IDC_COMBO_TYPE ))->EnableWindow( TRUE );
	}
	else
	{
		((CButton *)GetDlgItem( IDC_CHECK_SPECIAL ))->EnableWindow( FALSE );
		((CComboBox *)GetDlgItem( IDC_COMBO_TYPE ))->EnableWindow( FALSE );
	}

	// Enable Disk Serial Number checkbox only for Evaluation License
	if (m_bFullyLicensed)
	{
		// Enable Fully Licensed license without disk locking
		// only in bEnableSDK mode
		if (m_bEnableSDK)
		{
			((CButton *)GetDlgItem( IDC_CHECK_DISK ))->EnableWindow( TRUE );
		}
		// Otherwise a Fully Licensed file is always locked to a disk
		else
		{
			// Set serial back to required
			m_bUseSerialNumber = TRUE;
			UpdateData( FALSE );

			((CButton *)GetDlgItem( IDC_CHECK_DISK ))->EnableWindow( FALSE );
		}
	}
	else
	{
		((CButton *)GetDlgItem( IDC_CHECK_DISK ))->EnableWindow( TRUE );
	}

	// Enable the other checkboxes only if user license string exists
	if (m_zUser.GetLength() > 0)
	{
		((CButton *)GetDlgItem( IDC_CHECK_COMPUTER ))->EnableWindow( TRUE );
		((CButton *)GetDlgItem( IDC_CHECK_ADDRESS ))->EnableWindow( TRUE );
	}
	else
	{
		((CButton *)GetDlgItem( IDC_CHECK_COMPUTER ))->EnableWindow( FALSE );
		((CButton *)GetDlgItem( IDC_CHECK_ADDRESS ))->EnableWindow( FALSE );
	}

	// Enable/Disable the Generate Code button
	if (isDataValid())
	{
		m_ctlGenerate.EnableWindow( TRUE );
	}
	else
	{
		m_ctlGenerate.EnableWindow( FALSE );
	}

	// Check for existence of passwords
	if (m_zCode.GetLength() > 0)
	{
		m_ctlCopy.EnableWindow( TRUE );
	}
	else
	{
		m_ctlCopy.EnableWindow( FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::updateComponentCollection(std::vector<unsigned long> vecIDs) 
{
	// Check each ID in the vector
	for (unsigned int ui = 0; ui < vecIDs.size(); ui++)
	{
		// Search the master collection for this ID
		bool bFound = false;
		for (unsigned int uj = 0; uj < m_vecLicensedComponents.size(); uj++)
		{
			if (m_vecLicensedComponents[uj] == vecIDs[ui])
			{
				bFound = true;
				break;
			}
		}

		if (!bFound)
		{
			// Not found, so add this ID
			m_vecLicensedComponents.push_back( vecIDs[ui] );
		}
	}
}

//-------------------------------------------------------------------------------------------------
// Calendar EventSink methods
//-------------------------------------------------------------------------------------------------
BEGIN_EVENTSINK_MAP(CCOMLicenseGeneratorDlg, CDialog)
    //{{AFX_EVENTSINK_MAP(CCOMLicenseGeneratorDlg)
	ON_EVENT(CCOMLicenseGeneratorDlg, IDC_CALENDAR, 1 /* AfterUpdate */, OnAfterUpdateCalendar, VTS_NONE)
	ON_EVENT(CCOMLicenseGeneratorDlg, IDC_CALENDAR, 3 /* NewMonth */, OnAfterNewMonthCalendar, VTS_NONE)
	ON_EVENT(CCOMLicenseGeneratorDlg, IDC_CALENDAR, 4 /* NewYear */, OnAfterNewYearCalendar, VTS_NONE)
	//}}AFX_EVENTSINK_MAP
END_EVENTSINK_MAP()

//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnAfterUpdateCalendar() 
{
	// Retrieve the date selected
	CTime	timeTest( m_calendar.GetYear(), m_calendar.GetMonth(), 
		m_calendar.GetDay(), 23, 59, 59 );

	// Display it in the edit box
	m_zDate = timeTest.Format( "%m/%d/%Y" );
	UpdateData( FALSE );

	// Also store it in expiration data member
	m_timeExpire = timeTest;
	
	// Check to see if button states should be changed
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnAfterNewMonthCalendar() 
{
	if (m_bInitialized)
	{
		// Get HWND of Month combo box
		HWND hwndMonth = ::FindWindowEx( m_calendar.m_hWnd, 0, "ComboBox", NULL );

		// Find index of selected month
		int iMonthIndex = ::SendMessage( hwndMonth, CB_GETCURSEL, 0, 0 );

		// Update the expiration date
		CTime	timeNew( m_timeExpire.GetYear(), iMonthIndex + 1, 
			m_timeExpire.GetDay(), 23, 59, 59 );
		m_timeExpire = timeNew;

		// Set calendar day to this new expiration date
		m_calendar.SetYear( m_timeExpire.GetYear() );
		m_calendar.SetDay( m_timeExpire.GetDay() );

		// Toggle days to force proper day to display depressed
		m_calendar.NextDay();
		m_calendar.PreviousDay();

		// Create an appropriate date string and display it in the edit box
		m_zDate = m_timeExpire.Format( "%m/%d/%Y" );

		UpdateData( FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseGeneratorDlg::OnAfterNewYearCalendar() 
{
	if (m_bInitialized)
	{
		// Get HWND of Month and Year combo boxes
		HWND hwndMonth = ::FindWindowEx( m_calendar.m_hWnd, 0, "ComboBox", NULL );
		HWND hwndYear = ::FindWindowEx( m_calendar.m_hWnd, hwndMonth, "ComboBox", NULL );

		// Find index of selected year
		int iYearIndex = ::SendMessage( hwndYear, CB_GETCURSEL, 0, 0 );

		// Get year text
		char	*pszData = new char[5];
		int iTemp = ::SendMessage( hwndYear, CB_GETLBTEXT, iYearIndex, 
			(LPARAM) (LPCSTR) pszData );

		// Convert year text to long
		if (iTemp != -1)
		{
			int iYear = asLong( string( pszData ) );

			// Update the expiration date
			CTime	timeNew( iYear, m_timeExpire.GetMonth(), 
				m_timeExpire.GetDay(), 23, 59, 59 );
			m_timeExpire = timeNew;

			// Set calendar day to this new expiration date
			m_calendar.SetMonth( m_timeExpire.GetMonth() );
			m_calendar.SetDay( m_timeExpire.GetDay() );

			// Toggle days to force proper day to display depressed
			m_calendar.NextDay();
			m_calendar.PreviousDay();

			// Create an appropriate date string and display it in the edit box
			m_zDate = m_timeExpire.Format( "%m/%d/%Y" );

			UpdateData( FALSE );
		}

		// Release allocated memory
		delete[] pszData;
	}
}
//-------------------------------------------------------------------------------------------------
