// PromptValuesDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "PromptValuesDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


int PromptValuesDlg::m_cmbRangeDir = 0;
int PromptValuesDlg::m_cmbTownshipDir = 0;
CString PromptValuesDlg::m_zTownship = "";
CString PromptValuesDlg::m_zCountyCode = "";
CString PromptValuesDlg::m_zRange = "";
CString PromptValuesDlg::m_zSection = "";
CString PromptValuesDlg::m_zQuarter = "";
CString PromptValuesDlg::m_zQQ = "";
CString PromptValuesDlg::m_zQQQ = "";

//-------------------------------------------------------------------------------------------------
// PromptValuesDlg dialog
//-------------------------------------------------------------------------------------------------
PromptValuesDlg::PromptValuesDlg(CWnd* pParent /*=NULL*/)
: CDialog(PromptValuesDlg::IDD, pParent),
  m_bEnableQuarter(FALSE),
  m_bEnableQQ(FALSE),
  m_bEnableQQQ(FALSE)
  , m_zStaticSelectText(_T(""))
{
	//{{AFX_DATA_INIT(PromptValuesDlg)
	m_bDrawQuarter = TRUE;
	m_bDrawQQ = TRUE;
	m_bDrawQQQ = TRUE;
	m_bDrawQQQQ = TRUE;
	m_bSectionGT36 = FALSE;
	m_bUseExisting = FALSE;
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void PromptValuesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(PromptValuesDlg)
	DDX_Control(pDX, IDC_USE_EXISTING, m_btnUseExisting);
	DDX_Control(pDX, IDC_SECTION_GT36, m_btnSectionGT36);
	DDX_Control(pDX, IDC_CHECK_QQQQ, m_btnQQQQ);
	DDX_Control(pDX, IDC_CHECK_QQQ, m_btnQQQ);
	DDX_Control(pDX, IDC_CHECK_QQ, m_btnQQ);
	DDX_Control(pDX, IDC_CHECK_Q, m_btnQ);
	DDX_Control(pDX, IDC_EDIT_SECTION, m_editSection);
	DDX_Control(pDX, IDC_EDIT_QQ_SECTION, m_editQQ);
	DDX_Control(pDX, IDC_EDIT_QQQ_SECTION, m_editQQQ);
	DDX_Control(pDX, IDC_EDIT_QUARTER_SECTION, m_editQuarter);
	DDX_Check(pDX, IDC_CHECK_Q, m_bDrawQuarter);
	DDX_Check(pDX, IDC_CHECK_QQ, m_bDrawQQ);
	DDX_Check(pDX, IDC_CHECK_QQQ, m_bDrawQQQ);
	DDX_Check(pDX, IDC_CHECK_QQQQ, m_bDrawQQQQ);
	DDX_CBIndex(pDX, IDC_CMB_RANGE_DIR, m_cmbRangeDir);
	DDX_CBIndex(pDX, IDC_CMB_TOWNSHIP_DIR, m_cmbTownshipDir);
	DDX_Text(pDX, IDC_EDIT_TOWNSHIP, m_zTownship);
	DDV_MaxChars(pDX, m_zTownship, 3);
	DDX_Text(pDX, IDC_EDIT_COUNTY_CODE, m_zCountyCode);
	DDV_MaxChars(pDX, m_zCountyCode, 3);
	DDX_Text(pDX, IDC_EDIT_RANGE, m_zRange);
	DDV_MaxChars(pDX, m_zRange, 3);
	DDX_Text(pDX, IDC_EDIT_SECTION, m_zSection);
	DDV_MaxChars(pDX, m_zSection, 3);
	DDX_Text(pDX, IDC_EDIT_QQ_SECTION, m_zQQ);
	DDV_MaxChars(pDX, m_zQQ, 3);
	DDX_Text(pDX, IDC_EDIT_QUARTER_SECTION, m_zQuarter);
	DDV_MaxChars(pDX, m_zQuarter, 3);
	DDX_Text(pDX, IDC_EDIT_QQQ_SECTION, m_zQQQ);
	DDV_MaxChars(pDX, m_zQQQ, 3);
	DDX_Check(pDX, IDC_SECTION_GT36, m_bSectionGT36);
	DDX_Check(pDX, IDC_USE_EXISTING, m_bUseExisting );
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_STATIC_SELECT_TEXT, m_staticSelectText);
	DDX_Text(pDX, IDC_STATIC_SELECT_TEXT, m_zStaticSelectText);
}

