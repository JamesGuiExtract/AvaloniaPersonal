#pragma once

#include "SelectFileCondition.h"
#include "UCLIDFileProcessing.h"

#include <string>
#include <vector>
using namespace std;

//-------------------------------------------------------------------------------------------------
// FileTagCondition
//-------------------------------------------------------------------------------------------------
class FileTagCondition : public SelectFileCondition
{
public:
	FileTagCondition (void);
	FileTagCondition (const FileTagCondition & settings);

	~FileTagCondition (void) {};

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

	void setTagType(TagMatchType eTagType) { m_eTagType = eTagType; }
	TagMatchType getTagType() { return m_eTagType; }

	void setTags(const vector<string>& vecTags) { m_vecTags = vecTags; }
	vector<string> getTags() { return m_vecTags; }

private:

	////////////////
	// Variables
	////////////////

	TagMatchType m_eTagType; // Whether the user choose any tags or all tags
	vector<string> m_vecTags; // The list of tags selected
};

