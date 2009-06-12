#pragma once

#include "resource.h"
#include "UEXViewerDlg.h"

#include <string>

// CUEXFindDlg dialog
class IConfigurationSettingsPersistenceMgr;

class CUEXFindDlg : public CDialog
{
public:
	CUEXFindDlg(IConfigurationSettingsPersistenceMgr *pCfgMgr, CWnd* pParent);
	virtual ~CUEXFindDlg();

// Dialog Data
	//{{AFX_DATA(CUEXFindDlg)
	enum { IDD = IDD_UEXFIND_DLG };
	BOOL	m_bAsRegEx;
	int		m_nSelect;
	CEdit	m_editFind;
	CString	m_zPatterns;
	//}}AFX_DATA

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CUEXFindDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL


// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CUEXFindDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnFind();
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	// UEX Viewer dialog
	CUEXViewerDlg* m_pUEXDlg;

	// UEXViewer Persistence Manager
	IConfigurationSettingsPersistenceMgr *m_pCfgMgr;

	// Retrieves specified Exception from UEXViewerDlg
	// Searches for strSearch within appropriate portion of Exception
	// Returns true if successful, false otherwise
	bool	findStringInException(std::string strSearch, int iExceptionIndex);

	// Retrieves and applies registry settings 
	void	initPersistent();

	// Retrieves search string from edit box
	// Returns true if string is valid, false otherwise
	bool	isValidPattern();

	// Stores registry settings 
	void	storeSettings();
};
