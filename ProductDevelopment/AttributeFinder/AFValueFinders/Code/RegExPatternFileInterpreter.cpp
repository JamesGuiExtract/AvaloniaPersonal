
#include "stdafx.h"
#include "RegExPatternFileInterpreter.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <CommentedTextFileReader.h>
#include <EncryptedFileManager.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <comutils.h>
#include <StringTokenizer.h>

#include <fstream>
#include <io.h>

const string RegExPatternFileInterpreter::IMPORT_TAG = "#import";

//-------------------------------------------------------------------------------------------------
// RegExPatternFileInterpreter
//-------------------------------------------------------------------------------------------------
RegExPatternFileInterpreter::RegExPatternFileInterpreter()
{
}
//-------------------------------------------------------------------------------------------------
RegExPatternFileInterpreter::RegExPatternFileInterpreter(const RegExPatternFileInterpreter& objToCopy)
{
	m_vecPatterns = objToCopy.m_vecPatterns;
	m_setDefinedPatterns = objToCopy.m_setDefinedPatterns;
}
//-------------------------------------------------------------------------------------------------
RegExPatternFileInterpreter& RegExPatternFileInterpreter::operator=(const RegExPatternFileInterpreter& objToAssign)
{
	m_vecPatterns = objToAssign.m_vecPatterns;
	m_setDefinedPatterns = objToAssign.m_setDefinedPatterns;
	return *this;
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
bool RegExPatternFileInterpreter::foundPattern(IRegularExprParserPtr ipRegExpParser, 
										  UCLID_AFVALUEFINDERSLib::IREPMFinderPtr ipREPMFinder,
										  ISpatialStringPtr ipInputText,
										  IIUnknownVectorPtr& ripAttributes,
										  string& rstrPatternID)
{
	ASSERT_ARGUMENT("ELI33327", ripAttributes != __nullptr);
	ASSERT_ARGUMENT("ELI33328", ipRegExpParser != __nullptr);
	ASSERT_ARGUMENT("ELI33329", ipREPMFinder != __nullptr);

	// get the return match type from the SPM
	UCLID_AFVALUEFINDERSLib::EPMReturnMatchType eReturnMatchType = 
		ipREPMFinder->ReturnMatchType;

	// get the data scorer object
	IObjectWithDescriptionPtr ipDataScorerObjWithDesc = ipREPMFinder->DataScorer;
	ASSERT_RESOURCE_ALLOCATION("ELI33330", ipDataScorerObjWithDesc != __nullptr);

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
		ASSERT_RESOURCE_ALLOCATION("ELI33331", ipResultAttributes != __nullptr);
	}

	// local variable to keep track of the highest score so far
	long nHighestScore = 0;
	long nMinScoreToConsiderAsMatch = ipREPMFinder->MinScoreToConsiderAsMatch;
	long nMinFirstToConsiderAsMatch = ipREPMFinder->MinFirstToConsiderAsMatch;

	// Create a string to hold the pattern IDs of matching rules
	// This string gets used only in the 'ReturnAllMatches' mode
	string strPatternIDs;

	// iterate through the various pattern rules and see if any
	// of the patterns find the data we're looking for
	for (unsigned int nIndex = 0; nIndex < m_vecPatterns.size(); nIndex++)
	{	
		string strPattern = m_vecPatterns[nIndex].m_strPatternText;

		// run the pattern through the string pattern matcher
		_bstr_t _bstrPattern(strPattern.c_str());
		
		IIUnknownVectorPtr ipSearchResults;

		// Encapsulate call to Match1 to catch exceptions and add more debug info
		try
		{
			try
			{
				ipRegExpParser->Pattern = _bstrPattern;

				// The primary match doesn't matter (and may be zero-length); Use only the named groups.
				ipSearchResults = ipRegExpParser->FindNamedGroups(ipInputText->String, VARIANT_TRUE);
				ASSERT_RESOURCE_ALLOCATION("ELI33358", ipSearchResults != __nullptr);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33332");
		}
		catch(UCLIDException& ue)
		{
			// Add the ruleID to the debug info
			ue.addDebugInfo("RuleID", m_vecPatterns[nIndex].m_strPatternID);
			throw ue;
		}

		IIUnknownVectorPtr ipFoundAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI33354", ipFoundAttributes != __nullptr);
		
		// Did we find any?
		long nSize = ipSearchResults->Size();
		if (nSize > 0)
		{
			// Add each named group returned as a found attribute.
			for (int i = 0; i < nSize; i++)
			{
				ITokenPtr ipToken = ipSearchResults->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI33366", ipToken != __nullptr);
				
				// Ingore metadata or names that are not valid identifiers.
				string strName = asString(ipToken->Name);
				char cFirstChar = strName[0];
				if (!isDigitChar(cFirstChar) && cFirstChar != '_' && !asString(ipToken->Value).empty())
				{
					IAttributePtr ipAttribute = createAttribute(ipToken, ipInputText);
					ASSERT_RESOURCE_ALLOCATION("ELI33367", ipAttribute != __nullptr);

					ipFoundAttributes->PushBack(ipAttribute);
				}
			}

			// we have created a vector of attributes representing
			// the matches.  If a DataScorer has been provided
			// then, get the score of the attributes
			long nThisDataScore = 0;
			if (ipDataScorer != __nullptr)
			{
				// [FlexIDSCore:4801]
				// Make a copy of the attributes before scoring so that any modifications the
				// scorer makes do not end up as part of the result.
				IIUnknownVectorPtr ipFoundAttributesCopy(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI33388", ipFoundAttributesCopy != __nullptr);
				ICopyableObjectPtr ipCopyableObj = ipFoundAttributesCopy;
				ASSERT_RESOURCE_ALLOCATION("ELI33389", ipCopyableObj != __nullptr);
				ipCopyableObj->CopyFrom(ipFoundAttributes);

				// get the score of the data
				nThisDataScore = ipDataScorer->GetDataScore2(ipFoundAttributesCopy);
				
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
				THROW_LOGIC_ERROR_EXCEPTION("ELI33335");
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
void RegExPatternFileInterpreter::readPatterns(const string& strInputFile, bool bClearPatterns)
{
	if (bClearPatterns)
	{
		m_vecPatterns.clear();
		m_setDefinedPatterns.clear();
	}

	bool bPatternOpen = false;
	IDToPattern pattern;
	vector<string> vecCurrentPatternLines;
	
	// convert input into vector of uncommented lines that includes all imported files.
	vector<string> vecLines = parseCommentsAndImports(strInputFile);

	for (size_t i = 0; i < vecLines.size(); i++)
	{ 
		// Retrieve this line
		string strLine = vecLines[i];
		if (strLine.empty())
		{
			continue;
		}

		// Look for lines that define the start of a new pattern: "<pattern>==="
		size_t len = strLine.length();
		if (strLine[0] == '<' && len >= 5)
		{
			size_t nPos = strLine.find(">===");
			if (nPos != string::npos)
			{
				// If there is a pattern open, submit it before opening a new one.
				if (bPatternOpen && vecCurrentPatternLines.size() > 0)
				{
					// Build the regular expression string, removing any whitespace( \f\n\r\t\v)
					pattern.m_strPatternText = asString(vecCurrentPatternLines, true);
					m_vecPatterns.push_back(pattern);
					m_setDefinedPatterns.insert(pattern.m_strPatternID);
				}

				// Create a new pattern and extract the pattern ID.
				pattern = IDToPattern();
				pattern.m_strPatternID = ::trim(strLine.substr(1, nPos - 1), " ", " ");
				vecCurrentPatternLines.clear();
				if (m_setDefinedPatterns.find(pattern.m_strPatternID) != m_setDefinedPatterns.end())
				{
					UCLIDException ue("ELI33369", "Duplicate regular expression pattern matcher pattern defined.");
					ue.addDebugInfo("File", strInputFile, true);
					ue.addDebugInfo("Pattern id", pattern.m_strPatternID, true);
					throw ue;
				}

				bPatternOpen = true;

				// Remove the pattern definition from the current line (and move on to the next line
				// if there's nothing left.
				strLine = ::trim(strLine.substr(nPos + 4), " \t", "");
				if (strLine.empty())
				{
					continue;
				}
			}
		}

		if (bPatternOpen)
		{
			vecCurrentPatternLines.push_back(strLine);
		}
	}

	// If a pattern is open, add the current line to it.
	if (bPatternOpen && vecCurrentPatternLines.size() > 0)
	{
		// Build the regular expression string, removing any whitespace( \f\n\r\t\v)
		pattern.m_strPatternText = asString(vecCurrentPatternLines, true);
		m_vecPatterns.push_back(pattern);
		m_setDefinedPatterns.insert(pattern.m_strPatternID);
	}
}
//-------------------------------------------------------------------------------------------------
vector<string> RegExPatternFileInterpreter::parseCommentsAndImports(const string& strInputFile)
{
	// Create utility object, if needed
	if (m_ipMiscUtils == __nullptr)
	{
		m_ipMiscUtils.CreateInstance( CLSID_MiscUtils );
		ASSERT_RESOURCE_ALLOCATION( "ELI33337", m_ipMiscUtils != __nullptr );
	}

	// perform any appropriate auto-encrypt actions
	m_ipMiscUtils->AutoEncryptFile(_bstr_t(strInputFile.c_str()),
		_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));
		
	// make sure the file exists
	validateFileOrFolderExistence(strInputFile);

	// convert input into vector of lines(strings)
	vector<string> vecLines = convertToLines(strInputFile, true);
	// Provide the input lines to the file reader
	CommentedTextFileReader fileReader(vecLines, "//", true);

	vector<string> vecParsedLines;

	while (!fileReader.reachedEndOfStream())
	{ 
		// Retrieve this line
		string strLine = fileReader.getLineText();

		// Look at the tag at the very beginning of the line to tell if its an import
		if (strLine.find(IMPORT_TAG) == 0)
		{
			// the rest of the line contains the file name
			string strFileToImport = getImportFileName(strLine, strInputFile);
			
			vector<string> vecImportedLines = parseCommentsAndImports(strFileToImport);

			vecParsedLines.insert(vecParsedLines.end(),
				vecImportedLines.begin(), vecImportedLines.end());
		}
		else
		{
			vecParsedLines.push_back(strLine);
		}
	}

	return vecParsedLines;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
vector<string> RegExPatternFileInterpreter::convertToLines(const string& strInput, bool bIsFile)
{
	vector<string> vecLines;

	if (bIsFile)
	{
		// first make sure the file exists
		if (!isValidFile(strInput))
		{
			UCLIDException ue("ELI33339", "Input file doesn't exist.");
			ue.addDebugInfo("Input File", strInput);
			ue.addWin32ErrorInfo();
			throw ue;
		}

		// If input file is an etf file
		if (_strcmpi(getExtensionFromFullPath(strInput).c_str(), ".etf") == 0)
		{
			// Open an input file, which is encrypted
			MapLabelManager encryptedFileManager;
			// decrypt the file
			vecLines = encryptedFileManager.getMapLabel(strInput);
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
string RegExPatternFileInterpreter::getImportFileName(const string& strImportStatement,
													  const string& strCurrentFileName)
{
	string strFileToImport("");

	// find first quote sign
	int nQuotePos = strImportStatement.find("\"");
	if (nQuotePos == string::npos)
	{
		UCLIDException ue("ELI33342", "Invalid <import> statement.");
		ue.addDebugInfo("Text", strImportStatement);
		throw ue;
	}
	// find next quote sign
	int nUnquotePos = strImportStatement.find("\"", nQuotePos+1);
	if (nUnquotePos == string::npos)
	{
		UCLIDException ue("ELI33343", "Invalid <import> statement.");
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
IAttributePtr RegExPatternFileInterpreter::createAttribute(ITokenPtr ipToken, ISpatialStringPtr ipInput)
{
	ASSERT_ARGUMENT("ELI33359", ipToken != __nullptr);
	ASSERT_ARGUMENT("ELI33360", ipInput != __nullptr);

	long nStart, nEnd;
	ipToken->GetTokenInfo(&nStart, &nEnd, NULL, NULL);

	// create an attribute to store the value
	IAttributePtr ipAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI33361", ipAttribute != __nullptr);

	ISpatialStringPtr ipMatch;
	if (nStart != -1 && nEnd >= nStart)
	{
		// create a spatial string representing the match
		ipMatch = ipInput->GetSubString(nStart, nEnd);
		ASSERT_RESOURCE_ALLOCATION("ELI33362", ipMatch != __nullptr);
	}
	else
	{
		// It currently isn't actually possible to get an empty match result, but keeping this here
		// in case that behavior changes.
		ipMatch.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI33363", ipMatch != __nullptr);
		ipMatch->CreateNonSpatialString(ipToken->Value, "");
	}

	// Set the match as the value of the attribute, 
	ipAttribute->Value = ipMatch;
	
	// If the name of the token is not of zero length use it to set the attribute type.
	if (ipToken->Name.length() > 0)
	{
		// Get the name from the token
		ipAttribute->Type = ipToken->Name;
	}

	return ipAttribute;
}
//-------------------------------------------------------------------------------------------------	
