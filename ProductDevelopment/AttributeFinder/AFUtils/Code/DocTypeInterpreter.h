#pragma once

#include "PatternHolder.h"
#include "DocPageCache.h"
#include <afxmt.h>

#include <vector>
#include <string>

using namespace std;

class CommentedTextFileReader;

///////////////////////////////////////////////////////////////////////////////////////
// This class reads a document type file (either encrypted or not), and store each
// criteria in a PatternHolder. It will return appropriate doc type if found upon request.
///////////////////////////////////////////////////////////////////////////////////////

class DocTypeInterpreter
{
public:
	DocTypeInterpreter();
	DocTypeInterpreter(const DocTypeInterpreter& objToCopy);
	DocTypeInterpreter& operator=(const DocTypeInterpreter& objToAssign);

	~DocTypeInterpreter();

	// Determines if the doc type matches any pattern for the specified confidence level.
	// 0 - Zero
	// 1 - Maybe
	// 2 - Probable
	// 3 - Sure
	// Require: loadDocTypeFile() must be called prior to calling this function
	bool docConfidenceLevelMatches(int nLevel, const ISpatialStringPtr& ipInputText, DocPageCache& cache);

	// load the file and create PatternHolders
	// It will check if the file has .dcc or .etf extension.
	// bClearPatterns - whether or not to clear the vector of patterns before loading
	// the document type file
	void loadDocTypeFile(const string& strDocTypeFile, bool bClearPatterns);

	/////////////
	// Variables
	/////////////
	// store the current doc type name
	string m_strDocTypeName;

	// Store the document sub-type
	string m_strDocSubType;

	// Store the Block ID and Rule ID
	string m_strBlockID;
	string m_strRuleID;

private:
	/////////////
	// Constants
	/////////////
	const static string ImportTag;

	const static string DocTypeInterpreter::ZeroBeginTag;
	const static string ZeroEndTag;
	const static string SureBeginTag;
	const static string SureEndTag;
	const static string ProbableBeginTag;
	const static string ProbableEndTag;
	const static string MaybeBeginTag;
	const static string MaybeEndTag;

	const static string ORBeginTag;
	const static string OREndTag;
	const static string ANDBeginTag;
	const static string ANDEndTag;
	const static string SINGLEBeginTag;
	const static string SINGLEEndTag;
	const static string FINDXOFBeginTag;
	const static string FINDXOFEndTag;

	const static string SCOPEBeginTag;
	const static string SCOPEEndTag;

	////////////
	// Variables
	////////////
	// vector of pattern holders that contain criterias
	vector<PatternHolder> m_vecPatternHolders;

	// vector of Block IDs associated with m_vecPatternHolders
	vector<string> m_vecBlockIDs;

	// Provides access to encryption functionality
	IMiscUtilsPtr	m_ipUtils;

	// Scope information as applied to AND, OR, SINGLE blocks
	string m_strStartingRange;
	string m_strEndingRange;
	string m_strStartPage;
	string m_strEndPage;

	////////////
	// Methods
	////////////

	// check the file extension, decrypt the file if necessary, then
	// read each line and put them into the vector
	vector<string> convertToLines(const string& strInputFileName);

	// extract the import file name in full from the import statement
	string getImportFileName(const string& strImportStatement,
								  const string& strCurrentDocTypeFileName);

	// Returns true and adds item to m_vecBlockIDs if strBlockID is not found 
	// within m_vecBlockIDs, false otherwise.  
	// Returns true if strBlockID is empty.
	bool isUniqueBlockID(string strBlockID);

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
						   const string& strBlockStartingLineText,
						   const string& strEndTagToFind);

	void readBeginTag(const string& strBeginTag, PatternHolder& patternHolder, bool &rbIsFindXBlock);

	void readPageScope(const string& strStartPage,
					   const string& strEndPage,
					   PatternHolder& patternHolder);

	void readSearchScope(const string& strStartPos, 
						 const string strEndPos,
						 PatternHolder& patternHolder);

	void setScopeParameters(const string strLine);
};
