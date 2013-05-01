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
	bool m_bLimitToSubset; // Whether to narrow the selection to a subset (random or top)
	bool m_bSubsetIsRandom; // Whether the subset should be random or in order.
	bool m_bSubsetUsePercentage; // Whether to narrow by percentage or file count
	int m_nSubsetSize; // The size of the subset (percentage or filecount)

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

	void setSubsetUsePercentage(bool bUsePercentage) { m_bSubsetUsePercentage = bUsePercentage; }
	bool getSubsetUsePercentage() { return m_bSubsetUsePercentage; }

	void setSubsetSize(int nSubsetSize)
	{
		m_nSubsetSize = nSubsetSize;
	}
	int getSubsetSize() { return m_nSubsetSize; }

	bool selectingAllFiles() { return m_vecConditions.empty() && !m_bLimitToSubset; }

	// Builds the summary string
	string getSummaryString();

	// Builds a select query with the specified values selected for the current settings.
	// NOTE: strSelect should contain only the values to be selected by the query, for
	// example strSelect = "FAMFile.ID, FAMFile.FileName" or
	// strSelect = "FAMFile.ID, FAMFile.Priority", etc
	// NOTE2: It can be assumed that the FAMFile table will be included in the query.
	string buildQuery(UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB,
					  const string& strSelect, const string& strOrderByClause);

	UCLID_FILEPROCESSINGLib::IRandomMathConditionPtr getRandomCondition();
};