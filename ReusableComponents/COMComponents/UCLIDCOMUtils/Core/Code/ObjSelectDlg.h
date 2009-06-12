// ObjSelectDlg.h : header file
//

#pragma once

#include "resource.h"

#include <vector>
#include <map>
#include <string>

/////////////////////////////////////////////////////////////////////////////
// CObjSelectDlg dialog

class CObjSelectDlg : public CDialog
{
// Construction
public:
	CObjSelectDlg(std::string strTitleAfterSelect, std::string strDescriptionPrompt, 
		std::string strSelectPrompt, std::string strCategoryName, 
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObj, bool bAllowNone, 
		long nNumRequiredIIDs, IID *pRequiredIIDs, CWnd* pParent = NULL, 
		bool bConfigureDescription = true);
	~CObjSelectDlg();

// Dialog Data
	//{{AFX_DATA(CObjSelectDlg)
	enum { IDD = IDD_DLG_OBJ_SELECT };
	CStatic	m_lblConfigure;
	CComboBox	m_comboObject;
	CButton	m_btnConfigure;
	CString	m_zDescription;
	CString	m_zDescLabel;
	CString	m_zSelectLabel;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CObjSelectDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CObjSelectDlg)
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	afx_msg void OnBtnConfigure();
	afx_msg void OnSelchangeComboObj();
	afx_msg void OnUpdateEditDesc();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// User-supplied title for dialog will be prepended with "Select "
	// and also used by Configure dialog
	std::string	m_strTitleAfterSelect;

	// User-supplied category name for items presented in combo box
	std::string	m_strCategoryName;
	
	// User-supplied object
	UCLID_COMUTILSLib::IObjectWithDescriptionPtr	m_ipObject;

	// Pointer to object selected in combo box
	UCLID_COMUTILSLib::ICategorizedComponentPtr	m_ipComponent;

	// Stores association between registered Object names and ProgIDs
	UCLID_COMUTILSLib::IStrToStrMapPtr	m_ipObjectMap;

	// Provide object pointer for specified item
	UCLID_COMUTILSLib::ICategorizedComponentPtr	getObjectFromName(std::string strName);

	// Whether or not <None> is a choice available in the combo box
	// used to delete use of this object
	bool		m_bAllowNone;

	// User-supplied object description
	std::string	m_strUserDescription;
	
	// the number of required interfaces for components displayed in this dialog,
	// as well as the interface IDs.
	long m_nNumRequiredIIDs;
	IID *m_pRequiredIIDs;

	// Populate combo box with items registered in m_strCategoryName
	// PROMISE: To return the number of items added to the combo box
	void	populateCombo();

	// Make appropriate selection of combo box item based on m_ipObject
	void	setCombo();

	// Show or hide the static text that tells the user the selected object 
	// still needs to be configured
	void	showReminder();

	// true if the dialog should allow the a description to be configured
	bool m_bConfigureDescription;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
