// ConfigDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "ConfigDlg.h"

#include <UCLIDException.h>
#include <XBrowseForFolder.h>
#include <StringTokenizer.h>
#include <DateUtil.h>
#include <RegistryPersistenceMgr.h>
#include <PromptDlg.h>
#include "..\\..\\..\\AFCore\Code\Common.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrAF_UTILS_KEY = gstrAF_REG_ROOT_FOLDER_PATH + string("\\Utils");


//-------------------------------------------------------------------------------------------------
// CConfigDlg dialog
//-------------------------------------------------------------------------------------------------
CConfigDlg::CConfigDlg(IFeedbackMgrInternalsPtr ipFBMgr, CWnd* pParent /*=NULL*/)
	: CDialog(CConfigDlg::IDD, pParent),
	m_ipFBMgr(ipFBMgr)
{
	try
	{
		//{{AFX_DATA_INIT(CConfigDlg)
		m_bEnabled = FALSE;
		m_bConvertToText = FALSE;
		m_bTurnOff = FALSE;
		m_zDate = _T("");
		m_zCount = _T("");
		m_zFolder = _T("");
		m_bAllAttributes = 0;
		m_bNoCollect = 0;
		m_bOnDate = 0;
	m_zSkipCount = _T("");
	//}}AFX_DATA_INIT

		// Get Persistence Manager for dialog
		ma_pUserCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER,gstrAF_UTILS_KEY ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI09156", ma_pUserCfgMgr.get() != __nullptr );
		
		ma_pCfgFeedbackMgr = unique_ptr<PersistenceMgr>(new PersistenceMgr( 
			ma_pUserCfgMgr.get(), "\\FeedbackManager" ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI09157", ma_pCfgFeedbackMgr.get() != __nullptr );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08025")
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CConfigDlg)
	DDX_Control(pDX, IDC_LIST_ATTRIBUTES, m_list);
	DDX_Control(pDX, IDC_BTN_DELETE, m_btnDelete);
	DDX_Control(pDX, IDC_BTN_MODIFY, m_btnModify);
	DDX_Control(pDX, IDC_BTN_ADD, m_btnAdd);
	DDX_Check(pDX, IDC_CHECK_ENABLE, m_bEnabled);
	DDX_Check(pDX, IDC_CHECK_TOTEXT, m_bConvertToText);
	DDX_Check(pDX, IDC_CHECK_TURNOFF, m_bTurnOff);
	DDX_Text(pDX, IDC_EDIT_DATE, m_zDate);
	DDX_Text(pDX, IDC_EDIT_COUNT, m_zCount);
	DDX_Text(pDX, IDC_EDIT_FOLDER, m_zFolder);
	DDX_Radio(pDX, IDC_RADIO_ALL, m_bAllAttributes);
	DDX_Radio(pDX, IDC_RADIO_NOCOLLECT, m_bNoCollect);
	DDX_Radio(pDX, IDC_RADIO_ONDATE, m_bOnDate);
	DDX_Text(pDX, IDC_EDIT_SKIPCOUNT, m_zSkipCount);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CConfigDlg, CDialog)
	//{{AFX_MSG_MAP(CConfigDlg)
	ON_BN_CLICKED(IDC_BTN_BROWSEFOLDER, OnBtnBrowse)
	ON_BN_CLICKED(IDC_BTN_CLEAR, OnBtnClear)
	ON_BN_CLICKED(IDC_CHECK_TURNOFF, OnCheckTurnoff)
	ON_BN_CLICKED(IDC_CHECK_ENABLE, OnCheckEnable)
	ON_BN_CLICKED(IDC_RADIO_ONDATE, OnRadioOndate)
	ON_BN_CLICKED(IDC_RADIO_AFTER, OnRadioAfter)
	ON_BN_CLICKED(IDC_RADIO_NOCOLLECT, OnRadioNocollect)
	ON_BN_CLICKED(IDC_RADIO_ATEXECUTION, OnRadioAtexecution)
	ON_BN_CLICKED(IDC_RADIO_ATPACKAGING, OnRadioAtpackaging)
	ON_BN_CLICKED(IDC_BTN_ADD, OnBtnAdd)
	ON_BN_CLICKED(IDC_BTN_DELETE, OnBtnDelete)
	ON_BN_CLICKED(IDC_BTN_MODIFY, OnBtnModify)
	ON_BN_CLICKED(IDC_RADIO_ALL, OnRadioAll)
	ON_BN_CLICKED(IDC_RADIO_SOME, OnRadioSome)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CConfigDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CConfigDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		// Read stored settings
		readRegistrySettings();

		enableButtonStates();

		// Define a column header for the list
		m_list.InsertColumn( 0, "Attribute Name" );

		// Adjust the column width of the list
		CRect rect;
		m_list.GetClientRect( &rect );
		m_list.SetColumnWidth( 0, rect.Width() );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18600");

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnBtnBrowse() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		UpdateData( TRUE );

		// Display folder selection dialog
		char pszPath[MAX_PATH + 1];
		if (XBrowseForFolder( m_hWnd, m_zFolder, pszPath, sizeof(pszPath) ))
		{
			// Check for any files already in the Feedback folder
			std::vector<std::string>	vecFiles;
			getFilesInDir( vecFiles, pszPath, "*.*", false );
			long lCount = vecFiles.size();
			if (lCount > 0)
			{
				CString zPrompt;
				zPrompt.Format( "The %d files that are presently in the selected Feedback folder may be deleted after packaging.  Continue?", 
					lCount );
				int iResult = MessageBox( zPrompt, "Warning", MB_YESNOCANCEL | MB_ICONQUESTION );
				if (iResult == IDNO || iResult == IDCANCEL)
				{
					return;
				}
			}

			// Refresh the display
			m_zFolder = pszPath;
			UpdateData( FALSE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07997")
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnBtnClear() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Call method on interface
		if (m_ipFBMgr)
		{
			m_ipFBMgr->ClearFeedbackData( VARIANT_TRUE );

			// Close the database connection to avoid possible conflict
			m_ipFBMgr->CloseConnection();
		}
		else
		{
			// Throw exception
			UCLIDException ue( "ELI10358", "Unable to clear feedback data." );
			throw ue;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09122")
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnOK() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Retrieve current settings
		UpdateData( TRUE );

		// Check for folder definition
		if (m_bEnabled)
		{
			if (m_zFolder.IsEmpty())
			{
				MessageBox( "A feedback collection folder must be selected", "Error", 
					MB_ICONEXCLAMATION | MB_OK );
				return;
			}
		}

		// Check automatic turn-off item
		if (m_bTurnOff)
		{
			// Check for date
			if (m_bOnDate == 0)
			{
				bool bValidDate = false;

				// Empty date string
				if (!m_zDate.IsEmpty())
				{
					long lMonth = -1;
					long lDay = -1;
					long lYear = -1;
					if (isValidDate( LPCTSTR(m_zDate), &lMonth, &lDay, &lYear ))
					{
						// Store valid date in standard format
						bValidDate = true;
						m_zDate.Format( "%d/%d/%d", lMonth, lDay, lYear );
						UpdateData( FALSE );
					}
				}

				if (!bValidDate)
				{
					// Provide error message to user
					MessageBox( "A valid stop date for feedback collection must be defined", 
						"Error", MB_ICONEXCLAMATION | MB_OK );

					// Set focus to invalid date and return
					(GetDlgItem( IDC_EDIT_DATE ))->SetFocus();
					((CEdit *)GetDlgItem( IDC_EDIT_DATE ))->SetSel( 0, -1 );
					return;
				}
			}			// end if turn off after date
			else
			{
				bool bValidCount = false;
				long lCount = 0;

				if (!m_zCount.IsEmpty())
				{
					lCount = asLong( LPCTSTR(m_zCount) );
					if (lCount > 0)
					{
						// TODO: Store valid count

						// Set flag
						bValidCount = true;
					}
				}

				if (!bValidCount)
				{
					// Provide error message to user
					MessageBox( "A valid count of rule executions for feedback collection must be defined", 
						"Error", MB_ICONEXCLAMATION | MB_OK );

					// Set focus to invalid count and return
					(GetDlgItem( IDC_EDIT_COUNT ))->SetFocus();
					((CEdit *)GetDlgItem( IDC_EDIT_COUNT ))->SetSel( 0, -1 );
					return;
				}
			}			// end else turn off after count
		}
	
		// Save results to registry
		writeRegistrySettings();

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08014")
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnCheckTurnoff() 
{
	// Update button states
	enableButtonStates();
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnCheckEnable() 
{
	// Update button states
	enableButtonStates();
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnRadioOndate() 
{
	// Update button states
	enableButtonStates();
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnRadioAfter() 
{
	// Update button states
	enableButtonStates();
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnRadioNocollect() 
{
	// Update button states
	enableButtonStates();
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnRadioAtexecution() 
{
	// Update button states
	enableButtonStates();
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnRadioAtpackaging() 
{
	// Update button states
	enableButtonStates();
}
//-------------------------------------------------------------------------------------------------
// These OnBtn methods are present only for testing purposes
// Functionality is not complete because the controls are currently disabled
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnBtnAdd()
{
	// TODO: Add new Attribute name to list

	// Adjust the column width in case there is a vertical scrollbar now
//	CRect rect;
//	m_list.GetClientRect( &rect );
//	m_list.SetColumnWidth( 0, rect.Width() );
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnBtnDelete() 
{
	// TODO: Remove selected Attribute name from list after confirmation

	// Adjust the column width in case there is a vertical scrollbar now
//	CRect rect;
//	m_list.GetClientRect( &rect );
//	m_list.SetColumnWidth( 0, rect.Width() );
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnBtnModify() 
{
	// TODO: Modify selected Attribute name
}
//-------------------------------------------------------------------------------------------------
// These OnRadio methods are present only for testing purposes
// Functionality is not complete because only the All Attributes button is currently enabled
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnRadioAll() 
{
	// Update button states
	enableButtonStates();
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::OnRadioSome() 
{
	// Update button states
	enableButtonStates();
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CConfigDlg::enableButtonStates() 
{
	// Get current settings
	UpdateData( TRUE );

	// Check top-level Enabled checkbox
	if (m_bEnabled)
	{
		// Browse button
		(GetDlgItem( IDC_BTN_BROWSE ))->EnableWindow( TRUE );

		///////////////////////////
		// Automatic turn-off items
		///////////////////////////
		(GetDlgItem( IDC_CHECK_TURNOFF ))->EnableWindow( TRUE );

		if (m_bTurnOff)
		{
			// Enable radio buttons
			(GetDlgItem( IDC_RADIO_ONDATE ))->EnableWindow( TRUE );
			(GetDlgItem( IDC_RADIO_AFTER ))->EnableWindow( TRUE );

			// Enable date edit box if On Date
			(GetDlgItem( IDC_EDIT_DATE ))->EnableWindow( m_bOnDate ? FALSE : TRUE );

			// Disable count edit box if On Date
			(GetDlgItem( IDC_EDIT_COUNT ))->EnableWindow( m_bOnDate ? TRUE : FALSE );
		}
		else
		{
			// Disable radio buttons and edit boxes
			(GetDlgItem( IDC_RADIO_ONDATE ))->EnableWindow( FALSE );
			(GetDlgItem( IDC_RADIO_AFTER ))->EnableWindow( FALSE );
			(GetDlgItem( IDC_EDIT_DATE ))->EnableWindow( FALSE );
			(GetDlgItem( IDC_EDIT_COUNT ))->EnableWindow( FALSE );
		}

		(GetDlgItem( IDC_EDIT_SKIPCOUNT ))->EnableWindow( TRUE );

		///////////////////
		// Collection items
		///////////////////
		(GetDlgItem( IDC_RADIO_NOCOLLECT ))->EnableWindow( TRUE );
		(GetDlgItem( IDC_RADIO_ATEXECUTION ))->EnableWindow( TRUE );
		(GetDlgItem( IDC_RADIO_ATPACKAGING ))->EnableWindow( TRUE );

		// Enable or disable text check box
		(GetDlgItem( IDC_CHECK_TOTEXT ))->EnableWindow( (m_bNoCollect == 1) ? TRUE : FALSE );

		//////////////////
		// Selection items
		//////////////////
		(GetDlgItem( IDC_RADIO_ALL ))->EnableWindow( TRUE );

		if (m_bAllAttributes)
		{
			// Enable list box and buttons
			(GetDlgItem( IDC_LIST_ATTRIBUTES ))->EnableWindow( TRUE );
			(GetDlgItem( IDC_BTN_ADD ))->EnableWindow( TRUE );
			(GetDlgItem( IDC_BTN_MODIFY ))->EnableWindow( TRUE );
			(GetDlgItem( IDC_BTN_DELETE ))->EnableWindow( TRUE );
		}
		else
		{
			// Disable list box and buttons
			(GetDlgItem( IDC_LIST_ATTRIBUTES ))->EnableWindow( FALSE );
			(GetDlgItem( IDC_BTN_ADD ))->EnableWindow( FALSE );
			(GetDlgItem( IDC_BTN_MODIFY ))->EnableWindow( FALSE );
			(GetDlgItem( IDC_BTN_DELETE ))->EnableWindow( FALSE );
		}
	}
	else
	{
		//////////////////////////
		// Disable everything else
		//////////////////////////

		// Browse button
		(GetDlgItem( IDC_BTN_BROWSE ))->EnableWindow( FALSE );

		// Disable automatic turn-off items
		(GetDlgItem( IDC_CHECK_TURNOFF ))->EnableWindow( FALSE );
		(GetDlgItem( IDC_RADIO_ONDATE ))->EnableWindow( FALSE );
		(GetDlgItem( IDC_RADIO_AFTER ))->EnableWindow( FALSE );
		(GetDlgItem( IDC_EDIT_DATE ))->EnableWindow( FALSE );
		(GetDlgItem( IDC_EDIT_COUNT ))->EnableWindow( FALSE );

		// Disable Document collection items
		(GetDlgItem( IDC_RADIO_NOCOLLECT ))->EnableWindow( FALSE );
		(GetDlgItem( IDC_RADIO_ATEXECUTION ))->EnableWindow( FALSE );
		(GetDlgItem( IDC_RADIO_ATPACKAGING ))->EnableWindow( FALSE );
		(GetDlgItem( IDC_CHECK_TOTEXT ))->EnableWindow( FALSE );
		(GetDlgItem( IDC_EDIT_SKIPCOUNT ))->EnableWindow( FALSE );

		// Disable Attribute selection items
		(GetDlgItem( IDC_RADIO_ALL ))->EnableWindow( FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::readRegistrySettings() 
{
	// Get enabled setting
	m_bEnabled = ma_pCfgFeedbackMgr->getFeedbackEnabled();

	// Get feedback folder
	m_zFolder = (ma_pCfgFeedbackMgr->getFeedbackFolder()).c_str();

	// Get automatic turn-off enabled setting
	m_bTurnOff = ma_pCfgFeedbackMgr->getAutoTurnOffEnabled();

	// Get automatic turn-off settings
	if (m_bTurnOff)
	{
		string strTemp = ma_pCfgFeedbackMgr->getTurnOffDate();
		if (!strTemp.empty())
		{
			// Save as Turn-Off Date
			m_zDate = strTemp.c_str();

			// Set radio button
			m_bOnDate = 0;
		}
		else
		{
			// Retrieve Turn-Off Count
			m_zCount = asString( ma_pCfgFeedbackMgr->getTurnOffCount() ).c_str();

			// Set radio button
			m_bOnDate = 1;
		}
	}

	// Retrieve Skip Count
	m_zSkipCount = asString( ma_pCfgFeedbackMgr->getSkipCount() ).c_str();

	// Get document collection setting
	m_bNoCollect = ma_pCfgFeedbackMgr->getDocumentCollection();

	// Get document conversion setting
	m_bConvertToText = ma_pCfgFeedbackMgr->getDocumentConversion();

	// Get attribute selection setting
	m_bAllAttributes = ma_pCfgFeedbackMgr->getAttributeSelection();

	// TODO: Retrieve and display stored attributes
//	ma_pCfgFeedbackMgr->getAttributeNames();

	// Refresh display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CConfigDlg::writeRegistrySettings() 
{
	UpdateData( TRUE );

	// Store enabled setting
	ma_pCfgFeedbackMgr->setFeedbackEnabled( (m_bEnabled == TRUE) ? true : false );

	// Store feedback folder
	ma_pCfgFeedbackMgr->setFeedbackFolder( LPCTSTR(m_zFolder) );

	// Store automatic turn-off enabled setting
	ma_pCfgFeedbackMgr->setAutoTurnOffEnabled( (m_bTurnOff == TRUE) ? true : false );

	// Store Skip count
	ma_pCfgFeedbackMgr->setSkipCount( asLong( LPCTSTR(m_zSkipCount) ) );

	// Store or Clear automatic turn-off settings
	if (m_bTurnOff)
	{
		// Is Date defined?
		if (m_bOnDate == 0)
		{
			// Store date
			ma_pCfgFeedbackMgr->setTurnOffDate( LPCTSTR(m_zDate) );

			// Set count to 0
			ma_pCfgFeedbackMgr->setTurnOffCount( 0 );
		}
		else
		{
			// Clear date
			ma_pCfgFeedbackMgr->setTurnOffDate( "" );

			// Store count
			ma_pCfgFeedbackMgr->setTurnOffCount( asLong( LPCTSTR(m_zCount) ) );
		}
	}
	else
	{
		// Clear date
		ma_pCfgFeedbackMgr->setTurnOffDate( "" );

		// Set count to 0
		ma_pCfgFeedbackMgr->setTurnOffCount( 0 );
	}

	// Store document collection setting
	ma_pCfgFeedbackMgr->setDocumentCollection( m_bNoCollect );

	// Store document conversion setting
	ma_pCfgFeedbackMgr->setDocumentConversion( m_bConvertToText ? true : false );

	// Store attributes to be saved
	ma_pCfgFeedbackMgr->setAttributeSelection( m_bAllAttributes ? true : false );

	// Clear any existing named attributes
//	ma_pCfgFeedbackMgr->clearAttributeNames();

	// Get and store the list of attributes
	if (m_bAllAttributes > 0)
	{
		// Persistence of attribute names is not supported
		MessageBox( "Persistence of attribute names is not supported.", 
			"Error", MB_OK );
//		ma_pCfgFeedbackMgr->setAttributeNames( ipNames );
	}
}
//-------------------------------------------------------------------------------------------------
