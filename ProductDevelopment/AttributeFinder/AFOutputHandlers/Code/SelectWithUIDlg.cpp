// SelectWithUIDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "SelectWithUIDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// String for Count column after a Value edit
const std::string	gstrNOT_APPLICABLE			= "N/A";

// Dialog size bounds
const int	giUIDLG_MIN_WIDTH			= 450;
const int	giUIDLG_MIN_HEIGHT			= 140;

//-------------------------------------------------------------------------------------------------
// SelectWithUIDlg dialog
//-------------------------------------------------------------------------------------------------
SelectWithUIDlg::SelectWithUIDlg(IIUnknownVector* pAttributes,
								 IIUnknownVector* pResultAttributes,
								 CWnd* pParent /*=NULL*/)
: CDialog(SelectWithUIDlg::IDD, pParent),
  m_ipOriginAttributes(pAttributes),
  m_bInitialized(false), 
  m_ipReturnAttributes(pResultAttributes)
{
	//{{AFX_DATA_INIT(SelectWithUIDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
SelectWithUIDlg::~SelectWithUIDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16322");
}
//-------------------------------------------------------------------------------------------------
void SelectWithUIDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(SelectWithUIDlg)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SelectWithUIDlg, CDialog)
	//{{AFX_MSG_MAP(SelectWithUIDlg)
	ON_BN_CLICKED(IDC_SELECT_ALL, OnSelectAll)
	ON_BN_CLICKED(IDC_CLEAR_ALL, OnClearAll)
	ON_BN_CLICKED(IDC_SELECT_VALID, OnSelectValid)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// SelectWithUIDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL SelectWithUIDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	CDialog::OnInitDialog();
	
	// Please refer to the MFC documentation on 
	// SubclassDlgItem for information on this 
	// call. This makes sure that our C++ grid 
	// window class subclasses the window that 
	// is created with the User Control.
	m_wndGrid.SubclassDlgItem( IDC_GRID, this );

	// Set flag
	m_bInitialized = true;

	// Prepare Header labels
	std::vector<std::string>	vecHeader;
	vecHeader.push_back( string( "Accept" ) );
	vecHeader.push_back( string( "Attribute" ) );
	vecHeader.push_back( string( "Value" ) );
	vecHeader.push_back( string( "Type" ) );
	vecHeader.push_back( string( "Count" ) );

	// Prepare Column widths
	std::vector<int>	vecWidths;
	vecWidths.push_back( 50 );
	vecWidths.push_back( 80 );
	vecWidths.push_back( 0 );		// Will be resized by DoResize()
	vecWidths.push_back( 50 );
	vecWidths.push_back( 50 );

	// Setup the grid control
	//    5 columns of header labels
	//    5 columns of column widths
	//    DO     use checkboxes in column 1
	//    DO NOT use group information for checkboxes
	m_wndGrid.PrepareGrid( vecHeader, vecWidths, true, false );

	// Provide Modification string for Count column
	m_wndGrid.SetValueModifiedIndicator( gCOUNT_COLUMN, gstrNOT_APPLICABLE );

	// Add real entries from Rule Set before making Name and Count columns read-only
	processVector();
	displayItems();

	// Count column is centered static text
	m_wndGrid.SetStyleRange(CGXRange().SetCols( gCOUNT_COLUMN ),
							CGXStyle()
								.SetHorizontalAlignment( DT_CENTER )
								.SetControl( GX_IDS_CTRL_STATIC )
							);

	// Resize the cell heights and the Value column width
	m_wndGrid.SetValueColumn( gVALUE_COLUMN );
	m_wndGrid.DoResize();

	m_wndGrid.GetParam()->EnableUndo(TRUE);
	
	// Resize border
	CWnd*	pPicture = GetDlgItem( IDC_PICTURE );
	if (pPicture != NULL)
	{
		long lHeight = m_wndGrid.GetRowHeight( 0 );

		// Get rect size
		CRect	rectGrid;
		m_wndGrid.GetWindowRect( rectGrid );
		ScreenToClient( &rectGrid );

		// Adjust rect size
		rectGrid.left -= 1;
		rectGrid.top -= 1;
		rectGrid.right += 1;
		rectGrid.bottom += 1;

		// Set Picture size
		pPicture->MoveWindow( &rectGrid );
	}

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void SelectWithUIDlg::OnOK() 
{
	// Clear the output vector
	m_ipReturnAttributes->Clear();

	// Continue if some items are checked
	int iCheckedCount = m_wndGrid.GetCheckedCount();
	if (iCheckedCount > 0)
	{
		// Determine count of Grid items
		long lSize = m_vecAttrAndNum.size();

		// Evaluate each item in the grid
		CString	zTemp;
		CString	zCount;
		for (int i = 0; i < lSize; i++)
		{
			// Grid rows are 1-relative
			zTemp = m_wndGrid.GetValueRowCol( i + 1, gACCEPT_COLUMN );
			int nCheck = atol( zTemp.operator LPCTSTR() );

			// Item is checked
			if (nCheck == 1)
			{
				// Retrieve the attribute
				IAttributePtr ipAttr( m_vecAttrAndNum[i].ipAttribute );

				// Retrieve the Count string from the Grid
				zCount = m_wndGrid.GetValueRowCol( i + 1, gCOUNT_COLUMN );

				// Check Count string to see if Value was modified
				if (zCount.Compare( gstrNOT_APPLICABLE.c_str() ) == 0)
				{
					// Value was modified, retrieve the update
					zTemp = m_wndGrid.GetValueRowCol( i + 1, gVALUE_COLUMN );
					_bstr_t	bstrValue( zTemp.operator LPCTSTR() );

					// Modify the Value string in the Attribute
					ISpatialStringPtr ipValue = ipAttr->Value;
					ASSERT_RESOURCE_ALLOCATION("ELI25939", ipValue != NULL);

					if (ipValue->HasSpatialInfo() == VARIANT_TRUE)
					{
						ipValue->ReplaceAndDowngradeToHybrid(bstrValue);
					}
					else
					{
						ipValue->ReplaceAndDowngradeToNonSpatial(bstrValue);
					}
				}

				// Add the attribute to the output vector
				m_ipReturnAttributes->PushBack( ipAttr );
			}
		}
	}
	
	CDialog::OnOK();
}
//-------------------------------------------------------------------------------------------------
void SelectWithUIDlg::OnSelectAll() 
{
	// Determine count of Grid items
	long lSize = m_vecAttrAndNum.size();

	// Check each item in the grid
	for (int i = 0; i < lSize; i++)
	{
		// Grid rows are 1-relative
		m_wndGrid.CheckItem( i + 1, true );
	}
}
//-------------------------------------------------------------------------------------------------
void SelectWithUIDlg::OnClearAll() 
{
	// Determine count of Grid items
	long lSize = m_vecAttrAndNum.size();

	// Uncheck each item in the grid
	for (int i = 0; i < lSize; i++)
	{
		// Grid rows are 1-relative
		m_wndGrid.CheckItem( i + 1, false );
	}
}
//-------------------------------------------------------------------------------------------------
void SelectWithUIDlg::OnSelectValid() 
{
	// Determine count of Grid items
	long lSize = m_vecAttrAndNum.size();

	// Evaluate each item in the grid
	CString	zText;
	for (int i = 0; i < lSize; i++)
	{
		// Retrieve this item
		IAttributePtr	ipAttribute = m_vecAttrAndNum[i].ipAttribute;

		// Check for an associated Input Validator
		IInputValidatorPtr ipInputValidator( ipAttribute->InputValidator );
		if (ipInputValidator)
		{
			// Retrieve the Value text
			zText = m_wndGrid.GetValueRowCol( i + 1, gVALUE_COLUMN );

			// Create an ITextInput object
			ITextInputPtr	ipText( CLSID_TextInput );
			ASSERT_RESOURCE_ALLOCATION( "ELI05238", ipText != NULL);

			// Set the text string
			ipText->SetText( _bstr_t( zText.operator LPCTSTR() ) );

			// Test string with Input Validator
			if (ipInputValidator->ValidateInput( ipText ) == VARIANT_TRUE)
			{
				// Check the item
				m_wndGrid.CheckItem( i + 1, true );
			}
			else
			{
				// Uncheck the item
				m_wndGrid.CheckItem( i + 1, false );
			}
		}
		else
		{
			// No Input Validator available, uncheck the item
			m_wndGrid.CheckItem( i + 1, false );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void SelectWithUIDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	// Minimum width to allow display of buttons
	lpMMI->ptMinTrackSize.x = giUIDLG_MIN_WIDTH;

	// Minimum height to allow display of list
	lpMMI->ptMinTrackSize.y = giUIDLG_MIN_HEIGHT;
}
//-------------------------------------------------------------------------------------------------
void SelectWithUIDlg::OnSize(UINT nType, int cx, int cy) 
{
	if (m_bInitialized)
	{
		////////////////////////////
		// Prepare controls for move
		////////////////////////////
		CRect	rectDlg;
		CRect	rectOK;
		CRect	rectCancel;
		CRect	rectSelect;
		CRect	rectClear;
		CRect	rectValid;
		CRect	rectGrid;
		
		// Get total dialog size
		GetWindowRect( &rectDlg );
		ScreenToClient( &rectDlg );

		// Get original position of buttons and grid
		GetDlgItem( IDOK )->GetWindowRect( rectOK );
		GetDlgItem( IDCANCEL )->GetWindowRect( rectCancel );
		GetDlgItem( IDC_SELECT_ALL )->GetWindowRect( rectSelect );
		GetDlgItem( IDC_CLEAR_ALL )->GetWindowRect( rectClear );
		GetDlgItem( IDC_SELECT_VALID )->GetWindowRect( rectValid );
		GetDlgItem( IDC_GRID )->GetWindowRect( rectGrid );

		// Compute space between buttons
		int iDiffX1 = rectCancel.left - rectOK.right;

		// Convert to client coordinates to facilitate the move
		ScreenToClient( &rectOK );
		ScreenToClient( &rectCancel );
		ScreenToClient( &rectSelect );
		ScreenToClient( &rectClear );
		ScreenToClient( &rectValid );
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

		GetDlgItem( IDC_SELECT_ALL )->MoveWindow( rectSelect.left, 
			cy - iDiffX1 - rectSelect.Height(), 
			rectSelect.Width(), rectSelect.Height(), TRUE );

		GetDlgItem( IDC_CLEAR_ALL )->MoveWindow( rectClear.left, 
			cy - iDiffX1 - rectClear.Height(), 
			rectClear.Width(), rectClear.Height(), TRUE );

		GetDlgItem( IDC_SELECT_VALID )->MoveWindow( rectValid.left, 
			cy - iDiffX1 - rectValid.Height(), 
			rectValid.Width(), rectValid.Height(), TRUE );

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

	CDialog::OnSize(nType, cx, cy);
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void SelectWithUIDlg::displayItems() 
{
	long lCount = 0;
	CString	zCount;

	// Step through sorted vector
	long lSize = m_vecAttrAndNum.size();
	for (int i = 0; i < lSize; i++)
	{
		// Retrieve the Attribute Name
		string	strName = m_vecAttrAndNum[i].ipAttribute->Name;

		// Retrieve the Attribute Value
		ISpatialStringPtr ipValue = m_vecAttrAndNum[i].ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION( "ELI15533", ipValue != NULL);
		string strValue = ipValue->String;

		// Retrieve the Attribute Type
		string strType = m_vecAttrAndNum[i].ipAttribute->Type;

		// Retrieve the count
		lCount = m_vecAttrAndNum[i].nNumOfSameValue;
		zCount.Format( "%d", lCount );

		// Create a vector of strings for the Grid
		vector<string>	vecGridText;

		// Add Name, Value, Type, Count items
		vecGridText.push_back( strName );
		vecGridText.push_back( strValue );
		vecGridText.push_back( strType );
		vecGridText.push_back( zCount.operator LPCTSTR() );

		// Add the item to the Grid
		//    Row number is 1-relative
		//    Item will start unchecked
		//    Strings are in the vector
		m_wndGrid.SetRowInfo( i + 1, false, vecGridText );
	}
}
//-------------------------------------------------------------------------------------------------
void SelectWithUIDlg::processVector() 
{
	// Clear the vector of sorted attributes
	m_vecAttrAndNum.clear();

	// Step through Original vector
	bool bSameNameValueFound;
	long lCount = m_ipOriginAttributes->Size();
	for (int i = 0; i < lCount; i++)
	{
		// Clear flag
		bSameNameValueFound = false;

		// Retrieve this Attribute
		IAttributePtr	ipCurrentAttribute( m_ipOriginAttributes->At( i ) );

		// Retrieve the strings
		ISpatialStringPtr ipValue = ipCurrentAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION( "ELI15534", ipValue != NULL);

		string	strName = ipCurrentAttribute->Name;
		string	strValue = ipValue->String;
		string	strType = ipCurrentAttribute->Type;

		// Compare name/value with each attribute in the sorted vector
		for (unsigned int uj = 0; uj < m_vecAttrAndNum.size(); uj++)
		{
			ISpatialStringPtr ipThisValue = m_vecAttrAndNum[uj].ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION( "ELI15535", ipThisValue != NULL);

			// Retrieve Name and Value and Type strings
			string	strSortedName = m_vecAttrAndNum[uj].ipAttribute->Name;
			string	strSortedValue = ipThisValue->String;
			string	strSortedType = m_vecAttrAndNum[uj].ipAttribute->Type;

			// Compare all three strings
			if ((strName.compare( strSortedName ) == 0) &&
				(strValue.compare( strSortedValue ) == 0) &&
				(strType.compare( strSortedType ) == 0))
			{
				// Increment counter
				m_vecAttrAndNum[uj].nNumOfSameValue++;

				// Set flag
				bSameNameValueFound = true;

				// Stop checking for a match
				break;
			}
		}
		
		// Create a new item for the sorted vector if a match was not found
		if (!bSameNameValueFound)
		{
			AttributeAndNumber structAttrAndNum;

			// Set the Attribute pointer
			structAttrAndNum.ipAttribute = ipCurrentAttribute;

			// Initialize count
			structAttrAndNum.nNumOfSameValue = 1;

			// Add item to vector
			m_vecAttrAndNum.push_back( structAttrAndNum );
		}
	}
}
//-------------------------------------------------------------------------------------------------
