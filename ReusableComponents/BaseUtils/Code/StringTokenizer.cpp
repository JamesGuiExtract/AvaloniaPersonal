
#include "stdafx.h"
#include "StringTokenizer.h"
#include "UCLIDException.h"

#include <iostream>

//-------------------------------------------------------------------------------------------------
StringTokenizer::StringTokenizer()
: m_strDelimiter(","),  // use comma by default
  m_bInputTerminatedWith2Delimiters(false),
  m_bUseMultipleDelimeters(false)
{
	m_strStringBegin = "\0";
	m_strStringEnd = "\0";
}
//-------------------------------------------------------------------------------------------------
StringTokenizer::StringTokenizer(char cDelimeter, bool bInputTerminatedWith2Delimiters)
: m_bInputTerminatedWith2Delimiters(bInputTerminatedWith2Delimiters),
	m_bUseMultipleDelimeters(false)
{
	m_strDelimiter = cDelimeter;
	m_strStringBegin = "\0";
	m_strStringEnd = "\0";
}
//-------------------------------------------------------------------------------------------------
StringTokenizer::StringTokenizer(const string& strDelimeter, bool bInputTerminatedWith2Delimiters, 
								 bool bUseMulitpleDelimeters)
: m_strDelimiter(strDelimeter),
  m_bInputTerminatedWith2Delimiters(bInputTerminatedWith2Delimiters),
  m_bUseMultipleDelimeters(bUseMulitpleDelimeters)
{
	m_strStringBegin = "\0";
	m_strStringEnd = "\0";
}
//-------------------------------------------------------------------------------------------------
StringTokenizer::StringTokenizer(const StringTokenizer& stringTokenizer)
{
	m_strDelimiter = stringTokenizer.m_strDelimiter;
	m_bInputTerminatedWith2Delimiters = stringTokenizer.m_bInputTerminatedWith2Delimiters;

	m_strStringBegin = stringTokenizer.m_strStringBegin;
	m_strStringEnd = stringTokenizer.m_strStringEnd;
	m_bUseMultipleDelimeters = stringTokenizer.m_bUseMultipleDelimeters;
}
//-------------------------------------------------------------------------------------------------
StringTokenizer& StringTokenizer::operator=(const StringTokenizer& stringTokenizer)
{
	m_strDelimiter = stringTokenizer.m_strDelimiter;
	m_bInputTerminatedWith2Delimiters = stringTokenizer.m_bInputTerminatedWith2Delimiters;

	m_strStringBegin = stringTokenizer.m_strStringBegin;
	m_strStringEnd = stringTokenizer.m_strStringEnd;
	m_bUseMultipleDelimeters = stringTokenizer.m_bUseMultipleDelimeters;

	return *this;
}
//-------------------------------------------------------------------------------------------------
void StringTokenizer::parse(const string& strInput, vector<string>& rvecTokens)
{
	rvecTokens.clear();

	// return the empty vector since the input is an empty string
	if (strInput.empty())
	{
		return;
	}

	// the start position for each token
	unsigned int uiStartPos = 0;

	string strCurrTok = "";
	while(1)
	{
		// find delimiter
		unsigned int uiDelimPos = (m_bUseMultipleDelimeters) ?
			strInput.find_first_of( m_strDelimiter, uiStartPos ) : strInput.find( m_strDelimiter, uiStartPos );

		// Get the length of the delimeter
		int nDelimLen = (m_bUseMultipleDelimeters) ? 1 : m_strDelimiter.length();

		// find a beginString Qualifier
		unsigned int uiStrBeginPos = strInput.find( m_strStringBegin, uiStartPos );
		int nStrBeginLen = m_strStringBegin.length();

		if (m_strStringBegin == "\0" || m_strStringBegin == "")
		{
			uiStrBeginPos = string::npos;
		}

		if (uiStrBeginPos == string::npos && uiDelimPos == string::npos)
		{
			strCurrTok += strInput.substr( uiStartPos, strInput.size() - uiStartPos );
			rvecTokens.push_back(strCurrTok);
			break;
		}
		else if (uiDelimPos == string::npos || 
			(uiStrBeginPos != string::npos && uiStrBeginPos < uiDelimPos))
		{
			strCurrTok += strInput.substr( uiStartPos, uiStrBeginPos - uiStartPos );

			unsigned int uiStrEndPos = strInput.find( m_strStringEnd, uiStrBeginPos + nStrBeginLen );
			int nStrEndLen = m_strStringEnd.size();

			if (uiStrEndPos == string::npos)
			{
				UCLIDException ue("ELI09038", "No Closing String Qualifier.");
				ue.addDebugInfo("Begin Qualifier", m_strStringBegin);
				ue.addDebugInfo("End Qualifier", m_strStringEnd);
				ue.addDebugInfo("String", strInput.substr( uiStrBeginPos, 
					strInput.size() - uiStrBeginPos ));
				throw ue;
			}

			strCurrTok += strInput.substr( uiStrBeginPos + 1, uiStrEndPos - (uiStrBeginPos + 1) );
			
			uiStartPos = uiStrEndPos + nStrEndLen;
		}
		else
		{
			strCurrTok += strInput.substr( uiStartPos, uiDelimPos - uiStartPos );
			rvecTokens.push_back(strCurrTok);
			strCurrTok = "";

			// move the start position to the position 
			// right after the current found delimiter
			uiStartPos = uiDelimPos + nDelimLen;

			// find next delimiter
			uiDelimPos = (m_bUseMultipleDelimeters) ?
				strInput.find_first_of( m_strDelimiter, uiStartPos ) : strInput.find( m_strDelimiter, uiStartPos );
			
			if (m_bInputTerminatedWith2Delimiters)
			{
				// if terminated with 2 delimiter, then check if there's
				// a delimiter right after current found delimiter
				if (uiDelimPos == uiStartPos)
				{
					// jump out of the loop since 2 delimiter is found
					break;
				}
			}
		}
	}

	if (rvecTokens.size() == 0)
	{
		rvecTokens.push_back(strInput);
	}
}
//-------------------------------------------------------------------------------------------------
void StringTokenizer::parse(const char *pszInput, vector<string>& rvecTokens)
{
	string strInput = pszInput;
	parse(strInput, rvecTokens);
}
//-------------------------------------------------------------------------------------------------
void StringTokenizer::sGetTokens(const string& strInput, char cDelimeter, vector<string>& rvecTokens)
{
	StringTokenizer tokenizer(cDelimeter);
	tokenizer.parse(strInput, rvecTokens);
}
//-------------------------------------------------------------------------------------------------
void StringTokenizer::sGetTokens(const string& strInput, const string& strDelimeter, vector<string>& rvecTokens,
								 bool bUseMultipleDelimeters)
{
	StringTokenizer tokenizer(strDelimeter, false, bUseMultipleDelimeters);
	tokenizer.parse(strInput, rvecTokens);
}
//-------------------------------------------------------------------------------------------------
void StringTokenizer::sGetTokens(const string& strInput, char cDelimeter, char cStartQualifier, 
								 char cEndQualifier, vector<string>& rvecTokens)
{
	StringTokenizer tokenizer(cDelimeter);
	tokenizer.setStringQualifier(cStartQualifier, cEndQualifier);
	tokenizer.parse(strInput, rvecTokens);
}
//-------------------------------------------------------------------------------------------------
void StringTokenizer::sGetTokens(const string& strInput, const string& strDelimeter, string strStartQualifier, 
								 string strEndQualifier, vector<string>& rvecTokens, bool bUseMultipleDelimeters)
{
	StringTokenizer tokenizer(strDelimeter, false, bUseMultipleDelimeters);
	tokenizer.setStringQualifier(strStartQualifier, strEndQualifier);
	tokenizer.parse(strInput, rvecTokens);
}
//-------------------------------------------------------------------------------------------------
void StringTokenizer::setStringQualifier(string strStartQualifier, string strEndQualifier)
{
	m_strStringBegin = strStartQualifier;
	m_strStringEnd = strEndQualifier;
}
//-------------------------------------------------------------------------------------------------
void StringTokenizer::setStringQualifier(char cStartQualifier, char cEndQualifier)
{
	m_strStringBegin = "";
	m_strStringEnd = "";
	m_strStringBegin += cStartQualifier;
	m_strStringEnd += cEndQualifier;
}
//-------------------------------------------------------------------------------------------------
