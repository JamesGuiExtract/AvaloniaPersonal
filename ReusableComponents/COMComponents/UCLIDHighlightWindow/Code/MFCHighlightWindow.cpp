
#include "stdafx.h"
#include "resource.h"
#include "MFCHighlightWindow.h"

#include <TemporaryResourceOverride.h>
#include <ExtractMFCUtils.h>
#include <UCLIDException.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

const int iLARGE_BORDER_SIZE = 8;
const int iSMALL_BORDER_SIZE = 4;

extern CComModule _Module;

//--------------------------------------------------------------------------------------------------
MFCHighlightWindow::MFCHighlightWindow(CWnd* pParent /*=NULL*/)
	: CDialog(MFCHighlightWindow::IDD, pParent), m_bVisible(false),
	m_hWndLastParent(NULL), m_hWndLastChild(NULL), 
	m_eHighlightType(kHighlightUsingBorder)
{
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	//{{AFX_DATA_INIT(HighlightWindow)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT

	// set the default color to yellow
	setColor(RGB(255, 255, 0));

	// this dialog auto-creates itself...
	Create(IDD, pParent);

	// by default show the window
	ShowWindow(SW_SHOW);
}
//--------------------------------------------------------------------------------------------------
void MFCHighlightWindow::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(HighlightWindow)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(MFCHighlightWindow, CDialog)
	//{{AFX_MSG_MAP(MFCHighlightWindow)
	ON_WM_CTLCOLOR()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------
