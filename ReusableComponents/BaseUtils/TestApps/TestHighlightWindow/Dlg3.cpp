// Dlg3.cpp : implementation file
//

#include "stdafx.h"
#include "TestHighlightWindow.h"
#include "Dlg3.h"
#include <HighlightWindow.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern COLORREF DISABLED_COLOR;

/////////////////////////////////////////////////////////////////////////////
// Dlg3 dialog


Dlg3::Dlg3(CWnd* pParent /*=NULL*/)
	: CDialog(Dlg3::IDD, pParent)
{
	//{{AFX_DATA_INIT(Dlg3)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}


void Dlg3::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(Dlg3)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(Dlg3, CDialog)
	//{{AFX_MSG_MAP(Dlg3)
	ON_WM_ACTIVATE()
	ON_WM_MOVE()
	ON_WM_SIZE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

void Dlg3::OnActivate( UINT nState, CWnd* pWndOther, BOOL bMinimized )
{
	CDialog::OnActivate(nState, pWndOther, bMinimized);

	if (nState == WA_INACTIVE)
	{
		if (::IsWindowVisible(m_hWnd))
			Beep(1000, 100);
		else
			Beep(500, 100);
	}

	if (nState == WA_ACTIVE  || nState == WA_CLICKACTIVE)
	{
		HighlightWindow::sGetInstance().show(this, NULL);
		HighlightWindow::sGetInstance().setColor(DISABLED_COLOR);
	}
}

void Dlg3::OnMove(int x, int y) 
{
	CDialog::OnMove(x, y);
	
	HighlightWindow::sGetInstance().refresh();
}

void Dlg3::OnSize(UINT nType, int cx, int cy) 
{
	CDialog::OnSize(nType, cx, cy);
	
	HighlightWindow::sGetInstance().refresh();
}

LRESULT Dlg3::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam) 
{
	LRESULT result = CDialog::DefWindowProc(message, wParam, lParam);;
	
	if (message == WM_EXITSIZEMOVE)
	{
		HighlightWindow::sGetInstance().refresh();
	}
	
	return result;
}
