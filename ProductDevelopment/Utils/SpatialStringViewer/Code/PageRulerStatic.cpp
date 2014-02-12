// PageRulerStatic.cpp : implementation file
//

#include "stdafx.h"
#include "SpatialStringViewer.h"
#include "PageRulerStatic.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// CPageRulerStatic
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CPageRulerStatic, CStatic)

CPageRulerStatic::CPageRulerStatic()
: m_nTopPage(0)
{

}
//-------------------------------------------------------------------------------------------------
CPageRulerStatic::~CPageRulerStatic()
{
}

//-------------------------------------------------------------------------------------------------
// Message Map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPageRulerStatic, CStatic)
	ON_WM_ERASEBKGND()
	ON_WM_PAINT()
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CPageRulerStatic message handlers
//-------------------------------------------------------------------------------------------------
BOOL CPageRulerStatic::OnEraseBkgnd(CDC* pDC)
{
	// TODO: Add your message handler code here and/or call default

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CPageRulerStatic::OnPaint()
{
	try
	{
		CPaintDC dcPaint(this); // device context for painting

		CRect rect;
		GetClientRect( &rect );

		// We double buffer the drawing - 
		// preparing the memory CDC
		CDC dc;
		dc.CreateCompatibleDC( &dcPaint );
		int saved = dc.SaveDC();

		// Create GDI and select objects
		CBitmap bmp;
		CPen pen;
		bmp.CreateCompatibleBitmap( &dcPaint, rect.Width(), rect.Height() );
		pen.CreatePen( PS_SOLID, 1, RGB(0,0,0) );
		dc.SelectObject( &bmp );
		dc.SelectObject( &pen );

		// Painting the background
		dc.FillSolidRect( &rect, RGB(255,255,255) );
		dc.MoveTo( rect.right - 1, 0 );
		dc.LineTo( rect.right - 1, rect.bottom );

		// Setting other attributes
		dc.SetTextColor( RGB(0,0,0) );
		dc.SetBkColor(  RGB(255,255,255) );
		dc.SelectObject( GetParent()->GetFont() );

		TEXTMETRIC tm;
		dc.GetTextMetrics(&tm);

		// Only want to display the top and bottom page number if set > 0
		bool bAddTop = m_nTopPage > 0;

		// Set the positions of the top and bottom lines that will be displayed
		int nTopPos = 0;
	
		// Draw lines at the locations of the page break in the map
		for (PageLocationMap::iterator pg = m_mapOfPageLocations.begin();
			pg != m_mapOfPageLocations.end(); ++pg)
		{
			long pageNumber = pg->first;
			long lineNumber = pg->second;

			long linePos = tm.tmHeight * lineNumber;
			linePos = linePos + tm.tmHeight;

			// If the top and bottom location are the same as one of the page transitions
			// do not display
			bAddTop = bAddTop && (linePos != nTopPos);

			CString str;
			str.Format("%5li", pageNumber);
			dc.TextOut(2, linePos, str);

			dc.MoveTo(0, linePos);
			dc.LineTo(rect.right -1, linePos);
		}

		// Only add the top page number if not already displayed
		if (bAddTop)
		{
			CString str;
			str.Format("%5li", m_nTopPage);
			dc.TextOut(2, nTopPos, str);
		}

		dcPaint.BitBlt( 0, 0, rect. right, rect.bottom, &dc, 0, 0, SRCCOPY );
		dc.RestoreDC( saved );
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36395");
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void CPageRulerStatic::SetLinePageLocations(const PageLocationMap mapOfPageLocations, int nTop)
{
	m_mapOfPageLocations = mapOfPageLocations;
	m_nTopPage = nTop;

	// Redraw the screen if the window handle has been set
	if (m_hWnd)
	{
		RedrawWindow();
	}
}
//-------------------------------------------------------------------------------------------------
