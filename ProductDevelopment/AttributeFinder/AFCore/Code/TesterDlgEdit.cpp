// TesterDlgEdit.cpp : implementation file
//

#include "stdafx.h"
#include "afcore.h"
#include "TesterDlgEdit.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CTesterDlgEdit dialog
//-------------------------------------------------------------------------------------------------
CTesterDlgEdit::CTesterDlgEdit(CString zName, CString zValue, CString zType, 
								   CWnd* pParent /*=NULL*/)
	: CDialog(CTesterDlgEdit::IDD, pParent)
{
	//{{AFX_DATA_INIT(CTesterDlgEdit)
	m_zName = zName;
	m_zType = zType;
	m_zValue = zValue;
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void CTesterDlgEdit::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTesterDlgEdit)
	DDX_Text(pDX, IDC_EDIT_NAME, m_zName);
	DDX_Text(pDX, IDC_EDIT_TYPE, m_zType);
	DDX_Text(pDX, IDC_EDIT_VALUE, m_zValue);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTesterDlgEdit, CDialog)
	//{{AFX_MSG_MAP(CTesterDlgEdit)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CTesterDlgEdit message handlers
//-------------------------------------------------------------------------------------------------
void CTesterDlgEdit::OnOK() 
{
	CDialog::OnOK();
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
CString CTesterDlgEdit::GetName() 
{
	// Return data member
	return m_zName;
}
//-------------------------------------------------------------------------------------------------
CString CTesterDlgEdit::GetType() 
{
	// Return data member
	return m_zType;
}
//-------------------------------------------------------------------------------------------------
CString CTesterDlgEdit::GetValue() 
{
	// Return data member
	return m_zValue;
}
//-------------------------------------------------------------------------------------------------
