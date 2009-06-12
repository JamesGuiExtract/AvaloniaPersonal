
#include "stdafx.h"
#include "KeywordListReader.h"
#include "UCLIDException.h"
#include "cpputil.h"

using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// KeywordListReader
//-------------------------------------------------------------------------------------------------
KeywordListReader::KeywordListReader(std::ifstream& rif)
: m_pInputStream(&rif), m_pvecInputLines(NULL),
  m_nCurrentLine(0), m_bReadFromFile(true)
{
}
//-------------------------------------------------------------------------------------------------
KeywordListReader::KeywordListReader(std::vector<std::string>& rInputLines)
: m_pInputStream(NULL), m_pvecInputLines(&rInputLines),
  m_nCurrentLine(0), m_bReadFromFile(false)
{
}
//-------------------------------------------------------------------------------------------------
KeywordListReader::~KeywordListReader()
{
	try
	{
		m_mapKeywordsToStrings.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16386");
}
//-------------------------------------------------------------------------------------------------
bool KeywordListReader::GetStringsForKeyword(const std::string& rstrKeyword, 
											 std::vector<std::string>& rvecStrings)
{
	bool bReturn = false;

	// Clear the vector of strings
	rvecStrings.clear();

	// Retrieve collection of lines from previous reference
	if (rstrKeyword != "")
	{
		rvecStrings = m_mapKeywordsToStrings[rstrKeyword];

		if (rvecStrings.size() > 0)
		{
			bReturn = true;
		}
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
void KeywordListReader::ReadKeywords(vector<string>& rvecKeywords)
{
	// Clear the internal map and vector of keywords
	m_mapKeywordsToStrings.clear();
	rvecKeywords.clear();

	// Read non-commented, non-empty lines from the input file
	if (m_bReadFromFile)
	{
		CommentedTextFileReader	fileReader( *m_pInputStream, "//" );

		processLines( fileReader, rvecKeywords );
	}
	// Read non-commented, non-empty lines from the vector of lines
	else
	{
		CommentedTextFileReader	fileReader( *m_pvecInputLines, "//" );

		processLines( fileReader, rvecKeywords );
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool KeywordListReader::isNewKeyword(std::string& rstrText)
{
	bool bReturn = false;

	// Trim leading and trailing whitespace
	rstrText = trim( rstrText, " ", " " );

	// Check for leading and trailing angle brackets
	long lLength = rstrText.length();
	if ((lLength > 2) && (rstrText[0] == '<') && (rstrText[lLength - 1] == '>'))
	{
		// Remove the angle brackets
		rstrText = rstrText.substr( 1, lLength - 2 );
		bReturn = true;
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
bool KeywordListReader::isPastReference(std::string& rstrText)
{
	bool bReturn = false;

	// Trim leading and trailing whitespace
	rstrText = trim( rstrText, " ", " " );

	// Check for leading pound sign
	long lLength = rstrText.length();
	if ((lLength > 1) && (rstrText[0] == '#'))
	{
		// Remove the pound sign
		rstrText = rstrText.substr( 1, lLength - 1 );
		bReturn = true;
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
void KeywordListReader::processLines(CommentedTextFileReader& rFR, vector<string>& rvecKeywords)
{
	// Clear the internal map and vector of keywords
	m_mapKeywordsToStrings.clear();
	rvecKeywords.clear();

	bool	bCurrentKeyword = false;
	string	strCurrentKeyword;
	vector<string>	vecStrings;

	while (!rFR.reachedEndOfStream())
	{
		// Read the next line
		string	strLine = rFR.getLineText();

		// Check for new Keyword
		if (isNewKeyword( strLine ))
		{
			// Finish handling current keyword
			if ((strCurrentKeyword != "") && (vecStrings.size() > 0))
			{
				// Add the current collection to the internal map
				m_mapKeywordsToStrings[strCurrentKeyword] = vecStrings;

				// Add the keyword to the collection
				rvecKeywords.push_back( strCurrentKeyword );

				// Reset the current collection
				vecStrings.clear();

				// Save the new keyword
				strCurrentKeyword = strLine;
			}
			// Begin first keyword
			else
			{
				// Reset the current collection
				vecStrings.clear();

				// Save the new keyword
				strCurrentKeyword = strLine;
			}
		}		// end else this is a new Keyword

		// Check for reference to previous collection
		else if (isPastReference( strLine ))
		{
			// Retrieve collection of lines from previous reference
			vector<string>	vecPrevious = m_mapKeywordsToStrings[strLine];

			// Add these lines to the current collection
			long lSize = vecPrevious.size();
			if (lSize > 0)
			{
				vector<string>::const_iterator iter = vecPrevious.begin();

				for (int i = 0; i < lSize; i++)
				{
					// Add this item to the current collection
					vecStrings.push_back( *iter );

					// Advance iterator
					iter++;
				}
			}
		}		// end else this is a past reference
		
		// Add this line to the current collection
		else if (strLine != "")
		{
			// A keyword must already be defined
			if (strCurrentKeyword != "")
			{
				vecStrings.push_back( strLine );
			}
		}		// end else this is just another line
	}			// end while reading lines

	// Finish handling current keyword
	if ((strCurrentKeyword != "") && (vecStrings.size() > 0))
	{
		// Add the current collection to the internal map
		m_mapKeywordsToStrings[strCurrentKeyword] = vecStrings;

		// Add the keyword to the collection
		rvecKeywords.push_back( strCurrentKeyword );
	}
}
//-------------------------------------------------------------------------------------------------
