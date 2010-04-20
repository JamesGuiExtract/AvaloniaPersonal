// UEXFindDlg.cpp : implementation file
//

#include "stdafx.h"
#include "UEXViewer.h"
#include "UEXFindDlg.h"

#include <UCLIDException.h>
#include <RegistryPersistenceMgr.h>
#include <cpputil.h>

#include <string>
#include <vector>

//-------------------------------------------------------------------------------------------------
// Persistence Information
//-------------------------------------------------------------------------------------------------

// Persistence Folders
const string FIND				= "\\Find";

// Persistence Keys
const string SEARCH_TEXT		= "SearchText";
const string IS_REGEXP			= "IsRegularExpression";
const string SELECTION			= "Selection";
const string FIND_WINDOW_POSX	= "WindowPositionX";
const string FIND_WINDOW_POSY	= "WindowPositionY";
const string FIND_WINDOW_WIDTH	= "WindowWidth";
const string FIND_WINDOW_HEIGHT	= "WindowHeight";

//-------------------------------------------------------------------------------------------------
// CUEXFindDlg dialog
//-------------------------------------------------------------------------------------------------
CUEXFindDlg::CUEXFindDlg(IConfigurationSettingsPersistenceMgr *pCfgMgr, CWnd* pParent)
	: CDialog(CUEXFindDlg::IDD, pParent),
	m_pCfgMgr(pCfgMgr),
	m_pUEXDlg((CUEXViewerDlg *)pParent)
{
	//{{AFX_DATA_INIT(CUEXFindDlg)
	m_bAsRegEx = FALSE;
	m_nSelect = 0;
	m_zPatterns = _T("");
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
CUEXFindDlg::~CUEXFindDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16545");
}
//-------------------------------------------------------------------------------------------------
void CUEXFindDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CUEXFindDlg)
//	DDX_Check(pDX, IDC_CHK_REGEX, m_bAsRegEx);
	DDX_Control(pDX, IDC_EDIT_FIND, m_editFind);
	DDX_Radio(pDX, IDC_RADIO_ALL, m_nSelect);
	DDX_Text(pDX, IDC_EDIT_FIND, m_zPatterns);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CUEXFindDlg, CDialog)
	ON_BN_CLICKED(ID_BTN_FIND, OnBtnFind)
	ON_WM_SHOWWINDOW()
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CUEXFindDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CUEXFindDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	try
	{
		// Retrieve and apply settings
		initPersistent();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13418");

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void CUEXFindDlg::OnBtnFind() 
{
	try
	{
		// Validate pattern
		if (!isValidPattern())
		{
			CString zText;
			zText.Format( "Search string \"%s\" is not valid.", m_zPatterns );
			AfxMessageBox( zText.operator LPCTSTR() );
			return;
		}

		// Set wait cursor
		CWaitCursor wait;

		// Set search parameters - default to Select All
		std::vector<int>	vecFoundIndices;
		int iStartIndex = 0;
		int iStopIndex = m_pUEXDlg->GetExceptionCount() - 1;
		if (m_nSelect == 1)
		{
			// Find only the Next exception
			iStartIndex = m_pUEXDlg->GetFirstSelectionIndex() + 1;
		}
		else if (m_nSelect == 2)
		{
			// Find only the Previous exception
			iStartIndex = m_pUEXDlg->GetFirstSelectionIndex() - 1;
			iStopIndex = 0;
		}

		// Check for out-of-bounds Start index
		if ((iStartIndex < 0) || (iStartIndex >= m_pUEXDlg->GetExceptionCount()))
		{
			// Inform user that no match was found
			AfxMessageBox("No match found.");

			// Retrieve and store settings
			storeSettings();
			return;
		}

		////////////
		// Do search
		////////////
		string strSearch = m_zPatterns.operator LPCTSTR();
		if (iStopIndex >= iStartIndex)
		{
			// Step forward through Exceptions and check each one
			for (int i = iStartIndex; i <= iStopIndex; i++)
			{
				// Check this Exception for the search text
				if (findStringInException( strSearch, i ))
				{
					// Add this index to the vector
					vecFoundIndices.push_back( i );

					// Quit looking if just looking for the Next exception
					if (m_nSelect != 0)
					{
						break;
					}
				}
			}
		}
		else
		{
			// Step backward through Exceptions and check each one
			for (int i = iStartIndex; i >= iStopIndex; i--)
			{
				// Check this string for the search text
				if (findStringInException( strSearch, i ))
				{
					// Add this index to the vector
					vecFoundIndices.push_back( i );

					// Quit looking since just looking for the Previous exception
					break;
				}
			}
		}


		////////////////////////
		// Highlight any results
		////////////////////////
		unsigned int uiFound = vecFoundIndices.size();
		if (uiFound == 0)
		{
			// Inform user that no match was found
			AfxMessageBox("No match found.");
		}

		// Set or Clear the selection
		m_pUEXDlg->SelectExceptions( vecFoundIndices );

		this->SetFocus();

		// Retrieve and store settings
		storeSettings();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13419");
}
//-------------------------------------------------------------------------------------------------
void CUEXFindDlg::OnShowWindow(BOOL bShow, UINT nStatus) 
{
	CDialog::OnShowWindow(bShow, nStatus);
	
	try
	{
		// Show the window
		if (bShow == TRUE)
		{
			// Set focus to the Search text edit box
			m_editFind.SetFocus();

			// Bring this dialog to the front
			SetForegroundWindow();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13420");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool CUEXFindDlg::findStringInException(std::string strSearch, int iExceptionIndex)
{
	bool bFound = false;

	// Retrieve this Exception string
	string strExceptionText = m_pUEXDlg->GetWholeExceptionString( iExceptionIndex );

	// Convert strings to lower-case for case-insensitive searching (P13 #3715)
	makeLowerCase( strExceptionText );
	makeLowerCase( strSearch );

	// TODO: Add support for regular expression search

	// Check this string for the search text
	if (strExceptionText.find( strSearch.c_str() ) != string::npos)
	{
		bFound = true;
	}

	return bFound;
}
//-------------------------------------------------------------------------------------------------
void CUEXFindDlg::initPersistent()
{
	///////////////////////////
	// Dialog size and position
	///////////////////////////
	string	strTemp;
	bool	bMove = true;

	// Retrieve persistent size and position of dialog
	long	lLeft = 0;
	long	lTop = 0;
	long	lWidth = 0;
	long	lHeight = 0;

	// If X position key doesn't exist, retain default from resource file
	if (m_pCfgMgr->keyExists( FIND, FIND_WINDOW_POSX ))
	{
		// Retrieve value
		strTemp = m_pCfgMgr->getKeyValue( FIND, FIND_WINDOW_POSX );
		
		// Convert to integer
		lLeft = asLong( strTemp );
	}
	else
	{
		bMove = false;
	}

	// If Y position key doesn't exist, retain default from resource file
	if (m_pCfgMgr->keyExists( FIND, FIND_WINDOW_POSY ))
	{
		// Retrieve value
		strTemp = m_pCfgMgr->getKeyValue( FIND, FIND_WINDOW_POSY );
		
		// Convert to integer
		lTop = asLong( strTemp );
	}
	else
	{
		bMove = false;
	}

	// If width key doesn't exist, retain default from resource file
	if (m_pCfgMgr->keyExists( FIND, FIND_WINDOW_WIDTH ))
	{
		// Retrieve value
		strTemp = m_pCfgMgr->getKeyValue( FIND, FIND_WINDOW_WIDTH );
		
		// Convert to integer
		lWidth = asLong( strTemp );
	}
	else
	{
		bMove = false;
	}

	// If height key doesn't exist, retain default from resource file
	if (m_pCfgMgr->keyExists( FIND, FIND_WINDOW_HEIGHT ))
	{
		// Retrieve value
		strTemp = m_pCfgMgr->getKeyValue( FIND, FIND_WINDOW_HEIGHT );
		
		// Convert to integer
		lHeight = asLong( strTemp );
	}
	else
	{
		bMove = false;
	}

	// Only modify dialog position if settings were found
	if (bMove)
	{
		// Adjust window position based on retrieved settings
		MoveWindow( lLeft, lTop, lWidth, lHeight, TRUE );
	}

	// Retrieve Search String
	// Check for key
	if (m_pCfgMgr->keyExists( FIND, SEARCH_TEXT ))
	{
		// Retrieve value
		strTemp = m_pCfgMgr->getKeyValue( FIND, SEARCH_TEXT );

		// TODO: Change between CPP and Normal format to handle regular expression

		// Store in CString
		m_zPatterns = strTemp.c_str();
	}

	// TODO: Retrieve and apply setting for Regular Expression

	// Retrieve Selection
	// Default to Find All
	m_nSelect = 0;

	// Check for key
	if (m_pCfgMgr->keyExists( FIND, SELECTION ))
	{
		// Retrieve value
		strTemp = m_pCfgMgr->getKeyValue( FIND, SELECTION );

		// Validate result
		int iTemp = asLong( strTemp );
		if ((iTemp >= 0) && (iTemp <= 2))
		{
			// Valid setting, store in data member
			m_nSelect = iTemp;
		}
	}

	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
bool CUEXFindDlg::isValidPattern()
{
	// Retrieve dialog settings
	UpdateData();

	// TODO: Validate search string if this is a regular expression

	// Check string length
	if (m_zPatterns.GetLength() == 0)
	{
		return false;
	}
	else
	{
		return true;
	}
}
//-------------------------------------------------------------------------------------------------
void CUEXFindDlg::storeSettings()
{
	CRect	rect;
	char	pszKey[20];
	string	strKey;

	// Window position and size
	GetWindowRect( &rect );
	sprintf_s( pszKey, "%d", rect.left );
	m_pCfgMgr->setKeyValue( FIND, FIND_WINDOW_POSX, pszKey );

	sprintf_s( pszKey, "%d", rect.top );
	m_pCfgMgr->setKeyValue( FIND, FIND_WINDOW_POSY, pszKey );

	sprintf_s( pszKey, "%d", rect.Width() );
	m_pCfgMgr->setKeyValue( FIND, FIND_WINDOW_WIDTH, pszKey );

	sprintf_s( pszKey, "%d", rect.Height() );
	m_pCfgMgr->setKeyValue( FIND, FIND_WINDOW_HEIGHT, pszKey );

	// Search string
	string strSearch = m_zPatterns.operator LPCTSTR();
	m_pCfgMgr->setKeyValue( FIND, SEARCH_TEXT, strSearch.c_str() );

	// Regular expression
	m_pCfgMgr->setKeyValue( FIND, IS_REGEXP, (m_bAsRegEx == TRUE) ? "1" : "0" );

	// Selection
	m_pCfgMgr->setKeyValue( FIND, SELECTION, asString( m_nSelect ) );
}
//-------------------------------------------------------------------------------------------------
