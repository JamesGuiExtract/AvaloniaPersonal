
#include "stdafx.h"
#include "AFUtils.h"
#include "DocTypeInterpreter.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <CommentedTextFileReader.h>
#include <EncryptedFileManager.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <StringTokenizer.h>

#include <fstream>
#include <io.h>

using namespace std;

const string DocTypeInterpreter::ImportTag = "#import";

const string DocTypeInterpreter::ZeroBeginTag = "[ZERO_BEGIN]";
const string DocTypeInterpreter::ZeroEndTag = "[ZERO_END]";
const string DocTypeInterpreter::SureBeginTag = "[SURE_BEGIN]";
const string DocTypeInterpreter::SureEndTag = "[SURE_END]";
const string DocTypeInterpreter::ProbableBeginTag = "[PROBABLE_BEGIN]";
const string DocTypeInterpreter::ProbableEndTag = "[PROBABLE_END]";
const string DocTypeInterpreter::MaybeBeginTag = "[MAYBE_BEGIN]";
const string DocTypeInterpreter::MaybeEndTag = "[MAYBE_END]";

const string DocTypeInterpreter::ORBeginTag = "OR_BEGIN";
const string DocTypeInterpreter::OREndTag = "OR_END";
const string DocTypeInterpreter::ANDBeginTag = "AND_BEGIN";
const string DocTypeInterpreter::ANDEndTag = "AND_END";
const string DocTypeInterpreter::SINGLEBeginTag = "SINGLE_BEGIN";
const string DocTypeInterpreter::SINGLEEndTag = "SINGLE_END";
const string DocTypeInterpreter::FINDXOFBeginTag = "FINDXOF_BEGIN";
const string DocTypeInterpreter::FINDXOFEndTag = "FINDXOF_END";

const string DocTypeInterpreter::SCOPEBeginTag = "SCOPE_BEGIN";
const string DocTypeInterpreter::SCOPEEndTag = "SCOPE_END";

