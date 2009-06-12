// CheckGridWnd.h : header file
//

#pragma once

// Column IDs
const int	gACCEPT_COLUMN			= 1;
const int	gNAME_COLUMN			= 2;
const int	gVALUE_COLUMN			= 3;
const int	gCOUNT_COLUMN			= 4;

#include "..\..\..\APIs\RogueWave\Inc\Grid\gxall.h"
#include "RWUtils.h"

#include <vector>
#include <string>

/////////////////////////////////////////////////////////////////////////////
// CCheckGridWnd window

class RW_UTILS_API CCheckGridWnd : public CGXGridWnd
{
// Construction
public:
	CCheckGridWnd();

// Attributes
public:

// Operations
public:
	// Set or clear check for specified item
	void	CheckItem(int iItem, bool bCheck);

	// Empties cell contents
	void	Clear();

	// Adjust row-height and column spacing
	void	DoResize();

	// Return count of items checked or zero if m_bShowCheck == false
	int		GetCheckedCount();

	// Add dummy entries to grid
	void	PopulateGrid();

	// Define column count, column widths, prepare header labels, set checkbox and editable columns
	// First column width of zero will be the variable width column - usually for the editable column
	void	PrepareGrid(std::vector<std::string> vecHeaders, std::vector<int> vecWidths, bool bShowCheckboxes, bool bGroupByName);

	// Insert specified row data
	//    nRow > 0
	//    bChecked - sets check mark in first column or is ignored if m_bShowCheck == false
	//    vecStrings - text data for subsequent columns.  Later columns in row will be blank
	int		SetRowInfo(int nRow, bool bChecked, std::vector<std::string> vecStrings);

	// Sets Value column
	//    This column will be resized to use up remaining Grid width
	void	SetValueColumn(int nCol);

	// Specifies the string to be displayed in the specified column after the editable cell
	// has been modified in a particular row.  For example, after the Value string has been 
	// modified by the user, the Count column may indicate "N/A"
	void	SetValueModifiedIndicator(int nCol, std::string strModified);


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCheckGridWnd)
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CCheckGridWnd();
	virtual void OnClickedButtonRowCol(ROWCOL nRow, ROWCOL nCol);
	virtual BOOL GetStyleRowCol(ROWCOL nRow, ROWCOL nCol, CGXStyle& style, GXModifyType mt, int nType);
	virtual void DrawInvertCell(CDC* /*pDC*/, ROWCOL nRow, ROWCOL nCol, CRect rectItem);
	virtual BOOL OnTrackColWidth(ROWCOL nCol);
	virtual BOOL OnValidateCell(ROWCOL nRow, ROWCOL nCol);
	virtual BOOL OnStartEditing(ROWCOL nRow, ROWCOL nCol);
	virtual BOOL OnEndEditing(ROWCOL nRow, ROWCOL nCol);

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
	// Set or clear check for specified item and also provide Normal attribute value
	void	checkAttribute(int iItem, bool bCheck, long lNormal);

	// Determine Normal attribute for specified row
	long	getNormalAttribute(int iItem);

	///////////////
	// Data Members
	///////////////
	// Collected strings used for column headings
	std::vector<std::string> m_vecHeaders;

	// Show centered 3-D checkboxes in first column
	bool		m_bShowCheck;

	// Show alternating background colors based on first non-check column
	// If checkboxes present, they will also act like radio buttons if grouped
	bool		m_bGroupByName;

	// True if PrepareGrid() has been called
	// Used by DoResize()
	bool		m_bIsPrepared;

	// Column for width resizing
	int			m_iValueColumn;

	// Column for which Modified string applies
	int			m_iShowModificationColumn;

	// String to be displayed in specified column after Value string has changed
	std::string	m_strValueModified;

	// Row that is being edited
	int			m_iEditRow;

	// Editing is in progress
	bool		m_bEditStarted;
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
