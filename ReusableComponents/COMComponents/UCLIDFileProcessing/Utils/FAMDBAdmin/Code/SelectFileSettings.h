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
enum TagMatchType { eAnyTag = 0, eAllTag = 1, eNoneTag = 2 };

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
	TagMatchType m_eTagType; // Whether the user choose any tags or all tags
	vector<string> m_vecTags; // The list of tags selected

	// Value for the eAllFilesQuery scope
	string m_strSQL; // The query statement to complete the line SELECT FAMFile.ID FROM

	// Values for the eAllFilesPriority scope
	EFilePriority m_ePriority; // The priority to select
	string m_strPriority;

	// Values for the random subset selection restriction
	bool m_bLimitByRandomCondition; // Whether to narrow the selection to a random subset
	bool m_bRandomSubsetUsePercentage; // Whether to narrow by percentage or file count
	int m_nRandomAmount; // The size of the subset (percentage or filecount)

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

	void setTagType(TagMatchType eTagType) { m_eTagType = eTagType; }
	TagMatchType getTagType() { return m_eTagType; }

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

	void setRandomSubsetUsePercentage(bool bUsePercentage) { m_bRandomSubsetUsePercentage = bUsePercentage; }
	bool getRandomSubsetUsePercentage() { return m_bRandomSubsetUsePercentage; }

	void setRandomAmount(int nRandomAmount)
	{
		m_nRandomAmount = nRandomAmount;
	}
	int getRandomAmount() { return m_nRandomAmount; }

	// Builds the summary string
	string getSummaryString();

	// Builds a select query with the specified values selected for the current settings.
	// NOTE: strSelect should contain only the values to be selected by the query, for
	// example strSelect = "FAMFile.ID, FAMFile.FileName" or
	// strSelect = "FAMFile.ID, FAMFile.Priority", etc
	// NOTE2: It can be assumed that the FAMFile table will be included in the query.
	string buildQuery(const IFileProcessingDBPtr& ipFAMDB, const string& strSelect, const string& strOrderByClause);

	IRandomMathConditionPtr getRandomCondition();
};