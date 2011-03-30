// RDTConfigDlg.cpp : implementation file
//

#include "stdafx.h"
#include "RDTConfig.h"
#include "RDTConfigDlg.h"

#include <RegistryPersistenceMgr.h>
#include <cpputil.h>
#include <XBrowseForFolder.h>
#include <UCLIDException.h>
#include "..\\..\\..\\AFCore\\Code\\Common.h"
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Sections for different settings
const string CRDTConfigDlg::SETTINGS_SECTION = "\\AttributeFinder\\Settings";
const string CRDTConfigDlg::ENTITYFINDER_SECTION = "\\AttributeFinder\\AFUtils\\EntityFinder";
const string CRDTConfigDlg::GRANTORGRANTEE_SECTION = "\\AttributeFinder\\IndustrySpecific\\County\\CountyCustomComponents\\GrantorGranteeFinderV2";
const string CRDTConfigDlg::TESTING_SECTION = "\\TestingFramework\\Settings";
const string CRDTConfigDlg::ENTITYNAMEDATASCORER_SECTION = "\\AttributeFinder\\AFDataScorers\\EntityNameDataScorer";
const string CRDTConfigDlg::SPOTRECOGNITION_SECTION = "\\InputFunnel\\InputReceivers\\SpotRecIR";
const string CRDTConfigDlg::VOAVIEWER_SECTION =  "\\AttributeFinder\\Utils\\EAVGenerator";
const string CRDTConfigDlg::RULETESTER_SECTION = "\\AttributeFinder\\AFCore\\RuleTester";

// Individual key names
const string CRDTConfigDlg::AUTOENCRYPT_KEY = "AutoEncrypt";
const string CRDTConfigDlg::LOADONCE_KEY = "LoadFilePerSession";
const string CRDTConfigDlg::EFALOG_KEY = "LoggingEnabled";
const string CRDTConfigDlg::RULEIDTAG_KEY = "StoreRulesWorked";
const string CRDTConfigDlg::PREFIX_KEY = "Prefix";
const string CRDTConfigDlg::ROOTFOLDER_KEY = "RootTestFilesFolder";
const string CRDTConfigDlg::DATAFOLDER_KEY = "ComponentDataFolder";
const std::string CRDTConfigDlg::SCROLLLOGGER_KEY = "ScrollLogger";
const string CRDTConfigDlg::DIFF_COMMAND_LINE_KEY = "DiffCommandLine";
const string CRDTConfigDlg::ENDSLOG_KEY = "LoggingEnabled";
const string CRDTConfigDlg::DISPLAY_PERCENTAGE_KEY = "DisplayPercentageEnabled";
const string CRDTConfigDlg::AUTOOPENIMAGE_KEY = "AutoOpenImage";
const string CRDTConfigDlg::AUTOEXPANDATTRIBUTES_KEY = "AutoExpandAttributes";

// Default values
const string gstrEMPTY_DEFAULT = "<None>";
const std::string gstrDIFF_COMMAND_DEFAULT = "C:\\Program Files\\KDiff3\\kdiff3.exe %1 %2";

