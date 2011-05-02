#pragma once

#include "BaseUtils.h"
#include "Random.h"
#include "Win32CriticalSection.h"

#include <string>
#include <vector>
#include <memory>
#include <afxmt.h>

using namespace std;

class EXPORT_BaseUtils TextFunctionExpander
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
	const string expandFunctions(const string& str) const;

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

	// PURPOSE: return whether a certain function can be expanded using 
	//			strArgument as an argument and strParameter as zero or more 
	//			parameters.
	// REQUIRE: strFunction must be a plain function name i.e. 
	//			"dirof" not "$dirof" or "$dirof()
	//			strArgument must be an empty string if no argument is desired
	//			strParameters must be an empty string if no parameters are desired
	//			Parameters in strParameters must be comma separated
	// PROMISE: Returns true if strFunction can be expanded as specified, 
	//			otherwise false
	bool isValidParameters(const string& strFunction, 
		const string& strArgument, const string& strParameter) const;

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
	const string expandChangeExt(const string& str, const string& strToken) const;
	const string expandDirOf(const string&) const;
	const string expandDirNoDriveOf(const string&) const;
	const string expandDriveOf(const string&) const;
	const string expandExtOf(const string&) const;
	const string expandFileOf(const string&) const;
	const string expandFileNoExtOf(const string&) const;
	const string expandInsertBeforeExt(const string& str, const string& strToken) const;
	const string expandLeft(const string& str, const string& strToken) const;
	const string expandMid(const string& str, const string& strToken) const;
	const string expandOffset(const string& str, const string& strToken) const;
	const string expandPadValue(const string& str, const string& strToken) const;
	const string expandReplace(const string& str, const string& strToken) const;
	const string expandRight(const string& str, const string& strToken) const;
	const string expandEnv(const string& str) const;
	const string expandNow(const string& str) const;
	const string expandRandomAlphaNumeric(const string& str) const;
	const string expandUserName(const string& str) const;
	const string expandFullUserName(const string& str) const;
	const string expandTrimAndConsolidateWS(const string& str) const;
	const string expandThreadId(const string& str) const;
	const string expandProcessId(const string& str) const;

	// Random object used for "$RandomAlphaNumeric()" calls
	static Random ms_Rand;
};
