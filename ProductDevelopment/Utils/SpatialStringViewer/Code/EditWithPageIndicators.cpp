// EditWithPageIndicators.cpp : implementation file
//
#include "stdafx.h"
#include "SpatialStringViewer.h"
#include "PageRulerStatic.h"
#include "EditWithPageIndicators.h"
#include "UCLIDException.h"
#include "cpputil.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CEditWithPageIndicators
//-------------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(CEditWithPageIndicators, CEdit)

CEditWithPageIndicators::CEditWithPageIndicators()
: m_ipSpatialString(__nullptr)
{

}
//--------------------------------------------------------------------------------------------------
CEditWithPageIndicators::~CEditWithPageIndicators()
{
	m_ipSpatialString = __nullptr;
}

//--------------------------------------------------------------------------------------------------
// Message Map
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CEditWithPageIndicators, CEdit)
	ON_CONTROL_REFLECT(EN_CHANGE, OnChange)
	ON_WM_VSCROLL()
	ON_CONTROL_REFLECT(EN_VSCROLL, OnVscroll)
	ON_WM_SIZE()
	ON_MESSAGE(WM_SETTEXT, OnSetText)
	ON_MESSAGE(EM_LINESCROLL, OnLineScroll)	
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// CEditWithPageIndicators message handlers
//--------------------------------------------------------------------------------------------------
void CEditWithPageIndicators::OnChange()
{
	try
	{
		UpdatePageRuler();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36364");
}
//--------------------------------------------------------------------------------------------------
void CEditWithPageIndicators::OnVscroll()
{
	try
	{
		UpdatePageRuler();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36365");
}
//--------------------------------------------------------------------------------------------------
void CEditWithPageIndicators::OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{
	try
	{
		CEdit::OnVScroll(nSBCode, nPos, pScrollBar);
		UpdatePageRuler();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36366");
}
//--------------------------------------------------------------------------------------------------
void CEditWithPageIndicators::OnSize(UINT nType, int cx, int cy)
{
	try
	{
		CEdit::OnSize(nType, cx, cy);
		if (m_ruler.m_hWnd)
		{
			PrepareRuler();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36367");
}
//--------------------------------------------------------------------------------------------------
LRESULT CEditWithPageIndicators::OnSetText(WPARAM wParam, LPARAM lParam)
{
	LRESULT retValue = DefWindowProc(WM_SETTEXT, wParam, lParam);
	try
	{
		if ( m_ruler.m_hWnd == __nullptr)
		{
			PrepareRuler();
		}

		UpdatePageRuler();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36368");

	return retValue;
}
//-------------------------------------------------------------------------------------------------
LRESULT CEditWithPageIndicators::OnLineScroll(WPARAM wParam, LPARAM lParam)
{
	LRESULT retValue = DefWindowProc(EM_LINESCROLL, wParam, lParam);
	try
	{
		UpdatePageRuler();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI36369");

	return retValue;
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
int CEditWithPageIndicators::GetLastVisibleLine()
{
	int firstVisibleLine = GetFirstVisibleLine();
	int firstPos = LineIndex(firstVisibleLine);
	int lineCount = GetLineCount();
	
	CDC* pDC = GetDC();
	int savedDC = pDC->SaveDC();
	RECT clientRect, testRect;
	GetClientRect(&clientRect);
	GetRect(&testRect);
	TEXTMETRIC textMetrics;
		
	pDC->SelectObject(GetFont());
	pDC->GetTextMetrics(&textMetrics);
	
	pDC->RestoreDC(savedDC);
	ReleaseDC(pDC);

	long lineHeight =  textMetrics.tmHeight;
	long displayedLines = min( clientRect.bottom / lineHeight, lineCount - firstVisibleLine);

	return (displayedLines <= 1) ? firstVisibleLine : firstVisibleLine + displayedLines - 1;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CEditWithPageIndicators::UpdatePageRuler()
{
	try
	{
		// Determine the location of the page breaks
		PageLocationMap pageLocations;

		int nFirstPos, nLastPos;

		int nLineCount = GetLineCount();
		if (nLineCount < 1 )
		{
			m_ruler.SetLinePageLocations(pageLocations, 0);
			return;
		}

		int nFirstVisibleLine = GetFirstVisibleLine();
		nFirstPos = LineIndex(nFirstVisibleLine);

		int nLastVisibleLine = GetLastVisibleLine();

        // Get the first character of the last line since just interested in the page number
		// all the characters on the last line will have the same page number
		nLastPos =  LineIndex(nLastVisibleLine);

		// https://extract.atlassian.net/browse/ISSUE-12008
		// getPageAtPos will look for the next spatial letter starting at nLastPos... if nLastPos is
		// not spatial and there are no subsequent spatial letters, keep moving to previous lines
		// until one constains a spatial letter.
		CString szText;
		GetWindowText(szText);
		string strText = (LPCTSTR)szText;
		while (nLastPos != nFirstPos && !containsNonWhitespaceChars(strText.substr(nLastPos)))
		{
			nLastVisibleLine--;
			nLastPos =  LineIndex(nLastVisibleLine);
		}
	
		int nCurrPage = getPageAtPos(nFirstPos);
		int nTopPage = nCurrPage;
		int nLastPage = getPageAtPos(nLastPos);
		
		int nNextPagePos = m_ipSpatialString->GetFirstCharPositionOfPage(nCurrPage+1);
	
		if (nCurrPage < 0 || nCurrPage == nLastPage ||  nNextPagePos == 0)
		{
			m_ruler.SetLinePageLocations(pageLocations, nTopPage);
			return;
		}
		
		// Only get the locations for the displayed pages
		while ( nNextPagePos <= nLastPos && nNextPagePos > 0 && nCurrPage < nLastPage)
		{
			nCurrPage++;
			pageLocations[nCurrPage] = LineFromChar(nNextPagePos) - nFirstVisibleLine;
			nNextPagePos = m_ipSpatialString->GetFirstCharPositionOfPage(nCurrPage + 1);
		}
		m_ruler.SetLinePageLocations(pageLocations, nTopPage);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36370");
}
//-------------------------------------------------------------------------------------------------
void CEditWithPageIndicators::PrepareRuler()
{
	try
	{
		int width = 40;
		CRect rect;
		GetClientRect( &rect );
		CRect rectEdit( rect );
		rect.right = width;
		rectEdit.left = rect.right + 1;

		// Setting the edit rect and 
		// creating or moving child control
		SetRect( &rectEdit );
		if( m_ruler.m_hWnd )
		{
			m_ruler.MoveWindow( 0, 0, width, rect.Height() );
		}
		else
		{
			m_ruler.Create(NULL,WS_CHILD | WS_VISIBLE | SS_NOTIFY, rect, this, 1 );
		}

	
		UpdatePageRuler();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36371");
}
//-------------------------------------------------------------------------------------------------
long CEditWithPageIndicators::getPageAtPos(long position)
{
	try
	{
		long pageNumber = -1;
		if (m_ipSpatialString->GetMode() != kSpatialMode)
		{
			return pageNumber;
		}
		ILetterPtr ipLetter = m_ipSpatialString->GetOCRImageLetter(position);
		if (ipLetter != __nullptr)
		{
			if (ipLetter->IsSpatialChar == VARIANT_FALSE)
			{
				m_ipSpatialString->GetNextOCRImageSpatialLetter(position, &ipLetter);
			}
			if (ipLetter != __nullptr)
			{
				pageNumber = ipLetter->GetPageNumber();
			}
		}
		return pageNumber;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36372");
}
//-------------------------------------------------------------------------------------------------