//-------------------------------------------------------------------------------------------------
// CRDTConfigDlg
//-------------------------------------------------------------------------------------------------
CRDTConfigDlg::CRDTConfigDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CRDTConfigDlg::IDD, pParent),
	m_bEditingPrefix(false)
{
	//{{AFX_DATA_INIT(CRDTConfigDlg)
	m_bAutoEncrypt = FALSE;
	m_bEFALog = FALSE;
	m_bENDSLog = FALSE;
	m_bLoadOnce = FALSE;
	m_bRuleIDTag = FALSE;
	m_bScrollLogger = FALSE;
	m_bDisplaySRWPercent = FALSE;
	m_bAutoOpenImage = FALSE;
	//}}AFX_DATA_INIT

	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	// Setup Registry persistence item
	ma_pSettingsCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
		new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrREG_ROOT_KEY ) );

	// Create the MRU List objects
	ma_pRecentPrefixes = unique_ptr<MRUList>( new MRUList( ma_pSettingsCfgMgr.get(), 
			"\\AttributeFinder\\Utils\\RDTConfig\\PrefixList", "Prefix_%d", 8 ));

	ma_pRecentRootFolders = unique_ptr<MRUList>( new MRUList( ma_pSettingsCfgMgr.get(), 
			"\\AttributeFinder\\Utils\\RDTConfig\\RootFolderList", "RFolder_%d", 8 ));

	ma_pRecentDataFolders = unique_ptr<MRUList>( new MRUList( ma_pSettingsCfgMgr.get(), 
			"\\AttributeFinder\\Utils\\RDTConfig\\DataFolderList", "DFolder_%d", 8 ));
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CRDTConfigDlg)
	DDX_Control(pDX, IDC_COMBO_PREFIX, m_comboPrefix);
	DDX_Control(pDX, IDC_COMBO_ROOT, m_comboRoot);
	DDX_Control(pDX, IDC_COMBO_COMPONENT, m_comboData);
	DDX_Check(pDX, IDC_CHECK_AUTOENCRYPT, m_bAutoEncrypt);
	DDX_Check(pDX, IDC_CHECK_EFALOG, m_bEFALog);
	DDX_Check(pDX, IDC_CHECK_ENDSLOG, m_bENDSLog);
	DDX_Check(pDX, IDC_CHECK_LOADONCE, m_bLoadOnce);
	DDX_Check(pDX, IDC_CHECK_RULEIDTAG, m_bRuleIDTag);
	DDX_Check(pDX, IDC_CHECK_SCROLLLOGGER, m_bScrollLogger);
	DDX_Check(pDX, IDC_CHECK_SRW_PERCENT, m_bDisplaySRWPercent);
	DDX_Check(pDX, IDC_CHECK_AUTOOPENIMAGE, m_bAutoOpenImage);
	DDX_Text(pDX, IDC_EDIT_DIFF_COMMAND_LINE, m_zDiffCommandLine);
	DDX_Check(pDX, IDC_CHECK_AUTOEXPAND, m_bAutoExpandAttribute);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRDTConfigDlg, CDialog)
	//{{AFX_MSG_MAP(CRDTConfigDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_APPLY, OnApply)
	ON_BN_CLICKED(IDC_BROWSE_COMPONENT, OnBrowseComponent)
	ON_BN_CLICKED(IDC_BROWSE_ROOT, OnBrowseRoot)
	ON_BN_CLICKED(IDC_DEFAULTS, OnDefaults)
	ON_CBN_EDITUPDATE(IDC_COMBO_PREFIX, OnUpdatePrefix)
	ON_CBN_KILLFOCUS(IDC_COMBO_PREFIX, OnKillFocusPrefix)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CRDTConfigDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CRDTConfigDlg::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();

		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon
		
		// Add default items to combo boxes
		addDefaultItems();

		// Read current settings from registry
		getRegistrySettings();

		// Constrain Root folder combo box to disallow manual editing
		CEdit* pEdit = (CEdit*)(m_comboRoot.GetWindow( GW_CHILD ));
		if (pEdit != __nullptr)
		{
			pEdit->EnableWindow( TRUE );
			pEdit->SetReadOnly( TRUE );
		}

		// Constrain Data folder combo box to disallow manual editing
		pEdit = (CEdit*)(m_comboData.GetWindow( GW_CHILD ));
		if (pEdit != __nullptr)
		{
			pEdit->EnableWindow( TRUE );
			pEdit->SetReadOnly( TRUE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15358")

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CRDTConfigDlg::OnPaint() 
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
HCURSOR CRDTConfigDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::OnApply() 
{
	// Save settings to registry and stay active
	saveRegistrySettings();
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::OnBrowseComponent()
{
	// Get currently selected root folder
	int iIndex = m_comboData.GetCurSel();
	CString	zFolder;
	if (iIndex > -1)
	{
		m_comboData.GetLBText( iIndex, zFolder );
	}

	char pszPath[MAX_PATH + 1];
	if (XBrowseForFolder( m_hWnd, zFolder, pszPath, sizeof(pszPath) ))
	{
		// Add this item to the combo box
		int iIndex = m_comboData.AddString( pszPath );

		// Set selection
		m_comboData.SetCurSel( iIndex );
	}
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::OnBrowseRoot() 
{
	// Get currently selected root folder
	int iIndex = m_comboRoot.GetCurSel();
	CString	zFolder;
	if (iIndex > -1)
	{
		m_comboRoot.GetLBText( iIndex, zFolder );
	}

	char pszPath[MAX_PATH + 1];
	if (XBrowseForFolder( m_hWnd, zFolder, pszPath, sizeof(pszPath) ))
	{
		// Add this item to the combo box
		int iIndex = m_comboRoot.AddString( pszPath );

		// Set selection
		m_comboRoot.SetCurSel( iIndex );
	}
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::OnDefaults() 
{
	// Auto-Encrypt is TRUE [FlexIDSCore #3543]
	m_bAutoEncrypt = TRUE;

	// Load Once Per Session is TRUE
	m_bLoadOnce = TRUE;

	// EFA Logging is FALSE
	m_bEFALog = FALSE;

	// Store Rules Worked is FALSE
	m_bRuleIDTag = FALSE;

	// Display SRW Percentage is FALSE
	m_bDisplaySRWPercent = FALSE;

	// Auto expand attributes is FALSE
	m_bAutoExpandAttribute = FALSE;

	// Set default for prefix
	int iIndex = m_comboPrefix.FindString( -1, gstrEMPTY_DEFAULT.c_str() );
	if (iIndex > -1)
	{
		m_comboPrefix.SetCurSel( iIndex );
	}

	// Set default for Root folder
	iIndex = m_comboRoot.FindString( -1, gstrEMPTY_DEFAULT.c_str() );
	if (iIndex > -1)
	{
		m_comboRoot.SetCurSel( iIndex );
	}

	// Set default for Data folder
	iIndex = m_comboData.FindString( -1, gstrEMPTY_DEFAULT.c_str() );
	if (iIndex > -1)
	{
		m_comboData.SetCurSel( iIndex );
	}

	// Set default for diff command line
	m_zDiffCommandLine = gstrDIFF_COMMAND_DEFAULT.c_str();

	// Refresh display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::OnOK() 
{
	// Save settings to registry and exit application
	saveRegistrySettings();
	
	CDialog::OnOK();
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::OnUpdatePrefix() 
{
	// Set the being edited flag
	m_bEditingPrefix = true;
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::OnKillFocusPrefix() 
{
	// Combo box needs to be updated if it was being edited
	if (m_bEditingPrefix)
	{
		// Get this text
		CString zText;
		m_comboPrefix.GetWindowText( zText );

		// Add this item to the combo box
		int iIndex = m_comboPrefix.AddString( zText.operator LPCTSTR() );
		m_comboPrefix.SetCurSel( iIndex );

		// Reset the flag
		m_bEditingPrefix = false;
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::addDefaultItems()
{
	// If the ComponentDataFolder does not exist
	// display the exception and continue other settings
	try
	{
		// Create a temporary smart pointer from UCLIDCOMUtils lib before creating the 
		// AFUtility object to avoid the crash problem when closing RDTConfig [P16: 2256]
		IIUnknownVectorPtr ipDummy(CLSID_IUnknownVector);

		// Create AFUtility object
		IAFUtilityPtr	ipAFUtils;
		ipAFUtils.CreateInstance( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION( "ELI07696", ipAFUtils != __nullptr );

		// Get default Component Data folder from AFUtils
		string	strDataFolder = ipAFUtils->GetComponentDataFolder();
		m_comboData.AddString( strDataFolder.c_str() );
		m_comboData.SetCurSel( 0 );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15359")

	// Add default string to each combo box
	m_comboPrefix.AddString( gstrEMPTY_DEFAULT.c_str() );
	m_comboRoot.AddString( gstrEMPTY_DEFAULT.c_str() );

	// Default to selection of each default item
	m_comboPrefix.SetCurSel( 0 );
	m_comboRoot.SetCurSel( 0 );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::addStringToMRUList(std::unique_ptr<MRUList> &ma_pList, 
									   const std::string& strNew)
{
	// Keep the list updated
	ma_pList->readFromPersistentStore();
	ma_pList->addItem( strNew );
	ma_pList->writeToPersistentStore();
}
//-------------------------------------------------------------------------------------------------
bool CRDTConfigDlg::getAutoEncrypt()
{
	// Read Auto-Encrypt setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( SETTINGS_SECTION, AUTOENCRYPT_KEY ))
	{
		// Create key if not found, default to true [LRCAU #4926]
		ma_pSettingsCfgMgr->createKey( SETTINGS_SECTION, AUTOENCRYPT_KEY, "1" );
		return true;
	}

	return ma_pSettingsCfgMgr->getKeyValue( SETTINGS_SECTION, AUTOENCRYPT_KEY ) == "1" ? 
		true : false;
}
//-------------------------------------------------------------------------------------------------
string CRDTConfigDlg::getDataFolder()
{
	// Read Component Data folder setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( SETTINGS_SECTION, DATAFOLDER_KEY ))
	{
		// Create key if not found, default to empty string
		ma_pSettingsCfgMgr->createKey( SETTINGS_SECTION, DATAFOLDER_KEY, "" );
		return "";
	}

	return ma_pSettingsCfgMgr->getKeyValue( SETTINGS_SECTION, DATAFOLDER_KEY );
}
//-------------------------------------------------------------------------------------------------
bool CRDTConfigDlg::getDisplaySRWPercent()
{
	// Read SRW percent setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( SPOTRECOGNITION_SECTION, DISPLAY_PERCENTAGE_KEY ))
	{
		// Create key if not found, default to false
		ma_pSettingsCfgMgr->createKey( SPOTRECOGNITION_SECTION, DISPLAY_PERCENTAGE_KEY, 
			"0" );
		return false;
	}

	return ma_pSettingsCfgMgr->getKeyValue( SPOTRECOGNITION_SECTION, DISPLAY_PERCENTAGE_KEY ) == "1" ? 
		true : false;
}
//-------------------------------------------------------------------------------------------------
bool CRDTConfigDlg::getEFALogging()
{
	// Read EFA Logging setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( ENTITYFINDER_SECTION, EFALOG_KEY ))
	{
		// Create key if not found, default to false
		ma_pSettingsCfgMgr->createKey( ENTITYFINDER_SECTION, EFALOG_KEY, "0" );
		return false;
	}

	return ma_pSettingsCfgMgr->getKeyValue( ENTITYFINDER_SECTION, EFALOG_KEY ) == "1" ? 
		true : false;
}
//-------------------------------------------------------------------------------------------------
bool CRDTConfigDlg::getENDSLogging()
{
	// Read EntityNameDataScorer (ENDS) Logging setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( ENTITYNAMEDATASCORER_SECTION, ENDSLOG_KEY ))
	{
		// Create key if not found, default to false
		ma_pSettingsCfgMgr->createKey( ENTITYNAMEDATASCORER_SECTION, ENDSLOG_KEY, "0" );
		return false;
	}

	return ma_pSettingsCfgMgr->getKeyValue( ENTITYNAMEDATASCORER_SECTION, ENDSLOG_KEY ) == "1" ? 
		true : false;
}
//-------------------------------------------------------------------------------------------------
bool CRDTConfigDlg::getLoadOncePerSession()
{
	// Read Load Once Per Session setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( SETTINGS_SECTION, LOADONCE_KEY ))
	{
		// Create key if not found, default to true
		ma_pSettingsCfgMgr->createKey( SETTINGS_SECTION, LOADONCE_KEY, "1" );
		return true;
	}

	return ma_pSettingsCfgMgr->getKeyValue( SETTINGS_SECTION, LOADONCE_KEY ) == "1" ? 
		true : false;
}
//-------------------------------------------------------------------------------------------------
bool CRDTConfigDlg::getAutoOpenImage()
{
	// read auto-highlight setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( VOAVIEWER_SECTION, AUTOOPENIMAGE_KEY))
	{
		// create the key if not found and default to true
		ma_pSettingsCfgMgr->createKey(VOAVIEWER_SECTION, AUTOOPENIMAGE_KEY, "1");
		return false;
	}

	return (ma_pSettingsCfgMgr->getKeyValue(VOAVIEWER_SECTION, AUTOOPENIMAGE_KEY) == "1");
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::getRegistrySettings()
{
	///////////////////////////////
	// Read each item from registry
	///////////////////////////////
	m_bAutoEncrypt = asMFCBool(getAutoEncrypt());

	m_bLoadOnce = asMFCBool(getLoadOncePerSession());

	m_bEFALog = asMFCBool(getEFALogging());

	m_bENDSLog = asMFCBool(getENDSLogging());

	m_bRuleIDTag = asMFCBool(getRuleIDTag());

	m_bScrollLogger = asMFCBool(getScrollLogger());

	m_bDisplaySRWPercent = asMFCBool(getDisplaySRWPercent());

	m_bAutoOpenImage = asMFCBool(getAutoOpenImage());

	m_bAutoExpandAttribute = asMFCBool(getAutoExpandAttributes());

	// Load MRU list items
	loadMRUListItems();

	// Look for previously defined Root folder
	int iIndex = 0;
	string strTemp = getPrefix();
	if (strTemp.empty())
	{
		// Locate the default entry
		iIndex = m_comboPrefix.FindString( -1, gstrEMPTY_DEFAULT.c_str() );
	}
	else
	{
		// Locate this item
		iIndex = m_comboPrefix.FindString( -1, strTemp.c_str() );
	}

	// Set selection
	if (iIndex > -1)
	{
		m_comboPrefix.SetCurSel( iIndex );
	}

	// Look for previously defined Root folder
	strTemp = getRootFolder();
	if (strTemp.empty())
	{
		// Locate the default entry
		iIndex = m_comboRoot.FindString( -1, gstrEMPTY_DEFAULT.c_str() );
	}
	else
	{
		// Locate this item
		iIndex = m_comboRoot.FindString( -1, strTemp.c_str() );
	}

	// Set selection
	if (iIndex > -1)
	{
		m_comboRoot.SetCurSel( iIndex );
	}

	// Look for previously defined Data folder
	strTemp = getDataFolder();
	if (strTemp.empty())
	{
		// Locate the default entry
		iIndex = m_comboData.FindString( -1, gstrEMPTY_DEFAULT.c_str() );
	}
	else
	{
		// Locate this item
		iIndex = m_comboData.FindString( -1, strTemp.c_str() );
	}

	// Set selection
	if (iIndex > -1)
	{
		m_comboData.SetCurSel( iIndex );
	}

	// Get the value for the Diff command line string
	m_zDiffCommandLine = getDiffCommandString();

	// Refresh display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
string CRDTConfigDlg::getPrefix()
{
	// Read Prefix setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( SETTINGS_SECTION, PREFIX_KEY ))
	{
		// Create key if not found, default to empty string
		ma_pSettingsCfgMgr->createKey( SETTINGS_SECTION, PREFIX_KEY, "" );
		return "";
	}

	return ma_pSettingsCfgMgr->getKeyValue( SETTINGS_SECTION, PREFIX_KEY );
}
//-------------------------------------------------------------------------------------------------
string CRDTConfigDlg::getRootFolder()
{
	// Read Automated Testing Root folder setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( TESTING_SECTION, ROOTFOLDER_KEY ))
	{
		// Create key if not found, default to empty string
		ma_pSettingsCfgMgr->createKey( TESTING_SECTION, ROOTFOLDER_KEY, "" );
		return "";
	}

	return ma_pSettingsCfgMgr->getKeyValue( TESTING_SECTION, ROOTFOLDER_KEY );
}
//-------------------------------------------------------------------------------------------------
bool CRDTConfigDlg::getRuleIDTag()
{
	// Read Rule ID Tag setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( GRANTORGRANTEE_SECTION, RULEIDTAG_KEY ))
	{
		// Create key if not found, default to false
		ma_pSettingsCfgMgr->createKey( GRANTORGRANTEE_SECTION, RULEIDTAG_KEY, "0" );
		return false;
	}

	return ma_pSettingsCfgMgr->getKeyValue( GRANTORGRANTEE_SECTION, RULEIDTAG_KEY ) == "1" ? 
		true : false;
}
//-------------------------------------------------------------------------------------------------
bool CRDTConfigDlg::getScrollLogger()
{
	// Read Load Once Per Session setting from registry
	if (!ma_pSettingsCfgMgr->keyExists( TESTING_SECTION, SCROLLLOGGER_KEY ))
	{
		// Create key if not found, default to false
		ma_pSettingsCfgMgr->createKey( TESTING_SECTION, SCROLLLOGGER_KEY, "0" );
		return false;
	}

	return ma_pSettingsCfgMgr->getKeyValue( TESTING_SECTION, SCROLLLOGGER_KEY ) == "1" ? 
		true : false;
}
//-------------------------------------------------------------------------------------------------
CString CRDTConfigDlg::getDiffCommandString()
{
	if( !ma_pSettingsCfgMgr->keyExists(TESTING_SECTION, DIFF_COMMAND_LINE_KEY) )
	{
		// Set the default string if one doesnt exist
		ma_pSettingsCfgMgr->createKey(TESTING_SECTION, DIFF_COMMAND_LINE_KEY, gstrDIFF_COMMAND_DEFAULT);

		// Then return the default string
		return gstrDIFF_COMMAND_DEFAULT.c_str();
	}

	// If a string already exists, return it
	return ma_pSettingsCfgMgr->getKeyValue(TESTING_SECTION, DIFF_COMMAND_LINE_KEY).c_str();
}
//-------------------------------------------------------------------------------------------------
bool CRDTConfigDlg::getAutoExpandAttributes()
{
	if (!ma_pSettingsCfgMgr->keyExists(RULETESTER_SECTION, AUTOEXPANDATTRIBUTES_KEY))
	{
		ma_pSettingsCfgMgr->createKey(RULETESTER_SECTION, AUTOEXPANDATTRIBUTES_KEY, "0");

		return false;
	}
	
	return ma_pSettingsCfgMgr->getKeyValue(RULETESTER_SECTION, AUTOEXPANDATTRIBUTES_KEY) != "0";
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::loadMRUListItems()
{
	////////////////////
	// Load prefix items
	////////////////////

	// Update the list
	ma_pRecentPrefixes->readFromPersistentStore();
	int nSize = ma_pRecentPrefixes->getCurrentListSize();

	// Add each string to the Prefix combo box
	int i;
	for (i = 0; i < nSize ; i++)
	{
		// Retrieve this string
		CString zText( ma_pRecentPrefixes->at( i ).c_str() );

		// Add the string to the combo box at the end
		if (!zText.IsEmpty())
		{
			m_comboPrefix.InsertString( -1, zText.operator LPCTSTR() );
		}
	}

	/////////////////////////
	// Load Root folder items
	/////////////////////////

	// Update the list
	ma_pRecentRootFolders->readFromPersistentStore();
	nSize = ma_pRecentRootFolders->getCurrentListSize();

	// Add each string to the Root folder combo box
	for (i = 0; i < nSize ; i++)
	{
		// Retrieve this string
		CString zText( ma_pRecentRootFolders->at( i ).c_str() );

		// Add the string to the combo box at the end
		if (!zText.IsEmpty())
		{
			m_comboRoot.InsertString( -1, zText.operator LPCTSTR() );
		}
	}

	/////////////////////////
	// Load Data folder items
	/////////////////////////

	// Update the list
	ma_pRecentDataFolders->readFromPersistentStore();
	nSize = ma_pRecentDataFolders->getCurrentListSize();

	// Add each string to the Data folder combo box
	for (i = 0; i < nSize ; i++)
	{
		// Retrieve this string
		CString zText( ma_pRecentDataFolders->at( i ).c_str() );

		// Add the string to the combo box at the end
		if (!zText.IsEmpty())
		{
			m_comboData.InsertString( -1, zText.operator LPCTSTR() );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::saveRegistrySettings()
{
	// Retrieve current settings
	UpdateData( TRUE );

	///////////////////////////////////////
	// Write each checkbox item to registry
	///////////////////////////////////////
	setAutoEncrypt(asCppBool(m_bAutoEncrypt));

	setLoadOncePerSession(asCppBool(m_bLoadOnce));

	setEFALogging(asCppBool(m_bEFALog));

	setENDSLogging(asCppBool(m_bENDSLog));

	setRuleIDTag(asCppBool(m_bRuleIDTag));

	setScrollLogger(asCppBool(m_bScrollLogger));

	setDisplaySRWPercent(asCppBool(m_bDisplaySRWPercent));

	setAutoOpenImage(asCppBool(m_bAutoOpenImage));

	setAutoExpandAttributes(asCppBool(m_bAutoExpandAttribute));

	////////////////////////////////////////////
	// Write selected combobox items to registry
	// and update each MRU list
	////////////////////////////////////////////

	// Set prefix item
	CString zText;
	int iIndex = m_comboPrefix.GetCurSel();
	if (iIndex > -1)
	{
		m_comboPrefix.GetLBText( iIndex, zText );

		if (zText.Compare( gstrEMPTY_DEFAULT.c_str() ) == 0)
		{
			setPrefix( "" );
		}
		else
		{
			setPrefix( zText.operator LPCTSTR() );

			addStringToMRUList( ma_pRecentPrefixes, zText.operator LPCTSTR() );
		}
	}

	// Set Root folder item
	iIndex = m_comboRoot.GetCurSel();
	if (iIndex > -1)
	{
		m_comboRoot.GetLBText( iIndex, zText );

		if (zText.Compare( gstrEMPTY_DEFAULT.c_str() ) == 0)
		{
			setRootFolder( "" );
		}
		else
		{
			setRootFolder( zText.operator LPCTSTR() );

			addStringToMRUList( ma_pRecentRootFolders, zText.operator LPCTSTR() );
		}
	}

	// Save Data folder items
	iIndex = m_comboData.GetCurSel();
	if (iIndex > -1)
	{
		m_comboData.GetLBText( iIndex, zText );

		if (zText.Compare( gstrEMPTY_DEFAULT.c_str() ) == 0)
		{
			setDataFolder( "" );
		}
		else
		{
			setDataFolder( zText.operator LPCTSTR() );

			addStringToMRUList( ma_pRecentDataFolders, zText.operator LPCTSTR() );
		}
	}

	// Save the diff command line editbox to the registry
	setDiffCommandString( m_zDiffCommandLine );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setAutoEncrypt(bool bNewSetting)
{
	// Write Auto-Encrypt setting to registry
	ma_pSettingsCfgMgr->setKeyValue( SETTINGS_SECTION, AUTOENCRYPT_KEY, 
		bNewSetting ? "1" : "0" );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setDataFolder(string strNewFolder)
{
	// Write Component Data folder setting to registry
	ma_pSettingsCfgMgr->setKeyValue( SETTINGS_SECTION, DATAFOLDER_KEY, strNewFolder );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setDisplaySRWPercent(bool bNewSetting)
{
	// Write SRW percent setting to registry
	ma_pSettingsCfgMgr->setKeyValue( SPOTRECOGNITION_SECTION, DISPLAY_PERCENTAGE_KEY, 
		bNewSetting ? "1" : "0" );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setEFALogging(bool bNewSetting)
{
	// Write EFA Logging setting to registry
	ma_pSettingsCfgMgr->setKeyValue( ENTITYFINDER_SECTION, EFALOG_KEY, 
		bNewSetting ? "1" : "0" );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setENDSLogging(bool bNewSetting)
{
	// Write Entity Name Data Scorer (ENDS) Logging setting to registry
	ma_pSettingsCfgMgr->setKeyValue( ENTITYNAMEDATASCORER_SECTION, ENDSLOG_KEY, 
		bNewSetting ? "1" : "0" );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setLoadOncePerSession(bool bNewSetting)
{
	// Write Load Once Per Session setting to registry
	ma_pSettingsCfgMgr->setKeyValue( SETTINGS_SECTION, LOADONCE_KEY, 
		bNewSetting ? "1" : "0" );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setAutoOpenImage(bool bNewSetting)
{
	ma_pSettingsCfgMgr->setKeyValue(VOAVIEWER_SECTION, AUTOOPENIMAGE_KEY,
		bNewSetting ? "1" : "0");
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setPrefix(string strNewPrefix)
{
	// Write Prefix setting to registry
	ma_pSettingsCfgMgr->setKeyValue( SETTINGS_SECTION, PREFIX_KEY, strNewPrefix );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setRootFolder(string strNewFolder)
{
	// Write Automated Testing Root folder setting to registry
	ma_pSettingsCfgMgr->setKeyValue( TESTING_SECTION, ROOTFOLDER_KEY, strNewFolder );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setRuleIDTag(bool bNewSetting)
{
	// Write Rule ID Tag setting to registry
	ma_pSettingsCfgMgr->setKeyValue( GRANTORGRANTEE_SECTION, RULEIDTAG_KEY, 
		bNewSetting ? "1" : "0" );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setScrollLogger(bool bNewSetting)
{
	// Write Scroll Logger setting to registry
	ma_pSettingsCfgMgr->setKeyValue( TESTING_SECTION, SCROLLLOGGER_KEY, 
		bNewSetting ? "1" : "0" );
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setDiffCommandString( CString zDiffString )
{
	// Set the registry setting to the new key
	ma_pSettingsCfgMgr->setKeyValue( TESTING_SECTION, DIFF_COMMAND_LINE_KEY,
										(LPCTSTR)zDiffString);
}
//-------------------------------------------------------------------------------------------------
void CRDTConfigDlg::setAutoExpandAttributes(bool bNewSetting)
{
	ma_pSettingsCfgMgr->setKeyValue(RULETESTER_SECTION, AUTOEXPANDATTRIBUTES_KEY,
		bNewSetting ? "1" : "0");
}
//-------------------------------------------------------------------------------------------------
