//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RepositorySettings.h
//
// PURPOSE:	A helper class to CIDShieldLF.  Implements a property page to allow configuration of
//			redaction in ID Shield for Laserfiche
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================
#pragma once

#include "stdafx.h"
#include "resource.h"

#include "IDShieldLFHelper.h"

#include <string>
#include <vector>
using namespace std;

class CIDShieldLF;

///////////////////////////////////////////////////////////////////////////////////////////////////
// CRepositorySettingsPP
///////////////////////////////////////////////////////////////////////////////////////////////////
class CRepositorySettingsPP : public CPropertyPage, private CIDShieldLFHelper
{
	DECLARE_DYNAMIC(CRepositorySettingsPP)

public:
	CRepositorySettingsPP(CIDShieldLF *pIDShieldLF);
	virtual ~CRepositorySettingsPP();

	enum { IDD = IDD_TAB_REPOSITORY };

protected:

	//////////////////////
	// Overrides
	//////////////////////
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	//////////////////////
	// Message handlers
	//////////////////////
	afx_msg void OnBnClickedBtnCreateTags();
	afx_msg void OnBnClickedBtnAboutTags();
	afx_msg void OnBnClickedBtnRefresh();
	afx_msg void OnBnClickedBtnChangeRepository();
	afx_msg void OnBnClickedBtnUntagPending();
	afx_msg void OnBnClickedBtnMarkVerified();
	afx_msg void OnBnClickedBtnMarkFailedAsPending();

	//////////////////////
	// Control variables
	//////////////////////
	CString m_zSettingsDirectory;

private:

	//////////////////////
	// Variables
	//////////////////////
	vector<string> m_vecMissingTags;
	
	////////////////
	// Methods
	////////////////
	// Update controls and presence of ID Shield according to the current state of the repository.
	void updateStatus();

	DECLARE_MESSAGE_MAP()
};
