#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "TextFunctionExpander.h"

#include <cpputil.h>
#include <StringTokenizer.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <StringCSIS.h>
#include <Misc.h>
#include <CommentedTextFileReader.h>

#include <list>
#include <stack>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Globals
//-------------------------------------------------------------------------------------------------
bool g_bInit = false;
Win32CriticalSection g_cs;
vector<string> g_vecFunctions;
map<string, string> g_mapParameters;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrFUNC_CHANGE_EXT = "ChangeExt";
const string gstrFUNC_CHANGE_EXT_PARAMS = "source, extension";

const string gstrFUNC_DIR_NO_DRIVE_OF = "DirNoDriveOf";
const string gstrFUNC_DIR_NO_DRIVE_OF_PARAMS = "source";

const string gstrFUNC_DIR_OF = "DirOf";
const string gstrFUNC_DIR_OF_PARAMS = "source";

const string gstrFUNC_DRIVE_OF = "DriveOf";
const string gstrFUNC_DRIVE_OF_PARAMS = "source";

const string gstrFUNC_ENV = "Env";
const string gstrFUNC_ENV_PARAMS = "environment variable";

const string gstrFUNC_EXT_OF = "ExtOf";
const string gstrFUNC_EXT_OF_PARAMS = "source";

const string gstrFUNC_FILE_NO_EXT_OF = "FileNoExtOf";
const string gstrFUNC_FILE_NO_EXT_OF_PARAMS = "source";

const string gstrFUNC_FILE_OF = "FileOf";
const string gstrFUNC_FILE_OF_PARAMS = "source";

const string gstrFUNC_FULL_USER_NAME = "FullUserName";
const string gstrFUNC_FULL_USER_NAME_PARAMS = "1 = use $UserName as fall back";

const string gstrFUNC_LEFT = "Left";
const string gstrFUNC_LEFT_PARAMS = "source, count";

const string gstrFUNC_INSERT_BEFORE_EXT = "InsertBeforeExt";
const string gstrFUNC_INSERT_BEFORE_EXT_PARAMS = "source, text to insert";

const string gstrFUNC_MID = "Mid";
const string gstrFUNC_MID_PARAMS = "source, index, count (optional)";

const string gstrFUNC_NOW = "Now";
const string gstrFUNC_NOW_PARAMS = "format (optional)";

const string gstrFUNC_OFFSET = "Offset";
const string gstrFUNC_OFFSET_PARAMS = "number, offset";

const string gstrFUNC_PAD_VALUE = "PadValue";
const string gstrFUNC_PAD_VALUE_PARAMS = "source, character, length";

const string gstrFUNC_RANDOM_ALPHA_NUMERIC = "RandomAlphaNumeric";
const string gstrFUNC_RANDOM_ALPHA_NUMERIC_PARAMS = "num digits";

const string gstrFUNC_RANDOM_ENTRY_FROM_LIST_FILE = "RandomEntryFromListFile";
const string gstrFUNC_RANDOM_ENTRY_FROM_LIST_FILE_PARAMS = "filename";

const string gstrFUNC_RANDOM_ENTRY_FROM_LIST = "RandomEntryFromList";
const string gstrFUNC_RANDOM_ENTRY_FROM_LIST_PARAMS = "item, item, ...";

const string gstrFUNC_REPLACE = "Replace";
const string gstrFUNC_REPLACE_PARAMS = "source, search, replacement";

const string gstrFUNC_RIGHT = "Right";
const string gstrFUNC_RIGHT_PARAMS = "source, count";

const string gstrFUNC_TRIM_AND_CONSOLIDATE_WS = "TrimAndConsolidateWS";
const string gstrFUNC_TRIM_AND_CONSOLIDATE_WS_PARAMS = "source";

const string gstrFUNC_USER_NAME = "UserName";
const string gstrFUNC_USER_NAME_PARAMS = "";

const string gstrFUNC_THREAD_ID = "ThreadId";
const string gstrFUNC_THREAD_ID_PARAMS = "";

const string gstrFUNC_PROCESS_ID = "ProcessId";
const string gstrFUNC_PROCESS_ID_PARAMS = "";

