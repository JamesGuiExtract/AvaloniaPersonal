// DialogAdvanced.cpp : implementation file
//

#include "stdafx.h"
#include "FAMUtils.h"
#include "DialogAdvanced.h"
#include "ADOUtils.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <CsisUtils.h>

extern HINSTANCE gFAMUtilsModuleResource;

//-------------------------------------------------------------------------------------------------
// CDialogAdvanced dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDialogAdvanced, CDialog)

CDialogAdvanced::CDialogAdvanced(const string& strServer, const string& strDatabase,
		const string& strAdvConnStrProperties, CWnd* pParent /*=NULL*/)
: CDialog(CDialogAdvanced::IDD, pParent)
, m_strServer(strServer)
, m_strDatabase(strDatabase)
, m_zAdvConnStrProperties(strAdvConnStrProperties.c_str())
{
}
//-------------------------------------------------------------------------------------------------
CDialogAdvanced::~CDialogAdvanced()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI0");
}
//-------------------------------------------------------------------------------------------------
void CDialogAdvanced::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_ADDITIONAL_CONN_STR_ATTR, m_zAdvConnStrProperties);
}
//-------------------------------------------------------------------------------------------------
INT_PTR CDialogAdvanced::DoModal()
{
	TemporaryResourceOverride rcOverride(gFAMUtilsModuleResource);

	// call the base class member
	return CDialog::DoModal();
}

//-------------------------------------------------------------------------------------------------
// Message Map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDialogAdvanced, CDialog)
ON_BN_CLICKED(IDC_BUTTON_DEFAULT, &CDialogAdvanced::OnBnClickedButtonDefault)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CDialogAdvanced message handlers
//-------------------------------------------------------------------------------------------------
BOOL CDialogAdvanced::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();

		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI0");
	
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CDialogAdvanced::OnBnClickedButtonDefault()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		getServer(m_strServer);
		getDatabase(m_strDatabase);

		m_zAdvConnStrProperties = createConnectionString(m_strServer, m_strDatabase).c_str();
		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI0");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool CDialogAdvanced::getServer(string &rstrServer)
{
	return findConnectionStringProperty((LPCTSTR)m_zAdvConnStrProperties, "Server", &rstrServer) ||
		findConnectionStringProperty((LPCTSTR)m_zAdvConnStrProperties, "Data Source", &rstrServer);
}
//-------------------------------------------------------------------------------------------------
bool CDialogAdvanced::getDatabase(string &rstrDatabase)
{
	return findConnectionStringProperty((LPCTSTR)m_zAdvConnStrProperties, "Database", &rstrDatabase) ||
		findConnectionStringProperty((LPCTSTR)m_zAdvConnStrProperties, "Initial Catalog", &rstrDatabase);
}
//-------------------------------------------------------------------------------------------------
string CDialogAdvanced::getAdvConnStrProperties()
{
	return string((LPCTSTR)m_zAdvConnStrProperties);
}