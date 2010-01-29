// MERSFinder.h: interface for the MERSFinder class.
//
//////////////////////////////////////////////////////////////////////
#pragma once

#include <string>
using namespace std;

class MERSFinder  
{
public:
	MERSFinder();
	virtual ~MERSFinder();

	// Looks for MERS keywords in attributes and if found the keyword is the value
	// and then it looks for NomineeFor if found it is added as a sub attribute
	void findMERS( IIUnknownVectorPtr ipVecEntities, IAFDocumentPtr ipAFDoc );

private:
	//////////
	// Methods
	//////////

	//=============================================================================
	// Finds and adds the NomineeFor SubAttribute for MERS 
	void findNomineeFor( IAttributePtr ipAttr, IAFDocumentPtr ipAFDoc,
		IRegularExprParserPtr ipParser);

	//=============================================================================
	// Extracts entity out from input text, return NULL if no entity is found
	ISpatialStringPtr extractEntity(ISpatialStringPtr ipInputText);
	
	//=============================================================================
	// Takes the input text and return the found match entity,
	// NULL if nothing is found
	ISpatialStringPtr findMatchEntity(ISpatialString* pInputText, 
		const std::string& strPattern,	// pattern for searching
		bool bCaseSensitive,			// case sensitivity
		bool bExtractEntity,			// whether or not to extract entity
		bool bGreedySearch);			// whether SPM search should be greedy

	//=============================================================================
	// If certain MERS keywords exist in the input
	// If found, return the start and end positions for the keyword
	bool findMERSKeyword(const std::string& strInput, int& nStartPos, int& nEndPos,
		IRegularExprParserPtr ipParser);

	//=============================================================================
	// Look for the first word (i.e. ignore any leading non-alpha chars if any). And
	// check to see if the first letter of that word is in upper case.
	bool isFirstWordStartsUpperCase(const std::string& strInput);

	//=============================================================================
	// Use StringPatternMatcher to find pattern in strInput and return 
	// found variable matches
	IStrToObjectMapPtr match(ISpatialStringPtr& ripInput, 
		const std::string& strPattern, bool bCaseSensitive, bool bGreedy);

	//=============================================================================
	// search based on pattern set 1
	// Return : found spatial string, NULL if nothing is found
	ISpatialStringPtr patternSearch1(ISpatialString* pOriginalText);

	//=============================================================================
	// search based on pattern set 2
	// Return : found spatial string, NULL if nothing is found
	ISpatialStringPtr patternSearch2(ISpatialString* pOriginalText);

	//=============================================================================
	// search based on pattern set 3
	// Return : found spatial string, NULL if nothing is found
	ISpatialStringPtr patternSearch3(ISpatialString* pOriginalText);

	//=============================================================================
	// Sets predefined expressions for String Pattern Matcher
	void setPredefinedExpressions();

	//=============================================================================
	IRegularExprParserPtr getMERSParser();

	/////////////
	// Variables
	/////////////
	UCLID_AFUTILSLib::IEntityFinderPtr m_ipEntityFinder;
	IStringPatternMatcherPtr m_ipSPM;
	IStrToStrMapPtr m_ipExprDefined;

	IDataScorerPtr	m_ipDataScorer;

	IMiscUtilsPtr m_ipMiscUtils;


};
