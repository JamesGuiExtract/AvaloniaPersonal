#pragma once
#include "afxdlgs.h"
#include "resource.h"

#include <string>

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
	// PURPOSE: To retrieve the current data from the database and fill in the list control
	//			and the total files edit box based on that data
	void populatePage();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL PreTranslateMessage(MSG* pMsg); // added as per P13 #4774

	DECLARE_MESSAGE_MAP()

	//---------------------------------------------------------------------------------------------
	// Message Handlers
	//---------------------------------------------------------------------------------------------
	afx_msg void OnBnClickedRefreshSummary();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	virtual BOOL OnInitDialog();

private:
	//---------------------------------------------------------------------------------------------
	// Control Variables
	//---------------------------------------------------------------------------------------------
	CListCtrl m_listActions;
	CEdit m_editFileTotal;
	CButton m_btnRefreshSummary;
	CStatic m_lblTotals;

	bool m_bInitialized;

	// The Database pointer obj to work with
	IFileProcessingDBPtr m_ipFAMDB;

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
	//---------------------------------------------------------------------------------------------
};