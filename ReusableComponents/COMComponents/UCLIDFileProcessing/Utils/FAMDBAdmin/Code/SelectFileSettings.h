#pragma once
//-------------------------------------------------------------------------------------------------
// SelectFilesSettings.h : header file
// 
// Contains the definition of the SelectFileSettings class
//-------------------------------------------------------------------------------------------------
#include <UCLIDException.h>

#include <string>
#include <vector>

using namespace std;

const string gstrANY_USER = "<any user>";

enum FileSelectScope { eAllFiles, eAllFilesForWhich, eAllFilesTag, eAllFilesQuery };

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

	// Value for the eSelectQuery scope
	string m_strSQL; // The query statement to complete the line SELECT FAMFile.ID FROM

	// Values for the random subset selection restriction
	bool m_bNarrowScope; // Whether to narrow the selection
	int m_nRandomPercent; // The random percentage to narrow it by

public:
	// Default the setting to all files
	SelectFileSettings() : m_scope(eAllFiles)
	{
	}
	SelectFileSettings(const SelectFileSettings& settings)
	{
		*this = settings;
	}

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

	void setNarrowScope(bool bNarrowScope) { m_bNarrowScope = bNarrowScope; }
	bool getNarrowScope() { return m_bNarrowScope; }

	void setRandomPercent(int nRandomPercent)
	{
		// Ensure the percentage is between 1 and 100
		ASSERT_ARGUMENT("ELI26954", nRandomPercent > 0 && nRandomPercent <= 100);
		m_nRandomPercent = nRandomPercent;
	}
	int getRandomPercent() { return m_nRandomPercent; }

	// Builds the summary string
	string getSummaryString()
	{
		string strSummary = "All files ";
		switch (m_scope)
		{
		case eAllFiles:
			strSummary += "in the database";
			break;

		case eAllFilesForWhich:
			{
				strSummary += "for which the \"" + m_strAction + "\" action's status is \""
					+ m_strStatus + "\"";
				if (m_nStatus == kActionSkipped)
				{
					strSummary += " by " + m_strUser;
				}
			}
			break;

		case eAllFilesTag:
			{
				strSummary += "associated with ";
				strSummary += (m_bAnyTags ? "any" : "all");
				strSummary += " of the following tags: ";
				string strTagString = "";
				for (vector<string>::iterator it = m_vecTags.begin(); it != m_vecTags.end(); it++)
				{
					if (!strTagString.empty())
					{
						strTagString += ", ";
					}
					strTagString += (*it);
				}
				strSummary += strTagString;
			}
			break;

		case eAllFilesQuery:
			strSummary += "selected by this custom query: SELECT FAMFile.ID FROM " + m_strSQL;
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI26906");
		}

		return strSummary;
	}
};