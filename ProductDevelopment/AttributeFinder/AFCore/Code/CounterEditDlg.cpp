//

#include "stdafx.h"
#include "CounterEditDlg.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// CCounterEditDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CCounterEditDlg, CDialog)
//--------------------------------------------------------------------------------------------------
CCounterEditDlg::CCounterEditDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CCounterEditDlg::IDD, pParent)
{

}
//--------------------------------------------------------------------------------------------------
CCounterEditDlg::~CCounterEditDlg()
{
}
//--------------------------------------------------------------------------------------------------
void CCounterEditDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_EDIT_ID, m_editCounterID);
	DDX_Control(pDX, IDC_EDIT_NAME, m_editCounterName);
	DDX_Text(pDX, IDC_EDIT_ID, m_zCounterID);
	DDX_Text(pDX, IDC_EDIT_NAME, m_zCounterName);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCounterEditDlg, CDialog)
	ON_BN_CLICKED(IDOK, OnBnClickOK)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// CCounterEditDlg message handlers
//--------------------------------------------------------------------------------------------------
BOOL CCounterEditDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		// Custom counter IDs must be in the range 100 - 999
		m_editCounterID.SetLimitText(3);

		// update the data for the prompt, input data, and title
		SetWindowText(m_zCaption);
		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI39022");
	
	return FALSE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void CCounterEditDlg::OnBnClickOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		UpdateData(TRUE);

		long nID = asLong((LPCTSTR)m_zCounterID);

		if (nID < 100 || nID > 999)
		{
			MessageBox("Counter ID for a custom counter must be between 100 and 999",
				"Invalid Counter ID", MB_OK);
			return;
		}

		m_zCounterName = m_zCounterName.Trim();
		if ((m_zCounterName.GetLength() < 10 ||
				_strcmpi((LPCTSTR)m_zCounterName.Right(9), "(By Page)") != 0) &&
			(m_zCounterName.GetLength() < 14 ||
				_strcmpi((LPCTSTR)m_zCounterName.Right(13), "(By Document)") != 0))
		{
			MessageBox("Counter Name must end with (By Page) or (By Document)",
				"Invalid Counter Name", MB_OK);
			return;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI39023")

	OnOK();
}
//-------------------------------------------------------------------------------------------------