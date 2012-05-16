//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveWizardDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "resource.h"
#include <CurveDjinni.h>
#include "CurveToolManager.h"

#include <ECurveParameter.h>

#include <vector>


class CurveWizardDlg : public CDialog
{
// Construction
public:
	CurveWizardDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~CurveWizardDlg();

// Dialog Data
	//{{AFX_DATA(CurveWizardDlg)
	enum { IDD = IDD_DLG_CurveWizard };
	CButton	m_btnSaveAs;
	CComboBox	m_cbnParameter3;
	CComboBox	m_cbnParameter2;
	CComboBox	m_cbnParameter1;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CurveWizardDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:


	// Generated message map functions
	//{{AFX_MSG(CurveWizardDlg)
	virtual void OnCancel();
	virtual void OnOK();
	virtual BOOL OnInitDialog();
	afx_msg void OnBTNReset();
	afx_msg void OnBTNSaveAs();
	afx_msg void OnSelendokCBNParameter1();
	afx_msg void OnSelendokCBNParameter2();
	afx_msg void OnSelendokCBNParameter3();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

	enum EUsedParameterID
	{
		kUnused = 0,
		kParameter1,
		kParameter2,
		kParameter3
	};

	typedef struct 
	{
		ECurveParameterType eCurveParameterID;
		std::string strParameterDescription;
		EUsedParameterID eUsed;
	} ParameterControlBlock;

	typedef std::vector<ParameterControlBlock> PCBCollection;
	PCBCollection m_vecPCB;
	CurveDjinni* m_pDjinni;			// the CurveDjinni
	CToolTipCtrl *m_pToolTips;

	void clearUsedParameter(EUsedParameterID eParameterID);
	void createParameterControlBlock(void);
	void createToolTips();
	int getIndexPCB(const CComboBox& rComboBox);
	ECurveParameterType getParameterID(CComboBox& rComboBox); 
	CurveWizardDlg::ParameterControlBlock& getParameterControlBlock(int iIndexPCBCollection);
	CurveParameters getSelectedCurveParameters(void);
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	void resetDropDownList(const CurveMatrix& rMatrix,CComboBox& rComboBox);
	void resetDropDownList1(void);
	void resetDropDownList2(void);
	void resetDropDownList3(void);
	void updateCurrentCurveTool(void);


private:
	bool m_bInitialized;
	int m_nSelectedItemCbn1;
	int m_nSelectedItemCbn2;
	int m_nSelectedItemCbn3;

	// Icon to be displayed on dialog's title bar
	HICON			m_hIcon;

};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
