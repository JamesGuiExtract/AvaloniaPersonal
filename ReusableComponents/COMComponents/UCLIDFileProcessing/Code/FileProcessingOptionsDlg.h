#pragma once

// FileProcessingOptionsDlg.h : header file
//

#include "resource.h"

#include <FileProcessingConfigMgr.h>

#include <string>

/////////////////////////////////////////////////////////////////////////////
// FileProcessingOptionsDlg dialog

class FileProcessingOptionsDlg : public CDialog
{
// Construction
public:
	FileProcessingOptionsDlg(CWnd* pParent = NULL);   // standard constructor

	void setConfigManager(FileProcessingConfigMgr* pConfigManager);

	bool getRestrictDisplayRecords();
	void setRestrictDisplayRecords(bool bRestrictDisplayRecords);

	long getMaxDisplayRecords();
	void setMaxDisplayRecords(long nMaxDisplayRecords);

// Dialog Data
	//{{AFX_DATA(FileProcessingOptionsDlg)
	enum { IDD = IDD_DLG_OPTIONS };
	CEdit	m_editMaxDisplayRecords;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(FileProcessingOptionsDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(FileProcessingOptionsDlg)
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	FileProcessingConfigMgr* m_pConfigManager;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
