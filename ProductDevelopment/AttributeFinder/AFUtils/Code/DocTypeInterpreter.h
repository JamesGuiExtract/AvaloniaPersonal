#pragma once

#include "PatternHolder.h"
#include "DocPageCache.h"
#include <afxmt.h>

#include <vector>
#include <string>

class CommentedTextFileReader;

///////////////////////////////////////////////////////////////////////////////////////
// This class reads a document type file (either encrypted or not), and store each
// criteria in a PatternHolder. It will return appropriate doc type if found upon request.
///////////////////////////////////////////////////////////////////////////////////////

class DocTypeInterpreter
{
public:
	DocTypeInterpreter(IRegularExprParserPtr ipRegExpr);
	DocTypeInterpreter(const DocTypeInterpreter& objToCopy);
	DocTypeInterpreter& operator=(const DocTypeInterpreter& objToAssign);

	// Which confidence level does the input text at
	// 0 - Zero
	// 1 - Maybe
	// 2 - Probable
	// 3 - Sure
	// Require: loadDocTypeFile() must be called prior to calling this function
	int getDocConfidenceLevel(ISpatialStringPtr ipInputText, DocPageCache& cache);

	// load the file and create PatternHolders
	// It will check if the file has .dcc or .etf extension.
	// bClearPatterns - whether or not to clear the vector of patterns before loading
	// the document type file
	void loadDocTypeFile(const std::string& strDocTypeFile, bool bClearPatterns);

	/////////////
	// Variables
	/////////////
	// store the current doc type name
	std::string m_strDocTypeName;

	// Store the document sub-type
	std::string m_strDocSubType;

	// Store the Block ID and Rule ID
	std::string m_strBlockID;
	std::string m_strRuleID;

	// Provides access to encryption functionality
	IMiscUtilsPtr	m_ipUtils;

private:
	/////////////
	// Constants
	/////////////
	const static std::string ImportTag;

	const static std::string DocTypeInterpreter::ZeroBeginTag;
	const static std::string ZeroEndTag;
	const static std::string SureBeginTag;
	const static std::string SureEndTag;
	const static std::string ProbableBeginTag;
	const static std::string ProbableEndTag;
	const static std::string MaybeBeginTag;
	const static std::string MaybeEndTag;

	const static std::string ORBeginTag;
	const static std::string OREndTag;
	const static std::string ANDBeginTag;
	const static std::string ANDEndTag;
	const static std::string SINGLEBeginTag;
	const static std::string SINGLEEndTag;

	const static std::string SCOPEBeginTag;
	const static std::string SCOPEEndTag;

	////////////
	// Variables
	////////////
	// vector of pattern holders that contain criterias
	std::vector<PatternHolder> m_vecPatternHolders;

	// vector of Block IDs associated with m_vecPatternHolders
	std::vector<std::string> m_vecBlockIDs;

	IRegularExprParserPtr m_ipRegExpr;

	// Scope information as applied to AND, OR, SINGLE blocks
	std::string m_strStartingRange;
	std::string m_strEndingRange;
	std::string m_strStartPage;
	std::string m_strEndPage;

	// This mutext is used to protect against loading data in multiple threads
	CMutex m_mutex;

	////////////
	// Methods
	////////////

	// check the file extension, decrypt the file if necessary, then
	// read each line and put them into the vector
	std::vector<std::string> convertToLines(const std::string& strInputFileName);

	// extract the import file name in full from the import statement
	std::string getImportFileName(const std::string& strImportStatement,
								  const std::string& strCurrentDocTypeFileName);

	// Returns true and adds item to m_vecBlockIDs if strBlockID is not found 
	// within m_vecBlockIDs, false otherwise.  
	// Returns true if strBlockID is empty.
	bool isUniqueBlockID(std::string strBlockID);

	// loads a confidence level block, ex, Sure, Probable, etc.
	// which has OR block, AND block and/or SINGLE block
	// into the m_vecPatternHolders
	void loadConfidenceLevelBlocks(CommentedTextFileReader& fileReader,
								   EConfidenceLevel eConfidenceLevel);

	// loads each OR, AND or SINGLE block, which contains all patterns
	// into the m_vecPatternHolders
	// strBlockStartingLineText --  the first line of that block
	//								ex, "OR_BEGIN,0,1,true"
	// strEndTagToFind -- For the given block, what's the expecting end tag name
	//					  For instance, if the block is a AND block, then the 
	//					  expecting end tag name is AND_END
	void loadPatternsBlock(CommentedTextFileReader& fileReader,
						   EConfidenceLevel eConfidenceLevel,
						   const std::string& strBlockStartingLineText,
						   const std::string& strEndTagToFind);

	void readBeginTag(const std::string& strBeginTag, PatternHolder& patternHolder);

	void readPageScope(const std::string& strStartPage,
					   const std::string& strEndPage,
					   PatternHolder& patternHolder);

	void readSearchScope(const std::string& strStartPos, 
						 const std::string strEndPos,
						 PatternHolder& patternHolder);

	void setScopeParameters(const std::string strLine);
};
