#pragma once

#include "SelectFileCondition.h"

#include <string>
#include <vector>

using namespace std;

//-------------------------------------------------------------------------------------------------
// SpecifiedFilesCondition
//-------------------------------------------------------------------------------------------------
class SpecifiedFilesCondition : public SelectFileCondition
{
public:
	SpecifiedFilesCondition(void);
	SpecifiedFilesCondition(const SpecifiedFilesCondition& settings);

	~SpecifiedFilesCondition(void) {}

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

	void setUseSpecifiedFiles() { m_eFileListSource = eSpecifiedFiles; }
	bool getUseSpecifiedFiles() { return m_eFileListSource == eSpecifiedFiles; }

	void setUseListFile() { m_eFileListSource = eListFile; }
	bool getUseListFile() { return m_eFileListSource == eListFile; }

	void setSpecifiedFiles(const vector<string>& strSpecifiedFiles) { m_vecSpecifiedFiles = strSpecifiedFiles; }
	vector<string> getSpecifiedFiles() { return m_vecSpecifiedFiles; }

	void setListFileName(const string& strListFileName) { m_strListFileName = strListFileName; }
	string getListFileName() { return m_strListFileName; }

private:

	////////////////
	// Variables
	////////////////

	enum FileListSource { eSpecifiedFiles = 0, eListFile = 1 } m_eFileListSource;
	vector<string> m_vecSpecifiedFiles;
	string m_strListFileName;
};

