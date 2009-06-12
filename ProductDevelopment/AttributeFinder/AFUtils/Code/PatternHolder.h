#pragma once

#include <string>
#include <vector>
#include "DocPageCache.h"

enum EConfidenceLevel{kZero = 0, kMaybe, kProbable, kSure};

class PatternHolder
{
public:
	PatternHolder(IRegularExprParserPtr ipRegExpr);
	PatternHolder(const PatternHolder& objToCopy);
	PatternHolder& operator=(const PatternHolder& objToAssign);

	////////////
	// Methods
	////////////
	// Based on given variables, if one or more of
	// patterns from vec is found, then return true, 
	// otherwise return false.
	bool foundPatternsInText(ISpatialStringPtr ipInputText, DocPageCache& cache);

	// Returns true if Rule ID portion of strIdPlusPattern is not found within 
	// m_vecRuleIDs, false otherwise.  Returns false if Rule ID is not found
	// within strIdPlusPattern.
	bool isUniqueRuleID(std::string strIDPlusPattern);

	//////////////
	// Variables
	///////////////
	// confidence level for this group of patterns
	EConfidenceLevel m_eConfidenceLevel;

	// whether or not it's OR or AND relationship in between patterns
	bool m_bIsAndRelationship;

	// vector of patterns
	std::vector<std::string> m_vecPatterns;

	// search scope
	double m_dStartingRange, m_dEndingRange;

	// case sensitivity
	bool m_bCaseSensitive;

	// page scope
	int m_nStartPage, m_nEndPage;

	// NEW FORMAT: Block ID
	std::string	m_strBlockID;

	// NEW FORMAT: Sub-Type
	std::string	m_strSubType;

	// NEW FORMAT: Rule ID
	std::string	m_strRuleID;

private:

	// vector of Rule IDs associated with m_vecPatterns
	std::vector<std::string> m_vecRuleIDs;

	std::string getInputText(ISpatialStringPtr ipInputText, DocPageCache& cache);

	IRegularExprParserPtr m_ipRegExpr;
};
