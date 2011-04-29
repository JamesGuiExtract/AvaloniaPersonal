//-------------------------------------------------------------------------------------------------
// CAboutAFDlg dialog used for About Flex Index
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include <string>
#include "afxwin.h"

// AFAboutDlg.h : header file
//

//-------------------------------------------------------------------------------------------------
// These functions are accessed from several locations in this project's code
//-------------------------------------------------------------------------------------------------
std::string getAttributeFinderEngineVersion(EHelpAboutType eType);
std::string getModuleFileName();
std::string getProductName(EHelpAboutType eType);
std::string getProductVersion(std::string strProduct);

//-------------------------------------------------------------------------------------------------
// CAFAboutDlg dialog
//-------------------------------------------------------------------------------------------------
class CAFAboutDlg : public CDialog
{
// Construction
public:
	CAFAboutDlg(EHelpAboutType eHelpAboutType, std::string strProduct);

// Dialog Data
	//{{AFX_DATA(CAFAboutDlg)
	enum { IDD = IDD_ABOUTBOX };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAFAboutDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

	// Memeber function
	// Get the Icon for current application
	HICON getIconName(EHelpAboutType eType);

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CAFAboutDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// get the FKB version by looking at the version information file in the ComponentData folder
	std::string getFKBVersion();

	std::string	m_strProduct;
	EHelpAboutType m_eType;
	CStatic m_Icon;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
