// TLFrame.cpp : implementation file
//

#include "stdafx.h"
#include "TLFrame.h"
#include "UCLIDException.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Static
//-------------------------------------------------------------------------------------------------
bool CTLFrame::ms_bIsRegistered = false;

//-------------------------------------------------------------------------------------------------
// CTLFrame
//-------------------------------------------------------------------------------------------------
CTLFrame::CTLFrame() :
  m_iStretchColumn(-1),
  m_bInitialized(false)
{
}
//-------------------------------------------------------------------------------------------------
CTLFrame::~CTLFrame()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16371");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTLFrame, CWnd)
	//{{AFX_MSG_MAP(CTLFrame)
	ON_WM_HSCROLL()
	ON_WM_CONTEXTMENU()
	ON_WM_SIZE()
	ON_WM_MOVE()
	ON_WM_LBUTTONDBLCLK()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CTLFrame message handlers
//-------------------------------------------------------------------------------------------------
LONG FAR PASCAL CTLFrame::DummyWndProc(HWND h, UINT u, WPARAM w, LPARAM l)
{
	return ::DefWindowProc(h, u, w, l);
}
//-------------------------------------------------------------------------------------------------
void CTLFrame::RegisterClass()
{

	if (!ms_bIsRegistered)
	{
		WNDCLASS wc;
		memset(&wc, 0, sizeof(wc));

		wc.style = CS_DBLCLKS | CS_VREDRAW | CS_HREDRAW | CS_GLOBALCLASS;
		wc.lpfnWndProc = DummyWndProc;
		wc.hInstance = AfxGetInstanceHandle();
		wc.hCursor = 0;
		wc.lpszClassName = "LANTIVTREELISTCTRL";
		wc.hbrBackground = (HBRUSH) GetStockObject(LTGRAY_BRUSH);

		if (!::RegisterClass(&wc))
		{
			ASSERT(FALSE);
		}
		ms_bIsRegistered = true;
	}
}
//-------------------------------------------------------------------------------------------------
BOOL CTLFrame::SubclassDlgItem(UINT nID, CWnd* parent)
{
	if (!CWnd::SubclassDlgItem(nID, parent)) 
	{
		return FALSE;
	}

	Initialize();
	
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CTLFrame::Initialize()
{
	// creates all the objects in frame -
	// header, tree, horizontal scroll bar

	CRect m_wndRect;
	GetWindowRect(&m_wndRect);
	CRect m_headerRect;

	// create the header
	{
		m_headerRect.left = m_headerRect.top = -1;
		m_headerRect.right = m_wndRect.Width();

		m_tree.m_wndHeader.Create( WS_CHILD | WS_VISIBLE | HDS_BUTTONS | HDS_HORZ, 
			m_headerRect, this, ID_TREE_LIST_HEADER );
	}

	CSize textSize;
	// set header's position and dimensions
	{
		LOGFONT logfont;

		CFont *pFont = GetParent()->GetFont();
		pFont->GetLogFont( &logfont );

		m_tree.m_headerFont.CreateFontIndirect( &logfont );
		m_tree.m_wndHeader.SetFont(&m_tree.m_headerFont);

		CDC *pDC = m_tree.m_wndHeader.GetDC();
		pDC->SelectObject(&m_tree.m_headerFont);
		textSize = pDC->GetTextExtent("A");

		m_tree.m_wndHeader.SetWindowPos( &wndTop, 0, 0, m_headerRect.Width(), 
			textSize.cy + 4, SWP_SHOWWINDOW );

		m_tree.m_wndHeader.UpdateWindow();
	}

	CRect m_treeRect;

	// create the tree itself
	{
		GetWindowRect(&m_wndRect);

		m_treeRect.left=0;
		m_treeRect.top = textSize.cy + 4;
		m_treeRect.right = m_headerRect.Width() - 5;
		m_treeRect.bottom = m_wndRect.Height() - GetSystemMetrics(SM_CYHSCROLL) - 4;

		m_tree.Create( WS_CHILD | WS_VISIBLE | TVS_HASLINES | TVS_LINESATROOT | 
			TVS_HASBUTTONS | TVS_SHOWSELALWAYS, m_treeRect, this, ID_TREE_LIST_CTRL );
	}

	// finally, create the horizontal scroll bar
	{
		CRect m_scrollRect;
		m_scrollRect.left=0;
//		m_scrollRect.top = m_treeRect.bottom;
		m_scrollRect.top = m_treeRect.bottom + 4;
		m_scrollRect.right = m_treeRect.Width() - GetSystemMetrics(SM_CXVSCROLL);
//		m_scrollRect.bottom = m_wndRect.bottom;
		m_scrollRect.bottom = m_wndRect.bottom + 4;

		m_horScrollBar.Create( WS_CHILD | WS_VISIBLE | WS_DISABLED | SBS_HORZ | SBS_TOPALIGN, 
			m_scrollRect, this, ID_TREE_LIST_SCROLLBAR );

		SCROLLINFO si;
		si.fMask = SIF_PAGE;
		si.nPage = m_treeRect.Width();
		m_horScrollBar.SetScrollInfo(&si, FALSE);
	}

	// Set flag
	m_bInitialized = true;
}
//-------------------------------------------------------------------------------------------------
BOOL CTLFrame::VerticalScrollVisible()
{
	int sMin, sMax;
	m_tree.GetScrollRange(SB_VERT, &sMin, &sMax);
	return (sMax != 0);
}
//-------------------------------------------------------------------------------------------------
BOOL CTLFrame::HorizontalScrollVisible()
{
	int sMin, sMax;
	m_horScrollBar.GetScrollRange(&sMin, &sMax);
	return (sMax != 0);
}
//-------------------------------------------------------------------------------------------------
int CTLFrame::StretchWidth(int m_nWidth, int m_nMeasure)
{
	return ((m_nWidth/m_nMeasure)+1)*m_nMeasure;
}
//-------------------------------------------------------------------------------------------------
void CTLFrame::ResetScrollBar()
{
	// resetting the horizontal scroll bar

	int m_nTotalWidth=0, m_nPageWidth;

	CRect m_treeRect;
	m_tree.GetClientRect(&m_treeRect);

	CRect m_wndRect;
	GetClientRect(&m_wndRect);

	CRect m_headerRect;
	m_tree.m_wndHeader.GetClientRect(&m_headerRect);

	CRect m_barRect;
	m_horScrollBar.GetClientRect(m_barRect);

	m_nPageWidth = m_treeRect.Width();

	m_nTotalWidth = m_tree.GetColumnsWidth();

	if(m_nTotalWidth > m_nPageWidth)
	{
		// show the scroll bar and adjust its size
		{
			m_horScrollBar.EnableWindow(TRUE);

			m_horScrollBar.ShowWindow(SW_SHOW);

			// the tree becomes smaller
			CRect TreeRect;
			m_tree.GetWindowRect(&TreeRect);
			if (TreeRect.Width() != m_wndRect.Width() || 
				TreeRect.Height() != m_wndRect.Height() - m_barRect.Height() - m_headerRect.Height())
			{
				m_tree.SetWindowPos( &wndTop, 0, 0, m_wndRect.Width(), 
					m_wndRect.Height() - m_barRect.Height() - m_headerRect.Height(), SWP_NOMOVE);
			}

			CRect ScrollRect;
			m_horScrollBar.GetWindowRect(&ScrollRect);

			if (!VerticalScrollVisible())
			{
				// Vertical scroll bar is not visible
				m_horScrollBar.SetWindowPos( &wndTop, 0, 0, m_wndRect.Width(), 
					m_barRect.Height(), SWP_NOMOVE );
			}
			else
			{
				m_horScrollBar.SetWindowPos( &wndTop, 0, 0, 
					m_wndRect.Width() - GetSystemMetrics(SM_CXVSCROLL), 
					m_barRect.Height(), SWP_NOMOVE);
			}
		}

		SCROLLINFO si;
		si.fMask = SIF_PAGE | SIF_RANGE;
		si.nPage = m_treeRect.Width();
		si.nMin = 0;
		si.nMax = m_nTotalWidth;
		m_horScrollBar.SetScrollInfo(&si, FALSE);

		// recalculate the offset
		{
			CRect m_wndHeaderRect;
			m_tree.m_wndHeader.GetWindowRect(&m_wndHeaderRect);
			ScreenToClient(&m_wndHeaderRect);

			m_tree.m_nOffset = m_wndHeaderRect.left;
			m_horScrollBar.SetScrollPos(-m_tree.m_nOffset);
		}
	}
	else
	{
		m_horScrollBar.EnableWindow(FALSE);

		// we no longer need it, so hide it!
		{
			m_horScrollBar.ShowWindow(SW_HIDE);

			// the tree takes scroll's place
			CRect TreeRect;
			m_tree.GetClientRect(&TreeRect);
			m_tree.SetWindowPos( &wndTop, 0, 0, m_wndRect.Width(), 
				m_wndRect.Height() - m_headerRect.Height(), SWP_NOMOVE );
		}

		m_horScrollBar.SetScrollRange(0, 0);

		// set scroll offset to zero
		{
			m_tree.m_nOffset = 0;
			m_tree.Invalidate();
			CRect m_headerRect;
			m_tree.m_wndHeader.GetWindowRect(&m_headerRect);
			CRect m_wndRect;
			GetClientRect(&m_wndRect);
			m_tree.m_wndHeader.SetWindowPos( &wndTop, m_tree.m_nOffset, 0, 
				max(StretchWidth(m_tree.GetColumnsWidth(), m_wndRect.Width()), m_wndRect.Width()), 
				m_headerRect.Height(), SWP_SHOWWINDOW );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CTLFrame::OnHScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar) 
{
	CRect m_treeRect;
	m_tree.GetClientRect(&m_treeRect);

	if (pScrollBar == &m_horScrollBar)
	{
		int m_nCurPos = m_horScrollBar.GetScrollPos();
		int m_nPrevPos = m_nCurPos;
		switch(nSBCode)
		{
			case SB_LEFT:			m_nCurPos = 0;
									break;
			case SB_RIGHT:			m_nCurPos = m_horScrollBar.GetScrollLimit()-1;
									break;
			case SB_LINELEFT:		m_nCurPos = max(m_nCurPos-6, 0);
									break;
			case SB_LINERIGHT:		m_nCurPos = min(m_nCurPos+6, m_horScrollBar.GetScrollLimit()-1);
									break;
			case SB_PAGELEFT:		m_nCurPos = max(m_nCurPos-m_treeRect.Width(), 0);
									break;
			case SB_PAGERIGHT:		m_nCurPos = min(m_nCurPos+m_treeRect.Width(), m_horScrollBar.GetScrollLimit()-1);
									break;
			case SB_THUMBTRACK:
			case SB_THUMBPOSITION:  if (nPos == 0)
									{
									    m_nCurPos = 0;
									}
								    else
									{
									    m_nCurPos = min( StretchWidth(nPos, 6), 
											m_horScrollBar.GetScrollLimit() - 1 );
									}
								    break;
		}
		// 6 is Microsoft's step in a CListCtrl for example

		m_horScrollBar.SetScrollPos(m_nCurPos);
		m_tree.m_nOffset = -m_nCurPos;

		// smoothly scroll the tree control
		{
			CRect m_scrollRect;
			m_tree.GetClientRect(&m_scrollRect);
			m_tree.ScrollWindow(m_nPrevPos - m_nCurPos, 0, &m_scrollRect, &m_scrollRect);
		}

		CRect m_headerRect;
		m_tree.m_wndHeader.GetWindowRect(&m_headerRect);
		CRect m_wndRect;
		GetClientRect(&m_wndRect);

		m_tree.m_wndHeader.SetWindowPos( &wndTop, m_tree.m_nOffset, 0, 
			max(StretchWidth(m_tree.GetColumnsWidth(), m_treeRect.Width()), m_wndRect.Width()), 
			m_headerRect.Height(), SWP_SHOWWINDOW );
	}
	
	CWnd::OnHScroll(nSBCode, nPos, pScrollBar);
}
//-------------------------------------------------------------------------------------------------
BOOL CTLFrame::OnNotify(WPARAM wParam, LPARAM lParam, LRESULT* pResult) 
{
	HD_NOTIFY *pHDN = (HD_NOTIFY*)lParam;

	if ((wParam == ID_TREE_LIST_HEADER) && (pHDN->hdr.code == HDN_ITEMCHANGED))
	{
		int m_nPrevColumnsWidth = m_tree.GetColumnsWidth();
		m_tree.RecalcColumnsWidth();
		ResetScrollBar();

		// in case we were at the scroll bar's end,
		// and some column's width was reduced,
		// update header's position (move to the right).
		CRect m_treeRect;
		m_tree.GetClientRect(&m_treeRect);

		CRect m_headerRect;
		m_tree.m_wndHeader.GetClientRect(&m_headerRect);

		if((m_nPrevColumnsWidth > m_tree.GetColumnsWidth()) &&
		   (m_horScrollBar.GetScrollPos() == m_horScrollBar.GetScrollLimit()-1) &&
		   (m_treeRect.Width() < m_tree.GetColumnsWidth()))
		{
			m_tree.m_nOffset = -m_tree.GetColumnsWidth()+m_treeRect.Width();
			m_tree.m_wndHeader.SetWindowPos(&wndTop, m_tree.m_nOffset, 0, 0, 0, SWP_NOSIZE);
		}

		m_tree.Invalidate();
	}
	else
	{
		GetParent()->SendMessage(WM_NOTIFY, wParam, lParam);
	}
	
	return CWnd::OnNotify(wParam, lParam, pResult);
}
//-------------------------------------------------------------------------------------------------
void CTLFrame::OnContextMenu(CWnd* pWnd, CPoint /*point*/) 
{
	GetParent()->SendMessage(WM_CONTEXTMENU, (WPARAM)pWnd, 0);
}
//-------------------------------------------------------------------------------------------------
void CTLFrame::OnSize(UINT nType, int cx, int cy) 
{
	CWnd::OnSize(nType, cx, cy);

	// resize all the controls
	DoResize();
}
//-------------------------------------------------------------------------------------------------
void CTLFrame::OnMove(int x, int y) 
{
	CWnd::OnMove(x, y);
	
	// resize all the controls
	DoResize();
}
//-------------------------------------------------------------------------------------------------
void CTLFrame::OnLButtonDblClk(UINT nFlags, CPoint point) 
{
	GetParent()->SendMessage(WM_LBUTTONDBLCLK);
	
	CWnd::OnLButtonDblClk(nFlags, point);
}
//-------------------------------------------------------------------------------------------------
void CTLFrame::DoResize() 
{
	if (m_bInitialized)
	{
		// resize all the controls
		{
			CRect m_wndRect;
			GetClientRect(&m_wndRect);

			CRect m_headerRect;
			m_tree.m_wndHeader.GetWindowRect(&m_headerRect);
			m_tree.ScreenToClient(&m_headerRect);
			m_tree.m_wndHeader.SetWindowPos( &wndTop, 0, 0, -m_headerRect.left + m_wndRect.Width(), 
				m_headerRect.Height(), SWP_NOMOVE );

			CRect m_scrollRect;
			m_horScrollBar.GetClientRect(&m_scrollRect);

			m_tree.SetWindowPos( &wndTop, 0, 0, m_wndRect.Width(), 
				m_wndRect.Height() - m_scrollRect.Height(), SWP_NOMOVE );

			CRect m_treeRect;
			m_tree.GetClientRect(&m_treeRect);
			m_horScrollBar.SetWindowPos( &wndTop, 0, m_treeRect.bottom, m_wndRect.Width(), 
				m_scrollRect.Height(), SWP_SHOWWINDOW );

			m_tree.ResetVertScrollBar();
			ResetScrollBar();

			// Adjust width of columns so that all are even
			long lCount = m_tree.GetColumnsNum();
			long lWidth = m_treeRect.Width() / lCount;
			long lTotalWidth = 0;
			for (int i = 0; i < lCount; i++)
			{
				if (m_iStretchColumn == -1)
				{
					m_tree.SetColumnWidth( i, lWidth );
				}
				else
				{
					// Update total width of other columns
					if (i != m_iStretchColumn)
					{
						lTotalWidth += m_tree.GetColumnWidth( i );
					}
				}
			}

			// Remaining width is for the stretchable column
			if (m_iStretchColumn != -1)
			{
				// Get current width and proposed new width
				long lCurrent = m_tree.GetColumnWidth( m_iStretchColumn );
				long lNewWidth = max( 0, m_treeRect.Width() - lTotalWidth );

				// Adjust the width only if the change >= scroll bar width
				if (abs( lCurrent - lNewWidth ) >= GetSystemMetrics(SM_CXVSCROLL))
				{
					m_tree.SetColumnWidth( m_iStretchColumn, lNewWidth );
				}
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CTLFrame::SetStretchColumn(int iColumn) 
{
	if ((iColumn >= 0) && (iColumn < m_tree.GetColumnsNum()))
	{
		m_iStretchColumn = iColumn;
	}
}
//-------------------------------------------------------------------------------------------------
