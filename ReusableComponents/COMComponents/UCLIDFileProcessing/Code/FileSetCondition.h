#pragma once

#include "SelectFileCondition.h"

#include <string>
#include <vector>

using namespace std;

//-------------------------------------------------------------------------------------------------
// FileSetCondition
//-------------------------------------------------------------------------------------------------
class FileSetCondition : public SelectFileCondition
{
public:
	FileSetCondition(void);
	FileSetCondition(const FileSetCondition& settings);

	~FileSetCondition(void) {}

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
		const string& strSelect);

	void setFileSetName(const string& strFileSetName) { m_strFileSetName = strFileSetName; }
	string getFileSetName() { return m_strFileSetName; }

private:

	////////////////
	// Variables
	////////////////

	string m_strFileSetName;
};