BOOL MFCHighlightWindow::OnInitDialog() 
{
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	CDialog::OnInitDialog();

	// NOTE: following code below has been commented out because using
	// the HighlightWindow in transparent mode has been very problematic
	/*
	// by default, make this window transparent if possible
	if (windowTransparencyIsSupported())
	{
		makeWindowTransparent(this, true);
		m_eHighlightType = kHighlightUsingTransparency;
	}
	*/

	// by default, the window is so small that it is not visible
	SetWindowPos(&wndTopMost, 0, 0, 0, 0, 0);

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
void MFCHighlightWindow::showAsTransparentHighlight(HWND hParentWnd, HWND hChildWnd)
{
	CRect rectParentWnd;
	::GetWindowRect(hParentWnd, &rectParentWnd);

	MoveWindow(rectParentWnd.left, rectParentWnd.top, 
		rectParentWnd.Width(), rectParentWnd.Height(), TRUE);

	if (hChildWnd != NULL)
	{
		// Set this window's region to be what it maximally can be except
		// for a rectangular hole around the child window
		CRect rectChildWnd;
		::GetWindowRect(hChildWnd, &rectChildWnd);

		CRgn crRgn, crTmp;
		crRgn.CreateRectRgn(0, 0, 0, 0);

		crTmp.CreateRectRgn(0, 0, rectParentWnd.Width(),
			rectChildWnd.top - rectParentWnd.top);
		crRgn.CombineRgn(&crRgn, &crTmp, RGN_OR);
		crTmp.DeleteObject();

		crTmp.CreateRectRgn(0, rectChildWnd.top - rectParentWnd.top, 
			rectChildWnd.left - rectParentWnd.left,
			rectChildWnd.bottom - rectParentWnd.top);
		crRgn.CombineRgn(&crRgn, &crTmp, RGN_OR);
		crTmp.DeleteObject();

		crTmp.CreateRectRgn(rectChildWnd.right - rectParentWnd.left, 
			rectChildWnd.top - rectParentWnd.top, rectParentWnd.right,
			rectChildWnd.bottom - rectParentWnd.top);
		crRgn.CombineRgn(&crRgn, &crTmp, RGN_OR);
		crTmp.DeleteObject();

		crTmp.CreateRectRgn(0, 
			rectChildWnd.bottom - rectParentWnd.top, rectParentWnd.Width(),
			rectParentWnd.bottom);
		crRgn.CombineRgn(&crRgn, &crTmp, RGN_OR);
		crTmp.DeleteObject();

		SetWindowRgn(crRgn, TRUE);
		crRgn.DeleteObject();
	}
	else
	{
		// Set this window's region to be what it maximally can be.
		CRgn crRgn;
		crRgn.CreateRectRgn(0, 0, 
			rectParentWnd.Width(), rectParentWnd.Height());

		SetWindowRgn(crRgn, TRUE);
		crRgn.DeleteObject();
	}
}
//--------------------------------------------------------------------------------------------------
void MFCHighlightWindow::addParentWindowBorderToRegion(CRgn& rRegion, HWND hWnd)
{
	CRgn crTmp;

	CRect rect;
	::GetWindowRect(hWnd, &rect);

	crTmp.CreateRectRgn(0, 0, rect.Width() + iLARGE_BORDER_SIZE * 2,
		iLARGE_BORDER_SIZE);
	rRegion.CombineRgn(&rRegion, &crTmp, RGN_OR);
	crTmp.DeleteObject();

	crTmp.CreateRectRgn(0, iLARGE_BORDER_SIZE, 
		iLARGE_BORDER_SIZE, rect.Height() + iLARGE_BORDER_SIZE * 2);
	rRegion.CombineRgn(&rRegion, &crTmp, RGN_OR);
	crTmp.DeleteObject();

	crTmp.CreateRectRgn(rect.Width() + iLARGE_BORDER_SIZE, 
		iLARGE_BORDER_SIZE, rect.Width() + iLARGE_BORDER_SIZE * 2,
		rect.Height() + iLARGE_BORDER_SIZE * 2);
	rRegion.CombineRgn(&rRegion, &crTmp, RGN_OR);
	crTmp.DeleteObject();

	crTmp.CreateRectRgn(0, rect.Height() + iLARGE_BORDER_SIZE,
		rect.Width() + iLARGE_BORDER_SIZE * 2,
		rect.Height() + iLARGE_BORDER_SIZE * 2);
	rRegion.CombineRgn(&rRegion, &crTmp, RGN_OR);
	crTmp.DeleteObject();
}
//--------------------------------------------------------------------------------------------------
void MFCHighlightWindow::addChildWindowBorderToRegion(CRgn& rRegion, HWND hParentWnd,
												   HWND hChildWnd)
{
	CRgn crTmp;

	CRect rectParent, rectChild;
	::GetWindowRect(hParentWnd, &rectParent);
	::GetWindowRect(hChildWnd, &rectChild);

	long lChildActualLeft = rectChild.left - rectParent.left + iLARGE_BORDER_SIZE;
	long lChildActualTop = rectChild.top - rectParent.top + iLARGE_BORDER_SIZE;

	crTmp.CreateRectRgn(lChildActualLeft - iSMALL_BORDER_SIZE,
		lChildActualTop - iSMALL_BORDER_SIZE,
		lChildActualLeft + rectChild.Width() + iSMALL_BORDER_SIZE,
		lChildActualTop );
	rRegion.CombineRgn(&rRegion, &crTmp, RGN_OR);
	crTmp.DeleteObject();

	crTmp.CreateRectRgn(lChildActualLeft - iSMALL_BORDER_SIZE, lChildActualTop, 
		lChildActualLeft, lChildActualTop + rectChild.Height());
	rRegion.CombineRgn(&rRegion, &crTmp, RGN_OR);
	crTmp.DeleteObject();

	crTmp.CreateRectRgn(lChildActualLeft + rectChild.Width(), lChildActualTop, 
		lChildActualLeft + rectChild.Width() + iSMALL_BORDER_SIZE, 
		lChildActualTop + rectChild.Height());
	rRegion.CombineRgn(&rRegion, &crTmp, RGN_OR);
	crTmp.DeleteObject();

	crTmp.CreateRectRgn(lChildActualLeft - iSMALL_BORDER_SIZE, 
		lChildActualTop + rectChild.Height(),
		lChildActualLeft + rectChild.Width() + iSMALL_BORDER_SIZE,
		lChildActualTop + rectChild.Height() + iSMALL_BORDER_SIZE);
	rRegion.CombineRgn(&rRegion, &crTmp, RGN_OR);
	crTmp.DeleteObject();
}
//--------------------------------------------------------------------------------------------------
void MFCHighlightWindow::showAsWindowBorder(HWND hParentWnd, HWND hChildWnd)
{
	CRect rectParentWnd;
	::GetWindowRect(hParentWnd, &rectParentWnd);

	MoveWindow(rectParentWnd.left - iLARGE_BORDER_SIZE, 
		rectParentWnd.top - iLARGE_BORDER_SIZE, 
		rectParentWnd.Width() + iLARGE_BORDER_SIZE * 2, 
		rectParentWnd.Height() + iLARGE_BORDER_SIZE * 2, TRUE);

	CRgn crRgn;
	crRgn.CreateRectRgn(0, 0, 0, 0);

	// add border around parent window and child window (if applicable) to region
	addParentWindowBorderToRegion(crRgn, hParentWnd);
	if (hChildWnd != NULL)
		addChildWindowBorderToRegion(crRgn, hParentWnd, hChildWnd);
	
	SetWindowRgn(crRgn, TRUE);
	crRgn.DeleteObject();
}
//--------------------------------------------------------------------------------------------------
void MFCHighlightWindow::show(HWND hParentWnd, HWND hChildWnd)
{
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// ensure that the parent window is not NULL
	if (hParentWnd == NULL)
	{
		throw UCLIDException("ELI03976", "Parent window cannot be NULL!");
	}

	// if this window has already been destroyed, then ignore the
	// call to show
	if (!IsWindow(m_hWnd))
		return;

	// show the highlight window differently depending upon whether
	// transparency is supported by the underlying operating system
	if (m_eHighlightType == kHighlightUsingTransparency)
	{
		showAsTransparentHighlight(hParentWnd, hChildWnd);
	}
	else
	{
		showAsWindowBorder(hParentWnd, hChildWnd);
	}

	// update the last parent, child window handles
	m_hWndLastParent = hParentWnd;
	m_hWndLastChild = hChildWnd;

	// TBD
	// ::ShowWindow(m_hWnd, SW_SHOW);
	SetWindowPos(&wndTopMost, 0,0,0,0, SWP_NOMOVE + SWP_NOSIZE + SWP_NOACTIVATE +
		SWP_SHOWWINDOW);
	::SetWindowPos(m_hWndLastParent, HWND_NOTOPMOST, 0,0,0,0, SWP_NOMOVE + 
		SWP_NOSIZE);

	m_bVisible = true;
}
//--------------------------------------------------------------------------------------------------
void MFCHighlightWindow::show(CWnd *pParentWnd, CWnd *pChildWnd)
{
	// just call the other Show method.
	show(pParentWnd ? pParentWnd->m_hWnd : NULL,
		 pChildWnd ? pChildWnd->m_hWnd : NULL);
}
//--------------------------------------------------------------------------------------------------
void MFCHighlightWindow::hide(bool bRememberWindows)
{
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	if (!bRememberWindows)
	{
		m_hWndLastChild = m_hWndLastParent = NULL;
	}

	// TBD
	// to hide the window, just make it really small.
	//::MoveWindow(m_hWnd, 0, 0, 1, 1, TRUE);
	//::ShowWindow(m_hWnd, SW_HIDE);
	ShowWindow(SW_HIDE);

	m_bVisible = false;
}
//--------------------------------------------------------------------------------------------------
bool MFCHighlightWindow::isVisible() const
{
	return m_bVisible;
}
//--------------------------------------------------------------------------------------------------
HBRUSH MFCHighlightWindow::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor) 
{
	return m_pBrush;
}
//--------------------------------------------------------------------------------------------------
void MFCHighlightWindow::setColor(COLORREF color) 
{
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// NOTE: if the following two lines are moved inside the main if-block
	// (for optimization purposes), the code works as expected in debug mode,
	// but in release mode, the window does not refresh as expected !!!
	m_pBrush.DeleteObject();
	m_pBrush.CreateSolidBrush(color);

	if (m_BrushColor != color)
	{
		m_BrushColor = color;

		if (IsWindow(m_hWnd))
			RedrawWindow();
	}
}
//--------------------------------------------------------------------------------------------------
void MFCHighlightWindow::refresh()
{
	// if there was no last parent window, then just return
	if (m_hWndLastParent == NULL)
		return;

	show(m_hWndLastParent, m_hWndLastChild);
}
//--------------------------------------------------------------------------------------------------
bool MFCHighlightWindow::windowTransparencyIsSupported() const
{
	return ::windowTransparencyIsSupported();
}
//--------------------------------------------------------------------------------------------------
void MFCHighlightWindow::setHighlightType(EHighlightType eHighlightType)
{
	// if the current highlight type is same as the new highlight type
	// just return
	if (m_eHighlightType == eHighlightType)
		return;

	// if the user wants to change the highlight type to use transparency
	// make sure transparency is supported
	if (eHighlightType == kHighlightUsingTransparency && 
		!windowTransparencyIsSupported())
	{
		throw UCLIDException("ELI03981", "Window transparency is not supported!");
	}

	// set the window transparency depending upon the requested
	// highlight type
	if (eHighlightType == kHighlightUsingBorder)
	{
		makeWindowTransparent(this, false);
	}
	else
	{
		makeWindowTransparent(this, true);
	}

	// update the highlight type
	m_eHighlightType = eHighlightType;
	refresh();
}
//--------------------------------------------------------------------------------------------------
