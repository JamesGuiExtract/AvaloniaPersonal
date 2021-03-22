#pragma once
#include "afxdlgs.h"
#include "resource.h"

#include <string>
#include <stack>
#include "afxwin.h"
using namespace std;

class CFAMDBAdminSummaryDlg :
	public CPropertyPage
{
	DECLARE_DYNAMIC(CFAMDBAdminSummaryDlg)

public:
	CFAMDBAdminSummaryDlg(void);
	virtual ~CFAMDBAdminSummaryDlg(void);

	// Dialog Data
	enum { IDD = IDD_DLG_DBSUMMARY };

	//---------------------------------------------------------------------------------------------
	// PURPOSE: To set the current database object
	void setFAMDatabase(IFileProcessingDBPtr ipFAMDB);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To set the current database object
	void setFAMFileInspector(IFAMFileInspectorPtr ipFAMFileInspector);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To retrieve the current data from the database and fill in the list control
	//			and the total files edit box based on that data
	// ARGS:	nActionID- If -1, all actions are refreshed. Otherwise, only the action with the
	//			specified action ID is refreshed.
	void populatePage(long nActionIDToRefresh = -1);

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL PreTranslateMessage(MSG* pMsg); // added as per P13 #4774

	DECLARE_MESSAGE_MAP()

	//---------------------------------------------------------------------------------------------
	// Message Handlers
	//---------------------------------------------------------------------------------------------
	afx_msg void OnBnClickedRefreshSummary();
	afx_msg void OnBnClickedShowDeletedFileStats();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnNMRClickListActions(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnContextExportFileList();
	afx_msg void OnContextInspectFiles();
	afx_msg void OnContextSetFileActionStatus();
	afx_msg void OnContextViewFailed();
	afx_msg void OnContextCopyActionName();
	afx_msg void OnContextCopyCount();
	virtual BOOL OnInitDialog();

private:
	//---------------------------------------------------------------------------------------------
	// Control Variables
	//---------------------------------------------------------------------------------------------
	CListCtrl m_listActions;
	CEdit m_editFileTotal;
	CButton m_btnRefreshSummary;
	CStatic m_lblTotals;
	CStatic m_staticLastUpdated;
	CStatic m_staticStatisticsType;
	CButton m_btnShowNonDeletedFileStats;
	CButton m_btnShowDeletedFileStats;

	bool m_bInitialized;

	// The Database pointer obj to work with
	IFileProcessingDBPtr m_ipFAMDB;

	// Stores file selection information based on the right-click location in the summary grid for
	// use by the "Set file action status" and "Export file list" context menu options.
	UCLID_FILEPROCESSINGLib::IFAMFileSelectorPtr m_ipContextMenuFileSelector;

	// Allows inspection of files in the database using the FAMFileInspector utility.
	IFAMFileInspectorPtr m_ipFAMFileInspector;

	// The action and count associated with the context menu click location.
	string m_strContextMenuAction;
	string m_strContextMenuCount;

	// Indicates if the queries used should use OracleSyntax
	bool m_bUseOracleSyntax;

	// Indicates if the stats shown are for deleted files
	bool m_bShowDeletedFileStats;

	//---------------------------------------------------------------------------------------------
	// Helper methods
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To setup the summary list control
	void prepareListControl();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To resize the list control columns based on the size of the list control
	void resizeListColumns();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To build a query to gather the statistics for each action
	inline std::string buildStatsQueryFromActionColumn(const std::string& strActionColumn)
	{
		std::string strQuery = "SELECT COUNT(ID) as Total, " + strActionColumn +
			" FROM FAMFile GROUP BY " + strActionColumn;
		return strQuery;
	}
};
