#pragma once

#include "SelectFileCondition.h"

#include <string>
using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrANY_USER = "<any user>";
const string gstrNO_USER = "<no user>";

//-------------------------------------------------------------------------------------------------
// ActionStatusCondition
//-------------------------------------------------------------------------------------------------
class ActionStatusCondition : public SelectFileCondition
{
public:
	ActionStatusCondition(void);
	ActionStatusCondition(const ActionStatusCondition& settings);

	~ActionStatusCondition(void) {};

	// Allows configuration of this instance.
	bool configure(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
		const string& strQueryHeader);

	SelectFileCondition* clone();

	// Builds the summary string
	string getSummaryString(bool bFirstCondition);

	// Builds a select query with the specified values selected for the current settings.
	// NOTE: strSelect should contain only the values to be selected by the query, for
	// example strSelect = "FAMFile.ID, FAMFile.FileName" or
	// strSelect = "FAMFile.ID, FAMFile.Priority", etc
	// NOTE2: It can be assumed that the FAMFile table will be included in the query.
	string buildQuery(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
		const string& strSelect, long nWorkflowID);

	void setAction(const string& strAction) { m_strAction = strAction; }
	string getAction() { return m_strAction; }

	void setStatus(long nStatus) { m_nStatus = nStatus; }
	long getStatus() { return m_nStatus; }

	void setStatusString(const string& strStatus) { m_strStatus = strStatus; }
	string getStatusString() { return m_strStatus; }

	void setUser(const string& strUser) { m_strUser = strUser; }
	string getUser() { return m_strUser; }

private:

	////////////////
	// Variables
	////////////////

	string m_strAction; // The action name
	long m_nStatus; // The status for the specified action
	string m_strStatus; // The status as a string
	string m_strUser; // The user (specified if choosing skipped files)
};

