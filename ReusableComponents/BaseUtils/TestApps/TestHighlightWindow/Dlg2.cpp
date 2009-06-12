// Dlg2.cpp : implementation file
//

#include "stdafx.h"
#include "TestHighlightWindow.h"
#include "Dlg2.h"
#include <HighlightWindow.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// Dlg2 dialog


Dlg2::Dlg2(CWnd* pParent /*=NULL*/)
	: CDialog(Dlg2::IDD, pParent)
{
	//{{AFX_DATA_INIT(Dlg2)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}


void Dlg2::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(Dlg2)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(Dlg2, CDialog)
	//{{AFX_MSG_MAP(Dlg2)
	ON_WM_MOVE()
	ON_WM_SIZE()
	ON_WM_ACTIVATE()
	ON_EN_SETFOCUS(IDC_EDIT1, OnSetfocusEdit1)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

void Dlg2::OnActivate( UINT nState, CWnd* pWndOther, BOOL bMinimized )
{
	if (nState == WA_ACTIVE  || nState == WA_CLICKACTIVE)
	{
		HighlightWindow::sGetInstance().show(this, NULL);
		HighlightWindow::sGetInstance().setDefaultColor();
	}
}

void Dlg2::OnMove(int x, int y) 
{
	CDialog::OnMove(x, y);
	
	HighlightWindow::sGetInstance().refresh();
}

void Dlg2::OnSize(UINT nType, int cx, int cy) 
{
	CDialog::OnSize(nType, cx, cy);
	
	HighlightWindow::sGetInstance().refresh();
}

void Dlg2::OnSetfocusEdit1() 
{
	HighlightWindow::sGetInstance().show(this, GetDlgItem(IDC_EDIT1));
	HighlightWindow::sGetInstance().setDefaultColor();
}

LRESULT Dlg2::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam) 
{
	LRESULT result = CDialog::DefWindowProc(message, wParam, lParam);;
	
	if (message == WM_EXITSIZEMOVE)
	{
		HighlightWindow::sGetInstance().refresh();
	}
	
	return result;
}
