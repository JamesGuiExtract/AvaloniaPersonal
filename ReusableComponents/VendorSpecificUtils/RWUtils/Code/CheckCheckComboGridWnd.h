// CheckCheckGridWnd.h : header file
//

#pragma once

#include "..\..\..\APIs\RogueWave\Inc\Grid\gxall.h"
#include "RWUtils.h"

#include <vector>
#include <set>
#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CCheckCheckComboGridWnd window

class RW_UTILS_API CCheckCheckComboGridWnd : public CGXGridWnd
{
// Construction
public:
	CCheckCheckComboGridWnd();

// Attributes
public:

// Operations
public:
	// Set or clear specified checkbox for specified item
	// iSubitem = {1, 2}
	void	CheckItem(int iItem, int iSubitem, bool bCheck);

	// Empties cell contents
	void	Clear();

	// Adjust row-height and column spacing
	void	DoResize();

	// Returns current text within specified cell
	CString	GetCellValue(ROWCOL nRow, ROWCOL nCol);

	// Return True if this Row and Column pair is checked, False otherwise
	// iColumn = {1, 2}
	bool	GetCheckedState(int iRow, int iColumn);

	// Returns row number of first selected row {1, 2, ..., N}
	int		GetFirstSelectedRow();

	// Read/Write Locked attribute for specified row
	bool	GetRowLock(int nRow);
	void	SetRowLock(int nRow, bool bLocked);

	// Define column count, column widths, prepare header labels
	// First column width of zero will be the variable width column
	void	PrepareGrid(const vector<string>& vecHeaders, const vector<int>& vecWidths,
		const vector<string>& vecComboList);

	// Sets selected state for nRow
	void	SelectRow(int nRow);

	// Sets the Control ID data member used in notification messages
	void	SetControlID(int nID);

	// Insert specified row data
	//    nRow > 0
	//    bChecked1 - sets check mark in first column
	//    bChecked2 - sets check mark in second column
	//	  strComboValue - sets the appropriate combo box value
	//    vecStrings - text data for subsequent columns.  Later columns in row will be blank
	int		SetRowInfo(int nRow, bool bChecked1, bool bChecked2, const string& strComboValue,
		const vector<string>& vecStrings);

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCheckGridWnd)
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CCheckCheckComboGridWnd();
	virtual void Initialize();
	virtual void OnClickedButtonRowCol(ROWCOL nRow, ROWCOL nCol);
	virtual BOOL GetStyleRowCol(ROWCOL nRow, ROWCOL nCol, CGXStyle& style, GXModifyType mt, int nType);
	virtual void DrawInvertCell(CDC* /*pDC*/, ROWCOL nRow, ROWCOL nCol, CRect rectItem);
	virtual BOOL OnTrackColWidth(ROWCOL nCol);
	virtual BOOL OnStartSelection(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point);
	virtual BOOL OnLButtonDblClkRowCol(ROWCOL nRow, ROWCOL nCol, UINT nFlags, CPoint pt);
	virtual BOOL OnLButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol, UINT nFlags, CPoint pt);
	virtual BOOL OnLButtonHitRowCol(ROWCOL nRow, ROWCOL nCol, ROWCOL nDragRow, ROWCOL nDragCol,
		CPoint cpPoint, UINT uiFlags, WORD nHitState);
	virtual void OnModifyCell(ROWCOL nRow, ROWCOL nCol);

	// Generated message map functions
protected:
	//{{AFX_MSG(CCheckGridWnd)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	//////////
	// Methods
	//////////
	// Set or clear specified checkbox for specified item and also provide Normal attribute value
	void	checkAttribute(int iItem, int iSubitem, bool bCheck);

	///////////////
	// Data Members
	///////////////
	set<string> m_setComboList;

	// Control ID used in various notification messages.
	//   Changing the value will change ID for ALL notifications
	int			m_nControlID;

	// True if PrepareGrid() has been called
	// Used by DoResize()
	bool		m_bIsPrepared;

	// Column for width resizing
	int			m_iResizeColumn;

	// Row to exclude the highlight in the combo drop down cell
	int			m_iRowToNotHighlight;
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
