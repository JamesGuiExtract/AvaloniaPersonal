#pragma once

// ImportRuleSetDlg.h : header file
//

#include "resource.h"

/////////////////////////////////////////////////////////////////////////////
// CImportRuleSetDlg dialog

class CImportRuleSetDlg : public CDialog
{
// Construction
public:
	CImportRuleSetDlg(UCLID_AFCORELib::IRuleSetPtr ipRuleSet, 
		std::vector<std::string> vecReservedStrings, bool bDoImport, 
		CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CImportRuleSetDlg)
	enum { IDD = IDD_DLG_IMPORT_ATTRIBUTES };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CImportRuleSetDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CImportRuleSetDlg)
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	//////////
	// Methods
	//////////
	// Check collection of reserved strings and create unique Value
	std::string		getUnreservedString(std::string strOldValue);

	// Populate Attribute Names and Imported/Exported Attribute Names
	// from the Rule Set
	void	populateList();

	///////////////
	// Data Members
	///////////////
	// Grid window
	CkListCtrl	m_wndList;

	// Is this an Import or Export dialog
	bool			m_bDoImport;

	// Collection of rules to be adjusted downward for import or export
	UCLID_AFCORELib::IRuleSetPtr m_ipRuleSet;

	// Collection of reserved strings invalid for use as Values
	std::vector<std::string> m_vecReserved;

	// Have the dialog's controls been instantiated yet - allows for resize
	// and repositioning
	bool			m_bInitialized;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
