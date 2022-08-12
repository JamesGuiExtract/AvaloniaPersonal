#pragma once

#include "resource.h"       // main symbols
#include "StdAfx.h"

#include <Random.h>
#include <Win32CriticalSection.h>

#include <string>
#include <vector>
#include <memory>
#include <map>
#include <afxmt.h>
#include <StringCSIS.h>

using namespace std;

class TextFunctionExpander
{
public:	
	TextFunctionExpander();

	// PURPOSE: execute any test functions that exist in str and 
	//			replace them with their result
	// REQUIRE: All of the functions in str must have valid function syntax
	//			i.e. $funcname(strArg)
	//			All of the functions must be valid functions i.e. 
	//			isFunctionAvailable(funcname) returns true
	//			All arguments must be valid for their function
	// PROMISE: 
	const string expandFunctions(const string& str,
		UCLID_COMUTILSLib::ITagUtilityPtr ipTagUtility, BSTR bstrSourceDocName, IUnknown *pData, long recursiveDepth);

	// PURPOSE: Returns the position of the next path tag function in str to expand starting at or
	//			following ulSearchPos.
	//			If found, rstrFunction returns the name of the function and rstrToken returns
	//			the token to be used to delimit separate parameters.
	int findNextFunction(const string& str, unsigned long ulSearchPos, string &rstrFunction,
		string &rstrToken, UCLID_COMUTILSLib::ITagUtilityPtr ipTagUtility);

	// PURPOSE: to return a list of all functions that 
	//			this method supports i.e. "dirof", "fileof", "extof"
	// REQUIRE: NONE
	// PROMISE: 
	const vector<string>& getAvailableFunctions() const;

	// PURPOSE: return whether a certain function is available
	// REQUIRE: strFunction must be a plain function name i.e. 
	//			"dirof" not "$dirof" or "$dirof()
	// PROMISE:
	bool isFunctionAvailable(const string& strFunction) const;

	// PURPOSE: To add the function syntax around each function in a list of
	//			function names
	// REQUIRE: None
	// PROMISE: each string in vecFunctions (which should be a function name 
	//			e.g. "dirof") will be replaced by a formatted function name
	//			(e.g. "$dirof()")
	void formatFunctions(vector<string>& vecFunctions) const;

private:
	// Examples for path = C:\temp1\temp2\filename.tif
	//				FPS_PATH = C:\FPSFiles
	//				Now() = 11/25/2009 09:01:03:257
	//				Logged in user name = jsmith
	//				Human readable user name = John Smith
	// $ChangeExt(path, pdf) = C:\temp1\temp2\filename.pdf
	// $DirNoDriveOf(path) = temp1\temp2
	// $DirOf(path) = C:\temp1\temp2
	// $DriveOf(path) = C:\
	// $ExtOf(path) = .tif
	// $FileNoExtOf(path) = filename
	// $FileOf(path) = filename.tif
	// $Replace(path,te,bli) = C:\blimp1\blimp2\filename.tif
	// $InsertBefore(path,.new) = C:\temp1\temp2\filename.new.tif
	// $Env(FPS_PATH) = C:\FPSFiles
	// $Now() = 2009-11-25-09-01-03-257
	// $Now(%m/%d/%Y %H:%M) = 11/25/2009 09:01
	// $RandomAlphaNumeric(5) = A10FP
	// &UserName() = jsmith
	// &FullUserName() = John Smith
	const string expandChangeExt(vector<string>& vecParameters) const;
	const string expandDirOf(const string&) const;
	const string expandDirNoDriveOf(const string&) const;
	const string expandDriveOf(const string&) const;
	const string expandExtOf(const string&) const;
	const string expandFileOf(const string&) const;
	const string expandFileNoExtOf(const string&) const;
	const string expandInsertBeforeExt(vector<string>& vecParameters) const;
	const string expandLeft(vector<string>& vecParameters) const;
	const string expandMid(vector<string>& vecParameters) const;
	const string expandOffset(vector<string>& vecParameters) const;
	const string expandPadValue(vector<string>& vecParameters) const;
	const string expandReplace(vector<string>& vecParameters) const;
	const string expandRight(vector<string>& vecParameters) const;
	const string expandEnv(const string& str) const;
	const string expandNow(const string& str) const;
	const string expandRandomAlphaNumeric(const string& str) const;
	const string expandRandomEntryFromList(vector<string>& vecParameters) const;
	const string expandRandomEntryFromListFile(const string &str) const;
	const string expandUserName(const string& str) const;
	const string expandFullUserName(const string& str) const;
	const string expandTrimAndConsolidateWS(const string& str) const;
	const string expandThreadId(const string& str) const;
	const string expandProcessId(const string& str) const;
	const string expandLowerCase(const string& str) const;
	const string expandUpperCase(const string& str) const;
	const string expandRelativePathParts(const vector<string>& vecParameters) const;

	// Contains data pertaining to a function scope in the main loop in expandFunctions.
	struct expansionScopeData
	{
		// The result for scope.
		unsigned long ulArgStartPos;
		string strResult;
		string strFunction;
		string strFuncToken;
		string strUnExpandedArg;
		vector<string> vecExpandedArgs;
		bool bAcceptsMultipleArgs;

		expansionScopeData::expansionScopeData()
			: ulArgStartPos(0)
			, strResult("")
			, strFunction("")
			, strFuncToken(",")
			, vecExpandedArgs(0)
			, bAcceptsMultipleArgs(false)
		{
		}
	};

	// Random object used for "$RandomAlphaNumeric()" calls
	static Random ms_Rand;

	// The functions available including functions custom to the current ITagUtility implementation
	vector<string> m_vecFunctions;

	// Populates m_vecFunctions including any custom functions in ITagUtility.
	void getFunctions(UCLID_COMUTILSLib::ITagUtilityPtr ipTagUtility);

	// Expand tags and then functions repeatedly until the string is stable
	const string recursivelyExpandTagsAndFunctions(
		std::string input,
		UCLID_COMUTILSLib::ITagUtilityPtr ipTagUtility,
		_bstr_t bstrSourceDocName,
		IUnknown* pData,
		long recursiveDepth);
};
