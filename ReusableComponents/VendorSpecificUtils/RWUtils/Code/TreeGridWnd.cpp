// TreeGridWnd.cpp : implementation file
//

#include "stdafx.h"
#include "TreeGridWnd.h"
#include "resource.h"
#include "IconControl.h"
#include "Notifications.h"
#include <TemporaryResourceOverride.h>

#include <UCLIDException.h>

extern AFX_EXTENSION_MODULE RWUtilsDLL;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const pair<int, int> gprDEFAULT_PAIR = pair<int, int>(-1, -1);

//-------------------------------------------------------------------------------------------------
// CTreeGridWnd
//-------------------------------------------------------------------------------------------------
CTreeGridWnd::CTreeGridWnd()
{
	// Force that current cell is redrawn when user
	// navigates through the grid
	m_bRefreshOnSetCurrentCell = TRUE;

	// Default settings
	m_iValueColumn = -1;
	m_bIsPrepared = false;
	m_strIndentPrefix = "   ";
	m_iEditRow = -1;
	m_bEditStarted = false;
	m_lLeftNotifyCommandID = -1;
	m_lRightNotifyCommandID = -1;
	m_lSelectionNotifyCommandID = -1;
	m_lDblClkCellNotifyCommandID = -1;
	m_bSpecialBackground = false;
	m_nControlID = -1;
	m_prCellToNotHighlight = gprDEFAULT_PAIR;
}
//-------------------------------------------------------------------------------------------------
CTreeGridWnd::~CTreeGridWnd()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16476");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTreeGridWnd, CGXGridWnd)
	//{{AFX_MSG_MAP(CTreeGridWnd)
	ON_WM_MOUSEWHEEL()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CTreeGridWnd message handlers