//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(PromptValuesDlg, CDialog)
	//{{AFX_MSG_MAP(PromptValuesDlg)
	ON_BN_CLICKED(IDC_CANCEL, OnCancel)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// PromptValuesDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL PromptValuesDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();

	try
	{
		m_editQuarter.EnableWindow(m_bEnableQuarter);
		m_editQQ.EnableWindow(m_bEnableQQ);
		m_editQQQ.EnableWindow(m_bEnableQQQ);
		m_btnQ.EnableWindow(!m_bEnableQuarter);
		m_btnQQ.EnableWindow(!m_bEnableQQ);
		m_btnQQQ.EnableWindow(!m_bEnableQQQ);
		if ( m_bEnableQQQ )
		{
			m_btnQQQQ.SetCheck(BST_CHECKED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08193")
	
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void PromptValuesDlg::OnOK() 
{
	try
	{
		UpdateData();

		if (m_zCountyCode.IsEmpty() || m_zRange.IsEmpty() 
			|| m_zTownship.IsEmpty() || m_zSection.IsEmpty())
		{
			AfxMessageBox("Please specify non-empty value(s) for County, Range, Township and Section.");
			return;
		}

		// Section number must be 1 ~ 36
		long nSectionNum = ::asLong((LPCTSTR)m_zSection);
		if (nSectionNum < 1 || ((m_bSectionGT36) ? nSectionNum > 999: nSectionNum > 36))
		{
			AfxMessageBox("Invalid Section number!");
			m_editSection.SetFocus();
			m_editSection.SetSel(0, -1);
			return;
		}

		long nQuarterNum = 0;
		if (!m_editQuarter.IsWindowEnabled())
		{
			m_zQuarter = "";
		}
		else if (m_zQuarter.IsEmpty())
		{
			AfxMessageBox("Please specify non-empty value(s) for Quarter Section.");
			m_editQuarter.SetFocus();
			return;
		}
		else
		{
			nQuarterNum = ::asLong((LPCTSTR)m_zQuarter);
			if (nQuarterNum < 1 || nQuarterNum > 4)
			{
				AfxMessageBox("Invalid Quarter Section number!");
				m_editQuarter.SetFocus();
				m_editQuarter.SetSel(0, -1);
				return;
			}
		}

		if (!m_editQQ.IsWindowEnabled())
		{
			m_zQQ = "";
		}
		else if (m_zQQ.IsEmpty())
		{
			AfxMessageBox("Please specify non-empty value(s) for Quarter-Quarter Section.");
			m_editQQ.SetFocus();
			return;
		}
		else
		{
			string strQQ = (LPCTSTR)m_zQQ;
			if (strQQ.size() != 2)
			{
				AfxMessageBox("Invalid Quarter-Quarter Section number.");
				m_editQQ.SetFocus();
				m_editQQ.SetSel(0, -1);
				return;
			}
			
			string strFirstQ = strQQ.substr(0, 1);
			long nFirstQNum = ::asLong(strFirstQ);
			if (nFirstQNum < 1 || nFirstQNum > 4)
			{
				AfxMessageBox("Invalid Quarter-Quarter Section number!");
				m_editQQ.SetFocus();
				m_editQQ.SetSel(0, -1);
				return;
			}
			
			string strSecondQ = strQQ.substr(1);
			long nSecondQNum = ::asLong(strSecondQ);
			if (nSecondQNum != nQuarterNum)
			{
				AfxMessageBox("Invalid Quarter-Quarter Section number!");
				m_editQQ.SetFocus();
				m_editQQ.SetSel(0, -1);
				return;
			}
		}

		if (!m_editQQQ.IsWindowEnabled())
		{
			m_zQQQ = "";
		}
		else if (m_zQQQ.IsEmpty())
		{
			AfxMessageBox("Please specify non-empty value(s) for Quarter-Quarter-Quarter Section.");
			m_editQQQ.SetFocus();
			return;
		}
		else
		{
			string strQQQ = (LPCTSTR)m_zQQQ;
			if (strQQQ.size() != 3)
			{
				AfxMessageBox("Invalid Quarter-Quarter-Quarter Section number.");
				m_editQQQ.SetFocus();
				m_editQQQ.SetSel(0, -1);
				return;
			}
			
			string strFirstQ = strQQQ.substr(0, 1);
			long nFirstQNum = ::asLong(strFirstQ);
			if (nFirstQNum < 1 || nFirstQNum > 4)
			{
				AfxMessageBox("Invalid Quarter-Quarter-Quarter Section number!");
				m_editQQQ.SetFocus();
				m_editQQQ.SetSel(0, -1);
				return;
			}
			
		}


		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08194")
	
	CDialog::OnOK();
}
//-------------------------------------------------------------------------------------------------
void PromptValuesDlg::OnCancel() 
{
	CDialog::OnCancel();
}
//-------------------------------------------------------------------------------------------------
void PromptValuesDlg::setStaticSelectText ( CString zSelectText )
{
	m_zStaticSelectText = zSelectText;
}