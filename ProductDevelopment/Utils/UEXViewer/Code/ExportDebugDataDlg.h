#pragma once

#include "UEXViewerDlg.h"

//-------------------------------------------------------------------------------------------------
// CExportDebugDataDlg dialog
//-------------------------------------------------------------------------------------------------
class CExportDebugDataDlg : public CDialog
{
	DECLARE_DYNAMIC(CExportDebugDataDlg)

public:
	CExportDebugDataDlg(CWnd* pParent);   // standard constructor
	virtual ~CExportDebugDataDlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_EXPORT_DEBUG_DATA };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()
	afx_msg void OnBnClickedOk();
	afx_msg void OnBnClickedButtonBrowse();
	afx_msg void OnBnClickedCheckNarrowScope();
	afx_msg void OnEnUpdateEditElicode();

private:
	// UEX Viewer dialog
	CUEXViewerDlg* m_pUEXDlg;

	CString m_zDebugParamToExport;
	CString m_zExportfile;
	int m_iAppendToFile;
	int m_iScope;
	int m_iLimitScope;
	int m_iUniqueValues;
	CString m_zELICodeToLimit;
	CEdit m_editELICode;

	// Methods
	
	// Method finds the required params in the ueDebug exception and places the data in rvecDebugParams
	void getDebugDataFromException(vector<string> &rvecDebugParams, const UCLIDException &ueDebug);

	// Method gets the nItemNumber for the UEXlist in the UEX dialog and extracts
	// the debug data and places in it rvecDebugParams
	void getExceptionDataFromUEXList(int nItemNumber, vector<string> &rvecDebugParams);
};
