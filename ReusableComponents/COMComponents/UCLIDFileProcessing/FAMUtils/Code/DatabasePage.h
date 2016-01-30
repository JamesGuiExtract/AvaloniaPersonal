#pragma once

#include "FAMUtils.h"
#include "FileProcessingConfigMgr.h"
#include "StdAfx.h"

#include <RegistryPersistenceMgr.h>

#include <map>
#include <string>
#include "afxwin.h"

using namespace std;

//-------------------------------------------------------------------------------------------------
// INotifyDBConfigChanged class
//-------------------------------------------------------------------------------------------------
// If a class that uses the DatabasePage wants to know when the configuration file is changed
// it should derive from this interface class and override the SetDBConfigFile method
class IDBConfigNotifications
{
public:
	// PURPOSE: To allow the class that uses the Database page to be notified when
	//			a new server and database have been selected.  
	//			In the case that the specified connection information has path tags/functions to be
	//			expanded, the variable values are changed to represent the literal database upon
	//			completion of this call.
	// NOTE:	When implementing this method the setDBConnectionStatus should be called 
	//			to update the DB status field on the database page
	virtual void OnDBConfigChanged(string& rstrServer, string& rstrDatabase,
		string& rstrAdvConnStrProperties) = 0;

	// PURPOSE: Prompts user to select the context (ContactTags.sdf) to use. Assuming the user
	//			chooses a context, rbDBTagsAvailable will indicate whether the specified context
	//			has the <DatabaseServer> and <DatabaseName> tags defined.
	virtual bool PromptToSelectContext(bool& rbDBTagsAvailable) = 0;
};

//-------------------------------------------------------------------------------------------------
// DatabasePage dialog
//-------------------------------------------------------------------------------------------------
class FAMUTILS_API DatabasePage : public CPropertyPage
{
	DECLARE_DYNAMIC(DatabasePage)

public:
	DatabasePage();
	virtual ~DatabasePage();

	// Dialog Data
	enum { IDD = IDD_DATABASEPAGE };

	// PROMISE: To set the text that is displayed in the connection status edit box 
	// NOTE:	This should be called by the implementation of OnDBConfigChanged on the
	//			Notify object.
	void setDBConnectionStatus(const string& strStatusString);

	// PROMISE: Set the name SQL server, DBName and any advanced connection string properties to use.
	//			If the bNotifyObjects flag is true and an object has been set with 
	//			setNotifyDBConfigChanged then the OnDBConfigChanged method will be called
	void setConnectionInfo(const string& strSQLServer, const string& strDBName,
		const string& strAdvConnStrProperties, bool bNotifyObjects = true);

	// PROMISE: To set the object to notify when the configuration file has changed.
	void setNotifyDBConfigChanged(IDBConfigNotifications* pNotifyObject );

	// PROMISE: To clear the UI
	void clear();

	// PROMISE: Refreshes the connection based on the currently specified parameters.
	void refreshConnection();

	// PROMISE: To set the m_bBrowseEnabled flag
	void setBrowseEnabled(bool bBrowseEnabled);

	// Specifies whether gstrDATABASE_SERVER_TAG should be show in the server selection dropdown.
	void showDBServerTag(bool bShowDBServerTag);

	// Specifies whether gstrDATABASE_NAME_TAG should be show in the server name dropdown.
	void showDBNameTag(bool bShowDBNameTag);

	// PROMISE: To enable or disable the LastUsedDBButton based on whether
	//			there is a last db and server setting in the registry or not
	void updateLastUsedDBButton();

	// PROMISE: To enable or disable the entire property page
	void enableAllControls(bool bEnableAll);

	// PROMISE: To update context related controls on the database tab.
	// bFPSSaved- true if the current state is associated with an FPS file.
	// bValidContext- true if valid context is currently selected.
	// strContextName- The active context name.
	void setCurrentContextState(bool bFPSSaved, bool bValidContext, const string& strContextName);
	
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	
	DECLARE_MESSAGE_MAP()

	// Message Handlers
	afx_msg void OnBnClickedButtonBrowseDB();
	afx_msg void OnBnClickedButtonBrowseServer();
	afx_msg void OnBnClickedButtonRefresh();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnBnClickedButtonLastUsedDb();
	afx_msg void OnBnClickedButtonAdvConnStrProperties();
	afx_msg void OnBnClickedButtonUseCurrentContextDatabase();
	afx_msg void OnBnClickedButtonSelectContext();
	afx_msg void OnBnClickedCurrentContextLabel();
	afx_msg HBRUSH OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor);
	afx_msg BOOL OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message);
	virtual BOOL OnInitDialog();

private:
	// Control Values
	CString m_zServer;
	CString m_zDBName;
	CString m_zAdvConnStrProperties;

	// Represents the literal variables for the current DB connection. This may differ from the
	// above control values if the values above have path tag/functions that were evaluated.
	string m_strCurrServer;
	string m_strCurrDBName;
	string m_strCurrAdvConnStrProperties;

	// Control Variables
	CButton m_btnBrowseDB;
	CButton m_btnSqlServerBrowse;
	CEdit m_editDBServer;
	CEdit m_editDBName;
	CEdit m_editAdvConnStrProperties;
	CEdit m_editConnectStatus;
	CStatic m_labelCurrentContextLabel;
	CStatic m_labelCurrentContext;
	CFont m_contextLabelFont;
	CButton m_btnAdvConnStrProperties;
	CButton m_btnRefresh;
	CButton m_btnConnectLastUsedDB;
	CButton m_btnUseCurrentContextDatabase;
	CButton m_btnSelectContext;

	// Pointer is used to call SetDBConfigFile
	IDBConfigNotifications* m_pNotifyDBConfigChangedObject;

	// Flag to indicate if this object has been initialized
	bool m_bInitialized;

	// If this is true the browse buttons for the Server and database will be enabled.
	// If this is false the buttons will be disabled.
	// The default value is true
	bool m_bBrowseEnabled;

	// Whether gstrDATABASE_SERVER_TAG should be shown in the server selection dropdown.
	bool m_bShowDBServerTag;

	// Whether gstrDATABASE_NAME_TAG should be shown in the DB name dropdown.
	bool m_bShowDBNameTag;

	// Registry Persistence managers
	unique_ptr<FileProcessingConfigMgr> ma_pCfgMgr;

	// Color of the text for the context
	COLORREF m_crContextTextColor;

	// The Windows hand cursor.
	static HCURSOR g_hHandCursor;

	////////////////////
	// Methods
	////////////////////

	// PROMISE: To call the OnDBConfigChanged for the m_pNotifyDBConfigChangedObject object if it is 
	//			not NULL;
	void notifyObjects();

	// Applies a new server name setting (updates advanced connection properties if necessary).
	void setServer(const string& strServer);

	// Applies a new database name setting (updates advanced connection properties if necessary).
	void setDatabase(const string& strDatabase);

	// Sizes and positions the current context label based on the current text value.
	void positionContextLabel();
};
