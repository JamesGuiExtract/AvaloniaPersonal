
#pragma once

// TaskInfoDlg.h : header file
//
#include "FileProcessingRecord.h"

#include <memory>

class UCLIDException;
/////////////////////////////////////////////////////////////////////////////
// TaskInfoDlg dialog

class TaskInfoDlg : public CDialog
{
// Construction
public:
	TaskInfoDlg(CWnd* pParent = NULL);   // standard constructor

	// set the task to a new task;
	void setTask(const FileProcessingRecord& task);

// Dialog Data
	//{{AFX_DATA(TaskInfoDlg)
	enum { IDD = IDD_DLG_TASK_INFO };
	CListCtrl	m_listDetails;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(TaskInfoDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(TaskInfoDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnDblclkListDetail(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	/////////////
	// Variables
	/////////////
	FileProcessingRecord m_taskFileProcessing;
	std::unique_ptr<UCLIDException> m_apUCLIDException;

	// dialog minimum width
	int m_nDlgMinWidth;
	// dialog fixed height, i.e. user can't resize the height of the dialog
	int m_nDlgFixedHeight;

	bool m_bInitialized;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
