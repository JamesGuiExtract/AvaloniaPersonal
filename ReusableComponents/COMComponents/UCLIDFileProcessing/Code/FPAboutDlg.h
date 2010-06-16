#pragma once

#include "resource.h"
#include <string>
#include <memory.h>
#include <IConfigurationSettingsPersistenceMgr.h>

using namespace std;

// FPAboutDlg.h : header file
//

//-------------------------------------------------------------------------------------------------
// CAFAboutDlg dialog
//-------------------------------------------------------------------------------------------------

class CFPAboutDlg : public CDialog
{
	DECLARE_DYNAMIC(CFPAboutDlg)

// Construction
public:
	CFPAboutDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~CFPAboutDlg();

// Dialog Data
	//{{AFX_DATA(CAFAboutDlg)
	enum { IDD = IDD_DLG_ABOUT };
	// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA

protected:
	// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAFAboutDlg)
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

	// Generated message map functions
	//{{AFX_MSG(CAFAboutDlg)
	virtual BOOL OnInitDialog(void);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	////////////
	// Variables
	////////////
	// Handles Registry items
	auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pSettingsCfgMgr;

	////////////
	// Methods
	////////////
	// Retrieve the FPM version
	string getFileProcessingManagerVersion();

	// Retrieve the FKB update version
	string getFKBUpdateVersion();

	string getComponentDataDirectory();

};
//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
