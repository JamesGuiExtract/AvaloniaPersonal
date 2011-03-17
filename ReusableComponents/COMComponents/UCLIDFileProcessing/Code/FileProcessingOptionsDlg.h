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

	enum { IDD = IDD_DLG_OPTIONS };

	long getMaxDisplayRecords();
	bool getAutoSaveFPSFile();

// Overrides
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:

	virtual BOOL OnInitDialog();
	virtual void OnOK();
	afx_msg void OnEnChangeEditMaxDisplay();
	DECLARE_MESSAGE_MAP()

private:
	FileProcessingConfigMgr* m_pConfigManager;
	CEdit	m_editMaxDisplayRecords;
	CSpinButtonCtrl m_SpinMaxRecords;
	BOOL m_bAutoSave;

	void setMaxDisplayRecords(long nMaxDisplayRecords);
	void setAutoSaveFPSFile();

	long getMaxNumberOfRecordsFromDialog();
};
