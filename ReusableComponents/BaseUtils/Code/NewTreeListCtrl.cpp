// NewTreeListCtrl.cpp : implementation file
//

#include "stdafx.h"
#include "NewTreeListCtrl.h"
#include "TLFrame.h"
#include "UCLIDException.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CTLItem
//-------------------------------------------------------------------------------------------------
CTLItem::CTLItem()
{
	m_cEnding = '¶';
	m_itemString = "";
	m_Color = ::GetSysColor(COLOR_WINDOWTEXT);
	m_HasChildren = FALSE;
}
//-------------------------------------------------------------------------------------------------
CTLItem::CTLItem(CTLItem &copyItem)
{
	m_cEnding = copyItem.m_cEnding;
	m_itemString = copyItem.GetItemString();
	m_Color = copyItem.m_Color;
	itemData = copyItem.itemData;
	m_HasChildren = copyItem.m_HasChildren;
}
//-------------------------------------------------------------------------------------------------
CString CTLItem::GetSubstring(int m_nSub)
{
	CString m_tmpStr("");
	int i=0, nHits=0;
	int length = m_itemString.GetLength();

	while ((i<length) && (nHits<=m_nSub))
	{
		if(m_itemString[i] == m_cEnding)
		{
			nHits++;
		}
		else if (nHits == m_nSub)
		{
			m_tmpStr+=m_itemString[i];
		}

		i++;
	}

	if ((i >= length) && (nHits < m_nSub))
	{
		return "";
	}
	else
	{
		return m_tmpStr;
	}
}
//-------------------------------------------------------------------------------------------------
void CTLItem::SetSubstring(int m_nSub, CString m_sText)
{
	CString m_tmpStr("");
	int i=0, nHits=0, first=0;
	int length = m_itemString.GetLength();

	while ((i < length) && (nHits <= m_nSub))
	{
		if (m_itemString[i] == m_cEnding)
		{
			if (nHits != m_nSub)
			{
				first = i;
			}
			nHits++;
		}

		i++;
	}

	CString m_newStr("");
	if ((nHits > m_nSub) || ((nHits == m_nSub) && (i >= length)))
	{
		// insert in the middle
		if (first != 0)
		{
			m_newStr = m_itemString.Left(first);
			m_newStr += m_cEnding; 
		}
		m_newStr += m_sText;

		if (i < length)
		{
			m_newStr += m_cEnding;
			m_newStr += m_itemString.Right(m_itemString.GetLength()-i);
		}

		m_itemString=m_newStr;
	}
	else
	{
		// insert at the end
		for (i = nHits; i < m_nSub; i++)
		{
			m_itemString += m_cEnding;
		}

		m_itemString += m_sText;
	}
}

