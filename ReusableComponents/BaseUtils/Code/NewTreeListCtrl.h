// NewTreeListCtrl.h : header file
//
// Demo project found at http://www.codeproject.com/treectrl/treelist.asp
// Originally written by David Lantsman (davidlan@inter.net.il)
// Copyright (c) 1999
//
// Some unneeded functionality has been removed including
//   - Sorting
//   - Right-to-left display
//   - Bold, Group and other display options

#pragma once

#include "BaseUtils.h"
#include "NewHeaderCtrl.h"

// Control IDs associated with use of the Tree List
const int ID_TREE_LIST_HEADER    = 370;
const int ID_TREE_LIST_CTRL      = 373;
const int ID_TREE_LIST_SCROLLBAR = 377;

enum {ALIGN_LEFT, ALIGN_RIGHT, ALIGN_CENTER, NO_ALIGN};

class CTLItem
{
public:
	CString m_itemString;

public:
	CTLItem();
	CTLItem(CTLItem &copyItem);
	DWORD itemData;

	char m_cEnding;

	// visual attributes
	COLORREF m_Color;
	BOOL m_HasChildren;

	// m_nSub is zero-based
	CString GetItemString() { return m_itemString; };
	CString GetSubstring(int m_nSub);
	CString GetItemText() { return GetSubstring(0); };
	void SetSubstring(int m_nSub, CString m_sText);
	void InsertItem(CString m_sText) { SetSubstring(0, m_sText); };
};

/////////////////////////////////////////////////////////////////////////////
// CNewTreeListCtrl window

class EXPORT_BaseUtils CNewTreeListCtrl : public CTreeCtrl
{
// Construction
public:
	CNewTreeListCtrl();

// Attributes
private:
	int m_nColumns;
	int m_nColumnsWidth;
	int m_nItems;

public:
	CNewHeaderCtrl m_wndHeader;
	CFont m_headerFont;
	CImageList m_cImageList;
	int m_nOffset;

	BOOL m_ParentsOnTop; // whether all the items that have
						        // children should go first

	// drag & drop
	CImageList* m_pDragImage;
	HTREEITEM m_htiDrag, m_htiDrop, m_htiOldDrop;
	BOOL m_bLDragging, m_toDrag;
	UINT m_idTimer, m_scrollTimer;
	UINT m_timerticks;

// Operations
public:
	CRect CRectGet(int left, int top, int right, int bottom);
	void RecalcHeaderPosition();

	int GetColumnsNum() { return m_nColumns; };
	int GetColumnsWidth() { return m_nColumnsWidth; };
	int GetItemCount() { return m_nItems; };
	void RecalcColumnsWidth();

	void ResetVertScrollBar();

	HTREEITEM GetTreeItem(int nItem);
	int GetListItem(HTREEITEM hItem);

	int InsertColumn(int nCol, LPCTSTR lpszColumnHeading, int nFormat = LVCFMT_LEFT, 
		int nWidth = -1, int nSubItem = -1);
	int GetColumnWidth(int nCol);
	int GetColumnAlign(int nCol);
	void SetColumnWidth(int nCol, int nWidth);

	BOOL SetItemData(HTREEITEM hItem, DWORD dwData);
	DWORD GetItemData(HTREEITEM hItem) const;

	CString GetItemText( HTREEITEM hItem, int nSubItem = 0 );
	CString GetItemText( int nItem, int nSubItem );

	HTREEITEM InsertItem( LPCTSTR lpszItem, int nImage, int nSelectedImage, 
		HTREEITEM hParent = TVI_ROOT, HTREEITEM hInsertAfter = TVI_LAST);
	HTREEITEM InsertItem(LPCTSTR lpszItem, HTREEITEM hParent = TVI_ROOT, 
		HTREEITEM hInsertAfter = TVI_LAST );
	HTREEITEM InsertItem(UINT nMask, LPCTSTR lpszItem, int nImage, int nSelectedImage, 
		UINT nState, UINT nStateMask, LPARAM lParam, HTREEITEM hParent, HTREEITEM hInsertAfter );

	HTREEITEM FindParentItem(CString m_title, int nCol = 0, HTREEITEM hItem = NULL, LPARAM itemData = 0);

	HTREEITEM CopyItem(HTREEITEM hItem, HTREEITEM hParent=TVI_ROOT, HTREEITEM hInsertAfter=TVI_LAST);
	HTREEITEM MoveItem(HTREEITEM hItem, HTREEITEM hParent=TVI_ROOT, HTREEITEM hInsertAfter=TVI_LAST);

	BOOL DeleteItem( HTREEITEM hItem );
	BOOL DeleteItem( int nItem );
	void MemDeleteAllItems(HTREEITEM hParent);
	BOOL DeleteAllItems();

	BOOL SetItemText( HTREEITEM hItem, int nCol ,LPCTSTR lpszItem );

	BOOL SetItemColor( HTREEITEM hItem, COLORREF m_newColor, BOOL m_bInvalidate = TRUE );
	BOOL SetItemFrame( HTREEITEM hItem, int nFrame, BOOL bInvalidate = TRUE );
	BOOL GetItemFrame(HTREEITEM hItem);

	void DrawItemText (CDC* pDC, CString text, CRect rect, int nWidth, int nFormat);

	void Begindrag(CPoint point);
	HTREEITEM AlterDropTarget(HTREEITEM hSource, HTREEITEM hTarget);

	BOOL Expand(HTREEITEM hItem, UINT nCode);

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CNewTreeListCtrl)
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CNewTreeListCtrl();

	// Generated message map functions
protected:
	//{{AFX_MSG(CNewTreeListCtrl)
	afx_msg void OnPaint();
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);
	afx_msg void OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnTimer(UINT nIDEvent);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg void OnDestroy();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Developer Studio will insert additional declarations immediately before the previous line.