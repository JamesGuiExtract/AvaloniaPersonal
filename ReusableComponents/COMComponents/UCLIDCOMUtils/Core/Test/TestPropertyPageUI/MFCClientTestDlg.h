// MFCClientTestDlg.h : header file
//

#if !defined(AFX_MFCCLIENTTESTDLG_H__40C8C3E6_4BCE_441A_BADC_62487B0109EE__INCLUDED_)
#define AFX_MFCCLIENTTESTDLG_H__40C8C3E6_4BCE_441A_BADC_62487B0109EE__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CMFCClientTestDlg dialog

class CMFCClientTestDlg : public CDialog
{
// Construction
public:
	CMFCClientTestDlg(CWnd* pParent = NULL);	// standard constructor
	~CMFCClientTestDlg();

// Dialog Data
	//{{AFX_DATA(CMFCClientTestDlg)
	enum { IDD = IDD_MFCCLIENTTEST_DIALOG };
	int		m_nCurrentControl;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMFCClientTestDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CMFCClientTestDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnRadio1();
	afx_msg void OnRadio2();
	afx_msg void OnButtonApply();
	afx_msg void OnButtonGetInfo();
	virtual void OnOK();
	afx_msg void OnButtonShowInRcUi();
	afx_msg void OnButtonRefresh();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
	
	//{
	IPropertyPagePtr m_pCurrentPage;
	BOOL SetCurrentPage(IUnknown * pUnknown);

	DECLARE_INTERFACE_MAP()

	BEGIN_INTERFACE_PART( PropertyPageSite, IPropertyPageSite )
        STDMETHOD( OnStatusChange )( DWORD dwFlags );
        STDMETHOD( GetLocaleID )( LCID *pLocaleID );
        STDMETHOD( GetPageContainer )( IUnknown **ppUnk );
        STDMETHOD( TranslateAccelerator )( MSG *pMsg );
	END_INTERFACE_PART( PropertyPageSite )
	//}

	// static instance pointer for access to PropertyPageSite implementation
	static CMFCClientTestDlg *ms_pInstance;

private:
	// pointers to objects for which property pages need to be shown
	IUnknownPtr m_ipSomeCtrl1, m_ipSomeCtrl2;
	
	// pointer to current property page
	IPropertyPagePtr m_pPropPage;

	// method to switch to a new property page and handle the saving of 
	// settings in the current property page before switching to the
	// new property page
	void showPropertyPage(IUnknown *pNewControlUnknown, int iNewControlID);
	
	// the ID of the control whose property page is currently being shown
	int m_iCurrentPropPageControlID;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_MFCCLIENTTESTDLG_H__40C8C3E6_4BCE_441A_BADC_62487B0109EE__INCLUDED_)
