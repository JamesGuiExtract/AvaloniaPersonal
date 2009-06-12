//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	WordLengthDistributionDlg.h
//
// PURPOSE:	The purpose of the WordLengthDistributionDlg is to display 
//			a histogram of word lengths for the given spatial string
//			(or selected substring)
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================
#pragma once
#include "SpatialStringViewerCfg.h"
#include "SpatialStringViewerDlg.h"

//-------------------------------------------------------------------------------------------------
// WordLengthDistributionDlg class
//-------------------------------------------------------------------------------------------------
class WordLengthDistributionDlg : public CDialog
{
// Construction
public:
	WordLengthDistributionDlg(CSpatialStringViewerDlg* pSSVDlg, SpatialStringViewerCfg* pCfgDlg, 
		CWnd* pParent = NULL);

	enum { IDD = IDD_DLG_WORDLENGTHDISTRIBUTION };
	//----------------------------------------------------------------------------------------------
	// Variables
	//----------------------------------------------------------------------------------------------
	CStatic	m_staticNumWords;
	CListCtrl	m_listWordLengthDistribution;

	//----------------------------------------------------------------------------------------------
	// Methods
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To refresh the distribution list
	void refreshDistribution(int nStart, int nEnd);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To reset the control to its initial state and then refresh the
	//			list
	void resetAndRefresh();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	DECLARE_MESSAGE_MAP()

private:

	//----------------------------------------------------------------------------------------------
	// Variables
	//----------------------------------------------------------------------------------------------
	CSpatialStringViewerDlg* m_pSSVDlg;
	SpatialStringViewerCfg* m_pCfgDlg;

	// last start and end coordinates seen
	int m_nStart;
	int m_nEnd;

	long m_lDefaultPercentWidth;

	//----------------------------------------------------------------------------------------------
	// Methods
	//----------------------------------------------------------------------------------------------
};
