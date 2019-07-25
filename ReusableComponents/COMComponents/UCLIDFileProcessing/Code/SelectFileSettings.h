#pragma once
//-------------------------------------------------------------------------------------------------
// SelectFilesSettings.h : header file
// 
// Contains the definition of the SelectFileSettings class
//-------------------------------------------------------------------------------------------------
#include "SelectFileCondition.h"

#include <COMUtils.h>
#include <UCLIDException.h>

#include <string>
#include <vector>

using namespace std;

class SelectFileSettings
{
private:

	// The set of conditions to be applied when selecting files.
	vector<SelectFileCondition*> m_vecConditions;

	// Whether multiple conditions should be and'd or or'd together.
	bool m_bAnd;

	// Values for the random subset selection restriction
	bool m_bLimitToSubset; // Whether to narrow the selection to a subset (random, top or bottom)
	bool m_bSubsetIsRandom; // Whether the subset should be random or in order.
	bool m_bTopSubset; // Whether a non-random subset should be the first results (as opposed to the
					   // last). Used only when m_bSubsetIsRandom is false.
	bool m_bSubsetUsePercentage; // Whether to narrow by percentage or file count
	int m_nSubsetSize; // The size of the subset (percentage or filecount)
	int m_nOffset; // The number of rows to skip before the subset is started

public:
	// Default the setting to all files
	SelectFileSettings();
	SelectFileSettings(const SelectFileSettings& settings);

	~SelectFileSettings();

	SelectFileSettings & operator = (const SelectFileSettings &source);

	void addCondition(SelectFileCondition* pCondition)
		{ m_vecConditions.push_back(pCondition); }
	const vector<SelectFileCondition*>& getConditions() { return m_vecConditions; }
	void deleteCondition(int nIndex);
	void clearConditions();

	void setConjunction(bool bAnd) { m_bAnd = bAnd; }
	bool getConjunction() { return m_bAnd; }

	void setLimitToSubset(bool bLimitToSubset) { m_bLimitToSubset = bLimitToSubset; }
	bool getLimitToSubset() { return m_bLimitToSubset; }

	void setSubsetIsRandom(bool bSubsetIsRandom) { m_bSubsetIsRandom = bSubsetIsRandom; }
	bool getSubsetIsRandom() { return m_bSubsetIsRandom; }

	// Ignored unless SubsetIsRandom is false
	void setSubsetIsTop(bool bTopSubset) { m_bTopSubset = bTopSubset; }
	bool getSubsetIsTop() { return m_bTopSubset; }

	void setSubsetUsePercentage(bool bUsePercentage) { m_bSubsetUsePercentage = bUsePercentage; }
	bool getSubsetUsePercentage() { return m_bSubsetUsePercentage; }

	void setSubsetSize(int nSubsetSize)
	{
		m_nSubsetSize = nSubsetSize;
	}
	int getSubsetSize() { return m_nSubsetSize; }

	void setOffset(int nOffset)
	{
		m_nOffset = nOffset;
	}
	int getOffset() { return m_nOffset; }

	bool selectingAllFiles() { return m_vecConditions.empty() && !m_bLimitToSubset; }

	// Builds the summary string
	// The same ipFAMDB and bIgnoreWorkflows value should be used here as for build query so as to
	// be able to display to the user if results will be limited to a specific workflow.
	string getSummaryString(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB, bool bIgnoreWorkflows);

	// Builds a select query with the specified values selected for the current settings.
	// NOTE: strSelect should contain only the values to be selected by the query, for
	// example strSelect = "FAMFile.ID, FAMFile.FileName" or
	// strSelect = "FAMFile.ID, FAMFile.Priority", etc
	// NOTE2: It can be assumed that the FAMFile table will be included in the query.
	string buildQuery(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB,
					  const string& strSelect, string strOrderByClause, bool bIgnoreWorkflows);

	// Builds a select query with the specified values selected for the current settings
	// in the specified workflow.
	// NOTE: strSelect should contain only the values to be selected by the query, for
	// example strSelect = "FAMFile.ID, FAMFile.FileName" or
	// strSelect = "FAMFile.ID, FAMFile.Priority", etc
	// NOTE2: It can be assumed that the FAMFile table will be included in the query.
	string buildQueryForWorkflow(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB,
		const string& strSelect, long nWorkflowID);

	UCLID_FILEPROCESSINGLib::IRandomMathConditionPtr getRandomCondition();
};