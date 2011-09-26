#pragma once

#include "SelectFileCondition.h"

#include <string>
using namespace std;

//-------------------------------------------------------------------------------------------------
// QueryCondition
//-------------------------------------------------------------------------------------------------
class QueryCondition : public SelectFileCondition
{
public:
	QueryCondition(void);
	QueryCondition(const QueryCondition& settings);

	~QueryCondition(void) {};

	// Allows configuration of this instance.
	bool configure(const IFileProcessingDBPtr& ipFAMDB, const string& strSelect);

	SelectFileCondition* clone();

	// Builds the summary string
	string getSummaryString(bool bFirstCondition);

	// Builds a select query with the specified values selected for the current settings.
	// NOTE: strSelect should contain only the values to be selected by the query, for
	// example strSelect = "FAMFile.ID, FAMFile.FileName" or
	// strSelect = "FAMFile.ID, FAMFile.Priority", etc
	// NOTE2: It can be assumed that the FAMFile table will be included in the query.
	string buildQuery(const IFileProcessingDBPtr& ipFAMDB, const string& strQueryHeader);

	void setSQLString(const string& strSQL) { m_strSQL = strSQL; }
	string getSQLString() { return m_strSQL; }

private:

	////////////////
	// Variables
	////////////////

	string m_strSQL; // The query statement to complete the line SELECT FAMFile.ID FROM
};

