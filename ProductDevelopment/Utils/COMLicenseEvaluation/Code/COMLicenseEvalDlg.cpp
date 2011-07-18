//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	COMLicenseEvalDlg.cpp
//
// PURPOSE:	Implementation of the CCOMLicenseEvalDlg class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#include "stdafx.h"
#include "COMLicenseEval.h"
#include "COMLicenseEvalDlg.h"
#include "LMData.h"
#include "UCLIDCOMPackages.h"
#include "SpecialIcoMap.h"
#include "SpecialSimpleRules.h"
#include "TimeRollbackPreventer.h"

#include <cpputil.h>
#include <StringTokenizer.h>
#include <UCLIDException.h>
#include <EncryptionEngine.h>
#include <RegistryPersistenceMgr.h>

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
// CAboutDlg
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
// CCOMLicenseEvalDlg
//-------------------------------------------------------------------------------------------------
CCOMLicenseEvalDlg::CCOMLicenseEvalDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CCOMLicenseEvalDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CCOMLicenseEvalDlg)
	m_zCode = _T("");
	m_zDate = _T("");
	m_zLicensee = _T("");
	m_zOrganization = _T("");
	m_zIssuer = _T("");
	m_zIssueDate = _T("");
	m_zFile = _T("");
	m_bUseComputerName = FALSE;
	m_bUseSerialNumber = FALSE;
	m_zComputerName = _T("");
	m_zSerialNumber = _T("");
	m_zMACAddress = _T("");
	m_zVersion = _T("");
	m_bUseMACAddress = FALSE;
	m_bUseSpecialPasswords = FALSE;
	m_iType = giMinPasswordType;
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

#ifdef _DEBUG
	// Create registry persistence object
	ma_pSettingsCfgMgr.reset(new RegistryPersistenceMgr(HKEY_CURRENT_USER, gstrUTILITIES_FOLDER));
