#pragma once

#include "BaseUtils.h"
#include "Win32CriticalSection.h"

#include <string>
#include <vector>


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
	const std::string expandFunctions(const std::string& str) const;

	// PURPOSE: to return a list of all functions that 
	//			this method supports i.e. "dirof", "fileof", "extof"
	// REQUIRE: NONE
	// PROMISE: 
	const std::vector<std::string>& getAvailableFunctions() const;

	// PURPOSE: return whether a certain function is available
	// REQUIRE: strFunction must be a plain function name i.e. 
	//			"dirof" not "$dirof" or "$dirof()
	// PROMISE:
	bool isFunctionAvailable(const std::string& strFunction) const;

	// PURPOSE: To add the function syntax around each function in a list of
	//			function names
	// REQUIRE: None
	// PROMISE: each string in vecFunctions (which should be a function name 
	//			e.g. "dirof") will be replaced by a formatted function name
	//			(e.g. "$dirof()")
	void formatFunctions(std::vector<std::string>& vecFunctions) const;

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
	bool isValidParameters(const std::string& strFunction, 
		const std::string& strArgument, const std::string& strParameter) const;

private:
	// Examples for path = C:\temp1\temp2\filename.tif
	// $DirNoDriveOf(path) = temp1\temp2
	// $DirOf(path) = C:\temp1\temp2
	// $DriveOf(path) = C:\
	// $ExtOf(path) = .tif
	// $FileNoExtOf(path) = filename
	// $FileOf(path) = filename.tif
	// $Replace(path,te,bli) = C:\blimp1\blimp2\filename.tif
	// $InsertBefore(path,.new) = C:\temp1\temp2\filename.new.tif
	const std::string expandDirOf(const std::string&) const;
	const std::string expandDirNoDriveOf(const std::string&) const;
	const std::string expandDriveOf(const std::string&) const;
	const std::string expandExtOf(const std::string&) const;
	const std::string expandFileOf(const std::string&) const;
	const std::string expandFileNoExtOf(const std::string&) const;
	const std::string expandInsertBeforeExt(const std::string& str) const;
	const std::string expandOffset(const std::string& str) const;
	const std::string expandPadValue(const std::string& str) const;
	const std::string expandReplace(const std::string& str) const;
	const std::string expandTrimAndConsolidateWS(const std::string& str) const;
};
