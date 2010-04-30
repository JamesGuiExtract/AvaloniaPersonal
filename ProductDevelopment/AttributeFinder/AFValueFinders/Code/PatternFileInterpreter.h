///////////////////////////////////////////////////////////////////////////////////////
// This object stores in predefined variables and patterns. It is able to create a
// vector of attributes against a certain input text if one of the patterns is found
// in the input text.
///////////////////////////////////////////////////////////////////////////////////////

#pragma once

#include <string>
#include <vector>

class CommentedTextFileReader;

class PatternFileInterpreter
{
public:
	PatternFileInterpreter();
	PatternFileInterpreter(const PatternFileInterpreter& objToCopy);
	PatternFileInterpreter& operator=(const PatternFileInterpreter& objToAssign);

	///////////
	// Methods
	///////////
	// Check if any one of the patterns matches text in the input
	// Return : true - found match
	// ipAttributes - if match is found, return a vector of attributes. 
	//				  This parameter shall not be NULL
	// strPatternID - if match is found, the pattern ID of which is the match pattern
	bool foundPattern(IStringPatternMatcherPtr ipSPM, 
					  UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder,
					  ISpatialStringPtr ipInputText,
					  IIUnknownVectorPtr& ripAttributes,
					  std::string& rstrPatternID);

	// read input and store the variables and patterns definitions
	// bInputIsFile - whether or not strInput is a file name
	//				  true - treat strInput as a file name
	//				  false - treat strInput as a block of text
	// bClearPatterns - whether or not to clear all stored variables and patterns
	void readPatterns(const std::string& strInput, 
					  bool bInputIsFile,
					  bool bClearPatterns);

	// set preprocessors if any
	void setPreprocessors(IVariantVectorPtr ipPreprocessors);

private:
	///////////
	// Structs
	///////////
	// pair of strings for pattern ID and pattern text
	struct IDToPattern
	{
		std::string m_strPatternID;
		std::string m_strPatternText;
	};

	////////////
	// Constants
	////////////
	static const std::string IMPORT_TAG;
	static const std::string VARS_BEGIN;
	static const std::string VARS_END;
	static const std::string PATTERNS_BEGIN;
	static const std::string PATTERNS_END;

	////////////
	// Variables
	////////////
	// map of variable name to variable definition
	IStrToStrMapPtr m_ipVariables;

	// vector of IDToPattern
	std::vector<IDToPattern> m_vecPatterns;

	// Provides access to registry settings
	IMiscUtilsPtr	m_ipMiscUtils;

	// preprocessors defined by the user to be used while input text/file is read in
	std::vector<std::string> m_vecPreprocessors;

	/////////////
	// Methods
	////////////
	// Convert input into vector of lines (strings)
	std::vector<std::string> convertToLines(const std::string& strInput, bool bIsFile);
	
	// create an Attribute object according to the original input text,
	// the found text information (which contains the text and the position info),
	// and the attribute type.
	IAttributePtr generateAttribute(ISpatialStringPtr ipOriginalText, 
									ITokenPtr ipFoundTextInfo,  
									const std::string& strAttributeType);

	// extract the import file name in full from the import statement
	std::string getImportFileName(const std::string& strImportStatement,
								  const std::string& strCurrentFileName);

	// if a #ifdef is encountered, its following lines need to be
	// treated separately until its #endif is encountered
	// bSkipThisBlock - whether or not to skip this block
	// Note: "this block" refers to the block text inside #ifdef - #else,
	// #else - #endif or #ifdef - #endif
	void loadIfBlock(CommentedTextFileReader& rFileReader, 
		int& rnLevelOfNestedIfBlock, bool bSkipThisBlock);

	// Load all patterns within PATTERN block and store
	// them in m_vecPatterns
	void loadPatterns(CommentedTextFileReader& rFileReader);

	// Load all pre-defined variables within VARIABLE block 
	// and store them in m_ipVaraibles
	void loadVariables(CommentedTextFileReader& rFileReader);

	// stores the pattern line text
	void storePatternLine(const std::string& strPatternLineText);

	// validate the pair of #ifdef...#else...#endif or #ifdef...#endif in the input
	void validateDirectives(std::vector<std::string> vecLines);
};