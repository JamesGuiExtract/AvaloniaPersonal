#pragma once
// OCRFilterSchemesDlg.h : header file
//

#include "resource.h"
#include "OCRFilteringBaseDLL.h"
#include "FilterSchemeDefinition.h"
#include "CfgOCRFiltering.h"

#include <string>
#include <vector>
#include <map>
#include <memory>

/////////////////////////////////////////////////////////////////////////////
// OCRFilterSchemesDlg dialog

class OCRFilteringBaseDLL OCRFilterSchemesDlg : public CDialog
{
// Construction
public:
	//==============================================================================================
	// standard constructor
	OCRFilterSchemesDlg(CWnd* pParent = NULL);   
	~OCRFilterSchemesDlg();

	//==============================================================================================
	// Create this dialog as modeless
	void createModeless();

	//==============================================================================================
	// Returns current selected filtering scheme
	const std::string& getCurrentScheme();
	
	//==============================================================================================
	// Returns a set of valid characters for the specified input type
	// Note: No duplicate char is allowed
	std::string getValidChars(const std::string& strInputType);

	//==============================================================================================
	// Refresh the dialog contents 
	void refresh();
	
	//==============================================================================================
	// Sets current filtering scheme
	void setCurrentScheme(const std::string& strCurrentScheme);
	
	//==============================================================================================
	// Shows the OCRFilterSettingsDlg
	// bEditFOD -- Whether or not the settings dlg shall be shown as
	//			   in the FOD edit mode
	void showFilterSettingsDlg(bool bEditFSD = true);

// Dialog Data
	//{{AFX_DATA(OCRFilterSchemesDlg)
	enum { IDD = IDD_DLG_FilterSchemesDlg };
	CButton	m_btnOpen;
	CEdit	m_editValidFilterStrings;
	CComboBox	m_cmbSchemes;
	CString	m_zValidFilterStrings;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(OCRFilterSchemesDlg)
	public:
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(OCRFilterSchemesDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSelchangeCMBSchemeName();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	afx_msg void OnBTNOpen();
	afx_msg void OnDestroy();
	afx_msg void OnClose();
	afx_msg void OnCloseupCMBSchemeName();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// **************************
	// Helper functions

	//==============================================================================================
	// Creates an default scheme that doesn't exist in any file
	void createDefaultScheme();

	//==============================================================================================
	// Creates an FSD from the file and put it into the m_apFSDs
	void createFSD(const std::string& strFSDFileName);

	//==============================================================================================
	std::string getCurrentModuleFullPath();

	//==============================================================================================
	// Returns a set of input choice descriptions for current filter scheme
	std::string printValidFilterStringDescriptions();

	//==============================================================================================
	// Initializes the combo box with all filter schemes available from the bin
	void refreshSchemesList();

	//==============================================================================================
	// Reads FODs from bin directory
	void reloadFODs();

	//==============================================================================================
	// Reads FSDs from bin directory
	void reloadFSDs();


	// **************************
	// Member variables
	
	// All FODs exist in same directory as this module
	FilterOptionsDefinitions m_FODs;
	
	// All FSDs exist in same directory as this module
	FilterSchemeDefinitions m_FSDs;

	// configuration manager
	CfgOCRFiltering m_CfgOCRFiltering;

	// Current selected filtering scheme
	std::string m_strCurrentScheme;

	// Whether or not the dialog has been initialized
	bool m_bInitialized;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
