// CheckGridWnd.cpp : implementation file
//

#include "stdafx.h"
#include "CheckGridWnd.h"
#include "resource.h"

#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// Gray background color for rows where Normal attribute = 0
const COLORREF g_colorAltBack = RGB( 192, 192, 192 );

//-------------------------------------------------------------------------------------------------
// CCheckGridWnd
//-------------------------------------------------------------------------------------------------
CCheckGridWnd::CCheckGridWnd()
{
	// Force that current cell is redrawn when user
	// navigates through the grid
	m_bRefreshOnSetCurrentCell = TRUE;

	// Default columns to nonsense value
	m_iShowModificationColumn = -1;
	m_iValueColumn = -1;
	m_bIsPrepared = false;
	m_iEditRow = -1;
	m_bEditStarted = false;
}
//-------------------------------------------------------------------------------------------------
CCheckGridWnd::~CCheckGridWnd()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16474");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCheckGridWnd, CGXGridWnd)
	//{{AFX_MSG_MAP(CCheckGridWnd)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CCheckGridWnd message handlers
//-------------------------------------------------------------------------------------------------
void CCheckGridWnd::OnClickedButtonRowCol(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Retrieve new check state
		CString	zTemp;
		zTemp = GetValueRowCol( nRow, nCol );
		int nCheck = atol( zTemp.operator LPCTSTR() );
		
		// No need for further processing if box has been unchecked
		// or if Grouping is turned off
		if ((nCheck == 0) || (!m_bGroupByName))
		{
			return;
		}
		
		// Retrieve Normal attribute for this cell
		CGXStyle	style;
		BOOL bRet = CGXGridCore::GetStyleRowCol( nRow, nCol, style, gxCopy, 0 );
		long lGroupNormal = style.GetUserAttribute( IDS_NORMAL_ATTRIBUTE ).GetLongValue();
		
		// Step through earlier items with same attribute
		for (int i = nRow - 1; i > 0; i--)
		{
			// Retrieve earlier item's style
			bRet = CGXGridCore::GetStyleRowCol( i, nCol, style, gxCopy, 0 );
			long lItemNormal = style.GetUserAttribute( IDS_NORMAL_ATTRIBUTE ).GetLongValue();
			
			// Check for identical attribute value
			if (lItemNormal == lGroupNormal)
			{
				// Uncheck the other items in the group
				checkAttribute( i, false, lGroupNormal );
			}
			else
			{
				// Attributes do not match, break out of this loop
				break;
			}
		}
		
		// Step through later items with same attribute
		int iCount = GetRowCount();
		for (int i = nRow + 1; i <= iCount; i++)
		{
			// Retrieve later item's style
			bRet = CGXGridCore::GetStyleRowCol( i, nCol, style, gxCopy, 0 );
			long lItemNormal = style.GetUserAttribute( IDS_NORMAL_ATTRIBUTE ).GetLongValue();
			
			// Check for identical attribute value
			if (lItemNormal == lGroupNormal)
			{
				// Uncheck the other items in the group
				checkAttribute( i, false, lGroupNormal );
			}
			else
			{
				// Attributes do not match, break out of this loop
				break;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12788");
}
//-------------------------------------------------------------------------------------------------
BOOL CCheckGridWnd::GetStyleRowCol(ROWCOL nRow, ROWCOL nCol, CGXStyle& style, GXModifyType mt, int nType)
{ 
	try
	{
		// Load stored style information of the cell
		BOOL bRet = CGXGridCore::GetStyleRowCol(nRow, nCol, style, mt, nType);
		
		// Provide text and background color for individual cell
		if (nType == 0)
		{
			// Check for selected row
			if (GetInvertStateRowCol( nRow, nCol, GetParam()->GetRangeList() ))
			{
				// Make sure that this cell is not being edited
				if (!m_bEditStarted || (m_iEditRow != nRow) || (m_iValueColumn != nCol))
				{
					// Use system colors for background and text
					style.SetInterior(::GetSysColor(COLOR_HIGHLIGHT));
					style.SetTextColor(::GetSysColor(COLOR_HIGHLIGHTTEXT));
					bRet = TRUE; 
				}
			}
			else
			{
				///////////////////////
				// Check User Attribute for Normal vs. Gray background color
				///////////////////////
				if (m_bGroupByName)
				{
					// Information is to be grouped, check the Normal attribute
					long lNormal = style.GetUserAttribute( IDS_NORMAL_ATTRIBUTE ).GetLongValue();
					if (lNormal == 0)
					{
						// Set background to gray
						style.SetInterior( g_colorAltBack );
					}
				}
			}
		}
		
		return bRet;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12789");
	// if it gets here method failed
	return FALSE;
} 
//-------------------------------------------------------------------------------------------------
BOOL CCheckGridWnd::OnTrackColWidth(ROWCOL nCol)
{ 
	try
	{
		if (IsColHidden(nCol)) 
		{
			return FALSE; // Don't allow hidden columns to be restored 
		}
		
		// ... but do allow visible columns to be resized
		return TRUE; 
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12790");
	// if it gets here method failed
	return FALSE;
} 
//-------------------------------------------------------------------------------------------------
void CCheckGridWnd::DrawInvertCell(CDC* /*pDC*/, ROWCOL nRow, ROWCOL nCol, CRect rectItem)
{
	try
	{
		// If DrawInvertCell has been called 
		// from OnDrawTopLeftBottomRight
		// m_nNestedDraw is greater 0. There 
		// is no invalidation of the rectangle
		// necessary because the cell has 
		// already been drawn.
		if (m_nNestedDraw == 0)
		{ 
			// m_nNestedDraw equal to 0 means 
			// that PrepareChangeSelection,
			// PrepareClearSelection or 
			// UpdateSelectRange did call 
			// this method.
			CGXRange range;
			if (GetCoveredCellsRowCol(nRow, nCol, range)) 
			{
				rectItem = CalcRectFromRowCol(range.top, range.left, range.bottom, range.right); 
			}
			
			InvalidateRect(&rectItem); 
		} 
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12791");
} 
//-------------------------------------------------------------------------------------------------
BOOL CCheckGridWnd::OnValidateCell(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Quick return if fewer than 4 columns
		if (GetColCount() < gCOUNT_COLUMN)
		{
			return TRUE;
		}
		
		// Get original string
		CString	zOrig = GetValueRowCol( nRow, nCol );
		
		// Get new string
		CString s;
		CGXControl* pControl = GetControl(nRow, nCol);
		pControl->GetCurrentText(s);
		
		// Do not consider cell that was or is now empty
		if (!s.IsEmpty() && !zOrig.IsEmpty())
		{
			// Check for text change
			if (s.Compare( zOrig.operator LPCTSTR() ) != 0)
			{
				// Check for column to be modified and modification string
				if ((m_iShowModificationColumn > 0) && (m_strValueModified.length() > 0))
				{
					// Unlock the read-only cells
					GetParam()->SetLockReadOnly( FALSE );
					
					// Modify the value and the attribute
					if (nRow > -1)
					{
						SetStyleRange(CGXRange( nRow, m_iShowModificationColumn ),
							CGXStyle()
							.SetValue( m_strValueModified.c_str() )
							.SetUserAttribute( IDS_MODIFIED_ATTRIBUTE, 1L )
							);
					}
					
					// Relock the read-only cells
					GetParam()->SetLockReadOnly( TRUE );
				}
			}
			
			return TRUE;
		}
		
		return CGXGridCore::OnValidateCell(nRow, nCol);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12792");
	// if it gets here method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CCheckGridWnd::OnStartEditing(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Set flag
		m_bEditStarted = true;
		
		// Store edit row
		m_iEditRow = nRow;
		
		return CGXGridCore::OnStartEditing(nRow, nCol);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12793");
	// if it gets here method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CCheckGridWnd::OnEndEditing(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Clear flag
		m_bEditStarted = false;
		
		// Clear edit row
		m_iEditRow = -1;
		
		return CGXGridCore::OnStartEditing(nRow, nCol);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12794");
	// if it gets here method failed
	return FALSE;
}

//-------------------------------------------------------------------------------------------------
// CCheckGridWnd public methods
//-------------------------------------------------------------------------------------------------
void CCheckGridWnd::CheckItem(int iItem, bool bCheck)
{
	// Only continue if checkboxes are enabled
	if (m_bShowCheck)
	{
		// Determine Normal attribute
		long lNormal = getNormalAttribute( iItem );

		// Set the check and Normal attribute
		checkAttribute( iItem, bCheck, lNormal );
	}
}
//-------------------------------------------------------------------------------------------------
void CCheckGridWnd::Clear()
{
	long lRowCount = GetRowCount();
	long lColCount = GetColCount();

	if ((lRowCount > 0) && (lColCount > 0))
	{
		// Determine starting column
		int iStart = m_bShowCheck ? 2 : 1;

		// Unlock the read-only cells
		GetParam()->SetLockReadOnly( FALSE );

		// Set the cell range
		CGXRange	range( 1, iStart, lRowCount, lColCount );

		// Clear the text
		SetValueRange( range, "" );

		// Remove the extra rows
		RemoveRows( 1, lRowCount );

		// Relock the read-only cells
		GetParam()->SetLockReadOnly( TRUE );
	}
}
//-------------------------------------------------------------------------------------------------
void CCheckGridWnd::DoResize() 
{
	// Just return if PrepareGrid() has not been called
	if (!m_bIsPrepared)
	{
		return;
	}

	// Adjust row heights as needed
	ResizeRowHeightsToFit( CGXRange( ).SetTable() );

	// Determine overall width and width of Value column
	CRect	rectGrid;
	GetClientRect( rectGrid );
	long lWidth = rectGrid.Width();
	int iCount = GetColCount();
	for (int i = 1; i <= iCount; i++)
	{
		// Ignore width of Value column
		if (m_iValueColumn != i)
		{
			// Decrement final width by width of existing columns
			lWidth -= GetColWidth( i );
		}
	}

	// Set Value column width - allow space for border to prevent scrollbar appearance
	if (m_iValueColumn != -1)
	{
		SetColWidth( m_iValueColumn, m_iValueColumn, lWidth - 1, NULL, GX_UPDATENOW, gxDo );
	}
}
//-------------------------------------------------------------------------------------------------
int CCheckGridWnd::GetCheckedCount() 
{
	int iCount = 0;

	// Continue only if checkboxes are shown
	if (m_bShowCheck)
	{
		CString	zTemp;
		long lCount = GetRowCount();
		for (int i = 1; i <= lCount; i++)
		{
			zTemp = GetValueRowCol( i, gACCEPT_COLUMN );
			int nCheck = atol( zTemp.operator LPCTSTR() );

			if (nCheck == 1)
			{
				// Increment counter
				++iCount;
			}
		}
	}

	return iCount;
}
//--------------------------------------------------------------------------------------------------
void CCheckGridWnd::PrepareGrid(std::vector<std::string> vecHeaders, std::vector<int> vecWidths, 
								bool bShowCheckboxes, bool bGroupByName) 
{
	// Store settings
	m_bShowCheck = bShowCheckboxes;
	m_bGroupByName = bGroupByName;

	// Initialize the grid. For CWnd based grids 
	// this call is essential. For view based 
	// grids this initialization is done in 
	// OnInitialUpdate.
	Initialize();

	// Sample setup for the grid
	GetParam()->EnableUndo(FALSE);

	// Set full-row selection
	GetParam()->EnableSelection((WORD) (GX_SELFULL & ~GX_SELCOL & ~GX_SELTABLE));
	GetParam()->SetSpecialMode( GX_MODELBOX_SS );

	// Determine required column count
	long lCount = vecHeaders.size();

	// Create one row and proper number of columns
	SetRowCount( 0 );
	SetColCount( lCount );

	// Hide the unwanted first column
	HideCols( 0, 0, TRUE, NULL, GX_UPDATENOW, gxDo );

	// Set column widths and Header labels with Attribute = Normal
	int iColumnWidth = 0;
	for (int i = 1; i <= lCount; i++)
	{
		// Retrieve column width
		iColumnWidth = vecWidths[i-1];

		// Set column width
		// Zero width column will be adjusted by DoResize()
		SetColWidth( i, i, vecWidths[i-1], NULL, GX_UPDATENOW, gxDo );

		// Set Header label and Normal attribute
		SetStyleRange(CGXRange( 0, i ),
						CGXStyle()
							.SetValue( (vecHeaders[i-1]).c_str() )
							.SetUserAttribute( IDS_NORMAL_ATTRIBUTE, 1L )
						);
	}

	// Entire grid has vertically-centered items
	// and Modified attribute is OFF
	SetStyleRange(CGXRange().SetTable(),
					CGXStyle()
						.SetVerticalAlignment( DT_VCENTER )
						.SetUserAttribute( IDS_MODIFIED_ATTRIBUTE, 0L )
					);

	// Set column numbers - default for check boxes in first column
	int iAcceptColumn = 1;
	int iNameColumn   = 2;
	int iValueColumn  = 3;

	if (m_bShowCheck)
	{
		// First column is centered 3-D checkboxes
		SetStyleRange(CGXRange().SetCols( iAcceptColumn ),
						CGXStyle()
							.SetControl( GX_IDS_CTRL_CHECKBOX3D )
							.SetHorizontalAlignment( DT_CENTER )
						);

		// Name column is left-justified static text
		SetStyleRange(CGXRange().SetCols( iNameColumn ),
						CGXStyle()
							.SetControl( GX_IDS_CTRL_STATIC )
						);

		// Value column is multiple-line edit box
		SetStyleRange(CGXRange().SetCols( iValueColumn ),
						CGXStyle()
							.SetControl( GX_IDS_CTRL_EDIT )
							.SetAllowEnter( TRUE )
							.SetAutoSize( TRUE )
							.SetWrapText( TRUE )
						);
	}
	else
	{
		// Adjust column numbers
		iNameColumn   = 1;
		iValueColumn  = 2;

		// Name column is left-justified static text
		SetStyleRange(CGXRange().SetCols( iNameColumn ),
						CGXStyle()
							.SetControl( GX_IDS_CTRL_STATIC )
						);

		// Value column is multiple-line edit box
		SetStyleRange(CGXRange().SetCols( iValueColumn ),
						CGXStyle()
							.SetControl( GX_IDS_CTRL_EDIT )
							.SetAllowEnter( TRUE )
							.SetAutoSize( TRUE )
							.SetWrapText( TRUE )
						);
	}

	// Set flag
	m_bIsPrepared = true;
}
//-------------------------------------------------------------------------------------------------
void CCheckGridWnd::PopulateGrid() 
{
	////////////////////
	// Add dummy entries
	////////////////////
	int		iCount = 8;
	CString	zName;
	CString	zValue;
	int		j = 1;

	for (int i = 1; i <= iCount; i++)
	{
		// Create strings
		zName.Format( "Name %d", i );
		zValue.Format( "Value %d\nValue2 %d", i, i );

		// Populate the vector
		vector<string>	vecText;
		vecText.push_back( string( zName.operator LPCTSTR() ) );
		vecText.push_back( string( zValue.operator LPCTSTR() ) );

		// Add the item
		SetRowInfo( j++, (j % 2 == 0) ? true : false, vecText );

		// Maybe add a duplicate item
		if (i % 3)
		{
			SetRowInfo( j++, (j % 2 == 0) ? true : false, vecText );
		}
	}
}
//-------------------------------------------------------------------------------------------------
int CCheckGridWnd::SetRowInfo(int nRow, bool bChecked, std::vector<std::string> vecStrings)
{
	int iNewItem = nRow;

	/////////////////////
	// Check column count
	/////////////////////
	int iNumStrings = vecStrings.size();
	int iColumnCount = GetColCount();

	// Consider presence/absence of check boxes, 
	if ((m_bShowCheck && (iNumStrings > iColumnCount - 1)) ||
		(!m_bShowCheck && (iNumStrings > iColumnCount)))
	{
		// Throw exception
		UCLIDException	ue( "ELI05072", "Too many strings for Grid control." );
		ue.addDebugInfo( "String count", iNumStrings );
		ue.addDebugInfo( "Column count", iColumnCount );
		ue.addDebugInfo( "Visible Checkboxes", m_bShowCheck );
		throw ue;
	}

	//////////////////
	// Check row count
	//////////////////
	if (nRow < 1)
	{
		// Throw exception
		UCLIDException	ue( "ELI05073", "Invalid row number." );
		ue.addDebugInfo( "Row number", nRow );
		throw ue;
	}

	if ((unsigned int) nRow > GetRowCount())
	{
		// Extend grid size
		InsertRows( nRow, 1 );
	}

	////////////////////////////////////////
	// Determine starting column for strings
	////////////////////////////////////////
	int iStart = 1;
	if (m_bShowCheck)
	{
		// Column 1 is actually for checkboxes
		iStart = 2;
	}

	// Default Normal attribute to 1
	long lNormal = 1;

	// Unlock the read-only cells
	GetParam()->SetLockReadOnly( FALSE );

	/////////////////////////////
	// Add the strings to the row
	/////////////////////////////
	int j = 0;
	string	strText;
	for (int i = iStart; i < iStart + iNumStrings; i++)
	{
		// Retrieve the string and increment counter
		strText = vecStrings.at( j++ );

		// Add this string and the appropriate Normal setting
		SetStyleRange(CGXRange( nRow, i ),
						CGXStyle()
							.SetValue( strText.c_str() )
							.SetUserAttribute( IDS_NORMAL_ATTRIBUTE, lNormal )
						);
	}

	/////////////////////////////////////////
	// Add blank text for any missing strings
	/////////////////////////////////////////
	for (int i = iStart + iNumStrings; i <= iColumnCount; i++)
	{
		// Add blank text and the appropriate Normal setting
		SetStyleRange(CGXRange( nRow, i ),
						CGXStyle()
							.SetValue( "" )
							.SetUserAttribute( IDS_NORMAL_ATTRIBUTE, lNormal )
						);
	}

	// Get resulting Normal attribute
	lNormal = getNormalAttribute( nRow );

	////////////////////
	// Set checked state
	////////////////////
	if (m_bShowCheck)
	{
		checkAttribute( nRow, bChecked, lNormal );
	}

	// Relock the read-only cells
	GetParam()->SetLockReadOnly( TRUE );

	return nRow;
}
//-------------------------------------------------------------------------------------------------
void CCheckGridWnd::SetValueColumn(int nCol)
{
	// Check column count
	int iColumnCount = GetColCount();
	if (iColumnCount < nCol)
	{
		// Throw exception
		UCLIDException	ue( "ELI05252", "Invalid column number." );
		ue.addDebugInfo( "Column number", nCol );
		ue.addDebugInfo( "Column count", iColumnCount );
		throw ue;
	}

	// Store the column and the string
	m_iValueColumn = nCol;
}
//-------------------------------------------------------------------------------------------------
void CCheckGridWnd::SetValueModifiedIndicator(int nCol, std::string strModified)
{
	// Check column count
	int iColumnCount = GetColCount();
	if (iColumnCount < nCol)
	{
		// Throw exception
		UCLIDException	ue( "ELI05202", "Invalid column number." );
		ue.addDebugInfo( "Column number", nCol );
		ue.addDebugInfo( "Column count", iColumnCount );
		throw ue;
	}

	// Store the column and the string
	m_iShowModificationColumn = nCol;
	m_strValueModified = strModified;
}

//-------------------------------------------------------------------------------------------------
// CCheckGridWnd private methods
//-------------------------------------------------------------------------------------------------
void CCheckGridWnd::checkAttribute(int iItem, bool bCheck, long lNormal)
{
	if (iItem > -1)
	{
		// Set or Clear the Check
		SetStyleRange(CGXRange( iItem, gACCEPT_COLUMN ),
						CGXStyle()
							.SetValue( bCheck ? "1" : "0" )
							.SetUserAttribute( IDS_NORMAL_ATTRIBUTE, lNormal )
						);

		// Set Attribute for other columns if != 1
		if (lNormal != 1)
		{
			int iCount = GetColCount();
			for (int i = gNAME_COLUMN; i <= iCount; i++)
			{
				SetStyleRange(CGXRange( iItem, i ),
								CGXStyle()
									.SetUserAttribute( IDS_NORMAL_ATTRIBUTE, lNormal )
								);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
long CCheckGridWnd::getNormalAttribute(int iItem)
{
	// First row is always Normal
	long lNormal = 1;

	if (iItem > 1)
	{
		int iStart = m_bShowCheck ? gNAME_COLUMN : gACCEPT_COLUMN;

		// Retrieve previous text
		CString	zPreviousText;
		zPreviousText = GetValueRowCol( iItem - 1, iStart );

		// Retrieve style information from previous row
		CGXStyle	style;
		GetStyleRowCol( iItem - 1, iStart, style, gxCopy, 0 );
		lNormal = style.GetUserAttribute( IDS_NORMAL_ATTRIBUTE ).GetLongValue();

		// Retrieve current text
		CString	zCurrentText;
		zCurrentText = GetValueRowCol( iItem, iStart );

		// Check new Text against previous text
		if (zCurrentText.Compare( zPreviousText.operator LPCTSTR() ) != 0)
		{
			// Names differ, lNormal values should also differ
			lNormal = (lNormal == 0) ? 1 : 0;
		}
	}

	return lNormal;
}
//-------------------------------------------------------------------------------------------------
