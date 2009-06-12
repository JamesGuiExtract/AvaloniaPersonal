// CheckCheckGridWnd.cpp : implementation file
//

#include "stdafx.h"
#include "CheckCheckGridWnd.h"
#include "Notifications.h"
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
// CCheckCheckGridWnd
//-------------------------------------------------------------------------------------------------
CCheckCheckGridWnd::CCheckCheckGridWnd()
: m_bIsPrepared(false),
  m_iResizeColumn(-1),
  m_nControlID(-1)
{
}
//-------------------------------------------------------------------------------------------------
CCheckCheckGridWnd::~CCheckCheckGridWnd()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16473");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCheckCheckGridWnd, CGXGridWnd)
	//{{AFX_MSG_MAP(CCheckCheckGridWnd)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CCheckCheckGridWnd message handlers
//-------------------------------------------------------------------------------------------------
void CCheckCheckGridWnd::OnClickedButtonRowCol(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Retrieve new check state
		CString	zTemp;
		zTemp = GetValueRowCol( nRow, nCol );
		int nCheck = atol( zTemp.operator LPCTSTR() );

		// Checked Locked state
		if (GetRowLock( nRow ))
		{
			// Restore previous setting and return
			SetValueRange( CGXRange(nRow, nCol), (nCheck == 1) ? "0" : "1" );
			return;
		}
		else
		{
			// Only Post a message for checkbox columns
			// A message is posted for other columns within OnStartSelection()
			if ((nCol == 1) || (nCol == 2))
			{
				// Post the message with 
				//   wParam = Control ID
				//   lParam: LOWORD nRow, HIWORD = nCol
				GetParent()->PostMessage( WM_NOTIFY_GRID_LCLICK, m_nControlID, 
					MAKELPARAM( nRow, nCol ) );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13690");
}
//-------------------------------------------------------------------------------------------------
BOOL CCheckCheckGridWnd::GetStyleRowCol(ROWCOL nRow, ROWCOL nCol, CGXStyle& style, GXModifyType mt, int nType)
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
				if (m_iResizeColumn != nCol)
				{
					// Use system colors for background and text
					style.SetInterior(::GetSysColor(COLOR_HIGHLIGHT));
					style.SetTextColor(::GetSysColor(COLOR_HIGHLIGHTTEXT));
					bRet = TRUE; 
				}
			}
		}
		
		return bRet;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13691");
	// if it gets here method failed
	return FALSE;
} 
//-------------------------------------------------------------------------------------------------
BOOL CCheckCheckGridWnd::OnTrackColWidth(ROWCOL nCol)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13692");
	// if it gets here method failed
	return FALSE;
} 
//-------------------------------------------------------------------------------------------------
void CCheckCheckGridWnd::DrawInvertCell(CDC* /*pDC*/, ROWCOL nRow, ROWCOL nCol, CRect rectItem)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13693");
} 
//-------------------------------------------------------------------------------------------------
BOOL CCheckCheckGridWnd::OnLButtonDblClkRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
{
	try
	{
		// Check to see if the Control ID is defined
		if (m_nControlID != -1)
		{
			// Post the message with 
			//   wParam = Control ID
			//   lParam: LOWORD nRow, HIWORD = nCol
			GetParent()->PostMessage( WM_NOTIFY_CELL_DBLCLK, m_nControlID, 
				MAKELPARAM( nRow, nCol ) );
		}
		
		return TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13942");
	// if it gets here method failed
	return FALSE;
}

//-------------------------------------------------------------------------------------------------
// CCheckCheckGridWnd public methods
//-------------------------------------------------------------------------------------------------
void CCheckCheckGridWnd::CheckItem(int iItem, int iSubitem, bool bCheck)
{
	// Validate iSubitem
	if ((iSubitem == 1) || (iSubitem == 2))
	{
		// Set the check
		checkAttribute( iItem, iSubitem, bCheck);
	}
}
//-------------------------------------------------------------------------------------------------
void CCheckCheckGridWnd::Clear()
{
	long lRowCount = GetRowCount();
	long lColCount = GetColCount();

	if ((lRowCount > 0) && (lColCount > 0))
	{
		// Determine starting column
		int iStart = 3;

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
void CCheckCheckGridWnd::DoResize() 
{
	// Just return if PrepareGrid() has not been called
	if (!m_bIsPrepared)
	{
		return;
	}

	// Adjust row heights as needed
	ResizeRowHeightsToFit( CGXRange( ).SetTable() );

	// Determine overall width and width of unsized column
	CRect	rectGrid;
	GetClientRect( rectGrid );
	long lWidth = rectGrid.Width();
	int iCount = GetColCount();
	for (int i = 1; i <= iCount; i++)
	{
		// Get width of this column
		int iThisWidth = GetColWidth( i );

		// Save this column number if first unsized column
		if ((iThisWidth == 0) && (m_iResizeColumn == -1))
		{
			m_iResizeColumn = i;
		}

		// Subtract this width from total except for resized column
		if (i != m_iResizeColumn)
		{
			lWidth -= iThisWidth;
		}
	}

	// Set width of unsized column - allow space for border to prevent scrollbar appearance
	if (m_iResizeColumn != -1)
	{
		SetColWidth( m_iResizeColumn, m_iResizeColumn, lWidth - 1, NULL, GX_UPDATENOW, gxDo );
	}
}
//-------------------------------------------------------------------------------------------------
CString	CCheckCheckGridWnd::GetCellValue(ROWCOL nRow, ROWCOL nCol)
{
	// If this cell is currently active - do not depend on stored data
	if (IsCurrentCell(nRow, nCol) && IsActiveCurrentCell())
	{
		CString s;
		CGXControl* pControl = GetControl(nRow, nCol);
		pControl->GetValue(s);
		return s;
	}
	// This cell is not currently active - stored data is okay
	else
	{
		return GetValueRowCol(nRow, nCol);
	}
}
//-------------------------------------------------------------------------------------------------
bool CCheckCheckGridWnd::GetCheckedState(int iRow, int iColumn) 
{
	int iCount = 0;

	// Validate iRow
	long lCount = GetRowCount();
	if ((iRow < 1) || (iRow > lCount))
	{
		UCLIDException ue( "", "Invalid row number" );
		ue.addDebugInfo( "Row Number", iRow );
		ue.addDebugInfo( "Row Count", lCount );
		throw ue;
	}

	// Validate iColumn
	if ((iColumn == 1) || (iColumn == 2))
	{
		CString	zTemp;
		zTemp = GetValueRowCol( iRow, iColumn );
		int nCheck = atol( zTemp.operator LPCTSTR() );

		return (nCheck == 1);
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
int CCheckCheckGridWnd::GetFirstSelectedRow()
{
	// Retrieve list of selected rows
	CGXRangeList selList;
	if (CopyRangeList( selList, FALSE ))
	{
		// Return row number of first selected row
		CGXRange range = selList.GetTail();
		return range.top;
	}

	// No rows are selected
	return -1;
}
//-------------------------------------------------------------------------------------------------
bool CCheckCheckGridWnd::GetRowLock(int nRow)
{
	// Retrieve style information from first column of specified row
	CGXStyle	style;
	GetStyleRowCol( nRow, 1, style, gxCopy, 0 );
	long lValue = style.GetUserAttribute( IDS_LOCKED_ATTRIBUTE ).GetLongValue();

	return (lValue == 1);
}
//-------------------------------------------------------------------------------------------------
void CCheckCheckGridWnd::SetRowLock(int nRow, bool bLocked)
{
	// Set the attribute of first column in specified row
	SetStyleRange(CGXRange().SetCells( nRow, 1 ),
					CGXStyle()
						.SetUserAttribute( IDS_LOCKED_ATTRIBUTE, bLocked ? 1L : 0L )
					);
}
//--------------------------------------------------------------------------------------------------
void CCheckCheckGridWnd::PrepareGrid(std::vector<std::string> vecHeaders, std::vector<int> vecWidths) 
{
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

	// Set column widths and Header labels
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
						);
	}

	// Entire grid has vertically-centered items
	SetStyleRange(CGXRange().SetTable(),
					CGXStyle()
						.SetVerticalAlignment( DT_VCENTER )
					);

	// First two columns are centered 3-D checkboxes
	// Locked attribute is OFF
	SetStyleRange(CGXRange().SetCols( 1 ),
					CGXStyle()
						.SetControl( GX_IDS_CTRL_CHECKBOX3D )
						.SetHorizontalAlignment( DT_CENTER )
						.SetUserAttribute( IDS_LOCKED_ATTRIBUTE, 0L )
					);
	SetStyleRange(CGXRange().SetCols( 2 ),
					CGXStyle()
						.SetControl( GX_IDS_CTRL_CHECKBOX3D )
						.SetHorizontalAlignment( DT_CENTER )
						.SetUserAttribute( IDS_LOCKED_ATTRIBUTE, 0L )
					);

	// Last column is centered static text
	SetStyleRange(CGXRange().SetCols( lCount ),
					CGXStyle()
						.SetControl( GX_IDS_CTRL_STATIC )
						.SetHorizontalAlignment( DT_CENTER )
					);

	// Remaining columns are left-justified static text
	for (int j = 3; j < lCount; j++)
	{
		SetStyleRange(CGXRange().SetCols( j ),
						CGXStyle()
							.SetControl( GX_IDS_CTRL_STATIC )
							.SetHorizontalAlignment( DT_LEFT )
						);
	}

	// Set flag
	m_bIsPrepared = true;
}
//-------------------------------------------------------------------------------------------------
void CCheckCheckGridWnd::PopulateGrid() 
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
		zValue.Format( "Value %d", i );

		// Populate the vector
		vector<string>	vecText;
		vecText.push_back( string( zName.operator LPCTSTR() ) );
		vecText.push_back( string( zValue.operator LPCTSTR() ) );

		// Add the item
		SetRowInfo( j++, (j % 2 == 0) ? true : false, (j % 3 == 0) ? true : false, vecText );

		// Maybe add a duplicate item
		if (i % 3)
		{
			SetRowInfo( j++, (j % 2 == 0) ? true : false, (j % 3 == 0) ? true : false, vecText );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CCheckCheckGridWnd::SelectRow(int nRow)
{
	// Clear existing selection
	SetSelection( 0 );

	// Set selection on this row
	CGXRangeList* pList = GetParam()->GetRangeList();
	ASSERT_RESOURCE_ALLOCATION("ELI15633", pList != NULL);
	POSITION area = pList->AddTail( new CGXRange );
	SetSelection( area, nRow, 1, nRow, GetColCount() );
}
//-------------------------------------------------------------------------------------------------
void CCheckCheckGridWnd::SetControlID(int nID) 
{
	m_nControlID = nID;
}
//-------------------------------------------------------------------------------------------------
int CCheckCheckGridWnd::SetRowInfo(int nRow, bool bChecked1, bool bChecked2, 
							  std::vector<std::string> vecStrings)
{
	int iNewItem = nRow;

	// Check column count
	int iNumStrings = vecStrings.size();
	int iColumnCount = GetColCount();

	// Consider presence/absence of check boxes, 
	if (iNumStrings > iColumnCount + 2)
	{
		// Throw exception
		UCLIDException	ue( "ELI13694", "Too many strings for Grid control." );
		ue.addDebugInfo( "String count", iNumStrings );
		ue.addDebugInfo( "Column count", iColumnCount );
		ue.addDebugInfo( "Checkbox Count", 2 );
		throw ue;
	}

	// Check row count
	if (nRow < 1)
	{
		// Throw exception
		UCLIDException	ue( "ELI13695", "Invalid row number." );
		ue.addDebugInfo( "Row number", nRow );
		throw ue;
	}

	if ((unsigned int) nRow > GetRowCount())
	{
		// Extend grid size
		InsertRows( nRow, 1 );
	}

	// Set starting column for strings
	int iStart = 3;

	// Unlock the read-only cells
	GetParam()->SetLockReadOnly( FALSE );

	// Add the strings to the row
	int j = 0;
	string	strText;
	for (int i = iStart; i < iStart + iNumStrings; i++)
	{
		// Retrieve the string and increment counter
		strText = vecStrings.at( j++ );

		// Add this string
		SetStyleRange(CGXRange( nRow, i ),
						CGXStyle()
							.SetValue( strText.c_str() )
						);
	}

	// Add blank text for any missing strings
	for (int i = iStart + iNumStrings; i <= iColumnCount; i++)
	{
		// Add blank text
		SetStyleRange(CGXRange( nRow, i ),
						CGXStyle()
							.SetValue( "" )
						);
	}

	// Set checked states
	checkAttribute( nRow, 1, bChecked1 );
	checkAttribute( nRow, 2, bChecked2 );

	// Relock the read-only cells
	GetParam()->SetLockReadOnly( TRUE );

	return nRow;
}
//-------------------------------------------------------------------------------------------------
BOOL CCheckCheckGridWnd::OnStartSelection(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
{
	try
	{
		// Check flags for Left Button vs. Right Button
		if (flags && MK_LBUTTON)
		{
			// Check to see if Control ID is defined
			if (m_nControlID != -1)
			{
				// Only Post a message for non-checkbox columns
				// A message is posted for checkbox columns within OnClickedButtonRowCol()
				// Commented out to fix P13: 4099 (Liang)
				//if ((nCol != 1) && (nCol != 2))
				//{
					// Post the message with 
					//   wParam = Control ID
					//   lParam: LOWORD nRow, HIWORD = nCol
					GetParent()->PostMessage( WM_NOTIFY_GRID_LCLICK, m_nControlID, 
						MAKELPARAM( nRow, nCol ) );
				//}
			}
		}
		
		return TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13689");
	// if it gets here the start selection failed
	return FALSE;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CCheckCheckGridWnd::checkAttribute(int iItem, int iSubitem, bool bCheck)
{
	// Validate iSubitem
	if ((iSubitem < 1) || (iSubitem > 2))
	{
		// Create and throw exception
	}

	if (iItem > 0)
	{
		// Set or Clear the Check
		SetStyleRange(CGXRange( iItem, iSubitem ),
						CGXStyle()
							.SetValue( bCheck ? "1" : "0" )
						);
	}
}
//-------------------------------------------------------------------------------------------------
