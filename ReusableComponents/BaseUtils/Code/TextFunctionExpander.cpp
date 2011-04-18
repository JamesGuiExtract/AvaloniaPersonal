#include "stdafx.h"
#include "cpputil.h"
#include "StringTokenizer.h"
#include "TextFunctionExpander.h"
#include "UCLIDException.h"

#include <list>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Globals
//-------------------------------------------------------------------------------------------------
bool g_bInit = false;
Win32CriticalSection g_cs;
vector<string> g_vecFunctions;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrFUNC_CHANGE_EXT = "ChangeExt";
const string gstrFUNC_DIR_NO_DRIVE_OF = "DirNoDriveOf";
const string gstrFUNC_DIR_OF = "DirOf";
const string gstrFUNC_DRIVE_OF = "DriveOf";
const string gstrFUNC_ENV = "Env";
const string gstrFUNC_EXT_OF = "ExtOf";
const string gstrFUNC_FILE_NO_EXT_OF = "FileNoExtOf";
const string gstrFUNC_FILE_OF = "FileOf";
const string gstrFUNC_FULL_USER_NAME = "FullUserName";
const string gstrFUNC_LEFT = "Left";
const string gstrFUNC_INSERT_BEFORE_EXT = "InsertBeforeExt";
const string gstrFUNC_MID = "Mid";
const string gstrFUNC_NOW = "Now";
const string gstrFUNC_OFFSET = "Offset";
const string gstrFUNC_PAD_VALUE = "PadValue";
const string gstrFUNC_RANDOM_ALPHA_NUMERIC = "RandomAlphaNumeric";
const string gstrFUNC_REPLACE = "Replace";
const string gstrFUNC_RIGHT = "Right";
const string gstrFUNC_TRIM_AND_CONSOLIDATE_WS = "TrimAndConsolidateWS";
const string gstrFUNC_USER_NAME = "UserName";

//-------------------------------------------------------------------------------------------------
// Static initialization
//-------------------------------------------------------------------------------------------------
Random TextFunctionExpander::ms_Rand;

