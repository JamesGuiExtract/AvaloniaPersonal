#pragma once

#include "resource.h"

#include <afxwin.h>
#include <afxcmn.h>
#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// SelectActionDlg dialog
class AdvancedTaskSettingsDlg: public CDialog
{
	DECLARE_DYNAMIC(AdvancedTaskSettingsDlg)

public:
// constructor
	AdvancedTaskSettingsDlg(int iNumThreads=0, bool bKeepProcessing=false,
		IVariantVectorPtr ipSchedule=__nullptr, CWnd* pParent = __nullptr);
	virtual ~AdvancedTaskSettingsDlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_ADVANCED_TASK_SETTINGS };

// Method
	int getNumberOfThreads();
	bool getKeepProcessing();
	IVariantVectorPtr getSchedule();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnOK();
	afx_msg void OnBtnMaxThread();
	afx_msg void OnBtnNumThread();
	afx_msg void OnEnChangeEditThreads();
	afx_msg void OnBtnKeepProcessingWithEmptyQueue();
	afx_msg void OnBtnStopProcessingWithEmptyQueue();
	afx_msg void OnBtnClickedCheckLimitProcessing();
	afx_msg void OnBnClickedButtonSetSchedule();
	DECLARE_MESSAGE_MAP()

private:
	/////////////
	//Variable
	////////////

	// Controls
	CButton m_btnMaxThreads;
	CButton m_btnNumThreads;
	CEdit m_editThreads;
	CSpinButtonCtrl m_SpinThreads;
	CButton m_btnKeepProcessingWithEmptyQueue;
	CButton m_btnStopProcessingWithEmptyQueue;
	CStatic m_groupProcessingSchedule;
	CButton m_checkLimitProcessing;
	CButton m_btnSetSchedule;

	BOOL m_bLimitProcessingTimes;
	int m_iNumThreads;
	bool m_bKeepProcessing;
	IVariantVectorPtr m_ipSchedule;

	///////////
	//Methods
	//////////
	void updateEnabledStates();
	int getNumThreads();
};