const string gstrFUNC_LOWER_CASE = "LowerCase";
const string gstrFUNC_LOWER_CASE_PARAMS = "source";

const string gstrFUNC_UPPER_CASE = "UpperCase";
const string gstrFUNC_UPPER_CASE_PARAMS = "source";

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
			g_mapParameters[gstrFUNC_CHANGE_EXT] = gstrFUNC_CHANGE_EXT_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_DIR_NO_DRIVE_OF);
			g_mapParameters[gstrFUNC_DIR_NO_DRIVE_OF] = gstrFUNC_DIR_NO_DRIVE_OF_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_DIR_OF);
			g_mapParameters[gstrFUNC_DIR_OF] = gstrFUNC_DIR_OF_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_DRIVE_OF);
			g_mapParameters[gstrFUNC_DRIVE_OF] = gstrFUNC_DRIVE_OF_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_ENV);
			g_mapParameters[gstrFUNC_ENV] = gstrFUNC_ENV_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_EXT_OF);
			g_mapParameters[gstrFUNC_EXT_OF] = gstrFUNC_EXT_OF_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_FILE_NO_EXT_OF);
			g_mapParameters[gstrFUNC_FILE_NO_EXT_OF] = gstrFUNC_FILE_NO_EXT_OF_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_FILE_OF);
			g_mapParameters[gstrFUNC_FILE_OF] = gstrFUNC_FILE_OF_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_FULL_USER_NAME);
			g_mapParameters[gstrFUNC_FULL_USER_NAME] = gstrFUNC_FULL_USER_NAME_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_INSERT_BEFORE_EXT);
			g_mapParameters[gstrFUNC_INSERT_BEFORE_EXT] = gstrFUNC_INSERT_BEFORE_EXT_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_LEFT);
			g_mapParameters[gstrFUNC_LEFT] = gstrFUNC_LEFT_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_LOWER_CASE);
			g_mapParameters[gstrFUNC_LOWER_CASE] = gstrFUNC_LOWER_CASE_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_MID);
			g_mapParameters[gstrFUNC_MID] = gstrFUNC_MID_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_NOW);
			g_mapParameters[gstrFUNC_NOW] = gstrFUNC_NOW_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_OFFSET);
			g_mapParameters[gstrFUNC_OFFSET] = gstrFUNC_OFFSET_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_PAD_VALUE);
			g_mapParameters[gstrFUNC_PAD_VALUE] = gstrFUNC_PAD_VALUE_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_PROCESS_ID);
			g_mapParameters[gstrFUNC_PROCESS_ID] = gstrFUNC_PROCESS_ID_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_RANDOM_ALPHA_NUMERIC);
			g_mapParameters[gstrFUNC_RANDOM_ALPHA_NUMERIC] = gstrFUNC_RANDOM_ALPHA_NUMERIC_PARAMS;
			// So that "RandomEntryFromList" is not found as part of "RandomEntryFromListFile",
			// "RandomEntryFromListFile" must come first in this list.
			g_vecFunctions.push_back(gstrFUNC_RANDOM_ENTRY_FROM_LIST_FILE);
			g_mapParameters[gstrFUNC_RANDOM_ENTRY_FROM_LIST_FILE] = gstrFUNC_RANDOM_ENTRY_FROM_LIST_FILE_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_RANDOM_ENTRY_FROM_LIST);
			g_mapParameters[gstrFUNC_RANDOM_ENTRY_FROM_LIST] = gstrFUNC_RANDOM_ENTRY_FROM_LIST_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_REPLACE);
			g_mapParameters[gstrFUNC_REPLACE] = gstrFUNC_REPLACE_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_RIGHT);
			g_mapParameters[gstrFUNC_RIGHT] = gstrFUNC_RIGHT_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_THREAD_ID);
			g_mapParameters[gstrFUNC_THREAD_ID] = gstrFUNC_THREAD_ID_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_TRIM_AND_CONSOLIDATE_WS);
			g_mapParameters[gstrFUNC_TRIM_AND_CONSOLIDATE_WS] = gstrFUNC_TRIM_AND_CONSOLIDATE_WS_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_UPPER_CASE);
			g_mapParameters[gstrFUNC_UPPER_CASE] = gstrFUNC_UPPER_CASE_PARAMS;
			g_vecFunctions.push_back(gstrFUNC_USER_NAME);
			g_mapParameters[gstrFUNC_USER_NAME] = gstrFUNC_USER_NAME_PARAMS;

			g_bInit = true;
		}
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandFunctions(const string& str,
	UCLID_COMUTILSLib::ITagUtilityPtr ipTagUtility, BSTR bstrSourceDocName, IUnknown *pData, long recursionDepth)
{
	try
	{
		ASSERT_RUNTIME_CONDITION("ELI53547", recursionDepth < 42,
			"Exceeded maximum level of recursion allowed while expanding path tags!");

		// Define a stack to keep track of data relevant to each function scope as str is expanded.
		stack<expansionScopeData> stackScopeData;
		stackScopeData.push(expansionScopeData());

		// The current position of str for from which to search for the next section to expand.
		unsigned long ulSearchPos = 0;

		// Keeps track of data related to the next function in str to be expanded.
		bool bSearchForNextFunc = true;
		unsigned long ulFuncStart = string::npos;
		string strNextFunction;
		string strNextFuncToken;
		
		// Loop until str is fully expanded.
		while (true)
		{
			// Get the data relative to the current function scope.
			expansionScopeData& currentScope = stackScopeData.top();

			// If necessary, find the beginning of the next function if there is one
			if (bSearchForNextFunc)
			{
				ulFuncStart = findNextFunction(str, ulSearchPos, strNextFunction, strNextFuncToken, ipTagUtility);

				// Don't bother searching again until the found function is processed.
				bSearchForNextFunc = false;
			}
			
			// Keep track of the end of the next section of str to be processed. This may be the
			// start or end of a function or a token delimiting function parameters.
			unsigned long ulSectionEnd = ulFuncStart;

			// If parsing the argument for a function that takes multiple parameters, look for the
			// next token that delimits parameters.
			unsigned long ulNextToken = string::npos;
			if (currentScope.bAcceptsMultipleArgs)
			{
				ulNextToken = str.find(currentScope.strFuncToken, ulSearchPos);
				ulSectionEnd = (ulSectionEnd == string::npos) ? ulNextToken
					: (ulNextToken == string::npos) ? ulSectionEnd
						: min(ulSectionEnd, ulNextToken);
			}

			// If within the scope of a function, look for the closing paren that ends this scope.
			unsigned long ulNextScopeClose = string::npos;
			if (!currentScope.strFunction.empty())
			{
				ulNextScopeClose = str.find(')', ulSearchPos);
				ulSectionEnd = (ulSectionEnd == string::npos) ? ulNextScopeClose
					: (ulNextScopeClose == string::npos) ? ulSectionEnd
						: min(ulSectionEnd, ulNextScopeClose);
			}

			// If the section from ulSearchPos to ulSectionEnd is non-empty, expand any tags it
			// contains and add it to the running result for the current scope.
			if (ulSectionEnd == string::npos || ulSectionEnd > ulSearchPos)
			{
				string strNextSection = str.substr(ulSearchPos, (ulSectionEnd == string::npos)
					? string::npos
					: ulSectionEnd - ulSearchPos);

				strNextSection = recursivelyExpandTagsAndFunctions(strNextSection, ipTagUtility, bstrSourceDocName, pData, recursionDepth);

				currentScope.strResult += strNextSection;
			}

			if (ulSectionEnd == string::npos)
			{
				// There are no more arguments or function scope changes for the rest of str, return
				// the result for the current (top) scope.
				if (!currentScope.strFunction.empty())
				{
					UCLIDException ue("ELI35254", "Missing closing parenthesis for path tag function.");
					ue.addDebugInfo("Function", currentScope.strFunction);
					ue.addDebugInfo("Full string", str);
				}

				return currentScope.strResult;
			}
			else if (ulSectionEnd == ulFuncStart)
			{
				// A new function scope comes before anything else.
				// Find the beginning of the next function argument. i.e., the first '(' after the
				// next '$')
				// https://extract.atlassian.net/browse/ISSUE-12410
				// Added line to ignore parenthesis if not following a $ used to indicate a path tag
				// func.
				ulSearchPos = str.find('$', ulSearchPos);
				ulSearchPos = str.find('(', ulSearchPos);
				if (ulSearchPos == string::npos)
				{
					UCLIDException ue("ELI11767", "Invalid Text Function Syntax - no (.");
					ue.addDebugInfo("Text", str);
					throw ue;
				}
				ulSearchPos++;

				// Create the scope for the nested function.
				expansionScopeData nestedScope;
				nestedScope.ulArgStartPos = ulSearchPos;
				nestedScope.strFunction = strNextFunction;
				nestedScope.strFuncToken = strNextFuncToken;
				// If the parameter list for the function contains a comma, it can accept multiple
				// parameters and, therefore, its argument needs to be searched for the argument
				// delimiter token.
				nestedScope.bAcceptsMultipleArgs =
					(g_mapParameters[nestedScope.strFunction].find(',') != string::npos) ||
					// Rather that make separate COM calls to determine if multiple arguments are
					// expected, for efficiency, assume they are for custom functions.
					g_mapParameters[nestedScope.strFunction].empty();
				stackScopeData.push(nestedScope);
				
				// Now that we've created a scope for this function, we will need to look for the
				// next function (if any) in the next iteration.
				bSearchForNextFunc = true;
				continue;
			}
			else
			{
				// The current function argument ends before anything else. (the function scope
				// has closed or the argument delimiting token was encountered)

				// Add the expanded argument the currentScope's argument list.
				currentScope.vecExpandedArgs.push_back(currentScope.strResult);

				// Advance ulSearchPos
				ulSearchPos = ulSectionEnd + 1;

				if (ulSectionEnd == ulNextToken)
				{
					// Another argument for this function scope follows; reset the running result
					// to prepare for the next argument.
					currentScope.strResult = "";
					continue;
				}
			}

			// If we've gotten here, we've closed the scope on currentScope.strFunction.
			// For functions that accept 1 argument, an empty vecExpandedArgs is equivalent to "".
			string strExpandedArg = (currentScope.vecExpandedArgs.size() == 0) 
				? "" 
				: currentScope.vecExpandedArgs[0];
			
			// Evaluate the function.
			string strFuncResult;
			try
			{
				try
				{
					
					if (currentScope.strFunction == gstrFUNC_DIR_OF)
					{
						strFuncResult += expandDirOf(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_FILE_NO_EXT_OF)
					{
						strFuncResult += expandFileNoExtOf(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_FILE_OF)
					{
						strFuncResult += expandFileOf(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_DIR_NO_DRIVE_OF)
					{
						strFuncResult += expandDirNoDriveOf(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_DRIVE_OF)
					{
						strFuncResult += expandDriveOf(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_EXT_OF)
					{
						strFuncResult += expandExtOf(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_LEFT)
					{
						strFuncResult += expandLeft(currentScope.vecExpandedArgs);
					}
					else if (currentScope.strFunction == gstrFUNC_INSERT_BEFORE_EXT)
					{
						strFuncResult += expandInsertBeforeExt(currentScope.vecExpandedArgs);
					}
					else if (currentScope.strFunction == gstrFUNC_CHANGE_EXT)
					{
						strFuncResult += expandChangeExt(currentScope.vecExpandedArgs);
					}
					else if (currentScope.strFunction == gstrFUNC_MID)
					{
						strFuncResult += expandMid(currentScope.vecExpandedArgs);
					}
					else if (currentScope.strFunction == gstrFUNC_REPLACE)
					{
						strFuncResult += expandReplace(currentScope.vecExpandedArgs);
					}
					else if (currentScope.strFunction == gstrFUNC_RIGHT)
					{
						strFuncResult += expandRight(currentScope.vecExpandedArgs);
					}
					else if (currentScope.strFunction == gstrFUNC_PAD_VALUE)
					{
						strFuncResult += expandPadValue(currentScope.vecExpandedArgs);
					}
					else if (currentScope.strFunction == gstrFUNC_OFFSET)
					{
						strFuncResult += expandOffset(currentScope.vecExpandedArgs);
					}
					else if (currentScope.strFunction == gstrFUNC_TRIM_AND_CONSOLIDATE_WS)
					{
						strFuncResult += expandTrimAndConsolidateWS(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_ENV)
					{
						strFuncResult += expandEnv(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_NOW)
					{
						strFuncResult += expandNow(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_RANDOM_ALPHA_NUMERIC)
					{
						strFuncResult += expandRandomAlphaNumeric(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_RANDOM_ENTRY_FROM_LIST_FILE)
					{
						strFuncResult += expandRandomEntryFromListFile(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_RANDOM_ENTRY_FROM_LIST)
					{
						strFuncResult += expandRandomEntryFromList(currentScope.vecExpandedArgs);
					}
					else if (currentScope.strFunction == gstrFUNC_USER_NAME)
					{
						strFuncResult += expandUserName(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_FULL_USER_NAME)
					{
						strFuncResult += expandFullUserName(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_THREAD_ID)
					{
						strFuncResult += expandThreadId(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_PROCESS_ID)
					{
						strFuncResult += expandProcessId(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_LOWER_CASE)
					{
						strFuncResult += expandLowerCase(strExpandedArg);
					}
					else if (currentScope.strFunction == gstrFUNC_UPPER_CASE)
					{
						strFuncResult += expandUpperCase(strExpandedArg);
					}
					else
					{
						UCLID_COMUTILSLib::IVariantVectorPtr ipArgs(CLSID_VariantVector);
						ASSERT_RESOURCE_ALLOCATION("ELI43528", ipArgs != __nullptr);

						for each (string strArg in currentScope.vecExpandedArgs)
						{
							ipArgs->PushBack(strArg.c_str());
						}

						_bstr_t bstrExpandedValue = ipTagUtility->ExpandFunction(
							currentScope.strFunction.c_str(), ipArgs, bstrSourceDocName, pData);

						strFuncResult += asString(bstrExpandedValue);
					}
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI35255");
			}
			catch (UCLIDException &ue)
			{
				try
				{
					ue.addDebugInfo("Function", currentScope.strFunction);
					string strUnExpandedArg = str.substr(currentScope.ulArgStartPos,
						ulSearchPos - currentScope.ulArgStartPos - 1);
					ue.addDebugInfo(currentScope.bAcceptsMultipleArgs ? "Arguments" : "Argument",
						strUnExpandedArg);
					for (size_t i = 0; i < currentScope.vecExpandedArgs.size(); i++)
					{
						ue.addDebugInfo("Expanded argument", currentScope.vecExpandedArgs[i]);
					}
				}
				catch (...) {}

				throw ue;
			}

			// Exit the current function scope.
			stackScopeData.pop();
			
			// Append the function result to the running result of the parent scope.
			stackScopeData.top().strResult += strFuncResult;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI35223")
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::recursivelyExpandTagsAndFunctions(
	std::string input,
	UCLID_COMUTILSLib::ITagUtilityPtr ipTagUtility,
	_bstr_t bstrSourceDocName,
	IUnknown* pData,
	long recursionDepth)
{
	string _;
	set<string> previousStates;
	bool changed = false;

	do
	{
		// Store values seen during the loop in order to detect a cycle in tag definitions
		auto currentState = previousStates.insert(move(input));
		bool valueSeenBefore = !currentState.second;

		ASSERT_RUNTIME_CONDITION("ELI53549", !valueSeenBefore, "Cycle detected while expanding path tags!");

		const string& unexpanded = *currentState.first;

		// Expand instances of the first tag only, in case the expansion contains a function
		input = asString(ipTagUtility->ExpandTags(unexpanded.c_str(), bstrSourceDocName, pData, VARIANT_TRUE));

		changed = input != unexpanded;

		// https://extract.atlassian.net/browse/ISSUE-12939
		// Allow for the possibility that a tag function may be nested inside a custom tag.
		// If any tag replacements were done and the expanded value looks like it may have a
		// function, perform function expansion on strNextSection.
		if (changed && findNextFunction(input, 0, _, _, ipTagUtility) != string::npos)
		{
			input = expandFunctions(input, ipTagUtility, bstrSourceDocName, pData, recursionDepth + 1);
		}
	}
	while (changed);

	return input;
}
//-------------------------------------------------------------------------------------------------
int TextFunctionExpander::findNextFunction(const string& str, unsigned long ulSearchPos,
	string &rstrFunction, string &rstrToken, UCLID_COMUTILSLib::ITagUtilityPtr ipTagUtility)
{
	// find the beginning of the next function if there is one
	unsigned long ulFuncStart = str.find('$', ulSearchPos);

	while (ulFuncStart != string::npos)
	{
		// Initialize additional functions made available from the provided tag utility.
		getFunctions(ipTagUtility);

		// check if the location begins with one of the functions
		unsigned int nNumFunctions = m_vecFunctions.size();
		for ( unsigned int i = 0; i < nNumFunctions ; i++ )
		{
			unsigned long ulNamePos = str.find(m_vecFunctions[i], ulFuncStart + 1);
			if (ulNamePos == ulFuncStart + 1)
			{
				// Set the function
				rstrFunction = m_vecFunctions[i];

				// Check for {}
				unsigned long ulBracketPos = ulNamePos + rstrFunction.length();
				if (ulBracketPos < str.length() && str[ulBracketPos] == '{')
				{
					// Find the closing bracket
					unsigned long ulClosingBracket = str.find_first_of("}", ulBracketPos+1);
					bool bMatched = ulClosingBracket != string::npos;
					if (!bMatched || ulClosingBracket - ulBracketPos > 2)
					{
						string strMessage = bMatched
							? "Token length too long (only single token supported) "
							: "Unmatched '{' ";
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

					// Get the alternate token
					rstrToken = str[ulBracketPos+1];
				}
				else
				{
					rstrToken = ",";
				}

				break;
			}
		}

		if (rstrFunction.empty())
		{
			ulFuncStart = str.find('$', ulFuncStart + 1);
		}
		else
		{
			break;
		}
	}

	return ulFuncStart;
}
//-------------------------------------------------------------------------------------------------
void TextFunctionExpander::getFunctions(UCLID_COMUTILSLib::ITagUtilityPtr ipTagUtility)
{
	if (m_vecFunctions.empty())
	{
		UCLID_COMUTILSLib::IVariantVectorPtr ipFunctions = ipTagUtility->GetFunctionNames();
		long nCount = ipFunctions->Size;
		for (long i = 0; i < nCount; i++)
		{
			m_vecFunctions.push_back(asString(ipFunctions->Item[i].bstrVal));
		}
	}
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
		str = "$" + str + "(" + g_mapParameters[str] + ")";
	}
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
const string TextFunctionExpander::expandInsertBeforeExt(vector<string>& vecParameters) const
{
	// Check for proper number of tokens
	if (vecParameters.size() == 2)
	{
		// Retrieve each string
		string strSource = vecParameters[0];
		string strInsert = vecParameters[1];

		// Source string cannot be empty
		if (strSource.length() == 0)
		{
			// Create and throw exception
			UCLIDException ue( "ELI19657", "InsertBeforeExt function cannot operate on an empty string!");
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
		ue.addDebugInfo("NumOfArgs", vecParameters.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandOffset(vector<string>& vecParameters) const
{
	unsigned int i = 0;
	for ( ; i < vecParameters.size(); i++)
	{
		// remove leading and trailing spaces
		vecParameters[i] = trim(vecParameters[i], " ", " ");
	}

	long lValue = 0;

	if (vecParameters.size() == 2)
	{
		try
		{
			lValue = asLong(vecParameters[0]);
			long lOffset = asLong(vecParameters[1]);

			lValue += lOffset;
		}
		catch(...)
		{
			UCLIDException uex("ELI30027", "Invalid long value(s) for Offset function.");
			uex.addDebugInfo("Value", vecParameters[0]);
			uex.addDebugInfo("Offset Value", vecParameters[1]);
			throw uex;
		}
	}
	else
	{
		UCLIDException ue("ELI12624", "Offset Function has invalid number of arguments!");
		ue.addDebugInfo("NumOfArgs", vecParameters.size());
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
const string TextFunctionExpander::expandPadValue(vector<string>& vecParameters) const
{
	unsigned int i = 0;
	for( ; i < vecParameters.size(); i++)
	{
		// remove leading and trailing spaces
		vecParameters[i] = trim(vecParameters[i], " ", " ");
	}

	if(vecParameters.size() == 3)
	{
		char cPadWithChar = 0;

		if(vecParameters[1].size() == 1)
		{
			cPadWithChar = vecParameters[1][0];
		}
		else
		{
			UCLIDException ue("ELI12576", "PadValue Function has invalid padding character argument!");
			ue.addDebugInfo("PaddingChar", vecParameters[1]);
			throw ue;
		}

		int iPadLength = 0;
		try
		{
			iPadLength = (int)asUnsignedLong(vecParameters[2]);
		}
		catch(...)
		{
			UCLIDException uex("ELI30026", "Invalid padding length specified.");
			uex.addDebugInfo("Padding Length", vecParameters[2]);
			throw uex;
		}
		
		return padCharacter(vecParameters[0], true, cPadWithChar, iPadLength);
	}
	else
	{
		UCLIDException ue("ELI12575", "PadValue Function has invalid number of arguments!");
		ue.addDebugInfo("NumOfArgs", vecParameters.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandReplace(vector<string>& vecParameters) const
{
	// Check for proper number of tokens
	if (vecParameters.size() == 3)
	{
		// Retrieve each string
		string strSource = vecParameters[0];
		string strSearch = vecParameters[1];
		string strReplace = vecParameters[2];

		// Source string cannot be empty
		if (strSource.length() == 0)
		{
			// Create and throw exception
			UCLIDException ue( "ELI15712", "Replace function cannot operate on an empty string!");
			throw ue;
		}

		// Search string cannot be empty
		if (strSearch.length() == 0)
		{
			// Create and throw exception
			UCLIDException ue( "ELI15713", "Replace function cannot search for an empty string!");
			throw ue;
		}

		// [FlexIDSCore:5240]
		// Removed use of replaceVariable call here so that the search can be made case-insensitive.
		stringCSIS csisSource(strSource, false);
		size_t findpos = csisSource.find(strSearch);
		while (findpos != string::npos)
		{
			strSource.replace(findpos, strSearch.length(), strReplace);
			csisSource = stringCSIS(strSource, false);
			findpos = csisSource.find(strSearch, findpos + strReplace.length());
		}

		// Return the result of the string replace
		return strSource;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI15711", "Replace function has invalid number of arguments!");
		ue.addDebugInfo("NumOfArgs", vecParameters.size());
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
		throw uex;
	}

	// Return a random string of nLength containing only upper case letters and digits
	return ms_Rand.getRandomString(nLength, true, false, true);
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandRandomEntryFromList(vector<string>& vecParameters) const
{
	if (vecParameters.size() < 2)
	{
		throw UCLIDException("ELI37006", "RandomEntryFromList function requires at least two values.");
	}

	try
	{
		try
		{
			long index = ms_Rand.uniform(vecParameters.size());
			return vecParameters[index];
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37007");
	}
	catch(UCLIDException &ue)
	{
		UCLIDException uex("ELI37008",
			"Unable to parse random entry from file list.", ue);
		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandRandomEntryFromListFile(const string &str) const
{
	if (str.empty())
	{
		throw UCLIDException("ELI37003", "RandomEntryFromListFile function requires a filename.");
	}

	try
	{
		try
		{
			vector<string> vecLines = convertFileToLines(str);
			
			long index = ms_Rand.uniform(vecLines.size());
			return vecLines[index];
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37005");
	}
	catch(UCLIDException &ue)
	{
		UCLIDException uex("ELI37004",
			"Unable to parse random entry from file list.", ue);
		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandUserName(const string& str) const
{
	if (!str.empty())
	{
		UCLIDException uex("ELI28764", "$UserName() does not accept arguments.");
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
		throw uex;
	}

	bool bThrowException = str != "1";
	return getFullUserName(bThrowException);
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandLeft(vector<string>& vecParameters) const
{
	// Check for appropriate number of tokens
	if (vecParameters.size() == 2)
	{
		// Get the count of characters
		long lCount = 0;
		try
		{
			lCount = asLong(vecParameters[1]);
			if (lCount <= 0)
			{
				throw 42;
			}
		}
		catch(...)
		{
			UCLIDException ue("ELI29957", "Invalid length argument specified for $Left().");
			ue.addDebugInfo("Length Argument", vecParameters[1]);
			throw ue;
		}

		// Return the first lCount characters of the string
		return vecParameters[0].substr(0, lCount);
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI29958", "Left function has invalid number of arguments!");
		ue.addDebugInfo("NumOfArgs", vecParameters.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandMid(vector<string>& vecParameters) const
{
	// Get the string
	const string& strTemp = vecParameters[0];

	// Check for appropriate number of tokens
	size_t nParamCount = vecParameters.size();
	if (nParamCount == 2 || nParamCount == 3)
	{
		// Get the start character
		long lStart = 1;
		try
		{
			lStart = asLong(vecParameters[1]);
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
			ue.addDebugInfo("Start Position", vecParameters[1]);
			throw ue;
		}

		// Get the count of characters
		long lCount = -1;
		if (nParamCount == 3)
		{
			try
			{
				lCount = asLong(vecParameters[2]);
				if (lCount <= -2 || lCount == 0)
				{
					throw 42;
				}
			}
			catch(...)
			{
				UCLIDException ue("ELI29960", "Invalid count value specified for $Mid().");
				ue.addDebugInfo("Count", vecParameters[2]);
				throw ue;
			}
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
		UCLIDException ue("ELI29961", "Mid function has invalid number of arguments!");
		ue.addDebugInfo("NumOfArgs", nParamCount);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandRight(vector<string>& vecParameters) const
{
	// Check for appropriate number of tokens
	if (vecParameters.size() == 2)
	{
		// Get the count of characters
		long lCount = 0;
		try
		{
			lCount = asLong(vecParameters[1]);
			if (lCount <= 0)
			{
				throw 42;
			}
		}
		catch(...)
		{
			UCLIDException ue("ELI29962", "Invalid length argument specified for $Right().");
			ue.addDebugInfo("Length Argument", vecParameters[1]);
			throw ue;
		}

		// Get the string
		const string& strTemp = vecParameters[0];

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
		ue.addDebugInfo("NumOfArgs", vecParameters.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandChangeExt(vector<string>& vecParameters) const
{
	// Check for proper number of tokens
	if (vecParameters.size() == 2)
	{
		// Retrieve each string
		string strSource = vecParameters[0];
		string strExtension = vecParameters[1];

		// Source string cannot be empty
		if (strSource.length() == 0)
		{
			// Create and throw exception
			UCLIDException ue( "ELI30019", "ChangeExt function cannot operate on an empty string!");
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
		ue.addDebugInfo("NumOfArgs", vecParameters.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandThreadId(const string& str) const
{
	if (!str.empty())
	{
		UCLIDException uex("ELI32498", "$ThreadId() does not accept arguments.");
		throw uex;
	}

	return asString(GetCurrentThreadId());
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandProcessId(const string& str) const
{
	if (!str.empty())
	{
		UCLIDException uex("ELI32499", "$ProcessId() does not accept arguments.");
		throw uex;
	}

	return asString(GetCurrentProcessId());
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandLowerCase(const string& str) const
{
	string strRet = str;
	makeLowerCase(strRet);
	return strRet;
}
//-------------------------------------------------------------------------------------------------
const string TextFunctionExpander::expandUpperCase(const string& str) const
{
	string strRet = str;
	makeUpperCase(strRet);
	return strRet;
}