#pragma once

#include "FAMUtils.h"
#include "FileProcessingConfigMgr.h"

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
	// NOTE:	When implementing this method the setDBConnectionStatus should be called 
	//			to update the DB status field on the database page
	virtual void OnDBConfigChanged(const string& strServer, const string& strDatabase) = 0;
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

	// PROMISE: Set the name SQL server and DBName
	//			If the bNotifyObjects flag is true and an object has been set with 
	//			setNotifyDBConfigChanged then the OnDBConfigChanged method will be called
	void setServerAndDBName(const string& strSQLServer, const string& strDBName, bool bNotifyObjects = true);

	// PROMISE: To set the object to notify when the configuration file has changed.
	void setNotifyDBConfigChanged(IDBConfigNotifications* pNotifyObject );

	// PROMISE: To clear the UI
	void clear();

	// PROMISE: To set the m_bBrowseEnabled flag
	void setBrowseEnabled(bool bBrowseEnabled);

	// PROMISE: To enable or disable the LastUsedDBButton based on whether
	//			there is a last db and server setting in the registry or not
	void updateLastUsedDBButton();

	// PROMISE: To enable or disable the entire property page
	void enableAllControls(bool bEnableAll);
	
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
	virtual BOOL OnInitDialog();

private:
	// Control Values
	CString m_zServer;
	CString m_zDBName;

	// Control Variables
	CButton m_btnBrowseDB;
	CButton m_btnSqlServerBrowse;
	CEdit m_editDBServer;
	CEdit m_editDBName;
	CEdit m_editConnectStatus;
	CButton m_btnRefresh;
	CButton m_btnConnectLastUsedDB;

	// Pointer is used to call SetDBConfigFile
	IDBConfigNotifications* m_pNotifyDBConfigChangedObject;

	// Flag to indicate if this object has been initialized
	bool m_bInitialized;

	// If this is true the browse buttons for the Server and database will be enabled.
	// If this is false the buttons will be disabled.
	// The default value is true
	bool m_bBrowseEnabled;

	// Registry Persistence managers
	auto_ptr<FileProcessingConfigMgr> ma_pCfgMgr;

	////////////////////
	// Methods
	////////////////////

	// PROMISE: To call the OnDBConfigChanged for the m_pNotifyDBConfigChangedObject object if it is 
	//			not NULL;
	void notifyObjects();
};
