// ConvertFAMDBDlg.h : header file
//

#pragma once

#include <DBInfoCombo.h>

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

	// Stores the control that last had focus
	int m_iLastControlID;

	// Methods

	// Converts the database using the settings from the UI
	void convertDatabase();

	// Adds the actions found in the Action table on the ipSourceDBConnection to the
	// New database using the FileProcessingDB method AddNewAction - so all the 
	// columns will be created in the FAMFile
	void addActionsToNewDB(IFileProcessingDBPtr ipFAMDB, _ConnectionPtr ipSourceDBConnection);

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

	// Get a connection object using the given server and database
	_ConnectionPtr getConnection(const string& strServer, const string& strDatabase);

	// Method will turn IDENTITY_INSERT on or off for the table in strTable
	void identityInsert(_ConnectionPtr ipDestDBConnection, const string& strTable, bool bState);

	// This method does basic validation. It checks the to and from servers and database to make sure
	// they are not blank and verifies the schema version of the database to convert from
	// is for a 5.0 database. If it finds a problem is changes the focus to that control and 
	// displays a message.
	bool isInputDataValid();
};
