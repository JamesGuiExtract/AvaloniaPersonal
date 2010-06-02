#include "stdafx.h"
#include "CommentedTextFileReader.h"
#include "UCLIDException.h"
#include "cpputil.h"
#include "BlockExtractor.h"

#include <iostream>

using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

// dummy variables to initialize unused references with
static ifstream gDUMMY_INPUT_STREAM;
static vector<string> gDUMMY_VEC_LINES;

//-------------------------------------------------------------------------------------------------
CommentedTextFileReader::CommentedTextFileReader(ifstream& rif,
												 const string& strSingleComment, 
												 bool bSkipEmptyLines,
												 const string& strStartComment,
												 const string& strEndComment)
:m_rif(rif), m_rInputLines(gDUMMY_VEC_LINES), 
 m_strSingleComment(strSingleComment),
 m_bSkipEmptyLines(bSkipEmptyLines), 
 m_nCurrentLine(0), 
 m_bReadFromFile(true),
 m_strStartComment(strStartComment),
 m_strEndComment(strEndComment),
 m_bSkippingState(false)
{
	// pair of comment string must be in-sync, i.e. either all are empty
	// or none is empty
	if (m_strStartComment.empty() ^ m_strEndComment.empty())
	{
		UCLIDException ue("ELI10125", "Start and End comment string must be either all empty, or all non-empty.");
		ue.addDebugInfo("Start Comment String", m_strStartComment);
		ue.addDebugInfo("End Comment String", m_strEndComment);
		throw ue;
	}

	// make sure m_strSingleComment and m_strStartComment/m_strEndComment 
	// can't be empty at same time (sufficient to just check startComment
	// since earlier check guarantees that start and end are either both
	// empty or full
	if (m_strSingleComment.empty() 
		&& (m_strStartComment.empty()))
	{
		UCLIDException ue("ELI10123", "You must specify at least one set of comment string.");
		ue.addDebugInfo("Single Comment String", m_strSingleComment);
		ue.addDebugInfo("Start Comment String", m_strStartComment);
		ue.addDebugInfo("End Comment String", m_strEndComment);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
CommentedTextFileReader::CommentedTextFileReader(vector<string>& rInputLines, 
												 const string& strSingleComment, 
												 bool bSkipEmptyLines,
												 const string& strStartComment,
												 const string& strEndComment)
:m_rif(gDUMMY_INPUT_STREAM), m_rInputLines(rInputLines), 
 m_strSingleComment(strSingleComment), 
 m_bSkipEmptyLines(bSkipEmptyLines), 
 m_nCurrentLine(0), 
 m_bReadFromFile(false),
 m_strStartComment(strStartComment),
 m_strEndComment(strEndComment),
 m_bSkippingState(false)
{
	// pair of comment string must be in-sync, i.e. either all are empty
	// or none is empty
	if ((!m_strStartComment.empty() && m_strEndComment.empty())
		|| (m_strStartComment.empty() && !m_strEndComment.empty()))
	{
		UCLIDException ue("ELI10126", "Start and End comment string must be either all empty, or all non-empty.");
		ue.addDebugInfo("Start Comment String", m_strStartComment);
		ue.addDebugInfo("End Comment String", m_strEndComment);
		throw ue;
	}

	// make sure m_strSingleComment and m_strStartComment/m_strEndComment 
	// can't be empty at same time
	if (m_strSingleComment.empty() 
		&& (m_strStartComment.empty() || m_strEndComment.empty()))
	{
		UCLIDException ue("ELI10124", "You must specify at least one set of comment string.");
		ue.addDebugInfo("Single Comment String", m_strSingleComment);
		ue.addDebugInfo("Start Comment String", m_strStartComment);
		ue.addDebugInfo("End Comment String", m_strEndComment);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
string CommentedTextFileReader::getLineText()
{
	string strLine("");

	while (!reachedEndOfStream())
	{
		// get the next non-comment line and trim off the 
		// leading/trailing spaces+tabs from the string
		strLine = getNextNonCommentedLine();
		strLine = trim(strLine, " \t\r\n", " \t\r\n");

		// first see if it is an empty line
		while (strLine.empty())
		{
			// if its the end of the file, or the empty line is required to return
			if (reachedEndOfStream() || !m_bSkipEmptyLines)
			{
				// return the empty string
				return strLine;
			}
			
			// get the next line and trim off the left most empty spaces from the string
			strLine = getNextNonCommentedLine();
			strLine = trim(strLine, " \t\r\n", " \t\r\n");
		}

		break;
	}

	return strLine;
}
//-------------------------------------------------------------------------------------------------
bool CommentedTextFileReader::reachedEndOfStream()
{
	// determine whether we reached end-of-stream based upon the value
	// of m_bReadFromFile
	return (m_bReadFromFile == true && !m_rif) ||
		   (m_bReadFromFile == false && (unsigned long) m_nCurrentLine >= m_rInputLines.size());
}
//-------------------------------------------------------------------------------------------------
string CommentedTextFileReader::getNextNonCommentedLine()
{
	// get the next line from either the file or 
	// vector depending upon m_bReadFromFile
	string strLine("");

	while (!reachedEndOfStream())
	{
		if (m_bReadFromFile)
		{
			getline(m_rif, strLine);
		}
		else
		{
			strLine = m_rInputLines[m_nCurrentLine++];
		}

		// whether or not we shall ignore the return string
		string strRet("");
		bool bRet = getNonCommentText(strLine, strRet);
		if (bRet)
		{
			return strRet;
		}
		else
		{
			// we shall ignore this line
			strLine = "";
		}
	}

	return strLine;
}
//-------------------------------------------------------------------------------------------------
bool CommentedTextFileReader::getNonCommentText(const string& strLine, string& strReturnText)
{
	bool bRet = true;

	// checking state
	if (m_bSkippingState)
	{
		bRet = false;

		// look for end comment string
		unsigned int uiCurrentPos = strLine.find(m_strEndComment);
		if (uiCurrentPos == string::npos)
		{
			// no end comment string found, ignore the input
			return false;
		}

		// found end comment, set flag
		m_bSkippingState = false;
		// get rest of the string after the end comment string
		string strRestOfString = strLine.substr( uiCurrentPos + m_strEndComment.size() );
		string strRestOfReturn("");
		bool bNoIgnore = getNonCommentText(strRestOfString, strRestOfReturn);
		if (bNoIgnore)
		{
			strReturnText = strRestOfReturn;
			bRet = !strReturnText.empty();
		}
	}
	else
	{
		bRet = true;

		// look for the first start comment string (if defined)
		unsigned int uiStartCommentPos = m_strStartComment.empty() ? 
			(string::npos) : strLine.find(m_strStartComment);
		// look for single comment string (if defined)
		unsigned int uiSingleCommentPos = m_strSingleComment.empty() ? 
			(string::npos) : strLine.find(m_strSingleComment);

		// if none of the comment strings can be found
		if (uiStartCommentPos == string::npos && uiSingleCommentPos == string::npos)
		{
			// return the original input line of text
			strReturnText = strLine;
			return true;
		}
		else if (uiSingleCommentPos != string::npos
			&& (uiSingleCommentPos < uiStartCommentPos || uiStartCommentPos == string::npos))
		{
			// if single comment string found first, anything after 
			// the single comment string could all be ignored
			strReturnText = strLine.substr( 0, uiSingleCommentPos );
			bRet = !strReturnText.empty();
		}
		else if (uiStartCommentPos != string::npos
			&& (uiStartCommentPos < uiSingleCommentPos || uiSingleCommentPos == string::npos))
		{
			// if start comment string found first
			// in skipping state now
			m_bSkippingState = true;
			
			// get the text before the first start comment string
			strReturnText = strLine.substr( 0, uiStartCommentPos );
			
			// recursive call to getNonCommentText()
			string strRestOfString = strLine.substr( uiStartCommentPos + m_strStartComment.size() );
			string strRestOfReturn("");
			bool bNoIgnore = getNonCommentText(strRestOfString, strRestOfReturn);
			if (bNoIgnore)
			{
				strReturnText += strRestOfReturn;
			}
			
			bRet = !strReturnText.empty();
		}
	}

	return bRet;
}
//-------------------------------------------------------------------------------------------------
void CommentedTextFileReader::sGetUncommentedFileContents(std::list<std::string> &lstTextFile, 
														  const std::string &strSingleComment, 
														  const std::string &strBlockCommentStart, 
														  const std::string &strBlockCommentEnd)
{
	std::list<std::string>::iterator listIter = lstTextFile.begin();

	while(listIter != lstTextFile.end())
	{
		std::string strLine = (*listIter);

		// trim off the leading/trailing spaces+tabs from the string
		strLine = trim(strLine, " \t", " \t");

		unsigned int uiSingleCommentPos = strLine.find(strSingleComment);

		// handle single line comments and empty lines
		if (uiSingleCommentPos != std::string::npos)
		{
			strLine = strLine.substr( 0, uiSingleCommentPos );
		}
		
		// remove empty lines
		if (strLine == "")
		{
			listIter = lstTextFile.erase(listIter);
		}
		else
		{
			*listIter = strLine;
			listIter++;	
		}	
	}

	// handle block comments
	BlockExtractor::removeAllEnclosedBlocks(lstTextFile, strBlockCommentStart, strBlockCommentEnd);
}
//-------------------------------------------------------------------------------------------------

