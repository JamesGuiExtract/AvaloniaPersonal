// TreeGridWnd.h : header file
//

#pragma once

#include "..\..\..\APIs\RogueWave\Inc\Grid\gxall.h"
#include "RWUtils.h"

#include <vector>
#include <set>
#include <utility>
#include <string>

/////////////////////////////////////////////////////////////////////////////
// CTreeGridWnd window

class RW_UTILS_API CTreeGridWnd : public CGXGridWnd
{
// Construction
public:
	CTreeGridWnd();

// Attributes
public:

// Operations
public:
	// Empties cell contents
	void	Clear();

	// Clears selection(s) from Grid
	void	ClearSelection();

	// Adjust row-height and column spacing
	void	DoResize();

	// Add dummy entries to grid
	void	PopulateGrid();

	// Define column count, column widths, prepare header labels, set editable column
	// First column width of zero will be the variable width column - usually for the editable column
	void	PrepareGrid(std::vector<std::string> vecHeaders, std::vector<int> vecWidths);

	// Sets selected state for nRow
	void	SelectRow(int nRow);

	// Sets non-empty cells at nRow,nCol to Bold or Normal.  Also sets background color 
	// iff SetSpecialBackgroundColor() was already called.
	//    nRow > 0
	//    nCol > 0
	//    bBold - true for Bold text, false for Normal
	//    bBack - true for Special background color, false for Normal
	// FIXTHIS: why use int args here instead of unsigned int? using int args
	// causes additional casting work in the code
	// FIXTHIS: if this method signature is updated to use unsigned ints, then
	// update the method signatures of other methods taking similar args
	void	SetCellBoldBackground(int nRow, int nCol, bool bBold, bool bBack);

	// Sets the Control ID data member used in notification messages
	// If this method is called, notification messages will be sent with 
	// wParam = Control ID and lParam = MAKE_LPARAM( nRow, nCol )
	void	SetControlID(int nID);

	// Sets notification of cell-specific left-click ON
	// Sends specified Command to grid Parent with wParam = row AND lParam = column
	// NOTE: Set lCommandID = -1 to turn notification OFF
	void	SetLClickNotification(long lCommandID);

	// Specifies the string to be used as prefix for each level of indent.  For example, 
	// with strPrefix = " - -", a first-level child Name may be " - -Child1Name" and a 
	// second-level child Name may be " - - - -Child2Name"
	void	SetIndentPrefix(std::string strPrefix);

	// Sets text cells in nRow to Bold or Normal.  Also sets background color 
	// iff SetSpecialBackgroundColor() was already called.
	//    nRow > 0
	//    bBold - true for Bold text, false for Normal
	//    bBack - true for Special background color, false for Normal
	void	SetRowBoldBackground(int nRow, bool bBold, bool bBack);

	// Insert specified row data
	//    nRow > 0
	//    vecStrings - text data for subsequent columns.  Later columns in row will be blank
	//    lIndentLevel - stages of indent for display of Name information
	//                   0 = top level item
	//                   1 = first generation child
	//                   2 = second generation child
	int		SetRowInfo(int nRow, std::vector<std::string> vecStrings, long lIndentLevel);

	// Sets notification of cell-specific right-click ON
	// Sends specified Command to grid Parent with wParam = row AND lParam = column
	// NOTE: Set lCommandID = -1 to turn notification OFF
	void	SetRClickNotification(long lCommandID);

	// Sets notification of selection-change events
	// See documentation of WM_NOTIFY_SELECTION_CHANGED above for
	// interpretation of wParam and lParam associated with this command.
	// NOTE: Set lCommandID = -1 to turn notification OFF
	void	SetSelectionChangedNotification(long lCommandID);

	// Sets notification of double click cell events
	// See documentation of WM_NOTIFY_CELL_DBLCLK above for
	// interpretation of wParam and lParam associated with this command.
	// NOTE: Set lCommandID = -1 to turn notification OFF
	void	SetDblClkNotification(long lCommandID);

	// Specifies background color to be used by SetRowBoldBackground() and
	// SetCellBoldBackground()
	//    lHexColor - defines RGB color to be used
	void	SetSpecialBackgroundColor(long lHexColor);