//-------------------------------------------------------------------------------------------------
// DocTypeInterpreter
//-------------------------------------------------------------------------------------------------
DocTypeInterpreter::DocTypeInterpreter()
: m_ipUtils(NULL)
{
}
//-------------------------------------------------------------------------------------------------
DocTypeInterpreter::DocTypeInterpreter(const DocTypeInterpreter& objToCopy)
{
	m_strDocTypeName = objToCopy.m_strDocTypeName;
	m_strDocSubType = objToCopy.m_strDocSubType;
	m_ipUtils = objToCopy.m_ipUtils;
	m_vecPatternHolders = objToCopy.m_vecPatternHolders;
}
//-------------------------------------------------------------------------------------------------
DocTypeInterpreter::~DocTypeInterpreter()
{
	try
	{
		m_ipUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28219");
}
//-------------------------------------------------------------------------------------------------
DocTypeInterpreter& DocTypeInterpreter::operator=(const DocTypeInterpreter& objToAssign)
{
	m_strDocTypeName = objToAssign.m_strDocTypeName;
	m_strDocSubType = objToAssign.m_strDocSubType;
	m_ipUtils = objToAssign.m_ipUtils;
	m_vecPatternHolders = objToAssign.m_vecPatternHolders;

	return *this;
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
bool DocTypeInterpreter::docConfidenceLevelMatches(int nLevel, const ISpatialStringPtr& ipInputText,
	DocPageCache& cache)
{
	try
	{
		bool bFound = false;

		// go through all PatternHolders
		for (unsigned int ui = 0; ui < m_vecPatternHolders.size(); ui++)
		{
			PatternHolder& patternHolder = m_vecPatternHolders[ui];

			// Only consider patterns for the specified nLevel
			if (patternHolder.m_eConfidenceLevel != nLevel)
			{
				continue;
			}

			// find match
			bFound = patternHolder.foundPatternsInText(ipInputText, cache);
			if (bFound)
			{
				// Retrieve Block ID
				string& strBlockID = patternHolder.m_strBlockID;

				bool	bFoundMatch = false;
				if (strBlockID.length() > 0)
				{
					unsigned long ulCount = m_vecBlockIDs.size();
					for (unsigned int ui = 0; ui < ulCount; ui++)
					{
						string& strTest = m_vecBlockIDs[ui];
						if (strBlockID.compare( strTest ) == 0)
						{
							bFoundMatch = true;
						}
					}
				}

				// Block ID (if defined) must be unique
				if (!bFoundMatch)
				{
					// Set document sub-type, if defined
					if (patternHolder.m_strSubType.length() > 0)
					{
						m_strDocSubType = patternHolder.m_strSubType;
					}
					else
					{
						// Clear the sub-type
						m_strDocSubType = "";
					}

					// Set Block ID and rule ID
					m_strBlockID = strBlockID;
					m_strRuleID = patternHolder.m_strRuleID;

					// Update vector
					if (strBlockID.length() > 0)
					{
						m_vecBlockIDs.push_back( strBlockID );
					}
				}
				else
				{
					UCLIDException ue( "ELI10982", "Non-unique Block ID within DCC file.");
					ue.addDebugInfo( "Block ID", strBlockID );
					throw ue;
				}

				// A match was found at this level; no need to evaluate any further blocks.
				return true;
			}
		}

		// We've iterated all the patterns for nLevel without finding a match.
		return false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28728");
}
//-------------------------------------------------------------------------------------------------
void DocTypeInterpreter::loadDocTypeFile(const string& strDocTypeFile, bool bClearPatterns)
{
	try
	{
		try
		{
			if (m_ipUtils == __nullptr)
			{
				// Instantiate the AF Utils object
				m_ipUtils.CreateInstance(CLSID_MiscUtils);
				ASSERT_RESOURCE_ALLOCATION("ELI07624", m_ipUtils != __nullptr);
			}

			// before loading the file, if AutoEncrypt is on, and the file has
			// .etf extension, encrypt the base file
			m_ipUtils->AutoEncryptFile( _bstr_t(strDocTypeFile.c_str()),
				_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));

			// if asked to clear the vec of patterns
			if (bClearPatterns)
			{
				m_vecPatternHolders.clear();
			}
			// make sure doc type file exists
			if (!isValidFile(strDocTypeFile))
			{
				UCLIDException ue("ELI07093", "Doc Type file doesn't exist.");
				ue.addDebugInfo("File", strDocTypeFile);
				ue.addWin32ErrorInfo();
				throw ue;
			}

			vector<string> vecLines = convertToLines(strDocTypeFile);

			// Provide the input lines to the file reader
			CommentedTextFileReader fileReader(vecLines, "//", true);
			string strLine("");

			// Process each line in the Rules file
			while (!fileReader.reachedEndOfStream())
			{ 
				// Retrieve this line
				strLine = fileReader.getLineText();
				if (strLine.empty())
				{
					continue;
				}

				// look at the tag at the very beginning of the line
				// to tell if its an import or a pattern or a var
				if (strLine.find(ImportTag) == 0)
				{
					string strFileToImport = getImportFileName(strLine, strDocTypeFile);

					// call this function, do not clear the vec of patterns
					loadDocTypeFile(strFileToImport, false);
				}
				else if (strLine.find(ZeroBeginTag) != string::npos)
				{
					loadConfidenceLevelBlocks(fileReader, kZero);
				}
				else if (strLine.find(SureBeginTag) != string::npos)
				{
					loadConfidenceLevelBlocks(fileReader, kSure);
				}
				else if (strLine.find(ProbableBeginTag) != string::npos)
				{
					loadConfidenceLevelBlocks(fileReader, kProbable);
				}
				else if (strLine.find(MaybeBeginTag) != string::npos)
				{
					loadConfidenceLevelBlocks(fileReader, kMaybe);
				}
				else
				{
					// this is an invalid line
					UCLIDException ue("ELI07095", "This line is not an acceptable format.");
					ue.addDebugInfo("LineText", strLine);
					throw ue;
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28220");
	}
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("Doc Type File", strDocTypeFile);
		throw ue;
	}
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
vector<string> DocTypeInterpreter::convertToLines(const string& strInputFileName)
{
	vector<string> vecLines;
	
	// first make sure the file exists
	if (!isValidFile(strInputFileName))
	{
		UCLIDException ue("ELI07090", "Input file doesn't exist.");
		ue.addDebugInfo("Input File", strInputFileName);
		ue.addWin32ErrorInfo();
		throw ue;
	}
	
	// If input file is an etf file
	if (getExtensionFromFullPath(strInputFileName, true) == ".etf")
	{
		// Open an input file, which is encrypted
		MapLabelManager encryptedFileManager;
		vecLines = encryptedFileManager.getMapLabel(strInputFileName);
	}
	else
	{
		// treat the input file as ASCII text file
		ifstream ifs(strInputFileName.c_str());
		string strLine("");

		while (!ifs.eof())
		{
			getline(ifs, strLine);
			if (strLine.empty())
			{
				continue;
			}
			
			// save the line in the vector
			vecLines.push_back(strLine);
		}
	}

	return vecLines;
}
//-------------------------------------------------------------------------------------------------
string DocTypeInterpreter::getImportFileName(const string& strImportStatement,
											 const string& strCurrentDocTypeFileName)
{
	string strFileToImport("");

	// find first quote sign
	int nQuotePos = strImportStatement.find("\"");
	if (nQuotePos == string::npos)
	{
		UCLIDException ue("ELI07091", "Invalid <import> statement.");
		ue.addDebugInfo("Text", strImportStatement);
		throw ue;
	}
	// find next quote sign
	int nUnquotePos = strImportStatement.find("\"", nQuotePos+1);
	if (nUnquotePos == string::npos)
	{
		UCLIDException ue("ELI07092", "Invalid <import> statement.");
		ue.addDebugInfo("Text", strImportStatement);
		throw ue;
	}

	// take the file name part out from the import statement
	strFileToImport = strImportStatement.substr(nQuotePos+1, nUnquotePos-nQuotePos-1);
	string strParentDirectory(strCurrentDocTypeFileName);
	// the import file's path is relative to this file's path
	// so compute the full path
	strFileToImport = ::getAbsoluteFileName(strParentDirectory, strFileToImport);

	// if this doc type file has extension .etf, then convert the import
	// file to have .etf extension
	if (_strcmpi(::getExtensionFromFullPath(strCurrentDocTypeFileName).c_str(), ".etf") == 0)
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
bool DocTypeInterpreter::isUniqueBlockID(std::string strBlockID)
{
	// Empty Block ID is assumed unique
	if (strBlockID.length() == 0)
	{
		return true;
	}

	// Examine previous Rule ID strings
	long lCount = m_vecBlockIDs.size();
	for (int i = 0; i < lCount; i++)
	{
		// Retrieve this ID
		string strID = m_vecBlockIDs[i];

		// Compare
		if (strID.compare( strBlockID ) == 0)
		{
			// Match found, return false
			return false;
		}
	}

	// No match found, return true
	m_vecBlockIDs.push_back( strBlockID );
	return true;
}
//-------------------------------------------------------------------------------------------------
void DocTypeInterpreter::loadConfidenceLevelBlocks(CommentedTextFileReader& fileReader,
												   EConfidenceLevel eConfidenceLevel)
{
	// Process each line within this Confidence Block
	// Valid lines include:
	//    (optional) SCOPE Begin
	//       OR / AND / SINGLE Begin
	//          rule
	//       OR / AND / SINGLE End
	//
	//       OR / AND / SINGLE Begin
	//          rule
	//       OR / AND / SINGLE End
	//    (optional) SCOPE End
	//
	//    (optional) SCOPE Begin
	//       OR / AND / SINGLE Begin
	//          rule
	//       OR / AND / SINGLE End
	//
	//       OR / AND / SINGLE Begin
	//          rule
	//       OR / AND / SINGLE End
	//    (optional) SCOPE End
	//
	//    Confidence End

	string strLine("");
	bool bScopeFound = false;
	while (!fileReader.reachedEndOfStream())
	{
		strLine = fileReader.getLineText();
		if (strLine.empty())
		{
			continue;
		}

		// Build map of BEGIN and END tags
		map<string, string> vecBeginToEndTags;
		vecBeginToEndTags[ORBeginTag] = OREndTag;
		vecBeginToEndTags[ANDBeginTag] = ANDEndTag;
		vecBeginToEndTags[SINGLEBeginTag] = SINGLEEndTag;
		vecBeginToEndTags[FINDXOFBeginTag] = FINDXOFEndTag;

		// Check for beginning of SCOPE block
		if (strLine.find( SCOPEBeginTag ) != string::npos)
		{
			setScopeParameters( strLine );
			bScopeFound = true;
		}

		// Check for and process OR / AND / SINGLE / FINDXOF blocks
		map<string, string>::iterator itMap = vecBeginToEndTags.begin();
		// if OR/AND/SINGLE begin tag is found
		for (; itMap != vecBeginToEndTags.end(); itMap++)
		{
			if (strLine.find(itMap->first) != string::npos)
			{
				// Use SCOPE parameters if found, otherwise defaults
				if (!bScopeFound)
				{
					setScopeParameters( "" );
				}

				loadPatternsBlock(fileReader, eConfidenceLevel, strLine, itMap->second);
				break;
			}
		}

		// Check for end of SCOPE block
		if (bScopeFound && strLine.find( SCOPEEndTag ) != string::npos)
		{
			// Reset Scope settings to defaults
			setScopeParameters( "" );
			bScopeFound = false;
		}

		// Build map of Confidence Block END tags
		vector<string> vecConfidenceBlockEndTags;
		vecConfidenceBlockEndTags.push_back(ZeroEndTag);
		vecConfidenceBlockEndTags.push_back(MaybeEndTag);
		vecConfidenceBlockEndTags.push_back(ProbableEndTag);
		vecConfidenceBlockEndTags.push_back(SureEndTag);

		// Check for end of this confidence block
		if (strLine.find(vecConfidenceBlockEndTags[(int)eConfidenceLevel]) != string::npos)
		{
			// time to return 
			return;
		}
	}

	// if this point is reached, that means this block doesn't
	// have an END tag, it's invalid
	vector<string> vecTagNames;
	vecTagNames.push_back("ZERO");
	vecTagNames.push_back("MAYBE");
	vecTagNames.push_back("PROBABLE");
	vecTagNames.push_back("SURE");
	
	string strMsg = "Invalid " + vecTagNames[(int)eConfidenceLevel] + " block. Make sure the block has an END tag.";
	UCLIDException ue("ELI07096", strMsg);
	throw ue;
}
//-------------------------------------------------------------------------------------------------
void DocTypeInterpreter::loadPatternsBlock(CommentedTextFileReader& fileReader,
										   EConfidenceLevel eConfidenceLevel,
										   const string& strBlockStartingLineText,
										   const string& strEndTagToFind)
{
	// every beginning of a Patterns block contains:
	// 1) the begin tag : AND_BEGIN, OR_BEGIN, SINGLE_BEGIN, FINDXOF_BEGIN
	// 2) FINDXOF blocks only: The number of patterns which must be found
	// 3) The block ID
	// 5) Case sensitive : true or false
	// 4) The sub-type (optional)
	// Note that the associated SCOPE block also applies here

	// create a new PatternHolder
	PatternHolder patternHolder;
	patternHolder.m_eConfidenceLevel = eConfidenceLevel;

	// parse first line of BEGIN block
	vector<string> vecValues;
	StringTokenizer::sGetTokens(strBlockStartingLineText, ",", vecValues);
	int nSize = vecValues.size();

	// New Size A: 3 = XYZ_BEGIN,BLOCK_ID,bCase
	// New Size B: 4 = XYZ_BEGIN,BLOCK_ID,bCase,Subtype
	// New Size C: 4 = XYZ_FINDXOF,nRequiredCount,BLOCK_ID,bCase
	// New Size D: 5 = XYZ_FINDXOF,nRequiredCount,BLOCK_ID,bCase,SubType
	// Old Size A: 4 = XYZ_BEGIN,dStart,dEnd,bCase
	// Old Size B: 6 = XYZ_BEGIN,dStart,dEnd,bCase,nStartPage,nEndPage
	bool bOldSyntax = false;
	bool bFindXBlock = false;
	string strFindXRequirement;
	string strCaseSensitive;
	string strBlockID;
	string strSubType;

	// *************
	// get the BEGIN tag
	// *************
	string strBeginTag = ::trim(vecValues[0], " \t", " \t");
	readBeginTag(strBeginTag, patternHolder, bFindXBlock);

	switch (nSize)
	{
	// Position of Case-sensitivity flag depends on token count and syntax type
	case 3:
		// New-style syntax, without defined subtype
		strCaseSensitive = trim( vecValues[2], " \t", " \t" );
		strBlockID = trim( vecValues[1], " \t", " \t" );
		break;

	case 4:
		// Check last token, determine which syntax applies
		if (!bFindXBlock &&
			((vecValues[3].find( "true", 0 ) != string::npos) || 
			 (vecValues[3].find( "false", 0 ) != string::npos)))
		{
			bOldSyntax = true;
		}

		// Old-style syntax, without defined page ranges
		if (bOldSyntax)
		{
			strCaseSensitive = trim( vecValues[3], " \t", " \t" );

			// Also read starting and ending range
			m_strStartingRange = trim( vecValues[1], " \t", " \t" );
			m_strEndingRange = trim( vecValues[2], " \t", " \t" );

			// Use default page range
			m_strStartPage = "1";
			m_strEndPage = "-1";
		}
		// New-style syntax, FindX block
		else if (bFindXBlock)
		{
			strFindXRequirement = trim( vecValues[1], " \t", " \t" );
			strBlockID = trim( vecValues[2], " \t", " \t" );			
			strCaseSensitive = trim( vecValues[3], " \t", " \t" );
		}
		// New-style syntax, not a FindX block with defined subtype
		else
		{
			strCaseSensitive = trim( vecValues[2], " \t", " \t" );
			strBlockID = trim( vecValues[1], " \t", " \t" );
			strSubType = trim( vecValues[3], " \t", " \t" );
		}
		break;

	case 5:
		// New-style syntax, FindX block with defined subtype
		strFindXRequirement = trim( vecValues[1], " \t", " \t" );
		strBlockID = trim( vecValues[2], " \t", " \t" );			
		strCaseSensitive = trim( vecValues[3], " \t", " \t" );
		strSubType = trim( vecValues[4], " \t", " \t" );
		break;

	case 6:
		// Old-style syntax, with defined page ranges
		strCaseSensitive = trim( vecValues[3], " \t", " \t" );

		// Also read starting and ending range
		m_strStartingRange = trim( vecValues[1], " \t", " \t" );
		m_strEndingRange = trim( vecValues[2], " \t", " \t" );

		// Also read starting and ending pages
		m_strStartPage = trim( vecValues[4], " \t", " \t" );
		m_strEndPage = trim( vecValues[5], " \t", " \t" );
		break;

	default:
		UCLIDException ue("ELI07098", "Invalid BEGIN line text.");
		ue.addDebugInfo("BEGIN line", strBlockStartingLineText);
		throw ue;
		break;
	}
	
	// Confirm unique Block ID
	if (!isUniqueBlockID( strBlockID ))
	{
		// Create and throw exception
		UCLIDException ue( "ELI10986", "Non-unique Block ID within DCC file." );
		ue.addDebugInfo( "Input line", strBlockStartingLineText );
		ue.addDebugInfo( "Block ID", strBlockID );
		throw ue;
	}

	patternHolder.m_bCaseSensitive = (_strcmpi( strCaseSensitive.c_str(), "true" ) == 0);

	if (bFindXBlock)
	{
		patternHolder.m_nFindXRequirement = asLong(strFindXRequirement);
	}

	readPageScope( m_strStartPage, m_strEndPage, patternHolder );

	// Set Block ID and optional sub-type
	if (!bOldSyntax)
	{
		patternHolder.m_strBlockID = strBlockID;
		patternHolder.m_strSubType = strSubType;
	}

	// reading the contents of this block
	string strLine("");
	while (!fileReader.reachedEndOfStream())
	{
		strLine = fileReader.getLineText();
		if (strLine.empty())
		{
			continue;
		}

		// once END tag is found, it's time to go back to the caller
		if (strLine.find(strEndTagToFind) != string::npos)
		{
			// Apply Page Range (from SCOPE settings)
			readSearchScope( m_strStartingRange, m_strEndingRange, patternHolder );

			// Apply Page Scope (from SCOPE settings)
			readPageScope( m_strStartPage, m_strEndPage, patternHolder );

			// store it in the vec
			m_vecPatternHolders.push_back(patternHolder);

			// it's time to return
			return;
		}

		// Validate the rule ID for uniqueness
		if (patternHolder.isUniqueRuleID( strLine ))
		{
			// store each pattern
			patternHolder.m_vecPatterns.push_back(strLine);
		}
		else
		{
			// Create and throw exception
			UCLIDException ue( "ELI10981", "Non-unique rule ID within block." );
			ue.addDebugInfo( "Input line", strLine );
			throw ue;
		}
	}

	// this function is not supposed to read any BEGIN tag since
	// as soon as the END tag is reached, it returns immediately
	string strMsg = "Invalid OR/AND/SINGLE/FINDXOF block. Make sure it has a matching END tag.";
	UCLIDException ue("ELI07097", strMsg);
	ue.addDebugInfo("First Line of the block", strBlockStartingLineText);
	throw ue;
}
//-------------------------------------------------------------------------------------------------
void DocTypeInterpreter::readBeginTag(const string& strBeginTag,
									  PatternHolder& patternHolder,
									  bool &rbIsFindXBlock)
{
	if (strBeginTag == ANDBeginTag)
	{
		patternHolder.m_bIsAndRelationship = true;
	}
	else if (strBeginTag == ORBeginTag)
	{
	}
	else if (strBeginTag == SINGLEBeginTag)
	{
	}
	else if (strBeginTag == FINDXOFBeginTag)
	{
		rbIsFindXBlock = true;
	}
	else 
	{
		UCLIDException ue("ELI07415", "Invalid BEGIN tag name.");
		ue.addDebugInfo("BEGIN tag", strBeginTag);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void DocTypeInterpreter::readPageScope(const string& strStartPage,
									   const string& strEndPage,
									   PatternHolder& patternHolder)
{
	//	Special meaning for start/end page numbers:
	//	If startPage == -1, then the endPage field represents the "last X pages".
	//	If endPage == -1, then the search should go from the specified start page to the last page in the document.
	//	If startPage == endPage == -1, then that's an error condition.
	//	startPage != 0 , endPage != 0

	// by default, we are reading the entire document
	int nStartPage = 0, nEndPage = -1;
	nStartPage = ::asLong(strStartPage);
	nEndPage = ::asLong(strEndPage);
	
	// eliminate any invalid page inputs
	if ( (nStartPage == -1 && nEndPage == -1)
		|| (nStartPage > 0 && nEndPage > 0 && nStartPage > nEndPage)
		|| (nEndPage == 0 || nEndPage < -1) 
		|| (nStartPage == 0 || nStartPage < -1) )
	{
		string strMsg = "Invalid start/end page number specified.\r\n"
			"startPage = -1 OR startPage > 0"
			"endPage = -1 OR endPage > 0"
			"If startPage == -1, then the endPage field represents the \"last X pages\"."
			"If endPage == -1, then the search should go from the specified start page to the last page in the document."
			"If startPage == endPage == -1, then that's an error condition.";
		UCLIDException ue("ELI07416", strMsg);
		ue.addDebugInfo("Start Page", strStartPage);
		ue.addDebugInfo("End Page", strEndPage);
		throw ue;
	}

	patternHolder.m_nStartPage = nStartPage;
	patternHolder.m_nEndPage = nEndPage;
}
//-------------------------------------------------------------------------------------------------
void DocTypeInterpreter::readSearchScope(const string& strStartPos, 
										 const string strEndPos,
										 PatternHolder& patternHolder)
{
	double dStartingRange = ::asDouble(strStartPos);
	// make sure it's positive
	if (dStartingRange < 0)
	{
		UCLIDException ue("ELI07099", "Invalid starting range.");
		ue.addDebugInfo("Start Range", strStartPos);
		throw ue;
	}
	patternHolder.m_dStartingRange = dStartingRange;

	// Ending Range
	double dEndingRange = ::asDouble(strEndPos);
	// make sure it's positive
	if (dEndingRange < 0)
	{
		UCLIDException ue("ELI10513", "Invalid ending range.");
		ue.addDebugInfo("End Range", strEndPos);
		throw ue;
	}
	patternHolder.m_dEndingRange = dEndingRange;
}
//-------------------------------------------------------------------------------------------------
void DocTypeInterpreter::setScopeParameters(const string strLine)
{
	// Empty text means to reset Scope settings to defaults
	if (strLine.length() == 0)
	{
		// Entire page
		m_strStartingRange = "0";
		m_strEndingRange = "1";

		// Entire document
		m_strStartPage = "1";
		m_strEndPage = "-1";

		return;
	}

	// every beginning of a SCOPE block contains:
	// 1) the begin tag : SCOPE_BEGIN
	// 2) Search scope (starting and ending range i.e 0 -> 1)
	// 3) Page scope: start and end page numbers. 

	// Parse text of SCOPE BEGIN block
	vector<string> vecValues;
	StringTokenizer::sGetTokens( strLine, ",", vecValues );
	int nSize = vecValues.size();

	/////////////////////////////
	// Range scope and Page scope
	/////////////////////////////
	if (nSize == 5)
	{
		m_strStartingRange = ::trim(vecValues[1], " \t", " \t");
		m_strEndingRange = ::trim(vecValues[2], " \t", " \t");

		m_strStartPage = ::trim(vecValues[3], " \t", " \t");
		m_strEndPage = ::trim(vecValues[4], " \t", " \t");
	}
	else
	{
		UCLIDException ue( "ELI10512", "Invalid SCOPE BEGIN line." );
		ue.addDebugInfo( "Line", strLine );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
