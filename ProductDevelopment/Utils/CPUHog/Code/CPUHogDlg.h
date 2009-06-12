// CPUHogDlg.h : header file
//

#if !defined(AFX_CPUHOGDLG_H__F6C5156B_DF57_408C_A818_FDDA51E5ACF5__INCLUDED_)
#define AFX_CPUHOGDLG_H__F6C5156B_DF57_408C_A818_FDDA51E5ACF5__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include <afxmt.h>

/////////////////////////////////////////////////////////////////////////////
// CCPUHogDlg dialog

class CCPUHogDlg : public CDialog
{
// Construction
public:
	CCPUHogDlg(CWnd* pParent = NULL);	// standard constructor
	static UINT threadFunc( LPVOID pParam );

	virtual BOOL PreTranslateMessage(MSG *pMsg);


// Dialog Data
	//{{AFX_DATA(CCPUHogDlg)
	enum { IDD = IDD_CPUHOG_DIALOG };
	CButton	m_radio100;
	CButton	m_radio90;
	CButton	m_radioSpecified;
	CButton	m_btnRun;
	CEdit	m_editSeconds;
	CEdit	m_editPercent;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCPUHogDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CCPUHogDlg)
	virtual BOOL OnInitDialog();
	virtual void OnCancel();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnBtnRun();
	afx_msg void OnRadio100();
	afx_msg void OnRadio90();
	afx_msg void OnRadioSpecified();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	class ThreadParams
	{
	public:
		ThreadParams();
		long getDuration();
		void setDuration(long nDuration);
		double getPercent();
		void setPercent(double fPercent);
		void kill();
		bool killed();

		void reset();

		CWinThread* getThread();
		void setThread(CWinThread* pThread);
	private:
		CSemaphore m_semAll;
		long m_nDuration;
		double m_fPercent;
		bool m_bKill;
		CWinThread* m_pThread;
	};

	ThreadParams m_params;
	bool m_bRunning;
	CWinThread* m_pThread;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_CPUHOGDLG_H__F6C5156B_DF57_408C_A818_FDDA51E5ACF5__INCLUDED_)