//-------------------------------------------------------------------------------------------------
BOOL CTreeGridWnd::GetStyleRowCol(ROWCOL nRow, ROWCOL nCol, CGXStyle& style, GXModifyType mt, int nType)
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
				// and [p16 #2753] also ensure the cell is not a currently selected
				// drop down list cell
				if ((!m_bEditStarted || (m_iEditRow != nRow) || (m_iValueColumn != nCol))
					&& (pair<int, int>(nRow, nCol) != m_prCellToNotHighlight))
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12796");
	// if it gets here method failed
	return FALSE;
} 
//-------------------------------------------------------------------------------------------------
BOOL CTreeGridWnd::OnTrackColWidth(ROWCOL nCol)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12797");
	// if it gets here method failed
	return FALSE;
} 
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::DrawInvertCell(CDC* /*pDC*/, ROWCOL nRow, ROWCOL nCol, CRect rectItem)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12798");
} 
//-------------------------------------------------------------------------------------------------
BOOL CTreeGridWnd::OnStartEditing(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Set flag
		m_bEditStarted = true;
		
		// Store edit row
		m_iEditRow = nRow;
		
		return CGXGridCore::OnStartEditing(nRow, nCol);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12799");
	// if it gets here method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CTreeGridWnd::OnEndEditing(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Clear flag
		m_bEditStarted = false;
		
		// Clear edit row
		m_iEditRow = -1;
		
		return CGXGridCore::OnEndEditing(nRow, nCol);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12800");
	// if it gets here method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CTreeGridWnd::OnStartSelection(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
{
	try
	{
		// Check flags for Left Button vs. Right Button
		if (flags && MK_LBUTTON)
		{
			// Check to see if the Control ID is defined
			if (m_nControlID != -1)
			{
				// Post the message with 
				//   wParam = Control ID
				//   lParam: LOWORD nRow, HIWORD = nCol
				GetParent()->PostMessage( WM_NOTIFY_GRID_LCLICK, m_nControlID, 
					MAKELPARAM( nRow, nCol ) );
			}
			// Check to see if the Command ID is defined
			else if (m_lLeftNotifyCommandID != -1)
			{
				// Post the message with 
				//   wParam = nRow
				//   lParam = nCol
				GetParent()->PostMessage( m_lLeftNotifyCommandID, nRow, nCol );
			}
		}
		
		return TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12801");
	// if it gets here method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CTreeGridWnd::OnLButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
{
	try
	{
		// check if the selected cell is in a drop down list column
		if (m_setDropDownListColumns.find(nCol) != m_setDropDownListColumns.end())
		{
			// set the cell to not highlight since it is in a drop down list column
			m_prCellToNotHighlight = pair<int, int>(nRow, nCol);
		}
		else
		{
			// set the cell to the default since it is not in a drop down list column
			m_prCellToNotHighlight = gprDEFAULT_PAIR;
		}

		// Check to see if the Control ID is defined
		if (m_nControlID != -1)
		{
			// Post the message with 
			//   wParam = Control ID
			//   lParam: LOWORD nRow, HIWORD = nCol
			GetParent()->PostMessage( WM_NOTIFY_GRID_LCLICK, m_nControlID, 
				MAKELPARAM( nRow, nCol ) );
		}
		// Check to see if the Command ID is defined
		else if (m_lLeftNotifyCommandID != -1)
		{
			// Post the message with 
			//   wParam = nRow
			//   lParam = nCol
			GetParent()->PostMessage( m_lLeftNotifyCommandID, nRow, nCol );
		}
		
		return TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12802");
	// if it gets here method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CTreeGridWnd::OnRButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
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
		// Check to see if the Command ID is defined
		else if (m_lRightNotifyCommandID != -1)
		{
			// Post the message with 
			//   wParam = nRow
			//   lParam = nCol
			GetParent()->PostMessage( m_lRightNotifyCommandID, nRow, nCol );
		}
		
		return TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12803");
	// if it gets here method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::OnChangedSelection(const CGXRange* pRange, BOOL bIsDragging, BOOL bKey)
{
	try
	{
		// Check to see if the Control ID is defined
		if (m_nControlID != -1)
		{
			// Post the message with 
			//   wParam = Control ID
			//   lParam: pRange
			GetParent()->PostMessage( WM_NOTIFY_SELECTION_CHANGED, m_nControlID, 
				(LPARAM) pRange );
		}
		// Check to see if the Command ID is defined
		else if (m_lSelectionNotifyCommandID != -1)
		{
			// Post the message with 
			//   wParam = pRange
			//   lParam.loword = bIsDragging
			//	 lParam.hiword = bKey
			GetParent()->PostMessage( m_lSelectionNotifyCommandID, (WPARAM) pRange, 
				MAKELPARAM(bIsDragging, bKey));
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12805");
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::OnModifyCell(ROWCOL nRow, ROWCOL nCol)
{
	try
	{
		// Check to see if the Control ID is defined
		if (m_nControlID != -1)
		{
			// Post the message with 
			//   wParam = Control ID
			//   lParam: LOWORD nRow, HIWORD = nCol
			GetParent()->PostMessage( WM_NOTIFY_CELL_MODIFIED, m_nControlID, 
				MAKELPARAM( nRow, nCol ) );
		}
		// Check to see if the Command ID is defined
		else if (m_lSelectionNotifyCommandID != -1)
		{
			// Post the message with 
			//   wParam = nRow
			//   lParam = nCol
			GetParent()->PostMessage( m_lSelectionNotifyCommandID, nRow, nCol);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19855");
}
//-------------------------------------------------------------------------------------------------
BOOL CTreeGridWnd::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt) 
{
	try
	{
		// Use ProcessKeys to simulate Up/Down arrow
		CGXGridCore::ProcessKeys( this, WM_KEYDOWN, (zDelta > 0) ? VK_UP : VK_DOWN, 1, nFlags );
		
		return CGXGridWnd::OnMouseWheel(nFlags, zDelta, pt);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12806");
	// if it gets here method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CTreeGridWnd::OnLButtonDblClkRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
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
		// Check to see if the Command ID is defined
		else if (m_lDblClkCellNotifyCommandID != -1)
		{
			// Post the message with 
			//   wParam = nRow
			//   lParam = nCol
			GetParent()->PostMessage(m_lDblClkCellNotifyCommandID, nRow, nCol);
		}
		
		return TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12807");
	// if it gets here method failed
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL CTreeGridWnd::OnLButtonHitRowCol(ROWCOL nRow, ROWCOL nCol, ROWCOL nDragRow, ROWCOL nDragCol,
									  CPoint cpPoint, UINT uiFlags, WORD nHitState)
{
	try
	{
		// check if the selected cell is in a drop down list column
		if (m_setDropDownListColumns.find(nCol) != m_setDropDownListColumns.end())
		{
			// set the cell to not highlight since it is in a drop down list column
			m_prCellToNotHighlight = pair<int, int>(nRow, nCol);
		}
		else
		{
			// set the cell to the default since it is not in a drop down list column
			m_prCellToNotHighlight = gprDEFAULT_PAIR;
		}

		return CGXGridWnd::OnLButtonHitRowCol(nRow, nCol, nDragRow, nDragCol, cpPoint,
			uiFlags, nHitState);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20223");

	return TRUE;
}

//-------------------------------------------------------------------------------------------------
// CTreeGridWnd public methods
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::Initialize()
{
	CGXGridWnd::Initialize();

	// override the default combo box so that the drop down list behaves the
	// way we want it to, i.e. we want it to always display the drop down list button
	// [p16 #2825]
	CGXComboBox* pWnd = new CGXComboBox(this, GX_IDS_CTRL_CBS_DROPDOWN,
		GX_IDS_CTRL_CBS_DROPDOWN+1, GXCOMBO_TEXTFIT);
	pWnd->m_bInactiveDrawButton = TRUE;

	// register the new control (the Grid will automatically release the memory when the
	// control is destroyed)
	RegisterControl(GX_IDS_CTRL_CBS_DROPDOWNLIST, pWnd);
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::Clear()
{
	long lRowCount = GetRowCount();
	long lColCount = GetColCount();

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
void CTreeGridWnd::ClearSelection()
{
	SetSelection( 0 );
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::DoResize() 
{
	// Just return if PrepareGrid() has not been called
	if (!m_bIsPrepared)
	{
		return;
	}

	// Determine overall width and width of Value column
	CRect	rectGrid;
	GetClientRect( rectGrid );
	int iZeroColumn = -1;
	long lWidth = rectGrid.Width();
	int iCount = GetColCount();
	for (int i = 1; i <= iCount; i++)
	{
		// Ignore width of Value column
		if (m_iValueColumn != i)
		{
			// Decrement final width by width of existing columns
			long lThisWidth = GetColWidth( i );
			if (lThisWidth == 0)
			{
				iZeroColumn = i;
			}

			lWidth -= lThisWidth;
		}
	}

	// Value column takes precedence over a different zero-width column
	if (m_iValueColumn != -1)
	{
		iZeroColumn = m_iValueColumn;
	}

	// Set width of zero-width column - allow space for border to prevent scrollbar appearance
	if (iZeroColumn != -1)
	{
		SetColWidth( iZeroColumn, iZeroColumn, lWidth - 1, NULL, GX_UPDATENOW, gxDo );
	}

	// Adjust row heights as needed
	ResizeRowHeightsToFit( CGXRange( ).SetTable() );
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::PrepareGrid(std::vector<std::string> vecHeaders, std::vector<int> vecWidths) 
{
	TemporaryResourceOverride rcOverride(RWUtilsDLL.hResource);

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
			CGXStyle().SetValue(vecHeaders[i - 1].c_str()));
	}

	// Entire grid has vertically-centered items
	SetStyleRange(CGXRange().SetTable(),
					CGXStyle()
						.SetVerticalAlignment( DT_VCENTER )
					);

	// Set Value column to be editable, others are static
	for (int i = 1; i <= lCount; i++)
	{
		// Check for Value column
		if (i == m_iValueColumn)
		{
			// Value column is multiple-line edit box
			SetStyleRange(CGXRange().SetCols( i ),
							CGXStyle()
								.SetControl( GX_IDS_CTRL_EDIT )
								.SetAllowEnter( TRUE )
								.SetAutoSize( TRUE )
								.SetWrapText( TRUE )
							);
		}
		else
		{
			// Other columns are left-justified static text
			SetStyleRange(CGXRange().SetCols( i ),
							CGXStyle()
								.SetControl( GX_IDS_CTRL_STATIC )
							);
		}
	}

	// Set flag
	m_bIsPrepared = true;

	RegisterControl(GX_IDS_CTRL_ICON, new CGXIconControl(this));
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::PopulateGrid() 
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

		// Add the item with Level = 0
		SetRowInfo( j++, vecText, 0 );

		// Maybe add a duplicate item with Level = 1
		if (i % 3)
		{
			SetRowInfo( j++, vecText, 1 );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SelectRow(int nRow)
{
	// Clear existing selection
	SetSelection( 0 );

	// Set selection on this row
	CGXRangeList* pList = GetParam()->GetRangeList();
	ASSERT_RESOURCE_ALLOCATION("ELI15634", pList != NULL);
	POSITION area = pList->AddTail( new CGXRange );
	SetSelection( area, nRow, 1, nRow, GetColCount() );
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SetCellBoldBackground(int nRow, int nCol, bool bBold, bool bBack)
{
	if ((nRow < 0) || (nCol < 0))
	{
		return;
	}

	if (((unsigned int) nRow <= GetRowCount()) && ((unsigned int) nCol <= GetColCount()))
	{
		// Unlock the read-only cells
		GetParam()->SetLockReadOnly( FALSE );

		// Update this cell if it contains text
		CGXStyle	style;
		GetStyleRowCol( nRow, nCol, style, gxCopy, 0 );
		CString	zValue = style.GetValue();
		if (!zValue.IsEmpty())
		{
			if (m_bSpecialBackground)
			{
				// Set font to Bold or Normal
				// and background to Special color or White
				SetStyleRange(CGXRange( nRow, nCol ),
								CGXStyle()
									.SetFont( CGXFont().SetBold( bBold ? TRUE : FALSE ) )
									.SetInterior( CGXBrush().SetColor( bBack ? m_lBackgroundColor : 0xFFFFFF ) )
								);
			}
			else
			{
				// Set font to Bold or Normal
				SetStyleRange(CGXRange( nRow, nCol ),
								CGXStyle()
									.SetFont( CGXFont().SetBold( bBold ? TRUE : FALSE ) )
								);
			}
		}

		// Relock the read-only cells
		GetParam()->SetLockReadOnly( TRUE );
	}
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SetControlID(int nID) 
{
	m_nControlID = nID;
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SetLClickNotification(long lCommandID)
{
	// Store the Command ID
	m_lLeftNotifyCommandID = lCommandID;
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SetIndentPrefix(std::string strPrefix)
{
	// Store the string
	m_strIndentPrefix = strPrefix;
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SetRClickNotification(long lCommandID)
{
	// Store the Command ID
	m_lRightNotifyCommandID = lCommandID;
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SetSelectionChangedNotification(long lCommandID)
{
	// Store the Command ID
	m_lSelectionNotifyCommandID = lCommandID;
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SetDblClkNotification(long lCommandID)
{
	// Store the Command ID
	m_lDblClkCellNotifyCommandID = lCommandID;
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SetRowBoldBackground(int nRow, bool bBold, bool bBack)
{
	if ((unsigned int) nRow <= GetRowCount())
	{
		// Unlock the read-only cells
		GetParam()->SetLockReadOnly( FALSE );

		// Operate on each cell
		int iColumnCount = GetColCount();
		int i;
		for (i = 1; i <= iColumnCount; i++)
		{
			SetCellBoldBackground( nRow, i, bBold, bBack );
		}

		// Relock the read-only cells
		GetParam()->SetLockReadOnly( TRUE );
	}
}
//-------------------------------------------------------------------------------------------------
int CTreeGridWnd::SetRowInfo(int nRow, std::vector<std::string> vecStrings, long lIndentLevel)
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
		UCLIDException	ue( "ELI05331", "Too many strings for Grid control." );
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
		UCLIDException	ue( "ELI05332", "Invalid row number." );
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

		// Add the Indent Prefix(es) for first string
		if (i == 1)
		{
			for (int k = 0; k < lIndentLevel; k++)
			{
				// Insert another Indent Prefix
				strText = m_strIndentPrefix + strText;
			}
		}

		// Add this string and the appropriate Level setting
		SetStyleRange(CGXRange(nRow, i), 
			CGXStyle().SetValue(strText.c_str()).
			SetUserAttribute(IDS_LEVEL_ATTRIBUTE, lIndentLevel));
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
							.SetUserAttribute( IDS_LEVEL_ATTRIBUTE, lIndentLevel )
						);
	}

	// Relock the read-only cells
	GetParam()->SetLockReadOnly( TRUE );

	return nRow;
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SetSpecialBackgroundColor(long lHexColor)
{
	m_lBackgroundColor = lHexColor;
	m_bSpecialBackground = true;
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SetValueColumn(int nCol)
{
	// Check column count
//	int iColumnCount = GetColCount();
//	if (iColumnCount < nCol)
//	{
//		// Throw exception
//		UCLIDException	ue( "ELI05333", "Invalid column number." );
//		ue.addDebugInfo( "Column number", nCol );
//		ue.addDebugInfo( "Column count", iColumnCount );
//		throw ue;
//	}

	// Store the column and the string
	m_iValueColumn = nCol;
}
//-------------------------------------------------------------------------------------------------
// NOTE: function taken from RogueWave help file ogug.pdf
//		 page 119/636 under section 5.1.2 Getting Values
//		 from the Grid
std::string CTreeGridWnd::GetCellValue(ROWCOL nRow, ROWCOL nCol)
{
	CString zValue;
	if (IsCurrentCell(nRow, nCol) && IsActiveCurrentCell())
	{
		CGXControl* pControl = GetControl(nRow, nCol);
		ASSERT_ARGUMENT("ELI20271", pControl != NULL);

		pControl->GetValue(zValue);
	}
	else
	{
		zValue = GetValueRowCol(nRow, nCol);
	}

	return string(zValue);
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::SetDropDownListColumn(int nCol)
{
	m_setDropDownListColumns.insert(nCol);
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::RemoveDropDownListColumn(int nCol)
{
	m_setDropDownListColumns.erase(nCol);
}
//-------------------------------------------------------------------------------------------------
void CTreeGridWnd::UpdateCellToNotHighlight(int nRow, int nCol)
{
	m_prCellToNotHighlight = pair<int, int>(nRow, nCol);
}

//-------------------------------------------------------------------------------------------------
// CTreeGridWnd private methods
//-------------------------------------------------------------------------------------------------