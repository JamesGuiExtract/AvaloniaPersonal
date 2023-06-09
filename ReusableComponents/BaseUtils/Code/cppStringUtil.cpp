//==================================================================================================
//
// COPYRIGHT (c) 2000 - 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:    cppStringUtil.cpp
//
// PURPOSE: Various string utility functions
//
// NOTES:   
//
// AUTHORS: Arvind Ganesan (August 2000 - present)
//
//==================================================================================================

#include "stdafx.h"
#include "cpputil.h"
#include "UCLIDException.h"

#include <afxmt.h>
#include <locale>
#include <codecvt>
#include <string>
#include <regex>

using namespace std;

//--------------------------------------------------------------------------------------------------
bool replaceVariable(string& s, const string& t1, const string& t2)
{
    if (s.empty() || t1 == t2)
    {
        return false;
    }

    // this function replaces all occurrences of t1 in S by t2
    size_t findpos;
    bool bReturnType;

    findpos = s.find(t1);
    if (findpos == string::npos)
    {
        bReturnType = false;
    }
    else
    {
        bReturnType = true;
        while (findpos != string::npos)
        {
            s.replace(findpos, t1.length(), t2);
            findpos = s.find(t1, findpos + t2.length());
        }
    }

    return bReturnType;
}
//--------------------------------------------------------------------------------------------------
bool replaceVariable(string& s, const char * t1, const string& t2)
{
    string temps = t1;
  
    return(replaceVariable(s, temps, t2));
}
//--------------------------------------------------------------------------------------------------
bool replaceVariable(string& s, const string& t1, const string& t2, EReplaceType eReplaceType)
{
    bool bReturnType;
    size_t findpos;
    
    if (s.empty() || t1 == t2)
    {
        bReturnType = false;
    }
    else if(eReplaceType == kReplaceAll)
    {
        bReturnType = replaceVariable(s, t1, t2);
    }
    else if(eReplaceType == kReplaceFirst)
    {
        findpos = s.find(t1);
        if(findpos == string::npos)
        {
            bReturnType = false;
        }
        else
        {
            s.replace(findpos, t1.length(), t2);
            bReturnType = true;
        }
    }
    else if(eReplaceType == kReplaceLast)
    {
        findpos = s.rfind(t1);
        if(findpos == string::npos)
        {
            bReturnType = false;
        }
        else
        {
            s.replace(findpos, t1.length(), t2);
            bReturnType = true;
        }
    }
    else
    {
        throw UCLIDException("ELI00325", "The replacement type is not valid.");
    }
    return bReturnType;
}
//--------------------------------------------------------------------------------------------------
void makeUpperCase(string& strInput)
{
    // Converts the input string to all upper case
    for (unsigned int i = 0; i < strInput.length(); i++)
    {
        strInput[i] = (char)toupper(strInput[i]);
    }
}
//--------------------------------------------------------------------------------------------------
void makeLowerCase(string& strInput)
{
    // Converts the input string to all lower case
    for (unsigned int i = 0; i < strInput.length(); i++)
    {
        strInput[i] = (char)tolower(strInput[i]);
    }
}
//--------------------------------------------------------------------------------------------------
string asString(unsigned long ulValue)
{
    // return the unsigned long value as a string
    char pszTemp[128];
    sprintf_s(pszTemp, sizeof(pszTemp), "%u", ulValue);
    return string(pszTemp);
}
//--------------------------------------------------------------------------------------------------
string asString(unsigned int uiValue)
{
    // return the WORD value as a string
    char pszTemp[128];
    sprintf_s(pszTemp, sizeof(pszTemp), "%u", uiValue);
    return string(pszTemp);
}
//--------------------------------------------------------------------------------------------------
string asString(double dValue)
{
    // return the double value as a string
    char pszTemp[128];
    sprintf_s(pszTemp, sizeof(pszTemp), "%f", dValue);
    return string(pszTemp);
}
//--------------------------------------------------------------------------------------------------
string asString(double dValue, unsigned int uiNumDecimalPlaces)
{
    // create the format string
    char pszFormat[20] = {0};
    sprintf_s(pszFormat, sizeof(pszFormat), "%%0.%uf", uiNumDecimalPlaces);

    // return the double value as a string
    char pszTemp[128] = {0};
    sprintf_s(pszTemp, sizeof(pszTemp), pszFormat, dValue);
    return string(pszTemp);
}
//--------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string asString(double dValue, unsigned int uiMinDecimalPlaces,
                                 unsigned int uiMaxDecimalPlaces)
{
    ASSERT_ARGUMENT("ELI22724", uiMaxDecimalPlaces >= uiMinDecimalPlaces);

    // create the format string
    char pszTemp[20] = {0};
    int nSize = sprintf_s(pszTemp, sizeof(pszTemp), "%.*f", uiMaxDecimalPlaces, dValue);

    if (nSize < 0)
    {
        throw UCLIDException("ELI22723", "Internal error: Failed to convert double to string!");
    }

    // Truncate trailing zeros until uiMinDecimalPlaces is reached.
    // Examine the string from the end forward, first looking for trailing zeros,
    // then looking for the decimal place.
    int nTruncatePos = -1;
    for (unsigned int i = nSize - 1; i > 0; i--)
    {
        if (nTruncatePos > 0)
        {
            // If we've found a potential truncation position, look for a decimal place.
            if (pszTemp[i] == '.')
            {
                // If the decimal place is found, truncate the number at nTruncatePos.
                pszTemp[nTruncatePos] = '\0';
                break;
            }
        }
        else if (i >= uiMinDecimalPlaces && pszTemp[i - uiMinDecimalPlaces] == '.')
        {
            // As many trailing zeros as have been allowed have been processed.  Truncate now.
            pszTemp[i + ((uiMinDecimalPlaces == 0) ? 0 : 1)] = '\0';
            break;
        }
        else if (pszTemp[i] != '0')
        {
            // We've reached the end of any trailing zeros; set the truncation position
            nTruncatePos = i + 1;
        }
    }

    return string(pszTemp);
}
//--------------------------------------------------------------------------------------------------
string asString(long lValue)
{
    // return the long value as a string
    char pszTemp[128] = {0};
    sprintf_s(pszTemp, sizeof(pszTemp), "%d", lValue);
    return string(pszTemp);
}
//--------------------------------------------------------------------------------------------------
string asString(int iValue)
{
    // return the int value as a string
    char pszTemp[128] = {0};
    sprintf_s(pszTemp, sizeof(pszTemp), "%d", iValue);
    return string(pszTemp);
}
//--------------------------------------------------------------------------------------------------
string asString(LONGLONG llValue)
{
    // return the LONGLONG value as a string
    char pszTemp[128] = {0};
    sprintf_s(pszTemp, sizeof(pszTemp), "%I64d", llValue);
    return string(pszTemp);
}
//--------------------------------------------------------------------------------------------------
string asString(ULONGLONG ullValue)
{
    // return the ULONGLONG value as a string
    char pszTemp[128] = {0};
    sprintf_s(pszTemp, sizeof(pszTemp), "%I64u", ullValue);
    return string(pszTemp);
}
//--------------------------------------------------------------------------------------------------
string asString(const CLSID& clsID)
{
    try
    {
        OLECHAR wszCLSID[45] = {0};
        if (!StringFromGUID2(clsID, wszCLSID, 45))
        {
            throw UCLIDException("ELI02157",
                "Unable to convert GUID to string: Conversion function failed!");
        }

        // Get the string as a _bstr_t
        _bstr_t _bstrCLSID(wszCLSID);

        // Check for too short of a string (GUID is 35 characters, 37 if wrapped by {})
        if (_bstrCLSID.length() < 35)
        {
            UCLIDException ue("ELI27137",
                "Unable to convert GUID to string: Resulting string too short!");
            ue.addDebugInfo("String Length", _bstrCLSID.length());
            ue.addDebugInfo("Converted Value", _bstrCLSID.length() > 0 ? string(_bstrCLSID) : "");
            throw ue;
        }
        else
        {
            string strResult = _bstrCLSID;
            return strResult;
        }
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27136");
}
//--------------------------------------------------------------------------------------------------
double asDouble(const string& strValue)
{
    // ensure that the string is not empty
    if (strValue.empty())
    {
        UCLIDException ue( "ELI08616", "Cannot convert an empty string to a double value!" );
        throw ue;
    }

    char*   pszError;
    double  dValue = 0.0;
    string strExtractValue = trim(strValue, " ", " ");

    if (strExtractValue.find(',') != string::npos)
    {
        validateRemoveCommaDouble(strExtractValue);
    }

    // Extract value
    dValue = strtod( strExtractValue.c_str(), &pszError );
    if (pszError[0] != 0)
    {
        UCLIDException ue( "ELI02219", "Invalid double!" );
        ue.addDebugInfo( "Input string", strValue );
        throw ue;
    }

    return dValue;
}
//--------------------------------------------------------------------------------------------------
long asLong(const string& strValue)
{
    // ensure that the string is not empty
    if (strValue.empty())
    {
        UCLIDException ue( "ELI08614", "Cannot convert an empty string to a long value!" );
        throw ue;
    }

    char*   pszError;
    long    lValue = 0;
    string strExtractValue = trim(strValue, " ", " ");

    if (strExtractValue.find(',') != string::npos)
    {
        validateRemoveCommaInteger(strExtractValue);
    }

    // Extract value as base 10 long integer
    lValue = strtol( strExtractValue.c_str(), &pszError, 10 );
    if (pszError[0] != 0)
    {
        UCLIDException ue( "ELI02218", "Invalid long!" );
        ue.addDebugInfo( "Input string", strValue );
        throw ue;
    }

    return lValue;
}
//--------------------------------------------------------------------------------------------------
unsigned long asUnsignedLong(const string& strValue)
{
    // ensure that the string is not empty
    if (strValue.empty())
    {
        UCLIDException ue( "ELI08615", 
            "Cannot convert an empty string to an unsigned-long value!" );
        throw ue;
    }

    char*           pszError;
    unsigned long   ulValue = 0;
    string strExtractValue = trim(strValue, " ", " ");

    if (strExtractValue.find(',') != string::npos)
    {
        validateRemoveCommaInteger(strExtractValue);
    }

    // Check first character
    if (strExtractValue[0] == '-')
    {
        UCLIDException ue( "ELI04944", "Invalid unsigned long!" );
        ue.addDebugInfo( "Input string", strValue );
        throw ue;
    }

    // Extract value as base 10 unsigned long integer
    ulValue = strtoul( strExtractValue.c_str(), &pszError, 10 );
    if (pszError[0] != 0)
    {
        UCLIDException ue( "ELI02217", "Invalid unsigned long!" );
        ue.addDebugInfo( "Input string", strValue );
        throw ue;
    }

    return ulValue;
}
//--------------------------------------------------------------------------------------------------
LONGLONG asLongLong(const string& strValue)
{
    // ensure that the string is not empty
    if (strValue.empty())
    {
        UCLIDException ue( "ELI29969", "Cannot convert an empty string to a longlong value!" );
        throw ue;
    }

    char*   pszError;
    long    lValue = 0;
    string strExtractValue = trim(strValue, " ", " ");

    if (strExtractValue.find(',') != string::npos)
    {
        validateRemoveCommaInteger(strExtractValue);
    }

    // Convert to a longlong value
    LONGLONG llVal = _strtoi64(strExtractValue.c_str(), &pszError, 10);
    if (pszError[0] != 0)
    {
        UCLIDException ue( "ELI29970", "Invalid longlong!" );
        ue.addDebugInfo( "Input string", strValue );
        ue.addWin32ErrorInfo(errno);
        throw ue;
    }

    return llVal;
}
//--------------------------------------------------------------------------------------------------
ULONGLONG asUnsignedLongLong(const string& strValue)
{
    // ensure that the string is not empty
    if (strValue.empty())
    {
        UCLIDException ue( "ELI29971",
            "Cannot convert an empty string to an unsigned longlong value!" );
        throw ue;
    }

    char*   pszError;
    long    lValue = 0;
    string strExtractValue = trim(strValue, " ", " ");

    if (strExtractValue.find(',') != string::npos)
    {
        validateRemoveCommaInteger(strExtractValue);
    }

    // Check first character
    if (strExtractValue[0] == '-')
    {
        UCLIDException ue( "ELI29972", "Invalid unsigned longlong!" );
        ue.addDebugInfo( "Input string", strValue );
        throw ue;
    }

    // Convert to a longlong value
    ULONGLONG ullVal = _strtoui64(strExtractValue.c_str(), &pszError, 10);
    if (pszError[0] != 0)
    {
        UCLIDException ue( "ELI29973", "Invalid unsigned longlong!" );
        ue.addDebugInfo( "Input string", strValue );
        ue.addWin32ErrorInfo(errno);
        throw ue;
    }

    return ullVal;
}
//--------------------------------------------------------------------------------------------------
int getNextDelimiterPosition(const string &strText, int iStartingPosition, 
                             const string& strDelimiters)
{
    // find the first position in strText starting at iStartingPosition that contains any character
    // in strDelimiters.  If no position is found, return -1

    unsigned int iDelim;
    for (unsigned int iPos = iStartingPosition; iPos < strText.length(); iPos++)
    {
        for (iDelim = 0; iDelim < strDelimiters.length(); iDelim++)
        {
            if (strText[iPos] == strDelimiters[iDelim])
            {
                return iPos;
            }
        }
    }
    return -1;
}
//--------------------------------------------------------------------------------------------------
unsigned int getPositionOfFirstNumeric(const string &strText, unsigned int uiStartingPosition)
{
    return strText.find_first_of(gstrNUMBERS + ".", uiStartingPosition);
}
//--------------------------------------------------------------------------------------------------
unsigned int getPositionOfFirstAlpha(const string &strText, unsigned int uiStartingPosition)
{
    return strText.find_first_not_of(gstrNUMBERS + ".", uiStartingPosition);
}
//--------------------------------------------------------------------------------------------------
bool isAlphaNumeric(const string& strText, const unsigned int& uiPos)
{
    // check for empty string
    if (strText.empty())
    {
        return false;
    }

    if ( uiPos < strText.size() )
    {
        char cTemp = strText[uiPos];

        //if it's non-alphabetical chars
        if((cTemp >= 'A' && cTemp <= 'Z')
            || (cTemp >= 'a' && cTemp <= 'z')
            || (cTemp >= '0' && cTemp <= '9'))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    else
    {
        UCLIDException ue("ELI13020", "Position index is invalid!");
        ue.addDebugInfo("Position", uiPos);
        ue.addDebugInfo("Size", strText.size());
        throw ue;
    }
}
//--------------------------------------------------------------------------------------------------
bool containsNonWhitespaceChars(const string& strText)
{
    const string strWhiteSpaceChars = "\r\n\t\b\v\a\f ";
    return strText.find_first_not_of(strWhiteSpaceChars) != string::npos;
}
//--------------------------------------------------------------------------------------------------
bool containsAlphaNumericChar(const string& strText)
{
    return strText.find_first_of(gstrALPHA_NUMERIC) != string::npos;
}
//--------------------------------------------------------------------------------------------------
bool containsAlphaChar(const string& strText)
{
    return strText.find_first_of(gstrALPHA) != string::npos;
}
//--------------------------------------------------------------------------------------------------
bool isAllNumericChars(const string& strText, long& nAsLong)
{
	if (strText.empty())
	{
		return false;
	}
	for (auto& c : strText)
	{
		if (c < '0' || c > '9')
		{
			return false;
		}
	}
	char* pszError;
	nAsLong = strtol(strText.c_str(), &pszError, 10);
	if (pszError[0])
	{
		return false;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
// This function performs a search of the text to determine if it contains either:
//  1) all of the include text as a substring (false == matchSingleChar)
//  2) any character of the include text (true == matchSingleChar)
//
EXPORT_BaseUtils 
bool Contains( const std::string& text,            // the text to search
               const std::string& contains,        // the value or values to search text for
               ContainsMatchType matchType /*= MatchSubstring*/ )  // search for string (default) or single chars
{
    if ( matchType == MatchSingleChar )
    {
        for ( size_t i = 0; i < contains.size(); ++i )
        {
            if ( std::string::npos != text.find( contains[i] ) )
            {
                return true;
            }
        }

        return false;
    }

    return std::string::npos != text.find( contains );
}
//--------------------------------------------------------------------------------------------------
void makeFirstCharToUpper(string& strInput)
{
    // if the first character is an alphabetic character, then
    // make it upper case.
    if (!strInput.empty() && strInput[0] >= 'a' && strInput[0] <= 'z')
    {
        strInput[0] += 'A' - 'a';
    }
}
//--------------------------------------------------------------------------------------------------
string makeFirstCharToUpper(const string& strInput)
{
    string strResult = strInput;

    makeFirstCharToUpper(strResult);

    return strResult;
}
//--------------------------------------------------------------------------------------------------
string padCharacter(const string& strString, bool bPadBeginning, char cPadChar, 
                    unsigned long ulTotalStringLength)
{

    unsigned long ulInitialStringLength = strString.length();

    string strResult = strString;

    if (ulInitialStringLength < ulTotalStringLength)
    {
        // determine the number of characters of padding that is necessary
        unsigned long ulNumPadChar = ulTotalStringLength - ulInitialStringLength;

        // create the string that needs to be padded
        // TODO: for some reason the following code does not work in release mode...we need
        // to investigate this at some point.  Until then, let's use a more primitive method
        // char pszPadding[128]; // maximum only 128 digits of padding
        // _strnset(pszPadding, cPadChar, ulNumPadChar);
        // pszPadding[ulNumPadChar] = NULL; // terminate the padded string
        string strPadding;
        for (unsigned int i = 0; i < ulNumPadChar; i++)
        {
            strPadding += cPadChar;
        }

        // ifthe padding is to be done in the beginning, then insert the string
        // in the beginning, else append the string to the end
        if (bPadBeginning)
        {
            strResult.insert(0, strPadding);
        }
        else
        {
            strResult.append(strPadding);
        }
    }

    return strResult;
}
//--------------------------------------------------------------------------------------------------
string incrementNumericalSuffix(const string& strInput)
{
    string strResult = strInput;

    if (strInput != "")
    {
        string strTemp = strInput;

        const char *pszTemp = strTemp.c_str();
        const char *pszStart = pszTemp;

        pszTemp += strlen(pszTemp) - 1; // point to the last character.

        // the last character may not necessarily be a digit.  If it's a digit, then
        // we can try to perform the auto-increment
        if (*pszTemp >= '0' && *pszTemp <= '9')
        {
            // keep going backwards, searching for the first non-digit character
            while (pszTemp > pszStart && *pszTemp >= '0' && *pszTemp <= '9')
            {
                pszTemp--;
            }

            // we may have exited the previous loop because we hit an non-digit character
            // if that's the case, then we need to increment our pointer so that 
            // we point to the digits.
            if (*pszTemp < '0' || *pszTemp > '9')
            {
                pszTemp++;
            }

            // now pszTemp points to the last digits in the parcel ID.
            // determine a string that represents the next ID
            string  strLast = pszTemp;
            unsigned long ulLastID = asUnsignedLong( strLast );
            unsigned long ulNextID = ulLastID + 1;
            char pszNextID[128];
            sprintf_s(pszNextID, sizeof(pszNextID), "%d", ulNextID);

            // zero pad if necessary
            string strNextID = padCharacter(pszNextID, true, '0', strlen(pszTemp)); 

            // find the string that represents the characters that appeared before
            // the auto-incrementing digits
            unsigned long ulPrefixLength = pszTemp - pszStart;
            strResult.assign(strTemp.c_str(), ulPrefixLength);
            strResult += strNextID;
        }
        // Last character is not a digit
        else
        {
            // Just append a 1
            strResult.append( "1" );
        }
    }
    
    return strResult;
}
//--------------------------------------------------------------------------------------------------
int getNumberOfDigitsAfterPosition(const string &strText, int iPos)
{
    int iReturnVal = 0;
    for (unsigned int i = iPos; i < strText.length(); i++)
    {
        if (strText[i] >= '0' && strText[i] <= '9')
            iReturnVal++;
    }
    return iReturnVal;
}
//--------------------------------------------------------------------------------------------------
void replaceExprStrsWithEnvValues(string& strForReplace)
{
    //find the first "$(" characters
    unsigned int uiStartPos, uiEndPos;
    uiStartPos = strForReplace.find ("$(");
    //there's no env string in strForReplace
    if (uiStartPos == string::npos)
    {
        return;
    }

    uiEndPos = strForReplace.find(")", uiStartPos);
    if (uiEndPos == string::npos)
    {
        return;
    }

    while (uiEndPos < strForReplace.length())
    {   
        //the actual expression which includes "$(" and ")"
        string strExpr = strForReplace.substr(uiStartPos, uiEndPos - uiStartPos + 1);

        //get the environment string ( without spaces at two ends)
        string strEnvStr = trim(strForReplace.substr(uiStartPos + 2, uiEndPos - uiStartPos-2), 
            " \t", " \t");

        //retrieve the environment value
        string strEnvValue = getEnvironmentVariableValue(strEnvStr);
    
        //replace the expression with the env value
        replaceVariable(strForReplace, strExpr, strEnvValue);
        
        //re-search from position 0 since previous "$(..)" has been replaced
        uiStartPos = strForReplace.find("$(");

        //there's no more env string in strForReplace
        if (uiStartPos == string::npos)
        {
            return;
        }
        uiEndPos = strForReplace.find(")", uiStartPos);
        if (uiEndPos == string::npos)
        {
            return;
        }
    }
}
//-------------------------------------------------------------------------------------------------
string trim(string s)
{
    ltrim(s);
    rtrim(s);
    return s;
}
//-------------------------------------------------------------------------------------------------
string trimStart(string s)
{
    ltrim(s);
    return s;
}
//-------------------------------------------------------------------------------------------------
string trimEnd(string s)
{
    rtrim(s);
    return s;
}
//--------------------------------------------------------------------------------------------------
string trim(const string& s, const string& strBefore, const string& strAfter) 
{
    if (s.length() == 0)
    {
        return s;
    }

    unsigned int b = strBefore.empty() ? 0 : s.find_first_not_of(strBefore);
    unsigned int e = strAfter.empty() ? s.length() - 1 : s.find_last_not_of(strAfter);

    if (b == string::npos || e == string::npos)
    {
        return string("");
    }
    else
    {
        return s.substr(b, (e - b) + 1);
    }
}
//--------------------------------------------------------------------------------------------------
void convertCppStringToNormalString(string& strCppStr)
{
    replaceVariable(strCppStr, "\\", "\\\\");
    replaceVariable(strCppStr, "\n", "\\n");
    replaceVariable(strCppStr, "\r", "\\r");
    replaceVariable(strCppStr, "\t", "\\t");
    replaceVariable(strCppStr, "\a", "\\a");
    replaceVariable(strCppStr, "\b", "\\b");
    replaceVariable(strCppStr, "\f", "\\f");
    replaceVariable(strCppStr, "\v", "\\v");
    replaceVariable(strCppStr, "\"", "\\\"");
}
//--------------------------------------------------------------------------------------------------
void convertNormalStringToCppString(string& strNormalStr)
{
    size_t findpos = 0;
    unsigned int uiNormalStrLength = strNormalStr.length();
    while (findpos != string::npos && findpos < uiNormalStrLength)
    {
        findpos = strNormalStr.find("\\", findpos);
        if (findpos != string::npos && findpos < uiNormalStrLength - 1)
        {
            char nextChar(strNormalStr[findpos + 1]); 
            // replace "\\\\" with "\\"
            if (nextChar == '\\')
            {
                strNormalStr.replace(findpos, 2, "\\");
            }
            // replace "\\n" with "\n"
            else if (nextChar == 'n')
            {
                strNormalStr.replace(findpos, 2, "\n");
            }
            // replace "\\r" with "\r"
            else if (nextChar == 'r')
            {
                strNormalStr.replace(findpos, 2, "\r");
            }
            // replace "\\t" with "\t"
            else if (nextChar == 't')
            {
                strNormalStr.replace(findpos, 2, "\t");
            }
            // replace "\\xXX" with "\xXX"
            else if (nextChar == 'x')
            {
                // if the there is not a character after the x or the character is 
                // not a valid hex character there is nothing to do 
                // Only supporting specification of 1 or 2 hex digits after the x
                unsigned int uiFirstHexCharPos = findpos + 1;
                unsigned int uiSecondHexCharPos = findpos + 2;
    
                // if hex digits are found convert to the represented character
                if (uiFirstHexCharPos < uiNormalStrLength)
                {
                    char zChar[10] = "?"; 

                    unsigned char ucCurrChar = strNormalStr[uiFirstHexCharPos];

                    // Check if first location after x is a Hex digit
                    if (isHexChar(ucCurrChar))
                    {
                        // Convert to character value
                        zChar[0] = getValueOfHexChar(ucCurrChar);

                        int iNumberOfCharToReplaced = 3;

                        if (uiSecondHexCharPos < uiNormalStrLength )
                        {
                            ucCurrChar = strNormalStr[uiSecondHexCharPos];

                            // check if it is a hex digit
                            if ( isHexChar(ucCurrChar))
                            {
                                // move the current value in zChar[0] to the upper 4 bits and add the value 
                                // of the second digit
                                zChar[0] = zChar[0] * 16 + getValueOfHexChar(ucCurrChar);
                                iNumberOfCharToReplaced++;
                            }
                        }

                        // replace the characters representing the character with the character value
                        strNormalStr.replace(findpos, iNumberOfCharToReplaced, zChar);
                    }
                }
            }
            // replace "\\a" with "\a"
            else if (nextChar == 'a')
            {
                strNormalStr.replace(findpos, 2, "\a");
            }
            // replace "\\b" with "\b"
            else if (nextChar == 'b')
            {
                strNormalStr.replace(findpos, 2, "\b");
            }
            // replace "\\f" with "\f"
            else if (nextChar == 'f')
            {
                strNormalStr.replace(findpos, 2, "\f");
            }
            // replace "\\v" with "\v"
            else if (nextChar == 'v')
            {
                strNormalStr.replace(findpos, 2, "\v");
            }
            // replace "\\"" with "\""
            else if (nextChar == '\"')
            {
                strNormalStr.replace(findpos, 2, "\"");
            }
            
            findpos++;
        }
        else
        {
            break;
        }
    }
}
//--------------------------------------------------------------------------------------------------
void replaceASCIICharWithHex(char zAsciiChar, string& strForReplace)
{
    CString zHexNotion("");
    zHexNotion.Format("\\x%02x", zAsciiChar);
    
    replaceVariable(strForReplace, CString(zAsciiChar), string(zHexNotion));
}
//--------------------------------------------------------------------------------------------------
void makeTitleCase(string& strInput)
{   
    for (unsigned int i = 0; i < strInput.length(); i++)
    {
        if ((i != 0) && isalnum((unsigned char) strInput[i-1]))
        {
            strInput[i] = (char)tolower(strInput[i]);
        }
        else
        {
            strInput[i] = (char)toupper(strInput[i]);       
        }
    }
}
//--------------------------------------------------------------------------------------------------
void makeSentenceCase(string& strInput)
{
    if (strInput.empty())
    {
        return;
    }

    size_t nLength = strInput.length();
    strInput[0] = (char)toupper(strInput[0]);
    for(size_t i=1; i < nLength; i++)
    {
        strInput[i] = (char)tolower(strInput[i]);
    }
}
//--------------------------------------------------------------------------------------------------
void reverseString(string& strText)
{
    // Get pointer to input string
    char* pStr = (char*)strText.c_str();

    size_t nSize = strText.length();
    for (size_t i = 0; i < nSize / 2; i++)
    {
        // Do exclusive-or operations against pairs of characters
        pStr[i] ^= pStr[nSize - i - 1];
        pStr[nSize - i - 1] ^= pStr[i];
        pStr[i] ^= pStr[nSize - i - 1];
    }
}
//--------------------------------------------------------------------------------------------------
void convertStringToRegularExpression(string& str)
{
    string strResult = "";

    for(auto it = str.begin(); it != str.end(); it++)
    {
        switch(*it)
        {
        case '\\':
        case '.':
        case '?':
        case '*':
        case '^':
        case '$':
        case '+':
        case '(':
        case ')':
        case '[':
        case ']':
        case '{':
        case '}':
        case '|':
        case '!':
        case '-':
            strResult += '\\';
            strResult += *it;
            break;
        case '\r':
            strResult += "\\r";
            break;
        case '\n':
                strResult += "\\n";
            break;
        case '\t':
                strResult += "\\t";
            break;
        case '\f':
                strResult += "\\f";
            break;
        default:
            strResult += *it;
            break;
        }
    }
    
    str = strResult;
}
//--------------------------------------------------------------------------------------------------
// whether or not the strWord exists in the strInput and is a stand-alone word. i.e. there's
// no alpha-char on either side of the word.
// Returns the start position of the word in the input string. Returns -1 if not found 
// or is not a stand-alone word.
int findWordMatch(const string& strInput, const string& strWord, int nStartPos, bool bCaseSensitive)
{
    string strInputString(strInput);
    string strWordToSearch(strWord);
    // make all character to upper case if bCaseSensitive is false
    if (!bCaseSensitive)
    {
        makeUpperCase(strInputString);
        makeUpperCase(strWordToSearch);
    }
    
    unsigned int uiFoundPos = strInputString.find(strWordToSearch, nStartPos);
    // search each word in that paragraph
    if (uiFoundPos == string::npos)
    {
        return -1;
    }

    // make sure the word found the paragraph is not within another
    // word. Example, if 'lot' is the word to search for, then
    // 'lottery' shall not be counted
    
    // the position right before the word
    int nBeforePos = uiFoundPos - 1;
    // the position right after the word
    unsigned int uiAfterPos = uiFoundPos + strWordToSearch.size();
    
    if (nBeforePos >= 0 && uiAfterPos < strInputString.size())
    {
        if (isalnum((unsigned char) strInputString[nBeforePos]) 
            || isalnum((unsigned char) strInputString[uiAfterPos]))
        {
            // further search for the same word
            return findWordMatch(strInputString, strWord, uiAfterPos, bCaseSensitive);
        }
    }
    else if (nBeforePos < 0)
    {
        // if the word is actually followed by an alpha-char, return -1
        if (isAlphaChar(strInputString[uiAfterPos]))
        {
            // further search for the same word
            return findWordMatch(strInputString, strWord, uiAfterPos, bCaseSensitive);
        }
    }
    else if (uiAfterPos >= strInputString.size())
    {
        if (isAlphaChar(strInputString[nBeforePos]))
        {
            return -1;
        }
    }

    return uiFoundPos;
}
//--------------------------------------------------------------------------------------------------
void replaceWord(string& strInputText, 
                 const string& strWordToBeReplaced,
                 const string& strReplacementWord,
                 bool bReplaceFirstMatchOnly,
                 bool bCaseSensitive)
{
    int nFoundPos = ::findWordMatch(strInputText, strWordToBeReplaced, 0, bCaseSensitive);
    if (nFoundPos >= 0)
    {
        // replace
        strInputText.replace(nFoundPos, strWordToBeReplaced.size(), strReplacementWord);
        
        // if all matches need to be replaced
        if (!bReplaceFirstMatchOnly)
        {
            nFoundPos = ::findWordMatch(strInputText, strWordToBeReplaced, 0, bCaseSensitive);
            while (nFoundPos >= 0)
            {
                // replace
                strInputText.replace(nFoundPos, strWordToBeReplaced.size(), strReplacementWord);
                nFoundPos = ::findWordMatch(strInputText, strWordToBeReplaced, 0, bCaseSensitive);
            }
        }
    }
}
//--------------------------------------------------------------------------------------------------
//Note: This function was changed July 13, 2006 by Ryan Mulder from
//consolidateMultipleCharsIntoOne to its current state.
string replaceMultipleCharsWithOne(const string& strInput, 
                                       const string& strChars,
                                       const string& strReplace,
                                       bool bCaseSensitive)
{
    // the current starting position
    unsigned int uiCurrentPos = 0;
    // previously found position
    int nPrevFound = -1;
 
    string strToFind(strChars);
    string strRet(strInput);
    int strRetSize = strRet.size();

    if (!bCaseSensitive)
    {
        // make all upper case if case insensitive
        makeUpperCase(strToFind);
        makeUpperCase(strRet);
    }
    int pos = 0;
    //for(int x = 0; x < strRet.size(); x++)
    for(int x = 0; x < strRetSize; x++)
    {
        //check if the char at strTemp[x] is one that we're looking for
        pos = strToFind.find(strRet[x], 0);
        if(pos != string::npos )
        {
            //we know that it is one of our replacement characters.
            //check the previous character to see if it's our replacement char
            if(x > 0) //make sure we dont go out of bounds the first iteration
            {
                if(strRet[x-1] == strReplace[strReplace.size() - 1])
                {
                    //if we just replaced, erase the current 
                    //char and decrement x to check the next spot 
                    //(which is now moved over to the current spot.)
                    strRet.erase(x, 1);
                    x--;

                    //update the size variable to prevent out of bounds error
                    strRetSize = strRet.size();
                }//end if
                else
                {
                    //replace the char at location x with strReplace
                    //(use insert in case strReplace is longer than 1 character)
                    strRet.erase(x, 1);
                    strRet.insert(x, strReplace);
                    
                    //update the size variable to prevent out of bounds error
                    strRetSize = strRet.size();
                }//end else
            }//end if
        }//end if
    }//end for

    return strRet;
}//end replaceMultipleCharsWithOne
//--------------------------------------------------------------------------------------------------
string combine(const string& strInput, 
               const vector<string>& vecInterpretations,
               const string& strSpecialCharacter)
{
    string strRet("");

    unsigned int uiInputSize = strInput.length();

    // current reading position in the input string
    unsigned int uiCurrentPos = 0;
    unsigned int uiStartPos = 0, uiEndPos = 0;
    // find first percent sign if any
    uiStartPos = strInput.find(strSpecialCharacter);
    // get the text leading the first percent sign
    if (uiStartPos != string::npos)
    {
        strRet += strInput.substr(uiCurrentPos, uiStartPos - uiCurrentPos);
    }
    else
    {
        // if no percent sign is found, return the original string
        return strInput;
    }

    while (uiStartPos != string::npos)
    {
        if (uiStartPos >= uiInputSize - 1)
        {
            UCLIDException ue("ELI06915", 
                "Input string contains percent sign at the end of the string.");
            ue.addDebugInfo("Input String", strInput);
            throw ue;
        }

        // find next percent sign
        uiEndPos = strInput.find(strSpecialCharacter, uiStartPos + 1);
        // depends on the position of next found percent
        if (uiEndPos == string::npos)
        {
            // if there's no more percent sign
            // interpret the previous pattern, and append it to strRet
            string strNumber("");
            unsigned int uiNumberOfDigits = 0;
            for ( unsigned int u = uiStartPos + 1; u < uiInputSize; u++)
            {
                if (::isdigit((unsigned char) strInput[u]))
                {
                    strNumber += strInput.substr(u, 1);
                    
                    // number of digits after the last percent sign
                    uiNumberOfDigits++;
                }
                else
                {
                    break;
                }
            }

            // can't be empty
            if (strNumber.empty())
            {
                UCLIDException ue("ELI06916", 
                    "Any single percent sign must be followed by a number or "
                    "another percent sign.");
                ue.addDebugInfo("Input String", strInput);
                throw ue;
            }

            // now get interpreted value
            unsigned long ulNumber = asUnsignedLong(strNumber);
            if (ulNumber == 0 || ulNumber > vecInterpretations.size())
            {
                // can't be 0
                UCLIDException ue("ELI06917", 
                    "Invalid index! Any index number specified in the input string shall "
                    "not be zero or exceeding the vector size.");
                ue.addDebugInfo("Index", ulNumber);
                ue.addDebugInfo("Input String", strInput);
                throw ue;
            }

            strRet += vecInterpretations[ulNumber-1];
            
            uiCurrentPos = uiStartPos + uiNumberOfDigits + 1;

            // get the rest of the string
            strRet += strInput.substr(uiCurrentPos);

            // break out of the loop
            break;
        }

        // We found next percent sign, what to do?
        // 1) check if these two percent signs are next to each other
        if (uiEndPos == uiStartPos + 1)
        {
            // These two percent sign are next to each other
            // that is interpreted as one literal percent sign character
            strRet += strInput.substr(uiStartPos, 1);
            // Go find the next percent
            uiStartPos = strInput.find(strSpecialCharacter, uiEndPos + 1);
            if (uiStartPos != string::npos)
            {
                // get the string inbetween last percent the next percent
                strRet += strInput.substr(uiEndPos + 1, uiStartPos - uiEndPos - 1);
            }
        }
        // 2) if there's any character inbetween these two percent signs
        else
        {
            string strNumber("");
            unsigned int uiNumberOfDigits = 0;
            for ( unsigned int u = uiStartPos + 1; u < uiInputSize; u++)
            {
                if (::isdigit((unsigned char) strInput[u]))
                {
                    strNumber += strInput.substr(u, 1);
                    
                    // number of digits after the last percent sign
                    uiNumberOfDigits++;
                }
                else
                {
                    break;
                }
            }

            // can't be empty
            if (strNumber.empty())
            {
                UCLIDException ue("ELI06918", 
                    "Any single percent sign must be followed by a number "
                    "or another percent sign.");
                ue.addDebugInfo("Input String", strInput);
                throw ue;
            }

            // now get interpreted value
            unsigned long ulNumber = asUnsignedLong(strNumber);
            if (ulNumber == 0 || ulNumber > vecInterpretations.size())
            {
                // can't be 0
                UCLIDException ue("ELI06919", 
                    "Invalid index! Any index specified in the input string "
                    "shall not be zero or exceeding the vector size.");
                ue.addDebugInfo("Input String", strInput);
                ue.addDebugInfo("ulNumber", ulNumber);
                throw ue;
            }

            strRet += vecInterpretations[ulNumber-1];
            
            uiCurrentPos = uiStartPos + uiNumberOfDigits + 1;

            // get the string in between previous and next percent signs
            strRet += strInput.substr(uiCurrentPos, uiEndPos - uiCurrentPos);
            
            // set positions
            uiStartPos = uiEndPos;
        }
    };

    return strRet;
}
//--------------------------------------------------------------------------------------------------
string asString(vector<string> vecLines, 
                bool bRemoveWhiteSpace,
                const string& strSeparator )
{
    string strString = "";
    for (unsigned int i = 0; i < vecLines.size(); i++ )
    {
        string strCurrStr = vecLines[i];
        if ( bRemoveWhiteSpace )
        {
            unsigned int uiWSPos = 0;
            do
            {
                // find the first whitespace in the string from the previous location
                uiWSPos = strCurrStr.find_first_of(" \f\n\r\t\v", uiWSPos );
                // remove that character just one character
                if ( uiWSPos != string::npos )
                {
                    strCurrStr.erase( uiWSPos, 1 );
                }
            }
            while ( uiWSPos != string::npos );
        }

        // Add a separator only between items, not before the first item.
        if (i > 0)
        {           
            strString += strSeparator;
        }

        // Append line to return string with separator
        strString += strCurrStr;
    }
    return strString;
}
//--------------------------------------------------------------------------------------------------
unsigned long getCountOfSpecificCharInString(const string& strText, char cChar)
{
    unsigned long ulCount = 0;
    unsigned long ulLength = strText.length();
    const char *pszText = strText.c_str();
    for (unsigned int i = 0; i < ulLength; i++)
    {
        if (pszText[i] == cChar)
        {
            ulCount++;
        }
    }

    return ulCount;
}
//--------------------------------------------------------------------------------------------------
// NOTE: The character after a word can be anything except A-Z
//       and still get trimmed.
bool trimLeadingWord(string &strInput, const string& strWord)
{
    // Check for empty string
    if (strInput.length() == 0)
    {
        return false;
    }

    // Remove any leading and trailing whitespace from input string
    strInput = trim( strInput, " \r\n", " \r\n" );
    long    lOriginalLength = strInput.length();
    long    lLength = lOriginalLength;

    // Remove any leading and trailing whitespace from trimming word
    string  strTrimWord = trim( strWord, " \r\n", " \r\n" );
    long    lWordLength = strTrimWord.length();

    // Continue trimming of Word until no further trims are needed
    bool    bStillTrimming = (lLength > 0) ? true : false;
    while (bStillTrimming)
    {
        // Loop through characters in word to be trimmed
        long lPos = 0;
        for (int i = 0; (i < lWordLength && lPos < lLength); i++)
        {
            // Special handling for whitespace in trimming word
            bool bIsSpace = isWhitespaceChar( strTrimWord[i] );

            if (!bIsSpace)
            {
                // Get upper-case and lower-case characters
                int iUCChar = toupper( strTrimWord[i] );
                int iLCChar = tolower( strTrimWord[i] );

                // Input string must be long enough
                // Check this character
                if ((lLength < lWordLength) || 
                    (strInput[lPos] != iUCChar && strInput[lPos] != iLCChar))
                {
                    // Match failure, stop checking this word
                    bStillTrimming = false;
                    break;
                }

                // Increment character position
                lPos++;
            }
            else
            {
                // Look for one or more whitespace characters
                bool bFoundSpace = false;
                while (lLength > lPos)
                {
                    // Check for whitespace
                    if (isWhitespaceChar( strInput[lPos] ))
                    {
                        // Set flag and advance to the next character
                        bFoundSpace = true;
                        lPos++;
                    }
                    else
                    {
                        // Not a space, move on to next check
                        break;
                    }
                }

                // Update trim position if extra spaces found
                if (!bFoundSpace)
                {
                    bStillTrimming = false;
                    break;
                }
            }
        }

        // Each char matched, next char must not be alphabetic
        if (bStillTrimming)
        {
            if ((lLength > 0) && (lLength > lWordLength) && 
                isAlphaChar( strInput[lWordLength] ))
            {
                // Word is first part of a longer word, DO NOT trim
                bStillTrimming = false;
            }
        }

        // Trim the word and look for a repeat
        if ((lLength > 0) && bStillTrimming)
        {
            // Remove leading word and next character
            if (lLength > lPos + 1)
            {
                strInput = strInput.substr( lPos + 1, lLength - lPos - 1 );

                // Remove all subsequent leading whitespace
                strInput = trim( strInput, " \r\n", "" );
                lLength = strInput.length();
            }
            else
            {
                // Original string matches the trimmed word, just return ""
                strInput = "";
                lLength = 0;
                bStillTrimming = false;
            }
        }
    }       // end while continuing through the trimming sequence

    return (lLength < lOriginalLength);
}
//--------------------------------------------------------------------------------------------------
string commaFormatNumber(LONGLONG nNum)
{
    // Initialize number string to empty string
    string strNum = "";
    LONGLONG nInc = 1000;

    // Check to see if it is negative
    bool bNegative = nNum < 0;

    // Change number to the absolute value so number will always be > 0
    nNum = _abs64(nNum);

    // While number has at least 4 digits
    while(nNum > 999)
    {
        // This will return the last 3 digits
        LONGLONG nDigits = nNum % nInc;

        // Buffer for the string representation of the 3 digits
        char buf[4];
        
        // Convert to string with with 0 padding - will be exactly 3 digits
        sprintf_s(buf, sizeof(buf), "%03I64d", nDigits);

        // Insert the 3 digits at the beginning of the string
        strNum.insert(0, buf);

        // Add the comma for this group
        strNum.insert(0, ",");

        // subtract off the lower 3 digits
        nNum -= nDigits;

        // Divide by 1000 - adjusting value to get next 3 digits
        nNum /= nInc;
    }
    
    // There will be no more than 3 digits
    char buf[4];
    sprintf_s(buf, sizeof(buf), "%I64d", nNum);

    // Put the digits at the beginning of the string
    strNum.insert(0, buf);

    // if the value was negative add the '-' sign
    if ( bNegative )
    {
        strNum.insert(0,"-");
    }

    // return the formatted string
    return strNum;
}
//--------------------------------------------------------------------------------------------------
string commaFormatNumber(double fNum, long nPrecision)
{
    LONGLONG nNum = (LONGLONG) fNum;
    double digits = fNum - (double) nNum;
    char buf[512];
    sprintf_s(buf, sizeof(buf), "%.*f", nPrecision, digits);
    string strRet = buf;
    // remove the leading 0
    strRet.erase(strRet.begin());
    strRet = commaFormatNumber(nNum) + strRet;
    return strRet;
}
//--------------------------------------------------------------------------------------------------
string removeUnprintableCharacters(const string& strInput)
{
    string strRet("");

    // Evaluate each character and skip unprintable low-order ASCII chars
    long lSize = strInput.length();
    strRet.reserve(lSize);
    for (int i = 0; i < lSize; i++)
    {
        // Check for valid character
        unsigned char c = strInput[i];
        if (isPrintableChar(c))
        {
            // Character is valid, append to output string
            strRet.push_back(c);
        }
    }

    return strRet;
}
//--------------------------------------------------------------------------------------------------
bool isSentenceCase(const string& strText)
{
    return !strText.empty()
        && strText.substr(0,1).find_first_of(gstrUPPER_ALPHA) != string::npos
        && strText.substr(1).find_first_of(gstrUPPER_ALPHA) == string::npos;
}
//--------------------------------------------------------------------------------------------------
bool endsWith(const string& strValue, const string& strEnding)
{
	// Adapted from:
	// http://stackoverflow.com/questions/874134/find-if-string-ends-with-another-string-in-c
    if (strEnding.size() <= strValue.size())
	{
		return equal(strEnding.rbegin(), strEnding.rend(), strValue.rbegin());
	}
	else
	{
		return false;
	}
}
//--------------------------------------------------------------------------------------------------
VectorOfString Split( const std::string& source, const char delimiter )
{
	VectorOfString results;
	size_t pos = 0;
	size_t startPos = 0;
	while ( true )
	{
		pos = source.find( delimiter, startPos );
		const auto length = pos - startPos;
		results.push_back( source.substr( startPos, length ) );
		if ( pos == std::string::npos)
		{
			break;
		}

		startPos = pos + 1;
	}

	return std::move( results );
}
//--------------------------------------------------------------------------------------------------
EXPORT_BaseUtils char getWindows1252FromUTF8(const string& strCharacter)
{
	ASSERT_ARGUMENT("ELI46789", !strCharacter.empty());

	if (strCharacter.length() == 1)
	{
		return strCharacter[0];
	}

	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
	std::wstring UTF16VersionOfString = converter.from_bytes(strCharacter);

	// GCV OCR results can contain chars that use multiple UTF16 code points
	// (just use the first one without throwing an exception in this case)
	// https://extract.atlassian.net/browse/ISSUE-18817
	ASSERT_ARGUMENT("ELI46796", UTF16VersionOfString.length() >= 1);

	unsigned long code = (unsigned long)UTF16VersionOfString[0];
	if (code <= 0x007F || (code >= 0x00A0 && code <= 0x00FF))
	{
		return (char)code;
	}

	switch (code)
	{
		case 0x20AC:
			return (char)0x80;
		case 0xFFFD:
			return (char)0x81;
		case 0x201A:
			return (char)0x82;
		case 0x0192:
			return (char)0x83;
		case 0x201E:
			return (char)0x84;
		case 0x2026:
			return (char)0x85;
		case 0x2020:
			return (char)0x86;
		case 0x2021:
			return (char)0x87;
		case 0x02C6:
			return (char)0x88;
		case 0x2030:
			return (char)0x89;
		case 0x0160:
			return (char)0x8A;
		case 0x2039:
			return (char)0x8B;
		case 0x0152:
			return (char)0x8C;
		case 0x017D:
			return (char)0x8E;
		case 0x2018:
			return (char)0x91;
		case 0x2019:
			return (char)0x92;
		case 0x201C:
			return (char)0x93;
		case 0x201D:
			return (char)0x94;
		case 0x2022:
			return (char)0x95;
		case 0x2013:
			return (char)0x96;
		case 0x2014:
			return (char)0x97;
		case 0x02DC:
			return (char)0x98;
		case 0x2122:
			return (char)0x99;
		case 0x0161:
			return (char)0x9A;
		case 0x203A:
			return (char)0x9B;
		case 0x0153:
			return (char)0x9C;
		case 0x017E:
			return (char)0x9E;
		case 0x0178:
			return (char)0x9F;
		default:
			return '^';
	}
}
//--------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string normalizeNewlines(const string& strInput)
{
	if (strInput.empty())
	{
		return strInput;
	}

	bool endsInNewline = strInput.back() == '\n';
	stringstream source(strInput);
	string line;
	stringstream dest;
	while (std::getline(source, line, '\n'))
	{
		dest << line;
		if (!source.eof() || endsInNewline)
		{
			if (line.empty() || line.back() != '\r')
			{
				dest << '\r';
			}
			dest << '\n';
		}
	}

	return dest.str();
}
//-----------------------------------------------------------------------------------------------------
namespace Util
{
	void checkPasswordComplexity(const string& password, const string& complexityRequirements)
	{
		static const regex upper("[A-Z]");
		static const regex lower("[a-z]");
		static const regex digit("[0-9]");
		static const regex punct("[[:punct:]]");

        long lengthRequirement;
        bool requireUppercase, requireLowercase, requireDigit, requirePunctuation;

        decodePasswordComplexityRequirements(complexityRequirements.c_str(),
            lengthRequirement, requireUppercase, requireLowercase, requireDigit, requirePunctuation);

		// Validate the password, add info for any rule failure
		UCLIDException ue("ELI51867", "This password does not meet the complexity requirements");
		bool isValid = true;
		if ((long)password.length() < lengthRequirement)
		{
			isValid = false;
			ue.addDebugInfo("Failed requirement", Format("Password must be at least %d characters long", lengthRequirement));
		}
		if (requireUppercase && !regex_search(password, upper))
		{
			isValid = false;
			ue.addDebugInfo("Failed requirement", "Password must contain at least one upper case letter (A-Z)");
		}
		if (requireLowercase && !regex_search(password, lower))
		{
			isValid = false;
			ue.addDebugInfo("Failed requirement", "Password must contain at least one lower case letter (a-z)");
		}
		if (requireDigit && !regex_search(password, digit))
		{
			isValid = false;
			ue.addDebugInfo("Failed requirement", "Password must contain at least one digit (0-9)");
		}
		if (requirePunctuation && !regex_search(password, punct))
		{
			isValid = false;
			ue.addDebugInfo("Failed requirement", "Password must contain at least one punctuation character (!\"#$...)");
		}
		if (!isValid)
		{
			throw ue;
		}
	}
	//-------------------------------------------------------------------------------------------------
	void decodePasswordComplexityRequirements(const char* szComplexityRequirements,
        long& lengthRequirement, bool& requireUppercase, bool& requireLowercase, bool& requireDigit, bool& requirePunctuation)
	{
		static const regex complexityPattern("(\\d+)[ULDP]*");

		// Default to minimum 1 char with no other requirements if the complexity string is empty
		lengthRequirement = 1;
		requireUppercase = false;
		requireLowercase = false;
		requireDigit = false;
		requirePunctuation = false;

        string complexityRequirements = szComplexityRequirements == __nullptr ? "" : szComplexityRequirements;

		// Parse the complexity requirements if provided
		if (!complexityRequirements.empty())
		{
			smatch subMatches;
			bool isValid = regex_match(complexityRequirements, subMatches, complexityPattern);
			if (isValid)
			{
				// Get the length and make sure it is > 0
				string lengthPart = subMatches[1];
				try
				{
					lengthRequirement = asLong(lengthPart);
					isValid = lengthRequirement > 0;
				}
				catch (...)
				{
					isValid = false;
				}
			}

			// Get the categories if the encoding is valid
			if (isValid)
			{
				requireUppercase = complexityRequirements.find('U') != string::npos;
				requireLowercase = complexityRequirements.find('L') != string::npos;
				requireDigit = complexityRequirements.find('D') != string::npos;
				requirePunctuation = complexityRequirements.find('P') != string::npos;
			}
			// Use strict requirements, length of 8 and all categories required, if the encoded string isn't valid
			else
			{
				lengthRequirement = 8;
				requireUppercase = true;
				requireLowercase = true;
				requireDigit = true;
				requirePunctuation = true;
			}
		}
	}
} // end of namespace Util