#endif
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CCOMLicenseEvalDlg)
	DDX_Control(pDX, IDC_LIST_COMPONENTS, m_list);
	DDX_Control(pDX, IDC_BUTTON_PASTE, m_Paste);
	DDX_Control(pDX, IDC_BUTTON_EVALUATE, m_Evaluate);
	DDX_Text(pDX, IDC_EDIT_CODE, m_zCode);
	DDX_Text(pDX, IDC_EDIT_DATE, m_zDate);
	DDX_Text(pDX, IDC_EDIT_LICENSEE, m_zLicensee);
	DDX_Text(pDX, IDC_EDIT_ORGANIZATION, m_zOrganization);
	DDX_Text(pDX, IDC_EDIT_ISSUER, m_zIssuer);
	DDX_Text(pDX, IDC_EDIT_ISSUE_DATE, m_zIssueDate);
	DDX_Text(pDX, IDC_EDIT_FILE, m_zFile);
	DDX_Check(pDX, IDC_CHECK_NAME, m_bUseComputerName);
	DDX_Check(pDX, IDC_CHECK_NUMBER, m_bUseSerialNumber);
	DDX_Text(pDX, IDC_EDIT_COMPUTER_NAME, m_zComputerName);
	DDX_Text(pDX, IDC_EDIT_SERIAL_NUMBER, m_zSerialNumber);
	DDX_Text(pDX, IDC_EDIT_MAC_ADDRESS, m_zMACAddress);
	DDX_Text(pDX, IDC_EDIT_VERSION, m_zVersion);
	DDX_Check(pDX, IDC_CHECK_ADDRESS, m_bUseMACAddress);
	DDX_Check(pDX, IDC_CHECK_SPECIAL, m_bUseSpecialPasswords);
	DDX_Text(pDX, IDC_EDIT_TYPE, m_iType);
	DDV_MinMaxInt(pDX, m_iType, giMinPasswordType, giMaxPasswordType);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCOMLicenseEvalDlg, CDialog)
	//{{AFX_MSG_MAP(CCOMLicenseEvalDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_EN_CHANGE(IDC_EDIT_CODE, OnChangeEditCode)
	ON_BN_CLICKED(IDC_BUTTON_EVALUATE, OnButtonEvaluate)
	ON_BN_CLICKED(IDC_BUTTON_PASTE, OnButtonPaste)
	ON_BN_CLICKED(IDC_BROWSE, OnBrowse)
	ON_BN_CLICKED(IDC_RADIO_UCLID, OnRadioUclid)
	ON_BN_CLICKED(IDC_RADIO_USER, OnRadioUser)
	ON_BN_CLICKED(IDC_CHECK_SPECIAL, OnCheckSpecial)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CCOMLicenseEvalDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CCOMLicenseEvalDlg::OnInitDialog()
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

	///////////////////////////
	// Add single-column header
	///////////////////////////
	// Get dimensions of control
	CRect	rect;
	m_list.GetClientRect( &rect );

	// Add column to list
	m_list.InsertColumn( 0, "Library & Component Name", LVCFMT_LEFT, 
		rect.Width(), 0 );

	// Always default to extraction from the User String
	m_bUserString = true;
	((CButton *)GetDlgItem( IDC_RADIO_USER ))->SetCheck( 1 );

	// Disable Evaluate button, and special password checkbox and edit-box
	m_Evaluate.EnableWindow( FALSE );
	((CButton *)GetDlgItem( IDC_CHECK_SPECIAL ))->EnableWindow( FALSE );
	((CEdit *)GetDlgItem( IDC_EDIT_TYPE ))->EnableWindow( FALSE );

	// Default to expecting a license file
	m_bLicenseFile = true;

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::OnSysCommand(UINT nID, LPARAM lParam)
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
void CCOMLicenseEvalDlg::OnPaint() 
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
HCURSOR CCOMLicenseEvalDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::OnChangeEditCode() 
{
	try
	{
		// Retrieve code from control
		UpdateData( TRUE );

		// Enable or disable the Evaluate button
		if (m_zCode.IsEmpty())
		{
			m_Evaluate.EnableWindow( FALSE );
		}
		else
		{
			m_Evaluate.EnableWindow( TRUE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03907")
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::OnButtonEvaluate() 
{
	try
	{
		// Clear the list
		m_list.DeleteAllItems();

		// Read and validate current settings
		UpdateData( TRUE );
		if ((m_iType < giMinPasswordType) || (m_iType > giMaxPasswordType))
		{
			return;
		}

		// Check file type
		if (m_bLicenseFile)
		{
			// Do extraction from license file
			doExtract();
		}
		else
		{
			// Do extraction from unlock file
			evaluateUnlockFile();
		}

		// Disable the Evaluate button until next change to license string
		// if User String selected
		if (m_bUserString)
		{
			m_Evaluate.EnableWindow( FALSE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03908")
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::OnButtonPaste() 
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

			// Set selection to nothing
			pEdit->SetSel( -1, -1 );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03909")
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::OnBrowse() 
{
	/////////////////////////////
	// Prepare a File Open dialog
	/////////////////////////////
	
	// Create the filters string with all the supported extensions
	char pszStrFilter[] = "License Files (*.lic)|*.lic|Extract License Files (*.esl)|*.esl|Unlock Files (*.txt)|*.txt|All Files (*.*)|*.*||";

	// Construct a default file folder and name
	CString	zFile;
	zFile.Format( "%s*.lic", getTargetFolder().c_str() );

	// Show the file dialog to select the file to be opened
	CFileDialog fileDlg( TRUE, NULL, zFile.operator LPCTSTR(), 
		OFN_READONLY | OFN_HIDEREADONLY, 
		pszStrFilter, this );
	
	if (fileDlg.DoModal() == IDOK)
	{
		// Get the selected file complete path
		m_zFile = fileDlg.GetPathName();

		// Check file extension
		string strExt = getExtensionFromFullPath( m_zFile.operator LPCTSTR() );
		if ((strExt == ".lic") || (strExt == ".esl"))
		{
			// Set flag
			m_bLicenseFile = true;
		}
		else
		{
			// Clear flag
			m_bLicenseFile = false;
		}

		// Display file and path in edit box
		UpdateData( FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::OnRadioUclid() 
{
	// Read current settings
	UpdateData( TRUE );

	// Clear flag
	m_bUserString = false;

	// Enable the special passwords checkbox and edit box
	((CButton *)GetDlgItem( IDC_CHECK_SPECIAL ))->EnableWindow( TRUE );
	((CEdit *)GetDlgItem( IDC_EDIT_TYPE ))->EnableWindow( TRUE );

	// The Evaluate button is enabled for the UCLID string
	// unless special passwords are requested and no type is defined
	if (m_zFile.GetLength() > 0)
	{
		// Are special passwords requested
		if (m_bUseSpecialPasswords)
		{
			// Validate type of special passwords
			if ((m_iType < giMinPasswordType) || (m_iType > giMaxPasswordType))
			{
				m_Evaluate.EnableWindow( FALSE );
				MessageBox( "Invalid type for special passwords", "Error", 
					MB_OK | MB_ICONERROR );
			}
			else
			{
				m_Evaluate.EnableWindow( TRUE );
			}
		}
		// Else use regular passwords
		else
		{
			m_Evaluate.EnableWindow( TRUE );
		}
	}
	// Else no license file specified yet
	else
	{
		m_Evaluate.EnableWindow( FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::OnRadioUser() 
{
	// Set flag
	m_bUserString = true;

	// Disable the special passwords checkbox
	((CButton *)GetDlgItem( IDC_CHECK_SPECIAL ))->EnableWindow( FALSE );

	// Enable or disable the Evaluate button
	if (m_zCode.IsEmpty())
	{
		m_Evaluate.EnableWindow( FALSE );
	}
	else
	{
		// We also need a license file
		if (m_zFile.GetLength() > 0)
		{
			m_Evaluate.EnableWindow( TRUE );
		}
		else
		{
			m_Evaluate.EnableWindow( FALSE );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::OnCheckSpecial() 
{
	// Read current settings
	UpdateData( TRUE );	
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::doExtract() 
{
	unsigned long	ulKey1 = 0;
	unsigned long	ulKey2 = 0;
	unsigned long	ulKey3 = 0;
	unsigned long	ulKey4 = 0;

	// Parse comma-delimited license file password
	if (!m_zCode.IsEmpty())
	{
		vector<string> vecTokens;
		StringTokenizer	tok( ',', false );
		tok.parse( m_zCode.operator LPCTSTR(), vecTokens );
		if (vecTokens.size() != 4)
		{
			// Display prompt to user
			MessageBox( "Please provide a license file password.\r\nFormat: 123456789, 1234567890, 123456789, 1234567890", 
				"Error", MB_ICONEXCLAMATION | MB_OK );

			return;
		}
		else
		{
			ulKey1 = asUnsignedLong( vecTokens[0] );
			ulKey2 = asUnsignedLong( vecTokens[1] );
			ulKey3 = asUnsignedLong( vecTokens[2] );
			ulKey4 = asUnsignedLong( vecTokens[3] );
		}
	}

	// Create the data object
	LMData	lm;

	// Extract the desired data string from the license file
	string	strData;
	strData = lm.unzipStringFromFile( m_zFile.operator LPCTSTR(), m_bUserString );

	try
	{
		// Populate the data object from the license string
		if (m_bUserString)
		{
			// Decrypt the license data with the user passwords
			lm.extractDataFromString( strData.c_str(), ulKey1, ulKey2, ulKey3, 
				ulKey4 );
		}
		else
		{
			// Default passwords to Regular UCLID passwords
			ulKey1 = gulUCLIDKey1;
			ulKey2 = gulUCLIDKey2;
			ulKey3 = gulUCLIDKey3;
			ulKey4 = gulUCLIDKey4;

			if (m_bUseSpecialPasswords)
			{
				// Determine which special passwords are desired
				switch (m_iType)
				{
				case 0:
					// Use default Regular UCLID passwords
					break;

				case 1:
					// Special IcoMap passwords
					ulKey1 = gulIcoMapKey1;
					ulKey2 = gulIcoMapKey2;
					ulKey3 = gulIcoMapKey3;
					ulKey4 = gulIcoMapKey4;
					break;

				case 2:
					// Special Simple Rule Writing passwords
					ulKey1 = gulSimpleRulesKey1;
					ulKey2 = gulSimpleRulesKey2;
					ulKey3 = gulSimpleRulesKey3;
					ulKey4 = gulSimpleRulesKey4;
					break;

				default:
					UCLIDException ue( "ELI12411", "Invalid special password type" );
					ue.addDebugInfo( "Desired Type", m_iType );
					throw ue;
				}
			}
			// else just use the regular passwords

			// Decrypt the license data with the appropriate passwords
			lm.extractDataFromString( strData.c_str(), ulKey1, ulKey2, ulKey3, ulKey4 );
		}
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter( "ELI12141", 
			"Unable to decrypt license string - probably due to incorrect passwords!", ue);
		uexOuter.display();
	}

	/////////////////////////
	// Populate dialog fields
	/////////////////////////
	std::string	strTemp;
	CTime		timeTemp;

	// Version
	strTemp = lm.getVersion();
	m_zVersion = strTemp.c_str();

	// Issuer
	strTemp = lm.getIssuerName();
	m_zIssuer = strTemp.c_str();

	// Licensee
	strTemp = lm.getLicenseeName();
	m_zLicensee = strTemp.c_str();

	// Organization
	strTemp = lm.getOrganizationName();
	m_zOrganization = strTemp.c_str();

	// Issue Date
	timeTemp = lm.getIssueDate();
	m_zIssueDate = timeTemp.Format( "%m/%d/%Y %H:%M:%S" );

	// User License: Computer Name
	m_bUseComputerName = lm.getUseComputerName();
	strTemp = lm.getUserComputerName();
	m_zComputerName = strTemp.c_str();

	// User License: Serial Number
	m_bUseSerialNumber = lm.getUseSerialNumber();
	unsigned long	ulTemp = 0;
	CString	zTemp;
	ulTemp = lm.getUserSerialNumber();
	if (ulTemp != 0)
	{
		zTemp.Format( "%X", ulTemp );
		zTemp.Insert( 4, "-" );
		m_zSerialNumber.Format( "%ld: (%s)", ulTemp, zTemp );
	}
	else
	{
		m_zSerialNumber = "";
	}

	// User License: MAC Address
	m_bUseMACAddress = lm.getUseMACAddress();
	strTemp = lm.getUserMACAddress();
	m_zMACAddress = strTemp.c_str();

	// Populate components list and license state
	doComponents( &lm );

	// Refresh the display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::doComponents(LMData* pData) 
{
	CTime	timeTemp;

	// Create and initialize a COM packages object
	COMPackages	pkg;
	pkg.init();

	// Prepare license state flags
	bool	bFull = false;
	bool	bEval = false;

	// Retrieve the collection of defined components
	std::map<unsigned long,std::string>	mapComp;
	mapComp = pkg.getComponents();

	// Step through collection and check for each object in the license data
	map<unsigned long, std::string>::iterator iterComp;
	for (iterComp = mapComp.begin(); iterComp != mapComp.end(); iterComp++)
	{
		// Check this component's ID
		if (pData->isComponentFound( iterComp->first ))
		{
			// Create string for list
			CString	zTemp;
			zTemp.Format( "%s [%d]", (iterComp->second).c_str(), iterComp->first );

			// Add this component name to the list
			m_list.InsertItem( 0, zTemp );

			// Retrieve this component's license information
			ComponentData	CD = pData->getComponentData( iterComp->first );

			// Set appropriate license flag
			if (CD.m_bIsLicensed)
			{
				// Fully licensed
				bFull = true;
			}
			else
			{
				// Evaluation license
				bEval = true;

				// Store the expiration date
				timeTemp = CD.m_ExpirationDate;
			}
		}
	}

	// Determine which license state button should be set
	if (bFull && !bEval)
	{
		// All components were fully licensed
		CheckRadioButton( IDC_RADIO_FULL, IDC_RADIO_MIXED, IDC_RADIO_FULL );

		// Clear any previous expiration date info
		m_zDate = "";
	}
	else if (bEval && !bFull)
	{
		// All components were licensed in evaluation mode
		CheckRadioButton( IDC_RADIO_FULL, IDC_RADIO_MIXED, IDC_RADIO_EVAL );

		// Also add an expiration date text item to edit box
		// This code assumes that expiration dates are constant across 
		// components with evaluation licenses
		m_zDate = timeTemp.Format( "%m/%d/%Y %H:%M:%S" );
	}
	else
	{
		// Both license states were found
		CheckRadioButton( IDC_RADIO_FULL, IDC_RADIO_MIXED, IDC_RADIO_MIXED );

		// Also add an expiration date text item to edit box
		// This code assumes that expiration dates are constant across 
		// components with evaluation licenses
		m_zDate = timeTemp.Format( "%m/%d/%Y %H:%M:%S" );
	}

	// Check the number of components in the list against number in map
	unsigned long	ulListCount = m_list.GetItemCount();
	unsigned long	ulMapCount = pData->getComponentCount();

	// Add any needed filler items to list
	unsigned int ui;
	for (ui = 1; ui <= ulMapCount - ulListCount; ui++)
	{
		CString	zTemp;
		zTemp.Format( "Unknown component %d", ui );
		m_list.InsertItem( 0, zTemp.operator LPCTSTR() );
	}

	// Check for width of longest string
	long	lMaxWidth = 0;
	long	lWidth = 0;
	CString	zText;
	for (ui = 0; ui < ulListCount; ui++)
	{
		// Get this text
		zText = m_list.GetItemText( ui, 0 );

		// Get width of text rectangle
		lWidth = m_list.GetStringWidth( zText.operator LPCTSTR() );

		// Check for new maximum width
		if (lWidth > lMaxWidth)
		{
			lMaxWidth = lWidth;
		}
	}

	// Correct width of header to fit longest string plus a little more
	HDITEM	hd;
	hd.mask = HDI_WIDTH;
	hd.cxy = lMaxWidth + 20;
	m_list.GetHeaderCtrl()->SetItem( 0, &hd );
}
//-------------------------------------------------------------------------------------------------
void CCOMLicenseEvalDlg::evaluateUnlockFile()
{
	/////////////////////////////
	// Clear unused dialog fields
	/////////////////////////////

	// Version, Issuer, Licensee, Organization, Issue Date
	m_zVersion = "";
	m_zIssuer = "";
	m_zLicensee = "";
	m_zOrganization = "";
	m_zIssueDate = "";

	// Retrieve unlock code from file
	string strCode = getTextFileContentsAsString((LPCTSTR) m_zFile);
	string strUserComputerName(""), strUserMACAddress("");
	unsigned long ulUserSerialNumber;
	CTime tmExpires;

	///////////////////////////////
	// Set meaningful dialog fields
	///////////////////////////////
	if (TimeRollbackPreventer::getIdentityDataFromUnlockStream(strCode,
		strUserComputerName, ulUserSerialNumber, strUserMACAddress, tmExpires))
	{
		// Computer name
		m_zComputerName = strUserComputerName.c_str();

		// Disk serial number
		if (ulUserSerialNumber != 0)
		{
			CString zTemp;
			zTemp.Format( "%X", ulUserSerialNumber );
			zTemp.Insert( 4, "-" );
			m_zSerialNumber.Format( "%ld: (%s)", ulUserSerialNumber, zTemp );
		}
		else
		{
			m_zSerialNumber = "";
		}
	
		// MAC address
		m_zMACAddress = strUserMACAddress.c_str();

		// Expiration date
		CheckRadioButton( IDC_RADIO_FULL, IDC_RADIO_MIXED, IDC_RADIO_EVAL );
		m_zDate = tmExpires.Format( "%m/%d/%Y %H:%M:%S" );
	}
	else
	{
		// Clear the fields
		m_zComputerName = "";
		m_zSerialNumber = "";
		m_zMACAddress = "";
		m_zDate = "";

		// Provide error message to user
		MessageBox( "Invalid Unlock License file - expecting \"UCLID_UnlockLicense.txt\"", 
			"Error", MB_ICONEXCLAMATION | MB_OK );
	}

	// Refresh the display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
std::string	CCOMLicenseEvalDlg::getTargetFolder()
{
	string	strFolder;

	// Special treatment for Debug mode
#ifdef _DEBUG

	// Read Component Data folder setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( gstrLICENSE_SECTION, gstrTARGETFOLDER_KEY ))
	{
		// Create key if not found, default to empty string
		ma_pSettingsCfgMgr->createKey( gstrLICENSE_SECTION, gstrTARGETFOLDER_KEY,
			gstrDEFAULT_TARGETFOLDER );
		strFolder = gstrDEFAULT_TARGETFOLDER;
	}
	else
	{
		strFolder = ma_pSettingsCfgMgr->getKeyValue( gstrLICENSE_SECTION, 
			gstrTARGETFOLDER_KEY, gstrDEFAULT_TARGETFOLDER );
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
			// Check for Extract CommonComponents folder
			if (isFileOrFolderValid( gpszCommonComponentsFolderExtract ))
			{
				// Add path and filename for test file
				zFile.Format( "%sX_%ld.lic", gpszCommonComponentsFolderExtract, time( NULL ) );
				if (canCreateFile( zFile.operator LPCTSTR() ))
				{
					return gpszCommonComponentsFolderExtract;
				}
			}
			// Check for UCLID CommonComponents folder
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
