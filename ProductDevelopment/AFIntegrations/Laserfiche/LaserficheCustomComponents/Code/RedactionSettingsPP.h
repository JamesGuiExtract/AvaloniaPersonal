//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RedactionSettings.h
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
using namespace std;

class CIDShieldLF;

//--------------------------------------------------------------------------------------------------
// CRedactionSettingsPP
//--------------------------------------------------------------------------------------------------
class CRedactionSettingsPP : public CPropertyPage, private CIDShieldLFHelper
{
	DECLARE_DYNAMIC(CRedactionSettingsPP)

public:
	CRedactionSettingsPP(CIDShieldLF *pIDShieldLF);
	virtual ~CRedactionSettingsPP();

	enum { IDD = IDD_TAB_REDACTION };

protected:

	/////////////////////
	// Overrides
	/////////////////////
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnApply();
	virtual BOOL OnInitDialog();

	/////////////////////
	// Message handlers
	/////////////////////
	afx_msg void OnChange();
	afx_msg void OnBnClickedChkEnableVerification();
	afx_msg void OnBnClickedBtnBrowse();

	/////////////////////
	// Control variables
	/////////////////////
	CString m_zMasterRSD;
	BOOL m_bRedactHCData;
	BOOL m_bRedactMCData;
	BOOL m_bRedactLCData;
	BOOL m_bAutoTag;
	BOOL m_bOnDemand;
	BOOL m_bEnsureTextRedactions;

private:

	/////////////////////
	// Methods
	/////////////////////

	// Enables/disables verification check boxes and radio buttons as appropriate
	void updateVerifyControls();

	DECLARE_MESSAGE_MAP()
};
