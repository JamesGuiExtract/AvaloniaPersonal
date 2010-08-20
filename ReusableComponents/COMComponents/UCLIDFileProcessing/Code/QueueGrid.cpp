/////////////////////////////////////////////////////////////////////////////
//	Skeleton Class for a Derived CUGCtrl class

#include "stdafx.h"
#include "resource.h"
#include "QueueGrid.h"
#include "FilePriorityHelper.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

BEGIN_MESSAGE_MAP(CQueueGrid,CUGCtrl)
	//{{AFX_MSG_MAP(CQueueGrid)
		ON_WM_SIZE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

#define CHECK_UG_RETURN_VALUE(strELI, strCommand) \
	{ \
		int nUG_RETURN_VALUE = strCommand; \
		if (nUG_RETURN_VALUE != UG_SUCCESS) \
		{ \
			UCLIDException ue(strELI, "Grid error."); \
			ue.addDebugInfo("Command", #strCommand, true); \
			ue.addDebugInfo("Return value", nUG_RETURN_VALUE); \
			throw ue; \
		} \
	}

//--------------------------------------------------------------------------------------------------
// Statics
//--------------------------------------------------------------------------------------------------
string CQueueGrid::m_strPriorities;

//--------------------------------------------------------------------------------------------------
// Standard CQueueGrid construction/destruction
//--------------------------------------------------------------------------------------------------
CQueueGrid::CQueueGrid()
{
	try
	{
		vector<string> vecPriorities;
		getPrioritiesVector(vecPriorities);
		m_strPriorities = asString(vecPriorities, false, "\n");
		m_strPriorities = trim(m_strPriorities, "\n", "") + "\n";

		HFONT hFont = (HFONT)GetStockObject(ANSI_VAR_FONT);
		ASSERT_RESOURCE_ALLOCATION("ELI30499", hFont != NULL);

		LOGFONT logfont;
		if (GetObject(hFont, sizeof(logfont), &logfont) == 0)
		{
			UCLIDException ue("ELI30500", "Failed to retrieve font info.");
			ue.addWin32ErrorInfo();
			throw ue;
		}
		
		bool bFontCreated = asCppBool(m_font.CreateFontIndirect(&logfont));
		DeleteObject(&logfont);
	
		if (!bFontCreated)
		{
			throw UCLIDException("ELI30502", "Failed to create font.");
		}

		
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30434");
}
//--------------------------------------------------------------------------------------------------
CQueueGrid::~CQueueGrid()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI30498");
}
//--------------------------------------------------------------------------------------------------
//	OnSetup
//		This function is called just after the grid window 
//		is created or attached to a dialog item.
//		It can be used to initially setup the grid
void CQueueGrid::OnSetup()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CHECK_UG_RETURN_VALUE("ELI30444", SetHighlightRow(TRUE));
		CHECK_UG_RETURN_VALUE("ELI30445", SetSH_Width(0));
		CHECK_UG_RETURN_VALUE("ELI30446", SetNumberCols(5));

		CUGCell cell;
		CHECK_UG_RETURN_VALUE("ELI30447", cell.CopyInfoFrom(m_GI->m_hdgDefaults));
		CHECK_UG_RETURN_VALUE("ELI30449", cell.SetFont(&m_font));

		CHECK_UG_RETURN_VALUE("ELI30450", cell.SetText("Enabled"));
		CHECK_UG_RETURN_VALUE("ELI30451", SetColWidth(0, 70));
		CHECK_UG_RETURN_VALUE("ELI30452", SetCell(0, -1, &cell));

		CHECK_UG_RETURN_VALUE("ELI30453", cell.SetText("Force Processing"));
		CHECK_UG_RETURN_VALUE("ELI30454", SetColWidth(1, 120));
		CHECK_UG_RETURN_VALUE("ELI30455", SetCell(1, -1, &cell));

		CHECK_UG_RETURN_VALUE("ELI30456", cell.SetText("Priority"));
		CHECK_UG_RETURN_VALUE("ELI30457", SetColWidth(2, 100));
		CHECK_UG_RETURN_VALUE("ELI30458", SetCell(2, -1, &cell));

		CHECK_UG_RETURN_VALUE("ELI30459", cell.SetText("Description"));
		CHECK_UG_RETURN_VALUE("ELI30460", SetColWidth(3, 0));
		CHECK_UG_RETURN_VALUE("ELI30461", SetCell(3, -1, &cell));

		CHECK_UG_RETURN_VALUE("ELI30462", cell.SetText("Status"));
		CHECK_UG_RETURN_VALUE("ELI30463", SetColWidth(4, 80));
		CHECK_UG_RETURN_VALUE("ELI30464", SetCell(4, -1, &cell));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30435");
}
//--------------------------------------------------------------------------------------------------
//	OnSheetSetup	
//		This notification is called for each additional sheet that the grid
//		might contain, here you can customize each sheet in the grid.
//	Params:
//		sheetNumber - index of current sheet
//	Return:
//		<none>
void CQueueGrid::OnSheetSetup(int sheetNumber)
{
	UNREFERENCED_PARAMETER(sheetNumber);
}
//--------------------------------------------------------------------------------------------------
//	OnCanMove
//		is sent when a cell change action was instigated
//		( user clicked on another cell, used keyboard arrows,
//		or Goto[...] function was called ).
//	Params:
//		oldcol, oldrow - 
//		newcol, newrow - cell that is gaining focus
//	Return:
//		TRUE - to allow the move
//		FALSE - to prevent new cell from gaining focus
int CQueueGrid::OnCanMove(int oldcol,long oldrow,int newcol,long newrow)
{
	UNREFERENCED_PARAMETER(oldcol);
	UNREFERENCED_PARAMETER(oldrow);
	UNREFERENCED_PARAMETER(newcol);
	UNREFERENCED_PARAMETER(newrow);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnCanViewMove
//		is called when current grid's view is about to be scrolled.
//	Params:
//		oldcol, oldrow - coordinates of original top-left cell
//		newcol, newrow - coordinates of top-left cell that is gaining the focus
//	Return:
//		TRUE - to allow for the scroll
//		FALSE - to prevent the view from scrolling
int CQueueGrid::OnCanViewMove(int oldcol,long oldrow,int newcol,long newrow)
{
	UNREFERENCED_PARAMETER(oldcol);
	UNREFERENCED_PARAMETER(oldrow);
	UNREFERENCED_PARAMETER(newcol);
	UNREFERENCED_PARAMETER(newrow);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnHitBottom
//		This notification allows for dynamic row loading, it will be called
//		when the grid's drawing function has hit the last row.  It allows the grid
//		to ask the datasource/developer if there are additional rows to be displayed.
//	Params:
//		numrows		- known number of rows in the grid
//		rowspast	- number of extra rows that the grid is looking for in the datasource
//		rowsfound	- number of rows actually found, usually equal to rowspast or zero.
//	Return:
//		<none>
void CQueueGrid::OnHitBottom(long numrows,long rowspast,long rowsfound)
{
	UNREFERENCED_PARAMETER(numrows);
	UNREFERENCED_PARAMETER(rowspast);
	UNREFERENCED_PARAMETER(rowsfound);
	// used by the datasources
	/*if ( rowsfound > 0 )
	{
		SetNumberRows( numrows + rowsfound, FALSE );
	}*/
}
//--------------------------------------------------------------------------------------------------
//	OnHitTop
//		Is called when the user has scrolled all the way to the top of the grid.
//	Params:
//		numrows		- known number of rows in the grid
//		rowspast	- number of extra rows that the grid is looking for in the datasource
//	Return:
//		<none>
void CQueueGrid::OnHitTop(long numrows,long rowspast)
{
	UNREFERENCED_PARAMETER(numrows);
	UNREFERENCED_PARAMETER(rowspast);
}
//--------------------------------------------------------------------------------------------------
//	OnCanSizeCol
//		is sent when the mouse was moved in the area between
//		columns on the top heading, indicating that the user
//		might want to resize a column.
//	Params:
//		col - identifies column number that will be sized
//	Return:
//		TRUE - to allow sizing
//		FALSE - to prevent sizing
int CQueueGrid::OnCanSizeCol(int col)
{
	UNREFERENCED_PARAMETER(col);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnColSizing
//		is sent continuously when user is sizing a column.
//		This notification is ideal to provide min/max width checks.
//	Params:
//		col - column currently sizing
//		width - current new column width
//	Return:
//		<none>
void CQueueGrid::OnColSizing(int col,int *width)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(*width);
}
//--------------------------------------------------------------------------------------------------
//	OnColSized
//		This is sent when the user finished sizing the 
//		given column.(see above for more information)
//	Params:
//		col - column sized
//		width - new column width
//	Return:
//		<none>
void CQueueGrid::OnColSized(int col,int *width)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(*width);
}
//--------------------------------------------------------------------------------------------------
//	OnCanSizeRow
//		is sent when the mouse was moved in the area between
//		rows on the side heading, indicating that the user
//		might want to resize a row.
//	Params:
//		row - identifies row number that will be sized
//	Return:
//		TRUE - to allow sizing
//		FALSE - to prevent sizing
int CQueueGrid::OnCanSizeRow(long row)
{
	UNREFERENCED_PARAMETER(row);

	// [LegacyRCAndUtils:5871]
	return FALSE;
}
//--------------------------------------------------------------------------------------------------
//	OnRowSizing
//		Sent during user sizing of a row, can provide
//		feed back on current height
//	Params:
//		row - row sizing
//		height - pointer to current new row height
//	Return:
//		<none>
void CQueueGrid::OnRowSizing(long row,int *height)
{
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(*height);
}
//--------------------------------------------------------------------------------------------------
//	OnRowSized
//		This is sent when the user is finished sizing the
//		given row.
//	Params:
//		row - row sized
//		height - pointer to current new row height
//	Return:
//		<none>
void CQueueGrid::OnRowSized(long row,int *height)
{
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(*height);
}
//--------------------------------------------------------------------------------------------------
//	OnCanSizeSideHdg
//		Sent when the user is about to start sizing of the side heading
//	Params:
//		<none>
//	Return:
//		TRUE - to allow sizing
//		FALSE - to prevent sizing
int CQueueGrid::OnCanSizeSideHdg()
{
	return FALSE;
}
//--------------------------------------------------------------------------------------------------
//	OnSideHdgSizing
//		Sent while the heading is being sized
//	Params:
//		width - pointer to current new width of the side heading
//	Return:
//		TRUE - to accept current new size
//		FALSE - to stop sizing, the size is either too large or too small
int CQueueGrid::OnSideHdgSizing(int *width)
{
	UNREFERENCED_PARAMETER(*width);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnSideHdgSized
//		Sent when the user has completed the sizing of the side heading
//	Params:
//		width - pointer to new width
//	Return:
//		TRUE - to accept new size
//		FALSE - to revert to old size
int CQueueGrid::OnSideHdgSized(int *width)
{
	UNREFERENCED_PARAMETER(*width);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnCanSizeTopHdg
//		Sent when the user is about to start sizing of the top heading
//	Params:
//		<none>
//	Return:
//		TRUE - to allow sizing
//		FALSE - to prevent sizing
int CQueueGrid::OnCanSizeTopHdg()
{
	// [LegacyRCAndUtils:5871]
	return FALSE;
}
//--------------------------------------------------------------------------------------------------
//	OnTopHdgSizing
//		Sent while the top heading is being sized
//	Params:
//		height - pointer to current new height of the top heading
//	Return:
//		TRUE - to accept current new size
//		FALSE - to stop sizing, the size is either too large or too small
int CQueueGrid::OnTopHdgSizing(int *height)
{
	UNREFERENCED_PARAMETER(*height);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnTopHdgSized
//		Sent when the user has completed the sizing of the top heading
//	Params:
//		height - pointer to new height
//	Return:
//		TRUE - to accept new size
//		FALSE - to revert to old size
int CQueueGrid::OnTopHdgSized(int *height)
{
	UNREFERENCED_PARAMETER(*height);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnColChange
//		Sent whenever the current column changes
//	Params:
//		oldcol - column that is loosing the focus
//		newcol - column that the user move into
//	Return:
//		<none>
void CQueueGrid::OnColChange(int oldcol,int newcol)
{
	UNREFERENCED_PARAMETER(oldcol);
	UNREFERENCED_PARAMETER(newcol);
}
//--------------------------------------------------------------------------------------------------
//	OnRowChange
//		Sent whenever the current row changes
//	Params:
//		oldrow - row that is loosing the locus
//		newrow - row that user moved into
//	Return:
//		<none>
void CQueueGrid::OnRowChange(long oldrow,long newrow)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UNREFERENCED_PARAMETER(oldrow);

		m_currentRow = newrow;
		
		if (newrow >= 0 && newrow < GetNumberRows())
		{
			// Sets the "current" row so that the current row always matches the selected row. If current
			// and selected row are not in sync, OnRowChangeevents may not be received and the clicked
			// row may not be highlighted.
			CHECK_UG_RETURN_VALUE("ELI30516", GotoRow(newrow));
			CHECK_UG_RETURN_VALUE("ELI30467", ClearSelections());

			CHECK_UG_RETURN_VALUE("ELI30468", Select(-1, newrow));
		}
		else
		{
			CHECK_UG_RETURN_VALUE("ELI30518", HideCurrentCell());
		}

		Invalidate();

		GetParent()->PostMessage(WM_QUEUE_GRID_SELCHANGE, ::GetDlgCtrlID(m_hWnd), (LPARAM)newrow);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30466");
}
//--------------------------------------------------------------------------------------------------
//	OnCellChange
//		Sent whenever the current cell changes
//	Params:
//		oldcol, oldrow - coordinates of cell that is loosing the focus
//		newcol, newrow - coordinates of cell that is gaining the focus
//	Return:
//		<none>
void CQueueGrid::OnCellChange(int oldcol,int newcol,long oldrow,long newrow)
{
	UNREFERENCED_PARAMETER(oldcol);
	UNREFERENCED_PARAMETER(newcol);
	UNREFERENCED_PARAMETER(oldrow);
	UNREFERENCED_PARAMETER(newrow);
}
//--------------------------------------------------------------------------------------------------
//	OnLeftColChange
//		Sent whenever the left visible column in the grid changes, indicating
//		that the view was scrolled horizontally
//	Params:
//		oldcol - column that used to be on the left
//		newcol - new column that is going to be the first visible column from the left
//	Return:
//		<none>
void CQueueGrid::OnLeftColChange(int oldcol,int newcol)
{
	UNREFERENCED_PARAMETER(oldcol);
	UNREFERENCED_PARAMETER(newcol);
}
//--------------------------------------------------------------------------------------------------
//	OnTopRowChange
//		Sent whenever the top visible row in the grid changes, indicating
//		that the view was scrolled vertically
//	Params:
//		oldrow - row that used to be on the top
//		newrow - new row that is going to be the first visible row from the top
//	Return:
//		<none>
void CQueueGrid::OnTopRowChange(long oldrow,long newrow)
{
	UNREFERENCED_PARAMETER(oldrow);
	UNREFERENCED_PARAMETER(newrow);
}
//--------------------------------------------------------------------------------------------------
//	OnViewMoved
//		This notification is fired after the current viewing area
//		was scrolled.
//	Params:
//		nScrolDir - UG_VSCROLL, UG_HSCROLL
//
//		if the nScrolDir == UG_VSCROLL
//			oldPos - row that used to be on the top
//			newPos - row that is now the first visible row from the top
//
//		if the nScrolDir == UG_VSCROLL
//			oldPos - column that used to be on the left
//			newPos - column that is now the first visible column from the left
//	Return:
//		<none>
void CQueueGrid::OnViewMoved( int nScrolDir, long oldPos, long newPos )
{
	UNREFERENCED_PARAMETER(nScrolDir);
	UNREFERENCED_PARAMETER(oldPos);
	UNREFERENCED_PARAMETER(newPos);
}
//--------------------------------------------------------------------------------------------------
//	OnSelectionChanged
//		The OnSelectionChanged notification is called by the multiselect
//		class when a change occurred in current selection (i.e. user clicks
//		on a new cell, drags a mouse selecting range of cells, or uses
//		CTRL/SHIFT - left click key combination to select cells.)
//	Params:
//		startCol, startRow	- coordinates of the cell starting the selection block
//		endCol, endRow		- coordinates of the cell ending the selection block
//		blockNum			- block number representing this range, this will
//							always represent total number of selection blocks.
//	Return:
//		<none>
void CQueueGrid::OnSelectionChanged(int startCol,long startRow,int endCol,long endRow,int blockNum)
{
	UNREFERENCED_PARAMETER(startCol);
	UNREFERENCED_PARAMETER(endCol);
	UNREFERENCED_PARAMETER(endRow);
	UNREFERENCED_PARAMETER(blockNum);

	// Ensure the current row is always selected.
	if (startRow != m_currentRow)
	{
		OnRowChange(m_currentRow, startRow);
	}
}
//--------------------------------------------------------------------------------------------------
//	OnLClicked
//		Sent whenever the user clicks the left mouse button within the grid
//		this message is sent when the button goes down then again when the button goes up
//	Params:
//		col, row	- coordinates of a cell that received mouse click event
//		updn		- is TRUE if mouse button is 'down' and FALSE if button is 'up'
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnLClicked(int col,long row,int updn,RECT *rect,POINT *point,int processed)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(updn);
	UNREFERENCED_PARAMETER(*rect);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(processed);
}	
//--------------------------------------------------------------------------------------------------
//	OnRClicked
//		Sent whenever the user clicks the right mouse button within the grid
//		this message is sent when the button goes down then again when the button goes up
//	Params:
//		col, row	- coordinates of a cell that received mouse click event
//		updn		- is TRUE if mouse button is 'down' and FALSE if button is 'up'
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnRClicked(int col,long row,int updn,RECT *rect,POINT *point,int processed)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(updn);
	UNREFERENCED_PARAMETER(*rect);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(processed);
}
//--------------------------------------------------------------------------------------------------
//	OnDClicked
//		Sent whenever the user double clicks the left mouse button within the grid
//	Params:
//		col, row	- coordinates of a cell that received mouse click event
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnDClicked(int col,long row,RECT *rect,POINT *point,BOOL processed)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UNREFERENCED_PARAMETER(*rect);
		UNREFERENCED_PARAMETER(*point);
		UNREFERENCED_PARAMETER(processed);

		GetParent()->PostMessage(WM_QUEUE_GRID_DBLCLICK, ::GetDlgCtrlID(m_hWnd),
			MAKELPARAM(row, col));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30436");
}	
//--------------------------------------------------------------------------------------------------
//	OnMouseMove
//		is sent when the user moves mouse over the grid area.
//	Params:
//		col, row	- coordinates of a cell that received mouse click event
//		point		- represents the screen point where the mouse event was detected
//		nFlags		- 
//		processed	- indicates if current event was processed by other control in the grid.
//	Return:
//		<none>
void CQueueGrid::OnMouseMove(int col,long row,POINT *point,UINT nFlags,BOOL processed)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(nFlags);
	UNREFERENCED_PARAMETER(processed);
}
//--------------------------------------------------------------------------------------------------
//	OnTH_LClicked
//		Sent whenever the user clicks the left mouse button within the top heading
//		this message is sent when the button goes down then again when the button goes up
//	Params:
//		col, row	- coordinates of a cell that received mouse click event
//		updn		- is TRUE if mouse button is 'down' and FALSE if button is 'up'
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnTH_LClicked(int col,long row,int updn,RECT *rect,POINT *point,BOOL processed)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(updn);
	UNREFERENCED_PARAMETER(*rect);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(processed);
}
//--------------------------------------------------------------------------------------------------
//	OnTH_RClicked
//		Sent whenever the user clicks the right mouse button within the top heading
//		this message is sent when the button goes down then again when the button goes up
//	Params:
//		col, row	- coordinates of a cell that received mouse click event
//		updn		- is TRUE if mouse button is 'down' and FALSE if button is 'up'
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnTH_RClicked(int col,long row,int updn,RECT *rect,POINT *point,BOOL processed)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(updn);
	UNREFERENCED_PARAMETER(*rect);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(processed);
}
//--------------------------------------------------------------------------------------------------
//	OnTH_DClicked
//		Sent whenever the user double clicks the left mouse
//		button within the top heading
//	Params:
//		col, row	- coordinates of a cell that received mouse click event
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnTH_DClicked(int col,long row,RECT *rect,POINT *point,BOOL processed)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(*rect);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(processed);
}
//--------------------------------------------------------------------------------------------------
//	OnSH_LClicked
//		Sent whenever the user clicks the left mouse button within the side heading
//		this message is sent when the button goes down then again when the button goes up
//	Params:
//		col, row	- coordinates of a cell that received mouse click event
//		updn		- is TRUE if mouse button is 'down' and FALSE if button is 'up'
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnSH_LClicked(int col,long row,int updn,RECT *rect,POINT *point,BOOL processed)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(updn);
	UNREFERENCED_PARAMETER(*rect);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(processed);
}
//--------------------------------------------------------------------------------------------------
//	OnSH_RClicked
//		Sent whenever the user clicks the right mouse button within the side heading
//		this message is sent when the button goes down then again when the button goes up
//	Params:
//		col, row	- coordinates of a cell that received mouse click event
//		updn		- is TRUE if mouse button is 'down' and FALSE if button is 'up'
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnSH_RClicked(int col,long row,int updn,RECT *rect,POINT *point,BOOL processed)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(updn);
	UNREFERENCED_PARAMETER(*rect);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(processed);
}
//--------------------------------------------------------------------------------------------------
//	OnSH_DClicked
//		Sent whenever the user double clicks the left mouse button within the side heading
//	Params:
//		col, row	- coordinates of a cell that received mouse click event
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnSH_DClicked(int col,long row,RECT *rect,POINT *point,BOOL processed)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(*rect);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(processed);
}
//--------------------------------------------------------------------------------------------------
//	OnCB_LClicked
//		Sent whenever the user clicks the left mouse button within the top corner button
//		this message is sent when the button goes down then again when the button goes up
//	Params:
//		updn		- is TRUE if mouse button is 'down' and FALSE if button is 'up'
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnCB_LClicked(int updn,RECT *rect,POINT *point,BOOL processed)
{
	UNREFERENCED_PARAMETER(updn);
	UNREFERENCED_PARAMETER(*rect);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(processed);
}
//--------------------------------------------------------------------------------------------------
//	OnCB_RClicked
//		Sent whenever the user clicks the right mouse button within the top corner button
//		this message is sent when the button goes down then again when the button goes up
//	Params:
//		updn		- is TRUE if mouse button is 'down' and FALSE if button is 'up'
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnCB_RClicked(int updn,RECT *rect,POINT *point,BOOL processed)
{
	UNREFERENCED_PARAMETER(updn);
	UNREFERENCED_PARAMETER(*rect);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(processed);
}
//--------------------------------------------------------------------------------------------------
//	OnCB_DClicked
//		Sent whenever the user double clicks the left mouse
//		button within the top corner button
//	Params:
//		processed	- indicates if current event was processed by other control in the grid.
//		rect		- represents the CDC rectangle of cell in question
//		point		- represents the screen point where the mouse event was detected
//	Return:
//		<none>
void CQueueGrid::OnCB_DClicked(RECT *rect,POINT *point,BOOL processed)
{
	UNREFERENCED_PARAMETER(*rect);
	UNREFERENCED_PARAMETER(*point);
	UNREFERENCED_PARAMETER(processed);
}
//--------------------------------------------------------------------------------------------------
//	OnKeyDown
//		Sent when grid received a WM_KEYDOWN message, usually as a result
//		of a user pressing any key on the keyboard.
//	Params:
//		vcKey		- virtual key code of the key user has pressed
//		processed	- indicates if current event was processed by other control in the grid.
//	Return:
//		<none>
void CQueueGrid::OnKeyDown(UINT *vcKey,BOOL processed)
{
	UNREFERENCED_PARAMETER(*vcKey);
	UNREFERENCED_PARAMETER(processed);
//	GetScrollBarCtrl(SB_HORZ)->EnableScrollBar(ESB_DISABLE_BOTH);
}
//--------------------------------------------------------------------------------------------------
//	OnKeyUp
//		Sent when grid received a WM_KEYUP message, usually as a result
//		of a user releasing a key.
//	Params:
//		vcKey		- virtual key code of the key user has pressed
//		processed	- indicates if current event was processed by other control in the grid.
//	Return:
//		<none>
void CQueueGrid::OnKeyUp(UINT *vcKey,BOOL processed)
{
	UNREFERENCED_PARAMETER(*vcKey);
	UNREFERENCED_PARAMETER(processed);
//	GetScrollBarCtrl(SB_HORZ)->EnableScrollBar(ESB_DISABLE_BOTH);
}
//--------------------------------------------------------------------------------------------------
//	OnCharDown
//		Sent when grid received a WM_CHAR message, usually as a result
//		of a user pressing any key that represents a printable character.
//	Params:
//		vcKey		- virtual key code of the key user has pressed
//		processed	- indicates if current event was processed by other control in the grid.
//	Return:
//		<none>
void CQueueGrid::OnCharDown(UINT *vcKey,BOOL processed)
{
	UNREFERENCED_PARAMETER(*vcKey);
	UNREFERENCED_PARAMETER(processed);
//	GetScrollBarCtrl(SB_HORZ)->EnableScrollBar(ESB_DISABLE_BOTH);
}
//--------------------------------------------------------------------------------------------------
//	OnGetCell
//		This message is sent every time the grid needs to
//		draw a cell in the grid. At this point the cell
//		object has been populated with all of the information
//		that will be used to draw the cell. This information
//		can now be changed before it is used for drawing.
//	Warning:
//		This notification is called for each cell that needs to be painted
//		Placing complicated calculations here will slowdown the refresh speed.
//	Params:
//		col, row	- coordinates of cell currently drawing
//		cell		- pointer to the cell object that is being drawn
//	Return:
//		<none>
void CQueueGrid::OnGetCell(int col,long row,CUGCell *cell)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(*cell);
}
//--------------------------------------------------------------------------------------------------
//	OnSetCell
//		This notification is sent every time a cell is set,
//		here you have a chance to make final modification
//		to the cell's content before it is saved to the data source.
//	Params:
//		col, row	- coordinates of cell currently saving
//		cell		- pointer to the cell object that is being saved
//	Return:
//		<none>
void CQueueGrid::OnSetCell(int col,long row,CUGCell *cell)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UNREFERENCED_PARAMETER(*cell);

		if (m_enableUpdate)
		{
			GetParent()->PostMessage(WM_QUEUE_GRID_CELL_VALUE_CHANGE, ::GetDlgCtrlID(m_hWnd),
				MAKELPARAM(row, col));
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30437");
}
//--------------------------------------------------------------------------------------------------
//	OnDataSourceNotify
//		This message is sent from a data source and this message
//		depends on the data source - check the information
//		on the data source(s) being used
//		- The ID of the Data source is also returned
//	Params:
//		ID		- datasource ID
//		msg		- message ID to identify current process
//		param	- additional information or object that might be needed
//	Return:
//		<none>
void CQueueGrid::OnDataSourceNotify(int ID,long msg,long param)
{
	UNREFERENCED_PARAMETER(ID);
	UNREFERENCED_PARAMETER(msg);
	UNREFERENCED_PARAMETER(param);
}
//--------------------------------------------------------------------------------------------------
//	OnCellTypeNotify
//		This notification is sent by the celltype and it is different from cell-type
//		to celltype and even from notification to notification.  It is usually used to
//		provide the developer with some feed back on the cell events and sometimes to
//		ask the developer if given event is to be accepted or not
//	Params:
//		ID			- celltype ID
//		col, row	- co-ordinates cell that is processing the message
//		msg			- message ID to identify current process
//		param		- additional information or object that might be needed
//	Return:
//		TRUE - to allow celltype event
//		FALSE - to disallow the celltype event
int CQueueGrid::OnCellTypeNotify(long ID,int col,long row,long msg,long param)
{
	UNREFERENCED_PARAMETER(ID);
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(msg);
	UNREFERENCED_PARAMETER(param);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnEditStart
//		This message is sent whenever the grid is ready to start editing a cell
//	Params:
//		col, row - location of the cell that edit was requested over
//		edit -	pointer to a pointer to the edit control, allows for swap of edit control
//				if edit control is swapped permanently (for the whole grid) is it better
//				to use 'SetNewEditClass' function.
//	Return:
//		TRUE - to allow the edit to start
//		FALSE - to prevent the edit from starting
int CQueueGrid::OnEditStart(int col, long row,CWnd **edit)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(**edit);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnEditVerify
//		This notification is sent every time the user hits a key while in edit mode.
//		It is mostly used to create custom behavior of the edit control, because it is
//		so easy to allow or disallow keys hit.
//	Params:
//		col, row	- location of the edit cell
//		edit		-	pointer to the edit control
//		vcKey		- virtual key code of the pressed key
//	Return:
//		TRUE - to accept pressed key
//		FALSE - to do not accept the key
int CQueueGrid::OnEditVerify(int col, long row,CWnd *edit,UINT *vcKey)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(*edit);
	UNREFERENCED_PARAMETER(*vcKey);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnEditFinish
//		This notification is sent when the edit is being finished
//	Params:
//		col, row	- coordinates of the edit cell
//		edit		- pointer to the edit control
//		string		- actual string that user typed in
//		cancelFlag	- indicates if the edit is being canceled
//	Return:
//		TRUE - to allow the edit to proceed
//		FALSE - to force the user back to editing of that same cell
int CQueueGrid::OnEditFinish(int col, long row,CWnd *edit,LPCTSTR string,BOOL cancelFlag)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(*edit);
	UNREFERENCED_PARAMETER(string);
	UNREFERENCED_PARAMETER(cancelFlag);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnEditContinue
//		This notification is called when the user pressed 'tab' or 'enter' keys
//		Here you have a chance to modify the destination cell
//	Params:
//		oldcol, oldrow - edit cell that is loosing edit focus
//		newcol, newrow - cell that the edit is going into, by changing their
//							values you are able to change where to edit next
//	Return:
//		TRUE - allow the edit to continue
//		FALSE - to prevent the move, the edit will be stopped
int CQueueGrid::OnEditContinue(int oldcol,long oldrow,int* newcol,long* newrow)
{
	UNREFERENCED_PARAMETER(oldcol);
	UNREFERENCED_PARAMETER(oldrow);
	UNREFERENCED_PARAMETER(*newcol);
	UNREFERENCED_PARAMETER(*newrow);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnMenuCommand
//		This notification is called when the user has selected a menu item
//		in the pop-up menu.
//	Params:
//		col, row - the cell coordinates of where the menu originated from
//		section - identify for which portion of the gird the menu is for.
//				  possible sections:
//						UG_TOPHEADING, UG_SIDEHEADING,UG_GRID
//						UG_HSCROLL  UG_VSCROLL  UG_CORNERBUTTON
//		item - ID of the menu item selected
//	Return:
//		<none>
void CQueueGrid::OnMenuCommand(int col,long row,int section,int item)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(section);
	UNREFERENCED_PARAMETER(item);
}
//--------------------------------------------------------------------------------------------------
//	OnMenuStart
//		Is called when the pop up menu is about to be shown
//	Params:
//		col, row	- the cell coordinates of where the menu originated from
//		setcion		- identify for which portion of the gird the menu is for.
//					possible sections:
//						UG_TOPHEADING, UG_SIDEHEADING,UG_GRID
//						UG_HSCROLL  UG_VSCROLL  UG_CORNERBUTTON
//	Return:
//		TRUE - to allow menu to show
//		FALSE - to prevent the menu from poping up
int CQueueGrid::OnMenuStart(int col,long row,int section)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(section);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnGetContextHelpID 
//		this notification is called as result of context help bing activated
//		over the UG area and should return context help ID to be displayed
//	Params:
//		col, row	- coordinates of cell which received the message
//		section		- grid section which received this message
//	Return:
//		Context help ID to be shown in the help.
DWORD CQueueGrid::OnGetContextHelpID( int col, long row, int section )
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(section);
	return 0;
}
//--------------------------------------------------------------------------------------------------
//	OnHint
//		Is called whent the hint is about to be displayed on the main grid area,
//		here you have a chance to set the text that will be shown
//	Params:
//		col, row	- the cell coordinates of where the menu originated from
//		string		- pointer to the string that will be shown
//	Return:
//		TRUE - to show the hint
//		FALSE - to prevent the hint from showing
int CQueueGrid::OnHint(int col,long row,int section,CString *string)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(section);
	UNREFERENCED_PARAMETER(*string);
	
	return FALSE;
}
//--------------------------------------------------------------------------------------------------
//	OnVScrollHint
//		Is called when the hint is about to be displayed on the vertical scroll bar,
//		here you have a chance to set the text that will be shown
//	Params:
//		row		- current top row
//		string	- pointer to the string that will be shown
//	Return:
//		TRUE - to show the hint
//		FALSE - to prevent the hint from showing
int CQueueGrid::OnVScrollHint(long row,CString *string)
{
	UNREFERENCED_PARAMETER(row);
	UNREFERENCED_PARAMETER(*string);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnHScrollHint
//		Is called whent the hint is about to be displayed on the horizontal scroll bar,
//		here you have a chance to set the text that will be shown
//	Params:
//		col		- current left col
//		string	- pointer to the string that will be shown
//	Return:
//		TRUE - to show the hint
//		FALSE - to prevent the hint from showing
int CQueueGrid::OnHScrollHint(int col,CString *string)
{
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(*string);
	return TRUE;
}

#ifdef __AFXOLE_H__
//--------------------------------------------------------------------------------------------------
//	OLE
//		following 3 functions are to be used with drag and drop functionality
//--------------------------------------------------------------------------------------------------
//	OnDragEnter
//	Params:
//		pDataObject - 
//	Return:
//		DROPEFFECT_NONE - no drag and drop
//		DROPEFFECT_COPY - allow drag and drop for copying
DROPEFFECT CQueueGrid::OnDragEnter(COleDataObject* pDataObject)
{
	UNREFERENCED_PARAMETER(*pDataObject);
	return DROPEFFECT_NONE;
}
//--------------------------------------------------------------------------------------------------
//	OnDragOver
//	Params:
//		col, row	-
//		pDataObject - 
//	Return:
//		DROPEFFECT_NONE - no drag and drop
//		DROPEFFECT_COPY - allow drag and drop for copying
DROPEFFECT CQueueGrid::OnDragOver(COleDataObject* pDataObject,int col,long row)
{
	UNREFERENCED_PARAMETER(*pDataObject);
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	return DROPEFFECT_NONE;
}
//--------------------------------------------------------------------------------------------------
//	OnDragDrop
//	Params:
//		col, row	-
//		pDataObject - 
//	Return:
//		DROPEFFECT_NONE - no drag and drop
//		DROPEFFECT_COPY - allow drag and drop for copying
DROPEFFECT CQueueGrid::OnDragDrop(COleDataObject* pDataObject,int col,long row)
{
	UNREFERENCED_PARAMETER(*pDataObject);
	UNREFERENCED_PARAMETER(col);
	UNREFERENCED_PARAMETER(row);
	return DROPEFFECT_NONE;
}
#endif
//--------------------------------------------------------------------------------------------------
//	OnScreenDCSetup
//		Is called when each of the components are painted.
//	Params:
//		dc		- pointer to the current CDC object
//		section	- section of the grid painted.
//					possible sections:
//						UG_TOPHEADING, UG_SIDEHEADING,UG_GRID
//						UG_HSCROLL  UG_VSCROLL  UG_CORNERBUTTON
//	Return:
//		<none>
void CQueueGrid::OnScreenDCSetup(CDC *dc,int section)
{
	UNREFERENCED_PARAMETER(*dc);
	UNREFERENCED_PARAMETER(section);
}
//--------------------------------------------------------------------------------------------------
//	OnSortEvaluate
//		Sent as a result of the 'SortBy' call, this is called for each cell
//		comparison and allows for customization of the sorting routines.
//		We provide following code as example of what could be done here,
//		you might have to modify it to give your application customized sorting.
//	Params:
//		cell1, cell2	- pointers to cells that are compared
//		flags			- identifies sort direction
//	Return:
//		value less than zero to identify that the cell1 comes before cell2
//		value equal to zero to identify that the cell1 and cell2 are equal
//		value greater than zero to identify that the cell1 comes after cell2
int CQueueGrid::OnSortEvaluate(CUGCell *cell1,CUGCell *cell2,int flags)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// if one of the cells is NULL, do not compare its text
		if ( cell1 == NULL && cell2 == NULL )
			return 0;
		else if ( cell1 == NULL )
			return 1;
		else if ( cell2 == NULL )
			return -1;

		if(flags&UG_SORT_DESCENDING)
		{
			CUGCell *ptr = cell1;
			cell1 = cell2;
			cell2 = ptr;
		}

		// initialize variables for numeric check
		double num1, num2;
		// compare the cells, with respect to the cell's datatype
		switch(cell1->GetDataType())
		{
			case UGCELLDATA_STRING:
				if(NULL == cell1->GetText() && NULL == cell2->GetText())
					return 0;
				if(NULL == cell1->GetText())
					return 1;
				if(NULL == cell2->GetText())
					return -1;
				return _tcscmp(cell1->GetText(),cell2->GetText());	
			case UGCELLDATA_NUMBER:
			case UGCELLDATA_BOOL:
			case UGCELLDATA_CURRENCY:
				num1 = cell1->GetNumber();
				num2 = cell2->GetNumber();
				if(num1 < num2)
					return -1;
				if(num1 > num2)
					return 1;
				return 0;
			default:
				// if datatype is not recognized, compare cell's text
				if(NULL == cell1->GetText())
					return 1;
				if(NULL == cell2->GetText())
					return -1;
				return _tcscmp(cell1->GetText(),cell2->GetText());	
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30439");

	return 0;
}
//--------------------------------------------------------------------------------------------------
//	OnTabSelected
//		Called when the user selects one of the tabs on the bottom of the screen
//	Params:
//		ID	- id of selected tab
//	Return:
//		<none>
void CQueueGrid::OnTabSelected(int ID)
{
	UNREFERENCED_PARAMETER(ID);
}
//--------------------------------------------------------------------------------------------------
//	OnAdjustComponentSizes
//		Called when the grid components are being created,
//		sized, and arranged.  This notification provides
//		us with ability to further customize the way
//		the grid will be presented to the user.
//	Params:
//		grid, topHdg, sideHdg, cnrBtn, vScroll, hScroll, 
//		tabs	- sizes and location of each of the grid components
//	Return:
//		<none>
void CQueueGrid::OnAdjustComponentSizes(RECT *grid,RECT *topHdg,RECT *sideHdg,
		RECT *cnrBtn,RECT *vScroll,RECT *hScroll,RECT *tabs)
{
	UNREFERENCED_PARAMETER(*grid);
	UNREFERENCED_PARAMETER(*topHdg);
	UNREFERENCED_PARAMETER(*sideHdg);
	UNREFERENCED_PARAMETER(*cnrBtn);
	UNREFERENCED_PARAMETER(*vScroll);
	UNREFERENCED_PARAMETER(*hScroll);
	UNREFERENCED_PARAMETER(*tabs);
} 
//--------------------------------------------------------------------------------------------------
//	OnDrawFocusRect
//		Called when the focus rect needs to be painted.
//	Params:
//		dc		- pointer to the current CDC object
//		rect	- rect of the cell that requires the focus rect
//	Return:
//		<none>
void CQueueGrid::OnDrawFocusRect(CDC *dc,RECT *rect)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		//DrawExcelFocusRect(dc,rect);

		rect->bottom --;
		rect->right --;	
		dc->DrawFocusRect(rect);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30440");
}
//--------------------------------------------------------------------------------------------------
//	OnGetDefBackColor
//		Sent when the area behind the grid needs to be painted.
//	Params:
//		section - Id of the grid section that requested this color
//				  possible sections:
//						UG_TOPHEADING, UG_SIDEHEADING, UG_GRID
//	Return:
//		RGB value representing the color of choice
COLORREF CQueueGrid::OnGetDefBackColor(int section)
{
	UNREFERENCED_PARAMETER(section);
	return RGB(255,255,255);
}
//--------------------------------------------------------------------------------------------------
//	OnSetFocus
//		Sent when the grid is gaining focus.
//	Note:
//		The grid will loose focus when the edit is started, or drop list shown
//	Params:
//		section - Id of the grid section gaining focus, usually UG_GRID
//				  possible sections:
//						UG_TOPHEADING, UG_SIDEHEADING, UG_GRID
//	Return:
//		<none>
//
void CQueueGrid::OnSetFocus(int section)
{
	UNREFERENCED_PARAMETER(section);
}
//--------------------------------------------------------------------------------------------------
//	OnKillFocus
//		Sent when the grid is loosing focus.
//	Params:
//		section - Id of the grid section loosing focus, usually UG_GRID
//				  possible sections:
//						UG_TOPHEADING, UG_SIDEHEADING, UG_GRID
//		pNewWnd	- pointer to the window that is gaining focus
//	Return:
//		<none>
void CQueueGrid::OnKillFocus(int section, CWnd *pNewWnd)
{
	UNREFERENCED_PARAMETER(section);
}
//--------------------------------------------------------------------------------------------------
//	OnColSwapStart
//		Called to inform the grid that the col swap was started on given col.
//	Params:
//		col - identifies the column
//	Return:
//		TRUE - to allow for the swap to take place
//		FALSE - to disallow the swap
BOOL CQueueGrid::OnColSwapStart(int col)
{
	UNREFERENCED_PARAMETER(col);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnCanColSwap
//		Called when col swap is about to take place, here you can 'lock' certain
//		cols in place so that are not swappable.
//	Params:
//		fromCol - where the col originated from
//		toCol	- where the col will be located if the swap is allowed
//	Return:
//		TRUE - to allow for the swap to take place
//		FALSE - to disallow the swap
BOOL CQueueGrid::OnCanColSwap(int fromCol,int toCol)
{
	UNREFERENCED_PARAMETER(fromCol);
	UNREFERENCED_PARAMETER(toCol);
	return TRUE;
}
//--------------------------------------------------------------------------------------------------
//	OnColSwapped
//		Called just after column-swap operation was completed.
//	Params:
//		fromCol - where the col originated from
//		toCol	- where the col will be located if the swap is allowed
//	Return:
//		<none>
void CQueueGrid::OnColSwapped(int fromCol,int toCol)
{
	UNREFERENCED_PARAMETER(fromCol);
	UNREFERENCED_PARAMETER(toCol);
}
//--------------------------------------------------------------------------------------------------
//	OnTrackingWindowMoved
//		Called to notify the grid that the tracking window was moved
//	Params:
//		origRect	- from
//		newRect		- to location and size of the track window
//	Return:
//		<none>
void CQueueGrid::OnTrackingWindowMoved(RECT *origRect,RECT *newRect)
{
	UNREFERENCED_PARAMETER(*origRect);
	UNREFERENCED_PARAMETER(*newRect);
}
//--------------------------------------------------------------------------------------------------
void CQueueGrid::OnSize(UINT nType, int cx, int cy)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Determine the combined width of all columns except the description column.
		int nTotalWidth = 0;
		for (int i = 0; i < 5; i++)
		{
			if (i == 3)
			{
				continue;
			}

			nTotalWidth += GetColWidth(i);
		}

		// Set the description column to occupy the remaining width.
		CHECK_UG_RETURN_VALUE("ELI30472", SetColWidth(3, cx - nTotalWidth));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI30441");
}
//--------------------------------------------------------------------------------------------------
void CQueueGrid::InsertRow(int index)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI30442", index >= 0 && index <= GetNumberRows())

		CHECK_UG_RETURN_VALUE("ELI30473", EnableUpdate(FALSE));

		CHECK_UG_RETURN_VALUE("ELI30489", CUGCtrl::InsertRow(index));

		CUGCell cell;
		CHECK_UG_RETURN_VALUE("ELI30490", cell.CopyInfoFrom(m_GI->m_gridDefaults));
		CHECK_UG_RETURN_VALUE("ELI30474", cell.SetFont(&m_font));

		// Enabled
		CHECK_UG_RETURN_VALUE("ELI30475", cell.SetCellType(UGCT_CHECKBOX));
		CHECK_UG_RETURN_VALUE("ELI30476",
			cell.SetCellTypeEx(UGCT_CHECKBOXCHECKMARK | UGCT_CHECKBOXUSEALIGN));
		CHECK_UG_RETURN_VALUE("ELI30510", cell.SetAlignment(UG_ALIGNCENTER|UG_ALIGNVCENTER));
		CHECK_UG_RETURN_VALUE("ELI30477", SetCell(0, index, &cell));

		// Force Processing
		CHECK_UG_RETURN_VALUE("ELI30480", SetCell(1, index, &cell));

		// Priority
		CHECK_UG_RETURN_VALUE("ELI30481", cell.SetCellType(UGCT_DROPLIST));
		CHECK_UG_RETURN_VALUE("ELI30482", cell.SetLabelText(m_strPriorities.c_str()));
		CHECK_UG_RETURN_VALUE("ELI30483", SetCell(2, index, &cell));

		// Description
		CHECK_UG_RETURN_VALUE("ELI30506", cell.SetCellType(UGCT_NORMAL));
		CHECK_UG_RETURN_VALUE("ELI30507", cell.SetAlignment(UG_ALIGNLEFT));
		CHECK_UG_RETURN_VALUE("ELI30508", SetCell(3, index, &cell));

		// Status
		CHECK_UG_RETURN_VALUE("ELI30484", cell.SetCellType(UGCT_NORMAL));
		CHECK_UG_RETURN_VALUE("ELI30485", cell.SetAlignment(UG_ALIGNCENTER));
		CHECK_UG_RETURN_VALUE("ELI30486", SetCell(4, index, &cell));

		CHECK_UG_RETURN_VALUE("ELI30487", EnableUpdate(TRUE));
		CHECK_UG_RETURN_VALUE("ELI30488", RedrawAll());
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30443");
}
//--------------------------------------------------------------------------------------------------
void CQueueGrid::SetRowInfo(int nRow, bool bEnabled, bool bForceProcessing,
						   UCLID_FILEPROCESSINGLib::EFilePriority ePriority,
						   const string &strDescription, const string &strStatus)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		if (nRow >= GetNumberRows())
		{
			InsertRow(nRow);
		}

		CHECK_UG_RETURN_VALUE("ELI30491", EnableUpdate(FALSE));

		CHECK_UG_RETURN_VALUE("ELI30492", QuickSetText(0, nRow, bEnabled ? "1" : "0"));
		CHECK_UG_RETURN_VALUE("ELI30493", QuickSetText(1, nRow, bForceProcessing ? "1" : "0"));
		CHECK_UG_RETURN_VALUE("ELI30494", QuickSetText(2, nRow, getPriorityString(ePriority).c_str()));
		CHECK_UG_RETURN_VALUE("ELI30495", QuickSetText(3, nRow, strDescription.c_str()));
		CHECK_UG_RETURN_VALUE("ELI30496", QuickSetText(4, nRow, strStatus.c_str()));

		CHECK_UG_RETURN_VALUE("ELI30497", EnableUpdate(TRUE));
		Invalidate();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30465");
}