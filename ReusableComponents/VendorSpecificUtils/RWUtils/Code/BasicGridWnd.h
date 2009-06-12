#pragma once

// BasicGridWnd.h : header file
//

#include "..\..\..\APIs\RogueWave\Inc\Grid\gxall.h"
#include "RWUtils.h"

// Width of vertical scrollbar in pixels
const int giOFFSET_VSCROLL = 16;

/////////////////////////////////////////////////////////////////////////////
// CBasicGridWnd window

class RW_UTILS_API CBasicGridWnd : public CGXGridWnd
{
// Construction
public:
	CBasicGridWnd();

// Attributes
public:

	// Empties cell contents
	void	Clear();

	// Adjust column widths and row heights
	// allowing space for scrollbar within the grid
	void	DoResize(long lVerticalScrollPixels);

	// Returns current text within specified cell
	CString	GetCellValue(ROWCOL nRow, ROWCOL nCol, bool bSaveIt = false);

	// Returns resource ID of this Grid.
	int		GetControlID();

	// Returns false if:
	//		at least one non-header cell contains text
	// Otherwise true
	bool	IsEmpty();

	// Returns true if:
	//		cell contents have changed or
	//		a row has been added or removed
	// Otherwise false
	bool	IsModified();

	// Define column count, column widths, prepare column labels and row labels
	// show / hide column headers, show / hide row headers.
	// Columns with zero width will be of variable width
	// nStyle is used by DoResize
	//   nStyle = 0 : Just adjusts column widths
	//   nStyle = 1 : Expands height of rows to fill grid vertically
	void	PrepareGrid(std::vector<std::string> vecColHeaders, std::vector<int> vecWidths, 
		std::vector<std::string> vecRowHeaders, long lRowHeaderWidth, bool bShowColHeaders, 
		bool bShowRowHeaders, int nStyle = 0);

	// Sets selected state for nRow
	void	SelectRow(int nRow);

	// Insert specified row data
	//    nRow > 0
	//    vecStrings - text data for subsequent columns.  Later columns in row will be blank
	// Returns row number of modified row
	int		SetRowInfo(int nRow, std::vector<std::string> vecStrings);

	// Sets the Control ID data member used in notification messages
	void	SetControlID(int nID);

	// Sets or Clears modified flag
	void	SetModified(bool bIsModified = true);

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CBasicGridWnd)
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CBasicGridWnd();
	virtual void DrawInvertCell(CDC* /*pDC*/, ROWCOL nRow, ROWCOL nCol, CRect rectItem);
	virtual BOOL GetStyleRowCol(ROWCOL nRow, ROWCOL nCol, CGXStyle& style, 
		GXModifyType mt, int nType);
	virtual BOOL OnStartEditing(ROWCOL nRow, ROWCOL nCol);
	virtual BOOL OnEndEditing(ROWCOL nRow, ROWCOL nCol);
	virtual BOOL OnStartSelection(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point);
	virtual BOOL OnRButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point);
	virtual BOOL MoveCurrentCell(int direction, UINT nCell = 1 , BOOL bCanSelectRange = TRUE);


	// Generated message map functions
protected:
	//{{AFX_MSG(CBasicGridWnd)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	BOOL	isVerticalScrollVisible();

	// True if PrepareGrid() has been called
	// Used by DoResize()
	bool	m_bIsPrepared;

	// True if cell or row information has been modified
	bool	m_bIsModified;

	// Original contents of cell being edited
	CString	m_zOriginalText;

	// Describes visibility of header information for rows and columns
	bool	m_bShowColHeader;
	bool	m_bShowRowHeader;

	// Row that is being edited
	int		m_iEditRow;

	// Editing is in progress
	bool	m_bEditStarted;

	// Control ID used in various notification messages.
	//   Changing the value will change ID for ALL notifications
	int		m_nControlID;

	// Defines resize behavior
	int		m_nResizeStyle;
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
