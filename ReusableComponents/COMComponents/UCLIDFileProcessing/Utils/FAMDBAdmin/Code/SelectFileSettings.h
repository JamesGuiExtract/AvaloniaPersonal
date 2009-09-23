#pragma once
//-------------------------------------------------------------------------------------------------
// SelectFilesSettings.h : header file
// 
// Contains the definition of the SelectFileSettings class
//-------------------------------------------------------------------------------------------------
#include <COMUtils.h>
#include <UCLIDException.h>

#include <string>
#include <vector>

using namespace std;

const string gstrANY_USER = "<any user>";

enum FileSelectScope { eAllFiles, eAllFilesForWhich, eAllFilesTag,
						eAllFilesQuery, eAllFilesPriority };

class SelectFileSettings
{
private:
	// The scope for the file selection (all, all for which, all files with tag(s), query)
	FileSelectScope m_scope;

	// Values for the eAllFilesForWhich scope
	string m_strAction; // The action name
	long m_nActionID; // The action ID
	long m_nStatus; // The status for the specified action
	string m_strStatus; // The status as a string
	string m_strUser; // The user (specified if choosing skipped files)

	// Values for the eAllFilesTag scope
	bool m_bAnyTags; // Whether the user choose any tags or all tags
	vector<string> m_vecTags; // The list of tags selected

	// Value for the eAllFilesQuery scope
	string m_strSQL; // The query statement to complete the line SELECT FAMFile.ID FROM

	// Values for the eAllFilesPriority scope
	EFilePriority m_ePriority; // The priority to select
	string m_strPriority;

	// Values for the random subset selection restriction
	bool m_bLimitByRandomCondition; // Whether to narrow the selection
	int m_nRandomPercent; // The random percentage to narrow it by

public:
	// Default the setting to all files
	SelectFileSettings();
	SelectFileSettings(const SelectFileSettings& settings);

	~SelectFileSettings() {};

	void setScope(FileSelectScope scope) { m_scope = scope; }
	FileSelectScope getScope() { return m_scope; }

	void setAction(const string& strAction) { m_strAction = strAction; }
	string getAction() { return m_strAction; }

	void setActionID(long nActionID) { m_nActionID = nActionID; }
	long getActionID() { return m_nActionID; }

	void setStatus(long nStatus) { m_nStatus = nStatus; }
	long getStatus() { return m_nStatus; }

	void setStatusString(const string& strStatus) { m_strStatus = strStatus; }
	string getStatusString() { return m_strStatus; }

	void setUser(const string& strUser) { m_strUser = strUser; }
	string getUser() { return m_strUser; }

	void setAnyTags(bool bAnyTags) { m_bAnyTags = bAnyTags; }
	bool getAnyTags() { return m_bAnyTags; }

	void setTags(const vector<string>& vecTags) { m_vecTags = vecTags; }
	vector<string> getTags() { return m_vecTags; }

	void setSQLString(const string& strSQL) { m_strSQL = strSQL; }
	string getSQLString() { return m_strSQL; }

	void setPriority(EFilePriority ePriority) {
		m_ePriority = ePriority;
		IFileProcessingDBPtr ipDB(CLSID_FileProcessingDB);
		ASSERT_RESOURCE_ALLOCATION("ELI27675", ipDB != NULL);
		m_strPriority = asString(ipDB->AsPriorityString(ePriority));
	}
	EFilePriority getPriority() { return m_ePriority; }
	string getPriorityString() { return m_strPriority; }

	void setLimitByRandomCondition(bool bLimitByRandomCondition) { m_bLimitByRandomCondition = bLimitByRandomCondition; }
	bool getLimitByRandomCondition() { return m_bLimitByRandomCondition; }

	void setRandomPercent(int nRandomPercent)
	{
		// Ensure the percentage is between 1 and 100
		ASSERT_ARGUMENT("ELI26954", nRandomPercent > 0 && nRandomPercent <= 100);
		m_nRandomPercent = nRandomPercent;
	}
	int getRandomPercent() { return m_nRandomPercent; }

	// Builds the summary string
	string getSummaryString();

	// Builds a select query with the specified values selected for the current settings.
	// NOTE: strSelect should contain only the values to be selected by the query, for
	// example strSelect = "FAMFile.ID, FAMFile.FileName" or
	// strSelect = "FAMFile.ID, FAMFile.Priority", etc
	string buildQuery(const IFileProcessingDBPtr& ipFAMDB, const string& strSelect);

	IRandomMathConditionPtr getRandomCondition();
};