	// Sets Value column
	//    This column will be resized to use up remaining Grid width
	void	SetValueColumn(int nCol);

	// gets the current value of the selected cell regardless of the
	// active state of the cell.  it will not change the active state
	// of the cell either
	// NOTE: function taken from RogueWave help file ogug.pdf
	//		 page 119/636 under section 5.1.2 Getting Values
	//		 from the Grid
	// [p16 #2722]
	std::string GetCellValue(ROWCOL nRow, ROWCOL nCol);

	// used to set the column number of a grid that contains a drop down list
	void	SetDropDownListColumn(int nCol);

	// user to remove a column number that no longer contains a drop down list
	void	RemoveDropDownListColumn(int nCol);

	// allows the user to manually update the cell that should not be highlighted
	// ARGS: nRow - the current row to reset the highlight state for
	//		 nCol - the current column to reset the highlight state for
	// NOTE: this function was added for ID Shield related feature additions [p16 #2833]
	void	UpdateCellToNotHighlight(int nRow, int nCol);

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTreeGridWnd)
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CTreeGridWnd();
	virtual void Initialize(); // needed to properly set up drop down list controls
	virtual BOOL GetStyleRowCol(ROWCOL nRow, ROWCOL nCol, CGXStyle& style, GXModifyType mt, int nType);
	virtual void DrawInvertCell(CDC* /*pDC*/, ROWCOL nRow, ROWCOL nCol, CRect rectItem);
	virtual BOOL OnTrackColWidth(ROWCOL nCol);
	virtual BOOL OnStartEditing(ROWCOL nRow, ROWCOL nCol);
	virtual BOOL OnEndEditing(ROWCOL nRow, ROWCOL nCol);
	virtual BOOL OnStartSelection(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point);
	virtual BOOL OnLButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point);
	virtual BOOL OnRButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point);
	virtual BOOL OnLButtonDblClkRowCol(ROWCOL nRow, ROWCOL nCol, UINT nFlags, CPoint pt);
	virtual void OnChangedSelection(const CGXRange* pRange, BOOL bIsDragging, BOOL bKey);
	virtual void OnModifyCell(ROWCOL nRow, ROWCOL nCol); // added as per [p16 #2722]
	virtual BOOL OnLButtonHitRowCol(ROWCOL nRow, ROWCOL nCol, ROWCOL nDragRow, ROWCOL nDragCol,
		CPoint cpPoint, UINT uiFlags, WORD nHitState);

	// Generated message map functions
protected:
	//{{AFX_MSG(CTreeGridWnd)
	afx_msg BOOL OnMouseWheel(UINT nFlags, short zDelta, CPoint pt);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	//////////
	// Methods
	//////////

	///////////////
	// Data Members
	///////////////
	// Collected strings used for column headings
	std::vector<std::string> m_vecHeaders;

	// True if PrepareGrid() has been called
	// Used by DoResize()
	bool		m_bIsPrepared;

	// Column for width resizing
	int			m_iValueColumn;

	// String to use as prefix for each level of indent
	// Default string is three blank spaces
	std::string	m_strIndentPrefix;

	// Row that is being edited
	int			m_iEditRow;

	// Editing is in progress
	bool		m_bEditStarted;

	// Special Background color has been defined
	bool		m_bSpecialBackground;
	long		m_lBackgroundColor;

	// set to contain all the columns that are drop down lists
	std::set<int> m_setDropDownListColumns;

	// pair to store the location of a cell that should not be highlighted
	// [p16 #2753]
	std::pair<int, int> m_prCellToNotHighlight;

	// Command IDs for left-click and right-click notification messages
	long		m_lLeftNotifyCommandID;
	long		m_lRightNotifyCommandID;
	long		m_lSelectionNotifyCommandID;
	long		m_lDblClkCellNotifyCommandID;

	// Control ID used in various notification messages.
	// If this value is set, notification messages will be sent with 
	// wParam = Control ID and lParam = MAKE_LPARAM( nRow, nCol )
	int			m_nControlID;
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
