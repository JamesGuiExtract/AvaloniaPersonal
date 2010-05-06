#pragma once

// AddDocTypesDlg.h : header file
//

#include "resource.h"       // main symbols

#include <vector>
#include <string>

/////////////////////////////////////////////////////////////////////////////
// AddDocTypesDlg dialog

class AddDocTypesDlg : public CDialog
{
// Construction
public:
	AddDocTypesDlg(std::string strIndustry, bool bAllowSpecialTags, 
		bool bAllowMultiSelect, bool bAllowMultiplyClassified, CWnd* pParent = NULL);

	// Returns collection of selected document types
	std::vector<std::string>& getChosenTypes();

	// Returns selected industry name
	std::string	getSelectedIndustry();

	// Disables selection change in industry combo box
	void	lockIndustrySelection();

// Dialog Data
	//{{AFX_DATA(AddDocTypesDlg)
	enum { IDD = IDD_DLG_ADDDOCTYPES };
	CListBox	m_listTypes;
	CComboBox	m_cmbIndustry;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(AddDocTypesDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(AddDocTypesDlg)
	virtual void OnOK();
	virtual BOOL OnInitDialog();
	afx_msg void OnSelchangeComboCategory();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	///////////////
	// Methods
	///////////////
	void populateComboBox();

	void populateListBox();

	///////////////
	// Variables
	///////////////
	std::vector<std::string> m_vecTypes;

	UCLID_AFUTILSLib::IDocumentClassificationUtilsPtr m_ipDocUtils;

	// Collection of available Industry names
	IVariantVectorPtr	m_ipIndustries;

	// Selected industry
	std::string	m_strCategory;

	// Special tags will be displayed for selection
	bool		m_bAllowSpecial;

	// Multiple document types can be selected
	bool		m_bAllowMultipleSelection;

	// Allow an option to match a multiply classified document.
	// This option is only useful when m_bAllowSpecial==true since it is a special tag.
	bool		m_bAllowMultiplyClassified;

	// Industry combo box will be disabled
	bool		m_bLockIndustry;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