//-------------------------------------------------------------------------------------------------
// TextFunctionExpander
//-------------------------------------------------------------------------------------------------
TextFunctionExpander::TextFunctionExpander()
{
	if (!g_bInit)
	{
		Win32CriticalSectionLockGuard lg(g_cs);
		if (!g_bInit)
		{
			// Add the functions in alphabetical order
			g_vecFunctions.push_back(gstrFUNC_CHANGE_EXT);
			g_vecFunctions.push_back(gstrFUNC_DIR_NO_DRIVE_OF);
			g_vecFunctions.push_back(gstrFUNC_DIR_OF);
			g_vecFunctions.push_back(gstrFUNC_DRIVE_OF);
			g_vecFunctions.push_back(gstrFUNC_ENV);
			g_vecFunctions.push_back(gstrFUNC_EXT_OF);
			g_vecFunctions.push_back(gstrFUNC_FILE_NO_EXT_OF);
			g_vecFunctions.push_back(gstrFUNC_FILE_OF);
			g_vecFunctions.push_back(gstrFUNC_FULL_USER_NAME);
			g_vecFunctions.push_back(gstrFUNC_INSERT_BEFORE_EXT);
			g_vecFunctions.push_back(gstrFUNC_LEFT);
			g_vecFunctions.push_back(gstrFUNC_MID);
			g_vecFunctions.push_back(gstrFUNC_NOW);
			g_vecFunctions.push_back(gstrFUNC_OFFSET);
			g_vecFunctions.push_back(gstrFUNC_PAD_VALUE);
			g_vecFunctions.push_back(gstrFUNC_RANDOM_ALPHA_NUMERIC);
			g_vecFunctions.push_back(gstrFUNC_REPLACE);
			g_vecFunctions.push_back(gstrFUNC_RIGHT);
			g_vecFunctions.push_back(gstrFUNC_TRIM_AND_CONSOLIDATE_WS);
			g_vecFunctions.push_back(gstrFUNC_USER_NAME);

			g_bInit = true;
		}
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandFunctions(const string& str) const
{
	
	string strRet;

	unsigned long ulSearchPos = 0;
	while (1)
	{
		// find the beginning of the next function if there is one
		unsigned long ulFuncStart = str.find( '$', ulSearchPos );
		
		if (ulFuncStart == string::npos)
		{
			// there are no more functions so append any 
			// remaining text to the return string and exit
			strRet += str.substr( ulSearchPos, string::npos );
			break;
		}

		// check if the location begins with one of the functions
		unsigned int nNumFunctions = g_vecFunctions.size();
		unsigned long ulTemp = ulFuncStart + 1;
		string strFunction = "";
		string strToken = ",";
		for ( unsigned int i = 0; i < nNumFunctions ; i++ )
		{
			unsigned long ulNamePos = str.find( g_vecFunctions[i], ulTemp );
			if ( ulNamePos == ulTemp )
			{
				// Set the function
				strFunction = g_vecFunctions[i];

				// Check for {}
				unsigned long ulBracketPos = ulNamePos + strFunction.length();
				if (ulBracketPos < str.length() && str[ulBracketPos] == '{')
				{
					// Find the closing bracket
					unsigned long ulClosingBracket = str.find_first_of("}", ulBracketPos+1);
					bool bMatched = ulClosingBracket != string::npos;
					if (!bMatched || ulClosingBracket - ulBracketPos > 2)
					{
						string strMessage = bMatched ?
							"Token length too long (only single token supported) " : "Unmatched '{' ";
						strMessage += "in function string.";
						UCLIDException uex("ELI29707", strMessage);
						uex.addDebugInfo("Function String", str.substr(ulNamePos,
							bMatched ? ulClosingBracket : ulBracketPos));
						throw uex;
					}
					else if (ulClosingBracket - ulBracketPos == 1)
					{
						UCLIDException uex("ELI29709", "Missing token definition.");
						uex.addDebugInfo("Function String", str.substr(ulNamePos, ulClosingBracket));
						throw uex;
					}

					// Ensure the function is one that supports an alternate token
					if (strFunction != gstrFUNC_INSERT_BEFORE_EXT
						&& strFunction != gstrFUNC_OFFSET
						&& strFunction != gstrFUNC_PAD_VALUE
						&& strFunction != gstrFUNC_REPLACE
						&& strFunction != gstrFUNC_LEFT
						&& strFunction != gstrFUNC_MID
						&& strFunction != gstrFUNC_RIGHT
						&& strFunction != gstrFUNC_CHANGE_EXT)
					{
						UCLIDException uex("ELI29708", "Function does not support alternate token syntax.");
						uex.addDebugInfo("Function", strFunction);
						throw uex;
					}

					// Get the alternate token
					strToken = str[ulBracketPos+1];
				}

				break;
			}
		}

		if (strFunction.empty())
		{
			// a function was not found so $ is part of the file name
			// advance to the next char and continue loop
			strRet += str.substr( ulSearchPos, ulFuncStart - ulSearchPos + 1);
			ulSearchPos = ulTemp;
			continue;
		}

		// append any text before the new function to the 
		// return string
		if (ulFuncStart != ulSearchPos)
		{
			strRet += str.substr( ulSearchPos, ulFuncStart - ulSearchPos );
		}

		ulSearchPos = ulTemp;

		// find the beginning of the function argument i.e. '('
		unsigned long ulArgStart = str.find( '(', ulSearchPos );
		if (ulArgStart == string::npos)
		{
			UCLIDException ue("ELI11767", "Invalid Text Function Syntax - no (.");
			ue.addDebugInfo("Text", str);
			throw ue;
		}

		ulSearchPos = ulArgStart + 1;

		// find the end of the function argument i.e. the matching ')'
		// Note there may be multiple "()" pairs between the opening '(' 
		// and the closing ')', hence the use of getCloseScopePos
		unsigned long ulArgEnd = getCloseScopePos(str, ulArgStart, '(', ')');
		if (ulArgEnd == string::npos)
		{
			UCLIDException ue("ELI11768", "Invalid Text Function Syntax - no ).");
			ue.addDebugInfo("Text", str);
			throw ue;
		}
		// currently getCloseScopePos() returns the close pos +1
		ulArgEnd--;
		ulSearchPos = ulArgEnd + 1;

		// get the argument to the functions
		string strArg = str.substr( ulArgStart + 1, ulArgEnd - (ulArgStart + 1) );
		
		// recurse to expand any functions in the argument
		strArg = expandFunctions(strArg);

		// evaluate the function and append the result to the return 
		// text (expanded string)
		if (strFunction == gstrFUNC_DIR_OF)
		{
			strRet += expandDirOf(strArg);
		}
		else if (strFunction == gstrFUNC_FILE_NO_EXT_OF)
		{
			strRet += expandFileNoExtOf(strArg);
		}
		else if (strFunction == gstrFUNC_FILE_OF)
		{
			strRet += expandFileOf(strArg);
		}
		else if (strFunction == gstrFUNC_DIR_NO_DRIVE_OF)
		{
			strRet += expandDirNoDriveOf(strArg);
		}
		else if (strFunction == gstrFUNC_DRIVE_OF)
		{
			strRet += expandDriveOf(strArg);
		}
		else if (strFunction == gstrFUNC_EXT_OF)
		{
			strRet += expandExtOf(strArg);
		}
		else if (strFunction == gstrFUNC_LEFT)
		{
			strRet += expandLeft(strArg, strToken);
		}
		else if (strFunction == gstrFUNC_INSERT_BEFORE_EXT)
		{
			strRet += expandInsertBeforeExt(strArg, strToken);
		}
		else if (strFunction == gstrFUNC_CHANGE_EXT)
		{
			strRet += expandChangeExt(strArg, strToken);
		}
		else if (strFunction == gstrFUNC_MID)
		{
			strRet += expandMid(strArg, strToken);
		}
		else if (strFunction == gstrFUNC_REPLACE)
		{
			strRet += expandReplace(strArg, strToken);
		}
		else if (strFunction == gstrFUNC_RIGHT)
		{
			strRet += expandRight(strArg, strToken);
		}
		else if (strFunction == gstrFUNC_PAD_VALUE)
		{
			strRet += expandPadValue(strArg, strToken);
		}
		else if (strFunction == gstrFUNC_OFFSET)
		{
			strRet += expandOffset(strArg, strToken);
		}
		else if (strFunction == gstrFUNC_TRIM_AND_CONSOLIDATE_WS)
		{
			strRet += expandTrimAndConsolidateWS( strArg );
		}
		else if (strFunction == gstrFUNC_ENV)
		{
			strRet += expandEnv(strArg);
		}
		else if (strFunction == gstrFUNC_NOW)
		{
			strRet += expandNow(strArg);
		}
		else if (strFunction == gstrFUNC_RANDOM_ALPHA_NUMERIC)
		{
			strRet += expandRandomAlphaNumeric(strArg);
		}
		else if (strFunction == gstrFUNC_USER_NAME)
		{
			strRet += expandUserName(strArg);
		}
		else if (strFunction == gstrFUNC_FULL_USER_NAME)
		{
			strRet += expandFullUserName(strArg);
		}
		else
		{
			UCLIDException ue("ELI11769", "Invalid Text Function!");
			ue.addDebugInfo("Function", strFunction);
			throw ue;
		}
	}
	return strRet;
}
//-------------------------------------------------------------------------------------------------
const vector<string>& TextFunctionExpander::getAvailableFunctions() const
{
	return g_vecFunctions;
}
//-------------------------------------------------------------------------------------------------
bool TextFunctionExpander::isFunctionAvailable(const string& strFunction) const
{
	for (unsigned int i = 0; i < g_vecFunctions.size(); i++)
	{
		if (strFunction == g_vecFunctions[i])
		{
			return true;
		}
	}
	return false;
}
//--------------------------------------------------------------------------------------------------
void TextFunctionExpander::formatFunctions(vector<string>& vecFunctions) const
{
	for (unsigned int i = 0; i < vecFunctions.size(); i++)
	{
		string& str = vecFunctions[i];
		str = "$" + str + "()";
	}
}
//-------------------------------------------------------------------------------------------------
bool TextFunctionExpander::isValidParameters(const string& strFunction, 
											 const string& strArgument, 
											 const string& strParameter) const
{
	// Locate strFunction
	if (isFunctionAvailable(strFunction))
	{
		// Construct whole string as:
		// "$" + strFunction + "(" + strArgument + "," + strParameter + ")"
		string strTotal = string( "$" ) + strFunction + "(";
		if (strArgument != "")
		{
			// Argument is defined, add it to the total string
			strTotal += strArgument;
		}

		if (strParameter != "")
		{
			// Parameter is defined, add a comma separator and the parameter to the total string
			// Note that strParameter can contain multiple parameter items, each comma separated
			strTotal += ",";
			strTotal += strParameter;
		}

		// Complete the total string with a closing parenthesis
		strTotal += ")";

		// Test result and eat any exception
		try
		{
			string strTemp;
			strTemp = expandFunctions( strTotal );

			// Function + Argument + Parameter valid if no exception thrown
			return true;
		}
		catch (...)
		{
			return false;
		}
	}

	// strFunction not found
	return false;
}

//-------------------------------------------------------------------------------------------------
// Private
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandDirNoDriveOf(const string& str) const
{
	// Retrieve full directory with drive
	string strFullDir = getDirectoryFromFullPath(str, false);

	// Retrieve drive letter and backslash
	string strDrive = getDriveFromFullPath(str, false);

	// Remove leading drive and backslash
	long lFullDirLength = strFullDir.length();
	long lDriveLength = strDrive.length();
	if (lFullDirLength > lDriveLength)
	{
		// Return just the directory name
		return strFullDir.substr(lDriveLength);
	}
	else
	{
		// Drive length is at least as long as the directory length
		// so this may be a UNC path, just return an empty string (P13 #4610)
		return "";
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandDirOf(const string& str) const
{
	return getDirectoryFromFullPath(str, false);
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandDriveOf(const string& str) const
{
	return getDriveFromFullPath(str, false);
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandExtOf(const string& str) const
{
	string strTmp = getExtensionFromFullPath(str, false);
	strTmp.erase(0, 1);
	return strTmp;
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandFileOf(const string& str) const
{
	return getFileNameFromFullPath(str, false);
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandFileNoExtOf(const string& str) const
{
	return getFileNameWithoutExtension(str, false);
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandInsertBeforeExt(const string& str,
														 const string& strToken) const
{
	// Tokenize the string into strSource, strInsert
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(str, strToken, vecTokens);

	// Check for proper number of tokens
	if (vecTokens.size() == 2)
	{
		// Retrieve each string
		string strSource = vecTokens[0];
		string strInsert = vecTokens[1];

		// Source string cannot be empty
		if (strSource.length() == 0)
		{
			// Create and throw exception
			UCLIDException ue( "ELI19657", "InsertBeforeExt function cannot operate on an empty string!");
			ue.addDebugInfo("Arguments", str);
			throw ue;
		}

		// Get the file extension
		string strExt = getExtensionFromFullPath( strSource );
		if (strExt.length() == 0)
		{
			// Create and throw exception
			UCLIDException ue( "ELI19676", "Source text in InsertBeforeExt function does not have a file extension!");
			ue.addDebugInfo("Source Text", strSource);
			throw ue;
		}

		// Find starting position of this file extension
		long lPos = strSource.rfind( strExt );
		if (lPos != string::npos)
		{
			// No need to insert an empty string
			if (strInsert.length() > 0)
			{
				// Insert the desired string
				strSource.insert( lPos, strInsert.c_str() );
			}
		}
		else
		{
			// We are expecting to find the previously located extension
			THROW_LOGIC_ERROR_EXCEPTION( "ELI19675" );
		}

		// Return the result of the string insert
		return strSource;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI19656", "InsertBeforeExt function has invalid number of arguments!");
		ue.addDebugInfo("Arguments", str);
		ue.addDebugInfo("NumOfArgs", vecTokens.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandOffset(const string& str, const string& strToken) const
{
	vector<string> vecTokens;

	StringTokenizer::sGetTokens(str, strToken, vecTokens);

	unsigned int i = 0;
	for ( ; i < vecTokens.size(); i++)
	{
		// remove leading and trailing spaces
		vecTokens[i] = trim(vecTokens[i], " ", " ");
	}

	long lValue = 0;

	if (vecTokens.size() == 2)
	{
		try
		{
			lValue = asLong(vecTokens[0]);
			long lOffset = asLong(vecTokens[1]);

			lValue += lOffset;
		}
		catch(...)
		{
			UCLIDException uex("ELI30027", "Invalid long value(s) for Offset function.");
			uex.addDebugInfo("Value", vecTokens[0]);
			uex.addDebugInfo("Offset Value", vecTokens[1]);
			throw uex;
		}
	}
	else
	{
		UCLIDException ue("ELI12624", "Offset Function has invalid number of arguments!");
		ue.addDebugInfo("Args", str);
		ue.addDebugInfo("NumOfArgs", vecTokens.size());
		ue.addDebugInfo("ArgsExpected", 2);
		throw ue;
	}

	try
	{
		string strVal = asString(lValue);
		return strVal;
	}
	catch(...)
	{
		UCLIDException ue("ELI12936", "Unable to convert long to string!");
		ue.addDebugInfo("Value", lValue);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandPadValue(const string& str, const string& strToken) const
{
	vector<string> vecTokens;

	StringTokenizer::sGetTokens(str, strToken, vecTokens);

	unsigned int i = 0;
	for( ; i < vecTokens.size(); i++)
	{
		// remove leading and trailing spaces
		vecTokens[i] = trim(vecTokens[i], " ", " ");
	}

	if(vecTokens.size() == 3)
	{
		char cPadWithChar = 0;

		if(vecTokens[1].size() == 1)
		{
			cPadWithChar = vecTokens[1][0];
		}
		else
		{
			UCLIDException ue("ELI12576", "PadValue Function has invalid padding character argument!");
			ue.addDebugInfo("Args", str);
			ue.addDebugInfo("PaddingChar", vecTokens[1]);
			throw ue;
		}

		int iPadLength = 0;
		try
		{
			iPadLength = (int)asUnsignedLong(vecTokens[2]);
		}
		catch(...)
		{
			UCLIDException uex("ELI30026", "Invalid padding length specified.");
			uex.addDebugInfo("Padding Length", vecTokens[2]);
			throw uex;
		}
		
		return padCharacter(vecTokens[0], true, cPadWithChar, iPadLength);
	}
	else
	{
		UCLIDException ue("ELI12575", "PadValue Function has invalid number of arguments!");
		ue.addDebugInfo("Args", str);
		ue.addDebugInfo("NumOfArgs", vecTokens.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandReplace(const string& str, const string& strToken) const
{
	// Tokenize the string into strSource, strSearch, strReplace
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(str, strToken, vecTokens);

	// Check for proper number of tokens
	if (vecTokens.size() == 3)
	{
		// Retrieve each string
		string strSource = vecTokens[0];
		string strSearch = vecTokens[1];
		string strReplace = vecTokens[2];

		// Source string cannot be empty
		if (strSource.length() == 0)
		{
			// Create and throw exception
			UCLIDException ue( "ELI15712", "Replace function cannot operate on an empty string!");
			ue.addDebugInfo("Arguments", str);
			throw ue;
		}

		// Search string cannot be empty
		if (strSearch.length() == 0)
		{
			// Create and throw exception
			UCLIDException ue( "ELI15713", "Replace function cannot search for an empty string!");
			ue.addDebugInfo("Arguments", str);
			throw ue;
		}

		// Do the string replacement
		replaceVariable( strSource, strSearch, strReplace );

		// Return the result of the string replace
		return strSource;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI15711", "Replace function has invalid number of arguments!");
		ue.addDebugInfo("Arguments", str);
		ue.addDebugInfo("NumOfArgs", vecTokens.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandTrimAndConsolidateWS(const string& str) const
{
	string strResult = str;
	string strWS = " \t\r\n";

	// Trim all leading and trailing whitespace
	strResult = trim( strResult, strWS, strWS );

	//remove all internal whitespace
	strResult = replaceMultipleCharsWithOne(strResult, strWS, " ", true);

	return strResult;
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandEnv(const string &str) const
{
	if (str.empty())
	{
		throw UCLIDException("ELI29487", "Env function requires an environment variable name.");
	}

	// Return the value for the environment variable
	string strResult = getEnvironmentVariableValue(str);
	if (strResult.empty())
	{
		UCLIDException uex("ELI29490", "Specified environment variable could not be found.");
		uex.addDebugInfo("Environment Variable", str);
		throw uex;
	}

	return strResult;
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandNow(const string &str) const
{
	try
	{
		string strReturnTime;

		// If no format argument is present, return the default string
		// "YYYY-MM-DD-HH-MM-SS-mmm"
		if (str.empty())
		{
			// Get the local time
			SYSTEMTIME st;
			GetLocalTime(&st);

			char zTime[24] = {0};
			int nRet = sprintf_s(zTime, 24, "%04d-%02d-%02d-%02d-%02d-%02d-%03d", st.wYear,
				st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds);
			if (nRet == -1)
			{
				UCLIDException uex("ELI28701", "Unable to format time string.");
				uex.addWin32ErrorInfo(errno);
				throw uex;
			}

			strReturnTime = zTime;
		}
		else
		{
			// Get the current time and format it based on the format string
			CTime cTime(_time64(NULL));
			CString zTime = cTime.Format(str.c_str());

			// Return the formatted time
			strReturnTime = (LPCTSTR)zTime;
		}

		return strReturnTime;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28763");
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandRandomAlphaNumeric(const string &str) const
{
	if (str.empty())
	{
		throw UCLIDException("ELI29488", "RandomAlphaNumeric function requires a number of digits.");
	}

	long nLength = 0;
	try
	{
		// Get the length from the argument
		nLength = asLong(str);
	}
	catch(...)
	{
		UCLIDException uex("ELI29489",
			"Invalid number string specified for RandomAlphaNumeric function.");
		uex.addDebugInfo("Argument", str);
		throw uex;
	}

	// Return a random string of nLength containing only upper case letters and digits
	return ms_Rand.getRandomString(nLength, true, false, true);
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandUserName(const string& str) const
{
	if (!str.empty())
	{
		UCLIDException uex("ELI28764", "$UserName() does not accept arguments.");
		uex.addDebugInfo("Argument", str);
		throw uex;
	}

	return getCurrentUserName();
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandFullUserName(const string& str) const
{
	if (!str.empty() && (str != "0" && str != "1"))
	{
		UCLIDException uex("ELI28765", "Invalid argument for $FullUserName().");
		uex.addDebugInfo("Argument", str);
		throw uex;
	}

	bool bThrowException = str != "1";
	return getFullUserName(bThrowException);
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandLeft(const string& str, const string& strToken) const
{
	// Tokenize the string into strSource, strSearch, strReplace
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(str, strToken, vecTokens);

	// Check for appropriate number of tokens
	if (vecTokens.size() == 2)
	{
		// Get the count of characters
		long lCount = 0;
		try
		{
			lCount = asLong(vecTokens[1]);
			if (lCount <= 0)
			{
				throw 42;
			}
		}
		catch(...)
		{
			UCLIDException ue("ELI29957", "Invalid length argument specified for $Left().");
			ue.addDebugInfo("Length Argument", vecTokens[1]);
			throw ue;
		}

		// Return the first lCount characters of the string
		return vecTokens[0].substr(0, lCount);
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI29958", "Left function has invalid number of arguments!");
		ue.addDebugInfo("Arguments", str);
		ue.addDebugInfo("NumOfArgs", vecTokens.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandMid(const string& str, const string& strToken) const
{
	// Tokenize the string into strSource, strSearch, strReplace
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(str, strToken, vecTokens);

	// Get the string
	const string& strTemp = vecTokens[0];

	// Check for appropriate number of tokens
	if (vecTokens.size() == 3)
	{
		// Get the start character
		long lStart = 1;
		try
		{
			lStart = asLong(vecTokens[1]);
			if (lStart < 1 || ((unsigned long) lStart > strTemp.length()))
			{
				throw 42;
			}

			// Convert 1 based start to 0 based
			lStart--;
		}
		catch(...)
		{
			UCLIDException ue("ELI29959", "Invalid starting position specified for $Mid().");
			ue.addDebugInfo("Start Position", vecTokens[1]);
			throw ue;
		}

		// Get the count of characters
		long lCount = 0;
		try
		{
			lCount = asLong(vecTokens[2]);
			if (lCount <= -2 || lCount == 0)
			{
				throw 42;
			}
		}
		catch(...)
		{
			UCLIDException ue("ELI29960", "Invalid count value specified for $Mid().");
			ue.addDebugInfo("Count", vecTokens[2]);
			throw ue;
		}

		// If the count is -1, then return from the start to the end of the string
		if (lCount == -1)
		{
			return strTemp.substr(lStart);
		}
		else
		{
			// Return the substring starting at lStart and containing lCount characters
			return strTemp.substr(lStart, lCount);
		}
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI29961", "Mid function has invalid number of arguments!");
		ue.addDebugInfo("Arguments", str);
		ue.addDebugInfo("NumOfArgs", vecTokens.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandRight(const string& str, const string& strToken) const
{
	// Tokenize the string into strSource, strSearch, strReplace
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(str, strToken, vecTokens);

	// Check for appropriate number of tokens
	if (vecTokens.size() == 2)
	{
		// Get the count of characters
		long lCount = 0;
		try
		{
			lCount = asLong(vecTokens[1]);
			if (lCount <= 0)
			{
				throw 42;
			}
		}
		catch(...)
		{
			UCLIDException ue("ELI29962", "Invalid length argument specified for $Right().");
			ue.addDebugInfo("Length Argument", vecTokens[1]);
			throw ue;
		}

		// Get the string
		const string& strTemp = vecTokens[0];

		// Compute the starting point for the substring
		long lStart = strTemp.length() - lCount;
		if (lStart < 0)
		{
			return strTemp;
		}
		else
		{
			return strTemp.substr(lStart);
		}
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI29963", "Right function has invalid number of arguments!");
		ue.addDebugInfo("Arguments", str);
		ue.addDebugInfo("NumOfArgs", vecTokens.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandChangeExt(const string& str, const string& strToken) const
{
	// Tokenize the string into strSource, strInsert
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(str, strToken, vecTokens);

	// Check for proper number of tokens
	if (vecTokens.size() == 2)
	{
		// Retrieve each string
		string strSource = vecTokens[0];
		string strExtension = vecTokens[1];

		// Source string cannot be empty
		if (strSource.length() == 0)
		{
			// Create and throw exception
			UCLIDException ue( "ELI30019", "ChangeExt function cannot operate on an empty string!");
			ue.addDebugInfo("Arguments", str);
			throw ue;
		}

		string strExt = getExtensionFromFullPath( strSource );
		if (strExt.length() == 0)
		{
			// Create and throw exception
			UCLIDException ue( "ELI30020",
				"Source text in ChangeExt function does not have a file extension!");
			ue.addDebugInfo("Source Text", strSource);
			throw ue;
		}

		// Find starting position of this file extension
		long lPos = strSource.rfind( strExt );
		if (lPos != string::npos)
		{
			// Replace the extension
			strSource = strSource.substr(0, lPos+1) + strExtension;
		}
		else
		{
			// We are expecting to find the previously located extension
			THROW_LOGIC_ERROR_EXCEPTION( "ELI30021" );
		}

		// Return the result of the string insert
		return strSource;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI30022", "ChangeExt function has invalid number of arguments!");
		ue.addDebugInfo("Arguments", str);
		ue.addDebugInfo("NumOfArgs", vecTokens.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
