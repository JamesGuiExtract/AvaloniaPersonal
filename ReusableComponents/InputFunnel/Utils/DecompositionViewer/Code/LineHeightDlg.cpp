// LineHeightDlg.cpp : implementation file
//

#include "stdafx.h"
#include "DecompositionViewer.h"
#include "LineHeightDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CLineHeightDlg dialog


CLineHeightDlg::CLineHeightDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CLineHeightDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CLineHeightDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}


void CLineHeightDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CLineHeightDlg)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CLineHeightDlg, CDialog)
	//{{AFX_MSG_MAP(CLineHeightDlg)
		// NOTE: the ClassWizard will add message map macros here
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CLineHeightDlg message handlers
