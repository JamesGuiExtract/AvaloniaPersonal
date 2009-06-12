// ChoiceEditDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ChoiceEditDlg.h"

#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// ChoiceEditDlg dialog


ChoiceEditDlg::ChoiceEditDlg(CWnd* pParent /*=NULL*/)
	: CDialog(ChoiceEditDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(ChoiceEditDlg)
	m_strChars = _T("");
	m_strDescription = _T("");
	//}}AFX_DATA_INIT
}

ChoiceEditDlg::ChoiceEditDlg(CString cstrDes, CString cstrChars, CWnd* pParent /*=NULL*/)
	: CDialog(ChoiceEditDlg::IDD, pParent),
	  m_strDescription(cstrDes),
	  m_strChars(cstrChars)
{
}

void ChoiceEditDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(ChoiceEditDlg)
	DDX_Control(pDX, IDC_EDIT_Description, m_editDescription);
	DDX_Text(pDX, IDC_EDIT_Chars, m_strChars);
	DDX_Text(pDX, IDC_EDIT_Description, m_strDescription);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(ChoiceEditDlg, CDialog)
	//{{AFX_MSG_MAP(ChoiceEditDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// ChoiceEditDlg message handlers

BOOL ChoiceEditDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();
		
		// set focus on the first edit box
		m_editDescription.SetSel(-1);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18605");
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
