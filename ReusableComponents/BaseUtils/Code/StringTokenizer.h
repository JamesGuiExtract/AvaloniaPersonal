
#pragma once

#include "BaseUtils.h"

#include <vector>
#include <string>

using namespace std;

class EXPORT_BaseUtils StringTokenizer
{
public:
	StringTokenizer();
	// cDelimeter describes the separator between the token characters.
	// If bInputTerminatedWith2Delimiters == true, then the input string
	// is expected to be terminated with two cDelimeter characters.  If
	// bInputTerminatedWith2Delimiters ==  false, then the input string is 
	// expected to be null terminated.
	// Example usage:
	// To tokenize strings of format "a,b,c", set cDelimeter to ',' and 
	// bInputTerminatedWith2Delimiters to false;
	// To tokenize strings of format "a\0b\0c\0\0", set cDelimeter to '\0' and
	// bInputTerminatedWith2Delimiters to true.
	StringTokenizer(char cDelimeter, bool bInputTerminatedWith2Delimiters = false);
	// overloaded constructor which has the same functionality as the first 
	// constructor except it takes a string as the delimiter
	// if bUsMultipleDelimeters is true each character in strDelimeter will be treated
	// as a delimiter so the string "a,b;c" with strDelemiter=",;" would return
	// three tokens "a", "b", "c"
	StringTokenizer(const string& strDelimeter, bool bInputTerminatedWith2Delimiters = false, 
		bool bUseMultipleDelimeters = false);

	StringTokenizer(const StringTokenizer& stringTokenizer);
	StringTokenizer& operator=(const StringTokenizer& stringTokenizer);

	void setStringQualifier(char cStartQualifier, char cEndQualifier);
	void setStringQualifier(string strStartQualifier, string strEndQualifier);

	// use this parse method when the input string does not contain null characters
	// except as the terminating character
	// The tokens are returned in rvecTokens
	void parse(const string& strInput, vector<string>& rvecTokens);

	// use this parse method when the input string may contain embedded null characters
	// REQUIRE: pszInput must be terminated by a null character if 
	// m_bInputTerminatedWith2Delimiters == false;  pszInput must be terminated by
	// two m_cDelimeter characters if m_bInputTerminatedWith2Delimiters == true.
	
	void parse(const char *pszInput, vector<string>& rvecTokens);



	static void sGetTokens(const string& strInput, char cDelimeter, vector<string>& rvecTokens);
	static void sGetTokens(const string& strInput, const string& strDelimeter, vector<string>& rvecTokens,
		bool bUseMultipleDelimeters = false);

	static void sGetTokens(const string& strInput, char cDelimeter, char cStartQualifier, 
		char cEndQualifier, vector<string>& rvecTokens);
	static void sGetTokens(const string& strInput, const string& strDelimeter, string strStartQualifier,
		string strEndQualifier, vector<string>& rvecTokens, bool bUseMultipleDelimeters = false);

private:
	string m_strDelimiter;
	bool m_bInputTerminatedWith2Delimiters;
	bool m_bUseMultipleDelimeters;

	// These two vars specify the text qualifiers
	// m_cStringBegin means that anything that all characters following it are part of the 
	// same token until a m_cStringEnd Character is reached
	// they can be the same character (for instance a quote " )
	string m_strStringBegin, m_strStringEnd;


	bool isSubStringAtPos(string strText, string strSubString, long nPos);

};
