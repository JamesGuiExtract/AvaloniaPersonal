// ImportRuleSetDlg.cpp : implementation file
//

#include "stdafx.h"
#include "afcore.h"
#include "ImportRuleSetDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <ComUtils.h>

#include <string>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// Dialog size bounds
const int	giIMPORTDLG_MIN_WIDTH			= 270;
const int	giIMPORTDLG_MIN_HEIGHT			= 160;

//--------------------------------------------------------------------------------------------------
// CImportRuleSetDlg dialog
//--------------------------------------------------------------------------------------------------
CImportRuleSetDlg::CImportRuleSetDlg(UCLID_AFCORELib::IRuleSetPtr ipRuleSet, 
									 std::vector<std::string> vecReservedStrings, 
									 bool bDoImport, CWnd* pParent /*=NULL*/)
	: CDialog(CImportRuleSetDlg::IDD, pParent),
	m_vecReserved(vecReservedStrings),
	m_bInitialized(false), 
	m_bDoImport(bDoImport),
	m_ipRuleSet(ipRuleSet)
{
}
//--------------------------------------------------------------------------------------------------
void CImportRuleSetDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_GRID, m_wndList);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CImportRuleSetDlg, CDialog)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// CImportRuleSetDlg message handlers
//--------------------------------------------------------------------------------------------------
BOOL CImportRuleSetDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	CDialog::OnInitDialog();

	try
	{
		// Set the dialog title and prompt
		if (m_bDoImport)
		{
			SetWindowText( "Import Attribute Rules" );
			
			GetDlgItem( IDC_STATIC_SELECT )->SetWindowText( "Select attributes to import" );
		}
		else
		{
			SetWindowText( "Export Attribute Rules" );
			
			GetDlgItem( IDC_STATIC_SELECT )->SetWindowText( "Select attributes to export" );
		}
		
		// Set flag
		m_bInitialized = true;
		
		// Prepare Header labels
		std::vector<std::string>	vecHeader;
		if (m_bDoImport)
		{
			vecHeader.push_back( string( "Import" ) );
		}
		else
		{
			vecHeader.push_back( string( "Export" ) );
		}
		vecHeader.push_back( string( "Name" ) );
		vecHeader.push_back( string( "New Name" ) );
		
		// Prepare Column widths
		std::vector<int>	vecWidths;
		vecWidths.push_back( 50 );
		vecWidths.push_back( 80 );
		vecWidths.push_back( 0 );		// Will be resized by DoResize()
		
		// Add real entries from Rule Set
		populateList();
		
		// Resize the cell heights and the Value column width
		m_wndGrid.SetValueColumn( gVALUE_COLUMN );
		m_wndGrid.DoResize();
		
		m_wndGrid.GetParam()->EnableUndo(TRUE);
		
		// Resize the Picture control around the Grid
		CWnd*	pPicture = GetDlgItem( IDC_PICTURE );
		if (pPicture != NULL)
		{
			// Get the Grid dimensions
			CRect	rectGrid;
			m_wndGrid.GetWindowRect( rectGrid );
			
			// Adjust rect size
			ScreenToClient( rectGrid );
			rectGrid.left -= 1;
			rectGrid.top -= 1;
			rectGrid.right += 1;
			rectGrid.bottom += 1;
			
			// Set Picture size
			pPicture->MoveWindow( &rectGrid );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05344");

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
void CImportRuleSetDlg::OnOK() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check count of checked items
		if (m_wndGrid.GetCheckedCount() == 0)
		{
			// Nothing selected, present error message and return
			MessageBox( "At least one attribute must be selected.", 
				"Error", MB_OK | MB_ICONEXCLAMATION );
			return;
		}
		
		// Retrieve map of attribute names and infos
		IStrToObjectMapPtr ipPortMap;
		ipPortMap = m_ipRuleSet->AttributeNameToInfoMap;
		ASSERT_RESOURCE_ALLOCATION( "ELI05063", ipPortMap != NULL );
		
		// Review each item and discard the unchecked items from Rule Set
		CString	zTemp;
		long lCount = m_wndGrid.GetRowCount();
		for (int i = 1; i <= lCount; i++)
		{
			zTemp = m_wndGrid.GetValueRowCol( i, gACCEPT_COLUMN );
			int nCheck = atol( zTemp.operator LPCTSTR() );
			
			// Retrieve the original attribute name
			_bstr_t	bstrName = get_bstr_t( m_wndGrid.GetValueRowCol( i, gNAME_COLUMN ).operator LPCTSTR() );
			
			// Remove unchecked item
			if (nCheck == 0)
			{
				// Remove this item
				ipPortMap->RemoveItem( bstrName );
			}
			// Rename checked item, if desired
			else
			{
				// Retrieve the 'new' attribute name
				_bstr_t	bstrNewName = get_bstr_t( m_wndGrid.GetValueRowCol( i, gVALUE_COLUMN ).operator LPCTSTR() );
				
				// Compare the attribute names
				CString z1 = m_wndGrid.GetValueRowCol( i, gNAME_COLUMN );
				CString z2 = m_wndGrid.GetValueRowCol( i, gVALUE_COLUMN );
				if ( z1 != z2 )
				{
					// Names are different, use the new name
					ipPortMap->RenameKey( bstrName, bstrNewName );
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05345");

	CDialog::OnOK();
}
//--------------------------------------------------------------------------------------------------
void CImportRuleSetDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	// Minimum width to allow display of buttons
	lpMMI->ptMinTrackSize.x = giIMPORTDLG_MIN_WIDTH;

	// Minimum height to allow display of list
	lpMMI->ptMinTrackSize.y = giIMPORTDLG_MIN_HEIGHT;
}
//--------------------------------------------------------------------------------------------------
void CImportRuleSetDlg::OnSize(UINT nType, int cx, int cy) 
{
	try
	{
		if (m_bInitialized)
		{
			////////////////////////////
			// Prepare controls for move
			////////////////////////////
			CRect	rectDlg;
			CRect	rectOK;
			CRect	rectCancel;
			CRect	rectGrid;
			
			// Get total dialog size
			GetWindowRect( &rectDlg );
			ScreenToClient( &rectDlg );
			
			// Get original position of buttons and grid
			GetDlgItem( IDOK )->GetWindowRect( rectOK );
			GetDlgItem( IDCANCEL )->GetWindowRect( rectCancel );
			GetDlgItem( IDC_GRID )->GetWindowRect( rectGrid );
			
			// Compute space between buttons
			int iDiffX1 = rectCancel.left - rectOK.right;
			
			// Convert to client coordinates to facilitate the move
			ScreenToClient( &rectOK );
			ScreenToClient( &rectCancel );
			ScreenToClient( &rectGrid );
			
			///////////////
			// Do the moves
			///////////////
			GetDlgItem( IDCANCEL )->MoveWindow( cx - iDiffX1 - rectCancel.Width(), 
				cy - iDiffX1 - rectCancel.Height(), 
				rectCancel.Width(), rectCancel.Height(), TRUE );
			
			GetDlgItem( IDOK )->MoveWindow( cx - 2*iDiffX1 - rectOK.Width() - rectCancel.Width(), 
				cy - iDiffX1 - rectOK.Height(), 
				rectOK.Width(), rectOK.Height(), TRUE );
			
			GetDlgItem( IDC_GRID )->MoveWindow( rectGrid.left, rectGrid.top, 
				cx - 2*iDiffX1, cy - 5*iDiffX1, TRUE );
			
			///////////////////////////////////////
			// Adjust Value column width, if needed
			///////////////////////////////////////
			m_wndGrid.DoResize();
			
			////////////////
			// Resize border
			////////////////
			CWnd*	pPicture = GetDlgItem( IDC_PICTURE );
			if (pPicture != NULL)
			{
				long lHeight = m_wndGrid.GetRowHeight( 0 );
				
				// Adjust rect size
				rectGrid.left -= 1;
				rectGrid.top -= 1;
				rectGrid.right = cx - iDiffX1 + 1;
				rectGrid.bottom = cy - 4*iDiffX1 + 1;
				
				// Set Picture size
				pPicture->MoveWindow( &rectGrid );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19126");

	CDialog::OnSize(nType, cx, cy);
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
std::string CImportRuleSetDlg::getUnreservedString(std::string strOldValue)
{
	string	strNewValue;

	// Check collection of reserved strings
	std::vector<std::string>::const_iterator iter;
	iter = find( m_vecReserved.begin(), m_vecReserved.end(), strOldValue );
	if (iter == m_vecReserved.end())
	{
		// String is not in Reserved collection
		strNewValue = strOldValue;
	}
	else
	{
		// Need to append digits until a valid string is found
		int		i = 0;
		bool	bFoundValid = false;
		CString	zTemp;
		while (!bFoundValid)
		{
			// Create a test string
			zTemp.Format( "%s_%d", strOldValue.c_str(), ++i );

			// Check the string against the reserved collection
			iter = find( m_vecReserved.begin(), m_vecReserved.end(), 
				string( zTemp.operator LPCTSTR() ) );
			if (iter == m_vecReserved.end())
			{
				// Set flag
				bFoundValid = true;

				// Save string for return
				strNewValue = zTemp.operator LPCTSTR();
			}

			// Throw exception if counter > 1000
			if (i > 1000)
			{
				UCLIDException ue( "ELI05065", "Unable to find unreserved string." );
				ue.addDebugInfo( "Original String", strOldValue );
				throw ue;
			}
		}
	}

	return strNewValue;
}
//--------------------------------------------------------------------------------------------------
void CImportRulesSetDlg::prepareList()
{
	CRect recList;
	m_wndList.GetClientRect(&recList);
	int nTotalWidth = recList.Width();

	int nColWidth = nTotalWidth/(gnNUMBER_OF_COLUMNS+2);
	int nCol = 0;

	LVCOLUMN lvColumn;

}
//--------------------------------------------------------------------------------------------------
void CImportRuleSetDlg::populateList() 
{
	// Retrieve map of attribute names and attribute infos
	// to be imported
	IStrToObjectMapPtr ipPortMap;
	ipPortMap = m_ipRuleSet->AttributeNameToInfoMap;
	ASSERT_RESOURCE_ALLOCATION( "ELI05064", ipPortMap != NULL );

	// Add the selected rules to this rule set
	IVariantVectorPtr ipKeys = ipPortMap->GetKeys();
	long lNumKeys = ipKeys->Size;
	string	strName;
	string	strValue;
	for (int i = 0; i < lNumKeys; i++)
	{
		// Retrieve the attribute name
		_bstr_t bstrName = _bstr_t(ipKeys->GetItem( i ));
		strName = bstrName.operator const char *();

		// Get an appropriate unreserved string for Value column
		strValue = getUnreservedString( bstrName.operator const char *() );

		// Populate the vector of strings
		vector<string>	vecText;
		vecText.push_back( strName );
		vecText.push_back( strValue );

		// Add the attribute names to the grid - default to checked
		m_wndGrid.SetRowInfo( i + 1, true, vecText );
	}
}
//--------------------------------------------------------------------------------------------------
