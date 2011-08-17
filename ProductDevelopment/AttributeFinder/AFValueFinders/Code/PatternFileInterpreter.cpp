
#include "stdafx.h"
#include "PatternFileInterpreter.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <CommentedTextFileReader.h>
#include <EncryptedFileManager.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <comutils.h>
#include <StringTokenizer.h>

#include <fstream>
#include <io.h>

using namespace std;

const string PatternFileInterpreter::IMPORT_TAG = "#import";
const string PatternFileInterpreter::VARS_BEGIN = "[VARS_BEGIN]";
const string PatternFileInterpreter::VARS_END = "[VARS_END]";
const string PatternFileInterpreter::PATTERNS_BEGIN = "[PATTERNS_BEGIN]";
const string PatternFileInterpreter::PATTERNS_END = "[PATTERNS_END]";

//-------------------------------------------------------------------------------------------------
// PatternFileInterpreter
//-------------------------------------------------------------------------------------------------
PatternFileInterpreter::PatternFileInterpreter()
: m_ipVariables(NULL)
{
}
//-------------------------------------------------------------------------------------------------
PatternFileInterpreter::PatternFileInterpreter(const PatternFileInterpreter& objToCopy)
{
	if (objToCopy.m_ipVariables)
	{
		ICopyableObjectPtr ipVariablesCopy(objToCopy.m_ipVariables);
		m_ipVariables = ipVariablesCopy->Clone();
	}
	m_vecPatterns = objToCopy.m_vecPatterns;
}
//-------------------------------------------------------------------------------------------------
PatternFileInterpreter& PatternFileInterpreter::operator=(const PatternFileInterpreter& objToAssign)
{
	if (objToAssign.m_ipVariables)
	{
		ICopyableObjectPtr ipVariablesCopy(objToAssign.m_ipVariables);
		m_ipVariables = ipVariablesCopy->Clone();
	}

	m_vecPatterns = objToAssign.m_vecPatterns;
	return *this;
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
bool PatternFileInterpreter::foundPattern(IStringPatternMatcherPtr ipSPM, 
										  UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder,
										  ISpatialStringPtr ipInputText,
										  IIUnknownVectorPtr& ripAttributes,
										  string& rstrPatternID)
{
	ASSERT_ARGUMENT("ELI07165", ripAttributes != __nullptr);
	ASSERT_ARGUMENT("ELI08644", ipSPM != __nullptr);
	ASSERT_ARGUMENT("ELI08645", ipSPMFinder != __nullptr);

	// get the return match type from the SPM
	UCLID_AFVALUEFINDERSLib::EPMReturnMatchType eReturnMatchType = 
		ipSPMFinder->ReturnMatchType;

	// get the data scorer object
	IObjectWithDescriptionPtr ipDataScorerObjWithDesc = ipSPMFinder->DataScorer;
	ASSERT_RESOURCE_ALLOCATION("ELI08641", ipDataScorerObjWithDesc != __nullptr);

	IDataScorerPtr ipDataScorer = ipDataScorerObjWithDesc->Object;
	
	// create a local vector to store the result attributes
	// This vector gets used only in the 'ReturnAllMatches' or
	// 'ReturnBestMatch' mode
	IIUnknownVectorPtr ipResultAttributes;
	if (eReturnMatchType == UCLID_AFVALUEFINDERSLib::kReturnAllMatches 
		|| eReturnMatchType == UCLID_AFVALUEFINDERSLib::kReturnFirstOrBest
		|| eReturnMatchType == UCLID_AFVALUEFINDERSLib::kReturnBestMatch)
	{
		ipResultAttributes.CreateInstance(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI08642", ipResultAttributes != __nullptr);
	}

	// local variable to keep track of the highest score so far
	long nHighestScore = 0;
	long nMinScoreToConsiderAsMatch = ipSPMFinder->MinScoreToConsiderAsMatch;
	long nMinFirstToConsiderAsMatch = ipSPMFinder->MinFirstToConsiderAsMatch;

	// Create a string to hold the pattern IDs of matching rules
	// This string gets used only in the 'ReturnAllMatches' mode
	string strPatternIDs;

	VARIANT_BOOL vbGreedySearch = ipSPMFinder->GreedySearch;

	// iterate through the various pattern rules and see if any
	// of the patterns find the data we're looking for
	for (unsigned int nIndex = 0; nIndex < m_vecPatterns.size(); nIndex++)
	{	
		string strPattern = m_vecPatterns[nIndex].m_strPatternText;

		// run the pattern through the string pattern matcher
		_bstr_t _bstrPattern(strPattern.c_str());
		
		IStrToObjectMapPtr ipFoundMatches;

		// Encapsulate call to Match1 to catch exceptions and add more debug info
		try
		{
			try
			{
				// find match using string pattern matcher
				// the return value is a String to Object Map of of IToken objects
				ipFoundMatches = ipSPM->Match1(ipInputText->String, 
					_bstrPattern, m_ipVariables, vbGreedySearch);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16860");
		}
		catch(UCLIDException& ue)
		{
			// Add the ruleID to the debug info
			ue.addDebugInfo("RuleID", m_vecPatterns[nIndex].m_strPatternID);
			throw ue;
		}
		
		long nSize = ipFoundMatches->Size;
		
		// count the number of expected matches by counting the number
		// of ? characters
		int iNumExpectedMatches = getCountOfSpecificCharInString(strPattern, '?');
		
		// if only number of found matches equals to the number of 
		// attribute types for that specific pattern
		if (nSize == iNumExpectedMatches)
		{
			// create a local vector to store the found attributes in this
			// iteration of pattern matching
			IIUnknownVectorPtr ipFoundAttributes(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI08640", ipFoundAttributes != __nullptr);

			// Create each Attribute
			for (long n = 0; n < nSize; n++)
			{
				// get the match variable name and match value (which is an IToken object)
				CComBSTR bstrMatchVariableName;
				IUnknownPtr ipUnknown;
				ipFoundMatches->GetKeyValue(n, &bstrMatchVariableName, &ipUnknown);
				ITokenPtr ipToken = ipUnknown;
				ASSERT_RESOURCE_ALLOCATION("ELI07161", ipToken != __nullptr);
				
				// get the related type name
				string strAttributeType = asString(bstrMatchVariableName);
				
				// create an attribute
				IAttributePtr ipAttribute 
					= generateAttribute(ipInputText, ipToken, strAttributeType);
				if (ipAttribute)
				{
					ipFoundAttributes->PushBack(ipAttribute);
				}
			}

			// we have created a vector of attributes representing
			// the matches.  If a DataScorer has been provided
			// then, get the score of the attributes
			long nThisDataScore = 0;
			if (ipDataScorer != __nullptr)
			{
				// get the score of the data
				nThisDataScore = ipDataScorer->GetDataScore2(ipFoundAttributes);
				
				// if it does not meet the minimum standards, 
				// ignore it
				if (nThisDataScore < nMinScoreToConsiderAsMatch)
				{
					continue;
				}
			}
			
			// depending upon the return match type in the SPM,
			// either return the current value, or store it for further
			// analysis
			switch (eReturnMatchType)
			{
			case UCLID_AFVALUEFINDERSLib::kReturnFirstMatch:
				// return this match's data.  No more processing to do.
				ripAttributes = ipFoundAttributes;
				rstrPatternID = m_vecPatterns[nIndex].m_strPatternID;
				return true;
			case UCLID_AFVALUEFINDERSLib::kReturnFirstOrBest:
				if ( nThisDataScore >= nMinFirstToConsiderAsMatch )
				{
					ripAttributes = ipFoundAttributes;
					rstrPatternID = m_vecPatterns[nIndex].m_strPatternID;
					return true;
				}
				// if not found perform check for Best Match
			case UCLID_AFVALUEFINDERSLib::kReturnBestMatch:
				if (nThisDataScore > nHighestScore)
				{
					// reset the "best attributes" list
					ipResultAttributes = ipFoundAttributes;
					nHighestScore = nThisDataScore;

					// reset the best attribute pattern ID
					strPatternIDs = m_vecPatterns[nIndex].m_strPatternID;
				}
				else if (nThisDataScore == nHighestScore)
				{
					// append to the "best attributes" list
					ipResultAttributes->Append(ipFoundAttributes);
					
					// append to the best attribute pattern ID's
					if (!strPatternIDs.empty())
					{
						strPatternIDs += "|";
					}
					strPatternIDs += m_vecPatterns[nIndex].m_strPatternID;
				}

				break;

			case UCLID_AFVALUEFINDERSLib::kReturnAllMatches:
				// append the found attributes and pattern id
				// to the list of attributes and pattern id's already found
				ipResultAttributes->Append(ipFoundAttributes);
				if (!strPatternIDs.empty())
				{
					strPatternIDs += "|";
				}
				strPatternIDs += m_vecPatterns[nIndex].m_strPatternID;
				break;

			default:
				// we should never reach here as we should have handled
				// all the cases
				THROW_LOGIC_ERROR_EXCEPTION("ELI08643");
			}
		}
	}

	// we should only reach here if no attributes were found, 
	// or if the match-return-type was either "return all matches"
	// or "return best match"
	// if any attributes were found, return them
	if (ipResultAttributes != __nullptr && ipResultAttributes->Size() > 0)
	{
		// update the outer scope variables
		ripAttributes = ipResultAttributes;
		rstrPatternID = strPatternIDs;
		return true;
	}

	// if we reached here, it's because we could not find any satisfactory
	// attributs that match all our criteria
	return false;
}
//-------------------------------------------------------------------------------------------------
void PatternFileInterpreter::readPatterns(const string& strInput,
										  bool bInputIsFile,
										  bool bClearPatterns)
{
	if (m_ipVariables == __nullptr)
	{
		m_ipVariables.CreateInstance(CLSID_StrToStrMap);
		ASSERT_RESOURCE_ALLOCATION("ELI07143", m_ipVariables != __nullptr);
	}

	// Create utility object, if needed
	if (m_ipMiscUtils == __nullptr)
	{
		m_ipMiscUtils.CreateInstance( CLSID_MiscUtils );
		ASSERT_RESOURCE_ALLOCATION( "ELI07640", m_ipMiscUtils != __nullptr );
	}

	if (bClearPatterns)
	{
		m_ipVariables->Clear();
		m_vecPatterns.clear();
	}

	// if input is a file name
	if (bInputIsFile)
	{
		// perform any appropriate auto-encrypt actions
		m_ipMiscUtils->AutoEncryptFile(_bstr_t(strInput.c_str()),
			_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));
		
		// make sure the file exists
		validateFileOrFolderExistence(strInput);
	}
	
	// convert input into vector of lines(strings)
	vector<string> vecLines = convertToLines(strInput, bInputIsFile);

	// validate #ifdef, #else and #endif in the input text if any
	validateDirectives(vecLines);

	// Provide the input lines to the file reader
	CommentedTextFileReader fileReader(vecLines, "//", true);
	string strLine("");
	while (!fileReader.reachedEndOfStream())
	{ 
		// Retrieve this line
		strLine = ::trim(fileReader.getLineText(), " \t", "");
		if (strLine.empty())
		{
			continue;
		}

		// look at the tag at the very beginning of the line
		// to tell if its an import or a pattern or a var
		if (strLine.find(IMPORT_TAG) != string::npos)
		{
			// the rest of the line contains the file name
			string strCurrentFileName("");
			if (bInputIsFile)
			{
				strCurrentFileName = strInput;
			}
			string strFileToImport = getImportFileName(strLine, strCurrentFileName);
			
			// call this method recursively
			readPatterns(strFileToImport, true, false);
		}
		else if (strLine.find(VARS_BEGIN) != string::npos)
		{
			loadVariables(fileReader);
		}
		else if (strLine.find(PATTERNS_BEGIN) != string::npos)
		{
			loadPatterns(fileReader);
		}
		else
		{
			// this is an invalid line
			UCLIDException ue("ELI07156", "This line is not an acceptable format.");
			ue.addDebugInfo("LineText", strLine);
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void PatternFileInterpreter::setPreprocessors(IVariantVectorPtr ipPreprocessors)
{
	m_vecPreprocessors.clear();

	if (ipPreprocessors)
	{
		long nSize = ipPreprocessors->Size;
		for (long n=0; n<nSize; n++)
		{
			// convert into strings
			string strPreprocessor = asString(_bstr_t(ipPreprocessors->GetItem(n)));
			m_vecPreprocessors.push_back(strPreprocessor);
		}
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
vector<string> PatternFileInterpreter::convertToLines(const string& strInput, bool bIsFile)
{
	vector<string> vecLines;

	if (bIsFile)
	{
		// first make sure the file exists
		if (!isValidFile(strInput))
		{
			UCLIDException ue("ELI19337", "Input file doesn't exist.");
			ue.addDebugInfo("Input File", strInput);
			ue.addWin32ErrorInfo();
			throw ue;
		}

		// If input file is an etf file
		if (_strcmpi(getExtensionFromFullPath(strInput).c_str(), ".etf") == 0)
		{
			// Open an input file, which is encrypted
			EncryptedFileManager efm;
			// decrypt the file
			vecLines = efm.decryptTextFile(strInput);
		}
		else
		{
			// treat the input file as ASCII text file
			ifstream ifs(strInput.c_str());
			while (!ifs.eof())
			{
				string strLine("");
				getline(ifs, strLine);
				if (strLine.empty())
				{
					continue;
				}

				// save the line in the vector
				vecLines.push_back(strLine);
			}
		}
	}
	else	// if input is a block of text
	{
		// delimiter is line feed
		StringTokenizer::sGetTokens(strInput, "\r\n", vecLines);
	}

	return vecLines;
}
//-------------------------------------------------------------------------------------------------
IAttributePtr PatternFileInterpreter::generateAttribute(ISpatialStringPtr ipOriginalText, 
														ITokenPtr ipFoundTextInfo,  
														const string& strAttributeType)
{
	long nStartPos = ipFoundTextInfo->StartPosition;
	long nEndPos = ipFoundTextInfo->EndPosition;
	if (nEndPos >= nStartPos)
	{
		// store the position info
		// about the found value in the spatial string. i.e. get 
		// the substring out from original spatial string ipOriginalText
		// and store it in the Attribute.
		ISpatialStringPtr ipAttributeValue = ipOriginalText->GetSubString(nStartPos, nEndPos);
		ASSERT_RESOURCE_ALLOCATION("ELI07162", ipAttributeValue != __nullptr);

		// make sure the entity finder finds something
		if (ipAttributeValue->IsEmpty() == VARIANT_FALSE)
		{
			IAttributePtr ipAttribute(CLSID_Attribute);
			ASSERT_RESOURCE_ALLOCATION("ELI07163", ipAttribute != __nullptr);

			ipAttribute->Type = _bstr_t(strAttributeType.c_str());
			ipAttribute->Value = ipAttributeValue;
			return ipAttribute;
		}
	}

	// return null since no value is found
	return NULL;
}
//-------------------------------------------------------------------------------------------------
string PatternFileInterpreter::getImportFileName(const string& strImportStatement,
												 const string& strCurrentFileName)
{
	string strFileToImport("");

	// find first quote sign
	int nQuotePos = strImportStatement.find("\"");
	if (nQuotePos == string::npos)
	{
		UCLIDException ue("ELI07159", "Invalid <import> statement.");
		ue.addDebugInfo("Text", strImportStatement);
		throw ue;
	}
	// find next quote sign
	int nUnquotePos = strImportStatement.find("\"", nQuotePos+1);
	if (nUnquotePos == string::npos)
	{
		UCLIDException ue("ELI07160", "Invalid <import> statement.");
		ue.addDebugInfo("Text", strImportStatement);
		throw ue;
	}

	// take the file name part out from the import statement
	strFileToImport = strImportStatement.substr(nQuotePos+1, nUnquotePos-nQuotePos-1);
	string strParentDirectory(strCurrentFileName);

	// the import file's path is relative to this file's path
	// so compute the full path
	strFileToImport = ::getAbsoluteFileName(strParentDirectory, strFileToImport);

	// if this doc type file has extension .etf, then convert the import
	// file to have .etf extension
	if (_strcmpi(::getExtensionFromFullPath(strCurrentFileName).c_str(), ".etf") == 0)
	{
		// if the extension of the import filename is not .etf, then
		// get the equivalent import filename with the .etf extension
		if (_strcmpi(::getExtensionFromFullPath(strFileToImport).c_str(), ".etf") != 0)
		{	
			strFileToImport += ".etf";
		}
	}

	return strFileToImport;
}
//-------------------------------------------------------------------------------------------------
void PatternFileInterpreter::loadIfBlock(CommentedTextFileReader& rFileReader,
										 int& rnLevelOfNestedIfBlock,
										 bool bSkipThisBlock)
{
	// what is the level when it is passed in
	int nOriginLevel = rnLevelOfNestedIfBlock;

	string strLine("");
	while (!rFileReader.reachedEndOfStream())
	{
		strLine = ::trim(rFileReader.getLineText(), " \t", "");
		if (strLine.empty())
		{
			continue;
		}

		// if #ifdef, #else or #endif is found
		int nFoundPos = strLine.find("#ifdef");
		if (nFoundPos != string::npos)
		{
			// one nest found
			rnLevelOfNestedIfBlock++;

			if (bSkipThisBlock)
			{
				continue;
			}

			// get the preprocessor defined by this nesting #ifdef
			string strNestedPreproc = strLine.substr(nFoundPos+6);
			strNestedPreproc = ::trim(strNestedPreproc, " \t", " \t");
			vector<string>::iterator it = find(m_vecPreprocessors.begin(), 
										m_vecPreprocessors.end(), strNestedPreproc);
			// recursion
			loadIfBlock(rFileReader, rnLevelOfNestedIfBlock, it == m_vecPreprocessors.end());

			if (rnLevelOfNestedIfBlock == nOriginLevel-1)
			{
				return;
			}

			continue;
		}
		else if (strLine.find("#else") != string::npos)
		{
			if (rnLevelOfNestedIfBlock != nOriginLevel)
			{
				continue;
			}

			// otherwise this top level #else, either load or skip its block
			loadIfBlock(rFileReader, rnLevelOfNestedIfBlock, !bSkipThisBlock);

			if (rnLevelOfNestedIfBlock == nOriginLevel-1)
			{
				return;
			}

			continue;
		}
		else if (strLine.find("#endif") != string::npos)
		{
			if (rnLevelOfNestedIfBlock > 0)
			{
				// close the nesting #ifdef block
				rnLevelOfNestedIfBlock--;
			}

			if (rnLevelOfNestedIfBlock == nOriginLevel-1)
			{
				return;
			}

			continue;
		}

		if (bSkipThisBlock)
		{
			// go to next line
			continue;
		}

		// otherwise store the pattern text on this line
		storePatternLine(strLine);
	}
}
//-------------------------------------------------------------------------------------------------
void PatternFileInterpreter::loadPatterns(CommentedTextFileReader& rFileReader)
{
	string strLine("");

	while (!rFileReader.reachedEndOfStream())
	{ 
		// Retrieve this line
		strLine = ::trim(rFileReader.getLineText(), " \t", "");
		if (strLine.empty())
		{
			continue;
		}

		if (strLine.find(VARS_BEGIN) != string::npos
			|| strLine.find(VARS_END) != string::npos
			|| strLine.find(PATTERNS_BEGIN) != string::npos)
		{
			// shall not have any orphaned start/end tags for VARS/PATTERNS
			UCLIDException ue("ELI07152", "Unexpected start/end tags encountered. Please make sure all tags in this file are paired.");
			ue.addDebugInfo("Line Text", strLine);
			throw ue;
		}

		if (strLine.find(PATTERNS_END) != string::npos)
		{
			// time to return since the end of the patterns block is found
			return;
		}

		// if #ifdef, #else or #endif is found
		int nFoundPos = strLine.find("#ifdef");
		if (nFoundPos != string::npos)
		{
			// get the preprocessor
			string strPreprocessor = strLine.substr(nFoundPos+6);
			strPreprocessor = ::trim(strPreprocessor, " \t", " \t");
			// if the preprocessor is defined by user
			vector<string>::iterator it = find(m_vecPreprocessors.begin(), 
											m_vecPreprocessors.end(), strPreprocessor);

			int nNestedLevel = 1;
			loadIfBlock(rFileReader, nNestedLevel, it == m_vecPreprocessors.end());

			continue;
		}

		storePatternLine(strLine);
	}
}
//-------------------------------------------------------------------------------------------------
void PatternFileInterpreter::loadVariables(CommentedTextFileReader& rFileReader)
{
	string strLine("");
	while (!rFileReader.reachedEndOfStream())
	{ 
		// Retrieve this line
		strLine = ::trim(rFileReader.getLineText(), " \t", "");
		if (strLine.empty())
		{
			continue;
		}

		if (strLine.find(VARS_BEGIN) != string::npos
			|| strLine.find(PATTERNS_BEGIN) != string::npos
			|| strLine.find(PATTERNS_END) != string::npos)
		{
			// shall not have any orphaned start/end tags for VARS/PATTERNS
			UCLIDException ue("ELI07154", "Unexpected start/end tags encountered. Please make sure all tags in this file are paired.");
			ue.addDebugInfo("Line Text", strLine);
			throw ue;
		}

		if (strLine.find(VARS_END) != string::npos)
		{
			// time to return since the end of the variables block is reached
			return;
		}
	
		// parse the line into the variable name and variable definition
		int nEqualSignPos = strLine.find("=");
		if (nEqualSignPos == string::npos)
		{
			UCLIDException ue("ELI07155", "Invalid format of line. An equal sign (=) is expected here. Please refer to Help for correct syntax to define a pattern.");
			ue.addDebugInfo("Line Text", strLine);
			throw ue;
		}

		string strVarName = strLine.substr(0, nEqualSignPos);
		string strVarDefinition = strLine.substr(nEqualSignPos+1);
		// Convert from Normal string to CPP if any
		::convertNormalStringToCppString(strVarDefinition);

		m_ipVariables->Set(_bstr_t(strVarName.c_str()), _bstr_t(strVarDefinition.c_str()));
	}
}
//-------------------------------------------------------------------------------------------------
void PatternFileInterpreter::storePatternLine(const string& strPatternLineText)
{
	// parse the line into the pattern ID and the pattern text
	int nEqualSignPos = strPatternLineText.find("=");
	if (nEqualSignPos == string::npos)
	{
		UCLIDException ue("ELI07153", "Invalid format of line. An equal sign (=) is expected here. Please refer to Help for correct syntax to define a pattern.");
		ue.addDebugInfo("Line Text", strPatternLineText);
		throw ue;
	}
	
	// create a new IDToPattern
	IDToPattern pattern;
	pattern.m_strPatternID = ::trim(strPatternLineText.substr(0, nEqualSignPos), " \t", " \t");
	pattern.m_strPatternText = strPatternLineText.substr(nEqualSignPos+1);
	// Convert from Normal string to CPP if any
	::convertNormalStringToCppString(pattern.m_strPatternText);
	// store the pattern
	m_vecPatterns.push_back(pattern);
}
//-------------------------------------------------------------------------------------------------
void PatternFileInterpreter::validateDirectives(vector<string> vecLines)
{
	CommentedTextFileReader fileReader(vecLines, "//", true);
	string strLine("");
	int nLineNumber = 0;
	unsigned int nLevelOfIfBlocks = 0;
	// each element indicates number of else at this level
	vector<int> vecNumOfElse;
	vecNumOfElse.push_back(0);
	while (!fileReader.reachedEndOfStream())
	{
		nLineNumber++;
		// Retrieve this line
		strLine = ::trim(fileReader.getLineText(), " \t", "");
		if (strLine.empty())
		{
			continue;
		}

		if (strLine.find("#ifdef") != string::npos)
		{
			nLevelOfIfBlocks++;
			if (vecNumOfElse.size() < nLevelOfIfBlocks + 1)
			{
				vecNumOfElse.push_back(0);
			}

			continue;
		}
		else if (strLine.find("#else") != string::npos)
		{
			if (vecNumOfElse.size() < nLevelOfIfBlocks+1 || nLevelOfIfBlocks == 0)
			{
				UCLIDException ue("ELI08813", "Illegal #else is found in the input.");
				ue.addDebugInfo("Line Number", nLineNumber);
				throw ue;
			}

			vecNumOfElse[nLevelOfIfBlocks]++;

			if (vecNumOfElse[nLevelOfIfBlocks] > 1)
			{
				UCLIDException ue("ELI08797", "Illegal #else is found in the input.");
				ue.addDebugInfo("Line Number", nLineNumber);
				throw ue;
			}

			continue;
		}
		else if (strLine.find("#endif") != string::npos)
		{
			if (vecNumOfElse.size() < nLevelOfIfBlocks+1 || nLevelOfIfBlocks == 0)
			{
				UCLIDException ue("ELI08815", "Illegal #endif is found in the input.");
				ue.addDebugInfo("Line Number", nLineNumber);
				throw ue;
			}

			if (vecNumOfElse[nLevelOfIfBlocks] >= 1)
			{
				vecNumOfElse[nLevelOfIfBlocks]--;
			}

			nLevelOfIfBlocks--;
			continue;
		}
	}

	// if number of if blocks is non-zero once this point is reached,
	// the #ifdef, #else, #endif are illegally defined in the input
	if (nLevelOfIfBlocks != 0)
	{
		throw UCLIDException("ELI08798", "Illegal #ifdef, #else or #endif is defined in the input.");
	}
}
//-------------------------------------------------------------------------------------------------
