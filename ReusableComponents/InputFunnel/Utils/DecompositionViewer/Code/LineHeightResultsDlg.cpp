// LineHeightResultsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "DecompositionViewer.h"
#include "LineHeightResultsDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CLineHeightResultsDlg dialog


CLineHeightResultsDlg::CLineHeightResultsDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CLineHeightResultsDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CLineHeightResultsDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}


void CLineHeightResultsDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CLineHeightResultsDlg)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CLineHeightResultsDlg, CDialog)
	//{{AFX_MSG_MAP(CLineHeightResultsDlg)
		// NOTE: the ClassWizard will add message map macros here
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CLineHeightResultsDlg message handlers
