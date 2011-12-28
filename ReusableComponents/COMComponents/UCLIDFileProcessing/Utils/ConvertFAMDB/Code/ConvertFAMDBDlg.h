// ConvertFAMDBDlg.h : header file
//

#pragma once

#include "stdafx.h"

#include <DBInfoCombo.h>
#include <Win32Event.h>

// CConvertFAMDBDlg dialog
class CConvertFAMDBDlg : public CDialog
{
// Construction
public:
	CConvertFAMDBDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_CONVERTFAMDB_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnSetfocusControl();
	afx_msg void OnBnClickedOk();
	
	DECLARE_MESSAGE_MAP()

private:
	// Control Values
	CString m_zFromServer;
	CString m_zFromDB;
	CString m_zToServer;
	CString m_zToDB;
	
	DBInfoCombo m_cbFromServer;
	DBInfoCombo m_cbFromDB;
	DBInfoCombo m_cbToServer;
	DBInfoCombo m_cbToDB;
	CButton m_checkRetainHistoryData;
	CStatic m_staticCurrentStep;
	CStatic m_staticCurrentRecord;
	CProgressCtrl m_progressCurrent;
	CButton m_buttonStart;
	CButton m_buttonClose;

	// Stores the control that last had focus
	int m_iLastControlID;

	// Events used to signal the start of the conversion and the completion
	Win32Event m_eventConvertStarted;
	Win32Event m_eventConvertComplete;

	// Methods

	// Converts the database using the settings from the UI
	void convertDatabase();

	// Adds the actions found in the Action table on the ipSourceDBConnection to the
	// New database using the FileProcessingDB method AddNewAction - so all the 
	// columns will be created in the FAMFile
	void addActionsToNewDB80(_ConnectionPtr ipSourceDBConnection, _ConnectionPtr ipDestDBConnection);

	// Copies the records from the source to the dest
	// This will also convert ASCName to ActionID, MachineName to MachineID and UserName to FAMUserID
	void copyRecords(_ConnectionPtr ipSourceDBConnection, _ConnectionPtr ipDestDBConnection, 
		const string& strSource, const string& strDest, bool bCopyID = false);

	// Checks for fields in source that are the key value for a Foreign key relationship in the destination
	// database, sets the ActionID, MachineID or FAMUserID if they are in the required
	void addFKData(_ConnectionPtr ipDestDBConnection, FieldsPtr ipSourceFields, FieldsPtr ipDestFields);

	// If TS_Transition or TimeStamp is found in the source then it is moved to field 
	// named DateTimeStamp in Destination
	void copyTimeFields(FieldsPtr ipSource, FieldsPtr ipDest);

	// The DBInfoSettings are set to there default values when the database is created
	// This function will change the settings in the DBInfo table to the values that were in the 
	// old database except for the Version settings ( FAMDBSchemaVersion and IDShieldSchemaVersion)
	void copyDBInfoSettings(IFileProcessingDBPtr ipFAMDB, _ConnectionPtr ipSourceDBConnection);

	// Internal define new action function
	long defineNewAction80(_ConnectionPtr ipConnection, const string& strActionName);

	// Returns a record set containing the action with the specified name. The record set will be
	// empty if no such action exists. Code to work with the schema that existed as of the
	// Flex/IDS 8.0 release.
	_RecordsetPtr getActionSet80(_ConnectionPtr ipConnection, const string &strAction);

	// Adds an action with the specified name to the specified record set. Returns the action ID of
	// the newly created action. Code to work with the schema that existed as of the Flex/IDS 8.0
	// release.
	long addActionToRecordset80(_ConnectionPtr ipConnection, _RecordsetPtr ipRecordset, 
		const string &strAction);

	// Gets the last modified/added row from the specified table. Code to work with the schema that
	// existed as of the Flex/IDS 8.0 release.
	long getLastTableID80(const _ConnectionPtr& ipDBConnection, string strTableName);

	// Adds action related columns and indexes to the FPMFile table. Code to work with the schema
	// that existed as of the Flex/IDS 8.0 release.
	void addActionColumn80(const _ConnectionPtr& ipConnection, const string& strAction);

	// Get a connection object using the given server and database
	_ConnectionPtr getConnection(const string& strServer, const string& strDatabase);

	// Method will turn IDENTITY_INSERT on or off for the table in strTable
	void identityInsert(_ConnectionPtr ipDestDBConnection, const string& strTable, bool bState);

	// Method will build the string to be displayed for the given step and update the current step
	// control, rnStepNumber will be incremented.
	void updateCurrentStep(string strStepDescription, long &rnStepNumber, long nTotalSteps);

	// This method does basic validation. It checks the to and from servers and database to make sure
	// they are not blank and verifies the schema version of the database to convert from
	// is for a 5.0 database. If it finds a problem is changes the focus to that control and 
	// displays a message.
	bool isInputDataValid();

	// Enables or disables the controls
	void enableControls(bool bEnable);

	// Thread function that calls convertDatabase pData should be passed the this pointer
	static UINT convertDatabaseInThread(void *pData);
};
