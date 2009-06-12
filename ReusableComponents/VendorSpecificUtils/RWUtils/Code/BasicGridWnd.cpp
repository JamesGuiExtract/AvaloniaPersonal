// CBasicGridWnd.cpp : implementation file
//

#include "stdafx.h"
#include "BasicGridWnd.h"
#include "Notifications.h"

#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CBasicGridWnd
//-------------------------------------------------------------------------------------------------
CBasicGridWnd::CBasicGridWnd()
: m_bIsPrepared(false),
  m_bShowColHeader(true),
  m_bShowRowHeader(true),
  m_bEditStarted(false),
  m_iEditRow(-1),
  m_nControlID(-1),
  m_nResizeStyle(0),
  m_bIsModified(false)
{
}
//-------------------------------------------------------------------------------------------------
CBasicGridWnd::~CBasicGridWnd()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16472");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBasicGridWnd, CGXGridWnd)
	//{{AFX_MSG_MAP(CDataEntryGrid)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CBasicGridWnd message handlers
//-------------------------------------------------------------------------------------------------
void CBasicGridWnd::DrawInvertCell(CDC* /*pDC*/, ROWCOL nRow, ROWCOL nCol, CRect rectItem)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12782");
} 
//-------------------------------------------------------------------------------------------------
BOOL CBasicGridWnd::GetStyleRowCol(ROWCOL nRow, ROWCOL nCol, CGXStyle& style, GXModifyType mt, 
								   int nType)
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
				// and that this grid is active
				if (!m_bEditStarted || (m_iEditRow != nRow))
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12783");
	//if it gets here the setting of the information failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CBasicGridWnd::OnStartEditing(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Set flag
		m_bEditStarted = true;

		// Store edit row
		m_iEditRow = nRow;

		// Save original text
		m_zOriginalText = GetValueRowCol(nRow, nCol);

		return CGXGridCore::OnStartEditing(nRow, nCol);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12784");
	//if it gets here the StartEditing failed
	return FALSE;

}
//-------------------------------------------------------------------------------------------------
BOOL CBasicGridWnd::OnEndEditing(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Clear flag
		m_bEditStarted = false;
		
		// Clear edit row
		m_iEditRow = -1;

		// Retrieve current contents
		CString zCurrent = GetCellValue( nRow, nCol );

		// Compare with original - set modified flag if changed
		if (zCurrent.Compare( m_zOriginalText ) != 0)
		{
			SetModified();
			m_zOriginalText = zCurrent;
		}

		return CGXGridCore::OnEndEditing(nRow, nCol);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12785");
	//if it gets here the End Editing failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CBasicGridWnd::OnStartSelection(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
{
	try
	{
		// Check flags for Left Button vs. Right Button
		if (flags && MK_LBUTTON)
		{
			// Check to see if Control ID is defined
			if (m_nControlID != -1)
			{
				// Post the message with 
				//   wParam = Control ID
				//   lParam: LOWORD nRow, HIWORD = nCol
				GetParent()->PostMessage( WM_NOTIFY_GRID_LCLICK, m_nControlID, 
					MAKELPARAM( nRow, nCol ) );
			}
		}
		
		return TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12786");
	// if it gets here the start selection failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CBasicGridWnd::OnRButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
{
	try
	{
		// Check to see if the Control ID is defined
		if (m_nControlID != -1)
		{
			// Post the message with 
			//   wParam = Control ID
			//   lParam: LOWORD nRow, HIWORD = nCol
			GetParent()->PostMessage( WM_NOTIFY_GRID_RCLICK, m_nControlID, 
				MAKELPARAM( nRow, nCol ) );
		}

		return TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12787");
	// if it gets here the RButtonClickedRowCol failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CBasicGridWnd::MoveCurrentCell(int direction, UINT nCell, BOOL bCanSelectRange)
{
	try
	{
		// Get current cell in this grid
		ROWCOL	nRow = 0;
		ROWCOL	nCol = 0;
		BOOL bResult = GetCurrentCell( nRow, nCol );
		if (bResult == TRUE)
		{
			// Current cell is defined, tell the parent to do the move based on direction
			switch (direction)
			{
			case GX_LEFT:
				{
					// Tell Parent to move left
					GetParent()->PostMessage( WM_NOTIFY_MOVE_CELL_LEFT, m_nControlID, 0 );
				}
				break;

			case GX_UP:
				{
					// Tell Parent to move up
					GetParent()->PostMessage( WM_NOTIFY_MOVE_CELL_UP, m_nControlID, 0 );
				}
				break;

			case GX_RIGHT:
				{
					// Tell Parent to move right
					GetParent()->PostMessage( WM_NOTIFY_MOVE_CELL_RIGHT, m_nControlID, 0 );
				}
				break;

			case GX_DOWN:
				{
					// Tell Parent to move down
					GetParent()->PostMessage( WM_NOTIFY_MOVE_CELL_DOWN, m_nControlID, 0 );
				}
				break;

			// Other directions are ignored at this time

			}
		}

		return bResult;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13104");

	return FALSE;
}

//-------------------------------------------------------------------------------------------------
// CBasicGridWnd public methods
//-------------------------------------------------------------------------------------------------
void CBasicGridWnd::Clear()
{
	long lRowCount = GetRowCount();
	long lColCount = GetColCount();

	// Reset current cell, if needed
	if (IsCurrentCell())
	{
		SetCurrentCell(GX_INVALID, GX_INVALID);
	}

	if ((lRowCount > 0) && (lColCount > 0))
	{
		// Unlock the read-only cells
		GetParam()->SetLockReadOnly( FALSE );

		// Set the cell range
		CGXRange	range( 1, 1, lRowCount, lColCount );

		// Clear the text
		SetValueRange( range, "" );

		// Remove the extra rows
		RemoveRows( 1, lRowCount );

		// Relock the read-only cells
		GetParam()->SetLockReadOnly( TRUE );
	}
}
//-------------------------------------------------------------------------------------------------
void CBasicGridWnd::DoResize(long lVerticalScrollPixels)
{
	// Just return if PrepareGrid() has not been called
	if (!m_bIsPrepared)
	{
		return;
	}

	// Adjust row heights as needed
	ResizeRowHeightsToFit( CGXRange( ).SetTable() );

	// Determine overall width of grid
	CRect rectGrid = GetGridRect();
	long lWidth = rectGrid.Width();

	// Remove width of row headers, if present
	if (m_bShowRowHeader)
	{
		lWidth -= GetColWidth( 0 );
	}

	// Count number of columns with zero width
	int iCount = GetColCount();
	int iZeroCount = 0;
	for (int i = 1; i <= iCount; i++)
	{
		int iWidth = GetColWidth( i );

		if (iWidth == 0)
		{
			iZeroCount++;
		}
		// Remove size of fixed width column
		else
		{
			lWidth -= iWidth;
		}
	}

	// Remove width of grid-based vertical scrollbar
	lWidth -= lVerticalScrollPixels;

	// Adjust size of variable-width columns, or each fixed-width column
	long lAdjustment = lWidth / iCount;
	if (iZeroCount > 0)
	{
		lAdjustment = lWidth / iZeroCount;
	}

	for (int i = 1; i <= iCount; i++)
	{
		int iWidth = GetColWidth( i );

		if (i == iCount)
		{
			// Make space for border to avoid scroll bar
			lAdjustment--;
		}

		// Set variable-width column to appropriate fraction of remaining width
		if (iWidth == 0)
		{
			SetColWidth( i, i, lAdjustment, NULL, GX_UPDATENOW, gxDo );
		}
		// Adjust fixed-width column to use up fraction of remaining width
		else if (iZeroCount == 0)
		{
			SetColWidth( i, i, iWidth + lAdjustment - 1, NULL, GX_UPDATENOW, gxDo );
		}
		// Adjust fixed-width last column to shrink 1 pixel
		else if ((iZeroCount > 0) && (i == iCount))
		{
			SetColWidth( i, i, iWidth, NULL, GX_UPDATENOW, gxDo );
		}
	}

	// Adjust row heights based on input parameters
	if (m_nResizeStyle == 1)
	{
		int nRowCount = GetRowCount();
		int nNewHeight = rectGrid.Height() / nRowCount;

		if (nRowCount > 1)
		{
			SetRowHeight( 1, nRowCount - 1, nNewHeight );

			// Adjust new height of last row by 1 pixel to remove scroll bar
			SetRowHeight( nRowCount, nRowCount, nNewHeight - 1 );
		}
		else
		{
			// Adjust new height by 1 pixel to remove scroll bar
			SetRowHeight( 1, nRowCount, nNewHeight - 1 );
		}
	}
}
//-------------------------------------------------------------------------------------------------
CString	CBasicGridWnd::GetCellValue(ROWCOL nRow, ROWCOL nCol, bool bSaveIt)
{
	// If this cell is currently active - do not depend on stored data
	if (IsCurrentCell(nRow, nCol) && IsActiveCurrentCell())
	{
		CString s;
		CGXControl* pControl = GetControl(nRow, nCol);
		pControl->GetValue(s);
		if (bSaveIt)
		{
			m_zOriginalText = s;
		}
		return s;
	}
	// This cell is not currently active - stored data is okay
	else
	{
		return GetValueRowCol(nRow, nCol);
	}
}
//-------------------------------------------------------------------------------------------------
int CBasicGridWnd::GetControlID()
{
	return m_nControlID;
}
//-------------------------------------------------------------------------------------------------
bool CBasicGridWnd::IsEmpty()
{
	bool bEmpty = true;

	// Check each row and column - ignoring headers
	int nRows = GetRowCount();
	int nCols = GetColCount();
	for (int i = 1; i <= nRows; i++)
	{
		// Check each column in this row
		for (int j = 1; j <= nCols; j++)
		{
			CString zCell = GetCellValue( i, j );
			if (!zCell.IsEmpty())
			{
				bEmpty = false;
				break;
			}
		}

		// Quit checking rows if already found non-empty cell
		if (!bEmpty)
		{
			break;
		}
	}

	return bEmpty;
}
//-------------------------------------------------------------------------------------------------
bool CBasicGridWnd::IsModified()
{
	// Check state of currently editing cell
	bool bEditing = false;
	ROWCOL	nRow = 0;
	ROWCOL	nCol = 0;
	BOOL bResult2 = GetCurrentCell( nRow, nCol );
	if (bResult2 == TRUE)
	{
		// Is cell being edited?
		if (IsActiveCurrentCell())
		{
			// Get current contents
			CString zCurrent;
			CGXControl* pControl = GetControl(nRow, nCol);
			pControl->GetValue( zCurrent );

			// Compare with original contents
			if (zCurrent.Compare( m_zOriginalText ) != 0)
			{
				// Contents have changed
				bEditing = true;
			}
		}
		// else the current cell is not being edited
	}
	// else there is no current cell

	return (m_bIsModified || bEditing);
}
//-------------------------------------------------------------------------------------------------
void CBasicGridWnd::PrepareGrid(std::vector<std::string> vecColHeaders, std::vector<int> vecWidths, 
								std::vector<std::string> vecRowHeaders, long lRowHeaderWidth, 
								bool bShowColHeaders, bool bShowRowHeaders, int nStyle)
{
	// Store settings
	m_bShowColHeader = bShowColHeaders;
	m_bShowRowHeader = bShowRowHeaders;
	m_nResizeStyle = nStyle;

	// Initialize the grid. For CWnd based grids 
	// this call is essential. For view based 
	// grids this initialization is done in 
	// OnInitialUpdate.
	Initialize();

	// Setup grid - allow Undo only during an edit session (P16 #1890)
	GetParam()->EnableUndo(FALSE);

	// Set full-row selection
//	GetParam()->EnableSelection((WORD) (GX_SELFULL & ~GX_SELCOL & ~GX_SELTABLE));
//	GetParam()->SetSpecialMode( GX_MODELBOX_SS );

	// Disable row resizing
	GetParam()->EnableTrackRowHeight(FALSE);

	// Determine required column and row counts
	long lColCount = vecColHeaders.size();
	long lRowCount = vecRowHeaders.size();

	// Create proper number of rows and columns
	SetRowCount( lRowCount );
	SetColCount( lColCount );

	// Hide the possibly unwanted first row
	if (!m_bShowColHeader)
	{
		HideRows( 0, 0, TRUE, NULL, GX_UPDATENOW, gxDo );
	}

	// Set width and bold for row headers
	if (m_bShowRowHeader)
	{
		// Make row headers editable
		ChangeRowHeaderStyle( CGXStyle()
								.SetControl( GX_IDS_CTRL_EDIT )
						);

		// Reduce width of row header by one pixel to avoid scroll bar
		SetColWidth( 0, 0, lRowHeaderWidth - 1, NULL, GX_UPDATENOW, gxDo );

		// Set row Header labels
		for (int j = 1; j <= lRowCount; j++)
		{
			SetStyleRange(CGXRange( j, 0 ),
							CGXStyle()
								.SetValue( (vecRowHeaders[j-1]).c_str() )
								 .SetFont( 
									CGXFont()
									   .SetBold(TRUE) )
							);
		}
	}
	// Hide the unwanted first column
	else
	{
		HideCols( 0, 0, TRUE, NULL, GX_UPDATENOW, gxDo );
	}

	// Set column widths and Header labels
	for (int i = 1; i <= lColCount; i++)
	{
		// Set column width
		// Zero width column will be adjusted by DoResize()
		SetColWidth( i, i, vecWidths[i-1], NULL, GX_UPDATENOW, gxDo );

		// Editable column is multiple-line edit box
		SetStyleRange(CGXRange().SetCols( i ),
						CGXStyle()
							.SetControl( GX_IDS_CTRL_EDIT )
							.SetAllowEnter( TRUE )
							.SetAutoSize( TRUE )
							.SetWrapText( TRUE )
							.SetVerticalAlignment( DT_VCENTER )
							.SetFont( 
								CGXFont()
									.SetBold(FALSE) )
						);

		// Show active cell:
		//	OnClick()
		//	OnSetCurrent()
		GetParam()->SetActivateCellFlags( GX_CAFOCUS_CLICKONCELL | GX_CAFOCUS_SETCURRENT );

		// Set Header label
		if (m_bShowColHeader)
		{
			SetStyleRange(CGXRange( 0, i ),
							CGXStyle()
								.SetValue( (vecColHeaders[i-1]).c_str() )
								.SetVerticalAlignment( DT_VCENTER )
							);
		}
	}

	// Set flag
	m_bIsPrepared = true;
}
//-------------------------------------------------------------------------------------------------
void CBasicGridWnd::SelectRow(int nRow)
{
	// Clear existing selection
	SetSelection( 0 );

	// Set selection on this row
	CGXRangeList* pList = GetParam()->GetRangeList();
	ASSERT_RESOURCE_ALLOCATION("ELI15632", pList != NULL);
	POSITION area = pList->AddTail( new CGXRange );
	SetSelection( area, nRow, 1, nRow, GetColCount() );
}
//-------------------------------------------------------------------------------------------------
int	CBasicGridWnd::SetRowInfo(int nRow, std::vector<std::string> vecStrings)
{
	int iNewItem = nRow;

	/////////////////////
	// Check column count
	/////////////////////
	int iNumStrings = vecStrings.size();
	int iColumnCount = GetColCount();

	if (iNumStrings > iColumnCount)
	{
		// Throw exception
		UCLIDException	ue( "ELI10617", "Too many strings for Grid control." );
		ue.addDebugInfo( "String count", iNumStrings );
		ue.addDebugInfo( "Column count", iColumnCount );
		throw ue;
	}

	//////////////////
	// Check row count
	//////////////////
	if (nRow < 1)
	{
		// Throw exception
		UCLIDException	ue( "ELI10618", "Invalid row number." );
		ue.addDebugInfo( "Row number", nRow );
		throw ue;
	}

	if ((unsigned int) nRow > GetRowCount())
	{
		// Extend grid size
		InsertRows( nRow, 1 );
	}

	/////////////////////////////
	// Add the strings to the row
	/////////////////////////////

	// Unlock the read-only cells
	GetParam()->SetLockReadOnly( FALSE );

	// Step through vector of strings
	int j = 0;
	string	strText;
	for (int i = 1; i <= iNumStrings; i++)
	{
		// Retrieve the string and increment counter
		strText = vecStrings.at( j++ );

		// Add this string and the appropriate Level setting
		SetStyleRange(CGXRange( nRow, i ),
						CGXStyle()
							.SetValue( strText.c_str() )
						);
	}

	/////////////////////////////////////////
	// Add blank text for any missing strings
	/////////////////////////////////////////
	for (int i = iNumStrings + 1; i <= iColumnCount; i++)
	{
		// Add blank text and the appropriate Normal setting
		SetStyleRange(CGXRange( nRow, i ),
						CGXStyle()
							.SetValue( "" )
						);
	}

	// Relock the read-only cells
	GetParam()->SetLockReadOnly( TRUE );

	return nRow;
}
//-------------------------------------------------------------------------------------------------
void CBasicGridWnd::SetControlID(int nID) 
{
	m_nControlID = nID;
}
//-------------------------------------------------------------------------------------------------
void CBasicGridWnd::SetModified(bool bIsModified)
{
	m_bIsModified = bIsModified;
}

//-------------------------------------------------------------------------------------------------
// CBasicGridWnd private methods
//-------------------------------------------------------------------------------------------------
BOOL CBasicGridWnd::isVerticalScrollVisible()
{
	BOOL bLastRowVisible; 
	CRect rect = GetGridRect();
	CalcBottomRowFromRect(rect, bLastRowVisible);

	// TRUE if scrollbar is needed... 
	BOOL bVertScrollbar = GetTopRow() > GetFrozenRows() + 1 || !bLastRowVisible;

	return bVertScrollbar;
}
//-------------------------------------------------------------------------------------------------
