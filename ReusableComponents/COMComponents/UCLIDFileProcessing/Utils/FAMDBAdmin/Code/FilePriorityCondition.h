#pragma once

#include "SelectFileCondition.h"

#include <string>
using namespace std;

//-------------------------------------------------------------------------------------------------
// FilePriorityCondition
//-------------------------------------------------------------------------------------------------
class FilePriorityCondition : public SelectFileCondition
{
public:
	FilePriorityCondition(void);
	FilePriorityCondition(const FilePriorityCondition& settings);

	~FilePriorityCondition(void) {};

	// Allows configuration of this instance.
	bool configure(const IFileProcessingDBPtr& ipFAMDB, const string& strQueryHeader);

	SelectFileCondition* clone();

	// Builds the summary string
	string getSummaryString(bool bFirstCondition);

	// Builds a select query with the specified values selected for the current settings.
	// NOTE: strSelect should contain only the values to be selected by the query, for
	// example strSelect = "FAMFile.ID, FAMFile.FileName" or
	// strSelect = "FAMFile.ID, FAMFile.Priority", etc
	// NOTE2: It can be assumed that the FAMFile table will be included in the query.
	string buildQuery(const IFileProcessingDBPtr& ipFAMDB, const string& strSelect);

	void setPriority(EFilePriority ePriority);
	EFilePriority getPriority() { return m_ePriority; }

	string getPriorityString() { return m_strPriority; }

private:

	////////////////
	// Variables
	////////////////

	EFilePriority m_ePriority; // The priority to select
	string m_strPriority; // The readable name of the priority to select
};

