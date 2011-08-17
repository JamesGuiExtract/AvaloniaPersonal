///////////////////////////////////////////////////////////////////////////////////////
// This object stores in predefined variables and patterns. It is able to create a
// vector of attributes against a certain input text if one of the patterns is found
// in the input text.
///////////////////////////////////////////////////////////////////////////////////////

#pragma once

#include <string>
#include <vector>
#include <set>
using namespace std;

class CommentedTextFileReader;

class RegExPatternFileInterpreter
{
public:
	RegExPatternFileInterpreter();
	RegExPatternFileInterpreter(const RegExPatternFileInterpreter& objToCopy);
	RegExPatternFileInterpreter& operator=(const RegExPatternFileInterpreter& objToAssign);

	///////////
	// Methods
	///////////
	// Check if any one of the patterns matches text in the input
	// Return : true - found match
	// ipAttributes - if match is found, return a vector of attributes. 
	//				  This parameter shall not be NULL
	// strPatternID - if match is found, the pattern ID of which is the match pattern
	bool foundPattern(IRegularExprParserPtr ipRegExpParser, 
					  UCLID_AFVALUEFINDERSLib::IREPMFinderPtr ipREPMFinder,
					  ISpatialStringPtr ipInputText,
					  IIUnknownVectorPtr& ripAttributes,
					  string& rstrPatternID);

	// read input and store the variables and patterns definitions
	// bClearPatterns - whether or not to clear all stored variables and patterns
	void readPatterns(const string& strInputFile, bool bClearPatterns);

private:
	///////////
	// Structs
	///////////
	// pair of strings for pattern ID and pattern text
	struct IDToPattern
	{
		string m_strPatternID;
		string m_strPatternText;
	};

	////////////
	// Constants
	////////////
	static const string IMPORT_TAG;

	////////////
	// Variables
	////////////

	// vector of IDToPattern
	vector<IDToPattern> m_vecPatterns;

	// The pattens that have been defined (to prevent duplicates).
	set<string> m_setDefinedPatterns;

	// Provides access to registry settings
	IMiscUtilsPtr	m_ipMiscUtils;

	/////////////
	// Methods
	////////////
	// Convert input into vector of lines (strings)
	vector<string> convertToLines(const string& strInput, bool bIsFile);

	// extract the import file name in full from the import statement
	string getImportFileName(const string& strImportStatement,
						     const string& strCurrentFileName);

	// Removes comments from the specified file and evaluates #import directives to produce a vector
	// of uncommented lines that are ready to process.
	vector<string> parseCommentsAndImports(const string& strInputFile);

	// Generates an attribute based on the provided regular expression parser match token and the
	// source input string.
	IAttributePtr createAttribute(ITokenPtr ipToken, ISpatialStringPtr ipInput);
};