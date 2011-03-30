//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SplashWindow.cpp
//
// PURPOSE:	
//
// NOTES:	This code was originally downloaded from AutoDesk's ADN site, and was modified
//			thereafter.  Some copyrights of AutoDesk may still apply to this code.
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "resource.h"
#include "SplashWindow.h"
#include "UCLIDException.h"

// static/global variables
const int SplashWindow::iDISPLAY_DURATION = 3000;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SplashWindow, CWnd)
//{{AFX_MSG_MAP(SplashWindow)
ON_WM_CREATE()
ON_WM_PAINT()
ON_WM_TIMER()
//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
SplashWindow::SplashWindow(CBitmap *pBitmap)
:m_pBitmap(pBitmap)
{
}
//-----------------------------------------------------------------------------
SplashWindow::~SplashWindow() 
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16398");
}
//-----------------------------------------------------------------------------
void SplashWindow::sShowSplashScreen(CBitmap *pBitmap, CWnd *pParentWnd) 
{
	//----- Allocate a new splash screen, and create the window.
	SplashWindow* pSplashWindow = new SplashWindow(pBitmap);
	if (!pSplashWindow->Create(pParentWnd))
		delete pSplashWindow ;
	else
	{
		pSplashWindow->UpdateWindow();
		Sleep(iDISPLAY_DURATION);
	}
}
//-----------------------------------------------------------------------------
BOOL SplashWindow::Create(CWnd *pParentWnd) 
{
	BITMAP bm;
	m_pBitmap->GetBitmap(&bm);

	return CreateEx(
		0, AfxRegisterWndClass (0, AfxGetApp ()->LoadStandardCursor(IDC_ARROW)), NULL, 
		WS_POPUP | WS_VISIBLE, 0, 0, bm.bmWidth, bm.bmHeight, 
		pParentWnd->GetSafeHwnd(), NULL);
}
//-----------------------------------------------------------------------------
void SplashWindow::HideSplashScreen () 
{
	DestroyWindow();
	if (AfxGetMainWnd() != __nullptr)
		AfxGetMainWnd()->UpdateWindow();
}
//-----------------------------------------------------------------------------
void SplashWindow::PostNcDestroy () 
{
	//----- Free the C++ class.
	delete this;
}
//-----------------------------------------------------------------------------
int SplashWindow::OnCreate (LPCREATESTRUCT lpCreateStruct) 
{
	if (CWnd::OnCreate(lpCreateStruct) == -1)
		return -1;

	CenterWindow();

	SetTimer(1, iDISPLAY_DURATION, NULL);

	return 0;
}
//-----------------------------------------------------------------------------
void SplashWindow::OnPaint() 
{
	CPaintDC dc(this);
	CDC dcImage;
	if (!dcImage.CreateCompatibleDC(&dc))
		return;
	
	BITMAP bm;
	m_pBitmap->GetBitmap(&bm) ;

	//----- Paint the image.
	CBitmap *pOldBitmap = dcImage.SelectObject(m_pBitmap);
	dc.BitBlt(0, 0, bm.bmWidth, bm.bmHeight, &dcImage, 0, 0, SRCCOPY);
	dcImage.SelectObject(pOldBitmap);
}
//-----------------------------------------------------------------------------
void SplashWindow::OnTimer(UINT /*nIDEvent*/)
{
	HideSplashScreen();
}
//-----------------------------------------------------------------------------
