//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GeneralSettingsDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Duan Wang
//
//==================================================================================================

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// GeneralSettingsDlg dialog

class GeneralSettingsDlg : public CPropertyPage
{
	DECLARE_DYNCREATE(GeneralSettingsDlg)

// Construction
public:
	GeneralSettingsDlg();
	~GeneralSettingsDlg();
	void createToolTips();

	int m_iGeneralSettingsPageIndex;	//will be set by Property sheet who owns this page

// Dialog Data
	//{{AFX_DATA(GeneralSettingsDlg)
	enum { IDD = IDD_GENERAL_SETTINGS };
	CSpinButtonCtrl	m_spin;
	BOOL	m_bAutoLinking;
	BOOL	m_bCreateAttrField;
	int		m_iPrecision;
	int		m_nDefaultUnit;
	int		m_iPDFResolution;
	//}}AFX_DATA


// Overrides
	// ClassWizard generate virtual function overrides
	//{{AFX_VIRTUAL(GeneralSettingsDlg)
	public:
	virtual void OnFinalRelease();
	virtual BOOL OnSetActive();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual BOOL OnApply();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	CToolTipCtrl *m_pToolTips;
	// Generated message map functions
	//{{AFX_MSG(GeneralSettingsDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnCheckAutoLinkingSrcdoc();
	afx_msg void OnDeltaposSpinPrecision(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnChangeEditPrecision();
	afx_msg void OnSelchangeCmbUnitType();
	afx_msg void OnCheckCreateIcomapattr();
	afx_msg void OnChangeEditPDFResolution();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

	// Generated OLE dispatch map functions
	//{{AFX_DISPATCH(GeneralSettingsDlg)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_DISPATCH
	DECLARE_DISPATCH_MAP()
	DECLARE_INTERFACE_MAP()
private:
	void saveSettings();
	bool m_bInitialized;
	// whether or not any change made in this tab has been applied
	bool m_bApplied;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
