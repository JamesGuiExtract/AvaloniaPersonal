// Dlg1.cpp : implementation file
//

#include "stdafx.h"
#include "TestHighlightWindow.h"
#include "Dlg1.h"
#include <HighlightWindow.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern COLORREF DISABLED_COLOR;

/////////////////////////////////////////////////////////////////////////////
// Dlg1 dialog


Dlg1::Dlg1(CWnd* pParent /*=NULL*/)
	: CDialog(Dlg1::IDD, pParent)
{
	//{{AFX_DATA_INIT(Dlg1)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}

void Dlg1::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(Dlg1)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(Dlg1, CDialog)
	//{{AFX_MSG_MAP(Dlg1)
	ON_EN_SETFOCUS(IDC_EDIT1, OnSetfocusEdit1)
	ON_EN_SETFOCUS(IDC_EDIT2, OnSetfocusEdit2)
	ON_EN_SETFOCUS(IDC_EDIT3, OnSetfocusEdit3)
	ON_EN_SETFOCUS(IDC_EDIT4, OnSetfocusEdit4)
	ON_WM_ACTIVATE()
	ON_WM_MOVE()
	ON_WM_SIZE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

void Dlg1::OnActivate( UINT nState, CWnd* pWndOther, BOOL bMinimized )
{
	if (nState == WA_ACTIVE  || nState == WA_CLICKACTIVE)
	{
		HighlightWindow::sGetInstance().show(this, NULL);
		HighlightWindow::sGetInstance().setDefaultColor();
	}
}


void Dlg1::OnSetfocusEdit1() 
{
	HighlightWindow::sGetInstance().show(this, GetDlgItem(IDC_EDIT1));
	HighlightWindow::sGetInstance().setDefaultColor();
}

void Dlg1::OnSetfocusEdit2() 
{
	HighlightWindow::sGetInstance().show(this, GetDlgItem(IDC_EDIT2));
	HighlightWindow::sGetInstance().setDefaultColor();
}

void Dlg1::OnSetfocusEdit3() 
{
	HighlightWindow::sGetInstance().show(this, GetDlgItem(IDC_EDIT3));
	HighlightWindow::sGetInstance().setDefaultColor();
}

void Dlg1::OnSetfocusEdit4() 
{
	HighlightWindow::sGetInstance().show(this, GetDlgItem(IDC_EDIT4));
	HighlightWindow::sGetInstance().setColor(DISABLED_COLOR);
}

BOOL Dlg1::OnInitDialog() 
{
	CDialog::OnInitDialog();
		
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void Dlg1::OnMove(int x, int y) 
{
	CDialog::OnMove(x, y);
	
	HighlightWindow::sGetInstance().refresh();
}

void Dlg1::OnSize(UINT nType, int cx, int cy) 
{
	CDialog::OnSize(nType, cx, cy);
	
	HighlightWindow::sGetInstance().refresh();
}

LRESULT Dlg1::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam) 
{
	LRESULT result = CDialog::DefWindowProc(message, wParam, lParam);;
	
	if (message == WM_EXITSIZEMOVE)
	{
		HighlightWindow::sGetInstance().refresh();
	}
	
	return result;
}

BOOL Dlg1::PreTranslateMessage(MSG* pMsg) 
{	
	return CDialog::PreTranslateMessage(pMsg);
}
