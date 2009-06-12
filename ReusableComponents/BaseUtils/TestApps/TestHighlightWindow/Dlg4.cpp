// Dlg4.cpp : implementation file
//

#include "stdafx.h"
#include "TestHighlightWindow.h"
#include "Dlg4.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// Dlg4 dialog


Dlg4::Dlg4(CWnd* pParent /*=NULL*/)
	: CDialog(Dlg4::IDD, pParent)
{
	//{{AFX_DATA_INIT(Dlg4)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}


void Dlg4::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(Dlg4)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(Dlg4, CDialog)
	//{{AFX_MSG_MAP(Dlg4)
		// NOTE: the ClassWizard will add message map macros here
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// Dlg4 message handlers
