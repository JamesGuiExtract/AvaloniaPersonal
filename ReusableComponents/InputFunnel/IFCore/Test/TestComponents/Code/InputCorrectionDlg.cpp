// InputCorrectionDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "InputCorrectionDlg.h"

#include <TemporaryResourceOverride.h>


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern HINSTANCE g_Resource;

/////////////////////////////////////////////////////////////////////////////
// InputCorrectionDlg dialog


InputCorrectionDlg::InputCorrectionDlg(CWnd* pParent /*=NULL*/)
	: CDialog(InputCorrectionDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(InputCorrectionDlg)
	m_editCorrection = _T("");
	//}}AFX_DATA_INIT
}


void InputCorrectionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(InputCorrectionDlg)
	DDX_Text(pDX, IDC_EDIT_CORRECTION, m_editCorrection);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(InputCorrectionDlg, CDialog)
	//{{AFX_MSG_MAP(InputCorrectionDlg)
		// NOTE: the ClassWizard will add message map macros here
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// InputCorrectionDlg message handlers

int InputCorrectionDlg::DoModal() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( g_Resource );
	
	return CDialog::DoModal();
}