//-------------------------------------------------------------------------------------------------
// CNewTreeListCtrl
//-------------------------------------------------------------------------------------------------
CNewTreeListCtrl::CNewTreeListCtrl()
{
	m_nColumns = m_nColumnsWidth = 0;
	m_nOffset = 0;
	m_ParentsOnTop = TRUE;

	m_bLDragging = FALSE;
	m_htiOldDrop = m_htiDrop = m_htiDrag = NULL;
	m_scrollTimer = m_idTimer = 0;
	m_timerticks = 0;
	m_toDrag = FALSE;
}
//-------------------------------------------------------------------------------------------------
CNewTreeListCtrl::~CNewTreeListCtrl()
{
	try
	{
		// if the window is already destroyed, then no need to clean the memory
		// since it's been called in OnDestroy()
		if (m_hWnd)
		{
			// Delete all items in tree
			MemDeleteAllItems(GetRootItem());
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16370");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CNewTreeListCtrl, CTreeCtrl)
	//{{AFX_MSG_MAP(CNewTreeListCtrl)
	ON_WM_PAINT()
	ON_WM_CREATE()
	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONDBLCLK()
	ON_WM_KEYDOWN()
	ON_WM_TIMER()
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()
	ON_WM_DESTROY()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CNewTreeListCtrl message handlers
//-------------------------------------------------------------------------------------------------
int CNewTreeListCtrl::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
	if (CTreeCtrl::OnCreate(lpCreateStruct) == -1)
	{
		return -1;
	}

	return 0;
}
//-------------------------------------------------------------------------------------------------
HTREEITEM CNewTreeListCtrl::GetTreeItem(int nItem)
{
	HTREEITEM m_ParentItem = GetRootItem();
	int m_nCount = 0;

	while ((m_ParentItem != __nullptr) && (m_nCount < nItem))
	{
		m_nCount ++ ;
		GetNextSiblingItem(m_ParentItem);
	}

	return m_ParentItem;
}
//-------------------------------------------------------------------------------------------------
int CNewTreeListCtrl::GetListItem(HTREEITEM hItem)
{
	HTREEITEM m_ParentItem = GetRootItem();
	int m_nCount = 0;

	while ((m_ParentItem != __nullptr) && (m_ParentItem != hItem))
	{
		m_nCount ++ ;
		GetNextSiblingItem(m_ParentItem);
	}

	return m_nCount;
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::RecalcHeaderPosition()
{
}
//-------------------------------------------------------------------------------------------------
int CNewTreeListCtrl::InsertColumn(int nCol, LPCTSTR lpszColumnHeading, int nFormat, 
								   int nWidth, int /*nSubItem*/)
{
	HD_ITEM hdi;
	hdi.mask = HDI_TEXT | HDI_FORMAT;
	if (nWidth!=-1)
	{
		hdi.mask |= HDI_WIDTH;
		hdi.cxy = nWidth;
	}
	
	hdi.pszText = (LPTSTR)lpszColumnHeading;
	hdi.fmt = HDF_OWNERDRAW;

	if (nFormat == LVCFMT_RIGHT)
	{
		hdi.fmt |= HDF_RIGHT;
	}
	else if (nFormat == LVCFMT_CENTER)
	{
		hdi.fmt |= HDF_CENTER;
	}
	else
	{
		hdi.fmt |= HDF_LEFT;
	}

	m_nColumns ++;

	int m_nReturn = m_wndHeader.InsertItem(nCol, &hdi);

	RecalcColumnsWidth();

	UpdateWindow();

	return m_nReturn;
}
//-------------------------------------------------------------------------------------------------
int CNewTreeListCtrl::GetColumnWidth(int nCol)
{
	HD_ITEM hItem;
	hItem.mask = HDI_WIDTH;
	if (!m_wndHeader.GetItem(nCol, &hItem))
	{
		return 0;
	}

	return hItem.cxy;
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::SetColumnWidth(int nCol, int nWidth)
{
	HD_ITEM hItem;
	hItem.mask = HDI_WIDTH;
	hItem.cxy = nWidth;
	m_wndHeader.SetItem( nCol, &hItem );
}
//-------------------------------------------------------------------------------------------------
int CNewTreeListCtrl::GetColumnAlign(int nCol)
{
	HD_ITEM hItem;
	hItem.mask = HDI_FORMAT;
	if (!m_wndHeader.GetItem(nCol, &hItem))
	{
		return LVCFMT_LEFT;
	}

	if (hItem.fmt & HDF_RIGHT)
	{
		return LVCFMT_RIGHT;
	}
	else if (hItem.fmt & HDF_CENTER)
	{
		return LVCFMT_CENTER;
	}
	else
	{
		return LVCFMT_LEFT;
	}
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::RecalcColumnsWidth()
{
	m_nColumnsWidth = 0;
	for (int i = 0; i < m_nColumns; i++)
	{
		m_nColumnsWidth += GetColumnWidth(i);
	}
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::DrawItemText (CDC* pDC, CString text, CRect rect, int nWidth, int nFormat)
{
    //
    // Make sure the text will fit in the prescribed rectangle, and truncate
    // it if it won't.
    //
    BOOL bNeedDots = FALSE;
    int nMaxWidth = nWidth - 4;

    while ((text.GetLength()>0) && (pDC->GetTextExtent((LPCTSTR) text).cx > (nMaxWidth - 4))) 
	{
        text = text.Left (text.GetLength () - 1);
        bNeedDots = TRUE;
    }

    if (bNeedDots) 
	{
        if (text.GetLength () >= 1)
		{
            text = text.Left (text.GetLength () - 1);

			text += "...";
		}
    }

    //
    // Draw the text into the rectangle using MFC's handy CDC::DrawText
    // function.
    //
    rect.right = rect.left + nMaxWidth;

    UINT nStyle = DT_VCENTER | DT_SINGLELINE | DT_NOPREFIX;
    if (nFormat == LVCFMT_LEFT)
	{
        nStyle |= DT_LEFT;
	}
    else if (nFormat == LVCFMT_CENTER)
	{
        nStyle |= DT_CENTER;
	}
    else // nFormat == LVCFMT_RIGHT
	{
        nStyle |= DT_RIGHT;
	}

	if ((text.GetLength() > 0) && (rect.right > rect.left))
	{
		pDC->DrawText( text, rect, nStyle );
	}
}
//-------------------------------------------------------------------------------------------------
CRect CNewTreeListCtrl::CRectGet(int left, int top, int right, int bottom)
{
	return CRect(left, top, right, bottom);
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::OnPaint() 
{
	CPaintDC dc(this); // device context for painting
	CRect rcClient;
	GetClientRect(&rcClient);

	//CMemDC dc(&paintdc, rcClient);

	CRect rcClip;
	dc.GetClipBox( &rcClip );

	// Set clip region to be same as that in paint DC
	CRgn rgn;
	rgn.CreateRectRgnIndirect( &rcClip );
	dc.SelectClipRgn(&rgn);
	rgn.DeleteObject();
	
	COLORREF m_wndColor = GetSysColor( COLOR_WINDOW );

	dc.SetViewportOrg(m_nOffset, 0);

	dc.SetTextColor(m_wndColor);

	// First let the control do its default drawing.

	CRect m_clientRect;
	GetClientRect(&m_clientRect);

	CTreeCtrl::DefWindowProc( WM_PAINT, (WPARAM)dc.m_hDC, 0 );

	HTREEITEM hItem = GetFirstVisibleItem();

	int n = GetVisibleCount(), m_nWidth;

	CTLItem *pItem;

	// create the font
	CFont *pFontDC;
	CFont fontDC, boldFontDC;
	LOGFONT logfont;

	CFont *pFont = GetFont();
	pFont->GetLogFont( &logfont );

	fontDC.CreateFontIndirect( &logfont );
	pFontDC = dc.SelectObject( &fontDC );

	logfont.lfWeight = 700;
	boldFontDC.CreateFontIndirect( &logfont );

	// and now let's get to the painting itself

	hItem = GetFirstVisibleItem();
	n = GetVisibleCount();
	while(hItem!=NULL && n>=0)
	{
		CRect rect;

		UINT selflag = /*TVIS_DROPHILITED |*/ TVIS_SELECTED;
	
		pItem = (CTLItem *)CTreeCtrl::GetItemData(hItem);

		HTREEITEM hParent = GetParentItem(hItem);
		if (hParent != __nullptr)
		{
			CTLItem *pParent = (CTLItem *)CTreeCtrl::GetItemData(hParent);
		}

		if( !(GetItemState( hItem, selflag ) & selflag ))
		{
			dc.SetBkMode(TRANSPARENT);

			CString sItem = pItem->GetItemText();

			CRect m_labelRect;
			GetItemRect( hItem, &m_labelRect, TRUE );
			GetItemRect( hItem, &rect, FALSE );
			if (GetColumnsNum() > 1)
			{
				rect.left = min(m_labelRect.left, GetColumnWidth(0));
			}
			else
			{
				rect.left = m_labelRect.left;
			}
			rect.right = m_nColumnsWidth;

			dc.SetBkColor( m_wndColor );

			dc.SetTextColor( pItem->m_Color );

			DrawItemText( &dc, sItem, 
				CRectGet(rect.left+2, rect.top, GetColumnWidth(0), rect.bottom), 
				GetColumnWidth(0)-rect.left-2, GetColumnAlign(0) );

			m_nWidth = 0;
			for (int i = 1; i < m_nColumns; i++)
			{
				m_nWidth += GetColumnWidth(i-1);
				DrawItemText( &dc, pItem->GetSubstring(i), 
					CRectGet(m_nWidth, rect.top, m_nWidth+GetColumnWidth(i), rect.bottom), 
					GetColumnWidth(i), GetColumnAlign(i));
			}
			
			dc.SetTextColor(::GetSysColor (COLOR_WINDOWTEXT ));
		}
		else
		{
			CRect m_labelRect;
			GetItemRect( hItem, &m_labelRect, TRUE );
			GetItemRect( hItem, &rect, FALSE );
			if (GetColumnsNum() > 1)
			{
				rect.left = min(m_labelRect.left, GetColumnWidth(0));
			}
			else
			{
				rect.left = m_labelRect.left;
			}
			rect.right = m_nColumnsWidth;

			// If the item is selected, paint the rectangle with the system color
			// COLOR_HIGHLIGHT

			COLORREF m_highlightColor = ::GetSysColor (COLOR_HIGHLIGHT);

			CBrush brush(m_highlightColor);

			dc.FillRect(rect, &brush);

			// draw a dotted focus rectangle
			dc.DrawFocusRect(rect);

			pItem = (CTLItem *)CTreeCtrl::GetItemData(hItem);
			CString sItem = pItem->GetItemText();

			dc.SetBkColor(m_highlightColor);

			dc.SetTextColor(::GetSysColor (COLOR_HIGHLIGHTTEXT));

			DrawItemText( &dc, sItem, 
				CRectGet(rect.left+2, rect.top, GetColumnWidth(0), rect.bottom), 
				GetColumnWidth(0)-rect.left-2, GetColumnAlign(0));

			m_nWidth = 0;
			for (int i = 1; i < m_nColumns; i++)
			{
				m_nWidth += GetColumnWidth(i-1);
				DrawItemText( &dc, pItem->GetSubstring(i), 
					CRectGet(m_nWidth, rect.top, m_nWidth+GetColumnWidth(i), rect.bottom), 
					GetColumnWidth(i), GetColumnAlign(i));
			}
		}

		hItem = GetNextVisibleItem( hItem );
		n--;
	}

	dc.SelectObject( pFontDC );
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::ResetVertScrollBar()
{
	CTLFrame *pFrame = (CTLFrame*)GetParent();

	CRect m_treeRect;
	GetClientRect(&m_treeRect);

	CRect m_wndRect;
	pFrame->GetClientRect(&m_wndRect);

	CRect m_headerRect;
	m_wndHeader.GetClientRect(&m_headerRect);

	CRect m_barRect;
	pFrame->m_horScrollBar.GetClientRect(&m_barRect);

	if (!pFrame->HorizontalScrollVisible())
	{
		SetWindowPos( &wndTop, 0, 0, m_wndRect.Width(), 
			m_wndRect.Height()-m_headerRect.Height(), SWP_NOMOVE );
	}
	else
	{
		SetWindowPos( &wndTop, 0, 0, m_wndRect.Width(), 
			m_wndRect.Height() - m_barRect.Height() - m_headerRect.Height(), SWP_NOMOVE );
	}

	if (pFrame->HorizontalScrollVisible())
	{
		if (!pFrame->VerticalScrollVisible())
		{
			pFrame->m_horScrollBar.SetWindowPos( &wndTop, 0, 0, m_wndRect.Width(), 
				m_barRect.Height(), SWP_NOMOVE );

			int nMin, nMax;
			pFrame->m_horScrollBar.GetScrollRange(&nMin, &nMax);
			if ((nMax-nMin) == (GetColumnsWidth() - m_treeRect.Width() + GetSystemMetrics(SM_CXVSCROLL)))
				// i.e. it disappeared because of calling
				// SetWindowPos
			{
				if (nMax - GetSystemMetrics(SM_CXVSCROLL) > 0)
				{
					pFrame->m_horScrollBar.SetScrollRange(nMin, nMax - GetSystemMetrics(SM_CXVSCROLL));
				}
				else
					// hide the horz scroll bar and update the tree
				{
					pFrame->m_horScrollBar.EnableWindow(FALSE);

					// we no longer need it, so hide it!
					{
						pFrame->m_horScrollBar.ShowWindow(SW_HIDE);

						SetWindowPos( &wndTop, 0, 0, m_wndRect.Width(), 
							m_wndRect.Height() - m_headerRect.Height(), SWP_NOMOVE );
						// the tree takes scroll's place
					}

					pFrame->m_horScrollBar.SetScrollRange(0, 0);

					// set scroll offset to zero
					{
						m_nOffset = 0;
						Invalidate();
						m_wndHeader.GetWindowRect(&m_headerRect);
						m_wndHeader.SetWindowPos( &wndTop, m_nOffset, 0, 
							max(pFrame->StretchWidth(GetColumnsWidth(), m_wndRect.Width()), 
							m_wndRect.Width()), m_headerRect.Height(), SWP_SHOWWINDOW );
					}
				}
			}
		}
		else
		{
			pFrame->m_horScrollBar.SetWindowPos( &wndTop, 0, 0, 
				m_wndRect.Width() - GetSystemMetrics(SM_CXVSCROLL), m_barRect.Height(), SWP_NOMOVE);

			int nMin, nMax;
			pFrame->m_horScrollBar.GetScrollRange(&nMin, &nMax);
			if ((nMax-nMin) == (GetColumnsWidth() - m_treeRect.Width() - GetSystemMetrics(SM_CXVSCROLL)))
				// i.e. it appeared because of calling
				// SetWindowPos
			{
				pFrame->m_horScrollBar.SetScrollRange(nMin, nMax + GetSystemMetrics(SM_CXVSCROLL));
			}
		}
	}
	else if (pFrame->VerticalScrollVisible())
	{
		if (GetColumnsWidth()>m_treeRect.Width())
			// the vertical scroll bar takes some place
			// and the columns are a bit bigger than the client
			// area but smaller than (client area + vertical scroll width)
		{
			// show the horz scroll bar
			{
				pFrame->m_horScrollBar.EnableWindow(TRUE);

				pFrame->m_horScrollBar.ShowWindow(SW_SHOW);

				// the tree becomes smaller
				SetWindowPos( &wndTop, 0, 0, m_wndRect.Width(), 
					m_wndRect.Height() - m_barRect.Height() - m_headerRect.Height(), SWP_NOMOVE );

				pFrame->m_horScrollBar.SetWindowPos( &wndTop, 0, 0, 
					m_wndRect.Width() - GetSystemMetrics(SM_CXVSCROLL), 
					m_barRect.Height(), SWP_NOMOVE );
			}

			pFrame->m_horScrollBar.SetScrollRange(0, GetColumnsWidth()-m_treeRect.Width());
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::OnLButtonDown(UINT /*nFlags*/, CPoint point) 
{
	UINT flags;
	HTREEITEM m_selectedItem = HitTest(point, &flags);

	if ((flags & TVHT_ONITEMRIGHT) || (flags & TVHT_ONITEMINDENT) ||
	   (flags & TVHT_ONITEM))
	{
		SelectItem(m_selectedItem);
	}

	if ((GetColumnsNum() == 0) || (point.x < GetColumnWidth(0)))
	{
		point.x -= m_nOffset;
		m_selectedItem = HitTest(point, &flags);
		if (flags & TVHT_ONITEMBUTTON)
		{
			Expand(m_selectedItem, TVE_TOGGLE);
		}
	}

	SetFocus();

	// Resize of TLFrame parent already resets the vertical scroll bar
//	ResetVertScrollBar();
	((CTLFrame *)GetParent())->DoResize();

	m_toDrag = FALSE;
	m_idTimer = SetTimer( 1000, 70, NULL );

//	CTreeCtrl::OnLButtonDown(nFlags, point);
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::OnLButtonDblClk(UINT nFlags, CPoint point) 
{
	if ((GetColumnsNum() == 0) || (point.x < GetColumnWidth(0)))
	{
		CTreeCtrl::OnLButtonDblClk(nFlags, point);
		ResetVertScrollBar();
	}

	SetFocus();

	GetParent()->SendMessage(WM_LBUTTONDBLCLK);
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags) 
{
	CTreeCtrl::OnKeyDown(nChar, nRepCnt, nFlags);
	ResetVertScrollBar();
}
//-------------------------------------------------------------------------------------------------
BOOL CNewTreeListCtrl::SetItemData(HTREEITEM hItem, DWORD dwData)
{
	CTLItem *pItem = (CTLItem *)CTreeCtrl::GetItemData(hItem);
	if (!pItem)
	{
		return FALSE;
	}

	pItem->itemData = dwData;
	return CTreeCtrl::SetItemData(hItem, (LPARAM)pItem);
}
//-------------------------------------------------------------------------------------------------
DWORD CNewTreeListCtrl::GetItemData(HTREEITEM hItem) const
{
	CTLItem *pItem = (CTLItem *)CTreeCtrl::GetItemData(hItem);
	if (!pItem)
	{
		return NULL;
	}

	return pItem->itemData;
}
//-------------------------------------------------------------------------------------------------
HTREEITEM CNewTreeListCtrl::InsertItem(LPCTSTR lpszItem, HTREEITEM hParent, HTREEITEM hInsertAfter)
{
	CTLItem *pItem = new CTLItem;
	pItem->InsertItem(lpszItem);
	m_nItems++;

	if ((hParent!=NULL) && (hParent!=TVI_ROOT))
	{
		CTLItem *pParent = (CTLItem *)CTreeCtrl::GetItemData(hParent);
		pParent->m_HasChildren = TRUE;
	}

	HTREEITEM hReturn = CTreeCtrl::InsertItem( TVIF_PARAM|TVIF_TEXT, "", 0, 0, 0, 0, 
		(LPARAM)pItem, hParent, hInsertAfter);

	((CTLFrame*)GetParent())->ResetScrollBar();

	return hReturn;
}
//-------------------------------------------------------------------------------------------------
HTREEITEM CNewTreeListCtrl::InsertItem( LPCTSTR lpszItem, int nImage, int nSelectedImage, 
									   HTREEITEM hParent, HTREEITEM hInsertAfter)
{
	CTLItem *pItem = new CTLItem;
	pItem->InsertItem(lpszItem);
	m_nItems++;

	if ((hParent!=NULL) && (hParent!=TVI_ROOT))
	{
		CTLItem *pParent = (CTLItem *)CTreeCtrl::GetItemData(hParent);
		pParent->m_HasChildren = TRUE;
	}

	HTREEITEM hReturn = CTreeCtrl::InsertItem( 
		TVIF_PARAM|TVIF_TEXT|TVIF_IMAGE|TVIF_SELECTEDIMAGE, "", nImage, nSelectedImage, 
		0, 0, (LPARAM)pItem, hParent, hInsertAfter);

	((CTLFrame*)GetParent())->ResetScrollBar();

	return hReturn;
}
//-------------------------------------------------------------------------------------------------
HTREEITEM CNewTreeListCtrl::InsertItem(UINT nMask, LPCTSTR lpszItem, int nImage, int nSelectedImage, 
									   UINT nState, UINT nStateMask, LPARAM lParam, 
									   HTREEITEM hParent, HTREEITEM hInsertAfter )
{
	CTLItem *pItem = new CTLItem;
	pItem->InsertItem(lpszItem);
	pItem->itemData = lParam;
	m_nItems++;

	if ((hParent!=NULL) && (hParent!=TVI_ROOT))
	{
		CTLItem *pParent = (CTLItem *)CTreeCtrl::GetItemData(hParent);
		pParent->m_HasChildren = TRUE;
	}

	HTREEITEM hReturn = CTreeCtrl::InsertItem(nMask, "", nImage, nSelectedImage, nState, nStateMask, (LPARAM)pItem, hParent, hInsertAfter);

	((CTLFrame*)GetParent())->ResetScrollBar();

	return hReturn;
}
//-------------------------------------------------------------------------------------------------
HTREEITEM CNewTreeListCtrl::CopyItem(HTREEITEM hItem, HTREEITEM hParent, HTREEITEM hInsertAfter)
{
	if (ItemHasChildren(hItem))
	{
		return NULL;
	}

	TV_ITEM item;
	item.mask = TVIF_IMAGE | TVIF_PARAM | TVIF_SELECTEDIMAGE | TVIF_STATE | TVIF_TEXT;
	item.hItem = hItem;
	GetItem(&item);
	CTLItem *pItem = (CTLItem *)CTreeCtrl::GetItemData(hItem);
	CTLItem *pNewItem = new CTLItem(*pItem);

	item.lParam = (LPARAM)pNewItem;

	TV_INSERTSTRUCT insStruct;
	insStruct.item = item;
	insStruct.hParent = hParent;
	insStruct.hInsertAfter = hInsertAfter;

	if ((hParent != __nullptr) && (hParent != TVI_ROOT))
	{
		CTLItem *pParent = (CTLItem *)CTreeCtrl::GetItemData(hParent);
		pParent->m_HasChildren = TRUE;
	}

	return CTreeCtrl::InsertItem(&insStruct);
}
//-------------------------------------------------------------------------------------------------
HTREEITEM CNewTreeListCtrl::MoveItem(HTREEITEM hItem, HTREEITEM hParent, HTREEITEM hInsertAfter)
{
	if (ItemHasChildren(hItem))
	{
		return NULL;
	}

	TV_ITEM item;
	item.mask = TVIF_IMAGE | TVIF_SELECTEDIMAGE | TVIF_STATE;
	item.hItem = hItem;
	GetItem(&item);
	CTLItem *pItem = (CTLItem *)CTreeCtrl::GetItemData(hItem);
	CTLItem *pNewItem = new CTLItem(*pItem);

	item.pszText = "";
	item.lParam = (LPARAM)pNewItem;
	item.hItem = NULL;

	item.mask |= TVIF_TEXT | TVIF_PARAM;

	TV_INSERTSTRUCT insStruct;
	insStruct.item = item;
	insStruct.hParent = hParent;
	insStruct.hInsertAfter = hInsertAfter;

	if ((hParent != __nullptr) && (hParent != TVI_ROOT))
	{
		CTLItem *pParent = (CTLItem *)CTreeCtrl::GetItemData(hParent);
		pParent->m_HasChildren = TRUE;
	}

	DeleteItem(hItem);

	return CTreeCtrl::InsertItem(&insStruct);
}
//-------------------------------------------------------------------------------------------------
BOOL CNewTreeListCtrl::SetItemText( HTREEITEM hItem, int nCol ,LPCTSTR lpszItem )
{
	CTLItem *pItem = (CTLItem *)CTreeCtrl::GetItemData(hItem);
	if (!pItem)
	{
		return FALSE;
	}

	pItem->SetSubstring(nCol, lpszItem);
	return CTreeCtrl::SetItemData(hItem, (LPARAM)pItem);
}
//-------------------------------------------------------------------------------------------------
BOOL CNewTreeListCtrl::SetItemColor( HTREEITEM hItem, COLORREF m_newColor, BOOL m_bInvalidate )
{
	CTLItem *pItem = (CTLItem *)CTreeCtrl::GetItemData(hItem);
	if (!pItem)
	{
		return FALSE;
	}

	pItem->m_Color = m_newColor;
	if (!CTreeCtrl::SetItemData(hItem, (LPARAM)pItem))
	{
		return FALSE;
	}

	if (m_bInvalidate)
	{
		Invalidate();
	}

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
CString CNewTreeListCtrl::GetItemText( HTREEITEM hItem, int nSubItem )
{
	CTLItem *pItem = (CTLItem *)CTreeCtrl::GetItemData(hItem);
	if (!pItem)
	{
		return "";
	}

	return pItem->GetSubstring(nSubItem);
}
//-------------------------------------------------------------------------------------------------
CString CNewTreeListCtrl::GetItemText( int nItem, int nSubItem )
{
	return GetItemText(GetTreeItem(nItem), nSubItem);
}
//-------------------------------------------------------------------------------------------------
BOOL CNewTreeListCtrl::DeleteItem( HTREEITEM hItem )
{
	HTREEITEM hOldParent = GetParentItem(hItem);

	CTLItem *pItem = (CTLItem *)CTreeCtrl::GetItemData(hItem);
	if (!pItem)
	{
		return FALSE;
	}

	delete pItem;

	int m_bReturn = CTreeCtrl::DeleteItem(hItem);

	if (m_bReturn)
	{
		if ((hOldParent != TVI_ROOT) && (hOldParent != __nullptr))
		{
			CTLItem *pOldParent = (CTLItem *)CTreeCtrl::GetItemData(hOldParent);
			pOldParent->m_HasChildren = ItemHasChildren(hOldParent);
		}
	}

	return m_bReturn;
}
//-------------------------------------------------------------------------------------------------
BOOL CNewTreeListCtrl::DeleteItem( int nItem )
{
	return DeleteItem(GetTreeItem(nItem));
}
//-------------------------------------------------------------------------------------------------
HTREEITEM CNewTreeListCtrl::FindParentItem(CString m_title, int nCol, HTREEITEM hItem, LPARAM itemData)
{
	// finds an item which has m_title at the nCol column
	// searches only parent items

	if (hItem == NULL)
	{
		hItem = GetRootItem();
	}

	if (itemData == 0)
	{
		while ((hItem!=NULL) && (GetItemText(hItem, nCol)!=m_title))
		{
			hItem = GetNextSiblingItem(hItem);
		}
	}
	else
	{
		while (hItem!=NULL)
		{ 
			if ((GetItemText(hItem, nCol) == m_title) && 
				((LPARAM)GetItemData(hItem) == itemData))
			{
				break;
			}

			hItem = GetNextSiblingItem(hItem);
		}
	}

	return hItem;
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::MemDeleteAllItems(HTREEITEM hParent)
{
	HTREEITEM hItem = hParent;
	CTLItem *pItem;

	while (hItem!=NULL)
	{
		pItem = (CTLItem *)CTreeCtrl::GetItemData(hItem);
		delete pItem;
		CTreeCtrl::SetItemData(hItem, NULL);

		if (ItemHasChildren(hItem))
		{
			MemDeleteAllItems(GetChildItem(hItem));
		}

		hItem = GetNextSiblingItem(hItem);
	}
}
//-------------------------------------------------------------------------------------------------
BOOL CNewTreeListCtrl::DeleteAllItems()
{
	BeginWaitCursor();

	HTREEITEM hTreeRoot = GetRootItem();
	// Select the root to prevent exception when items deleted
	SelectItem( hTreeRoot );
	MemDeleteAllItems(hTreeRoot);
	BOOL m_bReturn = CTreeCtrl::DeleteAllItems();

	EndWaitCursor();
	Invalidate();

	((CTLFrame*)GetParent())->ResetScrollBar();

	return m_bReturn;
}
//-------------------------------------------------------------------------------------------------
HTREEITEM CNewTreeListCtrl::AlterDropTarget(HTREEITEM /*hSource*/, HTREEITEM hTarget)
{
	// TODO: the following lines should be adjusted
	//       according to your project's needs

	if(hTarget==TVI_ROOT)
	{
		return TVI_ROOT;
	}

	if (ItemHasChildren(hTarget))
	{
		return hTarget;
	}
	else
	{
		HTREEITEM hParent = GetParentItem(hTarget);
		if (hParent != __nullptr)
		{
			return hParent;
		}
		else
		{
			return TVI_ROOT;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::OnTimer(UINT nIDEvent) 
{
	if (nIDEvent == m_idTimer)
	{
		m_toDrag = TRUE;
		KillTimer(m_idTimer);
/*		HTREEITEM htiFloat = GetDropHilightItem();
		if(htiFloat && htiFloat == m_htiDrop )
		{
			if(ItemHasChildren(htiFloat))
				Expand( htiFloat, TVE_EXPAND );
        }*/
	}
/*	else
	if(nIDEvent == m_scrollTimer)
	{
		m_timerticks++;

		POINT pt;
		GetCursorPos( &pt );
		RECT rect;
		GetClientRect( &rect );
		ClientToScreen( &rect );

		// NOTE: Screen coordinate is being used because the call
		// to DragEnter had used the Desktop window.
		CImageList::DragMove(pt);

		HTREEITEM hitem = GetFirstVisibleItem();

		if( pt.y < rect.top + 10 )
		{
			// We need to scroll up
			// Scroll slowly if cursor near the treeview control
			int slowscroll = 6 - (rect.top + 10 - pt.y) / 20;
			if( 0 == ( m_timerticks % (slowscroll > 0? slowscroll : 1) ) )
			{
				CImageList::DragShowNolock(FALSE);
				SendMessage( WM_VSCROLL, SB_LINEUP);
				SelectDropTarget(hitem);
				m_htiDrop = hitem;
				CImageList::DragShowNolock(TRUE);
			}
		}
		else if( pt.y > rect.bottom - 10 )
		{
			// We need to scroll down
			// Scroll slowly if cursor near the treeview control
			int slowscroll = 6 - (pt.y - rect.bottom + 10 ) / 20;
			if( 0 == ( m_timerticks % (slowscroll > 0? slowscroll : 1) ) )
			{
				CImageList::DragShowNolock(FALSE);
				SendMessage( WM_VSCROLL, SB_LINEDOWN);
				int nCount = GetVisibleCount();
				for ( int i=0; i<nCount-1; ++i )
					hitem = GetNextVisibleItem(hitem);
				if( hitem )
					SelectDropTarget(hitem);
				m_htiDrop = hitem;
				CImageList::DragShowNolock(TRUE);
			}
		}
	}
*/
	CTreeCtrl::OnTimer(nIDEvent);
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::OnLButtonUp(UINT nFlags, CPoint point) 
{
	CTreeCtrl::OnLButtonUp(nFlags, point);
	if (m_bLDragging)
	{
		CImageList::DragLeave(this);
		CImageList::EndDrag();

		ReleaseCapture();

		delete m_pDragImage;

		SelectDropTarget(NULL);
		m_htiOldDrop = NULL;

		if (m_htiDrag == m_htiDrop)
		{
			m_bLDragging = FALSE;
			return;
		}

		if (m_htiDrop == NULL)
		{
			Invalidate();
			m_bLDragging = FALSE;
			return;
		}

		if (m_htiDrop != TVI_ROOT)
		{
			HTREEITEM htiParent = m_htiDrop;
			while ((htiParent = GetParentItem(htiParent)) != __nullptr)
			{
				if (htiParent == m_htiDrag)
				{
					m_bLDragging = FALSE;
					return;
				}
			}
		}

		// please remove this line if you need to be able
		// to drop any item everywhere
		m_htiDrop = AlterDropTarget(m_htiDrag, m_htiDrop);

		HTREEITEM hDragParent = GetParentItem(m_htiDrag);
		HTREEITEM htiNew = MoveItem(m_htiDrag, m_htiDrop, TVI_SORT);
		SelectItem( htiNew );

		// please remove the following block too if it's not
		// relevant to your project
		{
			// remove the parent item, if there was one, 
			// and we dragged out of it its last child
			{
				if (hDragParent != __nullptr)
				{
					HTREEITEM hSecParent;
					do
					{
						hSecParent = GetParentItem(hDragParent);
						if (GetChildItem(hDragParent) == NULL) // no more children left
						{
							DeleteItem(hDragParent);
						}
						hDragParent = hSecParent;
					} while (hSecParent != __nullptr);
				}
			}
		}

		Expand(m_htiDrop, TVE_EXPAND);

		if (m_idTimer)
		{
			KillTimer( m_idTimer );
			m_idTimer = 0;
		}

		if (m_scrollTimer)
		{
			KillTimer( m_scrollTimer );
			m_scrollTimer = 0;
		}
	}

	// Get parent of parent
	CWnd* pGrandparent = GetParent()->GetParent();
	if (pGrandparent != __nullptr)
	{
		pGrandparent->SendMessage(WM_LBUTTONUP);
	}

	m_bLDragging = FALSE;
	m_toDrag = FALSE;
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::OnMouseMove(UINT nFlags, CPoint point) 
{
	CTreeCtrl::OnMouseMove(nFlags, point);

	HTREEITEM hti;
	UINT      flags;

	if ((!m_bLDragging) && (m_htiDrop!=NULL) && (m_toDrag))
	{
		if(nFlags & MK_LBUTTON)
		{
			Begindrag(point);
		}
	}
	else if ((!m_bLDragging) && (m_htiDrop == NULL))
	{
		m_htiDrop = TVI_ROOT;
	}
	else if (m_bLDragging)
	{
		POINT pt = point;
		ClientToScreen( &pt );
		CImageList::DragMove(pt);

		hti = HitTest(point,&flags);
//		if( hti != __nullptr )
		{
			CImageList::DragShowNolock(FALSE);

			if (m_htiOldDrop == NULL)
			{
				m_htiOldDrop = GetDropHilightItem();
			}

			SelectDropTarget(hti);

			if (hti != __nullptr)
			{
				m_htiDrop = hti;
			}
			else
			{
				m_htiDrop = TVI_ROOT;
			}

/*			if( m_idTimer && hti == m_htiOldDrop )
			{
				KillTimer( m_idTimer );
				m_idTimer = 0;
			}

			if (!m_idTimer)
			{
				m_idTimer = SetTimer( 1000, 1500, NULL );
			}*/

			CImageList::DragShowNolock(TRUE);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::Begindrag(CPoint /*point*/)
{
	// disabling drag for now...
	return;
/*
	UINT flags;
	m_htiDrag = HitTest(point, &flags);

	if (!((flags & TVHT_ONITEMRIGHT) || (flags & TVHT_ONITEMINDENT) ||
	   (flags & TVHT_ONITEM)))
	{
		m_htiDrag = NULL;
		return;
	}

	m_htiDrop = NULL;

	m_pDragImage = CreateDragImage( m_htiDrag );
	if (!m_pDragImage)
	{
		return;
	}

	m_bLDragging = TRUE;

	CPoint pt(0,0);

	IMAGEINFO ii;
	m_pDragImage->GetImageInfo( 0, &ii );
	pt.x = (ii.rcImage.right - ii.rcImage.left) / 2;
	pt.y = (ii.rcImage.bottom - ii.rcImage.top) / 2;

	m_pDragImage->BeginDrag( 0, pt );
	ClientToScreen(&point);
	m_pDragImage->DragEnter(NULL,point);

//	m_scrollTimer = SetTimer(1001, 75, NULL);

	SetCapture();
	*/
}
//-------------------------------------------------------------------------------------------------
void CNewTreeListCtrl::OnDestroy() 
{
	MemDeleteAllItems(GetRootItem());

	CTreeCtrl::OnDestroy();
}
//-------------------------------------------------------------------------------------------------
BOOL CNewTreeListCtrl::Expand(HTREEITEM hItem, UINT nCode)
{
	BOOL bReturn = CTreeCtrl::Expand(hItem, nCode);

	((CTLFrame*)GetParent())->ResetScrollBar();

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
