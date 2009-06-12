// SelectRepositoryDlg.cpp : implementation file
//
#include "stdafx.h"
#include "LaserficheCustomComponents.h"
#include "SelectRepositoryDlg.h"

#include <UCLIDException.h>
#include <StringTokenizer.h>

//--------------------------------------------------------------------------------------------------
// CSelectRepositoryDlg
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CSelectRepositoryDlg, CDialog)

CSelectRepositoryDlg::CSelectRepositoryDlg(CWnd* pParent/* = NULL*/)
	: CDialog(CSelectRepositoryDlg::IDD, pParent)
	, m_zRepository(_T(""))
	, m_zUser(_T(""))
	, m_zPassword(_T(""))
{
}	
//--------------------------------------------------------------------------------------------------
CSelectRepositoryDlg::~CSelectRepositoryDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20811");
}
//--------------------------------------------------------------------------------------------------
void CSelectRepositoryDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CMB_REPOSITORY, m_cmbRepository);
	DDX_Text(pDX, IDC_EDIT_USER, m_zUser);
	DDX_Text(pDX, IDC_EDIT_PASSWORD, m_zPassword);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSelectRepositoryDlg, CDialog)
	ON_BN_CLICKED(IDC_BTN_LOGIN, &CSelectRepositoryDlg::OnBnClickedBtnLogin)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// Public members
//--------------------------------------------------------------------------------------------------
void CSelectRepositoryDlg::SetRepositoryList(const vector<string> &vecRepositories,
											 const map<string, string> &mapServers)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		m_vecRepositoryList = vecRepositories;
		m_mapServers = mapServers;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20726");
}
//--------------------------------------------------------------------------------------------------
int CSelectRepositoryDlg::GetLoginInfo(string &rstrServer, string &rstrRepository, 
									   string &rstrUser, string &rstrPassword)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	int nRes = IDABORT;

	try
	{
		// If a default server name is specified, check to see if the server name matches up with 
		// that of the corresponding available repository
		if (!rstrServer.empty())
		{
			if (m_mapServers[rstrRepository] != rstrServer)
			{
				// If the server name doesn't match up with an available repository, specify the
				// full path to the login dialog.
				rstrRepository = rstrServer + "/" + rstrRepository;;
			}
		}

		// Update the values to be displayed
		m_zRepository = rstrRepository.c_str();
		m_zUser = rstrUser.c_str();
		m_zPassword = rstrPassword.c_str();

		nRes = (int) DoModal();

		if (nRes == IDOK)
		{
			// Retrieve the info the user entered.
			rstrRepository = m_zRepository.GetString();
			rstrUser = m_zUser.GetString();
			rstrPassword = m_zPassword.GetString();

			// Calculate the server name based on the rstrRepository value.
			rstrServer = "";
			replace(rstrRepository.begin(), rstrRepository.end(), '\\', '/');
			vector<string> vecTokens;

			StringTokenizer::sGetTokens(rstrRepository, '/', vecTokens);

			if (vecTokens.size() == 1)
			{
				// No server specified, obtain it from m_mapServers
				rstrServer = m_mapServers[rstrRepository];
			}
			else if (vecTokens.size() == 2)
			{
				rstrServer = vecTokens[0];
				rstrRepository = vecTokens[1];
			}
			else
			{
				UCLIDException ue("ELI20727", "Invalid repository name!");
				ue.addDebugInfo("Repository", rstrRepository);
				throw ue;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20723");

	return nRes;
}

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
BOOL CSelectRepositoryDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		// Populate repository box with available repositories
		for each (string strRepository in m_vecRepositoryList)
		{
			m_cmbRepository.AddString(strRepository.c_str());
		}

		// Add and select the last used repository (if applicable)
		if (m_zRepository.IsEmpty() == false)
		{
			int nSelRepository = m_cmbRepository.FindStringExact(-1, m_zRepository);

			if (nSelRepository == CB_ERR)
			{
				nSelRepository = m_cmbRepository.AddString(m_zRepository);
			}

			m_cmbRepository.SetCurSel(nSelRepository);
		}
		else if (m_cmbRepository.GetCount() > 0 )
		{
			m_cmbRepository.SetCurSel(0);
		}

		// Without this call, the dialog may appear behind other windows.
		SetForegroundWindow();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20724");

	return TRUE;
}

//--------------------------------------------------------------------------------------------------
// Message handlers
//--------------------------------------------------------------------------------------------------
void CSelectRepositoryDlg::OnBnClickedBtnLogin()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UpdateData(TRUE);

		m_cmbRepository.GetWindowText(m_zRepository);

		OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21350");
}
//--------------------------------------------------------------------------------------------------