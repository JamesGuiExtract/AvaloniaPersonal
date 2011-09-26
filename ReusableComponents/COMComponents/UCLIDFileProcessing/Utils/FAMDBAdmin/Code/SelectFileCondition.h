#pragma once

#include <string>
using namespace std;

//-------------------------------------------------------------------------------------------------
// SelectFileCondition
//-------------------------------------------------------------------------------------------------
class SelectFileCondition abstract
{
public:

	// Allows configuration of this instance.
	virtual bool configure(const IFileProcessingDBPtr& ipFAMDB, const string& strQueryHeader) abstract;

	// Allows configuration of this instance.
	virtual SelectFileCondition* clone() abstract;

	// Builds the summary string
	virtual string getSummaryString(bool bFirstCondition) abstract;

	// Builds a select query with the specified values selected for the current settings.
	// NOTE: strSelect should contain only the values to be selected by the query, for
	// example strSelect = "FAMFile.ID, FAMFile.FileName" or
	// strSelect = "FAMFile.ID, FAMFile.Priority", etc
	// NOTE2: It can be assumed that the FAMFile table will be included in the query.
	virtual string buildQuery(const IFileProcessingDBPtr& ipFAMDB, const string& strSelect) abstract;
};

