#pragma once

#include <string>
#include <vector>
#include <set>
#include "DocPageCache.h"

using namespace std;

enum EConfidenceLevel{kZero = 0, kMaybe, kProbable, kSure};

class PatternHolder
{
public:
	PatternHolder();
	PatternHolder(const PatternHolder& objToCopy);
	PatternHolder& operator=(const PatternHolder& objToAssign);

	~PatternHolder();

	////////////
	// Methods
	////////////
	// Based on given variables, if one or more of
	// patterns from vec is found, then return true, 
	// otherwise return false.
	bool foundPatternsInText(const ISpatialStringPtr& ipInputText, DocPageCache& cache);

	// Returns true if Rule ID portion of strIdPlusPattern is not found within 
	// m_setRuleIDs, false otherwise.  Returns true if Rule ID is not found
	// within strIdPlusPattern.
	bool isUniqueRuleID(const string& strIDPlusPattern);

	//////////////
	// Variables
	///////////////
	// confidence level for this group of patterns
	EConfidenceLevel m_eConfidenceLevel;

	// whether or not it's OR or AND relationship in between patterns
	bool m_bIsAndRelationship;

	// If a FindX block, how many matches are required.
	int m_nFindXRequirement;

	// vector of patterns
	vector<string> m_vecPatterns;

	// search scope
	double m_dStartingRange, m_dEndingRange;

	// case sensitivity
	bool m_bCaseSensitive;

	// page scope
	int m_nStartPage, m_nEndPage;

	// NEW FORMAT: Block ID
	string	m_strBlockID;

	// NEW FORMAT: Sub-Type
	string	m_strSubType;

	// NEW FORMAT: Rule ID
	string	m_strRuleID;

private:

	// Misc utils pointer
	IMiscUtilsPtr m_ipMisc;

	// vector of Rule IDs associated with m_vecPatterns
	set<string> m_setRuleIDs;

	string getInputText(const ISpatialStringPtr& ipInputText, DocPageCache& cache);

	// Gets a regular expression parser with appropriate case sensitive flag set
	IRegularExprParserPtr getParser();
};
