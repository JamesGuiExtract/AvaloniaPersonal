// CheckCheckGridWnd.cpp : implementation file
//

#include "stdafx.h"
#include "CheckCheckComboGridWnd.h"
#include "Notifications.h"
#include "resource.h"

#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Gray background color for rows where Normal attribute = 0
const COLORREF g_colorAltBack = RGB( 192, 192, 192 );

// The column which contains the drop down column
const int giDROP_DOWN_COL = 3;

//-------------------------------------------------------------------------------------------------
// CCheckCheckComboGridWnd
//-------------------------------------------------------------------------------------------------
CCheckCheckComboGridWnd::CCheckCheckComboGridWnd()
: m_bIsPrepared(false),
  m_iResizeColumn(-1),
  m_iRowToNotHighlight(-1),
  m_nControlID(-1)
{
}
//-------------------------------------------------------------------------------------------------
CCheckCheckComboGridWnd::~CCheckCheckComboGridWnd()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27612");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCheckCheckComboGridWnd, CGXGridWnd)
	//{{AFX_MSG_MAP(CCheckCheckComboGridWnd)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CCheckCheckComboGridWnd message handlers
//-------------------------------------------------------------------------------------------------
void CCheckCheckComboGridWnd::Initialize()
{
	try
	{
		CGXGridWnd::Initialize();

		// Override the default combo box so the drop down list will always display button
		CGXComboBox* pWnd = new CGXComboBox(this, GX_IDS_CTRL_CBS_DROPDOWN,
			GX_IDS_CTRL_CBS_DROPDOWN+1, GXCOMBO_TEXTFIT);
		ASSERT_RESOURCE_ALLOCATION("ELI27613", pWnd != NULL);

		pWnd->m_bInactiveDrawButton = TRUE;

		// Register the new control (the Grid will automatically release the memory when
		// the control is destroyed)
		RegisterControl(GX_IDS_CTRL_CBS_DROPDOWNLIST, pWnd);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27614");
}
//-------------------------------------------------------------------------------------------------
void CCheckCheckComboGridWnd::OnClickedButtonRowCol(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Only perform the check if the column is a check box column
		if (nCol == 1 || nCol == 2)
		{
			// Checked Locked state
			if (GetRowLock( nRow ))
			{
				// Retrieve new check state
				CString	zTemp;
				zTemp = GetValueRowCol( nRow, nCol );
				long nCheck = asLong((LPCTSTR)zTemp);

				// Restore previous setting and return
				SetValueRange( CGXRange(nRow, nCol), (nCheck == 1) ? "0" : "1" );
				return;
			}
			else
			{
				// Post the message with 
				//   wParam = Control ID
				//   lParam: LOWORD nRow, HIWORD = nCol
				GetParent()->PostMessage( WM_NOTIFY_GRID_LCLICK, m_nControlID, 
					MAKELPARAM( nRow, nCol ) );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27615");
}
//-------------------------------------------------------------------------------------------------
BOOL CCheckCheckComboGridWnd::GetStyleRowCol(ROWCOL nRow, ROWCOL nCol, CGXStyle& style, GXModifyType mt, int nType)
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
				// Make sure this is not the selected drop down list column
				if (nCol != giDROP_DOWN_COL || nRow != m_iRowToNotHighlight)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27616");
	// if it gets here method failed
	return FALSE;
} 
//-------------------------------------------------------------------------------------------------
BOOL CCheckCheckComboGridWnd::OnTrackColWidth(ROWCOL nCol)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27617");
	// if it gets here method failed
	return FALSE;
} 
//-------------------------------------------------------------------------------------------------
void CCheckCheckComboGridWnd::DrawInvertCell(CDC* /*pDC*/, ROWCOL nRow, ROWCOL nCol, CRect rectItem)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27618");
} 
//-------------------------------------------------------------------------------------------------
BOOL CCheckCheckComboGridWnd::OnLButtonDblClkRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27619");
	// if it gets here method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CCheckCheckComboGridWnd::OnLButtonHitRowCol(ROWCOL nRow, ROWCOL nCol, ROWCOL nDragRow,
												 ROWCOL nDragCol, CPoint cpPoint, UINT uiFlags,
												 WORD nHitState)
{
	try
	{
		if (nCol == giDROP_DOWN_COL)
		{
			m_iRowToNotHighlight = nRow;
		}
		else
		{
			m_iRowToNotHighlight = -1;
		}

		return CGXGridWnd::OnLButtonHitRowCol(nRow, nCol, nDragRow, nDragCol, cpPoint,
			uiFlags, nHitState);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27620");

	// If we reach here the method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CCheckCheckComboGridWnd::OnLButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol,
													 UINT nFlags, CPoint pt)
{
	try
	{
		if (nCol == giDROP_DOWN_COL)
		{
			m_iRowToNotHighlight = nRow;
		}
		else
		{
			m_iRowToNotHighlight = -1;
		}

		return CGXGridWnd::OnLButtonClickedRowCol(nRow, nCol, nFlags, pt);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27621");

	// If we reached here the method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CCheckCheckComboGridWnd::OnModifyCell(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Only post the message if the cell being modified is the drop down column
		if (m_nControlID != -1 && nCol == giDROP_DOWN_COL)
		{
			// Post the message with 
			//   wParam = Control ID
			//   lParam: LOWORD nRow, HIWORD = nCol
			GetParent()->PostMessage( WM_NOTIFY_CELL_MODIFIED, m_nControlID, 
				MAKELPARAM( nRow, nCol ) );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27622");
}

//-------------------------------------------------------------------------------------------------
// CCheckCheckComboGridWnd public methods
//-------------------------------------------------------------------------------------------------
void CCheckCheckComboGridWnd::CheckItem(int iItem, int iSubitem, bool bCheck)
{
	// Validate iSubitem
	if ((iSubitem == 1) || (iSubitem == 2))
	{
		// Set the check
		checkAttribute( iItem, iSubitem, bCheck);
	}
}
//-------------------------------------------------------------------------------------------------
void CCheckCheckComboGridWnd::Clear()
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
void CCheckCheckComboGridWnd::DoResize() 
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
CString	CCheckCheckComboGridWnd::GetCellValue(ROWCOL nRow, ROWCOL nCol)
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
bool CCheckCheckComboGridWnd::GetCheckedState(int iRow, int iColumn) 
{
	int iCount = 0;

	// Validate iRow
	long lCount = GetRowCount();
	if ((iRow < 1) || (iRow > lCount))
	{
		UCLIDException ue( "ELI27629", "Invalid row number" );
		ue.addDebugInfo( "Row Number", iRow );
		ue.addDebugInfo( "Row Count", lCount );
		throw ue;
	}

	// Validate iColumn
	if ((iColumn == 1) || (iColumn == 2))
	{
		CString	zTemp;
		zTemp = GetValueRowCol( iRow, iColumn );
		long nCheck = asLong((LPCTSTR)zTemp);

		return (nCheck == 1);
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
int CCheckCheckComboGridWnd::GetFirstSelectedRow()
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
bool CCheckCheckComboGridWnd::GetRowLock(int nRow)
{
	// Retrieve style information from first column of specified row
	CGXStyle	style;
	GetStyleRowCol( nRow, 1, style, gxCopy, 0 );
	long lValue = style.GetUserAttribute( IDS_LOCKED_ATTRIBUTE ).GetLongValue();

	return (lValue == 1);
}
//-------------------------------------------------------------------------------------------------
void CCheckCheckComboGridWnd::SetRowLock(int nRow, bool bLocked)
{
	// Set the attribute of first column in specified row
	SetStyleRange(CGXRange().SetCells( nRow, 1 ),
					CGXStyle()
						.SetUserAttribute( IDS_LOCKED_ATTRIBUTE, bLocked ? 1L : 0L )
					);
}
//--------------------------------------------------------------------------------------------------
void CCheckCheckComboGridWnd::PrepareGrid(const vector<string>& vecHeaders,
										  const vector<int>& vecWidths,
										  const vector<string>& vecComboList) 
{
	// Ensure there is at least 1 item for the combo list
	ASSERT_ARGUMENT("ELI27623", vecComboList.size() > 0);

	// Clear the set
	m_setComboList.clear();

	// Add the combo list items to the set
	m_setComboList.insert(vecComboList.begin(), vecComboList.end());

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
		SetColWidth( i, i, iColumnWidth, NULL, GX_UPDATENOW, gxDo );

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

	// Set the choice list for the combo box
	vector<string>::const_iterator it = vecComboList.begin();
	string strList = *it;
	for (++it; it != vecComboList.end(); it++)
	{
		strList += "\n" + (*it);
	}

	// Third column is a combo box
	SetStyleRange(CGXRange().SetCols(3),
					CGXStyle()
						.SetControl(GX_IDS_CTRL_CBS_DROPDOWNLIST)
						.SetHorizontalAlignment( DT_LEFT )
						.SetChoiceList(strList.c_str())
					);

	// Last column is centered static text
	SetStyleRange(CGXRange().SetCols( lCount ),
					CGXStyle()
						.SetControl( GX_IDS_CTRL_STATIC )
						.SetHorizontalAlignment( DT_CENTER )
					);

	// Remaining columns are left-justified static text
	for (int j = 4; j < lCount; j++)
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
void CCheckCheckComboGridWnd::SelectRow(int nRow)
{
	// Clear existing selection
	SetSelection( 0 );

	// Set selection on this row
	CGXRangeList* pList = GetParam()->GetRangeList();
	ASSERT_RESOURCE_ALLOCATION("ELI27624", pList != NULL);
	POSITION area = pList->AddTail( new CGXRange );
	SetSelection( area, nRow, 1, nRow, GetColCount() );
}
//-------------------------------------------------------------------------------------------------
void CCheckCheckComboGridWnd::SetControlID(int nID) 
{
	m_nControlID = nID;
}
//-------------------------------------------------------------------------------------------------
int CCheckCheckComboGridWnd::SetRowInfo(int nRow, bool bChecked1, bool bChecked2,
										const string& strComboValue, const vector<string>& vecStrings)
{
	// Ensure the combo value is valid
	if (m_setComboList.find(strComboValue) == m_setComboList.end())
	{
		UCLIDException uex("ELI27625", "Combo value was not found in the list.");
		uex.addDebugInfo("Combo Value", strComboValue);
		throw uex;
	}

	int iNewItem = nRow;

	// Check column count
	int iNumStrings = vecStrings.size();
	int iColumnCount = GetColCount();

	// Consider presence/absence of check boxes, 
	if (iNumStrings > iColumnCount + 2)
	{
		// Throw exception
		UCLIDException	ue( "ELI27626", "Too many strings for Grid control." );
		ue.addDebugInfo( "String count", iNumStrings );
		ue.addDebugInfo( "Column count", iColumnCount );
		ue.addDebugInfo( "Checkbox Count", 2 );
		throw ue;
	}

	// Check row count
	if (nRow < 1)
	{
		// Throw exception
		UCLIDException	ue( "ELI27627", "Invalid row number." );
		ue.addDebugInfo( "Row number", nRow );
		throw ue;
	}

	if ((unsigned int) nRow > GetRowCount())
	{
		// Extend grid size
		InsertRows( nRow, 1 );
	}

	// Set starting column for strings
	int iStart = 4;

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

	// Set the appropriate combo value
	SetStyleRange(CGXRange(nRow, 3),
					CGXStyle()
						.SetValue(strComboValue.c_str())
					);

	// Set checked states
	checkAttribute( nRow, 1, bChecked1 );
	checkAttribute( nRow, 2, bChecked2 );

	// Set the drop down item

	// Relock the read-only cells
	GetParam()->SetLockReadOnly( TRUE );

	return nRow;
}
//-------------------------------------------------------------------------------------------------
BOOL CCheckCheckComboGridWnd::OnStartSelection(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27628");
	// if it gets here the start selection failed
	return FALSE;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CCheckCheckComboGridWnd::checkAttribute(int iItem, int iSubitem, bool bCheck)
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
