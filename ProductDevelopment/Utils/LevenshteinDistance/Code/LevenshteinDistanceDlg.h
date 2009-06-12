//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LevenshteinDistanceDlg.h
//
// PURPOSE:	Declaration of CLevenshteinDistanceDlg class
//
// NOTES:	For more info on the Levenshtein Distance (aka Edit Distance) 
//			algorithm, see:
//			http://www.merriampark.com/ld.htm#ALGORITHM
//
// AUTHORS:	Ryan Mulder
//
//============================================================================

#pragma once
#include "Resource.h"

#include <XInfoTip.h>

#include <string>
#include <vector>



// CLevenshteinDistanceDlg dialog
class CLevenshteinDistanceDlg : public CDialog
{
// Construction
public:
	CLevenshteinDistanceDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_LEVENSHTEINDISTANCE_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnBnClickedGetLevBtn();
	afx_msg void OnBnClickedExitBtn();
	afx_msg void OnBnClickedCopyBtn();
	afx_msg void OnStnClickedStaticTrueboxesInfo();
	DECLARE_MESSAGE_MAP()

public:
	CButton m_btnCopy;
	CString m_zExpected;
	CString m_zFound;
	CString m_zLDPercent;
	int m_iRemWS;
	int m_iUpdate;
	int m_iCaseSensitive;
	int m_iLDist;
	int m_iExpectedLen;
	int m_iFoundLen;
	double m_dLDPercent;
	CXInfoTip		m_infoTip;

};
