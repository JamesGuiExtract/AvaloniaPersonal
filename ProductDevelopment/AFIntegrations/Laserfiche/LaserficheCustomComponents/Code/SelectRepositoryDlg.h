//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SelectRepositoryDlg.h
//
// PURPOSE:	Prompt for a connection to a Laserfiche repository.  This class only collects the 
//			Laserfiche login info and does not attempt to login itself.
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================
#pragma once
#include "afxwin.h"
#include "resource.h"

#include <string>
#include <vector>
#include <map>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CSelectRepositoryDlg
//--------------------------------------------------------------------------------------------------
class CSelectRepositoryDlg : public CDialog
{
	DECLARE_DYNAMIC(CSelectRepositoryDlg)

public:
	CSelectRepositoryDlg(CWnd* pParent = NULL);
	virtual ~CSelectRepositoryDlg();

	enum { IDD = IDD_SELECT_REPOSITORY };

	// Initialize the repositories to be displayed in the repository dropdown box
	void SetRepositoryList(const vector<string> &vecRepositories,
						   const map<string, string> &mapServers);
	
	// Prompt the user to obtain Laserfiche repository login info.
	int GetLoginInfo(string &rstrServer, string &rstrRepository, 
					 string &rstrUser, string &rstrPassword);

protected:

	////////////////////
	// Overrides
	////////////////////
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	////////////////////
	// Message handlers
	////////////////////
	afx_msg void OnBnClickedBtnLogin();

	////////////////////
	// Variables
	////////////////////
	
	CComboBox m_cmbRepository;
	CString m_zRepository;
	CString m_zUser;
	CString m_zPassword;
	vector<string> m_vecRepositoryList;
	map<string, string> m_mapServers;

	DECLARE_MESSAGE_MAP()
};
