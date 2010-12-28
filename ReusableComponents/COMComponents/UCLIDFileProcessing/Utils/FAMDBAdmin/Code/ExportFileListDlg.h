//-------------------------------------------------------------------------------------------------
// CExportFileListDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"
#include "SelectFileSettings.h"

// ExportFileListDlg.h : header file
//

//-------------------------------------------------------------------------------------------------
// CExportFileListDlg dialog
//-------------------------------------------------------------------------------------------------
class CExportFileListDlg : public CDialog
{
public:
// Construction
	CExportFileListDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr pFAMDB);
	CExportFileListDlg(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr pFAMDB,
		const SelectFileSettings& selectSettings);
	~CExportFileListDlg();

// Dialog Data
	//{{AFX_DATA(CExportFileListDlg)
	enum { IDD = IDD_DLG_EXPORT_FILES };
	CEdit m_editSummary;
	CEdit m_editFileName;
	CButton m_btnOk;
	CButton m_btnBrowse;
	//}}AFX_DATA

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CExportFileListDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CExportFileListDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnClickedSelectFiles();
	afx_msg void OnClickedBrowseFile();
	afx_msg void OnClickedOK();
	afx_msg void OnChangedEditFileName();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	////////////
	//Variables
	///////////
	// The file action manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFAMDB;

	// Settings for the export
	SelectFileSettings m_settings;

	////////////
	//Methods
	///////////
	// Update controls
	void updateControls();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
