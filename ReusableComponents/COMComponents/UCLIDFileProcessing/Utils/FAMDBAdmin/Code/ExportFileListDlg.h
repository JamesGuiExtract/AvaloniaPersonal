//-------------------------------------------------------------------------------------------------
// CExportFileListDlg dialog used for FAMDBAdmin
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"

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

// Dialog Data
	//{{AFX_DATA(CExportFileListDlg)
	enum { IDD = IDD_DLG_EXPORT_FILES };
	CButton m_radioAllFiles;
	CButton m_radioStatus;
	CComboBox m_comboStatus;
	CComboBox m_comboActions;
	CButton m_radioQuery;
	CEdit m_editQuery;
	CEdit m_editFileName;
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
	afx_msg void OnRadioAllFiles();
	afx_msg void OnRadioStatus();
	afx_msg void OnRadioSqlQuery();
	afx_msg void OnClickedBrowseFile();
	afx_msg void OnClickedOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	////////////
	//Variables
	///////////
	// The file action manager DB pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFAMDB;

	// The SQL query string used to export related file list
	CString m_zSqlQuery;

	////////////
	//Methods
	///////////
	// Update controls
	void updateControls();

	// To return the string representation of the given EActionStatus
	CString asStatusString(UCLID_FILEPROCESSINGLib::EActionStatus eStatusID);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
