// FileProcessingOptionsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "uclidfileprocessing.h"
#include "FileProcessingOptionsDlg.h"
#include <cpputil.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// FileProcessingOptionsDlg dialog
//-------------------------------------------------------------------------------------------------
FileProcessingOptionsDlg::FileProcessingOptionsDlg(CWnd* pParent /*=NULL*/)
: CDialog(FileProcessingOptionsDlg::IDD, pParent),
  m_pConfigManager(NULL)
{
	//{{AFX_DATA_INIT(FileProcessingOptionsDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void FileProcessingOptionsDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(FileProcessingOptionsDlg)
	DDX_Control(pDX, IDC_EDIT_MAX_NUM_RECORDS, m_editMaxDisplayRecords);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingOptionsDlg, CDialog)
	//{{AFX_MSG_MAP(FileProcessingOptionsDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
void FileProcessingOptionsDlg::setConfigManager(FileProcessingConfigMgr* pConfigManager)
{
	m_pConfigManager = pConfigManager;
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingOptionsDlg::getRestrictDisplayRecords()
{
	return m_pConfigManager->getRestrictNumStoredRecords();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingOptionsDlg::setRestrictDisplayRecords(bool bRestrictDisplayRecords)
{
	m_pConfigManager->setRestrictNumStoredRecords(bRestrictDisplayRecords);
}
//-------------------------------------------------------------------------------------------------
long FileProcessingOptionsDlg::getMaxDisplayRecords()
{
	return m_pConfigManager->getMaxStoredRecords();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingOptionsDlg::setMaxDisplayRecords(long nMaxDisplayRecords)
{
	m_pConfigManager->setMaxStoredRecords(nMaxDisplayRecords);
}

//-------------------------------------------------------------------------------------------------
// FileProcessingOptionsDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingOptionsDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	try
	{
		if (!m_pConfigManager)
		{
			return TRUE;
		}

		if (getRestrictDisplayRecords())
		{
			m_editMaxDisplayRecords.SetWindowText(asString(getMaxDisplayRecords()).c_str());
		}
		else
		{
			m_editMaxDisplayRecords.SetWindowText("");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12470");
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void FileProcessingOptionsDlg::OnOK() 
{
	try
	{
		// Display of records is always restricted (P13 #3939) - WEL 11/22/06
		setRestrictDisplayRecords(true);
	
		// Save number of displayed records
		CString zStr;
		m_editMaxDisplayRecords.GetWindowText(zStr);
		string strRecs = zStr;
		setMaxDisplayRecords(asLong(strRecs));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12469");

	CDialog::OnOK();
}
//-------------------------------------------------------------------------------------------------
