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

class Function
{
public:
	string m_strFunction;
	string m_strArg;
	long m_nStartPos;
	long m_nEndPos;
};

//-------------------------------------------------------------------------------------------------
// Static initialization
//-------------------------------------------------------------------------------------------------
Random TextFunctionExpander::ms_Rand;

//-------------------------------------------------------------------------------------------------
// TextFunctionExpander
//-------------------------------------------------------------------------------------------------
TextFunctionExpander::TextFunctionExpander()
{
	Win32CriticalSectionLockGuard lg(g_cs);
	if (!g_bInit)
	{
		// Add the functions in alphabetical order
		g_vecFunctions.push_back("DirNoDriveOf");
		g_vecFunctions.push_back("DirOf");
		g_vecFunctions.push_back("DriveOf");
		g_vecFunctions.push_back("Env");
		g_vecFunctions.push_back("ExtOf");
		g_vecFunctions.push_back("FileNoExtOf");
		g_vecFunctions.push_back("FileOf");
		g_vecFunctions.push_back("FullUserName");
		g_vecFunctions.push_back("InsertBeforeExt");
		g_vecFunctions.push_back("Now");
		g_vecFunctions.push_back("Offset");
		g_vecFunctions.push_back("PadValue");
		g_vecFunctions.push_back("RandomAlphaNumeric");
		g_vecFunctions.push_back("Replace");
		g_vecFunctions.push_back("TrimAndConsolidateWS");
		g_vecFunctions.push_back("UserName");

		g_bInit = true;
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
		bool bFound = false;
		for ( unsigned int i = 0; !bFound && i < nNumFunctions ; i++ )
		{
			unsigned long ulNamePos = str.find( g_vecFunctions[i], ulFuncStart + 1 );
			if ( ulNamePos == ulFuncStart + 1 )
			{
				bFound = true;
				break;
			}
		}

		if ( !bFound )
		{
			// a function was not found so $ is part of the file name
			// advance to the next char and continue loop
			strRet += str.substr( ulSearchPos, ulFuncStart - ulSearchPos + 1);
			ulSearchPos = ulFuncStart + 1;
			continue;
		}

		// append any text before the new function to the 
		// return string
		if (ulFuncStart != ulSearchPos)
		{
			strRet += str.substr( ulSearchPos, ulFuncStart - ulSearchPos );
		}

		ulSearchPos = ulFuncStart + 1;

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

		// get the function name e.g. "dirof"
		string strFunction = str.substr( ulFuncStart + 1, ulArgStart - (ulFuncStart + 1) );
		
		// case insensitive function names
		makeLowerCase(strFunction);

		// evaluate the function and append the result to the return 
		// text (expanded string)
		if (strFunction == "dirof")
		{
			strRet += expandDirOf(strArg);
		}
		else if (strFunction == "filenoextof")
		{
			strRet += expandFileNoExtOf(strArg);
		}
		else if (strFunction == "fileof")
		{
			strRet += expandFileOf(strArg);
		}
		else if (strFunction == "dirnodriveof")
		{
			strRet += expandDirNoDriveOf(strArg);
		}
		else if (strFunction == "driveof")
		{
			strRet += expandDriveOf(strArg);
		}
		else if (strFunction == "extof")
		{
			strRet += expandExtOf(strArg);
		}
		else if (strFunction == "insertbeforeext")
		{
			strRet += expandInsertBeforeExt(strArg);
		}
		else if (strFunction == "replace")
		{
			strRet += expandReplace(strArg);
		}
		else if (strFunction == "padvalue")
		{
			strRet += expandPadValue(strArg);
		}
		else if (strFunction == "offset")
		{
			strRet += expandOffset(strArg);
		}
		else if (strFunction == "trimandconsolidatews")
		{
			strRet += expandTrimAndConsolidateWS( strArg );
		}
		else if (strFunction == "env")
		{
			strRet += expandEnv(strArg);
		}
		else if (strFunction == "now")
		{
			strRet += expandNow(strArg);
		}
		else if (strFunction == "randomalphanumeric")
		{
			strRet += expandRandomAlphaNumeric(strArg);
		}
		else if (strFunction == "username")
		{
			strRet += expandUserName(strArg);
		}
		else if (strFunction == "fullusername")
		{
			strRet += expandFullUserName(strArg);
		}
		else
		{
			UCLIDException ue("ELI11769", "Invalid Text Function!");
			ue.addDebugInfo("Function", strFunction);
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
	string strFullDir = expandDirOf( str );

	// Retrieve drive letter and backslash
	string strDrive = expandDriveOf( str );

	// Remove leading drive and backslash
	long lFullDirLength = strFullDir.length();
	long lDriveLength = strDrive.length();
	if (lFullDirLength > lDriveLength)
	{
		// Return just the directory name
		return str.substr( lDriveLength, lFullDirLength - lDriveLength );
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
const string TextFunctionExpander::expandInsertBeforeExt(const string& str) const
{
	// Tokenize the string into strSource, strInsert
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(str, ",", vecTokens);

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
const string TextFunctionExpander::expandOffset(const string& str) const
{
	vector<string> vecTokens;

	StringTokenizer::sGetTokens(str, ",", vecTokens);

	unsigned int i = 0;
	for ( ; i < vecTokens.size(); i++)
	{
		// remove leading and trailing spaces
		vecTokens[i] = trim(vecTokens[i], " ", " ");
	}

	long lValue = 0;

	if (vecTokens.size() == 2)
	{
		lValue = asLong(vecTokens[0]);
		long lOffset = asLong(vecTokens[1]);

		lValue += lOffset;
	}
	else
	{
		UCLIDException ue("ELI12624", "Offset Function has invalid number of arguments!");
		ue.addDebugInfo("Args", str);
		ue.addDebugInfo("NumOfArgs", vecTokens.size());
		ue.addDebugInfo("ArgsExpected", 2);
		throw ue;
	}

	char pszValue[50];
	if (_ltoa_s(lValue, pszValue, sizeof(pszValue), 10) != 0)
	{
		UCLIDException ue("ELI12936", "Unable to convert long to string!");
		ue.addDebugInfo("lValue", lValue);
		throw ue;
	}

	return string(pszValue);
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandPadValue(const string& str) const
{
	vector<string> vecTokens;

	StringTokenizer::sGetTokens(str, ",", vecTokens);

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

		int iPadLength = (int)asUnsignedLong(vecTokens[2]);
		
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
const string TextFunctionExpander::expandReplace(const string& str) const
{
	// Tokenize the string into strSource, strSearch, strReplace
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(str, ",", vecTokens);

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
