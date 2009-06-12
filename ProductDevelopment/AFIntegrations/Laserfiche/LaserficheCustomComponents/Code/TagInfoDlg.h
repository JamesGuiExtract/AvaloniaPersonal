#pragma once
#include "afxwin.h"

//--------------------------------------------------------------------------------------------------
// CTagInfoDlg
//--------------------------------------------------------------------------------------------------
class CTagInfoDlg : public CDialog
{
	DECLARE_DYNAMIC(CTagInfoDlg)

public:
	CTagInfoDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~CTagInfoDlg();

// Dialog Data
	enum { IDD = IDD_TAGINFO };

protected:

	////////////////
	// Overrides
	////////////////
	virtual BOOL OnInitDialog();
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	////////////////
	// Variables
	////////////////
	CStatic m_Icon;
	
	DECLARE_MESSAGE_MAP()
};